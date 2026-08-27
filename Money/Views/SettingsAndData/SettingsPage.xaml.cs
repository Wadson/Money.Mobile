using Money.Services;

namespace Money.Views.SettingsAndData;

public partial class SettingsPage : ContentPage
{
    private readonly DatabaseService _database;
    public SettingsPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        CurrencyPicker.ItemsSource = new[] { "R$", "US$", "€" };
        DateFormatPicker.ItemsSource = new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
        ThemePicker.ItemsSource = new[] { "Claro", "Escuro", "Sistema" };
        BackupFrequencyPicker.ItemsSource = new[] { "diario", "semanal", "mensal" };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var settings = await _database.GetSettingsAsync();
            CurrencyPicker.SelectedItem = settings.Currency;
            DateFormatPicker.SelectedItem = settings.DateFormat;
            ThemePicker.SelectedItem = settings.Theme;
            AlertsSwitch.IsToggled = settings.AlertsEnabled;
            AlertDaysEntry.Text = settings.DueAlertDays.ToString();
            AutomaticBackupSwitch.IsToggled = settings.AutomaticBackup;
            BackupFrequencyPicker.SelectedItem = settings.BackupFrequency;
            RenderNotifications(await _database.GetNotificationsAsync());
            RenderLogs(await _database.GetAuditLogsAsync(50));
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RenderNotifications(IEnumerable<Models.NotificationItem> items)
    {
        NotificationsContainer.Children.Clear();
        foreach (var item in items)
        {
            var button = new Button
            {
                Text = $"{(item.Read ? "Lida" : "Nova")}: {item.Title}\n{item.Message}",
                HorizontalOptions = LayoutOptions.Fill,
                FontSize = 11, TextColor = ThemeColor.Get("BlingText"),
                BackgroundColor = ThemeColor.Get(item.Read ? "BlingCard" : "BlingCard"),
                CommandParameter = item.Id
            };
            button.Clicked += OnNotificationClicked;
            NotificationsContainer.Children.Add(button);
        }
        if (NotificationsContainer.Children.Count == 0)
            NotificationsContainer.Children.Add(Empty("Nenhuma notificação."));
    }

    private void RenderLogs(IEnumerable<Models.AuditLogItem> items)
    {
        LogsContainer.Children.Clear();
        foreach (var item in items)
            LogsContainer.Children.Add(new Label
            {
                Text = $"{item.Date:dd/MM HH:mm} · {item.Action} · {item.Table ?? "Sistema"}\n{item.Details}",
                TextColor = ThemeColor.Get("BlingText"), FontSize = 10
            });
        if (LogsContainer.Children.Count == 0) LogsContainer.Children.Add(Empty("Nenhum log registrado."));
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!int.TryParse(AlertDaysEntry.Text, out var days) || days is < 0 or > 365)
        { ShowError("Informe de 0 a 365 dias de antecedência."); return; }
        try
        {
            await _database.SaveSettingsAsync(new Models.UserSettings(
                CurrencyPicker.SelectedItem?.ToString() ?? "R$",
                DateFormatPicker.SelectedItem?.ToString() ?? "dd/MM/yyyy",
                ThemePicker.SelectedItem?.ToString() ?? "Claro",
                AlertsSwitch.IsToggled, days, AutomaticBackupSwitch.IsToggled,
                BackupFrequencyPicker.SelectedItem?.ToString() ?? "semanal"));
            if (Application.Current is not null)
                Application.Current.UserAppTheme = ThemePicker.SelectedItem?.ToString() switch
                {
                    "Escuro" => AppTheme.Dark,
                    "Claro" => AppTheme.Light,
                    _ => AppTheme.Unspecified
                };
            await ThemedDialog.ShowAsync(this, "Configurações", "Preferências salvas.");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void OnNotificationClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: long id })
        { await _database.MarkNotificationReadAsync(id); OnAppearing(); }
    }

    private static Label Empty(string text) => new() { Text = text, TextColor = ThemeColor.Get("BlingText"), FontSize = 11 };
    private async void OnManageDataClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new DataCleanupPage(_database));
    private async void OnImportPayablesClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new PayablesCsvImportPage(_database));
    private void ShowError(Exception ex) => ShowError(SqliteErrorMessage.ToFriendly(ex));
    private void ShowError(string value) { ErrorLabel.Text = value; ErrorLabel.IsVisible = true; }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
