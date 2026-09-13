using System.Globalization;
using MauiIcons.Core;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money.Views.Dashboard;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly AuthService _auth;
    private readonly BackupService _backup;
    private readonly FinancialForecastService _forecast;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private DateTime _selectedPeriod = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _balanceVisible = true;
    private decimal _currentMonthResult;
    private int _refreshVersion;

    public MainPage(DatabaseService database, AuthService auth, BackupService backup,
        FinancialForecastService forecast)
    {
        InitializeComponent();
        _database = database;
        _auth = auth;
        _backup = backup;
        _forecast = forecast;
        UpdateGreeting();
        UpdateThemeToggleAppearance();
        ApplyThemeMetrics();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateGreeting();
        UpdateThemeToggleAppearance();
        ApplyThemeMetrics();
        try
        {
            await _database.InitializeAsync();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível carregar o painel",
                SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private void UpdateGreeting()
    {
        var greeting = DateTime.Now.Hour switch
        {
            < 12 => "Bom dia!",
            < 18 => "Boa tarde!",
            _ => "Boa noite!"
        };
        GreetingLabel.Text = string.IsNullOrWhiteSpace(_auth.UserName)
            ? greeting
            : $"{greeting} {_auth.UserName}";
    }

    /// <summary>
    /// Mantém as métricas específicas do tema claro em um único ponto e restaura
    /// explicitamente as medidas originais quando o tema escuro é ativado.
    /// </summary>
    private void ApplyThemeMetrics()
    {
        SelectedPeriodLabel.FontSize = 17;

        var panelLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Fluxo previsto", "Entradas", "Saídas", "Maiores gastos por categoria",
            "Pendências que pedem atenção", "Ver"
        };

        foreach (var element in this.GetVisualTreeDescendants())
        {
            if (element is Label label)
            {
                if (label.Text is "Entradas realizadas" or "Contas do mês")
                {
                    label.FontSize = 14;
                    label.LineBreakMode = LineBreakMode.NoWrap;
                    label.MaxLines = 1;
                }
                else if (panelLabels.Contains(label.Text ?? string.Empty))
                {
                    label.FontSize = 13;
                    label.LineBreakMode = label.Text == "Ver" ? LineBreakMode.NoWrap : LineBreakMode.TailTruncation;
                    label.MaxLines = 1;
                }
            }
            else if (element is Button button && (button.Text is "Despesa" or "Receita"))
            {
                button.HeightRequest = 36;
                button.Padding = new Thickness(8, 2);
            }
        }
    }

    private async Task RefreshAsync()
    {
        var refreshVersion = ++_refreshVersion;
        var period = _selectedPeriod;
        SelectedPeriodLabel.Text = _culture.TextInfo.ToTitleCase(period.ToString("MMMM / yyyy", _culture));
        ProjectionPeriodLabel.Text = _culture.TextInfo.ToTitleCase(period.ToString("MMMM / yyyy", _culture));
        IndicatorsPeriodLabel.Text = _culture.TextInfo.ToTitleCase(period.ToString("MMMM / yyyy", _culture));
        ForecastValuesGrid.IsVisible = false;
        ProjectionRiskLabel.Text = "CARREGANDO";
        ProjectionRiskLabel.TextColor = ThemeColor.Get("BlingPrimary");
        ProjectionRiskDetailLabel.Text = "Consultando o mês selecionado...";
        var primaryCardLoaded = false;
        try
        {
            var month = period.Month;
            var year = period.Year;
            var realizedTotalsTask = _database.GetMonthlyRealizedTotalsAsync(month, year);
            var dashboardTask = _database.GetDashboardAsync(month, year);
            var overviewTask = _database.GetMonthlyOverviewAsync(month, year);
            var payablesTask = _database.GetContasPagarAsync(month, year, false);
            var categoriesTask = _database.GetCategoryExpenseReportAsync(month, year);
            var projectionTask = _forecast.CalculateAsync(period, 1);
            await Task.WhenAll(realizedTotalsTask, dashboardTask, overviewTask, payablesTask, categoriesTask, projectionTask);
            // A delayed request must not repaint the screen after the user selects another month.
            if (refreshVersion != _refreshVersion) return;

            var realizedTotals = await realizedTotalsTask;
            var totalReceitas = realizedTotals?.TotalReceitas ?? 0.00m;
            var totalDespesas = realizedTotals?.TotalDespesas ?? 0.00m;
            _currentMonthResult = totalReceitas - totalDespesas;
            UpdateBalanceVisibility();
            SelectedPeriodLabel.Text = _culture.TextInfo.ToTitleCase(
                period.ToString("MMMM / yyyy", _culture));
            MonthlyIncomeLabel.Text = Currency(totalReceitas);
            MonthlyExpensesLabel.Text = Currency(totalDespesas);
            primaryCardLoaded = true;

            var dashboard = await dashboardTask;
            var monthly = await overviewTask;
            var payables = await payablesTask;
            var categories = await categoriesTask;
            var projection = await projectionTask;
            var projectedMonth = projection.Months.Single();
            UpdateMonthlyIndicators(dashboard);
            ForecastIncomeLabel.Text = Currency(projectedMonth.TotalIncome);
            ForecastExpensesLabel.Text = Currency(projectedMonth.TotalExpenses);
            ExpectedIncomeLabel.Text = Currency(projectedMonth.ExpectedIncome);
            ExpectedExpensesLabel.Text = Currency(projectedMonth.ExpectedExpenses);
            ForecastValuesGrid.IsVisible = true;
            var difference = projectedMonth.ProjectedResult;
            ProjectionRiskLabel.Text = !projectedMonth.HasActivity ? "SEM LANÇAMENTOS"
                : difference < 0 ? "DESPESAS MAIORES"
                : difference == 0 ? "EQUILIBRADO" : "RECEITAS COBREM AS DESPESAS";
            ProjectionRiskLabel.TextColor = difference < 0 ? ThemeColor.Get("BlingDanger") : ThemeColor.Get("BlingPrimary");
            ProjectionRiskDetailLabel.Text = !projectedMonth.HasActivity
                ? "Nenhuma receita ou despesa cadastrada para este mês."
                : difference < 0 ? "As despesas superam as receitas cadastradas neste mês."
                : difference == 0 ? "As receitas e despesas cadastradas têm o mesmo valor."
                : "As receitas cadastradas são suficientes para as despesas deste mês.";

            var scale = Math.Max(monthly.Income, monthly.TotalExpenses);
            IncomeProgress.Progress = scale <= 0 ? 0 : Math.Clamp((double)(monthly.Income / scale), 0, 1);
            ExpenseProgress.Progress = scale <= 0 ? 0 : Math.Clamp((double)(monthly.TotalExpenses / scale), 0, 1);
            FlowStatusLabel.Text = $"{monthly.Commitment:N0}% comprometido";
            FlowStatusLabel.TextColor = monthly.Commitment > 90
                ? ThemeColor.Get("BlingDanger") : ThemeColor.Get("BlingPrimary");
            FlowDetailLabel.Text = $"Pagas {Currency(monthly.PaidExpenses)} · Pendentes {Currency(monthly.PendingExpenses)}";

            BuildCategories(categories.Take(3));
            BuildPendingAlert(payables);
            BuildTransactions(dashboard.RecentTransactions.Take(5));
        }
        catch (Exception ex)
        {
            if (refreshVersion != _refreshVersion) return;
            ForecastValuesGrid.IsVisible = false;
            ProjectionRiskLabel.Text = "INDISPONÍVEL";
            ProjectionRiskDetailLabel.Text = "Não foi possível consultar este mês. Puxe a tela para tentar novamente.";
            // Uma falha secundária não pode apagar um card principal já calculado.
            if (!primaryCardLoaded)
            {
                _currentMonthResult = 0.00m;
                MonthlyIncomeLabel.Text = Currency(0.00m);
                MonthlyExpensesLabel.Text = Currency(0.00m);
                UpdateBalanceVisibility();
            }
            System.Diagnostics.Debug.WriteLine($"[MainPage.RefreshAsync] {ex}");
        }
    }

    private void UpdateMonthlyIndicators(DashboardSummary dashboard)
    {
        SavingsIndicatorLabel.Text = Currency(dashboard.Savings);
        SavingsIndicatorLabel.TextColor = dashboard.Savings < 0
            ? ThemeColor.Get("BlingDanger") : ThemeColor.Get("BlingPrimary");
        SavingsRateIndicatorLabel.Text = dashboard.Income <= 0
            ? "Não calculável" : $"{dashboard.SavingsRate:N1}%";
        DailyExpenseIndicatorLabel.Text = Currency(dashboard.DailyAverage);
        FinancialScoreIndicatorLabel.Text = dashboard.Income <= 0
            ? "Sem dados" : $"{dashboard.FinancialScore}/100";
    }

    private void BuildCategories(IEnumerable<CategoryExpenseReport> source)
    {
        CategoriesContainer.Children.Clear();
        var categories = source.ToList();
        if (categories.Count == 0)
        {
            CategoriesContainer.Children.Add(new Label
            {
                Text = "Nenhum gasto realizado no período.", FontSize = 13,
                TextColor = ThemeColor.Get("BlingText")
            });
            return;
        }

        var largest = Math.Max(1m, categories.Max(item => item.Total));
        foreach (var item in categories)
        {
            var row = new Grid
            {
                ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)],
                RowDefinitions = [new(GridLength.Auto), new(GridLength.Auto)],
                RowSpacing = 3
            };
            row.Add(new Label { Text = item.Category, FontSize = 14, TextColor = ThemeColor.Get("BlingText"), LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 });
            row.Add(new Label
            {
                Text = Currency(item.Total), FontSize = 14, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.NoWrap,
                TextColor = ThemeColor.Get("BlingPrimary")
            }, 1);
            var progress = new ProgressBar
            {
                Progress = Math.Clamp((double)(item.Total / largest), 0, 1), HeightRequest = 6,
                ProgressColor = ThemeColor.Get("CategoryProgressFill"),
                BackgroundColor = ThemeColor.Get("CategoryProgressTrack")
            };
            row.Add(progress, 0, 1);
            Grid.SetColumnSpan(progress, 2);
            var tap=new TapGestureRecognizer();
            tap.Tapped+=async(_,_)=>{
                try{
                    var main=(await _database.GetMainCategoriesAsync("despesa",true)).Single(x=>x.Name==item.Category);
                    var subs=await _database.GetSubcategoriesAsync(main.Id,includeInactive:true);
                    var transactions=await _database.GetFinancialReportItemsAsync(_selectedPeriod.Month,_selectedPeriod.Year,mainCategoryId:main.Id);
                    var options=subs.Select((sub,i)=>new SelectionOption{Index=i,Label=sub.Name,Subtitle=Currency(transactions.Where(t=>t.CategoryId==sub.Id).Sum(t=>t.Amount)),ImageSource=CategoryVisualResolver.Icon(sub),Foreground=CategoryVisualResolver.Foreground(sub),Background=CategoryVisualResolver.Background(sub)});
                    await Navigation.PushModalAsync(new Money.Views.Dialogs.OptionSelectionPage(main.Name+" / Subcategorias",options));
                }catch(Exception ex){await ThemedDialog.ShowAsync(this,"Categorias",ex.Message);}
            };
            row.GestureRecognizers.Add(tap);SemanticProperties.SetDescription(row,"Detalhar subcategorias de "+item.Category);
            CategoriesContainer.Children.Add(row);
        }
    }

    private void BuildPendingAlert(IEnumerable<ContaPagar> source)
    {
        var today = DateTime.Today;
        var open = source.Where(item => !item.IsPaid).ToList();
        var overdue = open.Count(item => item.DataVencimento.Date < today);
        var dueToday = open.Count(item => item.DataVencimento.Date == today);
        PendingAlertBorder.IsVisible = overdue + dueToday > 0;
        PendingAlertLabel.Text = $"{overdue} atrasada(s) · {dueToday} vencendo hoje";
    }

    private void BuildTransactions(IEnumerable<TransactionItem> source)
    {
        TransactionsContainer.Children.Clear();
        var items = source.ToList();
        if (items.Count == 0)
        {
            TransactionsContainer.Children.Add(new Label
            {
                Text = "Nenhuma movimentação realizada no período.", FontSize = 10,
                Padding = new Thickness(4, 12), HorizontalTextAlignment = TextAlignment.Center,
                TextColor = ThemeColor.Get("BlingText")
            });
            return;
        }

        foreach (var item in items)
        {
            var income = item.Type == "receita";
            var row = new Grid
            {
                ColumnDefinitions = [new(38), new(GridLength.Star), new(GridLength.Auto)],
                ColumnSpacing = 9, Padding = new Thickness(2, 8)
            };
            row.Add(new Border
            {
                WidthRequest = 34, HeightRequest = 34, StrokeThickness = 0,
                BackgroundColor = income ? Color.FromArgb("#1800A859") : ThemeColor.Get("BlingDangerSoft"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = new MauiIcon
                {
                    Icon = income ? MaterialIcons.TrendingUp : MaterialIcons.TrendingDown,
                    IconSize = 19, IconColor = income ? ThemeColor.Get("BlingPrimary") : ThemeColor.Get("BlingDanger")
                }
            });
            row.Add(new VerticalStackLayout
            {
                Spacing = 1, VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = item.Description, FontSize = 13, FontAttributes = FontAttributes.Bold,
                        LineBreakMode = LineBreakMode.TailTruncation, TextColor = ThemeColor.Get("BlingText") },
                    new Label { Text = $"{item.Account} · {item.Category} · {item.Date:dd/MM}", FontSize = 11,
                        LineBreakMode = LineBreakMode.TailTruncation, TextColor = ThemeColor.Get("BlingText") }
                }
            }, 1);
            row.Add(new Label
            {
                Text = $"{(income ? "+" : "−")} {Currency(item.Amount)}", FontSize = 12,
                FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center,
                TextColor = income ? ThemeColor.Get("BlingPrimary") : ThemeColor.Get("BlingDanger")
            }, 2);
            TransactionsContainer.Children.Add(row);
        }
    }

    private void UpdateBalanceVisibility()
    {
        BalanceLabel.Text = _balanceVisible ? Currency(_currentMonthResult) : "R$ ••••••";
        BalanceVisibilityButton.Source = (_balanceVisible ? MaterialIcons.Visibility : MaterialIcons.VisibilityOff)
            .ToImageSource(ThemeColor.Get("BlingTextLight"), 22);
    }

    private void OnToggleBalanceVisibilityClicked(object? sender, EventArgs e)
    {
        _balanceVisible = !_balanceVisible;
        UpdateBalanceVisibility();
    }

    private async void OnPreviousMonthClicked(object? sender, EventArgs e) { _selectedPeriod = _selectedPeriod.AddMonths(-1); await RefreshAsync(); }
    private async void OnNextMonthClicked(object? sender, EventArgs e) { _selectedPeriod = _selectedPeriod.AddMonths(1); await RefreshAsync(); }
    private async void OnPullToRefresh(object? sender, EventArgs e) { try { await RefreshAsync(); } finally { MainContent.IsRefreshing = false; } }

    private async void OnNewTransactionClicked(object? sender, EventArgs e)
    {
        var form = new TransactionFormPage(_database, "despesa");
        form.Saved += async (_, _) => await RefreshAsync();
        await Navigation.PushModalAsync(form);
    }

    private async void OnNewIncomeClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new IncomeListPage(_database));
    }

    private void OnPendingAlertTapped(object? sender, TappedEventArgs e) => OnAccountsPayableClicked(sender, EventArgs.Empty);
    private async void OnViewAllTransactionsClicked(object? sender, EventArgs e) => await Navigation.PushModalAsync(new ReportPage(_database));
    private async void OnNotificationsClicked(object? sender, EventArgs e) => await Navigation.PushModalAsync(new SettingsPage(_database));
    private async void OnThemeToggleClicked(object? sender, EventArgs e)
    {
        if (Application.Current is null) return;

        var nextTheme = IsDarkTheme() ? AppTheme.Light : AppTheme.Dark;
        Application.Current.UserAppTheme = nextTheme;
        UpdateThemeToggleAppearance();
        ApplyThemeMetrics();

        try
        {
            await RefreshAsync();
            var settings = await _database.GetSettingsAsync();
            await _database.SaveSettingsAsync(settings with
            {
                Theme = nextTheme == AppTheme.Dark ? "Escuro" : "Claro"
            });
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Tema aplicado",
                $"O tema foi alterado neste dispositivo, mas não foi possível sincronizar a configuração: {SqliteErrorMessage.ToFriendly(ex)}",
                "Fechar");
        }
    }

    private void UpdateThemeToggleAppearance()
    {
        var dark = IsDarkTheme();
        ThemeToggleButton.Text = dark ? "☀" : "☾";
        SemanticProperties.SetDescription(ThemeToggleButton,
            dark ? "Ativar tema claro" : "Ativar tema escuro");
    }

    private static bool IsDarkTheme() => Application.Current?.UserAppTheme == AppTheme.Dark ||
        (Application.Current?.UserAppTheme == AppTheme.Unspecified &&
         Application.Current?.RequestedTheme == AppTheme.Dark);
    private async void OnProfileClicked(object? sender, EventArgs e) => await Navigation.PushModalAsync(new ProfilePage(_auth));
    private async void OnAccountsPayableClicked(object? sender, EventArgs e) => await OpenPageSafelyAsync(() => new AccountsPayablePage(_database));
    private async void OnMobileMoreClicked(object? sender, EventArgs e) => await OpenDrawerAsync();
    private void OnBottomHomeTapped(object? sender, TappedEventArgs e) { }
    private async void OnBottomAccountsTapped(object? sender, TappedEventArgs e) =>
        await OpenPageSafelyAsync(() => new AccountsPayablePage(_database));
    private async void OnBottomCardsTapped(object? sender, TappedEventArgs e) =>
        await OpenPageSafelyAsync(() => new CreditCardAnalysisPage(_database));
    private async void OnBottomMenuTapped(object? sender, TappedEventArgs e) => await OpenDrawerAsync();

    private async Task OpenDrawerAsync()
    {
        DrawerOverlay.IsVisible = true;
        DrawerPanel.TranslationX = Math.Max(380, Width);
        await Task.WhenAll(DrawerScrim.FadeToAsync(1, 180, Easing.CubicOut),
            DrawerPanel.TranslateToAsync(0, 0, 240, Easing.CubicOut));
    }

    private async Task CloseDrawerAsync()
    {
        await Task.WhenAll(DrawerScrim.FadeToAsync(0, 150, Easing.CubicIn),
            DrawerPanel.TranslateToAsync(Math.Max(380, Width), 0, 190, Easing.CubicIn));
        DrawerOverlay.IsVisible = false;
    }

    private async void OnCloseDrawerTapped(object? sender, TappedEventArgs e) => await CloseDrawerAsync();
    private async void OnCloseDrawerClicked(object? sender, EventArgs e) => await CloseDrawerAsync();
    private async void OnDrawerRegistrationsClicked(object? sender, EventArgs e) { await CloseDrawerAsync(); await Navigation.PushModalAsync(new MorePage(_database, _auth, _backup)); }
    private async void OnDrawerReportsClicked(object? sender, EventArgs e) { await CloseDrawerAsync(); await OpenPageSafelyAsync(() => new ReportPage(_database)); }
    private bool _openingPage;
    private async Task OpenPageSafelyAsync(Func<Page> createPage)
    {
        if (_openingPage) return;
        _openingPage = true;
        try { await Navigation.PushModalAsync(createPage()); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await ThemedDialog.ShowAsync(this, "Não foi possível abrir a tela",
                SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
        finally { _openingPage = false; }
    }
    private async void OnDrawerBackupClicked(object? sender, EventArgs e) { await CloseDrawerAsync(); await Navigation.PushModalAsync(new BackupPage(_backup)); }
    private async void OnDrawerSettingsClicked(object? sender, EventArgs e) { await CloseDrawerAsync(); await Navigation.PushModalAsync(new SettingsPage(_database)); }
    private async void OnDrawerAboutClicked(object? sender, EventArgs e) { await CloseDrawerAsync(); await Navigation.PushModalAsync(new AboutPage()); }
    private async void OnDrawerLogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await ThemedDialog.ConfirmAsync(this, "Sair do sistema",
            "Deseja encerrar sua sessão e voltar para a tela de login?", "Sair", "Cancelar");
        if (!confirmed) return;

        await CloseDrawerAsync();
        (Application.Current as App)?.ShowLogin();
    }
    private string Currency(decimal value) => value.ToString("C2", _culture);
}
