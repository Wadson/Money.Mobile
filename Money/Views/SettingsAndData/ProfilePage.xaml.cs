using Money.Services;
namespace Money.Views.SettingsAndData;
public partial class ProfilePage : ContentPage
{
    private readonly AuthService _auth;
    private string? _selectedQuestion;
    private bool _profileLoaded;
    public ProfilePage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        ThemeVisual.Apply(QuestionButton, VisualRole.Selector);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_profileLoaded) return;
        try
        {
            var p = await _auth.GetProfileAsync(); NameEntry.Text=p.Name; EmailEntry.Text=p.Email;
            BirthPicker.Date=p.BirthDate ?? new DateTime(1990,1,1);
            _selectedQuestion = p.Question;
            QuestionButton.Text = string.IsNullOrWhiteSpace(p.Question) ? "Selecione uma pergunta" : p.Question;
            _profileLoaded = true;
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }
    private async void OnSaveClicked(object? s, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_selectedQuestion))
            {
                await ThemedDialog.ShowAsync(this, "Pergunta de segurança",
                    "Selecione uma pergunta de segurança antes de salvar.", "OK");
                return;
            }
            var r=await _auth.UpdateProfileAsync(NameEntry.Text??"",EmailEntry.Text??"",BirthPicker.Date??DateTime.Today,_selectedQuestion??"",AnswerEntry.Text);
            MessageLabel.Text=r.Message;
            MessageLabel.TextColor=ThemeColor.Get(r.Success ? "BlingPrimary" : "BlingDanger");
            MessageLabel.IsVisible=true;
            await ThemedDialog.ShowAsync(this, r.Success ? "Alterações salvas" : "Não foi possível salvar",
                r.Message, "OK");
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }
    private async void OnCloseClicked(object? s, EventArgs e)=>await Navigation.PopModalAsync();
    private async void OnSelectQuestionClicked(object? sender, EventArgs e)
    {
        var options = SecurityQuestions.All.Select((question, index) => new SelectionOption
        {
            Index = index,
            Label = question,
            Background = ThemeColor.Get("BlingCard"),
            Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = string.Equals(_selectedQuestion, question, StringComparison.Ordinal)
        });
        var page = new OptionSelectionPage("Selecione uma pergunta", options);
        page.Selected += (_, option) =>
        {
            _selectedQuestion = option.Label;
            QuestionButton.Text = option.Label;
        };
        await Navigation.PushModalAsync(page);
    }
    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await ThemedDialog.ConfirmAsync(this, "Sair da conta",
            "Deseja encerrar sua sessão neste dispositivo?", "Sair", "Cancelar");
        if (confirmed)
            (Application.Current as App)?.ShowLogin();
    }
}
