using Money.Views.Dialogs;

namespace Money.Controls;

/// <summary>
/// Campo de data do aplicativo. Ele mantém a API simples usada pelos formulários,
/// mas abre o calendário personalizado em vez do seletor do sistema operacional.
/// </summary>
public sealed class ThemedDateSelector : LeftAlignedFieldButton
{
    public static readonly BindableProperty DateProperty = BindableProperty.Create(
        nameof(Date), typeof(DateTime?), typeof(ThemedDateSelector), null,
        propertyChanged: static (bindable, oldValue, newValue) =>
            ((ThemedDateSelector)bindable).RefreshText());

    public static readonly BindableProperty MinimumDateProperty = BindableProperty.Create(
        nameof(MinimumDate), typeof(DateTime?), typeof(ThemedDateSelector), null);

    public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(
        nameof(MaximumDate), typeof(DateTime?), typeof(ThemedDateSelector), null);

    public static readonly BindableProperty FormatProperty = BindableProperty.Create(
        nameof(Format), typeof(string), typeof(ThemedDateSelector), "dd/MM/yyyy",
        propertyChanged: static (bindable, oldValue, newValue) =>
            ((ThemedDateSelector)bindable).RefreshText());

    public DateTime? Date
    {
        get => (DateTime?)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public DateTime? MinimumDate
    {
        get => (DateTime?)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public DateTime? MaximumDate
    {
        get => (DateTime?)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public event EventHandler? DateSelected;

    public ThemedDateSelector()
    {
        HorizontalOptions = LayoutOptions.Fill;
        Clicked += OnClicked;
        RefreshText();
    }

    private async void OnClicked(object? sender, EventArgs e)
    {
        if (!IsEnabled) return;
        var page = FindPage();
        if (page is null) return;

        var calendar = new CalendarDateSelectionPage("Selecione a data", Date ?? DateTime.Today);
        calendar.DateSelected += (_, selected) =>
        {
            if (MinimumDate is { } minimum && selected < minimum) selected = minimum;
            if (MaximumDate is { } maximum && selected > maximum) selected = maximum;
            Date = selected;
            DateSelected?.Invoke(this, EventArgs.Empty);
        };
        await page.Navigation.PushModalAsync(calendar);
    }

    private Page? FindPage()
    {
        Element? current = this;
        while (current is not null)
        {
            if (current is Page page) return page;
            current = current.Parent;
        }
        return Application.Current?.Windows.FirstOrDefault()?.Page;
    }

    private void RefreshText() => Text = Date?.ToString(Format) ?? "Selecione a data";
}
