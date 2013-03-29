using System.Drawing;
using SharpMap;
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
        /// <param name="map">The map that defines the affine coordinate transformation</param>
        /// <returns>The array of <see cref="PointF"/>s</returns>
        public static PointF[] TransformToImage(this ILineString self, Map map)
        {
            return TransformToImage(self.Coordinates, map);
        }

        /// <summary>
        /// Transforms an array of <see cref="Coordinate"/>s to an array of <see cref="PointF"/>s.
        /// </summary>
        /// <param name="vertices">The array of coordinates</param>
        /// <param name="map">The map that defines the affine coordinate transformation</param>
        /// <returns>The array of <see cref="PointF"/>s</returns>
        private static PointF[] TransformToImage(Coordinate[] vertices, Map map)
        {
            var v = new PointF[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
                v[i] = Transform.WorldtoMap(vertices[i], map);
            return v;
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
            return NetTopologySuite.Algorithm.CGAlgorithms.IsCCW(self.Coordinates);
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
        /// <returns>An array of PointFs</returns>
        public static PointF[] TransformToImage(this IPolygon self, Map map)
        {
            return TransformToImage(self.Coordinates, map);
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
    }
}