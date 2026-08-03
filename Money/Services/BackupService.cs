using Microsoft.Data.Sqlite;

namespace Money.Services;

public enum BackupDestination { Folder, Share }

public sealed class BackupService(DatabaseService database)
{
    public async Task<string> CreateBackupAsync(BackupDestination? selectedDestination = null)
    {
        await CheckpointAsync();
        var fileName = $"moneypro-backup-{DateTime.Now:yyyyMMdd-HHmmss}.db";

#if WINDOWS
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");
        var window = Application.Current!.Windows[0].Handler!.PlatformView as Microsoft.UI.Xaml.Window;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) throw new OperationCanceledException("Backup cancelado.");
        var destination = Path.Combine(folder.Path, fileName);
        File.Copy(database.DatabasePath, destination, false);
        await database.RecordBackupAsync(fileName, destination, new FileInfo(destination).Length);
        return destination;
#else
        var destination = Path.Combine(FileSystem.CacheDirectory, fileName);
        File.Copy(database.DatabasePath, destination, true);

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

    public async Task<string> RestoreBackupAsync()
    {
        var selected = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecione um backup do Money Pro",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".db"],
                [DevicePlatform.Android] = ["application/octet-stream", "application/x-sqlite3"]
            })
        });
        if (selected is null)
            throw new OperationCanceledException("Restauração cancelada.");
        var importPath = Path.Combine(FileSystem.CacheDirectory,
            $"moneypro-restore-{DateTime.Now:yyyyMMddHHmmss}.db");
        await using (var source = await selected.OpenReadAsync())
        await using (var destination = File.Create(importPath))
            await source.CopyToAsync(destination);
        await ValidateAsync(importPath);
        var safety = database.DatabasePath + $".antes-restauracao-{DateTime.Now:yyyyMMddHHmmss}.bak";
        await CheckpointAsync();
        File.Copy(database.DatabasePath, safety, true);
        File.Copy(importPath, database.DatabasePath, true);
        return safety;
    }

    private async Task CheckpointAsync()
    {
        await using var db = new SqliteConnection($"Data Source={database.DatabasePath}");
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task ValidateAsync(string path)
    {
        await using var db = new SqliteConnection($"Data Source={path};Mode=ReadOnly");
        await db.OpenAsync();
        await using var command = db.CreateCommand();
        command.CommandText = "PRAGMA integrity_check;";
        var result = Convert.ToString(await command.ExecuteScalarAsync());
        if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("O arquivo selecionado não é um banco SQLite íntegro.");
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Usuarios';";
        if (Convert.ToInt32(await command.ExecuteScalarAsync()) != 1)
            throw new InvalidDataException("O arquivo não é um backup válido do Money Pro.");
    }
}
