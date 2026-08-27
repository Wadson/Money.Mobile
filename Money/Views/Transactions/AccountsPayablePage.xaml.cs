using System.Globalization;
using System.Collections.ObjectModel;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money.Views.Transactions;

public partial class AccountsPayablePage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<ContaPagar> _items = [];
    private readonly ObservableCollection<ContaPagar> _visibleItems = [];
    private const int PageSize = 40;
    private int? _selectedMonth;
    private int? _selectedYear;
    private List<CategoryItem> _categories = [];
    private List<Fornecedor> _suppliers = [];
    private List<CardItem> _cards = [];
    private List<AccountItem> _paymentAccounts = [];
    private long? _selectedSupplierId;
    private long? _selectedCardId;
    private string? _selectedCategory;
    private long? _selectedCategoryId;
    private bool? _paidFilter = false;
    private bool _loading;

    public AccountsPayablePage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        MobileList.ItemsSource = _visibleItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _database.InitializeAsync();
            if (_categories.Count == 0)
                _categories = await _database.GetCategoriesAsync("despesa");
            if (_suppliers.Count == 0)
                _suppliers = await _database.GetSuppliersAsync();
            if (_cards.Count == 0)
                _cards = await _database.GetCardsAsync();
            if (_paymentAccounts.Count == 0)
                _paymentAccounts = await _database.GetAccountsAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = SqliteErrorMessage.ToFriendly(ex);
            ErrorLabel.IsVisible = true;
        }
    }

    private (int? Month, int? Year) SelectedPeriod() => (_selectedMonth, _selectedYear);

    private async void OnSelectPeriodClicked(object? sender, EventArgs e)
    {
        var page = new MonthYearSelectionPage(_selectedMonth, _selectedYear);
        page.PeriodSelected += (_, selected) =>
        {
            _selectedMonth = selected.Month;
            _selectedYear = selected.Year;
            PeriodSelectionButton.Text = selected.Month is int month && selected.Year is int year
                ? new DateTime(year, month, 1).ToString("MMMM/yyyy", _culture)
                : "Todos os vencimentos";
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectCategoryClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new()
            {
                Index = 0, Label = "Todas as categorias",
                Background = ThemeColor.Get("BlingCard"),
                Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = string.IsNullOrWhiteSpace(_selectedCategory)
            }
        };
        options.AddRange(_categories.Select((category, index) => new SelectionOption
        {
            Index = index + 1,
            Label = category.Name,
            ImageSource = CategoryVisualResolver.Icon(category),
            Background = CategoryVisualResolver.Background(category),
            Foreground = CategoryVisualResolver.Foreground(category),
            IsSelected = _selectedCategoryId == category.Id
        }));

        var page = new OptionSelectionPage("Selecione a categoria", options);
        page.Selected += (_, option) =>
        {
            if (option.Index == 0)
            {
                _selectedCategory = null;
                _selectedCategoryId = null;
                CategorySelectionButton.Text = "Todas as categorias";
                return;
            }
            var category = _categories[option.Index - 1];
            _selectedCategory = category.Name;
            _selectedCategoryId = category.Id;
            CategorySelectionButton.Text = category.Name;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectPaymentStatusClicked(object? sender, EventArgs e)
    {
        var options = new[]
        {
            new SelectionOption { Index = 0, Label = "Todas (abertas e pagas)", IsSelected = _paidFilter is null,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary") },
            new SelectionOption { Index = 1, Label = "Contas abertas", IsSelected = _paidFilter == false,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary") },
            new SelectionOption { Index = 2, Label = "Contas pagas", IsSelected = _paidFilter == true,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary") }
        };
        var page = new OptionSelectionPage("Selecione a situação", options);
        page.Selected += (_, option) =>
        {
            _paidFilter = option.Index switch { 0 => null, 1 => false, _ => true };
            PaymentStatusButton.Text = options[option.Index].Label;
        };
        await Navigation.PushModalAsync(page);
    }

    private async Task LoadAsync()
    {
        if (_loading) return;
        _loading = true;
        ErrorLabel.IsVisible = false;
        try
        {
            var (month, year) = SelectedPeriod();
            var items = await _database.GetContasPagarAsync(month, year, _paidFilter,
                _selectedSupplierId, _selectedCardId, _selectedCategoryId);
            _items = items;
            _visibleItems.Clear();
            AppendNextPage();
            UpdateSummaryCards(_items);
            UpdateSelectionButton();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            _loading = false;
        }
    }

    private void OnRemainingItemsThresholdReached(object? sender, EventArgs e) => AppendNextPage();

    /// <summary>
    /// Materializa os cartões em lotes. A consulta continua completa para preservar os
    /// totais, enquanto a árvore visual cria apenas o que está próximo da viewport.
    /// </summary>
    private void AppendNextPage()
    {
        if (_visibleItems.Count >= _items.Count)
            return;

        var end = Math.Min(_visibleItems.Count + PageSize, _items.Count);
        for (var index = _visibleItems.Count; index < end; index++)
            _visibleItems.Add(_items[index]);
    }

    /// <summary>Atualiza o resumo usando exclusivamente os itens exibidos pela lista.</summary>
    private void UpdateSummaryCards(IEnumerable<ContaPagar>? filteredItems)
    {
        var items = (filteredItems ?? Enumerable.Empty<ContaPagar>()).ToList();
        var pendingItems = items.Where(item => !item.IsPaid).ToList();
        var paidItems = items.Where(item => item.IsPaid).ToList();
        var today = DateTime.Today;
        var totalPending = pendingItems.Sum(item => item.Valor);
        var totalPaid = paidItems.Sum(item => item.Valor);
        var showPaidAsMain = pendingItems.Count == 0 && paidItems.Count > 0;

        MainSummaryTitleLabel.Text = showPaidAsMain ? "TOTAL PAGO" : "TOTAL PENDENTE";
        TotalValueLabel.Text = (showPaidAsMain ? totalPaid : totalPending).ToString("C2", _culture);
        TotalCountLabel.Text = (showPaidAsMain ? paidItems.Count : pendingItems.Count).ToString(_culture);
        OverdueCountLabel.Text = pendingItems.Count(item => item.DataVencimento.Date < today)
            .ToString(_culture);

        PaidSummaryBorder.IsVisible = paidItems.Count > 0 && !showPaidAsMain;
        PaidSummaryLabel.Text = $"Pago no período: {totalPaid.ToString("C2", _culture)} ({paidItems.Count} conta(s))";
        AllPaidBadge.IsVisible = showPaidAsMain;
        OverdueSummaryBorder.IsVisible = !showPaidAsMain;
    }

    private async void OnFilterClicked(object? sender, EventArgs e) => await LoadAsync();

    private async void OnClearFilterClicked(object? sender, EventArgs e)
    {
        _selectedMonth = null;
        _selectedYear = null;
        _selectedCategory = null;
        _selectedCategoryId = null;
        _paidFilter = false;
        _selectedSupplierId = null;
        _selectedCardId = null;
        PeriodSelectionButton.Text = "Todos os vencimentos";
        CategorySelectionButton.Text = "Todas as categorias";
        PaymentStatusButton.Text = "Contas abertas";
        SupplierFilterButton.Text = "Todos os fornecedores";
        CardFilterButton.Text = "Todos os cartões";
        await LoadAsync();
    }

    private async void OnSelectCardFilterClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new()
            {
                Index = 0, Label = "Todos os cartões",
                ImageSource = MauiIcons.Material.MaterialIcons.CreditCard,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = _selectedCardId is null
            }
        };
        options.AddRange(_cards.Select((card, index) => new SelectionOption
        {
            Index = index + 1, Label = card.Name,
            ImageSource = MauiIcons.Material.MaterialIcons.CreditCard,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = _selectedCardId == card.Id
        }));
        var page = new OptionSelectionPage("Selecione o cartão", options);
        page.Selected += async (_, option) =>
        {
            _selectedCardId = option.Index == 0 ? null : _cards[option.Index - 1].Id;
            CardFilterButton.Text = option.Label;
            await LoadAsync();
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectSupplierFilterClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new()
            {
                Index = 0, Label = "Todos os fornecedores",
                ImageSource = MauiIcons.Material.MaterialIcons.Storefront,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = _selectedSupplierId is null
            }
        };
        options.AddRange(_suppliers.Select((supplier, index) => new SelectionOption
        {
            Index = index + 1, Label = supplier.NomeFornecedor,
            ImageSource = MauiIcons.Material.MaterialIcons.Storefront,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = _selectedSupplierId == supplier.IdFornecedor
        }));

        var page = new OptionSelectionPage("Selecione o fornecedor", options);
        page.Selected += async (_, option) =>
        {
            _selectedSupplierId = option.Index == 0 ? null : _suppliers[option.Index - 1].IdFornecedor;
            SupplierFilterButton.Text = option.Label;
            await LoadAsync();
        };
        await Navigation.PushModalAsync(page);
    }

    private void OnItemCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox { BindingContext: ContaPagar item })
            item.Selecionado = e.Value;
        UpdateSelectionButton();
    }

    private void UpdateSelectionButton()
    {
        var selected = _items.Where(x => x.Selecionado).ToList();
        PaySelectedButton.IsVisible = selected.Count > 0;
        PaySelectedLabel.Text = selected.Count == 1
            ? "Pagar 1 selecionada"
            : $"Pagar {selected.Count} selecionadas";
    }

    private void OnEditSwipeInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem)
            OnEditClicked(new Button { CommandParameter = swipeItem.CommandParameter }, e);
    }

    private void OnDeleteSwipeInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem)
            OnDeleteClicked(new Button { CommandParameter = swipeItem.CommandParameter }, e);
    }

    private void OnPaySwipeInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem)
            OnPrimaryActionClicked(new Button { CommandParameter = swipeItem.CommandParameter }, e);
    }

    private async void OnPrimaryActionClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !long.TryParse(button.CommandParameter?.ToString(), out var id))
            return;
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item is null) return;
        if (item.IsPaid)
        {
            var reverse = await ThemedDialog.ConfirmAsync(this, "Estornar pagamento",
                $"Deseja reabrir “{item.Descricao}” e devolver {item.Valor.ToString("C2", _culture)} ao saldo da conta?",
                "Estornar", "Cancelar");
            if (!reverse) return;
            try
            {
                await _database.EstornarPagamentoAsync(id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                await ThemedDialog.ShowAsync(this, "Estorno não realizado", SqliteErrorMessage.ToFriendly(ex), "Fechar");
            }
            return;
        }
        var confirm = await ThemedDialog.ConfirmAsync(this, "Confirmar pagamento",
            $"Pagar “{item.Descricao}” no valor de {item.Valor.ToString("C2", _culture)}?",
            "Pagar", "Cancelar");
        if (!confirm) return;
        await SelectPaymentAccountAndPayAsync([id]);
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (!TryGetId(sender, out var id)) return;
        var page = new TransactionFormPage(_database, id, "despesa");
        page.Saved += async (_, _) => await LoadAsync();
        await Navigation.PushModalAsync(page);
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (!TryGetId(sender, out var id)) return;
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item is null) return;
        var confirm = await ThemedDialog.ConfirmDeleteAsync(this, "Excluir conta",
            $"Excluir permanentemente “{item.Descricao}”?");
        if (!confirm) return;
        try
        {
            await _database.DeleteListedTransactionAsync(id, "despesa");
            await LoadAsync();
            await ThemedDialog.ShowAsync(this, "Conta excluída", "Conta excluída com sucesso!");
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível excluir", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private static bool TryGetId(object? sender, out long id)
    {
        id = 0;
        var parameter = sender switch
        {
            Button button => button.CommandParameter,
            ImageButton imageButton => imageButton.CommandParameter,
            SwipeItem swipeItem => swipeItem.CommandParameter,
            _ => null
        };
        return long.TryParse(parameter?.ToString(), out id);
    }

    private async void OnPaySelectedClicked(object? sender, EventArgs e)
    {
        var selected = _items.Where(x => x.Selecionado).ToList();
        if (selected.Count == 0) return;
        var total = selected.Sum(x => x.Valor);
        var confirm = await ThemedDialog.ConfirmAsync(this, "Confirmar pagamentos",
            $"Pagar {selected.Count} contas no total de {total.ToString("C2", _culture)}?",
            "Pagar todas", "Cancelar");
        if (!confirm) return;
        await SelectPaymentAccountAndPayAsync(selected.Select(x => x.Id).ToArray());
    }

    private async Task SelectPaymentAccountAndPayAsync(IReadOnlyCollection<long> transactionIds)
    {
        if (_paymentAccounts.Count == 0)
        {
            await ThemedDialog.ShowAsync(this, "Nenhuma conta disponível",
                "Cadastre ou ative uma conta bancária antes de realizar a baixa.", "Fechar");
            return;
        }

        var options = _paymentAccounts.Select((account, index) => new SelectionOption
        {
            Index = index,
            Label = $"{account.Name} · {account.Balance.ToString("C2", _culture)}",
            ImageSource = MaterialIcons.AccountBalanceWallet,
            Background = ThemeColor.Get("BlingCard"),
            Foreground = ThemeColor.Get("BlingPrimary")
        }).ToList();
        var page = new OptionSelectionPage("Conta para pagamento", options);
        page.Selected += async (_, option) => await ExecutePaymentAsync(() =>
            _database.MarcarMultiplasComoPagaAsync(transactionIds, _paymentAccounts[option.Index].Id));
        await Navigation.PushModalAsync(page);
    }

    private async Task ExecutePaymentAsync(Func<Task> payment)
    {
        try
        {
            await payment();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Pagamento não realizado", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private async void OnNewExpenseClicked(object? sender, EventArgs e)
    {
        var form = new TransactionFormPage(_database, "despesa");
        form.Saved += async (_, _) => await ShowNewExpenseAsync();
        await Navigation.PushModalAsync(form);
    }

    private async void OnImportCsvClicked(object? sender, EventArgs e)
    {
        var page = new PayablesCsvImportPage(_database);
        page.Imported += async (_, _) => await ShowNewExpenseAsync();
        await Navigation.PushModalAsync(page);
    }

    private async Task ShowNewExpenseAsync()
    {
        // Um filtro antigo pode ocultar imediatamente a despesa recém-criada e
        // transmitir a impressão de que ela não foi salva.
        _selectedMonth = null;
        _selectedYear = null;
        _selectedCategory = null;
        _selectedCategoryId = null;
        _paidFilter = false;
        _selectedSupplierId = null;
        _selectedCardId = null;
        PeriodSelectionButton.Text = "Todos os vencimentos";
        CategorySelectionButton.Text = "Todas as categorias";
        PaymentStatusButton.Text = "Contas abertas";
        SupplierFilterButton.Text = "Todos os fornecedores";
        CardFilterButton.Text = "Todos os cartões";
        await LoadAsync();
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        // Breakpoint equivalente a uma media query: conserva o desenho e altera
        // somente o espaço útil para evitar cartões excessivamente largos no desktop.
        var desktop = Width >= 800;
        MobileList.Margin = desktop
            ? new Thickness(32, 20, 32, 92)
            : new Thickness(16, 14, 16, 86);
        MobileList.MaximumWidthRequest = desktop ? 1200 : double.PositiveInfinity;
    }

}
