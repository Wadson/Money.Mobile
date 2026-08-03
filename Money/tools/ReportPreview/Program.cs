using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
var output = args.Length > 0 ? args[0] : "relatorio-financeiro-amostra.pdf";
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
var rows = new[]
{
    new Row("02/08/2026", "Salário mensal", "Conta corrente", "Receita", 6850m, "Salário"),
    new Row("03/08/2026", "Supermercado", "Cartão principal", "Despesa", 486.72m, "Alimentação"),
    new Row("05/08/2026", "Restaurante", "Cartão principal", "Despesa", 94.50m, "Alimentação"),
    new Row("07/08/2026", "Conta de energia", "Conta corrente", "Despesa", 218.36m, "Moradia"),
    new Row("10/08/2026", "Aluguel", "Conta corrente", "Despesa", 1650m, "Moradia"),
    new Row("12/08/2026", "Combustível", "Cartão principal", "Despesa", 250m, "Transporte"),
    new Row("18/08/2026", "Trabalho freelance", "Conta corrente", "Receita", 1200m, "Renda extra")
};
var income = rows.Where(x => x.Type == "Receita").Sum(x => x.Amount);
var expenses = rows.Where(x => x.Type == "Despesa").Sum(x => x.Amount);

Document.Create(document => document.Page(page =>
{
    page.Size(PageSizes.A4);
    page.Margin(30);
    page.DefaultTextStyle(x => x.FontSize(9).FontColor("#1E293B"));
    page.Header().Background("#0B281E").Padding(18).Row(row =>
    {
        row.RelativeItem().Column(c =>
        {
            c.Item().Text("MONEY PRO").FontSize(18).Bold().FontColor("#FFFFFF");
            c.Item().Text("Relatório de lançamentos financeiros").FontSize(10).FontColor("#FFFFFF");
        });
        row.RelativeItem().AlignRight().Column(c =>
        {
            c.Item().AlignRight().Text("Agosto de 2026").FontSize(12).SemiBold().FontColor("#FFFFFF");
            c.Item().AlignRight().Text("Todas as categorias").FontSize(9).FontColor("#FFFFFF");
            c.Item().AlignRight().Text("Emitido em 01/08/2026 10:00").FontSize(8).FontColor("#FFFFFF");
        });
    });
    page.Content().PaddingVertical(16).Column(column =>
    {
        column.Spacing(14);
        column.Item().Row(row =>
        {
            row.Spacing(8);
            row.RelativeItem().Element(c => Card(c, "RECEITAS", income, "#FFFFFF", "#00A859"));
            row.RelativeItem().Element(c => Card(c, "DESPESAS", expenses, "#FFFFFF", "#0B281E"));
            row.RelativeItem().Element(c => Card(c, "SALDO", income - expenses, "#FFFFFF", "#0B281E"));
        });
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(c => { c.ConstantColumn(66); c.RelativeColumn(2.4f); c.RelativeColumn(1.35f); c.RelativeColumn(1.1f); c.ConstantColumn(72); });
            table.Header(h =>
            {
                Head(h.Cell()).Text("DATA"); Head(h.Cell()).Text("DESCRIÇÃO"); Head(h.Cell()).Text("ORIGEM");
                Head(h.Cell()).Text("TIPO"); Head(h.Cell()).AlignRight().Text("VALOR");
            });
            foreach (var group in rows.GroupBy(x => x.Category))
            {
                table.Cell().ColumnSpan(5).Background("#FFFFFF").BorderBottom(1).BorderColor("#00A859").Padding(7)
                    .Text($"{group.Key} - {group.Count()} lançamento(s)").SemiBold().FontColor("#0B281E");
                foreach (var item in group)
                {
                    Body(table.Cell()).Text(item.Date); Body(table.Cell()).Text(item.Description); Body(table.Cell()).Text(item.Source);
                    Body(table.Cell()).Text(item.Type).FontColor(item.Type == "Receita" ? "#00A859" : "#0B281E");
                    Body(table.Cell()).AlignRight().Text($"R$ {item.Amount:N2}").SemiBold().FontColor(item.Type == "Receita" ? "#00A859" : "#0B281E");
                }
                var categoryBalance = group.Sum(x => x.Type == "Receita" ? x.Amount : -x.Amount);
                table.Cell().ColumnSpan(4).Background("#FFFFFF").Padding(6).AlignRight().Text("Saldo da categoria").SemiBold().FontColor("#1E293B");
                table.Cell().Background("#FFFFFF").Padding(6).AlignRight().Text($"R$ {categoryBalance:N2}").Bold().FontColor("#0B281E");
            }
        });
    });
    page.Footer().AlignCenter().Text("Money Pro - página 1 de 1").FontColor("#1E293B");
})).GeneratePdf(output);
Console.WriteLine(Path.GetFullPath(output));

static void Card(IContainer c, string title, decimal value, string background, string color) =>
    c.Background(background).Border(1).BorderColor(color).Padding(12).Column(x =>
    { x.Item().Text(title).FontSize(8).SemiBold().FontColor(color); x.Item().Text($"R$ {value:N2}").FontSize(14).Bold().FontColor(color); });
static IContainer Head(IContainer c) => c.Background("#0B281E").PaddingVertical(7).PaddingHorizontal(5)
    .DefaultTextStyle(x => x.FontSize(8).SemiBold().FontColor("#FFFFFF"));
static IContainer Body(IContainer c) => c.BorderBottom(1).BorderColor("#00A859").PaddingVertical(6).PaddingHorizontal(5);
record Row(string Date, string Description, string Source, string Type, decimal Amount, string Category);
