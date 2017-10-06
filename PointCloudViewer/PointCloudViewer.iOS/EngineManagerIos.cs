using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PointCloudViewer.Abstract;
using PointCloudViewer.iOS;
using Xamarin.Forms;
using PointCloudViewer.ViewModel;
using PointCloudViewer.Engine;

[assembly: Dependency(typeof(EngineManagerIos))]
namespace PointCloudViewer.iOS
{
    class EngineManagerIos : IEngineManager
    {
        public bool IsDeviceWithKeyboard()
        {
            return false;
        }

        public void StartVisualization(string pointCloudName)
        {
            var settings = SettingsViewModel.GetSettings();
            settings.PointCloudName = pointCloudName;
            EngineSettings.Instance.ReadSettings(settings);
            var engine = new PointCloudViewer.Engine.Engine();
            engine.Run();
        }
    }
}