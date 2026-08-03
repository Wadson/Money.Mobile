using Money.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace Money;

public partial class RecoveryPage : ContentPage
{
    private readonly AuthService _auth;

    public RecoveryPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        BirthDatePicker.Date = DateTime.Today.AddYears(-18);
    }

    private async void OnVerifyClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ShowMessage("Informe o e-mail cadastrado.", false);
            return;
        }

        if (BirthDatePicker.Date == DateTime.MinValue)
        {
            ShowMessage("Selecione sua data de nascimento.", false);
            return;
        }

        try
        {
            VerifyButton.IsEnabled = false;
            VerifyButton.Text = "Verificando...";

            var result = await _auth.FindRecoveryAsync(
                EmailEntry.Text.Trim(),
                BirthDatePicker.Date ?? DateTime.MinValue
            );

            ShowMessage(result.Message, result.Success);

            if (!result.Success)
            {
                VerifyButton.IsEnabled = true;
                VerifyButton.Text = "Verificar identidade";
                return;
            }

            QuestionLabel.Text = result.Question ?? "Pergunta não encontrada";
            ResetPanel.IsVisible = true;
            EmailEntry.IsEnabled = false;
            BirthDatePicker.IsEnabled = false;
            VerifyButton.IsVisible = false;

            AnswerEntry.Focus();
        }
        catch (Exception ex)
        {
            ShowMessage($"Erro: {ex.Message}", false);
            VerifyButton.IsEnabled = true;
            VerifyButton.Text = "Verificar identidade";
        }
    }

    private async void OnResetClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AnswerEntry.Text))
        {
            ShowMessage("Digite sua resposta de segurança.", false);
            AnswerEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
        {
            ShowMessage("Digite a nova senha.", false);
            NewPasswordEntry.Focus();
            return;
        }

        if (NewPasswordEntry.Text.Length < 8)
        {
            ShowMessage("A senha deve ter pelo menos 8 caracteres.", false);
            NewPasswordEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(ConfirmationEntry.Text))
        {
            ShowMessage("Confirme a nova senha.", false);
            ConfirmationEntry.Focus();
            return;
        }

        if (NewPasswordEntry.Text != ConfirmationEntry.Text)
        {
            ShowMessage("As senhas não coincidem.", false);
            ConfirmationEntry.Focus();
            return;
        }

        try
        {
            var resetBtn = this.FindByName<Button>("ResetButton");
            if (resetBtn != null)
            {
                resetBtn.IsEnabled = false;
                resetBtn.Text = "Alterando...";
            }

            var result = await _auth.ResetPasswordAsync(
                EmailEntry.Text?.Trim() ?? "",
                BirthDatePicker.Date ?? DateTime.MinValue,
                AnswerEntry.Text.Trim(),
                NewPasswordEntry.Text,
                ConfirmationEntry.Text
            );

            ShowMessage(result.Message, result.Success);

            if (!result.Success)
            {
                if (resetBtn != null)
                {
                    resetBtn.IsEnabled = true;
                    resetBtn.Text = "Alterar senha";
                }
                return;
            }

            await ThemedDialog.ShowAsync(this,
                "Senha alterada com sucesso!",
                "Sua senha foi redefinida. Use a nova senha para acessar sua conta.",
                "Continuar"
            );

            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            ShowMessage($"Erro: {ex.Message}", false);
            var resetBtn = this.FindByName<Button>("ResetButton");
            if (resetBtn != null)
            {
                resetBtn.IsEnabled = true;
                resetBtn.Text = "Alterar senha";
            }
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void ShowMessage(string message, bool success)
    {
        MessageLabel.Text = message;
        MessageLabel.TextColor = success ? ThemeColor.Get("BlingPrimary") : ThemeColor.Get("BlingText");
        MessageLabel.IsVisible = true;
    }
}
