using MauiIcons.Core;
using MauiIcons.Material;

namespace Money.Helpers;

internal static class ThemedDialog
{
    public static async Task<bool> ConfirmDeleteAsync(Page owner, string title, string message)
        => await ConfirmAsync(owner, title, message, "Excluir", "Cancelar");

    public static async Task<bool> ConfirmAsync(Page owner, string title, string message,
        string confirmText, string cancelText = "Cancelar")
    {
        var dialog = new ThemedConfirmationPage(title, message, confirmText, cancelText);
        await owner.Navigation.PushModalAsync(dialog, false);
        return await dialog.Result;
    }

    public static async Task ShowAsync(Page owner, string title, string message, string buttonText = "OK")
    {
        var dialog = new ThemedConfirmationPage(title, message, buttonText, null);
        await owner.Navigation.PushModalAsync(dialog, false);
        await dialog.Result;
    }

    private sealed class ThemedConfirmationPage : ContentPage
    {
        private readonly TaskCompletionSource<bool> _result =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _closing;

        public Task<bool> Result => _result.Task;

        public ThemedConfirmationPage(string title, string message, string confirmText, string? cancelText)
        {
            BackgroundColor = ThemeColor.Get("BlingBackground");
            Shell.SetNavBarIsVisible(this, false);

            var cancel = new Button
            {
                Text = cancelText ?? string.Empty,
                BackgroundColor = ThemeColor.Get("BlingCard"),
                TextColor = ThemeColor.Get("BlingPrimary"),
                BorderColor = ThemeColor.Get("BlingPrimary"),
                BorderWidth = 1,
                CornerRadius = 8,
                HeightRequest = 48
            };
            cancel.Clicked += async (_, _) => await CloseAsync(false);

            var confirm = new Button
            {
                Text = confirmText,
                BackgroundColor = ThemeColor.Get("BlingPrimary"),
                TextColor = ThemeColor.Get("BlingTextLight"),
                BorderColor = ThemeColor.Get("BlingPrimary"),
                BorderWidth = 1,
                CornerRadius = 8,
                HeightRequest = 48,
                FontAttributes = FontAttributes.Bold
            };
            confirm.Clicked += async (_, _) => await CloseAsync(true);

            var actions = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 10
            };
            if (cancelText is null)
            {
                actions.ColumnDefinitions.Clear();
                actions.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                actions.Add(confirm);
            }
            else
            {
                actions.Add(cancel);
                actions.Add(confirm, 1);
            }

            var header = new Border
            {
                BackgroundColor = ThemeColor.Get("BlingHeader"),
                StrokeThickness = 0,
                Padding = new Thickness(18, 14),
                Content = new Label
                {
                    Text = title,
                    TextColor = ThemeColor.Get("BlingTextLight"),
                    FontSize = 19,
                    FontAttributes = FontAttributes.Bold
                }
            };

            var statusColor = ResolveStatusColor(title);
            var statusIcon = ResolveStatusIcon(title);
            var body = new VerticalStackLayout
            {
                Spacing = 18,
                Children =
                {
                    new Image
                    {
                        Source = statusIcon.ToImageSource(statusColor, 36),
                        WidthRequest = 44,
                        HeightRequest = 44,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = message,
                        TextColor = ThemeColor.Get("BlingText"),
                        FontSize = 15,
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    actions
                }
            };

            var card = new Border
            {
                BackgroundColor = ThemeColor.Get("BlingCard"),
                Stroke = ThemeColor.Get("BlingPrimary"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Opacity = 0.22f,
                    Radius = 18,
                    Offset = new Point(0, 8)
                },
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children =
                    {
                        header,
                        new Border { StrokeThickness = 0, Padding = 18, Content = body }
                    }
                }
            };

            Content = new Grid
            {
                Padding = 24,
                VerticalOptions = LayoutOptions.Center,
                Children = { card }
            };
        }

        private static MaterialIcons ResolveStatusIcon(string title)
        {
            var value = title.ToLowerInvariant();
            if (value.Contains("erro") || value.Contains("não") || value.Contains("falha"))
                return MaterialIcons.Error;
            if (value.Contains("sucesso") || value.Contains("salvo") || value.Contains("alterado") || value.Contains("criada"))
                return MaterialIcons.CheckCircle;
            return MaterialIcons.Info;
        }

        private static Color ResolveStatusColor(string title)
        {
            var value = title.ToLowerInvariant();
            if (value.Contains("erro") || value.Contains("não") || value.Contains("falha"))
                return Color.FromArgb("#E67E22");
            return ThemeColor.Get("BlingPrimary");
        }

        protected override bool OnBackButtonPressed()
        {
            _ = CloseAsync(false);
            return true;
        }

        private async Task CloseAsync(bool confirmed)
        {
            if (_closing)
                return;
            _closing = true;
            _result.TrySetResult(confirmed);
            await Navigation.PopModalAsync(false);
        }
    }
}
