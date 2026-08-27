using System.Globalization;

namespace Money.Views.Dialogs;

public partial class CalendarDateSelectionPage : ContentPage
{
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private DateTime _visibleMonth;
    private readonly DateTime _selected;
    public event EventHandler<DateTime>? DateSelected;

    public CalendarDateSelectionPage(string title, DateTime selected)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        _selected = selected.Date;
        _visibleMonth = new DateTime(selected.Year, selected.Month, 1);
        Render();
    }

    private void Render()
    {
        MonthLabel.Text = _culture.TextInfo.ToTitleCase(_visibleMonth.ToString("MMMM 'de' yyyy", _culture));
        DaysGrid.Children.Clear();
        var offset = (int)_visibleMonth.DayOfWeek;
        var count = DateTime.DaysInMonth(_visibleMonth.Year, _visibleMonth.Month);
        for (var day = 1; day <= count; day++)
        {
            var date = new DateTime(_visibleMonth.Year, _visibleMonth.Month, day);
            var selected = date == _selected;
            var button = new Button
            {
                Text = day.ToString(), CommandParameter = date, Padding = 0, CornerRadius = 20,
                BackgroundColor = selected ? ThemeColor.Get("BlingPrimary") : ThemeColor.Get("BlingCard"),
                TextColor = selected ? ThemeColor.Get("BlingTextLight") : ThemeColor.Get("BlingText"),
                BorderColor = date == DateTime.Today ? ThemeColor.Get("BlingPrimary") : ThemeColor.Get("BlingText"),
                BorderWidth = date == DateTime.Today || selected ? 2 : 1,
                FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None
            };
            button.Clicked += OnDayClicked;
            var position = offset + day - 1;
            Grid.SetColumn(button, position % 7);
            Grid.SetRow(button, position / 7);
            DaysGrid.Children.Add(button);
        }
    }

    private void OnPreviousMonthClicked(object? sender, EventArgs e) { _visibleMonth = _visibleMonth.AddMonths(-1); Render(); }
    private void OnNextMonthClicked(object? sender, EventArgs e) { _visibleMonth = _visibleMonth.AddMonths(1); Render(); }
    private void OnTodayClicked(object? sender, EventArgs e) => Select(DateTime.Today);
    private void OnDayClicked(object? sender, EventArgs e) { if (sender is Button { CommandParameter: DateTime date }) Select(date); }
    private async void Select(DateTime date) { DateSelected?.Invoke(this, date); await Navigation.PopModalAsync(); }
    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
