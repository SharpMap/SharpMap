using NetTopologySuite;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMap
{
    public static class Config
    {
        public static void Configurator(Configuration config)
        {
            NtsGeometryServices gss = new NtsGeometryServices();
            CoordinateSystemServices css = null;

            switch (config)
            {
                case Configuration.DotSpatial:
                    //css = new CoordinateSystemServices(
                    //        new DotSpatialCoordinateSystemFactory(),
                    //        new DotSpatialCoordinateTransformationFactory(),
                    //        SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems()
                    //);
                    break;

                case Configuration.Proj4Net:
                    css = new CoordinateSystemServices(
                        new CoordinateSystemFactory(Encoding.ASCII),
                        new CoordinateTransformationFactory(),
                        SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems()
                    );
                    break;

                default:
                    break;
            }

            GeoAPI.GeometryServiceProvider.Instance = gss;
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);

        }
    }

    public enum Configuration
    {
        DotSpatial,
        Proj4Net
    }
}
