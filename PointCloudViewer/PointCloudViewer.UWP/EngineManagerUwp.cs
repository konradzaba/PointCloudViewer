using PointCloudViewer.Abstract;
using PointCloudViewer.Domain;
using PointCloudViewer.UWP;
using PointCloudViewer.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Xamarin.Forms;

[assembly: Dependency(typeof(EngineManagerUwp))]
namespace PointCloudViewer.UWP
{
    class EngineManagerUwp : IEngineManager
    {
        /// <summary>
        /// List of built-in colors.
        /// Necessary because every API handles the colors differenty.
        /// </summary>
        /// <returns>List of colors.</returns>
        public List<ColorPortable> GetColors()
        {
            var toReturn = new List<ColorPortable>();

            var colors = typeof(Windows.UI.Colors)
                            .GetRuntimeProperties()
                            .Select(c => new
                            {
                                Color = (Windows.UI.Color)c.GetValue(null),
                                Name = c.Name
                            });
            foreach(var colorNamePair in colors)
            {
                toReturn.Add(new ColorPortable()
                {
                    Name = colorNamePair.Name,
                    R = colorNamePair.Color.R,
                    G = colorNamePair.Color.G,
                    B = colorNamePair.Color.B
                });
            }

            return toReturn;
        }

        /// <summary>
        /// Used to check whether the device has keyboard. If it has, no touch overlay is drawed.
        /// </summary>
        /// <returns>True if it has keyboard, false otherwise.</returns>
        public bool IsDeviceWithKeyboard()
        {
            var keyboardCapabilities = new Windows.Devices.Input.KeyboardCapabilities();
            return keyboardCapabilities.KeyboardPresent != 0;
        }

        /// <summary>
        /// Used to start the visualization in Monogame.
        /// </summary>
        /// <param name="pointCloudName">Name of the point cloud that will be displayed.</param>
        public void StartVisualization(string pointCloudName)
        {
            var frame = (Windows.UI.Xaml.Controls.Frame)Window.Current.Content;
            var page = (MainPage)frame.Content;
            var settings = SettingsViewModel.GetSettings();
            settings.PointCloudName = pointCloudName;
            page.NavigateToVisualization(settings);
        }
    }
}
