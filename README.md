# Money Pro

Aplicativo de controle financeiro pessoal para Windows e Android construído com .NET MAUI 10 e SQLite.

## Executar

Pré-requisitos: .NET SDK 10 e workloads `maui-windows` e/ou `maui-android`.

```powershell
dotnet run --project .\Money\Money.csproj -f net10.0-windows10.0.19041.0
```

No Android:

```powershell
dotnet build .\Money\Money.csproj -f net10.0-android
```

Na primeira execução, o aplicativo cria automaticamente `financeiro.db` na pasta local de dados
do aplicativo. O caminho exato é exibido no rodapé do dashboard.

## Recursos implementados

- dashboard consolidado com saldo, receitas, despesas, economia e score financeiro;
- navegação adaptativa: menu lateral agrupado no desktop e barra inferior no celular;
- dashboard de smartphone com cartões em duas colunas, análises empilhadas e extrato compacto;
- autenticação local com login, cadastro, sessão persistente e logout;
- modo atual de usuário único com entrada direta; telas de autenticação preservadas para uso futuro;
- senhas protegidas com PBKDF2-SHA256, salt aleatório e comparação em tempo constante;
- bloqueio temporário após cinco tentativas inválidas;
- recuperação de senha totalmente local por data de nascimento, pergunta e resposta de segurança;
- isolamento completo dos dados por usuário autenticado;
- patrimônio líquido, taxa de economia, dias de reserva, média diária e projeção de 90 dias;
- acompanhamento mensal de orçamentos;
- listagem de transações recentes;
- lançamento de receitas e despesas por conta ou cartão;
- atualização atômica de saldos e limites;
- schema SQLite completo para usuários, contas, cartões, categorias, transações, receitas,
  orçamentos, configurações, backups e auditoria;
- migrations versionadas por `PRAGMA user_version`, índices, constraints e triggers;
- migration v2 com segurança, histórico de importações e centro de notificações;
- categorias, contas, cartão, orçamento e movimentações de demonstração na primeira execução;
- banco local em modo WAL com chaves estrangeiras habilitadas.
- cadastro e edição de cartões, categorias, subcategorias e perfil;
- lançamento direto de salário, receitas e recebimentos;
- backup e restauração local com validação de integridade.

## Estrutura

- `Money/Models`: DTOs e modelos de leitura/escrita.
- `Money/Services`: persistência, migrations e regras transacionais.
- `Money/MainPage.*`: dashboard e fluxo de lançamento.
- `Money/Resources/Styles`: identidade visual e componentes reutilizáveis.

## Compilar

```powershell
dotnet restore .\Money\Money.csproj
dotnet build .\Money\Money.csproj -f net10.0-windows10.0.19041.0
```

## Conta demonstrativa

- Email: `demo@money.local`
- Senha: `Money123`
- Data de nascimento para recuperação: `01/01/1990`
- Pergunta: `Cidade onde nasceu?`
- Resposta: `Belém`

O hash legado da demonstração é atualizado automaticamente na primeira inicialização da versão 2.
