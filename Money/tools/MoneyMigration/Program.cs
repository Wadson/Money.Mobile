using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System.Globalization;

const string sourceConnection =
    @"Data Source=NOTEBOOK-DELL\SQLEXPRESS;Initial Catalog=bdMoney;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

if (args.Length == 2 && args[0] == "--inspect-categories")
{
    var inspectPath = Path.GetFullPath(args[1]);
    await using var inspect = new SqliteConnection($"Data Source={inspectPath};Mode=ReadOnly;Pooling=False");
    await inspect.OpenAsync();
    await using (var schema = inspect.CreateCommand())
    {
        schema.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='Categorias'";
        Console.WriteLine(await schema.ExecuteScalarAsync());
    }
    await using (var rows = inspect.CreateCommand())
    {
        rows.CommandText = "SELECT id_categoria,nome_categoria,icone,id_usuario,tipo,id_categoria_pai,ativo FROM Categorias ORDER BY id_categoria";
        await using var reader = await rows.ExecuteReaderAsync();
        var count = 0;
        while (await reader.ReadAsync())
        {
            count++;
            Console.WriteLine($"{reader.GetInt64(0)}|{reader.GetString(1)}|{(reader.IsDBNull(2) ? "" : reader.GetString(2))}|user={reader.GetInt64(3)}|{reader.GetString(4)}|parent={(reader.IsDBNull(5) ? "root" : reader.GetInt64(5))}|active={reader.GetInt64(6)}");
        }
        Console.WriteLine($"TOTAL={count}");
    }
    return 0;
}

if (args.Length == 3 && args[0] == "--update-category-icons")
{
    var databasePath = Path.GetFullPath(args[1]);
    var scriptPath = Path.GetFullPath(args[2]);
    await using var database = new SqliteConnection($"Data Source={databasePath};Mode=ReadWrite;Pooling=False");
    await database.OpenAsync();
    await using var iconTx = (SqliteTransaction)await database.BeginTransactionAsync();
    await using var update = database.CreateCommand();
    update.Transaction = iconTx;
    update.CommandText = await File.ReadAllTextAsync(scriptPath);
    await update.ExecuteNonQueryAsync();
    await iconTx.CommitAsync();

    await using var verify = database.CreateCommand();
    verify.CommandText = "SELECT COUNT(*), SUM(CASE WHEN icone LIKE '%.svg' THEN 1 ELSE 0 END), COUNT(DISTINCT icone) FROM Categorias";
    await using var result = await verify.ExecuteReaderAsync();
    await result.ReadAsync();
    Console.WriteLine($"TOTAL={result.GetInt64(0)}; SVG={result.GetInt64(1)}; ICONES={result.GetInt64(2)}");
    return 0;
}

if (args.Length != 1)
{
    Console.Error.WriteLine("Uso: MoneyMigration <caminho-moneypro.db>");
    return 2;
}

var targetPath = Path.GetFullPath(args[0]);
if (!File.Exists(targetPath))
    throw new FileNotFoundException("O banco SQLite de destino não foi encontrado.", targetPath);

await using var source = new SqlConnection(sourceConnection);
await source.OpenAsync();
await using var target = new SqliteConnection(new SqliteConnectionStringBuilder
{
    DataSource = targetPath,
    Mode = SqliteOpenMode.ReadWrite,
    ForeignKeys = true,
    Pooling = false
}.ToString());
await target.OpenAsync();

var sourceCategories = await ReadSourceAsync(source,
    "SELECT CategoriaID,NomeCategoria FROM Categorias ORDER BY CategoriaID",
    r => new SourceCategory(r.GetInt32(0), r.GetString(1)));
var sourceIncomeTypes = await ReadSourceAsync(source,
    "SELECT TipoReceitaID,NomeTipoReceita FROM TiposReceita ORDER BY TipoReceitaID",
    r => new SourceCategory(r.GetInt32(0), r.GetString(1)));
var sourceCards = await ReadSourceAsync(source, """
    SELECT CartaoID,NomeCartao,Limite,Fechamento,Vencimento,Bandeira
    FROM CartoesCredito ORDER BY CartaoID
    """, r => new SourceCard(r.GetInt32(0), r.GetString(1), r.GetDecimal(2),
        r.GetInt32(3), r.GetInt32(4), r.GetString(5)));
var sourceExpenses = await ReadSourceAsync(source, """
    SELECT d.DespesaID,d.Descricao,d.ValorDaCompra,d.DataDaCompra,d.CategoriaID,
      d.MetodoPgtoID,m.NomeMetodoPagamento,d.FornecedorID,f.Nome,d.NumeroParcelas,
      d.CartaoID,d.ValorPago,d.StatusDespesa
    FROM Despesas d
    LEFT JOIN MetodosPagamento m ON m.MetodoPgtoID=d.MetodoPgtoID
    LEFT JOIN Fornecedores f ON f.FornecedorID=d.FornecedorID
    ORDER BY d.DespesaID
    """, r => new SourceExpense(
        r.GetInt32(0), r.GetString(1), r.GetDecimal(2), r.GetDateTime(3),
        r.IsDBNull(4) ? null : r.GetInt32(4),
        r.IsDBNull(6) ? null : r.GetString(6),
        r.IsDBNull(8) ? null : r.GetString(8),
        r.GetInt32(9), r.IsDBNull(10) ? null : r.GetInt32(10),
        r.GetDecimal(11), r.GetInt32(12)));
var sourceInstallments = await ReadSourceAsync(source, """
    SELECT ParcelaID,DespesaID,NumeroParcela,ValorParcela,DataVencimento,DataPgto,Pago
    FROM Parcelas ORDER BY DespesaID,NumeroParcela,ParcelaID
    """, r => new SourceInstallment(
        r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetDecimal(3),
        r.GetDateTime(4), r.IsDBNull(5) ? null : r.GetDateTime(5), r.GetBoolean(6)));
var sourceIncomes = await ReadSourceAsync(source, """
    SELECT ReceitaID,Descricao,ValorDaReceita,DataRecebimento,TipoReceitaID,DataCadastro
    FROM Receitas ORDER BY ReceitaID
    """, r => new SourceIncome(r.GetInt32(0), r.GetString(1), r.GetDecimal(2),
        r.GetDateTime(3), r.IsDBNull(4) ? null : r.GetInt32(4), r.GetDateTime(5)));

await using var transaction = await target.BeginTransactionAsync();
try
{
    var userId = Convert.ToInt64(await ScalarAsync(target, transaction,
        "SELECT id_usuario FROM Usuarios WHERE ativo=1 ORDER BY id_usuario LIMIT 1")
        ?? throw new InvalidOperationException("Nenhum usuário ativo existe no SQLite."));
    var accountId = Convert.ToInt64(await ScalarAsync(target, transaction, """
        SELECT id_conta FROM Contas WHERE id_usuario=@user AND ativo=1
        ORDER BY CASE WHEN lower(nome_conta)='conta principal' THEN 0 ELSE 1 END,id_conta LIMIT 1
        """, ("@user", userId)) ?? throw new InvalidOperationException("Nenhuma conta ativa existe no SQLite."));

    foreach (var sql in new[]
    {
        "DELETE FROM TransacoesTags WHERE id_transacao IN(SELECT id_transacao FROM Transacoes WHERE id_usuario=@user)",
        "DELETE FROM Lembretes WHERE id_usuario=@user",
        "DELETE FROM FaturasCartao WHERE id_cartao IN(SELECT id_cartao FROM CartoesCredito WHERE id_usuario=@user)",
        "DELETE FROM Transacoes WHERE id_usuario=@user",
        "DELETE FROM Receitas WHERE id_usuario=@user",
        "DELETE FROM Orcamentos WHERE id_usuario=@user",
        "DELETE FROM CartoesCredito WHERE id_usuario=@user",
        "DELETE FROM Categorias WHERE id_usuario=@user",
        "DELETE FROM Logs WHERE id_usuario=@user"
    })
        await ExecuteAsync(target, transaction, sql, ("@user", userId));

    var expenseCategoryMap = new Dictionary<int, long>();
    foreach (var category in sourceCategories)
    {
        var id = Convert.ToInt64(await ScalarAsync(target, transaction, """
            INSERT INTO Categorias(id_usuario,nome_categoria,tipo,cor,icone,ativo)
            VALUES(@user,@name,'despesa',@color,@icon,1);
            SELECT last_insert_rowid();
            """, ("@user", userId), ("@name", category.Name),
            ("@color", CategoryColor(category.Name)), ("@icon", CategoryIcon(category.Name)))!);
        expenseCategoryMap[category.Id] = id;
    }

    var fallbackExpenseCategory = Convert.ToInt64(await ScalarAsync(target, transaction, """
        INSERT INTO Categorias(id_usuario,nome_categoria,tipo,cor,icone,ativo)
        SELECT @user,'Outros (importado)','despesa','#0B281E','Category',1
        WHERE NOT EXISTS(SELECT 1 FROM Categorias WHERE id_usuario=@user AND nome_categoria='Outros (importado)' AND tipo='despesa');
        SELECT id_categoria FROM Categorias
        WHERE id_usuario=@user AND nome_categoria='Outros (importado)' AND tipo='despesa';
        """, ("@user", userId))!);

    var incomeCategoryMap = new Dictionary<int, long>();
    foreach (var category in sourceIncomeTypes)
    {
        var id = Convert.ToInt64(await ScalarAsync(target, transaction, """
            INSERT INTO Categorias(id_usuario,nome_categoria,tipo,cor,icone,ativo)
            VALUES(@user,@name,'receita','#00A859','TrendingUp',1);
            SELECT last_insert_rowid();
            """, ("@user", userId), ("@name", category.Name))!);
        incomeCategoryMap[category.Id] = id;
    }
    var fallbackIncomeCategory = Convert.ToInt64(await ScalarAsync(target, transaction, """
        INSERT INTO Categorias(id_usuario,nome_categoria,tipo,cor,icone,ativo)
        SELECT @user,'Outros (importado)','receita','#00A859','TrendingUp',1
        WHERE NOT EXISTS(SELECT 1 FROM Categorias WHERE id_usuario=@user AND nome_categoria='Outros (importado)' AND tipo='receita');
        SELECT id_categoria FROM Categorias
        WHERE id_usuario=@user AND nome_categoria='Outros (importado)' AND tipo='receita';
        """, ("@user", userId))!);

    var cardMap = new Dictionary<int, long>();
    foreach (var card in sourceCards)
    {
        var id = Convert.ToInt64(await ScalarAsync(target, transaction, """
            INSERT INTO CartoesCredito(id_usuario,nome_cartao,limite_credito,dia_fechamento,
              dia_vencimento,limite_utilizado,ativo,cor)
            VALUES(@user,@name,@limit,@closing,@due,0,1,@color);
            SELECT last_insert_rowid();
            """, ("@user", userId), ("@name", card.Name), ("@limit", card.Limit),
            ("@closing", card.ClosingDay), ("@due", card.DueDay),
            ("@color", CardColor(card.Id)))!);
        cardMap[card.Id] = id;
    }

    var installmentsByExpense = sourceInstallments
        .GroupBy(x => x.ExpenseId).ToDictionary(x => x.Key, x => x.OrderBy(y => y.Number).ToList());
    var importedExpenseRows = 0;
    var importedParentRows = 0;

    foreach (var expense in sourceExpenses)
    {
        if (!installmentsByExpense.TryGetValue(expense.Id, out var installments) || installments.Count == 0)
            throw new InvalidOperationException($"A despesa {expense.Id} não possui parcelas.");

        var categoryId = expense.CategoryId is { } oldCategory &&
                         expenseCategoryMap.TryGetValue(oldCategory, out var mappedCategory)
            ? mappedCategory : fallbackExpenseCategory;
        var cardId = expense.CardId is { } oldCard && cardMap.TryGetValue(oldCard, out var mappedCard)
            ? mappedCard : (long?)null;
        var notes = BuildExpenseNotes(expense);
        long? parentId = null;

        if (installments.Count > 1)
        {
            parentId = Convert.ToInt64(await ScalarAsync(target, transaction, """
                INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                  parcelado,numero_parcelas,id_transacao_pai,observacoes,data_vencimento,pago,
                  numero_parcela,total_parcelas,recorrente)
                VALUES(@account,@card,@category,@user,@amount,@date,@description,'despesa',
                  1,@total,NULL,@notes,NULL,0,1,@total,0);
                SELECT last_insert_rowid();
                """, ("@account", cardId is null ? accountId : DBNull.Value),
                ("@card", cardId ?? (object)DBNull.Value), ("@category", categoryId), ("@user", userId),
                ("@amount", expense.Amount), ("@date", Iso(expense.PurchaseDate)),
                ("@description", expense.Description), ("@total", installments.Count), ("@notes", notes))!);
            importedParentRows++;
        }

        foreach (var installment in installments)
        {
            await ExecuteAsync(target, transaction, """
                INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                  parcelado,numero_parcelas,id_transacao_pai,observacoes,data_vencimento,pago,
                  data_pagamento,numero_parcela,total_parcelas,recorrente)
                VALUES(@account,@card,@category,@user,@amount,@date,@description,'despesa',
                  @installment,@total,@parent,@notes,@due,@paid,@paidDate,@number,@total,0)
                """, ("@account", cardId is null ? accountId : DBNull.Value),
                ("@card", cardId ?? (object)DBNull.Value), ("@category", categoryId), ("@user", userId),
                ("@amount", installment.Amount), ("@date", Iso(installment.DueDate)),
                ("@description", installments.Count > 1
                    ? $"{expense.Description} ({installment.Number}/{installments.Count})"
                    : expense.Description),
                ("@installment", installments.Count > 1 ? 1 : 0), ("@total", installments.Count),
                ("@parent", parentId ?? (object)DBNull.Value), ("@notes", notes),
                ("@due", Iso(installment.DueDate)), ("@paid", installment.Paid ? 1 : 0),
                ("@paidDate", installment.PaymentDate is { } payment ? Iso(payment) : DBNull.Value),
                ("@number", installment.Number));
            importedExpenseRows++;
        }
    }

    foreach (var income in sourceIncomes)
    {
        var categoryId = income.TypeId is { } oldType &&
                         incomeCategoryMap.TryGetValue(oldType, out var mappedCategory)
            ? mappedCategory : fallbackIncomeCategory;
        var notes = $"Importado do SQL Server • ReceitaID={income.Id}";
        await ExecuteAsync(target, transaction, """
            INSERT INTO Transacoes(id_conta,id_categoria,id_usuario,valor,data,descricao,tipo,
              parcelado,observacoes,data_vencimento,pago,data_pagamento,numero_parcela,total_parcelas,recorrente)
            VALUES(@account,@category,@user,@amount,@date,@description,'receita',
              0,@notes,@date,1,@date,1,1,0)
            """, ("@account", accountId), ("@category", categoryId), ("@user", userId),
            ("@amount", income.Amount), ("@date", Iso(income.Date)),
            ("@description", income.Description), ("@notes", notes));
        await ExecuteAsync(target, transaction, """
            INSERT INTO Receitas(id_usuario,id_categoria,valor,data,descricao,recorrente,observacoes,data_criacao,ativo)
            VALUES(@user,@category,@amount,@date,@description,0,@notes,@created,1)
            """, ("@user", userId), ("@category", categoryId), ("@amount", income.Amount),
            ("@date", Iso(income.Date)), ("@description", income.Description),
            ("@notes", notes), ("@created", income.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")));
    }

    var invoiceGroups = await ReadTargetAsync(target, transaction, """
        SELECT id_cartao,CAST(strftime('%m',data_vencimento) AS INTEGER),
          CAST(strftime('%Y',data_vencimento) AS INTEGER),SUM(valor),MIN(pago),
          MAX(data_pagamento)
        FROM Transacoes
        WHERE id_usuario=@user AND tipo='despesa' AND id_cartao IS NOT NULL
          AND data_vencimento IS NOT NULL
          AND NOT(parcelado=1 AND id_transacao_pai IS NULL)
        GROUP BY id_cartao,strftime('%Y-%m',data_vencimento)
        """, r => new InvoiceGroup(r.GetInt64(0), r.GetInt32(1), r.GetInt32(2),
            r.GetDecimal(3), r.GetInt32(4) == 1, r.IsDBNull(5) ? null : r.GetString(5)),
        ("@user", userId));
    var cardsByNewId = sourceCards.ToDictionary(x => cardMap[x.Id]);

    foreach (var invoice in invoiceGroups)
    {
        var card = cardsByNewId[invoice.CardId];
        var dueDate = new DateTime(invoice.Year, invoice.Month,
            Math.Min(card.DueDay, DateTime.DaysInMonth(invoice.Year, invoice.Month)));
        var status = invoice.AllPaid ? "paga" : dueDate.Date < DateTime.Today ? "vencida" : "aberta";
        var invoiceId = Convert.ToInt64(await ScalarAsync(target, transaction, """
            INSERT INTO FaturasCartao(id_cartao,mes_referencia,ano_referencia,valor_total,
              data_vencimento,status,data_pagamento)
            VALUES(@card,@month,@year,@amount,@due,@status,@paidDate);
            SELECT last_insert_rowid();
            """, ("@card", invoice.CardId), ("@month", invoice.Month), ("@year", invoice.Year),
            ("@amount", invoice.Amount), ("@due", Iso(dueDate)), ("@status", status),
            ("@paidDate", invoice.AllPaid && invoice.PaymentDate is not null
                ? invoice.PaymentDate : DBNull.Value))!);
        await ExecuteAsync(target, transaction, """
            UPDATE Transacoes SET id_fatura=@invoice
            WHERE id_usuario=@user AND id_cartao=@card
              AND CAST(strftime('%m',data_vencimento) AS INTEGER)=@month
              AND CAST(strftime('%Y',data_vencimento) AS INTEGER)=@year
              AND NOT(parcelado=1 AND id_transacao_pai IS NULL)
            """, ("@invoice", invoiceId), ("@user", userId), ("@card", invoice.CardId),
            ("@month", invoice.Month), ("@year", invoice.Year));
    }

    await ExecuteAsync(target, transaction, """
        UPDATE CartoesCredito SET limite_utilizado=COALESCE((
          SELECT SUM(t.valor) FROM Transacoes t
          WHERE t.id_cartao=CartoesCredito.id_cartao AND t.tipo='despesa' AND t.pago=0
            AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL)),0)
        WHERE id_usuario=@user
        """, ("@user", userId));

    await ExecuteAsync(target, transaction, """
        INSERT INTO Logs(id_usuario,acao,tabela,dados_novos,origem)
        VALUES(@user,'MIGRACAO_SQLSERVER','Sistema',@details,'MoneyMigration')
        """, ("@user", userId),
        ("@details", $"Despesas={sourceExpenses.Count};Parcelas={importedExpenseRows};" +
                     $"PaisParcelamento={importedParentRows};Receitas={sourceIncomes.Count};" +
                     $"Categorias={sourceCategories.Count + sourceIncomeTypes.Count + 2};Cartoes={sourceCards.Count}"));

    var actualExpenseRows = Convert.ToInt32(await ScalarAsync(target, transaction, """
        SELECT COUNT(*) FROM Transacoes WHERE id_usuario=@user AND tipo='despesa'
          AND NOT(parcelado=1 AND id_transacao_pai IS NULL)
        """, ("@user", userId)));
    var actualIncomeRows = Convert.ToInt32(await ScalarAsync(target, transaction,
        "SELECT COUNT(*) FROM Transacoes WHERE id_usuario=@user AND tipo='receita'", ("@user", userId)));
    var actualExpenseTotal = Convert.ToDecimal(await ScalarAsync(target, transaction, """
        SELECT COALESCE(SUM(valor),0) FROM Transacoes WHERE id_usuario=@user AND tipo='despesa'
          AND NOT(parcelado=1 AND id_transacao_pai IS NULL)
        """, ("@user", userId)), CultureInfo.InvariantCulture);
    var sourceExpenseTotal = sourceInstallments.Sum(x => x.Amount);
    var actualIncomeTotal = Convert.ToDecimal(await ScalarAsync(target, transaction,
        "SELECT COALESCE(SUM(valor),0) FROM Transacoes WHERE id_usuario=@user AND tipo='receita'",
        ("@user", userId)), CultureInfo.InvariantCulture);
    var sourceIncomeTotal = sourceIncomes.Sum(x => x.Amount);

    if (actualExpenseRows != sourceInstallments.Count ||
        actualIncomeRows != sourceIncomes.Count ||
        Math.Abs(actualExpenseTotal - sourceExpenseTotal) > .01m ||
        Math.Abs(actualIncomeTotal - sourceIncomeTotal) > .01m)
        throw new InvalidOperationException("A validação financeira da migração não confere com a origem.");

    await transaction.CommitAsync();
    Console.WriteLine("MIGRACAO_CONCLUIDA");
    Console.WriteLine($"DESPESAS_ORIGEM={sourceExpenses.Count}");
    Console.WriteLine($"PARCELAS_IMPORTADAS={actualExpenseRows}");
    Console.WriteLine($"PAIS_PARCELAMENTO={importedParentRows}");
    Console.WriteLine($"TOTAL_DESPESAS={actualExpenseTotal:F2}");
    Console.WriteLine($"RECEITAS_IMPORTADAS={actualIncomeRows}");
    Console.WriteLine($"TOTAL_RECEITAS={actualIncomeTotal:F2}");
    Console.WriteLine($"CATEGORIAS={sourceCategories.Count + sourceIncomeTypes.Count + 2}");
    Console.WriteLine($"CARTOES={sourceCards.Count}");
    Console.WriteLine($"FATURAS={invoiceGroups.Count}");
    return 0;
}
catch
{
    await transaction.RollbackAsync();
    throw;
}

static async Task<List<T>> ReadSourceAsync<T>(SqlConnection connection, string sql,
    Func<SqlDataReader, T> map)
{
    await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
    await using var reader = await command.ExecuteReaderAsync();
    var result = new List<T>();
    while (await reader.ReadAsync()) result.Add(map(reader));
    return result;
}

static async Task<List<T>> ReadTargetAsync<T>(SqliteConnection connection,
    System.Data.Common.DbTransaction transaction, string sql, Func<SqliteDataReader, T> map,
    params (string Name, object Value)[] parameters)
{
    await using var command = connection.CreateCommand();
    command.Transaction = (SqliteTransaction)transaction;
    command.CommandText = sql;
    AddParameters(command, parameters);
    await using var reader = await command.ExecuteReaderAsync();
    var result = new List<T>();
    while (await reader.ReadAsync()) result.Add(map(reader));
    return result;
}

static async Task<int> ExecuteAsync(SqliteConnection connection,
    System.Data.Common.DbTransaction transaction, string sql,
    params (string Name, object Value)[] parameters)
{
    await using var command = connection.CreateCommand();
    command.Transaction = (SqliteTransaction)transaction;
    command.CommandText = sql;
    AddParameters(command, parameters);
    return await command.ExecuteNonQueryAsync();
}

static async Task<object?> ScalarAsync(SqliteConnection connection,
    System.Data.Common.DbTransaction transaction, string sql,
    params (string Name, object Value)[] parameters)
{
    await using var command = connection.CreateCommand();
    command.Transaction = (SqliteTransaction)transaction;
    command.CommandText = sql;
    AddParameters(command, parameters);
    return await command.ExecuteScalarAsync();
}

static void AddParameters(SqliteCommand command, IEnumerable<(string Name, object Value)> parameters)
{
    foreach (var (name, value) in parameters)
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
}

static string BuildExpenseNotes(SourceExpense expense) =>
    $"Importado do SQL Server • DespesaID={expense.Id}" +
    (string.IsNullOrWhiteSpace(expense.PaymentMethod) ? "" : $" • Método: {expense.PaymentMethod}") +
    (string.IsNullOrWhiteSpace(expense.Supplier) ? "" : $" • Fornecedor: {expense.Supplier}") +
    $" • Status antigo: {expense.Status} • Valor pago antigo: {expense.PaidAmount:F2}";

static string Iso(DateTime value) => value.ToString("yyyy-MM-dd");

static string CategoryColor(string name)
{
    var value = name.ToLowerInvariant();
    if (value.Contains("alimenta") || value.Contains("lanche")) return "#0B281E";
    if (value.Contains("saúde") || value.Contains("saude") || value.Contains("farm")) return "#0B281E";
    if (value.Contains("combust") || value.Contains("veículo") || value.Contains("veiculo")) return "#0B281E";
    if (value.Contains("educa")) return "#0B281E";
    if (value.Contains("moradia") || value.Contains("casa")) return "#0B281E";
    if (value.Contains("compra") || value.Contains("roupa") || value.Contains("calçado")) return "#0B281E";
    if (value.Contains("energia") || value.Contains("água") || value.Contains("agua")) return "#0B281E";
    return "#0B281E";
}

static string CategoryIcon(string name)
{
    var value = name.ToLowerInvariant();
    if (value.Contains("alimenta") || value.Contains("lanche")) return "Restaurant";
    if (value.Contains("saúde") || value.Contains("saude") || value.Contains("farm")) return "MedicalServices";
    if (value.Contains("combust") || value.Contains("veículo") || value.Contains("veiculo")) return "DirectionsCar";
    if (value.Contains("educa")) return "School";
    if (value.Contains("moradia") || value.Contains("casa")) return "House";
    if (value.Contains("compra") || value.Contains("roupa") || value.Contains("calçado")) return "ShoppingBag";
    if (value.Contains("energia") || value.Contains("água") || value.Contains("agua")) return "Lightbulb";
    return "Category";
}

static string CardColor(int id) => id % 2 == 0 ? "#00A859" : "#0B281E";

sealed record SourceCategory(int Id, string Name);
sealed record SourceCard(int Id, string Name, decimal Limit, int ClosingDay, int DueDay, string Brand);
sealed record SourceExpense(int Id, string Description, decimal Amount, DateTime PurchaseDate,
    int? CategoryId, string? PaymentMethod, string? Supplier, int InstallmentCount,
    int? CardId, decimal PaidAmount, int Status);
sealed record SourceInstallment(int Id, int ExpenseId, int Number, decimal Amount,
    DateTime DueDate, DateTime? PaymentDate, bool Paid);
sealed record SourceIncome(int Id, string Description, decimal Amount, DateTime Date,
    int? TypeId, DateTime CreatedAt);
sealed record InvoiceGroup(long CardId, int Month, int Year, decimal Amount,
    bool AllPaid, string? PaymentDate);
