using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.IO;
using Npgsql;
using NpgsqlTypes;

namespace UnitTests.Data.Providers
{
    [TestFixture]
    public class PostGisGeographyTests
    {
        private List<uint> _insertedIds = new List<uint>(100);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            try
            {
                GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();

                var connStrBuilder = new NpgsqlConnectionStringBuilder(Properties.Settings.Default.PostGis);
                if (string.IsNullOrEmpty(connStrBuilder.Host) || string.IsNullOrEmpty(connStrBuilder.Database))
                {
                    Assert.Ignore("Requires PostgreSQL connectionstring");
                }

                // Set up sample table
                using (var conn = new NpgsqlConnection(Properties.Settings.Default.PostGis))
                {
                    conn.Open();
                    // Load data
                    using (var shapeFile = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("roads_ugl.shp"), false, false, 4326))
                    {
                        shapeFile.Open();

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "DROP TABLE IF EXISTS roads_ugl";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText =
                                "CREATE TABLE roads_ugl(id integer primary key, name character varying(100), geog geography);";
                            cmd.ExecuteNonQuery();
                        }


                        IEnumerable<uint> indexes = shapeFile.GetObjectIDsInView(shapeFile.GetExtents());

                        _insertedIds = new List<uint>(indexes.Take(100));
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText =
                                "INSERT INTO roads_ugl(id, name, geog) VALUES (@PId, @PName, ST_GeogFromWKB(@PGeom));";
                            var @params = cmd.Parameters;
                            @params.AddRange(
                                new[]
                                    {
                                        new NpgsqlParameter("PId", NpgsqlDbType.Integer),
                                        new NpgsqlParameter("PName", NpgsqlDbType.Varchar, 100),
                                        new NpgsqlParameter("PGeom", NpgsqlDbType.Bytea)
                                    });

                            var writer = new PostGisWriter();

                            foreach (var idx in _insertedIds)
                            {
                                var feature = shapeFile.GetFeature(idx);

                                @params["PId"].NpgsqlValue = (int)idx;
                                @params["PName"].NpgsqlValue = feature["NAME"];
                                @params["PGeom"].NpgsqlValue = writer.Write(feature.Geometry);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Verify
                        foreach (var pgp in GetTestProvider())
                        {
                            foreach (var idx in _insertedIds)
                            {
                                var g1 = pgp.GetGeometryByID(idx);
                                var g2 = shapeFile.GetGeometryByID(idx);
                                Assert.AreEqual(g1, g2);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Assert.Ignore("Failed to connect to PostgreSQL/PostGIS Server.\n{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            try
            {
                var connStrBuilder = new NpgsqlConnectionStringBuilder(Properties.Settings.Default.PostGis);
                if (string.IsNullOrEmpty(connStrBuilder.Host) || string.IsNullOrEmpty(connStrBuilder.Database))
                {
                    return;
                }

                // Drop sample table
                using (var conn = new NpgsqlConnection(Properties.Settings.Default.PostGis))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DROP TABLE roads_ugl";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private static SharpMap.Data.Providers.PostGIS[] GetTestProvider()
        {
            return new SharpMap.Data.Providers.PostGIS[] {
                new SharpMap.Data.Providers.PostGIS(Properties.Settings.Default.PostGis, "roads_ugl", "geog", "id"),
            new SharpMap.Data.Providers.PostGIS(Properties.Settings.Default.PostGis, "roads_ugl", "id")
            };
        }

        /// <summary>
        /// Get the envelope of the entire roads_ugl file
        /// </summary>
        private static Envelope GetTestEnvelope()
        {
            return new Envelope(-97.23724071609665, -82.424263624596563, 41.698023105763589, 49.000629000758515);
            //SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse("POLYGON ((-97.23724071609665 41.698023105763589, -82.424263624596563 41.698023105763589, -82.424263624596563 49.000629000758515, -97.23724071609665 49.000629000758515, -97.23724071609665 41.698023105763589))").EnvelopeInternal;
        }

        [Test]
        public void TestGetExtents()
        {
            foreach (var sq in GetTestProvider())
            {
                GeoAPI.Geometries.Envelope extents = sq.GetExtents();
                Assert.IsNotNull(extents);
            }
        }

        [Test]
        public void TestGetGeometriesInView()
        {
            foreach (var sq in GetTestProvider())
            {

                var geometries = sq.GetGeometriesInView(GetTestEnvelope());

                Assert.IsNotNull(geometries);
                Assert.AreEqual(100d, geometries.Count, 2);
            }
        }

        [Test]
        public void TestGetGeometriesInViewDefinitionQuery()
        {
            foreach (var sq in GetTestProvider())
            {
                sq.DefinitionQuery = "NAME LIKE 'A%'";

                var geometries = sq.GetGeometriesInView(GetTestEnvelope());

                Assert.IsNotNull(geometries);
                Assert.LessOrEqual(geometries.Count, 100);
                Assert.Greater(geometries.Count, 0);
            }
        }

        [Test]
        public void TestGetObjectIDsInView()
        {
            foreach (var sq in GetTestProvider())
            {

                var objectIds = sq.GetObjectIDsInView(GetTestEnvelope());

                Assert.IsNotNull(objectIds);
                Assert.AreEqual(100d, objectIds.Count, 2);
            }
        }

        [Test]
        public void TestExecuteIntersectionQuery()
        {
            foreach (var sq in GetTestProvider())
            {

                var ds = new SharpMap.Data.FeatureDataSet();

                sq.ExecuteIntersectionQuery(GetTestEnvelope(), ds);

                Assert.AreEqual(100d, ds.Tables[0].Rows.Count, 2);
            }
        }

        [Test]
        public void TestGetFeatureCount()
        {
            foreach (var sq in GetTestProvider())
            {

                int count = sq.GetFeatureCount();

                Assert.AreEqual(100, count);
            }
        }

        [Test]
        public void TestGetFeatureCountWithDefinitionQuery()
        {
            foreach (var sq in GetTestProvider())
            {

                sq.DefinitionQuery = "NAME LIKE 'A%'";

                int count = sq.GetFeatureCount();

                Assert.LessOrEqual(count, 100);
            }
        }

        [Test]
        public void TestGetFeature()
        {
            foreach (var sq in GetTestProvider())
            {
                var rnd = new Random();
                for (var i = 0; i < 10; i++)
                {
                    var feature = sq.GetFeature(_insertedIds[rnd.Next(0, 100)]);

                    Assert.IsNotNull(feature);
                }
            }
        }

        [Test]
        public void TestGetGeometryByID()
        {
            foreach (var sq in GetTestProvider())
            {
                var rnd = new Random();
                for (var i = 0; i < 10; i++)
                {
                    var feature = sq.GetGeometryByID(_insertedIds[rnd.Next(0, 100)]);

                    Assert.IsNotNull(feature);
                }
            }
        }

        [Test]
        public void TestGetFeatureNonExisting()
        {
            foreach (var sq in GetTestProvider())
            {

                var feature = sq.GetFeature(99999999);

                Assert.IsNull(feature);
            }
        }
    }
}
