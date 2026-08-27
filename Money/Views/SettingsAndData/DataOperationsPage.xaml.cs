using System.Globalization;
using System.Text.RegularExpressions;
using Money.Models;
using Money.Services;

namespace Money.Views.SettingsAndData;

public partial class DataOperationsPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly BackupService _backup;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");

    public DataOperationsPage(DatabaseService database, BackupService backup)
    {
        InitializeComponent();
        _database = database;
        _backup = backup;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            ImportsContainer.Children.Clear();
            foreach (var item in await _database.GetImportHistoryAsync())
            {
                ImportsContainer.Children.Add(Row(item.FileName,
                    $"{item.Format.ToUpperInvariant()} · {item.Total} registros · {item.Duplicates} duplicados · {item.ImportedAt:dd/MM/yyyy HH:mm}", "BlingPrimary"));
            }
            if (ImportsContainer.Children.Count == 0)
                ImportsContainer.Children.Add(Empty("Nenhuma importação registrada."));

            BackupsContainer.Children.Clear();
            foreach (var item in await _database.GetBackupHistoryAsync())
            {
                BackupsContainer.Children.Add(Row(item.FileName,
                    $"{item.SizeKb:N0} KB · {item.Type} · {item.CreatedAt:dd/MM/yyyy HH:mm}", "BlingPrimary"));
            }
            if (BackupsContainer.Children.Count == 0)
                BackupsContainer.Children.Add(Empty("Nenhum backup registrado."));
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecione um arquivo CSV ou OFX",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".csv", ".ofx"],
                [DevicePlatform.Android] = ["text/csv", "text/comma-separated-values", "application/x-ofx", "application/octet-stream"]
            })
        });
        if (file is null)
            return;
        ImportActivity.IsVisible = ImportActivity.IsRunning = true;
        try
        {
            using var reader = new StreamReader(await file.OpenReadAsync());
            var content = await reader.ReadToEndAsync();
            var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
            if (extension == "csv" && Regex.IsMatch(content,
                    @"(?:parcela|parc)\s*\d{1,3}\s*(?:de|/)\s*\d{1,3}|\(\s*\d{1,3}\s*/\s*\d{1,3}\s*\)",
                    RegexOptions.IgnoreCase))
                throw new InvalidDataException(
                    "Este arquivo contém compras parceladas. Use a opção 'Importar Fatura Cartão' para gerar a série completa e vincular as faturas.");
            var drafts = extension == "ofx" ? await ParseOfxAsync(content) : await ParseCsvAsync(content);
            if (drafts.Count == 0)
                throw new InvalidDataException("Nenhuma movimentação válida foi encontrada.");
            var result = await _database.ImportTransactionsAsync(drafts, file.FileName, extension);
            await ThemedDialog.ShowAsync(this, "Importação concluída", $"{result.Imported} registros importados com sucesso.");
            await LoadHistoryAsync();
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
        finally { ImportActivity.IsRunning = ImportActivity.IsVisible = false; }
    }

    private async void OnOpenPayablesImportClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new PayablesCsvImportPage(_database));

    private async void OnOpenCardInvoiceImportClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new ImportCardInvoicePage(_database));

    private async void OnDownloadPayablesTemplateClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = await CsvTemplateService.SavePayablesTemplateAsync();
            await ThemedDialog.ShowAsync(this, "Modelo CSV salvo",
                $"O modelo de Contas a Pagar foi salvo em:\n{path}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível salvar o modelo",
                SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private async void OnDownloadCardTemplateClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = await CsvTemplateService.SaveCardInvoiceTemplateAsync();
            await ThemedDialog.ShowAsync(this, "Modelo CSV salvo",
                $"O modelo de Fatura de Cartão foi salvo em:\n{path}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível salvar o modelo",
                SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private async Task<List<TransactionDraft>> ParseCsvAsync(string content)
    {
        var categories = await _database.GetCategoriesAsync();
        var accounts = await _database.GetAccountsAsync();
        var lines = content.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
            return [];
        var separator = lines[0].Count(x => x == ';') >= lines[0].Count(x => x == ',') ? ';' : ',';
        var headers = SplitCsv(lines[0], separator).Select(Normalize).ToArray();
        string Cell(string[] row, string name)
        {
            var index = Array.IndexOf(headers, Normalize(name));
            return index >= 0 && index < row.Length ? row[index].Trim() : "";
        }
        var result = new List<TransactionDraft>();
        foreach (var line in lines.Skip(1))
        {
            var row = SplitCsv(line, separator);
            var type = Normalize(Cell(row, "tipo")) switch
            {
                "receita" or "entrada" => "receita",
                "transferencia" => "transferencia",
                _ => "despesa"
            };
            if (!decimal.TryParse(Cell(row, "valor"), NumberStyles.Currency, _culture, out var amount))
                continue;
            amount = Math.Abs(amount);
            if (!DateTime.TryParse(Cell(row, "data"), _culture, DateTimeStyles.None, out var date))
                date = DateTime.Today;
            var account = accounts.FirstOrDefault(x => Normalize(x.Name) == Normalize(Cell(row, "conta"))) ?? accounts.FirstOrDefault();
            if (account is null)
                throw new InvalidDataException("Cadastre uma conta antes da importação.");
            var destination = type == "transferencia"
                ? accounts.FirstOrDefault(x => Normalize(x.Name) == Normalize(Cell(row, "conta_destino")))
                : null;
            if (type == "transferencia" && destination is null)
                throw new InvalidDataException("Informe a coluna conta_destino nas transferências.");
            var category = categories.FirstOrDefault(x => x.Type == type && Normalize(x.Name) == Normalize(Cell(row, "categoria")))
                ?? categories.FirstOrDefault(x => x.Type == type);
            if (type != "transferencia" && category is null)
                throw new InvalidDataException($"Cadastre uma categoria de {type} antes da importação.");
            result.Add(new(Cell(row, "descricao") is { Length: > 0 } d ? d : "Importação CSV",
                amount, date, type, category?.Id ?? 0, account.Id, null, "Importado de CSV",
                destination?.Id, type == "despesa" ? date : null, type == "receita",
                type == "receita" ? date : null));
        }
        return result;
    }

    private async Task<List<TransactionDraft>> ParseOfxAsync(string content)
    {
        var accounts = await _database.GetAccountsAsync();
        var categories = await _database.GetCategoriesAsync();
        var account = accounts.FirstOrDefault() ?? throw new InvalidDataException("Cadastre uma conta antes da importação.");
        var result = new List<TransactionDraft>();
        foreach (Match block in Regex.Matches(content, "<STMTTRN>(.*?)(?:</STMTTRN>|(?=<STMTTRN>)|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase))
        {
            string Tag(string name) => Regex.Match(block.Value, $"<{name}>([^<\\r\\n]+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
            if (!decimal.TryParse(Tag("TRNAMT"), NumberStyles.Any, CultureInfo.InvariantCulture, out var signed))
                continue;
            var type = signed >= 0 ? "receita" : "despesa";
            var category = categories.FirstOrDefault(x => x.Type == type) ?? throw new InvalidDataException($"Cadastre uma categoria de {type}.");
            var rawDate = Tag("DTPOSTED");
            var date = DateTime.TryParseExact(rawDate[..Math.Min(8, rawDate.Length)], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : DateTime.Today;
            result.Add(new(Tag("MEMO") is { Length: > 0 } memo ? memo : Tag("NAME"), Math.Abs(signed), date,
                type, category.Id, account.Id, null, $"OFX FITID: {Tag("FITID")}", null,
                type == "despesa" ? date : null, type == "receita", type == "receita" ? date : null));
        }
        return result;
    }

    private async void OnBackupClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = await _backup.CreateBackupAsync();
            await ThemedDialog.ShowAsync(this, "Backup criado com sucesso", path);
            await LoadHistoryAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private async void OnRestoreClicked(object? sender, EventArgs e)
    {
        if (!await ThemedDialog.ConfirmAsync(this, "Restaurar backup", "Os dados atuais serão substituídos. Uma cópia de segurança será preservada.", "Restaurar"))
            return;
        try
        {
            var safety = await _backup.RestoreBackupAsync();
            await ThemedDialog.ShowAsync(this, "Backup restaurado", $"Cópia anterior salva em:\n{safety}\n\nReinicie o aplicativo.");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private static string[] SplitCsv(string value, char separator) => Regex.Split(value, $"{separator}(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)").Select(x => x.Trim().Trim('"')).ToArray();
    private static string Normalize(string value) => value.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD).Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).Aggregate("", (s, c) => s + c);

    private static View Row(string title, string subtitle, string color) => new Border
    {
        BackgroundColor = ThemeColor.Get("BlingCard"),
        StrokeThickness = 0,
        Padding = 10,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
        Content = new VerticalStackLayout
        {
            Spacing = 2,
            Children = {
                new Label { Text = title, TextColor = ThemeColor.Get("BlingPrimary"), FontAttributes = FontAttributes.Bold, FontSize = 12 },
                new Label { Text = subtitle, TextColor = ThemeColor.Get(color), FontSize = 10 }
            }
        }
    };

    private static Label Empty(string text) => new() { Text = text, TextColor = ThemeColor.Get("BlingText"), FontSize = 11 };
    private void ShowError(Exception ex)
    {
        ErrorLabel.Text = SqliteErrorMessage.ToFriendly(ex);
        ErrorLabel.IsVisible = true;
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
