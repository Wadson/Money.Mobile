namespace Money.Views.SettingsAndData;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        VersionLabel.Text = $"v{AppInfo.Current.VersionString}";
        PlatformLabel.Text = $".NET {Environment.Version.Major} / C#";
    }

    private async void OnCloseClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
