namespace Money.Models;

public sealed record DashboardSummary(decimal Balance, decimal Income, decimal Expenses, decimal Savings,
    int FinancialScore, decimal NetWorth, decimal SavingsRate, decimal ReserveDays, decimal DailyAverage,
    decimal Forecast90Days, IReadOnlyList<TransactionItem> RecentTransactions, IReadOnlyList<BudgetProgress> Budgets);
public sealed record MonthlyRealizedTotals(decimal TotalReceitas, decimal TotalDespesas)
{
    public decimal ResultadoMes => TotalReceitas - TotalDespesas;
}
public enum FinancialProjectionRisk { Safe, Attention, Critical }
public sealed record FinancialProjectionEvent(long Id, DateTime Date, string Description,
    string Type, decimal Amount, bool Realized, long? AccountId, long? CardId);
public sealed record MonthlyFinancialProjection(DateTime Month, decimal OpeningBalance,
    decimal RealizedIncome, decimal ExpectedIncome, decimal PaidExpenses, decimal ExpectedExpenses,
    decimal RealizedResult, decimal ProjectedResult, decimal RealClosingBalance,
    decimal ProjectedClosingBalance, FinancialProjectionRisk Risk)
{
    public decimal TotalIncome => RealizedIncome + ExpectedIncome;
    public decimal TotalExpenses => PaidExpenses + ExpectedExpenses;
    public bool HasActivity => TotalIncome > 0 || TotalExpenses > 0;
}
public sealed record FinancialProjectionSummary(decimal CurrentBalance,
    IReadOnlyList<MonthlyFinancialProjection> Months, FinancialProjectionRisk Risk,
    DateTime? FirstRiskMonth, decimal MinimumProjectedBalance)
{
    public MonthlyFinancialProjection? FindMonth(int month, int year) =>
        Months.FirstOrDefault(item => item.Month.Month == month && item.Month.Year == year);
}
public sealed record TransactionItem(long Id, string Description, decimal Amount, DateTime Date,
    string Type, string Category, string Account, DateTime? DueDate = null,
    DateTime? PaymentDate = null, bool Paid = false, int InstallmentNumber = 1,
    int TotalInstallments = 1, bool Recurring = false, string? Frequency = null,
    long? DestinationAccountId = null, long? InvoiceId = null,
    IReadOnlyList<TagItem>? Tags = null);
public sealed record FinancialReportItem(long Id, DateTime Date, string Description, string Type,
    decimal Amount, long? CategoryId, string Category, string Source, bool Paid,
    int InstallmentNumber = 1, int TotalInstallments = 1,
    IReadOnlyList<TagItem>? Tags = null, string MainCategory = "", string Subcategory = "");
public sealed record ManagedUser(long Id, string Name, string Email, DateTime? BirthDate,
    string? RecoveryQuestion, bool Active, DateTime? LastAccess);
public sealed record BudgetProgress(string Category, string Color, decimal Limit, decimal Spent,
    double Percentage, string Status = "ok");
public sealed record CategoryItem(long Id, string Name, string Type, string Color, string? Icon,
    long? ParentId = null, int Level = 1, string? FullPath = null) : ICategoryVisual;
public sealed record AccountItem(long Id, string Name, decimal Balance, string Type,
    string Color = BlingPalette.PrimaryHex, string Icon = "AccountBalance");
public sealed record CardItem(long Id, string Name, decimal CreditLimit, decimal Used,
    int ClosingDay, int DueDay, string Color = BlingPalette.PrimaryHex);
public sealed record TransactionDraft(string Description, decimal Amount, DateTime Date, string Type,
    long CategoryId, long? AccountId, long? CardId, string? Notes,
    long? DestinationAccountId = null, DateTime? DueDate = null, bool Paid = false,
    DateTime? PaymentDate = null, bool Recurring = false, string? Frequency = null,
    IReadOnlyCollection<long>? TagIds = null, long? SupplierId = null,
    int? RecurrenceCount = null, IReadOnlyList<DateTime>? RecurrenceDueDates = null,
    IReadOnlyList<decimal>? RecurrenceAmounts = null);
public sealed record InstallmentPreview(int Number, int Total, DateTime Date, decimal Amount);
public sealed record InstallmentSaveResult(long ParentId, int Installments, decimal Total);
public sealed class ContaPagar
{
    public long Id { get; init; }
    public string Descricao { get; init; } = "";
    public string Categoria { get; init; } = "";
    public DateTime DataVencimento { get; init; }
    public decimal Valor { get; init; }
    public string Status { get; init; } = "Pendente";
    public string ContaNome { get; init; } = "";
    public long? IdFornecedor { get; init; }
    public string FornecedorNome { get; init; } = "Sem fornecedor";
    public int NumeroParcela { get; init; } = 1;
    public int TotalParcelas { get; init; } = 1;
    public string Parcela => $"{NumeroParcela}/{TotalParcelas}";
    public bool IsPaid => Status == "Paga";
    public bool IsOpen => !IsPaid;
    public string PrimaryActionText => IsPaid ? "Estornar" : "Pagar";
    public bool Selecionado { get; set; }
    public IReadOnlyList<TagItem> Tags { get; init; } = [];
}
public sealed record ResumoContasPagar(int TotalContas, decimal ValorTotal, int ContasVencidas);
public sealed record ReceitaListItem(long Id, string Descricao, string Categoria, DateTime Data,
    decimal Valor, string ContaNome, bool Paid = false, DateTime? PaymentDate = null,
    bool Recurring = false, IReadOnlyList<TagItem>? Tags = null)
{
    public string Status => Paid ? "Recebida" : Data.Date < DateTime.Today ? "Atrasada" : "Prevista";
    public string StatusActionText => Paid ? "Pendente" : "Receber";
}
public sealed record ResumoReceitas(int TotalReceitas, decimal ValorTotal, decimal Media);
public sealed record TransactionEditData(long Id, string Description, decimal Amount, DateTime Date,
    string Type, long CategoryId, long? AccountId, long? CardId, string? Notes,
    DateTime? DueDate = null, bool Paid = false, DateTime? PaymentDate = null,
    bool Recurring = false, string? Frequency = null, long? SupplierId = null,
    bool Installment = false, int InstallmentNumber = 1, int TotalInstallments = 1,
    long? ParentTransactionId = null);
public sealed record InstallmentScheduleItem(long Id, int Number, int Total, DateTime DueDate,
    bool Paid, decimal Amount);
public sealed record HomePaymentStats(int PaidThisMonth, int TotalThisMonth)
{
    public int PaidRate => TotalThisMonth == 0 ? 0 :
        (int)Math.Round(PaidThisMonth * 100d / TotalThisMonth);
}
public sealed record CardCategorySpend(string Category, string Color, decimal Amount);
public sealed record CreditCardAnalysis(long? CardId, string Name, decimal Invoice,
    decimal CreditLimit, decimal Available, int ClosingDay, int DueDay,
    IReadOnlyList<CardCategorySpend> Categories)
{
    public double Usage => CreditLimit <= 0 ? 0 :
        Math.Clamp((double)((CreditLimit - Available) / CreditLimit), 0, 1);
    public int BestPurchaseDay => ClosingDay >= 28 ? 1 : ClosingDay + 1;
}
public sealed record MonthlyOverview(decimal Income, decimal PaidExpenses, decimal PendingExpenses,
    decimal AccountExpenses, decimal CardInvoices, decimal BankBalance, decimal TotalExpenses,
    decimal Remaining, decimal Commitment, decimal AvailablePerDay, int DaysRemaining);
public sealed record MetaItem(long Id, long UserId, string Name, decimal TargetValue,
    decimal CurrentValue, DateTime StartDate, DateTime TargetDate, string? Category,
    string Priority, string Status, string? Notes);
public sealed record TagItem(long Id, long UserId, string Name, string Color, string Icon = "tag");
public sealed record Tag(long Id, long IdUsuario, string Nome, string Cor, string Icone = "tag");
public sealed record LembreteItem(long Id, long UserId, long? TransactionId, string Title,
    string? Description, DateTime ReminderDate, string Status);
public sealed record FaturaCartaoItem(long Id, long CardId, int ReferenceMonth,
    int ReferenceYear, decimal Total, DateTime DueDate, string Status, DateTime? PaymentDate,
    string CardName = "Cartão");
public sealed record MonthlyReportItem(string Period, decimal Income, decimal Expenses, decimal Balance);
public sealed record CategoryExpenseReport(string Category, string Color, decimal Total);
public sealed record ImportResult(int TotalRecords, int Imported, int Duplicates);
public sealed record UserSettings(string Currency, string DateFormat, string Theme,
    bool AlertsEnabled, int DueAlertDays, bool AutomaticBackup, string BackupFrequency);
public sealed record AuditLogItem(long Id, string Action, string? Table, long? RecordId,
    string? Details, DateTime Date, string? Origin);
public sealed record NotificationItem(long Id, string Title, string Message, string Level,
    bool Read, DateTime CreatedAt);
public sealed record FinancialHealth(decimal Income, decimal Expenses, decimal Balance,
    decimal ExpensePercentage);
public sealed record BudgetItem(long Id, long CategoryId, string Category, string Color,
    decimal Limit, int Month, int Year, decimal Spent, string Status, string? Notes);
public sealed record BudgetProjectionItem(long Id, long CategoryId, string Category, decimal Limit,
    decimal Realized, decimal Committed)
{
    public decimal Projected => Realized + Committed;
    public decimal Remaining => Limit - Projected;
    public decimal Percentage => Limit <= 0 ? 0 : Projected / Limit * 100m;
}
public sealed record ImportHistoryItem(long Id, string FileName, string Format, int Total,
    int Duplicates, DateTime ImportedAt);
public sealed record BackupHistoryItem(long Id, string FileName, long SizeKb, DateTime CreatedAt,
    string? Location, string Type);
public sealed record InvoiceTransactionItem(long Id, string Description, string Category,
    decimal Amount, DateTime Date, int InstallmentNumber, int TotalInstallments);
public sealed record AuthResult(bool Success, string Message, long? UserId = null, string? Name = null);
