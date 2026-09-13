using Microsoft.Data.Sqlite;
using Money.Models;
using MauiIcons.Material;

namespace Money.Services;

public sealed partial class DatabaseService
{
    private static async Task<List<(long Id, long User, long? Main, string Name, string Type, string Color, string? Icon, bool Active)>> ReadCategoryRowsAsync(SqliteConnection db, bool modern)
    {
        var rows = new List<(long, long, long?, string, string, string, string?, bool)>();
        await using var cmd = CreateCommand(db, $"SELECT id_categoria,id_usuario,{(modern ? "id_categoria_pilar" : "NULL")},nome_categoria,tipo,COALESCE(cor,''),icone,COALESCE(ativo,1) FROM Categorias");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) rows.Add((reader.GetInt64(0), reader.GetInt64(1), reader.IsDBNull(2) ? null : reader.GetInt64(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetInt64(7) != 0));
        return rows;
    }

    private static async Task MigrateCategories29Async(SqliteConnection db)
    {
        if (Convert.ToInt32(await ScalarAsync(db, "PRAGMA user_version")) >= 29) return;
        var modern = Convert.ToInt32(await ScalarAsync(db, "SELECT COUNT(*) FROM pragma_table_info('Categorias') WHERE name='id_categoria_pilar'")) == 1;
        var rows = await ReadCategoryRowsAsync(db, modern);
        var objects = new List<(string Type, string Name, string Sql)>();
        await using (var cmd = CreateCommand(db, "SELECT type,name,sql FROM sqlite_master WHERE (type='view' OR type='trigger') AND sql IS NOT NULL"))
        await using (var reader = await cmd.ExecuteReaderAsync())
            while (await reader.ReadAsync()) objects.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        await ExecuteAsync(db, "PRAGMA foreign_keys=OFF;");
        await using var transaction = db.BeginTransaction();
        try
        {
            foreach (var item in objects) await ExecuteAsync(db, $"DROP {item.Type} \"{item.Name.Replace("\"", "\"\"")}\";");
            await ExecuteAsync(db, """
                CREATE TABLE CategoriaPilar29(
                  id_categoria_pilar INTEGER PRIMARY KEY AUTOINCREMENT,
                  id_usuario INTEGER NOT NULL REFERENCES Usuarios(id_usuario),
                  nome TEXT NOT NULL CHECK(length(trim(nome))>=2), nome_normalizado TEXT NOT NULL,
                  tipo TEXT NOT NULL CHECK(tipo IN('receita','despesa')), icone TEXT,cor TEXT,
                  ordem INTEGER NOT NULL DEFAULT 0,ativo INTEGER NOT NULL DEFAULT 1 CHECK(ativo IN(0,1)),
                  data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  UNIQUE(id_usuario,tipo,nome_normalizado));
                """);
            if (modern)
            {
                await ExecuteAsync(db, "INSERT INTO CategoriaPilar29 SELECT * FROM CategoriaPilar;");
                await ExecuteAsync(db, "DROP TABLE CategoriaPilar;");
            }
            await ExecuteAsync(db, "ALTER TABLE CategoriaPilar29 RENAME TO CategoriaPilar;");
            if (!modern)
            {
                // Full legacy snapshot preserves original parent relationships and IDs.
                await ExecuteAsync(db, "CREATE TABLE IF NOT EXISTS CategoryLegacyMobile29 AS SELECT * FROM Categorias;");
                foreach (var user in rows.Select(x => x.User).Distinct())
                    foreach (var seed in CategoryCatalog.All)
                        await InsertMainSeedAsync(db, user, seed);
            }
            await ExecuteAsync(db, """
                CREATE TABLE Categorias29(
                  id_categoria INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL REFERENCES Usuarios(id_usuario),
                  id_categoria_pilar INTEGER NOT NULL REFERENCES CategoriaPilar(id_categoria_pilar) ON DELETE RESTRICT,
                  nome_categoria TEXT NOT NULL CHECK(length(trim(nome_categoria))>=2),nome_normalizado TEXT NOT NULL,
                  tipo TEXT NOT NULL CHECK(tipo IN('receita','despesa')),cor TEXT,icone TEXT,
                  ativo INTEGER NOT NULL DEFAULT 1 CHECK(ativo IN(0,1)),data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,UNIQUE(id_usuario,id_categoria_pilar,nome_normalizado));
                """);
            foreach (var row in rows)
            {
                var mainId = row.Main;
                if (mainId is null)
                {
                    var seed = CategoryCatalog.All.FirstOrDefault(x => x.Type == row.Type && x.Children.Any(c => CategoryCatalog.Normalize(c.Name) == CategoryCatalog.Normalize(row.Name)))
                        ?? CategoryCatalog.All.Single(x => x.Name == (row.Type == "receita" ? "Receitas" : "Outros"));
                    mainId = Convert.ToInt64(await ScalarAsync(db, "SELECT id_categoria_pilar FROM CategoriaPilar WHERE id_usuario=@u AND nome_normalizado=@n AND tipo=@t", ("@u",row.User),("@n",CategoryCatalog.Normalize(seed.Name)),("@t",row.Type)));
                }
                await ExecuteAsync(db, $"""
                    INSERT INTO Categorias29(id_categoria,id_usuario,id_categoria_pilar,nome_categoria,nome_normalizado,tipo,cor,icone,ativo,data_criacao,data_atualizacao)
                    SELECT @id,@u,@m,@n,@norm,@t,@c,@i,@a,data_criacao,{(modern ? "data_atualizacao" : "data_criacao")} FROM Categorias WHERE id_categoria=@id
                    """, ("@id",row.Id),("@u",row.User),("@m",mainId),("@n",row.Name),("@norm",CategoryCatalog.Normalize(row.Name)),("@t",row.Type),("@c",row.Color),("@i",row.Icon),("@a",row.Active?1:0));
            }
            await ExecuteAsync(db, "DROP TABLE Categorias; ALTER TABLE Categorias29 RENAME TO Categorias;");
            await ExecuteAsync(db, """
                CREATE TABLE IF NOT EXISTS Orcamentos(
                  id_orcamento INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL REFERENCES Usuarios(id_usuario),
                  id_categoria INTEGER NOT NULL REFERENCES Categorias(id_categoria),valor_limite NUMERIC NOT NULL,
                  mes INTEGER NOT NULL CHECK(mes BETWEEN 1 AND 12),ano INTEGER NOT NULL,valor_realizado NUMERIC DEFAULT 0,
                  observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  UNIQUE(id_usuario,id_categoria,mes,ano));
                CREATE TABLE IF NOT EXISTS CategoriaPilarOrcamentario(
                  id_usuario INTEGER NOT NULL REFERENCES Usuarios(id_usuario),
                  id_subcategoria INTEGER NOT NULL REFERENCES Categorias(id_categoria) ON DELETE RESTRICT,
                  pilar TEXT NOT NULL CHECK(pilar IN('FIXAS_ESSENCIAIS','VARIAVEIS_LAZER','RESERVA_EMERGENCIA','INVESTIMENTOS')),
                  PRIMARY KEY(id_usuario,id_subcategoria));
                CREATE TABLE IF NOT EXISTS CategoryCatalogRepair29(id_usuario INTEGER PRIMARY KEY REFERENCES Usuarios(id_usuario),data TEXT DEFAULT CURRENT_TIMESTAMP);
                CREATE INDEX idx_subcategorias_usuario_tipo_ativo ON Categorias(id_usuario,tipo,ativo,id_categoria_pilar);
                CREATE INDEX IF NOT EXISTS idx_transacoes_categoria_usuario ON Transacoes(id_usuario,id_categoria);
                """);
            var mainRows = new List<(long Id,string Name)>();
            await using (var cmd = CreateCommand(db,"SELECT id_categoria_pilar,nome FROM CategoriaPilar"))
            await using (var reader = await cmd.ExecuteReaderAsync())
                while(await reader.ReadAsync()) mainRows.Add((reader.GetInt64(0),reader.GetString(1)));
            foreach(var row in mainRows)
                await ExecuteAsync(db,"UPDATE CategoriaPilar SET nome_normalizado=@n WHERE id_categoria_pilar=@id",("@n",CategoryCatalog.Normalize(row.Name)),("@id",row.Id));
            // Preserve prior trigger rules and historical log entries; only new events change origin.
            foreach (var item in objects.Where(x => x.Type == "trigger"))
                await ExecuteAsync(db, item.Sql.Replace("Money.Web/CRUD", "Money.Mobile/CRUD"));
            foreach (var item in objects.Where(x => x.Type == "view")) await ExecuteAsync(db, item.Sql);
            await CreateCategoryGuardsAsync(db);
            var users = new List<long>();
            await using (var cmd = CreateCommand(db,"SELECT id_usuario FROM Usuarios"))
            await using (var reader = await cmd.ExecuteReaderAsync())
                while(await reader.ReadAsync()) users.Add(reader.GetInt64(0));
            foreach(var user in users) await SeedCategoryCatalogAsync(db,user);
            await ExecuteAsync(db,"PRAGMA user_version=29;");
            if (!string.Equals(Convert.ToString(await ScalarAsync(db,"PRAGMA integrity_check")),"ok",StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Falha de integridade na migração de categorias.");
            await using(var cmd=CreateCommand(db,"PRAGMA foreign_key_check"))
            await using(var reader=await cmd.ExecuteReaderAsync())
                if(await reader.ReadAsync()) throw new InvalidDataException("A migração deixaria referências inválidas.");
            transaction.Commit();
        }
        catch { transaction.Rollback(); throw; }
        finally { await ExecuteAsync(db,"PRAGMA foreign_keys=ON;"); }
    }

    private static async Task InsertMainSeedAsync(SqliteConnection db,long user,CategorySeed seed)
    {
        // Matching is normalized by the migration before seeding.
        await ExecuteAsync(db,"""
            INSERT OR IGNORE INTO CategoriaPilar(id_usuario,nome,nome_normalizado,tipo,icone,cor,ordem)
            VALUES(@u,@n,@norm,@t,@i,'#00875A',@o)
            """,("@u",user),("@n",seed.Name),("@norm",CategoryCatalog.Normalize(seed.Name)),("@t",seed.Type),("@i",seed.Icon),("@o",seed.Order));
    }
    internal static async Task SeedCategoryCatalogAsync(SqliteConnection db,long user)
    {
        foreach(var seed in CategoryCatalog.All)
        {
            await InsertMainSeedAsync(db,user,seed);
            var main=Convert.ToInt64(await ScalarAsync(db,"SELECT id_categoria_pilar FROM CategoriaPilar WHERE id_usuario=@u AND tipo=@t AND nome_normalizado=@n",("@u",user),("@t",seed.Type),("@n",CategoryCatalog.Normalize(seed.Name))));
            var mainIcon=Convert.ToString(await ScalarAsync(db,"SELECT icone FROM CategoriaPilar WHERE id_categoria_pilar=@m",("@m",main)));
            if(IsLegacyIcon(mainIcon)) await ExecuteAsync(db,"UPDATE CategoriaPilar SET icone=@i WHERE id_categoria_pilar=@m",("@i",seed.Icon),("@m",main));
            foreach(var child in seed.Children)
            {
                await ExecuteAsync(db,"""
                    INSERT OR IGNORE INTO Categorias(id_usuario,id_categoria_pilar,nome_categoria,nome_normalizado,tipo,icone,cor)
                    VALUES(@u,@m,@n,@norm,@t,@i,NULL)
                    """,("@u",user),("@m",main),("@n",child.Name),("@norm",CategoryCatalog.Normalize(child.Name)),("@t",seed.Type),("@i",child.Icon));
                var id=Convert.ToInt64(await ScalarAsync(db,"SELECT id_categoria FROM Categorias WHERE id_usuario=@u AND id_categoria_pilar=@m AND nome_normalizado=@n",("@u",user),("@m",main),("@n",CategoryCatalog.Normalize(child.Name))));
                var icon=Convert.ToString(await ScalarAsync(db,"SELECT icone FROM Categorias WHERE id_categoria=@id",("@id",id)));
                if(IsLegacyIcon(icon)) await ExecuteAsync(db,"UPDATE Categorias SET icone=@i WHERE id_categoria=@id",("@i",child.Icon),("@id",id));
                if(child.Pillar is not null) await ExecuteAsync(db,"INSERT OR IGNORE INTO CategoriaPilarOrcamentario VALUES(@u,@id,@p)",("@u",user),("@id",id),("@p",child.Pillar));
            }
        }
        // Legacy custom icons get a deterministic valid identifier; existing Material customizations stay intact.
        var customs=new List<(long Id,string Type,string? Icon)>();
        await using(var cmd=CreateCommand(db,"SELECT id_categoria,tipo,icone FROM Categorias WHERE id_usuario=@u",("@u",user)))
        await using(var reader=await cmd.ExecuteReaderAsync()) while(await reader.ReadAsync()) customs.Add((reader.GetInt64(0),reader.GetString(1),reader.IsDBNull(2)?null:reader.GetString(2)));
        foreach(var item in customs.Where(x=>IsLegacyIcon(x.Icon)))
            await ExecuteAsync(db,"UPDATE Categorias SET icone=@i WHERE id_categoria=@id",("@i",item.Type=="receita"?"Payments":"Sell"),("@id",item.Id));
        await ExecuteAsync(db,"INSERT OR IGNORE INTO CategoryCatalogRepair29(id_usuario) VALUES(@u)",("@u",user));
    }
    private static bool IsLegacyIcon(string? icon) => string.IsNullOrWhiteSpace(icon) || !Enum.TryParse<MaterialIcons>(icon,false,out var parsed) || !Enum.IsDefined(parsed);

    private static async Task CreateCategoryGuardsAsync(SqliteConnection db)
    {
        foreach(var operation in new[]{"INSERT","UPDATE"})
        {
            var changed = operation == "INSERT" ? "1=1" : "NEW.id_categoria<>OLD.id_categoria";
            await ExecuteAsync(db,$"""
                CREATE TRIGGER IF NOT EXISTS mobile29_transaction_active_{operation} BEFORE {operation} ON Transacoes
                WHEN ({changed}) AND NEW.tipo IN('receita','despesa') AND NOT EXISTS(
                  SELECT 1 FROM Categorias c JOIN CategoriaPilar p ON p.id_categoria_pilar=c.id_categoria_pilar
                  WHERE c.id_categoria=NEW.id_categoria AND c.id_usuario=NEW.id_usuario AND p.id_usuario=NEW.id_usuario
                    AND c.ativo=1 AND p.ativo=1)
                BEGIN SELECT RAISE(ABORT,'Selecione uma categoria e subcategoria ativas.'); END;
                CREATE TRIGGER IF NOT EXISTS mobile29_subcategory_{operation} BEFORE {operation} ON Categorias
                WHEN NOT EXISTS(SELECT 1 FROM CategoriaPilar p WHERE p.id_categoria_pilar=NEW.id_categoria_pilar AND p.id_usuario=NEW.id_usuario AND p.tipo=NEW.tipo)
                BEGIN SELECT RAISE(ABORT,'Categoria principal incompatível ou de outro usuário.'); END;
                CREATE TRIGGER IF NOT EXISTS mobile29_pillar_{operation} BEFORE {operation} ON CategoriaPilarOrcamentario
                WHEN NOT EXISTS(SELECT 1 FROM Categorias c WHERE c.id_categoria=NEW.id_subcategoria AND c.id_usuario=NEW.id_usuario AND c.tipo='despesa')
                BEGIN SELECT RAISE(ABORT,'Subcategoria inválida para o pilar orçamentário.'); END;
                CREATE TRIGGER IF NOT EXISTS mobile29_transaction_{operation} BEFORE {operation} ON Transacoes
                WHEN NEW.tipo IN('receita','despesa') AND NOT EXISTS(SELECT 1 FROM Categorias c JOIN CategoriaPilar p ON p.id_categoria_pilar=c.id_categoria_pilar AND p.id_usuario=c.id_usuario
                  WHERE c.id_categoria=NEW.id_categoria AND c.id_usuario=NEW.id_usuario AND c.tipo=NEW.tipo AND p.tipo=NEW.tipo)
                BEGIN SELECT RAISE(ABORT,'Subcategoria inválida para este lançamento.'); END;
                """);
        }
        await ExecuteAsync(db,"""
            CREATE TRIGGER IF NOT EXISTS mobile29_main_type BEFORE UPDATE OF tipo,id_usuario ON CategoriaPilar
            WHEN EXISTS(SELECT 1 FROM Categorias c WHERE c.id_categoria_pilar=OLD.id_categoria_pilar AND (c.tipo<>NEW.tipo OR c.id_usuario<>NEW.id_usuario))
            BEGIN SELECT RAISE(ABORT,'Categoria possui subcategorias de outro tipo.'); END;
            CREATE TRIGGER IF NOT EXISTS mobile29_sub_type BEFORE UPDATE OF tipo,id_usuario ON Categorias
            WHEN (NEW.tipo<>OLD.tipo OR NEW.id_usuario<>OLD.id_usuario) AND
              (EXISTS(SELECT 1 FROM Transacoes t WHERE t.id_categoria=OLD.id_categoria) OR EXISTS(SELECT 1 FROM Orcamentos o WHERE o.id_categoria=OLD.id_categoria))
            BEGIN SELECT RAISE(ABORT,'Não é possível alterar o tipo de uma subcategoria com histórico.'); END;
            """);
        foreach(var (table,id,entity) in new[]{("CategoriaPilar","id_categoria_pilar","CATEGORIA"),("Categorias","id_categoria","SUBCATEGORIA")})
        {
            if(Convert.ToInt32(await ScalarAsync(db,"SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND tbl_name=@t AND sql LIKE '%INSERT INTO Logs%'",("@t",table)))>0) continue;
            foreach(var operation in new[]{"INSERT","UPDATE","DELETE"})
            {
                var prefix=operation=="DELETE"?"OLD":"NEW";
                var action=operation=="INSERT"?$"'CRIAR_{entity}'":operation=="DELETE"?$"'EXCLUIR_{entity}'":$"CASE WHEN OLD.ativo=1 AND NEW.ativo=0 THEN 'DESATIVAR_{entity}' ELSE 'EDITAR_{entity}' END";
                await ExecuteAsync(db,$"CREATE TRIGGER mobile29_log_{table}_{operation} AFTER {operation} ON {table} BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,origem) VALUES({prefix}.id_usuario,{action},'{table}',{prefix}.{id},'Money.Mobile/CRUD'); END;");
            }
        }
    }
}
