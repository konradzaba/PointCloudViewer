using Microsoft.Xna.Framework;
using PointCloudViewer.Domain;
using PointCloudViewer.FileProcessing.Abstract;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace PointCloudViewer.FileProcessing.FileProcessing
{
    public class XyzProcessing : CommonProcessing, IFileProcessing
    {
        private Vector3 _minPoint = new Vector3(float.MaxValue);
        private Vector3 _maxPoint = new Vector3(float.MinValue);

        public List<RawPoint> GetPointsFromFile(string path)
        {
            var fileService = DependencyService.Get<IFileLoading>();
            return GetPointsFromString(fileService.LoadText(path));
        }

        private List<RawPoint> GetPointsFromString(string text)
        {
            var toReturn = new List<RawPoint>();
            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] lineSplit=null;
                    if (line.Contains("\t"))
                        lineSplit = line.Split('\t');
                    else if (line.Contains(" "))
                        lineSplit = line.Split(' ');
                    var point = GetPointsFromLine(lineSplit);
                    toReturn.Add(point);
                }
            }
            return toReturn;
        }

        private RawPoint GetPointsFromLine(string[] lineSplit)
        {
            var x = float.Parse(lineSplit[0], CultureInfo.InvariantCulture);
            var z = float.Parse(lineSplit[1], CultureInfo.InvariantCulture);
            var y = float.Parse(lineSplit[2], CultureInfo.InvariantCulture);

            if (x < _minPoint.X) _minPoint.X = x;
            if (y < _minPoint.Y) _minPoint.Y = y;
            if (z < _minPoint.Z) _minPoint.Z = z;

            if (x > _maxPoint.X) _maxPoint.X = x;
            if (y > _maxPoint.Y) _maxPoint.Y = y;
            if (z > _maxPoint.Z) _maxPoint.Z = z;

            if (lineSplit.Length > 3)
            {
                var r = int.Parse(lineSplit[3], CultureInfo.InvariantCulture);
                var g = int.Parse(lineSplit[4], CultureInfo.InvariantCulture);
                var b = int.Parse(lineSplit[5], CultureInfo.InvariantCulture);
                var color = Microsoft.Xna.Framework.Color.FromNonPremultiplied(r, g, b,255);
                return new RawPoint(new Vector3(x, y, z), color);
            }
            return new RawPoint(new Vector3(x, y, z));
        }

        public Vector3 GetMinPoint()
        {
            return _minPoint;
        }

        public Vector3 GetMaxPoint()
        {
            return _maxPoint;
        }
    }
}
