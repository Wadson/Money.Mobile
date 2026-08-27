using Money.Models;
using Money.Services;

namespace Money.Views.Management;

public partial class UserManagementPage : ContentPage
{
    private readonly AuthService _auth;
    public UserManagementPage(AuthService auth) { InitializeComponent(); _auth = auth; }
    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    private async Task LoadAsync()
    {
        try
        {
            var users = await _auth.GetManagedUsersAsync();
            UsersList.ItemsSource = users;
            SummaryLabel.Text = $"{users.Count} usuário(s) ativo(s)";
        }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Usuários", SqliteErrorMessage.ToFriendly(ex)); }
        finally { RefreshControl.IsRefreshing = false; }
    }
    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();
    private async void OnNewClicked(object? sender, EventArgs e) => await OpenEditorAsync(null);
    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is ImageButton { CommandParameter: ManagedUser user }) await OpenEditorAsync(user);
    }
    private async Task OpenEditorAsync(ManagedUser? user)
    {
        var page = new UserEditorPage(_auth, user);
        page.Saved += async (_, _) => await LoadAsync();
        await Navigation.PushModalAsync(page);
    }
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not ImageButton { CommandParameter: ManagedUser user }) return;
        if (!await ThemedDialog.ConfirmAsync(this, "Excluir usuário",
            $"Deseja excluir o acesso de “{user.Name}”? Os dados financeiros serão preservados.", "Excluir", "Cancelar")) return;
        var result = await _auth.DeleteManagedUserAsync(user.Id);
        await ThemedDialog.ShowAsync(this, result.Success ? "Usuário excluído" : "Não foi possível excluir", result.Message);
        if (result.Success) await LoadAsync();
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
