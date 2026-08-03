using Microsoft.Extensions.DependencyInjection;
using Money.Services;

namespace Money;

public partial class App : Application
{
    private readonly AuthService _auth;
    private readonly IServiceProvider _services;
    private readonly DatabaseService _database;
    private readonly Exception? _resourceLoadException;

    public App(AuthService auth, DatabaseService database, IServiceProvider services)
    {
        try
        {
            // Carregamento tipado: evita a dependência de localizar Money.App.xaml
            // como EmbeddedResource após o linker/empacotador Android Release.
            Resources = new ResourceDictionary();
            Resources.MergedDictionaries.Add(new global::Money.Resources.Styles.Colors());
            Resources.MergedDictionaries.Add(new global::Money.Resources.Styles.Styles());
        }
        catch (Exception ex)
        {
            _resourceLoadException = ex;
        }

        _auth = auth;
        _database = database;
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new ContentPage
        {
            BackgroundColor = ThemeColor.Get("BlingBackground"),
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new ActivityIndicator { IsRunning = true, Color = ThemeColor.Get("BlingPrimary") },
                    new Label { Text = "Iniciando Money Pro...", TextColor = ThemeColor.Get("BlingText") }
                }
            }
        });
        _ = InitializeWindowAsync(window);
        return window;
    }

    private async Task InitializeWindowAsync(Window window)
    {
        try
        {
            if (_resourceLoadException is not null)
                throw new InvalidOperationException(
                    "Falha ao carregar os recursos visuais do aplicativo.",
                    _resourceLoadException);

            await _auth.InitializeAsync();
            // Modo local de usuário único. O fluxo de login permanece disponível para reativação futura.
            await _auth.ActivateLocalUserAsync();
            var settings = await _database.GetSettingsAsync();
            UserAppTheme = settings.Theme switch
            {
                "Escuro" => AppTheme.Dark,
                "Claro" => AppTheme.Light,
                _ => AppTheme.Unspecified
            };

            var shell = new AppShell();
            shell.SetMainPage(_services.GetRequiredService<MainPage>());
            window.Page = shell;
        }
        catch (Exception ex)
        {
            window.Page = new ContentPage
            {
                BackgroundColor = ThemeColor.Get("BlingCard"),
                Padding = new Thickness(24),
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = "Não foi possível iniciar o aplicativo.",
                            FontSize = 20,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = ThemeColor.Get("BlingText")
                        },
                        new Label
                        {
                            Text = GetCompleteErrorMessage(ex),
                            TextColor = ThemeColor.Get("BlingText")
                        }
                    }
                }
            };
        }
    }

    private static string GetCompleteErrorMessage(Exception exception)
    {
        var messages = new List<string>();
        for (var current = exception; current is not null; current = current.InnerException)
            messages.Add(current.Message);
        return string.Join(Environment.NewLine, messages.Distinct());
    }
}
