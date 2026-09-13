using System.Globalization;
using Money.Services;

namespace Money.Views.ImportsAndReports;

public sealed class BudgetRuleAnalysisPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly MonthPeriodButton _period = new();
    private readonly VerticalStackLayout _results = new() { Spacing = 14 };
    private int _loadVersion;
    private static readonly CultureInfo Brazilian = CultureInfo.GetCultureInfo("pt-BR");
    private static string MoneyText(decimal amount) => amount.ToString("C2", Brazilian);

    public BudgetRuleAnalysisPage(DatabaseService database)
    {
        _database = database;
        Title = "Análise 60-30-10";
        BackgroundColor = ThemeColor.Get("BlingBackground");
        var back = new Button { Text = "‹  Voltar", MinimumHeightRequest = 44, BackgroundColor = ThemeColor.Get("BlingCard"),
            TextColor = ThemeColor.Get("BlingPrimary"), BorderWidth = 0, HorizontalOptions = LayoutOptions.Start };
        back.Clicked += async (_, _) => await Navigation.PopModalAsync();
        _period.HorizontalOptions = LayoutOptions.Fill;
        _period.FontAttributes = FontAttributes.Bold;
        var configure = new Button { Text = "Configurar pilares e orçamento", MinimumHeightRequest = 48,
            BackgroundColor = ThemeColor.Get("BlingPrimary"), TextColor = ThemeColor.Get("BlingTextLight"), CornerRadius = 12 };
        configure.Clicked += async (_, _) => await Navigation.PushModalAsync(new CategoryPlanningPage(database, _period.Date));
        var hero = new Border { Padding = 18, StrokeThickness = 0, BackgroundColor = ThemeColor.Get("BlingPrimary"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 }, Content = new VerticalStackLayout { Spacing = 4, Children =
            {
                new Label { Text = "ANÁLISE DE ORÇAMENTO", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = ThemeColor.Get("BlingTextLight") },
                new Label { Text = Title, FontSize = 27, FontAttributes = FontAttributes.Bold, TextColor = ThemeColor.Get("BlingTextLight") },
                new Label { Text = "Veja para onde o seu dinheiro está indo.", FontSize = 13, TextColor = ThemeColor.Get("BlingTextLight") }
            }} };
        Content = new ScrollView { Content = new VerticalStackLayout {
            Padding = 16, Spacing = 14, MaximumWidthRequest = 760,
            Children = { back, hero, Text("Período analisado", 12), _period,
                Text("Receitas recebidas no pagamento e despesas pelo vencimento. Transferências não entram no cálculo.", 12),
                configure, _results }
        }};
        _period.DateSelected += async (_, _) => await LoadAsync();
    }

    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }

    private static Label Text(string value, double size = 14) => new() {
        Text = value, FontSize = size, TextColor = ThemeColor.Get("BlingText")
    };

    private async Task LoadAsync()
    {
        var version = ++_loadVersion;
        _results.Children.Clear();
        _results.Children.Add(Text("Carregando análise…"));
        try
        {
            var date = _period.Date;
            var transactions = await _database.GetFinancialReportItemsAsync(date.Month, date.Year);
            var pillars = await _database.GetBudgetPillarsAsync();
            var analysis = BudgetRuleAnalysis.Calculate(transactions, pillars.ToDictionary(x => x.SubcategoryId, x => x.Pillar));
            if (version != _loadVersion) return;
            _results.Children.Clear();
            _results.Children.Add(SummaryCard(analysis));
            if (transactions.Count == 0) _results.Children.Add(Text("Nenhum lançamento neste mês."));
            if (analysis.Income <= 0) _results.Children.Add(Notice("Sem receitas positivas no mês: percentuais e metas ficam indisponíveis."));
            foreach (var group in analysis.Groups)
            {
                var detail = GroupCard(group, analysis.Income);
                AddDetails(detail, group.Transactions);
                _results.Children.Add(new Border { Padding = 16, Stroke = ThemeColor.Get("BlingBorder"), StrokeThickness = 1,
                    BackgroundColor = ThemeColor.Get("BlingCard"), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }, Content = detail });
            }
            var missing = new VerticalStackLayout { Spacing = 8, Children = {
                Text("Sem classificação", 18), Text($"{MoneyText(analysis.Unclassified.Sum(x => x.Amount))} sem pilar orçamentário", 13) }};
            AddDetails(missing, analysis.Unclassified);
            _results.Children.Add(new Border { Padding = 16, Stroke = ThemeColor.Get("BlingBorder"), StrokeThickness = 1,
                BackgroundColor = ThemeColor.Get("BlingCard"), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }, Content = missing });
        }
        catch (Exception ex)
        {
            if (version != _loadVersion) return;
            _results.Children.Clear();
            _results.Children.Add(Text("Não foi possível carregar a análise. Selecione o mês novamente para tentar."));
            await ThemedDialog.ShowAsync(this, Title, SqliteErrorMessage.ToFriendly(ex));
        }
    }

    private static Border SummaryCard(BudgetRuleAnalysis analysis) => new()
    {
        Padding = 16, StrokeThickness = 0, BackgroundColor = ThemeColor.Get("BlingCard"),
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }, Content = new Grid
        {
            ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], Children =
            {
                new VerticalStackLayout { Spacing = 3, Children = { Text("RECEITAS RECEBIDAS", 11), Text(MoneyText(analysis.Income), 25) } },
                AtColumn(new Label { Text = "60 · 30 · 10", FontSize = 15, FontAttributes = FontAttributes.Bold,
                    TextColor = ThemeColor.Get("BlingPrimary"), VerticalTextAlignment = TextAlignment.Center }, 1)
            }
        }
    };

    private static VerticalStackLayout GroupCard(BudgetRuleGroup group, decimal income)
    {
        var percentage = group.Percentage is decimal percent ? percent.ToString("0.0", Brazilian) + "% da receita" : "—";
        var status = income <= 0 ? "A comparação com a meta requer receitas positivas."
            : group.Ratio == .1m
                ? (group.Difference > 0 ? $"Faltam {MoneyText(group.Difference)} para a meta." : "Meta atingida.")
                : (group.Difference < 0 ? $"Acima da meta em {MoneyText(-group.Difference)}." : $"Dentro da meta: sobram {MoneyText(group.Difference)}.");
        return new VerticalStackLayout { Spacing = 9, Children =
        {
            new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], Children =
            {
                new VerticalStackLayout { Spacing = 1, Children = { Text($"{group.Ratio * 100:0}% · {group.Name}", 19), Text($"Meta mensal: {MoneyText(group.Target)}", 12) } },
                AtColumn(new Label { Text = MoneyText(group.Amount), FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = ThemeColor.Get("BlingPrimary"), VerticalTextAlignment = TextAlignment.Center }, 1)
            }},
            new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], Children =
            {
                Text($"Uso atual: {percentage}", 12), AtColumn(new Label { Text = status, FontSize = 11, TextColor = ThemeColor.Get("BlingText"), HorizontalTextAlignment = TextAlignment.End }, 1)
            }},
            new ProgressBar { BackgroundColor = ThemeColor.Get("CategoryProgressTrack"), Progress = group.Target > 0 ? (double)Math.Clamp(group.Amount / group.Target, 0m, 1m) : 0, ProgressColor = ThemeColor.Get("BlingPrimary") },
            Text("Lançamentos por categoria", 11)
        }};
    }

    private static Border Notice(string message) => new() { Padding = 12, BackgroundColor = ThemeColor.Get("BlingCard"), Stroke = ThemeColor.Get("BlingBorder"), StrokeThickness = 1,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }, Content = Text(message, 12) };

    private static T AtColumn<T>(T view, int column) where T : View { Grid.SetColumn(view, column); return view; }

    private static void AddDetails(VerticalStackLayout target, IReadOnlyList<FinancialReportItem> items)
    {
        if (items.Count == 0) target.Children.Add(Text("Nenhuma despesa."));
        foreach (var category in items.GroupBy(x => new { x.CategoryId, x.MainCategory, x.Subcategory }).OrderBy(x => x.Key.MainCategory).ThenBy(x => x.Key.Subcategory))
            target.Children.Add(Text($"{category.Key.MainCategory} / {category.Key.Subcategory}\n{MoneyText(category.Sum(x => x.Amount))} · em aberto: {MoneyText(category.Where(x => !x.Paid).Sum(x => x.Amount))}"));
    }
}
