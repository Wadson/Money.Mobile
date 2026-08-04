using System.Globalization;
using Money.Models;
using Money.Services;

namespace Money;

public partial class PaymentReversalPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<CategoryItem> _categories = [];
    private List<Fornecedor> _suppliers = [];
    private List<CardItem> _cards = [];
    private List<ContaPagar> _items = [];
    private int? _month, _year;
    private string? _category;
    private long? _supplierId, _cardId;

    public PaymentReversalPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_categories.Count == 0)
        {
            _categories = await _database.GetCategoriesAsync("despesa");
            _suppliers = await _database.GetSuppliersAsync();
            _cards = await _database.GetCardsAsync();
        }
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var items = await _database.GetContasPagarAsync(_month, _year, true, _supplierId, _cardId);
        if (!string.IsNullOrWhiteSpace(_category)) items = items.Where(x => x.Categoria.Equals(_category, StringComparison.OrdinalIgnoreCase)).ToList();
        var search = SearchEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(search)) items = items.Where(x => x.Descricao.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        _items = items;
        PaymentsList.ItemsSource = _items;
    }

    private async void OnSelectPeriodClicked(object? sender, EventArgs e)
    {
        var page = new MonthYearSelectionPage(_month, _year);
        page.PeriodSelected += (_, value) => { _month = value.Month; _year = value.Year; PeriodButton.Text = value.Month is int m && value.Year is int y ? new DateTime(y, m, 1).ToString("MMMM/yyyy", _culture) : "Todos os períodos"; };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectCategoryClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption> { new() { Index = 0, Label = "Todas as categorias", Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary") } };
        options.AddRange(_categories.Select((x, i) => CategoryVisualResolver.Option(x, i + 1, x.Name == _category)));
        var page = new OptionSelectionPage("Categoria", options);
        page.Selected += (_, value) => { _category = value.Index == 0 ? null : _categories[value.Index - 1].Name; CategoryButton.Text = value.Label; };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectSupplierClicked(object? sender, EventArgs e)
    {
        var options = Options("Todos os fornecedores", _suppliers.Select(x => x.NomeFornecedor));
        var page = new OptionSelectionPage("Fornecedor", options);
        page.Selected += (_, value) => { _supplierId = value.Index == 0 ? null : _suppliers[value.Index - 1].IdFornecedor; SupplierButton.Text = value.Label; };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectCardClicked(object? sender, EventArgs e)
    {
        var options = Options("Todos os cartões", _cards.Select(x => x.Name));
        var page = new OptionSelectionPage("Cartão", options);
        page.Selected += (_, value) => { _cardId = value.Index == 0 ? null : _cards[value.Index - 1].Id; CardButton.Text = value.Label; };
        await Navigation.PushModalAsync(page);
    }

    private static List<SelectionOption> Options(string all, IEnumerable<string> values) =>
        new[] { all }.Concat(values).Select((x, i) => new SelectionOption { Index = i, Label = x, Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary") }).ToList();

    private async void OnReverseClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !long.TryParse(button.CommandParameter?.ToString(), out var id)) return;
        var item = _items.FirstOrDefault(x => x.Id == id); if (item is null) return;
        if (!await ThemedDialog.ConfirmAsync(this, "Confirmar estorno", $"Estornar o pagamento de “{item.Descricao}” no valor de {item.Valor.ToString("C2", _culture)}? O valor será devolvido ao saldo da conta.", "Estornar", "Cancelar")) return;
        try { await _database.EstornarPagamentoAsync(id); await LoadAsync(); await ThemedDialog.ShowAsync(this, "Pagamento estornado", "A conta foi reaberta e o saldo atualizado com sucesso."); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Estorno não realizado", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private async void OnApplyClicked(object? sender, EventArgs e) => await LoadAsync();
    private async void OnSearchChanged(object? sender, TextChangedEventArgs e) => await LoadAsync();
    private async void OnClearClicked(object? sender, EventArgs e) { _month = _year = null; _category = null; _supplierId = _cardId = null; SearchEntry.Text = ""; PeriodButton.Text = "Todos os períodos"; CategoryButton.Text = "Todas as categorias"; SupplierButton.Text = "Todos os fornecedores"; CardButton.Text = "Todos os cartões"; await LoadAsync(); }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
