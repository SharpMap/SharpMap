using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using System;
using System.IO;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            var gss = new NtsGeometryServices();
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            NtsGeometryServices.Instance = gss;
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);
        }


        [TestMethod]
        public void TestMethod1()
        {
            var shp = new ShapeFile(@"E:\Data\测试导入问题面.shp", false, false);
            shp.Open();

            var bbox = shp.GetExtents();
            bbox.ExpandBy(bbox.Width * 1.1, bbox.Height * 1.1);
            var ids = shp.GetObjectIDsInView(bbox);
            foreach (var id in ids)
            {
                try
                {
                    var feature = shp.GetFeature(id);
                    Guid.TryParse(feature["uniqueid"] + "", out Guid uuid);
                    Guid.TryParse(feature["creator"] + "", out Guid creator);
                    DateTimeOffset.TryParse(feature["createat"] + "", out DateTimeOffset createat);

                    var name = feature["name"]?.ToString();
                    var location = feature["location"]?.ToString();
                    var descript = feature["descript"]?.ToString();

                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
