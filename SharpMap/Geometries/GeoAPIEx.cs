using SharpMap;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using NetTopologySuite.Geometries;
using SharpMap.Rendering;
using SharpMap.Utilities;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// Set of extension methods for use of GeoAPI within SharpMap
    /// </summary>
    public static class GeoAPIEx
    {

        /// <summary>
        /// Gets the minimum coordinate of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The minimum coordinate</returns>
        public static Coordinate Min(this Envelope self)
        {
            return new Coordinate(self.MinX, self.MinY);
        }

        /// <summary>
        /// Ensures that a CoordinateSequence forms a valid ring, 
        /// returning a new closed sequence of the correct length if required.
        /// If the input sequence is already a valid ring, it is returned 
        /// without modification.
        /// If the input sequence is too short or is not closed, 
        /// it is extended with one or more copies of the start point.
        /// </summary>
        /// <param name="fact">The CoordinateSequenceFactory to use to create the new sequence</param>
        /// <param name="seq">The sequence to test</param>
        /// <returns>The original sequence, if it was a valid ring, or a new sequence which is valid.</returns>
        public static ICoordinateSequence EnsureValidRing(ICoordinateSequenceFactory fact, ICoordinateSequence seq)
        {
            var n = seq.Count;
            // empty sequence is valid
            if (n == 0) return seq;
            // too short - make a new one
            if (n <= 3)
                return CreateClosedRing(fact, seq, 4);

            var isClosed = Math.Abs(seq.GetOrdinate(0, Ordinate.X) - seq.GetOrdinate(n - 1, Ordinate.X)) < double.Epsilon &&
                           Math.Abs(seq.GetOrdinate(0, Ordinate.Y) - seq.GetOrdinate(n - 1, Ordinate.Y)) < double.Epsilon;
            if (isClosed) return seq;
            // make a new closed ring
            return CreateClosedRing(fact, seq, n + 1);
        }

        private static ICoordinateSequence CreateClosedRing(ICoordinateSequenceFactory fact, ICoordinateSequence seq, int size)
        {
            var newseq = fact.Create(size, seq.Dimension);
            int n = seq.Count;
            Copy(seq, 0, newseq, 0, n);
            // fill remaining coordinates with start point
            for (int i = n; i < size; i++)
                Copy(seq, 0, newseq, i, 1);
            return newseq;
        }

        ///<summary>
        /// Copies a section of a <see cref="ICoordinateSequence"/> to another <see cref="ICoordinateSequence"/>.
        /// The sequences may have different dimensions;
        /// in this case only the common dimensions are copied.
        ///</summary>
        /// <param name="src">The sequence to copy coordinates from</param>
        /// <param name="srcPos">The starting index of the coordinates to copy</param>
        /// <param name="dest">The sequence to which the coordinates should be copied to</param>
        /// <param name="destPos">The starting index of the coordinates in <see paramref="dest"/></param>
        /// <param name="length">The number of coordinates to copy</param>
        public static void Copy(ICoordinateSequence src, int srcPos, ICoordinateSequence dest, int destPos, int length)
        {
            for (int i = 0; i < length; i++)
                CopyCoord(src, srcPos + i, dest, destPos + i);
        }

        ///<summary>
        /// Copies a coordinate of a <see cref="ICoordinateSequence"/> to another <see cref="ICoordinateSequence"/>.
        /// The sequences may have different dimensions;
        /// in this case only the common dimensions are copied.
        ///</summary>
        /// <param name="src">The sequence to copy coordinate from</param>
        /// <param name="srcPos">The index of the coordinate to copy</param>
        /// <param name="dest">The sequence to which the coordinate should be copied to</param>
        /// <param name="destPos">The index of the coordinate in <see paramref="dest"/></param>
        public static void CopyCoord(ICoordinateSequence src, int srcPos, ICoordinateSequence dest, int destPos)
        {
            int minDim = Math.Min(src.Dimension, dest.Dimension);
            for (int dim = 0; dim < minDim; dim++)
            {
                var ordinate = (Ordinate)dim;
                double value = src.GetOrdinate(srcPos, ordinate);
                dest.SetOrdinate(destPos, ordinate, value);
            }
        }

        /// <summary>
        /// Ensures that a CoordinateSequence forms a valid ring, 
        /// returning a new closed sequence of the correct length if required.
        /// If the input sequence is already a valid ring, it is returned 
        /// without modification.
        /// If the input sequence is too short or is not closed, 
        /// it is extended with one or more copies of the start point.
        /// </summary>
        /// <param name="coordinates">List of coordinates</param>
        /// <returns>The original sequence, if it was a valid ring, or a new sequence which is valid.</returns>
        public static void EnsureValidRing(this List<Coordinate> coordinates)
        {
            var seq = GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory.Create(coordinates.ToArray());
            seq = EnsureValidRing(GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory, seq);
            if (seq.Count != coordinates.Count)
            {
                for (int i = coordinates.Count; i < seq.Count; i++)
                {
                    coordinates.Add(seq.GetCoordinate(i));
                }
            }
        }

        /// <summary>
        /// Gets the maximum coordinate of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The maximum coordinate</returns>
        public static Coordinate Max(this Envelope self)
        {
            return new Coordinate(self.MaxX, self.MaxY);
        }

        /// <summary>
        /// Adds to coordinate's
        /// </summary>
        /// <param name="self">the first coordinate</param>
        /// <param name="summand">The second coordinate</param>
        public static Coordinate Add(this Coordinate self, Coordinate summand)
        {
            if (self == null && summand == null)
                return null;
            if (summand == null)
                return self;
            if (self == null)
                return summand;

            // for now we only care for 2D
            return new Coordinate(self.X + summand.X, self.Y + summand.Y);
        }

        /// <summary>
        /// Subtracts two coordinates from one another
        /// </summary>
        /// <param name="self">The first coordinate</param>
        /// <param name="summand">The second coordinate</param>
        public static Coordinate Subtract(this Coordinate self, Coordinate summand)
        {
            if (self == null && summand == null)
                return null;
            if (summand == null)
                return self;
            if (self == null)
                return new Coordinate(-summand.X, -summand.Y);

            // for now we only care for 2D
            return new Coordinate(self.X - summand.X, self.Y - summand.Y);  
        }

        /// <summary>
        /// Gets the axis of the longest axis
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Ordinate LongestAxis(this Envelope self)
        {
            if (self.MaxExtent == self.Width)
                return Ordinate.X;
            return Ordinate.Y;
        }

        /// <summary>
        /// Gets the bottom-left coordinate of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The bottom-left coordinate</returns>
        public static Coordinate BottomLeft(this Envelope self)
        {
            return self.Min();
        }

        /// <summary>
        /// Gets the bottom-right coordinate of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The bottom-right coordinate</returns>
        public static Coordinate BottomRight(this Envelope self)
        {
            return new Coordinate(self.MaxX, self.MinY);
        }

        /// <summary>
        /// Gets the top-left coordinate of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The top-left coordinate</returns>
        public static Coordinate TopLeft(this Envelope self)
        {
            return new Coordinate(self.MinX, self.MaxY);
        }

        /// <summary>
        /// Gets the top-right coordinate of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The top-right coordinate</returns>
        public static Coordinate TopRight(this Envelope self)
        {
            return self.Max();
        }

        /// <summary>
        /// Gets the minimum y-value of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The minimum y-value</returns>
        public static double Bottom(this Envelope self)
        {
            return self.MinY;
        }

        /// <summary>
        /// Gets the maximum y-value of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The maximum y-value</returns>
        public static double Top(this Envelope self)
        {
            return self.MaxY;
        }

        /// <summary>
        /// Gets the minimum x-value of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The minimum x-value</returns>
        public static double Left(this Envelope self)
        {
            return self.MinX;
        }

        /// <summary>
        /// Gets the maximum x-value of the <see cref="Envelope"/>
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <returns>The maximum x-value</returns>
        public static double Right(this Envelope self)
        {
            return self.MaxX;
        }

        /// <summary>
        /// Transforms a <see cref="ILineString"/> to an array of <see cref="PointF"/>s.
        /// </summary>
        /// <param name="self">The linestring</param>
        /// <param name="map">The mapviewport defining transformation parameters</param>
        /// <returns>The array of <see cref="PointF"/>s</returns>
        public static PointF[] TransformToImage(this ILineString self, MapViewport map)
        {
            if (map.MapTransformRotation.Equals(0f))
                return Transform.WorldToMap(self.Coordinates, map.Left, map.Top, map.PixelWidth, map.PixelHeight);

            return Transform.WorldToMap(self.Coordinates,map.WorldToMapTransform(false));
        }

        /// <summary>
        /// Converts a <see cref="Coordinate"/> to an array of <see cref="double"/>s.
        /// </summary>
        /// <param name="self">The coordinate</param>
        /// <returns>An array of doubles</returns>
        public static double [] ToDoubleArray(this Coordinate self)
        {
            return new[] { self.X, self.Y /*, self.Z */};
        }

        /// <summary>
        /// Abbreviation to counter clockwise function
        /// </summary>
        /// <param name="self">The ring</param>
        /// <returns><c>true</c> if the ring is oriented counter clockwise</returns>
        public static bool IsCCW(this ILinearRing self)
        {
            return NetTopologySuite.Algorithm.Orientation.IsCCW(self.Coordinates);
        }

        /// <summary>
        /// Increases the size of the boundingbox by the given amount in all directions
        /// </summary>
        /// <param name="self">The envelope to grow</param>
        /// <param name="amount">Amount to grow in all directions</param>
        public static Envelope Grow(this Envelope self, double amount)
        {
            return new Envelope(self.MinX - amount, self.MaxX + amount,
                                self.MinY - amount, self.MaxY + amount);
        }

        /// <summary>
        /// Increases the size of the boundingbox by the given amount in horizontal and vertical directions
        /// </summary>
        /// <param name="self">The envelope</param>
        /// <param name="amountInX">Amount to grow in horizontal direction</param>
        /// <param name="amountInY">Amount to grow in vertical direction</param>
        public static Envelope Grow(this Envelope self, double amountInX, double amountInY)
        {
            return new Envelope(self.MinX - amountInX, self.MaxX + amountInX,
                                self.MinY - amountInY, self.MaxY + amountInY);
        }

        /// <summary>
        /// Transforms a <see cref="IPolygon"/> to an array of <see cref="PointF"/>s
        /// </summary>
        /// <param name="self">The polygon</param>
        /// <param name="map">The map that defines the affine coordinate transformation.</param>
        /// <param name="useClipping">Use clipping for the polygon</param>
        /// <returns>An array of PointFs</returns>
        public static GraphicsPath TransformToImage(this IPolygon self, MapViewport map, bool useClipping = false)
        {
            var res = new GraphicsPath(FillMode.Alternate);
            if (useClipping)
            {
                res.AddPolygon(VectorRenderer.ClipPolygon(
                    VectorRenderer.LimitValues(self.ExteriorRing.TransformToImage(map), VectorRenderer.ExtremeValueLimit),
                    map.Size.Width, map.Size.Height));
                for (var i = 0; i < self.NumInteriorRings; i++)
                    res.AddPolygon(VectorRenderer.ClipPolygon(
                        VectorRenderer.LimitValues(self.GetInteriorRingN(i).TransformToImage(map),VectorRenderer.ExtremeValueLimit),
                        map.Size.Width, map.Size.Height));
            }
            else
            {
                res.AddPolygon(self.ExteriorRing.TransformToImage(map));
                for (var i = 0; i < self.NumInteriorRings; i++)
                    res.AddPolygon(self.GetInteriorRingN(i).TransformToImage(map));
            }
            return res;
        }

        /// <summary>
        /// Tests if a coordinate is empty
        /// </summary>
        /// <param name="c">The coordinate</param>
        public static bool IsEmpty(this Coordinate c)
        {
            if (c == null) return true;
            if (!double.IsNaN(c.X)) return false;
            if (!double.IsNaN(c.Y)) return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="wkt"></param>
        /// <returns></returns>
        public static IGeometry GeomFromText(this IGeometry self, string wkt)
        {
            var factory = self == null ? new NetTopologySuite.Geometries.GeometryFactory() : self.Factory;
            var reader = new NetTopologySuite.IO.WKTReader(factory);
            return reader.Read(wkt);
        }


        private static readonly FieldInfo _envFi;
        static GeoAPIEx()
        {
            try
            {
                _envFi = typeof(Geometry).GetField("_envelope", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            catch{}
        }

        public static void SetExtent(Geometry geom, Envelope envelope)
        {
            if (geom == null)
                return;

            if (_envFi != null)
                _envFi.SetValue(geom, envelope);
        }

    }
}
