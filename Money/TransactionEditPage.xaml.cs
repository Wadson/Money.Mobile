using System.Globalization;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money;

public partial class TransactionEditPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly long _id;
    private readonly string _type;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<CategoryItem> _categories = [];
    private List<TagItem> _tags = [];
    private List<Fornecedor> _suppliers = [];
    private List<AccountItem> _accounts = [];
    private List<CardItem> _cards = [];
    private readonly Dictionary<long, CheckBox> _tagChecks = [];
    private readonly Dictionary<long, DateTime> _installmentDueDates = [];
    private readonly Dictionary<long, Button> _installmentDateButtons = [];
    private List<InstallmentScheduleItem> _installmentSchedule = [];
    private bool _updatingFirstInstallmentDate;
    private bool _loaded;
    private bool _existingInstallment;
    private bool _originalPaid;

    public event EventHandler? Saved;

    public TransactionEditPage(DatabaseService database, long id, string type)
    {
        InitializeComponent();
        _database = database;
        _id = id;
        _type = type;
        TitleLabel.Text = type == "receita" ? "Editar receita" : "Editar conta a pagar";
        FrequencyPicker.ItemsSource = new[] { "mensal", "anual" };
        SourceTypePicker.ItemsSource = new[] { "Conta bancária", "Cartão de crédito" };
        FirstInstallmentDatePicker.Date = DateTime.Today;
        FirstInstallmentDatePicker.MinimumDate = new DateTime(2000, 1, 1);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded) return;
        try
        {
            var data = await _database.GetTransactionForEditAsync(_id, _type);
            _categories = await _database.GetCategoriesAsync(_type);
            _tags = await _database.GetTagsAsync();
            _suppliers = await _database.GetSuppliersAsync();
            _accounts = await _database.GetAccountsAsync();
            _cards = await _database.GetCardsAsync();
            var selectedTags = await _database.GetTransactionTagIdsAsync(_id);
            CategoryPicker.ItemsSource = _categories.Select(x => x.Name).ToList();
            DescriptionEntry.Text = data.Description;
            AmountEntry.Text = data.Amount.ToString("N2", _culture);
            TransactionDatePicker.Date = data.Date;
            TransactionDateButton.Text = data.Date.ToString("dd/MM/yyyy");
            CategoryPicker.SelectedIndex = _categories.FindIndex(x => x.Id == data.CategoryId);
            CategorySelectionButton.Text = CategoryPicker.SelectedIndex >= 0
                ? _categories[CategoryPicker.SelectedIndex].Name
                : "Selecione a categoria";
            NotesEditor.Text = data.Notes;
            SupplierPanel.IsVisible = _type == "despesa";
            SupplierPicker.ItemsSource = new[] { "Nenhum fornecedor" }.Concat(_suppliers.Select(x => x.NomeFornecedor)).ToList();
            SupplierPicker.SelectedIndex = data.SupplierId is null ? 0 : _suppliers.FindIndex(x => x.IdFornecedor == data.SupplierId) + 1;
            SupplierSelectionButton.Text = SupplierPicker.SelectedIndex > 0
                ? _suppliers[SupplierPicker.SelectedIndex - 1].NomeFornecedor : "Nenhum fornecedor";
            AccountPicker.ItemsSource = _accounts.Select(x => x.Name).ToList();
            CardPicker.ItemsSource = _cards.Select(x => x.Name).ToList();
            SourceTypePicker.SelectedIndex = data.CardId is not null ? 1 : 0;
            AccountPicker.SelectedIndex = data.AccountId is null ? -1 : _accounts.FindIndex(x => x.Id == data.AccountId);
            CardPicker.SelectedIndex = data.CardId is null ? -1 : _cards.FindIndex(x => x.Id == data.CardId);
            RefreshSourceControls();
            DueDatePanel.IsVisible = _type == "despesa";
            DueDatePicker.Date = data.DueDate ?? data.Date;
            DueDateButton.Text = (data.DueDate ?? data.Date).ToString("dd/MM/yyyy");
            PaymentStatusCard.IsVisible = _type == "despesa";
            PaidSwitch.IsToggled = data.Paid;
            _originalPaid = data.Paid;
            PaymentDatePicker.Date = data.PaymentDate ?? DateTime.Today;
            PaymentDateBorder.IsVisible = data.Paid;
            RecurringSwitch.IsToggled = data.Recurring;
            FrequencyBorder.IsVisible = data.Recurring;
            FrequencyPicker.SelectedItem = data.Frequency ?? "mensal";
            FrequencySelectionButton.Text = FrequencyPicker.SelectedItem?.ToString() ?? "Mensal";
            InstallmentCard.IsVisible = _type == "despesa";
            _existingInstallment = data.Installment;
            InstallmentSwitch.IsToggled = data.Installment;
            InstallmentPanel.IsVisible = data.Installment;
            InstallmentStepper.Value = Math.Max(2, data.TotalInstallments);
            InstallmentCountLabel.Text = $"{Math.Max(2, data.TotalInstallments)} parcelas";
            FirstInstallmentDatePicker.Date = data.DueDate ?? data.Date;
            if (_existingInstallment)
            {
                PaymentStatusCard.IsVisible = false;
                InstallmentSwitch.IsEnabled = false;
                InstallmentStepper.IsEnabled = false;
                FirstInstallmentDatePicker.IsVisible = true;
                FirstInstallmentDatePicker.IsEnabled = true;
                InstallmentHintLabel.Text = $"Parcela {data.InstallmentNumber}/{data.TotalInstallments} · altere a primeira data para recalcular toda a série";
                await LoadInstallmentScheduleAsync();
            }
            RenderTags(selectedTags);
            _loaded = true;
        }
        catch (Exception ex)
        {
            ShowError(SqliteErrorMessage.ToFriendly(ex));
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (CategoryPicker.SelectedIndex < 0)
        {
            ShowError("Selecione uma categoria.");
            return;
        }
        if (!decimal.TryParse(AmountEntry.Text, NumberStyles.Currency, _culture, out var amount))
        {
            ShowError("Informe um valor válido.");
            return;
        }
        var useCard = SourceTypePicker.SelectedIndex == 1;
        if ((!useCard && AccountPicker.SelectedIndex < 0) || (useCard && CardPicker.SelectedIndex < 0))
        {
            ShowError(useCard ? "Selecione um cartão." : "Selecione uma conta.");
            return;
        }
        try
        {
            if (_type == "despesa" && InstallmentSwitch.IsToggled && !_existingInstallment)
            {
                await _database.ConvertTransactionToInstallmentsAsync(_id,
                    DescriptionEntry.Text ?? "", amount, _categories[CategoryPicker.SelectedIndex].Id,
                    NotesEditor.Text?.Trim(), (int)InstallmentStepper.Value,
                    FirstInstallmentDatePicker.Date ?? DateTime.Today,
                    RecurringSwitch.IsToggled, FrequencyPicker.SelectedItem?.ToString(),
                    _tagChecks.Where(x => x.Value.IsChecked).Select(x => x.Key).ToArray(),
                    SupplierPicker.SelectedIndex > 0
                        ? _suppliers[SupplierPicker.SelectedIndex - 1].IdFornecedor : null);
                await ThemedDialog.ShowAsync(this, "Conta parcelada",
                    $"Conta convertida em {(int)InstallmentStepper.Value} parcelas com sucesso!");
                Saved?.Invoke(this, EventArgs.Empty);
                await Navigation.PopModalAsync();
                return;
            }
            await _database.UpdateListedTransactionAsync(_id, _type,
                DescriptionEntry.Text ?? "", amount, TransactionDatePicker.Date ?? DateTime.Today,
                _categories[CategoryPicker.SelectedIndex].Id, NotesEditor.Text?.Trim(),
                _type == "despesa" ? DueDatePicker.Date : null,
                RecurringSwitch.IsToggled, FrequencyPicker.SelectedItem?.ToString(),
                _tagChecks.Where(x => x.Value.IsChecked).Select(x => x.Key).ToArray(),
                _type == "despesa" && SupplierPicker.SelectedIndex > 0
                    ? _suppliers[SupplierPicker.SelectedIndex - 1].IdFornecedor : null,
                useCard ? null : _accounts[AccountPicker.SelectedIndex].Id,
                useCard ? _cards[CardPicker.SelectedIndex].Id : null,
                _type == "despesa" && (_existingInstallment ? _originalPaid : PaidSwitch.IsToggled),
                _type == "despesa" && (_existingInstallment ? _originalPaid : PaidSwitch.IsToggled) ? PaymentDatePicker.Date : null);
            if (_existingInstallment)
                await _database.UpdateInstallmentDueDatesAsync(_installmentDueDates);
            await ThemedDialog.ShowAsync(this,
                _type == "receita" ? "Salário alterado" : "Conta alterada",
                _type == "receita" ? "Salário alterado com sucesso!" : "Conta alterada com sucesso!");
            Saved?.Invoke(this, EventArgs.Empty);
            await Navigation.PopModalAsync();
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

    private void RenderTags(IReadOnlyCollection<long> selected)
    {
        TagsContainer.Children.Clear();
        _tagChecks.Clear();
        foreach (var tag in _tags)
        {
            var check = new CheckBox { IsChecked = selected.Contains(tag.Id), Color = SafeColor(tag.Color) };
            _tagChecks[tag.Id] = check;
            TagsContainer.Children.Add(new HorizontalStackLayout
            {
                Spacing = 2, Children = { check, new Label { Text = tag.Name, FontSize = 11, TextColor = ThemeColor.Get("BlingText"), VerticalTextAlignment = TextAlignment.Center } }
            });
        }
    }
    private async Task LoadInstallmentScheduleAsync()
    {
        var installments = await _database.GetTransactionInstallmentsAsync(_id);
        _installmentSchedule = installments.OrderBy(x => x.Number).ThenBy(x => x.Id).ToList();
        _installmentDueDates.Clear();
        _installmentDateButtons.Clear();
        InstallmentsContainer.Children.Clear();
        foreach (var item in _installmentSchedule)
        {
            _installmentDueDates[item.Id] = item.DueDate.Date;
            var dateButton = new Button
            {
                Text = item.DueDate.ToString("dd/MM/yyyy"), CommandParameter = item.Id,
                BackgroundColor = ThemeColor.Get("BlingPrimary"), TextColor = ThemeColor.Get("BlingTextLight"),
                FontSize = 12, FontAttributes = FontAttributes.Bold, HeightRequest = 40,
                Padding = new Thickness(10, 0), CornerRadius = 10
            };
            dateButton.Clicked += OnInstallmentDateClicked;
            _installmentDateButtons[item.Id] = dateButton;
            var details = new VerticalStackLayout
            {
                Spacing = 1, VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = $"Parcela {item.Number}/{item.Total}", FontSize = 12,
                        FontAttributes = FontAttributes.Bold, TextColor = ThemeColor.Get("BlingText") },
                    new Label { Text = item.Amount.ToString("C2", _culture), FontSize = 10,
                        TextColor = ThemeColor.Get("BlingTextMuted") }
                }
            };
            var row = new Grid
            {
                ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto)],
                ColumnSpacing = 8, Padding = new Thickness(10, 6), BackgroundColor = ThemeColor.Get("BlingCard")
            };
            row.Add(details);
            row.Add(new Label { Text = item.Paid ? "Paga" : "Pendente", TextColor = ThemeColor.Get("BlingPrimary"),
                FontSize = 10, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center }, 1);
            row.Add(dateButton, 2);
            InstallmentsContainer.Children.Add(new Border
            {
                Stroke = ThemeColor.Get("BlingPrimary"), StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 11 }, Content = row
            });
        }
        InstallmentSchedulePanel.IsVisible = _installmentSchedule.Count > 0;
        if (_installmentSchedule.Count > 0)
        {
            var first = _installmentSchedule.OrderBy(x => x.Number).First();
            _updatingFirstInstallmentDate = true;
            FirstInstallmentDatePicker.Date = first.DueDate.Date;
            _updatingFirstInstallmentDate = false;
        }
    }

    private void OnFirstInstallmentDateSelected(object? sender, DateChangedEventArgs e)
    {
        if (_updatingFirstInstallmentDate || !_existingInstallment || _installmentSchedule.Count == 0) return;
        var firstNumber = _installmentSchedule.Min(x => x.Number);
        var selectedDate = (e.NewDate ?? e.OldDate ?? DateTime.Today).Date;
        foreach (var installment in _installmentSchedule)
        {
            var dueDate = selectedDate.AddMonths(installment.Number - firstNumber);
            _installmentDueDates[installment.Id] = dueDate;
            if (_installmentDateButtons.TryGetValue(installment.Id, out var button))
                button.Text = dueDate.ToString("dd/MM/yyyy");
        }
    }

    private async void OnInstallmentDateClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !long.TryParse(button.CommandParameter?.ToString(), out var id) ||
            !_installmentDueDates.TryGetValue(id, out var currentDate)) return;
        await ShowDateSelectorAsync("Vencimento da parcela", currentDate, date =>
        {
            _installmentDueDates[id] = date.Date;
            button.Text = date.ToString("dd/MM/yyyy");
        });
    }

    private void OnRecurringToggled(object? sender, ToggledEventArgs e) => FrequencyBorder.IsVisible = e.Value;
    private void OnPaidToggled(object? sender, ToggledEventArgs e) => PaymentDateBorder.IsVisible = e.Value;
    private void OnInstallmentToggled(object? sender, ToggledEventArgs e) => InstallmentPanel.IsVisible = e.Value;
    private void OnInstallmentCountChanged(object? sender, ValueChangedEventArgs e) =>
        InstallmentCountLabel.Text = $"{(int)e.NewValue} parcelas";

    private void OnDecreaseInstallmentsClicked(object? sender, EventArgs e)
    {
        if (InstallmentStepper.IsEnabled)
            InstallmentStepper.Value = Math.Max(InstallmentStepper.Minimum, InstallmentStepper.Value - 1);
    }

    private void OnIncreaseInstallmentsClicked(object? sender, EventArgs e)
    {
        if (InstallmentStepper.IsEnabled)
            InstallmentStepper.Value = Math.Min(InstallmentStepper.Maximum, InstallmentStepper.Value + 1);
    }

    private async void OnSelectCategoryClicked(object? sender, EventArgs e)
    {
        var options = _categories.Select((category, index) =>
            CategoryVisualResolver.Option(category, index, CategoryPicker.SelectedIndex == index)).ToList();
        var page = new OptionSelectionPage("Selecione a categoria", options);
        page.Selected += (_, option) =>
        {
            CategoryPicker.SelectedIndex = option.Index;
            CategorySelectionButton.Text = options[option.Index].Label;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectSupplierClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new() { Index = 0, Label = "Nenhum fornecedor", ImageSource = MaterialIcons.Store,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = SupplierPicker.SelectedIndex <= 0 }
        };
        options.AddRange(_suppliers.Select((supplier, index) => new SelectionOption
        {
            Index = index + 1, Label = supplier.NomeFornecedor, ImageSource = MaterialIcons.Store,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = SupplierPicker.SelectedIndex == index + 1
        }));
        var page = new OptionSelectionPage("Selecione o fornecedor", options);
        page.Selected += (_, option) =>
        {
            SupplierPicker.SelectedIndex = option.Index;
            SupplierSelectionButton.Text = option.Label;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectSourceTypeClicked(object? sender, EventArgs e)
    {
        var labels = new[] { "Conta bancária", "Cartão de crédito" };
        var icons = new[] { MaterialIcons.AccountBalanceWallet, MaterialIcons.CreditCard };
        var options = labels.Select((label, index) => new SelectionOption
        {
            Index = index, Label = label, ImageSource = icons[index],
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = SourceTypePicker.SelectedIndex == index
        }).ToList();
        var page = new OptionSelectionPage("Origem da despesa", options);
        page.Selected += (_, option) => SourceTypePicker.SelectedIndex = option.Index;
        await Navigation.PushModalAsync(page);
    }

    private void OnSourceTypeChanged(object? sender, EventArgs e) => RefreshSourceControls();

    private void RefreshSourceControls()
    {
        var useCard = SourceTypePicker.SelectedIndex == 1;
        AccountPanel.IsVisible = !useCard;
        CardPanel.IsVisible = useCard;
        SourceTypeSelectionButton.Text = useCard ? "Cartão de crédito" : "Conta bancária";
        if (AccountPicker.SelectedIndex >= 0 && AccountPicker.SelectedIndex < _accounts.Count)
            AccountSelectionButton.Text = _accounts[AccountPicker.SelectedIndex].Name;
        if (CardPicker.SelectedIndex >= 0 && CardPicker.SelectedIndex < _cards.Count)
            CardSelectionButton.Text = _cards[CardPicker.SelectedIndex].Name;
    }

    private async void OnSelectAccountClicked(object? sender, EventArgs e)
    {
        var options = _accounts.Select((account, index) => new SelectionOption
        {
            Index = index, Label = account.Name, ImageSource = MaterialIcons.AccountBalanceWallet,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = AccountPicker.SelectedIndex == index
        }).ToList();
        var page = new OptionSelectionPage("Selecione a conta", options);
        page.Selected += (_, option) => { AccountPicker.SelectedIndex = option.Index; AccountSelectionButton.Text = option.Label; };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectCardClicked(object? sender, EventArgs e)
    {
        var options = _cards.Select((card, index) => new SelectionOption
        {
            Index = index, Label = card.Name, ImageSource = MaterialIcons.CreditCard,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = CardPicker.SelectedIndex == index
        }).ToList();
        var page = new OptionSelectionPage("Selecione o cartão", options);
        page.Selected += (_, option) => { CardPicker.SelectedIndex = option.Index; CardSelectionButton.Text = option.Label; };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectFrequencyClicked(object? sender, EventArgs e)
    {
        var values = new[] { "Mensal", "Anual" };
        var options = values.Select((value, index) => new SelectionOption
        {
            Index = index, Label = value, ImageSource = MaterialIcons.CalendarMonth,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = FrequencyPicker.SelectedIndex == index
        }).ToList();
        var page = new OptionSelectionPage("Selecione a frequência", options);
        page.Selected += (_, option) =>
        {
            FrequencyPicker.SelectedIndex = option.Index;
            FrequencySelectionButton.Text = values[option.Index];
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnTransactionDateClicked(object? sender, EventArgs e) =>
        await ShowDateSelectorAsync("Data do lançamento", TransactionDatePicker.Date ?? DateTime.Today,
            date => { TransactionDatePicker.Date = date; TransactionDateButton.Text = date.ToString("dd/MM/yyyy"); });

    private async void OnDueDateClicked(object? sender, EventArgs e) =>
        await ShowDateSelectorAsync("Data de vencimento", DueDatePicker.Date ?? DateTime.Today,
            date => { DueDatePicker.Date = date; DueDateButton.Text = date.ToString("dd/MM/yyyy"); });

    private async Task ShowDateSelectorAsync(string title, DateTime date, Action<DateTime> selected)
    {
        var page = new CalendarDateSelectionPage(title, date);
        page.DateSelected += (_, value) => selected(value);
        await Navigation.PushModalAsync(page);
    }
    private static Color SafeColor(string value) => ThemeColor.Parse(value);

    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
