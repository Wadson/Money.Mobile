using Microsoft.Data.Sqlite;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

internal static class CategoryAudit
{
    public static async Task RunAsync(Func<string,Func<Task>,Task> test)
    {
        var path=Path.Combine(Path.GetTempPath(),"MoneyCategoryAudit",Guid.NewGuid().ToString("N"),"audit.db");
        var db=new DatabaseService(path){CurrentUserId=1}; await db.InitializeAsync();
        async Task<object?> Sql(string sql) { await using var c=new SqliteConnection($"Data Source={path}");await c.OpenAsync();await using var cmd=c.CreateCommand();cmd.CommandText=sql;return await cmd.ExecuteScalarAsync(); }
        void Check(bool ok,string message="Unexpected result") { if(!ok)throw new Exception(message); }
        async Task Reject(Func<Task> action) { try{await action();}catch(Exception e) when(e is ArgumentException or InvalidOperationException or SqliteException or InvalidDataException){return;}throw new Exception("Invalid operation accepted"); }
        await test("cross-platform schema v30 and no recursive hierarchy",async()=>{Check(Convert.ToInt64(await Sql("PRAGMA user_version"))==30);Check(Convert.ToInt64(await Sql("SELECT COUNT(*) FROM pragma_table_info('Categorias') WHERE name='id_categoria_pai'"))==0);});
        await Sql("INSERT INTO Usuarios(id_usuario,nome,email,senha_hash,ativo) VALUES(2,'Catalog user','catalog@example.test','test',1)");
        await using(var c=new SqliteConnection($"Data Source={path}")){await c.OpenAsync();await using var tx=c.BeginTransaction();await DatabaseService.SeedCategoryCatalogAsync(c,2);tx.Commit();}
        var catalogDb=new DatabaseService(path){CurrentUserId=2};
        foreach(var seed in CategoryCatalog.All)
            await test("catalog and Material icons: "+seed.Name,async()=>{
                var main=(await catalogDb.GetMainCategoriesAsync(seed.Type)).Single(x=>x.Name==seed.Name);
                Check(main.Icon==seed.Icon);Check(Enum.TryParse<MaterialIcons>(main.Icon,out var icon)&&Enum.IsDefined(icon));
                var subs=await catalogDb.GetSubcategoriesAsync(main.Id);
                foreach(var child in seed.Children){var sub=subs.Single(x=>x.Name==child.Name);Check(sub.Icon==child.Icon);Check(Enum.TryParse<MaterialIcons>(sub.Icon,out icon)&&Enum.IsDefined(icon));}
            });
        long mainId=0,subId=0;
        await test("create main category",async()=>{mainId=await db.AddMainCategoryAsync("Audit main","despesa","#00875A","Category");Check((await db.GetMainCategoryAsync(mainId))?.Name=="Audit main");});
        await test("create subcategory with inherited color",async()=>{subId=await db.AddSubcategoryAsync(mainId,"Audit child","despesa",null,"Sell");Check((await db.GetSubcategoryAsync(subId))?.Color=="#00875A");});
        await test("main category is required",()=>Reject(()=>db.AddSubcategoryAsync(0,"Invalid","despesa",null,"Sell")));
        await test("subcategory type must match",()=>Reject(()=>db.AddSubcategoryAsync(mainId,"Invalid","receita",null,"Sell")));
        await test("normalized main duplicate rejected",()=>Reject(()=>db.AddMainCategoryAsync(" AUDIT MAIN ","despesa","#00875A","Category")));
        await test("normalized child duplicate rejected",()=>Reject(()=>db.AddSubcategoryAsync(mainId," AUDIT CHILD ","despesa",null,"Sell")));
        await test("same child name permitted under another main",async()=>{var other=await db.AddMainCategoryAsync("Other main","despesa","#00875A","Category");await db.AddSubcategoryAsync(other,"Audit child","despesa",null,"Sell");});
        await test("foreign user cannot read or mutate categories",async()=>{
            db.CurrentUserId=long.MaxValue;
            try{Check((await db.GetMainCategoriesAsync()).Count==0);Check((await db.GetSubcategoriesAsync()).Count==0);await Reject(()=>db.UpdateMainCategoryAsync(mainId,"Stolen","despesa","#00875A","Category"));await Reject(()=>db.DeleteSubcategoryAsync(subId));await Reject(()=>db.AddSubcategoryAsync(mainId,"Foreign","despesa",null,"Sell"));}
            finally{db.CurrentUserId=1;}
        });
        await test("main type change with children rejected",()=>Reject(()=>db.UpdateMainCategoryAsync(mainId,"Audit main","receita","#00875A","Category")));
        var food=(await db.GetMainCategoriesAsync()).Single(x=>x.Name=="Alimentação");
        var home=(await db.GetMainCategoriesAsync()).Single(x=>x.Name=="Casa/Moradia");
        var children=await db.GetSubcategoriesAsync();
        long Child(string name)=>children.Single(x=>x.Name==name).Id;
        await test("expense stores and edits subcategory ID",async()=>{
            await db.AddTransactionAsync(new("Category edit",10,new(2040,1,1),"despesa",Child("Supermercado"),null,null,null));
            var id=Convert.ToInt64(await Sql("SELECT id_transacao FROM Transacoes WHERE descricao='Category edit'"));
            Check((await db.GetTransactionForEditAsync(id,"despesa")).CategoryId==Child("Supermercado"));
            await db.UpdateListedTransactionAsync(id,"despesa","Category edit",10,new(2040,1,1),Child("Energia Elétrica"),null);
            Check((await db.GetTransactionForEditAsync(id,"despesa")).CategoryId==Child("Energia Elétrica"));
        });
        await test("category filters total 450 and subcategory 300",async()=>{
            foreach(var (name,amount) in new[]{("Lanches",50m),("Restaurantes",100m),("Supermercado",300m),("Combustível",200m)})
                await db.AddTransactionAsync(new("Filter "+name,amount,new(2041,1,1),"despesa",Child(name),null,null,null));
            Check((await db.GetContasPagarAsync(1,2041,mainCategoryId:food.Id)).Sum(x=>x.Valor)==450);
            Check((await db.GetContasPagarAsync(1,2041,categoryId:Child("Supermercado"))).Sum(x=>x.Valor)==300);
            Check((await db.GetFinancialReportItemsAsync(1,2041,mainCategoryId:food.Id)).Sum(x=>x.Amount)==450);
        });
        await test("dashboard groups main categories",async()=>{var result=await db.GetCategoryExpenseReportAsync(1,2041);Check(result.Single(x=>x.Category=="Alimentação").Total==450);Check(result.Single(x=>x.Category=="Transporte").Total==200);});
        await test("new and legacy CSV resolve exact child",()=>{Check(DatabaseService.ResolveImportedSubcategory(children,"Casa/Moradia","Internet").Id==Child("Internet"));Check(DatabaseService.ResolveImportedSubcategory(children,"Internet").Id==Child("Internet"));return Task.CompletedTask;});
        await test("ambiguous legacy CSV rejected",()=>Reject(async()=>{DatabaseService.ResolveImportedSubcategory(await db.GetSubcategoriesAsync(),"Audit child");}));
        await test("deactivation preserves history and blocks new selections",async()=>{await db.DeactivateSubcategoryAsync(Child("Supermercado"));Check(!(await db.GetSubcategoriesAsync()).Any(x=>x.Id==Child("Supermercado")));Check((await db.GetContasPagarAsync(1,2041,categoryId:Child("Supermercado"))).Sum(x=>x.Valor)==300);});
        await test("inactive child rejected for new transaction",()=>Reject(()=>db.AddTransactionAsync(new("Inactive expense",10,new(2041,1,1),"despesa",Child("Supermercado"),null,null,null))));
        await test("monthly budget retains subcategory relationship",async()=>{await db.SaveBudgetAsync(null,Child("Internet"),100,1,2041,null);var budget=(await db.GetBudgetsAsync(1,2041)).Single();Check(budget.Category=="Casa/Moradia / Internet");Check(budget.Limit==100);await db.DeleteBudgetAsync(budget.Id);});
        await test("history prevents deletion",()=>Reject(()=>db.DeleteSubcategoryAsync(Child("Supermercado"))));
        await test("main with children cannot be deleted",()=>Reject(()=>db.DeleteMainCategoryAsync(food.Id)));
        await test("unused category and child can be deleted",async()=>{await db.DeleteSubcategoryAsync(subId);Check(await db.GetSubcategoryAsync(subId)==null);await db.DeleteMainCategoryAsync(mainId);Check(await db.GetMainCategoryAsync(mainId)==null);});
        await test("pillar customization and seed idempotence",async()=>{
            await db.SetBudgetPillarAsync(Child("Internet"),"VARIAVEIS_LAZER");
            var count=await Sql("SELECT COUNT(*) FROM Categorias");
            await using var c=new SqliteConnection($"Data Source={path}");await c.OpenAsync();await using var tx=c.BeginTransaction();await DatabaseService.SeedCategoryCatalogAsync(c,1);await DatabaseService.SeedCategoryCatalogAsync(c,1);tx.Commit();
            Check(Equals(count,await Sql("SELECT COUNT(*) FROM Categorias")));Check((await db.GetBudgetPillarsAsync()).Single(x=>x.SubcategoryId==Child("Internet")).Pillar=="VARIAVEIS_LAZER");
        });
        await test("migration repeat preserves IDs logs and integrity",async()=>{var count=await Sql("SELECT COUNT(*) FROM Logs");await db.InitializeAsync();await db.InitializeAsync();Check(Equals(count,await Sql("SELECT COUNT(*) FROM Logs")));Check(Convert.ToString(await Sql("PRAGMA integrity_check"))=="ok");Check(await Sql("PRAGMA foreign_key_check")==null);});
    }
}
