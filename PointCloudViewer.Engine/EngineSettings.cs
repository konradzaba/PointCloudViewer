using PointCloudViewer.Domain;

namespace PointCloudViewer.Engine
{
    public class EngineSettings
    {
        private static EngineSettings _instance;
        public static EngineSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EngineSettings();
                }
                return _instance;
            }
        }

        //ugly reference due to lack of possibility to pass parameter from UWP
        public string PointCloudName { get; private set; }

        public void ReadSettings(Settings settings)
        {
            PointCloudName = settings.PointCloudName;
            IsDeviceWithKeyboard = settings.IsDeviceWithKeyboard;
            if (settings.WereInitialized)
            {
                ResolutionScaling = settings.Resolution / 100f;
                LimitFps = settings.LimitFPS ? 30 : 60;
                ViewDistance = 90 + settings.DrawDistance;
                LevelOfDetailDistance = settings.LevelOfDetail / 100f;
                CameraSpeed = settings.CameraSpeed / 100f;
                ColorQuality = 5 + settings.ColorQuality;
                ShowConsole = settings.ShowConsole;
                ShowFps = settings.ShowFPS;
                BackgroundColor = Microsoft.Xna.Framework.Color.FromNonPremultiplied(
                    settings.BackgroundR,
                    settings.BackgroundG,
                    settings.BackgroundB,
                    255);
            }
        }

        public float ResolutionScaling = 0.75f;
        public int LimitFps = 30;
        public float ViewDistance = 120f;//90f-190f
        public float LevelOfDetailDistance = 0.4f;//0%-100%
        public float CameraSpeed = 0.15f;//0-0.5f
        public int ColorQuality = 30;//5-100
        public bool ShowConsole = false;
        public bool ShowFps = true;
        public Microsoft.Xna.Framework.Color BackgroundColor = Microsoft.Xna.Framework.Color.Black;
        public bool IsDeviceWithKeyboard = true;
    }
}
