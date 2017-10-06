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
    class Point2dSystem : IDisposable
    {
        private readonly Camera _camera;
        private readonly Effect _billboardEffect;
        private readonly GraphicsDevice _device;
        private readonly Dictionary<Color, Point2dInstanced> _instances;
        private IEnumerable<Color> _allColors;
        private List<List<Color>> _portionedUpdate;
        private const int NoTriangles = 2;

        /// <summary>
        /// Depends on CPU speed, the larger the points number the quicker the new points are on screen.
        /// </summary>
        private const int PointsPerPortion = 4000;

        public Point2dSystem(Camera camera, Effect effect, GraphicsDevice device, IEnumerable<Color> allColors)
        {
            _camera = camera;
            _billboardEffect = effect;
            _device = device;
            _instances = new Dictionary<Color, Point2dInstanced>();

            foreach (var color in allColors)
            {
                _instances.Add(color, new Point2dInstanced(_billboardEffect, _device, CreateTexture(_device, 16, 16, pixel => color)));
            }
        }

        public Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
        {
            //initialize a texture
            Texture2D texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = paint(pixel);
            }

            //set the color
            texture.SetData(data);
            
            return texture;
        }
        public void TurnOffUnnecessaryColors(Dictionary<Color, List<ColoredPoint>> points)
        {
            _allColors = points.Select(x => x.Key).ToList();
            var unnecessaryColors = _instances.Select(x => x.Key).Except(_allColors);
            if (unnecessaryColors.Any())
            {
                //do something with unnecessary colors - turn off active or dispose them?
                foreach (var colorInstance in unnecessaryColors)
                    _instances[colorInstance].IsActive = false;
            }
        }

        /// <summary>
        /// Data to be updated is portioned due to low CPU performance on mobiles.
        /// </summary>
        /// <param name="_points"></param>
        public void PreparePortionedUpdate(Dictionary<Color, List<ColoredPoint>> _points)
        {
            _portionedUpdate = new List<List<Color>>();

            var singlePortion = new List<Color>();
            var singlePortionCount = 0;
            foreach(var color in _allColors)
            {
                if(singlePortionCount + _points[color].Count < PointsPerPortion)
                {
                    singlePortion.Add(color);
                    singlePortionCount += _points[color].Count;
                }
                else
                {
                    _portionedUpdate.Add(singlePortion);
                    singlePortion = new List<Color>();
                    singlePortion.Add(color);
                    singlePortionCount = _points[color].Count;
                }
            }
            if (singlePortion.Any()) _portionedUpdate.Add(singlePortion);
        }

        /// <summary>
        /// Process one of the portions of data in order to update the point instances
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool Update(Dictionary<Color, List<ColoredPoint>> points)
        {
            var colors = _portionedUpdate.FirstOrDefault();
            if (colors != null)
            {
                foreach (var color in colors)
                {
                    _instances[color].UpdateFromElements(points[color]);
                }
                _portionedUpdate = _portionedUpdate.Skip(1).ToList();
            }
            return !_portionedUpdate.Any();
        }

        public void Draw(GameTime gameTime)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var arePersistantValuesSet = false;
            foreach (var instance in _instances)
            {
                instance.Value.Draw(gameTime, _camera, arePersistantValuesSet);
                arePersistantValuesSet = true;
            }
            sw.Stop();
            if(sw.ElapsedMilliseconds>33)
                AppConsole.Instance.WriteLine($"DRAW {sw.ElapsedMilliseconds}");
        }

        public void Dispose()
        {
            if (_instances.Any())
            {
                foreach(var instance in _instances)
                {
                    instance.Value.Dispose();
                }
            }
        }
    }
}
