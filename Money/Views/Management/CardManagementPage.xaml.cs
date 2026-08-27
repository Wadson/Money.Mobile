using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using MauiIcons.Core;
using MauiIcons.Material;
using Money.Models;
using Money.Services;
using Money.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Money.Views.Management;

public partial class CardManagementPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<CardItem> _cards = new();
    private long? _editingId;

    public CardManagementPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        ColorEntry.Text = BlingPalette.PrimaryHex;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _cards = await _database.GetCardsAsync();
        RenderCards();
        UpdateSummary();
        UpdateEmptyState();
    }

    private void RenderCards()
    {
        CardsContainer.Children.Clear();

        if (_cards.Count == 0)
        {
            EmptyState.IsVisible = true;
            return;
        }

        EmptyState.IsVisible = false;

        foreach (var card in _cards.OrderBy(x => x.Name))
        {
            var cardControl = CreateCardControl(card);
            CardsContainer.Children.Add(cardControl);
        }
    }

    private Border CreateCardControl(CardItem card)
    {
        Color brand;
        brand = ThemeColor.Parse(card.Color);
        var usage = card.CreditLimit > 0 ? Math.Min(1, (double)(card.Used / card.CreditLimit)) : 0;
        var usagePercent = (int)(usage * 100);

        var cardBorder = new Border
        {
            Style = (Style)Application.Current!.Resources["CardItem"],
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 8),
            Stroke = brand.WithAlpha(.45f),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(14)
            },
            BackgroundColor = brand.WithAlpha(.06f),
            HorizontalOptions = LayoutOptions.Fill,
            MinimumWidthRequest = 300
        };

        var mainStack = new VerticalStackLayout
        {
            Spacing = 8
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star)
            },
            ColumnSpacing = 8
        };

        var nameStack = new HorizontalStackLayout
        {
            Spacing = 8
        };
        nameStack.Children.Add(new Border
        {
            HeightRequest = 38, WidthRequest = 38, Padding = 8, StrokeThickness = 0,
            BackgroundColor = brand.WithAlpha(.15f),
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = new MauiIcon
            {
                Icon = MaterialIcons.CreditCard,
                IconSize = 24,
                IconColor = brand,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        });
        nameStack.Children.Add(new Label
        {
            Text = card.Name,
            Style = (Style)Application.Current!.Resources["CardName"],
            VerticalTextAlignment = TextAlignment.Center
        });
        Grid.SetColumn(nameStack, 0);
        headerGrid.Children.Add(nameStack);

        var editBtn = new Button
        {
            Text = "Editar",
            ImageSource = MaterialIcons.Edit.ToImageSource(ThemeColor.Get("BlingTextLight"), 18),
            BackgroundColor = ThemeColor.Get("BlingPrimary"),
            TextColor = ThemeColor.Get("BlingTextLight"),
            FontSize = 12,
            Padding = new Thickness(12, 6),
            HeightRequest = 40
        };
        editBtn.Clicked += (_, _) => Edit(card);

        var deleteBtn = new Button
        {
            Text = "Excluir",
            ImageSource = MaterialIcons.Delete.ToImageSource(ThemeColor.Get("BlingTextLight"), 18),
            BackgroundColor = ThemeColor.Get("BlingPrimary"),
            TextColor = ThemeColor.Get("BlingTextLight"),
            FontSize = 12,
            Padding = new Thickness(12, 6),
            HeightRequest = 40
        };
        deleteBtn.Clicked += async (_, _) => await DeleteCard(card);

        mainStack.Children.Add(headerGrid);

        var actionsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star),
                new(GridLength.Star)
            },
            ColumnSpacing = 10
        };
        actionsGrid.Add(editBtn);
        actionsGrid.Add(deleteBtn, 1);
        mainStack.Children.Add(actionsGrid);

        var limitGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star),
                new(GridLength.Star),
                new(GridLength.Star)
            },
            ColumnSpacing = 8,
            Margin = new Thickness(0, 4, 0, 0)
        };

        var limitStack = new VerticalStackLayout
        {
            Spacing = 0
        };
        limitStack.Children.Add(new Label
        {
            Text = "Limite",
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 10,
            FontAttributes = FontAttributes.Bold
        });
        limitStack.Children.Add(new Label
        {
            Text = card.CreditLimit.ToString("C2", _culture),
            TextColor = ThemeColor.Get("BlingPrimary"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold
        });
        Grid.SetColumn(limitStack, 0);
        limitGrid.Children.Add(limitStack);

        var usedStack = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center
        };
        usedStack.Children.Add(new Label
        {
            Text = "Usado",
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 10,
            FontAttributes = FontAttributes.Bold
        });
        usedStack.Children.Add(new Label
        {
            Text = card.Used.ToString("C2", _culture),
            TextColor = usagePercent > 80 ? ThemeColor.Get("BlingText") : ThemeColor.Get("BlingPrimary"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold
        });
        Grid.SetColumn(usedStack, 1);
        limitGrid.Children.Add(usedStack);

        var availableStack = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.End
        };
        availableStack.Children.Add(new Label
        {
            Text = "Disponível",
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 10,
            FontAttributes = FontAttributes.Bold
        });
        availableStack.Children.Add(new Label
        {
            Text = (card.CreditLimit - card.Used).ToString("C2", _culture),
            TextColor = ThemeColor.Get("BlingPrimary"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold
        });
        Grid.SetColumn(availableStack, 2);
        limitGrid.Children.Add(availableStack);

        mainStack.Children.Add(limitGrid);

        var progressStack = new VerticalStackLayout
        {
            Spacing = 2,
            Margin = new Thickness(0, 4, 0, 0)
        };
        progressStack.Children.Add(new ProgressBar
        {
            Progress = usage,
            ProgressColor = usagePercent > 80 ? ThemeColor.Get("BlingText") : brand,
            BackgroundColor = ThemeColor.Get("BlingText"),
            HeightRequest = 6
        });
        progressStack.Children.Add(new Label
        {
            Text = $"{usagePercent}% utilizado",
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 10,
            HorizontalTextAlignment = TextAlignment.End
        });

        mainStack.Children.Add(progressStack);

        var footerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star),
                new(GridLength.Star)
            },
            ColumnSpacing = 8,
            Margin = new Thickness(0, 6, 0, 0)
        };

        var closingStack = new HorizontalStackLayout
        {
            Spacing = 4
        };
        closingStack.Children.Add(new Label
        {
            Text = "Fechamento:",
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 11
        });
        closingStack.Children.Add(new Label
        {
            Text = card.ClosingDay.ToString(),
            TextColor = ThemeColor.Get("BlingPrimary"),
            FontSize = 11,
            FontAttributes = FontAttributes.Bold
        });
        Grid.SetColumn(closingStack, 0);
        footerGrid.Children.Add(closingStack);

        var dueStack = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.End
        };
        dueStack.Children.Add(new Label
        {
            Text = "Vencimento:",
            TextColor = ThemeColor.Get("BlingText"),
            FontSize = 11
        });
        dueStack.Children.Add(new Label
        {
            Text = card.DueDay.ToString(),
            TextColor = ThemeColor.Get("BlingPrimary"),
            FontSize = 11,
            FontAttributes = FontAttributes.Bold
        });
        Grid.SetColumn(dueStack, 1);
        footerGrid.Children.Add(dueStack);

        mainStack.Children.Add(footerGrid);

        cardBorder.Content = mainStack;
        return cardBorder;
    }

    private void UpdateSummary()
    {
        TotalCardsLabel.Text = _cards.Count.ToString();
        TotalLimitLabel.Text = _cards.Sum(x => x.CreditLimit).ToString("C2", _culture);
        UsedLimitLabel.Text = _cards.Sum(x => x.Used).ToString("C2", _culture);
    }

    private void UpdateEmptyState()
    {
        var hasItems = CardsContainer.Children.Count > 0;
        EmptyState.IsVisible = !hasItems;
    }

    private void OnNewClicked(object? sender, EventArgs e)
    {
        _editingId = null;
        FormTitle.Text = "Novo cartão";
        Clear();
        FormCard.IsVisible = true;
        NameEntry.Focus();
    }

    private void Edit(CardItem card)
    {
        _editingId = card.Id;
        FormTitle.Text = "Editar cartão";
        NameEntry.Text = card.Name;
        LimitEntry.Text = card.CreditLimit.ToString("N2", _culture);
        ClosingEntry.Text = card.ClosingDay.ToString();
        DueEntry.Text = card.DueDay.ToString();
        ColorEntry.Text = card.Color;
        ErrorLabel.IsVisible = false;
        FormCard.IsVisible = true;
        NameEntry.Focus();
    }

    private void OnCancelFormClicked(object? sender, EventArgs e)
    {
        FormCard.IsVisible = false;
        Clear();
    }

    private void Clear()
    {
        NameEntry.Text = string.Empty;
        LimitEntry.Text = string.Empty;
        ClosingEntry.Text = string.Empty;
        DueEntry.Text = string.Empty;
        ColorEntry.Text = BlingPalette.PrimaryHex;
        ErrorLabel.IsVisible = false;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowError("O nome do cartão é obrigatório.");
            return;
        }

        if (!decimal.TryParse(LimitEntry.Text, NumberStyles.Currency, _culture, out var limit) || limit <= 0)
        {
            ShowError("Informe um limite válido (ex: 1000,00).");
            return;
        }

        if (!int.TryParse(ClosingEntry.Text, out var closing) || closing < 1 || closing > 31)
        {
            ShowError("Informe um dia de fechamento válido (1 a 31).");
            return;
        }

        if (!int.TryParse(DueEntry.Text, out var due) || due < 1 || due > 31)
        {
            ShowError("Informe um dia de vencimento válido (1 a 31).");
            return;
        }

        try
        {
            await _database.SaveCardAsync(
                _editingId,
                NameEntry.Text.Trim(),
                limit,
                closing,
                due,
                string.IsNullOrWhiteSpace(ColorEntry.Text) ? BlingPalette.PrimaryHex : ColorEntry.Text.Trim()
            );
            await ThemedDialog.ShowAsync(this, "Cartão salvo",
                _editingId is null ? "Cartão cadastrado com sucesso!" : "Cartão alterado com sucesso!");

            FormCard.IsVisible = false;
            Clear();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowError(SqliteErrorMessage.ToFriendly(ex));
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private async void OnImportInvoiceClicked(object? sender, EventArgs e)
    {
        var page = new ImportCardInvoicePage(_database);
        page.Imported += async (_, _) =>
        {
            await LoadAsync();
            await Navigation.PushModalAsync(new CreditCardAnalysisPage(_database));
        };
        await Navigation.PushModalAsync(page);
    }
    private void OnColorClicked(object? sender, EventArgs e)
    {
        if (sender is Button button) ColorEntry.Text = button.BackgroundColor.ToHex();
    }

    private async Task DeleteCard(CardItem card)
    {
        var confirm = await ThemedDialog.ConfirmDeleteAsync(this,
            "Excluir cartão",
            $"Tem certeza que deseja excluir o cartão '{card.Name}'?{Environment.NewLine}As transações vinculadas também serão removidas."
        );

        if (!confirm)
            return;

        try
        {
            await _database.DeleteCardAsync(card.Id);
            await LoadAsync();
            await ThemedDialog.ShowAsync(this, "Cartão excluído", "Cartão excluído com sucesso!");
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Erro", $"Não foi possível excluir: {SqliteErrorMessage.ToFriendly(ex)}");
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
