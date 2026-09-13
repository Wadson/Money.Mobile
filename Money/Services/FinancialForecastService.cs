using Money.Models;

namespace Money.Services;

/// <summary>Centraliza projeções mensais independentes pela data de vencimento.</summary>
public sealed class FinancialForecastService(DatabaseService database)
{
    public async Task<FinancialProjectionSummary> CalculateAsync(DateTime firstMonth, int monthCount)
    {
        if (monthCount is < 1 or > 24) throw new ArgumentOutOfRangeException(nameof(monthCount));
        firstMonth = new DateTime(firstMonth.Year, firstMonth.Month, 1);
        var end = firstMonth.AddMonths(monthCount);
        var events = await database.GetFinancialProjectionEventsAsync(firstMonth, end);
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
            var income = realizedIncome + expectedIncome;
            var expenses = paidExpenses + expectedExpenses;
            var monthlyResult = decimal.Round(income - expenses, 2, MidpointRounding.AwayFromZero);
            var risk = Classify(monthlyResult, income);
            var realizedResult = decimal.Round(realizedIncome - paidExpenses, 2, MidpointRounding.AwayFromZero);
            result.Add(new(month,0m,realizedIncome,expectedIncome,paidExpenses,expectedExpenses,
                realizedResult,monthlyResult,realizedResult,monthlyResult,risk));
        }
        var minimum = result.Count == 0 ? 0m : result.Min(x => x.ProjectedClosingBalance);
        var firstRisk = result.FirstOrDefault(x => x.Risk != FinancialProjectionRisk.Safe)?.Month;
        return new(0m,result,Classify(minimum,0m),firstRisk,minimum);
    }

    private static FinancialProjectionRisk Classify(decimal projectedBalance, decimal currentBalance)
    {
        if (projectedBalance < 0) return FinancialProjectionRisk.Critical;
        var attentionLimit = Math.Max(500m,Math.Abs(currentBalance)*0.10m);
        return projectedBalance <= attentionLimit ? FinancialProjectionRisk.Attention : FinancialProjectionRisk.Safe;
    }
}
