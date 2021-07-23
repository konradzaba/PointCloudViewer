
using Android.App;
using Android.Content;
using Android.OS;
using PointCloudViewer.Engine;
using Newtonsoft.Json;
using Android.Util;
using Android.Content.PM;
using Android.Views;

namespace PointCloudViewer.Droid
{
    [Activity(Label = "PointCloudViewer"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/MainTheme"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleTask // SingleTask means we only run a single instance, but can open other Activities (eg. Google+)
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    class PointCloudEngineActivity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        public PointCloudEngineActivity() { }

        private void HideStatusAndButtons()
        {
            var uiOptions =
                  SystemUiFlags.HideNavigation |
                  SystemUiFlags.Fullscreen |
                  SystemUiFlags.ImmersiveSticky |
                  SystemUiFlags.LowProfile
                  ;

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.P)
            {
                Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
            }
            Window.AddFlags(WindowManagerFlags.TranslucentStatus);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            HideStatusAndButtons();
            EngineSettings.Instance.ReadSettings(JsonConvert.DeserializeObject<PointCloudViewer.Domain.Settings>(Intent.GetStringExtra("Settings")));

            var disp = WindowManager.DefaultDisplay;
            var met = new DisplayMetrics();
            disp.GetRealMetrics(met);

            var engineInstance = new Engine.Engine(met.HeightPixels, met.WidthPixels);
            SetContentView((Android.Views.View)engineInstance.Services.GetService(typeof(Android.Views.View)));
            engineInstance.Run();
        }
    }
}