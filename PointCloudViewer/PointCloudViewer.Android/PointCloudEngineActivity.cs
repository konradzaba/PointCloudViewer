
using Android.App;
using Android.Content;
using Android.OS;
using PointCloudViewer.Engine;
using Newtonsoft.Json;

namespace PointCloudViewer.Droid
{
    [Activity(Label = "PointCloudViewer", Icon = "@drawable/icon", Theme = "@style/MainTheme", ScreenOrientation =Android.Content.PM.ScreenOrientation.Landscape)]
    class PointCloudEngineActivity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        public PointCloudEngineActivity() { }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            EngineSettings.Instance.ReadSettings(JsonConvert.DeserializeObject<PointCloudViewer.Domain.Settings>(Intent.GetStringExtra("Settings")));
            var engineInstance = new Engine.Engine();
            SetContentView((Android.Views.View)engineInstance.Services.GetService(typeof(Android.Views.View)));
            engineInstance.Run();
        }
    }
}