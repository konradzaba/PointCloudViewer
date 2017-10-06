using System;
using System.Collections.Generic;
using PointCloudViewer.Abstract;
using Xamarin.Forms;
using PointCloudViewer.Droid;
using PointCloudViewer.ViewModel;
using PointCloudViewer.Domain;
using System.Drawing;

[assembly: Dependency(typeof(EngineManagerDroid))]
namespace PointCloudViewer.Droid
{
    class EngineManagerDroid : IEngineManager
    {
        /// <summary>
        /// List of built-in colors.
        /// Necessary because every API handles the colors differenty.
        /// </summary>
        /// <returns>List of colors.</returns>
        public List<ColorPortable> GetColors()
        {
            var toReturn = new List<ColorPortable>();
            var colorsList = ((KnownColor[])Enum.GetValues(typeof(KnownColor)));//.Select(x => x.ToString());
            foreach(var color in colorsList)
            {
                var rgb = System.Drawing.Color.FromKnownColor(color);
                toReturn.Add(new ColorPortable()
                {
                    Name = color.ToString(),
                    R = rgb.R,
                    G = rgb.G,
                    B = rgb.B
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
            return false;
        }

        /// <summary>
        /// Used to start the visualization in Monogame.
        /// </summary>
        /// <param name="pointCloudName">Name of the point cloud that will be displayed.</param>
        public void StartVisualization(string pointCloudName)
        {
            var activity = (MainActivity)Forms.Context;
            var settings = SettingsViewModel.GetSettings();
            settings.PointCloudName = pointCloudName;
            activity.StartEngineActivity(settings);
        }
    }
}