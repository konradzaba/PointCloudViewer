using Microsoft.Xna.Framework;

namespace PointCloudViewer.Logic.VirtualControls
{
    class VirtualButton
    {
        public VirtualButtonKind ButtonKind { get; private set; }
        private Rectangle _buttonPosition;

        public VirtualButton(Rectangle position, VirtualButtonKind kind)
        {
            _buttonPosition = position;
            ButtonKind = kind;
        }

        internal bool IsClickedOnButton(Vector2 pos)
        {
            return _buttonPosition.Contains(pos);
        }
    }
}
