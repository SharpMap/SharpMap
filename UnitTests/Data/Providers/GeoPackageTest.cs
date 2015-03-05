using System.Collections.ObjectModel;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite;
using NUnit.Framework;
using SharpMap.Data;
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

        [TestCase(@"C:\Downloads\geonames_belgium.gpkg")]
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
                Assert.DoesNotThrow(() => p = gpkg.GetFeatureProvider(feature));

                TestProvider(p, feature);
                
                ILayer l = null;
                Assert.DoesNotThrow(() => l = gpkg.GetFeatureLayer(feature));

            }
        }

        private static void TestProvider(IProvider provider, GpkgContent content)
        {
            int numFeatures = 0;
            Assert.DoesNotThrow(() => numFeatures = provider.GetFeatureCount(),
                    "GetFeatureCount threw exception:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);

            var extent = provider.GetExtents();

            Collection<uint> oids = null;
            Assert.DoesNotThrow(() => oids = provider.GetObjectIDsInView(extent), 
                    "GetObjectIDsInView threw exception:\n\tConnection{0}\n\t{1}", 
                    provider.ConnectionID, content.TableName);
            Assert.AreEqual(numFeatures, oids.Count);

            foreach (var oid in oids)
            {
                IGeometry geom = null;
                Assert.DoesNotThrow(() => geom = provider.GetGeometryByID(oid), 
                    "GetGeometryByID threw exception:\n\tConnection{0}\n\t{1}", 
                    provider.ConnectionID, content.TableName);
                FeatureDataRow feat = null;
                Assert.DoesNotThrow(() => feat = provider.GetFeature(oid), 
                    "GetFeature threw exception:\n\tConnection{0}\n\t{1}", 
                    provider.ConnectionID, content.TableName);
                
                Assert.IsTrue(geom.EqualsExact(feat.Geometry));

            }

            Collection<IGeometry> geoms = null;
            Assert.DoesNotThrow(() => geoms = provider.GetGeometriesInView(extent),
                    "GetFeature threw exception:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);

            Assert.AreEqual(numFeatures, geoms.Count);
            
            var fds = new FeatureDataSet();
            Assert.DoesNotThrow(() => provider.ExecuteIntersectionQuery(extent, fds),
                    "GetFeature threw exception:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);
            Assert.AreEqual(numFeatures, fds.Tables[0].Rows.Count);


        }
    }
}