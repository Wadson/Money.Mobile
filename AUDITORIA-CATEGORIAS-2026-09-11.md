# Entrega de Categorias e Subcategorias — 11/09/2026

## Ambiente e diagnóstico

Solução existente `Money.slnx`, projeto `Money/Money.csproj`, SDK 10.0.401, .NET MAUI 10. TFMs reais: `net10.0-android` e `net10.0-windows10.0.19041.0`. Microsoft.Data.Sqlite 10.0.1 e MauiIcons.Material 6.0.0. Versão do aplicativo preservada: 1.0.5 (10).

O código legado tratava Categorias como lista plana, expunha CategoryItem/ParentId, escolhia a primeira categoria em importações e não diferenciava agrupamento principal. O banco fornecido em Data/moneypro.db já tinha o modelo estrutural da versão 28, incluindo os reparos 26/27. A integração interrompida continha as novas APIs e telas, mas faltavam conexões funcionais e havia erros XAML/C#.

As alterações anteriores do usuário foram preservadas. Nenhum reset/revert foi executado. Nenhum AGENTS.md foi encontrado. Este relatório substitui o relatório parcial anterior.

## Migração e preservação

Migration incremental `MigrateCategories29Async`, em `DatabaseService.CategoryMigration.cs`: user_version **28 → 29** no banco real. A inicialização faz backup SQLite antes de migrar. Também foi preservado `outputs/category-audit-20260911/moneypro-before-v29.db`.

A migração reconstrói CategoriaPilar e Categorias preservando IDs, acrescenta constraints normalizadas, índices e guards de usuário/tipo/estado, garante Orcamentos e CategoriaPilarOrcamentario, cria CategoryCatalogRepair29 e mantém as tabelas auxiliares anteriores. Em bancos legados cria CategoryLegacyMobile29. Remove temporariamente e recria views e triggers na transação para acomodar dependências de outras tabelas. A origem dos eventos futuros passa a Money.Mobile/CRUD. Logs históricos não são reescritos.

CategoriaPilar representa categoria principal; Categorias representa subcategoria com FK id_categoria_pilar obrigatória; CategoriaPilarOrcamentario permanece o mapeamento dos pilares. Não há id_categoria_pai no schema final. As rotinas antigas permanecem apenas para compatibilidade/inicialização de bancos legados, sem uso nos novos fluxos.

A comparação SQL EXCEPT confirmou preservação integral de 306 transações, 7 movimentos de contas, 3 contas, IDs das subcategorias, logs anteriores, mapeamentos orçamentários e tabelas CategorySeed26, CategoryLegacy26, CategoryMappingLegacy26, CategoryMigration26Log e CategoryCatalogRepair27. O banco fornecido tinha zero orçamentos; o CRUD de orçamento foi testado em banco temporário.

`PRAGMA integrity_check`: **ok**. `PRAGMA foreign_key_check`: **zero linhas**. Evidência detalhada: `outputs/category-audit-20260911/database-verification.json`.

## Models, serviços e interface

- MainCategoryItem, SubcategoryItem e ICategoryVisual distinguem as entidades. CategoryCatalog centraliza catálogo e pilares. CategoryItem ficou somente como adaptador legado.
- APIs GetMainCategories/GetMainCategory/AddMainCategory/UpdateMainCategory/DeactivateMainCategory/DeleteMainCategory e equivalentes Subcategory, além de GetSubcategoryWithMainCategory. Todas usam o usuário explicitamente configurado; sem sessão, operações protegidas falham.
- Nomes são normalizados sem distinção de acento/caixa. Duplicata principal é impedida por usuário/tipo; duplicata de subcategoria é impedida dentro da principal. Mesmo nome em principais diferentes é permitido. Tipo e titularidade são validados.
- Categorias e Subcategorias possuem entradas distintas no menu, páginas distintas, pesquisa, filtros de tipo/status, CRUD, ícones e estados. Exclusão com vínculos é impedida; desativação mantém o histórico.
- CategoryVisualResolver e CategoryIconCatalog centralizam os visuais. IconSelectionPage permite escolha visual. Cor vazia de subcategoria herda a principal; fallback determinístico. Ícones Material personalizados válidos são mantidos. Identificadores legados dos itens padrão são corrigidos.
- Transações e receitas selecionam Categoria e depois Subcategoria; mudança da principal limpa a seleção dependente. O ID persistido é o da subcategoria. Edição carrega a classificação histórica; novos lançamentos não aceitam entidades inativas.
- CategoryFilterPicker permite categoria inteira ou subcategoria nos filtros de contas a pagar e relatórios, por IDs. O teste Alimentação totaliza 450 e Supermercado 300.
- CSV de contas a pagar aceita Categoria/Subcategoria e o formato legado inequívoco, com indicação na tela. Correspondência ambígua é rejeitada. O modelo CSV foi atualizado. OFX usa explicitamente Outros/Outras Despesas ou Receitas/Outras Receitas e registra a classificação automática. Importação de cartão usa Outras Despesas, sem escolher o primeiro registro.
- Dashboard agrupa por categoria principal e permite detalhar subcategorias. Relatórios têm campos MainCategory/Subcategory, agrupam PDF por principal e exibem a subcategoria nos detalhes. Transferências ficam fora do agrupamento de despesas.
- Orçamento continua por subcategoria. CategoryPlanningPage, acessível nos cadastros, permite editar limite mensal e pilar, mostra a principal e calcula totais 60/30/10 sem duplicar parcelas-pai. Reserva e investimentos partilham os 10%. Nenhuma transferência foi convertida em despesa.
- Backup/restore aceita bancos até versão 29, executa inicialização/migração e preserva os dados. Bancos futuros ou inválidos são rejeitados.

## Arquivos de implementação

C# criados/refatorados para esta integração (incluindo o trabalho retomado):

- Money/Models/CategoryCatalog.cs e FinanceModels.cs.
- Money/Services/DatabaseService.cs, DatabaseService.Categories.cs, DatabaseService.CategoryMigration.cs, AuthService.cs, SqliteBackupStore.cs, ReportPdfService.cs e CsvTemplateService.cs.
- Money/Helpers/CategoryVisualResolver.cs, CategoryIconCatalog.cs e CategoryFilterPicker.cs.
- Money/Views/Management/CategoryManagementBase.cs, CategoryManagementPage.xaml.cs, SubcategoryManagementPage.xaml.cs, IconSelectionPage.cs e CategoryPlanningPage.cs.
- Money/Views/Transactions/TransactionFormPage.xaml.cs, TransactionEditPage.xaml.cs, IncomeFormPage.xaml.cs, AccountsPayablePage.xaml.cs e PaymentReversalPage.xaml.cs.
- Money/Views/ImportsAndReports/ReportPage.xaml.cs e PayablesCsvImportPage.xaml.cs.
- Money/Views/Dashboard/MainPage.xaml.cs; Money/Views/SettingsAndData/MorePage.xaml.cs e DataOperationsPage.xaml.cs.
- Money/Platforms/Android/ProfessionalReportPdfRenderer.cs; testes CategoryAudit.cs, Program.cs e BackupAudit.cs.

XAML da integração: CategoryManagementPage.xaml, SubcategoryManagementPage.xaml, TransactionFormPage.xaml, TransactionEditPage.xaml, IncomeFormPage.xaml, MorePage.xaml e PayablesCsvImportPage.xaml. Alguns arquivos já estavam alterados ao retomar; o diff total do repositório também contém trabalho anterior fora deste módulo.

## Testes e builds

**89 testes aprovados, zero falhas**, executados pelo programa Money.Audit em Release, usando bancos temporários e cópia do banco v28. CategoryAudit acrescenta cobertura de catálogo/ícones, CRUD, normalização, titularidade, tipos, desativação, exclusão, IDs de transações, filtros, dashboard, importação ambígua, orçamento, pilares, idempotência e integridade. BackupAudit cobre restauração real, WAL e conteúdo das transações.

`dotnet test` retornou zero; o projeto é um executável de auditoria, não uma suíte descoberta pelo VSTest. A contagem vem da execução efetiva do executável. Logs: tests-final.log e dotnet-test.log.

Windows Release publicado em pasta isolada para evitar bloqueios em obj mantidos por processos existentes. Android Release gera APK, com RunAOTCompilation=false informado na linha de comando, sem alterar o TFM nem inventar certificados. As configurações padrão de assinatura existentes foram utilizadas.

Persistem avisos de APIs MAUI obsoletas/nulabilidade e NU1900 porque o serviço NuGet de auditoria de vulnerabilidades não ficou acessível. Não houve downgrade ou supressão desses avisos.

## Artefatos e limitações

Saída exclusiva deste projeto: `D:/Projetos/Money.Mobile/outputs/category-audit-20260911`.

- APK instalável: `android/com.companyname.money-Signed.apk`.
- Publicação Windows: `windows/Money.exe` com suas dependências na mesma pasta.
- Banco migrado: `D:/Projetos/Money.Mobile/Data/moneypro.db`.
- Backup v28: `moneypro-before-v29.db`, além do backup automático junto ao banco.
- Logs, verificação JSON e scripts de reprodução em outputs/category-audit-20260911 e scripts.

Não foi instalado em aparelho/emulador: `adb devices -l` não encontrou dispositivos. A conferência visual interativa em Android/Windows ainda não foi realizada. Compilar XAML e executar testes de banco não substitui esse aceite visual. A assinatura existente de desenvolvimento não equivale a uma chave de distribuição de loja.

## Catálogo padrão e ícones definitivos

O catálogo abaixo descreve os seeds. Registros históricos/customizados adicionais e ícones Material personalizados válidos são preservados.

**Alimentação** (despesa) — `Restaurant`

- Lanches — `Fastfood` — VARIAVEIS_LAZER
- Restaurantes — `Restaurant` — VARIAVEIS_LAZER
- Supermercado — `ShoppingCart` — FIXAS_ESSENCIAIS

**Animais de Estimação** (despesa) — `Pets`

- Pet Shop — `Pets` — FIXAS_ESSENCIAIS
- Veterinário — `MedicalServices` — FIXAS_ESSENCIAIS

**Casa/Moradia** (despesa) — `House`

- Aluguel — `Key` — FIXAS_ESSENCIAIS
- Celular/Linha — `Smartphone` — FIXAS_ESSENCIAIS
- Condomínio — `Apartment` — FIXAS_ESSENCIAIS
- Energia Elétrica — `Bolt` — FIXAS_ESSENCIAIS
- Gás — `LocalFireDepartment` — FIXAS_ESSENCIAIS
- Internet — `Wifi` — FIXAS_ESSENCIAIS
- Lavanderia — `LocalLaundryService` — FIXAS_ESSENCIAIS
- Manutenção Residencial — `Build` — FIXAS_ESSENCIAIS
- Reforma/Reparo — `HomeRepairService` — FIXAS_ESSENCIAIS
- Serviços Domésticos — `CleaningServices` — FIXAS_ESSENCIAIS
- Telefone — `Phone` — FIXAS_ESSENCIAIS
- Água — `WaterDrop` — FIXAS_ESSENCIAIS

**Compras** (despesa) — `ShoppingBag`

- Acessórios — `Watch` — VARIAVEIS_LAZER
- Calçados — `Checkroom` — VARIAVEIS_LAZER
- Decoração — `Chair` — VARIAVEIS_LAZER
- Eletrônicos — `Devices` — VARIAVEIS_LAZER
- Informática — `Computer` — VARIAVEIS_LAZER
- Móveis — `Chair` — VARIAVEIS_LAZER
- Roupas — `Checkroom` — VARIAVEIS_LAZER

**Educação** (despesa) — `School`

- Cursos — `MenuBook` — VARIAVEIS_LAZER
- Livros — `MenuBook` — VARIAVEIS_LAZER
- Material Escolar — `Edit` — FIXAS_ESSENCIAIS
- Mensalidade Escolar — `School` — FIXAS_ESSENCIAIS

**Financeiro** (despesa) — `AccountBalanceWallet`

- Consórcios — `Groups` — FIXAS_ESSENCIAIS
- Impostos — `ReceiptLong` — FIXAS_ESSENCIAIS
- Investimentos — `TrendingUp` — INVESTIMENTOS
- Multas — `Gavel` — VARIAVEIS_LAZER
- Reserva Financeira — `Savings` — RESERVA_EMERGENCIA
- Tarifas Bancárias — `AccountBalance` — FIXAS_ESSENCIAIS
- Taxas — `RequestQuote` — VARIAVEIS_LAZER

**Lazer** (despesa) — `Movie`

- Cinema — `Movie` — VARIAVEIS_LAZER
- Jogos — `SportsEsports` — VARIAVEIS_LAZER
- Passeios — `Public` — VARIAVEIS_LAZER
- Streaming — `LiveTv` — VARIAVEIS_LAZER
- Viagens — `Flight` — VARIAVEIS_LAZER

**Saúde** (despesa) — `MedicalServices`

- Academia — `FitnessCenter` — VARIAVEIS_LAZER
- Consultas Médicas — `MedicalServices` — FIXAS_ESSENCIAIS
- Exames — `Biotech` — FIXAS_ESSENCIAIS
- Farmácia — `LocalPharmacy` — FIXAS_ESSENCIAIS
- Plano de Saúde — `HealthAndSafety` — FIXAS_ESSENCIAIS

**Transporte** (despesa) — `DirectionsCar`

- Combustível — `LocalGasStation` — FIXAS_ESSENCIAIS
- Manutenção Veículo — `Build` — FIXAS_ESSENCIAIS
- Seguro Veículo — `Shield` — FIXAS_ESSENCIAIS
- Transporte Público — `DirectionsBus` — FIXAS_ESSENCIAIS
- Uber / Táxi — `LocalTaxi` — VARIAVEIS_LAZER

**Outros** (despesa) — `Category`

- Doações — `VolunteerActivism` — VARIAVEIS_LAZER
- Presentes — `CardGiftcard` — VARIAVEIS_LAZER
- Outras Despesas — `MoreHoriz` — VARIAVEIS_LAZER
- Dízimos — `Church` — VARIAVEIS_LAZER

**Receitas** (receita) — `Payments`

- Salário — `Payments`
- Adiantamento Salarial — `Payments`
- Horas Extras — `Payments`
- Comissões — `Payments`
- Bônus — `Payments`
- Décimo Terceiro — `Payments`
- Férias — `Payments`
- Freelance — `Payments`
- Trabalho Autônomo — `Payments`
- Venda de Produtos — `Payments`
- Venda de Serviços — `Payments`
- Aluguel Recebido — `Payments`
- Dividendos — `Payments`
- Rendimentos de Investimentos — `Payments`
- Juros Recebidos — `Payments`
- Cashback — `Payments`
- Prêmios — `Payments`
- Bonificações — `Payments`
- Restituição de Imposto — `Payments`
- Reembolso — `Payments`
- Mesada — `Payments`
- Ajuda Familiar — `Payments`
- Doações Recebidas — `Payments`
- Herança — `Payments`
- Indenizações — `Payments`
- Resgate de Investimentos — `Payments`
- Venda de Bens — `Payments`
- Outras Receitas — `Payments`
