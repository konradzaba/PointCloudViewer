using Microsoft.Xna.Framework;

namespace PointCloudViewer.Domain
{
    public class RawPoint
    {
        public Vector3 Position;
        public Color? RealColor;

        public RawPoint(Vector3 position, Color realColor)
        {
            Position = position;
            RealColor = realColor;
        }

        public RawPoint(Vector3 position)
        {
            Position = position;
        }
    }
}
