using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Money.Models;
using Money.Services;

namespace Money.Views.ImportsAndReports;

public partial class PayablesCsvImportPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");

    private List<SubcategoryItem> _categories = [];
    private List<PayableImportRow> _rows = [];
    private string _fileName = "";
    public event EventHandler? Imported;

    public PayablesCsvImportPage(DatabaseService database) { InitializeComponent(); _database = database; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_categories.Count > 0) return;
        try { _categories = await _database.GetSubcategoriesAsync(type: "despesa"); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Falha ao carregar", SqliteErrorMessage.ToFriendly(ex)); }
    }
    private async void OnPickFileClicked(object? sender, EventArgs e)
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecione o CSV de contas a pagar",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".csv"],
                [DevicePlatform.Android] = ["text/csv", "text/comma-separated-values", "application/csv", "application/octet-stream"]
            })
        });
        if (file is null) return;
        try
        {
            using var reader = new StreamReader(await file.OpenReadAsync(), Encoding.UTF8, true);
            var content = await reader.ReadToEndAsync(); _rows = Parse(content);
            _fileName = file.FileName;
            FileLabel.Text = file.FileName + (content.Split('\n')[0].Contains("Subcategoria", StringComparison.OrdinalIgnoreCase) ? "" : " · Formato legado: Categoria identificada como subcategoria");
            PreviewList.ItemsSource = _rows;
            var valid = _rows.Count(x => x.IsValid);
            SummaryLabel.Text = $"{_rows.Count} linha(s) · {valid} válida(s) · {_rows.Count - valid} com erro";
            ImportButton.IsEnabled = valid > 0 && valid == _rows.Count;
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "CSV inválido", ex.Message, "Fechar"); }
    }
    private async void OnDownloadTemplateClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = await CsvTemplateService.SavePayablesTemplateAsync();
            await ThemedDialog.ShowAsync(this, "Modelo CSV salvo", $"Arquivo salvo em:\n{path}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Não foi possível salvar", ex.Message, "Fechar"); }
    }

    private List<PayableImportRow> Parse(string content)
    {
        var lines = content.TrimStart('\uFEFF').Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        char? declaredSeparator = null;
        if (lines.Count > 0 && lines[0].Trim().StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
        {
            var declaration = lines[0].Trim();
            if (declaration.Length > 4) declaredSeparator = declaration[4];
            lines.RemoveAt(0);
        }
        if (lines.Count < 2) throw new InvalidDataException("O arquivo não contém dados.");
        if (Regex.IsMatch(content,
                @"(?:parcela|parc)\s*\d{1,3}\s*(?:de|/)\s*\d{1,3}|\(\s*\d{1,3}\s*/\s*\d{1,3}\s*\)",
                RegexOptions.IgnoreCase))
            throw new InvalidDataException(
                "Este arquivo é uma fatura parcelada. Volte e escolha 'Importar Fatura Cartão'; essa opção reconhece Parcela 7 de 12 e gera automaticamente de 1/12 até 12/12.");
        var separator = declaredSeparator ??
            (lines[0].Count(x => x == ';') >= lines[0].Count(x => x == ',') ? ';' : ',');
        var headers = Split(lines[0], separator).Select(Normalize).ToArray();
        var required = new[] { "descricao", "valor", "datavencimento", "categoria" };
        var missing = required.Where(x => !headers.Contains(x)).ToArray();
        if (missing.Length > 0) throw new InvalidDataException($"Coluna(s) obrigatória(s) ausente(s): {string.Join(", ", missing)}.");
        string Cell(string[] row, string name) { var i = Array.IndexOf(headers, Normalize(name)); return i >= 0 && i < row.Length ? row[i].Trim() : ""; }
        var result = new List<PayableImportRow>();
        for (var i = 1; i < lines.Count; i++)
        {
            var cells = Split(lines[i], separator); var errors = new List<string>();
            var description = Cell(cells, "Descricao"); if (string.IsNullOrWhiteSpace(description)) errors.Add("Descrição obrigatória");
            var rawAmount = Cell(cells, "Valor").Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (!decimal.TryParse(rawAmount, NumberStyles.Number, _culture, out var amount) && !decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)) errors.Add("Valor inválido");
            if (amount <= 0) errors.Add("Valor deve ser maior que zero");
            if (!DateTime.TryParseExact(Cell(cells, "DataVencimento"), ["dd/MM/yyyy", "yyyy-MM-dd"], _culture, DateTimeStyles.None, out var due)) errors.Add("Data de vencimento inválida");
            var categoryName = Cell(cells, "Categoria");
            SubcategoryItem? category = null;
            try { category = DatabaseService.ResolveImportedSubcategory(_categories, categoryName, Cell(cells, "Subcategoria")); }
            catch (InvalidDataException ex) { errors.Add(ex.Message); }
            if (category is null) errors.Add($"Categoria não cadastrada: {categoryName}");
            result.Add(new PayableImportRow { LineNumber = i + 1, Description = description, Amount = amount, DueDate = due, Category = category?.DisplayName ?? categoryName, CategoryId = category?.Id, Supplier = Cell(cells, "Fornecedor"), Notes = Cell(cells, "Observacoes"), Error = string.Join(" · ", errors) });
        }
        return result;
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        if (_rows.Any(x => !x.IsValid)) { await ThemedDialog.ShowAsync(this, "Corrija o arquivo", "Todas as linhas precisam estar válidas antes da importação."); return; }
        if (!await ThemedDialog.ConfirmAsync(this, "Confirmar importação", $"Importar {_rows.Count} conta(s) a pagar?", "Importar")) return;
        try
        {
            await _database.ImportPayablesAsync(_rows, 0, _fileName);
            await ThemedDialog.ShowAsync(this, "Importação concluída", $"{_rows.Count} conta(s) importada(s) com sucesso!");
            Imported?.Invoke(this, EventArgs.Empty); await Navigation.PopModalAsync();
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Importação não realizada", SqliteErrorMessage.ToFriendly(ex), "Fechar"); }
    }

    private static string[] Split(string value, char separator) => Regex.Split(value, $"{separator}(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)").Select(x => x.Trim().Trim('"')).ToArray();
    private static string Normalize(string value) => new(value.Trim().TrimStart('\uFEFF').ToLowerInvariant().Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
