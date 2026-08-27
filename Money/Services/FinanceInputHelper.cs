using System.Globalization;

namespace Money.Services;

public static class FinanceInputHelper
{
    public static CultureInfo BrazilianCulture { get; } = CultureInfo.GetCultureInfo("pt-BR");

    public static bool TryParsePositiveAmount(string? text, out decimal amount) =>
        decimal.TryParse(text, NumberStyles.Currency, BrazilianCulture, out amount) && amount > 0;

    public static string Currency(decimal value) => value.ToString("C2", BrazilianCulture);

    public static string Date(DateTime value) => value.ToString("dd/MM/yyyy", BrazilianCulture);
}
