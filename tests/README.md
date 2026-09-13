# Auditoria Money.Mobile

Execute na raiz do projeto:

```powershell
dotnet run --project tests/Money.Audit/Money.Audit.csproj -- moneypro.db
```

O argumento opcional é um banco existente. Ele é aberto somente para leitura e copiado com a API de backup do SQLite. Todos os testes e migrações são executados em bancos temporários, sem alterar o original.

O executável retorna código 1 se qualquer teste falhar. Ele compila diretamente os serviços de banco, previsão, parcelamento e relatórios do aplicativo. As substituições de plataforma limitam-se a diretórios temporários, cores e compartilhamento (não executado).

São verificados consultas das telas, filtros, edição, pagamento e estorno, atomicidade, parcelamento, faturas, importação CSV, migração idempotente, integridade, limpeza seletiva e geração de PDF com dados e sem dados. O PDF é testado pelo renderizador desktop; o renderizador Android e a interação visual precisam de aparelho ou emulador.

Validação em 04/09/2026: 36 testes aprovados, nenhum reprovado, incluindo cópia de `moneypro.db`.
