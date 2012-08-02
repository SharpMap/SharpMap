// code adapted from: https://github.com/awcoats/mapstache
namespace Mapstache
{
    using System;
    using System.Drawing;

    public static class SphericalMercator
    {
        private const double Radius = 6378137;
        private const double D2R = Math.PI / 180;
        private const double HalfPi = Math.PI / 2;

        public static Point FromLonLat(PointF lonlat)
        {
            double lon = lonlat.X;
            double lat = lonlat.Y;
            double lonRadians = (D2R * lon);
            double latRadians = (D2R * lat);
            double x = Radius * lonRadians;
            double y = Radius * Math.Log(Math.Tan(Math.PI * 0.25 + latRadians * 0.5));
            return new Point((int)x, (int)y);
        }

        public static void FromLonLat(double lon, double lat, out double x, out double y)
        {
            double lonRadians = (D2R * lon);
            double latRadians = (D2R * lat);
            x = Radius * lonRadians;
            y = Radius * Math.Log(Math.Tan(Math.PI * 0.25 + latRadians * 0.5));
        }

        public static void ToLonLat(double x, double y, out double lon, out double lat)
        {
            double ts = Math.Exp(-y / (Radius));
            double latRadians = HalfPi - 2 * Math.Atan(ts);
            double lonRadians = x / (Radius);
            lon = (lonRadians / D2R);
            lat = (latRadians / D2R);
        }

        public static PointF ToLonLat(Point xy)
        {
            double x = xy.X;
            double y = xy.Y;
            double ts = Math.Exp(-y / (Radius));
            double latRadians = HalfPi - 2 * Math.Atan(ts);
            double lonRadians = x / (Radius);
            double lon = (lonRadians / D2R);
            double lat = (latRadians / D2R);
            return new PointF((float)lon, (float)lat);
        }

        public static Rectangle FromLonLat(RectangleF bbox)
        {
            Point min = FromLonLat(new PointF(bbox.X, bbox.Y));
            Point max = FromLonLat(new PointF(bbox.X + bbox.Width, bbox.Y + bbox.Height));
            return Rectangle.FromLTRB(min.X, min.Y, max.X, max.Y);
        }

        public static RectangleF ToLonLat(RectangleF bbox)
        {
            PointF min = ToLonLat(new Point((int)bbox.X, (int)bbox.Y));
            PointF max = ToLonLat(new Point((int)(bbox.X + bbox.Width), (int)(bbox.Y + bbox.Height)));
            return RectangleF.FromLTRB(min.X, min.Y, max.X, max.Y);
        }
    }
}
