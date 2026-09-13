using Money.Services;

namespace Money.Views.SettingsAndData;

public partial class BackupPage : ContentPage
{
    private readonly BackupService _backup;
    private bool _busy;

    public BackupPage(BackupService backup)
    {
        InitializeComponent();
        _backup = backup;
    }

    private async void OnCreateBackupClicked(object? sender, EventArgs e)
    {
        if (_busy) return;
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
        if (_busy) return;
        _busy = true;
        Content.IsEnabled = false;
        try
        {
            var path = await _backup.CreateBackupAsync(destination);
            await ThemedDialog.ShowAsync(this, "Backup realizado", destination == BackupDestination.Share
                ? "Arquivo preparado para compartilhamento. Confira o envio no aplicativo escolhido."
                : $"Arquivo salvo com sucesso:\n{path}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Não foi possível realizar o backup", SqliteErrorMessage.ToFriendly(ex)); }
        finally { _busy = false; Content.IsEnabled = true; }
    }

    private async void OnRestoreBackupClicked(object? sender, EventArgs e)
    {
        if (_busy) return;
        if (!await ThemedDialog.ConfirmAsync(this, "Restaurar backup", "Os dados atuais serão substituídos e uma cópia de segurança será preservada.", "Restaurar")) return;
        _busy = true;
        Content.IsEnabled = false;
        try
        {
            var result = await _backup.RestoreBackupAsync();
            try
            {
                await ThemedDialog.ShowAsync(this, "Backup restaurado",
                    result.Summary + "\n\nUma cópia dos dados anteriores foi preservada.");
            }
            finally { (Application.Current as App)?.ShowLogin(); }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await ThemedDialog.ShowAsync(this, "Não foi possível restaurar", SqliteErrorMessage.ToFriendly(ex)); }
        finally { _busy = false; Content.IsEnabled = true; }
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
