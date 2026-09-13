using System.Globalization;
using System.Text;
using MauiIcons.Material;
using Money.Models;

namespace Money.Helpers;

public static class CategoryVisualResolver
{
    private static readonly IReadOnlyDictionary<string, MaterialIcons> LegacyIcons =
        new Dictionary<string, MaterialIcons>(StringComparer.OrdinalIgnoreCase)
        {
            ["category_education"] = MaterialIcons.School,
            ["category_food"] = MaterialIcons.Restaurant,
            ["category_health"] = MaterialIcons.MedicalServices,
            ["category_housing"] = MaterialIcons.House,
            ["category_leisure"] = MaterialIcons.Movie,
            ["category_other"] = MaterialIcons.Category,
            ["category_shopping"] = MaterialIcons.ShoppingBag,
            ["category_taxes"] = MaterialIcons.RequestQuote,
            ["category_transport"] = MaterialIcons.DirectionsCar,
            ["category_utilities"] = MaterialIcons.Lightbulb,
            ["icon_expense"] = MaterialIcons.TrendingDown,
            ["icon_income"] = MaterialIcons.TrendingUp
        };

    public static MaterialIcons Icon(ICategoryVisual category)
    {
        var stored = Path.GetFileNameWithoutExtension(category.Icon?.Trim() ?? "");
        if (LegacyIcons.TryGetValue(stored, out var legacy)) return legacy;
        if (Enum.TryParse<MaterialIcons>(stored, true, out var parsed) && Enum.IsDefined(parsed)) return parsed;
        var pascal = string.Concat(stored.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
        if (Enum.TryParse<MaterialIcons>(pascal, true, out parsed) && Enum.IsDefined(parsed)) return parsed;

        var standard = CategoryCatalog.All.SelectMany(x => x.Children).FirstOrDefault(x => CategoryCatalog.Normalize(x.Name) == CategoryCatalog.Normalize(category.Name));
        if (standard is not null && Enum.TryParse<MaterialIcons>(standard.Icon, out var standardIcon)) return standardIcon;
        return category is MainCategoryItem ? MaterialIcons.Category : MaterialIcons.Sell;
    }

    public static Color Foreground(ICategoryVisual category)
    {
        try { return ThemeColor.Parse(category.Color); }
        catch { return ThemeColor.Get("BlingPrimary"); }
    }

    public static Color Background(ICategoryVisual category) => Foreground(category).WithAlpha(.14f);

    public static SelectionOption Option(ICategoryVisual category, int index, bool selected) => new()
    {
        Index = index,
        Label = category.Name,
        Subtitle = category is SubcategoryItem sub ? sub.MainCategoryName : null,
        ImageSource = Icon(category),
        // Em fundos escuros, cores de categorias como preto/grafite perdem contraste.
        Foreground = IsDarkTheme() ? Colors.White : Foreground(category),
        Background = Background(category),
        IsSelected = selected
    };

    private static bool IsDarkTheme()
    {
        var app = Application.Current;
        return app?.UserAppTheme == AppTheme.Dark ||
               (app?.UserAppTheme == AppTheme.Unspecified && app.RequestedTheme == AppTheme.Dark);
    }

    private static string Normalize(string value)
    {
        var text = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(text.Length);
        foreach (var character in text)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(character));
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
