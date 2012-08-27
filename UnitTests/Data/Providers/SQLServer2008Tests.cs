using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture]
    public class SQLServer2008Tests
    {
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        [NUnit.Framework.Test()]
        public void VerifySchemaDetection()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = new SharpMap.Data.Providers.SqlServer2008("", "schema.TableName", "oidcolumn");
            Assert.AreEqual("schema", sq.TableSchema);
            Assert.AreEqual("TableName", sq.Table);
            Assert.AreEqual("oidcolumn", sq.ObjectIdColumn);
        }
    }
}
