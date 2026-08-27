namespace Money.Controls;

public enum TransactionGenerationMode { Installment, Recurrence }

public sealed record TransactionGenerationItem(int Number, int Total, DateTime DueDate, decimal Amount);

/// <summary>Shared calculation model used by both transaction generation panels.</summary>
public sealed class TransactionGenerationPlan
{
    public IReadOnlyList<TransactionGenerationItem> Build(TransactionGenerationMode mode,
        decimal amount, int count, DateTime firstDueDate, IReadOnlyList<DateTime>? editedDueDates = null)
    {
        if (count < 1) return [];
        var result = new List<TransactionGenerationItem>(count);
        var regular = mode == TransactionGenerationMode.Installment
            ? Math.Round(amount / count, 2, MidpointRounding.AwayFromZero) : amount;
        decimal allocated = 0;
        for (var index = 0; index < count; index++)
        {
            var value = mode == TransactionGenerationMode.Installment && index == count - 1
                ? amount - allocated : regular;
            allocated += value;
            var dueDate = editedDueDates is { Count: > 0 } && index < editedDueDates.Count
                ? editedDueDates[index].Date : firstDueDate.Date.AddMonths(index);
            result.Add(new(index + 1, count, dueDate, value));
        }
        return result;
    }
}
