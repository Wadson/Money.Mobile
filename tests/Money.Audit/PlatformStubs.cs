global using Money.Helpers;
global using Microsoft.Maui.Graphics;
// Only the platform storage directory and visual constants are substituted.
// Database queries, migrations and financial rules are the production sources.
public static class FileSystem
{
    public static string AppDataDirectory { get; set; } = "";
    public static string CacheDirectory => AppDataDirectory;
}
public sealed class SecureStorage
{
    public static SecureStorage Default { get; } = new();
    private readonly Dictionary<string, string> _values = new();
    public Task SetAsync(string key, string value) { _values[key] = value; return Task.CompletedTask; }
    public Task<string?> GetAsync(string key) => Task.FromResult(_values.GetValueOrDefault(key));
    public bool Remove(string key) => _values.Remove(key);
}
public sealed record ShareFile(string Path, string MimeType);
public sealed record ShareFileRequest(string Title, ShareFile File);
public sealed class Share
{
    public static Share Default { get; } = new();
    public Task RequestAsync(ShareFileRequest request) => throw new NotSupportedException("Sharing is a device-only test.");
}
namespace Money.Helpers
{
    internal static class ThemeColor { public static Color Get(string key) => Color.FromArgb(BlingPalette.PrimaryHex); }
    internal static class BlingPalette
    {
        public const string PrimaryHex = "#00A859";
        public const string HeaderDarkHex = "#0B281E";
        public const string TextDarkHex = "#1E293B";
        public const string CardBackgroundHex = "#FFFFFF";
        public const string BorderColorHex = "#00A859";
    }
}
