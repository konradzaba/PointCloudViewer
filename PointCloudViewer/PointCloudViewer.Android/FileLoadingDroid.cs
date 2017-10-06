using PointCloudViewer.Droid;
using Xamarin.Forms;
using System.IO;
using PointCloudViewer.FileProcessing.Abstract;
using Android.App;

[assembly: Dependency(typeof(FileLoadingDroid))]
namespace PointCloudViewer.Droid
{
    public class FileLoadingDroid : Android.App.Application, IFileLoading
    {
        /// <summary>
        /// Reads the file to string and returns it.
        /// </summary>
        /// <param name="fileName">The file that is meant to be read.</param>
        /// <returns>String of file contents.</returns>
        public string LoadText(string filename)
        {
            var activity = (Activity)Forms.Context;
            using (StreamReader sr = new StreamReader(activity.Assets.Open(filename)))
                return sr.ReadToEnd();
        }
    }
}