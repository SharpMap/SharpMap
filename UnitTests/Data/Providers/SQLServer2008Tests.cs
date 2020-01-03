using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture]
    public class SQLServer2008Tests
    {
        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        [NUnit.Framework.TestCase("[sde].[gisadmin.di]", "sde", "gisadmin.di")]
        [NUnit.Framework.TestCase("[sde.gisadmin].[di]", "sde.gisadmin", "di")]
        [NUnit.Framework.TestCase("sde.gisadmin.di", "sde.gisadmin", "di")]
        public void VerifySchemaDetection(string schemaTable, string tableSchema, string table)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = new SharpMap.Data.Providers.SqlServer2008("", 
                schemaTable, "geom", "oidcolumn", 
                SqlServerSpatialObjectType.Geometry, 4326, SqlServer2008ExtentsMode.SpatialIndex);
            NUnit.Framework.Assert.AreEqual(tableSchema, sq.TableSchema);
            NUnit.Framework.Assert.AreEqual(table, sq.Table);
            NUnit.Framework.Assert.AreEqual("oidcolumn", sq.ObjectIdColumn);

            System.Reflection.PropertyInfo pi = sq.GetType().GetProperty("QualifiedTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
            string qualifiedTable = (string)pi.GetValue(sq, null);
            NUnit.Framework.Assert.IsTrue(qualifiedTable.Contains(tableSchema));
            NUnit.Framework.Assert.IsTrue(qualifiedTable.Contains(table));
        }
    }

    [NUnit.Framework.TestFixture]
    public class SQLServer2008DbTests
    {
        // IGNORE:
        // 1) see SetupFixture where all SQLServer2008DbTests will be ignored if a valid SqlServer connection string is not supplied
        // 2) see GetTestProvider(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType) where all 
        //      tests involving SqlServer2008Ex provider can be skipped
        // NUM RECORDS:
        // see SetupFixture: indexes = indexes.Take(100); may be commented out to use all shapefile records

        public enum SqlServerProviderMode
        {
            WellKnownBinary,
            NativeSqlBytes
        }

        private const int GeographySrid = 4326; // Geography types MUST have a valid spatial reference
        private int _geometrySrid = 0;
        private const string GeometrySpatialIndex = "IX_roads_ugl_GEOM";
        private const string GeographySpatialIndex = "IX_roads_ugl_GEOG";

        private int _numValidGeoms; // number of valid spatial features
        private int _numInvalidGeoms; // number of invalid spatial features
        private int _numValidatedGeoms; // number of spatial featuress = _numValidGeoms + Validated(_numInvalidGeoms)
        private int _numFeatures; // number of records
        private uint _idNullGeom;
        private uint _idEmptyGeom;
        private uint _idInvalidGeom;

        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder(Properties.Settings.Default.SqlServer2008);
            if (string.IsNullOrEmpty(connStrBuilder.DataSource) || string.IsNullOrEmpty(connStrBuilder.InitialCatalog))
            {
                NUnit.Framework.Assert.Ignore("Requires SQL Server connectionstring");
            }

            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            // Set up sample tables (Geometry + Geography)
            using (SqlConnection conn = new SqlConnection(UnitTests.Properties.Settings.Default.SqlServer2008))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE roads_ugl_geom(ID int identity(1,1) PRIMARY KEY, NAME nvarchar(100), GEOM geometry)";
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE roads_ugl_geog(ID int identity(1,1) PRIMARY KEY, NAME nvarchar(100), GEOG geography)";
                    cmd.ExecuteNonQuery();
                }

                // Load data
                using (SharpMap.Data.Providers.ShapeFile shapeFile = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("roads.ugl")))
                {
                    shapeFile.Open();
                    _geometrySrid = shapeFile.SRID;

                    IEnumerable<uint> indexes = shapeFile.GetObjectIDsInView(shapeFile.GetExtents());

                    // Note that spatial indexes may only kick in at certain number of records
                    // so for thorough testing comment out next line and load all features (approx 3500)
                    indexes = indexes.Take(100);

                    var cmdGeom = new SqlCommand("INSERT INTO roads_ugl_geom(NAME, GEOM) VALUES (@Name, geometry::STGeomFromText(@Geom, @Srid))", conn);
                    var cmdGeog = new SqlCommand("INSERT INTO roads_ugl_geog(NAME, GEOG) VALUES (@Name, geography::STGeomFromText(@Geog, @Srid))", conn);

                    foreach (uint idx in indexes)
                    {
                        SharpMap.Data.FeatureDataRow feature = shapeFile.GetFeature(idx);

                        string wkt;

                        if (feature.Geometry == null || feature.Geometry.IsEmpty)
                            wkt = "LINESTRING EMPTY";
                        else
                            wkt = feature.Geometry.AsText();

                        if (cmdGeom.Parameters.Count == 0)
                        {
                            cmdGeom.Parameters.AddWithValue("@Geom", wkt);
                            cmdGeom.Parameters.AddWithValue("@Name", feature["NAME"]);
                            cmdGeom.Parameters.AddWithValue("@Srid", _geometrySrid);
                        }
                        else
                        {
                            cmdGeom.Parameters[0].Value = wkt;
                            cmdGeom.Parameters[1].Value = feature["NAME"];
                        }
                        cmdGeom.ExecuteNonQuery();

                        if (cmdGeog.Parameters.Count == 0)
                        {
                            cmdGeog.Parameters.AddWithValue("@Geog", wkt);
                            cmdGeog.Parameters.AddWithValue("@Name", feature["NAME"]);
                            cmdGeog.Parameters.AddWithValue("@Srid", GeographySrid);
                        }
                        else
                        {
                            cmdGeog.Parameters[0].Value = wkt;
                            cmdGeog.Parameters[1].Value = feature["NAME"];
                        }
                        cmdGeog.ExecuteNonQuery();
                    }

                    cmdGeom.Dispose();
                    cmdGeog.Dispose();
                }

                // ensure we have some features with NULL and EMPTY geometries
                using (var cmd = conn.CreateCommand())
                {
                    // To find invalid geometries: 
                    // SELECT {OidColumn}, {GeometryColumn}.STIsValid() AS STIsValid, {GeometryColumn}.IsValidDetailed() AS IsValidDetailed FROM {QualifiedTableName}

                    // NULL
                    cmd.CommandText = "INSERT INTO roads_ugl_geom(NAME, GEOM) VALUES ('Test null wkt', NULL)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO roads_ugl_geog(NAME, GEOG) VALUES ('Test null wkt', NULL)";
                    cmd.ExecuteNonQuery();

                    // EMPTY
                    cmd.CommandText = "INSERT INTO roads_ugl_geom(NAME, GEOM) VALUES ('Test empty wkt', 'LINESTRING EMPTY')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO roads_ugl_geog(NAME, GEOG) VALUES ('Test empty wkt', 'LINESTRING EMPTY')";
                    cmd.ExecuteNonQuery();

                    // INVALID (ID 144 from shape file; see also ID 2055)
                    cmd.CommandText = $"INSERT INTO roads_ugl_geom(NAME, GEOM) VALUES ('Test invalid wkt', geometry::STGeomFromText('LINESTRING (-84.652756071629071 42.676743004284312, -84.652924071615374 42.676624004283632, -84.652756071629071 42.676743004284312, -84.652512071649028 42.676922004285323, -84.641022072594438 42.685478004332808, -84.638779072781034 42.687271004342172, -84.636932072941363 42.689831004350026, -84.634491073153043 42.693100004360424, -84.62387107404335 42.701092004405112, -84.603256075794022 42.715752004493233, -84.603142075803831 42.715832004493734, -84.599823076091937 42.718651004508146, -84.588676077031693 42.722431004556235, -84.586021077270672 42.725533004568049)', {_geometrySrid}))";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = $"INSERT INTO roads_ugl_geog(NAME, GEOG) VALUES ('Test invalid wkt', geography::STGeomFromText('LINESTRING (-84.652756071629071 42.676743004284312, -84.652924071615374 42.676624004283632, -84.652756071629071 42.676743004284312, -84.652512071649028 42.676922004285323, -84.641022072594438 42.685478004332808, -84.638779072781034 42.687271004342172, -84.636932072941363 42.689831004350026, -84.634491073153043 42.693100004360424, -84.62387107404335 42.701092004405112, -84.603256075794022 42.715752004493233, -84.603142075803831 42.715832004493734, -84.599823076091937 42.718651004508146, -84.588676077031693 42.722431004556235, -84.586021077270672 42.725533004568049)', {GeographySrid}))";
                    cmd.ExecuteNonQuery();
                }

                // Create GEOM spatial index 
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE SPATIAL INDEX [{GeometrySpatialIndex}] ON [dbo].[roads_ugl_geom](GEOM) USING GEOMETRY_GRID WITH (BOUNDING_BOX =(-98, 40, -82, 50), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM))";
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                }

                // Create GEOG spatial index
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE SPATIAL INDEX [{GeographySpatialIndex}] ON [dbo].[roads_ugl_geog](GEOG)";
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();

                }

                // initialise counts and test IDs
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(ID) FROM roads_ugl_geom WHERE Geom.STIsEmpty() = 0 AND Geom.STIsValid() = 1";
                    _numValidGeoms = (int)cmd.ExecuteScalar();

                    cmd.CommandText = $"SELECT COUNT(ID) FROM roads_ugl_geom WHERE GEOM IS NOT NULL AND Geom.STIsEmpty() = 0 AND Geom.STIsValid() = 0";
                    _numInvalidGeoms = (int)cmd.ExecuteScalar();

                    _numValidatedGeoms = _numValidGeoms + _numInvalidGeoms;

                    cmd.CommandText = $"SELECT COUNT(ID) FROM roads_ugl_geom";
                    _numFeatures = (int)cmd.ExecuteScalar();

                    _idNullGeom = (uint)(_numFeatures - 2);
                    _idEmptyGeom = (uint)(_numFeatures - 1);
                    _idInvalidGeom = (uint)(_numFeatures);
                }

            }
        }

        [NUnit.Framework.OneTimeTearDown]
        public void OneTimeTearDown()
        {
            SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder(UnitTests.Properties.Settings.Default.SqlServer2008);
            if (string.IsNullOrEmpty(connStrBuilder.DataSource) || string.IsNullOrEmpty(connStrBuilder.InitialCatalog))
            {
                return;
            }

            // Drop sample tables
            using (SqlConnection conn = new SqlConnection(UnitTests.Properties.Settings.Default.SqlServer2008))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DROP TABLE roads_ugl_geom";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DROP TABLE roads_ugl_geog";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private SharpMap.Data.Providers.SqlServer2008 GetTestProvider(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            return GetTestProvider(SqlServerProviderMode.WellKnownBinary, spatialType);
        }

        private SharpMap.Data.Providers.SqlServer2008 GetTestProvider(SqlServerProviderMode providerMode, SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 provider;

            switch (providerMode)
            {
                case SqlServerProviderMode.NativeSqlBytes:
                    //Assert.Ignore("Ignore SharpMap.Data.Providers.SqlServer2008Ex");

                    if (spatialType == SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)
                        // NB note forcing WGS84
                        provider = new SharpMap.Data.Providers.SqlServer2008Ex(UnitTests.Properties.Settings.Default.SqlServer2008,
                            "roads_ugl_geog", "GEOG", "ID", spatialType, GeographySrid,
                            SharpMap.Data.Providers.SqlServer2008ExtentsMode.QueryIndividualFeatures);
                    else
                        provider = new SharpMap.Data.Providers.SqlServer2008Ex(UnitTests.Properties.Settings.Default.SqlServer2008,
                            "roads_ugl_geom", "GEOM", "ID", spatialType, _geometrySrid,
                            SharpMap.Data.Providers.SqlServer2008ExtentsMode.QueryIndividualFeatures);
                    break;

                default:
                    if (spatialType == SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)
                        // NB note forcing WGS84
                        provider = new SharpMap.Data.Providers.SqlServer2008(UnitTests.Properties.Settings.Default.SqlServer2008,
                                "roads_ugl_geog", "GEOG", "ID", spatialType, GeographySrid,
                                SharpMap.Data.Providers.SqlServer2008ExtentsMode.QueryIndividualFeatures);
                    else
                        provider = new SharpMap.Data.Providers.SqlServer2008(UnitTests.Properties.Settings.Default.SqlServer2008,
                            "roads_ugl_geom", "GEOM", "ID", spatialType, _geometrySrid,
                            SharpMap.Data.Providers.SqlServer2008ExtentsMode.QueryIndividualFeatures);
                    break;
            }

            //provider.ValidateGeometries = true
            //provider.DefinitionQuery = "ID NOT IN (103)"  // Invalid Geom

            return provider;
        }

        /// <summary>
        /// Get the envelope of the entire roads_ugl file
        /// </summary>
        private GeoAPI.Geometries.Envelope GetTestEnvelope(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            var env = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse("POLYGON ((-97.23724071609665 41.698023105763589, -82.424263624596563 41.698023105763589, -82.424263624596563 49.000629000758515, -97.23724071609665 49.000629000758515, -97.23724071609665 41.698023105763589))").EnvelopeInternal;
            if (spatialType == SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)
                // Geography works with boundaries on the spheroid (not rectilinear), so enlarge to ensure we get all features
                env.ExpandBy(0.2);
            return env;
        }

        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetExtentsQueryIndividualFeatures(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.QueryIndividualFeatures;
            GeoAPI.Geometries.Envelope extents = sq.GetExtents();

            NUnit.Framework.Assert.IsNotNull(extents);
        }

        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetExtentsSpatialIndex(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            if (spatialType == SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)
            {
                var ex = NUnit.Framework.Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                {
                    sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.SpatialIndex;
                });
            }
            else
            {
                sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.SpatialIndex;
                GeoAPI.Geometries.Envelope extents = sq.GetExtents();

                NUnit.Framework.Assert.IsNotNull(extents);
            }
        }

        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestSqlServer2008ExProviderOverridesValidateGeometries(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            NUnit.Framework.Assert.Throws<System.ArgumentException>(() =>
            {
                var sq = GetTestProvider(SqlServerProviderMode.NativeSqlBytes, spatialType);
                sq.ValidateGeometries = false;
            });
        }

        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetExtentsEnvelopeAggregate(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            using (SqlConnection conn = new SqlConnection(UnitTests.Properties.Settings.Default.SqlServer2008))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT SERVERPROPERTY('productversion')";
                    string productversion = (string)cmd.ExecuteScalar();
                    if (System.Version.Parse(productversion).Major < 11)
                    {
                        NUnit.Framework.Assert.Ignore("Requires SQL Server 2012 connection");
                    }
                }
            }

            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.EnvelopeAggregate;
            GeoAPI.Geometries.Envelope extents = sq.GetExtents();

            NUnit.Framework.Assert.IsNotNull(extents);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestGetGeometriesInView(SqlServerProviderMode providerMode, SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);
            sq.ValidateGeometries = validateGeometries;

            var geometries = sq.GetGeometriesInView(GetTestEnvelope(spatialType));

            NUnit.Framework.Assert.IsNotNull(geometries);

            NUnit.Framework.Assert.AreEqual(sq.ValidateGeometries ? _numValidatedGeoms : _numValidGeoms, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetGeometriesInViewDefinitionQuery(SqlServerProviderMode providerMode, SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            sq.DefinitionQuery = "NAME LIKE 'A%'";

            var geometries = sq.GetGeometriesInView(GetTestEnvelope(spatialType));

            NUnit.Framework.Assert.IsNotNull(geometries);
            NUnit.Framework.Assert.LessOrEqual(geometries.Count, _numValidGeoms);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestGetGeometriesInViewNOLOCK(SqlServerProviderMode providerMode, SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            sq.NoLockHint = true;
            sq.ValidateGeometries = validateGeometries;

            var geometries = sq.GetGeometriesInView(GetTestEnvelope(spatialType));

            NUnit.Framework.Assert.AreEqual(sq.ValidateGeometries ? _numValidatedGeoms : _numValidGeoms, geometries.Count);

        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes, SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestGetGeometriesInViewFORCESEEK(SqlServerProviderMode providerMode, SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            sq.ForceSeekHint = true;
            sq.ValidateGeometries = validateGeometries;

            var geometries = sq.GetGeometriesInView(GetTestEnvelope(spatialType));

            NUnit.Framework.Assert.IsNotNull(geometries);
            // NOTE ValidateGeometries is ignored when using ForceSeek
            NUnit.Framework.Assert.AreEqual(_numValidGeoms, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestGetGeometriesInViewFORCEINDEX(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            sq.ForceIndex = spatialType ==  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry ? GeometrySpatialIndex : GeographySpatialIndex;
            sq.ValidateGeometries = validateGeometries;

            var geometries = sq.GetGeometriesInView(GetTestEnvelope(spatialType));

            NUnit.Framework.Assert.IsNotNull(geometries);
            // NOTE ValidateGeometries is ignored when using ForceIndex
            NUnit.Framework.Assert.AreEqual(_numValidGeoms, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestGetGeometriesInViewAllHints(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            sq.NoLockHint = true;
            sq.ForceSeekHint = true;
            sq.ForceIndex = spatialType ==  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry ? GeometrySpatialIndex : GeographySpatialIndex;

            sq.ValidateGeometries = validateGeometries;
            var geometries = sq.GetGeometriesInView(GetTestEnvelope(spatialType));

            NUnit.Framework.Assert.IsNotNull(geometries);
            // Note: ValidateGeometries is ignored when using ForceSeek or ForceIndex
            NUnit.Framework.Assert.AreEqual(_numValidGeoms, geometries.Count);
        }

        [NUnit.Framework.Test()]
        public void TestPerformanceSqlServer2008ExProvider()
        {
            // Note:
            // This test may fail with an InvalidCastException. This is caused by multiple versions of the 
            // Microsoft.SqlServer.Types assembly being available (e.g. SQL 2008 and 2012).
            // This can be solved with a <bindingRedirect> in the .config file.

            var spatialType =  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry;

            // testing with both providers using ExtentsMode = QueryIndividualFeatures (ie the "heaviest" lifting)
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(SqlServerProviderMode.WellKnownBinary, spatialType);
            SharpMap.Data.Providers.SqlServer2008 sqex = GetTestProvider(SqlServerProviderMode.NativeSqlBytes, spatialType);

            // Note: SqlServer2008Ex provider overrides ValidateGeometries behaviour, forcing this to true. 
            // So to compare apples-with-apples, the SqlServer2008 provider must also set ValidateGeometries = true.
            // From my testing, SqlServer2008 performance is slightly faster when using ValidateGeometries = true, 
            // as the SQL where clause is simpler (does not require explicitly excluding invalid geometries)
            sq.ValidateGeometries = true;
            
            GeoAPI.Geometries.Envelope envelope = GetTestEnvelope(spatialType);
            List<System.TimeSpan> measurements = new List<System.TimeSpan>(200);
            List<System.TimeSpan> measurementsex = new List<System.TimeSpan>(200);
            System.Diagnostics.Stopwatch timer;

            // 10 "startup" runs, followed by 200 measured runs
            for (int i = -10; i < 200; i++)
            {
                timer = System.Diagnostics.Stopwatch.StartNew();
                sq.GetGeometriesInView(envelope);
                timer.Stop();
                if (i >= 0) measurements.Add(timer.Elapsed);

                timer = System.Diagnostics.Stopwatch.StartNew();
                sqex.GetGeometriesInView(envelope);
                timer.Stop();
                if (i >= 0) measurementsex.Add(timer.Elapsed);
            }

            // Remove 10 slowest and 10 fastest times:
            measurements = measurements.OrderBy(x => x).Skip(10).Take(measurements.Count - 20).ToList();
            measurementsex = measurementsex.OrderBy(x => x).Skip(10).Take(measurementsex.Count - 20).ToList();

            // Average time:
            System.TimeSpan avg = System.TimeSpan.FromTicks((long)measurements.Average(x => x.Ticks));
            System.TimeSpan avgex = System.TimeSpan.FromTicks((long)measurementsex.Average(x => x.Ticks));

            // The SqlServer2008Ex provider should be faster:
            // Update Nov 2018: "should"....  highly dependent on client computer specs, and dataset characteristics
            // may also have an affect in real-world usage (eg num records, OGC geom type, geom complexity).
            // For the test dataset @ 100 records and also @ 3600 records on an AVERAGE computer, SqlServer2008 is
            // consistently out-performing SqlServer2008Ex as follows:
            //    for local instance SqlExpress, SqlServer2008  is consistently 30% faster than SqlServer2008Ex
            //    for SqlServer on local database server, - I don't have one to test against
            //    for Azure SQL (50DTU limit, test peaking at 26DTU), SqlServer2008 is about 5% faster than SqlServer2008Ex
            NUnit.Framework.Assert.Less(avgex, avg);
        }

        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestGetObjectIDsInView(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);
            sq.ValidateGeometries = validateGeometries;

            var objectIds = sq.GetObjectIDsInView(GetTestEnvelope(spatialType));

            NUnit.Framework.Assert.AreEqual(sq.ValidateGeometries ? _numValidatedGeoms : _numValidGeoms, objectIds.Count);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestExecuteIntersectionQuery(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            sq.ValidateGeometries = validateGeometries;

            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();

            sq.ExecuteIntersectionQuery(GetTestEnvelope(spatialType), ds);

            NUnit.Framework.Assert.AreEqual(sq.ValidateGeometries ? _numValidatedGeoms : _numValidGeoms, ds.Tables[0].Rows.Count);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, -179, 179, -89.4, 89.4)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, -180, 180, -90, 90)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, -179, 179, -89.4, 89.4)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, -180, 180, -90, 90)]
        public void TestExecuteIntersectionQueryExceedGeogMaxExtents(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, 
            double x1, double x2, double y1, double y2)
        {
            // occurs when user zooms out beyond map extents. For Geog, when latitude approaches 90 N or S can result in  
            // error 24206: "The specified input cannot be accepted because it contains an edge with antipodal points."
            // Longitudes exceeding -179.99999999 or 180.0 are "wrapped" resulting in unexpected polygon (also contributes to err 24206)
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();

            sq.ExecuteIntersectionQuery(new GeoAPI.Geometries.Envelope(x1, x2, y1, y2), ds);

            NUnit.Framework.Assert.AreEqual(sq.ValidateGeometries ? _numValidatedGeoms : _numValidGeoms, ds.Tables[0].Rows.Count);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestExecuteIntersectionQueryAllHints(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            sq.NoLockHint = true;
            sq.ForceSeekHint = true;
            sq.ForceIndex = spatialType ==  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry ? GeometrySpatialIndex : GeographySpatialIndex; ;
            sq.ValidateGeometries = validateGeometries;

            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();

            sq.ExecuteIntersectionQuery(GetTestEnvelope(spatialType), ds);
            // Note: ValidateGeometries ignored when using ForceSeek or ForceIndex
            NUnit.Framework.Assert.AreEqual(_numValidGeoms, ds.Tables[0].Rows.Count);
        }

        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureCount(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            int count = sq.GetFeatureCount();

            // includes NULL, EMPTY, and INVALID geoms
            NUnit.Framework.Assert.AreEqual(_numFeatures, count);
        }

        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureCountWithDefinitionQuery(SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(SqlServerProviderMode.WellKnownBinary, spatialType);

            sq.DefinitionQuery = "NAME LIKE 'A%'";

            int count = sq.GetFeatureCount();

            NUnit.Framework.Assert.LessOrEqual(count, _numValidGeoms);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetFeature(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            var feature = sq.GetFeature(1);

            NUnit.Framework.Assert.IsNotNull(feature);
            NUnit.Framework.Assert.IsNotNull(feature.Geometry);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureNonExisting(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            var feature = sq.GetFeature(99999999);

            NUnit.Framework.Assert.IsNull(feature);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureNullGeometry(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            var feature = sq.GetFeature(_idNullGeom);

            NUnit.Framework.Assert.IsNotNull(feature);
            NUnit.Framework.Assert.IsNull(feature.Geometry);
        }

        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureEmptyGeometry(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);

            var feature = sq.GetFeature(_idEmptyGeom);

            NUnit.Framework.Assert.IsNotNull(feature);
            NUnit.Framework.Assert.IsTrue(feature.Geometry.IsEmpty);
        }

        // NetTopologySuite.IO.WKBReader can make sense of SqlServer invalid Geoms!
        //[NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.WellKnownBinary,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, false)]
        //[NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, false)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geometry, true)]
        [NUnit.Framework.TestCase(SqlServerProviderMode.NativeSqlBytes,  SharpMap.Data.Providers.SqlServerSpatialObjectType.Geography, true)]
        public void TestGetFeatureInvalidGeometry(SqlServerProviderMode providerMode,  SharpMap.Data.Providers.SqlServerSpatialObjectType spatialType, bool validateGeometries)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(providerMode, spatialType);
            sq.ValidateGeometries = validateGeometries;

            var feature = sq.GetFeature(_idInvalidGeom);

            NUnit.Framework.Assert.IsNotNull(feature);
            if (providerMode== SqlServerProviderMode.NativeSqlBytes)
                // client side conversion always attempts validation
                NUnit.Framework.Assert.IsTrue(!feature.Geometry.IsEmpty && feature.Geometry.IsValid);
            else
            {
                if (validateGeometries)
                    NUnit.Framework.Assert.IsTrue(!feature.Geometry.IsEmpty && feature.Geometry.IsValid);
                else
                    NUnit.Framework.Assert.IsTrue(feature.Geometry.IsEmpty);
            }
        }
        
    }
}
