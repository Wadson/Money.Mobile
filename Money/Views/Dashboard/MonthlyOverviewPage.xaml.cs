using System.Globalization;
using Money.Models;
using Money.Services;

namespace Money.Views.Dashboard;

public partial class MonthlyOverviewPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private DateTime _selectedPeriod = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public MonthlyOverviewPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        PeriodLabel.Text = _selectedPeriod.ToString("MMMM / yyyy", _culture);
        var data = await _database.GetMonthlyOverviewAsync(
            _selectedPeriod.Month, _selectedPeriod.Year);
        IncomeLabel.Text = Currency(data.Income);
        ExpensesLabel.Text = Currency(data.TotalExpenses);
        RemainingLabel.Text = Currency(data.Remaining);
        RemainingLabel.TextColor = Application.Current?.UserAppTheme == AppTheme.Dark
            ? ThemeColor.Get(data.Remaining >= 0 ? "BlingTextLight" : "BlingText")
            : Colors.White;
        AccountExpensesLabel.Text = Currency(data.AccountExpenses);
        CardInvoicesLabel.Text = Currency(data.CardInvoices);
        AvailablePerDayLabel.Text = Currency(data.AvailablePerDay);
        DaysRemainingLabel.Text = $"{data.DaysRemaining} dia{(data.DaysRemaining == 1 ? "" : "s")} no período";

        var color = data.Commitment < 70 ? "BlingPrimary" : data.Commitment <= 90 ? "BlingText" : "BlingText";
        CommitmentProgress.ProgressColor = ThemeColor.Get(color);
        CommitmentProgress.Progress = Math.Clamp((double)(data.Commitment / 100), 0, 1);
        CommitmentLabel.Text = $"{data.Commitment:N1}%";
        CommitmentLabel.TextColor = ThemeColor.Get(color);
        MarginStatusLabel.Text = data.Commitment < 70 ? "Saudável" :
            data.Commitment <= 90 ? "Atenção" : "Crítico";
        // O selo fica sobre o card verde; no tema claro o branco garante contraste em todos os estados.
        MarginStatusLabel.TextColor = Application.Current?.UserAppTheme == AppTheme.Dark
            ? ThemeColor.Get(data.Commitment < 70 ? "BlingPrimary" : "BlingText")
            : Colors.White;
        CommitmentMessageLabel.Text = data.Commitment < 70
            ? "Sua renda mantém uma margem confortável."
            : data.Commitment <= 90
                ? "Sua margem está reduzida. Acompanhe os próximos gastos."
                : "A maior parte da renda está comprometida com despesas.";
    }

    private async void OnPreviousMonthClicked(object? sender, EventArgs e)
    {
        _selectedPeriod = _selectedPeriod.AddMonths(-1);
        await LoadAsync();
    }
    private async void OnNextMonthClicked(object? sender, EventArgs e)
    {
        _selectedPeriod = _selectedPeriod.AddMonths(1);
        await LoadAsync();
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
    private string Currency(decimal value) => value.ToString("C2", _culture);
}
