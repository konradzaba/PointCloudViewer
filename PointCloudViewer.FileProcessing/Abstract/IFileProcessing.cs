using Microsoft.Xna.Framework;
using PointCloudViewer.Domain;
using System.Collections.Generic;

namespace PointCloudViewer.FileProcessing.Abstract
{
    public interface IFileProcessing
    {
        Vector3 GetMinPoint();
        Vector3 GetMaxPoint();
        List<RawPoint> GetPointsFromFile(string path);
    }
}
