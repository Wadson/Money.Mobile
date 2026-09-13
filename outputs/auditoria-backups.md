# Auditoria de backups e previsão financeira — Money Pro 1.0.5

## Diagnóstico

O arquivo `moneyproNovo.db` contém os 250 lançamentos. Há dois usuários ativos chamados Wadson: o cadastro de ID 2 está vazio e o de ID 3 possui os 250 lançamentos. O login anterior consultava nome ou e-mail e aceitava o primeiro resultado. Isso permitia entrar no cadastro vazio ao usar o nome repetido, dando a impressão de que o backup havia perdido os dados.

Também havia um problema independente de consistência: criação e restauração copiavam diretamente o arquivo principal do SQLite. Fazer checkpoint antes da cópia não impede novas gravações nem elimina conexões mantidas no pool. A restauração podia substituir o arquivo ainda aberto, sem validar o banco efetivamente usado pelo aplicativo depois da troca.

Não foi possível inspecionar o estado interno do aparelho para afirmar que o segundo problema também ocorreu nele. A duplicidade e a distribuição dos lançamentos foram confirmadas no banco fornecido.

## Correções

- Backup pela API de snapshots do SQLite, incluindo dados já confirmados no WAL.
- Validação de integridade, tabelas e colunas essenciais, relacionamentos, usuário ativo e versão suportada.
- Preparação e migração de uma cópia isolada antes da restauração. O arquivo de origem não é modificado.
- Cópia de segurança consistente do banco atual antes da substituição.
- Restauração pela API do SQLite, em vez de sobrescrever o arquivo aberto. Conferência da contagem de lançamentos e dos usuários no destino; tentativa de retorno à cópia anterior em caso de falha.
- Nomes únicos para arquivos temporários e backups; serialização das operações de backup/restauração.
- Encerramento da sessão após restaurar, abertura da tela de login e criação de um painel novo após autenticar.
- Confirmação da restauração exibe total de lançamentos e a distribuição por usuário/e-mail. Não é mais necessário reiniciar manualmente.
- Login por nome repetido solicita o e-mail. Login por e-mail identifica o usuário correto.
- Ajustes aplicados tanto à tela Backup quanto ao fluxo de restauração em Operações de dados.
- No Android, o salvamento só informa sucesso depois de descarregar e fechar o documento. Compartilhamento informa que o arquivo foi preparado, sem prometer entrega pelo aplicativo externo.

## Validação

52 testes aprovados, zero falhas na execução final. Além dos testes financeiros existentes, foram verificados:

- Snapshot com dados no WAL e uma conexão aberta; alterações posteriores não mudam o backup.
- Restauração com conexão já aberta, verificação da cópia anterior e persistência após reabrir.
- Rejeição de arquivo inválido, banco incompleto, versão futura e relacionamentos quebrados.
- Bloqueio de login ambíguo, identificação por e-mail e limpeza do usuário financeiro ao sair.
- Restauração de uma cópia do `moneyproNovo.db`: 250 lançamentos preservados, comparação de todas as colunas das transações e consulta de Contas a Pagar pelo usuário proprietário.

Os testes usam diretórios temporários. Nenhuma conta, transferência ou tabela foi adicionada ao `moneyproNovo.db` nesta correção.

Não houve teste de interação em aparelho Android ou emulador. A escolha de arquivo, o provedor de documentos e a interface de login devem ser conferidos no dispositivo.

## Pendência identificada

A configuração “Backup automático” e sua frequência são persistidas, mas não há agendador nem rotina automática de execução no código atual. Essa opção não oferece proteção automática. A correção desta versão cobre criação manual, exportação, restauração e acesso aos dados restaurados; implementar execução automática continua pendente.

## Alteração posterior no arquivo fornecido

Durante a validação final, o arquivo original mudou: o usuário de ID 3 deixou de existir, mas os 250 lançamentos e seus cadastros relacionados continuaram apontando para ele. O hash mudou de `56E13755F1EC9FC517145416CA7F28758B7B2BF6B31280ADE1041AE63FBF4F24` para `65E5F25743A2B45CC0E6C69F0CE1F159B32FD6AFF8820A4FE985B22CA2C3E9AB`. A validação rejeitou corretamente essa versão inconsistente.

O original foi preservado. Foi criada `outputs/moneyproNovo-recuperado.db`, restaurando somente o usuário ausente a partir de `MigrationBackups/merge-legacy-20260904-211730-759616/target-before.db`. Nenhuma tabela foi criada e nenhum lançamento foi transferido para outro usuário. A cópia recuperada passou pelas verificações de integridade, relacionamentos e pelos testes de restauração/reabertura.

## Card Previsão financeira

- Removidos o campo “Resultado projetado” e a repetição do mesmo valor como saldo projetado.
- Quatro valores: Receitas do mês, Despesas do mês, A receber e A pagar.
- Totais incluem recebidos/pagos e pendentes, agrupados pelo vencimento do mês escolhido. A receber e A pagar mostram apenas pendências.
- Corrigida a separação entre receitas realizadas e previstas no serviço de cálculo.
- A consulta e o texto explicativo consideram exclusivamente o mês selecionado, inclusive passado e futuro distante. Removida a avaliação dos próximos 12/24 meses da tela principal.
- Mês sem dados tem estado explícito. Falhas de consulta não reutilizam a previsão de outro mês.
- Consultas atrasadas não substituem o resultado de uma seleção de mês mais recente.
- Testes específicos cobrem competência por vencimento, mês adjacente negativo, mês vazio, futuro distante, atualização após pagamento/recebimento e exclusão de duplicidade de parcelas pai.

## Uso

Instale a versão 1.0.5 e restaure `outputs/moneyproNovo-recuperado.db`. A confirmação mostrará qual e-mail possui os 250 lançamentos. Entre usando esse e-mail e a senha desse cadastro. Não use apenas “Wadson”, pois há dois cadastros com esse nome.

Logs: `outputs/backup-tests.log` e `outputs/backup-release-build.log`.

## APK entregue

Compilação Android Release concluída com código de saída 0 em pasta isolada (`.codex-build/backup-105`), após um bloqueio de arquivo no diretório compartilhado de build. Arquivo: `outputs/MoneyPro-1.0.5-Release.apk`, versão 1.0.5, código 10. Assinatura verificada pelo `apksigner` nos esquemas v1, v2 e v3.
