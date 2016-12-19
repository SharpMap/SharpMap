using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Data.Providers.Venus3;

namespace SharpMap.Test
{
    public class V3DemandProviderTests
    {
        private static string GetConnectionString(string mdb)
        {
            return string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Persist Security Info=False;", mdb);
        }

        [TestCase(@"D:\VenusDaten\V3\DFN\A0\DFN2950.mdb", V3DemandGeometry.Centroid)]
        [TestCase(@"D:\VenusDaten\V3\DFN\A0\DFN2950.mdb", V3DemandGeometry.Area)]
        [TestCase(@"D:\Projekte\BAM3473\Modell\Zellen_V2_2\BAM3473.mdb", V3DemandGeometry.Area)]
        [TestCase(@"D:\Projekte\BAM3473\Modell\Zellen_V2_2\BAM3473.mdb", V3DemandGeometry.Centroid)]
        [TestCase(@"D:\Projekte\BAM3473\Modell\Nachfrage\A0\BAM3473.mdb", V3DemandGeometry.Area)]
        [TestCase(@"D:\Projekte\BAM3473\Modell\Nachfrage\A0\BAM3473.mdb", V3DemandGeometry.Centroid)]
        // @"D:\Projekte\BAM3473\Modell\Zellen_V2_2\BAM3473.mdb"
        public void TestAccess(string mdb, V3DemandGeometry v3geom)
        {
            V3DemandProvider v3p = null;
            Assert.DoesNotThrow( () => v3p = new V3DemandProviderAccess(0, GetConnectionString(mdb)));
            v3p.Geometry = v3geom;

            // num features
            Assert.Greater(v3p.GetFeatureCount(), 0, "v3p.GetFeatureCount() > 0");

            // extent
            Envelope extent = null;
            Assert.DoesNotThrow(() => extent = v3p.GetExtents(), "exception");
            Assert.IsNotNull(extent, "extent != null");
            Assert.IsFalse(extent.IsNull, "extent.IsNull");


            // object Id
            Collection<uint> ids = null;
            Assert.DoesNotThrow(() => ids = v3p.GetObjectIDsInView(extent));
            Assert.IsNotNull(ids, "ids != null");
            Assert.Greater(ids.Count, 0, "ids.Count > 0");

            // geometries
            Collection<IGeometry> geometries = null;
            Assert.DoesNotThrow(() => geometries = v3p.GetGeometriesInView(extent));
            Assert.IsNotNull(geometries, "geometries != null");
            Assert.Greater(geometries.Count, 0, "geometries.Count > 0");

            var rnd = new Random();
            // geometry single access
            for (var i = 0; i < 5; i++)
            {
                IGeometry fdr = null;
                Assert.DoesNotThrow(() => fdr = v3p.GetGeometryByID(ids[rnd.Next(0, ids.Count - 1)]));
                Assert.IsNotNull(fdr, "fdr != null");
                //Assert.IsFalse(fdr.IsFeatureGeometryNull());
            }

            // features
            for (var i = 0; i < 5; i++)
            {
                FeatureDataRow fdr = null;
                Assert.DoesNotThrow(() => fdr = v3p.GetFeature(ids[rnd.Next(0, ids.Count - 1)]));
                Assert.IsNotNull(fdr, "fdr != null");
                Assert.IsFalse(fdr.IsFeatureGeometryNull());
            }

            var fds = new FeatureDataSet();
            Assert.DoesNotThrow(() => v3p.ExecuteIntersectionQuery(extent, fds));
            Assert.IsTrue(fds.Tables.Count == 1);
            Assert.IsTrue(fds.Tables[0].Rows.Count == v3p.GetFeatureCount());

            var ext2 = new Envelope(extent);
            ext2.ExpandBy(-0.3*extent.Width);
            Assert.DoesNotThrow(() => v3p.ExecuteIntersectionQuery(ext2, fds));
            Assert.IsTrue(fds.Tables.Count == 2);
            Assert.IsTrue(fds.Tables[1].Rows.Count < fds.Tables[0].Rows.Count);

            v3p.Include("EINW");
            v3p.Include("ERW");
            v3p.Include("SCHST");
            Assert.DoesNotThrow(() => v3p.ExecuteIntersectionQuery(ext2, fds));
            Assert.IsTrue(fds.Tables[2].Columns.IndexOf("_EINW") > -1);
            Assert.IsTrue(fds.Tables[2].Columns.IndexOf("_ERW") > -1);
            Assert.IsTrue(fds.Tables[2].Columns.IndexOf("_SCHST") > -1);
            //Assert.IsTrue(fds.Tables.Count == 3);
            //Assert.IsTrue(fds.Tables[0].Rows.Count < fds.Tables[1].Rows.Count);
        
        }

        [TestCase(@"D:\Projekte\BAM3473\Modell\Zellen_V2_2\BAM_a0.zgs",
                  @"D:\Projekte\BAM3473\Modell\Zellen_V2_2\BAM3473.mdb")]
        [TestCase(@"D:\Projekte\BAM3473\Modell\D_2015_160106\BAM_a0.zgs", 
                  @"D:\Projekte\BAM3473\Modell\Nachfrage\A0\BAM3473.mdb")]
        [Ignore("Run explicitly")]
        public void AddGeometries(string v2file, string v3file)
        {
            var v2zkk = Path.ChangeExtension(v2file, "zkk");
            var v2zpz = Path.ChangeExtension(v2file, "zpz");
            var zkk = xKKProvider.Load(v2zkk);
            var zpz = ZPZProvider.Load(v2zpz, zkk);

            var v3a = new V3DemandProviderAccess(31466, GetConnectionString(v3file));
            v3a.Geometry = V3DemandGeometry.Area;

            var count = zpz.GetFeatureCount();
            for (var i = 0; i < count; i++)
            {
                var id = (uint) i + 1;
                var itm = zpz.GetGeometryByID(id);
                if (itm != null && !itm.IsEmpty)
                    v3a.SetGeometryByID(id, itm);
                if (i%10 == 0) Console.Write(".");
            }
            Console.WriteLine();
        }
    }
}