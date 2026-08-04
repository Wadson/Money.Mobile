using System.Globalization;
using Money.Models;
using Money.Services;

namespace Money;

public partial class BudgetManagementPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private DateTime _period = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private List<CategoryItem> _categories = [];
    private List<BudgetItem> _items = [];
    private long? _editingId;

    public BudgetManagementPage(DatabaseService database) { InitializeComponent(); _database = database; }
    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    private async Task LoadAsync()
    {
        try
        {
            PeriodLabel.Text = _period.ToString("MMMM / yyyy", _culture);
            _categories = await _database.GetCategoriesAsync("despesa");
            CategoryPicker.ItemsSource = _categories.Select(x => x.Name).ToList();
            _items = await _database.GetBudgetsAsync(_period.Month, _period.Year);
            BudgetsContainer.Children.Clear();
            foreach (var item in _items) BudgetsContainer.Children.Add(Card(item));
            if (_items.Count == 0) BudgetsContainer.Children.Add(new Label { Text = "Nenhum orçamento neste período.", TextColor = ThemeColor.Get("BlingText"), HorizontalTextAlignment = TextAlignment.Center, Padding = 20 });
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private View Card(BudgetItem item)
    {
        var percentage = item.Limit <= 0 ? 0 : Math.Clamp((double)(item.Spent / item.Limit), 0, 1);
        var (label, color) = item.Status switch
        {
            "estourado" => ("Estourado", "BlingText"),
            "atencao" => ("Atenção", "BlingText"),
            "cuidado" => ("Cuidado", "BlingText"),
            _ => ("OK", "BlingPrimary")
        };
        var actions = new HorizontalStackLayout { Spacing = 4 };
        var edit = Action("Editar", "BlingCard", "BlingPrimary", item.Id); edit.Clicked += OnEditClicked;
        var delete = Action("Excluir", "BlingPrimary", "BlingTextLight", item.Id); delete.Clicked += OnDeleteClicked;
        actions.Children.Add(edit); actions.Children.Add(delete);
        var header = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
        header.Add(new Label { Text = item.Category, TextColor = ThemeColor.Get("BlingPrimary"), FontAttributes = FontAttributes.Bold });
        header.Add(new Label { Text = label, TextColor = ThemeColor.Get(color), FontSize = 11, FontAttributes = FontAttributes.Bold }, 1);
        return new Border
        {
            Style = (Style)Application.Current!.Resources["ContentCard"], Padding = 14,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    header,
                    new ProgressBar { Progress = percentage, ProgressColor = ThemeColor.Get(color), BackgroundColor = ThemeColor.Get("BlingCard"), HeightRequest = 9 },
                    new Label { Text = $"{item.Spent.ToString("C2", _culture)} de {item.Limit.ToString("C2", _culture)}", TextColor = ThemeColor.Get("BlingText"), FontSize = 11 },
                    actions
                }
            }
        };
    }

    private void OnNewClicked(object? sender, EventArgs e) { Clear(); FormCard.IsVisible = true; }
    private void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id }) return;
        var item = _items.Single(x => x.Id == id); _editingId = id; FormTitle.Text = "Editar orçamento";
        CategoryPicker.SelectedIndex = _categories.FindIndex(x => x.Id == item.CategoryId);
        CategorySelectionButton.Text = CategoryPicker.SelectedIndex >= 0
            ? _categories[CategoryPicker.SelectedIndex].Name
            : "Selecione a categoria";
        LimitEntry.Text = item.Limit.ToString("N2", _culture); NotesEditor.Text = item.Notes; FormCard.IsVisible = true;
    }
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (CategoryPicker.SelectedIndex < 0) { ShowError("Selecione uma categoria."); return; }
        if (!decimal.TryParse(LimitEntry.Text, NumberStyles.Currency, _culture, out var limit) || limit <= 0) { ShowError("Informe um limite maior que zero."); return; }
        try
        {
            await _database.SaveBudgetAsync(_editingId, _categories[CategoryPicker.SelectedIndex].Id,
                limit, _period.Month, _period.Year, NotesEditor.Text?.Trim());
            await ThemedDialog.ShowAsync(this, "Orçamento salvo", "Orçamento salvo com sucesso!");
            Clear(); FormCard.IsVisible = false; await LoadAsync();
        }
        catch (Exception ex) { ShowError(ex); }
    }
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id } || !await ThemedDialog.ConfirmDeleteAsync(this, "Excluir orçamento", "Deseja excluir este limite mensal?")) return;
        try { await _database.DeleteBudgetAsync(id); await LoadAsync(); await ThemedDialog.ShowAsync(this, "Orçamento excluído", "Orçamento excluído com sucesso!"); } catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }
    private async void OnSelectCategoryClicked(object? sender, EventArgs e)
    {
        var options = _categories.Select((category, index) => CategoryVisualResolver.Option(category, index,
            CategoryPicker.SelectedIndex == index)).ToList();
        var page = new OptionSelectionPage("Selecione a categoria", options);
        page.Selected += (_, option) => { CategoryPicker.SelectedIndex = option.Index; CategorySelectionButton.Text = option.Label; };
        await Navigation.PushModalAsync(page);
    }
    private async void OnPreviousClicked(object? sender, EventArgs e) { _period = _period.AddMonths(-1); await LoadAsync(); }
    private async void OnNextClicked(object? sender, EventArgs e) { _period = _period.AddMonths(1); await LoadAsync(); }
    private void OnCancelClicked(object? sender, EventArgs e) { Clear(); FormCard.IsVisible = false; }
    private void Clear() { _editingId = null; FormTitle.Text = "Novo orçamento"; CategoryPicker.SelectedIndex = -1; CategorySelectionButton.Text = "Selecione a categoria"; LimitEntry.Text = NotesEditor.Text = ""; ErrorLabel.IsVisible = false; }
    private void ShowError(Exception ex) => ShowError(SqliteErrorMessage.ToFriendly(ex));
    private void ShowError(string value) { ErrorLabel.Text = value; ErrorLabel.IsVisible = true; }
    private static Button Action(string text, string bg, string fg, long id) => new() { Text = text, FontSize = 10, HeightRequest = 36, Padding = 8, BackgroundColor = ThemeColor.Get(bg), TextColor = ThemeColor.Get(fg), CommandParameter = id };
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
