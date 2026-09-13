using Money.Models;
using Money.Services;
using MauiIcons.Core;
using MauiIcons.Material;

namespace Money.Views.Management;

public partial class UserEditorPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ManagedUser? _user;
    private string? _question;
    public event EventHandler? Saved;
    public UserEditorPage(AuthService auth, ManagedUser? user)
    {
        InitializeComponent(); _auth = auth; _user = user;
        ThemeVisual.Apply(QuestionButton, VisualRole.Selector);
        BirthPicker.Date = user?.BirthDate ?? DateTime.Today.AddYears(-18);
        if (user is null) return;
        TitleLabel.Text = "Editar usuário"; NameEntry.Text = user.Name; EmailEntry.Text = user.Email;
        _question = user.RecoveryQuestion; QuestionButton.Text = _question ?? "Selecione uma pergunta";
    }
    private async void OnQuestionClicked(object? sender, EventArgs e)
    {
        var options = SecurityQuestions.All.Select((q, i) => new SelectionOption { Index=i, Label=q,
            Background=ThemeColor.Get("BlingCard"), Foreground=ThemeColor.Get("BlingPrimary"), IsSelected=q==_question });
        var page = new OptionSelectionPage("Selecione uma pergunta", options);
        page.Selected += (_, option) => { _question=option.Label; QuestionButton.Text=option.Label; };
        await Navigation.PushModalAsync(page);
    }
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var result = await _auth.SaveManagedUserAsync(_user?.Id, NameEntry.Text??"", EmailEntry.Text??"",
            BirthPicker.Date??DateTime.Today, _question??"", AnswerEntry.Text, PasswordEntry.Text, ConfirmationEntry.Text);
        if (!result.Success) { await ThemedDialog.ShowAsync(this, "Não foi possível salvar", result.Message); return; }
        await ThemedDialog.ShowAsync(this, "Usuário salvo", result.Message);
        Saved?.Invoke(this, EventArgs.Empty); await Navigation.PopModalAsync();
    }
    private void OnPasswordEyeClicked(object? s, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        PasswordEye.ImageSource = (PasswordEntry.IsPassword ? MaterialIcons.Visibility : MaterialIcons.VisibilityOff)
            .ToImageSource(ThemeColor.Get("BlingPrimary"), 20);
    }
    private void OnConfirmationEyeClicked(object? s, EventArgs e)
    {
        ConfirmationEntry.IsPassword = !ConfirmationEntry.IsPassword;
        ConfirmationEye.ImageSource = (ConfirmationEntry.IsPassword ? MaterialIcons.Visibility : MaterialIcons.VisibilityOff)
            .ToImageSource(ThemeColor.Get("BlingPrimary"), 20);
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
