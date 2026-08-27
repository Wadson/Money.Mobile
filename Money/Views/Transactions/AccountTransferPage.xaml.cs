using System.Globalization;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money.Views.Transactions;

public partial class AccountTransferPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<AccountItem> _accounts = [];
    private int _sourceIndex = -1;
    private int _destinationIndex = -1;
    public event EventHandler? Saved;

    public AccountTransferPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        TransferDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            _accounts = await _database.GetAccountsAsync();
            if (_accounts.Count < 2)
                await ThemedDialog.ShowAsync(this, "Transferência indisponível",
                    "Cadastre ao menos duas contas ativas para transferir valores.");
        }
        catch (Exception ex) { ShowError(SqliteErrorMessage.ToFriendly(ex)); }
    }

    private async void OnSelectSourceClicked(object? sender, EventArgs e) =>
        await SelectAccountAsync(true);

    private async void OnSelectDestinationClicked(object? sender, EventArgs e) =>
        await SelectAccountAsync(false);

    private async Task SelectAccountAsync(bool source)
    {
        var selected = source ? _sourceIndex : _destinationIndex;
        var options = _accounts.Select((account, index) => new SelectionOption
        {
            Index = index,
            Label = $"{account.Name} · {account.Balance.ToString("C2", _culture)}",
            ImageSource = account.Type is "poupanca" or "investimento"
                ? MaterialIcons.Savings : MaterialIcons.AccountBalance,
            Background = ThemeColor.Get("BlingCard"),
            Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = selected == index
        });
        var page = new OptionSelectionPage(source ? "Conta de origem" : "Conta de destino", options);
        page.Selected += (_, option) =>
        {
            if (source)
            {
                _sourceIndex = option.Index;
                SourceButton.Text = $"{_accounts[option.Index].Name} · {_accounts[option.Index].Balance.ToString("C2", _culture)}";
            }
            else
            {
                _destinationIndex = option.Index;
                DestinationButton.Text = _accounts[option.Index].Name;
            }
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (_sourceIndex < 0 || _destinationIndex < 0)
        {
            await FailAsync("Selecione as contas de origem e destino.");
            return;
        }
        if (_accounts[_sourceIndex].Id == _accounts[_destinationIndex].Id)
        {
            await FailAsync("As contas de origem e destino devem ser diferentes.");
            return;
        }
        if (!decimal.TryParse(AmountEntry.Text, NumberStyles.Currency, _culture, out var amount) || amount <= 0)
        {
            await FailAsync("Informe um valor maior que zero.");
            return;
        }

        SaveButton.IsEnabled = false;
        try
        {
            var source = _accounts[_sourceIndex];
            var destination = _accounts[_destinationIndex];
            var notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim();
            await _database.AddTransferAsync(source.Id, destination.Id, amount,
                TransferDatePicker.Date ?? DateTime.Today,
                $"Transferência de {source.Name} para {destination.Name}", notes);
            await ThemedDialog.ShowAsync(this, "Transferência concluída",
                $"{amount.ToString("C2", _culture)} transferidos para {destination.Name}.");
            Saved?.Invoke(this, EventArgs.Empty);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex) { await FailAsync(SqliteErrorMessage.ToFriendly(ex)); }
        finally { SaveButton.IsEnabled = true; }
    }

    private async Task FailAsync(string message)
    {
        ShowError(message);
        await ThemedDialog.ShowAsync(this, "Não foi possível transferir", message, "Corrigir");
    }

    private void ShowError(string message) { ErrorLabel.Text = message; ErrorLabel.IsVisible = true; }
    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
