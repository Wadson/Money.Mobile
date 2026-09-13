using Money.Models;
using Money.Services;
using Money.Views.Dialogs;
namespace Money.Helpers;

public static class CategoryFilterPicker
{
    public static async Task ShowAsync(Page owner, DatabaseService db, Action<long?,long?,string> selected)
    {
        var rows = new List<(long? Main,long? Sub,string Label,ICategoryVisual? Visual)> { (null,null,"Todas as categorias",null) };
        var children=await db.GetSubcategoriesAsync(includeInactive:true);
        foreach(var main in await db.GetMainCategoriesAsync(includeInactive:true))
        {
            rows.Add((main.Id,null,main.Name+" / Todas as subcategorias",main));
            rows.AddRange(children.Where(x=>x.MainCategoryId==main.Id).Select(x=>((long?)main.Id,(long?)x.Id,x.DisplayName,(ICategoryVisual?)x)));
        }
        var options=rows.Select((row,i)=>row.Visual is null
            ? new SelectionOption { Index=i,Label=row.Label,Foreground=ThemeColor.Get("BlingPrimary"),Background=ThemeColor.Get("BlingCard") }
            : new SelectionOption { Index=i,Label=row.Label,ImageSource=CategoryVisualResolver.Icon(row.Visual),Foreground=CategoryVisualResolver.Foreground(row.Visual),Background=CategoryVisualResolver.Background(row.Visual) });
        var page=new OptionSelectionPage("Categoria / Subcategoria",options);
        page.Selected+=(_,option)=>{var row=rows[option.Index];selected(row.Main,row.Sub,row.Label);};
        await owner.Navigation.PushModalAsync(page);
    }
}
