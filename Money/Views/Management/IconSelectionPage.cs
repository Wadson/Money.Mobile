using Money.Helpers;
using Money.Views.Dialogs;
namespace Money.Views.Management;
public sealed class IconSelectionPage : OptionSelectionPage
{
    public event EventHandler<string>? IconSelected;
    public IconSelectionPage() : base("Selecione um ícone",CategoryIconCatalog.All.Select((x,i)=>new SelectionOption
    {
        Index=i,Label=x.Name,Subtitle=x.Id,ImageSource=Enum.Parse<MauiIcons.Material.MaterialIcons>(x.Id),
        Foreground=ThemeColor.Get("BlingPrimary")
    }))
    { Selected+=(_,x)=>IconSelected?.Invoke(this,CategoryIconCatalog.All[x.Index].Id); }
}
