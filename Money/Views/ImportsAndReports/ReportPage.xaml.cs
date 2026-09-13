using System.Globalization;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money.Views.ImportsAndReports;

public partial class ReportPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly ReportPdfService _reports;
    private readonly FinancialForecastService _forecast;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<SubcategoryItem> _categories = [];
    private List<Fornecedor> _suppliers = [];
    private List<CardItem> _cards = [];
    private int _month = DateTime.Today.Month;
    private int _year = DateTime.Today.Year;
    private long? _categoryId;
    private string? _categoryName;
    private long? _supplierId;
    private string? _supplierName;
    private long? _cardId;
    private string? _cardName;
    private string _paymentStatus = "todas";
    private ReportPdfResult? _result;

    public ReportPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        _reports = new ReportPdfService(database);
        _forecast = new FinancialForecastService(database);
        UpdatePeriodText();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
        ErrorLabel.IsVisible = false;
        if (_categories.Count == 0)
            _categories = await _database.GetSubcategoriesAsync();
        if (_suppliers.Count == 0)
            _suppliers = await _database.GetSuppliersAsync();
        if (_cards.Count == 0)
            _cards = await _database.GetCardsAsync();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = SqliteErrorMessage.ToFriendly(ex);
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnSelectPeriodClicked(object? sender, EventArgs e)
    {
        var page = new MonthYearSelectionPage(_month, _year);
        page.PeriodSelected += (_, selected) =>
        {
            if (selected.Month is not int month || selected.Year is not int year) return;
            _month = month;
            _year = year;
            UpdatePeriodText();
            ResetResult();
        };
        await Navigation.PushModalAsync(page);
    }

    private long? _mainCategoryId;
    private async void OnSelectCategoryClicked(object? sender, EventArgs e)
    {
        try { await CategoryFilterPicker.ShowAsync(this,_database,(main,sub,label)=> { _mainCategoryId=main; _categoryId=sub; _categoryName=label; CategoryButton.Text=label; ResetResult(); }); }
        catch(Exception ex) { await ThemedDialog.ShowAsync(this,Title,ex.Message); }
    }

    private async void OnSelectSupplierClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new()
            {
                Index = 0, Label = "Todos os fornecedores", ImageSource = MaterialIcons.Storefront,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = _supplierId is null
            }
        };
        options.AddRange(_suppliers.Select((supplier, index) => new SelectionOption
        {
            Index = index + 1, Label = supplier.NomeFornecedor,
            ImageSource = MaterialIcons.Storefront,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = _supplierId == supplier.IdFornecedor
        }));

        var page = new OptionSelectionPage("Selecione o fornecedor", options);
        page.Selected += (_, option) =>
        {
            if (option.Index == 0)
            {
                _supplierId = null;
                _supplierName = null;
                SupplierButton.Text = "Todos os fornecedores";
            }
            else
            {
                var supplier = _suppliers[option.Index - 1];
                _supplierId = supplier.IdFornecedor;
                _supplierName = supplier.NomeFornecedor;
                SupplierButton.Text = _supplierName;
            }
            ResetResult();
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectPaymentStatusClicked(object? sender, EventArgs e)
    {
        var values = new[]
        {
            (Value: "todas", Label: "Todas (abertas e pagas)"),
            (Value: "abertas", Label: "Somente abertas"),
            (Value: "pagas", Label: "Somente pagas")
        };
        var options = values.Select((value, index) => new SelectionOption
        {
            Index = index, Label = value.Label, IsSelected = _paymentStatus == value.Value,
            ImageSource = MaterialIcons.RequestQuote,
            Background = ThemeColor.Get(value.Value == "pagas" ? "BlingCard" : "BlingCard"),
            Foreground = ThemeColor.Get(value.Value == "pagas" ? "BlingPrimary" : "BlingPrimary")
        });
        var page = new OptionSelectionPage("Selecione a situação", options);
        page.Selected += (_, option) =>
        {
            _paymentStatus = values[option.Index].Value;
            PaymentStatusButton.Text = values[option.Index].Label;
            ResetResult();
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnSelectCardClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new()
            {
                Index = 0, Label = "Todos os cartões", ImageSource = MaterialIcons.CreditCard,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = _cardId is null
            }
        };
        options.AddRange(_cards.Select((card, index) => new SelectionOption
        {
            Index = index + 1, Label = card.Name, ImageSource = MaterialIcons.CreditCard,
            Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
            IsSelected = _cardId == card.Id
        }));
        var page = new OptionSelectionPage("Selecione o cartão", options);
        page.Selected += (_, option) =>
        {
            if (option.Index == 0)
            {
                _cardId = null;
                _cardName = null;
            }
            else
            {
                var card = _cards[option.Index - 1];
                _cardId = card.Id;
                _cardName = card.Name;
            }
            CardButton.Text = option.Label;
            ResetResult();
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnGenerateClicked(object? sender, EventArgs e)
    {
        SetBusy(true);
        try
        {
            await Task.Yield();
            _result = await _reports.GenerateAsync(_month, _year, _categoryId, _categoryName,
                _paymentStatus, _supplierId, _supplierName, _cardId, _cardName, _mainCategoryId);
            var projectionStart = new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
            var selectedOffset = Math.Max(0,(_year-projectionStart.Year)*12+_month-projectionStart.Month);
            var projection = await _forecast.CalculateAsync(projectionStart,Math.Min(24,Math.Max(12,selectedOffset+1)));
            var projectedMonth = projection.FindMonth(_month,_year);
            ResultLabel.Text = $"{_result.ItemCount} lançamento(s)\n" +
                               $"Receitas realizadas: {_result.Income.ToString("C2", _culture)}\n" +
                               $"Despesas pagas: {_result.PaidExpenses.ToString("C2", _culture)}\n" +
                               $"Despesas pendentes: {_result.PendingExpenses.ToString("C2", _culture)}\n" +
                               $"Saldo realizado: {_result.Balance.ToString("C2", _culture)}\n" +
                               $"Receitas previstas: {(projectedMonth?.ExpectedIncome ?? 0).ToString("C2",_culture)}\n" +
                               $"Contas previstas: {(projectedMonth?.ExpectedExpenses ?? 0).ToString("C2",_culture)}\n" +
                               $"Resultado projetado do mês: {(projectedMonth?.ProjectedResult ?? 0).ToString("C2",_culture)}";
            ResultCard.IsVisible = true;
            GenerateStatusLabel.Text = "PDF gerado. Escolha onde deseja salvar.";
            var destination = await _reports.SaveAndOpenAsync(_result);
            ResultLabel.Text += $"\n\nSalvo em: {destination}";
            await ThemedDialog.ShowAsync(this, "Relatório PDF", "O relatório foi salvo e será aberto agora.");
        }
        catch (OperationCanceledException)
        {
            ErrorLabel.Text = "O PDF foi gerado, mas o salvamento foi cancelado.";
            ErrorLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            var message = SqliteErrorMessage.ToFriendly(ex);
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
            await ThemedDialog.ShowAsync(this, "Não foi possível gerar o PDF", message);
        }
        finally { SetBusy(false); }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_result is null) return;
        try
        {
            var destination = await _reports.SaveAsync(_result);
            await ThemedDialog.ShowAsync(this, "PDF salvo", $"Arquivo salvo em:\n{destination}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível salvar",
                SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private async void OnShareClicked(object? sender, EventArgs e)
    {
        if (_result is null) return;
        try { await _reports.ShareAsync(_result); }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível compartilhar",
                SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private void UpdatePeriodText() => PeriodButton.Text =
        _culture.TextInfo.ToTitleCase(new DateTime(_year, _month, 1).ToString("MMMM/yyyy", _culture));

    private void ResetResult()
    {
        _result = null;
        ResultCard.IsVisible = false;
    }

    private void SetBusy(bool busy)
    {
        GenerateButton.IsEnabled = !busy;
        GenerateActivity.IsVisible = busy;
        GenerateActivity.IsRunning = busy;
        GenerateStatusLabel.IsVisible = busy;
        if (busy) GenerateStatusLabel.Text = "Gerando o arquivo PDF...";
        ErrorLabel.IsVisible = false;
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
