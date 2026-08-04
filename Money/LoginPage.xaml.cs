using Money.Services;

namespace Money;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    public LoginPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        var result = await _auth.LoginAsync(EmailEntry.Text ?? "", PasswordEntry.Text ?? "", RememberCheck.IsChecked);
        if (!result.Success)
        {
            ErrorLabel.Text = result.Message;
            ErrorLabel.IsVisible = true;
            return;
        }
        (Application.Current as App)?.ShowAuthenticatedArea();
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
        => await Navigation.PushModalAsync(new RegisterPage(_auth));

    private async void OnForgotClicked(object? sender, EventArgs e)
        => await Navigation.PushModalAsync(new RecoveryPage(_auth));

    private void OnTogglePasswordClicked(object? sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        PasswordVisibilityButton.Text = PasswordEntry.IsPassword ? "Mostrar" : "Ocultar";
    }
}
