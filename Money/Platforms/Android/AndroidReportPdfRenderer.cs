#if ANDROID
using Android.Graphics;
using Android.Graphics.Pdf;
using Money.Models;
using System.Globalization;
using Color = Android.Graphics.Color;
using Paint = Android.Graphics.Paint;
using Path = System.IO.Path;

namespace Money.Platforms.Android;

internal static class AndroidReportPdfRenderer
{
    private const int PageWidth = 595;
    private const int PageHeight = 842;
    private const float Margin = 30;

    public static Task GenerateAsync(string path, IReadOnlyList<FinancialReportItem> items,
        string period, string? categoryName, string statusLabel,
        decimal income, decimal paidExpenses, decimal pendingExpenses, decimal balance) => Task.Run(() =>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        items ??= Array.Empty<FinancialReportItem>();
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        using var document = new PdfDocument();
        using var paint = new Paint(PaintFlags.AntiAlias);
        var pageNumber = 0;
        PdfDocument.Page? page = null;
        Canvas? canvas = null;
        float y = 0;

        void Text(string value, float x, float baseline, float size, string color,
            Paint.Align? align = null, bool bold = false)
        {
            paint.TextSize = size;
            paint.Color = Color.ParseColor(color);
            paint.TextAlign = align ?? Paint.Align.Left;
            paint.SetTypeface(bold ? Typeface.DefaultBold : Typeface.Default);
            canvas!.DrawText(value, x, baseline, paint);
        }

        void Rectangle(float left, float top, float right, float bottom, string color,
            Paint.Style? style = null, float stroke = 1)
        {
            paint.Color = Color.ParseColor(color);
            paint.SetStyle(style ?? Paint.Style.Fill);
            paint.StrokeWidth = stroke;
            canvas!.DrawRect(left, top, right, bottom, paint);
            paint.SetStyle(Paint.Style.Fill);
        }

        void FinishPage()
        {
            if (page is null) return;
            Text($"Money Pro - página {pageNumber}", PageWidth / 2f, PageHeight - 18,
                8, BlingPalette.TextDarkHex, Paint.Align.Center);
            document.FinishPage(page);
            page = null;
        }

        void StartPage()
        {
            FinishPage();
            pageNumber++;
            page = document.StartPage(new PdfDocument.PageInfo.Builder(PageWidth, PageHeight, pageNumber).Create());
            canvas = page!.Canvas;
            Rectangle(Margin, Margin, PageWidth - Margin, 108, BlingPalette.HeaderDarkHex);
            Text("MONEY PRO", Margin + 18, 65, 18, BlingPalette.CardBackgroundHex, bold: true);
            Text("Relatório de lançamentos financeiros", Margin + 18, 84, 10, BlingPalette.CardBackgroundHex);
            Text(culture.TextInfo.ToTitleCase(period), PageWidth - Margin - 18, 58, 11,
                BlingPalette.CardBackgroundHex, Paint.Align.Right, true);
            Text(categoryName ?? "Todas as categorias", PageWidth - Margin - 18, 76, 8,
                BlingPalette.CardBackgroundHex, Paint.Align.Right);
            Text(statusLabel, PageWidth - Margin - 18, 92, 8,
                BlingPalette.CardBackgroundHex, Paint.Align.Right);
            y = 126;
        }

        void SummaryCard(float left, string title, decimal value, string background, string color)
        {
            var width = (PageWidth - Margin * 2 - 16) / 3;
            Rectangle(left, y, left + width, y + 58, background);
            Rectangle(left, y, left + width, y + 58, color, Paint.Style.Stroke, 1);
            Text(title, left + 10, y + 20, 8, color, bold: true);
            Text(value.ToString("C2", culture), left + 10, y + 43, 13, color, bold: true);
        }

        void TableHeader()
        {
            Rectangle(Margin, y, PageWidth - Margin, y + 30, BlingPalette.HeaderDarkHex);
            Text("DATA", Margin + 6, y + 20, 8, BlingPalette.CardBackgroundHex, bold: true);
            Text("DESCRIÇÃO", Margin + 78, y + 20, 8, BlingPalette.CardBackgroundHex, bold: true);
            Text("CATEGORIA", Margin + 270, y + 20, 8, BlingPalette.CardBackgroundHex, bold: true);
            Text("STATUS", Margin + 390, y + 20, 8, BlingPalette.CardBackgroundHex, bold: true);
            Text("VALOR", PageWidth - Margin - 6, y + 20, 8, BlingPalette.CardBackgroundHex, Paint.Align.Right, true);
            y += 30;
        }

        void EnsureSpace(float required)
        {
            if (y + required <= PageHeight - 40) return;
            StartPage();
            TableHeader();
        }

        StartPage();
        SummaryCard(Margin, "RECEITAS", income, BlingPalette.CardBackgroundHex, BlingPalette.PrimaryHex);
        var cardWidth = (PageWidth - Margin * 2 - 16) / 3;
        SummaryCard(Margin + cardWidth + 8, "DESP. PAGAS", paidExpenses, BlingPalette.CardBackgroundHex, BlingPalette.HeaderDarkHex);
        SummaryCard(Margin + (cardWidth + 8) * 2, "PENDENTES", pendingExpenses, BlingPalette.CardBackgroundHex, BlingPalette.HeaderDarkHex);
        y += 74;
        Text($"SALDO REALIZADO: {balance.ToString("C2", culture)}", PageWidth - Margin, y - 8,
            9, BlingPalette.PrimaryHex, Paint.Align.Right, true);
        TableHeader();

        if (items.Count == 0)
        {
            Rectangle(Margin, y, PageWidth - Margin, y + 54, BlingPalette.CardBackgroundHex);
            Text("Nenhum lançamento encontrado para os filtros selecionados.",
                PageWidth / 2f, y + 31, 10, BlingPalette.TextDarkHex, Paint.Align.Center, true);
        }
        else
        {
            foreach (var group in items.GroupBy(x => x.Category))
            {
                EnsureSpace(60);
                Rectangle(Margin, y, PageWidth - Margin, y + 27, BlingPalette.CardBackgroundHex);
                Text($"{group.Key} - {group.Count()} lançamento(s)", Margin + 7, y + 18,
                    9, BlingPalette.PrimaryHex, bold: true);
                y += 27;
                foreach (var item in group)
                {
                    EnsureSpace(31);
                    Text(item.Date.ToString("dd/MM/yyyy"), Margin + 5, y + 19, 8, BlingPalette.TextDarkHex);
                    var description = item.Description.Length > 30 ? item.Description[..27] + "..." : item.Description;
                    Text(description, Margin + 78, y + 19, 8, BlingPalette.TextDarkHex);
                    var category = item.Category.Length > 18 ? item.Category[..15] + "..." : item.Category;
                    Text(category, Margin + 270, y + 19, 8, BlingPalette.TextDarkHex);
                    var incomeItem = item.Type == "receita";
                    Text(item.Paid ? "Realizado" : "Pendente", Margin + 390, y + 19, 8,
                        item.Paid ? BlingPalette.PrimaryHex : BlingPalette.HeaderDarkHex);
                    Text(item.Amount.ToString("C2", culture), PageWidth - Margin - 5, y + 19, 8,
                        incomeItem ? BlingPalette.PrimaryHex : BlingPalette.HeaderDarkHex, Paint.Align.Right, true);
                    Rectangle(Margin, y + 29, PageWidth - Margin, y + 30, BlingPalette.BorderColorHex);
                    y += 30;
                }
                var subtotal = group.Sum(x => x.Type == "receita" ? x.Amount : -x.Amount);
                EnsureSpace(28);
                Rectangle(Margin, y, PageWidth - Margin, y + 27, BlingPalette.CardBackgroundHex);
                Text("Saldo da categoria", PageWidth - Margin - 95, y + 18, 8, BlingPalette.TextDarkHex,
                    Paint.Align.Right, true);
                Text(subtotal.ToString("C2", culture), PageWidth - Margin - 5, y + 18, 8,
                    BlingPalette.PrimaryHex, Paint.Align.Right, true);
                y += 27;
            }
        }

        FinishPage();
        var directory = Path.GetDirectoryName(path)
            ?? throw new IOException("A pasta temporária do relatório é inválida.");
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write,
                       FileShare.None, 64 * 1024, FileOptions.SequentialScan))
            {
                document.WriteTo(stream);
                stream.Flush(true);
            }
            File.Move(temporaryPath, path, true);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException("O Android não permitiu gravar o arquivo temporário do PDF.", ex);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    });
}
#endif
