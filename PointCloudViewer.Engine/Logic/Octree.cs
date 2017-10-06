using Microsoft.Xna.Framework;
using PointCloudViewer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PointCloudViewer.Engine.Logic
{
    //https://web.archive.org/web/20130313083633/http://www.xnawiki.com/index.php/Octree
    public class Octree
    {
        private int _modelsInNodes;
        private const int maxObjectsInNode = 100;
        private Vector3 _center;
        private BoundingBox? _bbox;
        private float _size;

        Octree _parent = null;
        Octree _nodeUFL;
        Octree _nodeUFR;
        Octree _nodeUBL;
        Octree _nodeUBR;
        Octree _nodeDFL;
        Octree _nodeDFR;
        Octree _nodeDBL;
        Octree _nodeDBR;

        protected List<Octree> _children;
        List<ColoredPoint> _points;
        Dictionary<Color, List<ColoredPoint>> _pointsDic;
        Dictionary<Color, List<ColoredPoint>> _pointsLevelOfDetail;

        public Octree(Vector3 center, float size, Octree parent = null)
        {
            _parent = parent;
            _center = center;
            _size = size;
            _children = new List<Octree>(8);
            _points = new List<ColoredPoint>();
            _pointsDic = new Dictionary<Color, List<ColoredPoint>>();

            var diagonalVector = new Vector3(size / 2f);
            _bbox = new BoundingBox(center - diagonalVector, center + diagonalVector);
        }

        public void ComputeLevelOfDetail()
        {
            foreach (var child in _children)
                child.ComputeLevelOfDetail();

            var tmp = new Dictionary<Color, List<ColoredPoint>>();
            foreach (var colorPoints in _pointsDic)
            {
                var lodPoints = colorPoints.Value.Where((x, i) => i % 4 == 0);
                tmp.Add(colorPoints.Key, lodPoints.ToList());
            }
            _pointsLevelOfDetail = tmp;
        }

        public float GetRadius()
        {
            return _size / 2f;
        }

        public Vector3 GetProperCenter()
        {
            return _center; //(_bbox.Value.Max - _bbox.Value.Min) / 2 + _bbox.Value.Min;
        }

        public Dictionary<Color,List<ColoredPoint>> GetPoints(BoundingFrustum boundingFrustum, Vector3 camPosition, Dictionary<Color, List<ColoredPoint>> points, bool getFullDetail)
        {
            if (_points != null && _points.Any())
            {
                var sourcePoints = getFullDetail ? _pointsDic : _pointsLevelOfDetail;
                foreach (var element in sourcePoints)
                {
                    if (!points.ContainsKey(element.Key))
                        points.Add(element.Key, new List<ColoredPoint>());
                    points[element.Key].AddRange(element.Value);
                }
            }

            foreach (var octree in _children)
            {
                var result = GetOctreeQueryResult(octree, boundingFrustum, camPosition);
                
                if (result!=OctreeQueryResult.NotVisible)
                {
                    var childPoints = octree.GetPoints(boundingFrustum, camPosition, new Dictionary<Color, List<ColoredPoint>>(),result == OctreeQueryResult.FullDetail);
                    if (childPoints != null && childPoints.Any())
                    {
                        foreach(var element in childPoints)
                        {
                            if (!points.ContainsKey(element.Key))
                                points.Add(element.Key, new List<ColoredPoint>());
                            points[element.Key].AddRange(element.Value);
                        }
                    }
                }
            }

            return points;
        }
        private OctreeQueryResult GetOctreeQueryResult(Octree octree, BoundingFrustum boundingFrustum, Vector3 camPosition)
        {
            if (!octree._bbox.HasValue) return OctreeQueryResult.NotVisible;
            if (!boundingFrustum.Intersects(octree._bbox.Value)) return OctreeQueryResult.NotVisible;

            var bbox = octree._bbox.Value;
            if (bbox.Contains(camPosition) == ContainmentType.Contains) return OctreeQueryResult.FullDetail;
            var center = (bbox.Max - bbox.Min) / 2 + bbox.Min;
            var v = camPosition - center;
            var abs = new Vector3(Math.Abs(v.X),
                Math.Abs(v.Y), Math.Abs(v.Z));
            var point = center + ((v / abs) * (_size / 2));

            //var dist = Vector2.Distance(new Vector2(point.X, point.Z), new Vector2(camPosition.X, camPosition.Z));//Vector3.Distance(point, camPosition);
            var dist = Vector3.Distance(point, camPosition);

            if (dist < EngineSettings.Instance.LevelOfDetailDistance* EngineSettings.Instance.ViewDistance)
                return OctreeQueryResult.FullDetail;
            if (dist < EngineSettings.Instance.ViewDistance)
                return OctreeQueryResult.LevelOfDetail;
            return OctreeQueryResult.NotVisible;
        }
        //private bool IsDistanceProper(Vector3 camPosition, BoundingBox bbox)
        //{
        //    if (bbox.Contains(camPosition)==ContainmentType.Contains) return true;
        //    var center = (bbox.Max - bbox.Min) / 2 + bbox.Min;
        //    var v = camPosition - center;
        //    var abs = new Vector3(Math.Abs(v.X),
        //        Math.Abs(v.Y), Math.Abs(v.Z));
        //    var point = center + ((v / abs) * (_size / 2));

        //    var dist = Vector3.Distance(point, camPosition);

        //    if (dist < 100f)
        //        return true;
        //    return false;
        //}

        private bool IsNeedForSplit()
        {
            return _points.Any() && maxObjectsInNode < _points.Count;
        }

        private bool HasChildren()
        {
            return _children.Any();
        }

        private bool IsEmpty()
        {
            return _points.Count == 0 && !HasChildren();
        }

        private bool AreChildrenEmpty()
        {
            if (!HasChildren()) return true;
            if (_children.Any(x => !x.IsEmpty())) return false;
            return true;
        }

        public bool IsContainingObjects()
        {
            if (_points.Any()) return true;
            if (_children.Any())
            {
                if (_children.Any(x => x.IsContainingObjects()))
                    return true;
            }
            return false;
        }

        private Octree FindObjectParent(ColoredPoint point)
        {
            Octree toReturn = null;
            if (_points.Any(x => x == point))
                return this;

            int child = 0;
            while ((toReturn == null) && (child < _children.Count))
            {
                toReturn = _children[child++].FindObjectParent(point);
            }

            return toReturn;
        }

        public bool AddObject(ColoredPoint octreePoint)
        {
            if (octreePoint == null) { throw new NullReferenceException("octreePoint"); }

            if (!CanContainOrIntersect(octreePoint)) return false;

            if(!HasChildren() || IsNeedForSplit())
            {
                AddObjectHere(octreePoint);

                if (IsNeedForSplit())
                {
                    CreateChildren();
                    DistributeObjectsToChildren();
                }
            }
            else
            {
                if (!AddObjectToChildren(octreePoint))
                {
                    AddObjectHere(octreePoint);
                }
            }
            return true;
        }



        private void AddObjectHere(ColoredPoint octreePoint)
        {
            if (!_pointsDic.ContainsKey(octreePoint.CurrentlyUsedColor))
                _pointsDic.Add(octreePoint.CurrentlyUsedColor, new List<ColoredPoint>());
            _points.Add(octreePoint);
            _pointsDic[octreePoint.CurrentlyUsedColor].Add(octreePoint);
            _modelsInNodes++;
        }

        private void CreateChildren()
        {
            var sizeOver2 = _size / 2f;
            var sizeOver4 = _size / 4f;

            _nodeUFR = new Octree(_center + new Vector3(sizeOver4, sizeOver4, -sizeOver4), sizeOver2, this);
            _nodeUFL = new Octree(_center + new Vector3(-sizeOver4, sizeOver4, -sizeOver4), sizeOver2, this);
            _nodeUBR = new Octree(_center + new Vector3(sizeOver4, sizeOver4, sizeOver4), sizeOver2, this);
            _nodeUBL = new Octree(_center + new Vector3(-sizeOver4, sizeOver4, sizeOver4), sizeOver2, this);
            _nodeDFR = new Octree(_center + new Vector3(sizeOver4, -sizeOver4, -sizeOver4), sizeOver2, this);
            _nodeDFL = new Octree(_center + new Vector3(-sizeOver4, -sizeOver4, -sizeOver4), sizeOver2, this);
            _nodeDBR = new Octree(_center + new Vector3(sizeOver4, -sizeOver4, sizeOver4), sizeOver2, this);
            _nodeDBL = new Octree(_center + new Vector3(-sizeOver4, -sizeOver4, sizeOver4), sizeOver2, this);

            _children.Add(_nodeUFR);
            _children.Add(_nodeUFL);
            _children.Add(_nodeUBR);
            _children.Add(_nodeUBL);
            _children.Add(_nodeDFR);
            _children.Add(_nodeDFL);
            _children.Add(_nodeDBR);
            _children.Add(_nodeDBL);
        }

        private void DistributeObjectsToChildren()
        {
            if(_children.Any() && _points.Any())
            {
                for (int i = _points.Count - 1; i >= 0; --i)
                {
                    var octreeObject = _points[i];
                    if (AddObjectToChildren(octreeObject))
                    {
                        RemoveObject(octreeObject);
                    }
                }
            }
        }

        private bool AddObjectToChildren(ColoredPoint point)
        {
            if (_children.Any())
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    if (_children[i].CanContain(point))
                    {
                        _children[i].AddObject(point);
                        return true;
                    }
                }


                var added = false;
                var counter = 0;
                for (int index = 0; index < _children.Count; index++)
                {
                    if (_children[index].CanContain(point))
                        counter++;
                }
                if (counter == _children.Count)
                    return false;

                for (int index = 0; index < _children.Count; index++)
                {
                    if (_children[index].CanContain(point))
                    {
                        _children[index].AddObject(point);
                        added = true;
                    }

                }
                return added;
            }
            return false;
        }

        public bool RemoveObject(ColoredPoint octreePoint)
        {
            if (octreePoint == null) throw new NullReferenceException("octreePoint");

            if (!CanContain(octreePoint))
                return false;

            if (RemoveObjectHere(octreePoint))
            {
                return true;
            }
            else
            {
                if (!_children.Any())
                    return false;

                foreach (var child in _children)
                    if (child.RemoveObject(octreePoint))
                        return true;
            }
            return false;
        }

        private bool RemoveObjectHere(ColoredPoint octreePoint, bool merge = true)
        {
            if (!_points.Any())
                return false;

            int index = _points.IndexOf(octreePoint);
            if (index == -1) return false;

            _pointsDic[octreePoint.CurrentlyUsedColor].Remove(octreePoint);
            RemoveObject(index, merge);
            return true;
        }

        private void RemoveObject(int pointIndex, bool merge = true)
        {
            // remove the object            
            _points.RemoveAt(pointIndex);
            _modelsInNodes--;

            if (merge == true)
            {
                // if all the children are empty lets clear the list
                if (AreChildrenEmpty())
                    _children.Clear();

                // try and collapse / merge the Octree by notifying the parent to check other children
                if (IsEmpty())
                {
                    if (_parent != null)
                        _parent.AttemptMerge(this);
                }
            }
        }

        private void AttemptMerge(Octree abandonedChild)
        {
            if (AreChildrenEmpty())
            {
                _children.Clear();

                if (_parent != null && _points.Count == 0)
                {
                    _parent.AttemptMerge(this);
                }
            }
        }

        private bool CanContain(ColoredPoint point)
        {
            return _bbox.Value.Contains(point.Position)==ContainmentType.Contains;
        }

        private bool CanContainOrIntersect(ColoredPoint octreePoint)
        {
            return CanContain(octreePoint);
        }
    }
}
