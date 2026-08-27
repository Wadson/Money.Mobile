using Microsoft.Data.Sqlite;
using Money.Models;

namespace Money.Services;

/// <summary>Manages the local SQLite database and all financial operations.</summary>
public sealed class DatabaseService
{
    private static readonly SemaphoreSlim InitializationGate = new(1, 1);
    private readonly string _path = Path.Combine(FileSystem.AppDataDirectory, "financeiro.db");
    private string ConnectionString => $"Data Source={_path};Foreign Keys=True";
    public string DatabasePath => _path;
    public long CurrentUserId { get; set; } = 1;

    /// <summary>Creates and migrates the database on first use.</summary>
    public async Task InitializeAsync()
    {
        await InitializationGate.WaitAsync();
        try
        {
        SQLitePCL.Batteries_V2.Init();
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await ExecuteAsync(db, "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;");
        await ExecuteAsync(db, Schema);
        var version = Convert.ToInt32(await ScalarAsync(db, "PRAGMA user_version;") ?? 0);
        if (version < 1)
        {
            await SeedAsync(db);
            await ExecuteAsync(db, "PRAGMA user_version=1;");
        }
        if (version < 2)
        {
            await ExecuteAsync(db, """
                ALTER TABLE Usuarios ADD COLUMN tentativas_falhas INTEGER NOT NULL DEFAULT 0;
                ALTER TABLE Usuarios ADD COLUMN bloqueado_ate TEXT;
                ALTER TABLE Usuarios ADD COLUMN senha_alterada_em TEXT;
                CREATE TABLE IF NOT EXISTS Importacoes(
                  id_importacao INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,nome_arquivo TEXT NOT NULL,
                  formato TEXT NOT NULL,total_registros INTEGER NOT NULL DEFAULT 0,duplicados INTEGER NOT NULL DEFAULT 0,
                  data_importacao TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
                CREATE TABLE IF NOT EXISTS Notificacoes(
                  id_notificacao INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,titulo TEXT NOT NULL,
                  mensagem TEXT NOT NULL,nivel TEXT NOT NULL DEFAULT 'info',lida INTEGER NOT NULL DEFAULT 0,
                  data_criacao TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
                CREATE INDEX IF NOT EXISTS idx_importacoes_usuario ON Importacoes(id_usuario,data_importacao);
                CREATE INDEX IF NOT EXISTS idx_notificacoes_usuario ON Notificacoes(id_usuario,lida,data_criacao);
                PRAGMA user_version=2;
                """);
        }
        if (version < 3)
        {
            await ExecuteAsync(db, """
                ALTER TABLE Usuarios ADD COLUMN data_nascimento TEXT;
                ALTER TABLE Usuarios ADD COLUMN pergunta_recuperacao TEXT;
                ALTER TABLE Usuarios ADD COLUMN resposta_recuperacao_hash TEXT;
                UPDATE Usuarios SET data_nascimento='1990-01-01',
                  pergunta_recuperacao='Cidade onde nasceu?',
                  resposta_recuperacao_hash='5A87DDB783507E6928C71FA58DC76BBA267FF3D23F8D05CA1A80F409E58F5659'
                WHERE email='demo@money.local' AND data_nascimento IS NULL;
                PRAGMA user_version=3;
                """);
        }
        if (version < 4)
        {
            await ExecuteAsync(db, """
                ALTER TABLE Transacoes ADD COLUMN pago INTEGER NOT NULL DEFAULT 0;
                ALTER TABLE Transacoes ADD COLUMN data_pagamento TEXT;
                UPDATE Transacoes SET pago=1, data_pagamento=COALESCE(data_pagamento,data)
                WHERE tipo='despesa';
                CREATE INDEX IF NOT EXISTS idx_transacoes_pagar
                  ON Transacoes(id_usuario,tipo,pago,data);
                PRAGMA user_version=4;
                """);
        }
        if (version < 5)
        {
            // Remove exclusivamente os registros demonstrativos criados pelo seed
            // original e neutraliza seus reflexos nos saldos.
            await ExecuteAsync(db, """
                DELETE FROM Transacoes
                WHERE id_usuario=1 AND (
                  (descricao='Salário mensal' AND valor=7200) OR
                  (descricao='Supermercado' AND valor=460) OR
                  (descricao='Aluguel' AND valor=1450)
                );
                DELETE FROM Orcamentos WHERE id_usuario=1 AND valor_limite IN(1200,2000);
                UPDATE Contas SET saldo_inicial=0,
                  saldo_atual=MAX(0,saldo_atual-saldo_inicial-5290)
                WHERE id_usuario=1 AND id_conta=1 AND saldo_inicial=8500;
                UPDATE Contas SET saldo_inicial=0,
                  saldo_atual=MAX(0,saldo_atual-saldo_inicial)
                WHERE id_usuario=1 AND id_conta=2 AND saldo_inicial=12000;
                UPDATE CartoesCredito SET limite_utilizado=MAX(0,limite_utilizado-1840)
                WHERE id_usuario=1 AND id_cartao=1 AND nome_cartao='Cartão Platinum';
                PRAGMA user_version=5;
                """);
        }
        if (version < 6)
        {
            await ExecuteAsync(db, """
                UPDATE Transacoes
                SET pago=0,data_pagamento=NULL,data_atualizacao=CURRENT_TIMESTAMP
                WHERE tipo='despesa' AND parcelado=1 AND id_transacao_pai IS NOT NULL
                  AND pago=1 AND date(data_pagamento)=date(data)
                  AND NOT EXISTS (
                    SELECT 1 FROM Logs l
                    WHERE l.tabela='Transacoes'
                      AND l.registro_id=Transacoes.id_transacao
                      AND l.acao='PAGAR'
                  );
                PRAGMA user_version=6;
                """);
        }
        if (version < 7)
        {
            await EnsureColumnAsync(db, "Contas", "cor", "TEXT DEFAULT '#00A859'");
            await EnsureColumnAsync(db, "CartoesCredito", "cor", "TEXT DEFAULT '#00A859'");
            await EnsureColumnAsync(db, "Transacoes", "pago", "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync(db, "Transacoes", "data_pagamento", "TEXT");
            await ExecuteAsync(db, "PRAGMA foreign_keys=OFF;");
            await ExecuteAsync(db, """
                ALTER TABLE Contas ADD COLUMN icone TEXT DEFAULT 'AccountBalance';

                ALTER TABLE Transacoes RENAME TO Transacoes_legacy;
                CREATE TABLE Transacoes(
                  id_transacao INTEGER PRIMARY KEY AUTOINCREMENT,
                  id_conta INTEGER,id_cartao INTEGER,id_categoria INTEGER,id_usuario INTEGER NOT NULL,
                  valor REAL NOT NULL,data TEXT NOT NULL,descricao TEXT NOT NULL,tipo TEXT NOT NULL,
                  parcelado INTEGER DEFAULT 0,numero_parcelas INTEGER,id_transacao_pai INTEGER,
                  observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,id_fatura INTEGER,
                  id_conta_destino INTEGER,data_vencimento TEXT,pago INTEGER NOT NULL DEFAULT 0,
                  data_pagamento TEXT,numero_parcela INTEGER DEFAULT 1,total_parcelas INTEGER DEFAULT 1,
                  recorrente INTEGER DEFAULT 0,frequencia TEXT,
                  FOREIGN KEY(id_conta) REFERENCES Contas(id_conta) ON DELETE SET NULL,
                  FOREIGN KEY(id_cartao) REFERENCES CartoesCredito(id_cartao) ON DELETE SET NULL,
                  FOREIGN KEY(id_categoria) REFERENCES Categorias(id_categoria) ON DELETE RESTRICT,
                  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,
                  FOREIGN KEY(id_transacao_pai) REFERENCES Transacoes(id_transacao) ON DELETE CASCADE,
                  FOREIGN KEY(id_conta_destino) REFERENCES Contas(id_conta) ON DELETE SET NULL
                );
                INSERT INTO Transacoes(
                  id_transacao,id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                  parcelado,numero_parcelas,id_transacao_pai,observacoes,data_criacao,data_atualizacao,
                  data_vencimento,pago,data_pagamento,numero_parcela,total_parcelas,recorrente)
                SELECT id_transacao,id_conta,id_cartao,id_categoria,id_usuario,CAST(valor AS REAL),
                  date(data),descricao,tipo,COALESCE(parcelado,0),numero_parcelas,id_transacao_pai,
                  observacoes,data_criacao,data_atualizacao,
                  CASE WHEN tipo='despesa' THEN date(data) ELSE NULL END,
                  CASE WHEN tipo='receita' THEN 1 ELSE COALESCE(pago,0) END,data_pagamento,
                  1,COALESCE(numero_parcelas,1),0
                FROM Transacoes_legacy;
                DROP TABLE Transacoes_legacy;

                CREATE TABLE IF NOT EXISTS Tags(
                  id_tag INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,
                  nome_tag TEXT NOT NULL,cor TEXT DEFAULT '#00A859',
                  data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,
                  UNIQUE(id_usuario,nome_tag));
                CREATE TABLE IF NOT EXISTS Metas(
                  id_meta INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,
                  nome_meta TEXT NOT NULL,valor_objetivo REAL NOT NULL,valor_atual REAL DEFAULT 0,
                  data_inicio TEXT NOT NULL,data_meta TEXT NOT NULL,categoria TEXT,
                  prioridade TEXT DEFAULT 'media',status TEXT DEFAULT 'em_andamento',
                  observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE);
                CREATE TABLE IF NOT EXISTS Lembretes(
                  id_lembrete INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,
                  id_transacao INTEGER,titulo TEXT NOT NULL,descricao TEXT,data_lembrete TEXT NOT NULL,
                  status TEXT DEFAULT 'pendente',data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,
                  FOREIGN KEY(id_transacao) REFERENCES Transacoes(id_transacao) ON DELETE SET NULL);
                CREATE TABLE IF NOT EXISTS TransacoesTags(
                  id_transacao INTEGER NOT NULL,id_tag INTEGER NOT NULL,
                  PRIMARY KEY(id_transacao,id_tag),
                  FOREIGN KEY(id_transacao) REFERENCES Transacoes(id_transacao) ON DELETE CASCADE,
                  FOREIGN KEY(id_tag) REFERENCES Tags(id_tag) ON DELETE CASCADE);
                CREATE TABLE IF NOT EXISTS FaturasCartao(
                  id_fatura INTEGER PRIMARY KEY AUTOINCREMENT,id_cartao INTEGER NOT NULL,
                  mes_referencia INTEGER NOT NULL,ano_referencia INTEGER NOT NULL,
                  valor_total REAL NOT NULL DEFAULT 0,data_vencimento TEXT NOT NULL,
                  status TEXT NOT NULL DEFAULT 'aberta',data_pagamento TEXT,
                  FOREIGN KEY(id_cartao) REFERENCES CartoesCredito(id_cartao) ON DELETE CASCADE,
                  UNIQUE(id_cartao,mes_referencia,ano_referencia));

                CREATE UNIQUE INDEX IF NOT EXISTS ux_usuarios_email ON Usuarios(email);
                CREATE INDEX IF NOT EXISTS idx_tags_usuario ON Tags(id_usuario);
                CREATE INDEX IF NOT EXISTS idx_metas_status_data ON Metas(id_usuario,status,data_meta);
                CREATE INDEX IF NOT EXISTS idx_categorias_tipo ON Categorias(id_usuario,tipo);
                CREATE INDEX IF NOT EXISTS idx_orcamentos_usuario_data ON Orcamentos(id_usuario,ano,mes,id_categoria);
                CREATE INDEX IF NOT EXISTS idx_cartoes_usuario_ativo ON CartoesCredito(id_usuario,ativo);
                CREATE INDEX IF NOT EXISTS idx_transacoes_usuario_data ON Transacoes(id_usuario,data);
                CREATE INDEX IF NOT EXISTS idx_transacoes_fatura ON Transacoes(id_fatura);
                CREATE INDEX IF NOT EXISTS idx_transacoes_dashboard ON Transacoes(id_usuario,data,tipo,pago);
                CREATE INDEX IF NOT EXISTS idx_lembretes_data ON Lembretes(id_usuario,data_lembrete,status);
                CREATE INDEX IF NOT EXISTS idx_faturas_cartao ON FaturasCartao(id_cartao,ano_referencia,mes_referencia);
                CREATE INDEX IF NOT EXISTS idx_faturas_vencimento ON FaturasCartao(data_vencimento,status);

                DROP TRIGGER IF EXISTS trg_transacoes_updated;
                CREATE TRIGGER trg_transacoes_updated AFTER UPDATE ON Transacoes
                BEGIN UPDATE Transacoes SET data_atualizacao=CURRENT_TIMESTAMP
                  WHERE id_transacao=NEW.id_transacao; END;
                CREATE TRIGGER IF NOT EXISTS trg_contas_tipo_check BEFORE INSERT ON Contas
                WHEN LOWER(NEW.tipo_conta) NOT IN('carteira','corrente','poupanca','investimento')
                BEGIN SELECT RAISE(ABORT,'Tipo de conta inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_transacoes_tipo_check BEFORE INSERT ON Transacoes
                WHEN LOWER(NEW.tipo) NOT IN('receita','despesa','transferencia')
                  OR (LOWER(NEW.tipo)='transferencia' AND NEW.id_conta_destino IS NULL)
                  OR (LOWER(NEW.tipo) IN('receita','despesa') AND NEW.id_categoria IS NULL)
                BEGIN SELECT RAISE(ABORT,'Tipo ou relacionamento da transação inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_transacoes_valores BEFORE INSERT ON Transacoes
                WHEN NEW.valor<=0 OR (NEW.parcelado=1 AND
                  (NEW.total_parcelas<2 OR NEW.total_parcelas>36 OR NEW.numero_parcela>NEW.total_parcelas))
                BEGIN SELECT RAISE(ABORT,'Valor ou parcelamento inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_orcamentos_validacao BEFORE INSERT ON Orcamentos
                WHEN NEW.mes NOT BETWEEN 1 AND 12 OR NEW.ano NOT BETWEEN 2000 AND 2100
                  OR NEW.valor_limite<=0
                BEGIN SELECT RAISE(ABORT,'Período ou valor do orçamento inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_cartoes_validacao BEFORE INSERT ON CartoesCredito
                WHEN NEW.dia_fechamento NOT BETWEEN 1 AND 31
                  OR NEW.dia_vencimento NOT BETWEEN 1 AND 31 OR NEW.limite_credito<=0
                BEGIN SELECT RAISE(ABORT,'Dados do cartão inválidos.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_metas_validacao BEFORE INSERT ON Metas
                WHEN LOWER(NEW.prioridade) NOT IN('baixa','media','alta')
                  OR LOWER(NEW.status) NOT IN('em_andamento','concluida','cancelada')
                  OR NEW.valor_atual>NEW.valor_objetivo OR NEW.valor_objetivo<=0
                BEGIN SELECT RAISE(ABORT,'Dados da meta inválidos.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_lembretes_validacao BEFORE INSERT ON Lembretes
                WHEN LOWER(NEW.status) NOT IN('pendente','executado','cancelado')
                  OR date(NEW.data_lembrete)<date('now','localtime')
                BEGIN SELECT RAISE(ABORT,'Dados do lembrete inválidos.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_faturas_status_check BEFORE INSERT ON FaturasCartao
                WHEN LOWER(NEW.status) NOT IN('aberta','fechada','paga','vencida','cancelada')
                BEGIN SELECT RAISE(ABORT,'Status da fatura inválido.'); END;

                DROP VIEW IF EXISTS vw_resumo_mensal;
                CREATE VIEW vw_resumo_mensal AS
                SELECT id_usuario,
                  strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data END) mes_ano,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END) total_receitas,
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) total_despesas,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END)-
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) saldo_mes
                FROM Transacoes WHERE pago=1 AND tipo IN('receita','despesa')
                  AND NOT(parcelado=1 AND id_transacao_pai IS NULL)
                GROUP BY id_usuario,
                  strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data END);
                DROP VIEW IF EXISTS vw_gastos_categoria;
                CREATE VIEW vw_gastos_categoria AS
                SELECT t.id_usuario,c.nome_categoria,c.tipo,c.cor,
                  strftime('%Y-%m',COALESCE(t.data_vencimento,t.data)) mes_ano,SUM(t.valor) total
                FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria
                WHERE t.pago=1 AND t.tipo='despesa'
                  AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL)
                GROUP BY t.id_usuario,c.id_categoria,strftime('%Y-%m',COALESCE(t.data_vencimento,t.data));
                DROP VIEW IF EXISTS vw_saude_financeira;
                CREATE VIEW vw_saude_financeira AS
                SELECT id_usuario,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END) total_receitas,
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) total_despesas,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END)-
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) saldo_geral,
                  CASE WHEN SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END)=0 THEN 0
                    ELSE ROUND(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END)*100.0/
                    SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END),2) END percentual_gastos
                FROM Transacoes WHERE tipo IN('receita','despesa') GROUP BY id_usuario;
                DROP VIEW IF EXISTS vw_contas_pagar;
                CREATE VIEW vw_contas_pagar AS
                SELECT t.id_usuario,t.id_transacao,t.descricao,t.valor,
                  COALESCE(t.data_vencimento,t.data) data_vencimento,c.nome_categoria categoria,
                  CASE WHEN date(COALESCE(t.data_vencimento,t.data))<date('now','localtime') THEN 'vencida'
                    WHEN date(COALESCE(t.data_vencimento,t.data))=date('now','localtime') THEN 'hoje'
                    WHEN date(COALESCE(t.data_vencimento,t.data))<=date('now','localtime','+7 days')
                      THEN 'proxima_semana' ELSE 'futura' END status_vencimento
                FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria
                WHERE t.tipo='despesa' AND t.pago=0
                  AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL);
                DROP VIEW IF EXISTS vw_orcamento_mensal;
                CREATE VIEW vw_orcamento_mensal AS
                SELECT o.id_usuario,o.mes,o.ano,o.id_categoria,c.nome_categoria,c.cor,
                  o.valor_limite,COALESCE(SUM(t.valor),0) valor_realizado,
                  CASE WHEN o.valor_limite=0 THEN 0
                    ELSE ROUND(COALESCE(SUM(t.valor),0)*100.0/o.valor_limite,2) END percentual_utilizado,
                  CASE WHEN COALESCE(SUM(t.valor),0)>o.valor_limite THEN 'estourado'
                    WHEN COALESCE(SUM(t.valor),0)>o.valor_limite*.9 THEN 'atencao'
                    WHEN COALESCE(SUM(t.valor),0)>o.valor_limite*.75 THEN 'cuidado'
                    ELSE 'ok' END status
                FROM Orcamentos o JOIN Categorias c ON c.id_categoria=o.id_categoria
                LEFT JOIN Transacoes t ON t.id_categoria=o.id_categoria AND t.tipo='despesa'
                  AND t.pago=1 AND CAST(strftime('%m',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.mes
                  AND CAST(strftime('%Y',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.ano
                  AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL)
                GROUP BY o.id_orcamento;
                PRAGMA user_version=7;
                """);
            await ExecuteAsync(db, "PRAGMA foreign_keys=ON;");
        }
        if (version < 8)
        {
            await ExecuteAsync(db, """
                CREATE TRIGGER IF NOT EXISTS trg_contas_tipo_check_update BEFORE UPDATE ON Contas
                WHEN LOWER(NEW.tipo_conta) NOT IN('carteira','corrente','poupanca','investimento')
                BEGIN SELECT RAISE(ABORT,'Tipo de conta inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_transacoes_tipo_check_update BEFORE UPDATE ON Transacoes
                WHEN LOWER(NEW.tipo) NOT IN('receita','despesa','transferencia')
                  OR (LOWER(NEW.tipo)='transferencia' AND NEW.id_conta_destino IS NULL)
                  OR (LOWER(NEW.tipo) IN('receita','despesa') AND NEW.id_categoria IS NULL)
                BEGIN SELECT RAISE(ABORT,'Tipo ou relacionamento da transação inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_transacoes_valores_update BEFORE UPDATE ON Transacoes
                WHEN NEW.valor<=0 OR (NEW.parcelado=1 AND
                  (NEW.total_parcelas<2 OR NEW.total_parcelas>36
                   OR NEW.numero_parcela<1 OR NEW.numero_parcela>NEW.total_parcelas))
                BEGIN SELECT RAISE(ABORT,'Valor ou parcelamento inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_orcamentos_validacao_update BEFORE UPDATE ON Orcamentos
                WHEN NEW.mes NOT BETWEEN 1 AND 12 OR NEW.ano NOT BETWEEN 2000 AND 2100
                  OR NEW.valor_limite<=0
                BEGIN SELECT RAISE(ABORT,'Período ou valor do orçamento inválido.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_cartoes_validacao_update BEFORE UPDATE ON CartoesCredito
                WHEN NEW.dia_fechamento NOT BETWEEN 1 AND 31
                  OR NEW.dia_vencimento NOT BETWEEN 1 AND 31 OR NEW.limite_credito<=0
                BEGIN SELECT RAISE(ABORT,'Dados do cartão inválidos.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_metas_validacao_update BEFORE UPDATE ON Metas
                WHEN LOWER(NEW.prioridade) NOT IN('baixa','media','alta')
                  OR LOWER(NEW.status) NOT IN('em_andamento','concluida','cancelada')
                  OR NEW.valor_atual<0 OR NEW.valor_atual>NEW.valor_objetivo
                  OR NEW.valor_objetivo<=0 OR date(NEW.data_meta)<date(NEW.data_inicio)
                BEGIN SELECT RAISE(ABORT,'Dados da meta inválidos.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_lembretes_validacao_update BEFORE UPDATE ON Lembretes
                WHEN LOWER(NEW.status) NOT IN('pendente','executado','cancelado')
                  OR (LOWER(NEW.status)='pendente' AND date(NEW.data_lembrete)<date('now','localtime'))
                BEGIN SELECT RAISE(ABORT,'Dados do lembrete inválidos.'); END;
                CREATE TRIGGER IF NOT EXISTS trg_faturas_status_check_update BEFORE UPDATE ON FaturasCartao
                WHEN LOWER(NEW.status) NOT IN('aberta','fechada','paga','vencida','cancelada')
                BEGIN SELECT RAISE(ABORT,'Status da fatura inválido.'); END;
                PRAGMA user_version=8;
                """);
        }
        if (version < 9)
        {
            await EnsureColumnAsync(db, "Tags", "icone", "TEXT DEFAULT 'tag'");
            await ExecuteAsync(db, "PRAGMA user_version=9;");
        }
        if (version < 10)
        {
            // Normaliza dados persistidos para a paleta corporativa atual.
            await ExecuteAsync(db, "UPDATE Categorias SET cor=@warning WHERE tipo='despesa';",
                ("@warning", BlingPalette.HeaderDarkHex));
            await ExecuteAsync(db, "UPDATE Categorias SET cor=@primary WHERE tipo='receita';",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "UPDATE Contas SET cor=@primary;",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "UPDATE CartoesCredito SET cor=@primary;",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "UPDATE Tags SET cor=@primary;",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "PRAGMA user_version=10;");
        }
        if (version < 11)
        {
            // Remove dos dados existentes as cores de temas anteriores.
            await ExecuteAsync(db, "UPDATE Categorias SET cor=@expense WHERE tipo='despesa';",
                ("@expense", BlingPalette.HeaderDarkHex));
            await ExecuteAsync(db, "UPDATE Categorias SET cor=@primary WHERE tipo='receita';",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "UPDATE Contas SET cor=@primary;",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "UPDATE CartoesCredito SET cor=@primary;",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "UPDATE Tags SET cor=@primary;",
                ("@primary", BlingPalette.PrimaryHex));
            await ExecuteAsync(db, "PRAGMA user_version=11;");
        }
        if (version < 12)
        {
            await ExecuteAsync(db, """
                CREATE TABLE IF NOT EXISTS Fornecedores(
                  id_fornecedor INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                  id_usuario INTEGER NOT NULL,
                  nome_fornecedor TEXT NOT NULL,
                  ativo INTEGER DEFAULT 1,
                  data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE);
                CREATE INDEX IF NOT EXISTS idx_fornecedores_usuario_nome
                  ON Fornecedores(id_usuario,ativo,nome_fornecedor);
                """);
            await EnsureColumnAsync(db, "Transacoes", "id_fornecedor", "INTEGER");
            await ExecuteAsync(db, "CREATE INDEX IF NOT EXISTS idx_transacoes_fornecedor ON Transacoes(id_fornecedor);");
            await ExecuteAsync(db, "PRAGMA user_version=12;");
        }

        // Reparação idempotente para bancos restaurados que possuam user_version=12,
        // mas tenham sido produzidos antes da estrutura de fornecedores existir.
        if (version < 13)
        {
            // Competência civil: receitas usam a data do lançamento/recebimento;
            // despesas usam o vencimento, igual às telas de contas a pagar.
            await ExecuteAsync(db, """
                DROP VIEW IF EXISTS vw_resumo_mensal;
                CREATE VIEW vw_resumo_mensal AS
                SELECT id_usuario,
                  strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data END) mes_ano,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END) total_receitas,
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) total_despesas,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END)-
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) saldo_mes
                FROM Transacoes WHERE pago=1 AND tipo IN('receita','despesa')
                  AND NOT(parcelado=1 AND id_transacao_pai IS NULL)
                GROUP BY id_usuario,
                  strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data END);

                DROP VIEW IF EXISTS vw_gastos_categoria;
                CREATE VIEW vw_gastos_categoria AS
                SELECT t.id_usuario,c.nome_categoria,c.tipo,c.cor,
                  strftime('%Y-%m',COALESCE(t.data_vencimento,t.data)) mes_ano,SUM(t.valor) total
                FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria
                WHERE t.pago=1 AND t.tipo='despesa'
                  AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL)
                GROUP BY t.id_usuario,c.id_categoria,
                  strftime('%Y-%m',COALESCE(t.data_vencimento,t.data));
                PRAGMA user_version=13;
                """);
        }

        if (version < 14)
        {
            // O aplicativo trabalha somente com categorias simples. Preserva todos
            // os registros existentes e remove apenas os vínculos hierárquicos legados.
            await ExecuteAsync(db, """
                UPDATE Categorias SET id_categoria_pai=NULL
                WHERE id_categoria_pai IS NOT NULL;
                PRAGMA user_version=14;
                """);
        }
        if (version < 15)
        {
            await ExecuteAsync(db, """
                DROP TRIGGER IF EXISTS trg_transacoes_transferencia_insert;
                DROP TRIGGER IF EXISTS trg_transacoes_transferencia_update;
                CREATE TRIGGER trg_transacoes_transferencia_insert BEFORE INSERT ON Transacoes
                WHEN LOWER(NEW.tipo)='transferencia' AND
                  (NEW.valor<=0 OR NEW.id_conta IS NULL OR NEW.id_conta_destino IS NULL
                   OR NEW.id_conta=NEW.id_conta_destino)
                BEGIN SELECT RAISE(ABORT,'Transferência inválida.'); END;
                CREATE TRIGGER trg_transacoes_transferencia_update BEFORE UPDATE ON Transacoes
                WHEN LOWER(NEW.tipo)='transferencia' AND
                  (NEW.valor<=0 OR NEW.id_conta IS NULL OR NEW.id_conta_destino IS NULL
                   OR NEW.id_conta=NEW.id_conta_destino)
                BEGIN SELECT RAISE(ABORT,'Transferência inválida.'); END;
                DROP VIEW IF EXISTS vw_resumo_mensal;
                CREATE VIEW vw_resumo_mensal AS
                SELECT id_usuario,
                  strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data END) mes_ano,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END) total_receitas,
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) total_despesas,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END)-
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) saldo_mes
                FROM Transacoes WHERE pago=1 AND tipo IN('receita','despesa')
                  AND NOT(parcelado=1 AND id_transacao_pai IS NULL)
                GROUP BY id_usuario,
                  strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data END);
                DROP VIEW IF EXISTS vw_saude_financeira;
                CREATE VIEW vw_saude_financeira AS
                SELECT id_usuario,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END) total_receitas,
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) total_despesas,
                  SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END)-
                  SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END) saldo_geral,
                  CASE WHEN SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END)=0 THEN 0
                    ELSE ROUND(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END)*100.0/
                    SUM(CASE WHEN tipo='receita' AND pago=1 THEN valor ELSE 0 END),2) END percentual_gastos
                FROM Transacoes WHERE tipo IN('receita','despesa') GROUP BY id_usuario;
                PRAGMA user_version=15;
                """);
        }

        if (version < 16)
        {
            // Migração aditiva e idempotente: mantém os dados de recuperação legados.
            await EnsureColumnAsync(db, "Usuarios", "palavra_chave_hash", "TEXT");
            await EnsureColumnAsync(db, "Usuarios", "dica_palavra_chave", "TEXT");
            await ExecuteAsync(db, """
                UPDATE Usuarios
                SET palavra_chave_hash=COALESCE(palavra_chave_hash,resposta_recuperacao_hash),
                    dica_palavra_chave=COALESCE(dica_palavra_chave,pergunta_recuperacao,'Palavra-chave cadastrada no perfil')
                WHERE palavra_chave_hash IS NULL OR dica_palavra_chave IS NULL;
                PRAGMA user_version=16;
                """);
        }

        if (version < 17)
        {
            await EnsureFinancialCompetenceSchemaAsync(db);
            await ExecuteAsync(db, "PRAGMA user_version=17;");
        }

        // Reparação final idempotente para bancos restaurados com versão inconsistente.
        await EnsureColumnAsync(db, "Usuarios", "data_nascimento", "TEXT");
        await EnsureColumnAsync(db, "Usuarios", "pergunta_recuperacao", "TEXT");
        await EnsureColumnAsync(db, "Usuarios", "resposta_recuperacao_hash", "TEXT");
        await EnsureColumnAsync(db, "Usuarios", "palavra_chave_hash", "TEXT");
        await EnsureColumnAsync(db, "Usuarios", "dica_palavra_chave", "TEXT");
        await ExecuteAsync(db, """
            UPDATE Usuarios
            SET palavra_chave_hash=COALESCE(palavra_chave_hash,resposta_recuperacao_hash),
                dica_palavra_chave=COALESCE(dica_palavra_chave,pergunta_recuperacao)
            WHERE palavra_chave_hash IS NULL OR dica_palavra_chave IS NULL;
            """);
        await EnsureSupplierSchemaAsync(db);
        await EnsureFinancialCompetenceSchemaAsync(db);
        }
        finally
        {
            InitializationGate.Release();
        }
    }

    private static Task EnsureFinancialCompetenceSchemaAsync(SqliteConnection db) => ExecuteAsync(db, """
        CREATE INDEX IF NOT EXISTS idx_transacoes_usuario_tipo_pago_vencimento ON Transacoes(id_usuario,tipo,pago,data_vencimento);
        CREATE INDEX IF NOT EXISTS idx_transacoes_usuario_tipo_pago_pagamento ON Transacoes(id_usuario,tipo,pago,data_pagamento);
        DROP VIEW IF EXISTS vw_resumo_mensal;
        CREATE VIEW vw_resumo_mensal AS SELECT id_usuario,
          strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data_pagamento END) mes_ano,
          COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0) total_receitas,
          COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) total_despesas,
          COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0)-COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) saldo_mes
        FROM Transacoes WHERE ((tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL) OR (tipo='despesa' AND pago=1))
          AND NOT(parcelado=1 AND id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=Transacoes.id_transacao))
        GROUP BY id_usuario,strftime('%Y-%m',CASE WHEN tipo='despesa' THEN COALESCE(data_vencimento,data) ELSE data_pagamento END);
        DROP VIEW IF EXISTS vw_gastos_categoria;
        CREATE VIEW vw_gastos_categoria AS SELECT t.id_usuario,c.nome_categoria,c.tipo,c.cor,strftime('%Y-%m',COALESCE(t.data_vencimento,t.data)) mes_ano,COALESCE(SUM(t.valor),0) total
        FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria WHERE t.pago=1 AND t.tipo='despesa'
          AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao))
        GROUP BY t.id_usuario,c.id_categoria,strftime('%Y-%m',COALESCE(t.data_vencimento,t.data));
        DROP VIEW IF EXISTS vw_saude_financeira;
        CREATE VIEW vw_saude_financeira AS SELECT id_usuario,
          COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0) total_receitas,
          COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) total_despesas,
          COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0)-COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) saldo_geral,
          CASE WHEN COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0)=0 THEN 0 ELSE ROUND(COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0)*100.0/SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),2) END percentual_gastos
        FROM Transacoes WHERE tipo IN('receita','despesa') AND NOT(parcelado=1 AND id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=Transacoes.id_transacao)) GROUP BY id_usuario;
        DROP VIEW IF EXISTS vw_contas_pagar;
        CREATE VIEW vw_contas_pagar AS SELECT t.id_usuario,t.id_transacao,t.descricao,t.valor,COALESCE(t.data_vencimento,t.data) data_vencimento,c.nome_categoria categoria,
          CASE WHEN date(COALESCE(t.data_vencimento,t.data))<date('now','localtime') THEN 'vencida' WHEN date(COALESCE(t.data_vencimento,t.data))=date('now','localtime') THEN 'hoje' WHEN date(COALESCE(t.data_vencimento,t.data))<=date('now','localtime','+7 days') THEN 'proxima_semana' ELSE 'futura' END status_vencimento
        FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria WHERE t.tipo='despesa' AND t.pago=0
          AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao));
        DROP VIEW IF EXISTS vw_orcamento_mensal;
        CREATE VIEW vw_orcamento_mensal AS SELECT o.id_usuario,o.mes,o.ano,o.id_categoria,c.nome_categoria,c.cor,o.valor_limite,COALESCE(SUM(t.valor),0) valor_realizado,
          CASE WHEN o.valor_limite=0 THEN 0 ELSE ROUND(COALESCE(SUM(t.valor),0)*100.0/o.valor_limite,2) END percentual_utilizado,
          CASE WHEN COALESCE(SUM(t.valor),0)>o.valor_limite THEN 'estourado' WHEN COALESCE(SUM(t.valor),0)>o.valor_limite*.9 THEN 'atencao' WHEN COALESCE(SUM(t.valor),0)>o.valor_limite*.75 THEN 'cuidado' ELSE 'ok' END status
        FROM Orcamentos o JOIN Categorias c ON c.id_categoria=o.id_categoria LEFT JOIN Transacoes t ON t.id_usuario=o.id_usuario AND t.id_categoria=o.id_categoria AND t.tipo='despesa' AND t.pago=1
          AND CAST(strftime('%m',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.mes AND CAST(strftime('%Y',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.ano
          AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao)) GROUP BY o.id_orcamento;
        """);

    private static async Task EnsureSupplierSchemaAsync(SqliteConnection db)
    {
        await ExecuteAsync(db, """
            CREATE TABLE IF NOT EXISTS Fornecedores(
              id_fornecedor INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
              id_usuario INTEGER NOT NULL,
              nome_fornecedor TEXT NOT NULL,
              ativo INTEGER DEFAULT 1,
              data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,
              FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE);
            CREATE INDEX IF NOT EXISTS idx_fornecedores_usuario_nome
              ON Fornecedores(id_usuario,ativo,nome_fornecedor);
            """);
        await EnsureColumnAsync(db, "Transacoes", "id_fornecedor", "INTEGER");
        await ExecuteAsync(db,
            "CREATE INDEX IF NOT EXISTS idx_transacoes_fornecedor ON Transacoes(id_fornecedor);");
    }

    // ============================================================
    // CARTÕES
    // ============================================================

    public async Task DeleteCardAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            // Remover transações vinculadas ao cartão
            await ExecuteAsync(db, "DELETE FROM Transacoes WHERE id_cartao = @id", ("@id", id));
            // Remover o cartão
            await ExecuteAsync(db, "DELETE FROM CartoesCredito WHERE id_cartao = @id", ("@id", id));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<List<CardItem>> GetCardsAsync() => QueryAsync(
        $"SELECT id_cartao,nome_cartao,limite_credito,limite_utilizado,dia_fechamento,dia_vencimento,COALESCE(cor,'#00A859') FROM CartoesCredito WHERE id_usuario={CurrentUserId} AND ativo=1 ORDER BY nome_cartao",
        r => new CardItem(r.GetInt64(0), r.GetString(1), r.GetDecimal(2), r.GetDecimal(3),
            r.GetInt32(4), r.GetInt32(5), r.GetString(6)));

    public async Task SaveCardAsync(long? id, string name, decimal limit, int closingDay,
        int dueDay, string color = BlingPalette.PrimaryHex)
    {
        if (name.Trim().Length < 2)
            throw new ArgumentException("Informe o nome do cartão.");
        if (limit <= 0)
            throw new ArgumentException("O limite deve ser maior que zero.");
        if (closingDay is < 1 or > 31 || dueDay is < 1 or > 31)
            throw new ArgumentException("Os dias devem estar entre 1 e 31.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        if (id is null)
            await ExecuteAsync(db, """
                INSERT INTO CartoesCredito(id_usuario,nome_cartao,limite_credito,dia_fechamento,dia_vencimento,cor)
                VALUES(@user,@name,@limit,@closing,@due,@color)
                """, ("@user", CurrentUserId), ("@name", name.Trim()), ("@limit", limit),
                ("@closing", closingDay), ("@due", dueDay), ("@color", color));
        else
            await ExecuteAsync(db, """
                UPDATE CartoesCredito SET nome_cartao=@name,limite_credito=@limit,
                  dia_fechamento=@closing,dia_vencimento=@due,cor=@color
                WHERE id_cartao=@id AND id_usuario=@user
                """, ("@name", name.Trim()), ("@limit", limit), ("@closing", closingDay),
                ("@due", dueDay), ("@color", color), ("@id", id), ("@user", CurrentUserId));
    }

    // ============================================================
    // CATEGORIAS
    // ============================================================

    public async Task DeleteCategoryAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            // Buscar todas as subcategorias
            var subcategories = await QueryAsync(db,
                "SELECT id_categoria FROM Categorias WHERE id_categoria_pai = @id",
                r => r.GetInt64(0), ("@id", id));

            // Remover transações vinculadas às subcategorias
            foreach (var subId in subcategories)
            {
                await ExecuteAsync(db, "DELETE FROM Transacoes WHERE id_categoria = @id", ("@id", subId));
            }

            // Remover transações da categoria principal
            await ExecuteAsync(db, "DELETE FROM Transacoes WHERE id_categoria = @id", ("@id", id));

            // Remover orçamentos vinculados
            await ExecuteAsync(db, "DELETE FROM Orcamentos WHERE id_categoria = @id", ("@id", id));

            // Remover subcategorias
            await ExecuteAsync(db, "DELETE FROM Categorias WHERE id_categoria_pai = @id", ("@id", id));

            // Remover a categoria principal
            await ExecuteAsync(db, "DELETE FROM Categorias WHERE id_categoria = @id", ("@id", id));

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<List<CategoryItem>> GetCategoriesAsync(string? type = null) => QueryAsync(
        """
        SELECT id_categoria,nome_categoria,tipo,cor,icone,NULL,1,nome_categoria
        FROM Categorias
        WHERE id_usuario=@user AND ativo=1 AND (@type IS NULL OR tipo=@type)
        ORDER BY tipo,nome_categoria
        """,
        r => new CategoryItem(r.GetInt64(0), r.GetString(1), r.GetString(2), r.GetString(3),
            r.IsDBNull(4) ? null : r.GetString(4), r.IsDBNull(5) ? null : r.GetInt64(5),
            r.GetInt32(6), r.GetString(7)),
        ("@user", CurrentUserId), ("@type", type));

    public Task<List<FinancialReportItem>> GetFinancialReportItemsAsync(
        int month, int year, long? categoryId = null, string paymentStatus = "todas",
        long? supplierId = null, long? cardId = null) => QueryAsync(
        """
        WITH RECURSIVE categorias_selecionadas(id_categoria) AS (
          SELECT @category WHERE @category IS NOT NULL
          UNION ALL
          SELECT c.id_categoria FROM Categorias c
          JOIN categorias_selecionadas p ON c.id_categoria_pai=p.id_categoria
          WHERE c.id_usuario=@user
        )
        SELECT t.id_transacao,
               date(CASE WHEN t.tipo='despesa' THEN COALESCE(t.data_vencimento,t.data) ELSE t.data_pagamento END),
               t.descricao,t.tipo,t.valor,c.id_categoria,
               c.nome_categoria,COALESCE(a.nome_conta,cc.nome_cartao,'—'),COALESCE(t.pago,0),
               COALESCE(t.numero_parcela,1),COALESCE(t.total_parcelas,1)
        FROM Transacoes t
        LEFT JOIN Categorias c ON c.id_categoria=t.id_categoria
        LEFT JOIN Contas a ON a.id_conta=t.id_conta
        LEFT JOIN CartoesCredito cc ON cc.id_cartao=t.id_cartao
        WHERE t.id_usuario=@user
          AND t.tipo IN('receita','despesa')
          AND NOT (COALESCE(t.parcelado,0)=1 AND t.id_transacao_pai IS NULL
                   AND EXISTS(SELECT 1 FROM Transacoes filha
                              WHERE filha.id_transacao_pai=t.id_transacao))
          AND (t.tipo<>'receita' OR (t.pago=1 AND t.data_pagamento IS NOT NULL))
          AND date(CASE WHEN t.tipo='despesa' THEN COALESCE(t.data_vencimento,t.data) ELSE t.data_pagamento END)>=date(@start)
          AND date(CASE WHEN t.tipo='despesa' THEN COALESCE(t.data_vencimento,t.data) ELSE t.data_pagamento END)<date(@end)
          AND (@category IS NULL OR t.id_categoria=@category)
          AND (@supplier IS NULL OR t.id_fornecedor=@supplier)
          AND (@card IS NULL OR t.id_cartao=@card)
          AND (@status='todas' OR (@status='pagas' AND COALESCE(t.pago,0)=1)
               OR (@status='abertas' AND COALESCE(t.pago,0)=0))
        ORDER BY COALESCE(c.nome_categoria,'Sem categoria'),
                 date(CASE WHEN t.tipo='despesa' THEN COALESCE(t.data_vencimento,t.data) ELSE t.data_pagamento END),
                 t.id_transacao
        """,
        r => new FinancialReportItem(
            r.GetInt64(0), r.GetDateTime(1), r.GetString(2), r.GetString(3), r.GetDecimal(4),
            r.IsDBNull(5) ? null : r.GetInt64(5), r.IsDBNull(6) ? "Sem categoria" : r.GetString(6),
            r.GetString(7), r.GetInt32(8) == 1, r.GetInt32(9), r.GetInt32(10)),
        ("@user", CurrentUserId), ("@start", $"{year:D4}-{month:D2}-01"),
        ("@end", new DateTime(year, month, 1).AddMonths(1).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)),
        ("@category", categoryId), ("@status", paymentStatus), ("@supplier", supplierId),
        ("@card", cardId));

    /// <summary>Cria uma categoria simples. A hierarquia legada não é utilizada pela interface.</summary>
    public async Task AddCategoryAsync(string name, string type, string color, string? icon, long? parentId)
    {
        parentId = null;
        name = name.Trim();
        if (name.Length < 2)
            throw new ArgumentException("Informe um nome com pelo menos dois caracteres.");
        if (type is not ("receita" or "despesa"))
            throw new ArgumentException("Tipo de categoria inválido.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$"))
            throw new ArgumentException("Informe uma cor hexadecimal válida.");

        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        if (parentId is not null)
        {
            await using var parent = CreateCommand(db, """
                WITH RECURSIVE pais(id_categoria,id_categoria_pai,nivel) AS (
                  SELECT id_categoria,id_categoria_pai,1 FROM Categorias
                  WHERE id_categoria=@parent AND id_usuario=@user AND ativo=1 AND tipo=@type
                  UNION ALL SELECT c.id_categoria,c.id_categoria_pai,p.nivel+1
                  FROM Categorias c JOIN pais p ON c.id_categoria=p.id_categoria_pai
                )
                SELECT COALESCE(MAX(nivel),0) FROM pais
                """, ("@parent", parentId), ("@user", CurrentUserId), ("@type", type));
            var level = Convert.ToInt32(await parent.ExecuteScalarAsync());
            if (level == 0)
                throw new ArgumentException("A categoria pai não foi encontrada ou possui outro tipo.");
            if (level >= 3)
                throw new ArgumentException("O limite de três níveis de categorias foi atingido.");
        }
        try
        {
            await ExecuteAsync(db, """
                INSERT INTO Categorias(id_usuario,nome_categoria,tipo,id_categoria_pai,cor,icone)
                VALUES(@user,@name,@type,@parent,@color,@icon)
                """, ("@user", CurrentUserId), ("@name", name), ("@type", type),
                ("@parent", parentId), ("@color", color.ToUpperInvariant()), ("@icon", icon));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new ArgumentException("Já existe uma categoria com este nome e tipo.", ex);
        }
    }

    public async Task UpdateCategoryAsync(long id, string name, string type, string color, string? icon, long? parentId)
    {
        parentId = null;
        if (id == parentId)
            throw new ArgumentException("Uma categoria não pode depender dela mesma.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        var descendant = parentId is null ? 0 : Convert.ToInt32(await ScalarAsync(db, """
            WITH RECURSIVE filhos(id) AS (
              SELECT id_categoria FROM Categorias WHERE id_categoria_pai=@id
              UNION ALL SELECT c.id_categoria FROM Categorias c JOIN filhos f ON c.id_categoria_pai=f.id
            ) SELECT COUNT(*) FROM filhos WHERE id=@parent
            """, ("@id", id), ("@parent", parentId)));
        if (descendant > 0)
            throw new ArgumentException("A categoria pai não pode ser uma subcategoria deste item.");

        name = name.Trim();
        if (name.Length < 2)
            throw new ArgumentException("Informe um nome válido.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$"))
            throw new ArgumentException("Informe uma cor hexadecimal válida.");
        if (parentId is not null)
        {
            var parentType = Convert.ToString(await ScalarAsync(db,
                "SELECT tipo FROM Categorias WHERE id_categoria=@parent AND id_usuario=@user AND ativo=1",
                ("@parent", parentId), ("@user", CurrentUserId)));
            if (parentType != type)
                throw new ArgumentException("Categoria e subcategoria devem possuir o mesmo tipo.");
            var parentDepth = Convert.ToInt32(await ScalarAsync(db, """
                WITH RECURSIVE pais(id,pai,nivel) AS (
                  SELECT id_categoria,id_categoria_pai,1 FROM Categorias WHERE id_categoria=@parent
                  UNION ALL SELECT c.id_categoria,c.id_categoria_pai,p.nivel+1
                  FROM Categorias c JOIN pais p ON c.id_categoria=p.pai)
                SELECT COALESCE(MAX(nivel),0) FROM pais
                """, ("@parent", parentId)) ?? 0);
            var subtreeDepth = Convert.ToInt32(await ScalarAsync(db, """
                WITH RECURSIVE filhos(id,nivel) AS (
                  SELECT id_categoria,1 FROM Categorias WHERE id_categoria=@id
                  UNION ALL SELECT c.id_categoria,f.nivel+1 FROM Categorias c
                  JOIN filhos f ON c.id_categoria_pai=f.id)
                SELECT COALESCE(MAX(nivel),1) FROM filhos
                """, ("@id", id)) ?? 1);
            if (parentDepth + subtreeDepth > 3)
                throw new ArgumentException("A alteração ultrapassaria o limite de três níveis.");
        }
        try
        {
            await ExecuteAsync(db, """
                UPDATE Categorias SET nome_categoria=@name,tipo=@type,id_categoria_pai=@parent,
                  cor=@color,icone=@icon WHERE id_categoria=@id AND id_usuario=@user
                """, ("@name", name), ("@type", type), ("@parent", parentId), ("@color", color.ToUpperInvariant()),
                ("@icon", icon), ("@id", id), ("@user", CurrentUserId));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new ArgumentException("Já existe uma categoria com este nome e tipo.", ex);
        }
    }

    // ============================================================
    // CONTAS
    // ============================================================

    public Task<List<AccountItem>> GetAccountsAsync() => QueryAsync(
        $"SELECT id_conta,nome_conta,saldo_atual,tipo_conta,COALESCE(cor,'#00A859'),COALESCE(icone,'AccountBalance') FROM Contas WHERE id_usuario={CurrentUserId} AND ativo=1 ORDER BY nome_conta",
        r => new AccountItem(r.GetInt64(0), r.GetString(1), r.GetDecimal(2),
            r.GetString(3), r.GetString(4), r.GetString(5)));

    public async Task<long> SaveAccountAsync(long? id, string name, decimal balance, string type,
        string color, string icon)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Informe o nome da conta.");
        if (type is not ("corrente" or "poupanca" or "carteira" or "investimento"))
            throw new ArgumentException("Selecione um tipo de conta válido.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$"))
            throw new ArgumentException("Informe uma cor hexadecimal válida.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        var isNew = id is null;
        if (isNew)
        {
            var inserted = await ExecuteCountAsync(db, """
                INSERT INTO Contas(id_usuario,nome_conta,saldo_inicial,saldo_atual,tipo_conta,cor,icone)
                VALUES(@user,@name,@balance,@balance,@type,@color,@icon)
                """, ("@user", CurrentUserId), ("@name", name.Trim()), ("@balance", balance),
                ("@type", type), ("@color", color.ToUpperInvariant()), ("@icon", icon));
            if (inserted != 1) throw new InvalidOperationException("A conta não foi gravada no banco de dados.");
            id = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid()"));
        }
        else
        {
            var updated = await ExecuteCountAsync(db, """
                UPDATE Contas SET nome_conta=@name,saldo_atual=@balance,tipo_conta=@type,
                  cor=@color,icone=@icon WHERE id_conta=@id AND id_usuario=@user
                """, ("@name", name.Trim()), ("@balance", balance), ("@type", type),
                ("@color", color.ToUpperInvariant()), ("@icon", icon),
                ("@id", id), ("@user", CurrentUserId));
            if (updated != 1) throw new InvalidOperationException("A conta não foi encontrada para alteração.");
        }
        var persisted = Convert.ToInt64(await ScalarAsync(db,
            "SELECT COUNT(*) FROM Contas WHERE id_conta=@id AND id_usuario=@user",
            ("@id", id), ("@user", CurrentUserId)) ?? 0);
        if (persisted != 1) throw new InvalidOperationException("Não foi possível confirmar a conta gravada.");
        await WriteLogAsync(db, isNew ? "CRIAR" : "EDITAR", "Contas", id,
            $"nome={name}; saldo={balance:0.00}");
        DataChanged?.Invoke(this, EventArgs.Empty);
        return id ?? throw new InvalidOperationException("O banco não retornou o identificador da conta.");
    }

    public async Task DeleteAccountAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        var linked = Convert.ToInt32(await ScalarAsync(db, """
            SELECT COUNT(*) FROM Transacoes
            WHERE id_usuario=@user AND (id_conta=@id OR id_conta_destino=@id)
            """, ("@user", CurrentUserId), ("@id", id)) ?? 0);
        if (linked > 0)
            await ExecuteAsync(db, "UPDATE Contas SET ativo=0 WHERE id_conta=@id AND id_usuario=@user",
                ("@id", id), ("@user", CurrentUserId));
        else
            await ExecuteAsync(db, "DELETE FROM Contas WHERE id_conta=@id AND id_usuario=@user",
                ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, linked > 0 ? "INATIVAR" : "EXCLUIR", "Contas", id, null);
    }

    // ============================================================
    // DASHBOARD
    // ============================================================

    /// <summary>Returns the current balance of all active accounts owned by the signed-in user.</summary>
    public async Task<decimal> GetTotalActiveAccountsBalanceAsync()
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        return await GetTotalActiveAccountsBalanceAsync(db, CurrentUserId);
    }

    private static Task<decimal> GetTotalActiveAccountsBalanceAsync(SqliteConnection db, long userId) =>
        DecimalAsync(db,
            "SELECT COALESCE(SUM(COALESCE(saldo_atual,0.00)),0.00) FROM Contas WHERE ativo=1 AND id_usuario=@user",
            ("@user", userId));

    public Task<List<FinancialProjectionEvent>> GetFinancialProjectionEventsAsync(DateTime start, DateTime end) =>
        QueryAsync("""
            SELECT t.id_transacao,
              date(CASE WHEN t.tipo='receita' AND t.pago=1 THEN t.data_pagamento
                        WHEN t.tipo='receita' THEN t.data
                        ELSE COALESCE(t.data_vencimento,t.data) END),
              t.descricao,t.tipo,COALESCE(t.valor,0),COALESCE(t.pago,0),t.id_conta,t.id_cartao
            FROM Transacoes t
            WHERE t.id_usuario=@user AND t.tipo IN('receita','despesa')
              AND ((t.tipo='receita' AND ((t.pago=1 AND t.data_pagamento IS NOT NULL) OR t.pago=0))
                   OR t.tipo='despesa')
              AND date(CASE WHEN t.tipo='receita' AND t.pago=1 THEN t.data_pagamento
                            WHEN t.tipo='receita' THEN t.data
                            ELSE COALESCE(t.data_vencimento,t.data) END)>=date(@start)
              AND date(CASE WHEN t.tipo='receita' AND t.pago=1 THEN t.data_pagamento
                            WHEN t.tipo='receita' THEN t.data
                            ELSE COALESCE(t.data_vencimento,t.data) END)<date(@end)
              AND NOT(COALESCE(t.parcelado,0)=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))
            ORDER BY 2,t.id_transacao
            """, r => new FinancialProjectionEvent(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),
                r.GetString(3),r.GetDecimal(4),r.GetInt32(5)==1,
                r.IsDBNull(6)?null:r.GetInt64(6),r.IsDBNull(7)?null:r.GetInt64(7)),
            ("@user",CurrentUserId),("@start",start.Date),("@end",end.Date));

    /// <summary>
    /// Returns only realized income and paid expenses whose due date belongs to the
    /// selected month. This is the single source for the three values in MainPage's hero card.
    /// SQL and reader-level fallbacks keep an empty period safely represented by zeroes.
    /// </summary>
    public async Task<MonthlyRealizedTotals> GetMonthlyRealizedTotalsAsync(int month, int year)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        return await GetMonthlyRealizedTotalsAsync(db, CurrentUserId, month, year);
    }

    private static async Task<MonthlyRealizedTotals> GetMonthlyRealizedTotalsAsync(
        SqliteConnection db, long userId, int month, int year)
    {
        var selectedPeriod = $"{year:D4}-{month:D2}";
        await using var command = CreateCommand(db, """
            SELECT
              COALESCE(SUM(CASE WHEN tipo='receita' AND COALESCE(pago,0)=1
                AND data_pagamento IS NOT NULL AND strftime('%Y-%m',data_pagamento)=@selectedPeriod
                THEN COALESCE(valor,0.00) ELSE 0.00 END),0.00),
              COALESCE(SUM(CASE WHEN tipo='despesa' AND COALESCE(pago,0)=1
                AND strftime('%Y-%m',COALESCE(data_vencimento,data))=@selectedPeriod
                THEN COALESCE(valor,0.00) ELSE 0.00 END),0.00)
            FROM Transacoes WHERE id_usuario=@user
              AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(
                SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=Transacoes.id_transacao))
            """, ("@selectedPeriod", selectedPeriod), ("@user", userId));
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return new(0.00m, 0.00m);

        var totalReceitas = reader.IsDBNull(0) ? 0.00m : Convert.ToDecimal(reader.GetValue(0));
        var totalDespesas = reader.IsDBNull(1) ? 0.00m : Convert.ToDecimal(reader.GetValue(1));
        return new(totalReceitas, totalDespesas);
    }

    /// <summary>Loads consolidated dashboard metrics for the current month.</summary>
    public async Task<DashboardSummary> GetDashboardAsync(int? month = null, int? year = null)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        var selectedMonth = month ?? DateTime.Today.Month;
        var selectedYear = year ?? DateTime.Today.Year;
        var start = new DateTime(selectedYear, selectedMonth, 1);
        var end = start.AddMonths(1);
        var userId = CurrentUserId;
        var balance = await GetTotalActiveAccountsBalanceAsync(db, userId);
        var realized = await GetMonthlyRealizedTotalsAsync(db, userId, selectedMonth, selectedYear);
        var income = realized.TotalReceitas;
        var expenses = realized.TotalDespesas;
        var cardDebt = await DecimalAsync(db, "SELECT COALESCE(SUM(limite_utilizado),0) FROM CartoesCredito WHERE id_usuario=@user AND ativo=1", ("@user", userId));
        var reserve = await DecimalAsync(db, "SELECT COALESCE(SUM(saldo_atual),0) FROM Contas WHERE id_usuario=@user AND ativo=1 AND tipo_conta IN('poupanca','investimento')", ("@user", userId));

        var recent = new List<TransactionItem>();
        await using (var command = db.CreateCommand())
        {
            command.CommandText = """
                SELECT t.id_transacao,t.descricao,t.valor,
                       CASE WHEN t.tipo='receita' THEN t.data_pagamento ELSE COALESCE(t.data_vencimento,t.data) END,
                       t.tipo,c.nome_categoria,
                       COALESCE(a.nome_conta,cc.nome_cartao,'—')
                FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria
                LEFT JOIN Contas a ON a.id_conta=t.id_conta LEFT JOIN CartoesCredito cc ON cc.id_cartao=t.id_cartao
                WHERE t.id_usuario=@user AND t.pago=1
                  AND ((t.tipo='receita' AND t.data_pagamento IS NOT NULL
                        AND date(t.data_pagamento)>=date(@start) AND date(t.data_pagamento)<date(@end))
                    OR (t.tipo='despesa' AND date(COALESCE(t.data_vencimento,t.data))>=date(@start)
                        AND date(COALESCE(t.data_vencimento,t.data))<date(@end)))
                  AND NOT (t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                    SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao))
                ORDER BY 4 DESC,t.id_transacao DESC LIMIT 10
                """;
            command.Parameters.AddWithValue("@user", userId);
            command.Parameters.AddWithValue("@start", start);
            command.Parameters.AddWithValue("@end", end);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                recent.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetDecimal(2), reader.GetDateTime(3),
                    reader.GetString(4), reader.GetString(5), reader.GetString(6)));
        }

        var budgets = new List<BudgetProgress>();
        await using (var command = db.CreateCommand())
        {
            command.CommandText = """
                SELECT nome_categoria,cor,valor_limite,valor_realizado,status
                FROM vw_orcamento_mensal
                WHERE id_usuario=@user AND mes=@month AND ano=@year
                ORDER BY percentual_utilizado DESC
                """;
            command.Parameters.AddWithValue("@user", userId);
            command.Parameters.AddWithValue("@month", selectedMonth);
            command.Parameters.AddWithValue("@year", selectedYear);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var limit = reader.GetDecimal(2);
                var spent = reader.GetDecimal(3);
                budgets.Add(new(reader.GetString(0), reader.GetString(1), limit, spent,
                    limit == 0 ? 0 : Math.Min(1, (double)(spent / limit)), reader.GetString(4)));
            }
        }

        var savings = income - expenses;
        var score = income == 0 ? 0 : Math.Clamp((int)(60 + savings / income * 40), 0, 100);
        var days = selectedMonth == DateTime.Today.Month && selectedYear == DateTime.Today.Year
            ? Math.Max(1, DateTime.Today.Day)
            : DateTime.DaysInMonth(selectedYear, selectedMonth);
        var savingsRate = income == 0 ? 0 : savings / income * 100;
        var reserveDays = expenses == 0 ? 0 : reserve / expenses * 30;
        var dailyAverage = expenses / days;
        var forecast = balance + savings * 3;
        return new(balance, income, expenses, savings, score, balance - cardDebt, savingsRate,
            reserveDays, dailyAverage, forecast, recent, budgets);
    }

    // ============================================================
    // TRANSAÇÕES
    // ============================================================

    public async Task<List<ReceitaListItem>> GetReceitasAsync(int? mes, int? ano)
    {
        var items = await QueryAsync(
        """
        SELECT t.id_transacao,t.descricao,c.nome_categoria,
               CASE WHEN t.pago=1 THEN t.data_pagamento ELSE t.data END,t.valor,
               COALESCE(a.nome_conta,'—'),COALESCE(t.pago,0),t.data_pagamento,COALESCE(t.recorrente,0)
        FROM Transacoes t
        JOIN Categorias c ON c.id_categoria=t.id_categoria
        LEFT JOIN Contas a ON a.id_conta=t.id_conta
        WHERE t.id_usuario=@user AND t.tipo='receita'
          AND ((t.pago=1 AND t.data_pagamento IS NOT NULL) OR t.pago=0)
          AND NOT (t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
            SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao))
          AND (@month IS NULL OR CAST(strftime('%m',CASE WHEN t.pago=1 THEN t.data_pagamento ELSE t.data END) AS INTEGER)=@month)
          AND (@year IS NULL OR CAST(strftime('%Y',CASE WHEN t.pago=1 THEN t.data_pagamento ELSE t.data END) AS INTEGER)=@year)
        ORDER BY CASE WHEN t.pago=1 THEN t.data_pagamento ELSE t.data END DESC,t.id_transacao DESC
        """,
        r => new ReceitaListItem(r.GetInt64(0), r.GetString(1), r.GetString(2),
            r.GetDateTime(3), r.GetDecimal(4), r.GetString(5),r.GetInt32(6)==1,
            r.IsDBNull(7)?null:r.GetDateTime(7),r.GetInt32(8)==1),
        ("@user", CurrentUserId), ("@month", mes), ("@year", ano));
        var tags = await GetTransactionTagsLookupAsync();
        return items.Select(item => item with { Tags = tags.GetValueOrDefault(item.Id, []) }).ToList();
    }

    public async Task<ResumoReceitas> GetResumoReceitasAsync(int? mes, int? ano)
    {
        var items = await GetReceitasAsync(mes, ano);
        return new(items.Count, items.Sum(x => x.Valor),
            items.Count == 0 ? 0 : items.Average(x => x.Valor));
    }

    public async Task<TransactionEditData> GetTransactionForEditAsync(long id, string expectedType)
    {
        var items = await QueryAsync("""
            SELECT id_transacao,descricao,valor,data,tipo,id_categoria,id_conta,id_cartao,observacoes,
              data_vencimento,pago,data_pagamento,recorrente,frequencia,id_fornecedor,
              COALESCE(parcelado,0),COALESCE(numero_parcela,1),COALESCE(total_parcelas,1),id_transacao_pai
            FROM Transacoes WHERE id_transacao=@id AND id_usuario=@user AND tipo=@type
            """, r => new TransactionEditData(r.GetInt64(0), r.GetString(1), r.GetDecimal(2),
                r.GetDateTime(3), r.GetString(4), r.GetInt64(5),
                r.IsDBNull(6) ? null : r.GetInt64(6), r.IsDBNull(7) ? null : r.GetInt64(7),
                r.IsDBNull(8) ? null : r.GetString(8), r.IsDBNull(9) ? null : r.GetDateTime(9),
                r.GetInt32(10) == 1, r.IsDBNull(11) ? null : r.GetDateTime(11),
                r.GetInt32(12) == 1, r.IsDBNull(13) ? null : r.GetString(13),
                r.IsDBNull(14) ? null : r.GetInt64(14), r.GetInt32(15) == 1,
                r.GetInt32(16), r.GetInt32(17), r.IsDBNull(18) ? null : r.GetInt64(18)),
            ("@id", id), ("@user", CurrentUserId), ("@type", expectedType));
        return items.SingleOrDefault()
            ?? throw new InvalidOperationException("O lançamento não foi encontrado.");
    }

    public Task<List<InstallmentScheduleItem>> GetTransactionInstallmentsAsync(long transactionId) => QueryAsync(
        """
        WITH selected AS (
          SELECT COALESCE(id_transacao_pai,id_transacao) series_id
          FROM Transacoes WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa'
        )
        SELECT t.id_transacao,COALESCE(t.numero_parcela,1),COALESCE(t.total_parcelas,1),
               COALESCE(t.data_vencimento,t.data),COALESCE(t.pago,0),t.valor
        FROM Transacoes t CROSS JOIN selected s
        WHERE t.id_usuario=@user AND t.tipo='despesa' AND t.parcelado=1
          AND (t.id_transacao=s.series_id OR t.id_transacao_pai=s.series_id)
          AND NOT (t.id_transacao=s.series_id AND EXISTS(
              SELECT 1 FROM Transacoes child WHERE child.id_transacao_pai=s.series_id AND child.numero_parcela=1))
        ORDER BY COALESCE(t.numero_parcela,1),COALESCE(t.data_vencimento,t.data),t.id_transacao
        """,
        r => new InstallmentScheduleItem(r.GetInt64(0), r.GetInt32(1), r.GetInt32(2),
            r.GetDateTime(3), r.GetInt32(4) == 1, r.GetDecimal(5)),
        ("@id", transactionId), ("@user", CurrentUserId));

    public async Task UpdateInstallmentDueDatesAsync(IReadOnlyDictionary<long, DateTime> dueDates)
    {
        if (dueDates.Count == 0) return;
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            foreach (var item in dueDates)
            {
                var updated = await ExecuteCountAsync(db, """
                    UPDATE Transacoes SET data_vencimento=@due,data_atualizacao=CURRENT_TIMESTAMP
                    WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa' AND parcelado=1
                    """, ("@due", item.Value.Date), ("@id", item.Key), ("@user", CurrentUserId));
                if (updated != 1) throw new InvalidOperationException("Uma das parcelas não foi encontrada.");
                await ExecuteAsync(db, """
                    INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                    VALUES(@user,'ALTERAR_VENCIMENTO','Transacoes',@id,@new,'TransactionEditPage')
                    """, ("@user", CurrentUserId), ("@id", item.Key), ("@new", $"data_vencimento={item.Value:yyyy-MM-dd}"));
            }
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetIncomePaidStatusAsync(long id, bool paid, DateTime? paymentDate = null)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            await using var command = CreateCommand(db,"""
                SELECT valor,id_conta,COALESCE(pago,0) FROM Transacoes
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='receita'
                """,("@id",id),("@user",CurrentUserId));
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("A receita não foi encontrada.");
            var amount=reader.GetDecimal(0); var accountId=reader.IsDBNull(1)?(long?)null:reader.GetInt64(1);
            var wasPaid=reader.GetInt32(2)==1; await reader.DisposeAsync();
            if (paid==wasPaid) return;
            if (accountId is null) throw new InvalidOperationException("A receita precisa possuir uma conta de destino.");
            if (paid && paymentDate is null) throw new ArgumentException("Informe a data efetiva do recebimento.");
            await ExecuteAsync(db,"""
                UPDATE Contas SET saldo_atual=saldo_atual+@delta
                WHERE id_conta=@account AND id_usuario=@user AND ativo=1
                """,("@delta",paid?amount:-amount),("@account",accountId),("@user",CurrentUserId));
            await ExecuteAsync(db,"""
                UPDATE Transacoes SET pago=@paid,data_pagamento=@payment,data_atualizacao=CURRENT_TIMESTAMP
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='receita'
                """,("@paid",paid?1:0),("@payment",paid?paymentDate!.Value.Date:null),
                ("@id",id),("@user",CurrentUserId));
            await WriteLogAsync(db,paid?"RECEBER":"ESTORNAR_RECEITA","Transacoes",id,
                paid?$"recebimento={paymentDate:yyyy-MM-dd}":"receita voltou a pendente");
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
        DataChanged?.Invoke(this,EventArgs.Empty);
    }

    public async Task UpdateInstallmentScheduleAsync(long transactionId,
        IReadOnlyList<InstallmentPreview> schedule)
    {
        if (schedule.Count < 2 || schedule.Any(item => item.Amount <= 0))
            throw new ArgumentException("Informe ao menos duas parcelas/ocorrências com valores válidos.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            var seriesValue = await ScalarAsync(db, """
                SELECT COALESCE(id_transacao_pai,id_transacao) FROM Transacoes
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa' AND parcelado=1
                """, ("@id", transactionId), ("@user", CurrentUserId));
            if (seriesValue is null) throw new InvalidOperationException("Série não encontrada.");
            var seriesId = Convert.ToInt64(seriesValue);
            var rows = await QueryAsync(db, """
                SELECT id_transacao FROM Transacoes
                WHERE id_usuario=@user AND id_transacao_pai=@series
                ORDER BY COALESCE(numero_parcela,1),id_transacao
                """, reader => reader.GetInt64(0), ("@user", CurrentUserId), ("@series", seriesId));
            if (rows.Count != schedule.Count)
                throw new InvalidOperationException("Altere a quantidade antes de editar os itens da série.");
            for (var index = 0; index < rows.Count; index++)
                await ExecuteAsync(db, """
                    UPDATE Transacoes SET valor=@amount,data_vencimento=@date,
                      data_atualizacao=CURRENT_TIMESTAMP
                    WHERE id_transacao=@id AND id_usuario=@user AND pago=0
                    """, ("@amount", schedule[index].Amount), ("@date", schedule[index].Date.Date),
                    ("@id", rows[index]), ("@user", CurrentUserId));
            await ExecuteAsync(db, """
                UPDATE Transacoes SET valor=@total,data_vencimento=@date,
                  data_atualizacao=CURRENT_TIMESTAMP
                WHERE id_transacao=@series AND id_usuario=@user
                """, ("@total", schedule.Sum(item => item.Amount)),
                ("@date", schedule[0].Date.Date), ("@series", seriesId), ("@user", CurrentUserId));
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task ResizeInstallmentSeriesAsync(long transactionId, int installmentCount,
        DateTime firstInstallmentDate)
    {
        if (installmentCount is < 2 or > 36)
            throw new ArgumentException("Informe entre 2 e 36 parcelas.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            var seriesValue = await ScalarAsync(db, """
                SELECT COALESCE(id_transacao_pai,id_transacao) FROM Transacoes
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa' AND parcelado=1
                """, ("@id", transactionId), ("@user", CurrentUserId));
            if (seriesValue is null) throw new InvalidOperationException("Parcelamento não encontrado.");
            var seriesId = Convert.ToInt64(seriesValue);
            var paidCount = Convert.ToInt32(await ScalarAsync(db, """
                SELECT COUNT(*) FROM Transacoes WHERE id_usuario=@user AND pago=1
                  AND (id_transacao=@series OR id_transacao_pai=@series)
                """, ("@user", CurrentUserId), ("@series", seriesId)) ?? 0);
            if (paidCount > 0)
                throw new InvalidOperationException("Não é possível alterar a quantidade porque já existe parcela paga.");

            await using var source = CreateCommand(db, """
                SELECT descricao,valor,id_conta,id_cartao,id_categoria,observacoes,
                       COALESCE(recorrente,0),frequencia,id_fornecedor
                      ,data
                FROM Transacoes WHERE id_transacao=@series AND id_usuario=@user
                """, ("@series", seriesId), ("@user", CurrentUserId));
            await using var reader = await source.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("Série de parcelas não encontrada.");
            var description = reader.GetString(0);
            var totalAmount = reader.GetDecimal(1);
            long? accountId = reader.IsDBNull(2) ? null : reader.GetInt64(2);
            long? cardId = reader.IsDBNull(3) ? null : reader.GetInt64(3);
            var categoryId = reader.GetInt64(4);
            var notes = reader.IsDBNull(5) ? null : reader.GetString(5);
            var recurring = reader.GetInt32(6) == 1;
            var frequency = reader.IsDBNull(7) ? null : reader.GetString(7);
            long? supplierId = reader.IsDBNull(8) ? null : reader.GetInt64(8);
            var purchaseDate = reader.GetDateTime(9).Date;
            await reader.DisposeAsync();

            var installments = TransactionSchedulePlanner.Build(totalAmount, installmentCount,
                firstInstallmentDate.Date, recurring ? TransactionScheduleMode.Recurrence : TransactionScheduleMode.Installment);
            await ExecuteAsync(db, "DELETE FROM Transacoes WHERE id_transacao_pai=@series AND id_usuario=@user",
                ("@series", seriesId), ("@user", CurrentUserId));
            await ExecuteAsync(db, """
                UPDATE Transacoes SET numero_parcelas=@count,total_parcelas=@count,numero_parcela=1,
                    data_vencimento=@first,data_atualizacao=CURRENT_TIMESTAMP
                WHERE id_transacao=@series AND id_usuario=@user
                """, ("@count", installmentCount), ("@first", firstInstallmentDate.Date),
                ("@series", seriesId), ("@user", CurrentUserId));
            foreach (var item in installments)
                await ExecuteAsync(db, """
                    INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                      parcelado,numero_parcelas,id_transacao_pai,observacoes,data_vencimento,pago,
                      numero_parcela,total_parcelas,recorrente,frequencia,id_fornecedor)
                    VALUES(@account,@card,@category,@user,@amount,@purchaseDate,@description,'despesa',
                      1,@count,@parent,@notes,@dueDate,0,@number,@count,@recurring,@frequency,@supplier)
                    """, ("@account", accountId), ("@card", cardId), ("@category", categoryId),
                    ("@user", CurrentUserId), ("@amount", item.Amount), ("@purchaseDate", purchaseDate),
                    ("@dueDate", item.Date),
                    ("@description", $"{description} - Parcela {item.Number}/{item.Total}"),
                    ("@count", installmentCount), ("@parent", seriesId), ("@notes", notes),
                    ("@number", item.Number), ("@recurring", recurring ? 1 : 0),
                    ("@frequency", frequency), ("@supplier", supplierId));
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task UpdateListedTransactionAsync(long id, string expectedType, string description,
        decimal amount, DateTime date, long categoryId, string? notes,
        DateTime? dueDate = null, bool recurring = false, string? frequency = null,
        IReadOnlyCollection<long>? tagIds = null, long? supplierId = null,
        long? accountId = null, long? cardId = null, bool paid = false,
        DateTime? paymentDate = null)
    {
        description = description.Trim();
        if (description.Length < 2) throw new ArgumentException("Informe uma descrição válida.");
        if (amount <= 0) throw new ArgumentException("O valor deve ser maior que zero.");

        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            var current = await GetTransactionSnapshotAsync(db, id, expectedType);

            if ((accountId is null) == (cardId is null))
                throw new ArgumentException("Selecione uma conta ou um cartão.");

            // Desfaz o efeito financeiro anterior antes de validar/aplicar a nova origem.
            if (expectedType == "receita")
            {
                if (current.AccountId is not null && current.Paid)
                    await ExecuteAsync(db, """
                        UPDATE Contas SET saldo_atual=saldo_atual-@amount
                        WHERE id_conta=@account AND id_usuario=@user
                        """, ("@amount", current.Amount), ("@account", current.AccountId),
                        ("@user", CurrentUserId));
                if (accountId is null)
                    throw new ArgumentException("Receitas devem estar vinculadas a uma conta.");
                if (paid && paymentDate is null)
                    throw new ArgumentException("Informe a data efetiva de recebimento da receita.");
                if (paid)
                    await ExecuteAsync(db, """
                        UPDATE Contas SET saldo_atual=saldo_atual+@amount
                        WHERE id_conta=@account AND id_usuario=@user
                        """, ("@amount", amount), ("@account", accountId), ("@user", CurrentUserId));
            }
            else
            {
                if (current.AccountId is not null && current.Paid)
                    await ExecuteAsync(db, """
                        UPDATE Contas SET saldo_atual=saldo_atual+@amount
                        WHERE id_conta=@account AND id_usuario=@user
                        """, ("@amount", current.Amount), ("@account", current.AccountId),
                        ("@user", CurrentUserId));
                if (current.CardId is not null)
                    await ExecuteAsync(db, """
                        UPDATE CartoesCredito SET limite_utilizado=MAX(0,limite_utilizado-@amount)
                        WHERE id_cartao=@card AND id_usuario=@user
                        """, ("@amount", current.Amount), ("@card", current.CardId),
                        ("@user", CurrentUserId));

                if (accountId is not null && paid)
                {
                    var balance = await DecimalAsync(db, """
                        SELECT saldo_atual FROM Contas
                        WHERE id_conta=@account AND id_usuario=@user AND ativo=1
                        """, ("@account", accountId), ("@user", CurrentUserId));
                    if (balance < amount) throw new InvalidOperationException("Saldo insuficiente para pagar esta conta.");
                    await ExecuteAsync(db, """
                        UPDATE Contas SET saldo_atual=saldo_atual-@amount
                        WHERE id_conta=@account AND id_usuario=@user
                        """, ("@amount", amount), ("@account", accountId), ("@user", CurrentUserId));
                }
                else if (cardId is not null)
                {
                    var available = await DecimalAsync(db, """
                        SELECT limite_credito-limite_utilizado FROM CartoesCredito
                        WHERE id_cartao=@card AND id_usuario=@user AND ativo=1
                        """, ("@card", cardId), ("@user", CurrentUserId));
                    if (available < amount) throw new InvalidOperationException("Limite insuficiente no cartão selecionado.");
                    await ExecuteAsync(db, """
                        UPDATE CartoesCredito SET limite_utilizado=limite_utilizado+@amount
                        WHERE id_cartao=@card AND id_usuario=@user
                        """, ("@amount", amount), ("@card", cardId), ("@user", CurrentUserId));
                }
            }

            await ExecuteAsync(db, """
                UPDATE Transacoes SET descricao=@description,valor=@amount,data=@date,
                  id_conta=@account,id_cartao=@card,id_categoria=@category,observacoes=@notes,
                  data_vencimento=@due,pago=@paid,data_pagamento=@payment,
                  recorrente=@recurring,frequencia=@frequency,id_fornecedor=@supplier,
                  data_atualizacao=CURRENT_TIMESTAMP
                WHERE id_transacao=@id AND id_usuario=@user AND tipo=@type
                """, ("@description", description), ("@amount", amount), ("@date", date.Date),
                ("@account", accountId), ("@card", cardId),
                ("@category", categoryId), ("@notes", notes),
                ("@due", expectedType == "despesa" ? dueDate ?? date.Date : null),
                ("@paid", paid ? 1 : 0), ("@payment", paid ? paymentDate : null),
                ("@recurring", recurring ? 1 : 0), ("@frequency", recurring ? frequency : null),
                ("@supplier", expectedType == "despesa" ? supplierId : null),
                ("@id", id),
                ("@user", CurrentUserId), ("@type", expectedType));
            if (tagIds is not null)
            {
                await ExecuteAsync(db, "DELETE FROM TransacoesTags WHERE id_transacao=@id", ("@id", id));
                foreach (var tagId in tagIds.Distinct())
                    await ExecuteAsync(db, """
                        INSERT INTO TransacoesTags(id_transacao,id_tag)
                        SELECT @transaction,@tag WHERE EXISTS(
                          SELECT 1 FROM Tags WHERE id_tag=@tag AND id_usuario=@user)
                        """, ("@transaction", id), ("@tag", tagId), ("@user", CurrentUserId));
            }
            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem)
                VALUES(@user,'EDITAR','Transacoes',@id,@old,@new,'TransactionEditPage')
                """, ("@user", CurrentUserId), ("@id", id),
                ("@old", $"valor={current.Amount:0.00}"), ("@new", $"valor={amount:0.00}"));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ConvertTransactionToInstallmentsAsync(long id, string description,
        decimal totalAmount, long categoryId, string? notes, int installmentCount,
        DateTime firstInstallmentDate, bool recurring, string? frequency,
        IReadOnlyCollection<long>? tagIds, long? supplierId,
        IReadOnlyList<InstallmentPreview>? editedSchedule = null)
    {
        description = description.Trim();
        if (description.Length < 2) throw new ArgumentException("Informe uma descrição válida.");
        if (totalAmount <= 0) throw new ArgumentException("O valor deve ser maior que zero.");
        if (installmentCount is < 2 or > 36) throw new ArgumentException("Informe entre 2 e 36 parcelas.");
        // Na edição de uma conta existente, a primeira parcela pode estar vencida.
        // AddMonths mantém corretamente a sequência das parcelas seguintes.

        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            await using var sourceCommand = CreateCommand(db, """
                SELECT id_conta,id_cartao,pago,COALESCE(parcelado,0)
                FROM Transacoes WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa'
                """, ("@id", id), ("@user", CurrentUserId));
            await using var reader = await sourceCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("A conta não foi encontrada.");
            long? accountId = reader.IsDBNull(0) ? null : reader.GetInt64(0);
            long? cardId = reader.IsDBNull(1) ? null : reader.GetInt64(1);
            var paid = reader.GetInt32(2) == 1;
            var alreadyInstallment = reader.GetInt32(3) == 1;
            await reader.DisposeAsync();
            if (paid) throw new InvalidOperationException("Uma conta paga não pode ser parcelada.");
            if (alreadyInstallment) throw new InvalidOperationException("Esta conta já pertence a um parcelamento.");

            var installments = editedSchedule is { Count: > 0 }
                ? editedSchedule
                : TransactionSchedulePlanner.Build(totalAmount, installmentCount,
                    firstInstallmentDate.Date, recurring
                        ? TransactionScheduleMode.Recurrence
                        : TransactionScheduleMode.Installment);
            if (installments.Count != installmentCount || installments.Any(item => item.Amount <= 0))
                throw new ArgumentException("A programação informada é inválida.");
            var updated = await ExecuteCountAsync(db, """
                UPDATE Transacoes SET descricao=@description,valor=@amount,data=@date,
                  id_categoria=@category,observacoes=@notes,data_vencimento=@date,
                  parcelado=1,numero_parcelas=@count,numero_parcela=1,total_parcelas=@count,
                  recorrente=@recurring,frequencia=@frequency,id_fornecedor=@supplier,
                  data_atualizacao=CURRENT_TIMESTAMP
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa'
                """, ("@description", description), ("@amount", totalAmount),
                ("@date", firstInstallmentDate.Date), ("@category", categoryId),
                ("@notes", notes), ("@count", installmentCount),
                ("@recurring", recurring ? 1 : 0), ("@frequency", recurring ? frequency : null),
                ("@supplier", supplierId), ("@id", id), ("@user", CurrentUserId));
            if (updated != 1) throw new InvalidOperationException("Não foi possível preparar o parcelamento.");

            await ExecuteAsync(db, "DELETE FROM TransacoesTags WHERE id_transacao=@id", ("@id", id));
            foreach (var item in installments)
            {
                var inserted = await ExecuteCountAsync(db, """
                    INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                      parcelado,numero_parcelas,id_transacao_pai,observacoes,data_vencimento,pago,
                      numero_parcela,total_parcelas,recorrente,frequencia,id_fornecedor)
                    VALUES(@account,@card,@category,@user,@amount,@date,@description,'despesa',
                      1,@count,@parent,@notes,@date,0,@number,@count,@recurring,@frequency,@supplier)
                    """, ("@account", accountId), ("@card", cardId), ("@category", categoryId),
                    ("@user", CurrentUserId), ("@amount", item.Amount), ("@date", item.Date),
                    ("@description", recurring
                        ? $"{description} - Ocorrência {item.Number}/{item.Total}"
                        : $"{description} - Parcela {item.Number}/{item.Total}"),
                    ("@count", installmentCount), ("@parent", id), ("@notes", notes),
                    ("@number", item.Number), ("@recurring", recurring ? 1 : 0),
                    ("@frequency", recurring ? frequency : null), ("@supplier", supplierId));
                if (inserted != 1) throw new InvalidOperationException($"Não foi possível criar a parcela {item.Number}.");
                var childId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid()"));
                foreach (var tagId in tagIds?.Distinct() ?? [])
                    await ExecuteAsync(db, """
                        INSERT INTO TransacoesTags(id_transacao,id_tag)
                        SELECT @transaction,@tag WHERE EXISTS(
                          SELECT 1 FROM Tags WHERE id_tag=@tag AND id_usuario=@user)
                        """, ("@transaction", childId), ("@tag", tagId), ("@user", CurrentUserId));
            }
            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                VALUES(@user,'CONVERTER_PARCELAMENTO','Transacoes',@id,@data,'TransactionEditPage')
                """, ("@user", CurrentUserId), ("@id", id),
                ("@data", $"parcelas={installmentCount}; total={totalAmount:0.00}"));
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task DeleteListedTransactionAsync(long id, string expectedType)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            var current = await GetTransactionSnapshotAsync(db, id, expectedType);
            if (expectedType == "despesa" && current.Paid)
                throw new InvalidOperationException("Uma conta já paga não pode ser excluída nesta tela.");

            if (expectedType == "receita" && current.AccountId is not null && current.Paid)
                await ExecuteAsync(db, """
                    UPDATE Contas SET saldo_atual=saldo_atual-@amount
                    WHERE id_conta=@account AND id_usuario=@user
                    """, ("@amount", current.Amount), ("@account", current.AccountId), ("@user", CurrentUserId));
            else if (expectedType == "despesa" && current.CardId is not null)
                await ExecuteAsync(db, """
                    UPDATE CartoesCredito SET limite_utilizado=MAX(0,limite_utilizado-@amount)
                    WHERE id_cartao=@card AND id_usuario=@user
                    """, ("@amount", current.Amount), ("@card", current.CardId), ("@user", CurrentUserId));

            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,origem)
                VALUES(@user,'EXCLUIR','Transacoes',@id,@old,'ListPage')
                """, ("@user", CurrentUserId), ("@id", id),
                ("@old", $"tipo={expectedType}; valor={current.Amount:0.00}"));
            await ExecuteAsync(db,
                "DELETE FROM Transacoes WHERE id_transacao=@id AND id_usuario=@user AND tipo=@type",
                ("@id", id), ("@user", CurrentUserId), ("@type", expectedType));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<(decimal Amount, long? AccountId, long? CardId, bool Paid)>
        GetTransactionSnapshotAsync(SqliteConnection db, long id, string type)
    {
        await using var command = CreateCommand(db, """
            SELECT valor,id_conta,id_cartao,pago FROM Transacoes
            WHERE id_transacao=@id AND id_usuario=@user AND tipo=@type
            """, ("@id", id), ("@user", CurrentUserId), ("@type", type));
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException("O lançamento não foi encontrado.");
        return (reader.GetDecimal(0), reader.IsDBNull(1) ? null : reader.GetInt64(1),
            reader.IsDBNull(2) ? null : reader.GetInt64(2), reader.GetInt32(3) == 1);
    }

    public async Task<List<ContaPagar>> GetContasPagarAsync(int? mes, int? ano, bool? pagas = false,
        long? supplierId = null, long? cardId = null, long? categoryId = null)
    {
        var items = await QueryAsync(
        """
        WITH RECURSIVE categorias_selecionadas(id_categoria) AS (
          SELECT @category WHERE @category IS NOT NULL
          UNION ALL
          SELECT c.id_categoria FROM Categorias c
          JOIN categorias_selecionadas p ON c.id_categoria_pai=p.id_categoria
          WHERE c.id_usuario=@user
        )
        SELECT t.id_transacao,t.descricao,COALESCE(c.nome_categoria,'Sem categoria'),
               COALESCE(t.data_vencimento,t.data),t.valor,
               CASE WHEN t.pago=1 THEN 'Paga'
                    WHEN date(COALESCE(t.data_vencimento,t.data))<date('now','localtime') THEN 'Vencida'
                    ELSE 'Pendente' END,
               COALESCE(a.nome_conta,cc.nome_cartao,'—'),
               COALESCE(t.numero_parcela,1),COALESCE(t.total_parcelas,1),
               t.id_fornecedor,COALESCE(f.nome_fornecedor,'Sem fornecedor')
        FROM Transacoes t
        LEFT JOIN Categorias c ON c.id_categoria=t.id_categoria
        LEFT JOIN Contas a ON a.id_conta=t.id_conta
        LEFT JOIN CartoesCredito cc ON cc.id_cartao=t.id_cartao
        LEFT JOIN Fornecedores f ON f.id_fornecedor=t.id_fornecedor
        WHERE t.id_usuario=@user AND t.tipo='despesa'
          AND (@paid IS NULL OR COALESCE(t.pago,0)=@paid)
          AND NOT (COALESCE(t.parcelado,0)=1 AND t.id_transacao_pai IS NULL AND EXISTS(
            SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))
          AND (@month IS NULL OR CAST(strftime('%m',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=@month)
          AND (@year IS NULL OR CAST(strftime('%Y',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=@year)
          AND (@supplier IS NULL OR t.id_fornecedor=@supplier)
          AND (@card IS NULL OR t.id_cartao=@card)
          AND (@category IS NULL OR t.id_categoria=@category)
        ORDER BY CASE WHEN t.pago=0 AND date(COALESCE(t.data_vencimento,t.data))<date('now','localtime') THEN 0 ELSE 1 END,
          COALESCE(t.data_vencimento,t.data),t.id_transacao
        """,
        r => new ContaPagar
        {
            Id = r.GetInt64(0), Descricao = r.GetString(1), Categoria = r.GetString(2),
            DataVencimento = r.GetDateTime(3), Valor = r.GetDecimal(4),
            Status = r.GetString(5), ContaNome = r.GetString(6),
            NumeroParcela = r.GetInt32(7), TotalParcelas = r.GetInt32(8),
            IdFornecedor = r.IsDBNull(9) ? null : r.GetInt64(9), FornecedorNome = r.GetString(10)
        }, ("@user", CurrentUserId), ("@month", mes), ("@year", ano),
        ("@paid", pagas is null ? null : pagas.Value ? 1 : 0), ("@supplier", supplierId),
        ("@card", cardId), ("@category", categoryId));
        var tags = await GetTransactionTagsLookupAsync();
        return items.Select(item => new ContaPagar
        {
            Id = item.Id, Descricao = item.Descricao, Categoria = item.Categoria,
            DataVencimento = item.DataVencimento, Valor = item.Valor, Status = item.Status,
            ContaNome = item.ContaNome, IdFornecedor = item.IdFornecedor,
            FornecedorNome = item.FornecedorNome, NumeroParcela = item.NumeroParcela,
            TotalParcelas = item.TotalParcelas, Selecionado = item.Selecionado,
            Tags = tags.GetValueOrDefault(item.Id, [])
        }).ToList();
    }

    public async Task<ResumoContasPagar> GetResumoContasPagarAsync(int? mes, int? ano)
    {
        var items = await GetContasPagarAsync(mes, ano);
        return new(items.Count, items.Sum(x => x.Valor), items.Count(x => x.Status == "Vencida"));
    }

    public Task<List<ContaPagar>> GetRecentPaidExpensesAsync(int limit = 3) => QueryAsync(
        """
        SELECT t.id_transacao,t.descricao,c.nome_categoria,
               COALESCE(t.data_vencimento,t.data),t.valor,'Paga',
                COALESCE(a.nome_conta,cc.nome_cartao,'—'),
                COALESCE(t.numero_parcela,1),COALESCE(t.total_parcelas,1)
        FROM Transacoes t
        JOIN Categorias c ON c.id_categoria=t.id_categoria
        LEFT JOIN Contas a ON a.id_conta=t.id_conta
        LEFT JOIN CartoesCredito cc ON cc.id_cartao=t.id_cartao
        WHERE t.id_usuario=@user AND t.tipo='despesa' AND t.pago=1
          AND NOT (t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
            SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))
        ORDER BY COALESCE(t.data_vencimento,t.data) DESC,t.id_transacao DESC
        LIMIT @limit
        """, r => new ContaPagar
        {
            Id = r.GetInt64(0), Descricao = r.GetString(1), Categoria = r.GetString(2),
            DataVencimento = r.GetDateTime(3), Valor = r.GetDecimal(4),
            Status = r.GetString(5), ContaNome = r.GetString(6),
            NumeroParcela = r.GetInt32(7), TotalParcelas = r.GetInt32(8)
        }, ("@user", CurrentUserId), ("@limit", limit));

    public async Task<HomePaymentStats> GetHomePaymentStatsAsync()
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = start.AddMonths(1);
        var total = Convert.ToInt32(await ScalarAsync(db, """
            SELECT COUNT(*) FROM Transacoes
            WHERE id_usuario=@user AND tipo='despesa'
              AND NOT (parcelado=1 AND id_transacao_pai IS NULL AND EXISTS(
                SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
              AND date(COALESCE(data_vencimento,data))>=date(@start)
              AND date(COALESCE(data_vencimento,data))<date(@end)
            """, ("@user", CurrentUserId), ("@start", start), ("@end", end)) ?? 0);
        var paid = Convert.ToInt32(await ScalarAsync(db, """
            SELECT COUNT(*) FROM Transacoes
            WHERE id_usuario=@user AND tipo='despesa' AND pago=1
              AND NOT (parcelado=1 AND id_transacao_pai IS NULL AND EXISTS(
                SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
              AND date(COALESCE(data_vencimento,data))>=date(@start)
              AND date(COALESCE(data_vencimento,data))<date(@end)
            """, ("@user", CurrentUserId), ("@start", start), ("@end", end)) ?? 0);
        return new(paid, total);
    }

    public async Task<List<CreditCardAnalysis>> GetCreditCardAnalysisAsync(int month, int year)
    {
        var cards = await GetCardsAsync();
        var result = new List<CreditCardAnalysis>();
        foreach (var card in cards)
        {
            var categories = await QueryAsync(
                """
                SELECT COALESCE(c.nome_categoria,'Sem categoria'),COALESCE(c.cor,'#6B7280'),
                       COALESCE(SUM(t.valor),0)
                FROM Transacoes t
                LEFT JOIN Categorias c ON c.id_categoria=t.id_categoria
                WHERE t.id_usuario=@user AND t.id_cartao=@card AND t.tipo='despesa'
                  AND NOT (t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                    SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))
                  -- A competência da análise de cartões é sempre o vencimento do lançamento.
                  -- Não usa data da compra, fechamento ou data registrada na fatura.
                  AND CAST(strftime('%m',t.data_vencimento) AS INTEGER)=@month
                  AND CAST(strftime('%Y',t.data_vencimento) AS INTEGER)=@year
                GROUP BY COALESCE(c.id_categoria,0),
                         COALESCE(c.nome_categoria,'Sem categoria'),
                         COALESCE(c.cor,'#6B7280')
                ORDER BY SUM(t.valor) DESC
                """, r => new CardCategorySpend(r.GetString(0), r.GetString(1), r.GetDecimal(2)),
                ("@user", CurrentUserId), ("@card", card.Id), ("@month", month), ("@year", year));
            var invoice = categories.Sum(x => x.Amount);
            var used = (await QueryAsync(
                """
                SELECT COALESCE(SUM(t.valor),0)
                FROM Transacoes t
                WHERE t.id_usuario=@user AND t.id_cartao=@card AND t.tipo='despesa'
                  AND COALESCE(t.pago,0)=0
                  AND NOT (t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                    SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))
                """, r => r.GetDecimal(0),
                ("@user", CurrentUserId), ("@card", card.Id))).Single();
            result.Add(new(card.Id, card.Name, invoice, card.CreditLimit,
                Math.Max(0, card.CreditLimit - used), card.ClosingDay, card.DueDay, categories));
        }
        return result;
    }

    public async Task<MonthlyOverview> GetMonthlyOverviewAsync(int month, int year)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        decimal income, paidExpenses, pendingExpenses, accountExpenses, cardInvoices, total;
        await using (var command = CreateCommand(db, """
            SELECT
              COALESCE(SUM(CASE WHEN tipo='receita' AND COALESCE(pago,0)=1
                AND data_pagamento IS NOT NULL
                AND date(data_pagamento)>=date(@start) AND date(data_pagamento)<date(@end)
                AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(
                  SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
                THEN valor ELSE 0 END),0) income,
              COALESCE(SUM(CASE WHEN tipo='despesa' AND COALESCE(pago,0)=1
                AND date(COALESCE(data_vencimento,data))>=date(@start)
                AND date(COALESCE(data_vencimento,data))<date(@end)
                AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(
                  SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
                THEN valor ELSE 0 END),0) paid_expenses,
              COALESCE(SUM(CASE WHEN tipo='despesa' AND COALESCE(pago,0)=0
                AND date(COALESCE(data_vencimento,data))>=date(@start)
                AND date(COALESCE(data_vencimento,data))<date(@end)
                AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(
                  SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
                THEN valor ELSE 0 END),0) pending_expenses,
              COALESCE(SUM(CASE WHEN tipo='despesa' AND id_conta IS NOT NULL
                AND date(COALESCE(data_vencimento,data))>=date(@start)
                AND date(COALESCE(data_vencimento,data))<date(@end)
                AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(
                  SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
                THEN valor ELSE 0 END),0) account_expenses,
              COALESCE(SUM(CASE WHEN tipo='despesa' AND id_cartao IS NOT NULL
                AND date(COALESCE(data_vencimento,data))>=date(@start)
                AND date(COALESCE(data_vencimento,data))<date(@end)
                AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(
                  SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
                THEN valor ELSE 0 END),0) card_expenses,
              COALESCE(SUM(CASE WHEN tipo='despesa'
                AND date(COALESCE(data_vencimento,data))>=date(@start)
                AND date(COALESCE(data_vencimento,data))<date(@end)
                AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(
                  SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
                THEN valor ELSE 0 END),0) total_expenses
            FROM Transacoes WHERE id_usuario=@user
            """, ("@start", start.Date), ("@end", end.Date), ("@user", CurrentUserId)))
        await using (var reader = await command.ExecuteReaderAsync())
        {
            await reader.ReadAsync();
            income = reader.GetDecimal(0); paidExpenses = reader.GetDecimal(1);
            pendingExpenses = reader.GetDecimal(2); accountExpenses = reader.GetDecimal(3);
            cardInvoices = reader.GetDecimal(4); total = reader.GetDecimal(5);
        }
        var realized = await GetMonthlyRealizedTotalsAsync(db, CurrentUserId, month, year);
        income = realized.TotalReceitas;
        paidExpenses = realized.TotalDespesas;
        var bankBalance = await GetTotalActiveAccountsBalanceAsync(db, CurrentUserId);
        var remaining = realized.ResultadoMes;
        var commitment = income <= 0 ? (total > 0 ? 100 : 0) : total / income * 100;
        var today = DateTime.Today;
        var isCurrentMonth = month == today.Month && year == today.Year;
        var daysRemaining = isCurrentMonth
            ? Math.Max(1, DateTime.DaysInMonth(year, month) - today.Day + 1)
            : DateTime.DaysInMonth(year, month);
        System.Diagnostics.Debug.WriteLine(
            $"[MonthlyOverview] {year:D4}-{month:D2}: receitas={income:0.00}; pagas={paidExpenses:0.00}; " +
            $"pendentes={pendingExpenses:0.00}; cartao={cardInvoices:0.00}; total={total:0.00}; saldo={remaining:0.00}");
        return new(income, paidExpenses, pendingExpenses, accountExpenses, cardInvoices, bankBalance,
            total, remaining, commitment, remaining / daysRemaining, daysRemaining);
    }

    public Task MarcarComoPagaAsync(long idTransacao) =>
        MarcarMultiplasComoPagaAsync([idTransacao]);

    public async Task EstornarPagamentoAsync(long idTransacao)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            decimal amount;
            long? accountId;
            await using (var select = CreateCommand(db, """
                SELECT valor,id_conta FROM Transacoes
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa' AND pago=1
                """, ("@id", idTransacao), ("@user", CurrentUserId)))
            await using (var reader = await select.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException("A conta não está paga ou não existe.");
                amount = reader.GetDecimal(0);
                accountId = reader.IsDBNull(1) ? null : reader.GetInt64(1);
            }

            if (accountId is not null)
                await ExecuteAsync(db, """
                    UPDATE Contas SET saldo_atual=saldo_atual+@amount
                    WHERE id_conta=@account AND id_usuario=@user
                    """, ("@amount", amount), ("@account", accountId), ("@user", CurrentUserId));

            await ExecuteAsync(db, """
                UPDATE Transacoes SET pago=0,data_pagamento=NULL,data_atualizacao=CURRENT_TIMESTAMP
                WHERE id_transacao=@id AND id_usuario=@user AND pago=1
                """, ("@id", idTransacao), ("@user", CurrentUserId));
            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                VALUES(@user,'ESTORNAR_PAGAMENTO','Transacoes',@id,'pago=0','AccountsPayablePage')
                """, ("@user", CurrentUserId), ("@id", idTransacao));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<List<Fornecedor>> GetSuppliersAsync(bool includeInactive = false, string? search = null) =>
        QueryAsync("""
            SELECT id_fornecedor,id_usuario,nome_fornecedor,COALESCE(ativo,1),
              COALESCE(data_criacao,CURRENT_TIMESTAMP)
            FROM Fornecedores
            WHERE id_usuario=@user AND (@all=1 OR COALESCE(ativo,1)=1)
              AND (@search='' OR nome_fornecedor LIKE '%' || @search || '%' COLLATE NOCASE)
            ORDER BY COALESCE(ativo,1) DESC,nome_fornecedor COLLATE NOCASE
            """, r => new Fornecedor
            {
                IdFornecedor = r.GetInt64(0), IdUsuario = r.GetInt64(1),
                NomeFornecedor = r.GetString(2), Ativo = r.GetInt64(3), DataCriacao = r.GetString(4)
            }, ("@user", CurrentUserId), ("@all", includeInactive ? 1 : 0),
            ("@search", search?.Trim() ?? ""));

    public async Task<long> SaveSupplierAsync(long? id, string name, bool active)
    {
        var creating = id is null;
        name = name.Trim();
        if (name.Length < 2) throw new ArgumentException("Informe um nome de fornecedor válido.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        var duplicate = Convert.ToInt32(await ScalarAsync(db, """
            SELECT COUNT(*) FROM Fornecedores WHERE id_usuario=@user
              AND lower(trim(nome_fornecedor))=lower(trim(@name)) AND (@id IS NULL OR id_fornecedor<>@id)
            """, ("@user", CurrentUserId), ("@name", name), ("@id", id))) > 0;
        if (duplicate) throw new InvalidOperationException("Já existe um fornecedor com este nome.");
        if (id is null)
        {
            await ExecuteAsync(db, "INSERT INTO Fornecedores(id_usuario,nome_fornecedor,ativo) VALUES(@user,@name,@active)",
                ("@user", CurrentUserId), ("@name", name), ("@active", active ? 1 : 0));
            id = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));
        }
        else
        {
            await ExecuteAsync(db, """
                UPDATE Fornecedores SET nome_fornecedor=@name,ativo=@active
                WHERE id_fornecedor=@id AND id_usuario=@user
                """, ("@name", name), ("@active", active ? 1 : 0), ("@id", id), ("@user", CurrentUserId));
            var changed = Convert.ToInt32(await ScalarAsync(db, "SELECT changes();"));
            if (changed == 0) throw new InvalidOperationException("Fornecedor não encontrado.");
        }
        await ExecuteAsync(db, """
            INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
            VALUES(@user,@action,'Fornecedores',@id,@name,'SupplierManagementPage')
            """, ("@user", CurrentUserId), ("@action", creating ? "CRIAR" : "SALVAR"),
            ("@id", id), ("@name", name));
        return id.Value;
    }

    public async Task DeleteSupplierAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        var linked = Convert.ToInt32(await ScalarAsync(db,
            "SELECT COUNT(*) FROM Transacoes WHERE id_fornecedor=@id AND id_usuario=@user",
            ("@id", id), ("@user", CurrentUserId)));
        var sql = linked > 0
            ? "UPDATE Fornecedores SET ativo=0 WHERE id_fornecedor=@id AND id_usuario=@user"
            : "DELETE FROM Fornecedores WHERE id_fornecedor=@id AND id_usuario=@user";
        await ExecuteAsync(db, sql, ("@id", id), ("@user", CurrentUserId));
        if (Convert.ToInt32(await ScalarAsync(db, "SELECT changes();")) == 0)
            throw new InvalidOperationException("Fornecedor não encontrado.");
        await ExecuteAsync(db, """
            INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
            VALUES(@user,@action,'Fornecedores',@id,@details,'SupplierManagementPage')
            """, ("@user", CurrentUserId), ("@action", linked > 0 ? "DESATIVAR" : "EXCLUIR"),
            ("@id", id), ("@details", linked > 0 ? "Fornecedor vinculado; desativado." : "Fornecedor excluído."));
    }

    public async Task MarcarMultiplasComoPagaAsync(IReadOnlyCollection<long> listaIds, long? paymentAccountId = null)
    {
        var ids = listaIds.Distinct().ToArray();
        if (ids.Length == 0)
            throw new ArgumentException("Selecione pelo menos uma conta.");

        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            foreach (var id in ids)
            {
                decimal amount;
                long? accountId;
                string description;
                await using (var select = CreateCommand(db, """
                    SELECT valor,id_conta,descricao FROM Transacoes
                    WHERE id_transacao=@id AND id_usuario=@user AND tipo='despesa' AND pago=0
                    """, ("@id", id), ("@user", CurrentUserId)))
                await using (var reader = await select.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        throw new InvalidOperationException($"A conta #{id} não está pendente ou não existe.");
                    amount = reader.GetDecimal(0);
                    accountId = reader.IsDBNull(1) ? null : reader.GetInt64(1);
                    description = reader.GetString(2);
                }

                var accountToDebit = paymentAccountId ?? accountId;
                if (accountToDebit is not null)
                {
                    await using var debit = CreateCommand(db, """
                        UPDATE Contas SET saldo_atual=saldo_atual-@amount
                        WHERE id_conta=@account AND id_usuario=@user AND saldo_atual>=@amount
                        """, ("@amount", amount), ("@account", accountToDebit), ("@user", CurrentUserId));
                    if (await debit.ExecuteNonQueryAsync() != 1)
                        throw new InvalidOperationException($"Saldo insuficiente para pagar “{description}”.");
                }

                await ExecuteAsync(db, """
                    UPDATE Transacoes SET id_conta=COALESCE(@account,id_conta),
                      pago=1,data_pagamento=date('now','localtime'),
                      data_atualizacao=CURRENT_TIMESTAMP
                    WHERE id_transacao=@id AND id_usuario=@user AND pago=0
                    """, ("@account", paymentAccountId), ("@id", id), ("@user", CurrentUserId));
                await ExecuteAsync(db, """
                    INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                    VALUES(@user,'PAGAR','Transacoes',@id,@data,'AccountsPayablePage')
                    """, ("@user", CurrentUserId), ("@id", id),
                    ("@data", $"pago=1; valor={amount:0.00}"));
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>Adds a transaction and updates its funding source atomically.</summary>
    public async Task AddTransactionAsync(TransactionDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Description))
            throw new ArgumentException("Informe a descrição.");
        if (draft.Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");
        if (draft.Type == "transferencia")
        {
            if (draft.AccountId is null || draft.DestinationAccountId is null)
                throw new ArgumentException("Selecione as contas de origem e destino.");
            await AddTransferAsync(draft.AccountId.Value, draft.DestinationAccountId.Value,
                draft.Amount, draft.Date, draft.Description, draft.Notes);
            return;
        }
        if (draft.AccountId is not null && draft.CardId is not null)
            throw new ArgumentException("Selecione apenas uma origem.");
        if (draft.Type == "receita" && draft.AccountId is null)
            throw new ArgumentException("Selecione a conta que receberá a receita.");

        if (draft.Type == "receita" && draft.Recurring)
        {
            await AddRecurringIncomeAsync(draft);
            return;
        }
        if (draft.Type == "despesa" && draft.Recurring)
        {
            await AddRecurringExpenseAsync(draft);
            return;
        }

        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            var dueDate = draft.Type == "despesa" ? (draft.DueDate ?? draft.Date).Date : (DateTime?)null;
            // Uma despesa futura só entra no caixa quando for efetivamente baixada.
            var effectiveExpensePayment = draft.Type == "despesa" && draft.Paid && draft.AccountId is not null;
            var realizedIncome = draft.Type == "receita" && draft.Paid;
            if (realizedIncome && draft.PaymentDate is null)
                throw new ArgumentException("Informe a data efetiva de recebimento da receita.");
            if (draft.Type == "despesa" && (effectiveExpensePayment || draft.CardId is not null))
                await ValidateSourceFundsAsync(db, draft);
            var inserted = await ExecuteCountAsync(db, """
                INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                  observacoes,data_vencimento,pago,data_pagamento,recorrente,frequencia,id_fornecedor)
                VALUES(@account,@card,@category,@user,@amount,@date,@description,@type,@notes,@due,
                  @paid,@payment,@recurring,@frequency,@supplier)
                """, ("@account", draft.AccountId), ("@card", draft.CardId), ("@category", draft.CategoryId),
                ("@user", CurrentUserId),
                ("@amount", draft.Amount), ("@date", draft.Date), ("@description", draft.Description.Trim()),
                ("@type", draft.Type), ("@notes", draft.Notes),
                ("@due", dueDate),
                ("@paid", realizedIncome || effectiveExpensePayment ? 1 : 0),
                ("@payment", realizedIncome || effectiveExpensePayment ? draft.PaymentDate : null),
                ("@recurring", draft.Recurring ? 1 : 0), ("@frequency", draft.Frequency),
                ("@supplier", draft.Type == "despesa" ? draft.SupplierId : null));
            if (inserted != 1)
                throw new InvalidOperationException("O lançamento não foi gravado no banco de dados.");
            if (draft.AccountId is not null && realizedIncome)
                await ExecuteAsync(db, "UPDATE Contas SET saldo_atual=saldo_atual+@delta WHERE id_conta=@id AND id_usuario=@user",
                    ("@delta", draft.Amount), ("@id", draft.AccountId), ("@user", CurrentUserId));
            else if (draft.AccountId is not null && draft.Type == "despesa" && effectiveExpensePayment)
                await ExecuteAsync(db, "UPDATE Contas SET saldo_atual=saldo_atual-@delta WHERE id_conta=@id AND id_usuario=@user",
                    ("@delta", draft.Amount), ("@id", draft.AccountId), ("@user", CurrentUserId));
            else if (draft.CardId is not null)
                await ExecuteAsync(db, "UPDATE CartoesCredito SET limite_utilizado=limite_utilizado+@amount WHERE id_cartao=@id AND id_usuario=@user",
                    ("@amount", draft.Amount), ("@id", draft.CardId), ("@user", CurrentUserId));
            var transactionId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));
            var persisted = Convert.ToInt64(await ScalarAsync(db,
                "SELECT COUNT(*) FROM Transacoes WHERE id_transacao=@id AND id_usuario=@user",
                ("@id", transactionId), ("@user", CurrentUserId)) ?? 0);
            if (persisted != 1)
                throw new InvalidOperationException("Não foi possível confirmar o lançamento gravado.");
            foreach (var tagId in draft.TagIds?.Distinct() ?? [])
                await ExecuteAsync(db, """
                    INSERT INTO TransacoesTags(id_transacao,id_tag)
                    SELECT @transaction,@tag WHERE EXISTS(
                      SELECT 1 FROM Tags WHERE id_tag=@tag AND id_usuario=@user)
                    """, ("@transaction", transactionId), ("@tag", tagId), ("@user", CurrentUserId));
            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                VALUES(@user,'CRIAR','Transacoes',@id,@data,'TransactionFormPage')
                """, ("@user", CurrentUserId), ("@id", transactionId),
                ("@data", $"tipo={draft.Type}; valor={draft.Amount:0.00}"));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Saves a grouping transaction and all installments atomically. The parent is
    /// an audit/grouping row; balances and dashboard totals use the child rows.
    /// </summary>
    public async Task<InstallmentSaveResult> SaveInstallmentTransactionAsync(
        TransactionDraft draft, int installmentCount, DateTime firstInstallmentDate,
        IReadOnlyList<InstallmentPreview>? editedSchedule = null)
    {
        ValidateDraft(draft);
        if (draft.Type != "despesa")
            throw new ArgumentException("Apenas despesas podem ser parceladas.");
        if (installmentCount is < 2 or > 36)
            throw new ArgumentException("O número de parcelas deve estar entre 2 e 36.");
        // Parcelamentos importados ou cadastrados posteriormente podem começar em meses passados.

        var installments = editedSchedule is { Count: > 0 }
            ? editedSchedule
            : TransactionSchedulePlanner.Build(draft.Amount, installmentCount,
                firstInstallmentDate, TransactionScheduleMode.Installment);
        if (installments.Count != installmentCount || installments.Any(item => item.Amount <= 0) ||
            installments.Sum(item => item.Amount) != draft.Amount)
            throw new ArgumentException("As parcelas devem ser positivas e somar exatamente o valor total.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            if (draft.CardId is not null)
                await ValidateSourceFundsAsync(db, draft);
            await ExecuteAsync(db, """
                INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                  parcelado,numero_parcelas,observacoes,data_vencimento,pago,numero_parcela,total_parcelas,id_fornecedor)
                VALUES(@account,@card,@category,@user,@amount,@purchaseDate,@description,@type,1,@count,@notes,
                  @dueDate,0,1,@count,@supplier)
                """, ("@account", draft.AccountId), ("@card", draft.CardId), ("@category", draft.CategoryId),
                ("@user", CurrentUserId), ("@amount", draft.Amount), ("@purchaseDate", draft.Date.Date),
                ("@dueDate", firstInstallmentDate.Date),
                ("@description", draft.Description.Trim()), ("@type", draft.Type),
                ("@count", installmentCount), ("@notes", draft.Notes), ("@supplier", draft.SupplierId));
            var parentId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));

            foreach (var item in installments)
                await ExecuteAsync(db, """
                    INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                      parcelado,numero_parcelas,id_transacao_pai,observacoes,data_vencimento,pago,
                      numero_parcela,total_parcelas,id_fornecedor)
                    VALUES(@account,@card,@category,@user,@amount,@purchaseDate,@description,@type,1,@count,@parent,
                      @notes,@dueDate,0,@number,@count,@supplier)
                    """, ("@account", draft.AccountId), ("@card", draft.CardId), ("@category", draft.CategoryId),
                    ("@user", CurrentUserId), ("@amount", item.Amount), ("@purchaseDate", draft.Date.Date),
                    ("@dueDate", item.Date),
                    ("@description", $"{draft.Description.Trim()} - Parcela {item.Number}/{item.Total}"),
                    ("@type", draft.Type), ("@count", installmentCount), ("@parent", parentId),
                    ("@notes", draft.Notes), ("@number", item.Number), ("@supplier", draft.SupplierId));

            await ApplySourceDeltaAsync(db, draft);
            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                VALUES(@user,'CRIAR_PARCELAMENTO','Transacoes',@id,@data,'TransactionFormPage')
                """, ("@user", CurrentUserId), ("@id", parentId),
                ("@data", $"{installmentCount} parcelas; total={draft.Amount:0.00}"));
            await transaction.CommitAsync();
            return new(parentId, installmentCount, draft.Amount);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static IReadOnlyList<InstallmentPreview> BuildInstallments(
        decimal total, int count, DateTime firstDate)
    {
        var regular = Math.Round(total / count, 2, MidpointRounding.AwayFromZero);
        var result = new List<InstallmentPreview>(count);
        decimal allocated = 0;
        for (var number = 1; number <= count; number++)
        {
            var amount = number == count ? total - allocated : regular;
            allocated += amount;
            result.Add(new(number, count, firstDate.Date.AddMonths(number - 1), amount));
        }
        return result;
    }

    private static void ValidateDraft(TransactionDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Description))
            throw new ArgumentException("Informe a descrição.");
        if (draft.Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");
        if (draft.AccountId is not null && draft.CardId is not null)
            throw new ArgumentException("Selecione apenas uma origem.");
        if (draft.Type == "receita" && draft.AccountId is null)
            throw new ArgumentException("Selecione a conta que receberá a receita.");
    }

    private async Task ValidateSourceFundsAsync(SqliteConnection db, TransactionDraft draft)
    {
        if (draft.AccountId is not null)
        {
            var balance = await DecimalAsync(db,
                "SELECT COALESCE(MAX(saldo_atual),-1) FROM Contas WHERE id_conta=@id AND id_usuario=@user AND ativo=1",
                ("@id", draft.AccountId), ("@user", CurrentUserId));
            if (balance < draft.Amount)
                throw new InvalidOperationException("Saldo insuficiente na conta selecionada.");
        }
        else
        {
            var available = await DecimalAsync(db, """
                SELECT COALESCE(MAX(limite_credito-limite_utilizado),-1)
                FROM CartoesCredito WHERE id_cartao=@id AND id_usuario=@user AND ativo=1
                """, ("@id", draft.CardId), ("@user", CurrentUserId));
            if (available < draft.Amount)
                throw new InvalidOperationException("Limite insuficiente no cartão selecionado.");
        }
    }

    private async Task ApplySourceDeltaAsync(SqliteConnection db, TransactionDraft draft)
    {
        if (draft.CardId is not null)
            await ExecuteAsync(db, """
                UPDATE CartoesCredito SET limite_utilizado=limite_utilizado+@amount
                WHERE id_cartao=@id AND id_usuario=@user
                """, ("@amount", draft.Amount), ("@id", draft.CardId), ("@user", CurrentUserId));
    }

    // ============================================================
    // METAS, TAGS, LEMBRETES, FATURAS E RELATÓRIOS
    // ============================================================

    /// <summary>Lista as metas financeiras do usuário, ordenadas pela data-alvo.</summary>
    public Task<List<MetaItem>> GetMetasAsync() => QueryAsync(
        """
        SELECT id_meta,id_usuario,nome_meta,valor_objetivo,valor_atual,data_inicio,data_meta,
          categoria,prioridade,status,observacoes
        FROM Metas WHERE id_usuario=@user ORDER BY
          CASE status WHEN 'em_andamento' THEN 0 WHEN 'concluida' THEN 1 ELSE 2 END,data_meta
        """, r => new MetaItem(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetDecimal(3),
            r.GetDecimal(4), r.GetDateTime(5), r.GetDateTime(6),
            r.IsDBNull(7) ? null : r.GetString(7), r.GetString(8), r.GetString(9),
            r.IsDBNull(10) ? null : r.GetString(10)), ("@user", CurrentUserId));

    /// <summary>Cria ou atualiza uma meta financeira com auditoria.</summary>
    public async Task SaveMetaAsync(long? id, string name, decimal target, decimal current,
        DateTime start, DateTime due, string? category, string priority, string status, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Informe o nome da meta.");
        if (target <= 0) throw new ArgumentException("O valor objetivo deve ser maior que zero.");
        if (current < 0 || current > target)
            throw new ArgumentException("O valor atual deve estar entre zero e o valor objetivo.");
        if (due.Date < start.Date)
            throw new ArgumentException("A data da meta não pode ser anterior à data inicial.");
        if (priority is not ("baixa" or "media" or "alta"))
            throw new ArgumentException("Selecione uma prioridade válida.");
        if (status is not ("em_andamento" or "concluida" or "cancelada"))
            throw new ArgumentException("Selecione um status válido.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        if (id is null)
            await ExecuteAsync(db, """
                INSERT INTO Metas(id_usuario,nome_meta,valor_objetivo,valor_atual,data_inicio,data_meta,
                  categoria,prioridade,status,observacoes)
                VALUES(@user,@name,@target,@current,@start,@due,@category,@priority,@status,@notes)
                """, ("@user", CurrentUserId), ("@name", name.Trim()), ("@target", target),
                ("@current", current), ("@start", Iso(start)), ("@due", Iso(due)),
                ("@category", category), ("@priority", priority), ("@status", status), ("@notes", notes));
        else
            await ExecuteAsync(db, """
                UPDATE Metas SET nome_meta=@name,valor_objetivo=@target,valor_atual=@current,
                  data_inicio=@start,data_meta=@due,categoria=@category,prioridade=@priority,
                  status=@status,observacoes=@notes,data_atualizacao=CURRENT_TIMESTAMP
                WHERE id_meta=@id AND id_usuario=@user
                """, ("@name", name.Trim()), ("@target", target), ("@current", current),
                ("@start", Iso(start)), ("@due", Iso(due)), ("@category", category),
                ("@priority", priority), ("@status", status), ("@notes", notes),
                ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, id is null ? "CRIAR" : "EDITAR", "Metas", id, $"meta={name}");
    }

    /// <summary>Exclui uma meta pertencente ao usuário atual.</summary>
    public async Task DeleteMetaAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, "DELETE FROM Metas WHERE id_meta=@id AND id_usuario=@user",
            ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, "EXCLUIR", "Metas", id, null);
    }

    private async Task AddRecurringIncomeAsync(TransactionDraft draft)
    {
        if (draft.AccountId is null)
            throw new ArgumentException("Selecione a conta que receberá o salário.");

        var frequency = string.Equals(draft.Frequency, "anual", StringComparison.OrdinalIgnoreCase)
            ? "anual" : "mensal";
        var occurrenceCount = Math.Clamp(draft.RecurrenceCount ?? (frequency == "anual" ? 5 : 12), 1, 36);

        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            long? firstId = null;
            for (var index = 0; index < occurrenceCount; index++)
            {
                var receiptDate = frequency == "anual"
                    ? draft.Date.Date.AddYears(index)
                    : draft.Date.Date.AddMonths(index);
                var received = index == 0 && draft.Paid;
                var paymentDate = received ? draft.PaymentDate : null;
                if (received && paymentDate is null)
                    throw new ArgumentException("Informe a data efetiva de recebimento da receita.");
                var inserted = await ExecuteCountAsync(db, """
                    INSERT INTO Transacoes(id_conta,id_categoria,id_usuario,valor,data,descricao,tipo,
                      observacoes,pago,data_pagamento,recorrente,frequencia)
                    VALUES(@account,@category,@user,@amount,@date,@description,'receita',@notes,
                      @paid,@payment,1,@frequency)
                    """, ("@account", draft.AccountId), ("@category", draft.CategoryId),
                    ("@user", CurrentUserId), ("@amount", draft.Amount), ("@date", receiptDate),
                    ("@description", draft.Description.Trim()), ("@notes", draft.Notes),
                    ("@paid", received ? 1 : 0), ("@payment", paymentDate),
                    ("@frequency", frequency));
                if (inserted != 1)
                    throw new InvalidOperationException($"Não foi possível criar a recorrência de {receiptDate:MM/yyyy}.");

                var transactionId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));
                firstId ??= transactionId;
                foreach (var tagId in draft.TagIds?.Distinct() ?? [])
                    await ExecuteAsync(db, """
                        INSERT INTO TransacoesTags(id_transacao,id_tag)
                        SELECT @transaction,@tag WHERE EXISTS(
                          SELECT 1 FROM Tags WHERE id_tag=@tag AND id_usuario=@user)
                        """, ("@transaction", transactionId), ("@tag", tagId), ("@user", CurrentUserId));
            }

            if (draft.Paid)
                await ExecuteAsync(db,
                    "UPDATE Contas SET saldo_atual=saldo_atual+@amount WHERE id_conta=@account AND id_usuario=@user",
                    ("@amount", draft.Amount), ("@account", draft.AccountId), ("@user", CurrentUserId));
            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                VALUES(@user,'CRIAR_RECORRENCIA','Transacoes',@id,@data,'TransactionFormPage')
                """, ("@user", CurrentUserId), ("@id", firstId),
                ("@data", $"frequencia={frequency}; ocorrencias={occurrenceCount}; valor={draft.Amount:0.00}"));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task AddRecurringExpenseAsync(TransactionDraft draft)
    {
        var frequency = string.Equals(draft.Frequency, "anual", StringComparison.OrdinalIgnoreCase)
            ? "anual" : "mensal";
        var occurrenceCount = Math.Clamp(draft.RecurrenceCount ?? (frequency == "anual" ? 5 : 12), 1, 36);
        var firstDueDate = (draft.DueDate ?? draft.Date).Date;

        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            if (draft.CardId is not null)
                await ValidateSourceFundsAsync(db, draft);

            long? firstId = null;
            for (var index = 0; index < occurrenceCount; index++)
            {
                var transactionDate = frequency == "anual"
                    ? draft.Date.Date.AddYears(index)
                    : draft.Date.Date.AddMonths(index);
                var dueDate = draft.RecurrenceDueDates is { Count: > 0 } && index < draft.RecurrenceDueDates.Count
                    ? draft.RecurrenceDueDates[index].Date
                    : frequency == "anual" ? firstDueDate.AddYears(index) : firstDueDate.AddMonths(index);
                var inserted = await ExecuteCountAsync(db, """
                    INSERT INTO Transacoes(id_conta,id_cartao,id_categoria,id_usuario,valor,data,
                      descricao,tipo,observacoes,data_vencimento,pago,data_pagamento,recorrente,
                      frequencia,id_fornecedor)
                    VALUES(@account,@card,@category,@user,@amount,@date,@description,'despesa',
                      @notes,@due,@paid,@payment,1,@frequency,@supplier)
                    """, ("@account", draft.AccountId), ("@card", draft.CardId),
                    ("@category", draft.CategoryId), ("@user", CurrentUserId),
                    ("@amount", draft.RecurrenceAmounts is { Count: > 0 } && index < draft.RecurrenceAmounts.Count
                        ? draft.RecurrenceAmounts[index] : draft.Amount), ("@date", transactionDate),
                    ("@description", draft.Description.Trim()), ("@notes", draft.Notes),
                    ("@due", dueDate), ("@paid", 0), ("@payment", null),
                    ("@frequency", frequency), ("@supplier", draft.SupplierId));
                if (inserted != 1)
                    throw new InvalidOperationException($"Não foi possível criar a conta de {dueDate:MM/yyyy}.");

                var transactionId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));
                firstId ??= transactionId;
                foreach (var tagId in draft.TagIds?.Distinct() ?? [])
                    await ExecuteAsync(db, """
                        INSERT INTO TransacoesTags(id_transacao,id_tag)
                        SELECT @transaction,@tag WHERE EXISTS(
                          SELECT 1 FROM Tags WHERE id_tag=@tag AND id_usuario=@user)
                        """, ("@transaction", transactionId), ("@tag", tagId), ("@user", CurrentUserId));
            }

            if (draft.CardId is not null)
                await ExecuteAsync(db, """
                    UPDATE CartoesCredito SET limite_utilizado=limite_utilizado+@amount
                    WHERE id_cartao=@card AND id_usuario=@user
                    """, ("@amount", draft.Amount), ("@card", draft.CardId),
                    ("@user", CurrentUserId));

            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                VALUES(@user,'CRIAR_RECORRENCIA','Transacoes',@id,@data,'TransactionFormPage')
                """, ("@user", CurrentUserId), ("@id", firstId),
                ("@data", $"tipo=despesa; frequencia={frequency}; ocorrencias={occurrenceCount}; valor={draft.Amount:0.00}"));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task AddMetaContributionAsync(long id, decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("O aporte deve ser maior que zero.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        var affected = await ExecuteCountAsync(db, """
            UPDATE Metas SET valor_atual=MIN(valor_objetivo,valor_atual+@amount),
              status=CASE WHEN valor_atual+@amount>=valor_objetivo THEN 'concluida' ELSE status END,
              data_atualizacao=CURRENT_TIMESTAMP
            WHERE id_meta=@id AND id_usuario=@user AND status='em_andamento'
            """, ("@amount", amount), ("@id", id), ("@user", CurrentUserId));
        if (affected != 1) throw new InvalidOperationException("A meta não está disponível para aporte.");
        await WriteLogAsync(db, "APORTE", "Metas", id, $"valor={amount:0.00}");
    }

    /// <summary>Lista todas as tags do usuário.</summary>
    public Task<List<TagItem>> GetTagsAsync() => QueryAsync(
        "SELECT id_tag,id_usuario,nome_tag,cor,COALESCE(icone,'tag') FROM Tags WHERE id_usuario=@user ORDER BY nome_tag",
        r => new TagItem(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3), r.GetString(4)),
        ("@user", CurrentUserId));

    public Task<List<Tag>> GetTagsAsync(long idUsuario)
    {
        if (idUsuario != CurrentUserId) return Task.FromResult(new List<Tag>());
        return QueryAsync(
            "SELECT id_tag,id_usuario,nome_tag,COALESCE(cor,@color),COALESCE(icone,'tag') FROM Tags WHERE id_usuario=@user ORDER BY nome_tag",
            reader => new Tag(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4)),
            ("@color", BlingPalette.PrimaryHex), ("@user", idUsuario));
    }

    /// <summary>Cria ou atualiza uma tag.</summary>
    public async Task SaveTagAsync(long? id, string name, string color, string icon = "tag")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Informe o nome da tag.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        if (id is null)
            await ExecuteAsync(db, "INSERT INTO Tags(id_usuario,nome_tag,cor,icone) VALUES(@user,@name,@color,@icon)",
                ("@user", CurrentUserId), ("@name", name.Trim()), ("@color", color), ("@icon", icon));
        else
            await ExecuteAsync(db, "UPDATE Tags SET nome_tag=@name,cor=@color,icone=@icon WHERE id_tag=@id AND id_usuario=@user",
                ("@name", name.Trim()), ("@color", color), ("@icon", icon), ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, id is null ? "CRIAR" : "EDITAR", "Tags", id, $"tag={name}");
    }

    public Task SaveTagAsync(Tag tag)
    {
        if (tag.IdUsuario != CurrentUserId)
            throw new InvalidOperationException("A tag não pertence ao usuário autenticado.");
        return SaveTagAsync(tag.Id <= 0 ? null : tag.Id, tag.Nome,
            string.IsNullOrWhiteSpace(tag.Cor) ? BlingPalette.PrimaryHex : tag.Cor,
            string.IsNullOrWhiteSpace(tag.Icone) ? "tag" : tag.Icone);
    }

    /// <summary>Exclui uma tag e seus vínculos N:N.</summary>
    public async Task DeleteTagAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, "DELETE FROM Tags WHERE id_tag=@id AND id_usuario=@user",
            ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, "EXCLUIR", "Tags", id, null);
    }

    /// <summary>Substitui atomicamente as tags vinculadas a uma transação.</summary>
    public async Task SetTransactionTagsAsync(long transactionId, IEnumerable<long> tagIds)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            var owner = Convert.ToInt32(await ScalarAsync(db,
                "SELECT COUNT(*) FROM Transacoes WHERE id_transacao=@id AND id_usuario=@user",
                ("@id", transactionId), ("@user", CurrentUserId)) ?? 0);
            if (owner != 1) throw new InvalidOperationException("Transação não encontrada.");
            await ExecuteAsync(db, "DELETE FROM TransacoesTags WHERE id_transacao=@id", ("@id", transactionId));
            foreach (var tagId in tagIds.Distinct())
                await ExecuteAsync(db, """
                    INSERT INTO TransacoesTags(id_transacao,id_tag)
                    SELECT @transaction,@tag WHERE EXISTS(
                      SELECT 1 FROM Tags WHERE id_tag=@tag AND id_usuario=@user)
                    """, ("@transaction", transactionId), ("@tag", tagId), ("@user", CurrentUserId));
            await tx.CommitAsync();
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    /// <summary>Lista lembretes, opcionalmente filtrados por status.</summary>
    public Task<List<LembreteItem>> GetLembretesAsync(string? status = null) => QueryAsync(
        """
        SELECT id_lembrete,id_usuario,id_transacao,titulo,descricao,data_lembrete,status
        FROM Lembretes WHERE id_usuario=@user AND (@status IS NULL OR status=@status)
        ORDER BY data_lembrete
        """, r => new LembreteItem(r.GetInt64(0), r.GetInt64(1),
            r.IsDBNull(2) ? null : r.GetInt64(2), r.GetString(3),
            r.IsDBNull(4) ? null : r.GetString(4), r.GetDateTime(5), r.GetString(6)),
        ("@user", CurrentUserId), ("@status", status));

    /// <summary>Cria ou atualiza um lembrete validado pelo banco.</summary>
    public async Task SaveLembreteAsync(long? id, long? transactionId, string title,
        string? description, DateTime reminderDate, string status = "pendente")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Informe o título do lembrete.");
        if (status == "pendente" && reminderDate.Date < DateTime.Today)
            throw new ArgumentException("A data do lembrete não pode estar no passado.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        if (id is null)
            await ExecuteAsync(db, """
                INSERT INTO Lembretes(id_usuario,id_transacao,titulo,descricao,data_lembrete,status)
                VALUES(@user,@transaction,@title,@description,@date,@status)
                """, ("@user", CurrentUserId), ("@transaction", transactionId), ("@title", title.Trim()),
                ("@description", description), ("@date", Iso(reminderDate)), ("@status", status));
        else
            await ExecuteAsync(db, """
                UPDATE Lembretes SET id_transacao=@transaction,titulo=@title,descricao=@description,
                  data_lembrete=@date,status=@status WHERE id_lembrete=@id AND id_usuario=@user
                """, ("@transaction", transactionId), ("@title", title.Trim()),
                ("@description", description), ("@date", Iso(reminderDate)), ("@status", status),
                ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, id is null ? "CRIAR" : "EDITAR", "Lembretes", id, $"titulo={title}");
    }

    public async Task DeleteLembreteAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, "DELETE FROM Lembretes WHERE id_lembrete=@id AND id_usuario=@user",
            ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, "EXCLUIR", "Lembretes", id, null);
    }

    public async Task UpdateReminderStatusAsync(long id, string status)
    {
        if (status is not ("pendente" or "executado" or "cancelado"))
            throw new ArgumentException("Status de lembrete inválido.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, """
            UPDATE Lembretes SET status=@status WHERE id_lembrete=@id AND id_usuario=@user
            """, ("@status", status), ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, "STATUS", "Lembretes", id, $"status={status}");
    }

    public Task<List<long>> GetTransactionTagIdsAsync(long transactionId) => QueryAsync(
        """
        SELECT tt.id_tag FROM TransacoesTags tt
        JOIN Tags t ON t.id_tag=tt.id_tag
        JOIN Transacoes x ON x.id_transacao=tt.id_transacao
        WHERE tt.id_transacao=@transaction AND x.id_usuario=@user AND t.id_usuario=@user
        """, r => r.GetInt64(0), ("@transaction", transactionId), ("@user", CurrentUserId));

    /// <summary>Registra uma transferência entre duas contas e atualiza saldos atomicamente.</summary>
    public async Task AddTransferAsync(long sourceAccountId, long destinationAccountId,
        decimal amount, DateTime date, string description, string? notes = null)
    {
        if (sourceAccountId == destinationAccountId) throw new ArgumentException("Selecione contas diferentes.");
        if (amount <= 0) throw new ArgumentException("O valor deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Informe a descrição.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            await using var debit = CreateCommand(db, """
                UPDATE Contas SET saldo_atual=saldo_atual-@amount
                WHERE id_conta=@source AND id_usuario=@user AND ativo=1 AND saldo_atual>=@amount
                """, ("@amount", amount), ("@source", sourceAccountId), ("@user", CurrentUserId));
            if (await debit.ExecuteNonQueryAsync() != 1) throw new InvalidOperationException("Saldo insuficiente.");
            await using var credit = CreateCommand(db, """
                UPDATE Contas SET saldo_atual=saldo_atual+@amount
                WHERE id_conta=@destination AND id_usuario=@user AND ativo=1
                """, ("@amount", amount), ("@destination", destinationAccountId), ("@user", CurrentUserId));
            if (await credit.ExecuteNonQueryAsync() != 1) throw new InvalidOperationException("Conta destino inválida.");
            await ExecuteAsync(db, """
                INSERT INTO Transacoes(id_conta,id_conta_destino,id_usuario,valor,data,descricao,tipo,pago,
                  data_pagamento,observacoes)
                VALUES(@source,@destination,@user,@amount,@date,@description,'transferencia',1,@date,@notes)
                """, ("@source", sourceAccountId), ("@destination", destinationAccountId),
                ("@user", CurrentUserId), ("@amount", amount), ("@date", Iso(date)),
                ("@description", description.Trim()), ("@notes", notes));
            var id = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid();"));
            await WriteLogAsync(db, "TRANSFERIR", "Transacoes", id, $"valor={amount:0.00}");
            await tx.CommitAsync();
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<List<Tag>> GetTagsByTransacaoAsync(long idTransacao)
    {
        var tags = await QueryAsync("""
            SELECT t.id_tag,t.id_usuario,t.nome_tag,COALESCE(t.cor,@color),COALESCE(t.icone,'tag')
            FROM TransacoesTags tt
            JOIN Tags t ON t.id_tag=tt.id_tag
            JOIN Transacoes x ON x.id_transacao=tt.id_transacao
            WHERE tt.id_transacao=@transaction AND x.id_usuario=@user AND t.id_usuario=@user
            ORDER BY t.nome_tag
            """, reader => new Tag(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4)),
            ("@color", BlingPalette.PrimaryHex), ("@transaction", idTransacao), ("@user", CurrentUserId));
        return tags ?? [];
    }

    public Task SaveTransacaoTagsAsync(long idTransacao, List<long> tagIds) =>
        SetTransactionTagsAsync(idTransacao, tagIds ?? []);

    /// <summary>Altera uma transferência revertendo o impacto anterior e aplicando o novo, atomicamente.</summary>
    public async Task UpdateTransferAsync(long id, long sourceAccountId, long destinationAccountId,
        decimal amount, DateTime date, string description, string? notes = null)
    {
        if (sourceAccountId == destinationAccountId) throw new ArgumentException("Selecione contas diferentes.");
        if (amount <= 0) throw new ArgumentException("O valor deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Informe a descrição.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            await using var select = CreateCommand(db, """
                SELECT id_conta,id_conta_destino,valor FROM Transacoes
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='transferencia'
                """, ("@id", id), ("@user", CurrentUserId));
            await using var reader = await select.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("A transferência não foi encontrada.");
            var oldSource = reader.GetInt64(0); var oldDestination = reader.GetInt64(1); var oldAmount = reader.GetDecimal(2);
            await reader.DisposeAsync();

            if (await ExecuteCountAsync(db, """
                UPDATE Contas SET saldo_atual=saldo_atual+@amount
                WHERE id_conta=@account AND id_usuario=@user
                """, ("@amount", oldAmount), ("@account", oldSource), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("Conta de origem anterior inválida.");
            if (await ExecuteCountAsync(db, """
                UPDATE Contas SET saldo_atual=saldo_atual-@amount
                WHERE id_conta=@account AND id_usuario=@user AND saldo_atual>=@amount
                """, ("@amount", oldAmount), ("@account", oldDestination), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("A conta de destino anterior não possui saldo para o estorno.");
            if (await ExecuteCountAsync(db, """
                UPDATE Contas SET saldo_atual=saldo_atual-@amount
                WHERE id_conta=@account AND id_usuario=@user AND ativo=1 AND saldo_atual>=@amount
                """, ("@amount", amount), ("@account", sourceAccountId), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("Saldo insuficiente na nova conta de origem.");
            if (await ExecuteCountAsync(db, """
                UPDATE Contas SET saldo_atual=saldo_atual+@amount
                WHERE id_conta=@account AND id_usuario=@user AND ativo=1
                """, ("@amount", amount), ("@account", destinationAccountId), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("Nova conta de destino inválida.");
            if (await ExecuteCountAsync(db, """
                UPDATE Transacoes SET id_conta=@source,id_conta_destino=@destination,valor=@amount,
                  data=@date,data_pagamento=@date,descricao=@description,observacoes=@notes
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='transferencia'
                """, ("@source", sourceAccountId), ("@destination", destinationAccountId),
                ("@amount", amount), ("@date", Iso(date)), ("@description", description.Trim()),
                ("@notes", notes), ("@id", id), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("Não foi possível alterar a transferência.");
            await WriteLogAsync(db, "ALTERAR_TRANSFERENCIA", "Transacoes", id, $"valor={amount:0.00}");
            await tx.CommitAsync();
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    /// <summary>Exclui uma transferência e estorna os dois saldos na mesma transação de banco.</summary>
    public async Task DeleteTransferAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            await using var select = CreateCommand(db, """
                SELECT id_conta,id_conta_destino,valor FROM Transacoes
                WHERE id_transacao=@id AND id_usuario=@user AND tipo='transferencia'
                """, ("@id", id), ("@user", CurrentUserId));
            await using var reader = await select.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("A transferência não foi encontrada.");
            var source = reader.GetInt64(0); var destination = reader.GetInt64(1); var amount = reader.GetDecimal(2);
            await reader.DisposeAsync();
            if (await ExecuteCountAsync(db, """
                UPDATE Contas SET saldo_atual=saldo_atual+@amount
                WHERE id_conta=@account AND id_usuario=@user
                """, ("@amount", amount), ("@account", source), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("Conta de origem inválida para o estorno.");
            if (await ExecuteCountAsync(db, """
                UPDATE Contas SET saldo_atual=saldo_atual-@amount
                WHERE id_conta=@account AND id_usuario=@user AND saldo_atual>=@amount
                """, ("@amount", amount), ("@account", destination), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("A conta de destino não possui saldo para o estorno.");
            await WriteLogAsync(db, "EXCLUIR_TRANSFERENCIA", "Transacoes", id, $"valor={amount:0.00}");
            if (await ExecuteCountAsync(db, """
                DELETE FROM Transacoes WHERE id_transacao=@id AND id_usuario=@user AND tipo='transferencia'
                """, ("@id", id), ("@user", CurrentUserId)) != 1)
                throw new InvalidOperationException("Não foi possível excluir a transferência.");
            await tx.CommitAsync();
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    /// <summary>Gera ou recalcula as faturas mensais de todos os cartões ativos.</summary>
    public async Task<List<FaturaCartaoItem>> GenerateCardInvoicesAsync(int month, int year)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100)
            throw new ArgumentException("Período inválido.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            var cards = await QueryAsync(db,
                "SELECT id_cartao,dia_vencimento FROM CartoesCredito WHERE id_usuario=@user AND ativo=1",
                r => (Id: r.GetInt64(0), DueDay: r.GetInt32(1)), ("@user", CurrentUserId));
            foreach (var card in cards)
            {
                var due = new DateTime(year, month, Math.Min(card.DueDay, DateTime.DaysInMonth(year, month)));
                await ExecuteAsync(db, """
                    INSERT INTO FaturasCartao(id_cartao,mes_referencia,ano_referencia,valor_total,data_vencimento,status)
                    VALUES(@card,@month,@year,0,@due,'aberta')
                    ON CONFLICT(id_cartao,mes_referencia,ano_referencia)
                    DO UPDATE SET data_vencimento=excluded.data_vencimento
                    WHERE FaturasCartao.status<>'paga'
                    """, ("@card", card.Id), ("@month", month), ("@year", year),
                    ("@due", Iso(due)));
                var invoiceId = Convert.ToInt64(await ScalarAsync(db, """
                    SELECT id_fatura FROM FaturasCartao
                    WHERE id_cartao=@card AND mes_referencia=@month AND ano_referencia=@year
                    """, ("@card", card.Id), ("@month", month), ("@year", year)));
                await ExecuteAsync(db, """
                    UPDATE Transacoes SET id_fatura=@invoice
                    WHERE id_cartao=@card
                      AND CAST(strftime('%m',data_vencimento) AS INTEGER)=@month
                      AND CAST(strftime('%Y',data_vencimento) AS INTEGER)=@year
                      AND (id_fatura IS NULL OR id_fatura IN(
                        SELECT id_fatura FROM FaturasCartao WHERE status<>'paga'))
                    """, ("@invoice", invoiceId), ("@card", card.Id), ("@month", month), ("@year", year));
                await ExecuteAsync(db, """
                    UPDATE FaturasCartao SET valor_total=(
                      SELECT COALESCE(SUM(t.valor),0) FROM Transacoes t WHERE t.id_fatura=@invoice
                        AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                          SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao)))
                    WHERE id_fatura=@invoice AND status<>'paga'
                    """, ("@invoice", invoiceId));
            }
            await tx.CommitAsync();
        }
        catch { await tx.RollbackAsync(); throw; }
        return await GetCardInvoicesAsync(month, year);
    }

    /// <summary>Lista faturas de cartão de um período.</summary>
    public Task<List<FaturaCartaoItem>> GetCardInvoicesAsync(int month, int year) => QueryAsync(
        """
        SELECT f.id_fatura,f.id_cartao,f.mes_referencia,f.ano_referencia,
          COALESCE((
            SELECT SUM(t.valor) FROM Transacoes t
            WHERE t.id_cartao=f.id_cartao AND t.tipo='despesa'
              AND NOT (t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))
              AND CAST(strftime('%m',t.data_vencimento) AS INTEGER)=f.mes_referencia
              AND CAST(strftime('%Y',t.data_vencimento) AS INTEGER)=f.ano_referencia
          ),0),
          f.data_vencimento,f.status,f.data_pagamento,c.nome_cartao
        FROM FaturasCartao f JOIN CartoesCredito c ON c.id_cartao=f.id_cartao
        WHERE c.id_usuario=@user AND f.mes_referencia=@month AND f.ano_referencia=@year
        ORDER BY f.data_vencimento
        """, r => new FaturaCartaoItem(r.GetInt64(0), r.GetInt64(1), r.GetInt32(2),
            r.GetInt32(3), r.GetDecimal(4), r.GetDateTime(5), r.GetString(6),
            r.IsDBNull(7) ? null : r.GetDateTime(7), r.GetString(8)),
        ("@user", CurrentUserId), ("@month", month), ("@year", year));

    public async Task PayCardInvoiceAsync(long invoiceId, long accountId)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var tx = await db.BeginTransactionAsync();
        try
        {
            var invoice = (await QueryAsync(db, """
                SELECT COALESCE((SELECT SUM(t.valor) FROM Transacoes t
                                  WHERE t.id_fatura=f.id_fatura
                                    AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                                      SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))),0),
                       f.status,f.id_cartao,f.mes_referencia,f.ano_referencia
                FROM FaturasCartao f JOIN CartoesCredito c ON c.id_cartao=f.id_cartao
                WHERE f.id_fatura=@invoice AND c.id_usuario=@user
                """, r => (Total: r.GetDecimal(0), Status: r.GetString(1), CardId: r.GetInt64(2),
                    Month: r.GetInt32(3), Year: r.GetInt32(4)),
                ("@invoice", invoiceId), ("@user", CurrentUserId))).SingleOrDefault();
            if (invoice == default) throw new InvalidOperationException("Fatura não encontrada.");
            if (invoice.Status == "paga") throw new InvalidOperationException("Esta fatura já foi paga.");

            await using var debit = CreateCommand(db, """
                UPDATE Contas SET saldo_atual=saldo_atual-@total
                WHERE id_conta=@account AND id_usuario=@user AND ativo=1 AND saldo_atual>=@total
                """, ("@total", invoice.Total), ("@account", accountId), ("@user", CurrentUserId));
            if (await debit.ExecuteNonQueryAsync() != 1)
                throw new InvalidOperationException("Conta inválida ou saldo insuficiente.");

            var today = Iso(DateTime.Today);
            await ExecuteAsync(db, """
                UPDATE FaturasCartao SET status='paga',data_pagamento=@today
                WHERE id_fatura=@invoice
                """, ("@today", today), ("@invoice", invoiceId));
            await ExecuteAsync(db, """
                UPDATE Transacoes SET pago=1,data_pagamento=CURRENT_TIMESTAMP
                WHERE id_fatura=@invoice AND tipo='despesa'
                  AND NOT(parcelado=1 AND id_transacao_pai IS NULL AND EXISTS(
                    SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=Transacoes.id_transacao))
                """, ("@invoice", invoiceId));
            await ExecuteAsync(db, """
                UPDATE CartoesCredito SET limite_utilizado=MAX(0,limite_utilizado-@total)
                WHERE id_cartao=@card
                """, ("@total", invoice.Total), ("@card", invoice.CardId));
            await WriteLogAsync(db, "PAGAR_FATURA", "FaturasCartao", invoiceId,
                $"valor={invoice.Total:0.00}; conta={accountId}");
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public Task<List<InvoiceTransactionItem>> GetInvoiceTransactionsAsync(long invoiceId) => QueryAsync(
        """
        SELECT t.id_transacao,t.descricao,COALESCE(c.nome_categoria,'Sem categoria'),t.valor,
          t.data,COALESCE(t.numero_parcela,1),COALESCE(t.total_parcelas,1)
        FROM FaturasCartao f
        JOIN CartoesCredito cc ON cc.id_cartao=f.id_cartao
        JOIN Transacoes t ON t.id_cartao=f.id_cartao
        LEFT JOIN Categorias c ON c.id_categoria=t.id_categoria
        WHERE f.id_fatura=@invoice AND cc.id_usuario=@user AND t.tipo='despesa'
          AND NOT (t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
            SELECT 1 FROM Transacoes filha WHERE filha.id_transacao_pai=t.id_transacao))
          AND CAST(strftime('%m',t.data_vencimento) AS INTEGER)=f.mes_referencia
          AND CAST(strftime('%Y',t.data_vencimento) AS INTEGER)=f.ano_referencia
        ORDER BY t.data_vencimento,t.id_transacao
        """, r => new InvoiceTransactionItem(r.GetInt64(0), r.GetString(1), r.GetString(2),
            r.GetDecimal(3), r.GetDateTime(4), r.GetInt32(5), r.GetInt32(6)),
        ("@invoice", invoiceId), ("@user", CurrentUserId));

    public async Task CloseCardInvoiceAsync(long invoiceId)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        var affected = await ExecuteCountAsync(db, """
            UPDATE FaturasCartao SET status='fechada'
            WHERE id_fatura=@invoice AND status='aberta'
              AND id_cartao IN(SELECT id_cartao FROM CartoesCredito WHERE id_usuario=@user)
            """, ("@invoice", invoiceId), ("@user", CurrentUserId));
        if (affected != 1) throw new InvalidOperationException("A fatura não está aberta ou não existe.");
        await WriteLogAsync(db, "FECHAR_FATURA", "FaturasCartao", invoiceId, null);
    }

    /// <summary>Retorna o resumo mensal otimizado pela view vw_resumo_mensal.</summary>
    public Task<List<MonthlyReportItem>> GetMonthlyReportAsync() => QueryAsync(
        """
        SELECT mes_ano,total_receitas,total_despesas,saldo_mes
        FROM vw_resumo_mensal WHERE id_usuario=@user ORDER BY mes_ano DESC
        """, r => new MonthlyReportItem(r.GetString(0), r.GetDecimal(1),
            r.GetDecimal(2), r.GetDecimal(3)), ("@user", CurrentUserId));

    /// <summary>Retorna gastos por categoria usando a view otimizada.</summary>
    public Task<List<CategoryExpenseReport>> GetCategoryExpenseReportAsync(int month, int year) =>
        QueryAsync("""
            SELECT nome_categoria,cor,total FROM vw_gastos_categoria
            WHERE id_usuario=@user AND mes_ano=@period ORDER BY total DESC
            """, r => new CategoryExpenseReport(r.GetString(0), r.GetString(1), r.GetDecimal(2)),
            ("@user", CurrentUserId), ("@period", $"{year:D4}-{month:D2}"));

    public async Task<FinancialHealth> GetFinancialHealthAsync()
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        var rows = await QueryAsync(db, """
            SELECT total_receitas,total_despesas,saldo_geral,percentual_gastos
            FROM vw_saude_financeira WHERE id_usuario=@user
            """, r => new FinancialHealth(r.GetDecimal(0), r.GetDecimal(1),
                r.GetDecimal(2), r.GetDecimal(3)), ("@user", CurrentUserId));
        return rows.SingleOrDefault() ?? new FinancialHealth(0, 0, 0, 0);
    }

    public Task<List<BudgetItem>> GetBudgetsAsync(int month, int year) => QueryAsync(
        """
        SELECT o.id_orcamento,o.id_categoria,v.nome_categoria,v.cor,v.valor_limite,
          v.mes,v.ano,v.valor_realizado,v.status,o.observacoes
        FROM vw_orcamento_mensal v JOIN Orcamentos o ON o.id_orcamento=v.id_orcamento
        WHERE v.id_usuario=@user AND v.mes=@month AND v.ano=@year
        ORDER BY v.percentual_utilizado DESC
        """, r => new BudgetItem(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3),
            r.GetDecimal(4), r.GetInt32(5), r.GetInt32(6), r.GetDecimal(7), r.GetString(8),
            r.IsDBNull(9) ? null : r.GetString(9)),
        ("@user", CurrentUserId), ("@month", month), ("@year", year));

    public Task<List<BudgetProjectionItem>> GetBudgetProjectionAsync(int month, int year) => QueryAsync("""
        SELECT o.id_orcamento,o.id_categoria,c.nome_categoria,o.valor_limite,
          COALESCE(SUM(CASE WHEN t.pago=1 THEN t.valor ELSE 0 END),0),
          COALESCE(SUM(CASE WHEN t.pago=0 THEN t.valor ELSE 0 END),0)
        FROM Orcamentos o JOIN Categorias c ON c.id_categoria=o.id_categoria
        LEFT JOIN Transacoes t ON t.id_usuario=o.id_usuario AND t.id_categoria=o.id_categoria
          AND t.tipo='despesa'
          AND CAST(strftime('%m',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.mes
          AND CAST(strftime('%Y',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.ano
          AND NOT(COALESCE(t.parcelado,0)=1 AND t.id_transacao_pai IS NULL AND EXISTS(
            SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao))
        WHERE o.id_usuario=@user AND o.mes=@month AND o.ano=@year
        GROUP BY o.id_orcamento ORDER BY c.nome_categoria
        """,r=>new BudgetProjectionItem(r.GetInt64(0),r.GetInt64(1),r.GetString(2),
            r.GetDecimal(3),r.GetDecimal(4),r.GetDecimal(5)),
        ("@user",CurrentUserId),("@month",month),("@year",year));

    public async Task SaveBudgetAsync(long? id, long categoryId, decimal limit, int month,
        int year, string? notes)
    {
        if (limit <= 0) throw new ArgumentException("O limite deve ser maior que zero.");
        if (month is < 1 or > 12 || year is < 2000 or > 2100)
            throw new ArgumentException("Selecione um período válido.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        var categoryExists = Convert.ToInt32(await ScalarAsync(db, """
            SELECT COUNT(*) FROM Categorias WHERE id_categoria=@category
              AND id_usuario=@user AND tipo='despesa' AND ativo=1
            """, ("@category", categoryId), ("@user", CurrentUserId)) ?? 0);
        if (categoryExists != 1) throw new ArgumentException("Selecione uma categoria de despesa válida.");
        if (id is null)
            await ExecuteAsync(db, """
                INSERT INTO Orcamentos(id_usuario,id_categoria,valor_limite,mes,ano,observacoes)
                VALUES(@user,@category,@limit,@month,@year,@notes)
                """, ("@user", CurrentUserId), ("@category", categoryId), ("@limit", limit),
                ("@month", month), ("@year", year), ("@notes", notes));
        else
            await ExecuteAsync(db, """
                UPDATE Orcamentos SET id_categoria=@category,valor_limite=@limit,mes=@month,
                  ano=@year,observacoes=@notes WHERE id_orcamento=@id AND id_usuario=@user
                """, ("@category", categoryId), ("@limit", limit), ("@month", month),
                ("@year", year), ("@notes", notes), ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, id is null ? "CRIAR" : "EDITAR", "Orcamentos", id,
            $"categoria={categoryId}; limite={limit:0.00}; periodo={year:D4}-{month:D2}");
    }

    public async Task DeleteBudgetAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, "DELETE FROM Orcamentos WHERE id_orcamento=@id AND id_usuario=@user",
            ("@id", id), ("@user", CurrentUserId));
        await WriteLogAsync(db, "EXCLUIR", "Orcamentos", id, null);
    }

    public Task<List<ImportHistoryItem>> GetImportHistoryAsync() => QueryAsync(
        """
        SELECT id_importacao,nome_arquivo,formato,total_registros,duplicados,data_importacao
        FROM Importacoes WHERE id_usuario=@user ORDER BY data_importacao DESC
        """, r => new ImportHistoryItem(r.GetInt64(0), r.GetString(1), r.GetString(2),
            r.GetInt32(3), r.GetInt32(4), r.GetDateTime(5)), ("@user", CurrentUserId));

    public Task<List<BackupHistoryItem>> GetBackupHistoryAsync() => QueryAsync(
        """
        SELECT id_backup,nome_arquivo,COALESCE(tamanho_kb,0),data_criacao,localizacao,tipo
        FROM Backups WHERE id_usuario=@user ORDER BY data_criacao DESC
        """, r => new BackupHistoryItem(r.GetInt64(0), r.GetString(1), r.GetInt64(2),
            r.GetDateTime(3), r.IsDBNull(4) ? null : r.GetString(4), r.GetString(5)),
        ("@user", CurrentUserId));

    public async Task<UserSettings> GetSettingsAsync()
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, "INSERT OR IGNORE INTO Configuracoes(id_usuario) VALUES(@user)",
            ("@user", CurrentUserId));
        return (await QueryAsync(db, """
            SELECT moeda,formato_data,tema,notificacoes_email,alerta_vencimento_dias,
              backup_automatico,frequencia_backup
            FROM Configuracoes WHERE id_usuario=@user
            """, r => new UserSettings(r.GetString(0), r.GetString(1), r.GetString(2),
                r.GetInt32(3) == 1, r.GetInt32(4), r.GetInt32(5) == 1, r.GetString(6)),
            ("@user", CurrentUserId))).Single();
    }

    public async Task SaveSettingsAsync(UserSettings settings)
    {
        if (settings.DueAlertDays is < 0 or > 365)
            throw new ArgumentException("Os dias de antecedência devem estar entre 0 e 365.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, """
            INSERT INTO Configuracoes(id_usuario,moeda,formato_data,tema,notificacoes_email,
              alerta_vencimento_dias,backup_automatico,frequencia_backup)
            VALUES(@user,@currency,@date,@theme,@alerts,@days,@automatic,@frequency)
            ON CONFLICT(id_usuario) DO UPDATE SET moeda=excluded.moeda,
              formato_data=excluded.formato_data,tema=excluded.tema,
              notificacoes_email=excluded.notificacoes_email,
              alerta_vencimento_dias=excluded.alerta_vencimento_dias,
              backup_automatico=excluded.backup_automatico,
              frequencia_backup=excluded.frequencia_backup
            """, ("@user", CurrentUserId), ("@currency", settings.Currency),
            ("@date", settings.DateFormat), ("@theme", settings.Theme),
            ("@alerts", settings.AlertsEnabled ? 1 : 0), ("@days", settings.DueAlertDays),
            ("@automatic", settings.AutomaticBackup ? 1 : 0),
            ("@frequency", settings.BackupFrequency));
        await WriteLogAsync(db, "CONFIGURAR", "Configuracoes", null, $"tema={settings.Theme}");
    }

    public Task<List<AuditLogItem>> GetAuditLogsAsync(int limit = 100) => QueryAsync(
        """
        SELECT id_log,acao,tabela,registro_id,dados_novos,data_hora,origem
        FROM Logs WHERE id_usuario=@user OR id_usuario IS NULL
        ORDER BY data_hora DESC LIMIT @limit
        """, r => new AuditLogItem(r.GetInt64(0), r.GetString(1),
            r.IsDBNull(2) ? null : r.GetString(2), r.IsDBNull(3) ? null : r.GetInt64(3),
            r.IsDBNull(4) ? null : r.GetString(4), r.GetDateTime(5),
            r.IsDBNull(6) ? null : r.GetString(6)),
        ("@user", CurrentUserId), ("@limit", Math.Clamp(limit, 1, 500)));

    public Task<List<NotificationItem>> GetNotificationsAsync(bool unreadOnly = false) => QueryAsync(
        """
        SELECT id_notificacao,titulo,mensagem,nivel,lida,data_criacao
        FROM Notificacoes WHERE id_usuario=@user AND (@unread=0 OR lida=0)
        ORDER BY data_criacao DESC
        """, r => new NotificationItem(r.GetInt64(0), r.GetString(1), r.GetString(2),
            r.GetString(3), r.GetInt32(4) == 1, r.GetDateTime(5)),
        ("@user", CurrentUserId), ("@unread", unreadOnly ? 1 : 0));

    public async Task MarkNotificationReadAsync(long id)
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, """
            UPDATE Notificacoes SET lida=1 WHERE id_notificacao=@id AND id_usuario=@user
            """, ("@id", id), ("@user", CurrentUserId));
    }

    public async Task RecordBackupAsync(string fileName, string location, long sizeBytes,
        string type = "manual")
    {
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, """
            INSERT INTO Backups(id_usuario,nome_arquivo,tamanho_kb,localizacao,tipo)
            VALUES(@user,@name,@size,@location,@type)
            """, ("@user", CurrentUserId), ("@name", fileName),
            ("@size", Math.Max(0, sizeBytes / 1024)), ("@location", location), ("@type", type));
        await WriteLogAsync(db, "BACKUP", "Backups", null, $"arquivo={fileName}");
    }

    /// <summary>Importa rascunhos sequencialmente e registra o resultado da importação.</summary>
    public async Task<ImportResult> ImportTransactionsAsync(
        IEnumerable<TransactionDraft> drafts, string fileName, string format)
    {
        var source = drafts.ToList();
        var imported = 0;
        foreach (var draft in source)
        {
            await AddTransactionAsync(draft);
            imported++;
        }
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await ExecuteAsync(db, """
            INSERT INTO Importacoes(id_usuario,nome_arquivo,formato,total_registros,duplicados)
            VALUES(@user,@file,@format,@total,0)
            """, ("@user", CurrentUserId), ("@file", fileName),
            ("@format", format), ("@total", imported));
        await WriteLogAsync(db, "IMPORTAR", "Importacoes", null, $"arquivo={fileName}; total={imported}");
        return new(source.Count, imported, 0);
    }

    public async Task<ImportResult> ImportPayablesAsync(
        IReadOnlyCollection<PayableImportRow> rows, long accountId, string fileName)
    {
        if (rows.Count == 0 || rows.Any(x => !x.IsValid || x.CategoryId is null))
            throw new InvalidDataException("Existem linhas inválidas na pré-visualização.");
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            var accountExists = Convert.ToInt64(await ScalarAsync(db,
                "SELECT COUNT(*) FROM Contas WHERE id_conta=@account AND id_usuario=@user AND ativo=1",
                ("@account", accountId), ("@user", CurrentUserId)) ?? 0);
            if (accountExists != 1) throw new InvalidOperationException("A conta selecionada não está disponível.");

            foreach (var row in rows)
            {
                var categoryExists = Convert.ToInt64(await ScalarAsync(db,
                    "SELECT COUNT(*) FROM Categorias WHERE id_categoria=@category AND id_usuario=@user AND tipo='despesa' AND ativo=1",
                    ("@category", row.CategoryId), ("@user", CurrentUserId)) ?? 0);
                if (categoryExists != 1)
                    throw new InvalidOperationException($"Categoria inválida na linha {row.LineNumber}.");

                long? supplierId = null;
                if (!string.IsNullOrWhiteSpace(row.Supplier))
                {
                    var existing = await ScalarAsync(db,
                        "SELECT id_fornecedor FROM Fornecedores WHERE id_usuario=@user AND nome_fornecedor=@name COLLATE NOCASE LIMIT 1",
                        ("@user", CurrentUserId), ("@name", row.Supplier.Trim()));
                    if (existing is null)
                    {
                        await ExecuteAsync(db,
                            "INSERT INTO Fornecedores(id_usuario,nome_fornecedor,ativo) VALUES(@user,@name,1)",
                            ("@user", CurrentUserId), ("@name", row.Supplier.Trim()));
                        existing = await ScalarAsync(db, "SELECT last_insert_rowid()");
                    }
                    supplierId = Convert.ToInt64(existing);
                }

                var inserted = await ExecuteCountAsync(db, """
                    INSERT INTO Transacoes(id_conta,id_categoria,id_usuario,valor,data,descricao,tipo,
                      observacoes,data_vencimento,pago,recorrente,id_fornecedor)
                    VALUES(@account,@category,@user,@amount,@date,@description,'despesa',
                      @notes,@due,0,0,@supplier)
                    """, ("@account", accountId), ("@category", row.CategoryId),
                    ("@user", CurrentUserId), ("@amount", row.Amount), ("@date", row.DueDate.Date),
                    ("@description", row.Description.Trim()), ("@notes", row.Notes),
                    ("@due", row.DueDate.Date), ("@supplier", supplierId));
                if (inserted != 1)
                    throw new InvalidOperationException($"A linha {row.LineNumber} não foi gravada.");
            }

            await ExecuteAsync(db, """
                INSERT INTO Importacoes(id_usuario,nome_arquivo,formato,total_registros,duplicados)
                VALUES(@user,@file,'csv-contas-pagar',@total,0)
                """, ("@user", CurrentUserId), ("@file", fileName), ("@total", rows.Count));
            await WriteLogAsync(db, "IMPORTAR_CONTAS_PAGAR", "Transacoes", null,
                $"arquivo={fileName}; total={rows.Count}");
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        DataChanged?.Invoke(this, EventArgs.Empty);
        return new(rows.Count, rows.Count, 0);
    }

    public async Task<int> ImportCardInvoiceAsync(long cardId,
        IReadOnlyCollection<CardInstallmentImport> installments, string fileName)
    {
        if (installments.Count == 0) throw new InvalidDataException("Nenhuma parcela foi gerada.");
        await using var db = new SqliteConnection(ConnectionString); await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        try
        {
            var cardExists = Convert.ToInt64(await ScalarAsync(db,
                "SELECT COUNT(*) FROM CartoesCredito WHERE id_cartao=@card AND id_usuario=@user AND ativo=1",
                ("@card", cardId), ("@user", CurrentUserId)) ?? 0);
            if (cardExists != 1) throw new InvalidOperationException("O cartão selecionado não está disponível.");
            var categoryId = await ScalarAsync(db,
                "SELECT id_categoria FROM Categorias WHERE id_usuario=@user AND tipo='despesa' AND ativo=1 ORDER BY id_categoria LIMIT 1",
                ("@user", CurrentUserId));
            if (categoryId is null) throw new InvalidOperationException("Cadastre ao menos uma categoria de despesa antes da importação.");

            long? firstTransactionId = null;
            var importedItems = installments.ToList();
            for (var itemIndex = 0; itemIndex < importedItems.Count; itemIndex++)
            {
                var item = importedItems[itemIndex];
                if (item.Number == 1 && item.Total > 1)
                {
                    var series = importedItems.Skip(itemIndex).TakeWhile((candidate, offset) =>
                        offset == 0 || candidate.Number != 1).Take(item.Total).ToList();
                    var seriesTotal = series.Sum(candidate => candidate.Amount);
                    await ExecuteAsync(db, """
                        INSERT INTO Transacoes(id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                          parcelado,numero_parcelas,observacoes,data_vencimento,pago,numero_parcela,total_parcelas)
                        VALUES(@card,@category,@user,@amount,@date,@description,'despesa',1,@total,@notes,@due,0,1,@total)
                        """, ("@card", cardId), ("@category", Convert.ToInt64(categoryId)),
                        ("@user", CurrentUserId), ("@amount", seriesTotal), ("@date", item.PurchaseDate.Date),
                        ("@description", item.Description), ("@total", item.Total),
                        ("@notes", $"Agrupador da importação CSV: {fileName}"), ("@due", item.InvoiceDueDate.Date));
                    firstTransactionId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid()"));
                }
                else if (item.Total == 1)
                {
                    firstTransactionId = null;
                }
                var duplicate = Convert.ToInt64(await ScalarAsync(db, """
                    SELECT COUNT(*) FROM Transacoes WHERE id_usuario=@user AND id_cartao=@card
                      AND descricao=@description AND date(data)=date(@date)
                      AND COALESCE(numero_parcela,1)=@number AND COALESCE(total_parcelas,1)=@total
                    """, ("@user", CurrentUserId), ("@card", cardId), ("@description", item.Description),
                    ("@date", item.PurchaseDate.Date), ("@number", item.Number), ("@total", item.Total)) ?? 0);
                if (duplicate > 0)
                    throw new InvalidOperationException($"A parcela '{item.Description}' já está cadastrada. A importação foi cancelada para evitar duplicidade.");

                await ExecuteAsync(db, """
                    INSERT INTO FaturasCartao(id_cartao,mes_referencia,ano_referencia,valor_total,data_vencimento,status)
                    VALUES(@card,@month,@year,0,@due,@status)
                    ON CONFLICT(id_cartao,mes_referencia,ano_referencia) DO UPDATE SET
                      data_vencimento=excluded.data_vencimento,
                      status=CASE WHEN FaturasCartao.status='paga' THEN 'paga' ELSE excluded.status END
                    """, ("@card", cardId), ("@month", item.InvoiceMonth), ("@year", item.InvoiceYear),
                    ("@due", item.InvoiceDueDate.Date), ("@status", item.InvoiceStatus));
                var invoiceId = Convert.ToInt64(await ScalarAsync(db, """
                    SELECT id_fatura FROM FaturasCartao
                    WHERE id_cartao=@card AND mes_referencia=@month AND ano_referencia=@year
                    """, ("@card", cardId), ("@month", item.InvoiceMonth), ("@year", item.InvoiceYear)));

                var inserted = await ExecuteCountAsync(db, """
                    INSERT INTO Transacoes(id_cartao,id_categoria,id_usuario,valor,data,descricao,tipo,
                      parcelado,numero_parcelas,id_transacao_pai,observacoes,id_fatura,data_vencimento,
                      pago,numero_parcela,total_parcelas)
                    VALUES(@card,@category,@user,@amount,@date,@description,'despesa',
                      @parcelled,@total,@parent,@notes,@invoice,@due,0,@number,@total)
                    """, ("@card", cardId), ("@category", Convert.ToInt64(categoryId)),
                    ("@user", CurrentUserId), ("@amount", item.Amount), ("@date", item.PurchaseDate.Date),
                    ("@description", item.Description), ("@parcelled", item.Total > 1 ? 1 : 0),
                    ("@total", item.Total), ("@parent", firstTransactionId),
                    ("@notes", $"Importado da fatura CSV: {fileName}"), ("@invoice", invoiceId),
                    ("@due", item.InvoiceDueDate.Date), ("@number", item.Number));
                if (inserted != 1) throw new InvalidOperationException($"Não foi possível gravar {item.Description}.");
                var insertedId = Convert.ToInt64(await ScalarAsync(db, "SELECT last_insert_rowid()"));
                _ = insertedId;
            }

            await ExecuteAsync(db, """
                UPDATE FaturasCartao SET valor_total=(
                  SELECT COALESCE(SUM(t.valor),0) FROM Transacoes t
                  WHERE t.id_fatura=FaturasCartao.id_fatura
                    AND NOT(t.parcelado=1 AND t.id_transacao_pai IS NULL AND EXISTS(
                      SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao)))
                WHERE id_cartao=@card
                """, ("@card", cardId));
            await ExecuteAsync(db,
                "UPDATE CartoesCredito SET limite_utilizado=limite_utilizado+@amount WHERE id_cartao=@card AND id_usuario=@user",
                ("@amount", installments.Sum(x => x.Amount)), ("@card", cardId), ("@user", CurrentUserId));
            await ExecuteAsync(db, """
                INSERT INTO Importacoes(id_usuario,nome_arquivo,formato,total_registros,duplicados)
                VALUES(@user,@file,'csv-fatura-cartao',@total,0)
                """, ("@user", CurrentUserId), ("@file", fileName), ("@total", installments.Count));
            await WriteLogAsync(db, "IMPORTAR_FATURA_CARTAO", "Transacoes", firstTransactionId,
                $"cartao={cardId}; parcelas={installments.Count}; arquivo={fileName}");
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
        DataChanged?.Invoke(this, EventArgs.Empty);
        return installments.Count;
    }

    private async Task WriteLogAsync(SqliteConnection db, string action, string table,
        long? recordId, string? data)
    {
        try
        {
            await ExecuteAsync(db, """
                INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_novos,origem)
                VALUES(@user,@action,@table,@record,@data,'DatabaseService')
                """, ("@user", CurrentUserId), ("@action", action), ("@table", table),
                ("@record", recordId), ("@data", data));
        }
        catch (Exception ex)
        {
            // Auditoria é defensiva: uma indisponibilidade do log não pode desfazer
            // a operação financeira principal que já foi validada.
            System.Diagnostics.Debug.WriteLine($"[AuditLog] {action}/{table}: {ex.Message}");
        }
    }

    private static string Iso(DateTime date) => date.ToString("yyyy-MM-dd",
        System.Globalization.CultureInfo.InvariantCulture);

    // ============================================================
    // MÉTODOS PRIVADOS DE BANCO DE DADOS
    // ============================================================

    private async Task<Dictionary<long, IReadOnlyList<TagItem>>> GetTransactionTagsLookupAsync()
    {
        var rows = await QueryAsync("""
            SELECT tt.id_transacao,t.id_tag,t.id_usuario,t.nome_tag,
                   COALESCE(t.cor,@color),COALESCE(t.icone,'tag')
            FROM TransacoesTags tt
            JOIN Tags t ON t.id_tag=tt.id_tag
            JOIN Transacoes x ON x.id_transacao=tt.id_transacao
            WHERE x.id_usuario=@user AND t.id_usuario=@user
            ORDER BY t.nome_tag
            """, reader => (TransactionId: reader.GetInt64(0),
                Tag: new TagItem(reader.GetInt64(1), reader.GetInt64(2), reader.GetString(3),
                    reader.GetString(4), reader.GetString(5))),
            ("@color", BlingPalette.PrimaryHex), ("@user", CurrentUserId));
        return (rows ?? []).GroupBy(row => row.TransactionId)
            .ToDictionary(group => group.Key,
                group => (IReadOnlyList<TagItem>)group.Select(row => row.Tag).ToList());
    }

    private async Task<List<T>> QueryAsync<T>(string sql, Func<SqliteDataReader, T> map, params (string, object?)[] args)
    {
        var result = new List<T>();
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var command = CreateCommand(db, sql, args);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(map(reader));
        return result;
    }

    private async Task<List<T>> QueryAsync<T>(SqliteConnection db, string sql, Func<SqliteDataReader, T> map, params (string, object?)[] args)
    {
        var result = new List<T>();
        await using var command = CreateCommand(db, sql, args);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(map(reader));
        return result;
    }

    private static SqliteCommand CreateCommand(SqliteConnection db, string sql, params (string, object?)[] args)
    {
        var command = db.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in args)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }

    public event EventHandler? DataChanged;

    public async Task<IReadOnlyDictionary<string, long>> GetDataCleanupCountsAsync()
    {
        await InitializeAsync();
        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        var user = CurrentUserId;
        var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["Transacoes"] = await CountAsync(db, "SELECT COUNT(*) FROM Transacoes WHERE id_usuario=@user AND tipo='despesa'", user),
            ["Receitas"] = await CountAsync(db, "SELECT COUNT(*) FROM Transacoes WHERE id_usuario=@user AND tipo='receita'", user)
                + await CountAsync(db, "SELECT COUNT(*) FROM Receitas WHERE id_usuario=@user", user),
            ["FaturasCartao"] = await CountAsync(db, "SELECT COUNT(*) FROM FaturasCartao f JOIN CartoesCredito c ON c.id_cartao=f.id_cartao WHERE c.id_usuario=@user", user),
            ["Contas"] = await CountAsync(db, "SELECT COUNT(*) FROM Contas WHERE id_usuario=@user", user),
            ["CartoesCredito"] = await CountAsync(db, "SELECT COUNT(*) FROM CartoesCredito WHERE id_usuario=@user", user),
            ["Fornecedores"] = await CountAsync(db, "SELECT COUNT(*) FROM Fornecedores WHERE id_usuario=@user", user),
            ["Metas"] = await CountAsync(db, "SELECT COUNT(*) FROM Metas WHERE id_usuario=@user", user),
            ["Orcamentos"] = await CountAsync(db, "SELECT COUNT(*) FROM Orcamentos WHERE id_usuario=@user", user),
            ["Lembretes"] = await CountAsync(db, "SELECT COUNT(*) FROM Lembretes WHERE id_usuario=@user", user),
            ["Notificacoes"] = await CountAsync(db, "SELECT COUNT(*) FROM Notificacoes WHERE id_usuario=@user", user),
            ["Importacoes"] = await CountAsync(db, "SELECT COUNT(*) FROM Importacoes WHERE id_usuario=@user", user),
            ["Logs"] = await CountAsync(db, "SELECT COUNT(*) FROM Logs WHERE id_usuario=@user", user),
            ["Tags"] = await CountAsync(db, "SELECT COUNT(*) FROM Tags WHERE id_usuario=@user", user)
                + await CountAsync(db, "SELECT COUNT(*) FROM TransacoesTags tt JOIN Transacoes t ON t.id_transacao=tt.id_transacao WHERE t.id_usuario=@user", user)
        };
        return result;
    }

    public async Task CleanSelectedDataAsync(IReadOnlyCollection<string> selectedKeys)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Transacoes", "Receitas", "FaturasCartao", "Contas", "CartoesCredito",
            "Fornecedores", "Metas", "Orcamentos", "Lembretes", "Notificacoes",
            "Importacoes", "Logs", "Tags"
        };
        var selected = selectedKeys.Where(allowed.Contains).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (selected.Count == 0)
            throw new ArgumentException("Selecione ao menos um grupo de dados.");
        if (selected.Count != selectedKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            throw new InvalidOperationException("A seleção contém uma tabela não permitida.");

        await using var db = new SqliteConnection(ConnectionString);
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        var args = new[] { ("@user", (object?)CurrentUserId) };
        try
        {
            // Dependências são removidas/desvinculadas antes dos registros principais.
            if (selected.Contains("FaturasCartao") || selected.Contains("CartoesCredito"))
            {
                await ExecuteAsync(db, "UPDATE Transacoes SET id_fatura=NULL WHERE id_usuario=@user AND id_fatura IS NOT NULL", args);
                await ExecuteAsync(db, "DELETE FROM FaturasCartao WHERE id_cartao IN (SELECT id_cartao FROM CartoesCredito WHERE id_usuario=@user)", args);
            }

            if (selected.Contains("Transacoes"))
            {
                await ExecuteAsync(db, "DELETE FROM TransacoesTags WHERE id_transacao IN (SELECT id_transacao FROM Transacoes WHERE id_usuario=@user AND tipo='despesa')", args);
                await ExecuteAsync(db, "UPDATE Lembretes SET id_transacao=NULL WHERE id_usuario=@user AND id_transacao IN (SELECT id_transacao FROM Transacoes WHERE id_usuario=@user AND tipo='despesa')", args);
                await ExecuteAsync(db, "DELETE FROM Transacoes WHERE id_usuario=@user AND tipo='despesa'", args);
            }
            if (selected.Contains("Receitas"))
            {
                await ExecuteAsync(db, "DELETE FROM TransacoesTags WHERE id_transacao IN (SELECT id_transacao FROM Transacoes WHERE id_usuario=@user AND tipo='receita')", args);
                await ExecuteAsync(db, "UPDATE Lembretes SET id_transacao=NULL WHERE id_usuario=@user AND id_transacao IN (SELECT id_transacao FROM Transacoes WHERE id_usuario=@user AND tipo='receita')", args);
                await ExecuteAsync(db, "DELETE FROM Transacoes WHERE id_usuario=@user AND tipo='receita'", args);
                await ExecuteAsync(db, "DELETE FROM Receitas WHERE id_usuario=@user", args);
            }
            if (selected.Contains("Tags"))
            {
                await ExecuteAsync(db, "DELETE FROM TransacoesTags WHERE id_tag IN (SELECT id_tag FROM Tags WHERE id_usuario=@user)", args);
                await ExecuteAsync(db, "DELETE FROM Tags WHERE id_usuario=@user", args);
            }
            if (selected.Contains("Lembretes")) await ExecuteAsync(db, "DELETE FROM Lembretes WHERE id_usuario=@user", args);
            if (selected.Contains("Metas")) await ExecuteAsync(db, "DELETE FROM Metas WHERE id_usuario=@user", args);
            if (selected.Contains("Orcamentos")) await ExecuteAsync(db, "DELETE FROM Orcamentos WHERE id_usuario=@user", args);
            if (selected.Contains("Notificacoes")) await ExecuteAsync(db, "DELETE FROM Notificacoes WHERE id_usuario=@user", args);
            if (selected.Contains("Importacoes")) await ExecuteAsync(db, "DELETE FROM Importacoes WHERE id_usuario=@user", args);

            if (selected.Contains("Fornecedores"))
            {
                await ExecuteAsync(db, "UPDATE Transacoes SET id_fornecedor=NULL WHERE id_usuario=@user", args);
                await ExecuteAsync(db, "DELETE FROM Fornecedores WHERE id_usuario=@user", args);
            }
            if (selected.Contains("Contas"))
            {
                await ExecuteAsync(db, "UPDATE Transacoes SET id_conta=NULL,id_conta_destino=NULL WHERE id_usuario=@user", args);
                await ExecuteAsync(db, "DELETE FROM Contas WHERE id_usuario=@user", args);
            }
            if (selected.Contains("CartoesCredito"))
            {
                await ExecuteAsync(db, "UPDATE Transacoes SET id_cartao=NULL WHERE id_usuario=@user", args);
                await ExecuteAsync(db, "DELETE FROM CartoesCredito WHERE id_usuario=@user", args);
            }
            if (selected.Contains("Logs")) await ExecuteAsync(db, "DELETE FROM Logs WHERE id_usuario=@user", args);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private static async Task<long> CountAsync(SqliteConnection db, string sql, long userId) =>
        Convert.ToInt64(await ScalarAsync(db, sql, ("@user", userId)) ?? 0);

    private static async Task EnsureColumnAsync(
        SqliteConnection db, string table, string column, string definition)
    {
        await using var info = CreateCommand(db, $"PRAGMA table_info(\"{table}\")");
        await using var reader = await info.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return;
        await reader.DisposeAsync();
        await ExecuteAsync(db, $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition}");
    }

    private static async Task ExecuteAsync(SqliteConnection db, string sql, params (string, object?)[] args)
    {
        await using var command = CreateCommand(db, sql, args);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> ExecuteCountAsync(
        SqliteConnection db, string sql, params (string, object?)[] args)
    {
        await using var command = CreateCommand(db, sql, args);
        return await command.ExecuteNonQueryAsync();
    }

    private static async Task<object?> ScalarAsync(SqliteConnection db, string sql, params (string, object?)[] args)
    {
        await using var command = CreateCommand(db, sql, args);
        return await command.ExecuteScalarAsync();
    }

    private static async Task<decimal> DecimalAsync(SqliteConnection db, string sql, params (string, object?)[] args)
        => Convert.ToDecimal(await ScalarAsync(db, sql, args) ?? 0m);

    // ============================================================
    // SEED E SCHEMA
    // ============================================================

    private static async Task SeedAsync(SqliteConnection db)
    {
        await ExecuteAsync(db, """
            INSERT OR IGNORE INTO Usuarios(id_usuario,nome,email,senha_hash) VALUES(1,'Visitante','demo@money.local','LOCAL_DESKTOP_ACCOUNT');
            INSERT OR IGNORE INTO Configuracoes(id_usuario) VALUES(1);
            INSERT OR IGNORE INTO Contas(id_conta,id_usuario,nome_conta,saldo_inicial,saldo_atual,tipo_conta)
              VALUES(1,1,'Conta principal',8500,8500,'corrente');
            INSERT OR IGNORE INTO Contas(id_conta,id_usuario,nome_conta,saldo_inicial,saldo_atual,tipo_conta)
              VALUES(2,1,'Reserva',12000,12000,'investimento');
            INSERT OR IGNORE INTO CartoesCredito(id_cartao,id_usuario,nome_cartao,limite_credito,dia_fechamento,dia_vencimento,limite_utilizado)
              VALUES(1,1,'Cartão Platinum',10000,8,15,1840);
            """);
        var categories = new (string, string, string, string)[]
        {
            ("Alimentação","despesa",BlingPalette.HeaderDarkHex,"Restaurant"),("Moradia","despesa",BlingPalette.HeaderDarkHex,"House"),
            ("Transporte","despesa",BlingPalette.HeaderDarkHex,"DirectionsCar"),("Saúde","despesa",BlingPalette.HeaderDarkHex,"MedicalServices"),
            ("Educação","despesa",BlingPalette.HeaderDarkHex,"School"),("Lazer","despesa",BlingPalette.HeaderDarkHex,"Movie"),
            ("Compras","despesa",BlingPalette.HeaderDarkHex,"ShoppingBag"),("Utilidades","despesa",BlingPalette.HeaderDarkHex,"Lightbulb"),
            ("Impostos","despesa",BlingPalette.HeaderDarkHex,"RequestQuote"),("Outros","despesa",BlingPalette.HeaderDarkHex,"Category"),
            ("Salário","receita",BlingPalette.PrimaryHex,"TrendingUp"),("Investimentos","receita",BlingPalette.PrimaryHex,"Savings"),
            ("Freelance","receita",BlingPalette.PrimaryHex,"Work"),("Outros","receita",BlingPalette.PrimaryHex,"Category")
        };
        foreach (var (name, type, color, icon) in categories)
            await ExecuteAsync(db, "INSERT OR IGNORE INTO Categorias(id_usuario,nome_categoria,tipo,cor,icone) VALUES(1,@n,@t,@c,@i)",
                ("@n", name), ("@t", type), ("@c", color), ("@i", icon));

        var today = DateTime.Today;
        await ExecuteAsync(db, """
            INSERT INTO Transacoes(id_conta,id_categoria,id_usuario,valor,data,descricao,tipo)
              SELECT 1,id_categoria,1,7200,@d1,'Salário mensal','receita' FROM Categorias WHERE nome_categoria='Salário' LIMIT 1;
            INSERT INTO Transacoes(id_conta,id_categoria,id_usuario,valor,data,descricao,tipo)
              SELECT 1,id_categoria,1,460,@d2,'Supermercado','despesa' FROM Categorias WHERE nome_categoria='Alimentação' LIMIT 1;
            INSERT INTO Transacoes(id_conta,id_categoria,id_usuario,valor,data,descricao,tipo)
              SELECT 1,id_categoria,1,1450,@d3,'Aluguel','despesa' FROM Categorias WHERE nome_categoria='Moradia' LIMIT 1;
            UPDATE Contas SET saldo_atual=saldo_atual+5290 WHERE id_conta=1;
            INSERT OR IGNORE INTO Orcamentos(id_usuario,id_categoria,valor_limite,mes,ano)
              SELECT 1,id_categoria,1200,@m,@y FROM Categorias WHERE nome_categoria='Alimentação' LIMIT 1;
            INSERT OR IGNORE INTO Orcamentos(id_usuario,id_categoria,valor_limite,mes,ano)
              SELECT 1,id_categoria,2000,@m,@y FROM Categorias WHERE nome_categoria='Moradia' LIMIT 1;
            """, ("@d1", today.AddDays(-12)), ("@d2", today.AddDays(-3)), ("@d3", today.AddDays(-8)),
            ("@m", today.Month), ("@y", today.Year));
    }

    private const string Schema = """
        CREATE TABLE IF NOT EXISTS Usuarios(id_usuario INTEGER PRIMARY KEY AUTOINCREMENT,nome TEXT NOT NULL,email TEXT NOT NULL UNIQUE,senha_hash TEXT NOT NULL,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,ultimo_acesso TEXT,ativo INTEGER DEFAULT 1);
        CREATE TABLE IF NOT EXISTS Contas(id_conta INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,nome_conta TEXT NOT NULL,saldo_inicial NUMERIC NOT NULL DEFAULT 0,saldo_atual NUMERIC NOT NULL DEFAULT 0,tipo_conta TEXT NOT NULL CHECK(tipo_conta IN('corrente','poupanca','carteira','investimento')),data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,ativo INTEGER DEFAULT 1,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
        CREATE TABLE IF NOT EXISTS CartoesCredito(id_cartao INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,nome_cartao TEXT NOT NULL,limite_credito NUMERIC NOT NULL CHECK(limite_credito>0),dia_fechamento INTEGER NOT NULL CHECK(dia_fechamento BETWEEN 1 AND 31),dia_vencimento INTEGER NOT NULL CHECK(dia_vencimento BETWEEN 1 AND 31),limite_utilizado NUMERIC DEFAULT 0,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,ativo INTEGER DEFAULT 1,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
        CREATE TABLE IF NOT EXISTS Categorias(id_categoria INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,nome_categoria TEXT NOT NULL,tipo TEXT NOT NULL CHECK(tipo IN('receita','despesa')),id_categoria_pai INTEGER,cor TEXT DEFAULT '#00A859',icone TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,ativo INTEGER DEFAULT 1,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario),FOREIGN KEY(id_categoria_pai) REFERENCES Categorias(id_categoria),UNIQUE(id_usuario,nome_categoria,tipo));
        CREATE TABLE IF NOT EXISTS Transacoes(id_transacao INTEGER PRIMARY KEY AUTOINCREMENT,id_conta INTEGER,id_cartao INTEGER,id_categoria INTEGER NOT NULL,id_usuario INTEGER NOT NULL,valor NUMERIC NOT NULL CHECK(valor>0),data TEXT NOT NULL,descricao TEXT NOT NULL,tipo TEXT NOT NULL CHECK(tipo IN('receita','despesa')),parcelado INTEGER DEFAULT 0,numero_parcelas INTEGER,id_transacao_pai INTEGER,observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,CHECK((id_conta IS NOT NULL AND id_cartao IS NULL) OR (id_conta IS NULL AND id_cartao IS NOT NULL)),FOREIGN KEY(id_conta) REFERENCES Contas(id_conta),FOREIGN KEY(id_cartao) REFERENCES CartoesCredito(id_cartao),FOREIGN KEY(id_categoria) REFERENCES Categorias(id_categoria),FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario),FOREIGN KEY(id_transacao_pai) REFERENCES Transacoes(id_transacao));
        CREATE TABLE IF NOT EXISTS Receitas(id_receita INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,id_categoria INTEGER NOT NULL,valor NUMERIC NOT NULL,data TEXT NOT NULL,descricao TEXT NOT NULL,recorrente INTEGER DEFAULT 0,frequencia TEXT CHECK(frequencia IN('mensal','anual')),data_proximo_pagamento TEXT,observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,ativo INTEGER DEFAULT 1,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario),FOREIGN KEY(id_categoria) REFERENCES Categorias(id_categoria));
        CREATE TABLE IF NOT EXISTS Orcamentos(id_orcamento INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,id_categoria INTEGER NOT NULL,valor_limite NUMERIC NOT NULL,mes INTEGER NOT NULL CHECK(mes BETWEEN 1 AND 12),ano INTEGER NOT NULL,valor_realizado NUMERIC DEFAULT 0,observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario),FOREIGN KEY(id_categoria) REFERENCES Categorias(id_categoria),UNIQUE(id_usuario,id_categoria,mes,ano));
        CREATE TABLE IF NOT EXISTS Configuracoes(id_configuracao INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL UNIQUE,moeda TEXT DEFAULT 'R$',formato_data TEXT DEFAULT 'dd/MM/yyyy',primeiro_dia_semana TEXT DEFAULT 'Domingo',tema TEXT DEFAULT 'Claro',idioma TEXT DEFAULT 'pt-BR',notificacoes_email INTEGER DEFAULT 1,alerta_vencimento_dias INTEGER DEFAULT 3,backup_automatico INTEGER DEFAULT 0,frequencia_backup TEXT DEFAULT 'semanal',FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
        CREATE TABLE IF NOT EXISTS Backups(id_backup INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,nome_arquivo TEXT NOT NULL,tamanho_kb INTEGER,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,localizacao TEXT,tipo TEXT CHECK(tipo IN('manual','automatico')),FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
        CREATE TABLE IF NOT EXISTS Logs(id_log INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER,acao TEXT NOT NULL,tabela TEXT,registro_id INTEGER,dados_anteriores TEXT,dados_novos TEXT,data_hora TEXT DEFAULT CURRENT_TIMESTAMP,origem TEXT,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
        CREATE INDEX IF NOT EXISTS idx_transacoes_usuario_data ON Transacoes(id_usuario,data);
        CREATE INDEX IF NOT EXISTS idx_transacoes_categoria ON Transacoes(id_categoria);
        CREATE INDEX IF NOT EXISTS idx_orcamentos_periodo ON Orcamentos(id_usuario,ano,mes);
        CREATE INDEX IF NOT EXISTS idx_categorias_tipo ON Categorias(id_usuario,tipo);
        CREATE TRIGGER IF NOT EXISTS trg_transacoes_updated AFTER UPDATE ON Transacoes BEGIN UPDATE Transacoes SET data_atualizacao=CURRENT_TIMESTAMP WHERE id_transacao=NEW.id_transacao; END;
        CREATE TRIGGER IF NOT EXISTS trg_orcamentos_updated AFTER UPDATE ON Orcamentos BEGIN UPDATE Orcamentos SET data_atualizacao=CURRENT_TIMESTAMP WHERE id_orcamento=NEW.id_orcamento; END;
        CREATE TRIGGER IF NOT EXISTS trg_contas_updated AFTER UPDATE ON Contas BEGIN UPDATE Contas SET data_atualizacao=CURRENT_TIMESTAMP WHERE id_conta=NEW.id_conta; END;
        """;
}
