using PointCloudViewer.Domain;
using System.Collections.Generic;

namespace PointCloudViewer.Abstract
{
    public interface IEngineManager
    {
        /// <summary>
        /// Used to start the visualization in Monogame.
        /// </summary>
        /// <param name="pointCloudName">Name of the point cloud that will be displayed.</param>
        void StartVisualization(string pointCloudName);

        /// <summary>
        /// Used to check whether the device has keyboard. If it has, no touch overlay is drawed.
        /// </summary>
        /// <returns>True if it has keyboard, false otherwise.</returns>
        bool IsDeviceWithKeyboard();

        /// <summary>
        /// List of built-in colors.
        /// Necessary because every API handles the colors differenty.
        /// </summary>
        /// <returns>List of colors.</returns>
        List<ColorPortable> GetColors();
    }
}
