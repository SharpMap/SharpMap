using System.IO;
using NetTopologySuite;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace UnitTests.Data.Providers
{
    public class GeoPackageTest
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NtsGeometryServices();
        }

        [TestCase(@"C:\Downloads\gdal_sample.gpkg")]
        [TestCase(@"C:\Downloads\haiti-vectors-split.gpkg")]
        [TestCase(@"C:\Downloads\simple_sewer_features.gpkg")]
        public void TestGeoPackage(string file)
        {
            if (!File.Exists(file))
                throw new IgnoreException(string.Format("Test data not present: '{0}'!", file));
            
            GeoPackage gpkg = null;
            Assert.DoesNotThrow(() => gpkg = GeoPackage.Open(file), "Opened did not prove to be a valid geo package");

            Assert.Greater(gpkg.Features.Count + gpkg.Tiles.Count, 0);

            foreach (var feature in gpkg.Features)
            {
                IProvider p = null;
                Assert.DoesNotThrow(() => p = gpkg.GetFeatureProvider(feature.TableName));
                ILayer l = null;
                Assert.DoesNotThrow(() => l = gpkg.GetFeatureLayer(feature));

            }
        }
    }
}