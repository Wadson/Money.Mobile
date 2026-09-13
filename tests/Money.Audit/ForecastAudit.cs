using Microsoft.Data.Sqlite;
using Money.Services;

internal static class ForecastAudit
{
    public static async Task RunAsync(Func<string, Func<Task>, Task> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "MoneyForecastAudit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var database = new DatabaseService(Path.Combine(directory, "forecast.db")) { CurrentUserId = 1 };
        await database.InitializeAsync();
        await using var db = new SqliteConnection($"Data Source={database.DatabasePath};Foreign Keys=True;Pooling=False");
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = """
            DELETE FROM Transacoes;
            INSERT INTO Transacoes(id_usuario,id_categoria,valor,data,data_vencimento,descricao,tipo,pago)
            SELECT 1,id_categoria,1000,'2023-12-20','2024-01-01','Recebida','receita',1 FROM Categorias WHERE id_usuario=1 AND tipo='receita' LIMIT 1;
            INSERT INTO Transacoes(id_usuario,id_categoria,valor,data,data_vencimento,descricao,tipo,pago)
            SELECT 1,id_categoria,500,'2023-12-20','2024-01-20','A receber','receita',0 FROM Categorias WHERE id_usuario=1 AND tipo='receita' LIMIT 1;
            INSERT INTO Transacoes(id_usuario,id_categoria,valor,data,data_vencimento,descricao,tipo,pago)
            SELECT 1,id_categoria,100,'2023-12-20','2024-01-15','Paga','despesa',1 FROM Categorias WHERE id_usuario=1 AND tipo='despesa' LIMIT 1;
            INSERT INTO Transacoes(id_usuario,id_categoria,valor,data,data_vencimento,descricao,tipo,pago)
            SELECT 1,id_categoria,200,'2023-12-20','2024-01-31','A pagar','despesa',0 FROM Categorias WHERE id_usuario=1 AND tipo='despesa' LIMIT 1;
            INSERT INTO Transacoes(id_usuario,id_categoria,valor,data,data_vencimento,descricao,tipo,pago)
            SELECT 1,id_categoria,4000,'2024-01-01','2024-02-01','Outro mês','despesa',0 FROM Categorias WHERE id_usuario=1 AND tipo='despesa' LIMIT 1;
            INSERT INTO Transacoes(id_usuario,id_categoria,valor,data,data_vencimento,descricao,tipo,pago)
            SELECT 1,id_categoria,700,'2030-01-01','2030-01-01','Futuro distante','receita',0 FROM Categorias WHERE id_usuario=1 AND tipo='receita' LIMIT 1;
            """;
        await command.ExecuteNonQueryAsync();
        var service = new FinancialForecastService(database);
        void Equal(decimal expected, decimal actual)
        { if (expected != actual) throw new Exception($"expected {expected}, actual {actual}"); }
        await test("forecast separates received, pending, paid and total by due month", async () =>
        {
            var month = (await service.CalculateAsync(new(2024, 1, 1), 1)).Months.Single();
            Equal(1000, month.RealizedIncome); Equal(500, month.ExpectedIncome);
            Equal(100, month.PaidExpenses); Equal(200, month.ExpectedExpenses);
            Equal(1500, month.TotalIncome); Equal(300, month.TotalExpenses);
            Equal(900, month.RealizedResult); Equal(1200, month.ProjectedResult);
        });
        await test("forecast selected month is unaffected by a negative adjacent month", async () =>
        {
            var january = (await service.CalculateAsync(new(2024, 1, 1), 1)).Months.Single();
            var february = (await service.CalculateAsync(new(2024, 2, 1), 1)).Months.Single();
            Equal(1200, january.ProjectedResult); Equal(-4000, february.ProjectedResult);
        });
        await test("forecast supports distant future and empty past months", async () =>
        {
            var future = (await service.CalculateAsync(new(2030, 1, 1), 1)).Months.Single();
            var empty = (await service.CalculateAsync(new(2023, 12, 1), 1)).Months.Single();
            Equal(700, future.ExpectedIncome); Equal(0, empty.TotalIncome); Equal(0, empty.TotalExpenses);
            if (empty.HasActivity) throw new Exception("empty month marked as active");
        });
        await test("forecast pending amounts update after receiving and paying", async () =>
        {
            command.CommandText = "UPDATE Transacoes SET pago=1 WHERE descricao IN('A receber','A pagar')";
            await command.ExecuteNonQueryAsync();
            var month = (await service.CalculateAsync(new(2024, 1, 1), 1)).Months.Single();
            Equal(0, month.ExpectedIncome); Equal(0, month.ExpectedExpenses);
            Equal(1500, month.TotalIncome); Equal(300, month.TotalExpenses);
        });
        await test("forecast counts installments without duplicating the parent", async () =>
        {
            var category = (await database.GetCategoriesAsync("despesa")).First();
            await database.SaveInstallmentTransactionAsync(new("Parcelas",300,new(2024,1,1),"despesa",category.Id,null,null,null),2,new(2024,1,15));
            var january = (await service.CalculateAsync(new(2024,1,1),1)).Months.Single();
            var february = (await service.CalculateAsync(new(2024,2,1),1)).Months.Single();
            Equal(450,january.TotalExpenses); Equal(150,january.ExpectedExpenses);
            Equal(4150,february.TotalExpenses);
        });
    }
}
