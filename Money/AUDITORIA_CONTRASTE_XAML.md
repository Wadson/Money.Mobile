# Auditoria de contraste e cores em XAML

Data: 31/07/2026

## Resultado geral

- 25 arquivos XAML analisados (páginas, shell, recursos e bootstrap Windows).
- 70 controles de entrada/seleção inspecionados.
- Todos os controles visíveis `Entry`, `Editor`, `Picker`, `DatePicker` e `TimePicker` dentro de cards estão delimitados por `Border`.
- Todos os controles visíveis `RadioButton`, `Switch` e `CheckBox` possuem superfície interna delimitada.
- Dois `Picker` auxiliares de `TransactionFormPage.xaml` permanecem sem contêiner porque são intencionalmente invisíveis (`IsVisible="False"`) e servem apenas como estado interno.
- A estrutura inválida de `VisualStateManager.VisualStateGroups` foi corrigida em três estilos.
- Build Windows: concluído com 0 erros e 0 avisos.

## Tokens e estilos globais

`Resources/Styles/Styles.xaml`:

- `TextSecondary`, `TextMuted`, `DisabledText` e `PlaceholderText` foram escurecidos.
- `InputBackground`, `InputBorder` e `Divider` receberam maior separação visual.
- `Success`, `Danger` e `Warning` foram ajustados para melhor contraste sobre fundos claros.
- `InputContainer` fornece fundo, contorno e cantos arredondados aos campos.
- `SelectionControlContainer` foi criado para `RadioButton`, `Switch` e `CheckBox`.
- O estilo global de `RadioButton` agora fornece texto, fundo e contorno legíveis no estado desligado.
- `MetricCard` e `ContentCard` mantêm contorno e sombra de separação.

## Arquivos analisados

| Arquivo | Resultado da auditoria |
|---|---|
| `AccountManagementPage.xaml` | Campos e seletores já delimitados; seletor de cores com destaque persistente. |
| `AccountsPayablePage.xaml` | `CheckBox` recebeu superfície interna; cards, badges e ações já usam cores semânticas. |
| `App.xaml` | Apenas mesclagem de recursos; sem controles visuais auditáveis. |
| `AppShell.xaml` | Apenas estrutura do Shell; sem cards ou entradas. |
| `BudgetManagementPage.xaml` | `Picker`, `Entry` e `Editor` do formulário receberam `InputContainer`. |
| `CardManagementPage.xaml` | Cinco `Entry` do formulário receberam `InputContainer`. |
| `CategoryManagementPage.xaml` | Cinco campos já estavam dentro de `Border`; títulos e ações possuem contraste adequado após os tokens globais. |
| `CreditCardAnalysisPage.xaml` | `PaymentAccountPicker` recebeu `InputContainer`; cards e ação de recálculo já possuem separação. |
| `DataOperationsPage.xaml` | Sem entradas ou seleção; cards e textos já usam estilos contrastantes. |
| `IncomeListPage.xaml` | `PeriodPicker` recebeu `InputContainer`; estados vazios beneficiados pelo novo `TextMuted`. |
| `LoginPage.xaml` | Dois `Entry` receberam `InputContainer`; `CheckBox` e rótulo receberam superfície delimitada. |
| `MainPage.xaml` | Sem entradas/seletores; 19 borders/cards verificados, com textos secundários corrigidos globalmente. |
| `MonthlyOverviewPage.xaml` | Sem entradas/seletores; cards e subtítulos verificados. |
| `MorePage.xaml` | Sem entradas/seletores; 24 opções delimitadas e ícones beneficiados pelo novo `TextMuted`. |
| `OptionSelectionPage.xaml` | `RadioButton` recebeu contêiner interno e estilo desligado contrastante. |
| `PlanningPage.xaml` | Quinze campos já estavam corretamente dentro de `Border`; nenhuma correção estrutural necessária. |
| `Platforms/Windows/App.xaml` | Bootstrap da plataforma; sem controles visuais auditáveis. |
| `ProfilePage.xaml` | Cinco campos receberam `InputContainer`. |
| `RecoveryPage.xaml` | Cinco campos receberam `InputContainer`; textos auxiliares beneficiados pelo novo contraste. |
| `RegisterPage.xaml` | Sete campos já estavam delimitados; aceite de termos recebeu superfície para `CheckBox`. |
| `Resources/Styles/Colors.xaml` | Paleta de compatibilidade inspecionada; os tokens ativos são sobrescritos pelo dicionário `Styles.xaml`. |
| `Resources/Styles/Styles.xaml` | Corrigido e reforçado conforme a seção de tokens globais. |
| `SettingsPage.xaml` | Cinco campos e dois grupos de `Switch` receberam contêineres delimitados. |
| `TransactionEditPage.xaml` | Sete campos já delimitados; grupo do `Switch` recebeu superfície interna. |
| `TransactionFormPage.xaml` | Campos visíveis já delimitados; quatro grupos de `Switch` receberam superfície interna. |

## Arquivos completos alterados

Os arquivos abaixo são os próprios fontes completos e compiláveis do projeto:

- `Resources/Styles/Styles.xaml`
- `AccountManagementPage.xaml`
- `AccountManagementPage.xaml.cs`
- `AccountsPayablePage.xaml`
- `BudgetManagementPage.xaml`
- `CardManagementPage.xaml`
- `CreditCardAnalysisPage.xaml`
- `IncomeListPage.xaml`
- `LoginPage.xaml`
- `OptionSelectionPage.xaml`
- `ProfilePage.xaml`
- `RecoveryPage.xaml`
- `RegisterPage.xaml`
- `SettingsPage.xaml`
- `TransactionEditPage.xaml`
- `TransactionFormPage.xaml`

