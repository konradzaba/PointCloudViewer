
using Android.App;
using Android.Views;
using Android.OS;
using Android.Content;
using PointCloudViewer.Domain;
using Newtonsoft.Json;

namespace PointCloudViewer.Droid
{
    [Activity(Label = "PointCloudViewer", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }


        public void StartEngineActivity(Settings settings)
        {
            //Intents are used on Android to pass data to activity
            Intent intent = new Intent(this, typeof(PointCloudEngineActivity));
            intent.PutExtra("Settings", JsonConvert.SerializeObject(settings, Formatting.Indented));
            StartActivity(intent);
        }
    }
}

