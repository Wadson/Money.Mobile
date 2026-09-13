# Auditoria — Money Pro 1.0.4

## Migração

- Origem: `moneyproAntigo.db`, preservada sem alteração.
- Destino: `moneyproNovo.db`.
- Estrutura do destino comparada com o backup anterior: tabelas, índices, views e triggers idênticos.
- Somente tabelas já existentes receberam dados. Nenhuma tabela auxiliar permaneceu no banco.
- 250 lançamentos: 128 despesas abertas, 117 despesas pagas e 5 receitas pagas.
- Valores preservados: R$ 30.550,76 em despesas abertas, R$ 12.183,65 em despesas pagas e R$ 9.424,60 em receitas pagas. Esses totais de registros incluem os lançamentos pais de parcelamentos; não são os totais líquidos da interface.
- 2 cartões, 60 fornecedores, 4 faturas e 194 categorias de origem mapeados. Categorias já existentes foram reutilizadas quando coincidiam usuário, nome e tipo.
- Usuários e configurações relacionados foram mapeados, preservando registros existentes no destino. Identificadores conflitantes foram remapeados.
- As 10 transferências e as 6 contas bancárias antigas não foram migradas.
- Valores, datas, situação de pagamento e parcelas reconciliados. Integridade SQLite e relacionamentos aprovados.
- Reexecução da migração: zero duplicações. Controle de IDs em arquivo JSON externo, dentro de `MigrationBackups`.
- Backup original do destino: `MigrationBackups/merge-legacy-20260904-210757-988313/target-before.db`.
- Relatório final da migração: `MigrationBackups/merge-legacy-20260904-210950-879421/report.json`.

## Cadastros ativos

| Cadastro | Novo | Alterar | Excluir | Acesso |
|---|---|---|---|---|
| Cartões | Botão Novo cartão | Botão em cada cartão | Botão em cada cartão | Mais opções → Cadastro de cartões; também em Análise de cartões |
| Categorias | Botão Novo | Botão em cada categoria | Botão em cada categoria | Mais opções → Categorias |
| Fornecedores | Botão Novo | Botão em cada fornecedor | Botão em cada fornecedor | Mais opções → Fornecedores |
| Usuários | Botão Novo | Botão em cada usuário | Botão em cada usuário | Mais opções → Usuários |
| Contas a Pagar | Botão + | Botão em cada conta e arraste | Botão em cada conta e arraste | Contas a Pagar |
| Receitas | Botão + | Botão da listagem | Botão da listagem | Receitas |

Perfis e configurações são telas de edição do registro atual. Análises, relatórios, estornos e backups não são cadastros financeiros. Os módulos bancários removidos anteriormente não foram recriados.

## Comportamento

- Na tela Contas a Pagar, a quitação acontece exclusivamente pela seleção de contas abertas no checkbox e pelo botão inferior que aparece após selecionar.
- Arrastar da esquerda para a direita revela Excluir. Alterar permanece no arraste oposto.
- Exclusões continuam exigindo confirmação. Contas pagas exigem estorno antes da exclusão, conforme a regra existente.
- Excluir cartão confirma a remoção dos lançamentos vinculados e respeita o usuário atual.
- Fornecedores em uso são desativados; a exclusão de acesso de usuário preserva seus dados financeiros, conforme as regras existentes.

## Verificação

- 39 testes de serviço aprovados, zero falhas, com `tests/Money.Audit`, incluindo uma cópia do banco migrado.
- Cobertura: criação/alteração/exclusão de cartões, isolamento por usuário, parcelas/faturas relacionadas, pagamento em lote e rollback, estorno, exclusão de despesas, importação, relatórios, integridade e preservação de dados após inicialização.
- Todos os arquivos XAML das telas passaram pela verificação de XML.
- Não houve teste de interação em aparelho ou emulador Android nesta execução.
- Compilação Android Release concluída com código de saída 0. A primeira tentativa teve uma falha de processo auxiliar do MSBuild no sandbox; a repetição fora dele concluiu a publicação.
- APK entregue: `outputs/MoneyPro-1.0.4-Release.apk`. Assinatura verificada pelo `apksigner` (v1, v2 e v3).
- Para usar os dados migrados no celular, restaure `moneyproNovo.db` em Mais opções → Backup. O APK e o banco são arquivos separados.
