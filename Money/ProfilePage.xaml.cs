using Money.Services;
namespace Money;
public partial class ProfilePage : ContentPage
{
    private readonly AuthService _auth;
    public ProfilePage(AuthService auth) { InitializeComponent(); _auth = auth; }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var p = await _auth.GetProfileAsync(); NameEntry.Text=p.Name; EmailEntry.Text=p.Email;
            BirthPicker.Date=p.BirthDate ?? new DateTime(1990,1,1); QuestionPicker.SelectedItem=p.Question;
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }
    private async void OnSaveClicked(object? s, EventArgs e)
    {
        try
        {
            var r=await _auth.UpdateProfileAsync(NameEntry.Text??"",EmailEntry.Text??"",BirthPicker.Date??DateTime.Today,QuestionPicker.SelectedItem?.ToString()??"",AnswerEntry.Text);
            MessageLabel.Text=r.Message; MessageLabel.TextColor=ThemeColor.Get(r.Success ? "BlingPrimary" : "BlingText"); MessageLabel.IsVisible=true;
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }
    private async void OnCloseClicked(object? s, EventArgs e)=>await Navigation.PopModalAsync();
    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await ThemedDialog.ConfirmAsync(this, "Sair da conta",
            "Deseja encerrar sua sessão neste dispositivo?", "Sair", "Cancelar");
        if (confirmed)
            (Application.Current as App)?.ShowLogin();
    }
}
