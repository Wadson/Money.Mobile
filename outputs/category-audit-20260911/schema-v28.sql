CREATE TABLE sqlite_sequence(name,seq)
;
CREATE TABLE [Usuarios] (
  [id_usuario] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [nome] text NOT NULL
, [email] text NOT NULL
, [senha_hash] text NOT NULL
, [data_criacao] text DEFAULT (CURRENT_TIMESTAMP) NULL
, [ultimo_acesso] text NULL
, [ativo] bigint DEFAULT (1) NULL
, [tentativas_falhas] bigint DEFAULT (0) NOT NULL
, [bloqueado_ate] text NULL
, [senha_alterada_em] text NULL
, [data_nascimento] text NULL
, [pergunta_recuperacao] text NULL
, [resposta_recuperacao_hash] text NULL
, [palavra_chave_hash] text NULL
, [dica_palavra_chave] text NULL
)
;
CREATE TABLE [Notificacoes] (
  [id_notificacao] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_usuario] bigint NOT NULL
, [titulo] text NOT NULL
, [mensagem] text NOT NULL
, [nivel] text DEFAULT ('info') NOT NULL
, [lida] bigint DEFAULT (0) NOT NULL
, [data_criacao] text DEFAULT (CURRENT_TIMESTAMP) NOT NULL
, CONSTRAINT [FK_Notificacoes_0_0] FOREIGN KEY ([id_usuario]) REFERENCES [Usuarios] ([id_usuario]) ON DELETE NO ACTION ON UPDATE NO ACTION
)
;
CREATE TABLE [Logs] (
  [id_log] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_usuario] bigint NULL
, [acao] text NOT NULL
, [tabela] text NULL
, [registro_id] bigint NULL
, [dados_anteriores] text NULL
, [dados_novos] text NULL
, [data_hora] text DEFAULT (CURRENT_TIMESTAMP) NULL
, [origem] text NULL
, "correlation_id" TEXT, CONSTRAINT [FK_Logs_0_0] FOREIGN KEY ([id_usuario]) REFERENCES [Usuarios] ([id_usuario]) ON DELETE NO ACTION ON UPDATE NO ACTION
)
;
CREATE TABLE [Fornecedores] (
  [id_fornecedor] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_usuario] bigint NOT NULL
, [nome_fornecedor] text NOT NULL
, [ativo] bigint DEFAULT (1) NULL
, [data_criacao] text DEFAULT (CURRENT_TIMESTAMP) NULL
, CONSTRAINT [FK_Fornecedores_0_0] FOREIGN KEY ([id_usuario]) REFERENCES [Usuarios] ([id_usuario]) ON DELETE CASCADE ON UPDATE NO ACTION
)
;
CREATE TABLE [Configuracoes] (
  [id_configuracao] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_usuario] bigint NOT NULL
, [moeda] text DEFAULT ('R$') NULL
, [formato_data] text DEFAULT ('dd/MM/yyyy') NULL
, [primeiro_dia_semana] text DEFAULT ('Domingo') NULL
, [tema] text DEFAULT ('Claro') NULL
, [idioma] text DEFAULT ('pt-BR') NULL
, [notificacoes_email] bigint DEFAULT (1) NULL
, [alerta_vencimento_dias] bigint DEFAULT (3) NULL
, [backup_automatico] bigint DEFAULT (0) NULL
, [frequencia_backup] text DEFAULT ('semanal') NULL
, CONSTRAINT [FK_Configuracoes_0_0] FOREIGN KEY ([id_usuario]) REFERENCES [Usuarios] ([id_usuario]) ON DELETE NO ACTION ON UPDATE NO ACTION
)
;
CREATE TABLE [CartoesCredito] (
  [id_cartao] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_usuario] bigint NOT NULL
, [nome_cartao] text NOT NULL
, [limite_credito] numeric(53,0) NOT NULL
, [dia_fechamento] bigint NOT NULL
, [dia_vencimento] bigint NOT NULL
, [limite_utilizado] numeric(53,0) DEFAULT (0) NULL
, [data_criacao] text DEFAULT (CURRENT_TIMESTAMP) NULL
, [ativo] bigint DEFAULT (1) NULL
, [cor] text DEFAULT ('#00A859') NULL
, [bandeira] text DEFAULT ('outra') NOT NULL
, "id_instituicao" INTEGER, "ultimos_quatro" TEXT, "nome_impresso" TEXT, "variante_visual" TEXT, CONSTRAINT [FK_CartoesCredito_0_0] FOREIGN KEY ([id_usuario]) REFERENCES [Usuarios] ([id_usuario]) ON DELETE NO ACTION ON UPDATE NO ACTION
)
;
CREATE TABLE [FaturasCartao] (
  [id_fatura] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_cartao] bigint NOT NULL
, [mes_referencia] bigint NOT NULL
, [ano_referencia] bigint NOT NULL
, [valor_total] real DEFAULT (0) NOT NULL
, [data_vencimento] text NOT NULL
, [status] text DEFAULT ('aberta') NOT NULL
, [data_pagamento] text NULL
, CONSTRAINT [FK_FaturasCartao_0_0] FOREIGN KEY ([id_cartao]) REFERENCES [CartoesCredito] ([id_cartao]) ON DELETE CASCADE ON UPDATE NO ACTION
)
;
CREATE TABLE [Transacoes] (
  [id_transacao] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_cartao] bigint NULL
, [id_categoria] bigint NOT NULL
, [id_usuario] bigint NOT NULL
, [valor] numeric(53,0) NOT NULL
, [data] text NOT NULL
, [descricao] text NOT NULL
, [tipo] text NOT NULL
, [parcelado] bigint DEFAULT (0) NOT NULL
, [numero_parcelas] bigint NULL
, [id_transacao_pai] bigint NULL
, [observacoes] text NULL
, [data_criacao] text DEFAULT (CURRENT_TIMESTAMP) NULL
, [data_atualizacao] text DEFAULT (CURRENT_TIMESTAMP) NULL
, [id_fatura] bigint NULL
, [data_vencimento] text NOT NULL
, [pago] bigint DEFAULT (0) NOT NULL
, [data_pagamento] text NULL
, [numero_parcela] bigint DEFAULT (1) NOT NULL
, [total_parcelas] bigint DEFAULT (1) NOT NULL
, [recorrente] bigint DEFAULT (0) NOT NULL
, [frequencia] text NULL
, [id_fornecedor] bigint NULL
, id_conta INTEGER REFERENCES Contas(id_conta) ON DELETE SET NULL, id_conta_destino INTEGER REFERENCES Contas(id_conta) ON DELETE SET NULL, CONSTRAINT [FK_Transacoes_0_0] FOREIGN KEY ([id_fornecedor]) REFERENCES [Fornecedores] ([id_fornecedor]) ON DELETE SET NULL ON UPDATE NO ACTION
, CONSTRAINT [FK_Transacoes_1_0] FOREIGN KEY ([id_fatura]) REFERENCES [FaturasCartao] ([id_fatura]) ON DELETE SET NULL ON UPDATE NO ACTION
, CONSTRAINT [FK_Transacoes_2_0] FOREIGN KEY ([id_transacao_pai]) REFERENCES [Transacoes] ([id_transacao]) ON DELETE CASCADE ON UPDATE NO ACTION
, CONSTRAINT [FK_Transacoes_3_0] FOREIGN KEY ([id_usuario]) REFERENCES [Usuarios] ([id_usuario]) ON DELETE CASCADE ON UPDATE NO ACTION
, CONSTRAINT [FK_Transacoes_4_0] FOREIGN KEY ([id_categoria]) REFERENCES [Categorias] ([id_categoria]) ON DELETE RESTRICT ON UPDATE NO ACTION
, CONSTRAINT [FK_Transacoes_5_0] FOREIGN KEY ([id_cartao]) REFERENCES [CartoesCredito] ([id_cartao]) ON DELETE SET NULL ON UPDATE NO ACTION
)
;
CREATE TABLE [Backups] (
  [id_backup] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [id_usuario] bigint NOT NULL
, [nome_arquivo] text NOT NULL
, [tamanho_kb] bigint NULL
, [data_criacao] text DEFAULT (CURRENT_TIMESTAMP) NULL
, [localizacao] text NULL
, [tipo] text NULL
)
;
CREATE TABLE InstituicoesFinanceiras(id_instituicao INTEGER PRIMARY KEY AUTOINCREMENT,codigo TEXT NOT NULL UNIQUE,nome TEXT NOT NULL,codigo_compe TEXT,logo_arquivo TEXT,cor_primaria TEXT,cor_secundaria TEXT,ativo INTEGER NOT NULL DEFAULT 1)
;
CREATE TABLE Receitas(id_receita INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,id_categoria INTEGER NOT NULL,valor NUMERIC NOT NULL,data TEXT NOT NULL,descricao TEXT NOT NULL,recorrente INTEGER DEFAULT 0,frequencia TEXT CHECK(frequencia IN('mensal','anual')),data_proximo_pagamento TEXT,observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,ativo INTEGER DEFAULT 1,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario),FOREIGN KEY(id_categoria) REFERENCES Categorias(id_categoria))
;
CREATE TABLE Orcamentos(id_orcamento INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,id_categoria INTEGER NOT NULL,valor_limite NUMERIC NOT NULL,mes INTEGER NOT NULL CHECK(mes BETWEEN 1 AND 12),ano INTEGER NOT NULL,valor_realizado NUMERIC DEFAULT 0,observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario),FOREIGN KEY(id_categoria) REFERENCES Categorias(id_categoria),UNIQUE(id_usuario,id_categoria,mes,ano))
;
CREATE INDEX idx_transacoes_usuario_data ON Transacoes(id_usuario,data)
;
CREATE INDEX idx_transacoes_categoria ON Transacoes(id_categoria)
;
CREATE INDEX idx_orcamentos_periodo ON Orcamentos(id_usuario,ano,mes)
;
CREATE TRIGGER trg_transacoes_updated AFTER UPDATE ON Transacoes BEGIN UPDATE Transacoes SET data_atualizacao=CURRENT_TIMESTAMP WHERE id_transacao=NEW.id_transacao; END
;
CREATE TRIGGER trg_orcamentos_updated AFTER UPDATE ON Orcamentos BEGIN UPDATE Orcamentos SET data_atualizacao=CURRENT_TIMESTAMP WHERE id_orcamento=NEW.id_orcamento; END
;
CREATE INDEX idx_fornecedores_usuario_nome
  ON Fornecedores(id_usuario,ativo,nome_fornecedor)
;
CREATE INDEX idx_transacoes_fornecedor ON Transacoes(id_fornecedor)
;
CREATE INDEX idx_transacoes_usuario_tipo_pago_vencimento ON Transacoes(id_usuario,tipo,pago,data_vencimento)
;
CREATE INDEX idx_transacoes_usuario_tipo_pago_pagamento ON Transacoes(id_usuario,tipo,pago,data_pagamento)
;
CREATE INDEX idx_cartoes_usuario_ativo_instituicao_bandeira ON CartoesCredito(id_usuario,ativo,id_instituicao,bandeira)
;
CREATE TABLE Importacoes(

  id_importacao INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,nome_arquivo TEXT NOT NULL,

  formato TEXT NOT NULL,total_registros INTEGER NOT NULL DEFAULT 0,duplicados INTEGER NOT NULL DEFAULT 0,

  data_importacao TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario))
;
CREATE TABLE Tags(

  id_tag INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,

  nome_tag TEXT NOT NULL,cor TEXT DEFAULT '#00A859',

  data_criacao TEXT DEFAULT CURRENT_TIMESTAMP, "icone" TEXT DEFAULT 'tag',

  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,

  UNIQUE(id_usuario,nome_tag))
;
CREATE TABLE Metas(

  id_meta INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,

  nome_meta TEXT NOT NULL,valor_objetivo REAL NOT NULL,valor_atual REAL DEFAULT 0,

  data_inicio TEXT NOT NULL,data_meta TEXT NOT NULL,categoria TEXT,

  prioridade TEXT DEFAULT 'media',status TEXT DEFAULT 'em_andamento',

  observacoes TEXT,data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,

  data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE)
;
CREATE TABLE Lembretes(

  id_lembrete INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,

  id_transacao INTEGER,titulo TEXT NOT NULL,descricao TEXT,data_lembrete TEXT NOT NULL,

  status TEXT DEFAULT 'pendente',data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,

  FOREIGN KEY(id_transacao) REFERENCES Transacoes(id_transacao) ON DELETE SET NULL)
;
CREATE TABLE TransacoesTags(

  id_transacao INTEGER NOT NULL,id_tag INTEGER NOT NULL,

  PRIMARY KEY(id_transacao,id_tag),

  FOREIGN KEY(id_transacao) REFERENCES Transacoes(id_transacao) ON DELETE CASCADE,

  FOREIGN KEY(id_tag) REFERENCES Tags(id_tag) ON DELETE CASCADE)
;
CREATE TABLE "Contas"(id_conta INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,nome_conta TEXT NOT NULL,saldo_inicial NUMERIC NOT NULL DEFAULT 0,saldo_atual NUMERIC NOT NULL DEFAULT 0,tipo_conta TEXT NOT NULL CHECK(tipo_conta IN('corrente','poupanca','carteira','investimento','reserva_financeira')),data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,ativo INTEGER DEFAULT 1,id_instituicao INTEGER,agencia TEXT,numero_conta_mascarado TEXT, cor TEXT DEFAULT '#00A859', icone TEXT DEFAULT 'AccountBalance',FOREIGN KEY(id_usuario) REFERENCES Usuarios(id_usuario))
;
CREATE TRIGGER trg_contas_updated AFTER UPDATE ON Contas BEGIN UPDATE Contas SET data_atualizacao=CURRENT_TIMESTAMP WHERE id_conta=NEW.id_conta; END
;
CREATE INDEX idx_contas_usuario_ativo_instituicao ON Contas(id_usuario,ativo,id_instituicao)
;
CREATE TABLE MovimentosConta(
  id_movimento INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL,
  id_conta INTEGER NOT NULL REFERENCES Contas(id_conta),data TEXT NOT NULL,
  criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,tipo TEXT NOT NULL,
  natureza TEXT NOT NULL CHECK(natureza IN('credito','debito')),
  valor_centavos INTEGER NOT NULL CHECK(valor_centavos>0),saldo_apos_centavos INTEGER NOT NULL,
  descricao TEXT NOT NULL,referencia TEXT,correlacao TEXT NOT NULL,
  movimento_estornado INTEGER REFERENCES MovimentosConta(id_movimento))
;
CREATE INDEX idx_movimentos_conta_data ON MovimentosConta(id_usuario,id_conta,data,id_movimento)
;
CREATE INDEX idx_movimentos_correlacao ON MovimentosConta(id_usuario,correlacao,tipo)
;
CREATE UNIQUE INDEX idx_movimento_estorno ON MovimentosConta(movimento_estornado) WHERE movimento_estornado IS NOT NULL
;
CREATE TABLE OperacoesFinanceiras(
  id_usuario INTEGER NOT NULL,chave TEXT NOT NULL,payload TEXT NOT NULL,criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY(id_usuario,chave))
;
CREATE TRIGGER ledger_insert AFTER INSERT ON Contas WHEN round(NEW.saldo_atual*100)<>0
BEGIN
  INSERT INTO MovimentosConta(id_usuario,id_conta,data,tipo,natureza,valor_centavos,saldo_apos_centavos,descricao,correlacao)
  VALUES(NEW.id_usuario,NEW.id_conta,date('now','localtime'),'saldo_inicial',CASE WHEN NEW.saldo_atual>=0 THEN 'credito' ELSE 'debito' END,
  abs(CAST(round(NEW.saldo_atual*100) AS INTEGER)),CAST(round(NEW.saldo_atual*100) AS INTEGER),'Saldo inicial cadastrado',lower(hex(randomblob(16))));
END
;
CREATE TRIGGER ledger_nonnegative BEFORE UPDATE OF saldo_atual ON Contas
WHEN round(NEW.saldo_atual*100)<0 AND NEW.saldo_atual<OLD.saldo_atual
BEGIN SELECT RAISE(ABORT,'Saldo insuficiente para esta operação.'); END
;
CREATE TRIGGER ledger_update AFTER UPDATE OF saldo_atual ON Contas
WHEN round(NEW.saldo_atual*100)<>round(OLD.saldo_atual*100)
BEGIN
  INSERT INTO MovimentosConta(id_usuario,id_conta,data,tipo,natureza,valor_centavos,saldo_apos_centavos,descricao,referencia,correlacao,movimento_estornado)
  VALUES(NEW.id_usuario,NEW.id_conta,
    COALESCE((SELECT data FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),date('now','localtime')),
    COALESCE((SELECT tipo FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),'movimento_legado'),
    CASE WHEN NEW.saldo_atual>OLD.saldo_atual THEN 'credito' ELSE 'debito' END,
    abs(CAST(round(NEW.saldo_atual*100) AS INTEGER)-CAST(round(OLD.saldo_atual*100) AS INTEGER)),CAST(round(NEW.saldo_atual*100) AS INTEGER),
    COALESCE((SELECT descricao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),'Alteração por fluxo financeiro existente'),
    (SELECT referencia FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),
    COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16)))),
    (SELECT movimento_estornado FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario));
END
;
CREATE TRIGGER ledger_no_update BEFORE UPDATE ON MovimentosConta BEGIN SELECT RAISE(ABORT,'Movimento imutável; registre estorno.'); END
;
CREATE TRIGGER ledger_no_delete BEFORE DELETE ON MovimentosConta BEGIN SELECT RAISE(ABORT,'Histórico financeiro não pode ser apagado.'); END
;
CREATE TABLE "ContextoMovimento"(
  id INTEGER PRIMARY KEY ,id_usuario INTEGER NOT NULL,tipo TEXT NOT NULL,
  descricao TEXT NOT NULL,data TEXT NOT NULL,referencia TEXT,correlacao TEXT NOT NULL,movimento_estornado INTEGER)
;
CREATE UNIQUE INDEX ux_contexto_usuario ON ContextoMovimento(id_usuario)
;
CREATE TABLE ConfiguracoesHistorico(
  id_configuracao INT,
  id_usuario INT,
  moeda TEXT,
  formato_data TEXT,
  primeiro_dia_semana TEXT,
  tema TEXT,
  idioma TEXT,
  notificacoes_email INT,
  alerta_vencimento_dias INT,
  backup_automatico INT,
  frequencia_backup TEXT,
  origem_rowid INT
)
;
CREATE UNIQUE INDEX ux_configuracoes_usuario25 ON Configuracoes(id_usuario)
;
CREATE UNIQUE INDEX ux_fatura_periodo25 ON FaturasCartao(id_cartao,mes_referencia,ano_referencia)
;
CREATE UNIQUE INDEX ux_orcamento_periodo25 ON Orcamentos(id_usuario,id_categoria,mes,ano)
;
CREATE UNIQUE INDEX ux_usuarios_email_normalizado25 ON Usuarios(lower(trim(email)))
;
CREATE TRIGGER audit25_conta_encerramento BEFORE UPDATE OF ativo ON Contas
WHEN NEW.ativo=0 AND round(NEW.saldo_atual*100)<>0
BEGIN SELECT RAISE(ABORT,'Transfira ou ajuste o saldo antes de encerrar a conta.'); END
;
CREATE TRIGGER audit25_conta_exclusao BEFORE DELETE ON Contas
WHEN round(OLD.saldo_atual*100)<>0 OR EXISTS(SELECT 1 FROM MovimentosConta WHERE id_conta=OLD.id_conta)
BEGIN SELECT RAISE(ABORT,'Conta com saldo ou histórico deve ser preservada.'); END
;
CREATE TABLE SchemaAudit(id INTEGER PRIMARY KEY CHECK(id=1),fingerprint TEXT NOT NULL)
;
CREATE TABLE CategoryMigration26Log(id INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER,id_categoria INTEGER,mensagem TEXT NOT NULL,data TEXT DEFAULT CURRENT_TIMESTAMP)
;
CREATE TABLE CategoryLegacy26(
  id_categoria INT,
  id_usuario INT,
  nome_categoria TEXT,
  tipo TEXT,
  id_categoria_pai INT,
  cor TEXT,
  icone TEXT,
  data_criacao TEXT,
  ativo INT
)
;
CREATE TABLE CategoryMappingLegacy26(
  id_usuario INT,
  id_categoria INT,
  pilar TEXT
)
;
CREATE TABLE CategoriaPilar(
  id_categoria_pilar INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL REFERENCES Usuarios(id_usuario),
  nome TEXT NOT NULL CHECK(length(trim(nome))>=2),nome_normalizado TEXT NOT NULL,tipo TEXT NOT NULL CHECK(tipo IN('receita','despesa')),
  icone TEXT,cor TEXT,ordem INTEGER NOT NULL DEFAULT 0,ativo INTEGER NOT NULL DEFAULT 1 CHECK(ativo IN(0,1)),
  data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,UNIQUE(id_usuario,nome_normalizado))
;
CREATE TABLE "Categorias"(
  id_categoria INTEGER PRIMARY KEY AUTOINCREMENT,id_usuario INTEGER NOT NULL REFERENCES Usuarios(id_usuario),
  id_categoria_pilar INTEGER NOT NULL REFERENCES CategoriaPilar(id_categoria_pilar) ON DELETE RESTRICT,
  nome_categoria TEXT NOT NULL,nome_normalizado TEXT NOT NULL,tipo TEXT NOT NULL CHECK(tipo IN('receita','despesa')),
  cor TEXT,icone TEXT,ativo INTEGER NOT NULL DEFAULT 1 CHECK(ativo IN(0,1)),data_criacao TEXT DEFAULT CURRENT_TIMESTAMP,data_atualizacao TEXT DEFAULT CURRENT_TIMESTAMP,
  UNIQUE(id_usuario,id_categoria_pilar,nome_normalizado))
;
CREATE TABLE CategoriaPilarOrcamentario(id_usuario INTEGER NOT NULL REFERENCES Usuarios(id_usuario),
  id_subcategoria INTEGER NOT NULL REFERENCES Categorias(id_categoria) ON DELETE RESTRICT,pilar TEXT NOT NULL,
  PRIMARY KEY(id_usuario,id_subcategoria),CHECK(pilar IN('FIXAS_ESSENCIAIS','VARIAVEIS_LAZER','RESERVA_EMERGENCIA','INVESTIMENTOS')))
;
CREATE TABLE CategorySeed26(id_usuario INTEGER PRIMARY KEY REFERENCES Usuarios(id_usuario),data TEXT DEFAULT CURRENT_TIMESTAMP)
;
CREATE INDEX idx_categorias_tipo ON Categorias(id_usuario,tipo)
;
CREATE INDEX idx_subcategorias_main26 ON Categorias(id_usuario,id_categoria_pilar,ativo)
;
CREATE INDEX idx_main_categories26 ON CategoriaPilar(id_usuario,ativo,ordem)
;
CREATE TRIGGER audit25_Categorias_principal26_INSERT BEFORE INSERT ON Categorias WHEN NOT EXISTS(SELECT 1 FROM CategoriaPilar p WHERE p.id_categoria_pilar=NEW.id_categoria_pilar AND p.id_usuario=NEW.id_usuario AND p.tipo=NEW.tipo) BEGIN SELECT RAISE(ABORT,'Categoria principal inválida para este usuário ou tipo.'); END
;
CREATE TRIGGER audit25_Categorias_principal26_UPDATE BEFORE UPDATE ON Categorias WHEN NOT EXISTS(SELECT 1 FROM CategoriaPilar p WHERE p.id_categoria_pilar=NEW.id_categoria_pilar AND p.id_usuario=NEW.id_usuario AND p.tipo=NEW.tipo) BEGIN SELECT RAISE(ABORT,'Categoria principal inválida para este usuário ou tipo.'); END
;
CREATE TRIGGER audit25_CategoriaPilarOrcamentario_pilar26_INSERT BEFORE INSERT ON CategoriaPilarOrcamentario WHEN NOT EXISTS(SELECT 1 FROM Categorias c WHERE c.id_categoria=NEW.id_subcategoria AND c.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Subcategoria de outro usuário.'); END
;
CREATE TRIGGER audit25_CategoriaPilarOrcamentario_pilar26_UPDATE BEFORE UPDATE ON CategoriaPilarOrcamentario WHEN NOT EXISTS(SELECT 1 FROM Categorias c WHERE c.id_categoria=NEW.id_subcategoria AND c.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Subcategoria de outro usuário.'); END
;
CREATE TRIGGER audit25_log_CategoriaPilar_INSERT AFTER INSERT ON CategoriaPilar 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'CRIAR_CATEGORIA','CategoriaPilar' ,NEW.id_categoria_pilar,NULL,json_object('id_categoria_pilar',NEW."id_categoria_pilar",'id_usuario',NEW."id_usuario",'nome',NEW."nome",'nome_normalizado',NEW."nome_normalizado",'tipo',NEW."tipo",'icone',NEW."icone",'cor',NEW."cor",'ordem',NEW."ordem",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CategoriaPilar_UPDATE AFTER UPDATE ON CategoriaPilar WHEN json_object('id_categoria_pilar',OLD."id_categoria_pilar",'id_usuario',OLD."id_usuario",'nome',OLD."nome",'nome_normalizado',OLD."nome_normalizado",'tipo',OLD."tipo",'icone',OLD."icone",'cor',OLD."cor",'ordem',OLD."ordem",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao")<>json_object('id_categoria_pilar',NEW."id_categoria_pilar",'id_usuario',NEW."id_usuario",'nome',NEW."nome",'nome_normalizado',NEW."nome_normalizado",'tipo',NEW."tipo",'icone',NEW."icone",'cor',NEW."cor",'ordem',NEW."ordem",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,CASE WHEN OLD.ativo=1 AND NEW.ativo=0 THEN 'DESATIVAR_CATEGORIA' ELSE 'EDITAR_CATEGORIA' END,'CategoriaPilar' ,NEW.id_categoria_pilar,json_object('id_categoria_pilar',OLD."id_categoria_pilar",'id_usuario',OLD."id_usuario",'nome',OLD."nome",'nome_normalizado',OLD."nome_normalizado",'tipo',OLD."tipo",'icone',OLD."icone",'cor',OLD."cor",'ordem',OLD."ordem",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),json_object('id_categoria_pilar',NEW."id_categoria_pilar",'id_usuario',NEW."id_usuario",'nome',NEW."nome",'nome_normalizado',NEW."nome_normalizado",'tipo',NEW."tipo",'icone',NEW."icone",'cor',NEW."cor",'ordem',NEW."ordem",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CategoriaPilar_DELETE AFTER DELETE ON CategoriaPilar 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'EXCLUIR_CATEGORIA','CategoriaPilar' ,OLD.id_categoria_pilar,json_object('id_categoria_pilar',OLD."id_categoria_pilar",'id_usuario',OLD."id_usuario",'nome',OLD."nome",'nome_normalizado',OLD."nome_normalizado",'tipo',OLD."tipo",'icone',OLD."icone",'cor',OLD."cor",'ordem',OLD."ordem",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CategoriaPilarOrcamentario_INSERT AFTER INSERT ON CategoriaPilarOrcamentario 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','CategoriaPilarOrcamentario' ,NEW.id_subcategoria,NULL,json_object('id_usuario',NEW."id_usuario",'id_subcategoria',NEW."id_subcategoria",'pilar',NEW."pilar"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CategoriaPilarOrcamentario_UPDATE AFTER UPDATE ON CategoriaPilarOrcamentario WHEN json_object('id_usuario',OLD."id_usuario",'id_subcategoria',OLD."id_subcategoria",'pilar',OLD."pilar")<>json_object('id_usuario',NEW."id_usuario",'id_subcategoria',NEW."id_subcategoria",'pilar',NEW."pilar")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','CategoriaPilarOrcamentario' ,NEW.id_subcategoria,json_object('id_usuario',OLD."id_usuario",'id_subcategoria',OLD."id_subcategoria",'pilar',OLD."pilar"),json_object('id_usuario',NEW."id_usuario",'id_subcategoria',NEW."id_subcategoria",'pilar',NEW."pilar"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CategoriaPilarOrcamentario_DELETE AFTER DELETE ON CategoriaPilarOrcamentario 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','CategoriaPilarOrcamentario' ,OLD.id_subcategoria,json_object('id_usuario',OLD."id_usuario",'id_subcategoria',OLD."id_subcategoria",'pilar',OLD."pilar"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TABLE CategoryCatalogRepair27(id_usuario INTEGER PRIMARY KEY REFERENCES Usuarios(id_usuario),data TEXT DEFAULT CURRENT_TIMESTAMP)
;
CREATE TRIGGER trg_contas_tipo_check BEFORE INSERT ON Contas
WHEN NEW.tipo_conta NOT IN('carteira','corrente','poupanca','investimento','reserva_financeira')
BEGIN SELECT RAISE(ABORT,'Tipo de conta inválido.'); END
;
CREATE TRIGGER trg_contas_tipo_check_update BEFORE UPDATE ON Contas
WHEN NEW.tipo_conta NOT IN('carteira','corrente','poupanca','investimento','reserva_financeira')
BEGIN SELECT RAISE(ABORT,'Tipo de conta inválido.'); END
;
CREATE VIEW vw_resumo_mensal AS SELECT id_usuario,
  strftime('%Y-%m',data_pagamento) mes_ano,
  COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0) total_receitas,
  COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) total_despesas,
  COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0)-COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) saldo_mes
FROM Transacoes WHERE ((tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL) OR (tipo='despesa' AND pago=1))
  AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=Transacoes.id_transacao))
GROUP BY id_usuario,strftime('%Y-%m',data_pagamento)
;
CREATE VIEW vw_gastos_categoria AS SELECT t.id_usuario,c.nome_categoria,c.tipo,c.cor,strftime('%Y-%m',COALESCE(t.data_vencimento,t.data)) mes_ano,COALESCE(SUM(t.valor),0) total
FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria WHERE t.pago=1 AND t.tipo='despesa'
  AND NOT(COALESCE(t.parcelado,0)=1 AND t.id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao))
GROUP BY t.id_usuario,c.id_categoria,strftime('%Y-%m',COALESCE(t.data_vencimento,t.data))
;
CREATE VIEW vw_saude_financeira AS SELECT id_usuario,
  COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0) total_receitas,
  COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) total_despesas,
  COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0)-COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0) saldo_geral,
  CASE WHEN COALESCE(SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),0)=0 THEN 0 ELSE ROUND(COALESCE(SUM(CASE WHEN tipo='despesa' AND pago=1 THEN valor ELSE 0 END),0)*100.0/SUM(CASE WHEN tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL THEN valor ELSE 0 END),2) END percentual_gastos
FROM Transacoes WHERE tipo IN('receita','despesa') AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=Transacoes.id_transacao)) GROUP BY id_usuario
;
CREATE VIEW vw_contas_pagar AS SELECT t.id_usuario,t.id_transacao,t.descricao,t.valor,COALESCE(t.data_vencimento,t.data) data_vencimento,c.nome_categoria categoria,
  CASE WHEN date(COALESCE(t.data_vencimento,t.data))<date('now','localtime') THEN 'vencida' WHEN date(COALESCE(t.data_vencimento,t.data))=date('now','localtime') THEN 'hoje' WHEN date(COALESCE(t.data_vencimento,t.data))<=date('now','localtime','+7 days') THEN 'proxima_semana' ELSE 'futura' END status_vencimento
FROM Transacoes t JOIN Categorias c ON c.id_categoria=t.id_categoria WHERE t.tipo='despesa' AND t.pago=0
  AND NOT(COALESCE(t.parcelado,0)=1 AND t.id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao))
;
CREATE VIEW vw_orcamento_mensal AS SELECT o.id_usuario,o.mes,o.ano,o.id_categoria,c.nome_categoria,c.cor,o.valor_limite,COALESCE(SUM(t.valor),0) valor_realizado,
  CASE WHEN o.valor_limite=0 THEN 0 ELSE ROUND(COALESCE(SUM(t.valor),0)*100.0/o.valor_limite,2) END percentual_utilizado,
  CASE WHEN COALESCE(SUM(t.valor),0)>o.valor_limite THEN 'estourado' WHEN COALESCE(SUM(t.valor),0)>o.valor_limite*.9 THEN 'atencao' WHEN COALESCE(SUM(t.valor),0)>o.valor_limite*.75 THEN 'cuidado' ELSE 'ok' END status
FROM Orcamentos o JOIN Categorias c ON c.id_categoria=o.id_categoria LEFT JOIN Transacoes t ON t.id_usuario=o.id_usuario AND t.id_categoria=o.id_categoria AND t.tipo='despesa' AND t.pago=1
  AND CAST(strftime('%m',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.mes AND CAST(strftime('%Y',COALESCE(t.data_vencimento,t.data)) AS INTEGER)=o.ano
  AND NOT(COALESCE(t.parcelado,0)=1 AND t.id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=t.id_transacao)) GROUP BY o.id_orcamento
;
CREATE TRIGGER audit25_Usuarios_email_INSERT BEFORE INSERT ON Usuarios WHEN EXISTS(SELECT 1 FROM Usuarios u WHERE lower(trim(u.email))=lower(trim(NEW.email)) AND u.id_usuario<>NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'E-mail já utilizado.'); END
;
CREATE TRIGGER audit25_Usuarios_email_UPDATE BEFORE UPDATE ON Usuarios WHEN EXISTS(SELECT 1 FROM Usuarios u WHERE lower(trim(u.email))=lower(trim(NEW.email)) AND u.id_usuario<>NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'E-mail já utilizado.'); END
;
CREATE TRIGGER audit25_Transacoes_regras_INSERT BEFORE INSERT ON Transacoes WHEN NEW.tipo NOT IN('receita','despesa','transferencia') OR NEW.valor IS NULL OR NEW.valor<=0 OR (NEW.tipo IN('receita','despesa') AND NEW.id_categoria IS NULL) OR (NEW.tipo='despesa' AND NEW.data_vencimento IS NULL) OR (NEW.tipo='transferencia' AND (NEW.id_conta IS NULL OR NEW.id_conta_destino IS NULL OR NEW.id_conta=NEW.id_conta_destino)) BEGIN SELECT RAISE(ABORT,'Dados financeiros inválidos.'); END
;
CREATE TRIGGER audit25_Transacoes_regras_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.tipo NOT IN('receita','despesa','transferencia') OR NEW.valor IS NULL OR NEW.valor<=0 OR (NEW.tipo IN('receita','despesa') AND NEW.id_categoria IS NULL) OR (NEW.tipo='despesa' AND NEW.data_vencimento IS NULL) OR (NEW.tipo='transferencia' AND (NEW.id_conta IS NULL OR NEW.id_conta_destino IS NULL OR NEW.id_conta=NEW.id_conta_destino)) BEGIN SELECT RAISE(ABORT,'Dados financeiros inválidos.'); END
;
CREATE TRIGGER audit25_Orcamentos_regras_INSERT BEFORE INSERT ON Orcamentos WHEN NEW.mes IS NULL OR NEW.mes NOT BETWEEN 1 AND 12 OR NEW.ano IS NULL OR NEW.ano NOT BETWEEN 1900 AND 2100 OR NEW.valor_limite IS NULL OR NEW.valor_limite<=0 OR NOT EXISTS(SELECT 1 FROM Categorias c WHERE c.id_categoria=NEW.id_categoria AND c.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Orçamento inválido.'); END
;
CREATE TRIGGER audit25_Orcamentos_regras_UPDATE BEFORE UPDATE ON Orcamentos WHEN NEW.mes IS NULL OR NEW.mes NOT BETWEEN 1 AND 12 OR NEW.ano IS NULL OR NEW.ano NOT BETWEEN 1900 AND 2100 OR NEW.valor_limite IS NULL OR NEW.valor_limite<=0 OR NOT EXISTS(SELECT 1 FROM Categorias c WHERE c.id_categoria=NEW.id_categoria AND c.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Orçamento inválido.'); END
;
CREATE TRIGGER audit25_MovimentosConta_regras_INSERT BEFORE INSERT ON MovimentosConta WHEN NEW.natureza IS NULL OR NEW.natureza NOT IN('credito','debito') OR NEW.valor_centavos IS NULL OR NEW.valor_centavos<=0 OR NOT EXISTS(SELECT 1 FROM Contas c WHERE c.id_conta=NEW.id_conta AND c.id_usuario=NEW.id_usuario) OR (NEW.movimento_estornado IS NOT NULL AND NOT EXISTS(SELECT 1 FROM MovimentosConta m WHERE m.id_movimento=NEW.movimento_estornado AND m.id_conta=NEW.id_conta AND m.id_usuario=NEW.id_usuario AND m.natureza<>NEW.natureza AND m.valor_centavos=NEW.valor_centavos)) BEGIN SELECT RAISE(ABORT,'Movimento financeiro inválido.'); END
;
CREATE TRIGGER audit25_MovimentosConta_regras_UPDATE BEFORE UPDATE ON MovimentosConta WHEN NEW.natureza IS NULL OR NEW.natureza NOT IN('credito','debito') OR NEW.valor_centavos IS NULL OR NEW.valor_centavos<=0 OR NOT EXISTS(SELECT 1 FROM Contas c WHERE c.id_conta=NEW.id_conta AND c.id_usuario=NEW.id_usuario) OR (NEW.movimento_estornado IS NOT NULL AND NOT EXISTS(SELECT 1 FROM MovimentosConta m WHERE m.id_movimento=NEW.movimento_estornado AND m.id_conta=NEW.id_conta AND m.id_usuario=NEW.id_usuario AND m.natureza<>NEW.natureza AND m.valor_centavos=NEW.valor_centavos)) BEGIN SELECT RAISE(ABORT,'Movimento financeiro inválido.'); END
;
CREATE TRIGGER audit25_Contas_usuario_INSERT BEFORE INSERT ON Contas WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Contas_usuario_UPDATE BEFORE UPDATE ON Contas WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_CartoesCredito_usuario_INSERT BEFORE INSERT ON CartoesCredito WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_CartoesCredito_usuario_UPDATE BEFORE UPDATE ON CartoesCredito WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Categorias_usuario_INSERT BEFORE INSERT ON Categorias WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Categorias_usuario_UPDATE BEFORE UPDATE ON Categorias WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Transacoes_usuario_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Transacoes_usuario_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Fornecedores_usuario_INSERT BEFORE INSERT ON Fornecedores WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Fornecedores_usuario_UPDATE BEFORE UPDATE ON Fornecedores WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Orcamentos_usuario_INSERT BEFORE INSERT ON Orcamentos WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Orcamentos_usuario_UPDATE BEFORE UPDATE ON Orcamentos WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_CategoriaPilar_usuario_INSERT BEFORE INSERT ON CategoriaPilar WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_CategoriaPilar_usuario_UPDATE BEFORE UPDATE ON CategoriaPilar WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Tags_usuario_INSERT BEFORE INSERT ON Tags WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Tags_usuario_UPDATE BEFORE UPDATE ON Tags WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Metas_usuario_INSERT BEFORE INSERT ON Metas WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Metas_usuario_UPDATE BEFORE UPDATE ON Metas WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Lembretes_usuario_INSERT BEFORE INSERT ON Lembretes WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Lembretes_usuario_UPDATE BEFORE UPDATE ON Lembretes WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Configuracoes_usuario_INSERT BEFORE INSERT ON Configuracoes WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Configuracoes_usuario_UPDATE BEFORE UPDATE ON Configuracoes WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_MovimentosConta_usuario_INSERT BEFORE INSERT ON MovimentosConta WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_MovimentosConta_usuario_UPDATE BEFORE UPDATE ON MovimentosConta WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_OperacoesFinanceiras_usuario_INSERT BEFORE INSERT ON OperacoesFinanceiras WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_OperacoesFinanceiras_usuario_UPDATE BEFORE UPDATE ON OperacoesFinanceiras WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_ContextoMovimento_usuario_INSERT BEFORE INSERT ON ContextoMovimento WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_ContextoMovimento_usuario_UPDATE BEFORE UPDATE ON ContextoMovimento WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Backups_usuario_INSERT BEFORE INSERT ON Backups WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Backups_usuario_UPDATE BEFORE UPDATE ON Backups WHEN NEW.id_usuario IS NULL OR NOT EXISTS(SELECT 1 FROM Usuarios u WHERE u.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Usuário inválido.'); END
;
CREATE TRIGGER audit25_Transacoes_id_conta_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_conta IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Contas r WHERE r.id_conta=NEW.id_conta AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_conta_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_conta IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Contas r WHERE r.id_conta=NEW.id_conta AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_conta_destino_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_conta_destino IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Contas r WHERE r.id_conta=NEW.id_conta_destino AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_conta_destino_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_conta_destino IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Contas r WHERE r.id_conta=NEW.id_conta_destino AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_cartao_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_cartao IS NOT NULL AND NOT EXISTS(SELECT 1 FROM CartoesCredito r WHERE r.id_cartao=NEW.id_cartao AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_cartao_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_cartao IS NOT NULL AND NOT EXISTS(SELECT 1 FROM CartoesCredito r WHERE r.id_cartao=NEW.id_cartao AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_categoria_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_categoria IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Categorias r WHERE r.id_categoria=NEW.id_categoria AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_categoria_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_categoria IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Categorias r WHERE r.id_categoria=NEW.id_categoria AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_fornecedor_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_fornecedor IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Fornecedores r WHERE r.id_fornecedor=NEW.id_fornecedor AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_fornecedor_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_fornecedor IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Fornecedores r WHERE r.id_fornecedor=NEW.id_fornecedor AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_transacao_pai_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_transacao_pai IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Transacoes r WHERE r.id_transacao=NEW.id_transacao_pai AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_id_transacao_pai_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_transacao_pai IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Transacoes r WHERE r.id_transacao=NEW.id_transacao_pai AND r.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Vínculo não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Lembretes_transacao_INSERT BEFORE INSERT ON Lembretes WHEN NEW.id_transacao IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Transacoes t WHERE t.id_transacao=NEW.id_transacao AND t.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Lançamento do lembrete não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Lembretes_transacao_UPDATE BEFORE UPDATE ON Lembretes WHEN NEW.id_transacao IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Transacoes t WHERE t.id_transacao=NEW.id_transacao AND t.id_usuario=NEW.id_usuario) BEGIN SELECT RAISE(ABORT,'Lançamento do lembrete não pertence ao usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_fatura_INSERT BEFORE INSERT ON Transacoes WHEN NEW.id_fatura IS NOT NULL AND NOT EXISTS(SELECT 1 FROM FaturasCartao f JOIN CartoesCredito c ON c.id_cartao=f.id_cartao WHERE f.id_fatura=NEW.id_fatura AND c.id_usuario=NEW.id_usuario AND c.id_cartao=NEW.id_cartao) BEGIN SELECT RAISE(ABORT,'Fatura incompatível com o cartão ou usuário.'); END
;
CREATE TRIGGER audit25_Transacoes_fatura_UPDATE BEFORE UPDATE ON Transacoes WHEN NEW.id_fatura IS NOT NULL AND NOT EXISTS(SELECT 1 FROM FaturasCartao f JOIN CartoesCredito c ON c.id_cartao=f.id_cartao WHERE f.id_fatura=NEW.id_fatura AND c.id_usuario=NEW.id_usuario AND c.id_cartao=NEW.id_cartao) BEGIN SELECT RAISE(ABORT,'Fatura incompatível com o cartão ou usuário.'); END
;
CREATE TRIGGER audit25_TransacoesTags_propriedade_INSERT BEFORE INSERT ON TransacoesTags WHEN NOT EXISTS(SELECT 1 FROM Transacoes t JOIN Tags g ON g.id_usuario=t.id_usuario WHERE t.id_transacao=NEW.id_transacao AND g.id_tag=NEW.id_tag) BEGIN SELECT RAISE(ABORT,'Tag e lançamento devem pertencer ao mesmo usuário.'); END
;
CREATE TRIGGER audit25_TransacoesTags_propriedade_UPDATE BEFORE UPDATE ON TransacoesTags WHEN NOT EXISTS(SELECT 1 FROM Transacoes t JOIN Tags g ON g.id_usuario=t.id_usuario WHERE t.id_transacao=NEW.id_transacao AND g.id_tag=NEW.id_tag) BEGIN SELECT RAISE(ABORT,'Tag e lançamento devem pertencer ao mesmo usuário.'); END
;
CREATE TRIGGER audit25_log_Usuarios_INSERT AFTER INSERT ON Usuarios 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Usuarios' ,NEW.id_usuario,NULL,json_object('id_usuario',NEW."id_usuario",'nome',NEW."nome",'email',NEW."email",'data_criacao',NEW."data_criacao",'ultimo_acesso',NEW."ultimo_acesso",'ativo',NEW."ativo",'tentativas_falhas',NEW."tentativas_falhas",'bloqueado_ate',NEW."bloqueado_ate",'data_nascimento',NEW."data_nascimento"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Usuarios_UPDATE AFTER UPDATE ON Usuarios WHEN json_object('id_usuario',OLD."id_usuario",'nome',OLD."nome",'email',OLD."email",'data_criacao',OLD."data_criacao",'ultimo_acesso',OLD."ultimo_acesso",'ativo',OLD."ativo",'tentativas_falhas',OLD."tentativas_falhas",'bloqueado_ate',OLD."bloqueado_ate",'data_nascimento',OLD."data_nascimento")<>json_object('id_usuario',NEW."id_usuario",'nome',NEW."nome",'email',NEW."email",'data_criacao',NEW."data_criacao",'ultimo_acesso',NEW."ultimo_acesso",'ativo',NEW."ativo",'tentativas_falhas',NEW."tentativas_falhas",'bloqueado_ate',NEW."bloqueado_ate",'data_nascimento',NEW."data_nascimento")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Usuarios' ,NEW.id_usuario,json_object('id_usuario',OLD."id_usuario",'nome',OLD."nome",'email',OLD."email",'data_criacao',OLD."data_criacao",'ultimo_acesso',OLD."ultimo_acesso",'ativo',OLD."ativo",'tentativas_falhas',OLD."tentativas_falhas",'bloqueado_ate',OLD."bloqueado_ate",'data_nascimento',OLD."data_nascimento"),json_object('id_usuario',NEW."id_usuario",'nome',NEW."nome",'email',NEW."email",'data_criacao',NEW."data_criacao",'ultimo_acesso',NEW."ultimo_acesso",'ativo',NEW."ativo",'tentativas_falhas',NEW."tentativas_falhas",'bloqueado_ate',NEW."bloqueado_ate",'data_nascimento',NEW."data_nascimento"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Usuarios_DELETE AFTER DELETE ON Usuarios 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Usuarios' ,OLD.id_usuario,json_object('id_usuario',OLD."id_usuario",'nome',OLD."nome",'email',OLD."email",'data_criacao',OLD."data_criacao",'ultimo_acesso',OLD."ultimo_acesso",'ativo',OLD."ativo",'tentativas_falhas',OLD."tentativas_falhas",'bloqueado_ate',OLD."bloqueado_ate",'data_nascimento',OLD."data_nascimento"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Contas_INSERT AFTER INSERT ON Contas 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Contas' ,NEW.id_conta,NULL,json_object('id_conta',NEW."id_conta",'id_usuario',NEW."id_usuario",'nome_conta',NEW."nome_conta",'saldo_inicial',NEW."saldo_inicial",'saldo_atual',NEW."saldo_atual",'tipo_conta',NEW."tipo_conta",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao",'ativo',NEW."ativo",'id_instituicao',NEW."id_instituicao",'agencia',NEW."agencia",'numero_conta_mascarado',NEW."numero_conta_mascarado",'cor',NEW."cor",'icone',NEW."icone"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Contas_UPDATE AFTER UPDATE ON Contas WHEN json_object('id_conta',OLD."id_conta",'id_usuario',OLD."id_usuario",'nome_conta',OLD."nome_conta",'saldo_inicial',OLD."saldo_inicial",'saldo_atual',OLD."saldo_atual",'tipo_conta',OLD."tipo_conta",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao",'ativo',OLD."ativo",'id_instituicao',OLD."id_instituicao",'agencia',OLD."agencia",'numero_conta_mascarado',OLD."numero_conta_mascarado",'cor',OLD."cor",'icone',OLD."icone")<>json_object('id_conta',NEW."id_conta",'id_usuario',NEW."id_usuario",'nome_conta',NEW."nome_conta",'saldo_inicial',NEW."saldo_inicial",'saldo_atual',NEW."saldo_atual",'tipo_conta',NEW."tipo_conta",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao",'ativo',NEW."ativo",'id_instituicao',NEW."id_instituicao",'agencia',NEW."agencia",'numero_conta_mascarado',NEW."numero_conta_mascarado",'cor',NEW."cor",'icone',NEW."icone")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Contas' ,NEW.id_conta,json_object('id_conta',OLD."id_conta",'id_usuario',OLD."id_usuario",'nome_conta',OLD."nome_conta",'saldo_inicial',OLD."saldo_inicial",'saldo_atual',OLD."saldo_atual",'tipo_conta',OLD."tipo_conta",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao",'ativo',OLD."ativo",'id_instituicao',OLD."id_instituicao",'agencia',OLD."agencia",'numero_conta_mascarado',OLD."numero_conta_mascarado",'cor',OLD."cor",'icone',OLD."icone"),json_object('id_conta',NEW."id_conta",'id_usuario',NEW."id_usuario",'nome_conta',NEW."nome_conta",'saldo_inicial',NEW."saldo_inicial",'saldo_atual',NEW."saldo_atual",'tipo_conta',NEW."tipo_conta",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao",'ativo',NEW."ativo",'id_instituicao',NEW."id_instituicao",'agencia',NEW."agencia",'numero_conta_mascarado',NEW."numero_conta_mascarado",'cor',NEW."cor",'icone',NEW."icone"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Contas_DELETE AFTER DELETE ON Contas 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Contas' ,OLD.id_conta,json_object('id_conta',OLD."id_conta",'id_usuario',OLD."id_usuario",'nome_conta',OLD."nome_conta",'saldo_inicial',OLD."saldo_inicial",'saldo_atual',OLD."saldo_atual",'tipo_conta',OLD."tipo_conta",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao",'ativo',OLD."ativo",'id_instituicao',OLD."id_instituicao",'agencia',OLD."agencia",'numero_conta_mascarado',OLD."numero_conta_mascarado",'cor',OLD."cor",'icone',OLD."icone"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CartoesCredito_INSERT AFTER INSERT ON CartoesCredito 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','CartoesCredito' ,NEW.id_cartao,NULL,json_object('id_cartao',NEW."id_cartao",'id_usuario',NEW."id_usuario",'nome_cartao',NEW."nome_cartao",'limite_credito',NEW."limite_credito",'dia_fechamento',NEW."dia_fechamento",'dia_vencimento',NEW."dia_vencimento",'limite_utilizado',NEW."limite_utilizado",'data_criacao',NEW."data_criacao",'ativo',NEW."ativo",'cor',NEW."cor",'bandeira',NEW."bandeira",'id_instituicao',NEW."id_instituicao",'ultimos_quatro',NEW."ultimos_quatro",'nome_impresso',NEW."nome_impresso",'variante_visual',NEW."variante_visual"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CartoesCredito_UPDATE AFTER UPDATE ON CartoesCredito WHEN json_object('id_cartao',OLD."id_cartao",'id_usuario',OLD."id_usuario",'nome_cartao',OLD."nome_cartao",'limite_credito',OLD."limite_credito",'dia_fechamento',OLD."dia_fechamento",'dia_vencimento',OLD."dia_vencimento",'limite_utilizado',OLD."limite_utilizado",'data_criacao',OLD."data_criacao",'ativo',OLD."ativo",'cor',OLD."cor",'bandeira',OLD."bandeira",'id_instituicao',OLD."id_instituicao",'ultimos_quatro',OLD."ultimos_quatro",'nome_impresso',OLD."nome_impresso",'variante_visual',OLD."variante_visual")<>json_object('id_cartao',NEW."id_cartao",'id_usuario',NEW."id_usuario",'nome_cartao',NEW."nome_cartao",'limite_credito',NEW."limite_credito",'dia_fechamento',NEW."dia_fechamento",'dia_vencimento',NEW."dia_vencimento",'limite_utilizado',NEW."limite_utilizado",'data_criacao',NEW."data_criacao",'ativo',NEW."ativo",'cor',NEW."cor",'bandeira',NEW."bandeira",'id_instituicao',NEW."id_instituicao",'ultimos_quatro',NEW."ultimos_quatro",'nome_impresso',NEW."nome_impresso",'variante_visual',NEW."variante_visual")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','CartoesCredito' ,NEW.id_cartao,json_object('id_cartao',OLD."id_cartao",'id_usuario',OLD."id_usuario",'nome_cartao',OLD."nome_cartao",'limite_credito',OLD."limite_credito",'dia_fechamento',OLD."dia_fechamento",'dia_vencimento',OLD."dia_vencimento",'limite_utilizado',OLD."limite_utilizado",'data_criacao',OLD."data_criacao",'ativo',OLD."ativo",'cor',OLD."cor",'bandeira',OLD."bandeira",'id_instituicao',OLD."id_instituicao",'ultimos_quatro',OLD."ultimos_quatro",'nome_impresso',OLD."nome_impresso",'variante_visual',OLD."variante_visual"),json_object('id_cartao',NEW."id_cartao",'id_usuario',NEW."id_usuario",'nome_cartao',NEW."nome_cartao",'limite_credito',NEW."limite_credito",'dia_fechamento',NEW."dia_fechamento",'dia_vencimento',NEW."dia_vencimento",'limite_utilizado',NEW."limite_utilizado",'data_criacao',NEW."data_criacao",'ativo',NEW."ativo",'cor',NEW."cor",'bandeira',NEW."bandeira",'id_instituicao',NEW."id_instituicao",'ultimos_quatro',NEW."ultimos_quatro",'nome_impresso',NEW."nome_impresso",'variante_visual',NEW."variante_visual"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_CartoesCredito_DELETE AFTER DELETE ON CartoesCredito 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','CartoesCredito' ,OLD.id_cartao,json_object('id_cartao',OLD."id_cartao",'id_usuario',OLD."id_usuario",'nome_cartao',OLD."nome_cartao",'limite_credito',OLD."limite_credito",'dia_fechamento',OLD."dia_fechamento",'dia_vencimento',OLD."dia_vencimento",'limite_utilizado',OLD."limite_utilizado",'data_criacao',OLD."data_criacao",'ativo',OLD."ativo",'cor',OLD."cor",'bandeira',OLD."bandeira",'id_instituicao',OLD."id_instituicao",'ultimos_quatro',OLD."ultimos_quatro",'nome_impresso',OLD."nome_impresso",'variante_visual',OLD."variante_visual"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Categorias_INSERT AFTER INSERT ON Categorias 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'CRIAR_SUBCATEGORIA','Categorias' ,NEW.id_categoria,NULL,json_object('id_categoria',NEW."id_categoria",'id_usuario',NEW."id_usuario",'id_categoria_pilar',NEW."id_categoria_pilar",'nome_categoria',NEW."nome_categoria",'nome_normalizado',NEW."nome_normalizado",'tipo',NEW."tipo",'cor',NEW."cor",'icone',NEW."icone",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Categorias_UPDATE AFTER UPDATE ON Categorias WHEN json_object('id_categoria',OLD."id_categoria",'id_usuario',OLD."id_usuario",'id_categoria_pilar',OLD."id_categoria_pilar",'nome_categoria',OLD."nome_categoria",'nome_normalizado',OLD."nome_normalizado",'tipo',OLD."tipo",'cor',OLD."cor",'icone',OLD."icone",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao")<>json_object('id_categoria',NEW."id_categoria",'id_usuario',NEW."id_usuario",'id_categoria_pilar',NEW."id_categoria_pilar",'nome_categoria',NEW."nome_categoria",'nome_normalizado',NEW."nome_normalizado",'tipo',NEW."tipo",'cor',NEW."cor",'icone',NEW."icone",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,CASE WHEN OLD.ativo=1 AND NEW.ativo=0 THEN 'DESATIVAR_SUBCATEGORIA' ELSE 'EDITAR_SUBCATEGORIA' END,'Categorias' ,NEW.id_categoria,json_object('id_categoria',OLD."id_categoria",'id_usuario',OLD."id_usuario",'id_categoria_pilar',OLD."id_categoria_pilar",'nome_categoria',OLD."nome_categoria",'nome_normalizado',OLD."nome_normalizado",'tipo',OLD."tipo",'cor',OLD."cor",'icone',OLD."icone",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),json_object('id_categoria',NEW."id_categoria",'id_usuario',NEW."id_usuario",'id_categoria_pilar',NEW."id_categoria_pilar",'nome_categoria',NEW."nome_categoria",'nome_normalizado',NEW."nome_normalizado",'tipo',NEW."tipo",'cor',NEW."cor",'icone',NEW."icone",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Categorias_DELETE AFTER DELETE ON Categorias 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'EXCLUIR_SUBCATEGORIA','Categorias' ,OLD.id_categoria,json_object('id_categoria',OLD."id_categoria",'id_usuario',OLD."id_usuario",'id_categoria_pilar',OLD."id_categoria_pilar",'nome_categoria',OLD."nome_categoria",'nome_normalizado',OLD."nome_normalizado",'tipo',OLD."tipo",'cor',OLD."cor",'icone',OLD."icone",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Fornecedores_INSERT AFTER INSERT ON Fornecedores 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Fornecedores' ,NEW.id_fornecedor,NULL,json_object('id_fornecedor',NEW."id_fornecedor",'id_usuario',NEW."id_usuario",'nome_fornecedor',NEW."nome_fornecedor",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Fornecedores_UPDATE AFTER UPDATE ON Fornecedores WHEN json_object('id_fornecedor',OLD."id_fornecedor",'id_usuario',OLD."id_usuario",'nome_fornecedor',OLD."nome_fornecedor",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao")<>json_object('id_fornecedor',NEW."id_fornecedor",'id_usuario',NEW."id_usuario",'nome_fornecedor',NEW."nome_fornecedor",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Fornecedores' ,NEW.id_fornecedor,json_object('id_fornecedor',OLD."id_fornecedor",'id_usuario',OLD."id_usuario",'nome_fornecedor',OLD."nome_fornecedor",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao"),json_object('id_fornecedor',NEW."id_fornecedor",'id_usuario',NEW."id_usuario",'nome_fornecedor',NEW."nome_fornecedor",'ativo',NEW."ativo",'data_criacao',NEW."data_criacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Fornecedores_DELETE AFTER DELETE ON Fornecedores 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Fornecedores' ,OLD.id_fornecedor,json_object('id_fornecedor',OLD."id_fornecedor",'id_usuario',OLD."id_usuario",'nome_fornecedor',OLD."nome_fornecedor",'ativo',OLD."ativo",'data_criacao',OLD."data_criacao"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Orcamentos_INSERT AFTER INSERT ON Orcamentos 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Orcamentos' ,NEW.id_orcamento,NULL,json_object('id_orcamento',NEW."id_orcamento",'id_usuario',NEW."id_usuario",'id_categoria',NEW."id_categoria",'valor_limite',NEW."valor_limite",'mes',NEW."mes",'ano',NEW."ano",'valor_realizado',NEW."valor_realizado",'observacoes',NEW."observacoes",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Orcamentos_UPDATE AFTER UPDATE ON Orcamentos WHEN json_object('id_orcamento',OLD."id_orcamento",'id_usuario',OLD."id_usuario",'id_categoria',OLD."id_categoria",'valor_limite',OLD."valor_limite",'mes',OLD."mes",'ano',OLD."ano",'valor_realizado',OLD."valor_realizado",'observacoes',OLD."observacoes",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao")<>json_object('id_orcamento',NEW."id_orcamento",'id_usuario',NEW."id_usuario",'id_categoria',NEW."id_categoria",'valor_limite',NEW."valor_limite",'mes',NEW."mes",'ano',NEW."ano",'valor_realizado',NEW."valor_realizado",'observacoes',NEW."observacoes",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Orcamentos' ,NEW.id_orcamento,json_object('id_orcamento',OLD."id_orcamento",'id_usuario',OLD."id_usuario",'id_categoria',OLD."id_categoria",'valor_limite',OLD."valor_limite",'mes',OLD."mes",'ano',OLD."ano",'valor_realizado',OLD."valor_realizado",'observacoes',OLD."observacoes",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),json_object('id_orcamento',NEW."id_orcamento",'id_usuario',NEW."id_usuario",'id_categoria',NEW."id_categoria",'valor_limite',NEW."valor_limite",'mes',NEW."mes",'ano',NEW."ano",'valor_realizado',NEW."valor_realizado",'observacoes',NEW."observacoes",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Orcamentos_DELETE AFTER DELETE ON Orcamentos 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Orcamentos' ,OLD.id_orcamento,json_object('id_orcamento',OLD."id_orcamento",'id_usuario',OLD."id_usuario",'id_categoria',OLD."id_categoria",'valor_limite',OLD."valor_limite",'mes',OLD."mes",'ano',OLD."ano",'valor_realizado',OLD."valor_realizado",'observacoes',OLD."observacoes",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Tags_INSERT AFTER INSERT ON Tags 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Tags' ,NEW.id_tag,NULL,json_object('id_tag',NEW."id_tag",'id_usuario',NEW."id_usuario",'nome_tag',NEW."nome_tag",'cor',NEW."cor",'data_criacao',NEW."data_criacao",'icone',NEW."icone"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Tags_UPDATE AFTER UPDATE ON Tags WHEN json_object('id_tag',OLD."id_tag",'id_usuario',OLD."id_usuario",'nome_tag',OLD."nome_tag",'cor',OLD."cor",'data_criacao',OLD."data_criacao",'icone',OLD."icone")<>json_object('id_tag',NEW."id_tag",'id_usuario',NEW."id_usuario",'nome_tag',NEW."nome_tag",'cor',NEW."cor",'data_criacao',NEW."data_criacao",'icone',NEW."icone")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Tags' ,NEW.id_tag,json_object('id_tag',OLD."id_tag",'id_usuario',OLD."id_usuario",'nome_tag',OLD."nome_tag",'cor',OLD."cor",'data_criacao',OLD."data_criacao",'icone',OLD."icone"),json_object('id_tag',NEW."id_tag",'id_usuario',NEW."id_usuario",'nome_tag',NEW."nome_tag",'cor',NEW."cor",'data_criacao',NEW."data_criacao",'icone',NEW."icone"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Tags_DELETE AFTER DELETE ON Tags 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Tags' ,OLD.id_tag,json_object('id_tag',OLD."id_tag",'id_usuario',OLD."id_usuario",'nome_tag',OLD."nome_tag",'cor',OLD."cor",'data_criacao',OLD."data_criacao",'icone',OLD."icone"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Metas_INSERT AFTER INSERT ON Metas 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Metas' ,NEW.id_meta,NULL,json_object('id_meta',NEW."id_meta",'id_usuario',NEW."id_usuario",'nome_meta',NEW."nome_meta",'valor_objetivo',NEW."valor_objetivo",'valor_atual',NEW."valor_atual",'data_inicio',NEW."data_inicio",'data_meta',NEW."data_meta",'categoria',NEW."categoria",'prioridade',NEW."prioridade",'status',NEW."status",'observacoes',NEW."observacoes",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Metas_UPDATE AFTER UPDATE ON Metas WHEN json_object('id_meta',OLD."id_meta",'id_usuario',OLD."id_usuario",'nome_meta',OLD."nome_meta",'valor_objetivo',OLD."valor_objetivo",'valor_atual',OLD."valor_atual",'data_inicio',OLD."data_inicio",'data_meta',OLD."data_meta",'categoria',OLD."categoria",'prioridade',OLD."prioridade",'status',OLD."status",'observacoes',OLD."observacoes",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao")<>json_object('id_meta',NEW."id_meta",'id_usuario',NEW."id_usuario",'nome_meta',NEW."nome_meta",'valor_objetivo',NEW."valor_objetivo",'valor_atual',NEW."valor_atual",'data_inicio',NEW."data_inicio",'data_meta',NEW."data_meta",'categoria',NEW."categoria",'prioridade',NEW."prioridade",'status',NEW."status",'observacoes',NEW."observacoes",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Metas' ,NEW.id_meta,json_object('id_meta',OLD."id_meta",'id_usuario',OLD."id_usuario",'nome_meta',OLD."nome_meta",'valor_objetivo',OLD."valor_objetivo",'valor_atual',OLD."valor_atual",'data_inicio',OLD."data_inicio",'data_meta',OLD."data_meta",'categoria',OLD."categoria",'prioridade',OLD."prioridade",'status',OLD."status",'observacoes',OLD."observacoes",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),json_object('id_meta',NEW."id_meta",'id_usuario',NEW."id_usuario",'nome_meta',NEW."nome_meta",'valor_objetivo',NEW."valor_objetivo",'valor_atual',NEW."valor_atual",'data_inicio',NEW."data_inicio",'data_meta',NEW."data_meta",'categoria',NEW."categoria",'prioridade',NEW."prioridade",'status',NEW."status",'observacoes',NEW."observacoes",'data_criacao',NEW."data_criacao",'data_atualizacao',NEW."data_atualizacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Metas_DELETE AFTER DELETE ON Metas 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Metas' ,OLD.id_meta,json_object('id_meta',OLD."id_meta",'id_usuario',OLD."id_usuario",'nome_meta',OLD."nome_meta",'valor_objetivo',OLD."valor_objetivo",'valor_atual',OLD."valor_atual",'data_inicio',OLD."data_inicio",'data_meta',OLD."data_meta",'categoria',OLD."categoria",'prioridade',OLD."prioridade",'status',OLD."status",'observacoes',OLD."observacoes",'data_criacao',OLD."data_criacao",'data_atualizacao',OLD."data_atualizacao"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Lembretes_INSERT AFTER INSERT ON Lembretes 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Lembretes' ,NEW.id_lembrete,NULL,json_object('id_lembrete',NEW."id_lembrete",'id_usuario',NEW."id_usuario",'id_transacao',NEW."id_transacao",'titulo',NEW."titulo",'descricao',NEW."descricao",'data_lembrete',NEW."data_lembrete",'status',NEW."status",'data_criacao',NEW."data_criacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Lembretes_UPDATE AFTER UPDATE ON Lembretes WHEN json_object('id_lembrete',OLD."id_lembrete",'id_usuario',OLD."id_usuario",'id_transacao',OLD."id_transacao",'titulo',OLD."titulo",'descricao',OLD."descricao",'data_lembrete',OLD."data_lembrete",'status',OLD."status",'data_criacao',OLD."data_criacao")<>json_object('id_lembrete',NEW."id_lembrete",'id_usuario',NEW."id_usuario",'id_transacao',NEW."id_transacao",'titulo',NEW."titulo",'descricao',NEW."descricao",'data_lembrete',NEW."data_lembrete",'status',NEW."status",'data_criacao',NEW."data_criacao")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Lembretes' ,NEW.id_lembrete,json_object('id_lembrete',OLD."id_lembrete",'id_usuario',OLD."id_usuario",'id_transacao',OLD."id_transacao",'titulo',OLD."titulo",'descricao',OLD."descricao",'data_lembrete',OLD."data_lembrete",'status',OLD."status",'data_criacao',OLD."data_criacao"),json_object('id_lembrete',NEW."id_lembrete",'id_usuario',NEW."id_usuario",'id_transacao',NEW."id_transacao",'titulo',NEW."titulo",'descricao',NEW."descricao",'data_lembrete',NEW."data_lembrete",'status',NEW."status",'data_criacao',NEW."data_criacao"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Lembretes_DELETE AFTER DELETE ON Lembretes 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Lembretes' ,OLD.id_lembrete,json_object('id_lembrete',OLD."id_lembrete",'id_usuario',OLD."id_usuario",'id_transacao',OLD."id_transacao",'titulo',OLD."titulo",'descricao',OLD."descricao",'data_lembrete',OLD."data_lembrete",'status',OLD."status",'data_criacao',OLD."data_criacao"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Configuracoes_INSERT AFTER INSERT ON Configuracoes 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'INSERT','Configuracoes' ,NEW.id_usuario,NULL,json_object('id_configuracao',NEW."id_configuracao",'id_usuario',NEW."id_usuario",'moeda',NEW."moeda",'formato_data',NEW."formato_data",'primeiro_dia_semana',NEW."primeiro_dia_semana",'tema',NEW."tema",'idioma',NEW."idioma",'notificacoes_email',NEW."notificacoes_email",'alerta_vencimento_dias',NEW."alerta_vencimento_dias",'backup_automatico',NEW."backup_automatico",'frequencia_backup',NEW."frequencia_backup"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Configuracoes_UPDATE AFTER UPDATE ON Configuracoes WHEN json_object('id_configuracao',OLD."id_configuracao",'id_usuario',OLD."id_usuario",'moeda',OLD."moeda",'formato_data',OLD."formato_data",'primeiro_dia_semana',OLD."primeiro_dia_semana",'tema',OLD."tema",'idioma',OLD."idioma",'notificacoes_email',OLD."notificacoes_email",'alerta_vencimento_dias',OLD."alerta_vencimento_dias",'backup_automatico',OLD."backup_automatico",'frequencia_backup',OLD."frequencia_backup")<>json_object('id_configuracao',NEW."id_configuracao",'id_usuario',NEW."id_usuario",'moeda',NEW."moeda",'formato_data',NEW."formato_data",'primeiro_dia_semana',NEW."primeiro_dia_semana",'tema',NEW."tema",'idioma',NEW."idioma",'notificacoes_email',NEW."notificacoes_email",'alerta_vencimento_dias',NEW."alerta_vencimento_dias",'backup_automatico',NEW."backup_automatico",'frequencia_backup',NEW."frequencia_backup")
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(NEW.id_usuario,'UPDATE','Configuracoes' ,NEW.id_usuario,json_object('id_configuracao',OLD."id_configuracao",'id_usuario',OLD."id_usuario",'moeda',OLD."moeda",'formato_data',OLD."formato_data",'primeiro_dia_semana',OLD."primeiro_dia_semana",'tema',OLD."tema",'idioma',OLD."idioma",'notificacoes_email',OLD."notificacoes_email",'alerta_vencimento_dias',OLD."alerta_vencimento_dias",'backup_automatico',OLD."backup_automatico",'frequencia_backup',OLD."frequencia_backup"),json_object('id_configuracao',NEW."id_configuracao",'id_usuario',NEW."id_usuario",'moeda',NEW."moeda",'formato_data',NEW."formato_data",'primeiro_dia_semana',NEW."primeiro_dia_semana",'tema',NEW."tema",'idioma',NEW."idioma",'notificacoes_email',NEW."notificacoes_email",'alerta_vencimento_dias',NEW."alerta_vencimento_dias",'backup_automatico',NEW."backup_automatico",'frequencia_backup',NEW."frequencia_backup"),'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=NEW.id_usuario),lower(hex(randomblob(16))))); END
;
CREATE TRIGGER audit25_log_Configuracoes_DELETE AFTER DELETE ON Configuracoes 
BEGIN INSERT INTO Logs(id_usuario,acao,tabela,registro_id,dados_anteriores,dados_novos,origem,correlation_id)
VALUES(OLD.id_usuario,'DELETE','Configuracoes' ,OLD.id_usuario,json_object('id_configuracao',OLD."id_configuracao",'id_usuario',OLD."id_usuario",'moeda',OLD."moeda",'formato_data',OLD."formato_data",'primeiro_dia_semana',OLD."primeiro_dia_semana",'tema',OLD."tema",'idioma',OLD."idioma",'notificacoes_email',OLD."notificacoes_email",'alerta_vencimento_dias',OLD."alerta_vencimento_dias",'backup_automatico',OLD."backup_automatico",'frequencia_backup',OLD."frequencia_backup"),NULL,'Money.Web/CRUD',COALESCE((SELECT correlacao FROM ContextoMovimento WHERE id_usuario=OLD.id_usuario),lower(hex(randomblob(16))))); END