using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Money.Models;

namespace Money.Services;

/// <summary>Provides local account authentication, lockout, and encrypted session persistence.</summary>
public sealed class AuthService(DatabaseService database)
{
    private const string SessionKey = "moneypro.session.user";
    private const int Iterations = 210_000;
    public long? UserId
    {
        get; private set;
    }
    public string? UserName
    {
        get; private set;
    }

    public async Task InitializeAsync()
    {
        await database.InitializeAsync();
        await UpgradeDemoPasswordAsync();
    }

    public async Task<AuthResult> LoginAsync(string user, string password, bool remember)
    {
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            return new(false, "Informe usuário e senha.");

        await using var db = Connection();
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = """
            SELECT id_usuario,nome,senha_hash,tentativas_falhas,bloqueado_ate
            FROM Usuarios WHERE (lower(nome)=lower(@user) OR lower(email)=lower(@user)) AND ativo=1
            """;
        command.Parameters.AddWithValue("@user", user.Trim());
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return new(false, "Usuário ou senha inválidos.");
        var id = reader.GetInt64(0);
        var name = reader.GetString(1);
        var hash = reader.GetString(2);
        var attempts = reader.GetInt32(3);
        DateTime? lockedUntil = reader.IsDBNull(4) ? null : reader.GetDateTime(4);
        await reader.DisposeAsync();

        if (lockedUntil > DateTime.UtcNow)
            return new(false, $"Conta temporariamente bloqueada. Tente novamente às {lockedUntil.Value.ToLocalTime():HH:mm}.");

        if (!VerifyPassword(password, hash))
        {
            attempts++;
            var block = attempts >= 5 ? DateTime.UtcNow.AddMinutes(5) : (DateTime?)null;
            await UpdateAttemptsAsync(db, id, attempts >= 5 ? 0 : attempts, block);
            return new(false, attempts >= 5 ? "Conta bloqueada por 5 minutos após cinco tentativas." : "Usuário ou senha inválidos.");
        }

        await UpdateAttemptsAsync(db, id, 0, null);
        await ExecuteAsync(db, "UPDATE Usuarios SET ultimo_acesso=CURRENT_TIMESTAMP WHERE id_usuario=@id", ("@id", id));
        SetSession(id, name);
        if (remember)
            await SecureStorage.Default.SetAsync(SessionKey, id.ToString());
        else
            SecureStorage.Default.Remove(SessionKey);
        return new(true, "Login realizado.", id, name);
    }

    public async Task<List<ManagedUser>> GetManagedUsersAsync()
    {
        await InitializeAsync();
        await using var db = Connection();
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = """
            SELECT id_usuario,nome,email,data_nascimento,pergunta_recuperacao,ativo,ultimo_acesso
            FROM Usuarios WHERE ativo=1 ORDER BY nome COLLATE NOCASE
            """;
        var result = new List<ManagedUser>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(new ManagedUser(reader.GetInt64(0), reader.GetString(1), reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetDateTime(3), reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5) == 1, reader.IsDBNull(6) ? null : reader.GetDateTime(6)));
        return result;
    }

    public async Task<AuthResult> SaveManagedUserAsync(long? id, string name, string email,
        DateTime birthDate, string question, string? answer, string? password, string? confirmation)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3)
            return new(false, "Informe o nome do usuário.");
        if (!email.Contains('@') || !email.Contains('.'))
            return new(false, "Informe um e-mail válido.");
        if (birthDate > DateTime.Today.AddYears(-12) || birthDate < DateTime.Today.AddYears(-120))
            return new(false, "Informe uma data de nascimento válida.");
        if (!SecurityQuestions.All.Contains(question))
            return new(false, "Selecione uma pergunta de segurança.");
        if (id is null && string.IsNullOrWhiteSpace(answer))
            return new(false, "Informe a resposta de segurança.");
        if (id is null && string.IsNullOrWhiteSpace(password))
            return new(false, "Informe a senha inicial.");
        if (!string.IsNullOrWhiteSpace(password) &&
            (password.Length < 8 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit)))
            return new(false, "A senha deve ter ao menos 8 caracteres, letras e números.");
        if (!string.IsNullOrWhiteSpace(password) && password != confirmation)
            return new(false, "As senhas não coincidem.");

        await using var db = Connection();
        await db.OpenAsync();
        try
        {
            if (id is null)
            {
                var answerHash = HashRecoveryAnswer(answer!);
                await ExecuteAsync(db, """
                    INSERT INTO Usuarios(nome,email,senha_hash,senha_alterada_em,data_nascimento,
                      pergunta_recuperacao,resposta_recuperacao_hash,palavra_chave_hash,dica_palavra_chave,ativo)
                    VALUES(@name,@email,@password,CURRENT_TIMESTAMP,@birth,@question,@answer,@answer,@question,1)
                    """, ("@name", name.Trim()), ("@email", email.Trim().ToLowerInvariant()),
                    ("@password", HashPassword(password!)), ("@birth", birthDate.Date),
                    ("@question", question), ("@answer", answerHash));
                var newId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));
                await ExecuteAsync(db, "INSERT INTO Configuracoes(id_usuario) VALUES(@id)", ("@id", newId));
                await CopyDefaultCategoriesAsync(db, newId);
                return new(true, "Usuário criado com sucesso.", newId, name.Trim());
            }

            await ExecuteAsync(db, """
                UPDATE Usuarios SET nome=@name,email=@email,data_nascimento=@birth,
                  pergunta_recuperacao=@question,dica_palavra_chave=@question
                WHERE id_usuario=@id AND ativo=1
                """, ("@name", name.Trim()), ("@email", email.Trim().ToLowerInvariant()),
                ("@birth", birthDate.Date), ("@question", question), ("@id", id.Value));
            if (!string.IsNullOrWhiteSpace(answer))
            {
                var answerHash = HashRecoveryAnswer(answer);
                await ExecuteAsync(db, """
                    UPDATE Usuarios SET resposta_recuperacao_hash=@answer,palavra_chave_hash=@answer
                    WHERE id_usuario=@id
                    """, ("@answer", answerHash), ("@id", id.Value));
            }
            if (!string.IsNullOrWhiteSpace(password))
                await ExecuteAsync(db, "UPDATE Usuarios SET senha_hash=@hash,senha_alterada_em=CURRENT_TIMESTAMP WHERE id_usuario=@id",
                    ("@hash", HashPassword(password)), ("@id", id.Value));
            if (id == UserId) UserName = name.Trim();
            return new(true, "Usuário atualizado com sucesso.", id, name.Trim());
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return new(false, "Nome ou e-mail já utilizado por outro usuário.");
        }
    }

    public async Task<AuthResult> DeleteManagedUserAsync(long id)
    {
        if (id == UserId) return new(false, "O usuário conectado não pode excluir a própria conta.");
        await using var db = Connection();
        await db.OpenAsync();
        var active = Convert.ToInt32(await ScalarAsync(db, "SELECT COUNT(*) FROM Usuarios WHERE ativo=1"));
        if (active <= 1) return new(false, "É necessário manter ao menos um usuário ativo.");
        await ExecuteAsync(db, "UPDATE Usuarios SET ativo=0 WHERE id_usuario=@id", ("@id", id));
        return new(true, "Usuário excluído com segurança.");
    }

    // CORRIGIDO: REMOVER DateTime? - usar DateTime diretamente
    public async Task<AuthResult> RegisterAsync(string name, string email, string password, string confirmation,
        DateTime birthDate, string recoveryQuestion, string recoveryAnswer)  // DateTime, não DateTime?
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3)
            return new(false, "Informe seu nome completo.");
        if (!email.Contains('@') || !email.Contains('.'))
            return new(false, "Informe um email válido.");
        if (password.Length < 8 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            return new(false, "A senha deve ter ao menos 8 caracteres, letras e números.");
        if (password != confirmation)
            return new(false, "As senhas não coincidem.");
        if (birthDate > DateTime.Today.AddYears(-12) || birthDate < DateTime.Today.AddYears(-120))
            return new(false, "Informe uma data de nascimento válida.");
        if (string.IsNullOrWhiteSpace(recoveryQuestion) || recoveryAnswer.Trim().Length < 3)
            return new(false, "Escolha uma pergunta e informe uma resposta válida.");

        await using var db = Connection();
        await db.OpenAsync();
        try
        {
            await ExecuteAsync(db, """
                INSERT INTO Usuarios(nome,email,senha_hash,senha_alterada_em,data_nascimento,
                  pergunta_recuperacao,resposta_recuperacao_hash,palavra_chave_hash,dica_palavra_chave)
                VALUES(@name,@email,@hash,CURRENT_TIMESTAMP,@birth,@question,@answer,@answer,@question)
                """, ("@name", name.Trim()), ("@email", email.Trim().ToLowerInvariant()),
                ("@hash", HashPassword(password)), ("@birth", birthDate.Date),
                ("@question", recoveryQuestion), ("@answer", HashRecoveryAnswer(recoveryAnswer)));
            var id = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));
            await ExecuteAsync(db, "INSERT INTO Configuracoes(id_usuario) VALUES(@id)", ("@id", id));
            await CopyDefaultCategoriesAsync(db, id);
            SetSession(id, name.Trim());
            return new(true, "Conta criada com sucesso.", id, name.Trim());
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return new(false, "Este email já está cadastrado.");
        }
    }

    // CORRIGIDO: REMOVER DateTime? - usar DateTime diretamente
    public async Task<(bool Success, string Message, string? Question, string? Hint)> FindRecoveryAsync(string user, DateTime birthDate)
    {
        await using var db = Connection();
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = """
            SELECT pergunta_recuperacao,dica_palavra_chave FROM Usuarios
            WHERE (lower(nome)=lower(@user) OR lower(email)=lower(@user))
              AND date(data_nascimento)=date(@birth) AND ativo=1
            """;
        command.Parameters.AddWithValue("@user", user.Trim());
        command.Parameters.AddWithValue("@birth", birthDate.Date);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return (false, "Os dados informados não correspondem a uma conta.", null, null);
        return (true, "Identidade localizada.", reader.IsDBNull(0) ? "Informe sua palavra-chave" : reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetString(1));
    }

    // CORRIGIDO: REMOVER DateTime? - usar DateTime diretamente
    public async Task<AuthResult> ResetPasswordAsync(string user, DateTime birthDate, string answer,
        string newPassword, string confirmation)
    {
        if (newPassword.Length < 8 || !newPassword.Any(char.IsLetter) || !newPassword.Any(char.IsDigit))
            return new(false, "A nova senha deve ter ao menos 8 caracteres, letras e números.");
        if (newPassword != confirmation)
            return new(false, "As senhas não coincidem.");
        await using var db = Connection();
        await db.OpenAsync();
        var stored = Convert.ToString(await ScalarAsync(db, """
            SELECT COALESCE(palavra_chave_hash,resposta_recuperacao_hash) FROM Usuarios
            WHERE (lower(nome)=lower(@user) OR lower(email)=lower(@user))
              AND date(data_nascimento)=date(@birth) AND ativo=1
            """, ("@user", user.Trim()), ("@birth", birthDate.Date)));
        if (string.IsNullOrWhiteSpace(stored))
            return new(false, "Não foi possível validar a recuperação.");
        var expected = Convert.FromHexString(stored);
        var actual = Convert.FromHexString(HashRecoveryAnswer(answer));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            return new(false, "A resposta de segurança está incorreta.");
        await ExecuteAsync(db, """
            UPDATE Usuarios SET senha_hash=@hash,senha_alterada_em=CURRENT_TIMESTAMP,
              tentativas_falhas=0,bloqueado_ate=NULL
            WHERE (lower(nome)=lower(@user) OR lower(email)=lower(@user))
            """, ("@hash", HashPassword(newPassword)), ("@user", user.Trim()));
        return new(true, "Senha alterada com sucesso.");
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var value = await SecureStorage.Default.GetAsync(SessionKey);
        if (!long.TryParse(value, out var id))
            return false;
        await using var db = Connection();
        await db.OpenAsync();
        var name = Convert.ToString(await ScalarAsync(db, "SELECT nome FROM Usuarios WHERE id_usuario=@id AND ativo=1", ("@id", id)));
        if (string.IsNullOrWhiteSpace(name))
        {
            SecureStorage.Default.Remove(SessionKey);
            return false;
        }
        SetSession(id, name);
        return true;
    }

    public async Task ActivateLocalUserAsync()
    {
        await using var db = Connection();
        await db.OpenAsync();
        var name = Convert.ToString(await ScalarAsync(db,
            "SELECT nome FROM Usuarios WHERE id_usuario=1 AND ativo=1"));
        SetSession(1, string.IsNullOrWhiteSpace(name) ? "Usuário local" : name);
    }

    public async Task<(string Name, string Email, DateTime? BirthDate, string? Question)> GetProfileAsync()
    {
        await using var db = Connection();
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = "SELECT nome,email,data_nascimento,pergunta_recuperacao FROM Usuarios WHERE id_usuario=@id";
        command.Parameters.AddWithValue("@id", UserId ?? 1);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException("Usuário local não encontrado.");
        return (reader.GetString(0), reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetDateTime(2),
            reader.IsDBNull(3) ? null : reader.GetString(3));
    }

    public async Task<AuthResult> UpdateProfileAsync(string name, string email, DateTime birthDate,
        string question, string? newRecoveryAnswer)
    {
        if (name.Trim().Length < 3)
            return new(false, "Informe o nome completo.");
        if (!email.Contains('@') || !email.Contains('.'))
            return new(false, "Informe um email válido.");
        if (birthDate > DateTime.Today.AddYears(-12))
            return new(false, "Informe uma data de nascimento válida.");
        await using var db = Connection();
        await db.OpenAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(newRecoveryAnswer))
                await ExecuteAsync(db, """
                    UPDATE Usuarios SET nome=@name,email=@email,data_nascimento=@birth,
                      pergunta_recuperacao=@question WHERE id_usuario=@id
                    """, ("@name", name.Trim()), ("@email", email.Trim().ToLowerInvariant()),
                    ("@birth", birthDate.Date), ("@question", question), ("@id", UserId ?? 1));
            else
                await ExecuteAsync(db, """
                    UPDATE Usuarios SET nome=@name,email=@email,data_nascimento=@birth,
                      pergunta_recuperacao=@question,resposta_recuperacao_hash=@answer,
                      palavra_chave_hash=@answer,dica_palavra_chave=@question WHERE id_usuario=@id
                    """, ("@name", name.Trim()), ("@email", email.Trim().ToLowerInvariant()),
                    ("@birth", birthDate.Date), ("@question", question),
                    ("@answer", HashRecoveryAnswer(newRecoveryAnswer)), ("@id", UserId ?? 1));
            UserName = name.Trim();
            return new(true, "Perfil atualizado.");
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { return new(false, "Este email já está em uso."); }
    }

    public void Logout()
    {
        SecureStorage.Default.Remove(SessionKey);
        UserId = null;
        UserName = null;
    }

    private void SetSession(long id, string name)
    {
        UserId = id;
        UserName = name;
        database.CurrentUserId = id;
    }

    private async Task UpgradeDemoPasswordAsync()
    {
        await using var db = Connection();
        await db.OpenAsync();
        await ExecuteAsync(db, """
            UPDATE Usuarios SET senha_hash=@hash,senha_alterada_em=CURRENT_TIMESTAMP
            WHERE email='demo@money.local' AND senha_hash='LOCAL_DESKTOP_ACCOUNT'
            """, ("@hash", HashPassword("Money123")));
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2-SHA256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string encoded)
    {
        var parts = encoded.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
            return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }

    private static string HashRecoveryAnswer(string answer)
    {
        var normalized = string.Join(' ', answer.Trim().ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(normalized)));
    }

    private static async Task CopyDefaultCategoriesAsync(SqliteConnection db, long id)
    {
        await ExecuteAsync(db, """
            INSERT INTO Categorias(id_usuario,nome_categoria,tipo,cor,icone)
            SELECT @id,nome_categoria,tipo,cor,icone FROM Categorias
            WHERE id_usuario=1 AND id_categoria_pai IS NULL
            """, ("@id", id));
    }

    private static Task UpdateAttemptsAsync(SqliteConnection db, long id, int attempts, DateTime? block)
        => ExecuteAsync(db, "UPDATE Usuarios SET tentativas_falhas=@attempts,bloqueado_ate=@block WHERE id_usuario=@id",
            ("@attempts", attempts), ("@block", block), ("@id", id));

    private SqliteConnection Connection() => new($"Data Source={database.DatabasePath};Foreign Keys=True");
    private static async Task ExecuteAsync(SqliteConnection db, string sql, params (string, object?)[] args)
    {
        await using var command = db.CreateCommand();
        command.CommandText = sql;
        foreach (var (key, value) in args)
            command.Parameters.AddWithValue(key, value ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }
    private static async Task<object?> ScalarAsync(SqliteConnection db, string sql, params (string, object?)[] args)
    {
        await using var command = db.CreateCommand();
        command.CommandText = sql;
        foreach (var (key, value) in args)
            command.Parameters.AddWithValue(key, value ?? DBNull.Value);
        return await command.ExecuteScalarAsync();
    }
}
