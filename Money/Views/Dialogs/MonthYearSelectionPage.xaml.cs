using System.Globalization;

namespace Money.Views.Dialogs;

public sealed class MonthYearSelectedEventArgs(int? month, int? year) : EventArgs
{
    public int? Month { get; } = month;
    public int? Year { get; } = year;
}

public partial class MonthYearSelectionPage : ContentPage
{
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private int _year;
    private readonly int? _selectedMonth;
    private readonly int? _selectedYear;

    public event EventHandler<MonthYearSelectedEventArgs>? PeriodSelected;

    public MonthYearSelectionPage(int? selectedMonth, int? selectedYear, bool allowAllPeriods = true)
    {
        InitializeComponent();
        AllPeriodsButton.IsVisible = allowAllPeriods;
        _selectedMonth = selectedMonth;
        _selectedYear = selectedYear;
        _year = selectedYear ?? DateTime.Today.Year;
        YearEntry.Text = _year.ToString(_culture);
        BuildMonthButtons();
    }

    private void BuildMonthButtons()
    {
        MonthsGrid.Children.Clear();
        for (var month = 1; month <= 12; month++)
        {
            var isSelected = month == _selectedMonth && _year == _selectedYear;
            var button = new Button
            {
                Text = _culture.DateTimeFormat.GetAbbreviatedMonthName(month).TrimEnd('.').ToUpper(_culture),
                CommandParameter = month,
                HeightRequest = 48,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = ThemeColor.Get(isSelected ? "BlingPrimary" : "BlingCard"),
                TextColor = ThemeColor.Get(isSelected ? "BlingTextLight" : "BlingPrimary"),
                BorderColor = ThemeColor.Get(isSelected ? "BlingPrimary" : "BlingText"),
                BorderWidth = isSelected ? 2 : 1,
                CornerRadius = 10
            };
            button.Clicked += OnMonthClicked;
            MonthsGrid.Add(button, (month - 1) % 3, (month - 1) / 3);
        }
    }

    private void ChangeYear(int delta)
    {
        _year = Math.Clamp(_year + delta, 1, 9999);
        YearEntry.Text = _year.ToString(_culture);
        BuildMonthButtons();
    }

    private void ApplyTypedYear()
    {
        if (!int.TryParse(YearEntry.Text, out var value) || value is < 1 or > 9999)
        {
            YearEntry.Text = _year.ToString(_culture);
            return;
        }
        _year = value;
        BuildMonthButtons();
    }

    private void OnPreviousYearClicked(object? sender, EventArgs e) => ChangeYear(-1);
    private void OnNextYearClicked(object? sender, EventArgs e) => ChangeYear(1);
    private void OnYearCompleted(object? sender, EventArgs e) => ApplyTypedYear();
    private void OnYearUnfocused(object? sender, FocusEventArgs e) => ApplyTypedYear();

    private async void OnMonthClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: int month }) return;
        PeriodSelected?.Invoke(this, new MonthYearSelectedEventArgs(month, _year));
        await Navigation.PopModalAsync();
    }

    private async void OnAllPeriodsClicked(object? sender, EventArgs e)
    {
        PeriodSelected?.Invoke(this, new MonthYearSelectedEventArgs(null, null));
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
