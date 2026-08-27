using System.Globalization;
using MauiIcons.Core;
using MauiIcons.Material;
using Money.Controls;
using Money.Models;
using Money.Services;

namespace Money.Views.Management;

public partial class AccountManagementPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<AccountItem> _items = [];
    private long? _editingId;
    private static readonly (string Label, string Value, MaterialIcons Icon)[] Icons =
    [
        ("Banco", "bank", MaterialIcons.AccountBalance), ("Carteira", "wallet", MaterialIcons.AccountBalanceWallet),
        ("Investimento", "investment", MaterialIcons.Savings)
    ];
    private static readonly (string Label, string Value, MaterialIcons Icon, string DefaultIcon)[] AccountTypes =
    [
        ("Conta corrente", "corrente", MaterialIcons.AccountBalance, "bank"),
        ("Poupança", "poupanca", MaterialIcons.Savings, "investment"),
        ("Carteira", "carteira", MaterialIcons.AccountBalanceWallet, "wallet"),
        ("Investimento", "investimento", MaterialIcons.TrendingUp, "investment")
    ];

    public AccountManagementPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        ColorEntry.Text = BlingPalette.PrimaryHex;
        TypePicker.ItemsSource = AccountTypes.Select(x => x.Label).ToList();
        IconPicker.ItemsSource = Icons.Select(x => x.Label).ToList();
    }

    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    private async Task LoadAsync()
    {
        try
        {
            var accountsTask = _database.GetAccountsAsync();
            var totalBalanceTask = _database.GetTotalActiveAccountsBalanceAsync();
            await Task.WhenAll(accountsTask, totalBalanceTask);
            _items = await accountsTask;
            TotalBalanceLabel.Text = (await totalBalanceTask).ToString("C2", _culture);
            AccountsContainer.Children.Clear();
            foreach (var item in _items) AccountsContainer.Children.Add(CreateCard(item));
            if (_items.Count == 0) AccountsContainer.Children.Add(new Label { Text = "Nenhuma conta cadastrada.", TextColor = ThemeColor.Get("BlingText"), HorizontalTextAlignment = TextAlignment.Center, Padding = 20 });
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private View CreateCard(AccountItem item)
    {
        var color = SafeColor(item.Color);
        var grid = new Grid
        {
            ColumnDefinitions = [new(44), new(GridLength.Star), new(GridLength.Auto)],
            RowDefinitions = [new(GridLength.Auto), new(GridLength.Auto)],
            ColumnSpacing = 8, RowSpacing = 6
        };
        var icon = new Border
        {
            HeightRequest = 40, WidthRequest = 40, BackgroundColor = color.WithAlpha(.14f), StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Content = new MauiIcon
            {
                Icon = PathFor(item.Icon), IconSize = 24, IconColor = ThemeColor.Get("BlingText"), HeightRequest = 30, WidthRequest = 30,
                HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
            }
        };
        grid.Add(icon, 0, 0);
        var identity = new VerticalStackLayout
        {
            Spacing = 2, VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = item.Name, TextColor = ThemeColor.Get("BlingText"), FontAttributes = FontAttributes.Bold },
                new Label { Text = item.Type, TextColor = ThemeColor.Get("BlingPrimary"), FontSize = 12, FontAttributes = FontAttributes.Bold }
            }
        };
        grid.Add(identity, 1, 0);
        Grid.SetColumnSpan(identity, 2);
        var balance = new Label
        {
            Text = item.Balance.ToString("C2", _culture), TextColor = ThemeColor.Get("BlingText"),
            FontSize = 20, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center
        };
        grid.Add(balance, 0, 1);
        Grid.SetColumnSpan(balance, 2);
        var actions = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.End };
        var edit = Action("Editar", "BlingPrimary", "BlingTextLight", item.Id); edit.Clicked += OnEditClicked;
        var delete = Action("Excluir", "BlingPrimary", "BlingTextLight", item.Id); delete.Clicked += OnDeleteClicked;
        actions.Children.Add(edit); actions.Children.Add(delete);
        grid.Add(actions, 2, 1);
        return new Border { Style = (Style)Application.Current!.Resources["ContentCard"], Padding = 10, Content = grid };
    }

    private void OnNewClicked(object? sender, EventArgs e) { Clear(); FormCard.IsVisible = true; NameEntry.Focus(); }
    private async void OnTransferClicked(object? sender, EventArgs e)
    {
        var page = new AccountTransferPage(_database);
        page.Saved += async (_, _) => await LoadAsync();
        await Navigation.PushModalAsync(page);
    }
    private void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id }) return;
        var item = _items.Single(x => x.Id == id); _editingId = id; FormTitle.Text = "Editar conta";
        NameEntry.Text = item.Name; BalanceEntry.Text = item.Balance.ToString("N2", _culture);
        TypePicker.SelectedIndex = Array.IndexOf(new[] { "corrente", "poupanca", "carteira", "investimento" }, item.Type);
        TypeSelectionButton.Text = TypePicker.SelectedIndex >= 0
            ? AccountTypes[TypePicker.SelectedIndex].Label : "Selecione o tipo";
        if (TypePicker.SelectedIndex >= 0)
            TypeIconImage.Source = AccountTypes[TypePicker.SelectedIndex].Icon
                .ToImageSource(ThemeColor.Get("BlingPrimary"), 20);
        ColorEntry.Text = item.Color; IconPicker.SelectedIndex = Array.FindIndex(Icons, x => x.Value == item.Icon);
        if (IconPicker.SelectedIndex < 0) IconPicker.SelectedIndex = 0;
        IconSelectionButton.Text = Icons[IconPicker.SelectedIndex].Label;
        SelectedIconImage.Source = Icons[IconPicker.SelectedIndex].Icon
            .ToImageSource(ThemeColor.Get("BlingPrimary"), 20);
        HighlightSelectedColor(item.Color);
        FormCard.IsVisible = true;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text)) { await ThemedDialog.ShowAsync(this, "Campo obrigatório", "Informe o nome da conta.", "Corrigir"); NameEntry.Focus(); return; }
        if (!decimal.TryParse(BalanceEntry.Text, NumberStyles.Currency, _culture, out var balance)) { await ThemedDialog.ShowAsync(this, "Saldo inválido", "Informe um saldo válido.", "Corrigir"); BalanceEntry.Focus(); return; }
        if (TypePicker.SelectedIndex < 0) { await ThemedDialog.ShowAsync(this, "Campo obrigatório", "Selecione o tipo da conta.", "Corrigir"); return; }
        if (IconPicker.SelectedIndex < 0) { await ThemedDialog.ShowAsync(this, "Campo obrigatório", "Selecione o ícone da conta.", "Corrigir"); return; }
        try
        {
            await _database.SaveAccountAsync(_editingId, NameEntry.Text ?? "", balance,
                AccountTypes[TypePicker.SelectedIndex].Value, ColorEntry.Text ?? BlingPalette.PrimaryHex, Icons[IconPicker.SelectedIndex].Value);
            await ThemedDialog.ShowAsync(this, "Conta salva",
                _editingId is null ? "Conta cadastrada com sucesso!" : "Conta alterada com sucesso!");
            Clear(); FormCard.IsVisible = false; await LoadAsync();
        }
        catch (Exception ex) { var message = SqliteErrorMessage.ToFriendly(ex); ShowError(message); await ThemedDialog.ShowAsync(this, "Não foi possível salvar a conta", message, "Fechar"); }
    }

    private async void OnSelectTypeClicked(object? sender, EventArgs e)
    {
        var options = AccountTypes.Select((type, index) => new SelectionOption
        {
            Index = index, Label = type.Label, ImageSource = type.Icon,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = TypePicker.SelectedIndex == index
        }).ToList();
        var page = new OptionSelectionPage("Selecione o tipo de conta", options);
        page.Selected += (_, option) =>
        {
            TypePicker.SelectedIndex = option.Index;
            TypeSelectionButton.Text = AccountTypes[option.Index].Label;
            TypeIconImage.Source = AccountTypes[option.Index].Icon
                .ToImageSource(ThemeColor.Get("BlingPrimary"), 20);
            var iconIndex = Array.FindIndex(Icons, x => x.Value == AccountTypes[option.Index].DefaultIcon);
            if (iconIndex >= 0)
            {
                IconPicker.SelectedIndex = iconIndex;
                IconSelectionButton.Text = Icons[iconIndex].Label;
                SelectedIconImage.Source = Icons[iconIndex].Icon
                    .ToImageSource(ThemeColor.Get("BlingPrimary"), 20);
            }
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectIconClicked(object? sender, EventArgs e)
    {
        var options = Icons.Select((icon, index) => new SelectionOption
        {
            Index = index, Label = icon.Label, ImageSource = icon.Icon,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = IconPicker.SelectedIndex == index
        }).ToList();
        var page = new OptionSelectionPage("Selecione o ícone da conta", options);
        page.Selected += (_, option) =>
        {
            IconPicker.SelectedIndex = option.Index;
            IconSelectionButton.Text = Icons[option.Index].Label;
            SelectedIconImage.Source = Icons[option.Index].Icon
                .ToImageSource(ThemeColor.Get("BlingPrimary"), 20);
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id } ||
            !await ThemedDialog.ConfirmDeleteAsync(this, "Excluir conta", "Contas com movimentações serão inativadas para preservar o histórico. Continuar?")) return;
        try { await _database.DeleteAccountAsync(id); await LoadAsync(); await ThemedDialog.ShowAsync(this, "Conta excluída", "Operação concluída com sucesso."); }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Atenção", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private void OnCancelClicked(object? sender, EventArgs e) { Clear(); FormCard.IsVisible = false; }
    private void OnColorClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;
        ColorEntry.Text = button.BackgroundColor.ToHex();
        HighlightSelectedColor(ColorEntry.Text);
    }
    private void OnInputFocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry { Parent: Border border })
        {
            border.Stroke = ThemeColor.Get("BlingPrimary");
            border.StrokeThickness = 2;
        }
    }
    private void OnInputUnfocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry { Parent: Border border })
        {
            border.Stroke = ThemeColor.Get("BlingText");
            border.StrokeThickness = 1;
        }
    }
    private void HighlightSelectedColor(string? selectedColor)
    {
        foreach (var option in ColorOptions.Children.OfType<Border>())
        {
            var isSelected = option.Content is Button button &&
                string.Equals(button.BackgroundColor.ToHex(), selectedColor, StringComparison.OrdinalIgnoreCase);
            option.Stroke = isSelected ? ThemeColor.Get("BlingPrimary") : Colors.Transparent;
        }
    }
    private void Clear() { _editingId = null; FormTitle.Text = "Nova conta"; NameEntry.Text = BalanceEntry.Text = ""; ColorEntry.Text = BlingPalette.PrimaryHex; TypePicker.SelectedIndex = IconPicker.SelectedIndex = -1; TypeSelectionButton.Text = "Selecione o tipo"; IconSelectionButton.Text = "Selecione o ícone"; TypeIconImage.Source = MaterialIcons.Category.ToImageSource(ThemeColor.Get("BlingPrimary"), 20); SelectedIconImage.Source = MaterialIcons.AccountBalanceWallet.ToImageSource(ThemeColor.Get("BlingPrimary"), 20); ErrorLabel.IsVisible = false; }
    private void ShowError(Exception ex) => ShowError(SqliteErrorMessage.ToFriendly(ex));
    private void ShowError(string value) { ErrorLabel.Text = value; ErrorLabel.IsVisible = true; }
    private static Button Action(string text, string bg, string fg, long id) => new()
    {
        Text = text, FontSize = 10, HeightRequest = 32, MinimumHeightRequest = 32, WidthRequest = 68, Padding = 3,
        BackgroundColor = ThemeColor.Get(bg), TextColor = ThemeColor.Get(fg),
        ImageSource = (text == "Editar" ? MaterialIcons.Edit : MaterialIcons.Delete)
            .ToImageSource(ThemeColor.Get(fg), 20),
        CommandParameter = id
    };
    private static Color SafeColor(string value) => ThemeColor.Parse(value);
    private static MaterialIcons PathFor(string icon)
    {
        var match = Icons.FirstOrDefault(x => x.Value == icon);
        return match == default ? MaterialIcons.AccountBalanceWallet : match.Icon;
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
