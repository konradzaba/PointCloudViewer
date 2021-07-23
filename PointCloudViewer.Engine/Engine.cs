using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using PointCloudViewer.Domain;
using PointCloudViewer.Engine.Graphics;
using PointCloudViewer.Engine.Graphics.Point2d;
using PointCloudViewer.Engine.Logic;
using PointCloudViewer.FileProcessing.FileProcessing;
using PointCloudViewer.Logic.VirtualControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PointCloudViewer.Engine
{
    public class Engine : Game
    {
        private GraphicsDeviceManager _graphics;
        private Camera _camera;
        private Point2dSystem _point2dSystem;
        private Octree _octree;
        private AppInterface _interface;
        private Vector3 _oldCameraPosition;
        private float _oldLeftRightValue;
        private Task _updateTask;
        private object _locker = new object();
        private bool _isPaused = false;
        private KeyboardState _oldKeyState;
        private IList<Color> _allRealColors;
        private Dictionary<Color,List<ColoredPoint>> _points;
        private RenderTarget2D _renderTarget;
        public Engine()
        {
            _graphics = new GraphicsDeviceManager(this);
            //_graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            //it would be better to check if it is mobile
            _graphics.IsFullScreen = !EngineSettings.Instance.IsDeviceWithKeyboard;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / EngineSettings.Instance.LimitFps);//24 fps (1/25 => 0.04)

            this.IsFixedTimeStep = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            var xRes = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * EngineSettings.Instance.ResolutionScaling;
            var yRes = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * EngineSettings.Instance.ResolutionScaling;
            _renderTarget = new RenderTarget2D(GraphicsDevice, (int)xRes, (int)yRes);
            var processor = FactoryProcessing.GetFileProcessor(FileProcessing.Abstract.SupportedFile.XYZ);
            var points = processor.GetPointsFromFile($"Data/{EngineSettings.Instance.PointCloudName}.xyz");
            points = NormalizePoints(points, processor.GetMinPoint());
            
            #region compute world size
            var min = Vector3.Zero;
            var max = processor.GetMaxPoint()-processor.GetMinPoint();
            var centerOfWorld = new Vector3(min.X + (max.X - min.X) * 0.5f,
                min.Y + (max.Y - min.Y) * 0.5f,
                min.Z + (max.Z - min.Z) * 0.5f);
            var worldSize = new List<float>() { max.X, max.Y, max.Z }.Max();
            #endregion

            #region compute octree
            _octree = new Octree(centerOfWorld, worldSize);
            _points = new Dictionary<Color, List<ColoredPoint>>();
            _allRealColors = new List<Color>();
            Parallel.ForEach(points, point => 
            {
                var coloredPoint = new Domain.ColoredPoint(point);
                lock (_locker)
                {
                    if (!_allRealColors.Contains(coloredPoint.CurrentlyUsedColor))
                        _allRealColors.Add(coloredPoint.CurrentlyUsedColor);
                    _octree.AddObject(coloredPoint);
                }
            });
            _octree.ComputeLevelOfDetail();
            #endregion

            #region init camera
            _camera = new Camera(GraphicsDevice,_octree);
            _camera.SetPosition(centerOfWorld.X, centerOfWorld.Y+5f, centerOfWorld.Z, -1.6f, -0.25f);
            _camera.UpdateCamera(0,_isPaused);
            #endregion

            base.Initialize();
        }
        /// <summary>
        /// Points have real geographical values which are not good for addressing in 3D environment.
        /// Here normalization is performed against the origin of 3D space (Vector3.Zero)
        /// </summary>
        /// <param name="points"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        private List<RawPoint> NormalizePoints(List<RawPoint> points, Vector3 min)
        {
            object locker=new object();
            var toReturn = new List<RawPoint>();
            Parallel.ForEach(points, p =>
            {
                var normalized = p.Position - min;
                var correctedPoint = p.RealColor.HasValue ? new RawPoint(normalized, SimplifyColor(p.RealColor.Value)) : new RawPoint(normalized);
                lock (locker)
                {
                    toReturn.Add(correctedPoint);
                }
            });
            return toReturn;
        }

        /// <summary>
        /// The more points colors, the more instances need to be drawed what is detrimental for performance.
        /// Therefore simplification allows find colors that are close and assign a single color in such case.
        /// Too large simplification (example: less than 5 colors) would also be bad for performance, because
        /// the data portions (see Point2dSystem) would be to large to consume during 33 ms.
        /// </summary>
        /// <param name="realColor"></param>
        /// <returns></returns>
        private Color SimplifyColor(Color realColor)
        {
            var divider = 256 / EngineSettings.Instance.ColorQuality;
            int red = (realColor.R * EngineSettings.Instance.ColorQuality) / 256;
            int green = (realColor.G * EngineSettings.Instance.ColorQuality) / 256;
            int blue = (realColor.B * EngineSettings.Instance.ColorQuality) / 256;
            return Color.FromNonPremultiplied(red*divider, green*divider, blue*divider,255);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            var font = Content.Load<SpriteFont>("font");
            var uiDisplay = Content.Load<Texture2D>("interface");
            _interface = new AppInterface(GraphicsDevice, font, uiDisplay);

            /*
             * Points are displayed as 2D so-called billboards. 
             * Each point is represented as a flat square composed of two triangles.
             * When camera moves, these flat squares rotate to be always in front of the camera.
             * Drawing such amount of points as 3D models would be impossible on mobile devices.
             * This effect was used in old video games - notice how the trees are rendered: https://www.youtube.com/watch?v=h-U889j84O8
             * */
            var billBoardEffect = Content.Load<Effect>("BillBoard");

            _point2dSystem = new Point2dSystem(_camera, billBoardEffect, GraphicsDevice, _allRealColors);
        }

        /// <summary>
        /// Used to clean resources on exit.
        /// </summary>
        protected override void UnloadContent()
        {
            if(_point2dSystem!=null)
                _point2dSystem.Dispose();
        }

        /// <summary>
        /// All logic is run each frame in this function.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            ProcessInput(timeDifference);

            _camera.UpdateCamera(timeDifference,_isPaused);
            
            if (_oldCameraPosition != _camera.CameraPosition ||
                _oldLeftRightValue!=_camera.LeftrightRot ||
                _updateTask!=null)
            {
                if (_camera.BoundingFrustum != null)
                {
                    /*performance trick:
                    Getting points from octree takes a lot of time
                    Create a task and don't wait for it
                    It will be finished in a couple of frames
                    And the performance won't go down*/
                    if (_updateTask == null)
                    {
                        _updateTask = new Task(() =>
                          {
                              _points = _octree.GetPoints(_camera.BoundingFrustum, _camera.CameraPosition, new Dictionary<Color, List<ColoredPoint>>(),true);
                              _point2dSystem.TurnOffUnnecessaryColors(_points);
                              _point2dSystem.PreparePortionedUpdate(_points);
                          });
                        _updateTask.Start();
                    }
                    if (_updateTask.IsCompleted)
                    {
                        
                        if (_point2dSystem.Update(_points))
                        {
                            _updateTask = null;
                        }
                    }
                }
                _oldCameraPosition = _camera.CameraPosition;
                _oldLeftRightValue = _camera.LeftrightRot;
            }

            sw.Stop();
            
            if(sw.ElapsedMilliseconds>33)//30fps
                AppConsole.Instance.WriteLine($"UPDATE {sw.ElapsedMilliseconds}");
            base.Update(gameTime);
        }

        private void ProcessInput(float amount)
        {
            var virtualButton = _interface.ProcessVirtualButtonInput(TouchPanel.GetState());
            var keyState = Keyboard.GetState();
            var currentMouseState = Mouse.GetState();

            var moveVector = Vector3.Zero;

            if (keyState.IsKeyDown(Keys.Q) || virtualButton == VirtualButtonKind.Higher)
                _camera.UpdateHeight(amount);
            if (keyState.IsKeyDown(Keys.W) || virtualButton == VirtualButtonKind.Lower)
                _camera.UpdateHeight(-amount);

            if (keyState.IsKeyDown(Keys.Z) || virtualButton == VirtualButtonKind.Closer)
                _camera.UpdateRadius(-amount);
            if (keyState.IsKeyDown(Keys.X) || virtualButton == VirtualButtonKind.Further)
                _camera.UpdateRadius(amount);

            if (keyState.IsKeyDown(Keys.Escape))
                Exit();


            if (keyState.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
                _isPaused = !_isPaused;

            _camera.AddToCameraPosition(moveVector * amount);
            _oldKeyState = keyState;
        }


        /// <summary>
        /// This is called when the next frame should be drawed.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(EngineSettings.Instance.BackgroundColor);
            _point2dSystem.Draw(gameTime);
            GraphicsDevice.SetRenderTarget(null);

            _interface.DrawInterface(GraphicsDevice, _renderTarget, gameTime);

            base.Draw(gameTime);
        }
    }
}
