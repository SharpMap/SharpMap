using System;
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
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NtsGeometryServices();
        }

        [TestCase(@"geonames_belgium.gpkg")]
        [TestCase(@"gdal_sample.gpkg")]
        [TestCase(@"haiti-vectors-split.gpkg")]
        [TestCase(@"simple_sewer_features.gpkg")]

        public void TestGeoPackage(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
                filePath = TestUtility.GetPathToTestFile(filePath);

            if (!File.Exists(filePath))
                throw new IgnoreException(string.Format("Test data not present: '{0}'!", filePath));
            
            GeoPackage gpkg = null;
            Assert.DoesNotThrow(() => gpkg = GeoPackage.Open(filePath), "Opened did not prove to be a valid geo package");

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

            if (numFeatures > 0)
                Assert.IsTrue(oids.Count > 0);

            var tmpFeatures = 0;
            foreach (var oid in oids)
            {
                if (tmpFeatures > 50) break;

                IGeometry geom = null;
                Assert.DoesNotThrow(() => geom = provider.GetGeometryByID(oid),
                    "GetGeometryByID threw exception:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);
                FeatureDataRow feat = null;
                Assert.DoesNotThrow(() => feat = provider.GetFeature(oid),
                    "GetFeature threw exception:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);

                if (geom != null)
                    Assert.IsTrue(geom.EqualsExact(feat.Geometry));

                tmpFeatures++;
            }

            Collection<IGeometry> geoms = null;
            Assert.DoesNotThrow(() => geoms = provider.GetGeometriesInView(extent),
                "GetGeometriesInView threw exception:\n\tConnection{0}\n\t{1}",
                provider.ConnectionID, content.TableName);

            if (numFeatures > 0)
            {
                Assert.IsTrue(geoms.Count > 0,
                    "GetGeometriesInView with full extent did not return 90% of the geometries:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);
            }
        

            var fds = new FeatureDataSet();
            Assert.DoesNotThrow(() => provider.ExecuteIntersectionQuery(extent, fds),
                    "ExecuteIntersectionQuery threw exception:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);

            if (numFeatures > 0)
            {
                Assert.IsTrue(fds.Tables[0].Rows.Count > 0,
                    "ExecuteIntersectionQuery with full extent did not return 90% of the features:\n\tConnection{0}\n\t{1}",
                    provider.ConnectionID, content.TableName);
            }
        }
    }
}
