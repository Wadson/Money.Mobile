using System.Globalization;
using MauiIcons.Core;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly AuthService _auth;
    private readonly BackupService _backup;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private DateTime _selectedPeriod = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public MainPage(DatabaseService database, AuthService auth, BackupService backup)
    {
        InitializeComponent();
        _database = database;
        _auth = auth;
        _backup = backup;
        GreetingLabel.Text = DateTime.Now.Hour switch
        {
            < 12 => "Bom dia!",
            < 18 => "Boa tarde!",
            _ => "Boa noite!"
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _database.InitializeAsync();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível carregar o painel", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private async Task RefreshAsync()
    {
        var month = _selectedPeriod.Month;
        var year = _selectedPeriod.Year;
        var dashboard = await _database.GetDashboardAsync(month, year);
        var monthly = await _database.GetMonthlyOverviewAsync(month, year);
        var payables = await _database.GetContasPagarAsync(month, year);
        SelectedPeriodLabel.Text = _selectedPeriod.ToString("MMMM / yyyy", _culture);
        BalanceLabel.Text = Currency(dashboard.Balance);
        ForecastLabel.Text = Currency(dashboard.Forecast90Days);
        MonthlyIncomeLabel.Text = Currency(monthly.Income);
        MonthlyExpensesLabel.Text = Currency(monthly.TotalExpenses);
        MonthlyRemainingLabel.Text = Currency(monthly.Remaining);
        DetailIncomeLabel.Text = Currency(monthly.Income);
        DetailPaidExpensesLabel.Text = Currency(monthly.PaidExpenses);
        DetailPendingExpensesLabel.Text = Currency(monthly.PendingExpenses);
        DetailCardExpensesLabel.Text = Currency(monthly.CardInvoices);
        DetailBankBalanceLabel.Text = Currency(monthly.BankBalance);
        DetailNetResultLabel.Text = Currency(monthly.Remaining);
        var commitmentColor = monthly.Commitment < 70 ? "BlingPrimary" :
            monthly.Commitment <= 90 ? "BlingText" : "BlingText";
        CommitmentProgress.Progress = Math.Clamp((double)(monthly.Commitment / 100), 0, 1);
        CommitmentProgress.ProgressColor = ThemeColor.Get(commitmentColor);
        CommitmentPercentLabel.Text = $"{monthly.Commitment:N1}%";
        CommitmentPercentLabel.TextColor = ThemeColor.Get(commitmentColor);
        CommitmentTextLabel.Text =
            $"Você já comprometeu {monthly.Commitment:N1}% da sua renda neste mês";
        MonthlyStatusLabel.Text = monthly.Commitment < 70 ? "Saudável" :
            monthly.Commitment <= 90 ? "Atenção" : "Crítico";
        MonthlyStatusLabel.TextColor = ThemeColor.Get(commitmentColor);

        var overdueCount = payables.Count(x => x.Status == "Vencida");
        OverdueSummaryLabel.Text = overdueCount == 0
            ? "Tudo em dia por enquanto"
            : $"{overdueCount} conta{(overdueCount == 1 ? "" : "s")} vencida{(overdueCount == 1 ? "" : "s")}";
        OverdueSummaryLabel.TextColor = ThemeColor.Get(overdueCount == 0 ? "BlingText" : "BlingText");

        var nextBills = payables
            .Where(x => x.Status == "Vencida")
            .OrderBy(x => x.DataVencimento)
            .Take(4);
        BuildBills(nextBills);
        BuildBudgets(dashboard.Budgets.Take(4));
        BuildTransactions(dashboard.RecentTransactions.Take(6));
    }

    private async void OnPreviousMonthClicked(object? sender, EventArgs e)
    {
        _selectedPeriod = _selectedPeriod.AddMonths(-1);
        await RefreshAsync();
    }

    private async void OnNextMonthClicked(object? sender, EventArgs e)
    {
        _selectedPeriod = _selectedPeriod.AddMonths(1);
        await RefreshAsync();
    }

    private void BuildBills(IEnumerable<ContaPagar> source)
    {
        BillsContainer.Children.Clear();
        var items = source.ToList();
        if (items.Count == 0)
        {
            BillsContainer.Children.Add(EmptyState(MaterialIcons.CheckCircle, "Nenhuma conta vencida",
                "Todas as contas do período estão em dia."));
            return;
        }

        foreach (var item in items)
        {
            var overdue = item.Status == "Vencida";
            var grid = new Grid
            {
                ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)],
                ColumnSpacing = 12
            };
            grid.Add(new Border
            {
                HeightRequest = 42, WidthRequest = 42, StrokeThickness = 0,
                BackgroundColor = ThemeColor.Get(overdue ? "BlingCard" : "BlingCard"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Content = new MauiIcon
                {
                    Icon = overdue ? MaterialIcons.Warning : MaterialIcons.ReceiptLong,
                    IconSize = 24,
                    IconColor = ThemeColor.Get(overdue ? "BlingText" : "BlingPrimary"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            });
            grid.Add(new VerticalStackLayout
            {
                Spacing = 2, VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = item.Descricao, TextColor = ThemeColor.Get("BlingText"),
                        FontSize = 13, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation },
                    new Label { Text = $"{item.Categoria} • {item.DataVencimento:dd/MM/yyyy}",
                        TextColor = ThemeColor.Get(overdue ? "BlingText" : "BlingText"), FontSize = 10 }
                }
            }, 1);
            var payButton = new Button
            {
                Text = $"{Currency(item.Valor)}\nPagar", FontSize = 10,
                TextColor = ThemeColor.Get(overdue ? "BlingText" : "BlingText"),
                BackgroundColor = Colors.Transparent, FontAttributes = FontAttributes.Bold,
                CommandParameter = item.Id, Padding = new Thickness(5)
            };
            payButton.Clicked += OnPayBillClicked;
            grid.Add(payButton, 2);
            BillsContainer.Children.Add(new Border
            {
                BackgroundColor = ThemeColor.Get("BlingCard"), Stroke = ThemeColor.Get(overdue ? "BlingText" : "BlingText"),
                Padding = new Thickness(13), StrokeShape =
                    new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }, Content = grid
            });
        }
    }

    private void BuildBudgets(IEnumerable<BudgetProgress> source)
    {
        BudgetsContainer.Children.Clear();
        var items = source.ToList();
        if (items.Count == 0)
        {
            BudgetsContainer.Children.Add(new Label
            {
                Text = "Nenhum orçamento cadastrado.", TextColor = ThemeColor.Get("BlingText"), FontSize = 11
            });
            return;
        }
        foreach (var budget in items)
        {
            var title = new Label { Text = budget.Category, TextColor = ThemeColor.Get("BlingText"),
                FontSize = 11, FontAttributes = FontAttributes.Bold };
            var status = budget.Status switch
            {
                "estourado" => "Estourado",
                "atencao" => "Atenção",
                "cuidado" => "Acompanhe",
                _ => "OK"
            };
            var statusColor = budget.Status switch
            {
                "estourado" => "BlingText",
                "atencao" => "BlingText",
                "cuidado" => "BlingText",
                _ => "BlingPrimary"
            };
            var value = new Label
            {
                Text = $"{status} · {Currency(budget.Spent)} / {Currency(budget.Limit)}",
                TextColor = ThemeColor.Get(statusColor), FontSize = 9,
                HorizontalTextAlignment = TextAlignment.End
            };
            var header = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
            header.Add(title); header.Add(value, 1);
            BudgetsContainer.Children.Add(new VerticalStackLayout
            {
                Spacing = 4, Children =
                {
                    header,
                    new ProgressBar { Progress = budget.Percentage, HeightRequest = 7,
                        ProgressColor = ThemeColor.Get(statusColor),
                        BackgroundColor = ThemeColor.Get("BlingCard") }
                }
            });
        }
    }

    private void BuildTransactions(IEnumerable<TransactionItem> source)
    {
        TransactionsContainer.Children.Clear();
        var items = source.ToList();
        if (items.Count == 0)
        {
            TransactionsContainer.Children.Add(new Label
            {
                Text = "Nenhuma movimentação confirmada.", TextColor = ThemeColor.Get("BlingText"),
                HorizontalTextAlignment = TextAlignment.Center, Padding = new Thickness(10, 18)
            });
            return;
        }
        foreach (var item in items)
        {
            var income = item.Type == "receita";
            var row = new Grid
            {
                ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)],
                ColumnSpacing = 11, Padding = new Thickness(8, 10)
            };
            row.Add(new Border
            {
                HeightRequest = 38, WidthRequest = 38, StrokeThickness = 0,
                BackgroundColor = ThemeColor.Get(income ? "BlingCard" : "BlingCard"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Content = new Label { Text = income ? "↓" : "↑", FontSize = 17,
                    TextColor = ThemeColor.Get(income ? "BlingPrimary" : "BlingText"),
                    HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }
            });
            row.Add(new VerticalStackLayout
            {
                Spacing = 1, VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = item.Description, TextColor = ThemeColor.Get("BlingText"),
                        FontSize = 12, FontAttributes = FontAttributes.Bold },
                    new Label { Text = $"{item.Category} • {item.Date:dd/MM}",
                        TextColor = ThemeColor.Get("BlingText"), FontSize = 9 }
                }
            }, 1);
            row.Add(new Label
            {
                Text = $"{(income ? "+" : "−")} {Currency(item.Amount)}",
                TextColor = ThemeColor.Get(income ? "BlingPrimary" : "BlingText"),
                FontSize = 11, FontAttributes = FontAttributes.Bold,
                VerticalTextAlignment = TextAlignment.Center
            }, 2);
            TransactionsContainer.Children.Add(row);
        }
    }

    private static View EmptyState(MaterialIcons icon, string title, string subtitle) =>
        new Border
        {
            BackgroundColor = ThemeColor.Get("BlingCard"), Stroke = ThemeColor.Get("BlingBorder"), Padding = new Thickness(20),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Content = new VerticalStackLayout
            {
                Spacing = 4, Children =
                {
                    new MauiIcon { Icon = icon, IconSize = 24, IconColor = ThemeColor.Get("BlingPrimary"),
                        HorizontalOptions = LayoutOptions.Center },
                    new Label { Text = title, TextColor = ThemeColor.Get("BlingText"),
                        FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center },
                    new Label { Text = subtitle, TextColor = ThemeColor.Get("BlingText"), FontSize = 10,
                        HorizontalTextAlignment = TextAlignment.Center }
                }
            }
        };

    private async void OnPayBillClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !long.TryParse(button.CommandParameter?.ToString(), out var id)) return;
        if (!await ThemedDialog.ConfirmAsync(this, "Confirmar pagamento", "Deseja pagar esta conta agora?", "Pagar")) return;
        try { await _database.MarcarComoPagaAsync(id); await RefreshAsync(); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Pagamento não realizado", SqliteErrorMessage.ToFriendly(ex), "Fechar"); }
    }

    private async void OnNewTransactionClicked(object? sender, EventArgs e)
    {
        var form = new TransactionFormPage(_database, "despesa");
        form.Saved += async (_, _) => await RefreshAsync();
        await Navigation.PushModalAsync(form);
    }

    private async void OnNewIncomeClicked(object? sender, EventArgs e)
    {
        var form = new TransactionFormPage(_database, "receita");
        form.Saved += async (_, _) => await RefreshAsync();
        await Navigation.PushModalAsync(form);
    }

    private void OnQuickExpenseTapped(object? sender, TappedEventArgs e) =>
        OnNewTransactionClicked(sender, EventArgs.Empty);
    private void OnQuickIncomeTapped(object? sender, TappedEventArgs e) =>
        OnNewIncomeClicked(sender, EventArgs.Empty);
    private void OnQuickPayTapped(object? sender, TappedEventArgs e) =>
        OnAccountsPayableClicked(sender, EventArgs.Empty);
    private void OnQuickCardsTapped(object? sender, TappedEventArgs e) =>
        OnManageCardsClicked(sender, EventArgs.Empty);
    private async void OnQuickAccountsTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new AccountManagementPage(_database));

    private async void OnRefreshClicked(object? sender, EventArgs e) => await RefreshAsync();
    private async void OnPullToRefresh(object? sender, EventArgs e)
    {
        try { await RefreshAsync(); }
        finally { MainContent.IsRefreshing = false; }
    }
    private async void OnProfileClicked(object? sender, EventArgs e)
        => await Navigation.PushModalAsync(new ProfilePage(_auth));
    private async void OnToggleThemeTapped(object? sender, TappedEventArgs e)
    {
        if (Application.Current is not { } app)
            return;

        var isDark = app.UserAppTheme == AppTheme.Dark ||
                     (app.UserAppTheme == AppTheme.Unspecified && app.RequestedTheme == AppTheme.Dark);
        var nextTheme = isDark ? AppTheme.Light : AppTheme.Dark;
        var themeName = nextTheme == AppTheme.Dark ? "Escuro" : "Claro";

        app.UserAppTheme = nextTheme;
        await RefreshAsync();
        try
        {
            var settings = await _database.GetSettingsAsync();
            await _database.SaveSettingsAsync(settings with { Theme = themeName });
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Tema", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }
    private async void OnAccountsPayableClicked(object? sender, EventArgs e)
    {
        try
        {
            await _database.InitializeAsync();
            await Navigation.PushModalAsync(new AccountsPayablePage(_database));
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Contas a pagar", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }
    private async void OnIncomeClicked(object? sender, EventArgs e)
        => await Navigation.PushModalAsync(new IncomeListPage(_database));
    private async void OnManageCardsClicked(object? sender, EventArgs e)
    {
        try
        {
            await Navigation.PushModalAsync(new CardManagementPage(_database));
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Cartões", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }
    private async void OnMobileHomeClicked(object? sender, EventArgs e) => await ReturnHomeAsync();
    private async void OnBottomHomeTapped(object? sender, TappedEventArgs e) => await ReturnHomeAsync();

    private async Task ReturnHomeAsync()
    {
        if (Navigation.ModalStack.Count > 0)
            await Navigation.PopToRootAsync(false);
        await RefreshAsync();
    }
    private void OnBottomBillsTapped(object? sender, TappedEventArgs e) =>
        OnAccountsPayableClicked(sender, EventArgs.Empty);
    private void OnBottomLaunchTapped(object? sender, TappedEventArgs e) =>
        OnNewTransactionClicked(sender, EventArgs.Empty);
    private void OnBottomIncomeTapped(object? sender, TappedEventArgs e) =>
        OnIncomeClicked(sender, EventArgs.Empty);
    private void OnBottomMoreTapped(object? sender, TappedEventArgs e) =>
        OnMobileMoreClicked(sender, EventArgs.Empty);
    private async void OnBudgetsClicked(object? sender, EventArgs e)
        => await ThemedDialog.ShowAsync(this, "Orçamentos", "Os valores exibidos são atualizados com as despesas pagas do mês.", "Fechar");

    private async void OnMobileMoreClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new MorePage(_database, _auth, _backup));
    }

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        var confirm = await ThemedDialog.ConfirmAsync(this, "Fechar aplicativo",
            "Deseja realmente fechar o aplicativo?", "Fechar", "Cancelar");
        if (!confirm) return;
        var app = Application.Current;
        var window = app?.Windows.FirstOrDefault();
        if (app is not null && window is not null)
            app.CloseWindow(window);
    }

    private async Task CreateBackupAsync()
    {
        try
        {
            var path = await _backup.CreateBackupAsync();
            await ThemedDialog.ShowAsync(this, "Backup criado", $"Arquivo:\n{path}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Erro no backup", ex.Message); }
    }

    private async Task RestoreBackupAsync()
    {
        if (!await ThemedDialog.ConfirmAsync(this, "Restaurar backup", "Os dados atuais serão substituídos.", "Restaurar"))
            return;
        try
        {
            await _backup.RestoreBackupAsync();
            await ThemedDialog.ShowAsync(this, "Backup restaurado", "Reinicie o aplicativo.");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Erro na restauração", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private string Currency(decimal value) => value.ToString("C2", _culture);
}
