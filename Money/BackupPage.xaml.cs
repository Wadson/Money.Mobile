using Money.Services;

namespace Money;

public partial class BackupPage : ContentPage
{
    private readonly BackupService _backup;

    public BackupPage(BackupService backup)
    {
        InitializeComponent();
        _backup = backup;
    }

    private async void OnCreateBackupClicked(object? sender, EventArgs e)
    {
        var options = new[]
        {
            new SelectionOption { Index = 0, Label = "Salvar em uma pasta", ImageSource = MauiIcons.Material.MaterialIcons.Folder, Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary") },
            new SelectionOption { Index = 1, Label = "Compartilhar arquivo", ImageSource = MauiIcons.Material.MaterialIcons.Share, Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary") }
        };
        var selector = new OptionSelectionPage("Destino do backup", options);
        selector.Selected += async (_, option) => await CreateAsync(option.Index == 0 ? BackupDestination.Folder : BackupDestination.Share);
        await Navigation.PushModalAsync(selector);
    }

    private async Task CreateAsync(BackupDestination destination)
    {
        try
        {
            var path = await _backup.CreateBackupAsync(destination);
            await ThemedDialog.ShowAsync(this, "Backup realizado", $"Arquivo salvo com sucesso:\n{path}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Não foi possível realizar o backup", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private async void OnRestoreBackupClicked(object? sender, EventArgs e)
    {
        if (!await ThemedDialog.ConfirmAsync(this, "Restaurar backup", "Os dados atuais serão substituídos e uma cópia de segurança será preservada.", "Restaurar")) return;
        try
        {
            var safety = await _backup.RestoreBackupAsync();
            await ThemedDialog.ShowAsync(this, "Backup restaurado", $"Restauração concluída. Cópia anterior:\n{safety}\n\nReinicie o aplicativo.");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Não foi possível restaurar", SqliteErrorMessage.ToFriendly(ex)); }
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
