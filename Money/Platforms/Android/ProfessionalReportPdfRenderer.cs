#if ANDROID
using Android.Graphics;
using Android.Graphics.Pdf;
using Money.Models;
using System.Globalization;
using Color = Android.Graphics.Color;
using Paint = Android.Graphics.Paint;

namespace Money.Platforms.Android;

internal static class ProfessionalReportPdfRenderer
{
    private const int Width = 595;
    private const int Height = 842;
    private const float Margin = 30;

    public static Task GenerateAsync(string path, IReadOnlyList<FinancialReportItem> items,
        string period, string? filters, string statusFilter, decimal income, decimal paid,
        decimal pending, decimal balance) => Task.Run(() =>
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        using var document = new PdfDocument();
        using var paint = new Paint(PaintFlags.AntiAlias);
        PdfDocument.Page? page = null;
        Canvas? canvas = null;
        var pageNumber = 0;
        float y = 0;

        void Text(string text, float x, float baseline, float size, string color,
            Paint.Align? align = null, bool bold = false)
        {
            paint.TextSize = size;
            paint.Color = Color.ParseColor(color);
            paint.TextAlign = align ?? Paint.Align.Left;
            paint.SetTypeface(bold ? Typeface.DefaultBold : Typeface.Default);
            canvas!.DrawText(text, x, baseline, paint);
        }

        void Box(float left, float top, float right, float bottom, string color,
            bool outline = false, float radius = 0, float stroke = 1)
        {
            paint.Color = Color.ParseColor(color);
            paint.SetStyle(outline ? Paint.Style.Stroke : Paint.Style.Fill);
            paint.StrokeWidth = stroke;
            if (radius > 0) canvas!.DrawRoundRect(left, top, right, bottom, radius, radius, paint);
            else canvas!.DrawRect(left, top, right, bottom, paint);
            paint.SetStyle(Paint.Style.Fill);
        }

        static string Cut(string text, int max) =>
            text.Length <= max ? text : text[..(max - 1)] + "…";

        void FinishPage()
        {
            if (page is null) return;
            Text($"Money Pro • página {pageNumber}", Width / 2f, Height - 17, 7,
                BlingPalette.TextDarkHex, Paint.Align.Center);
            document.FinishPage(page);
            page = null;
        }

        void StartPage()
        {
            FinishPage();
            pageNumber++;
            page = document.StartPage(new PdfDocument.PageInfo.Builder(Width, Height, pageNumber).Create());
            canvas = page?.Canvas ?? throw new InvalidOperationException("Não foi possível iniciar a página do relatório.");
            Box(Margin, Margin, Width - Margin, 106, BlingPalette.HeaderDarkHex);
            Text("MONEY PRO", Margin + 18, 62, 18, "#FFFFFF", bold: true);
            Text("Relatório financeiro detalhado", Margin + 18, 82, 9, "#FFFFFF");
            Text(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(period), Width - Margin - 18, 56,
                11, "#FFFFFF", Paint.Align.Right, true);
            Text(Cut(filters ?? "Todas as categorias", 44), Width - Margin - 18, 75,
                7.5f, "#FFFFFF", Paint.Align.Right);
            Text(statusFilter, Width - Margin - 18, 91, 7.5f, "#FFFFFF", Paint.Align.Right);
            y = 122;
        }

        void Summary(float left, string title, decimal value, string color)
        {
            var cardWidth = (Width - Margin * 2 - 16) / 3;
            Box(left, y, left + cardWidth, y + 50, "#FFFFFF", radius: 5);
            Box(left, y, left + cardWidth, y + 50, color, true, 5);
            Text(title, left + 9, y + 16, 6.8f, color, bold: true);
            Text(value.ToString("C2", culture), left + 9, y + 37, 11.5f, color, bold: true);
        }

        void TableHeader()
        {
            Box(Margin, y, Width - Margin, y + 24, BlingPalette.HeaderDarkHex);
            Text("DATA", Margin + 5, y + 16, 6.5f, "#FFFFFF", bold: true);
            Text("DESCRIÇÃO", Margin + 58, y + 16, 6.5f, "#FFFFFF", bold: true);
            Text("CONTA / CARTÃO", Margin + 210, y + 16, 6.5f, "#FFFFFF", bold: true);
            Text("PARC.", Margin + 315, y + 16, 6.5f, "#FFFFFF", bold: true);
            Text("STATUS", Margin + 358, y + 16, 6.5f, "#FFFFFF", bold: true);
            Text("VALOR", Width - Margin - 5, y + 16, 6.5f, "#FFFFFF", Paint.Align.Right, true);
            y += 24;
        }

        void Ensure(float required)
        {
            if (y + required <= Height - 35) return;
            StartPage();
            TableHeader();
        }

        StartPage();
        var cardWidth = (Width - Margin * 2 - 16) / 3;
        Summary(Margin, "RECEITAS RECEBIDAS", income, BlingPalette.PrimaryHex);
        Summary(Margin + cardWidth + 8, "DESPESAS PAGAS", paid, BlingPalette.HeaderDarkHex);
        Summary(Margin + (cardWidth + 8) * 2, "PENDENTES", pending, "#C62828");
        y += 60;

        var balanceColor = balance < 0 ? "#C62828" : BlingPalette.PrimaryHex;
        Box(Margin, y, Width - Margin, y + 44, balance < 0 ? "#FFF1F1" : "#EDF9F2", radius: 6);
        Box(Margin, y, Width - Margin, y + 44, balanceColor, true, 6, 1.5f);
        Text("SALDO REALIZADO", Margin + 13, y + 17, 8, balanceColor, bold: true);
        Text("Receitas recebidas menos despesas pagas", Margin + 13, y + 32, 6.8f,
            BlingPalette.TextDarkHex);
        Text(balance.ToString("C2", culture), Width - Margin - 13, y + 29, 16,
            balanceColor, Paint.Align.Right, true);
        y += 55;
        TableHeader();

        if (items.Count == 0)
        {
            Text("Nenhum lançamento encontrado.", Width / 2, y + 30, 9,
                BlingPalette.TextDarkHex, Paint.Align.Center, true);
        }

        foreach (var group in items.GroupBy(item => item.Category))
        {
            Ensure(45);
            Box(Margin, y, Width - Margin, y + 21, "#EDF9F2");
            Text($"{group.Key}  •  {group.Count()} lançamento(s)", Margin + 6, y + 14,
                7.5f, BlingPalette.PrimaryHex, bold: true);
            y += 21;

            foreach (var item in group)
            {
                Ensure(22);
                Text(item.Date.ToString("dd/MM/yy"), Margin + 4, y + 14, 6.8f, BlingPalette.TextDarkHex);
                Text(Cut(item.Description, 27), Margin + 58, y + 14, 7, BlingPalette.TextDarkHex);
                Text(Cut(item.Source, 18), Margin + 210, y + 14, 6.8f, BlingPalette.TextDarkHex);
                Text(item.TotalInstallments > 1 ? $"{item.InstallmentNumber}/{item.TotalInstallments}" : "—",
                    Margin + 315, y + 14, 6.8f, BlingPalette.TextDarkHex);
                Text(item.Paid ? "Realizado" : "Pendente", Margin + 358, y + 14, 6.8f,
                    item.Paid ? BlingPalette.PrimaryHex : "#C62828", bold: true);
                Text(item.Amount.ToString("C2", culture), Width - Margin - 4, y + 14, 7,
                    item.Type == "receita" ? BlingPalette.PrimaryHex : BlingPalette.HeaderDarkHex,
                    Paint.Align.Right, true);
                Box(Margin, y + 20.5f, Width - Margin, y + 21, BlingPalette.BorderColorHex);
                y += 21;
            }

            var subtotal = group.Sum(item => item.Type == "receita" ? item.Amount : -item.Amount);
            Ensure(22);
            Text("Subtotal", Width - Margin - 92, y + 14, 6.8f, BlingPalette.TextDarkHex,
                Paint.Align.Right, true);
            Text(subtotal.ToString("C2", culture), Width - Margin - 4, y + 14, 7,
                subtotal < 0 ? "#C62828" : BlingPalette.PrimaryHex, Paint.Align.Right, true);
            y += 22;
        }

        FinishPage();
        var directory = System.IO.Path.GetDirectoryName(path)
            ?? throw new IOException("Pasta temporária inválida.");
        Directory.CreateDirectory(directory);
        var temporary = System.IO.Path.Combine(directory, $".{System.IO.Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                document.WriteTo(stream);
                stream.Flush(true);
            }
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    });
}
#endif
