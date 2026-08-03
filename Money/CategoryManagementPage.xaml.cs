using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using MauiIcons.Core;
using MauiIcons.Material;
using Money.Models;
using Money.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Money;

public partial class CategoryManagementPage : ContentPage
{
    private readonly DatabaseService _database;
    private List<CategoryItem> _all = new();
    private List<CategoryItem> _availableParents = new();
    private long? _editingId;
    private string _selectedType = "despesa";
    private const int CategoryPageSize = 30;
    private int _visibleCategoryCount = CategoryPageSize;
    private static readonly IReadOnlyDictionary<string, MaterialIcons> LegacyCategoryIcons =
        new Dictionary<string, MaterialIcons>(StringComparer.OrdinalIgnoreCase)
    {
        ["category_education"] = MaterialIcons.School,
        ["category_food"] = MaterialIcons.Restaurant,
        ["category_health"] = MaterialIcons.MedicalServices,
        ["category_housing"] = MaterialIcons.House,
        ["category_leisure"] = MaterialIcons.Movie,
        ["category_other"] = MaterialIcons.Category,
        ["category_shopping"] = MaterialIcons.ShoppingBag,
        ["category_taxes"] = MaterialIcons.RequestQuote,
        ["category_transport"] = MaterialIcons.DirectionsCar,
        ["category_utilities"] = MaterialIcons.Lightbulb,
        ["icon_expense"] = MaterialIcons.TrendingDown,
        ["icon_income"] = MaterialIcons.TrendingUp
    };

    public CategoryManagementPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        ColorEntry.Text = BlingPalette.PrimaryHex;
        LevelPicker.SelectedIndex = 0;

        OnTypeExpenseClicked(null, EventArgs.Empty);
        OnFilterAllClicked(null, EventArgs.Empty);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _all = await _database.GetCategoriesAsync();
        RenderList();
        UpdateParents();
        UpdateEmptyState();
    }

    private void RenderList()
    {
        CategoriesContainer.Children.Clear();

        var filter = GetCurrentFilter();
        var items = _all.Where(x => filter == "all" ||
                                   (filter == "expense" && x.Type == "despesa") ||
                                   (filter == "income" && x.Type == "receita")).ToList();
        var search = NormalizeSearchText(CategorySearchBar.Text ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(search))
            items = items.Where(x => NormalizeSearchText(x.FullPath ?? x.Name).Contains(search,
                StringComparison.OrdinalIgnoreCase)).ToList();

        if (items.Count == 0)
        {
            EmptyState.IsVisible = true;
            LoadMoreButton.IsVisible = false;
            CountLabel.Text = "Nenhuma categoria encontrada";
            return;
        }

        EmptyState.IsVisible = false;
        var ordered = items.OrderBy(x => x.Level).ThenBy(x => x.Name).ToList();
        var visibleItems = ordered.Take(_visibleCategoryCount).ToList();
        CountLabel.Text = visibleItems.Count < ordered.Count
            ? $"{ordered.Count} categorias • exibindo {visibleItems.Count}"
            : $"{ordered.Count} categorias • {ordered.Count(x => x.ParentId is null)} principais";
        LoadMoreButton.IsVisible = visibleItems.Count < ordered.Count;

        foreach (var item in visibleItems)
        {
            var card = CreateCategoryCard(item);
            CategoriesContainer.Children.Add(card);
        }
    }

    private Border CreateCategoryCard(CategoryItem item)
    {
        var indent = (item.Level - 1) * 20;

        var card = new Border
        {
            Style = (Style)Application.Current!.Resources["CardItem"],
            Padding = new Thickness(12 + indent, 10, 12, 10),
            Margin = new Thickness(0, 0, 0, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(10)
            },
            BackgroundColor = ThemeColor.Get("BlingCard"),
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Auto),
                new(GridLength.Star),
                new(GridLength.Auto),
                new(GridLength.Auto),
            },
            ColumnSpacing = 10,
            VerticalOptions = LayoutOptions.Center
        };

        var icon = new Image
        {
            Source = CategoryIconPath(item).ToImageSource(ThemeColor.Get("BlingPrimary"), 28),
            WidthRequest = 34,
            HeightRequest = 34,
            Aspect = Aspect.Center
        };
        Grid.SetColumn(icon, 0);
        grid.Children.Add(icon);

        var name = new Label
        {
            Text = item.Name,
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center
        };
        Grid.SetColumn(name, 1);
        grid.Children.Add(name);

        var levelBadge = new Border
        {
            BackgroundColor = item.Level == 1 ? ThemeColor.Get("BlingCard") : ThemeColor.Get("BlingCard"),
            Padding = new Thickness(10, 4),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(12)
            },
            StrokeThickness = 0,
            VerticalOptions = LayoutOptions.Center
        };
        var levelText = new Label
        {
            Text = item.Level == 1 ? "Principal" : $"Nível {item.Level}",
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 10,
            FontAttributes = FontAttributes.Bold
        };
        levelBadge.Content = levelText;
        Grid.SetColumn(levelBadge, 2);
        grid.Children.Add(levelBadge);

        var actionStack = new HorizontalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center
        };

        var editBtn = new Button
        {
            Text = "",
            ImageSource = MaterialIcons.Edit.ToImageSource(ThemeColor.Get("BlingPrimary"), 20),
            BackgroundColor = ThemeColor.Get("BlingCard"),
            TextColor = ThemeColor.Get("BlingPrimary"),
            FontSize = 16,
            Padding = new Thickness(4, 0),
            WidthRequest = 44,
            HeightRequest = 44
        };
        editBtn.Clicked += (_, _) => BeginEdit(item);

        var deleteBtn = new Button
        {
            Text = "",
            ImageSource = MaterialIcons.Delete.ToImageSource(ThemeColor.Get("BlingPrimary"), 20),
            BackgroundColor = ThemeColor.Get("BlingCard"),
            TextColor = ThemeColor.Get("BlingPrimary"),
            FontSize = 16,
            Padding = new Thickness(4, 0),
            WidthRequest = 44,
            HeightRequest = 44
        };
        deleteBtn.Clicked += async (_, _) => await DeleteCategory(item);

        actionStack.Children.Add(editBtn);
        actionStack.Children.Add(deleteBtn);
        Grid.SetColumn(actionStack, 3);
        grid.Children.Add(actionStack);

        card.Content = grid;
        return card;
    }

    private string GetCurrentFilter()
    {
        if (FilterAll.BackgroundColor == ThemeColor.Get("BlingPrimary"))
            return "all";
        if (FilterExpense.BackgroundColor == ThemeColor.Get("BlingPrimary"))
            return "expense";
        if (FilterIncome.BackgroundColor == ThemeColor.Get("BlingPrimary"))
            return "income";
        return "all";
    }
    private void OnQuickIconClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton { CommandParameter: string icon })
        {
            IconEntry.Text = icon;
            foreach (var button in QuickIconsContainer.Children.OfType<ImageButton>())
            {
                button.BackgroundColor = ThemeColor.Get("BlingCard");
                button.BorderColor = Colors.Transparent;
                button.BorderWidth = 0;
            }

            var selected = (ImageButton)sender;
            selected.BackgroundColor = ThemeColor.Get("BlingCard");
            selected.BorderColor = ThemeColor.Get("BlingPrimary");
            selected.BorderWidth = 2;
        }
    }
    private void ResetFilterButtons()
    {
        var inactiveBg = ThemeColor.Get("BlingCard");
        var inactiveText = ThemeColor.Get("BlingText");

        FilterAll.BackgroundColor = inactiveBg;
        FilterAll.TextColor = inactiveText;
        FilterAll.ImageSource = MaterialIcons.Category.ToImageSource(inactiveText, 18);
        FilterExpense.BackgroundColor = inactiveBg;
        FilterExpense.TextColor = inactiveText;
        FilterExpense.ImageSource = MaterialIcons.TrendingDown.ToImageSource(inactiveText, 18);
        FilterIncome.BackgroundColor = inactiveBg;
        FilterIncome.TextColor = inactiveText;
        FilterIncome.ImageSource = MaterialIcons.TrendingUp.ToImageSource(inactiveText, 18);
    }

    private void OnFilterAllClicked(object? sender, EventArgs e)
    {
        _visibleCategoryCount = CategoryPageSize;
        ResetFilterButtons();
        FilterAll.BackgroundColor = ThemeColor.Get("BlingPrimary");
        FilterAll.TextColor = ThemeColor.Get("BlingTextLight");
        FilterAll.ImageSource = MaterialIcons.Category.ToImageSource(ThemeColor.Get("BlingTextLight"), 18);
        RenderList();
        UpdateEmptyState();
    }

    private void OnFilterExpenseClicked(object? sender, EventArgs e)
    {
        _visibleCategoryCount = CategoryPageSize;
        ResetFilterButtons();
        FilterExpense.BackgroundColor = ThemeColor.Get("BlingPrimary");
        FilterExpense.TextColor = ThemeColor.Get("BlingTextLight");
        FilterExpense.ImageSource = MaterialIcons.TrendingDown.ToImageSource(ThemeColor.Get("BlingTextLight"), 18);
        RenderList();
        UpdateEmptyState();
    }

    private void OnFilterIncomeClicked(object? sender, EventArgs e)
    {
        _visibleCategoryCount = CategoryPageSize;
        ResetFilterButtons();
        FilterIncome.BackgroundColor = ThemeColor.Get("BlingPrimary");
        FilterIncome.TextColor = ThemeColor.Get("BlingTextLight");
        FilterIncome.ImageSource = MaterialIcons.TrendingUp.ToImageSource(ThemeColor.Get("BlingTextLight"), 18);
        RenderList();
        UpdateEmptyState();
    }

    private void OnLoadMoreClicked(object? sender, EventArgs e)
    {
        _visibleCategoryCount += CategoryPageSize;
        RenderList();
        UpdateEmptyState();
    }

    private void OnCategorySearchChanged(object? sender, TextChangedEventArgs e)
    {
        _visibleCategoryCount = CategoryPageSize;
        RenderList();
        UpdateEmptyState();
    }

    private void OnNewClicked(object? sender, EventArgs e)
    {
        _editingId = null;
        FormTitle.Text = "Nova categoria";
        NameEntry.Text = "";
        IconEntry.Text = "";
        LevelPicker.SelectedIndex = 0;
        ColorEntry.Text = BlingPalette.PrimaryHex;
        ColorPreview.BackgroundColor = ThemeColor.Get("BlingPrimary");
        ParentPanel.IsVisible = false;
        ErrorLabel.IsVisible = false;
        FormCard.IsVisible = true;
        NameEntry.Focus();
    }

    private void BeginEdit(CategoryItem item)
    {
        _editingId = item.Id;
        FormTitle.Text = "Editar categoria";
        LevelPicker.SelectedIndex = item.ParentId is null ? 0 : 1;
        NameEntry.Text = item.Name;
        ColorEntry.Text = item.Color ?? BlingPalette.PrimaryHex;
        ColorPreview.BackgroundColor = ThemeColor.Parse(item.Color);
        IconEntry.Text = item.Icon ?? string.Empty;
        ErrorLabel.IsVisible = false;

        if (item.Type == "receita")
            OnTypeIncomeClicked(null, EventArgs.Empty);
        else
            OnTypeExpenseClicked(null, EventArgs.Empty);

        UpdateParents();

        if (item.ParentId is not null)
        {
            var index = _availableParents.FindIndex(x => x.Id == item.ParentId);
            if (index >= 0)
                ParentPicker.SelectedIndex = index + 1;
        }

        FormCard.IsVisible = true;
        NameEntry.Focus();
    }

    private void OnHideFormClicked(object? sender, EventArgs e)
    {
        FormCard.IsVisible = false;
    }

    private void OnLevelChanged(object? sender, EventArgs e)
    {
        ParentPanel.IsVisible = LevelPicker.SelectedIndex == 1;
        if (LevelPicker.SelectedIndex == 1)
            UpdateParents();
        else
            ParentPicker.SelectedIndex = -1;
    }

    private void OnTypeExpenseClicked(object? sender, EventArgs e)
    {
        _selectedType = "despesa";
        ResetCategoryTypeButtons();
        TypeExpense.Text = "Despesa";
        TypeExpense.BackgroundColor = ThemeColor.Get("BlingPrimary");
        TypeExpense.TextColor = ThemeColor.Get("BlingTextLight");
        TypeExpense.ImageSource = MaterialIcons.TrendingDown.ToImageSource(ThemeColor.Get("BlingTextLight"), 20);
        TypeExpense.BorderColor = ThemeColor.Get("BlingPrimary");
        TypeExpense.BorderWidth = 2;
        UpdateParents();
    }

    private void OnTypeIncomeClicked(object? sender, EventArgs e)
    {
        _selectedType = "receita";
        ResetCategoryTypeButtons();
        TypeIncome.Text = "Receita";
        TypeIncome.BackgroundColor = ThemeColor.Get("BlingPrimary");
        TypeIncome.TextColor = ThemeColor.Get("BlingTextLight");
        TypeIncome.ImageSource = MaterialIcons.TrendingUp.ToImageSource(ThemeColor.Get("BlingTextLight"), 20);
        TypeIncome.BorderColor = ThemeColor.Get("BlingPrimary");
        TypeIncome.BorderWidth = 2;
        UpdateParents();
    }

    private void ResetCategoryTypeButtons()
    {
        TypeExpense.Text = "Despesa";
        TypeIncome.Text = "Receita";
        foreach (var button in new[] { TypeExpense, TypeIncome })
        {
            button.BackgroundColor = ThemeColor.Get("BlingCard");
            button.TextColor = ThemeColor.Get("BlingText");
            button.BorderColor = ThemeColor.Get("BlingText");
            button.BorderWidth = 1;
        }
        TypeExpense.ImageSource = MaterialIcons.TrendingDown.ToImageSource(ThemeColor.Get("BlingText"), 20);
        TypeIncome.ImageSource = MaterialIcons.TrendingUp.ToImageSource(ThemeColor.Get("BlingText"), 20);
    }

    private void OnColorChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.StartsWith("#"))
            {
                ColorPreview.BackgroundColor = ThemeColor.Parse(e.NewTextValue);
            }
        }
        catch { }
    }

    private void OnColorPaletteTapped(object? sender, EventArgs e)
    {
        if (sender is not Border border)
            return;

        var color = border.BackgroundColor;
        ColorEntry.Text = color.ToHex();
        ColorPreview.BackgroundColor = color;
    }

    private void UpdateParents()
    {
        var type = _selectedType;
        _availableParents = _all
            .Where(x => x.Type == type && x.Level < 3 && x.Id != _editingId)
            .ToList();

        var items = _availableParents.Select(x =>
            $"{new string('·', x.Level)} {x.Name}").ToList();

        items.Insert(0, "-- Selecione a categoria pai --");
        ParentPicker.ItemsSource = items;
        ParentPicker.SelectedIndex = 0;
    }

    private static MaterialIcons CategoryIconPath(CategoryItem item)
    {
        var storedIcon = item.Icon?.Trim();
        if (!string.IsNullOrWhiteSpace(storedIcon))
        {
            var iconName = System.IO.Path.GetFileNameWithoutExtension(storedIcon);
            if (Enum.TryParse<MaterialIcons>(iconName, true, out var materialIcon))
                return materialIcon;

            var pascalName = string.Concat(iconName.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
            if (Enum.TryParse<MaterialIcons>(pascalName, true, out materialIcon))
                return materialIcon;

            if (LegacyCategoryIcons.TryGetValue(iconName, out materialIcon))
                return materialIcon;
        }

        var value = NormalizeSearchText(item.Name);
        if (value.Contains("alimenta") || value.Contains("mercado") || value.Contains("restaurante"))
            return MaterialIcons.Restaurant;
        if (value.Contains("compra") || value.Contains("roupa"))
            return MaterialIcons.ShoppingBag;
        if (value.Contains("educa") || value.Contains("curso") || value.Contains("escola"))
            return MaterialIcons.School;
        if (value.Contains("imposto") || value.Contains("taxa"))
            return MaterialIcons.RequestQuote;
        if (value.Contains("lazer") || value.Contains("viagem") || value.Contains("assinatura"))
            return MaterialIcons.Movie;
        if (value.Contains("moradia") || value.Contains("casa") || value.Contains("aluguel") ||
            value.Contains("condominio"))
            return MaterialIcons.House;
        if (value.Contains("saude") || value.Contains("farmacia") || value.Contains("medic"))
            return MaterialIcons.MedicalServices;
        if (value.Contains("transport") || value.Contains("combustivel") || value.Contains("uber"))
            return MaterialIcons.DirectionsCar;
        if (value.Contains("utilidade") || value.Contains("energia") || value.Contains("internet") ||
            value.Contains("telefone") || value.Contains("agua"))
            return MaterialIcons.Lightbulb;
        if (item.Type == "receita" || value.Contains("salario") || value.Contains("renda"))
            return MaterialIcons.TrendingUp;
        return MaterialIcons.Category;
    }

    private static string NormalizeSearchText(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(character));
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowError("O nome da categoria é obrigatório.");
            return;
        }

        var isChild = LevelPicker.SelectedIndex == 1;
        if (isChild && ParentPicker.SelectedIndex <= 0)
        {
            ShowError("Selecione uma categoria pai para a subcategoria.");
            return;
        }

        try
        {
            var isNew = _editingId is null;
            var type = _selectedType;
            long? parent = null;

            if (isChild && ParentPicker.SelectedIndex > 0)
            {
                var selectedParent = _availableParents[ParentPicker.SelectedIndex - 1];
                parent = selectedParent.Id;
            }

            if (_editingId is null)
            {
                await _database.AddCategoryAsync(
                    NameEntry.Text.Trim(),
                    type,
                    ColorEntry.Text?.Trim() ?? BlingPalette.PrimaryHex,
                    IconEntry.Text?.Trim() ?? string.Empty,
                    parent
                );
            }
            else
            {
                await _database.UpdateCategoryAsync(
                    _editingId.Value,
                    NameEntry.Text.Trim(),
                    type,
                    ColorEntry.Text?.Trim() ?? BlingPalette.PrimaryHex,
                    IconEntry.Text?.Trim() ?? string.Empty,
                    parent
                );
            }

            await ThemedDialog.ShowAsync(this, "Categoria salva",
                isNew ? "Categoria cadastrada com sucesso!" : "Categoria alterada com sucesso!");

            NameEntry.Text = "";
            IconEntry.Text = "";
            FormCard.IsVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowError(SqliteErrorMessage.ToFriendly(ex));
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private async Task DeleteCategory(CategoryItem item)
    {
        var confirm = await ThemedDialog.ConfirmDeleteAsync(this,
            "Excluir categoria",
            $"Tem certeza que deseja excluir '{item.Name}'?{Environment.NewLine}As subcategorias também serão removidas."
        );

        if (!confirm)
            return;

        try
        {
            await _database.DeleteCategoryAsync(item.Id);
            await LoadAsync();
            await ThemedDialog.ShowAsync(this, "Categoria excluída", "Categoria excluída com sucesso!");
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Erro", $"Não foi possível excluir: {SqliteErrorMessage.ToFriendly(ex)}");
        }
    }

    private void UpdateEmptyState()
    {
        var hasItems = CategoriesContainer.Children.Count > 0;
        EmptyState.IsVisible = !hasItems;
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
