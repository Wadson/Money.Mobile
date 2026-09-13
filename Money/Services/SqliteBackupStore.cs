using Microsoft.Data.Sqlite;

namespace Money.Services;

public sealed record RestoredBackupUser(string Name, string Email, long Transactions);
public sealed record BackupRestoreResult(string SafetyPath, long Transactions, IReadOnlyList<RestoredBackupUser> Users)
{
    public string Summary => $"{Transactions} lançamento(s) restaurado(s).\n\n" +
        string.Join("\n", Users.Select(u => $"{u.Name} — {u.Email}: {u.Transactions} lançamento(s)")) +
        "\n\nEntre com o e-mail do usuário cujos dados deseja consultar.";
}

/// <summary>SQLite snapshots include committed WAL pages and never overwrite an open database file.</summary>
public sealed class SqliteBackupStore(DatabaseService database)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static SqliteConnection Connection(string path, SqliteOpenMode mode) => new(new SqliteConnectionStringBuilder
    {
        DataSource = Path.GetFullPath(path), Mode = mode, Pooling = false, DefaultTimeout = 15
    }.ToString());

    public async Task CreateAsync(string destination)
    {
        await Gate.WaitAsync();
        try
        {
            if (File.Exists(destination)) throw new IOException("O arquivo de destino já existe.");
            await SnapshotAsync(database.DatabasePath, destination);
            await ValidateAsync(destination);
        }
        finally { Gate.Release(); }
    }

    private static async Task SnapshotAsync(string sourcePath, string destinationPath)
    {
        if (Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Origem e destino do backup devem ser diferentes.");
        await using var source = Connection(sourcePath, SqliteOpenMode.ReadOnly);
        await using var destination = Connection(destinationPath, SqliteOpenMode.ReadWriteCreate);
        await source.OpenAsync();
        await destination.OpenAsync();
        // SQLite copies a consistent, committed snapshot, including pages in -wal.
        // Updating an existing database is an atomic SQLite transaction.
        source.BackupDatabase(destination);
    }

    public async Task<BackupRestoreResult> RestoreAsync(string sourcePath)
    {
        await Gate.WaitAsync();
        var candidate = Path.Combine(Path.GetDirectoryName(database.DatabasePath)!, $"restore-{Guid.NewGuid():N}.db");
        var safety = database.DatabasePath + $".antes-restauracao-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.bak";
        try
        {
            await ValidateAsync(sourcePath);
            await SnapshotAsync(sourcePath, candidate);
            // All schema upgrades run on an isolated copy, before touching current data.
            await new DatabaseService(candidate).InitializeAsync();
            await ValidateAsync(candidate);
            var expected = await InspectAsync(candidate, safety);
            await SnapshotAsync(database.DatabasePath, safety);
            await ValidateAsync(safety);
            try
            {
                await SnapshotAsync(candidate, database.DatabasePath);
                await ValidateAsync(database.DatabasePath);
                var actual = await InspectAsync(database.DatabasePath, safety);
                if (actual.Transactions != expected.Transactions || !actual.Users.SequenceEqual(expected.Users))
                    throw new InvalidDataException("A conferência dos dados restaurados não corresponde ao backup.");
                return actual;
            }
            catch (Exception restoreError)
            {
                try { await SnapshotAsync(safety, database.DatabasePath); }
                catch (Exception rollbackError)
                {
                    throw new IOException($"A restauração falhou. A cópia anterior está preservada em {safety}.",
                        new AggregateException(restoreError, rollbackError));
                }
                throw;
            }
        }
        finally
        {
            // Only the uniquely named staging file belongs to this operation.
            foreach (var suffix in new[] { "", "-wal", "-shm" })
                try { File.Delete(candidate + suffix); } catch (IOException) { }
            Gate.Release();
        }
    }

    public static async Task ValidateAsync(string path)
    {
        await using var db = Connection(path, SqliteOpenMode.ReadOnly);
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = "PRAGMA integrity_check";
        if (!string.Equals(Convert.ToString(await command.ExecuteScalarAsync()), "ok", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("O arquivo selecionado não é um banco SQLite íntegro.");
        command.CommandText = "PRAGMA user_version";
        if (Convert.ToInt64(await command.ExecuteScalarAsync()) > DatabaseService.SupportedSchemaVersion)
            throw new InvalidDataException("Este backup foi criado por uma versão mais recente do Money Pro. Atualize o aplicativo.");
        var required = new Dictionary<string, string[]>
        {
            ["Usuarios"] = ["id_usuario", "nome", "email", "senha_hash", "ativo"],
            ["Transacoes"] = ["id_transacao", "id_usuario", "id_categoria", "valor", "tipo", "data", "descricao"],
            ["Categorias"] = ["id_categoria", "id_usuario", "nome_categoria", "tipo"],
            ["CartoesCredito"] = ["id_cartao", "id_usuario", "nome_cartao", "limite_credito"],
            ["Configuracoes"] = ["id_configuracao", "id_usuario"]
        };
        foreach (var (table, columns) in required)
        {
            command.CommandText = $"PRAGMA table_info('{table}')";
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) found.Add(reader.GetString(1));
            if (columns.Any(c => !found.Contains(c)))
                throw new InvalidDataException($"Backup incompatível: estrutura de {table} incompleta.");
        }
        command.CommandText = "PRAGMA foreign_key_check";
        if (await command.ExecuteScalarAsync() is not null)
            throw new InvalidDataException("O backup contém relacionamentos inválidos entre os cadastros.");
        command.CommandText = "SELECT COUNT(*) FROM Usuarios WHERE ativo=1";
        if (Convert.ToInt64(await command.ExecuteScalarAsync()) == 0)
            throw new InvalidDataException("O backup não possui um usuário ativo para acessar os dados.");
    }

    private static async Task<BackupRestoreResult> InspectAsync(string path, string safety)
    {
        await using var db = Connection(path, SqliteOpenMode.ReadOnly);
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Transacoes";
        var total = Convert.ToInt64(await command.ExecuteScalarAsync());
        command.CommandText = """
            SELECT u.nome,u.email,(SELECT COUNT(*) FROM Transacoes t WHERE t.id_usuario=u.id_usuario)
            FROM Usuarios u WHERE u.ativo=1 ORDER BY u.id_usuario
            """;
        var users = new List<RestoredBackupUser>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) users.Add(new(reader.GetString(0), reader.GetString(1), reader.GetInt64(2)));
        return new(safety, total, users);
    }
}
