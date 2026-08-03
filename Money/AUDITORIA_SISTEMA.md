# Auditoria de sistema, interface e regras de negócio

Data: 30/07/2026

## Escopo verificado

- Migrações e schema SQLite (`DatabaseService`)
- Modelos financeiros
- Telas XAML e code-behind
- Dashboard e views SQL
- Transações, parcelamentos, transferências e contas a pagar
- Categorias, cartões, faturas, metas, lembretes e tags
- Configurações, notificações, logs e backup
- Confirmações, validação de entrada e tratamento de exceções

## Correções realizadas

1. Migração v7 tornada mais segura para colunas que possam existir em bancos parcialmente atualizados.
2. Migração v8 adicionada com validações em `UPDATE` correspondentes às oito famílias de triggers.
3. Validações de metas e lembretes antecipadas na camada de serviço.
4. Formulário de lançamento ampliado com:
   - transferência entre contas;
   - conta de destino;
   - vencimento;
   - status pago e data de pagamento;
   - recorrência e frequência;
   - seleção N:N de tags.
5. Despesa paga passa a debitar a conta no cadastro; despesa pendente não exige saldo antecipadamente.
6. Tela de planejamento adicionada para metas, lembretes e tags, incluindo confirmações de exclusão.
7. Tela de cartões ampliada para gerar/recalcular, fechar e pagar faturas.
8. Pagamento de fatura implementado em transação SQLite: débito da conta, quitação das transações,
   liberação do limite do cartão e log.
9. Cadastro de cartão ampliado com seleção da cor persistida no banco.
10. Dashboard passou a apresentar o status da view `vw_orcamento_mensal` com cor e texto:
    `Estourado`, `Atenção`, `Acompanhe` e `OK`.
11. Tela de configurações e auditoria adicionada para preferências, notificações e logs.
12. Backups manuais passam a gerar registro em `Backups` e `Logs`.
13. Exceções SQLite críticas passam pelo conversor de mensagens amigáveis.
14. Validação de árvore de categoria reforçada também ao mover uma categoria com descendentes.

## Inconsistências ainda identificadas

| Item | Situação | Impacto |
|---|---|---|
| Edição de transação | A tela antiga ainda edita somente descrição, valor, data, categoria e observação | Não permite alterar tags, recorrência ou vencimento após o cadastro |
| Recorrência | O schema e o cadastro persistem recorrência/frequência, mas não existe gerador automático de ocorrências | A recorrência é apenas informativa |
| Configurações globais | Preferências são gravadas e possuem tela, mas moeda/data/tema ainda estão fixos em telas antigas | Formatação não é global |
| Importação/exportação tabular | Serviço de importação existe, mas não há seletor/mapeador CSV/OFX na interface | Recurso não acessível ao usuário |
| Contas bancárias | Não existe tela completa de CRUD de contas no conjunto atual de Views | Cadastro/edição/exclusão de contas não está acessível |
| Orçamentos | Dashboard consulta e estiliza a view, mas não existe formulário de CRUD de orçamento | Limites não podem ser geridos pela interface atual |
| Faturas históricas | A interface de faturas trabalha com o mês atual | Falta navegação livre por mês/ano |
| Notificações | Há consulta e marcação como lida; não há rotina agendada para materializar alertas | Alertas dependem de futura tarefa em segundo plano |
| Logs anteriores | Operações antigas não registravam log de forma uniforme | Auditoria histórica pode ter lacunas |

## Views SQL

| View | Consumidor | Estado |
|---|---|---|
| `vw_resumo_mensal` | Dashboard e relatório mensal | Integrada |
| `vw_gastos_categoria` | Relatório por categoria | Integrada no serviço |
| `vw_saude_financeira` | `GetFinancialHealthAsync` | Integrada no serviço |
| `vw_contas_pagar` | Contas a pagar e dashboard | Integrada |
| `vw_orcamento_mensal` | Dashboard | Integrada e estilizada |

## Resultado de compilação

- `net10.0-windows10.0.19041.0`, Debug: sucesso, zero avisos e zero erros.
- `net10.0-android`, Release: XAML/C# gerou `Money.dll`; o empacotamento completo excedeu o
  limite de execução durante as etapas nativas/AOT.
