using MauiIcons.Core;
using MauiIcons.Material;

namespace Money.Controls;

public sealed class VectorIcon : ContentView
{
    private readonly MauiIcon _icon;
    public static readonly BindableProperty DataProperty = BindableProperty.Create(
        nameof(Data), typeof(string), typeof(VectorIcon), IconCatalog.Wallet,
        propertyChanged: (b, _, value) => ((VectorIcon)b).SetData((string?)value));
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color), typeof(Color), typeof(VectorIcon), ThemeColor.Get("BlingText"),
        propertyChanged: (b, _, value) => ((VectorIcon)b)._icon.IconColor = (Color)value);

    public string Data { get => (string)GetValue(DataProperty); set => SetValue(DataProperty, value); }
    public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }

    public VectorIcon()
    {
        _icon = new MauiIcon
        {
            Icon = MaterialIcons.AccountBalanceWallet,
            IconColor = Color,
            IconSize = 24,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        Content = _icon;
        SetData(Data);
    }

    private void SetData(string? data)
    {
        _icon.Icon = Enum.TryParse<MaterialIcons>(data, true, out var icon)
            ? icon
            : MaterialIcons.AccountBalanceWallet;
    }
}

public static class IconCatalog
{
    public const string Wallet = nameof(MaterialIcons.AccountBalanceWallet);
    public const string Bank = nameof(MaterialIcons.AccountBalance);
    public const string Card = nameof(MaterialIcons.CreditCard);
    public const string Tag = nameof(MaterialIcons.Sell);
    public const string Target = nameof(MaterialIcons.Flag);
    public const string Bell = nameof(MaterialIcons.Notifications);
    public const string Settings = nameof(MaterialIcons.Settings);
    public const string Import = nameof(MaterialIcons.ImportExport);
    public const string Backup = nameof(MaterialIcons.Backup);
    public const string Audit = nameof(MaterialIcons.FactCheck);
    public const string Plus = nameof(MaterialIcons.Add);
    public const string Trash = nameof(MaterialIcons.Delete);
    public const string Edit = nameof(MaterialIcons.Edit);
    public const string Calendar = nameof(MaterialIcons.CalendarToday);
    public const string User = nameof(MaterialIcons.Person);
    public const string Refresh = nameof(MaterialIcons.Refresh);
}
