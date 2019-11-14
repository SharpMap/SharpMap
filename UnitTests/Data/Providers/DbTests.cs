namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture]
    public abstract class DbTests<TProvider> where TProvider : SharpMap.Data.Providers.SpatialDbProvider
    {
        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Close();
                }
            }
            catch (System.Exception)
            {
                throw new NUnit.Framework.IgnoreException("Provider cound not be instantiated!");
            }
        }
        
        protected abstract System.Data.Common.DbConnection GetOpenConnection();
        protected TProvider GetProvider()
        {
            try
            {
                return GetProviderInternal();
            }
            catch (System.Exception)
            {
                throw new NUnit.Framework.IgnoreException("Provider cound not be instantiated!");
            }
        }
        protected abstract TProvider GetProviderInternal();

        protected string TestTable;
        protected string TestOidColumn;
        protected string TestGeometryColumn;

        [NUnit.Framework.Test]
        public void Test01EstablishConnection()
        {
            using (var conn = GetOpenConnection())
            {
                NUnit.Framework.Assert.IsTrue(conn.State == System.Data.ConnectionState.Open);
            }
        }

        [NUnit.Framework.Test]
        public void Test02CreateProvider()
        {
            using (var provider = GetProvider())
            {
                NUnit.Framework.Assert.IsNotNull(provider, "Creation of provider failed");
                NUnit.Framework.Assert.AreEqual(provider.ConnectionID, Properties.Settings.Default.MsSqlSpatial, "ConnectionID is not correct");
                NUnit.Framework.Assert.IsTrue(provider.SRID >= 0);
            }
        }

        [NUnit.Framework.Test]
        public void Test03GetFeatureCount()
        {
            using (var provider = GetProvider())
            {
                var fc = provider.GetFeatureCount();
                NUnit.Framework.Assert.IsNotNull(fc > 0);
            }
        }

        [NUnit.Framework.Test]
        public void Test04GetExtents()
        {
            using (var provider = GetProvider())
            {
                var extent = provider.GetExtents();
                NUnit.Framework.Assert.IsNotNull(extent);
                NUnit.Framework.Assert.IsFalse(extent.IsNull);
            }
        }

        [NUnit.Framework.Test]
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

        [NUnit.Framework.Test]
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

        [NUnit.Framework.Test]
        public void Test07GetGeometryByID()
        {
            using (var provider = GetProvider())
            {
                GeoAPI.Geometries.IGeometry result = null;
                NUnit.Framework.Assert.DoesNotThrow( () => result = provider.GetGeometryByID(1));
                NUnit.Framework.Assert.IsNotNull(result);
            }
        }

        [NUnit.Framework.Test]
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
        [NUnit.Framework.Test]
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
    
        [NUnit.Framework.Test]
        public void Test11GetFeature()
        {
            using (var provider = GetProvider())
            {
                SharpMap.Data.FeatureDataRow feature = null;
                NUnit.Framework.Assert.DoesNotThrow(()=>  feature = provider.GetFeature(5));
                NUnit.Framework.Assert.NotNull(feature);
                NUnit.Framework.Assert.AreEqual(5, feature[provider.ObjectIdColumn]);
            }
        }

        [NUnit.Framework.Test]
        public void Test99GetMap()
        {
            var m = new SharpMap.Map(new System.Drawing.Size(512, 1048));
            var p = GetProvider();
            //p.SRID = 4326;
            //p.TargetSRID = 25832;
            var l = new SharpMap.Layers.VectorLayer("SpDb", p);
            m.Layers.Add(l);
            m.ZoomToExtents();

            var path = System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), $"SpatialDbProvider_{ImplementationName}.png");
            using (var i = m.GetMap())
                i.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            System.Diagnostics.Trace.WriteLine(new System.Uri(path).LocalPath);
        }

        /// <summary>
        /// Gets the name of the test implementation
        /// </summary>
        public abstract string ImplementationName { get; }
    }
}
