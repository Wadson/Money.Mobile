import re,json
from pathlib import Path
root=Path.cwd(); out=root/'outputs/category-audit-20260911'
verification=json.loads((out/'database-verification.json').read_text(encoding='utf-8'))
s=(root/'Money/Models/CategoryCatalog.cs').read_text(encoding='utf-8-sig')
lines=[]
for line in s.splitlines():
 m=re.search(r'new\("([^"]+)", "([^"]+)", "(despesa|receita)",',line)
 if m: lines.append(f'\n**{m[1]}** ({m[3]}) — `{m[2]}`\n');continue
 m=re.search(r'new\("([^"]+)", "([^"]+)", ("([^"]+)"|null)\)',line)
 if m:lines.append(f'- {m[1]} — `{m[2]}`'+(f' — {m[4]}' if m[4] else ''))
report='''# Entrega de Categorias e Subcategorias — 11/09/2026

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
'''
report+='\n'.join(lines)+'\n'
(root/'AUDITORIA-CATEGORIAS-2026-09-11.md').write_text(report,encoding='utf-8')
