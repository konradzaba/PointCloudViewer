using PointCloudViewer.FileProcessing.Abstract;
using PointCloudViewer.UWP;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileLoadingUwp))]
namespace PointCloudViewer.UWP
{
    public class FileLoadingUwp : IFileLoading
    {
        /// <summary>
        /// Reads the file to string and returns it.
        /// </summary>
        /// <param name="fileName">The file that is meant to be read.</param>
        /// <returns>String of file contents.</returns>
        public string LoadText(string fileName)
        {
            //it is by default async only on UWP
            var task = Task.Run<string>(async () => await LoadTextAsync(fileName));
            return task.Result;
        }

        private async Task<string> LoadTextAsync(string fileName)
        {
            fileName = @"ms-appx:///" + fileName;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(fileName)).AsTask().ConfigureAwait(false);
            return await FileIO.ReadTextAsync(file);
        }
    }
}
