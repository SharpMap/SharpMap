namespace UnitTests.Data.Providers
{
    public class MsSqlServerSpatialTests : DbTests<SharpMap.Data.Providers.MsSqlSpatial>
    {
        public MsSqlServerSpatialTests()
        {
            TestTable = "Countries";
            TestGeometryColumn = "Geometry";
            TestOidColumn = "Oid";
        }

        public override string ImplementationName
        {
            get { return "MSSqlSpatial"; }
        }

        protected override System.Data.Common.DbConnection GetOpenConnection()
        {
            var connection = new System.Data.SqlClient.SqlConnection(Properties.Settings.Default.MsSqlSpatial);
            connection.Open();
            return connection;
        }

        protected override SharpMap.Data.Providers.MsSqlSpatial GetProviderInternal()
        {
            var res = new SharpMap.Data.Providers.MsSqlSpatial(Properties.Settings.Default.MsSqlSpatial,
                TestTable, TestGeometryColumn, TestOidColumn);

            res.FeatureColumns.Add(new SharpMap.Data.Providers.SharpMapFeatureColumn { Column = "NAME", DbType = System.Data.DbType.AnsiString });
            res.FeatureColumns.Add(new SharpMap.Data.Providers.SharpMapFeatureColumn { Column = "POPDENS", DbType = System.Data.DbType.Double, As = "BevDichte" });

            return res;
        }

    }
}