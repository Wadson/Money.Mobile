namespace Money.Controls;

public enum GenerationMode { Installment, Recurrence }

public sealed record GenerationPlanItem(int Number, int Count, decimal Amount, DateTime DueDate)
{
    public string Label => Number + "/" + Count;
}

public static class InstallmentRecurrencePlan
{
    public static IReadOnlyList<GenerationPlanItem> Build(GenerationMode mode, decimal value, int count, DateTime firstDueDate)
    {
        if (count < 1) return [];
        var result = new List<GenerationPlanItem>(count);
        var regular = mode == GenerationMode.Recurrence ? value : Math.Round(value / count, 2, MidpointRounding.AwayFromZero);
        decimal allocated = 0;
        for (var index = 0; index < count; index++)
        {
            var itemValue = mode == GenerationMode.Installment && index == count - 1 ? value - allocated : regular;
            allocated += itemValue;
            result.Add(new(index + 1, count, itemValue, firstDueDate.Date.AddMonths(index)));
        }
        return result;
    }
}
