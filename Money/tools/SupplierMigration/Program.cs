using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

SQLitePCL.Batteries_V2.Init();
const string sourceConnection = "Data Source=NOTEBOOK-DELL\\SQLEXPRESS;Initial Catalog=bdMoney;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";
const string targetPath = @"D:\Projetos\Money\moneypro.db";

await using var source = new SqlConnection(sourceConnection);
await source.OpenAsync();
var suppliers = new List<Supplier>();
await using (var cmd = new SqlCommand("SELECT FornecedorID,Nome,Ativo,DataCadastro FROM Fornecedores ORDER BY FornecedorID", source))
await using (var reader = await cmd.ExecuteReaderAsync())
    while (await reader.ReadAsync())
        suppliers.Add(new(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) || reader.GetBoolean(2),
            reader.IsDBNull(3) ? null : reader.GetDateTime(3)));

var expenseSupplier = new Dictionary<int, int>();
await using (var cmd = new SqlCommand("SELECT DespesaID,FornecedorID FROM Despesas WHERE FornecedorID IS NOT NULL", source))
await using (var reader = await cmd.ExecuteReaderAsync())
    while (await reader.ReadAsync()) expenseSupplier[reader.GetInt32(0)] = reader.GetInt32(1);

await using var target = new SqliteConnection($"Data Source={targetPath};Foreign Keys=True");
await target.OpenAsync();
await using var tx = await target.BeginTransactionAsync();
try
{
    async Task Exec(string sql, params (string, object?)[] args)
    {
        await using var command = target.CreateCommand(); command.Transaction = (SqliteTransaction)tx; command.CommandText = sql;
        foreach (var (name, value) in args) command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }
    await Exec("""
        CREATE TABLE IF NOT EXISTS Fornecedores(
          id_fornecedor INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,id_usuario INTEGER NOT NULL,
          nome_fornecedor TEXT NOT NULL,ativo INTEGER DEFAULT 1,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP);
        CREATE INDEX IF NOT EXISTS idx_fornecedores_usuario_nome ON Fornecedores(id_usuario,ativo,nome_fornecedor);
        """);
    await using (var check = target.CreateCommand())
    {
        check.Transaction = (SqliteTransaction)tx;
        check.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Transacoes') WHERE name='id_fornecedor'";
        if (Convert.ToInt32(await check.ExecuteScalarAsync()) == 0) await Exec("ALTER TABLE Transacoes ADD COLUMN id_fornecedor INTEGER NULL");
    }
    await Exec("CREATE INDEX IF NOT EXISTS idx_transacoes_fornecedor ON Transacoes(id_fornecedor)");
    var userId = 1L;
    await using (var user = target.CreateCommand())
    {
        user.Transaction = (SqliteTransaction)tx; user.CommandText = "SELECT id_usuario FROM Usuarios WHERE ativo=1 ORDER BY id_usuario LIMIT 1";
        userId = Convert.ToInt64(await user.ExecuteScalarAsync() ?? throw new InvalidOperationException("SQLite sem usuário ativo."));
    }
    foreach (var item in suppliers)
        await Exec("""
            INSERT INTO Fornecedores(id_fornecedor,id_usuario,nome_fornecedor,ativo,data_criacao)
            VALUES(@id,@user,@name,@active,@created)
            ON CONFLICT(id_fornecedor) DO UPDATE SET id_usuario=excluded.id_usuario,
              nome_fornecedor=excluded.nome_fornecedor,ativo=excluded.ativo,data_criacao=excluded.data_criacao
            """, ("@id", item.Id), ("@user", userId), ("@name", item.Name), ("@active", item.Active ? 1 : 0),
            ("@created", item.Created?.ToString("yyyy-MM-dd HH:mm:ss")));

    var links = 0;
    await using (var rows = target.CreateCommand())
    {
        rows.Transaction = (SqliteTransaction)tx; rows.CommandText = "SELECT id_transacao,observacoes FROM Transacoes WHERE id_usuario=@user AND observacoes LIKE '%DespesaID=%'";
        rows.Parameters.AddWithValue("@user", userId);
        await using var reader = await rows.ExecuteReaderAsync();
        var updates = new List<(long Transaction, int Supplier)>();
        while (await reader.ReadAsync())
        {
            var match = Regex.Match(reader.IsDBNull(1) ? "" : reader.GetString(1), @"DespesaID=(\d+)");
            if (match.Success && expenseSupplier.TryGetValue(int.Parse(match.Groups[1].Value), out var supplier)) updates.Add((reader.GetInt64(0), supplier));
        }
        await reader.CloseAsync();
        foreach (var update in updates) { await Exec("UPDATE Transacoes SET id_fornecedor=@supplier WHERE id_transacao=@id", ("@supplier", update.Supplier), ("@id", update.Transaction)); links++; }
    }
    await Exec("INSERT INTO Logs(id_usuario,acao,tabela,dados_novos,origem) VALUES(@user,'MIGRAR_FORNECEDORES','Fornecedores',@data,'SupplierMigration')",
        ("@user", userId), ("@data", $"Fornecedores={suppliers.Count};Vinculos={links}"));
    await tx.CommitAsync();
    Console.WriteLine($"MIGRACAO_CONCLUIDA FORNECEDORES={suppliers.Count} VINCULOS_TRANSACOES={links}");
}
catch { await tx.RollbackAsync(); throw; }

sealed record Supplier(int Id, string Name, bool Active, DateTime? Created);
