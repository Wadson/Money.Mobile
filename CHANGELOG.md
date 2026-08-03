# Changelog

## 2.4.0 — 2026-07-28

- Inicialização em modo usuário único, sem tela de login.
- Cadastro e edição de cartões com limite, fechamento e vencimento.
- Edição de categorias e subcategorias na própria estrutura hierárquica.
- Edição do perfil local e dos dados de recuperação.
- Atalho específico para salários, receitas e recebimentos.
- Backup SQLite com escolha de pasta no Windows.
- Restauração com seleção do arquivo, verificação de integridade e cópia preventiva.

## 2.3.0 — 2026-07-28

- Gestão visual de categorias e subcategorias.
- Formulário dinâmico alternando categoria principal e subcategoria.
- Categoria pai obrigatória para subcategorias.
- Validação de mesmo tipo, mesmo usuário e limite de três níveis.
- Listagem hierárquica com recuo, cor, ícone e nível.
- Filtros por receitas e despesas.
- Seletor hierárquico integrado ao formulário de transações.

## 2.2.0 — 2026-07-28

- Barra inferior móvel ampliada para 66 px de altura.
- Ícones separados das legendas, com 25 px e áreas de toque maiores.
- Cadastro de transação migrado de prompts para formulário completo.
- Categorias filtradas dinamicamente conforme receita ou despesa.
- Seletores de conta e cartão alternados conforme a origem escolhida.
- Campo de observações exibido apenas quando solicitado.
- Validação integrada de tipo, categoria, valor e origem.

## 2.1.0 — 2026-07-28

- Dashboard reorganizado especificamente para smartphones.
- Cards principais em grade 2×2 e indicadores avançados em grade 2×3.
- Fluxo e orçamento empilhados em telas estreitas.
- Extrato móvel compacto, com descrição e valor legíveis.
- Cadastro ampliado com nascimento, pergunta e resposta de segurança.
- Recuperação de senha local em duas etapas, sem provedor de email.
- Respostas de recuperação armazenadas somente como hash SHA-256 normalizado.
- Migration SQLite v3 para os novos dados de recuperação.

## 2.0.2 — 2026-07-28

- Corrigido `DllNotFoundException: e_sqlite3` no Windows.
- Substituídas referências nativas por `SQLitePCLRaw.bundle_e_sqlite3`.
- Inicialização explícita do provedor SQLite no bootstrap do aplicativo.

## 2.0.0 — 2026-07-28

- Login e cadastro integrados ao fluxo inicial.
- Sessão persistente no armazenamento seguro do sistema.
- PBKDF2-SHA256 com 210.000 iterações e salt individual.
- Bloqueio por cinco minutos após cinco falhas consecutivas.
- Email único, senha forte e aceite de termos no cadastro.
- Logout seguro e isolamento das consultas por usuário.
- Migration SQLite v2 para segurança, importações e notificações.
- Novos indicadores: patrimônio líquido, taxa de economia, dias de reserva,
  gasto médio diário e projeção de saldo em 90 dias.

## 1.0.0 — 2026-07-28

- Dashboard, SQLite local, contas, cartões, categorias, transações e orçamentos.
