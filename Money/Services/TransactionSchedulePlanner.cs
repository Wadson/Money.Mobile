using Money.Models;

namespace Money.Services;

public enum TransactionScheduleMode { Installment, Recurrence }

public static class TransactionSchedulePlanner
{
    public static IReadOnlyList<InstallmentPreview> Build(
        decimal value, int count, DateTime firstDate, TransactionScheduleMode mode)
    {
        if (value <= 0) return [];
        if (count is < 1 or > 36) throw new ArgumentOutOfRangeException(nameof(count));

        var result = new List<InstallmentPreview>(count);
        if (mode == TransactionScheduleMode.Recurrence)
        {
            for (var number = 1; number <= count; number++)
                result.Add(new(number, count, firstDate.Date.AddMonths(number - 1), value));
            return result;
        }

        var regular = Math.Round(value / count, 2, MidpointRounding.AwayFromZero);
        decimal allocated = 0;
        for (var number = 1; number <= count; number++)
        {
            var amount = number == count ? value - allocated : regular;
            allocated += amount;
            result.Add(new(number, count, firstDate.Date.AddMonths(number - 1), amount));
        }
        return result;
    }
}
