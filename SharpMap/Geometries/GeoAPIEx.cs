using System.Collections.Generic;
using System.Drawing;
using SharpMap;
using SharpMap.Utilities;

namespace GeoAPI.Geometries
{
    public static class GeoAPIEx
    {
        public static Coordinate Min(this Envelope self)
        {
            return new Coordinate(self.MinX, self.MinY);
        }
        
        public static Coordinate Max(this Envelope self)
        {
            return new Coordinate(self.MaxX, self.MaxY);
        }

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

        public static Coordinate Subtract(this Coordinate self, Coordinate summand)
        {
            return summand == null 
                ? self 
                : self.Add(new Coordinate(-summand.X, -summand.Y));
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

        public static Coordinate BottomLeft(this Envelope self)
        {
            return self.Min();
        }
        public static Coordinate BottomRight(this Envelope self)
        {
            return new Coordinate(self.MaxX, self.MinY);
        }
        public static Coordinate TopLeft(this Envelope self)
        {
            return new Coordinate(self.MinX, self.MaxY);
        }
        public static Coordinate TopRight(this Envelope self)
        {
            return self.Max();
        }

        public static double Bottom(this Envelope self)
        {
            return self.MinY;
        }
        public static double Top(this Envelope self)
        {
            return self.MaxY;
        }
        public static double Left(this Envelope self)
        {
            return self.MinX;
        }
        public static double Right(this Envelope self)
        {
            return self.MaxX;
        }

        public static PointF[] TransformToImage(this ILineString self, Map map)
        {
            return TransformToImage(self.Coordinates, map);
        }

        private static PointF[] TransformToImage(Coordinate[] vertices, Map map)
        {
            var v = new PointF[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
                v[i] = Transform.WorldtoMap(vertices[i], map);
            return v;
        }

        public static double [] ToDoubleArray(this Coordinate self)
        {
            return new[] { self.X, self.Y /*, self.Z */};
        }

        public static bool IsCCW(this ILinearRing self)
        {
            return NetTopologySuite.Algorithm.CGAlgorithms.IsCCW(self.Coordinates);
        }

        /// <summary>
        /// Increases the size of the boundingbox by the givent amount in all directions
        /// </summary>
        /// <param name="self">The envelope to grow</param>
        /// <param name="amount">Amount to grow in all directions</param>
        public static Envelope Grow(this Envelope self, double amount)
        {
            return new Envelope(self.MinX - amount, self.MinY - amount, 
                                self.MaxX + amount, self.MaxY + amount);
        }

        /// <summary>
        /// Increases the size of the boundingbox by the givent amount in horizontal and vertical directions
        /// </summary>
        /// <param name="amountInX">Amount to grow in horizontal direction</param>
        /// <param name="amountInY">Amount to grow in vertical direction</param>
        public static Envelope Grow(this Envelope self, double amountInX, double amountInY)
        {
            return new Envelope(self.MinX - amountInX, self.MinY - amountInY, 
                                self.MaxX + amountInX, self.MaxY + amountInY);
        }

        public static PointF[] TransformToImage(this IPolygon self, Map map)
        {
            return TransformToImage(self.Coordinates, map);
        }

        public static bool IsEmpty(this Coordinate c)
        {
            if (c == null) return true;
            if (!double.IsNaN(c.X)) return false;
            if (!double.IsNaN(c.Y)) return false;
            return true;
        }

        public static IGeometry GeomFromText(this IGeometry self, string wkt)
        {
            var factory = self == null ? new NetTopologySuite.Geometries.GeometryFactory() : self.Factory;
            var reader = new NetTopologySuite.IO.WKTReader(factory);
            return reader.Read(wkt);
        }
    }
}