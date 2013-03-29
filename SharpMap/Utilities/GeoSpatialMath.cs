using System;

namespace SharpMap.Utilities
{
    /// <summary>
    /// A
    /// </summary>
    public static class GeoSpatialMath
    {
        /// <summary>
        /// Conversion factor degrees to radians
        /// </summary>
        public const double DegToRad = Math.PI/180d; //0.01745329252; // Convert Degrees to Radians

        /// <summary>
        /// Meters per inch
        /// </summary>
        public const double MetersPerInch = 0.0254;

        /// <summary>
        /// Meters per mile
        /// </summary>
        public const double MetersPerMile = 1609.347219;

        /// <summary>
        /// Miles per degree at equator
        /// </summary>
        public const double MilesPerDegreeAtEquator = 69.171;

        /// <summary>
        /// Meters per degree at equator
        /// </summary>
        public const double MetersPerDegreeAtEquator = MetersPerMile * MilesPerDegreeAtEquator;

        /// <summary>
        /// Calculate the distance between 2 points on the great circle
        /// </summary>
        /// <param name="lon1">The first longitue value</param>
        /// <param name="lat1">The latitude value for <paramref name="lon1"/></param>
        /// <param name="lon2">The second longitue value</param>
        /// <param name="lat2">The latitude value for <paramref name="lon2"/></param>
        /// <returns>The distance in meters</returns>
        public static double GreatCircleDistance(double lon1, double lat1, double lon2, double lat2)
        {
            var lonDistance = DiffLongitude(lon1, lon2);
            var arg1 = Math.Sin(lat1*DegToRad)*Math.Sin(lat2*DegToRad);
            var arg2 = Math.Cos(lat1*DegToRad)*Math.Cos(lat2*DegToRad)*Math.Cos(lonDistance*DegToRad);

            return MetersPerDegreeAtEquator*Math.Acos(arg1 + arg2)/DegToRad;
        }

        /// <summary>
        /// Calculate the distance between 2 points on the great circle
        /// </summary>
        /// <param name="lon1">The first longitue value</param>
        /// <param name="lon2">The second longitue value</param>
        /// <param name="lat">The common latitued value for <paramref name="lon1"/> and <paramref name="lon2"/></param>
        /// <returns>The distance in meters</returns>
        public static double GreatCircleDistance(double lon1, double lon2, double lat)
        {
            var lonDistance = DiffLongitude(lon1, lon2);
            lat = Math.Abs(lat);
            if (lat >= 90.0)
                lat = 89.999;
            var distance = Math.Cos(lat*DegToRad)*MetersPerDegreeAtEquator*lonDistance;
            return distance;
        }

        /// <summary>
        /// Calculate the difference between two longitudal values
        /// </summary>
        /// <param name="lon1">The first longitue value in degrees</param>
        /// <param name="lon2">The second longitue value in degrees</param>
        /// <returns>The distance in degrees</returns>
        public static double DiffLongitude(double lon1, double lon2)
        {
            double diff;

            if (lon1 > 180.0)
                lon1 = 360.0 - lon1;
            if (lon2 > 180.0)
                lon2 = 360.0 - lon2;

            if ((lon1 >= 0.0) && (lon2 >= 0.0))
                diff = lon2 - lon1;
            else if ((lon1 < 0.0) && (lon2 < 0.0))
                diff = lon2 - lon1;
            else
            {
                // different hemispheres
                if (lon1 < 0)
                    lon1 = -1 * lon1;
                if (lon2 < 0)
                    lon2 = -1 * lon2;
                diff = lon1 + lon2;
                if (diff > 180.0)
                    diff = 360.0 - diff;
            }
            return diff;
        }
    }
}