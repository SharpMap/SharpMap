using System;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;
using OgrProvider = SharpMap.Data.Providers.Ogr;

namespace UnitTests.Data.Providers
{
    [TestFixture]
    public class OgrProviderTest
    {
        [Test]
        public void TestCreateFromFeatureDataTable()
        {
            var fds = new SharpMap.Data.FeatureDataSet();
            FeatureDataTable fdt = null;
            try
            {
                var p = new Ogr("C:\\Users\\obe.IVV-AACHEN\\Downloads\\SharpMap Codeplex\\SHPFiles\\RxLevel-Idle.shp");
                p.SRID = 4326;
                var layer = string.Empty;
                if (!string.IsNullOrEmpty(layer)) p.LayerName = layer;

                p.ExecuteIntersectionQuery(p.GetExtents(), fds);
                fdt = fds.Tables[0];
                if (fdt.Rows.Count == 0)
                    throw new Exception("no data in layer");
            }
            catch (Exception ex)
            {
                throw new IgnoreException("Getting data failed", ex);
            }

            Assert.DoesNotThrow(() => Ogr.CreateFromFeatureDataTable(fdt, 
                ((FeatureDataRow)fdt.Rows[0]).Geometry.OgcGeometryType, 4326, 
                "Mapinfo File", 
                "C:\\Users\\obe.IVV-AACHEN\\Downloads\\SharpMap Codeplex\\SHPFiles\\RxLevel-Idle.tab"));
        }
    }
}