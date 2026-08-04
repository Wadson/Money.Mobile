using Microsoft.Data.Sqlite;

var path = args[0];
SQLitePCL.Batteries_V2.Init();
await using var db = new SqliteConnection($"Data Source={path};Mode=ReadOnly");
await db.OpenAsync();

async Task Print(string title, string sql)
{
    Console.WriteLine($"\n## {title}");
    await using var cmd = db.CreateCommand(); cmd.CommandText = sql;
    await using var r = await cmd.ExecuteReaderAsync();
    Console.WriteLine(string.Join(" | ", Enumerable.Range(0, r.FieldCount).Select(r.GetName)));
    while (await r.ReadAsync()) Console.WriteLine(string.Join(" | ", Enumerable.Range(0, r.FieldCount).Select(i => r.IsDBNull(i) ? "NULL" : Convert.ToString(r.GetValue(i)))));
}

await Print("Totais por data de vencimento", """
SELECT tipo,pago,CASE WHEN id_cartao IS NULL THEN 'conta' ELSE 'cartao' END origem,
       COUNT(*) qtd,ROUND(SUM(valor),2) total
FROM Transacoes
WHERE NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL)
  AND date(COALESCE(data_vencimento,data))>='2026-08-01'
  AND date(COALESCE(data_vencimento,data))<'2026-09-01'
GROUP BY tipo,pago,origem ORDER BY tipo,pago,origem;
""");
await Print("Comparação data x vencimento", """
SELECT
 ROUND(SUM(CASE WHEN tipo='receita' AND date(data)>='2026-08-01' AND date(data)<'2026-09-01' THEN valor ELSE 0 END),2) receitas_data,
 ROUND(SUM(CASE WHEN tipo='despesa' AND date(data)>='2026-08-01' AND date(data)<'2026-09-01' AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL) THEN valor ELSE 0 END),2) despesas_data,
 ROUND(SUM(CASE WHEN tipo='despesa' AND date(COALESCE(data_vencimento,data))>='2026-08-01' AND date(COALESCE(data_vencimento,data))<'2026-09-01' AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL) THEN valor ELSE 0 END),2) despesas_vencimento
FROM Transacoes;
""");
await Print("Despesas de agosto", """
SELECT id_transacao,descricao,valor,date(data) data,date(data_vencimento) vencimento,pago,
       numero_parcela,total_parcelas,id_conta,id_cartao,id_transacao_pai
FROM Transacoes WHERE tipo='despesa'
AND date(COALESCE(data_vencimento,data))>='2026-08-01' AND date(COALESCE(data_vencimento,data))<'2026-09-01'
AND NOT(COALESCE(parcelado,0)=1 AND id_transacao_pai IS NULL)
ORDER BY date(COALESCE(data_vencimento,data)),id_transacao;
""");
await Print("Saldos bancários", "SELECT id_conta,nome_conta,saldo_inicial,saldo_atual,ativo FROM Contas ORDER BY id_conta;");
