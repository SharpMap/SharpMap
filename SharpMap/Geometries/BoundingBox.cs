// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.ObjectModel;
using System.IO;

namespace SharpMap.Geometries
{
    /// <summary>
    /// Bounding box type with double precision
    /// </summary>
    /// <remarks>
    /// The Bounding Box represents a box whose sides are parallel to the two axes of the coordinate system.
    /// </remarks>
    [Serializable]
    public class BoundingBox : IEquatable<BoundingBox>
    {
        private Point _max;
        private Point _min;

        /// <summary>
        /// Initializes a bounding box
        /// </summary>
        /// <remarks>
        /// In case min values are larger than max values, the parameters will be swapped to ensure correct min/max boundary
        /// </remarks>
        /// <param name="minX">left</param>
        /// <param name="minY">bottom</param>
        /// <param name="maxX">right</param>
        /// <param name="maxY">top</param>
        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            _min = new Point(minX, minY);
            _max = new Point(maxX, maxY);
            CheckMinMax();
        }

        internal BoundingBox(BinaryReader br)
        {
            var bytes = new byte[32];
            br.Read(bytes, 0, 32);

            var doubles = new double[4];
            Buffer.BlockCopy(bytes, 0, doubles, 0, 32);
            
            _min = new Point(doubles[0], doubles[1]);
            _max = new Point(doubles[2], doubles[3]);
        }

        /// <summary>
        /// Initializes a bounding box
        /// </summary>
        /// <param name="lowerLeft">Lower left corner</param>
        /// <param name="upperRight">Upper right corner</param>
        public BoundingBox(Point lowerLeft, Point upperRight)
            : this(lowerLeft.X, lowerLeft.Y, upperRight.X, upperRight.Y)
        {
        }

        /// <summary>
        /// Initializes a new Bounding Box based on the bounds from a set of geometries
        /// </summary>
        /// <param name="objects">list of objects</param>
        public BoundingBox(Collection<Geometry> objects)
        {
            if (objects == null || objects.Count == 0)
            {
                _min = null;
                _max = null;
                return;
            }
            _min = objects[0].GetBoundingBox().Min.Clone();
            _max = objects[0].GetBoundingBox().Max.Clone();
            CheckMinMax();
            for (int i = 1; i < objects.Count; i++)
            {
                BoundingBox box = objects[i].GetBoundingBox();
                _min.X = Math.Min(box.Min.X, Min.X);
                _min.Y = Math.Min(box.Min.Y, Min.Y);
                _max.X = Math.Max(box.Max.X, Max.X);
                _max.Y = Math.Max(box.Max.Y, Max.Y);
            }
        }

        /// <summary>
        /// Initializes a new Bounding Box based on the bounds from a set of bounding boxes
        /// </summary>
        /// <param name="objects">list of objects</param>
        public BoundingBox(Collection<BoundingBox> objects)
        {
            if (objects.Count == 0)
            {
                _max = null;
                _min = null;
            }
            else
            {
                _min = objects[0].Min.Clone();
                _max = objects[0].Max.Clone();
                for (int i = 1; i < objects.Count; i++)
                {
                    _min.X = Math.Min(objects[i].Min.X, Min.X);
                    _min.Y = Math.Min(objects[i].Min.Y, Min.Y);
                    _max.X = Math.Max(objects[i].Max.X, Max.X);
                    _max.Y = Math.Max(objects[i].Max.Y, Max.Y);
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower left corner.
        /// </summary>
        public Point Min
        {
            get { return _min; }
            set
            {
                _min = value;
                _centroid = null;
            }
        }

        /// <summary>
        /// Gets or sets the upper right corner.
        /// </summary>
        public Point Max
        {
            get { return _max; }
            set 
            { 
                _max = value;
                _centroid = null;
            }
        }

        /// <summary>
        /// Gets the left boundary
        /// </summary>
        public Double Left
        {
            get { return _min.X; }
        }

        /// <summary>
        /// Gets the right boundary
        /// </summary>
        public Double Right
        {
            get { return _max.X; }
        }

        /// <summary>
        /// Gets the top boundary
        /// </summary>
        public Double Top
        {
            get { return _max.Y; }
        }

        /// <summary>
        /// Gets the bottom boundary
        /// </summary>
        public Double Bottom
        {
            get { return _min.Y; }
        }

        /// <summary>
        /// The upper left <see cref="Point"/> of this bounding box
        /// </summary>
        public Point TopLeft
        {
            get { return new Point(Left, Top); }
        }

        /// <summary>
        /// The upper right <see cref="Point"/> of this bounding box
        /// </summary>
        public Point TopRight
        {
            get { return new Point(Right, Top); }
        }

        /// <summary>
        /// The lower left <see cref="Point"/> of this bounding box
        /// </summary>
        public Point BottomLeft
        {
            get { return new Point(Left, Bottom); }
        }

        /// <summary>
        /// The lower right <see cref="Point"/> of this bounding box
        /// </summary>
        public Point BottomRight
        {
            get { return new Point(Right, Bottom); }
        }

        /// <summary>
        /// Returns the width of the bounding box
        /// </summary>
        /// <returns>Width of boundingbox</returns>
        public double Width
        {
            get { return Math.Abs(_max.X - _min.X); }
        }

        /// <summary>
        /// Returns the height of the bounding box
        /// </summary>
        /// <returns>Height of boundingbox</returns>
        public double Height
        {
            get { return Math.Abs(_max.Y - _min.Y); }
        }

        /// <summary>
        /// Intersection scalar (used for weighting in building the tree) 
        /// </summary>
        public uint LongestAxis
        {
            get
            {
                Point boxdim = Max - Min;
                uint la = 0; // longest axis
                double lav = 0; // longest axis length
                // for each dimension  
                for (uint ii = 0; ii < 2; ii++)
                {
                    // check if its longer
                    if (boxdim[ii] > lav)
                    {
                        // store it if it is
                        la = ii;
                        lav = boxdim[ii];
                    }
                }
                return la;
            }
        }

        /// <summary>
        /// This bounding box only contains valid ordinates
        /// </summary>
        public bool IsValid
        {
            get
            {
                return (!double.IsNaN(_min.X) && !double.IsNaN(_max.X) &&
                        !double.IsNaN(_min.Y) && !double.IsNaN(_max.Y));
            }
            //set { throw new NotImplementedException(); }
        }

        #region IEquatable<BoundingBox> Members

        /// <summary>
        /// Checks whether the values of this instance is equal to the values of another instance.
        /// </summary>
        /// <param name="other"><see cref="BoundingBox"/> to compare to.</param>
        /// <returns>True if equal</returns>
        public bool Equals(BoundingBox other)
        {
            if (other == null) return false;
            return Left == other.Left && Right == other.Right && Top == other.Top && Bottom == other.Bottom;
        }

        #endregion

        /// <summary>
        /// Moves/translates the <see cref="BoundingBox"/> along the the specified vector
        /// </summary>
        /// <param name="vector">Offset vector</param>
        public void Offset(Point vector)
        {
            _min += vector;
            _max += vector;
        }

        /// <summary>
        /// Checks whether min values are actually smaller than max values and in that case swaps them.
        /// </summary>
        /// <returns>true if the bounding was changed</returns>
        public bool CheckMinMax()
        {
            bool wasSwapped = false;
            if (_min.X > _max.X)
            {
                double tmp = _min.X;
                _min.X = _max.X;
                _max.X = tmp;
                wasSwapped = true;
            }
            if (_min.Y > _max.Y)
            {
                double tmp = _min.Y;
                _min.Y = _max.Y;
                _max.Y = tmp;
                wasSwapped = true;
            }
            return wasSwapped;
        }

        /// <summary>
        /// Determines whether the boundingbox intersects another boundingbox
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool Intersects(BoundingBox box)
        {
            return !(box.Min.X > Max.X ||
                     box.Max.X < Min.X ||
                     box.Min.Y > Max.Y ||
                     box.Max.Y < Min.Y);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> intersects the geometry
        /// </summary>
        /// <param name="g">Geometry</param>
        /// <returns>True if intersects</returns>
        public bool Intersects(Geometry g)
        {
            return Touches(g);
        }

        /// <summary>
        /// Returns true if this instance touches the <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="r"><see cref="BoundingBox"/></param>
        /// <returns>True it touches</returns>
        public bool Touches(BoundingBox r)
        {
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if ((Min[cIndex] > r.Min[cIndex] && Min[cIndex] < r.Min[cIndex]) ||
                    (Max[cIndex] > r.Max[cIndex] && Max[cIndex] < r.Max[cIndex]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> touches the geometry
        /// </summary>
        /// <param name="s">Geometry</param>
        /// <returns>True if touches</returns>
        public bool Touches(Geometry s)
        {
            if (s is Point) return Touches(s as Point);
            throw new NotImplementedException("Touches: Not implemented on this geometry type");
        }

        /// <summary>
        /// Returns true if this instance contains the <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="r"><see cref="BoundingBox"/></param>
        /// <returns>True it contains</returns>
        public bool Contains(BoundingBox r)
        {
            for (uint cIndex = 0; cIndex < 2; cIndex++)
                if (Min[cIndex] > r.Min[cIndex] || Max[cIndex] < r.Max[cIndex]) return false;

            return true;
        }

        /// <summary>
        /// Returns true if this instance contains the geometry
        /// </summary>
        /// <param name="s"><see cref="BoundingBox"/></param>
        /// <returns>True it contains</returns>
        public bool Contains(Geometry s)
        {
            if (s is Point) return Contains(s as Point);
            throw new NotImplementedException("Contains: Not implemented on these geometries");
        }

        /// <summary>
        /// Returns true if this instance touches the <see cref="Point"/>
        /// </summary>
        /// <param name="p">Geometry</param>
        /// <returns>True if touches</returns>
        public bool Touches(Point p)
        {
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if ((Min[cIndex] > p[cIndex] && Min[cIndex] < p[cIndex]) ||
                    (Max[cIndex] > p[cIndex] && Max[cIndex] < p[cIndex]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the area of the BoundingBox
        /// </summary>
        /// <returns>Area of box</returns>
        public double GetArea()
        {
            return Width*Height;
        }

        /// <summary>
        /// Gets the intersecting area between two boundingboxes
        /// </summary>
        /// <param name="r">BoundingBox</param>
        /// <returns>Area</returns>
        public double GetIntersectingArea(BoundingBox r)
        {
            uint cIndex;
            for (cIndex = 0; cIndex < 2; cIndex++)
                if (Min[cIndex] > r.Max[cIndex] || Max[cIndex] < r.Min[cIndex]) return 0.0;

            double ret = 1.0;
            double f1, f2;

            for (cIndex = 0; cIndex < 2; cIndex++)
            {
                f1 = Math.Max(Min[cIndex], r.Min[cIndex]);
                f2 = Math.Min(Max[cIndex], r.Max[cIndex]);
                ret *= f2 - f1;
            }
            return ret;
        }

        /// <summary>
        /// Computes the joined boundingbox of this instance and another boundingbox
        /// </summary>
        /// <param name="box">Boundingbox to join with</param>
        /// <returns>Boundingbox containing both boundingboxes</returns>
        public BoundingBox Join(BoundingBox box)
        {
            if (box == null)
                return Clone();

            return new BoundingBox(Math.Min(Min.X, box.Min.X), Math.Min(Min.Y, box.Min.Y),
                                   Math.Max(Max.X, box.Max.X), Math.Max(Max.Y, box.Max.Y));
        }

        /// <summary>
        /// Computes the joined boundingbox of two boundingboxes
        /// </summary>
        /// <param name="box1"></param>
        /// <param name="box2"></param>
        /// <returns></returns>
        public static BoundingBox Join(BoundingBox box1, BoundingBox box2)
        {
            if (box1 == null && box2 == null)
                return null;
            if (box1 == null)
                return box2.Clone();
            
            return box1.Join(box2);
        }

        /// <summary>
        /// Computes the joined <see cref="BoundingBox"/> of an array of boundingboxes.
        /// </summary>
        /// <param name="boxes">Boxes to join</param>
        /// <returns>Combined BoundingBox</returns>
        public static BoundingBox Join(BoundingBox[] boxes)
        {
            if (boxes == null) return null;
            if (boxes.Length == 1) return boxes[0];
            BoundingBox box = boxes[0].Clone();
            for (int i = 1; i < boxes.Length; i++)
                box = box.Join(boxes[i]);
            return box;
        }

        /// <summary>
        /// Increases the size of the boundingbox by the givent amount in all directions
        /// </summary>
        /// <param name="amount">Amount to grow in all directions</param>
        public BoundingBox Grow(double amount)
        {
            BoundingBox box = Clone();
            box.Min.X -= amount;
            box.Min.Y -= amount;
            box.Max.X += amount;
            box.Max.Y += amount;
            box.CheckMinMax();
            return box;
        }

        /// <summary>
        /// Increases the size of the boundingbox by the givent amount in horizontal and vertical directions
        /// </summary>
        /// <param name="amountInX">Amount to grow in horizontal direction</param>
        /// <param name="amountInY">Amount to grow in vertical direction</param>
        public BoundingBox Grow(double amountInX, double amountInY)
        {
            BoundingBox box = Clone();
            box.Min.X -= amountInX;
            box.Min.Y -= amountInY;
            box.Max.X += amountInX;
            box.Max.Y += amountInY;
            box.CheckMinMax();
            return box;
        }

        /// <summary>
        /// Checks whether a point lies within the bounding box
        /// </summary>
        /// <param name="p">Point</param>
        /// <returns>true if point is within</returns>
        public bool Contains(Point p)
        {
            if (Max.X < p.X)
                return false;
            if (Min.X > p.X)
                return false;
            if (Max.Y < p.Y)
                return false;
            if (Min.Y > p.Y)
                return false;
            return true;
        }

        /// <summary> 
        /// Computes the minimum distance between this and another <see cref="BoundingBox"/>.
        /// The distance between overlapping bounding boxes is 0.  Otherwise, the
        /// distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <param name="box">Box to calculate distance to</param>
        /// <returns>The distance between this and another <see cref="BoundingBox"/>.</returns>
        public virtual double Distance(BoundingBox box)
        {
            double ret = 0.0;
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                double x = 0.0;

                if (box.Max[cIndex] < Min[cIndex]) x = Math.Abs(box.Max[cIndex] - Min[cIndex]);
                else if (Max[cIndex] < box.Min[cIndex]) x = Math.Abs(box.Min[cIndex] - Max[cIndex]);
                ret += x*x;
            }
            return Math.Sqrt(ret);
        }

        /// <summary>
        /// Computes the minimum distance between this BoundingBox and a <see cref="Point"/>
        /// </summary>
        /// <param name="p"><see cref="Point"/> to calculate distance to.</param>
        /// <returns>Minimum distance.</returns>
        public virtual double Distance(Point p)
        {
            double ret = 0.0;

            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if (p[cIndex] < Min[cIndex]) ret += Math.Pow(Min[cIndex] - p[cIndex], 2.0);
                else if (p[cIndex] > Max[cIndex]) ret += Math.Pow(p[cIndex] - Max[cIndex], 2.0);
            }

            return Math.Sqrt(ret);
        }

        private Point _centroid ;
        /// <summary>
        /// Returns the center of the bounding box
        /// </summary>
        public Point GetCentroid()
        {
            return (_centroid ?? (_centroid = (_min + _max) * .5f));
        }

        /// <summary>
        /// Creates a copy of the BoundingBox
        /// </summary>
        /// <returns></returns>
        public BoundingBox Clone()
        {
            return new BoundingBox(_min.X, _min.Y, _max.X, _max.Y);
        }

        /// <summary>
        /// Returns a string representation of the boundingbox as LowerLeft + UpperRight formatted as "MinX,MinY MaxX,MaxY"
        /// </summary>
        /// <returns>MinX,MinY MaxX,MaxY</returns>
        public override string ToString()
        {
            return String.Format(Map.NumberFormatEnUs, "{0},{1} {2},{3}", Min.X, Min.Y, Max.X, Max.Y);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var box = obj as BoundingBox;
            if (obj == null) 
                return false;
            return Equals(box);
        }

        /// <summary>
        /// Returns a hash code for the specified object
        /// </summary>
        /// <returns>A hash code for the specified object</returns>
        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        /// <summary>
        /// Returns the intersection of this bounding box and a default envelope
        /// </summary>
        /// <param name="defaultEnvelope"></param>
        /// <returns></returns>
        public BoundingBox Intersection(double[] defaultEnvelope)
        {
            if (defaultEnvelope == null)
                return this;
            
            System.Diagnostics.Debug.Assert(defaultEnvelope.Length == 4);

            var minX = Min.X > defaultEnvelope[0] ? Min.X : defaultEnvelope[0];
            var minY = Min.Y > defaultEnvelope[1] ? Min.Y : defaultEnvelope[1];
            var maxX = Max.X < defaultEnvelope[2] ? Max.X : defaultEnvelope[2];
            var maxY = Max.Y < defaultEnvelope[3] ? Max.Y : defaultEnvelope[3];
            
            return new BoundingBox(minX, minY, maxX, maxY);
        }
    }
}