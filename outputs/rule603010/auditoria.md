# Auditoria e implementação da análise 60-30-10

Escopo: complemento solicitado em 11/09/2026 ao trabalho de categorias/subcategorias. As alterações anteriores do repositório foram preservadas.

## Constatações

- .NET MAUI 10; TFMs reais: `net10.0-android` e `net10.0-windows10.0.19041.0`.
- Existia `CategoryPlanningPage`, acessível dentro dos cadastros, com um resumo textual e edição de pilares/limites. Faltava uma análise independente no menu de análises.
- `CategoriaPilar` representa categoria principal, `Categorias` representa subcategoria e `CategoriaPilarOrcamentario` associa usuário/subcategoria ao pilar. A estrutura existente atende ao módulo.
- A consulta atribuía lazer silenciosamente às subcategorias sem mapeamento. Agora elas aparecem como sem classificação.
- A edição podia acessar índice -1 quando não houvesse pilar válido. Agora exige seleção e exibe o estado sem classificação com segurança.

## Implementação

- `Money/Views/ImportsAndReports/BudgetRuleAnalysisPage.cs`: tela mensal em Mais opções → Análises e relatórios → Análise 60-30-10. Mostra receitas recebidas, metas, valores, percentual sobre receita, progresso, diferenças, detalhamento categoria/subcategoria e valores em aberto. Trata mês vazio, ausência de receita, falha de carregamento e respostas atrasadas ao trocar mês.
- `Money/Models/BudgetRuleAnalysis.cs`: cálculo compartilhado pela nova análise e pelo planejamento. Agrupa essenciais em 60%, lazer em 30% e reserva/investimentos em 10%; não converte transferências em despesas.
- `Money/Services/DatabaseService.Categories.cs`: `GetBudgetPillarsAsync` conserva ausência de mapeamento. `SetBudgetPillarAsync` existente continua persistindo a seleção e validando usuário/subcategoria/pilar.
- `Money/Views/Management/CategoryPlanningPage.cs`: cálculo compartilhado, total sem classificação e validação da seleção.
- `Money/Views/SettingsAndData/MorePage.xaml` e `.xaml.cs`: entrada direta no menu, com ícone Material PieChart.
- `tests/Money.Audit/Program.cs`: três novos testes de cálculo, mês vazio, falta de mapeamento, persistência e isolamento.

## Regras de dados

Receitas realizadas são filtradas por pagamento e data de pagamento. Despesas usam vencimento (data como fallback), incluindo abertas. A consulta existente exclui o lançamento pai quando existem parcelas filhas. O detalhamento conserva os IDs das subcategorias e os nomes das duas entidades. Categorias inativas com histórico continuam contribuindo para os totais. Reserva e investimentos são agrupados apenas para a meta dos 10%; seus quatro códigos oficiais continuam preservados no banco.

Não foi necessária migration: schema suportado 29 → 29, sem mudança em tabelas, índices, triggers ou catálogo. Não se alterou banco de produção. Os fluxos de transações, contas a pagar, importação, dashboard, PDF, backup e cadastros permanecem cobertos pela suíte de regressão existente; não foram reimplementados neste complemento. Catálogo e ícones continuam definidos em `Money/Models/CategoryCatalog.cs`.

## Validação

Baseline: 87 verificações aprovadas. Após a implementação: **90 aprovadas, zero falhas**, executadas com `dotnet run --project tests/Money.Audit -c Release`. O projeto de auditoria é um executável próprio; `dotnet test` não substitui sua execução.

Novos casos verificam receita não recebida excluída, despesa aberta incluída, excesso sobre meta, agrupamento reserva/investimentos, transferências excluídas, mês vazio sem divisão por zero, classificação ausente, gravação do pilar e rejeição de alteração por outro usuário. SQLite nas bases de teste: `integrity_check=ok` e `foreign_key_check` sem linhas.

Build Windows Release: sem erros; 40 avisos preexistentes de nulabilidade e APIs obsoletas. A compilação Android inicialmente encontrou intermediários inválidos de ícones na pasta padrão; a publicação em `.codex-build/rule603010` resolveu esse problema. Houve também restrição do sandbox ao processo auxiliar do linker, resolvida executando a publicação autorizada fora dele. O restore emitiu NU1900 porque o serviço de consulta de vulnerabilidades estava indisponível no proxy local; os pacotes necessários estavam disponíveis.

APK publicado em `outputs/rule603010/MoneyPro-1.0.5-603010.apk` (versão existente 1.0.5, código 10). Instalação como atualização com `adb install -r` no moto g86 5G conectado, sem limpeza dos dados.

Teste no aparelho: login, Menu → Cadastros básicos → Mais opções → Análise 60-30-10; rolagem pelos três grupos e pelos valores sem classificação; abertura do seletor de mês; troca para outubro sem receita recebida; acesso à configuração e ao seletor com os quatro pilares; cancelamento da edição sem alterar dados financeiros. O teste identificou e corrigiu a perda do mês selecionado ao abrir a configuração. Evidências em `analysis-top.png`, `analysis-bottom.png` e arquivos XML desta pasta. Não se reproduzem credenciais no relatório.
