using MauiIcons.Core;
using System.Globalization;
using Money.Helpers;
using Money.Models;
using Money.Services;
using Money.Views.Dialogs;

namespace Money.Views.Transactions;

public partial class IncomeFormPage : ContentPage
{
    private readonly DatabaseService _database; private readonly long? _id;
    private long? _mainCategoryId;
    private List<SubcategoryItem> _categories=[];
    private bool _saving;
    public event EventHandler? Saved;
    public IncomeFormPage(DatabaseService database,long? id=null)
    {
        InitializeComponent();
        CategoryPicker.SelectedIndexChanged += (_, _) =>
        {
            var index = CategoryPicker.SelectedIndex;
            if (index < 0 || index >= _categories.Count) return;
            var sub = _categories[index];
            _mainCategoryId = sub.MainCategoryId;
            MainCategorySelectionButton.Text = sub.MainCategoryName;
            CategorySelectionButton.Text = sub.Name;
            CategorySelectionButton.ImageSource = CategoryVisualResolver.Icon(sub).ToImageSource(CategoryVisualResolver.Foreground(sub), 24);
        };
 _database=database; _id=id;
        ExpectedDatePicker.Date=DateTime.Today;
        FrequencyPicker.ItemsSource=new[]{"mensal","anual"}; FrequencyPicker.SelectedIndex=0;
        DescriptionEntry.TextChanged += OnFormValueChanged;
        AmountEntry.TextChanged += OnFormValueChanged;
        CategoryPicker.SelectedIndexChanged += OnFormSelectionChanged;
        if(id is not null) TitleLabel.Text="Editar salário/recebimento";
        UpdateRecurrenceCountLabel();
        UpdateSaveState();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); if(_categories.Count>0)return;
        try
        {
            _categories=await _database.GetSubcategoriesAsync(type: "receita", includeInactive: true);
            CategoryPicker.ItemsSource=_categories.Select(x=>x.Name).ToList();
            if(_id is long id)
            {
                var data=await _database.GetTransactionForEditAsync(id,"receita");
                DescriptionEntry.Text=data.Description; AmountEntry.Text=data.Amount.ToString("N2",CultureInfo.GetCultureInfo("pt-BR"));
                ExpectedDatePicker.Date=data.DueDate??data.Date;
                RecurringSwitch.IsToggled=data.Recurring; FrequencyPicker.SelectedItem=data.Frequency??"mensal"; NotesEditor.Text=data.Notes;
                FrequencySelectionButton.Text=CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(data.Frequency??"mensal");
                CategoryPicker.SelectedIndex=_categories.FindIndex(x=>x.Id==data.CategoryId);
                CategorySelectionButton.Text=CategoryPicker.SelectedIndex>=0?_categories[CategoryPicker.SelectedIndex].Name:"Selecione a categoria";
            }
            UpdateSaveState();
        }
        catch(Exception ex){ShowError(SqliteErrorMessage.ToFriendly(ex));}
    }
    private async void OnSaveClicked(object? sender,EventArgs e)
    {
        if (_saving) return;
        if(string.IsNullOrWhiteSpace(DescriptionEntry.Text)||!FinanceInputHelper.TryParsePositiveAmount(AmountEntry.Text,out var amount)||CategoryPicker.SelectedIndex<0){ShowError("Preencha descrição, valor, categoria e vencimento.");UpdateSaveState();return;}
        var recurrenceCount = (int)RecurrenceCountStepper.Value;
        if (recurrenceCount is < 1 or > 36) { ShowError("A quantidade deve estar entre 1 e 36."); return; }
        try
        {
            _saving = true; UpdateSaveState();
            var date=ExpectedDatePicker.Date??DateTime.Today;
            if(_id is long id)
                await _database.UpdateListedTransactionAsync(id,"receita",DescriptionEntry.Text,amount,date,_categories[CategoryPicker.SelectedIndex].Id,NotesEditor.Text?.Trim(),null,RecurringSwitch.IsToggled,FrequencyPicker.SelectedItem?.ToString(),null,null,null,null,false,null);
            else
                await _database.AddTransactionAsync(new TransactionDraft(DescriptionEntry.Text,amount,date,"receita",_categories[CategoryPicker.SelectedIndex].Id,null,null,NotesEditor.Text?.Trim(),DueDate:date,Recurring:RecurringSwitch.IsToggled,Frequency:FrequencyPicker.SelectedItem?.ToString(),RecurrenceCount:RecurringSwitch.IsToggled?recurrenceCount:null));
            Saved?.Invoke(this,EventArgs.Empty);
            var message = RecurringSwitch.IsToggled && _id is null
                ? $"{recurrenceCount} recebimento{(recurrenceCount == 1 ? "" : "s")} programado{(recurrenceCount == 1 ? "" : "s")}."
                : "Recebimento salvo com sucesso.";
            await ThemedDialog.ShowAsync(this,"Receita salva",message,"Continuar");
            await Navigation.PopModalAsync();
        }
        catch(Exception ex){ShowError(SqliteErrorMessage.ToFriendly(ex));}
        finally { _saving = false; UpdateSaveState(); }
    }
    private void OnRecurringToggled(object? sender,ToggledEventArgs e){FrequencyPanel.IsVisible=e.Value;UpdateSaveState();}
    private void OnRecurrenceCountChanged(object? sender,ValueChangedEventArgs e){UpdateRecurrenceCountLabel();UpdateSaveState();}
    private void UpdateRecurrenceCountLabel(){var count=(int)RecurrenceCountStepper.Value;RecurrenceCountLabel.Text=$"{count} recebimento{(count==1?"":"s")}";}
    private void OnFormValueChanged(object? sender,TextChangedEventArgs e)=>UpdateSaveState();
    private void OnFormSelectionChanged(object? sender,EventArgs e)=>UpdateSaveState();
    private async void OnSelectMainCategoryClicked(object? sender, EventArgs e)
    {
        try
        {
            var parents = await _database.GetMainCategoriesAsync("receita");
            var page = new OptionSelectionPage("Categoria principal", parents.Select((x,i)=>CategoryVisualResolver.Option(x,i,x.Id==_mainCategoryId)));
            page.Selected += (_, option) =>
            {
                var main = parents[option.Index];
                _mainCategoryId = main.Id;
                MainCategorySelectionButton.Text = main.Name;
                MainCategorySelectionButton.ImageSource = CategoryVisualResolver.Icon(main).ToImageSource(CategoryVisualResolver.Foreground(main),24);
                CategoryPicker.SelectedIndex = -1;
                CategorySelectionButton.Text = "Selecione a subcategoria";
            };
            await Navigation.PushModalAsync(page);
        }
        catch(Exception ex) { await ThemedDialog.ShowAsync(this,"Categorias",ex.Message); }
    }
    private async void OnSelectCategoryClicked(object? sender, EventArgs e)
    {
        if (_mainCategoryId is null) { OnSelectMainCategoryClicked(sender,e); return; }
        var options = _categories.Select((x,i)=>(Item:x,Index:i))
            .Where(x=>x.Item.MainCategoryId==_mainCategoryId && x.Item.Active)
            .Select(x=>CategoryVisualResolver.Option(x.Item,x.Index,CategoryPicker.SelectedIndex==x.Index));
        var page = new OptionSelectionPage("Subcategoria",options);
        page.Selected += (_,option)=>CategoryPicker.SelectedIndex=option.Index;
        await Navigation.PushModalAsync(page);
    }
    private async void OnSelectFrequencyClicked(object? sender, EventArgs e)
    {
        var values = new[] { "mensal", "anual" };
        var page = new OptionSelectionPage("Selecione a frequência", values.Select((value, index) => new SelectionOption
        {
            Index = index, Label = CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(value),
            IsSelected = FrequencyPicker.SelectedIndex == index, Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary")
        }));
        page.Selected += (_, option) => { FrequencyPicker.SelectedIndex = option.Index; FrequencySelectionButton.Text = option.Label; };
        await Navigation.PushModalAsync(page);
    }

    private void UpdateSaveState()=>SaveButton.IsEnabled=!_saving&&!string.IsNullOrWhiteSpace(DescriptionEntry.Text)&&FinanceInputHelper.TryParsePositiveAmount(AmountEntry.Text,out _)&&CategoryPicker.SelectedIndex>=0&&(!RecurringSwitch.IsToggled||RecurrenceCountStepper.Value is >=1 and <=36);
    private void ShowError(string message){ErrorLabel.Text=message;ErrorLabel.IsVisible=true;}
    private async void OnCancelClicked(object? sender,EventArgs e)=>await Navigation.PopModalAsync();
}
