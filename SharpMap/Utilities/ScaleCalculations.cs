using System;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Functions for calculating Scales
    /// </summary>
    public static class ScaleCalculations
    {
        /// <summary>
        /// Calculates the Zoom-Level for a given Scale, DPI and MapWidth
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="mapUnitFactor"></param>
        /// <param name="dpi"></param>
        /// <param name="mapSizeWidth"></param>
        /// <returns></returns>
        public static double GetMapZoomFromScaleNonLatLong(double scale, double mapUnitFactor, int dpi, double mapSizeWidth)
        {
            int nPxlPerInch = dpi;
            double zoom;

            if (mapSizeWidth <= 0) return 0.0;

            try
            {
                double pageWidth = mapSizeWidth/nPxlPerInch*GeoSpatialMath.MetersPerInch;
                zoom = Math.Abs(scale*pageWidth)/mapUnitFactor;
            }
            catch
            {
                zoom = 0.0;
            }
            return zoom;
        }

        /// <summary>
        /// Calculate the Representative Fraction Scale for non Lat/Long map.
        /// </summary>
        /// <param name="mapWidthMeters">The current extent width of the Map</param>
        /// <param name="mapSizeWidth">The width of the display area</param>
        /// <param name="mapUnitFactor">MapUnitFactor is the factor the unit used on the map</param>
        /// <param name="dpi">DPI used to render the map</param>
        /// <returns></returns>
        public static double CalculateScaleNonLatLong(double mapWidthMeters, double mapSizeWidth, double mapUnitFactor, int dpi)
        {
            int nPxlPerInch = dpi;
            double ratio;

            if (mapSizeWidth <= 0) return 0.0;
            //convert map width to meters
            double mapWidth = mapWidthMeters * mapUnitFactor;
            //convert page width to meters.
            try
            {
                double pageWidth = mapSizeWidth / nPxlPerInch * GeoSpatialMath.MetersPerInch;
                ratio = Math.Abs(mapWidth / pageWidth);
            }
            catch
            {
                ratio = 0.0;
            }
            return ratio;
        }

        /// <summary>
        /// Calculate the Representative Fraction Scale for a Lat/Long map.
        /// </summary>
        /// <param name="lon1">LowerLeft Longitude</param>
        /// <param name="lon2">LowerRight Longitude</param>
        /// <param name="lat">LowerLeft Latitude</param>
        /// <param name="widthPage">The width of the display area</param>
        /// <param name="dpi">DPI used to render the map</param>
        /// <returns></returns>
        public static double CalculateScaleLatLong(double lon1, double lon2, double lat, double widthPage, int dpi)
        {
            double distance = GeoSpatialMath.GreatCircleDistanceReflex(lon1, lon2, lat);
            double r = CalculateScaleNonLatLong(distance, widthPage, 1, dpi);
            return r;
        }
    }
}
