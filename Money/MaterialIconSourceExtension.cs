using MauiIcons.Core;
using MauiIcons.Material;
using Microsoft.Maui.Controls.Xaml;

namespace Money;

/// <summary>
/// Creates a Material icon image source with a semantic, explicit color.
/// This prevents the icon from changing to white when the device uses dark theme
/// while the application control is rendered on a fixed light surface.
/// </summary>
[ContentProperty(nameof(Icon))]
[AcceptEmptyServiceProvider]
public sealed class MaterialIconSourceExtension : IMarkupExtension<ImageSource>
{
    public MaterialIcons Icon { get; set; } = MaterialIcons.Category;
    public string ColorResource { get; set; } = "BlingText";
    public double IconSize { get; set; } = 24;

    public ImageSource ProvideValue(IServiceProvider serviceProvider) =>
        Icon.ToImageSource(ResolveColor(), IconSize);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
        ProvideValue(serviceProvider);

    private Color ResolveColor()
    {
        var color = ThemeColor.Get(ColorResource);
        return color == Colors.Transparent ? ThemeColor.Get("BlingText") : color;
    }
}
