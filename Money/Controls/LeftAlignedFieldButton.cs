namespace Money.Controls;

/// <summary>Campo de seleção com conteúdo fixado ao início no controle nativo.</summary>
public sealed class LeftAlignedFieldButton : Button
{
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
#if ANDROID
        if (Handler?.PlatformView is Android.Widget.Button nativeButton)
        {
            nativeButton.Gravity = Android.Views.GravityFlags.Start | Android.Views.GravityFlags.CenterVertical;
            nativeButton.TextAlignment = Android.Views.TextAlignment.ViewStart;
            nativeButton.CompoundDrawablePadding = (int)(12 * DeviceDisplay.MainDisplayInfo.Density);
        }
#endif
    }
}
