# MONEY — Regras de Negócios, Objetivos e Especificação Funcional
## Sistema de Controle Financeiro Pessoal

**Documento de referência do projeto**  
**Versão:** 1.0  
**Data:** 17/08/2026

---

## 1. Finalidade do documento

Este documento define as regras de negócio, objetivos funcionais, conceitos financeiros, telas, indicadores, projeções e relatórios que devem orientar a evolução do aplicativo **Money — Controle Financeiro Pessoal**.

Ele deve ser mantido dentro do projeto como documento de referência para futuras implementações, auditorias, correções e pesquisas realizadas pelo desenvolvedor ou pelo Codex.

O sistema deve ser simples para o usuário, mas financeiramente consistente.

O objetivo não é apenas registrar o que já aconteceu. O Money deve permitir que o usuário:

- registre receitas e despesas;
- controle contas a pagar;
- acompanhe contas já pagas;
- controle cartões de crédito;
- acompanhe boletos, crediários e compras à vista;
- acompanhe débitos em contas;
- registre salários e outras fontes de renda;
- visualize o saldo realizado;
- visualize o saldo previsto;
- antecipe meses futuros;
- saiba quanto dinheiro provavelmente restará depois dos compromissos;
- identifique meses de risco;
- acompanhe orçamentos;
- acompanhe metas;
- acompanhe dívidas e compromissos;
- tome decisões antes que o problema financeiro aconteça.

---

# 2. Princípio central do Money

O Money deve separar claramente três conceitos:

### 2.1 Realizado

Aquilo que efetivamente aconteceu.

### 2.2 Comprometido

Aquilo que já foi cadastrado e representa uma obrigação financeira futura, mesmo que ainda não tenha sido pago.

### 2.3 Projetado

Uma estimativa do futuro baseada nos lançamentos cadastrados e nas previsões informadas pelo usuário.

Esses conceitos nunca devem ser misturados.

---

# 3. Conceito financeiro oficial

## 3.1 Despesas

Para contas a pagar, a competência financeira é determinada pela:

**DATA DE VENCIMENTO**

Campos:

- `data` = data do lançamento/compra;
- `data_vencimento` = data financeira da obrigação;
- `pago` = indica se a obrigação foi paga;
- `data_pagamento` = data efetiva do pagamento.

Exemplo:

Compra: 25/07  
Vencimento: 05/08  
Pagamento: 04/08

A despesa pertence a agosto.

Se for paga em 15/08, continua pertencendo a agosto.

---

# 4. Receitas

## 4.1 Receita realizada

Uma receita somente é considerada realizada quando:

- `pago = 1`;
- `data_pagamento` está preenchida.

A competência da receita realizada é:

**DATA DE RECEBIMENTO**

Exemplo:

Lançamento: 25/07  
Recebimento: 05/08

A receita realizada pertence a agosto.

---

# 5. Receita prevista

O sistema deve permitir registrar uma receita futura sem considerá-la como dinheiro recebido.

Exemplo:

Salário previsto:

- descrição: Salário;
- valor: R$ 5.000;
- data prevista: 05/09;
- `pago = 0`.

Essa receita:

- não entra em "Entradas Realizadas";
- não aumenta o saldo bancário real;
- entra na "Previsão Financeira";
- entra no "Saldo Projetado";
- pode ser usada para calcular quanto deverá sobrar no futuro.

Quando o salário realmente for recebido:

- `pago = 1`;
- `data_pagamento = data real do recebimento`.

A partir desse momento ele passa a integrar os valores realizados.

---

# 6. Regra para previsão de receitas

Para receitas futuras ainda não recebidas, o sistema deve utilizar a data prevista do lançamento para projeção.

Enquanto a receita estiver pendente:

`data` = data prevista/origem da previsão.

Quando for recebida:

`data_pagamento` = data real do recebimento.

Assim:

### Realizado

Receita:

`pago = 1 AND data_pagamento`

### Projetado

Receita:

`pago = 0 AND data`

Essa distinção é fundamental.

---

# 7. Saldo real x saldo projetado

O aplicativo deve apresentar dois conceitos diferentes.

## 7.1 Saldo atual real

Representa o dinheiro efetivamente disponível nas contas.

Deve considerar:

- saldo inicial;
- receitas efetivamente recebidas;
- despesas efetivamente pagas;
- transferências;
- demais movimentações reais.

Não deve incluir salário futuro nem conta futura.

## 7.2 Saldo projetado

Representa uma previsão do dinheiro que provavelmente estará disponível no futuro.

Para cada período:

`Saldo Projetado = Saldo Inicial do Período + Entradas Previstas/Realizadas - Saídas Previstas/Comprometidas`

O saldo projetado deve ser calculado em sequência.

Exemplo:

Saldo atual: R$ 3.000

Setembro:
+ salário previsto R$ 5.000
- contas previstas R$ 3.500

Saldo projetado setembro:

R$ 4.500

Outubro deve iniciar sua projeção a partir do saldo projetado de setembro, somando as entradas e subtraindo as saídas de outubro.

---

# 8. Dashboard principal

O Dashboard deve ser dividido em duas visões:

## 8.1 Visão Realizada

### BALANÇO DO MÊS

Exibir:

- Entradas realizadas;
- Despesas pagas;
- Resultado realizado;
- Saldo atual das contas.

### ENTRADAS REALIZADAS

Somente:

- receitas recebidas;
- salários recebidos;
- outras entradas efetivamente recebidas.

### CONTAS DO MÊS

Mostrar:

- total de contas do período;
- contas pagas;
- contas pendentes;
- contas vencidas;
- valor comprometido.

---

# 9. Nova visão obrigatória: PREVISÃO FINANCEIRA

Deve existir no Dashboard um bloco específico:

## PREVISÃO FINANCEIRA

Mostrar:

- Entradas previstas;
- Contas futuras;
- Total comprometido;
- Saldo projetado;
- Primeiro mês com saldo insuficiente;
- Maior compromisso futuro;
- quantidade de meses cobertos pela previsão.

Exemplo:

| Mês | Entradas | Contas | Resultado | Saldo Projetado |
|---|---:|---:|---:|---:|
| Agosto | R$ 5.000 | R$ 3.800 | R$ 1.200 | R$ 4.200 |
| Setembro | R$ 5.000 | R$ 4.500 | R$ 500 | R$ 4.700 |
| Outubro | R$ 5.000 | R$ 5.200 | -R$ 200 | R$ 4.500 |

O usuário deve perceber imediatamente que outubro possui déficit de fluxo naquele período.

---

# 10. Filtro de período

O Dashboard e os relatórios devem permitir:

- Hoje;
- Semana atual;
- Mês atual;
- Próximo mês;
- Próximos 3 meses;
- Próximos 6 meses;
- Próximos 12 meses;
- Ano atual;
- Ano seguinte;
- Personalizado;
- Todos os períodos.

Para filtros futuros, a previsão deve ser exibida.

Não retornar zero simplesmente porque ainda não houve pagamento.

---

# 11. Filtros globais

Sempre que tecnicamente aplicável, permitir:

- período;
- conta;
- cartão;
- categoria;
- tipo;
- status;
- fornecedor;
- tag;
- forma/origem de pagamento;
- pago/pendente;
- previsto/realizado;
- parcelado;
- recorrente.

Os filtros devem afetar simultaneamente:

- totais;
- gráficos;
- listas;
- relatórios;
- saldo;
- previsão.

---

# 12. Tela de Receitas

A tela de Receitas deve existir novamente.

Ela deve permitir:

## Cadastro

Campos:

- Descrição;
- Categoria;
- Valor;
- Data prevista;
- Data de recebimento;
- Conta;
- Status;
- Recorrente;
- Frequência;
- Observações.

## Status

- Prevista;
- Recebida;
- Atrasada;
- Cancelada.

## Lista

Colunas:

- Descrição;
- Categoria;
- Valor;
- Data prevista;
- Data recebimento;
- Conta;
- Status;
- Recorrência;
- Ações.

## Indicadores

No topo:

- Total de receitas;
- Total recebido;
- Total previsto;
- Total atrasado;
- Média das receitas;
- Próxima receita;
- Maior receita.

---

# 13. Cadastro de salário

O salário deve ser tratado como uma receita recorrente.

Exemplo:

Descrição: Salário  
Categoria: Salário  
Valor: R$ 5.000  
Frequência: Mensal  
Data prevista: dia 5

O sistema deve permitir:

- criar apenas a previsão;
- gerar previsões futuras;
- alterar uma ocorrência individual;
- alterar toda a série;
- marcar uma ocorrência como recebida;
- registrar a data real do recebimento.

O salário futuro não deve aumentar o saldo real.

Ele deve aumentar somente o saldo projetado.

---

# 14. Receitas recorrentes

O sistema deve permitir:

- mensal;
- anual;
- outras frequências suportadas pela arquitetura.

Ao gerar ocorrências futuras, cada ocorrência deve possuir sua própria data.

Exemplo:

Salário:

05/09  
05/10  
05/11  
05/12

Cada ocorrência é independente.

---

# 15. Contas a pagar

O sistema deve permitir registrar:

- aluguel;
- energia;
- água;
- telefone;
- internet;
- escola;
- impostos;
- boletos;
- crediário;
- financiamento;
- compras à vista;
- despesas de conta corrente;
- outras obrigações.

Cada lançamento deve possuir:

- descrição;
- valor;
- categoria;
- fornecedor;
- conta/cartão;
- data;
- vencimento;
- status;
- data de pagamento;
- recorrência;
- parcelamento;
- observações;
- tags.

---

# 16. Status das despesas

## Pendente

`pago = 0` e vencimento futuro.

## Vence hoje

`pago = 0` e vencimento = hoje.

## Vencida

`pago = 0` e vencimento < hoje.

## Paga

`pago = 1`.

---

# 17. Previsão de contas

Toda conta futura cadastrada deve aparecer na previsão.

Exemplo:

Aluguel:

Vencimento: 10/09  
Valor: R$ 1.500

Mesmo não paga, deve aparecer:

- em setembro;
- na previsão;
- no comprometimento futuro.

Não deve aparecer em "Despesas Pagas".

---

# 18. Fluxo de caixa futuro

O sistema deve oferecer uma visão de fluxo de caixa futuro.

Por dia ou mês:

`Saldo anterior + entradas previstas - saídas previstas = saldo projetado`

Essa funcionalidade deve permitir descobrir antecipadamente:

- meses negativos;
- semanas apertadas;
- excesso de compromissos;
- necessidade de economizar;
- necessidade de adiar compras;
- necessidade de aumentar receita;
- necessidade de reduzir despesas.

O conceito de orçamento de fluxo de caixa é especialmente importante porque considera o momento em que o dinheiro entra e sai, e não apenas o total mensal.

---

# 19. Orçamento

A tabela `Orcamentos` deve ser utilizada como orçamento planejado por categoria.

Exemplo:

Alimentação:
Limite: R$ 1.000

O sistema deve mostrar:

- orçamento;
- realizado;
- comprometido;
- previsto;
- restante;
- percentual utilizado.

Não confundir orçamento com fluxo de caixa.

---

# 20. Orçamento prévio

Além do orçamento por categoria, o usuário deve poder observar:

### Quanto vou receber?

### Quanto já estou comprometido?

### Quanto provavelmente vai sobrar?

### Em que mês vou ficar apertado?

### Qual categoria está consumindo mais?

### Quanto posso gastar sem comprometer o mês?

Essa é uma das principais finalidades do sistema.

---

# 21. Cartões de crédito

O cartão deve possuir:

- limite;
- limite utilizado;
- limite disponível;
- fechamento;
- vencimento;
- fatura atual;
- próximas faturas;
- parcelas futuras.

As compras parceladas devem ser distribuídas pelas respectivas datas de vencimento.

A compra pai não pode ser somada junto com as parcelas.

---

# 22. Fatura de cartão

Para cada fatura:

- mês de referência;
- ano;
- valor;
- vencimento;
- status;
- data de pagamento.

Status:

- aberta;
- fechada;
- vencida;
- paga.

---

# 23. Contas e saldos

Cada conta deve apresentar:

- saldo inicial;
- saldo atual;
- entradas;
- saídas;
- transferências;
- saldo projetado;
- movimentações futuras.

Tipos existentes devem ser preservados:

- corrente;
- poupança;
- carteira;
- investimento.

---

# 24. Transferências

Transferências entre contas próprias não devem ser consideradas:

- receita;
- despesa;
- renda;
- consumo.

Devem apenas movimentar dinheiro entre contas.

O patrimônio financeiro total não deve aumentar nem diminuir por uma transferência interna.

---

# 25. Categorias

As categorias devem permitir análise por:

### Despesas

- Moradia;
- Alimentação;
- Transporte;
- Saúde;
- Educação;
- Lazer;
- Compras;
- Serviços;
- Impostos;
- Dívidas;
- Outras.

### Receitas

- Salário;
- Freelance;
- Investimentos;
- Aluguéis;
- Benefícios;
- Outras receitas.

A estrutura existente deve continuar sendo compatível com categorias definidas pelo usuário.

---

# 26. Fornecedores

Fornecedores devem permitir descobrir:

- quanto gasto com cada fornecedor;
- quantas contas existem;
- quanto está pendente;
- quanto já foi pago;
- histórico;
- próximos vencimentos.

---

# 27. Tags

Tags devem permitir classificações complementares sem substituir categorias.

Exemplos:

- Casa;
- Trabalho;
- Família;
- Viagem;
- Urgente;
- Reembolsável;
- Assinatura.

---

# 28. Metas

As metas devem permitir:

- valor objetivo;
- valor atual;
- prazo;
- prioridade;
- status;
- observações.

O sistema pode apresentar:

- percentual alcançado;
- valor restante;
- média mensal necessária;
- projeção de conclusão.

---

# 29. Reserva financeira

O sistema deve incentivar o acompanhamento de uma reserva financeira.

Não deve prometer uma quantia universal como regra obrigatória.

Deve permitir ao usuário definir:

- objetivo da reserva;
- valor alvo;
- valor atual;
- prazo;
- contribuição mensal desejada.

O aplicativo deve mostrar a evolução.

---

# 30. Indicadores financeiros

O Dashboard deve poder apresentar:

- saldo atual;
- receitas realizadas;
- receitas previstas;
- despesas pagas;
- despesas previstas;
- contas vencidas;
- contas próximas do vencimento;
- resultado do mês;
- resultado projetado;
- taxa de economia;
- comprometimento da renda;
- limite de cartão utilizado;
- limite disponível;
- orçamento consumido;
- metas;
- reserva financeira;
- patrimônio financeiro simplificado;
- meses futuros com risco de saldo negativo.

---

# 31. Taxa de economia

Para períodos realizados:

`Taxa de economia = (Receitas recebidas - Despesas pagas) / Receitas recebidas × 100`

Não calcular quando não houver receita.

Para projeção:

`Taxa de economia projetada = (Entradas previstas - Saídas previstas) / Entradas previstas × 100`

As duas métricas devem possuir nomes diferentes.

---

# 32. Comprometimento da renda

O sistema deve calcular:

`Despesas comprometidas / Receita prevista ou realizada × 100`

Deve indicar claramente se o indicador utiliza:

- renda realizada;
- renda prevista;
- período selecionado.

Nunca apresentar uma porcentagem sem explicar sua base.

---

# 33. Alertas

O sistema deve poder alertar:

- conta vencendo;
- conta vencida;
- cartão próximo do limite;
- fatura próxima do vencimento;
- saldo projetado baixo;
- mês com déficit projetado;
- orçamento próximo do limite;
- orçamento excedido;
- meta atrasada;
- receita prevista atrasada;
- backup pendente.

---

# 34. Relatórios obrigatórios

## 34.1 Relatório de Fluxo de Caixa

Período:

- diário;
- semanal;
- mensal;
- personalizado.

Colunas:

- Data;
- Entradas;
- Saídas;
- Saldo do período;
- Saldo acumulado.

Deve possuir modo:

- Realizado;
- Projetado;
- Realizado + Projetado.

---

# 35. Relatório de Receitas

Mostrar:

- data prevista;
- data recebida;
- descrição;
- categoria;
- conta;
- valor;
- status.

Resumo:

- total previsto;
- total recebido;
- total atrasado.

---

# 36. Relatório de Despesas

Mostrar:

- data;
- vencimento;
- pagamento;
- descrição;
- categoria;
- fornecedor;
- conta/cartão;
- valor;
- status.

---

# 37. Relatório de Contas a Pagar

Mostrar:

- vencidas;
- vencendo hoje;
- próximas;
- pagas;
- pendentes.

Ordenação principal:

**data de vencimento**.

---

# 38. Relatório de Contas Pagas

Mostrar:

- data de vencimento;
- data de pagamento;
- descrição;
- categoria;
- fornecedor;
- conta;
- valor.

Permitir análise de pagamentos antecipados ou atrasados.

---

# 39. Relatório de Receitas por Categoria

Mostrar:

- categoria;
- quantidade;
- valor;
- percentual;
- evolução mensal.

---

# 40. Relatório de Despesas por Categoria

Mostrar:

- categoria;
- orçamento;
- realizado;
- comprometido;
- percentual;
- evolução.

---

# 41. Relatório de Fornecedores

Mostrar:

- fornecedor;
- quantidade;
- total;
- pago;
- pendente;
- vencido.

---

# 42. Relatório de Cartões

Mostrar:

- cartão;
- limite;
- utilizado;
- disponível;
- fatura atual;
- próximas faturas;
- parcelas futuras.

---

# 43. Relatório de Parcelamentos

Mostrar:

- descrição;
- valor total;
- parcela atual;
- total de parcelas;
- valor da parcela;
- próximo vencimento;
- total restante.

---

# 44. Relatório de Orçamento

Mostrar:

- categoria;
- limite;
- realizado;
- comprometido;
- restante;
- percentual utilizado.

---

# 45. Relatório de Evolução Mensal

Comparar:

- receitas;
- despesas;
- resultado;
- taxa de economia;
- saldo;
- comprometimento.

Permitir:

- 3 meses;
- 6 meses;
- 12 meses;
- período personalizado.

---

# 46. Relatório de Previsão Financeira

Este deve ser um dos relatórios mais importantes do sistema.

Mostrar mês a mês:

- saldo inicial;
- receitas previstas;
- receitas realizadas;
- despesas pagas;
- contas futuras;
- faturas;
- parcelas;
- resultado;
- saldo projetado;
- indicador de risco.

O usuário deve conseguir responder:

**"Se eu não cadastrar mais nada e mantiver o que já registrei, quanto dinheiro provavelmente terei nos próximos meses?"**

---

# 47. Relatório de Situação Financeira

Resumo executivo:

- saldo disponível;
- compromissos;
- receitas;
- resultado;
- dívidas;
- cartões;
- reserva;
- metas;
- meses de risco.

Deve ser compreensível para usuário sem conhecimento financeiro.

---

# 48. Relatório de Saúde Financeira

Pode apresentar um indicador próprio do aplicativo, mas deve deixar claro que ele é um indicador interno e não uma avaliação oficial.

Pode considerar:

- capacidade de pagar contas;
- saldo projetado;
- regularidade de pagamentos;
- comprometimento da renda;
- utilização do cartão;
- existência de reserva;
- evolução das metas;
- resultado mensal.

---

# 49. Relatório de Auditoria

Para administração e suporte:

- alterações;
- exclusões;
- importações;
- restaurações;
- usuário;
- data;
- origem;
- registro alterado.

Utilizar a tabela `Logs`.

---

# 50. Relatório de Dados Inconsistentes

Deve identificar:

- receita paga sem data de recebimento;
- despesa paga sem data de pagamento;
- despesa sem vencimento;
- receita pendente com data de pagamento;
- parcela sem vencimento;
- parcela duplicada;
- pai de parcelamento contabilizado junto com filhos;
- cartão com limite utilizado inconsistente;
- saldo incompatível;
- categoria inexistente;
- conta inexistente;
- registros de outro usuário.

Esse relatório é especialmente importante para futuras auditorias.

---

# 51. Importação

A importação deve preservar:

- usuário;
- categoria;
- fornecedor;
- conta;
- cartão;
- data;
- vencimento;
- pagamento;
- status;
- valor.

Antes de importar:

- validar;
- identificar duplicidades;
- mostrar prévia;
- permitir cancelar.

Depois:

- informar quantidade importada;
- duplicados;
- erros;
- registros ignorados.

---

# 52. Backup

O sistema deve oferecer:

- backup manual;
- backup automático;
- restauração;
- histórico de backups;
- tamanho;
- data;
- localização;
- validação do arquivo.

---

# 53. Experiência do usuário

O sistema deve priorizar:

- poucos passos;
- textos claros;
- valores fáceis de entender;
- filtros simples;
- telas com resumo antes da lista;
- ações rápidas;
- confirmação de operações destrutivas;
- mensagens explicando o que aconteceu.

O usuário não deve precisar entender contabilidade para utilizar o aplicativo.

---

# 54. Regra de apresentação dos valores

Sempre diferenciar visualmente:

### Realizado

Dinheiro que já entrou ou saiu.

### Previsto

Dinheiro esperado.

### Comprometido

Obrigação já cadastrada.

### Projetado

Resultado calculado considerando o futuro cadastrado.

Não usar somente a palavra "saldo" quando houver risco de confundir saldo real com saldo projetado.

---

# 55. Arquitetura de dados

A estrutura atual possui `Transacoes` e também `Receitas`.

Isso deve ser tratado cuidadosamente.

A regra recomendada é:

### Transacoes

Fonte principal das movimentações financeiras que afetam:

- contas;
- cartões;
- pagamentos;
- recebimentos;
- fluxo financeiro;
- saldo;
- previsão.

### Receitas

Cadastro especializado para gerenciamento de fontes de receita e receitas recorrentes, quando utilizado pela aplicação.

Não permitir que a mesma receita seja contabilizada duas vezes por existir em ambas as estruturas.

Sempre que uma receita cadastrada em `Receitas` gerar uma movimentação financeira, deve existir vínculo lógico entre o cadastro e a movimentação.

---

# 56. Não duplicar receitas

O sistema não pode contar:

`Receitas.valor`

e:

`Transacoes.tipo='receita'`

como duas entradas independentes.

Deve existir uma definição única para o que entra no resultado financeiro.

---

# 57. Não duplicar despesas parceladas

Se existir uma transação pai e suas parcelas:

- pai = agrupador;
- filhos = lançamentos financeiros.

Somente as parcelas devem entrar nos cálculos financeiros.

---

# 58. Previsão não altera saldo real

Este é um princípio obrigatório.

Cadastrar:

Salário futuro R$ 5.000

não pode:

- aumentar `saldo_atual`;
- criar dinheiro real;
- alterar saldo bancário real.

Somente deve alterar:

- saldo projetado;
- fluxo de caixa previsto;
- indicadores de previsão.

---

# 59. Pagamento altera saldo real

Quando uma conta for efetivamente paga:

- `pago = 1`;
- `data_pagamento` = data real;
- saldo da conta é atualizado.

A despesa continua pertencendo à competência do vencimento.

---

# 60. Recebimento altera saldo real

Quando uma receita for efetivamente recebida:

- `pago = 1`;
- `data_pagamento` = data real;
- saldo da conta aumenta.

A receita realizada entra no mês da data de recebimento.

---

# 61. Projeção de longo prazo

O usuário deve poder selecionar:

- 1 mês;
- 3 meses;
- 6 meses;
- 12 meses;
- 24 meses, se houver dados.

A projeção deve considerar:

- contas recorrentes;
- salários previstos;
- receitas recorrentes;
- parcelas;
- faturas;
- contas futuras;
- demais lançamentos cadastrados.

---

# 62. Cenários

Como evolução futura, o sistema poderá oferecer:

### Cenário atual

Tudo que já está cadastrado.

### Cenário conservador

Receitas previstas não recebidas não são consideradas como garantidas.

### Cenário planejado

Inclui receitas previstas pelo usuário.

### Cenário personalizado

Permite adicionar/remover previsões sem alterar os lançamentos reais.

Esses cenários devem ser apresentados como simulações e nunca como saldo bancário real.

---

# 63. Planejamento de compras

Como evolução futura:

O usuário poderá informar:

- compra pretendida;
- valor;
- data;
- número de parcelas.

O sistema deve mostrar:

**"Se você fizer esta compra, como ficará seu saldo projetado?"**

Não deve criar uma transação real até o usuário confirmar.

---

# 64. Planejamento de despesas

Como evolução futura:

Permitir adicionar uma despesa hipotética.

Exemplo:

"Quero viajar em dezembro e gastarei aproximadamente R$ 3.000."

O sistema simula o impacto no saldo futuro.

---

# 65. Princípios financeiros utilizados

A arquitetura funcional do Money deve privilegiar:

1. controle do fluxo de caixa;
2. visão antecipada das obrigações;
3. separação entre realizado e previsto;
4. acompanhamento das contas no vencimento;
5. acompanhamento das receitas quando recebidas;
6. orçamento;
7. comparação entre planejado e realizado;
8. criação de reserva financeira;
9. acompanhamento de dívidas;
10. metas financeiras;
11. capacidade de absorver imprevistos;
12. liberdade de escolha financeira.

Esses princípios são coerentes com materiais do Consumer Financial Protection Bureau (CFPB), que tratam o bem-estar financeiro como segurança e liberdade de escolha no presente e no futuro, incluindo controle das finanças do dia a dia, capacidade de absorver choques, progresso em metas e liberdade de escolha.

---

# 66. Fluxo de caixa como conceito central

O sistema deve considerar o momento em que o dinheiro entra e sai.

Uma visão mensal é importante, mas a visão semanal ou diária pode ser necessária quando houver concentração de contas em determinados períodos.

O saldo projetado deve ser calculado cronologicamente.

Exemplo:

Saldo inicial: R$ 2.000

05/09 salário previsto + R$ 5.000  
06/09 conta - R$ 1.000  
10/09 aluguel - R$ 1.500  
20/09 cartão - R$ 2.000

Saldo projetado após cada evento:

05/09 = R$ 7.000  
06/09 = R$ 6.000  
10/09 = R$ 4.500  
20/09 = R$ 2.500

Isso é mais útil para planejamento do que simplesmente mostrar:

Receitas do mês = R$ 5.000  
Despesas do mês = R$ 4.500.

---

# 67. Regra para saldo projetado

Para uma sequência cronológica de eventos:

`Saldo projetado do evento = saldo projetado anterior + entrada prevista - saída prevista`

O sistema deve poder mostrar essa evolução.

---

# 68. Indicador de risco

Criar indicador simples:

### Seguro

Saldo projetado positivo e sem déficit futuro.

### Atenção

Saldo projetado positivo, mas próximo de zero.

### Crítico

Existe pelo menos um período futuro com saldo projetado negativo.

A classificação deve ser baseada nos números do próprio usuário.

Não deve ser apresentada como aconselhamento financeiro profissional.

---

# 69. Dashboard ideal

A tela principal deve conter, em ordem de prioridade:

## 1. Saldo atual

Dinheiro efetivamente disponível.

## 2. Balanço do mês

Entradas realizadas - despesas pagas.

## 3. Entradas realizadas

Receitas efetivamente recebidas.

## 4. Contas do mês

Contas pagas + pendentes + vencidas.

## 5. Saldo projetado

Quanto deverá restar considerando o que está cadastrado.

## 6. Próximos vencimentos

Lista dos próximos compromissos.

## 7. Próximas receitas

Salários e outras receitas previstas.

## 8. Cartões

Fatura atual e próximas faturas.

## 9. Orçamento

Categorias próximas do limite.

## 10. Metas

Progresso.

---

# 70. Regra para "Balanço do Mês"

O título deve deixar claro se representa:

### Balanço realizado

`Receitas recebidas - Despesas pagas`

ou:

### Balanço projetado

`Receitas previstas/recebidas - Despesas previstas/pagas`

Não misturar os dois valores.

Uma solução recomendada é mostrar ambos:

**Realizado:** R$ X  
**Projetado:** R$ Y

---

# 71. Correção necessária no Dashboard atual

O Dashboard atualmente possui indicadores de visão mensal, mas a navegação para meses futuros não deve retornar zero simplesmente porque as receitas ainda não foram recebidas ou as contas ainda não foram pagas.

Para meses futuros:

### Entradas

Buscar receitas previstas.

### Contas

Buscar contas com vencimento no período.

### Resultado projetado

`Entradas previstas - Contas previstas`

### Saldo projetado

Usar o saldo inicial/projetado acumulado.

O mês futuro deve possuir uma visão própria de previsão.

---

# 72. Critérios de aceitação do Dashboard

Se o usuário cadastrar:

### Salário

R$ 5.000  
05/09/2026  
Pendente

### Aluguel

R$ 1.500  
10/09/2026  
Pendente

### Cartão

R$ 1.000  
15/09/2026  
Pendente

O Dashboard de setembro deve mostrar aproximadamente:

Entradas previstas: R$ 5.000  
Contas previstas: R$ 2.500  
Resultado projetado: R$ 2.500

E não:

Entradas realizadas: R$ 0

sem qualquer informação de previsão.

"Entradas realizadas" pode continuar sendo R$ 0, mas deve existir a seção de entradas previstas.

---

# 73. Critérios de aceitação da tela de Receitas

A tela deve permitir:

1. cadastrar receita;
2. editar;
3. excluir;
4. filtrar;
5. pesquisar;
6. visualizar pendentes;
7. visualizar recebidas;
8. visualizar atrasadas;
9. visualizar recorrentes;
10. marcar como recebida;
11. registrar data real do recebimento;
12. gerar previsões futuras;
13. visualizar resumo;
14. exportar relatório.

---

# 74. Critérios de aceitação do sistema

O Money somente será considerado completo quando o usuário puder responder facilmente:

- Quanto tenho hoje?
- Quanto recebi este mês?
- Quanto ainda vou receber?
- Quanto já paguei?
- Quanto ainda preciso pagar?
- O que está vencido?
- Quanto vou gastar no próximo mês?
- Quanto vai sobrar?
- Em qual mês posso ficar sem dinheiro?
- Quanto estou gastando por categoria?
- Quanto estou comprometido no cartão?
- Quanto ainda tenho de limite?
- Quanto posso gastar sem comprometer meus próximos meses?
- Quanto preciso economizar para atingir uma meta?
- Como minhas finanças evoluíram?
- Quanto tenho de reserva?
- Quais são minhas maiores despesas?
- Quais contas vencem primeiro?

---

# 75. Relatórios resumidos a implementar

Prioridade alta:

1. Dashboard financeiro;
2. Fluxo de caixa realizado;
3. Fluxo de caixa projetado;
4. Receitas;
5. Contas a pagar;
6. Despesas;
7. Despesas por categoria;
8. Receitas por categoria;
9. Cartões;
10. Faturas;
11. Parcelamentos;
12. Orçamento;
13. Evolução mensal;
14. Previsão financeira;
15. Situação financeira.

Prioridade média:

16. Fornecedores;
17. Tags;
18. Metas;
19. Reserva;
20. Dívidas;
21. Auditoria;
22. Dados inconsistentes.

---

# 76. Regras de implementação para o Codex

Antes de alterar código:

1. localizar todos os cálculos financeiros;
2. localizar todas as consultas SQL;
3. localizar todas as Views;
4. localizar todos os ViewModels;
5. localizar todos os serviços;
6. localizar todas as páginas relacionadas;
7. mapear a origem dos dados;
8. identificar duplicidade entre `Receitas` e `Transacoes`.

Não implementar apenas a tela.

A funcionalidade deve atravessar:

`Banco → Repository/DatabaseService → Serviço → ViewModel → Página → Dashboard → Relatório`

---

# 77. Regra de não regressão

Nenhuma nova funcionalidade pode quebrar:

- login;
- usuários;
- contas;
- cartões;
- categorias;
- fornecedores;
- transações;
- parcelamentos;
- importação;
- backup;
- restauração;
- metas;
- orçamentos;
- notificações;
- relatórios existentes.

---

# 78. Auditoria final

Após implementar qualquer alteração financeira, procurar no projeto por:

- `t.data`;
- `data`;
- `data_vencimento`;
- `data_pagamento`;
- `pago`;
- `tipo='receita'`;
- `tipo='despesa'`;
- `strftime`;
- `SUM(valor)`;
- `saldo`;
- `saldo_atual`;
- `limite_utilizado`.

Cada ocorrência deve ser revisada para verificar se utiliza a data financeira correta.

---

# 79. Testes mínimos

## Receita futura

Lançamento: 01/09  
Recebimento: futuro  
Pago: 0

Esperado:

- realizado = 0;
- previsto = valor;
- saldo real não muda;
- saldo projetado aumenta.

## Receita recebida

Data pagamento: 05/09  
Pago: 1

Esperado:

- realizado setembro = valor;
- saldo real aumenta.

## Conta futura

Vencimento: 10/09  
Pago: 0

Esperado:

- contas setembro = valor;
- contas pagas = 0;
- previsão = valor.

## Conta paga

Vencimento: 10/09  
Pagamento: 08/09  
Pago: 1

Esperado:

- despesa de setembro;
- despesa paga setembro;
- saldo real reduz.

## Conta vencida

Vencimento anterior a hoje  
Pago: 0

Esperado:

- status vencida;
- aparece em contas vencidas;
- entra na obrigação financeira.

---

# 80. Princípio final

O Money não deve ser apenas um "livro de anotações".

Ele deve funcionar como:

**Registro + Controle + Orçamento + Fluxo de Caixa + Previsão + Planejamento.**

O sistema deve ajudar o usuário a enxergar o futuro financeiro com base no que ele já cadastrou, sem confundir previsão com dinheiro efetivamente disponível.

A informação mais importante para o usuário não é apenas:

**"Quanto eu tenho?"**

Mas também:

**"Quanto eu vou ter depois de pagar tudo o que já sei que preciso pagar?"**

E:

**"Quanto vai sobrar se eu receber as receitas que estou prevendo?"**

Essa visão deve orientar o Dashboard, as telas, os filtros, os relatórios e a evolução futura do Money.

---

## Referências externas utilizadas para fundamentação conceitual

A definição de bem-estar financeiro adotada neste documento foi inspirada em pesquisas e materiais públicos do **Consumer Financial Protection Bureau (CFPB)** sobre controle das finanças no presente, capacidade de absorver choques, metas e segurança financeira futura.

Também foi considerada a abordagem de **cash-flow budget**, que acompanha o momento em que receitas e despesas entram e saem do caixa, permitindo identificar antecipadamente períodos em que o saldo pode ficar insuficiente.

O documento não constitui aconselhamento financeiro individual. As regras de cálculo e os indicadores aqui definidos são especificações funcionais do aplicativo.

---

## Histórico

### 1.1 — 17/08/2026

Implementação inicial do motor centralizado de previsão:

- o saldo real é sempre a soma de `Contas.saldo_atual` das contas ativas;
- receitas pendentes são eventos projetados pela `data` prevista e não alteram o saldo real;
- receitas realizadas usam `data_pagamento` e exigem `pago = 1`;
- despesas, pagas ou pendentes, pertencem à competência de `COALESCE(data_vencimento,data)`;
- o saldo projetado parte do saldo real atual e aplica cronologicamente apenas eventos futuros pendentes;
- meses posteriores recebem como saldo inicial o saldo projetado final do mês anterior;
- transações pais que possuem parcelas-filhas são apenas agrupadores e não entram no fluxo;
- faturas são apresentações agregadas das transações do cartão e não são novamente subtraídas da projeção.

### 1.2 — 17/08/2026

Refinamento específico da apresentação do Dashboard:

- o card mensal nunca incorpora `Contas.saldo_atual`;
- resultado realizado mensal = receitas recebidas do mês menos despesas pagas do mês;
- resultado projetado mensal = receitas pendentes previstas no mês menos despesas pendentes com vencimento no mês;
- saldo de conta continua sendo atualizado por recebimentos e pagamentos, mas permanece restrito às telas de Bancos/Contas;
- a tela de lançamento geral é exclusiva para despesas; salários e recebimentos usam fluxo próprio.

### 1.0 — 17/08/2026

Primeira versão consolidada das regras de negócio, planejamento, previsão financeira, Dashboard, tela de Receitas e catálogo de relatórios.
