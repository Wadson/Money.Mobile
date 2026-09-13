# Contrato de banco compartilhado — Money.Web e Money.Mobile

Ambos os aplicativos usam SQLite e adotam `PRAGMA user_version=30` como contrato de portabilidade.

## Fluxo suportado

1. O aplicativo de origem cria um snapshot SQLite consistente (incluindo dados no WAL).
2. O aplicativo de destino valida `integrity_check`, chaves estrangeiras e estrutura mínima.
3. A cópia importada é inicializada em área isolada e migrada para a versão 30.
4. Somente após a validação a base local é substituída, mantendo uma cópia de segurança.

## Estrutura comum v30

Além das tabelas financeiras centrais, os dois inicializadores criam e preservam:

- `InstituicoesFinanceiras`
- `MovimentosConta`
- `ContextoMovimento`
- `OperacoesFinanceiras`
- `SchemaAudit`
- `ConfiguracoesHistorico`

Os campos bancários de `Contas` e `CartoesCredito` também são comuns. Isso garante que um backup criado no Web não perca identidade bancária ou razão ao ser aberto no Mobile e salvo novamente.

## Validação

No Mobile: `dotnet run --project tests/Money.Audit/Money.Audit.csproj --no-restore`.

No Web: `dotnet test Money.Web.Tests/Money.Web.Tests.csproj --no-restore -m:1`.
