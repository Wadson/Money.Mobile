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

namespace Money;

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

        // Inicializar com o tipo correto
        if (initialType == "receita")
            OnTypeIncomeClicked(null, null);
        else
            OnTypeExpenseClicked(null, null);
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
        DestinationAccountPicker.ItemsSource = _accounts.Select(x =>
            $"{x.Name} · {x.Balance.ToString("C2", _culture)}").ToList();
        RenderTags();
        ConfigureSource();
    }

    private async Task LoadCategoriesAsync()
    {
        _categories = await _database.GetCategoriesAsync(_selectedType);
        CategoryPicker.ItemsSource = _categories.Select(x =>
            $"{new string('·', Math.Max(0, x.Level - 1))} {x.Name}".Trim()).ToList();
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
        SourceTypePicker.ItemsSource = new[] { "Conta bancária", "Cartão de crédito" };
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
                : MaterialIcons.AccountBalance,
            Background = ThemeColor.Get(index == 1 ? "BlingCard" : "BlingCard"),
            Foreground = ThemeColor.Get(index == 1 ? "BlingPrimary" : "BlingPrimary"),
            IsSelected = SourceTypePicker.SelectedIndex == index
        });
        var page = new OptionSelectionPage("Selecione conta ou cartão", options);
        page.Selected += (_, option) => SourceTypePicker.SelectedIndex = option.Index;
        await Navigation.PushModalAsync(page);
    }

    private void OnTypeTransferClicked(object? sender, EventArgs? e)
    {
        _selectedType = "transferencia";
        InstallmentSwitch.IsToggled = false;
        InstallmentCard.IsVisible = false;
        ResetTypeButtons();
        TypeTransfer.Text = "Transferir";
        TypeTransfer.BackgroundColor = ThemeColor.Get("BlingPrimary");
        TypeTransfer.TextColor = ThemeColor.Get("BlingTextLight");
        TypeTransfer.BorderColor = ThemeColor.Get("BlingPrimary");
        TypeTransfer.BorderWidth = 2;
        SourceTypePicker.ItemsSource = new[] { "Conta bancária" };
        SourceTypePicker.SelectedIndex = 0;
        ConfigureTypePanels();
        ConfigureSource();
    }

    private void ResetTypeButtons()
    {
        TypeExpense.Text = "Despesa";
        TypeIncome.Text = "Receita";
        TypeTransfer.Text = "Transferir";

        foreach (var button in new[] { TypeExpense, TypeIncome, TypeTransfer })
        {
            button.BackgroundColor = ThemeColor.Get("BlingCard");
            button.TextColor = ThemeColor.Get("BlingText");
            button.BorderColor = ThemeColor.Get("BlingText");
            button.BorderWidth = 1;
        }
    }

    private void ConfigureTypePanels()
    {
        var transfer = _selectedType == "transferencia";
        CategoryPanel.IsVisible = !transfer;
        DestinationPanel.IsVisible = transfer;
        ScheduleCard.IsVisible = !transfer;
        ExpenseStatusPanel.IsVisible = _selectedType == "despesa";
        SupplierPanel.IsVisible = _selectedType == "despesa";
        RecurrenceRow.IsVisible = !transfer;
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
        UpdateSwitchState(RecurringStateLabel, e.Value);
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
        AccountPanel.IsVisible = !useCard;
        CardPanel.IsVisible = useCard;

        if (useCard)
        {
            AccountPicker.SelectedIndex = -1;
            AccountSelectionButton.Text = "Selecione uma conta";
        }
        else
        {
            CardPicker.SelectedIndex = -1;
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
        RebuildInstallmentPreview();
    }

    private void OnInstallmentInputChanged(object? sender, EventArgs e) => RebuildInstallmentPreview();

    private void RebuildInstallmentPreview()
    {
        if (InstallmentsContainer is null || InstallmentStepper is null)
            return;

        var count = (int)InstallmentStepper.Value;
        InstallmentCountLabel.Text = $"{count} parcelas";
        InstallmentsContainer.Children.Clear();
        if (!InstallmentSwitch.IsToggled ||
            !decimal.TryParse(AmountEntry.Text, NumberStyles.Currency, _culture, out var total) ||
            total <= 0)
        {
            InstallmentTotalLabel.Text = Currency(0);
            return;
        }

        var regular = Math.Round(total / count, 2, MidpointRounding.AwayFromZero);
        decimal allocated = 0;
        for (var number = 1; number <= count; number++)
        {
            var amount = number == count ? total - allocated : regular;
            allocated += amount;
            var row = new Grid
            {
                ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)],
                Padding = new Thickness(4, 7)
            };
            row.Add(new Label { Text = $"{number}/{count}", TextColor = ThemeColor.Get("BlingText"), FontSize = 12 });
            row.Add(new Label
            {
                Text = (FirstInstallmentDatePicker.Date ?? DateTime.Today).AddMonths(number - 1).ToString("dd/MM/yyyy"),
                TextColor = ThemeColor.Get("BlingText"), FontSize = 12, HorizontalTextAlignment = TextAlignment.Center
            }, 1);
            row.Add(new Label
            {
                Text = Currency(amount), TextColor = ThemeColor.Get("BlingPrimary"),
                FontSize = 12, FontAttributes = FontAttributes.Bold
            }, 2);
            InstallmentsContainer.Children.Add(row);
        }
        InstallmentTotalLabel.Text = Currency(allocated);
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
        if (_selectedType != "transferencia" && CategoryPicker.SelectedIndex < 0)
        {
            await ShowValidationAsync(CategorySelectionButton, "Selecione uma categoria para o lançamento.");
            return;
        }

        // Validar valor
        if (!decimal.TryParse(AmountEntry.Text, NumberStyles.Currency, _culture, out var amount) || amount <= 0)
        {
            await ShowValidationAsync(AmountBorder, "O campo Valor deve ser preenchido com um valor maior que zero.");
            return;
        }

        // Validar origem
        var useCard = SourceTypePicker.SelectedIndex == 1;
        if ((!useCard && AccountPicker.SelectedIndex < 0) || (useCard && CardPicker.SelectedIndex < 0))
        {
            await ShowValidationAsync(useCard ? CardSelectionButton : AccountSelectionButton,
                useCard ? "Selecione um cartão de crédito." : "Selecione uma conta bancária.");
            return;
        }
        if (_selectedType == "transferencia")
        {
            if (AccountPicker.SelectedIndex < 0 || DestinationAccountPicker.SelectedIndex < 0)
            {
                await ShowValidationAsync(AccountSelectionButton, "Selecione as contas de origem e destino.");
                return;
            }
            if (_accounts[AccountPicker.SelectedIndex].Id ==
                _accounts[DestinationAccountPicker.SelectedIndex].Id)
            {
                await ShowValidationAsync(AccountSelectionButton, "As contas de origem e destino devem ser diferentes.");
                return;
            }
        }

        try
        {
            var transaction = new TransactionDraft(
                DescriptionEntry.Text.Trim(),
                amount,
                DatePicker.Date ?? DateTime.Today,
                _selectedType,
                _selectedType == "transferencia" ? 0 : _categories[CategoryPicker.SelectedIndex].Id,
                useCard ? null : _accounts[AccountPicker.SelectedIndex].Id,
                useCard ? _cards[CardPicker.SelectedIndex].Id : null,
                DetailsSwitch.IsToggled ? NotesEditor.Text?.Trim() : null,
                _selectedType == "transferencia"
                    ? _accounts[DestinationAccountPicker.SelectedIndex].Id : null,
                _selectedType == "despesa" ? DueDatePicker.Date : null,
                _selectedType == "receita" || PaidSwitch.IsToggled,
                PaidSwitch.IsToggled ? PaymentDatePicker.Date : null,
                RecurringSwitch.IsToggled,
                RecurringSwitch.IsToggled ? FrequencyPicker.SelectedItem?.ToString() : null,
                _tagChecks.Where(x => x.Value.IsChecked).Select(x => x.Key).ToArray(),
                _selectedType == "despesa" && SupplierPicker.SelectedIndex > 0
                    ? _suppliers[SupplierPicker.SelectedIndex - 1].IdFornecedor : null
            );

            if (InstallmentSwitch.IsToggled)
            {
                var result = await _database.SaveInstallmentTransactionAsync(
                    transaction, (int)InstallmentStepper.Value,
                    FirstInstallmentDatePicker.Date ?? DateTime.Today);
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

    private string Currency(decimal value) => value.ToString("C2", _culture);

    // ============================================================
    // CANCELAR
    // ============================================================

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
