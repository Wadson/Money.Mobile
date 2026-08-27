using System.Globalization;
using Money.Models;
#if !ANDROID
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QContainer = QuestPDF.Infrastructure.IContainer;
#endif

namespace Money.Services;

public sealed record ReportPdfResult(string Path, string FileName, int ItemCount,
    decimal Income, decimal PaidExpenses, decimal PendingExpenses, decimal Balance)
{
    public decimal Expenses => PaidExpenses + PendingExpenses;
}

public sealed class ReportPdfService(DatabaseService database)
{
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<ReportPdfResult> GenerateAsync(int month, int year, long? categoryId,
        string? categoryName, string paymentStatus = "todas", long? supplierId = null,
        string? supplierName = null, long? cardId = null, string? cardName = null)
    {
        // Inicialização tardia: evita carregar o mecanismo nativo de PDF durante
        // a abertura do aplicativo, especialmente no Android.
        var items = await database.GetFinancialReportItemsAsync(
            month, year, categoryId, paymentStatus, supplierId, cardId);
        var income = items.Where(x => x.Type == "receita" && x.Paid).Sum(x => x.Amount);
        var paidExpenses = items.Where(x => x.Type == "despesa" && x.Paid).Sum(x => x.Amount);
        var pendingExpenses = items.Where(x => x.Type == "despesa" && !x.Paid).Sum(x => x.Amount);
        var balance = income - paidExpenses;
        var period = new DateTime(year, month, 1).ToString("MMMM 'de' yyyy", _culture);
        var fileName = $"money-relatorio-{year:D4}-{month:D2}" +
                       (categoryId is null ? "" : $"-{SafeFileName(categoryName)}") +
                       (supplierId is null ? "" : $"-{SafeFileName(supplierName)}") +
                       (cardId is null ? "" : $"-{SafeFileName(cardName)}") +
                       $"-{paymentStatus}.pdf";
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);

        var statusLabel = paymentStatus switch
        {
            "abertas" => "Contas abertas",
            "pagas" => "Contas pagas",
            _ => "Contas abertas e pagas"
        };
        var filterLabel = BuildFilterLabel(categoryName, supplierName, cardName);
#if ANDROID
        await Money.Platforms.Android.ProfessionalReportPdfRenderer.GenerateAsync(
            path, items, period, filterLabel, statusLabel, income, paidExpenses, pendingExpenses, balance);
#else
        QuestPDF.Settings.License = LicenseType.Community;
        await Task.Run(() => CreateDocument(items, period, filterLabel, statusLabel,
            income, paidExpenses, pendingExpenses, balance).GeneratePdf(path));
#endif
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
            throw new IOException("O arquivo PDF não foi criado corretamente.");
        return new(path, fileName, items.Count, income, paidExpenses, pendingExpenses, balance);
    }

    private static string BuildFilterLabel(string? categoryName, string? supplierName, string? cardName)
    {
        var filters = new List<string> { categoryName ?? "Todas as categorias" };
        if (supplierName is not null) filters.Add($"Fornecedor: {supplierName}");
        if (cardName is not null) filters.Add($"Cartão: {cardName}");
        return string.Join(" | ", filters);
    }

    public async Task<string> SaveAsync(ReportPdfResult report)
    {
        ValidateReportFile(report);
#if WINDOWS
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");
        var window = Application.Current!.Windows[0].Handler!.PlatformView as Microsoft.UI.Xaml.Window;
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(window));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) throw new OperationCanceledException("Exportação cancelada.");
        var destination = Path.Combine(folder.Path, report.FileName);
        File.Copy(report.Path, destination, true);
        return destination;
#elif ANDROID
        return await Money.Platforms.Android.AndroidBackupFileService.SaveAsync(
            report.Path, report.FileName, "application/pdf");
#else
        await ShareAsync(report);
        return report.Path;
#endif
    }

    public Task ShareAsync(ReportPdfResult report)
    {
        ValidateReportFile(report);
        return Share.Default.RequestAsync(new ShareFileRequest("Relatório financeiro - Money Pro",
            new ShareFile(report.Path, "application/pdf")));
    }

    public async Task<string> SaveAndOpenAsync(ReportPdfResult report)
    {
        var destination = await SaveAsync(report);
#if ANDROID
        await Money.Platforms.Android.AndroidBackupFileService.OpenPdfAsync(destination);
#elif WINDOWS
        await Launcher.Default.OpenAsync(new OpenFileRequest(
            "Abrir relatório financeiro", new ReadOnlyFile(destination, "application/pdf")));
#endif
        return destination;
    }

    private static void ValidateReportFile(ReportPdfResult report)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (string.IsNullOrWhiteSpace(report.Path) || !File.Exists(report.Path) ||
            new FileInfo(report.Path).Length == 0)
            throw new FileNotFoundException("O arquivo PDF não está mais disponível. Gere o relatório novamente.",
                report.Path);
    }

#if !ANDROID
    public IDocument CreateDocument(IReadOnlyList<FinancialReportItem> items, string period,
        string? categoryName, string statusLabel, decimal income, decimal paidExpenses,
        decimal pendingExpenses, decimal balance) =>
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(BlingPalette.TextDarkHex));
                page.Header().Element(c => ComposeHeader(c, period, categoryName, statusLabel));
                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(14);
                    column.Item().Element(c => ComposeSummary(c, income, paidExpenses, pendingExpenses, balance));
                    column.Item().Element(c => ComposeTransactions(c, items));
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Money Pro - página ").FontColor(BlingPalette.TextDarkHex);
                    text.CurrentPageNumber().FontColor(BlingPalette.PrimaryHex).SemiBold();
                    text.Span(" de ").FontColor(BlingPalette.TextDarkHex);
                    text.TotalPages().FontColor(BlingPalette.PrimaryHex).SemiBold();
                });
            });
        });

    private void ComposeHeader(QContainer container, string period, string? categoryName, string statusLabel)
    {
        container.Background(BlingPalette.HeaderDarkHex).Padding(18).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("MONEY PRO").FontSize(18).Bold().FontColor(BlingPalette.CardBackgroundHex);
                column.Item().Text("Relatório de lançamentos financeiros")
                    .FontSize(10).FontColor(BlingPalette.CardBackgroundHex);
            });
            row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().AlignRight().Text(_culture.TextInfo.ToTitleCase(period))
                    .FontSize(12).SemiBold().FontColor(BlingPalette.CardBackgroundHex);
                column.Item().AlignRight().Text(categoryName ?? "Todas as categorias")
                    .FontSize(9).FontColor(BlingPalette.CardBackgroundHex);
                column.Item().AlignRight().Text(statusLabel).FontSize(8).FontColor(BlingPalette.CardBackgroundHex);
                column.Item().AlignRight().Text($"Emitido em {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(BlingPalette.CardBackgroundHex);
            });
        });
    }

    private void ComposeSummary(QContainer container, decimal income, decimal paidExpenses,
        decimal pendingExpenses, decimal balance)
    {
        container.Row(row =>
        {
            row.Spacing(8);
            row.RelativeItem().Element(c => SummaryCard(c, "RECEITAS", income, BlingPalette.CardBackgroundHex, BlingPalette.PrimaryHex));
            row.RelativeItem().Element(c => SummaryCard(c, "DESP. PAGAS", paidExpenses, BlingPalette.CardBackgroundHex, BlingPalette.HeaderDarkHex));
            row.RelativeItem().Element(c => SummaryCard(c, "DESP. PENDENTES", pendingExpenses, BlingPalette.CardBackgroundHex, BlingPalette.HeaderDarkHex));
            row.RelativeItem().Element(c => SummaryCard(c, "SALDO", balance, BlingPalette.CardBackgroundHex, BlingPalette.PrimaryHex));
        });
    }

    private void SummaryCard(QContainer container, string title, decimal value, string background, string color)
    {
        container.Background(background).Border(1).BorderColor(color).Padding(12).Column(column =>
        {
            column.Item().Text(title).FontSize(8).SemiBold().FontColor(color);
            column.Item().Text(value.ToString("C2", _culture)).FontSize(14).Bold().FontColor(color);
        });
    }

    private void ComposeTransactions(QContainer container, IReadOnlyList<FinancialReportItem> items)
    {
        if (items.Count == 0)
        {
            container.Background(BlingPalette.CardBackgroundHex).Border(1).BorderColor(BlingPalette.TextDarkHex).Padding(24)
                .AlignCenter().Text("Nenhum lançamento encontrado para os filtros selecionados.")
                .FontColor(BlingPalette.TextDarkHex).SemiBold();
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(66);
                columns.RelativeColumn(2.4f);
                columns.RelativeColumn(1.35f);
                columns.RelativeColumn(1.1f);
                columns.ConstantColumn(72);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell()).Text("DATA");
                HeaderCell(header.Cell()).Text("DESCRIÇÃO");
                HeaderCell(header.Cell()).Text("CATEGORIA");
                HeaderCell(header.Cell()).Text("STATUS");
                HeaderCell(header.Cell()).AlignRight().Text("VALOR");
            });

            foreach (var group in items.GroupBy(x => x.Category))
            {
                table.Cell().ColumnSpan(5).Background(BlingPalette.CardBackgroundHex).BorderBottom(1)
                    .BorderColor(BlingPalette.PrimaryHex).Padding(7)
                    .Text($"{group.Key} - {group.Count()} lançamento(s)")
                    .SemiBold().FontColor(BlingPalette.PrimaryHex);

                foreach (var item in group)
                {
                    BodyCell(table.Cell()).Text(item.Date.ToString("dd/MM/yyyy"));
                    BodyCell(table.Cell()).Text(item.Description);
                    BodyCell(table.Cell()).Text(item.Category);
                    BodyCell(table.Cell()).Text(item.Paid ? "Realizado" : "Pendente")
                        .FontColor(item.Paid ? BlingPalette.PrimaryHex : BlingPalette.HeaderDarkHex);
                    BodyCell(table.Cell()).AlignRight().Text(item.Amount.ToString("C2", _culture))
                        .SemiBold().FontColor(item.Type == "receita" ? BlingPalette.PrimaryHex : BlingPalette.HeaderDarkHex);
                }

                var categoryIncome = group.Where(x => x.Type == "receita").Sum(x => x.Amount);
                var categoryExpenses = group.Where(x => x.Type == "despesa").Sum(x => x.Amount);
                table.Cell().ColumnSpan(4).Background(BlingPalette.CardBackgroundHex).Padding(6)
                    .AlignRight().Text("Saldo da categoria").SemiBold().FontColor(BlingPalette.TextDarkHex);
                table.Cell().Background(BlingPalette.CardBackgroundHex).Padding(6).AlignRight()
                    .Text((categoryIncome - categoryExpenses).ToString("C2", _culture))
                    .Bold().FontColor(BlingPalette.PrimaryHex);
            }
        });
    }

    private static QContainer HeaderCell(QContainer container) => container
        .Background(BlingPalette.HeaderDarkHex).PaddingVertical(7).PaddingHorizontal(5)
        .DefaultTextStyle(x => x.FontSize(8).SemiBold().FontColor(BlingPalette.CardBackgroundHex));

    private static QContainer BodyCell(QContainer container) => container
        .BorderBottom(1).BorderColor(BlingPalette.BorderColorHex).PaddingVertical(6).PaddingHorizontal(5);
#endif

    private static string SafeFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "categoria";
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(value.ToLowerInvariant().Select(c => invalid.Contains(c) || char.IsWhiteSpace(c) ? '-' : c))
            .Trim('-');
    }
}
