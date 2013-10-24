using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace SharpMap.Demo.Wms.Helpers
{
    public static class ProjHelper
    {
        private static readonly ICoordinateTransformation Transformation;

        static ProjHelper()
        {
            CoordinateTransformationFactory factory = new CoordinateTransformationFactory();
            IGeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;
            IProjectedCoordinateSystem mercator = ProjectedCoordinateSystem.WebMercator;
            Transformation = factory.CreateFromCoordinateSystems(wgs84, mercator);
        }

        public static ICoordinateTransformation LatLonToGoogle()
        {            
            return Transformation;
        }
    }    
}