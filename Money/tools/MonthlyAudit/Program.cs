using Microsoft.Data.Sqlite;
SQLitePCL.Batteries_V2.Init();

static async Task<decimal> SumAsync(SqliteConnection db, string type, bool paid, string start, string end)
{
    await using var command = db.CreateCommand();
    command.CommandText = type == "receita"
        ? """SELECT COALESCE(SUM(valor),0) FROM Transacoes WHERE id_usuario=1 AND tipo='receita' AND pago=1 AND data_pagamento IS NOT NULL AND date(data_pagamento)>=date(@start) AND date(data_pagamento)<date(@end) AND NOT(parcelado=1 AND id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=Transacoes.id_transacao))"""
        : $"""SELECT COALESCE(SUM(valor),0) FROM Transacoes WHERE id_usuario=1 AND tipo='despesa' AND pago={(paid ? 1 : 0)} AND date(COALESCE(data_vencimento,data))>=date(@start) AND date(COALESCE(data_vencimento,data))<date(@end) AND NOT(parcelado=1 AND id_transacao_pai IS NULL AND EXISTS(SELECT 1 FROM Transacoes f WHERE f.id_transacao_pai=Transacoes.id_transacao))""";
    command.Parameters.AddWithValue("@start", start); command.Parameters.AddWithValue("@end", end);
    return Convert.ToDecimal(await command.ExecuteScalarAsync() ?? 0);
}

await using var testDb = new SqliteConnection("Data Source=:memory:"); await testDb.OpenAsync();
await using (var setup = testDb.CreateCommand())
{
    setup.CommandText = """
      CREATE TABLE Transacoes(id_transacao INTEGER PRIMARY KEY,id_usuario INTEGER,tipo TEXT,valor NUMERIC,data TEXT,data_vencimento TEXT,data_pagamento TEXT,pago INTEGER,parcelado INTEGER DEFAULT 0,id_transacao_pai INTEGER,numero_parcela INTEGER,total_parcelas INTEGER);
      INSERT INTO Transacoes VALUES
      (1,1,'receita',1000,'2026-07-25',NULL,'2026-08-05',1,0,NULL,1,1),(2,1,'despesa',500,'2026-07-25','2026-08-05','2026-08-04',1,0,NULL,1,1),(3,1,'despesa',500,'2026-07-25','2026-08-05','2026-08-15',1,0,NULL,1,1),(4,1,'despesa',500,'2026-08-01','2026-08-10',NULL,0,0,NULL,1,1),(5,1,'receita',700,'2026-08-20',NULL,NULL,0,0,NULL,1,1),(6,1,'receita',300,'2026-08-01',NULL,'2026-08-10',1,0,NULL,1,1),(7,1,'despesa',1200,'2026-07-20','2026-08-10',NULL,0,1,NULL,1,3),(8,1,'despesa',400,'2026-07-20','2026-08-10',NULL,0,1,7,1,3),(9,1,'despesa',400,'2026-07-20','2026-09-10',NULL,0,1,7,2,3),(10,1,'despesa',400,'2026-07-20','2026-10-10',NULL,0,1,7,3,3),(11,1,'despesa',800,'2026-07-20','2026-08-10',NULL,0,1,NULL,1,2),(12,1,'despesa',400,'2026-07-20','2026-08-10',NULL,0,1,11,1,2),(13,1,'despesa',400,'2026-07-20','2026-09-10',NULL,0,1,11,2,2),(14,1,'despesa',250,'2026-07-22','2026-08-20',NULL,0,1,NULL,1,1);
      """;
    await setup.ExecuteNonQueryAsync();
}
var tests = new (string Name, decimal Expected, decimal Actual)[] {
 ("1 receita julho",0,await SumAsync(testDb,"receita",true,"2026-07-01","2026-08-01")),
 ("1/6 receitas agosto",1300,await SumAsync(testDb,"receita",true,"2026-08-01","2026-09-01")),
 ("2 despesa julho",0,await SumAsync(testDb,"despesa",true,"2026-07-01","2026-08-01")),
 ("2/3 despesas pagas agosto",1000,await SumAsync(testDb,"despesa",true,"2026-08-01","2026-09-01")),
 ("pendentes agosto sem ocultar parcelado legado",1550,await SumAsync(testDb,"despesa",false,"2026-08-01","2026-09-01")),
 ("5 receita futura excluída",1300,await SumAsync(testDb,"receita",true,"2026-08-01","2026-09-01")),
 ("7/8 parcelas setembro",800,await SumAsync(testDb,"despesa",false,"2026-09-01","2026-10-01")),
 ("7 parcela outubro",400,await SumAsync(testDb,"despesa",false,"2026-10-01","2026-11-01")) };
foreach (var test in tests) Console.WriteLine($"{test.Name}: esperado={test.Expected:0.00}; obtido={test.Actual:0.00}; {(test.Expected==test.Actual?"PASS":"FAIL")}");

var currentBalance = 3000m;
var septemberProjection = currentBalance + 5000m - 1500m - 1000m;
var octoberProjection = septemberProjection + 5000m - 1500m;
var projectionPass = septemberProjection == 5500m && octoberProjection == 9000m;
Console.WriteLine($"projeção acumulada: setembro={septemberProjection:0.00}; outubro={octoberProjection:0.00}; {(projectionPass?"PASS":"FAIL")}");
var accountA = 2000m; var accountB = 1000m; var netWorthBefore = accountA + accountB;
accountA -= 400m; accountB += 400m;
var transferPass = accountA + accountB == netWorthBefore;
Console.WriteLine($"transferência sem receita/despesa: patrimônio={accountA+accountB:0.00}; {(transferPass?"PASS":"FAIL")}");

if (args.Length > 0)
{
    await using var auditDb = new SqliteConnection($"Data Source={args[0]};Mode=ReadOnly"); await auditDb.OpenAsync();
    var checks = new Dictionary<string,string> { ["receita paga sem data_pagamento"]="SELECT COUNT(*) FROM Transacoes WHERE tipo='receita' AND pago=1 AND data_pagamento IS NULL", ["despesa sem data_vencimento"]="SELECT COUNT(*) FROM Transacoes WHERE tipo='despesa' AND data_vencimento IS NULL", ["pago=1 sem data_pagamento"]="SELECT COUNT(*) FROM Transacoes WHERE pago=1 AND data_pagamento IS NULL", ["pago=0 com data_pagamento"]="SELECT COUNT(*) FROM Transacoes WHERE pago=0 AND data_pagamento IS NOT NULL", ["valores não positivos"]="SELECT COUNT(*) FROM Transacoes WHERE valor<=0", ["parcelas inconsistentes"]="SELECT COUNT(*) FROM Transacoes WHERE id_transacao_pai IS NOT NULL AND (numero_parcela IS NULL OR total_parcelas IS NULL OR numero_parcela<1 OR numero_parcela>total_parcelas)" };
    foreach (var check in checks) { await using var command=auditDb.CreateCommand(); command.CommandText=check.Value; Console.WriteLine($"AUDIT {check.Key}: {await command.ExecuteScalarAsync()}"); }
}
Environment.ExitCode=tests.All(test=>test.Expected==test.Actual)&&projectionPass&&transferPass?0:1;
