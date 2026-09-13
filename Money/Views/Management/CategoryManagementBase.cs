using MauiIcons.Core;
using MauiIcons.Material;
using Money.Helpers;
using Money.Models;
using Money.Services;
using Money.Views.Dialogs;

namespace Money.Views.Management;

public class CategoryManagementBase : ContentPage
{
    private readonly DatabaseService _db; private readonly bool _main;
    private readonly SearchBar _search = new() { Placeholder = "Pesquisar por nome", HeightRequest = 44 };
    private readonly Button _type = Chip("Despesa"), _status = Chip("Ativos"), _parent = Chip("Todas as categorias");
    private readonly CollectionView _list = new() { SelectionMode = SelectionMode.None, EmptyView = "Nenhum cadastro encontrado." };
    private List<MainCategoryItem> _parents = []; private List<CategoryRow> _rows = [];
    private int _typeIndex = 1, _statusIndex, _parentIndex;

    protected CategoryManagementBase(DatabaseService db, bool main)
    {
        _db = db; _main = main; Title = main ? "Categorias" : "Subcategorias";
        SetTheme(this);
        _search.SetAppThemeColor(SearchBar.BackgroundColorProperty, Color.FromArgb("#F4FAF6"), Color.FromArgb("#1E293B"));
        _search.SetAppThemeColor(SearchBar.TextColorProperty, Color.FromArgb("#1E293B"), Colors.White);
        _search.SetAppThemeColor(SearchBar.PlaceholderColorProperty, Color.FromArgb("#64748B"), Color.FromArgb("#CBD5E1"));
        var back = new Button { Text = "←", FontSize = 28, Padding = 0, WidthRequest = 44, HeightRequest = 44, BackgroundColor = Colors.Transparent };
        ThemeVisual.Apply(back, VisualRole.Selector); SemanticProperties.SetDescription(back, "Voltar"); back.Clicked += async (_, _) => await Navigation.PopModalAsync();
        var add = new Button { Text = main ? "Nova categoria" : "Nova subcategoria", HeightRequest = 44, CornerRadius = 22, FontAttributes = FontAttributes.Bold }; Primary(add); add.Clicked += async (_, _) => await EditAsync(null);
        var description = TextLabel(main ? "Organize suas despesas em grandes grupos." : "Detalhe suas categorias para classificar melhor seus lançamentos.", 15); Muted(description);
        var filters = new HorizontalStackLayout { Spacing = 7, Children = { _type, _status } }; if (!main) filters.Children.Add(_parent);
        var header = new VerticalStackLayout { Spacing = 8, Children = { back, TextLabel(Title, 28, FontAttributes.Bold), description, add, _search, filters } };
        if (main) { var planning = new Button { Text = "Orçamento e regra 60-30-10", HeightRequest = 38, CornerRadius = 12, FontSize = 13 }; Primary(planning); planning.Clicked += async (_, _) => await Navigation.PushModalAsync(new CategoryPlanningPage(db)); header.Children.Add(planning); }
        _list.ItemTemplate = new DataTemplate(CreateCard);
        var layout = new Grid { RowDefinitions = { new RowDefinition { Height = GridLength.Auto }, new RowDefinition { Height = GridLength.Star } }, Padding = new Thickness(16, 10), MaximumWidthRequest = 760 };
        layout.Add(header); layout.Add(_list, 0, 1); Content = layout;
        _search.TextChanged += (_, _) => Filter(); _type.Clicked += async (_, _) => await PickType(); _status.Clicked += async (_, _) => await PickStatus(); _parent.Clicked += async (_, _) => await PickParent();
    }
    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    private View CreateCard()
    {
        var image = new Image { WidthRequest = 40, HeightRequest = 40, VerticalOptions = LayoutOptions.Center }; image.SetBinding(Image.SourceProperty, nameof(CategoryRow.Image));
        var name = TextLabel("", 17, FontAttributes.Bold); name.MaxLines = 1; name.LineBreakMode = LineBreakMode.TailTruncation; name.SetBinding(Microsoft.Maui.Controls.Label.TextProperty, nameof(CategoryRow.Name));
        var detail = TextLabel("", 12); Muted(detail); detail.MaxLines = 1; detail.LineBreakMode = LineBreakMode.TailTruncation; detail.SetBinding(Microsoft.Maui.Controls.Label.TextProperty, nameof(CategoryRow.Detail));
        var actions = new HorizontalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { Action("Editar", MaterialIcons.Edit, false, false), Action("Desat.", MaterialIcons.VisibilityOff, false, true), Action("Excluir", MaterialIcons.Delete, true, false) } };
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition { Width = 44 }, new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = 132 } }, ColumnSpacing = 8, MinimumHeightRequest = 64 };
        grid.Add(image); grid.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { name, detail } }, 1); grid.Add(actions, 2);
        var card = new Border { Content = grid, Padding = new Thickness(12, 7), Margin = new Thickness(0, 4), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }, Shadow = new Shadow { Brush = Colors.Black, Opacity = .10f, Radius = 7, Offset = new Point(0, 2) } }; ThemeVisual.Apply(card, VisualRole.StandardCard); return card;
    }
    private View Action(string text, MaterialIcons icon, bool danger, bool deactivate)
    {
        var button = new Button { ImageSource = icon.ToImageSource(danger ? ThemeColor.Get("BlingDanger") : ThemeColor.Get("BlingPrimary"), 19), WidthRequest = 40, HeightRequest = 40, Padding = 0, CornerRadius = 12, BackgroundColor = Colors.Transparent };
        button.SetAppThemeColor(Button.BackgroundColorProperty, danger ? Color.FromArgb("#FFF1F2") : Color.FromArgb("#ECFDF3"), danger ? Color.FromArgb("#4C1D2A") : Color.FromArgb("#123524"));
        button.Clicked += async (s, _) => { if ((s as BindableObject)?.BindingContext is not CategoryRow row) return; if (text == "Editar") await EditAsync(row); else await ChangeAsync(row, !deactivate); };
        SemanticProperties.SetDescription(button, $"{(text == "Editar" ? "Editar" : deactivate ? "Desativar" : "Excluir")} {(_main ? "categoria" : "subcategoria")}");
        var caption = TextLabel(text, 9); caption.HorizontalTextAlignment = TextAlignment.Center; Muted(caption); return new VerticalStackLayout { WidthRequest = 42, Spacing = 0, Children = { button, caption } };
    }
    private async Task LoadAsync()
    {
        try { _parents = await _db.GetMainCategoriesAsync(includeInactive: true); if (_parentIndex > _parents.Count) { _parentIndex = 0; _parent.Text = "Todas as categorias"; } _rows = _main ? _parents.Select(x => new CategoryRow(x.Id, x.Name, x.Type, x.Active, $"{x.SubcategoryCount} subcategorias · {(x.Active ? "Ativa" : "Inativa")}", x, 0)).ToList() : (await _db.GetSubcategoriesAsync(includeInactive: true)).Select(x => new CategoryRow(x.Id, x.Name, x.Type, x.Active, $"{x.MainCategoryName} · {(x.Active ? "Ativa" : "Inativa")}", x, x.MainCategoryId)).ToList(); Filter(); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, Title, ex.Message); }
    }
    private void Filter() => _list.ItemsSource = _rows.Where(x => (string.IsNullOrWhiteSpace(_search.Text) || CategoryCatalog.Normalize(x.Name + " " + x.Detail).Contains(CategoryCatalog.Normalize(_search.Text))) && (_typeIndex == 0 || x.Type == (_typeIndex == 1 ? "despesa" : "receita")) && (_statusIndex == 2 || x.Active == (_statusIndex == 0)) && (_main || _parentIndex == 0 || x.MainId == _parents[_parentIndex - 1].Id)).ToList();
    private async Task PickType() { var labels = new[] { "Todos os tipos", "Despesa", "Receita" }; var icons = new[] { MaterialIcons.Category, MaterialIcons.TrendingDown, MaterialIcons.TrendingUp }; var page = new OptionSelectionPage("Tipo", labels.Select((x, i) => new SelectionOption { Index = i, Label = x, ImageSource = icons[i], Foreground = ThemeColor.Get("BlingPrimary"), IsSelected = _typeIndex == i })); page.Selected += (_, x) => { _typeIndex = x.Index; _type.Text = x.Label; Filter(); }; await Navigation.PushModalAsync(page); }
    private async Task PickStatus() { var labels = new[] { "Ativos", "Inativos", "Todos" }; var page = new OptionSelectionPage("Status", labels.Select((x, i) => new SelectionOption { Index = i, Label = x, ImageSource = i == 0 ? MaterialIcons.Visibility : i == 1 ? MaterialIcons.VisibilityOff : MaterialIcons.Category, Foreground = ThemeColor.Get("BlingPrimary"), IsSelected = _statusIndex == i })); page.Selected += (_, x) => { _statusIndex = x.Index; _status.Text = x.Label; Filter(); }; await Navigation.PushModalAsync(page); }
    private async Task PickParent() { var options = new[] { new SelectionOption { Index = 0, Label = "Todas as categorias", ImageSource = MaterialIcons.Category, Foreground = ThemeColor.Get("BlingPrimary"), IsSelected = _parentIndex == 0 } }.Concat(_parents.Select((x, i) => CategoryVisualResolver.Option(x, i + 1, _parentIndex == i + 1))); var page = new OptionSelectionPage("Categoria principal", options, allowClear: true); page.Selected += (_, x) => { _parentIndex = Math.Max(0, x.Index); _parent.Text = _parentIndex == 0 ? "Todas as categorias" : _parents[_parentIndex - 1].Name; Filter(); }; await Navigation.PushModalAsync(page); }
    private async Task EditAsync(CategoryRow? row) { var page = new CategoryEditorPage(_db, _main, row?.Visual); page.Saved += async (_, _) => await LoadAsync(); await Navigation.PushModalAsync(page); }
    private async Task ChangeAsync(CategoryRow row, bool delete) { if (!await ThemedDialog.ConfirmAsync(this, $"{(delete ? "Excluir" : "Desativar")} {row.Name}?", delete ? "A exclusão é permanente e só será permitida se não houver nenhum vínculo." : "O histórico financeiro será preservado. Este cadastro deixará de aparecer para novos lançamentos.", delete ? "Excluir" : "Desativar")) return; try { if (_main) { if (delete) await _db.DeleteMainCategoryAsync(row.Id); else await _db.DeactivateMainCategoryAsync(row.Id); } else { if (delete) await _db.DeleteSubcategoryAsync(row.Id); else await _db.DeactivateSubcategoryAsync(row.Id); } await LoadAsync(); } catch (Exception ex) { await ThemedDialog.ShowAsync(this, Title, ex.Message); } }
    private static Button Chip(string text) { var b = new Button { Text = text, HeightRequest = 36, CornerRadius = 18, Padding = new Thickness(12, 0), FontSize = 12, BackgroundColor = Colors.Transparent, BorderWidth = 1 }; b.SetAppThemeColor(Button.TextColorProperty, ThemeColor.Get("BlingPrimary"), Colors.White); b.SetAppThemeColor(Button.BorderColorProperty, ThemeColor.Get("BlingPrimary"), ThemeColor.Get("BlingPrimary")); return b; }
    private static Label TextLabel(string text, double size, FontAttributes attr = FontAttributes.None) { var l = new Microsoft.Maui.Controls.Label { Text = text, FontSize = size, FontAttributes = attr }; ThemeVisual.Apply(l, VisualRole.CommonText); return l; }
    private static void Muted(Label label) => label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#64748B"), Color.FromArgb("#CBD5E1"));
    private static void Primary(Button button) { button.BackgroundColor = ThemeColor.Get("BlingPrimary"); button.TextColor = Colors.White; }
    private static void SetTheme(Page page) => page.SetAppThemeColor(VisualElement.BackgroundColorProperty, Colors.White, Color.FromArgb("#0F172A"));
    private sealed record CategoryRow(long Id, string Name, string Type, bool Active, string Detail, ICategoryVisual Visual, long MainId) { public ImageSource Image => CategoryVisualResolver.Icon(Visual).ToImageSource(CategoryVisualResolver.Foreground(Visual), 32); }
}

internal sealed class CategoryEditorPage : ContentPage
{
    public event EventHandler? Saved;
    private readonly DatabaseService _db; private readonly bool _main; private readonly ICategoryVisual? _item;
    private readonly Entry _name = new() { Placeholder = "Nome (mínimo 2 caracteres)" }, _color = new() { Placeholder = "#00875A ou vazio para herdar" };
    private readonly Button _type = new() { HeightRequest = 44, CornerRadius = 10 }, _parent = new() { Text = "Selecione a categoria", HeightRequest = 44, CornerRadius = 10 }, _icon = new() { Text = "Selecionar ícone", HeightRequest = 44, CornerRadius = 10 }, _save = new() { Text = "Salvar", HeightRequest = 48, CornerRadius = 12 };
    private readonly Switch _active = new() { IsToggled = true }; private readonly Image _preview = new() { WidthRequest = 40, HeightRequest = 40 };
    private long? _parentId; private string _iconId = "Category"; private int _typeIndex; private bool _busy;
    public CategoryEditorPage(DatabaseService db, bool main, ICategoryVisual? item)
    {
        _db = db; _main = main; _item = item; Title = main ? "Categoria" : "Subcategoria"; this.SetAppThemeColor(VisualElement.BackgroundColorProperty, Colors.White, Color.FromArgb("#0F172A"));
        _typeIndex = item?.Type == "receita" ? 1 : 0; _type.Text = _typeIndex == 1 ? "Receita" : "Despesa";
        foreach (var b in new[] { _type, _parent, _icon }) { b.SetAppThemeColor(Button.BackgroundColorProperty, Color.FromArgb("#F8FAFC"), Color.FromArgb("#1E293B")); b.SetAppThemeColor(Button.TextColorProperty, Color.FromArgb("#1E293B"), Colors.White); b.BorderColor = ThemeColor.Get("BlingPrimary"); b.BorderWidth = 1; }
        _save.BackgroundColor = ThemeColor.Get("BlingPrimary"); _save.TextColor = Colors.White;
        var stack = new VerticalStackLayout { Spacing = 10, Padding = 20, MaximumWidthRequest = 600, Children = { new Label { Text = Title, FontSize = 24, FontAttributes = FontAttributes.Bold }, new Label { Text = "Tipo" }, _type } };
        if (!main) { stack.Children.Add(new Label { Text = "Categoria principal *" }); stack.Children.Add(_parent); }
        stack.Children.Add(new Label { Text = "Nome *" }); stack.Children.Add(_name); stack.Children.Add(_preview); stack.Children.Add(_icon); stack.Children.Add(new Label { Text = "Cor" }); stack.Children.Add(_color); stack.Children.Add(new Label { Text = "Ativo" }); stack.Children.Add(_active); stack.Children.Add(_save);
        var cancel = new Button { Text = "Cancelar", HeightRequest = 44 }; cancel.Clicked += async (_, _) => await Navigation.PopModalAsync(); stack.Children.Add(cancel); Content = new ScrollView { Content = stack };
        if (item is not null) { _name.Text = item.Name; _color.Text = item.Color; _iconId = CategoryVisualResolver.Icon(item).ToString(); }
        if (item is MainCategoryItem m) _active.IsToggled = m.Active; if (item is SubcategoryItem sub) { _parentId = sub.MainCategoryId; _parent.Text = sub.MainCategoryName; _active.IsToggled = sub.Active; } UpdateIcon();
        SemanticProperties.SetDescription(_type, "Selecionar tipo"); SemanticProperties.SetDescription(_parent, "Selecionar categoria principal"); SemanticProperties.SetDescription(_icon, "Selecionar ícone da categoria");
        _type.Clicked += async (_, _) => await SelectTypeAsync(); _parent.Clicked += async (_, _) => await SelectParentAsync(); _icon.Clicked += async (_, _) => { var page = new IconSelectionPage(); page.IconSelected += (_, id) => { _iconId = id; UpdateIcon(); }; await Navigation.PushModalAsync(page); }; _save.Clicked += SaveAsync;
    }
    private async Task SelectTypeAsync() { var page = new OptionSelectionPage("Tipo", new[] { "Despesa", "Receita" }.Select((x, i) => new SelectionOption { Index = i, Label = x, ImageSource = i == 0 ? MaterialIcons.TrendingDown : MaterialIcons.TrendingUp, Foreground = ThemeColor.Get("BlingPrimary"), IsSelected = _typeIndex == i })); page.Selected += (_, x) => { _typeIndex = x.Index; _type.Text = x.Label; _parentId = null; _parent.Text = "Selecione a categoria"; }; await Navigation.PushModalAsync(page); }
    private async Task SelectParentAsync() { try { var parents = await _db.GetMainCategoriesAsync(_typeIndex == 1 ? "receita" : "despesa"); var page = new OptionSelectionPage("Categoria principal", parents.Select((x, i) => CategoryVisualResolver.Option(x, i, x.Id == _parentId))); page.Selected += (_, x) => { _parentId = parents[x.Index].Id; _parent.Text = parents[x.Index].Name; }; await Navigation.PushModalAsync(page); } catch (Exception ex) { await ThemedDialog.ShowAsync(this, Title, ex.Message); } }
    private void UpdateIcon() => _preview.Source = Enum.Parse<MaterialIcons>(_iconId).ToImageSource(ThemeColor.Get("BlingPrimary"), 40);
    private async void SaveAsync(object? sender, EventArgs e) { if (_busy) return; _busy = true; _save.IsEnabled = false; try { var type = _typeIndex == 1 ? "receita" : "despesa"; if (_main) { if (_item is MainCategoryItem m) await _db.UpdateMainCategoryAsync(m.Id, _name.Text ?? "", type, _color.Text ?? "", _iconId, m.Order, _active.IsToggled); else await _db.AddMainCategoryAsync(_name.Text ?? "", type, _color.Text ?? "", _iconId, active: _active.IsToggled); } else { if (_parentId is null) throw new ArgumentException("Selecione a categoria principal."); if (_item is SubcategoryItem sub) await _db.UpdateSubcategoryAsync(sub.Id, _parentId.Value, _name.Text ?? "", type, _color.Text, _iconId, _active.IsToggled); else await _db.AddSubcategoryAsync(_parentId.Value, _name.Text ?? "", type, _color.Text, _iconId, _active.IsToggled); } Saved?.Invoke(this, EventArgs.Empty); await Navigation.PopModalAsync(); } catch (Exception ex) { await ThemedDialog.ShowAsync(this, Title, ex.Message); } finally { _busy = false; _save.IsEnabled = true; } }
}
