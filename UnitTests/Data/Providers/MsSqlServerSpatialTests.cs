using System.Data;
using System.Data.SqlClient;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    
    [NUnit.Framework.TestFixture]
    public abstract class DbTests<TProvider> where TProvider: SpatialDbProvider
    {
        protected abstract System.Data.Common.DbConnection GetOpenConnection();
        protected abstract TProvider GetProvider();

        protected string TestTable;
        protected string TestOidColumn;
        protected string TestGeometryColumn;

        [NUnit.Framework.Test()]
        public void Test01EstablishConnection()
        {
            using (var conn = GetOpenConnection())
            {
                NUnit.Framework.Assert.IsTrue(conn.State == ConnectionState.Open);

            }
        }

        [NUnit.Framework.Test()]
        public void Test02CreateProvider()
        {
            using (var provider = GetProvider())
            {
                NUnit.Framework.Assert.IsNotNull(provider, "Creation of provider failed");
                NUnit.Framework.Assert.AreEqual(provider.ConnectionID, Properties.Settings.Default.MsSqlSpatial, "ConnectionID is not correct");
                NUnit.Framework.Assert.IsTrue(provider.SRID >= 0);
            }
        }

        [NUnit.Framework.Test()]
        public void Test03GetFeatureCount()
        {
            using (var provider = GetProvider())
            {
                var fc = provider.GetFeatureCount();
                NUnit.Framework.Assert.IsNotNull(fc > 0);
            }
        }

        [NUnit.Framework.Test()]
        public void Test04GetExtents()
        {
            using (var provider = GetProvider())
            {
                var extent = provider.GetExtents();
                NUnit.Framework.Assert.IsNotNull(extent);
                NUnit.Framework.Assert.IsFalse(extent.IsNull);
            }
        }

        [NUnit.Framework.Test()]
        public void Test05GetOidInView()
        {
            using (var provider = GetProvider())
            {
                var extent = provider.GetExtents();
                var oids = provider.GetObjectIDsInView(extent);
                NUnit.Framework.Assert.IsNotNull(oids);
                NUnit.Framework.Assert.AreEqual(provider.GetFeatureCount(), oids.Count);
            }
        }

        [NUnit.Framework.Test()]
        public void Test06GetGeometriesInView()
        {
            using (var provider = GetProvider())
            {
                var extent = provider.GetExtents();
                var geoms = provider.GetGeometriesInView(extent);
                NUnit.Framework.Assert.IsNotNull(geoms);
                NUnit.Framework.Assert.AreEqual(provider.GetFeatureCount(), geoms.Count);
            }
        }

        [NUnit.Framework.Test()]
        public void Test07GetGeometryByID()
        {
            using (var provider = GetProvider())
            {
                IGeometry result = null;
                NUnit.Framework.Assert.DoesNotThrow( () => result = provider.GetGeometryByID(1));
                NUnit.Framework.Assert.IsNotNull(result);
            }
        }

        [NUnit.Framework.Test()]
        public void Test09ExecuteIntersectionQuery()
        {
            using (var provider = GetProvider())
            {
                var ext = provider.GetExtents();
                ext.ExpandBy(-0.25*ext.Width, -0.25*ext.Height);

                var fds = new SharpMap.Data.FeatureDataSet();
                NUnit.Framework.Assert.DoesNotThrow(() => provider.ExecuteIntersectionQuery(ext, fds));
                NUnit.Framework.Assert.Greater(fds.Tables.Count, 0);
                NUnit.Framework.Assert.Greater(fds.Tables[0].Count, 0);
                NUnit.Framework.Assert.Less(fds.Tables[0].Count, provider.GetFeatureCount());
            }
        }
        [NUnit.Framework.Test()]
        public void Test10ExecuteIntersectionQuery()
        {
            using (var provider = GetProvider())
            {
                var ext = provider.GetExtents();
                ext.ExpandBy(-0.25 * ext.Width, -0.25 * ext.Height);

                var geom = provider.Factory.ToGeometry(ext);
                var fds = new SharpMap.Data.FeatureDataSet();
                NUnit.Framework.Assert.DoesNotThrow(() => provider.ExecuteIntersectionQuery(geom, fds));
                NUnit.Framework.Assert.Greater(fds.Tables.Count, 0);
                NUnit.Framework.Assert.Greater(fds.Tables[0].Count, 0);
                NUnit.Framework.Assert.Less(fds.Tables[0].Count, provider.GetFeatureCount());
            }
        }    
    }

    //[NUnit.Framework.Ignore("Requires SqlServerSpatial")]
    public class MsSqlServerSpatialTests : DbTests<MsSqlSpatial>
    {
        public MsSqlServerSpatialTests()
        {
            TestTable = "Countries";
            TestGeometryColumn = "Geometry";
            TestOidColumn = "Oid";
        }
        
        protected override System.Data.Common.DbConnection GetOpenConnection()
        {
            var connection = new SqlConnection(Properties.Settings.Default.MsSqlSpatial);
            connection.Open();
            return connection;
        }

        protected override MsSqlSpatial GetProvider()
        {
            var res = new SharpMap.Data.Providers.MsSqlSpatial(Properties.Settings.Default.MsSqlSpatial,
                TestTable, TestGeometryColumn, TestOidColumn);

            res.FeatureColumns.Add(new SharpMapFeatureColumn { Column = "NAME", DbType = DbType.AnsiString });
            res.FeatureColumns.Add(new SharpMapFeatureColumn { Column = "POPDENS", DbType = DbType.Double });

            return res;
        }

    }
}