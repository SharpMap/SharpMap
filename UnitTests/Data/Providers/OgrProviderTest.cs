using System;
using System.IO;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using OgrProvider = SharpMap.Data.Providers.Ogr;

namespace UnitTests.Data.Providers
{
    [TestFixture]
#if LINUX
    [Ignore("ONLY Windows native binaries present!)]
#endif
    public class OgrProviderTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GdalConfiguration.ConfigureGdal();
            if (!GdalConfiguration.Usable)
                Assert.Ignore($"GDAL not set up correctly, no binaries found in '{TestContext.CurrentContext.TestDirectory}\\gdal'");
        }

        [TestCase("roads_aur.shp", 31466, "Mapinfo File", "roads_aur.mif")]
        [TestCase("roads_ugl.shp", 4326, "Mapinfo File", "roads_ugl.mif")]
        public void TestCreateFromFeatureDataTable(string filePath, int srid, string driver, string connection)
        {
            var fds = new SharpMap.Data.FeatureDataSet();
            if (!Path.IsPathRooted(filePath))
                filePath = TestUtility.GetPathToTestFile(filePath);

            if (!System.IO.File.Exists(filePath))
                throw new IgnoreException($"'{filePath}' not found.");

            FeatureDataTable fdt = null;
            try
            {
                var p = new OgrProvider(filePath);
                if (p.SRID == 0)
                    p.SRID = srid;
                else
                    srid = p.SRID;

                string layer = string.Empty;
                if (!string.IsNullOrEmpty(layer)) p.LayerName = layer;

                p.ExecuteIntersectionQuery(p.GetExtents(), fds);
                fdt = fds.Tables[0];
                if (fdt.Rows.Count == 0)
                    throw new Exception("no data in layer");
                p.Dispose();
            }
            catch (Exception ex)
            {
                throw new IgnoreException("Getting data failed", ex);
            }

            Assert.DoesNotThrow(() => OgrProvider.CreateFromFeatureDataTable(fdt, 
                ((FeatureDataRow)fdt.Rows[0]).Geometry.OgcGeometryType, srid, 
                driver, connection));
        }
    }
}
