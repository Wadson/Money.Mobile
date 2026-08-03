namespace Money.Models;

public sealed class PayableImportRow
{
    public int LineNumber { get; init; }
    public string Description { get; init; } = "";
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public string Category { get; init; } = "";
    public long? CategoryId { get; init; }
    public string Supplier { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Error { get; init; } = "";
    public bool IsValid => string.IsNullOrEmpty(Error);
    public string Status => IsValid ? "Válida" : Error;
    public Color StatusColor => IsValid ? ThemeColor.Get("BlingPrimary") : Color.FromArgb("#D97706");
    public string AmountText => Amount.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
}
