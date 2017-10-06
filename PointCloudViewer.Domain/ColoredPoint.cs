using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PointCloudViewer.Domain
{
    public class ColoredPoint
    {
        public Vector3 Position;
        public Color CurrentlyUsedColor;

        public VertexPositionTexture[] BillboardVertices { get; set; }

        public ColoredPoint(RawPoint pos)
        {
            Position = pos.Position;
            CurrentlyUsedColor = Color.Gray;
            if (pos.RealColor.HasValue)
            {
                CurrentlyUsedColor = pos.RealColor.Value;
            }

            BillboardVertices = new VertexPositionTexture[4];

            BillboardVertices[0] = new VertexPositionTexture(Position, Vector2.Zero);
            BillboardVertices[1] = new VertexPositionTexture(Position, Vector2.One);
            BillboardVertices[2] = new VertexPositionTexture(Position, Vector2.UnitX);
            BillboardVertices[3] = new VertexPositionTexture(Position, Vector2.UnitY);
        }
    }
}
