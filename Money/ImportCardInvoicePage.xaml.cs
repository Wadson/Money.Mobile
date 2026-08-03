using System.Globalization;
using System.Text;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money;

public partial class ImportCardInvoicePage : ContentPage
{
    private readonly DatabaseService _database;
    private List<CardItem> _cards = [];
    private List<CardCsvPurchase> _purchases = [];
    private string _fileName = "";
    public event EventHandler? Imported;

    public ImportCardInvoicePage(DatabaseService database) { InitializeComponent(); _database = database; }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); if (_cards.Count > 0) return;
        _cards = await _database.GetCardsAsync(); CardPicker.ItemsSource = _cards.Select(x => x.Name).ToList();
    }
    private void OnCardChanged(object? sender, EventArgs e)
    {
        var valid = CardPicker.SelectedIndex >= 0 && CardPicker.SelectedIndex < _cards.Count;
        PickFileButton.IsEnabled = valid;
        if (!valid) return;
        var card = _cards[CardPicker.SelectedIndex];
        CardSelectionButton.Text = card.Name;
        CardInfoLabel.Text = $"Fecha dia {card.ClosingDay} · vence dia {card.DueDay} · limite {card.CreditLimit:C2}";
        _purchases.Clear(); PreviewList.ItemsSource = null; ConfirmButton.IsEnabled = false; SummaryLabel.Text = "";
    }

    private async void OnSelectCardClicked(object? sender, EventArgs e)
    {
        if (_cards.Count == 0)
        {
            await ThemedDialog.ShowAsync(this, "Nenhum cartão", "Cadastre um cartão de crédito antes da importação.");
            return;
        }

        var options = _cards.Select((card, index) => new SelectionOption
        {
            Index = index,
            Label = $"{card.Name} · limite {card.CreditLimit:C2}",
            ImageSource = MaterialIcons.CreditCard,
            Background = SafeCardColor(card.Color).WithAlpha(.16f),
            Foreground = IsDarkTheme() ? Colors.White : SafeCardColor(card.Color),
            IsSelected = CardPicker.SelectedIndex == index
        }).ToList();
        var page = new OptionSelectionPage("Cartão de destino", options);
        page.Selected += (_, option) => CardPicker.SelectedIndex = option.Index;
        await Navigation.PushModalAsync(page);
    }

    private static Color SafeCardColor(string? value)
    {
        try { return Color.FromArgb(value ?? BlingPalette.PrimaryHex); }
        catch { return ThemeColor.Get("BlingPrimary"); }
    }

    private static bool IsDarkTheme()
    {
        var app = Application.Current;
        return app?.UserAppTheme == AppTheme.Dark ||
               (app?.UserAppTheme == AppTheme.Unspecified && app.RequestedTheme == AppTheme.Dark);
    }
    private async void OnPickFileClicked(object? sender, EventArgs e)
    {
        if (CardPicker.SelectedIndex < 0) return;
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Selecione a fatura CSV",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> { [DevicePlatform.WinUI] = [".csv"], [DevicePlatform.Android] = ["text/csv", "text/comma-separated-values", "application/octet-stream"] }) });
        if (file is null) return;
        try
        {
            using var reader = new StreamReader(await file.OpenReadAsync(), Encoding.UTF8, true);
            _purchases = CardInvoiceCsvParser.Parse(await reader.ReadToEndAsync()); _fileName = file.FileName;
            var expanded = CardInvoiceCsvParser.Expand(_purchases, _cards[CardPicker.SelectedIndex]);
            PreviewList.ItemsSource = _purchases;
            var first = expanded.Min(x => new DateTime(x.InvoiceYear, x.InvoiceMonth, 1));
            var last = expanded.Max(x => new DateTime(x.InvoiceYear, x.InvoiceMonth, 1));
            SummaryLabel.Text = $"{_purchases.Count} compra(s) lida(s) → {expanded.Count} transação(ões) · Total {expanded.Sum(x => x.Amount):C2}\nFaturas: {first:MMM/yyyy} a {last:MMM/yyyy}";
            ConfirmButton.IsEnabled = expanded.Count > 0;
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "CSV inválido", ex.Message, "Fechar"); }
    }
    private async void OnDownloadTemplateClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = await CsvTemplateService.SaveCardInvoiceTemplateAsync();
            await ThemedDialog.ShowAsync(this, "Modelo CSV salvo", $"Arquivo salvo em:\n{path}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Não foi possível salvar", ex.Message, "Fechar"); }
    }
    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (CardPicker.SelectedIndex < 0 || _purchases.Count == 0) return;
        var card = _cards[CardPicker.SelectedIndex]; var expanded = CardInvoiceCsvParser.Expand(_purchases, card);
        if (!await ThemedDialog.ConfirmAsync(this, "Confirmar importação", $"Gerar {expanded.Count} parcela(s) no cartão {card.Name}?", "Importar")) return;
        try
        {
            var imported = await _database.ImportCardInvoiceAsync(card.Id, expanded, _fileName);
            await ThemedDialog.ShowAsync(this, "Importação de Fatura Concluída!", $"{imported} parcelas distribuídas em suas faturas com sucesso.");
            await Navigation.PopModalAsync(); Imported?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Importação não realizada", SqliteErrorMessage.ToFriendly(ex), "Fechar"); }
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
