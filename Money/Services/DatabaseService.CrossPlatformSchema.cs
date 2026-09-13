using Microsoft.Data.Sqlite;

namespace Money.Services;

public sealed partial class DatabaseService
{
    /// <summary>
    /// Tables and fields shared with the web application. The mobile UI may not expose every
    /// banking feature yet, but it must preserve and create the same SQLite contract so a
    /// snapshot can be restored in either application without data loss.
    /// </summary>
    private static async Task EnsureCrossPlatformSchema30Async(SqliteConnection db)
    {
        await ExecuteAsync(db, """
            CREATE TABLE IF NOT EXISTS Contas(
              id_conta INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,
              nome_conta TEXT NOT NULL,saldo_inicial NUMERIC NOT NULL DEFAULT 0,
              saldo_atual NUMERIC NOT NULL DEFAULT 0,
              tipo_conta TEXT NOT NULL CHECK(tipo_conta IN('corrente','poupanca','carteira','investimento')),
              data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,
              ativo INTEGER DEFAULT 1,id_instituicao INTEGER,agencia TEXT,numero_conta_mascarado TEXT,
              FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario));
            CREATE TABLE IF NOT EXISTS InstituicoesFinanceiras(
              id_instituicao INTEGER PRIMARY KEY AUTOINCREMENT,codigo TEXT NOT NULL UNIQUE,
              nome TEXT NOT NULL,codigo_compe TEXT,logo_arquivo TEXT,cor_primaria TEXT,
              cor_secundaria TEXT,ativo INTEGER NOT NULL DEFAULT 1);
            INSERT OR IGNORE INTO InstituicoesFinanceiras(codigo,nome,codigo_compe,logo_arquivo,cor_primaria,cor_secundaria) VALUES
              ('banco-do-brasil','Banco do Brasil','001','banco-do-brasil.svg','#F8D117','#163A70'),
              ('bradesco','Bradesco','237','bradesco.svg','#CC092F','#8B0622'),
              ('itau','Itaú','341','itau.svg','#EC7000','#003399'),
              ('nubank','Nubank','260','nubank.svg','#820AD1','#4C0677'),
              ('mercado-pago','Mercado Pago','323','mercado-pago.svg','#009EE3','#007EB5'),
              ('caixa','Caixa Econômica Federal','104','caixa.svg','#0066B3','#F7941D'),
              ('santander','Santander','033','santander.svg','#EC0000','#990000'),
              ('inter','Banco Inter','077','inter.svg','#FF7A00','#C95600'),
              ('sicredi','Sicredi','748','sicredi.svg','#3FAE2A','#146E37'),
              ('sicoob','Sicoob','756','sicoob.svg','#003641','#7DB61C'),
              ('outro','Outro banco/carteira',NULL,'outro.svg','#286B50','#163F31');
            CREATE TABLE IF NOT EXISTS MovimentosConta(
              id_movimento INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,
              id_conta INTEGER NOT NULL REFERENCES Contas(id_conta),data TEXT NOT NULL,
              criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,tipo TEXT NOT NULL,
              natureza TEXT NOT NULL CHECK(natureza IN('credito','debito')),
              valor_centavos INTEGER NOT NULL CHECK(valor_centavos>0),saldo_apos_centavos INTEGER NOT NULL,
              descricao TEXT NOT NULL,referencia TEXT,correlacao TEXT NOT NULL,
              movimento_estornado INTEGER REFERENCES MovimentosConta(id_movimento));
            CREATE TABLE IF NOT EXISTS ContextoMovimento(
              id INTEGER PRIMARY KEY,id_usuario INTEGER NOT NULL,tipo TEXT NOT NULL,descricao TEXT NOT NULL,
              data TEXT NOT NULL,referencia TEXT,correlacao TEXT NOT NULL,movimento_estornado INTEGER);
            CREATE TABLE IF NOT EXISTS OperacoesFinanceiras(
              id_usuario INTEGER NOT NULL,chave TEXT NOT NULL,payload TEXT NOT NULL,
              criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,PRIMARY KEY(id_usuario,chave));
            CREATE TABLE IF NOT EXISTS SchemaAudit(id INTEGER PRIMARY KEY CHECK(id=1),fingerprint TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS ConfiguracoesHistorico AS SELECT *,rowid AS origem_rowid FROM Configuracoes WHERE 0;
            CREATE INDEX IF NOT EXISTS idx_movimentos_conta_data ON MovimentosConta(id_usuario,id_conta,data,id_movimento);
            CREATE INDEX IF NOT EXISTS idx_movimentos_correlacao ON MovimentosConta(id_usuario,correlacao,tipo);
            CREATE UNIQUE INDEX IF NOT EXISTS idx_movimento_estorno ON MovimentosConta(movimento_estornado) WHERE movimento_estornado IS NOT NULL;
            CREATE UNIQUE INDEX IF NOT EXISTS ux_contexto_usuario ON ContextoMovimento(id_usuario);
            """);
        await EnsureColumnAsync(db, "Contas", "id_instituicao", "INTEGER");
        await EnsureColumnAsync(db, "Contas", "agencia", "TEXT");
        await EnsureColumnAsync(db, "Contas", "numero_conta_mascarado", "TEXT");
        await EnsureColumnAsync(db, "CartoesCredito", "id_instituicao", "INTEGER");
        await EnsureColumnAsync(db, "CartoesCredito", "bandeira", "TEXT");
        await EnsureColumnAsync(db, "CartoesCredito", "ultimos_quatro", "TEXT");
        await EnsureColumnAsync(db, "CartoesCredito", "nome_impresso", "TEXT");
        await EnsureColumnAsync(db, "CartoesCredito", "variante_visual", "TEXT");
        await ExecuteAsync(db, """
            CREATE INDEX IF NOT EXISTS idx_contas_usuario_ativo_instituicao ON Contas(id_usuario,ativo,id_instituicao);
            CREATE INDEX IF NOT EXISTS idx_cartoes_usuario_ativo_instituicao_bandeira ON CartoesCredito(id_usuario,ativo,id_instituicao,bandeira);
            PRAGMA user_version=30;
            """);
    }
}
