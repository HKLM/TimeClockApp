using Android.App;
using Android.Content.PM;
using Android.OS;

using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace TimeClockApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.FullUser)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        Microsoft.Maui.ApplicationModel.Platform.Init(this, bundle);
        App.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}
