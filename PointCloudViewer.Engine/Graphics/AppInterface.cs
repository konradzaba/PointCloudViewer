using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointCloudViewer.Engine.Logic;
using System;
using Microsoft.Xna.Framework.Input.Touch;
using PointCloudViewer.Logic.VirtualControls;
using System.Collections.Generic;
using System.Linq;

namespace PointCloudViewer.Engine.Graphics
{
    /// <summary>
    /// Used to draw 2D UI overlay on top of the 3D visualizatin
    /// </summary>
    class AppInterface
    {
        private readonly SpriteFont _font;
        private float _fps;
        private float _totalTime;
        private float _displayFps;
        private readonly SpriteBatch _sprite;
        private int _xRes;
        private int _yRes;
        private Texture2D _uiTexture;
        private Matrix _sizeTransformation;
        private IEnumerable<VirtualButton> _buttons;

        public AppInterface(GraphicsDevice device, SpriteFont font, Texture2D uiTexture)
        {
            _displayFps = 0f;
            _sprite = new SpriteBatch(device);
            _font = font;
            _uiTexture = uiTexture;
            _xRes = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _yRes = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            #region prepare virtual buttons controls
            Vector2 baseScreenSize = new Vector2(800, 480);
            float horScaling = baseScreenSize.X / _xRes;//_xRes / baseScreenSize.X;
            float verScaling = baseScreenSize.Y / _yRes;//_yRes / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            _sizeTransformation = Matrix.CreateScale(screenScalingFactor);
            _buttons = new List<VirtualButton>()
            {
                new VirtualButton(new Rectangle(33,298,65,65),VirtualButtonKind.Closer),
                new VirtualButton(new Rectangle(33,403,65,65),VirtualButtonKind.Further),
                new VirtualButton(new Rectangle(704,298,65,65), VirtualButtonKind.Higher),
                new VirtualButton(new Rectangle(704,403,65,65),VirtualButtonKind.Lower),
                new VirtualButton(new Rectangle(0,0,65,65),VirtualButtonKind.Exit)
            };
            #endregion
        }

        private void DrawFps(GameTime gameTime, SpriteBatch sprite)
        {
            if (EngineSettings.Instance.ShowFps)
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _totalTime += elapsedTime;

                if (_totalTime >= 1)
                {
                    _displayFps = _fps;
                    _fps = 0;
                    _totalTime = 0;
                }
                _fps += 1;

                var toDisplay = _displayFps + "FPS";
                sprite.DrawString(_font, toDisplay, new Vector2((_xRes / 2) - 70, 20), Color.Red);
            }
        }

        /// <summary>
        /// Draws AppConsole for debugging
        /// </summary>
        /// <param name="sprite"></param>
        private void DrawConsole(SpriteBatch sprite)
        {
            if (EngineSettings.Instance.ShowConsole)
            {
                int y = 0;

                foreach (String message in AppConsole.Instance.GetLastMessages())
                {
                    sprite.DrawString(_font, message, new Vector2(0, y), Color.Blue);
                    y += 20;
                }
            }
        }

        /// <summary>
        /// Draws the entire UI overlay
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="gameTime"></param>
        public void DrawInterface(GraphicsDevice graphicsDevice, RenderTarget2D renderTarget, GameTime gameTime)
        {
            _sprite.Begin();
            _sprite.Draw(renderTarget, new Rectangle(0, 0, _xRes, _yRes), Color.White);
            if (!EngineSettings.Instance.IsDeviceWithKeyboard)
                _sprite.Draw(_uiTexture, new Rectangle(0, 0, _xRes, _yRes), Color.White);
            DrawFps(gameTime, _sprite);
            DrawConsole(_sprite);
            _sprite.End();
        }

        internal VirtualButtonKind? ProcessVirtualButtonInput(TouchCollection touchCol)
        {
            foreach (var touch in touchCol)
            {
                AppConsole.Instance.WriteLine($"{touch.Position.X} {touch.Position.Y}");

                //Scale the touch position to be in screen texture coordinates
                Vector2 pos = touch.Position;
                Vector2.Transform(ref pos, ref _sizeTransformation, out pos);
                var buttonClicked = _buttons.FirstOrDefault(x => x.IsClickedOnButton(pos));
                if (buttonClicked != null) return buttonClicked.ButtonKind;
            }
            return null;
        }
    }
}
