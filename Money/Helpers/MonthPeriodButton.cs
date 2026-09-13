using System.Globalization;

namespace Money.Helpers;

// Reuses the app's themed month/year page; never opens a platform date dialog.
public sealed class MonthPeriodButton : Button
{
    private DateTime _date = DateTime.Today;
    public event EventHandler? DateSelected;
    public DateTime Date
    {
        get => _date;
        set { _date = value; Text = value.ToString("MMMM / yyyy", CultureInfo.GetCultureInfo("pt-BR")); }
    }

    public MonthPeriodButton()
    {
        Date = DateTime.Today;
        MinimumHeightRequest = 48;
        BackgroundColor = ThemeColor.Get("BlingCard");
        TextColor = ThemeColor.Get("BlingText");
        BorderColor = ThemeColor.Get("BlingPrimary");
        BorderWidth = 1;
        SemanticProperties.SetDescription(this, "Selecionar mês e ano");
        Clicked += async (_, _) => {
            var page = new MonthYearSelectionPage(Date.Month, Date.Year, allowAllPeriods: false);
            page.PeriodSelected += (_, period) => {
                if (period.Month is not int month || period.Year is not int year) return;
                Date = new DateTime(year, month, 1);
                DateSelected?.Invoke(this, EventArgs.Empty);
            };
            await Navigation.PushModalAsync(page);
        };
    }
}
