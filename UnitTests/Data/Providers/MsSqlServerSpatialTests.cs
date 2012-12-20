using System.Data;
using System.Data.SqlClient;
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
                NUnit.Framework.Assert.IsTrue(provider.SRID > 0);
                NUnit.Framework.Assert.IsNotNull(provider.GetFeatureCount()>0);
            }
        }

    }

    [NUnit.Framework.Ignore("Requires SqlServerSpatial")]
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
            return new SharpMap.Data.Providers.MsSqlSpatial(Properties.Settings.Default.MsSqlSpatial,
                TestTable, TestGeometryColumn, TestOidColumn);
        }

    }
}