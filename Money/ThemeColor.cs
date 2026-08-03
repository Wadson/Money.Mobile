using Microsoft.Maui.Graphics;

namespace Money;

internal static class BlingPalette
{
    public const string HeaderDarkHex = "#0B281E";
    public const string PrimaryHex = "#00A859";
    public const string PrimaryHoverHex = "#008542";
    public const string AppBackgroundHex = "#FFFFFF";
    public const string CardBackgroundHex = "#FFFFFF";
    public const string TextDarkHex = "#1E293B";
    public const string TextMutedHex = "#1E293B";
    public const string TextLightHex = "#FFFFFF";
    public const string BorderColorHex = "#00A859";
    public const string HeaderDarkDarkHex = "#061711";
    public const string BackgroundDarkHex = "#0F172A";
    public const string CardDarkHex = "#1E293B";
    public const string TextDarkDarkHex = "#FFFFFF";
    public const string TextMutedDarkHex = "#FFFFFF";
    public const string BorderDarkHex = "#00A859";
}

/// <summary>
/// Resolve as cores Bling para controles construídos programaticamente.
/// </summary>
internal static class ThemeColor
{
    public static Color Get(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var resource) == true)
        {
            if (resource is Color color)
                return color;
            if (resource is SolidColorBrush brush)
                return brush.Color;
        }

        var app = Application.Current;
        var dark = app?.UserAppTheme == AppTheme.Dark ||
                   (app?.UserAppTheme == AppTheme.Unspecified && app.RequestedTheme == AppTheme.Dark);
        var hex = key switch
        {
            "BlingHeader" => dark ? BlingPalette.HeaderDarkDarkHex : BlingPalette.HeaderDarkHex,
            "BlingPrimary" => BlingPalette.PrimaryHex,
            "BlingPrimaryHover" => BlingPalette.PrimaryHoverHex,
            "BlingBackground" => dark ? BlingPalette.BackgroundDarkHex : BlingPalette.AppBackgroundHex,
            "BlingCard" => dark ? BlingPalette.CardDarkHex : BlingPalette.CardBackgroundHex,
            "BlingText" => dark ? BlingPalette.TextDarkDarkHex : BlingPalette.TextDarkHex,
            "BlingTextMuted" => dark ? BlingPalette.TextMutedDarkHex : BlingPalette.TextMutedHex,
            "BlingTextLight" => BlingPalette.TextLightHex,
            "BlingBorder" => dark ? BlingPalette.BorderDarkHex : BlingPalette.BorderColorHex,
            _ => dark ? BlingPalette.TextDarkDarkHex : BlingPalette.TextDarkHex
        };

        return Color.FromArgb(hex);
    }

    public static Color Parse(string? value, string fallbackKey = "BlingPrimary")
    {
        var normalized = value?.Trim().ToUpperInvariant();
        return normalized switch
        {
            BlingPalette.HeaderDarkHex => Get("BlingHeader"),
            BlingPalette.PrimaryHex => Get("BlingPrimary"),
            BlingPalette.PrimaryHoverHex => Get("BlingPrimaryHover"),
            BlingPalette.HeaderDarkDarkHex => Get("BlingHeader"),
            BlingPalette.BackgroundDarkHex => Get("BlingBackground"),
            BlingPalette.CardDarkHex => Get("BlingCard"),
            BlingPalette.TextLightHex => Get("BlingText"),
            _ => Get(fallbackKey)
        };
    }
}
