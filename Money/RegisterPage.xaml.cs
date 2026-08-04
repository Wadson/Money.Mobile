using Money.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace Money;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;

    public RegisterPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        BirthDatePicker.Date = DateTime.Today.AddYears(-18);
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        if (!TermsCheck.IsChecked)
        {
            ShowError("Aceite os termos para continuar.");
            return;
        }

        if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 3)
        {
            ShowError("Informe seu nome completo (mínimo 3 caracteres).");
            NameEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !EmailEntry.Text.Contains("@"))
        {
            ShowError("Informe um e-mail válido.");
            EmailEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text) || PasswordEntry.Text.Length < 8)
        {
            ShowError("A senha deve ter pelo menos 8 caracteres.");
            PasswordEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(ConfirmationEntry.Text))
        {
            ShowError("Confirme a senha.");
            ConfirmationEntry.Focus();
            return;
        }

        if (PasswordEntry.Text != ConfirmationEntry.Text)
        {
            ShowError("As senhas não coincidem.");
            ConfirmationEntry.Focus();
            return;
        }

        if (RecoveryQuestionPicker.SelectedIndex < 0)
        {
            ShowError("Selecione uma pergunta de segurança.");
            RecoveryQuestionPicker.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(RecoveryAnswerEntry.Text) || RecoveryAnswerEntry.Text.Length < 3)
        {
            ShowError("Digite uma resposta de segurança (mínimo 3 caracteres).");
            RecoveryAnswerEntry.Focus();
            return;
        }

        try
        {
            var submitBtn = this.FindByName<Button>("SubmitButton");
            if (submitBtn != null)
            {
                submitBtn.IsEnabled = false;
                submitBtn.Text = "Criando conta...";
            }

            var result = await _auth.RegisterAsync(
                NameEntry.Text.Trim(),
                EmailEntry.Text.Trim(),
                PasswordEntry.Text,
                ConfirmationEntry.Text,
                BirthDatePicker.Date ?? DateTime.Today.AddYears(-18),
                RecoveryQuestionPicker.SelectedItem?.ToString() ?? "",
                RecoveryAnswerEntry.Text.Trim()
            );

            if (!result.Success)
            {
                ShowError(result.Message);
                if (submitBtn != null)
                {
                    submitBtn.IsEnabled = true;
                    submitBtn.Text = "Criar conta";
                }
                return;
            }

            await ThemedDialog.ShowAsync(this,
                "Conta criada com sucesso!",
                "Bem-vindo ao Money Pro! Sua jornada financeira começa agora.",
                "Continuar"
            );

            await Navigation.PopModalAsync();
            (Application.Current as App)?.ShowAuthenticatedArea();
        }
        catch (Exception ex)
        {
            ShowError($"Erro: {ex.Message}");
            var submitBtn = this.FindByName<Button>("SubmitButton");
            if (submitBtn != null)
            {
                submitBtn.IsEnabled = true;
                submitBtn.Text = "Criar conta";
            }
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSelectRecoveryQuestionClicked(object? sender, EventArgs e)
    {
        var questions = RecoveryQuestionPicker.ItemsSource?.Cast<object>()
            .Select(x => x.ToString() ?? string.Empty).ToList()
            ?? RecoveryQuestionPicker.Items.Select(x => x?.ToString() ?? string.Empty).ToList();
        var options = questions.Select((question, index) => new SelectionOption
        {
            Index = index,
            Label = question,
            Background = ThemeColor.Get("BlingCard"),
            Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = RecoveryQuestionPicker.SelectedIndex == index
        });
        var page = new OptionSelectionPage("Pergunta de segurança", options);
        page.Selected += (_, option) =>
        {
            RecoveryQuestionPicker.SelectedIndex = option.Index;
            RecoveryQuestionButton.Text = option.Label;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnTermsTapped(object sender, EventArgs e)
    {
        await ThemedDialog.ShowAsync(this,
            "Termos de Uso",
            "Aqui você pode exibir os termos de uso do aplicativo.",
            "OK"
        );
    }

    private async void OnPrivacyTapped(object sender, EventArgs e)
    {
        await ThemedDialog.ShowAsync(this,
            "Política de Privacidade",
            "Aqui você pode exibir a política de privacidade do aplicativo.",
            "OK"
        );
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
