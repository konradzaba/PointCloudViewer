using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PointCloudViewer.Engine.Logic
{
    public class Camera
    {
        private readonly GraphicsDevice _device;
        private readonly Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
        private const float MoveSpeed = 10.0f;

        public float LeftrightRot = MathHelper.PiOver2;
        public float UpdownRot = -MathHelper.Pi / 10.0f;

        /// <summary>
        /// What camera can 'see'
        /// </summary>
        public BoundingFrustum BoundingFrustum;
        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;

        public Vector3 CameraPosition;

        #region flyby camera

        private double _movementAround;
        public double MovementAround
        {
            get { return _movementAround; }
            set
            {
                if (value < 0)
                    value += 2 * Math.PI;
                _movementAround = value % (2 * Math.PI);
            }
        }

        private Vector3 _center;
        private float _radius;

        #endregion

        public Camera(GraphicsDevice device, Octree octree)
        {
            this._device = device;
            CameraPosition = new Vector3(12, 4, -4.5f);
            SetUpCamera();
            LeftrightRot = -1.6f;
            UpdownRot = -0.25f;

            _center = octree.GetProperCenter();
            _radius = octree.GetRadius() + 25;
        }

        private void SetUpCamera()
        {
            ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 4, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _device.Viewport.AspectRatio, 0.1f, 500.0f);
        }

        public void AddToCameraPosition(Vector3 vectorToAdd)
        {
            if (vectorToAdd != Vector3.Zero)
            {
                Matrix cameraRotation = Matrix.CreateRotationX(UpdownRot) * Matrix.CreateRotationY(LeftrightRot);
                Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
                CameraPosition += MoveSpeed * rotatedVector;
            }
        }

        public void UpdateCamera(float changeAmount, bool isPaused)
        {
            if (!isPaused)
            {
                MovementAround += changeAmount * EngineSettings.Instance.CameraSpeed;
            }
            CameraPosition = new Vector3((float)(_radius * Math.Cos(MovementAround) + _center.X),
                CameraPosition.Y,
                -(float)(_radius * Math.Sin(MovementAround) - _center.Z));
            ViewMatrix = Matrix.CreateLookAt(CameraPosition,
                new Vector3(_center.X, _center.Y, _center.Z), cameraOriginalUpVector);

            BoundingFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
        }

        internal void SetPosition(float x, float y, float z, float leftRight, float upDown)
        {
            CameraPosition = new Vector3(x, y, z);
            LeftrightRot = leftRight;
            UpdownRot = upDown;
        }

        internal void UpdateRadius(float amount)
        {
            _radius += amount * 10;

            //limits:
            if (_radius < 35f) _radius = 35f;
            if (_radius > 140f) _radius = 140f;

            AppConsole.Instance.WriteLine(_radius.ToString());
        }

        internal void UpdateHeight(float amount)
        {
            AddToCameraPosition(new Vector3(0, amount, 0));
            _center = new Vector3(_center.X, _center.Y+amount, _center.Z);
        }
    }
}
