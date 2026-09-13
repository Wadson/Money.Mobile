using Money.Models;
using Money.Services;

namespace Money.Views.Management;

public partial class SupplierManagementPage : ContentPage
{
    private readonly DatabaseService _database;
    private bool _loading;

    public SupplierManagementPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _database.InitializeAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Fornecedores", SqliteErrorMessage.ToFriendly(ex));
        }
    }

    private async Task LoadAsync()
    {
        if (_loading) return;
        _loading = true;
        try { SuppliersList.ItemsSource = await _database.GetSuppliersAsync(InactiveCheck.IsChecked, SearchEntry.Text); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Fornecedores", SqliteErrorMessage.ToFriendly(ex)); }
        finally { _loading = false; MainRefresh.IsRefreshing = false; }
    }

    private async Task EditAsync(Fornecedor? supplier)
    {
        var page = new SupplierEditPage(_database, supplier);
        page.Saved += async (_, _) => await LoadAsync();
        await Navigation.PushModalAsync(page);
    }

    private async void OnNewClicked(object? sender, EventArgs e) => await EditAsync(null);
    private async void OnEditClicked(object? sender, EventArgs e) => await EditAsync((sender as Button)?.CommandParameter as Fornecedor);
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Fornecedor item) return;
        if (!await ThemedDialog.ConfirmAsync(this, "Excluir fornecedor", $"Deseja excluir {item.NomeFornecedor}? Fornecedores em uso serão apenas desativados.", "Excluir")) return;
        try { await _database.DeleteSupplierAsync(item.IdFornecedor); await LoadAsync(); await ThemedDialog.ShowAsync(this, "Fornecedor excluído", "Operação concluída com sucesso."); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }
    private async void OnFilterChanged(object? sender, EventArgs e) => await LoadAsync();
    private async void OnFilterChanged(object? sender, CheckedChangedEventArgs e) => await LoadAsync();
    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
