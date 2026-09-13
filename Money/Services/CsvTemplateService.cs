using System.Text;

namespace Money.Services;

internal static class CsvTemplateService
{
    public static Task<string> SaveCardInvoiceTemplateAsync() => SaveAsync(
        "modelo-importacao-fatura-cartao.csv",
        "Data;Descricao;Valor;DataPrimeiroVencimento\r\n" +
        "01/01/2026;GRAN EDUCACAO Parcela 7 de 12;69,90;10/07/2025\r\n" +
        "05/01/2026;SUPERMERCADO;245,80;10/02/2026\r\n" +
        "10/01/2026;NOTEBOOK (03/10);350,00;10/11/2025\r\n");

    public static Task<string> SavePayablesTemplateAsync() => SaveAsync(
        "modelo-importacao-contas-a-pagar.csv",
        "Descricao;Valor;DataVencimento;Categoria;Subcategoria;Fornecedor;Observacoes\r\n" +
        "Calça Jeans;120,50;15/08/2026;Compras;Roupas;Loja Z;Compra parcelada\r\n" +
        "Internet Fibra;99,90;20/08/2026;Casa/Moradia;Internet;Provedor X;Mensalidade\r\n");

    private static async Task<string> SaveAsync(string fileName, string content)
    {
        var source = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(source, content, new UTF8Encoding(true));
#if WINDOWS
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");
        var window = Application.Current!.Windows[0].Handler!.PlatformView as Microsoft.UI.Xaml.Window;
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(window));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) throw new OperationCanceledException();
        var destination = Path.Combine(folder.Path, fileName);
        File.Copy(source, destination, true);
        return destination;
#elif ANDROID
        return await Money.Platforms.Android.AndroidBackupFileService.SaveAsync(
            source, fileName, "text/csv");
#else
        await Share.Default.RequestAsync(new ShareFileRequest("Modelo CSV - Money Pro",
            new ShareFile(source)));
        return source;
#endif
    }
}
