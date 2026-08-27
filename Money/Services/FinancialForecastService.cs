using Money.Models;

namespace Money.Services;

/// <summary>Centraliza realizado, comprometido e projeção acumulada sem alterar saldos reais.</summary>
public sealed class FinancialForecastService(DatabaseService database)
{
    public async Task<FinancialProjectionSummary> CalculateAsync(DateTime firstMonth, int monthCount)
    {
        if (monthCount is < 1 or > 24) throw new ArgumentOutOfRangeException(nameof(monthCount));
        firstMonth = new DateTime(firstMonth.Year, firstMonth.Month, 1);
        var end = firstMonth.AddMonths(monthCount);
        var today = DateTime.Today;
        var currentBalance = await database.GetTotalActiveAccountsBalanceAsync();
        var events = await database.GetFinancialProjectionEventsAsync(firstMonth, end);

        // O saldo real atual já contém todos os recebimentos/pagamentos realizados.
        // A projeção parte desse saldo e aplica somente eventos ainda pendentes,
        // evitando contabilizar novamente movimentações já efetivadas.
        var projectedBalance = currentBalance;
        var result = new List<MonthlyFinancialProjection>(monthCount);
        for (var index = 0; index < monthCount; index++)
        {
            var month = firstMonth.AddMonths(index);
            var monthEnd = month.AddMonths(1);
            var monthly = events.Where(item => item.Date >= month && item.Date < monthEnd).ToList();
            var realizedIncome = monthly.Where(x => x.Type == "receita" && x.Realized).Sum(x => x.Amount);
            var expectedIncome = monthly.Where(x => x.Type == "receita" && !x.Realized).Sum(x => x.Amount);
            var paidExpenses = monthly.Where(x => x.Type == "despesa" && x.Realized).Sum(x => x.Amount);
            var expectedExpenses = monthly.Where(x => x.Type == "despesa" && !x.Realized).Sum(x => x.Amount);
            var opening = projectedBalance;
            foreach (var item in monthly.Where(x => !x.Realized && x.Date.Date >= today).OrderBy(x => x.Date).ThenBy(x => x.Id))
                projectedBalance += item.Type == "receita" ? item.Amount : -item.Amount;
            var risk = Classify(projectedBalance, currentBalance);
            result.Add(new(month,opening,realizedIncome,expectedIncome,paidExpenses,expectedExpenses,
                realizedIncome-paidExpenses,expectedIncome-expectedExpenses,
                realizedIncome-paidExpenses,projectedBalance,risk));
        }
        var minimum = result.Count == 0 ? 0m : result.Min(x => x.ProjectedClosingBalance);
        var firstRisk = result.FirstOrDefault(x => x.Risk != FinancialProjectionRisk.Safe)?.Month;
        return new(currentBalance,result,Classify(minimum,0m),firstRisk,minimum);
    }

    private static FinancialProjectionRisk Classify(decimal projectedBalance, decimal currentBalance)
    {
        if (projectedBalance < 0) return FinancialProjectionRisk.Critical;
        var attentionLimit = Math.Max(500m,Math.Abs(currentBalance)*0.10m);
        return projectedBalance <= attentionLimit ? FinancialProjectionRisk.Attention : FinancialProjectionRisk.Safe;
    }
}
