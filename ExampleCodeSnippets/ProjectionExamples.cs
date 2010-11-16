using System.Diagnostics;
using NUnit.Framework;

namespace ExampleCodeSnippets
{
    
    [NUnit.Framework.TestFixture]
    public class ProjectionExamples
    {
        private string osgb36 =
            "COMPD_CS[\"OSGB36 / British National Grid + ODN\",PROJCS[\"OSGB 1936 / British National Grid\",GEOGCS[\"OSGB 1936\",DATUM[\"OSGB 1936\",SPHEROID[\"Airy 1830\",6377563.396,299.3249646,AUTHORITY[\"EPSG\",\"7001\"]],TOWGS84[446.448,-125.157,542.06,0.15,0.247,0.842,-4.2261596151967575],AUTHORITY[\"EPSG\",\"6277\"]],PRIMEM[\"Greenwich\",0.0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.017453292519943295],AXIS[\"Geodetic latitude\",NORTH],AXIS[\"Geodetic longitude\",EAST],AUTHORITY[\"EPSG\",\"4277\"]],PROJECTION[\"Transverse Mercator\",AUTHORITY[\"EPSG\",\"9807\"]],PARAMETER[\"central_meridian\",-2.0],PARAMETER[\"latitude_of_origin\",49.0],PARAMETER[\"scale_factor\",0.9996012717],PARAMETER[\"false_easting\",400000.0],PARAMETER[\"false_northing\",-100000.0],UNIT[\"m\",1.0],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH],AUTHORITY[\"EPSG\",\"27700\"]],VERT_CS[\"Newlyn\",VERT_DATUM[\"Ordnance Datum Newlyn\",2005,AUTHORITY[\"EPSG\",\"5101\"]],UNIT[\"m\",1.0],AXIS[\"Gravity-related height\",UP],AUTHORITY[\"EPSG\",\"5701\"]],AUTHORITY[\"EPSG\",\"7405\"]]";

        private string wgs84 =
            "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";


#if !DotSpatialProjections
        [Test]
        [Ignore]
        public void TestConversionProjNet()
        {
            var csf = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
            var cs1 = csf.CreateFromWkt(osgb36);
            var cs2 = csf.CreateFromWkt(wgs84);

            var ctf = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(cs1, cs2);

            Debug.Assert(ct != null);
        }
#else
        [Test]
        public void TestConversionDSProjection()
        {
            var pi1 = new DotSpatial.Projections.ProjectionInfo();
            pi1.ReadEsriString(osgb36);
            var pi2 = new DotSpatial.Projections.ProjectionInfo();
            pi2.ReadEsriString(wgs84);

            var ct = new DotSpatial.Projections.CoordinateTransformation();
            ct.Source = pi1;
            ct.Target = pi2;


        }
#endif
    }
}