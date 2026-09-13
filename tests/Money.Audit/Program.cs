using Money.Services;
using Money.Models;
using Microsoft.Data.Sqlite;

if(args.Length==2 && args[0]=="--migrate")
{
    var target=new DatabaseService(args[1]);
    await target.InitializeAsync();
    Console.WriteLine("Migrated to schema "+DatabaseService.SupportedSchemaVersion+": "+target.DatabasePath);
    return;
}

FileSystem.AppDataDirectory = Path.Combine(Path.GetTempPath(), "MoneyAudit", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(FileSystem.AppDataDirectory);
var database = new DatabaseService { CurrentUserId = 1 };
var failed = 0;
var passed = 0;
async Task Test(string name, Func<Task> action)
{
    try { await action(); passed++; Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failed++; Console.WriteLine($"FAIL {name}: {ex.Message}"); }
}
void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"expected {expected}, actual {actual}"); }
async Task<object?> Sql(string sql)
{
    await using var db = new SqliteConnection($"Data Source={database.DatabasePath}");
    await db.OpenAsync();
    await using var cmd = db.CreateCommand(); cmd.CommandText = sql;
    return await cmd.ExecuteScalarAsync();
}
await Test("fresh database migration", database.InitializeAsync);
await Test("financial access requires explicit user", async () => {
    var anonymous = new DatabaseService(database.DatabasePath);
    Equal(0L, anonymous.CurrentUserId);
    try { await anonymous.GetCategoriesAsync(); throw new Exception("anonymous read accepted"); }
    catch (InvalidOperationException) { }
    try { await anonymous.SaveSupplierAsync(null, "Anonymous supplier", true); throw new Exception("anonymous write accepted"); }
    catch (InvalidOperationException) { }
    Equal(0L, Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Fornecedores WHERE nome_fornecedor='Anonymous supplier'")));
});
await Test("local identity must be explicitly configured and active", async () => {
    var isolated = new DatabaseService(database.DatabasePath);
    var auth = new AuthService(isolated);
    try { await auth.ActivateLocalUserAsync(0); throw new Exception("zero identity accepted"); }
    catch (ArgumentException) { }
    try { await auth.ActivateLocalUserAsync(long.MaxValue); throw new Exception("missing identity accepted"); }
    catch (InvalidOperationException) { }
    Equal(0L, isolated.CurrentUserId);
    await auth.ActivateLocalUserAsync(1);
    Equal(1L, isolated.CurrentUserId);
    auth.Logout();
    try { await isolated.GetSettingsAsync(); throw new Exception("settings available after logout"); }
    catch (InvalidOperationException) { }
});
await Test("future schema is rejected before any mutation", async () => {
    var futurePath = Path.Combine(FileSystem.AppDataDirectory, "future-schema.db");
    await using var connection = new SqliteConnection($"Data Source={futurePath};Pooling=False");
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = "CREATE TABLE FutureMarker(value TEXT); INSERT INTO FutureMarker VALUES('preserved'); PRAGMA user_version=99;";
    await command.ExecuteNonQueryAsync();
    var future = new DatabaseService(futurePath);
    try { await future.InitializeAsync(); throw new Exception("unsupported schema migrated"); }
    catch (InvalidDataException) { }
    command.CommandText = "PRAGMA user_version"; Equal(99L, Convert.ToInt64(await command.ExecuteScalarAsync()));
    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table'";
    Equal(1L, Convert.ToInt64(await command.ExecuteScalarAsync()));
    command.CommandText = "SELECT value FROM FutureMarker";
    Equal("preserved", Convert.ToString(await command.ExecuteScalarAsync()));
});
await Test("cards", async () => { await database.GetCardsAsync(); });
await Test("card analysis", async () => { await database.GetCreditCardAnalysisAsync(9, 2026); });
await Test("payables", async () => { await database.GetContasPagarAsync(null, null); });
await Test("reports", async () => { await database.GetFinancialReportItemsAsync(9, 2026, null); });
await Test("dashboard", async () => { await database.GetDashboardAsync(9, 2026); });
await Test("monthly overview", async () => { await database.GetMonthlyOverviewAsync(9, 2026); });
await Test("forecast", async () => { await new FinancialForecastService(database).CalculateAsync(new(2026,9,1), 12); });
await Test("settings", async () => { await database.GetSettingsAsync(); });
await Test("notifications", async () => { await database.GetNotificationsAsync(); });
await Test("cleanup counts", async () => { await database.GetDataCleanupCountsAsync(); });
await Test("card insert", async () => { await database.SaveCardAsync(null, "Audit Card", 10000, 5, 15); });
var card = (await database.GetCardsAsync()).Single(x => x.Name == "Audit Card");
var category = (await database.GetCategoriesAsync("despesa")).First();
await Test("supplier insert", async () => { await database.SaveSupplierAsync(null,"Audit Supplier",true); });
var supplier = (await database.GetSuppliersAsync()).Single(x => x.NomeFornecedor == "Audit Supplier");
var draft = new TransactionDraft("Audit expense",123.45m,new(2026,9,1),"despesa",category.Id,null,card.Id,null,
    DueDate:new(2026,9,15),SupplierId:supplier.IdFornecedor);
await Test("expense insert", () => database.AddTransactionAsync(draft));
var id = Convert.ToInt64(await Sql("SELECT id_transacao FROM Transacoes WHERE descricao='Audit expense'"));
await Test("expense edit load", async () => { Equal(123.45m,(await database.GetTransactionForEditAsync(id,"despesa")).Amount); });
await Test("payable filters", async () => {
    var rows = await database.GetContasPagarAsync(9,2026,false,supplier.IdFornecedor,card.Id,category.Id);
    Equal(1, rows.Count); Equal(123.45m,rows[0].Valor);
});
await Test("report filters", async () => {
    var rows = await database.GetFinancialReportItemsAsync(9,2026,category.Id,"abertas",supplier.IdFornecedor,card.Id);
    Equal(1,rows.Count); Equal(123.45m,rows[0].Amount);
});
await Test("expense update without bank account", async () => {
    await database.UpdateListedTransactionAsync(id,"despesa","Audit expense",123.45m,new(2026,9,1),category.Id,null,new(2026,9,15),supplierId:supplier.IdFornecedor,cardId:card.Id);
    Equal(123.45m,(await database.GetTransactionForEditAsync(id,"despesa")).Amount);
});
await Test("PDF generation", async () => {
    var pdf = await new ReportPdfService(database).GenerateAsync(9,2026,category.Id,category.Name,"abertas",supplier.IdFornecedor,supplier.NomeFornecedor,card.Id,card.Name);
    Equal(1,pdf.ItemCount); Equal(123.45m,pdf.PendingExpenses);
    Equal("%PDF-",System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(pdf.Path),0,5));
});
await Test("empty PDF", async () => {
    var pdf = await new ReportPdfService(database).GenerateAsync(1,2040,null,null);
    Equal(0,pdf.ItemCount);
});
await Test("payment atomic rollback", async () => {
    try { await database.MarcarMultiplasComoPagaAsync([id, long.MaxValue]); throw new Exception("invalid payment accepted"); }
    catch (InvalidOperationException) { }
    Equal(0L, Convert.ToInt64(await Sql($"SELECT pago FROM Transacoes WHERE id_transacao={id}")));
});
await Test("payment", async () => { await database.MarcarMultiplasComoPagaAsync([id]); Equal(1L,Convert.ToInt64(await Sql($"SELECT pago FROM Transacoes WHERE id_transacao={id}"))); });
await Test("payment reversal", async () => { await database.EstornarPagamentoAsync(id); Equal(0L,Convert.ToInt64(await Sql($"SELECT pago FROM Transacoes WHERE id_transacao={id}"))); });
await Test("installments", async () => { await database.SaveInstallmentTransactionAsync(draft with { Description="Audit installments",Amount=100 },3,new(2026,9,15)); });
await Test("invoices generation", async () => { await database.GenerateCardInvoicesAsync(9,2026); });
await Test("invoices payment", async () => { var invoices = await database.GetCardInvoicesAsync(9,2026); await database.PayCardInvoiceAsync(invoices.First(x=>x.CardId==card.Id).Id); });
await Test("paid expense deletion blocked", async () => {
    try { await database.DeleteListedTransactionAsync(id,"despesa"); throw new Exception("paid deletion accepted"); }
    catch (InvalidOperationException) { }
});
await Test("delete expense after reversal", async () => { await database.EstornarPagamentoAsync(id); await database.DeleteListedTransactionAsync(id,"despesa"); });
var incomeCategory = (await database.GetCategoriesAsync("receita")).First();
await Test("income insert and edit", async () => {
    await database.AddTransactionAsync(new("Audit income",1000,new(2026,9,1),"receita",incomeCategory.Id,null,null,null));
    var incomeId=Convert.ToInt64(await Sql("SELECT id_transacao FROM Transacoes WHERE descricao='Audit income'"));
    await database.UpdateListedTransactionAsync(incomeId,"receita","Audit income edited",1200,new(2026,9,1),incomeCategory.Id,null);
    Equal(1200m,(await database.GetTransactionForEditAsync(incomeId,"receita")).Amount);
    await database.SetIncomePaidStatusAsync(incomeId,true,new(2026,9,2));
});
await Test("payables CSV import", async () => {
    var result=await database.ImportPayablesAsync([new() { Description="CSV expense",Amount=25,DueDate=new(2026,10,1),CategoryId=category.Id }],0,"test.csv");
    Equal(1L,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE descricao='CSV expense'")));
});
await Test("card CSV installments", async () => {
    await database.ImportCardInvoiceAsync(card.Id,[new("CSV card",25,new(2026,9,1),1,2,10,2026,new(2026,10,15),"aberta"),new("CSV card",25,new(2026,9,1),2,2,11,2026,new(2026,11,15),"aberta")],"card.csv");
    Equal(2L,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE descricao='CSV card' AND id_transacao_pai IS NOT NULL")));
});
await Test("repair missing card color on existing schema", async () => {
    await Sql("ALTER TABLE CartoesCredito DROP COLUMN cor");
    await database.InitializeAsync(); Equal(card.Name,(await database.GetCardsAsync()).Single(x=>x.Id==card.Id).Name);
});
await Test("repeat initialization preserves data", async () => {
    var before = Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes"));
    var paidBefore=Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='receita' AND pago=1"));
    for(var i=0;i<5;i++) await database.InitializeAsync();
    Equal(before,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes")));
    Equal(paidBefore,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='receita' AND pago=1")));
    Equal(0L,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM pragma_table_info('Transacoes') WHERE name IN ('id_conta','id_conta_destino')")));
});
await Test("integrity", async () => { Equal("ok",Convert.ToString(await Sql("PRAGMA integrity_check"))); Equal<object?>(null,await Sql("PRAGMA foreign_key_check")); });
await Test("cleanup rollback and selective deletion", async () => {
    var cardsBefore=(await database.GetCardsAsync()).Count;
    await database.CleanSelectedDataAsync(["Transacoes"]);
    Equal(0L,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='despesa'")));
    Equal(cardsBefore,(await database.GetCardsAsync()).Count);
});
await Test("card alteration", async () => {
    await database.SaveCardAsync(card.Id,"Audit Card edited",15000,7,20);
    var edited=(await database.GetCardsAsync()).Single(x=>x.Id==card.Id);
    Equal("Audit Card edited",edited.Name); Equal(15000m,edited.CreditLimit);
    Equal(7,edited.ClosingDay); Equal(20,edited.DueDay);
});
await Test("card deletion respects current user", async () => {
    var owner=database.CurrentUserId;
    try {
        database.CurrentUserId=long.MaxValue;
        try { await database.DeleteCardAsync(card.Id); throw new Exception("foreign card deleted"); }
        catch(InvalidOperationException) { }
    } finally { database.CurrentUserId=owner; }
    Equal(1,(await database.GetCardsAsync()).Count(x=>x.Id==card.Id));
});
await Test("card deletion with invoices and installments", async () => {
    await database.SaveInstallmentTransactionAsync(draft with {Description="Delete card installments"},2,new(2026,10,15));
    await database.GenerateCardInvoicesAsync(10,2026);
    await database.DeleteCardAsync(card.Id);
    Equal(0L,Convert.ToInt64(await Sql($"SELECT COUNT(*) FROM Transacoes WHERE id_cartao={card.Id}")));
    Equal(0L,Convert.ToInt64(await Sql($"SELECT COUNT(*) FROM FaturasCartao WHERE id_cartao={card.Id}")));
    Equal(0,(await database.GetCardsAsync()).Count(x=>x.Id==card.Id));
    Equal<object?>(null,await Sql("PRAGMA foreign_key_check"));
});
if(args.Length > 0)
{
    FileSystem.AppDataDirectory=Path.Combine(FileSystem.AppDataDirectory,"existing-copy");
    Directory.CreateDirectory(FileSystem.AppDataDirectory);
    database=new DatabaseService { CurrentUserId = 1 };
    await using(var original=new SqliteConnection($"Data Source={Path.GetFullPath(args[0])};Mode=ReadOnly"))
    await using(var copy=new SqliteConnection($"Data Source={database.DatabasePath}"))
    { await original.OpenAsync(); await copy.OpenAsync(); original.BackupDatabase(copy); }
    await Test("existing database upgrade preserves expenses", async () => {
        var count=Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='despesa'"));
        var paidIncome=Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='receita' AND pago=1"));
        await database.InitializeAsync();
        Equal(count,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='despesa'")));
        Equal(paidIncome,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='receita' AND pago=1")));
        await database.GetCardsAsync(); await database.GetContasPagarAsync(null,null);
        await database.GetCreditCardAnalysisAsync(9,2026); await database.GetFinancialReportItemsAsync(9,2026,null);
        await database.InitializeAsync();
        Equal(count,Convert.ToInt64(await Sql("SELECT COUNT(*) FROM Transacoes WHERE tipo='despesa'")));
    });
}
await BackupAudit.RunAsync(Test, args.Length > 0 ? Path.GetFullPath(args[0]) : null);
await ForecastAudit.RunAsync(Test);
await CategoryAudit.RunAsync(Test);
await Test("60-30-10 totals percentages missing classification and transfers", () => {
    FinancialReportItem Item(long id, string type, decimal amount, long? sub, bool paid = true)
        => new(id, new(2026, 9, 1), "Analysis", type, amount, sub, "Category", "", paid);
    var items = new[] { Item(1,"receita",1000,null), Item(2,"receita",900,null,false),
        Item(3,"despesa",650,1), Item(4,"despesa",200,2,false), Item(5,"despesa",40,3),
        Item(6,"despesa",60,4), Item(7,"despesa",25,5), Item(8,"transferencia",500,3) };
    var result = BudgetRuleAnalysis.Calculate(items, new Dictionary<long,string> {
        [1]="FIXAS_ESSENCIAIS", [2]="VARIAVEIS_LAZER", [3]="RESERVA_EMERGENCIA", [4]="INVESTIMENTOS" });
    Equal(1000m,result.Income); Equal(650m,result.Groups[0].Amount);
    Equal(600m,result.Groups[0].Target); Equal<decimal?>(65m,result.Groups[0].Percentage);
    Equal(-50m,result.Groups[0].Difference); Equal(200m,result.Groups[1].Amount);
    Equal(100m,result.Groups[2].Amount); Equal(0m,result.Groups[2].Difference);
    Equal(25m,result.Unclassified.Sum(x=>x.Amount));
    return Task.CompletedTask;
});
await Test("60-30-10 zero income and empty period", () => {
    var result=BudgetRuleAnalysis.Calculate([],new Dictionary<long,string>());
    Equal(0m,result.Income); Equal(3,result.Groups.Count);
    foreach(var group in result.Groups) { Equal(0m,group.Target); Equal<decimal?>(null,group.Percentage); }
    return Task.CompletedTask;
});
await Test("60-30-10 unmapped subcategory stays unclassified and mapping is isolated", async () => {
    var parent=(await database.GetMainCategoriesAsync("despesa")).First();
    var id=await database.AddSubcategoryAsync(parent.Id,"Audit unclassified","despesa", "#00875A","Category");
    Equal("",(await database.GetBudgetPillarsAsync()).Single(x=>x.SubcategoryId==id).Pillar);
    await database.SetBudgetPillarAsync(id,"INVESTIMENTOS");
    Equal("INVESTIMENTOS",(await database.GetBudgetPillarsAsync()).Single(x=>x.SubcategoryId==id).Pillar);
    var owner=database.CurrentUserId;
    try {
        database.CurrentUserId=long.MaxValue;
        Equal(0,(await database.GetBudgetPillarsAsync()).Count);
        try { await database.SetBudgetPillarAsync(id,"VARIAVEIS_LAZER"); throw new Exception("foreign mapping accepted"); }
        catch(ArgumentException) { }
    } finally { database.CurrentUserId=owner; }
    Equal("INVESTIMENTOS",(await database.GetBudgetPillarsAsync()).Single(x=>x.SubcategoryId==id).Pillar);
    Equal("ok",Convert.ToString(await Sql("PRAGMA integrity_check")));
    Equal<object?>(null,await Sql("PRAGMA foreign_key_check"));
});
Console.WriteLine($"RESULT {passed} passed / {failed} failed; database: {database.DatabasePath}");
Environment.ExitCode = failed == 0 ? 0 : 1;
