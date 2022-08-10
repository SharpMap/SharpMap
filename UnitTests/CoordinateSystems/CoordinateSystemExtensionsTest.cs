using NetTopologySuite;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap;
using SharpMap.CoordinateSystems;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using System.Drawing;

namespace UnitTests.CoordinateSystems
{
    [TestFixture]
    public class CoordinateSystemExtensionsTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var gss = NtsGeometryServices.Instance;
            var css = new CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());
            Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);
        }

        [TestCase(4326)]
        [TestCase(25832)]
        [TestCase(31467)]
        [TestCase(3758)]
        public void TestCoordinateSystemForMap(int srid)
        {
            var map = new Map(new Size(200, 150)) { SRID = srid };
            CoordinateSystem cs = null;
            Assert.DoesNotThrow(() => cs = map.GetCoordinateSystem());
            Assert.NotNull(cs);
            Assert.AreEqual("EPSG", cs.Authority);
            Assert.AreEqual((long)srid, cs.AuthorityCode);
        }

        [TestCase(4326)]
        [TestCase(25832)]
        [TestCase(31467)]
        [TestCase(3758)]
        public void TestCoordinateSystemForLayer(int srid)
        {
            var map = new VectorLayer("LayerName", new GeometryFeatureProvider(new FeatureDataTable())) { SRID = srid };
            CoordinateSystem cs = null;
            Assert.DoesNotThrow(() => cs = map.GetCoordinateSystem());
            Assert.NotNull(cs);
            Assert.AreEqual("EPSG", cs.Authority);
            Assert.AreEqual((long)srid, cs.AuthorityCode);
        }

        [TestCase(4326)]
        [TestCase(25832)]
        [TestCase(31467)]
        [TestCase(3758)]
        public void TestCoordinateSystemForProvider(int srid)
        {
            var map = new GeometryFeatureProvider(new FeatureDataTable()) { SRID = srid };
            CoordinateSystem cs = null;
            Assert.DoesNotThrow(() => cs = map.GetCoordinateSystem());
            Assert.NotNull(cs);
            Assert.AreEqual("EPSG", cs.Authority);
            Assert.AreEqual((long)srid, cs.AuthorityCode);
        }

        [TestCase(4326)]
        [TestCase(25832)]
        [TestCase(31467)]
        [TestCase(3758)]
        public void TestCoordinateSystemForGeometry(int srid)
        {
            var map = NtsGeometryServices.Instance.CreateGeometryFactory(srid);
            var g = map.CreatePoint(new Coordinate(10, 10));
            CoordinateSystem cs = null;
            Assert.DoesNotThrow(() => cs = g.GetCoordinateSystem());
            Assert.NotNull(cs);
            Assert.AreEqual("EPSG", cs.Authority);
            Assert.AreEqual((long)srid, cs.AuthorityCode);
        }
    }
}
