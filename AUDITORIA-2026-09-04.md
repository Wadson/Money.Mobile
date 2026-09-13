# Money.Mobile — auditoria de 04/09/2026

Versão preparada: **1.0.3**, código Android **8**.

## Problemas corrigidos

- **Contas a Pagar:** o aviso de erro estava sobreposto ao resumo. Ele passou a ocupar espaço próprio no cabeçalho rolável da lista, sem cobrir os valores.
- **Cartões e Relatórios:** a inicialização do banco ignorava migrações antigas que criavam `CartoesCredito.cor` e `FaturasCartao`. A reparação agora cria os elementos necessários também em instalações novas e bancos existentes. O carregamento das telas passa a tratar exceções.
- **Compatibilidade do banco:** consultas de despesas e relatórios ainda usavam tabelas/colunas de contas bancárias já removidas no modelo atual. Consultas, baixa de despesas/faturas, edição, parcelamento e importação foram adequados ao modelo atual.
- **Reabertura das telas:** a inicialização recriava uma coluna legada e disparava novamente a conversão das transações. Isso foi corrigido; os testes verificam preservação de registros e do estado de receitas pagas em inicializações repetidas.
- **Importação de cartão:** o agrupador de um parcelamento não é mais confundido com uma parcela duplicada. A gravação do histórico não depende da tabela de importações removida.
- **Limpeza de dados:** a seleção exibe somente entidades disponíveis no banco atual.
- **Navegação principal:** proteção contra abertura simultânea por toques repetidos e tratamento de falhas ao construir as telas afetadas.
- **Biblioteca nativa desnecessária:** removidos o pacote SkiaSharp.Extended.UI.Maui e seus registros/declaracões sem uso. O pacote trazia `libSkiaSharp.so` 2.88.6, que gerava aviso de incompatibilidade com páginas de memória de 16 KB. Nenhum controle ou desenho do aplicativo dependia dele.

## Evidências

- 36 testes automatizados aprovados, sem falhas.
- Serviços reais do aplicativo vinculados ao projeto `tests/Money.Audit`.
- Consultas de Contas a Pagar, Cartões, Relatórios, Dashboard, visão mensal, previsões e configurações.
- Filtros por mês, categoria, fornecedor, cartão e situação.
- Cadastro e edição, baixa, estorno, bloqueio de exclusão de despesas pagas e rollback de pagamento em lote inválido.
- Parcelamento, geração e pagamento de fatura, importações CSV de despesas e cartão.
- Geração de PDF com dados e vazio pelo renderizador desktop, validação de totais e assinatura de arquivo PDF.
- Reparação de coluna ausente, cinco inicializações consecutivas, verificação de integridade e referências SQLite.
- Migração de cópia consistente do banco local com preservação da quantidade de despesas. O banco original não foi alterado.
- Compilação Android Debug concluída com zero erros; avisos preexistentes de nulidade/API foram mantidos.
- Validação de XML e existência das chaves de recursos estáticos nos 36 arquivos XAML: aprovada. Essa verificação não substitui a inspeção visual.

## Limites da validação

Nenhum aparelho Android estava conectado e não havia emulador instalado no SDK disponível. Portanto, não houve teste visual em aparelho, teste do renderizador PDF Android nem instalação interativa do APK. Os testes de banco/PDF desktop e a compilação não substituem essa validação em dispositivo.

Os métodos legados que não fazem parte dos fluxos atuais não tiveram cobertura exaustiva. Não se afirma ausência absoluta de defeitos.
