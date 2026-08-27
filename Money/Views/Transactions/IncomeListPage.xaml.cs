using System.Collections.ObjectModel;
using System.Globalization;
using Money.Models;
using Money.Services;

namespace Money.Views.Transactions;

public partial class IncomeListPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly ObservableCollection<ReceitaListItem> _items = [];
    private int? _selectedMonth;
    private int? _selectedYear;

    public IncomeListPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        DesktopList.ItemsSource = _items;
        MobileList.ItemsSource = _items;
        // Sem filtro por padrão: exibe todas as receitas cadastradas.
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        ErrorLabel.IsVisible = false;
        try
        {
            var month = _selectedMonth;
            var year = _selectedYear;
            var items = await _database.GetReceitasAsync(month, year);
            var summary = await _database.GetResumoReceitasAsync(month, year);
            _items.Clear();
            foreach (var item in items) _items.Add(item);
            CountLabel.Text = summary.TotalReceitas.ToString();
            ValueLabel.Text = summary.ValorTotal.ToString("C2", _culture);
            AverageLabel.Text = summary.Media.ToString("C2", _culture);
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnFilterClicked(object? sender, EventArgs e) => await LoadAsync();

    private async void OnSelectPeriodClicked(object? sender, EventArgs e)
    {
        var page = new MonthYearSelectionPage(_selectedMonth, _selectedYear);
        page.PeriodSelected += (_, selected) =>
        {
            _selectedMonth = selected.Month;
            _selectedYear = selected.Year;
            PeriodSelectionButton.Text = selected.Month is int month && selected.Year is int year
                ? new DateTime(year, month, 1).ToString("MMMM/yyyy", _culture)
                : "Todos os períodos";
        };
        await Navigation.PushModalAsync(page);
    }

    private async void OnClearFilterClicked(object? sender, EventArgs e)
    {
        _selectedMonth = null;
        _selectedYear = null;
        PeriodSelectionButton.Text = "Todos os períodos";
        await LoadAsync();
    }
    private async void OnNewIncomeClicked(object? sender, EventArgs e)
    {
        var page=new IncomeFormPage(_database); page.Saved+=async(_,_)=>await LoadAsync();
        await Navigation.PushModalAsync(page);
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (!TryGetId(sender, out var id)) return;
        var page = new IncomeFormPage(_database, id);
        page.Saved += async (_, _) => await LoadAsync();
        await Navigation.PushModalAsync(page);
    }

    private async void OnToggleStatusClicked(object? sender,EventArgs e)
    {
        if(!TryGetId(sender,out var id))return; var item=_items.FirstOrDefault(x=>x.Id==id); if(item is null)return;
        try { await _database.SetIncomePaidStatusAsync(id,!item.Paid,!item.Paid?DateTime.Today:null); await LoadAsync(); }
        catch(Exception ex){await ThemedDialog.ShowAsync(this,"Não foi possível alterar o status",SqliteErrorMessage.ToFriendly(ex));}
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (!TryGetId(sender, out var id)) return;
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item is null) return;
        var confirm = await ThemedDialog.ConfirmDeleteAsync(this, "Excluir receita",
            $"Excluir permanentemente “{item.Descricao}” e reverter seu valor do saldo?");
        if (!confirm) return;
        try
        {
            await _database.DeleteListedTransactionAsync(id, "receita");
            await LoadAsync();
            await ThemedDialog.ShowAsync(this, "Receita excluída", "Receita excluída com sucesso!");
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Não foi possível excluir", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }

    private static bool TryGetId(object? sender, out long id)
    {
        id = 0;
        return sender is Button button &&
               long.TryParse(button.CommandParameter?.ToString(), out id);
    }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        var desktop = Width >= 800;
        DesktopHeader.IsVisible = desktop;
        DesktopList.IsVisible = desktop;
        MobileList.IsVisible = !desktop;
        SummaryGrid.ColumnDefinitions = desktop
            ? [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)]
            : [new(GridLength.Star)];
        ValueCard.SetValue(Grid.ColumnProperty, desktop ? 1 : 0);
        ValueCard.SetValue(Grid.RowProperty, desktop ? 0 : 1);
        AverageCard.SetValue(Grid.ColumnProperty, desktop ? 2 : 0);
        AverageCard.SetValue(Grid.RowProperty, desktop ? 0 : 2);
    }
}
