using System.Data;
using GeoAPI.Geometries;
using NetTopologySuite;
using NetTopologySuite.Features;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    public class NtsProviderTests
    {
        [Test, Ignore("not relevant")]
        public void FeatureWithNullDataThrowsException()
        {
            /*
             * arrange
             */

            // create datasource
            var ds = DataTablePointTests.CreateDataTableSource();
            // add row with null value
            ds.BeginLoadData();
            
            var row = ds.LoadDataRow(new object[] { 1001, null, 1, 1 }, LoadOption.OverwriteChanges);
            ds.EndLoadData();
            Assert.That(ds.Rows.Count, Is.EqualTo(101));

            var dsp = new DataTablePoint(ds, "oid", "x", "y");
            Assert.That(dsp.GetFeatureCount(), Is.EqualTo(101));

            /*
             * act
             */
            // Create provider
            NtsProvider p = null;
            Assert.DoesNotThrow(() => p = new NtsProvider(dsp));
            
            /*
             * assert
             */
            Assert.That(p.SRID, Is.EqualTo(dsp.SRID));
            Assert.That(p.Factory, Is.EqualTo(dsp.Factory));
#if !LINUX
            Assert.That(p.GetFeatureCount(), Is.EqualTo(101));
#endif

        }

        [Test, Ignore("not relevant")]
        public void FeatureWithNullDataThrowsException2()
        {
            // arrange
            var gf = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
            var features = new[]
            {
                new Feature(gf.CreatePoint(new Coordinate(10, 14)), CreateAttributes(1, "Label 1")),
                new Feature(gf.CreatePoint(new Coordinate(11, 13)), CreateAttributes(2, "Label 2")),
                new Feature(gf.CreatePoint(new Coordinate(12, 12)), CreateAttributes(3, "Label 3")),
                new Feature(gf.CreatePoint(new Coordinate(13, 11)), CreateAttributes(4, "Label 4")),
                new Feature(gf.CreatePoint(new Coordinate(14, 10)), CreateAttributes(5, null)),
            };

            // act
            IProvider p = null;
            Assert.DoesNotThrow(() => p = new NtsProvider(features));
            
            // assert
            Assert.That(p, Is.Not.Null);
            Assert.That(p.GetFeatureCount(), Is.EqualTo(5));
            Assert.That(p.GetExtents(), Is.EqualTo(new Envelope(10, 14, 10, 14)));
            Assert.That(p.SRID, Is.EqualTo(gf.SRID));
        }

        private static AttributesTable CreateAttributes(int id, string label)
        {
            var res = new AttributesTable();
            res.Add("id", id);
            res.Add("label", label);

            return res;
        }
    }
}
