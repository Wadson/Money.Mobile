using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Money.Models;

namespace Money.Services;

internal static partial class CardInvoiceCsvParser
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static List<CardCsvPurchase> Parse(string content)
    {
        var lines = content.TrimStart('\uFEFF').Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        char? declaredSeparator = null;
        if (lines.Count > 0 && lines[0].Trim().StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
        {
            var declaration = lines[0].Trim();
            if (declaration.Length > 4) declaredSeparator = declaration[4];
            lines.RemoveAt(0);
        }
        if (lines.Count == 0) throw new InvalidDataException("O CSV não contém lançamentos.");
        var separator = declaredSeparator ?? DetectSeparator(lines[0]);
        var headers = Split(lines[0], separator).Select(Normalize).ToArray();
        var dateIndex = FindHeader(headers, "data", "date", "data compra", "data da compra",
            "data lancamento", "data da transacao", "dt compra", "purchase date", "transaction date");
        var descriptionIndex = FindHeader(headers, "descricao", "description", "title", "titulo", "estabelecimento", "historico", "lancamento");
        var valueIndex = FindHeader(headers, "valor", "value", "amount", "valor compra", "total");
        var firstDueDateIndex = FindHeader(headers, "data primeiro vencimento", "data do primeiro vencimento",
            "dataprimeirovencimento", "primeiro vencimento", "first due date");
        var firstCells = Split(lines[0], separator);
        var headerless = TryDate(firstCells.FirstOrDefault(), out _) || LooseLineRegex().IsMatch(lines[0]);
        var dataStart = headerless ? 0 : 1;
        if (!headerless && (dateIndex < 0 || descriptionIndex < 0 || valueIndex < 0 || firstDueDateIndex < 0))
        {
            var detected = string.Join(", ", headers.Where(x => !string.IsNullOrWhiteSpace(x)));
            throw new InvalidDataException(
                $"O arquivo precisa identificar Data, Descrição, Valor e DataPrimeiroVencimento. Colunas detectadas: {detected}.");
        }
        if (headerless)
        {
            dateIndex = 0; descriptionIndex = 1; valueIndex = 2; firstDueDateIndex = 3;
            if (firstCells.Length < 4)
                throw new InvalidDataException(
                    "Informe a coluna DataPrimeiroVencimento. Baixe o novo modelo CSV para preencher a data da parcela 1.");
        }
        var result = new List<CardCsvPurchase>();
        for (var i = dataStart; i < lines.Count; i++)
        {
            var row = Split(lines[i], separator);
            string rawDate;
            string rawDescription;
            string rawValue;
            string rawFirstDueDate;
            var loose = LooseLineRegex().Match(lines[i]);
            if (row.Length < 4 && loose.Success)
            {
                throw new InvalidDataException(
                    $"DataPrimeiroVencimento não informada na linha {i + 1}. Use o novo modelo CSV.");
            }
            else
            {
                rawDate = Cell(row, dateIndex);
                rawDescription = Cell(row, descriptionIndex);
                rawValue = Cell(row, valueIndex);
                rawFirstDueDate = Cell(row, firstDueDateIndex);
            }
            if (!TryDate(rawDate, out var date))
                throw new InvalidDataException($"Data inválida na linha {i + 1}.");
            if (!TryDate(rawFirstDueDate, out var firstDueDate))
                throw new InvalidDataException($"Data do primeiro vencimento inválida na linha {i + 1}.");
            rawValue = rawValue.Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (!decimal.TryParse(rawValue, NumberStyles.Number, PtBr, out var amount) &&
                !decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) || amount == 0)
                throw new InvalidDataException($"Valor inválido na linha {i + 1}.");
            var match = InstallmentRegex().Match(rawDescription);
            var current = match.Success ? int.Parse(match.Groups[1].Value) : 1;
            var total = match.Success ? int.Parse(match.Groups[2].Value) : 1;
            if (current < 1 || total < current || total > 120)
                throw new InvalidDataException($"Parcelamento inválido na linha {i + 1}.");
            var clean = match.Success ? InstallmentRegex().Replace(rawDescription, "").Trim(' ', '-', '(', ')') : rawDescription.Trim();
            if (string.IsNullOrWhiteSpace(clean)) throw new InvalidDataException($"Descrição inválida na linha {i + 1}.");
            result.Add(new(i + 1, date, clean, Math.Abs(amount), current, total,
                date.AddMonths(-(current - 1)), firstDueDate.Date));
        }
        return result;
    }

    private static int FindHeader(IReadOnlyList<string> headers, params string[] aliases)
    {
        var normalizedAliases = aliases.Select(Normalize).ToHashSet();
        for (var i = 0; i < headers.Count; i++)
            if (normalizedAliases.Contains(headers[i])) return i;
        return -1;
    }

    private static char DetectSeparator(string firstLine)
    {
        var candidates = new[] { ';', '\t', '|', ',' };
        return candidates.OrderByDescending(separator => firstLine.Count(character => character == separator))
            .First();
    }

    private static string Cell(IReadOnlyList<string> row, int index) =>
        index >= 0 && index < row.Count ? row[index].Trim() : "";

    private static bool TryDate(string? value, out DateTime date) =>
        DateTime.TryParseExact(value?.Trim(), ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy"],
            PtBr, DateTimeStyles.None, out date);

    public static List<CardInstallmentImport> Expand(IEnumerable<CardCsvPurchase> purchases, CardItem card)
    {
        var result = new List<CardInstallmentImport>();
        foreach (var purchase in purchases)
            for (var number = 1; number <= purchase.TotalInstallments; number++)
            {
                var date = purchase.FirstDate.AddMonths(number - 1);
                var due = purchase.FirstDueDate.AddMonths(number - 1);
                var reference = new DateTime(due.Year, due.Month, 1);
                var status = reference < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) ? "fechada" : "aberta";
                result.Add(new($"{purchase.Description} ({number}/{purchase.TotalInstallments})",
                    purchase.InstallmentAmount, date, number, purchase.TotalInstallments,
                    reference.Month, reference.Year, due, status));
            }
        return result;
    }

    private static string[] Split(string value, char separator) => Regex.Split(value,
        $"{separator}(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)").Select(x => x.Trim().Trim('"')).ToArray();
    private static string Normalize(string value) => new(value.Trim().TrimStart('\uFEFF').ToLowerInvariant()
        .Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());

    [GeneratedRegex(@"(?:\b(?:parcela|parc)\s*)?\(?\s*(\d{1,3})\s*(?:de|/)\s*(\d{1,3})\s*\)?", RegexOptions.IgnoreCase)]
    private static partial Regex InstallmentRegex();

    [GeneratedRegex(@"^\s*(?<date>\d{1,4}[-/]\d{1,2}[-/]\d{1,4})\s*[;\s]+""?(?<description>.+?)""?\s*[;\s]+(?:R\$\s*)?(?<value>-?[\d.]+(?:,\d{1,2})?)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex LooseLineRegex();
}
