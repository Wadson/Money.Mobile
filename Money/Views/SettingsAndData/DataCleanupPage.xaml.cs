using System.Collections.ObjectModel;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money.Views.SettingsAndData;

public partial class DataCleanupPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly ObservableCollection<DataCleanupItem> _items = [];
    private bool _updatingAll;

    public DataCleanupPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        CleanupList.ItemsSource = _items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_items.Count == 0)
            await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var counts = await _database.GetDataCleanupCountsAsync();
            _items.Clear();
            Add("Transacoes", "Lançamentos e despesas", "Despesas, parcelas e pagamentos", MaterialIcons.ReceiptLong, counts);
            Add("Receitas", "Receitas e salários", "Entradas financeiras e salários", MaterialIcons.TrendingUp, counts);
            Add("FaturasCartao", "Faturas de cartão", "Histórico de faturas", MaterialIcons.RequestQuote, counts);
            Add("CartoesCredito", "Cartões de crédito", "Cartões e limites cadastrados", MaterialIcons.CreditCard, counts);
            Add("Fornecedores", "Fornecedores", "Cadastros de fornecedores", MaterialIcons.Storefront, counts);
            Add("Notificacoes", "Notificações", "Histórico de avisos", MaterialIcons.NotificationsActive, counts);
            Add("Logs", "Logs de auditoria", "Registros de ações", MaterialIcons.History, counts);
            UpdateSelectionState();
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível carregar", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private void Add(string key, string name, string description, MaterialIcons icon,
        IReadOnlyDictionary<string, long> counts) => _items.Add(new DataCleanupItem
        {
            Key = key, Name = name, Description = description, Icon = icon,
            Count = counts.GetValueOrDefault(key)
        });

    private void OnSelectAllChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (_updatingAll) return;
        foreach (var item in _items) item.IsSelected = e.Value;
        UpdateSelectionState();
    }

    private void OnItemCheckedChanged(object? sender, CheckedChangedEventArgs e) => UpdateSelectionState();

    private void UpdateSelectionState()
    {
        var selected = _items.Count(x => x.IsSelected);
        var records = _items.Where(x => x.IsSelected).Sum(x => x.Count);
        SelectionSummaryLabel.Text = selected == 0
            ? "Nenhum histórico selecionado"
            : $"{selected} grupo(s) · {records:N0} registro(s)";
        DeleteButton.IsEnabled = selected > 0;
        _updatingAll = true;
        SelectAllCheckBox.IsChecked = _items.Count > 0 && selected == _items.Count;
        _updatingAll = false;
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var selected = _items.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) return;
        var records = selected.Sum(x => x.Count);
        var confirm = await ThemedDialog.ConfirmAsync(this, "Confirmar Exclusão Definitiva?",
            $"Você está prestes a apagar {selected.Count} histórico(s), totalizando {records:N0} registro(s). Esta ação é irreversível.",
            "Sim, Apagar Agora", "Cancelar");
        if (!confirm) return;

        DeleteButton.IsEnabled = false;
        try
        {
            await _database.CleanSelectedDataAsync(selected.Select(x => x.Key).ToArray());
            await ThemedDialog.ShowAsync(this, "Limpeza concluída", "Registros excluídos com sucesso!");
            _items.Clear();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível excluir", SqliteErrorMessage.ToFriendly(ex), "Fechar");
            UpdateSelectionState();
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
