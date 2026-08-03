namespace Money.Models;

public sealed record CardCsvPurchase(int LineNumber, DateTime CurrentDate, string Description,
    decimal InstallmentAmount, int CurrentInstallment, int TotalInstallments, DateTime FirstDate,
    DateTime FirstDueDate)
{
    public int GeneratedCount => TotalInstallments;
    public decimal SeriesTotal => InstallmentAmount * TotalInstallments;
    public string ReadInfo => TotalInstallments == 1
        ? "Compra à vista · 1 lançamento será gerado"
        : $"Parcela {CurrentInstallment}/{TotalInstallments} lida → série completa de {TotalInstallments} parcelas";
    public string DueDateInfo => $"1º vencimento: {FirstDueDate:dd/MM/yyyy}";
}

public sealed record CardInstallmentImport(string Description, decimal Amount, DateTime PurchaseDate,
    int Number, int Total, int InvoiceMonth, int InvoiceYear, DateTime InvoiceDueDate,
    string InvoiceStatus);
