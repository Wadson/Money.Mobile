using System.Globalization;
using MauiIcons.Material;
using Money.Models;
using Money.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Money.Views.Transactions;

public partial class TransactionFormPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<CategoryItem> _categories = new();
    private List<AccountItem> _accounts = new();
    private List<CardItem> _cards = new();
    private List<TagItem> _tags = new();
    private List<Fornecedor> _suppliers = new();
    private readonly Dictionary<long, CheckBox> _tagChecks = [];
    private string _selectedType = "despesa";
    private bool _dataLoaded;
    private bool _loadingData;
    private long? _editingId;
    private TransactionEditData? _editingData;
    private bool _existingInstallment;
    private readonly List<InstallmentPreview> _editableSchedule = [];
    public event EventHandler? Saved;

    public TransactionFormPage(DatabaseService database, string initialType = "despesa")
    {
        InitializeComponent();
        _database = database;
        DatePicker.Date = DateTime.Today;
        FirstInstallmentDatePicker.Date = DateTime.Today;
        // Permite cadastrar parcelamentos retroativos e navegar livremente entre os meses.
        FirstInstallmentDatePicker.MinimumDate = new DateTime(2000, 1, 1);
        FirstInstallmentDatePicker.MaximumDate = new DateTime(2100, 12, 31);
        DueDatePicker.Date = DateTime.Today;
        PaymentDatePicker.Date = DateTime.Today;
        PaidSwitch.IsToggled = false;
        FrequencyPicker.ItemsSource = new[] { "mensal", "anual" };
        FrequencyPicker.SelectedIndex = 0;

        // Este formulário é deliberadamente exclusivo para despesas.
        OnTypeExpenseClicked(null, null);
    }

    public TransactionFormPage(DatabaseService database, long transactionId, string type)
        : this(database, type)
    {
        _editingId = transactionId;
        PageTitleLabel.Text = type == "despesa" ? "Editar conta a pagar" : "Editar lançamento";
        PageSubtitleLabel.Text = "Atualize os dados e salve";
        TypeExpense.IsEnabled = false;
        TypeIncome.IsEnabled = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_dataLoaded || _loadingData)
            return;

        _loadingData = true;
        try
        {
            await LoadDataAsync();
            _dataLoaded = true;
        }
        catch (Exception ex)
        {
            ShowError($"Não foi possível carregar o formulário: {SqliteErrorMessage.ToFriendly(ex)}");
        }
        finally
        {
            _loadingData = false;
        }
    }

    // ============================================================
    // CARREGAMENTO DE DADOS
    // ============================================================

    private async Task LoadDataAsync()
    {
        _accounts = await _database.GetAccountsAsync();
        _cards = await _database.GetCardsAsync();
        _tags = await _database.GetTagsAsync();
        _suppliers = await _database.GetSuppliersAsync();

        AccountPicker.ItemsSource = _accounts.Select(x => $"{x.Name} · {x.Balance.ToString("C2", _culture)}").ToList();
        CardPicker.ItemsSource = _cards.Select(x => $"{x.Name} · {(x.CreditLimit - x.Used).ToString("C2", _culture)} disponíveis").ToList();
        SupplierPicker.ItemsSource = new[] { "Nenhum fornecedor" }.Concat(_suppliers.Select(x => x.NomeFornecedor)).ToList();
        SupplierPicker.SelectedIndex = 0;

        await LoadCategoriesAsync();
        RenderTags();
        ConfigureSource();
        if (_editingId is long editingId)
            await LoadEditingDataAsync(editingId);
    }

    private async Task LoadEditingDataAsync(long id)
    {
        var data = await _database.GetTransactionForEditAsync(id, _selectedType);
        _editingData = data;
        DescriptionEntry.Text = data.Description;
        AmountEntry.Text = data.Amount.ToString("N2", _culture);
        DatePicker.Date = data.Date;
        CategoryPicker.SelectedIndex = _categories.FindIndex(item => item.Id == data.CategoryId);
        CategorySelectionButton.Text = CategoryPicker.SelectedIndex >= 0
            ? _categories[CategoryPicker.SelectedIndex].Name : "Selecione uma categoria";
        SupplierPicker.SelectedIndex = data.SupplierId is null
            ? 0 : _suppliers.FindIndex(item => item.IdFornecedor == data.SupplierId) + 1;
        SupplierSelectionButton.Text = SupplierPicker.SelectedIndex > 0
            ? _suppliers[SupplierPicker.SelectedIndex - 1].NomeFornecedor : "Nenhum fornecedor";
        SourceTypePicker.SelectedIndex = data.CardId is null ? 0 : 1;
        AccountPicker.SelectedIndex = data.AccountId is null ? -1 : _accounts.FindIndex(item => item.Id == data.AccountId);
        CardPicker.SelectedIndex = data.CardId is null ? -1 : _cards.FindIndex(item => item.Id == data.CardId);
        ConfigureSource();
        if (AccountPicker.SelectedIndex >= 0) AccountSelectionButton.Text = _accounts[AccountPicker.SelectedIndex].Name;
        if (CardPicker.SelectedIndex >= 0) CardSelectionButton.Text = _cards[CardPicker.SelectedIndex].Name;
        DueDatePicker.Date = data.DueDate ?? data.Date;
        PaidSwitch.IsToggled = data.Paid;
        PaymentDatePicker.Date = data.PaymentDate ?? DateTime.Today;
        NotesEditor.Text = data.Notes;
        DetailsSwitch.IsToggled = !string.IsNullOrWhiteSpace(data.Notes);
        FrequencyPicker.SelectedItem = data.Frequency ?? "mensal";
        FrequencySelectionButton.Text = _culture.TextInfo.ToTitleCase(data.Frequency ?? "mensal");

        _existingInstallment = data.Installment;
        if (data.Installment)
        {
            var items = await _database.GetTransactionInstallmentsAsync(id);
            var persistedSchedule = items.Select(item =>
                new InstallmentPreview(item.Number, item.Total, item.DueDate, item.Amount)).ToArray();
            AmountEntry.Text = persistedSchedule.Sum(item => item.Amount).ToString("N2", _culture);
            InstallmentStepper.Value = Math.Max(2, data.TotalInstallments);
            FirstInstallmentDatePicker.Date = persistedSchedule.Length > 0
                ? persistedSchedule.Min(item => item.Date) : data.DueDate ?? data.Date;
            InstallmentSwitch.IsToggled = !data.Recurring;
            RecurringSwitch.IsToggled = data.Recurring;
            // Os eventos dos controles recalculam a prévia; restaura os valores/datas
            // persistidos para que o modo edição represente fielmente a série atual.
            _editableSchedule.Clear();
            _editableSchedule.AddRange(persistedSchedule);
        }
        else if (data.Recurring)
        {
            RecurringSwitch.IsToggled = true;
            FirstInstallmentDatePicker.Date = data.DueDate ?? data.Date;
        }

        var selectedTags = await _database.GetTransactionTagIdsAsync(id);
        foreach (var pair in _tagChecks)
            pair.Value.IsChecked = selectedTags.Contains(pair.Key);
        RebuildInstallmentPreview(preserveEdits: true);
    }

    private async Task LoadCategoriesAsync()
    {
        _categories = await _database.GetCategoriesAsync(_selectedType);
        CategoryPicker.ItemsSource = _categories.Select(x => x.Name).ToList();
        CategoryPicker.SelectedIndex = -1;
        CategorySelectionButton.Text = "Selecione uma categoria";
    }

    // ============================================================
    // TIPO (Despesa/Receita) - COM ALTO CONTRASTE
    // ============================================================

    private void OnTypeExpenseClicked(object? sender, EventArgs? e)
    {
        _selectedType = "despesa";
        ConfigureTypePanels();
        InstallmentCard.IsVisible = true;

        ResetTypeButtons();
        TypeExpense.Text = "Despesa";
        TypeExpense.BackgroundColor = ThemeColor.Get("BlingPrimary");
        TypeExpense.TextColor = ThemeColor.Get("BlingTextLight");
        TypeExpense.BorderColor = ThemeColor.Get("BlingPrimary");
        TypeExpense.BorderWidth = 2;

        // Evita iniciar consultas durante a construção da página. OnAppearing
        // faz o primeiro carregamento; cliques posteriores atualizam o cache.
        if (_dataLoaded)
            _ = ReloadCategoriesSafelyAsync();

        // Atualizar opções de origem
        SourceTypePicker.ItemsSource = new[]
        {
            "Conta Bancária", "Cartão de Crédito", "Boleto", "Crediário / Promissória"
        };
        SourceTypePicker.SelectedIndex = 0;
        ConfigureSource();
    }

    private void OnTypeIncomeClicked(object? sender, EventArgs? e)
    {
        _selectedType = "receita";
        ConfigureTypePanels();
        InstallmentSwitch.IsToggled = false;
        InstallmentCard.IsVisible = false;

        ResetTypeButtons();
        TypeIncome.Text = "Receita";
        TypeIncome.BackgroundColor = ThemeColor.Get("BlingPrimary");
        TypeIncome.TextColor = ThemeColor.Get("BlingTextLight");
        TypeIncome.BorderColor = ThemeColor.Get("BlingPrimary");
        TypeIncome.BorderWidth = 2;

        if (_dataLoaded)
            _ = ReloadCategoriesSafelyAsync();

        // Atualizar opções de origem
        SourceTypePicker.ItemsSource = new[] { "Conta bancária" };
        SourceTypePicker.SelectedIndex = 0;
        ConfigureSource();
    }

    // ============================================================
    // ORIGEM / DESTINO
    // ============================================================

    private void OnSourceTypeChanged(object? sender, EventArgs e)
    {
        SourceTypeSelectionButton.Text = SourceTypePicker.SelectedItem?.ToString() ?? "Selecione conta ou cartão";
        ConfigureSource();
    }

    private async void OnSelectSourceTypeClicked(object? sender, EventArgs e)
    {
        var values = SourceTypePicker.ItemsSource?.Cast<object>().Select(x => x.ToString() ?? "").ToList() ?? [];
        var options = values.Select((label, index) => new SelectionOption
        {
            Index = index,
            Label = label,
            ImageSource = label.Contains("cartão", StringComparison.OrdinalIgnoreCase)
                ? MaterialIcons.CreditCard
                : label.Contains("boleto", StringComparison.OrdinalIgnoreCase)
                    ? MaterialIcons.ReceiptLong
                    : label.Contains("crediário", StringComparison.OrdinalIgnoreCase)
                        ? MaterialIcons.Description
                        : MaterialIcons.AccountBalance,
            Background = ThemeColor.Get(index == 1 ? "BlingCard" : "BlingCard"),
            Foreground = ThemeColor.Get(index == 1 ? "BlingPrimary" : "BlingPrimary"),
            IsSelected = SourceTypePicker.SelectedIndex == index
        });
        var page = new OptionSelectionPage("Selecione a origem / destino", options);
        page.Selected += (_, option) => SourceTypePicker.SelectedIndex = option.Index;
        await Navigation.PushModalAsync(page);
    }

    private void ResetTypeButtons()
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
    }

    private void ConfigureTypePanels()
    {
        CategoryPanel.IsVisible = true;
        ScheduleCard.IsVisible = true;
        ExpenseStatusPanel.IsVisible = _selectedType == "despesa";
        SupplierPanel.IsVisible = _selectedType == "despesa";
        RecurrenceRow.IsVisible = true;
    }

    private void RenderTags()
    {
        TagsContainer.Children.Clear();
        _tagChecks.Clear();
        foreach (var tag in _tags)
        {
            var check = new CheckBox { Color = ThemeColor.Parse(tag.Color) };
            _tagChecks[tag.Id] = check;
            TagsContainer.Children.Add(new HorizontalStackLayout
            {
                Spacing = 2,
                Margin = new Thickness(2),
                Children =
                {
                    check,
                    new Label
                    {
                        Text = tag.Name, FontSize = 11, TextColor = ThemeColor.Get("BlingText"),
                        VerticalTextAlignment = TextAlignment.Center
                    }
                }
            });
        }
        if (_tags.Count == 0)
            TagsContainer.Children.Add(new Label
            {
                Text = "Cadastre tags em Mais opções > Planejamento.",
                FontSize = 10, TextColor = ThemeColor.Get("BlingText")
            });
    }

    private void OnPaidToggled(object? sender, ToggledEventArgs e)
    {
        PaymentDatePicker.IsVisible = e.Value;
        UpdateSwitchState(PaidStateLabel, e.Value);
    }
    private void OnRecurringToggled(object? sender, ToggledEventArgs e)
    {
        if (e.Value && InstallmentSwitch.IsToggled)
            InstallmentSwitch.IsToggled = false;
        FrequencySelectionBorder.IsVisible = e.Value;
        RecurrencePlanPanel.IsVisible = false;
        InstallmentPanel.IsVisible = e.Value || InstallmentSwitch.IsToggled;
        UpdateSwitchState(RecurringStateLabel, e.Value);
        ConfigureSchedulePanel();
        RebuildInstallmentPreview();
    }

    private readonly List<DateTime> _recurrenceDueDates = [];
    private void OnRecurrencePlanChanged(object? sender, ValueChangedEventArgs e) => RebuildRecurrencePlan();
    private void RebuildRecurrencePlan()
    {
        var count = (int)RecurrenceStepper.Value;
        var start = (DueDatePicker.Date ?? DatePicker.Date ?? DateTime.Today).Date;
        var amount = decimal.TryParse(AmountEntry.Text, NumberStyles.Currency, _culture, out var total) && total > 0
            ? total : 0m;
        while (_recurrenceDueDates.Count < count)
            _recurrenceDueDates.Add(start.AddMonths(_recurrenceDueDates.Count));
        if (_recurrenceDueDates.Count > count) _recurrenceDueDates.RemoveRange(count, _recurrenceDueDates.Count - count);
        RecurrenceCountLabel.Text = $"{count} lançamentos";
        RecurrenceDatesContainer.Children.Clear();
        for (var index = 0; index < count; index++)
        {
            var current = index;
            var installmentValue = amount;
            var button = new Button { Text = $"{index + 1}/{count}  -  {_recurrenceDueDates[index]:dd/MM/yyyy}  -  {Currency(installmentValue)}",
                BackgroundColor = ThemeColor.Get("BlingCard"), TextColor = ThemeColor.Get("BlingPrimary"),
                BorderColor = ThemeColor.Get("BlingPrimary"), BorderWidth = 1, CornerRadius = 10, HeightRequest = 42 };
            button.Clicked += async (_, _) =>
            {
                var page = new CalendarDateSelectionPage("Vencimento da recorrência", _recurrenceDueDates[current]);
                page.DateSelected += (_, date) => { _recurrenceDueDates[current] = date.Date; RebuildRecurrencePlan(); };
                await Navigation.PushModalAsync(page);
            };
            RecurrenceDatesContainer.Children.Add(button);
        }
    }

    private async void OnSelectFrequencyClicked(object? sender, EventArgs e)
    {
        var values = FrequencyPicker.ItemsSource?.Cast<object>().Select(x => x.ToString() ?? "").ToList() ?? [];
        var options = values.Select((value, index) => new SelectionOption
        {
            Index = index,
            Label = _culture.TextInfo.ToTitleCase(value),
            ImageSource = MaterialIcons.Refresh,
            Background = ThemeColor.Get("BlingCard"),
            Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = FrequencyPicker.SelectedIndex == index
        });
        var page = new OptionSelectionPage("Selecione a frequência", options);
        page.Selected += (_, option) =>
        {
            FrequencyPicker.SelectedIndex = option.Index;
            FrequencySelectionButton.Text = option.Label;
        };
        await Navigation.PushModalAsync(page);
    }

    private async Task ReloadCategoriesSafelyAsync()
    {
        try
        {
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            ShowError($"Não foi possível carregar as categorias: {SqliteErrorMessage.ToFriendly(ex)}");
        }
    }

    private async void OnSelectCategoryClicked(object? sender, EventArgs e)
    {
        try
        {
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            ShowError($"Não foi possível carregar as categorias: {SqliteErrorMessage.ToFriendly(ex)}");
            return;
        }

        var options = _categories.Select((category, index) =>
        {
            return CategoryVisualResolver.Option(category, index, CategoryPicker.SelectedIndex == index);
        });
        var page = new OptionSelectionPage("Selecione a categoria", options);
        page.Selected += (_, option) =>
        {
            CategoryPicker.SelectedIndex = option.Index;
            CategorySelectionButton.Text = option.Label;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectAccountClicked(object? sender, EventArgs e)
    {
        var options = _accounts.Select((account, index) =>
        {
            var reserve = account.Name.Contains("reserva", StringComparison.OrdinalIgnoreCase) ||
                          account.Type is "poupanca" or "investimento";
            return new SelectionOption
            {
                Index = index, Label = $"{account.Name} · {account.Balance.ToString("C2", _culture)}",
                ImageSource = reserve ? MaterialIcons.Savings : MaterialIcons.AccountBalance,
                Background = ThemeColor.Get(reserve ? "BlingCard" : "BlingCard"),
                Foreground = ThemeColor.Get(reserve ? "BlingPrimary" : "BlingPrimary"),
                IsSelected = AccountPicker.SelectedIndex == index
            };
        });
        var page = new OptionSelectionPage("Selecione a conta", options);
        page.Selected += (_, option) =>
        {
            AccountPicker.SelectedIndex = option.Index;
            AccountSelectionButton.Text = _accounts[option.Index].Name;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectSupplierClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new()
            {
                Index = 0, Label = "Nenhum fornecedor", ImageSource = MaterialIcons.Storefront,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = SupplierPicker.SelectedIndex <= 0
            }
        };
        options.AddRange(_suppliers.Select((supplier, index) => new SelectionOption
        {
            Index = index + 1, Label = supplier.NomeFornecedor, ImageSource = MaterialIcons.Storefront,
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

    private async void OnSelectCardClicked(object? sender, EventArgs e)
    {
        var options = _cards.Select((card, index) => new SelectionOption
        {
            Index = index,
            Label = $"{card.Name} · {(card.CreditLimit - card.Used).ToString("C2", _culture)} disponíveis",
            ImageSource = MaterialIcons.CreditCard,
            Background = ThemeColor.Get("BlingCard"),
            Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = CardPicker.SelectedIndex == index
        });
        var page = new OptionSelectionPage("Selecione o cartão", options);
        page.Selected += (_, option) => CardPicker.SelectedIndex = option.Index;
        await Navigation.PushModalAsync(page);
    }

    private static (MaterialIcons Icon, string Background, string Foreground) CategoryVisual(string name)
    {
        var normalized = name.ToLowerInvariant();
        if (normalized.Contains("alimenta")) return (MaterialIcons.Restaurant, "AccentOrangeSurface", "AccentOrange");
        if (normalized.Contains("compra")) return (MaterialIcons.ShoppingBag, "AccentPinkSurface", "AccentPink");
        if (normalized.Contains("educa")) return (MaterialIcons.School, "BlingCard", "BlingPrimary");
        if (normalized.Contains("imposto")) return (MaterialIcons.RequestQuote, "BlingCard", "BlingText");
        if (normalized.Contains("lazer")) return (MaterialIcons.Movie, "BlingCard", "BlingText");
        if (normalized.Contains("moradia")) return (MaterialIcons.House, "BlingCard", "BlingText");
        if (normalized.Contains("saúde") || normalized.Contains("saude")) return (MaterialIcons.MedicalServices, "BlingCard", "BlingText");
        if (normalized.Contains("transporte")) return (MaterialIcons.DirectionsCar, "BlingCard", "AccentTeal");
        if (normalized.Contains("utilidade")) return (MaterialIcons.Lightbulb, "BlingCard", "BlingText");
        return (MaterialIcons.Category, "BlingCard", "BlingText");
    }

    private static MaterialIcons CategoryImage(CategoryItem category)
    {
        var storedIcon = category.Icon?.Trim();
        if (!string.IsNullOrWhiteSpace(storedIcon))
        {
            var iconName = Path.GetFileNameWithoutExtension(storedIcon);
            if (Enum.TryParse<MaterialIcons>(iconName, true, out var icon))
                return icon;

            var pascalName = string.Concat(iconName.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
            if (Enum.TryParse<MaterialIcons>(pascalName, true, out icon))
                return icon;
        }

        var value = category.Name.ToLowerInvariant();
        if (value.Contains("alimenta")) return MaterialIcons.Restaurant;
        if (value.Contains("compra")) return MaterialIcons.ShoppingBag;
        if (value.Contains("educa")) return MaterialIcons.School;
        if (value.Contains("imposto")) return MaterialIcons.RequestQuote;
        if (value.Contains("lazer")) return MaterialIcons.Movie;
        if (value.Contains("moradia") || value.Contains("casa")) return MaterialIcons.House;
        if (value.Contains("saúde") || value.Contains("saude")) return MaterialIcons.MedicalServices;
        if (value.Contains("transport")) return MaterialIcons.DirectionsCar;
        if (value.Contains("utilidade")) return MaterialIcons.Lightbulb;
        return MaterialIcons.Category;
    }

    private void OnCardChanged(object? sender, EventArgs e)
    {
        if (CardPicker.SelectedIndex >= 0 && CardPicker.SelectedIndex < _cards.Count)
        {
            var card = _cards[CardPicker.SelectedIndex];
            CardSelectionButton.Text = card.Name;
            var purchaseDate = DatePicker.Date ?? DateTime.Today;
            var cycle = purchaseDate.Day > card.ClosingDay ? purchaseDate.AddMonths(1) : purchaseDate;
            var dueDate = new DateTime(cycle.Year, cycle.Month,
                Math.Min(card.DueDay, DateTime.DaysInMonth(cycle.Year, cycle.Month)));
            if (dueDate < purchaseDate.Date)
            {
                cycle = cycle.AddMonths(1);
                dueDate = new DateTime(cycle.Year, cycle.Month,
                    Math.Min(card.DueDay, DateTime.DaysInMonth(cycle.Year, cycle.Month)));
            }
            FirstInstallmentDatePicker.Date = dueDate;
        }
        UpdateCardScheduleHint();
        RebuildInstallmentPreview();
    }

    private void ConfigureSource()
    {
        var useCard = SourceTypePicker.SelectedIndex == 1;
        var useAccount = SourceTypePicker.SelectedIndex == 0;
        AccountPanel.IsVisible = useAccount;
        CardPanel.IsVisible = useCard;

        if (useCard)
        {
            AccountPicker.SelectedIndex = -1;
            AccountSelectionButton.Text = "Selecione uma conta";
        }
        else if (useAccount)
        {
            CardPicker.SelectedIndex = -1;
            CardSelectionButton.Text = "Selecione o cartão";
        }
        else
        {
            AccountPicker.SelectedIndex = -1;
            CardPicker.SelectedIndex = -1;
            AccountSelectionButton.Text = "Selecione uma conta";
            CardSelectionButton.Text = "Selecione o cartão";
        }

        UpdateCardScheduleHint();
    }

    // ============================================================
    // PARCELAMENTO
    // ============================================================

    private void OnInstallmentToggled(object? sender, ToggledEventArgs e)
    {
        if (e.Value && RecurringSwitch.IsToggled)
            RecurringSwitch.IsToggled = false;
        InstallmentPanel.IsVisible = e.Value;
        UpdateSwitchState(InstallmentStateLabel, e.Value);
        DatePicker.IsEnabled = !e.Value;
        ConfigureSchedulePanel();
        RebuildInstallmentPreview();
    }

    private void OnInstallmentInputChanged(object? sender, EventArgs e) => RebuildInstallmentPreview();

    private void ConfigureSchedulePanel()
    {
        var recurring = RecurringSwitch.IsToggled;
        InstallmentSwitch.IsVisible = !recurring;
        InstallmentStateLabel.IsVisible = !recurring;
        ScheduleTitleLabel.Text = recurring ? "↻ CONFIGURAÇÃO DE RECORRÊNCIA" : "↻ CONFIGURAÇÃO DE PARCELAMENTO";
        ScheduleHintLabel.Text = recurring ? "Repita o valor integral em lançamentos mensais" : "Divida a despesa em até 36 parcelas";
        ScheduleCountCaption.Text = recurring ? "Quantidade de recorrências" : "Número de parcelas";
        ScheduleSummaryLabel.Text = recurring ? "RESUMO DAS OCORRÊNCIAS" : "RESUMO DAS PARCELAS";
        InstallmentCountLabel.Text = recurring
            ? $"{(int)InstallmentStepper.Value} ocorrências"
            : $"{(int)InstallmentStepper.Value} parcelas";
    }

    private void RebuildInstallmentPreview(bool preserveEdits = false)
    {
        if (InstallmentsContainer is null || InstallmentStepper is null)
            return;

        var count = (int)InstallmentStepper.Value;
        ConfigureSchedulePanel();
        InstallmentsContainer.Children.Clear();
        if ((!InstallmentSwitch.IsToggled && !RecurringSwitch.IsToggled) ||
            !decimal.TryParse(AmountEntry.Text, NumberStyles.Currency, _culture, out var total) ||
            total <= 0)
        {
            InstallmentTotalLabel.Text = Currency(0);
            return;
        }

        var mode = RecurringSwitch.IsToggled ? TransactionScheduleMode.Recurrence : TransactionScheduleMode.Installment;
        var schedule = preserveEdits && _editableSchedule.Count == count
            ? _editableSchedule.ToArray()
            : TransactionSchedulePlanner.Build(total, count,
                FirstInstallmentDatePicker.Date ?? DateTime.Today, mode);
        _editableSchedule.Clear();
        _editableSchedule.AddRange(schedule);
        _recurrenceDueDates.Clear();
        _recurrenceDueDates.AddRange(schedule.Select(item => item.Date));
        for (var index = 0; index < _editableSchedule.Count; index++)
        {
            var current = index;
            var item = _editableSchedule[current];
            var row = new Grid
            {
                ColumnDefinitions = [new(88), new(GridLength.Star), new(112)],
                ColumnSpacing = 6, Padding = new Thickness(2, 4)
            };
            row.Add(new Label { Text = RecurringSwitch.IsToggled ? $"Ocorrência {item.Number}" : $"{item.Number}/{item.Total}", TextColor = ThemeColor.Get("BlingText"), FontSize = 12 });
            var dateButton = new Button
            {
                Text = item.Date.ToString("dd/MM/yyyy"),
                TextColor = ThemeColor.Get("BlingPrimary"), FontSize = 11,
                BackgroundColor = Colors.Transparent, BorderColor = ThemeColor.Get("BlingBorder"),
                BorderWidth = 1, CornerRadius = 8, Padding = 4, HeightRequest = 38
            };
            dateButton.Clicked += async (_, _) =>
            {
                var page = new CalendarDateSelectionPage("Vencimento", _editableSchedule[current].Date);
                page.DateSelected += (_, date) =>
                {
                    var old = _editableSchedule[current];
                    _editableSchedule[current] = old with { Date = date.Date };
                    _recurrenceDueDates[current] = date.Date;
                    dateButton.Text = date.ToString("dd/MM/yyyy");
                };
                await Navigation.PushModalAsync(page);
            };
            row.Add(dateButton, 1);
            var amountEntry = new Entry
            {
                Text = item.Amount.ToString("N2", _culture), Keyboard = Keyboard.Numeric,
                TextColor = ThemeColor.Get("BlingPrimary"), FontSize = 12,
                HorizontalTextAlignment = TextAlignment.End, BackgroundColor = Colors.Transparent
            };
            amountEntry.Unfocused += (_, _) =>
            {
                if (decimal.TryParse(amountEntry.Text, NumberStyles.Currency, _culture, out var value) && value > 0)
                {
                    var old = _editableSchedule[current];
                    _editableSchedule[current] = old with { Amount = value };
                    InstallmentTotalLabel.Text = Currency(_editableSchedule.Sum(entry => entry.Amount));
                }
                else
                    amountEntry.Text = _editableSchedule[current].Amount.ToString("N2", _culture);
            };
            row.Add(amountEntry, 2);
            InstallmentsContainer.Children.Add(row);
        }
        InstallmentTotalLabel.Text = Currency(_editableSchedule.Sum(item => item.Amount));
        UpdateCardScheduleHint();
    }

    private void UpdateCardScheduleHint()
    {
        if (CardScheduleHint is null)
            return;
        var useCard = SourceTypePicker.SelectedIndex == 1;
        var card = useCard && CardPicker.SelectedIndex >= 0 && CardPicker.SelectedIndex < _cards.Count
            ? _cards[CardPicker.SelectedIndex] : null;
        CardScheduleHint.IsVisible = card is not null && InstallmentSwitch.IsToggled;
        if (card is not null)
            CardScheduleHint.Text =
                $"Cartão fecha dia {card.ClosingDay} e vence dia {card.DueDay}. As datas representam os vencimentos mensais.";
    }

    // ============================================================
    // OBSERVAÇÕES
    // ============================================================

    private void OnDetailsToggled(object? sender, ToggledEventArgs e)
    {
        DetailsPanel.IsVisible = e.Value;
        UpdateSwitchState(DetailsStateLabel, e.Value);
        if (e.Value)
            NotesEditor.Focus();
    }

    private static void UpdateSwitchState(Label label, bool isOn)
    {
        label.Text = isOn ? "Ligado" : "Desligado";
        label.TextColor = ThemeColor.Get(isOn ? "BlingPrimary" : "BlingText");
    }

    // ============================================================
    // SALVAR
    // ============================================================

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        ResetValidationHighlights();

        // Validar descrição
        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
        {
            await ShowValidationAsync(DescriptionBorder, "O campo Descrição deve ser preenchido.");
            return;
        }

        // Validar categoria
        if (CategoryPicker.SelectedIndex < 0)
        {
            await ShowValidationAsync(CategorySelectionButton, "Selecione uma categoria para o lançamento.");
            return;
        }

        // Validar valor
        if (!FinanceInputHelper.TryParsePositiveAmount(AmountEntry.Text, out var amount))
        {
            await ShowValidationAsync(AmountBorder, "O campo Valor deve ser preenchido com um valor maior que zero.");
            return;
        }

        // Validar origem
        var useCard = SourceTypePicker.SelectedIndex == 1;
        var useAccount = SourceTypePicker.SelectedIndex == 0;
        var accountRequiredNow = _selectedType == "receita" || PaidSwitch.IsToggled;
        if ((useCard && CardPicker.SelectedIndex < 0) ||
            (useAccount && accountRequiredNow && AccountPicker.SelectedIndex < 0))
        {
            await ShowValidationAsync(useCard ? CardSelectionButton : AccountSelectionButton,
                useCard ? "Selecione um cartão de crédito." : "Selecione uma conta bancária.");
            return;
        }
        try
        {
            var transaction = new TransactionDraft(
                DescriptionEntry.Text.Trim(),
                amount,
                DatePicker.Date ?? DateTime.Today,
                _selectedType,
                _categories[CategoryPicker.SelectedIndex].Id,
                useAccount && AccountPicker.SelectedIndex >= 0 ? _accounts[AccountPicker.SelectedIndex].Id : null,
                useCard ? _cards[CardPicker.SelectedIndex].Id : null,
                DetailsSwitch.IsToggled ? NotesEditor.Text?.Trim() : null,
                null,
                _selectedType == "despesa" ? DueDatePicker.Date : null,
                PaidSwitch.IsToggled,
                PaidSwitch.IsToggled ? PaymentDatePicker.Date : null,
                RecurringSwitch.IsToggled,
                RecurringSwitch.IsToggled ? FrequencyPicker.SelectedItem?.ToString() : null,
                _tagChecks.Where(x => x.Value.IsChecked).Select(x => x.Key).ToArray(),
                _selectedType == "despesa" && SupplierPicker.SelectedIndex > 0
                    ? _suppliers[SupplierPicker.SelectedIndex - 1].IdFornecedor : null,
                RecurringSwitch.IsToggled ? (int)InstallmentStepper.Value : null,
                RecurringSwitch.IsToggled ? _recurrenceDueDates.ToArray() : null,
                RecurringSwitch.IsToggled ? _editableSchedule.Select(item => item.Amount).ToArray() : null
            );

            if (_editingId is long editingId)
            {
                if (_existingInstallment)
                {
                    await _database.UpdateInstallmentScheduleAsync(editingId, _editableSchedule);
                }
                else if (InstallmentSwitch.IsToggled || RecurringSwitch.IsToggled && !_editingData!.Recurring)
                {
                    await _database.ConvertTransactionToInstallmentsAsync(editingId,
                        transaction.Description, transaction.Amount, transaction.CategoryId,
                        transaction.Notes, (int)InstallmentStepper.Value,
                        FirstInstallmentDatePicker.Date ?? DateTime.Today,
                        RecurringSwitch.IsToggled, transaction.Frequency, transaction.TagIds,
                        transaction.SupplierId, _editableSchedule);
                }
                else
                {
                    await _database.UpdateListedTransactionAsync(editingId, _selectedType,
                        transaction.Description, transaction.Amount, transaction.Date,
                        transaction.CategoryId, transaction.Notes, transaction.DueDate,
                        transaction.Recurring, transaction.Frequency, transaction.TagIds,
                        transaction.SupplierId, transaction.AccountId, transaction.CardId,
                        transaction.Paid, transaction.PaymentDate);
                }
                await ThemedDialog.ShowAsync(this, "Conta alterada", "Conta alterada com sucesso!");
                Saved?.Invoke(this, EventArgs.Empty);
                await Navigation.PopModalAsync();
                return;
            }

            if (InstallmentSwitch.IsToggled)
            {
                var result = await _database.SaveInstallmentTransactionAsync(
                    transaction, (int)InstallmentStepper.Value,
                    FirstInstallmentDatePicker.Date ?? DateTime.Today, _editableSchedule);
                await ThemedDialog.ShowAsync(this, "Parcelamento salvo",
                    $"{result.Installments} parcelas geradas.\nTotal: {Currency(result.Total)}");
            }
            else
            {
                await _database.AddTransactionAsync(transaction);
                var successMessage = _selectedType == "receita" && RecurringSwitch.IsToggled
                    ? (string.Equals(FrequencyPicker.SelectedItem?.ToString(), "anual", StringComparison.OrdinalIgnoreCase)
                        ? "Receita recorrente salva: 5 lançamentos anuais foram gerados."
                        : "Salário recorrente salvo: 12 lançamentos mensais foram gerados.")
                    : _selectedType == "receita"
                    ? "Salário/receita lançado com sucesso!"
                    : _selectedType == "despesa" && RecurringSwitch.IsToggled
                    ? (string.Equals(FrequencyPicker.SelectedItem?.ToString(), "anual", StringComparison.OrdinalIgnoreCase)
                        ? "Conta recorrente salva: 5 lançamentos anuais foram gerados."
                        : "Conta recorrente salva: 12 contas mensais foram geradas.")
                    : "Despesa lançada com sucesso!";
                await ThemedDialog.ShowAsync(this, "Lançamento salvo", successMessage);
            }

            Saved?.Invoke(this, EventArgs.Empty);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            var message = SqliteErrorMessage.ToFriendly(ex);
            ShowError(message);
            await ThemedDialog.ShowAsync(this, "Não foi possível salvar", message, "Fechar");
        }
    }

    private async Task ShowValidationAsync(VisualElement field, string message)
    {
        HighlightField(field);
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
        await ThemedDialog.ShowAsync(this, "Campo obrigatório", message, "Corrigir");
        field.Focus();
    }

    private static void HighlightField(VisualElement field)
    {
        var color = Color.FromArgb("#F59E0B");
        if (field is Border border)
        {
            border.Stroke = color;
            border.StrokeThickness = 2;
        }
        else if (field is Button button)
        {
            button.BorderColor = color;
            button.BorderWidth = 2;
        }
    }

    private void ResetValidationHighlights()
    {
        var color = ThemeColor.Get("BlingBorder");
        DescriptionBorder.Stroke = color;
        DescriptionBorder.StrokeThickness = 1;
        AmountBorder.Stroke = color;
        AmountBorder.StrokeThickness = 1;
        foreach (var button in new[] { CategorySelectionButton, AccountSelectionButton, CardSelectionButton })
        {
            button.BorderColor = color;
            button.BorderWidth = 1;
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private static string Currency(decimal value) => FinanceInputHelper.Currency(value);

    // ============================================================
    // CANCELAR
    // ============================================================

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
