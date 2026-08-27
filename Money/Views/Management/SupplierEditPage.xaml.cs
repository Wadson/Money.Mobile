using Money.Models;
using Money.Services;

namespace Money.Views.Management;

public partial class SupplierEditPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly Fornecedor? _supplier;
    public event EventHandler? Saved;

    public SupplierEditPage(DatabaseService database, Fornecedor? supplier = null)
    {
        InitializeComponent();
        _database = database;
        _supplier = supplier;
        if (supplier is null) return;
        TitleLabel.Text = "Editar fornecedor";
        NameEntry.Text = supplier.NomeFornecedor;
        ActiveSwitch.IsToggled = supplier.IsActive;
    }

    protected override void OnAppearing() { base.OnAppearing(); Dispatcher.Dispatch(() => NameEntry.Focus()); }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        try
        {
            await _database.SaveSupplierAsync(_supplier?.IdFornecedor, NameEntry.Text ?? "", ActiveSwitch.IsToggled);
            await ThemedDialog.ShowAsync(this, "Fornecedor salvo",
                _supplier is null ? "Fornecedor cadastrado com sucesso!" : "Fornecedor alterado com sucesso!");
            Saved?.Invoke(this, EventArgs.Empty);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex) { ErrorLabel.Text = SqliteErrorMessage.ToFriendly(ex); ErrorLabel.IsVisible = true; }
    }

    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
