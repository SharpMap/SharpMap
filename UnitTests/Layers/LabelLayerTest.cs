using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace UnitTests.Layers
{
    public class LabelLayerTest
    {
        private FeatureDataTable _featureDataTable;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var fdt = new FeatureDataTable();
            fdt.Columns.Add(new DataColumn("ID", typeof (int)));
            fdt.Columns.Add(new DataColumn("LABEL", typeof (string)));
            fdt.Columns.Add(new DataColumn("HALIGN", typeof (int)));
            fdt.Columns.Add(new DataColumn("VALIGN", typeof (int)));

            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4236);
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var fdr = fdt.NewRow();
                    fdr[0] = i*3 + j;
                    fdr[1] = string.Format("Point({0}, {1})\nID {2}", i, j, i*3 + j);
                    fdr[2] = j;
                    fdr[3] = i;
                    fdr.Geometry = factory.CreatePoint(new Coordinate(j*100, i*100));
                    fdt.AddRow(fdr);
                }
            }
            _featureDataTable = fdt;
        }

        private static ITheme CreateTheme()
        {
            return new CustomTheme(StyleBasedOnAlignment);
        }

        private static IStyle StyleBasedOnAlignment(FeatureDataRow dr)
        {
            var style = new LabelStyle
            {
                HorizontalAlignment = (LabelStyle.HorizontalAlignmentEnum)(int) dr[2],
                VerticalAlignment = (LabelStyle.VerticalAlignmentEnum)(int) dr[3],
                Rotation = -20,
                BackColor = Brushes.Pink,
                Halo = new Pen(Brushes.LightBlue, 2)
            };
            return style;
        }

        [Test]
        public void MultiLineCenterAlignedTest()
        {
            using (var m = new SharpMap.Map(new Size(600, 400)))
            {
                m.BackColor = Color.SeaShell;
                //_featureDataTable.Clear();
                var gfp = new GeometryFeatureProvider(_featureDataTable);
                var vl = new VectorLayer("VL", gfp);
                var ll = new LabelLayer("MultiLineCenterAligned") {DataSource = gfp};
                ll.Theme = CreateTheme();
                ll.LabelColumn = "LABEL";
                m.Layers.Add(vl);
                m.Layers.Add(ll);

                m.ZoomToExtents();
                using (var mapImage = m.GetMap())
                    mapImage.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "MultiLineCenterAligned.png"), ImageFormat.Png);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _featureDataTable.Dispose();
        }
    }
}
