using System.Globalization;
using System.Text;
using MauiIcons.Material;
using Money.Models;

namespace Money;

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

    public static MaterialIcons Icon(CategoryItem category)
    {
        var stored = Path.GetFileNameWithoutExtension(category.Icon?.Trim() ?? "");
        if (LegacyIcons.TryGetValue(stored, out var legacy)) return legacy;
        if (Enum.TryParse<MaterialIcons>(stored, true, out var parsed)) return parsed;
        var pascal = string.Concat(stored.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
        if (Enum.TryParse<MaterialIcons>(pascal, true, out parsed)) return parsed;

        var value = Normalize(category.Name);
        if (value.Contains("alimenta") || value.Contains("mercado") || value.Contains("restaurante")) return MaterialIcons.Restaurant;
        if (value.Contains("compra") || value.Contains("roupa")) return MaterialIcons.ShoppingBag;
        if (value.Contains("educa") || value.Contains("curso") || value.Contains("escola")) return MaterialIcons.School;
        if (value.Contains("imposto") || value.Contains("taxa")) return MaterialIcons.RequestQuote;
        if (value.Contains("lazer") || value.Contains("viagem") || value.Contains("assinatura")) return MaterialIcons.Movie;
        if (value.Contains("moradia") || value.Contains("casa") || value.Contains("aluguel")) return MaterialIcons.House;
        if (value.Contains("saude") || value.Contains("farmacia") || value.Contains("medic")) return MaterialIcons.MedicalServices;
        if (value.Contains("transport") || value.Contains("combustivel") || value.Contains("uber")) return MaterialIcons.DirectionsCar;
        if (value.Contains("utilidade") || value.Contains("energia") || value.Contains("internet") || value.Contains("agua")) return MaterialIcons.Lightbulb;
        return category.Type == "receita" ? MaterialIcons.TrendingUp : MaterialIcons.Category;
    }

    public static Color Foreground(CategoryItem category)
    {
        try { return ThemeColor.Parse(category.Color); }
        catch { return ThemeColor.Get("BlingPrimary"); }
    }

    public static Color Background(CategoryItem category) => Foreground(category).WithAlpha(.14f);

    public static SelectionOption Option(CategoryItem category, int index, bool selected) => new()
    {
        Index = index,
        Label = category.Name,
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
