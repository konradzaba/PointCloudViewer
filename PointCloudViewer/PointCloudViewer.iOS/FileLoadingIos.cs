using PointCloudViewer.iOS;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.IO;
using Foundation;
using System.Linq;
using PointCloudViewer.FileProcessing.Abstract;

[assembly: Dependency(typeof(FileLoadingIos))]
namespace PointCloudViewer.iOS
{
    public class FileLoadingIos : IFileLoading
    {
        public string LoadText(string filename)
        {
            string path = CreatePathToFile(filename);
            using (StreamReader sr = File.OpenText(path))
                return sr.ReadToEnd();
        }

        static string CreatePathToFile(string fileName)
        {
            return Path.Combine(DocumentsPath, fileName);
        }

        public static string DocumentsPath
        {
            get
            {
                var documentsDirUrl = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User).Last();
                return documentsDirUrl.Path;
            }
        }
    }
}
