using System.Globalization;
using Money.Helpers;
using Money.Models;
using Money.Services;
namespace Money.Views.Management;

public sealed class CategoryPlanningPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly MonthPeriodButton _period=new();
    private readonly Label _summary=new(){TextColor=ThemeColor.Get("BlingText")};
    private readonly CollectionView _list=new(){SelectionMode=SelectionMode.None};
    private static readonly string[] Pillars=["FIXAS_ESSENCIAIS","VARIAVEIS_LAZER","RESERVA_EMERGENCIA","INVESTIMENTOS"];
    private static readonly string[] Labels=["Fixas e essenciais (60%)","Variáveis e lazer (30%)","Reserva de emergência (parte dos 10%)","Investimentos (parte dos 10%)"];
    public CategoryPlanningPage(DatabaseService db, DateTime? period = null)
    {
        _db=db;_period.Date=period??DateTime.Today;Title="Orçamento e regra 60-30-10";BackgroundColor=ThemeColor.Get("BlingBackground");
        var back=new Button{Text="Voltar",MinimumHeightRequest=48};back.Clicked+=async(_,_)=>await Navigation.PopModalAsync();
        var header=new VerticalStackLayout{Spacing=8,Children={back,new Label{Text=Title,FontSize=22,TextColor=ThemeColor.Get("BlingText")},_period,_summary}};
        _list.ItemTemplate=new DataTemplate(()=>{
            var title=new Label{FontAttributes=FontAttributes.Bold,TextColor=ThemeColor.Get("BlingText")};title.SetBinding(Label.TextProperty,nameof(Row.Title));
            var detail=new Label{TextColor=ThemeColor.Get("BlingText")};detail.SetBinding(Label.TextProperty,nameof(Row.Detail));
            var edit=new Button{Text="Editar pilar e limite",MinimumHeightRequest=48};edit.Clicked+=async(s,_)=>await EditAsync((Row)((Button)s!).BindingContext);
            return new Border{Padding=12,Margin=4,Content=new VerticalStackLayout{Spacing=8,Children={title,detail,edit}}};
        });
        var grid=new Grid{Padding=16,MaximumWidthRequest=760,RowDefinitions={new(){Height=GridLength.Auto},new(){Height=GridLength.Star}}};grid.Add(header);grid.Add(_list,0,1);Content=grid;
        _period.DateSelected+=async(_,_)=>await LoadAsync();
    }
    protected override async void OnAppearing(){base.OnAppearing();await LoadAsync();}
    private async Task LoadAsync()
    {
        try{
            var date=_period.Date;
            var budgets=await _db.GetBudgetsAsync(date.Month,date.Year);
            var pillars=await _db.GetBudgetPillarsAsync();
            var transactions=await _db.GetFinancialReportItemsAsync(date.Month,date.Year);
            var analysis=BudgetRuleAnalysis.Calculate(transactions,pillars.ToDictionary(p=>p.SubcategoryId,p=>p.Pillar));
            _summary.Text=$"Receitas realizadas: {analysis.Income:C2}\n" + string.Join("\n",analysis.Groups.Select(g=>$"{g.Name}: {g.Amount:C2} / meta {g.Target:C2}")) + $"\nSem classificação: {analysis.Unclassified.Sum(x=>x.Amount):C2}\nDespesas por vencimento, incluindo abertas. Limites são definidos por subcategoria.";
            _list.ItemsSource=pillars.Select(p=>new Row(p.SubcategoryId,$"{p.MainCategory} / {p.Subcategory}",p.Pillar,budgets.SingleOrDefault(b=>b.CategoryId==p.SubcategoryId))).ToList();
        }catch(Exception ex){await ThemedDialog.ShowAsync(this,Title,ex.Message);}
    }
    private async Task EditAsync(Row row)
    {
        var page=new ContentPage{Title=row.Title,BackgroundColor=ThemeColor.Get("BlingBackground")};
        var selectedIndex=Array.IndexOf(Pillars,row.Pillar);
        var picker=new Button{Text=selectedIndex>=0?Labels[selectedIndex]:"Selecione o pilar orçamentário",MinimumHeightRequest=48,BackgroundColor=ThemeColor.Get("BlingCard"),TextColor=ThemeColor.Get("BlingText"),BorderColor=ThemeColor.Get("BlingPrimary"),BorderWidth=1};
        picker.Clicked+=async(_,_)=>{
            var options=new OptionSelectionPage("Pilar orçamentário",Labels.Select((label,index)=>new SelectionOption{Index=index,Label=label,IsSelected=index==selectedIndex,ImageSource=MauiIcons.Material.MaterialIcons.PieChart,Foreground=ThemeColor.Get("BlingPrimary")}));
            options.Selected+=(_,option)=>{selectedIndex=option.Index;picker.Text=Labels[selectedIndex];};
            await page.Navigation.PushModalAsync(options);
        };
        var amount=new Entry{Placeholder="Limite mensal (vazio para remover)",Keyboard=Keyboard.Numeric,TextColor=ThemeColor.Get("BlingText"),PlaceholderColor=ThemeColor.Get("BlingTextMuted"),BackgroundColor=ThemeColor.Get("BlingCard"),Text=row.Budget?.Limit.ToString("0.00",CultureInfo.GetCultureInfo("pt-BR"))};
        var save=new Button{Text="Salvar",MinimumHeightRequest=48};var cancel=new Button{Text="Cancelar",MinimumHeightRequest=48};
        cancel.Clicked+=async(_,_)=>await Navigation.PopModalAsync();
        save.Clicked+=async(_,_)=>{save.IsEnabled=false;try{
            if(selectedIndex < 0) throw new ArgumentException("Selecione um pilar orçamentário.");
            decimal? limit=null;
            if(!string.IsNullOrWhiteSpace(amount.Text)){if(!decimal.TryParse(amount.Text,NumberStyles.Number,CultureInfo.GetCultureInfo("pt-BR"),out var value)||value<=0)throw new ArgumentException("Informe um limite positivo.");limit=value;}
            var date=_period.Date;
            if(limit is not null)await _db.SaveBudgetAsync(row.Budget?.Id,row.Id,limit.Value,date.Month,date.Year,row.Budget?.Notes);
            else if(row.Budget is not null)await _db.DeleteBudgetAsync(row.Budget.Id);
            await _db.SetBudgetPillarAsync(row.Id,Pillars[selectedIndex]);await Navigation.PopModalAsync();await LoadAsync();
        }catch(Exception ex){await ThemedDialog.ShowAsync(page,Title,ex.Message);}finally{save.IsEnabled=true;}};
        page.Content=new ScrollView{Content=new VerticalStackLayout{Padding=24,Spacing=12,MaximumWidthRequest=600,Children={new Label{Text=row.Title,FontSize=22,TextColor=ThemeColor.Get("BlingText")},new Label{Text="Pilar orçamentário",TextColor=ThemeColor.Get("BlingText")},picker,amount,save,cancel}}};await Navigation.PushModalAsync(page);
    }
    private sealed record Row(long Id,string Title,string Pillar,BudgetItem? Budget)
    {public string Detail=>$"{(Array.IndexOf(Pillars,Pillar) is var index && index >= 0 ? Labels[index] : "Sem classificação")}\n"+(Budget is null?"Sem limite mensal":$"Limite: {Budget.Limit:C2} · Pago: {Budget.Spent:C2}");}
}
