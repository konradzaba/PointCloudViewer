using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointCloudViewer.Domain;
using PointCloudViewer.Engine.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PointCloudViewer.Engine.Graphics.Point2d
{
    /// <summary>
    /// The idea behind point instances is:
    /// it is cheaper (CPU) to draw 10 points of one color, than 5 points of two colors
    /// if points share the same color, a mesh can be created in a buffer to draw them in one iteration
    /// </summary>
    class Point2dInstanced : IDisposable
    {
        private readonly Effect _billboardEffect;
        private readonly GraphicsDevice _device;
        private readonly Texture2D _texture;
        private int _polyCount;
        private IEnumerable<ColoredPoint> _oldElements;
        private const float _size = 0.2f;
        private const int VerticesPerPoint = 6;

        #region GPU settings
        private EffectParameter _worldParameter;
        private EffectParameter _viewParameter;
        private EffectParameter _projectionParameter;
        private EffectParameter _cameraPositionParameter;
        private EffectParameter _scaleParameter;
        private EffectParameter _billboardTextureParameter;
        private EffectParameter _eyeVectorParameter;
        private EffectParameter _allowedRotationDirectionParameter;

        private DynamicVertexBuffer _pointBuffer;
        #endregion

        public bool IsActive;

        public Point2dInstanced(Effect effect, GraphicsDevice device, Texture2D texture)
        {
            _billboardEffect = effect;
            _device = device;
            _texture = texture;

            InitEffectParameters();
        }

        private void InitEffectParameters()
        {
            _worldParameter = _billboardEffect.Parameters["xWorld"];
            _viewParameter = _billboardEffect.Parameters["xView"];
            _projectionParameter = _billboardEffect.Parameters["xProjection"];
            _cameraPositionParameter = _billboardEffect.Parameters["xCamPos"];
            _scaleParameter = _billboardEffect.Parameters["scale"];
            _billboardTextureParameter = _billboardEffect.Parameters["xBillboardTexture"];
            _allowedRotationDirectionParameter = _billboardEffect.Parameters["xAllowedRotDir"];
            _eyeVectorParameter = _billboardEffect.Parameters["vecEye"];

            _billboardEffect.CurrentTechnique = _billboardEffect.Techniques["CylBillboard"];
            _worldParameter.SetValue(Matrix.Identity);
            _allowedRotationDirectionParameter.SetValue(Vector3.Up);
            _scaleParameter.SetValue(_size);
        }

        
        public void UpdateFromElements(List<ColoredPoint> elements)
        {
            IsActive = true;
            var elementsCount = elements.Count();

            //cheap comparison - of course may create issues due to inaccuracy
            if (_oldElements == null || _oldElements.Count() != elementsCount)
            {
                _oldElements = elements;

                Stopwatch sw1 = new Stopwatch();
                sw1.Start();
                var vertexPositionArray = new VertexPositionTexture[elementsCount * VerticesPerPoint];
                for (int i = 0; i < elementsCount; i++)
                {
                    var element = elements[i];
                    vertexPositionArray[i * VerticesPerPoint] = element.BillboardVertices[0];
                    vertexPositionArray[i * VerticesPerPoint + 1] = element.BillboardVertices[2];
                    vertexPositionArray[i * VerticesPerPoint + 2] = element.BillboardVertices[1];
                    vertexPositionArray[i * VerticesPerPoint + 3] = element.BillboardVertices[0];
                    vertexPositionArray[i * VerticesPerPoint + 4] = element.BillboardVertices[1];
                    vertexPositionArray[i * VerticesPerPoint + 5] = element.BillboardVertices[3];
                }
                sw1.Stop();
                Stopwatch sw2 = new Stopwatch();
                sw2.Start();
                _polyCount = elementsCount * 2;
                if (_pointBuffer == null || vertexPositionArray.Length > _pointBuffer.VertexCount)
                {
                    if (_pointBuffer != null) _pointBuffer.Dispose();
                    _pointBuffer = new DynamicVertexBuffer(_device, VertexPositionTexture.VertexDeclaration, _polyCount * 3, BufferUsage.WriteOnly);
                }

                _pointBuffer.SetData(vertexPositionArray, 0, vertexPositionArray.Length, SetDataOptions.Discard);
                sw2.Stop();
                if(sw1.ElapsedMilliseconds+sw2.ElapsedMilliseconds > 33)
                    AppConsole.Instance.WriteLine($"{sw1.ElapsedMilliseconds} {sw2.ElapsedMilliseconds} elements count {elementsCount}");
            }
        }

        public void Draw(GameTime gameTime, Camera camera, bool arePersistantValuesSet)
        {
            if (IsActive)
            {
                if (!arePersistantValuesSet)
                {
                    _viewParameter.SetValue(camera.ViewMatrix);
                    _projectionParameter.SetValue(camera.ProjectionMatrix);
                    _cameraPositionParameter.SetValue(camera.CameraPosition);
                }
                _billboardTextureParameter.SetValue(_texture);

                foreach (EffectPass pass in _billboardEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    if (_pointBuffer != null)
                    {
                        _device.SetVertexBuffer(_pointBuffer);
                        _device.DrawPrimitives(PrimitiveType.TriangleList, 0, _polyCount);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_pointBuffer != null)
                _pointBuffer.Dispose();
            if (_texture != null)
                _texture.Dispose();
        }
    }
}
