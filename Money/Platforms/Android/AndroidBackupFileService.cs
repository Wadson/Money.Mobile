#if ANDROID
using Android.App;
using Android.Content;

namespace Money.Platforms.Android;

internal static class AndroidBackupFileService
{
    internal const int SaveRequestCode = 7319;
    private static TaskCompletionSource<global::Android.Net.Uri?>? _completion;
    private static string? _sourcePath;

    public static async Task<string> SaveAsync(string sourcePath, string fileName,
        string mimeType = "application/octet-stream")
    {
        if (_completion is not null)
            throw new InvalidOperationException("Já existe uma escolha de pasta em andamento.");

        var activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("A tela Android não está disponível.");
        _sourcePath = sourcePath;
        _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var intent = new Intent(Intent.ActionCreateDocument);
        intent.AddCategory(Intent.CategoryOpenable);
        intent.SetType(mimeType);
        intent.PutExtra(Intent.ExtraTitle, fileName);
        activity.StartActivityForResult(intent, SaveRequestCode);

        var uri = await _completion.Task;
        return uri?.ToString() ?? throw new OperationCanceledException("Backup cancelado.");
    }

    public static async void Complete(Activity activity, Result resultCode, Intent? data)
    {
        var completion = _completion;
        var sourcePath = _sourcePath;
        _completion = null;
        _sourcePath = null;
        if (completion is null) return;
        if (resultCode != Result.Ok || data?.Data is null || sourcePath is null)
        {
            completion.TrySetResult(null);
            return;
        }

        try
        {
            await using (var source = File.OpenRead(sourcePath))
            await using (var destination = activity.ContentResolver!.OpenOutputStream(data.Data, "w")
                ?? throw new IOException("Não foi possível criar o arquivo na pasta escolhida."))
            {
                await source.CopyToAsync(destination);
                await destination.FlushAsync();
            }
            // Report success only after the document provider has closed the file.
            completion.TrySetResult(data.Data);
        }
        catch (Exception ex)
        {
            completion.TrySetException(ex);
        }
    }

    public static Task OpenPdfAsync(string contentUri)
    {
        var activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("A tela Android não está disponível.");
        var uri = global::Android.Net.Uri.Parse(contentUri)
            ?? throw new InvalidOperationException("O endereço do PDF salvo é inválido.");
        var intent = new Intent(Intent.ActionView);
        intent.SetDataAndType(uri, "application/pdf");
        intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);
        try
        {
            activity.StartActivity(Intent.CreateChooser(intent, "Abrir relatório PDF"));
            return Task.CompletedTask;
        }
        catch (ActivityNotFoundException)
        {
            throw new InvalidOperationException("Nenhum aplicativo leitor de PDF foi encontrado no dispositivo.");
        }
    }
}
#endif
