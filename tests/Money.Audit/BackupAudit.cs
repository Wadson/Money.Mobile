using Microsoft.Data.Sqlite;
using Money.Services;

internal static class BackupAudit
{
    public static async Task RunAsync(Func<string, Func<Task>, Task> test, string? suppliedBackup)
    {
        var directory = Path.Combine(Path.GetTempPath(), "MoneyBackupAudit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var db = new DatabaseService(Path.Combine(directory, "live.db"));
        await db.InitializeAsync();
        var store = new SqliteBackupStore(db);
        async Task<object?> Sql(string path, string text)
        {
            await using var connection = new SqliteConnection($"Data Source={path};Pooling=False");
            await connection.OpenAsync();
            await using var command = connection.CreateCommand(); command.CommandText = text;
            return await command.ExecuteScalarAsync();
        }
        void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        var snapshot = Path.Combine(directory, "snapshot.db");
        await using var lingering = new SqliteConnection($"Data Source={db.DatabasePath};Pooling=True");
        await lingering.OpenAsync();
        await test("backup captures committed WAL pages with connection open", async () =>
        {
            await using var command = lingering.CreateCommand();
            command.CommandText = "PRAGMA wal_autocheckpoint=0; UPDATE Usuarios SET nome='WAL backup marker' WHERE id_usuario=1;";
            await command.ExecuteNonQueryAsync();
            Check(new FileInfo(db.DatabasePath + "-wal").Length > 0, "test requires committed WAL pages");
            await store.CreateAsync(snapshot);
            Check(Convert.ToString(await Sql(snapshot, "SELECT nome FROM Usuarios WHERE id_usuario=1")) == "WAL backup marker", "snapshot omitted WAL data");
            await Sql(db.DatabasePath, "UPDATE Usuarios SET nome='Changed after backup' WHERE id_usuario=1");
            Check(Convert.ToString(await Sql(snapshot, "SELECT nome FROM Usuarios WHERE id_usuario=1")) == "WAL backup marker", "snapshot changed after later writes");
        });
        await test("restore replaces data with open connection and preserves previous snapshot", async () =>
        {
            var result = await store.RestoreAsync(snapshot);
            await using var command = lingering.CreateCommand();
            command.CommandText = "SELECT nome FROM Usuarios WHERE id_usuario=1";
            Check(Convert.ToString(await command.ExecuteScalarAsync()) == "WAL backup marker", "open connection retained old data");
            Check(Convert.ToString(await Sql(result.SafetyPath, command.CommandText)) == "Changed after backup", "safety backup not preserved");
            await new DatabaseService(db.DatabasePath).InitializeAsync();
            Check(Convert.ToString(await Sql(db.DatabasePath, command.CommandText)) == "WAL backup marker", "restart lost restored data");
        });
        await test("invalid backup is rejected without changing current database", async () =>
        {
            var invalid = Path.Combine(directory, "not-sqlite.db");
            await File.WriteAllTextAsync(invalid, "invalid database");
            try { await store.RestoreAsync(invalid); throw new Exception("invalid file accepted"); }
            catch (SqliteException) { }
            Check(Convert.ToString(await Sql(db.DatabasePath, "SELECT nome FROM Usuarios WHERE id_usuario=1")) == "WAL backup marker", "invalid restore changed current data");
        });
        await test("SQLite with only Usuarios is rejected", async () =>
        {
            var invalid = Path.Combine(directory, "incomplete.db");
            await Sql(invalid, "CREATE TABLE Usuarios(id_usuario INTEGER,nome TEXT,email TEXT,senha_hash TEXT,ativo INTEGER)");
            try { await store.RestoreAsync(invalid); throw new Exception("incomplete schema accepted"); }
            catch (InvalidDataException) { }
        });
        await test("future schema and broken foreign keys are rejected", async () =>
        {
            var invalid = Path.Combine(directory, "future.db");
            await store.CreateAsync(invalid);
            await Sql(invalid, "PRAGMA user_version=999");
            try { await store.RestoreAsync(invalid); throw new Exception("future schema accepted"); }
            catch (InvalidDataException) { }
            await Sql(invalid, "PRAGMA user_version=29; PRAGMA foreign_keys=OFF; INSERT INTO Orcamentos(id_usuario,id_categoria,valor_limite,mes,ano) VALUES(99999,99999,100,1,2040)");
            try { await store.RestoreAsync(invalid); throw new Exception("broken foreign keys accepted"); }
            catch (InvalidDataException) { }
        });
        await test("duplicate login names require email and logout clears financial user", async () =>
        {
            await Sql(db.DatabasePath, "INSERT INTO Usuarios(nome,email,senha_hash,ativo) SELECT nome,'other@example.test',senha_hash,1 FROM Usuarios WHERE id_usuario=1");
            var auth = new AuthService(db);
            var result = await auth.LoginAsync("WAL backup marker", "unused", false);
            Check(!result.Success && result.Message.Contains("e-mail"), "ambiguous name selected a user");
            auth.Logout();
            Check(db.CurrentUserId == 0 && auth.UserId is null, "logout retained financial user");
        });
        await test("email login selects the intended user despite duplicate names", async () =>
        {
            var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
            var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2("Audit1234", salt, 1000,
                System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
            var encoded = $"PBKDF2-SHA256$1000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
            await Sql(db.DatabasePath, $"UPDATE Usuarios SET senha_hash='{encoded}' WHERE email='other@example.test'");
            var auth = new AuthService(db);
            var result = await auth.LoginAsync("other@example.test", "Audit1234", true);
            Check(result.Success, "email login failed");
            var expected = Convert.ToInt64(await Sql(db.DatabasePath, "SELECT id_usuario FROM Usuarios WHERE email='other@example.test'"));
            Check(auth.UserId == expected && db.CurrentUserId == expected, "wrong user selected by email");
            auth.Logout();
        });
        if (suppliedBackup is not null)
        {
            await test("moneyproNovo restore and restart retain all transactions for correct user", async () =>
            {
                var before = Convert.ToInt64(await Sql(suppliedBackup, "SELECT COUNT(*) FROM Transacoes"));
                var result = await store.RestoreAsync(suppliedBackup);
                Check(result.Transactions == before, "restored count mismatch");
                Check(result.Users.Any(u => u.Transactions > 0), "restored users do not expose data owner");
                var restarted = new DatabaseService(db.DatabasePath);
                await restarted.InitializeAsync();
                Check(Convert.ToInt64(await Sql(db.DatabasePath, "SELECT COUNT(*) FROM Transacoes")) == before, "restart lost data");
                var owner = Convert.ToInt64(await Sql(db.DatabasePath, "SELECT id_usuario FROM Transacoes GROUP BY id_usuario ORDER BY COUNT(*) DESC LIMIT 1"));
                restarted.CurrentUserId = owner;
                Check((await restarted.GetContasPagarAsync(null, null, null)).Count > 0, "payables empty for data owner");
                await using var connection = new SqliteConnection($"Data Source={db.DatabasePath};Pooling=False");
                await connection.OpenAsync();
                await using var compare = connection.CreateCommand();
                compare.CommandText = "ATTACH DATABASE @source AS original";
                compare.Parameters.AddWithValue("@source", Path.GetFullPath(suppliedBackup));
                await compare.ExecuteNonQueryAsync();
                compare.CommandText = "SELECT COUNT(*) FROM (SELECT * FROM main.Transacoes EXCEPT SELECT * FROM original.Transacoes)";
                Check(Convert.ToInt64(await compare.ExecuteScalarAsync()) == 0, "transaction contents changed");
            });
        }
    }
}
