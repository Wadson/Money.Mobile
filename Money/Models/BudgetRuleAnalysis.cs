namespace Money.Models;

public sealed record BudgetRuleGroup(string Name, decimal Ratio, decimal Amount, decimal Income,
    IReadOnlyList<FinancialReportItem> Transactions)
{
    public decimal Target => Income * Ratio;
    public decimal? Percentage => Income > 0 ? Amount / Income * 100 : null;
    public decimal Difference => Target - Amount;
}

public sealed record BudgetRuleAnalysis(decimal Income, IReadOnlyList<BudgetRuleGroup> Groups,
    IReadOnlyList<FinancialReportItem> Unclassified)
{
    public static BudgetRuleAnalysis Calculate(IEnumerable<FinancialReportItem> transactions,
        IReadOnlyDictionary<long, string> pillars)
    {
        var items = transactions.ToList();
        var income = items.Where(x => x.Type == "receita" && x.Paid).Sum(x => x.Amount);
        string Pillar(FinancialReportItem item) => item.CategoryId is long id && pillars.TryGetValue(id, out var value) ? value : "";
        var expenses = items.Where(x => x.Type == "despesa").ToList();
        BudgetRuleGroup Group(string name, decimal ratio, params string[] codes)
        {
            var rows = expenses.Where(x => codes.Contains(Pillar(x))).ToList();
            return new(name, ratio, rows.Sum(x => x.Amount), income, rows);
        }
        string[] valid = ["FIXAS_ESSENCIAIS", "VARIAVEIS_LAZER", "RESERVA_EMERGENCIA", "INVESTIMENTOS"];
        return new(income,
            [Group("Fixas e essenciais", .6m, valid[0]), Group("Variáveis e lazer", .3m, valid[1]),
             Group("Reserva e investimentos", .1m, valid[2], valid[3])],
            expenses.Where(x => !valid.Contains(Pillar(x))).ToList());
    }
}
