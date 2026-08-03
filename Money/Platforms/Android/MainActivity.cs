using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;

namespace Money
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == Platforms.Android.AndroidBackupFileService.SaveRequestCode)
                Platforms.Android.AndroidBackupFileService.Complete(this, resultCode, data);
        }
    }
}
