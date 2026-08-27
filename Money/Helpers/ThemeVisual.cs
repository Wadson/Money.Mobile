namespace Money.Helpers;

public enum VisualRole { CommonText, MutedText, Selector, StandardCard, PositiveCard, CriticalCard, WarningCard }

/// <summary>Único ponto para aplicar cores semânticas a controles criados ou ajustados em código.</summary>
public static class ThemeVisual
{
    public static void Apply(VisualElement element, VisualRole role)
    {
        var lightText = Color.FromArgb(BlingPalette.TextDarkHex);
        var darkText = Color.FromArgb(BlingPalette.TextDarkDarkHex);
        if (element is Label label)
            label.SetAppThemeColor(Label.TextColorProperty, lightText, darkText);
        if (element is Button button)
        {
            button.SetAppThemeColor(Button.TextColorProperty, lightText, darkText);
            if (role == VisualRole.Selector)
                button.SetAppThemeColor(Button.BackgroundColorProperty, Colors.Transparent, Colors.Transparent);
        }
        if (element is Border card)
        {
            var lightBackground = role switch
            {
                VisualRole.PositiveCard => "#ECFDF3", VisualRole.CriticalCard => "#FEF2F2",
                VisualRole.WarningCard => "#FFF7E6", _ => BlingPalette.CardBackgroundHex
            };
            var darkBackground = role switch
            {
                VisualRole.PositiveCard => "#123524", VisualRole.CriticalCard => "#3B1717",
                VisualRole.WarningCard => "#3A2A12", _ => BlingPalette.CardDarkHex
            };
            card.SetAppThemeColor(Border.BackgroundColorProperty,
                Color.FromArgb(lightBackground), Color.FromArgb(darkBackground));
        }
    }
}
