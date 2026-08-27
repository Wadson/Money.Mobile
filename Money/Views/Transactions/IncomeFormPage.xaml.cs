using System.Globalization;
using MauiIcons.Material;
using Money.Helpers;
using Money.Models;
using Money.Services;
using Money.Views.Dialogs;

namespace Money.Views.Transactions;

public partial class IncomeFormPage : ContentPage
{
    private readonly DatabaseService _database; private readonly long? _id;
    private List<CategoryItem> _categories=[]; private List<AccountItem> _accounts=[];
    private bool _saving;
    public event EventHandler? Saved;
    public IncomeFormPage(DatabaseService database,long? id=null)
    {
        InitializeComponent(); _database=database; _id=id;
        ExpectedDatePicker.Date=DateTime.Today; PaymentDatePicker.Date=DateTime.Today;
        FrequencyPicker.ItemsSource=new[]{"mensal","anual"}; FrequencyPicker.SelectedIndex=0;
        DescriptionEntry.TextChanged += OnFormValueChanged;
        AmountEntry.TextChanged += OnFormValueChanged;
        CategoryPicker.SelectedIndexChanged += OnFormSelectionChanged;
        AccountPicker.SelectedIndexChanged += OnFormSelectionChanged;
        if(id is not null) TitleLabel.Text="Editar salário/recebimento";
        UpdateRecurrenceCountLabel();
        UpdateSaveState();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); if(_categories.Count>0)return;
        try
        {
            _categories=await _database.GetCategoriesAsync("receita"); _accounts=await _database.GetAccountsAsync();
            CategoryPicker.ItemsSource=_categories.Select(x=>x.Name).ToList(); AccountPicker.ItemsSource=_accounts.Select(x=>x.Name).ToList();
            if(_id is long id)
            {
                var data=await _database.GetTransactionForEditAsync(id,"receita");
                DescriptionEntry.Text=data.Description; AmountEntry.Text=data.Amount.ToString("N2",CultureInfo.GetCultureInfo("pt-BR"));
                ExpectedDatePicker.Date=data.Date; PaidSwitch.IsToggled=data.Paid; PaymentDatePicker.Date=data.PaymentDate??DateTime.Today;
                RecurringSwitch.IsToggled=data.Recurring; FrequencyPicker.SelectedItem=data.Frequency??"mensal"; NotesEditor.Text=data.Notes;
                CategoryPicker.SelectedIndex=_categories.FindIndex(x=>x.Id==data.CategoryId); AccountPicker.SelectedIndex=_accounts.FindIndex(x=>x.Id==data.AccountId);
                CategorySelectionButton.Text=CategoryPicker.SelectedIndex>=0?_categories[CategoryPicker.SelectedIndex].Name:"Selecione a categoria";
                AccountSelectionButton.Text=AccountPicker.SelectedIndex>=0?_accounts[AccountPicker.SelectedIndex].Name:"Selecione a conta";
            }
            UpdateSaveState();
        }
        catch(Exception ex){ShowError(SqliteErrorMessage.ToFriendly(ex));}
    }
    private async void OnSaveClicked(object? sender,EventArgs e)
    {
        if (_saving) return;
        if(string.IsNullOrWhiteSpace(DescriptionEntry.Text)||!FinanceInputHelper.TryParsePositiveAmount(AmountEntry.Text,out var amount)||CategoryPicker.SelectedIndex<0||AccountPicker.SelectedIndex<0){ShowError("Preencha descrição, valor, categoria e conta de destino.");UpdateSaveState();return;}
        var recurrenceCount = (int)RecurrenceCountStepper.Value;
        if (recurrenceCount is < 1 or > 36) { ShowError("A quantidade deve estar entre 1 e 36."); return; }
        try
        {
            _saving = true; UpdateSaveState();
            var date=ExpectedDatePicker.Date??DateTime.Today; var paid=PaidSwitch.IsToggled; var payment=paid?PaymentDatePicker.Date:null;
            if(_id is long id)
                await _database.UpdateListedTransactionAsync(id,"receita",DescriptionEntry.Text,amount,date,_categories[CategoryPicker.SelectedIndex].Id,NotesEditor.Text?.Trim(),null,RecurringSwitch.IsToggled,FrequencyPicker.SelectedItem?.ToString(),null,null,_accounts[AccountPicker.SelectedIndex].Id,null,paid,payment);
            else
                await _database.AddTransactionAsync(new TransactionDraft(DescriptionEntry.Text,amount,date,"receita",_categories[CategoryPicker.SelectedIndex].Id,_accounts[AccountPicker.SelectedIndex].Id,null,NotesEditor.Text?.Trim(),Paid:paid,PaymentDate:payment,Recurring:RecurringSwitch.IsToggled,Frequency:FrequencyPicker.SelectedItem?.ToString(),RecurrenceCount:RecurringSwitch.IsToggled?recurrenceCount:null));
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
    private void OnPaidToggled(object? sender,ToggledEventArgs e)=>PaymentDatePanel.IsVisible=e.Value;
    private void OnRecurringToggled(object? sender,ToggledEventArgs e){FrequencyPanel.IsVisible=e.Value;UpdateSaveState();}
    private void OnRecurrenceCountChanged(object? sender,ValueChangedEventArgs e){UpdateRecurrenceCountLabel();UpdateSaveState();}
    private void UpdateRecurrenceCountLabel(){var count=(int)RecurrenceCountStepper.Value;RecurrenceCountLabel.Text=$"{count} recebimento{(count==1?"":"s")}";}
    private void OnFormValueChanged(object? sender,TextChangedEventArgs e)=>UpdateSaveState();
    private void OnFormSelectionChanged(object? sender,EventArgs e)=>UpdateSaveState();
    private async void OnSelectCategoryClicked(object? sender,EventArgs e)
    {
        var options=_categories.Select((category,index)=>CategoryVisualResolver.Option(category,index,index==CategoryPicker.SelectedIndex)).ToList();
        var page=new OptionSelectionPage("Selecione a categoria",options,true,true);
        page.Selected+=(_,option)=>{CategoryPicker.SelectedIndex=option.Index;CategorySelectionButton.Text=option.Index>=0?option.Label:"Selecione a categoria";UpdateSaveState();};
        await Navigation.PushModalAsync(page);
    }
    private async void OnSelectAccountClicked(object? sender,EventArgs e)
    {
        var culture=CultureInfo.GetCultureInfo("pt-BR");
        var options=_accounts.Select((account,index)=>new SelectionOption{Index=index,Label=account.Name,
            Subtitle=$"Saldo atual: {account.Balance.ToString("C2",culture)}",ImageSource=account.Type.Contains("poup",StringComparison.OrdinalIgnoreCase)?MaterialIcons.Savings:MaterialIcons.AccountBalance,
            Background=ThemeColor.Get("BlingCard"),Foreground=ThemeColor.Get("BlingPrimary"),IsSelected=index==AccountPicker.SelectedIndex}).ToList();
        var page=new OptionSelectionPage("Selecione a conta",options,true,true);
        page.Selected+=(_,option)=>{AccountPicker.SelectedIndex=option.Index;AccountSelectionButton.Text=option.Index>=0?option.Label:"Selecione a conta";UpdateSaveState();};
        await Navigation.PushModalAsync(page);
    }
    private void UpdateSaveState()=>SaveButton.IsEnabled=!_saving&&!string.IsNullOrWhiteSpace(DescriptionEntry.Text)&&FinanceInputHelper.TryParsePositiveAmount(AmountEntry.Text,out _)&&CategoryPicker.SelectedIndex>=0&&AccountPicker.SelectedIndex>=0&&(!RecurringSwitch.IsToggled||RecurrenceCountStepper.Value is >=1 and <=36);
    private void ShowError(string message){ErrorLabel.Text=message;ErrorLabel.IsVisible=true;}
    private async void OnCancelClicked(object? sender,EventArgs e)=>await Navigation.PopModalAsync();
}
