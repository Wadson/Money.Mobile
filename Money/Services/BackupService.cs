
namespace Money.Services;

public enum BackupDestination { Folder, Share }

public sealed class BackupService(DatabaseService database, AuthService auth)
{
    public async Task<string> CreateBackupAsync(BackupDestination? selectedDestination = null)
    {
        var fileName = $"moneypro-backup-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.db";

#if WINDOWS
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");
        var window = Application.Current!.Windows[0].Handler!.PlatformView as Microsoft.UI.Xaml.Window;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) throw new OperationCanceledException("Backup cancelado.");
        var destination = Path.Combine(folder.Path, fileName);
        await new SqliteBackupStore(database).CreateAsync(destination);
        await database.RecordBackupAsync(fileName, destination, new FileInfo(destination).Length);
        return destination;
#else
        var destination = Path.Combine(FileSystem.CacheDirectory, fileName);
        await new SqliteBackupStore(database).CreateAsync(destination);

#if ANDROID
        // Adicionados ícones/glifos modernos nas opções do menu
        var action = selectedDestination switch
        {
            BackupDestination.Folder => "Salvar em uma pasta",
            BackupDestination.Share => "Compartilhar (WhatsApp, Telegram...)",
            _ => await Application.Current!.Windows[0].Page!.DisplayActionSheetAsync(
                "Backup do Money Pro", "Cancelar", null, "Salvar em uma pasta",
                "Compartilhar (WhatsApp, Telegram...)")
        };

        if (action == "Salvar em uma pasta")
        {
            var saved = await Money.Platforms.Android.AndroidBackupFileService.SaveAsync(destination, fileName);
            await database.RecordBackupAsync(fileName, saved, new FileInfo(destination).Length);
            return saved;
        }
        if (action == "Compartilhar (WhatsApp, Telegram...)")
        {
            await Share.Default.RequestAsync(new ShareFileRequest(
                "Backup do Money Pro", new ShareFile(destination, "application/octet-stream")));
            await database.RecordBackupAsync(fileName, destination, new FileInfo(destination).Length);
            return destination;
        }
        throw new OperationCanceledException("Backup cancelado.");
#else
        await Share.Default.RequestAsync(new ShareFileRequest(
            "Salvar ou compartilhar backup do Money Pro", new ShareFile(destination)));
        await database.RecordBackupAsync(fileName, destination, new FileInfo(destination).Length);
        return destination;
#endif
#endif
    }

    public async Task<BackupRestoreResult> RestoreBackupAsync()
    {
        var selected = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecione um backup do Money Pro",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".db"],
                [DevicePlatform.Android] = ["application/octet-stream", "application/x-sqlite3", "application/vnd.sqlite3", "application/x-sqlite", "*/*"]
            })
        });
        if (selected is null)
            throw new OperationCanceledException("Restauração cancelada.");
        var importPath = Path.Combine(FileSystem.CacheDirectory, $"moneypro-restore-{Guid.NewGuid():N}.db");
        try
        {
            await using (var source = await selected.OpenReadAsync())
            await using (var destination = File.Create(importPath))
                await source.CopyToAsync(destination);
            var result = await new SqliteBackupStore(database).RestoreAsync(importPath);
            auth.Logout();
            return result;
        }
        finally
        {
            try { File.Delete(importPath); } catch (IOException) { }
        }
    }
}
