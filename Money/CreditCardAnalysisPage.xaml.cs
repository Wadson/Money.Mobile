using System.Collections.ObjectModel;
using System.Globalization;
using Money.Models;
using Money.Services;

namespace Money;

public partial class CreditCardAnalysisPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly ObservableCollection<CreditCardAnalysis> _analyses = [];
    private List<AccountItem> _accounts = [];
    private DateTime _period = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public CreditCardAnalysisPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        CardsCarousel.ItemsSource = _analyses;
        PeriodLabel.Text = _period.ToString("MMMM 'de' yyyy", _culture);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _accounts = await _database.GetAccountsAsync();
        PaymentAccountPicker.ItemsSource = _accounts.Select(x => $"{x.Name} · {Currency(x.Balance)}").ToList();
        if (_accounts.Count > 0 && PaymentAccountPicker.SelectedIndex < 0)
            PaymentAccountPicker.SelectedIndex = 0;
        PeriodLabel.Text = InvoicePeriodLabel.Text = _period.ToString("MMMM 'de' yyyy", _culture);
        var cards = await _database.GetCreditCardAnalysisAsync(_period.Month, _period.Year);
        _analyses.Clear();
        var grouped = cards.SelectMany(x => x.Categories)
            .GroupBy(x => new { x.Category, x.Color })
            .Select(x => new CardCategorySpend(x.Key.Category, x.Key.Color, x.Sum(y => y.Amount)))
            .OrderByDescending(x => x.Amount).ToList();
        var totalLimit = cards.Sum(x => x.CreditLimit);
        var totalAvailable = cards.Sum(x => x.Available);
        var totalInvoice = cards.Sum(x => x.Invoice);
        _analyses.Add(new(null, "Todos os cartões", totalInvoice, totalLimit, totalAvailable, 0, 0, grouped));
        foreach (var card in cards) _analyses.Add(card);
        TotalInvoiceLabel.Text = Currency(totalInvoice);
        TotalLimitLabel.Text = Currency(totalLimit);
        TotalAvailableLabel.Text = Currency(totalAvailable);
        CardsCarousel.Position = 0;
        ShowSelected(_analyses[0]);
        RenderInvoices(await _database.GetCardInvoicesAsync(_period.Month, _period.Year));
    }

    private void RenderInvoices(IEnumerable<FaturaCartaoItem> invoices)
    {
        InvoicesContainer.Children.Clear();
        foreach (var invoice in invoices)
        {
            var header = new Grid
            {
                ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)],
                ColumnSpacing = 10
            };
            header.Add(new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    new Label { Text = invoice.CardName, TextColor = ThemeColor.Get("BlingPrimary"),
                        FontSize = 15, FontAttributes = FontAttributes.Bold },
                    new Label { Text = $"Fatura {invoice.ReferenceMonth:00}/{invoice.ReferenceYear}",
                        TextColor = ThemeColor.Get("BlingTextMuted"), FontSize = 10 }
                }
            });
            header.Add(new Label { Text = Currency(invoice.Total), TextColor = ThemeColor.Get("BlingText"),
                FontSize = 19, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center }, 1);

            var information = new Grid
            {
                ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star)],
                ColumnSpacing = 10, Margin = new Thickness(0, 5)
            };
            information.Add(new VerticalStackLayout
            {
                Spacing = 1, Children =
                {
                    new Label { Text = "VENCIMENTO", TextColor = ThemeColor.Get("BlingTextMuted"), FontSize = 9,
                        FontAttributes = FontAttributes.Bold },
                    new Label { Text = invoice.DueDate.ToString("dd/MM/yyyy"), TextColor = ThemeColor.Get("BlingText"),
                        FontSize = 12, FontAttributes = FontAttributes.Bold }
                }
            });
            information.Add(new VerticalStackLayout
            {
                Spacing = 1, Children =
                {
                    new Label { Text = "STATUS", TextColor = ThemeColor.Get("BlingTextMuted"), FontSize = 9,
                        FontAttributes = FontAttributes.Bold },
                    new Label { Text = invoice.Status.ToUpperInvariant(), TextColor = ThemeColor.Get("BlingPrimary"),
                        FontSize = 12, FontAttributes = FontAttributes.Bold }
                }
            }, 1);

            var actions = new Grid
            {
                ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)],
                ColumnSpacing = 8
            };
            var close = new Button { Text = invoice.Status == "aberta" ? "Fechar" : "Fechada", FontSize = 11, HeightRequest = 42,
                BackgroundColor = ThemeColor.Get("BlingCard"), TextColor = ThemeColor.Get("BlingPrimary"),
                BorderColor = ThemeColor.Get("BlingPrimary"), BorderWidth = 1, CornerRadius = 10,
                CommandParameter = invoice.Id, IsEnabled = invoice.Status == "aberta" };
            close.Clicked += OnCloseInvoiceClicked;
            actions.Add(close);
            var details = new Button { Text = "Detalhes", FontSize = 11, HeightRequest = 42,
                BackgroundColor = ThemeColor.Get("BlingCard"), TextColor = ThemeColor.Get("BlingPrimary"),
                BorderColor = ThemeColor.Get("BlingPrimary"), BorderWidth = 1, CornerRadius = 10,
                CommandParameter = invoice.Id };
            details.Clicked += OnInvoiceDetailsClicked;
            actions.Add(details, 1);
            var pay = new Button { Text = invoice.Status == "paga" ? "Pago" : "Pagar", FontSize = 11,
                HeightRequest = 42, BackgroundColor = ThemeColor.Get("BlingPrimary"),
                TextColor = ThemeColor.Get("BlingTextLight"), CornerRadius = 10,
                CommandParameter = invoice.Id, IsEnabled = invoice.Status != "paga" };
            pay.Clicked += OnPayInvoiceClicked;
            actions.Add(pay, 2);

            InvoicesContainer.Children.Add(new Border
            {
                BackgroundColor = ThemeColor.Get("BlingCard"),
                Stroke = new SolidColorBrush(ThemeColor.Get("BlingBorder")),
                StrokeThickness = 1, Padding = 14,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                Content = new VerticalStackLayout { Spacing = 10, Children = { header, information, actions } }
            });
        }
        if (InvoicesContainer.Children.Count == 0)
            InvoicesContainer.Children.Add(new Label { Text = "Nenhuma fatura gerada para este mês.", TextColor = ThemeColor.Get("BlingText"), FontSize = 11 });
    }

    private async void OnGenerateInvoicesClicked(object? sender, EventArgs e)
    {
        try { RenderInvoices(await _database.GenerateCardInvoicesAsync(_period.Month, _period.Year)); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Não foi possível gerar as faturas", SqliteErrorMessage.ToFriendly(ex), "Fechar"); }
    }

    private async void OnInvoiceDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id }) return;
        try
        {
            var items = await _database.GetInvoiceTransactionsAsync(id);
            InvoiceDetailsContainer.Children.Clear();
            InvoiceDetailsContainer.Children.Add(new Label { Text = "Lançamentos da fatura", TextColor = ThemeColor.Get("BlingPrimary"), FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 8, 0, 2) });
            foreach (var item in items)
            {
                var row = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], Padding = 7 };
                row.Add(new Label { Text = $"{item.Description}\n{item.Category} · {item.Date:dd/MM} · {item.InstallmentNumber}/{item.TotalInstallments}", TextColor = ThemeColor.Get("BlingText"), FontSize = 10 });
                row.Add(new Label { Text = Currency(item.Amount), TextColor = ThemeColor.Get("BlingText"), FontAttributes = FontAttributes.Bold, FontSize = 11 }, 1);
                InvoiceDetailsContainer.Children.Add(row);
            }
            if (items.Count == 0) InvoiceDetailsContainer.Children.Add(new Label { Text = "Nenhum lançamento vinculado.", TextColor = ThemeColor.Get("BlingText"), FontSize = 10 });
            InvoiceDetailsContainer.IsVisible = true;
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private async void OnCloseInvoiceClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id } ||
            !await ThemedDialog.ConfirmAsync(this, "Fechar fatura", "Após o fechamento, confirme o pagamento quando ele ocorrer.", "Fechar fatura")) return;
        try { await _database.CloseCardInvoiceAsync(id); await LoadAsync(); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Fatura não fechada", SqliteErrorMessage.ToFriendly(ex), "Fechar"); }
    }

    private async void OnPayInvoiceClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id }) return;
        if (PaymentAccountPicker.SelectedIndex < 0) { await ThemedDialog.ShowAsync(this, "Selecione uma conta", "Escolha a conta usada no pagamento.", "Fechar"); return; }
        if (!await ThemedDialog.ConfirmAsync(this, "Pagar fatura", "O valor será debitado da conta selecionada. Continuar?", "Pagar")) return;
        try { await _database.PayCardInvoiceAsync(id, _accounts[PaymentAccountPicker.SelectedIndex].Id); await LoadAsync(); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Pagamento não realizado", SqliteErrorMessage.ToFriendly(ex), "Fechar"); }
    }

    private void OnCardPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        if (e.CurrentPosition >= 0 && e.CurrentPosition < _analyses.Count)
            ShowSelected(_analyses[e.CurrentPosition]);
    }

    private void ShowSelected(CreditCardAnalysis card)
    {
        SelectedCardNameLabel.Text = card.Name;
        SelectedInvoiceLabel.Text = Currency(card.Invoice);
        UsageProgress.Progress = card.Usage;
        var used = Math.Max(0, card.CreditLimit - card.Available);
        UsageLabel.Text = $"{Currency(used)} utilizado";
        AvailableLabel.Text = $"{Currency(card.Available)} disponível";
        ClosingLabel.Text = card.CardId is null ? "Vários" : $"Dia {card.ClosingDay}";
        BestDayLabel.Text = card.CardId is null ? "Varia por cartão" : $"Dia {card.BestPurchaseDay}";
        CategoriesContainer.Children.Clear();
        if (card.Categories.Count == 0)
        {
            CategoriesContainer.Children.Add(new Label
            {
                Text = "Nenhum gasto em cartão neste mês.", TextColor = ThemeColor.Get("BlingText"), FontSize = 11
            });
            return;
        }
        var max = card.Categories.Max(x => x.Amount);
        foreach (var category in card.Categories)
        {
            var header = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
            header.Add(new Label { Text = category.Category, TextColor = ThemeColor.Get("BlingText"),
                FontSize = 11, FontAttributes = FontAttributes.Bold });
            header.Add(new Label { Text = Currency(category.Amount), TextColor = ThemeColor.Get("BlingText"),
                FontSize = 10 }, 1);
            CategoriesContainer.Children.Add(new VerticalStackLayout
            {
                Spacing = 4, Children =
                {
                    header,
                    new ProgressBar { Progress = max == 0 ? 0 : (double)(category.Amount/max),
                        ProgressColor = ThemeColor.Parse(category.Color), BackgroundColor = ThemeColor.Get("BlingCard"),
                        HeightRequest = 7 }
                }
            });
        }
    }

    private async void OnRefresh(object? sender, EventArgs e)
    {
        try { await LoadAsync(); }
        finally { RefreshContent.IsRefreshing = false; }
    }
    private async void OnPreviousMonthClicked(object? sender, EventArgs e) { _period = _period.AddMonths(-1); await LoadAsync(); }
    private async void OnNextMonthClicked(object? sender, EventArgs e) { _period = _period.AddMonths(1); await LoadAsync(); }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
    private string Currency(decimal value) => value.ToString("C2", _culture);
}
