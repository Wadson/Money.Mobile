using MauiIcons.Core;
using MauiIcons.Material;
using Money.Models;
namespace Money.Helpers;

public static class CategoryIconCatalog
{
    public static IReadOnlyList<(string Id,string Name)> All { get; } = CategoryCatalog.All
        .Select(x=>(Id:x.Icon,Name:x.Name)).Concat(CategoryCatalog.All.SelectMany(x=>x.Children.Select(c=>(Id:c.Icon,Name:c.Name))))
        .Append((Id:"Sell",Name:"Subcategoria")).DistinctBy(x=>x.Id).ToArray();
}
