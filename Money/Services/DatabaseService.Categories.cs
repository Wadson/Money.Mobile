using Microsoft.Data.Sqlite;
using Money.Models;
using MauiIcons.Material;
namespace Money.Services;

public sealed partial class DatabaseService
{
    public Task<List<MainCategoryItem>> GetMainCategoriesAsync(string? type=null,bool includeInactive=false) => QueryAsync("""
        SELECT p.id_categoria_pilar,p.nome,p.tipo,COALESCE(NULLIF(p.cor,''),'#00875A'),p.icone,p.ordem,p.ativo,
          (SELECT COUNT(*) FROM Categorias c WHERE c.id_categoria_pilar=p.id_categoria_pilar AND c.id_usuario=p.id_usuario)
        FROM CategoriaPilar p WHERE p.id_usuario=@u AND (@all=1 OR p.ativo=1) AND (@t IS NULL OR p.tipo=@t)
        ORDER BY p.tipo,p.ordem,p.nome
        """,r=>new MainCategoryItem(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetString(3),r.IsDBNull(4)?null:r.GetString(4),r.GetInt32(5),r.GetInt64(6)==1,r.GetInt32(7)),("@u",RequiredUserId),("@all",includeInactive?1:0),("@t",type));
    public async Task<MainCategoryItem?> GetMainCategoryAsync(long id) => (await GetMainCategoriesAsync(includeInactive:true)).SingleOrDefault(x=>x.Id==id);
    public Task<List<SubcategoryItem>> GetSubcategoriesAsync(long? mainCategoryId=null,string? type=null,bool includeInactive=false) => QueryAsync("""
        SELECT c.id_categoria,c.id_categoria_pilar,p.nome,c.nome_categoria,c.tipo,
          COALESCE(NULLIF(c.cor,''),NULLIF(p.cor,''),'#00875A'),c.icone,c.ativo
        FROM Categorias c JOIN CategoriaPilar p ON p.id_categoria_pilar=c.id_categoria_pilar AND p.id_usuario=c.id_usuario AND p.tipo=c.tipo
        WHERE c.id_usuario=@u AND (@m IS NULL OR c.id_categoria_pilar=@m) AND (@t IS NULL OR c.tipo=@t)
          AND (@all=1 OR (c.ativo=1 AND p.ativo=1)) ORDER BY p.ordem,p.nome,c.nome_categoria
        """,r=>new SubcategoryItem(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.IsDBNull(6)?null:r.GetString(6),r.GetInt64(7)==1),
        ("@u",RequiredUserId),("@m",mainCategoryId),("@t",type),("@all",includeInactive?1:0));
    public async Task<SubcategoryItem?> GetSubcategoryAsync(long id) => (await GetSubcategoriesAsync(includeInactive:true)).SingleOrDefault(x=>x.Id==id);
    public Task<SubcategoryItem?> GetSubcategoryWithMainCategoryAsync(long id) => GetSubcategoryAsync(id);
    public Task<long> AddMainCategoryAsync(string name,string type,string color,string icon,int order=0,bool active=true) => SaveCategoryEntityAsync(null,null,name,type,color,icon,order,active);
    public Task<long> UpdateMainCategoryAsync(long id,string name,string type,string color,string icon,int order=0,bool active=true) => SaveCategoryEntityAsync(id,null,name,type,color,icon,order,active);
    public Task<long> AddSubcategoryAsync(long mainCategoryId,string name,string type,string? color,string icon,bool active=true) => SaveCategoryEntityAsync(null,mainCategoryId,name,type,color,icon,0,active);
    public Task<long> UpdateSubcategoryAsync(long id,long mainCategoryId,string name,string type,string? color,string icon,bool active=true) => SaveCategoryEntityAsync(id,mainCategoryId,name,type,color,icon,0,active);
    private async Task<long> SaveCategoryEntityAsync(long? id,long? main,string name,string type,string? color,string icon,int order,bool active)
    {
        var user=RequiredUserId;
        name=name.Trim();
        if(name.Length<2) throw new ArgumentException("Informe um nome com pelo menos dois caracteres.");
        if(type is not ("receita" or "despesa")) throw new ArgumentException("Tipo inválido.");
        if(!string.IsNullOrWhiteSpace(color) && !System.Text.RegularExpressions.Regex.IsMatch(color,"^#[0-9a-fA-F]{6}$")) throw new ArgumentException("Informe uma cor hexadecimal válida.");
        if(!Enum.TryParse<MaterialIcons>(icon,false,out var value)||!Enum.IsDefined(value)) throw new ArgumentException("Selecione um ícone válido.");
        var table=main is null?"CategoriaPilar":"Categorias";
        var key=main is null?"id_categoria_pilar":"id_categoria";
        var nameColumn=main is null?"nome":"nome_categoria";
        await using var db=new SqliteConnection(ConnectionString);await db.OpenAsync();
        await using var tx=db.BeginTransaction();
        try
        {
            if(id is not null && Convert.ToInt32(await ScalarAsync(db,$"SELECT COUNT(*) FROM {table} WHERE {key}=@id AND id_usuario=@u",("@id",id),("@u",user)))!=1)
                throw new InvalidOperationException("Cadastro não encontrado para este usuário.");
            if(main is not null)
            {
                var allowed=Convert.ToInt32(await ScalarAsync(db,"""
                    SELECT COUNT(*) FROM CategoriaPilar p WHERE p.id_categoria_pilar=@m AND p.id_usuario=@u AND p.tipo=@t
                    AND (p.ativo=1 OR EXISTS(SELECT 1 FROM Categorias c WHERE c.id_categoria=@id AND c.id_categoria_pilar=p.id_categoria_pilar AND c.id_usuario=@u))
                    """,("@m",main),("@u",user),("@t",type),("@id",id)));
                if(allowed!=1) throw new ArgumentException("Selecione uma categoria ativa, do mesmo usuário e tipo.");
            }
            var parameters=new (string,object?)[]{("@id",id),("@u",user),("@n",name),("@norm",CategoryCatalog.Normalize(name)),("@t",type),("@c",string.IsNullOrWhiteSpace(color)?null:color.ToUpperInvariant()),("@i",icon),("@o",order),("@a",active?1:0),("@m",main)};
            if(id is null)
            {
                await ExecuteAsync(db,$"INSERT INTO {table}(id_usuario,{nameColumn},nome_normalizado,tipo,cor,icone,ativo,{(main is null?"ordem":"id_categoria_pilar")}) VALUES(@u,@n,@norm,@t,@c,@i,@a,{(main is null?"@o":"@m")})",parameters);
                id=Convert.ToInt64(await ScalarAsync(db,"SELECT last_insert_rowid()"));
            }
            else await ExecuteAsync(db,$"UPDATE {table} SET {nameColumn}=@n,nome_normalizado=@norm,tipo=@t,cor=@c,icone=@i,ativo=@a,{(main is null?"ordem=@o":"id_categoria_pilar=@m")},data_atualizacao=CURRENT_TIMESTAMP WHERE {key}=@id AND id_usuario=@u",parameters);
            tx.Commit();return id.Value;
        }
        catch(SqliteException ex) when(ex.SqliteErrorCode==19) { tx.Rollback();throw new ArgumentException("Nome duplicado ou alteração incompatível com o histórico e relacionamentos existentes.",ex); }
    }
    public Task DeactivateMainCategoryAsync(long id) => ChangeCategoryStateAsync(id,true,false);
    public Task DeactivateSubcategoryAsync(long id) => ChangeCategoryStateAsync(id,false,false);
    public Task DeleteMainCategoryAsync(long id) => ChangeCategoryStateAsync(id,true,true);
    public Task DeleteSubcategoryAsync(long id) => ChangeCategoryStateAsync(id,false,true);
    private async Task ChangeCategoryStateAsync(long id,bool main,bool delete)
    {
        var user=RequiredUserId;var table=main?"CategoriaPilar":"Categorias";var key=main?"id_categoria_pilar":"id_categoria";
        await using var db=new SqliteConnection(ConnectionString);await db.OpenAsync();await using var tx=db.BeginTransaction();
        if(Convert.ToInt32(await ScalarAsync(db,$"SELECT COUNT(*) FROM {table} WHERE {key}=@id AND id_usuario=@u",("@id",id),("@u",user)))!=1) throw new InvalidOperationException("Cadastro não encontrado para este usuário.");
        if(delete)
        {
            // Enumerate all actual foreign keys, including tables introduced by restored databases.
            var tables=new List<string>();
            await using(var cmd=CreateCommand(db,"SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'"))
            await using(var reader=await cmd.ExecuteReaderAsync()) while(await reader.ReadAsync()) tables.Add(reader.GetString(0));
            foreach(var referencing in tables)
            {
                var columns=new List<string>();var quoted=referencing.Replace("\"","\"\"");
                await using(var cmd=CreateCommand(db,$"PRAGMA foreign_key_list(\"{quoted}\")"))
                await using(var reader=await cmd.ExecuteReaderAsync()) while(await reader.ReadAsync()) if(reader.GetString(2).Equals(table,StringComparison.OrdinalIgnoreCase)) columns.Add(reader.GetString(3));
                foreach(var column in columns)
                    if(Convert.ToInt64(await ScalarAsync(db,$"SELECT COUNT(*) FROM \"{quoted}\" WHERE \"{column.Replace("\"","\"\"")}\"=@id",("@id",id)))>0)
                        throw new InvalidOperationException("Este cadastro possui histórico ou vínculos. Use Desativar para preservá-los.");
            }
        }
        await ExecuteAsync(db,delete?$"DELETE FROM {table} WHERE {key}=@id AND id_usuario=@u":$"UPDATE {table} SET ativo=0,data_atualizacao=CURRENT_TIMESTAMP WHERE {key}=@id AND id_usuario=@u",("@id",id),("@u",user));tx.Commit();
    }
    public static SubcategoryItem ResolveImportedSubcategory(IEnumerable<SubcategoryItem> categories,string mainOrLegacyName,string? subcategoryName=null)
    {
        var matches=categories.Where(x=>CategoryCatalog.Normalize(x.Name)==CategoryCatalog.Normalize(string.IsNullOrWhiteSpace(subcategoryName)?mainOrLegacyName:subcategoryName)
            && (string.IsNullOrWhiteSpace(subcategoryName)||CategoryCatalog.Normalize(x.MainCategoryName)==CategoryCatalog.Normalize(mainOrLegacyName))).ToList();
        if(matches.Count!=1) throw new InvalidDataException(matches.Count==0?"Categoria/subcategoria não encontrada.":"Subcategoria ambígua. Informe Categoria e Subcategoria no CSV.");
        return matches[0];
    }
    public Task<List<(long SubcategoryId,string MainCategory,string Subcategory,string Pillar) >> GetBudgetPillarsAsync() => QueryAsync("""
        SELECT c.id_categoria,p.nome,c.nome_categoria,COALESCE(o.pilar,'')
        FROM Categorias c JOIN CategoriaPilar p ON p.id_categoria_pilar=c.id_categoria_pilar AND p.id_usuario=c.id_usuario
        LEFT JOIN CategoriaPilarOrcamentario o ON o.id_subcategoria=c.id_categoria AND o.id_usuario=c.id_usuario
        WHERE c.id_usuario=@u AND c.tipo='despesa' ORDER BY p.ordem,c.nome_categoria
        """,r=>(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetString(3)),("@u",RequiredUserId));
    public async Task SetBudgetPillarAsync(long subcategoryId,string pillar)
    {
        if(pillar is not ("FIXAS_ESSENCIAIS" or "VARIAVEIS_LAZER" or "RESERVA_EMERGENCIA" or "INVESTIMENTOS")) throw new ArgumentException("Pilar inválido.");
        var sub=await GetSubcategoryAsync(subcategoryId);
        if(sub is null||sub.Type!="despesa") throw new ArgumentException("Subcategoria inválida.");
        await using var db=new SqliteConnection(ConnectionString);await db.OpenAsync();
        await ExecuteAsync(db,"INSERT INTO CategoriaPilarOrcamentario VALUES(@u,@id,@p) ON CONFLICT(id_usuario,id_subcategoria) DO UPDATE SET pilar=excluded.pilar",("@u",RequiredUserId),("@id",subcategoryId),("@p",pillar));
    }
}
