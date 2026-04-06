using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Serilog;

namespace Pea.Meter
{
    [Activity(Theme = "@style/Maui.SplashTheme",
        MainLauncher = true, 
        LaunchMode = LaunchMode.SingleTop, 
        ScreenOrientation = ScreenOrientation.Landscape, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                if (args.Exception != null)
                {
                    Log.Fatal(args.Exception, "Unhandled Android exception: {Message}", args.Exception.Message);
                }
            };
        }
    }
}
