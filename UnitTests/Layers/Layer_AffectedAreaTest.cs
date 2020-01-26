using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Decoration;
using LabelStyle = SharpMap.Styles.LabelStyle;

namespace UnitTests.Layers
{
    [TestFixture, Category("RequiresWindows")]
    public class Layer_AffectedAreaTest
    {
        public enum LabelLayerMode
        {
            BasicLabel,
            BasicLabelRot,
            TextOnPath,
            PathOnLabel
        }
        
        private readonly float[] _rotations = new float[] {0f, 30f, 60f, 90f, 120f, 150f, 180f, 210f, 240f, 270f, 310f, 330f};
    
        /// <summary>
        /// Validate calculated affectedArea on 3 primary code paths of LabelLayer
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="testRotations"></param>
        [NUnit.Framework.TestCase(LabelLayerMode.BasicLabel, true)]
        [NUnit.Framework.TestCase(LabelLayerMode.BasicLabelRot, true)]
        [NUnit.Framework.TestCase(LabelLayerMode.TextOnPath, true)]
        [NUnit.Framework.TestCase(LabelLayerMode.PathOnLabel, true)]
        public void LabelLayer_AffectedArea(LabelLayerMode mode,  bool testRotations)
        {
            using (var map = new Map())
            {
                ConfigureMap(map);

                Envelope extents = null;
                switch (mode)
                {
                    case LabelLayerMode.BasicLabel:
                    case LabelLayerMode.BasicLabelRot:
                        AddBasicLabelLayers(map, mode);
                        extents = map.GetExtents();
                        extents.ExpandBy(0.125);
                        break;
                    case LabelLayerMode.TextOnPath:
                        AddTextOnPathLayers(map);
                        extents = map.GetExtents();
                        break;
                    case LabelLayerMode.PathOnLabel:
                        AddPathOnLabelLayers(map);
                        extents = map.GetExtents();
                        extents.ExpandBy(0.125);
                        break;
                }

                foreach (var rot in _rotations)
                {
                    SetMapTransform(map, rot);
                    map.ZoomToBox(extents, true);
                    
                    var affectedArea = GetAffectedArea(map, (Layer)map.Layers[1]);
                    AddAffectedAreaLayer(map, affectedArea);

                    using (var img = map.GetMap())
                        img.Save(Path.Combine(UnitTestsFixture.GetImageDirectory(this), $"LabelLayer_{mode.ToString()}_{rot:000}.png"),
                            System.Drawing.Imaging.ImageFormat.Png);

                    // remove affected area layer
                    map.Layers.RemoveAt(2);
                    if (!testRotations) break;
                }
            }
        }
        private void ConfigureMap(Map map)
        {
            map.Size = new Size(800, 640);
            map.BackColor = Color.AliceBlue;
            map.SRID = 4326;
            map.Decorations.Add(new NorthArrow());
            map.Decorations.Add(new Disclaimer()
            {
                 Text = "Affected Area envelope should surround label"
            });
        }
        private void SetMapTransform(Map map, float rotationDeg)
        {
            if (rotationDeg.Equals(0f))
            {
                map.MapTransform = null;
            }
            else
            {
                var matrix = new System.Drawing.Drawing2D.Matrix();
                matrix.RotateAt(rotationDeg, new PointF(map.Size.Width * 0.5f, map.Size.Height * 0.5f));
                map.MapTransform = matrix;
            }
        }
        private Envelope GetAffectedArea(Map map, Layer layer)
        {
            using (var img = new Bitmap(map.Size.Width, map.Size.Height))
            using (var g = Graphics.FromImage(img))
            {
                layer.Render(g, (MapViewport) map, out var affectedArea);
                return affectedArea;
            }
        }
        private void AddAffectedAreaLayer(Map map, Envelope affectedArea)
        {
            var coords = new Coordinate[]
            {
                new Coordinate(affectedArea.TopLeft()),
                new Coordinate(affectedArea.TopRight()),
                new Coordinate(affectedArea.BottomRight()),
                new Coordinate(affectedArea.BottomLeft()),
                new Coordinate(affectedArea.TopLeft())
            };

            var polygon = new Polygon(new LinearRing(coords));
            var gp = new GeometryProvider(polygon);
            var vLayer = new VectorLayer("Affected Area")
            {
                DataSource = gp,
                SRID = map.SRID
            };
            vLayer.Style.Fill = null;
            vLayer.Style.EnableOutline = true;

            map.Layers.Add(vLayer);
        }
        private void AddBasicLabelLayers(Map map, LabelLayerMode mode)
        {
            var fdt = new FeatureDataTable();
            fdt.Columns.Add(new DataColumn("ID", typeof(int)));
            fdt.Columns.Add(new DataColumn("LABEL", typeof(string)));

            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4236);
            var fdr = fdt.NewRow();
            fdr[0] = 1;
            fdr[1] = "Test Label";
            fdr.Geometry = factory.CreatePoint(new Coordinate(99, 13));
            fdt.AddRow(fdr);
            
            var vLyr = new VectorLayer("Basic Point", new GeometryFeatureProvider(fdt));
            map.Layers.Add(vLyr);

            var labLyr = new LabelLayer("Basic Point Labels")
            {
                DataSource = vLyr.DataSource,
                Enabled = true,
                LabelColumn = "LABEL",
                Style = new LabelStyle
                {
                    Offset = new PointF(20, -20),
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
                    VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom
                }
            };
            
            if (mode == LabelLayerMode.BasicLabelRot)
                labLyr.Style.Rotation = 315f;
            
            map.Layers.Add(labLyr);
        }
        private void AddTextOnPathLayers(Map map)
        {
            string shapefile = System.IO.Path.GetFullPath(
                TestContext.CurrentContext.TestDirectory +
                @"\..\..\..\..\Examples\WinFormSamples\GeoData\World/shp_textonpath/DeMo_Quan5.shp");

            // TextOnPath (adopted from WinformSamples)
            var vLyr = new VectorLayer("TextOnPath");
            vLyr.DataSource = new ShapeFile(shapefile);
            (vLyr.DataSource as ShapeFile).Encoding = Encoding.UTF8;
            vLyr.Style.Fill = new SolidBrush(Color.Yellow);
            vLyr.Style.Line = new Pen(Color.Yellow, 4);
            vLyr.Style.Outline = new Pen(Color.Black, 5);
            ;
            vLyr.Style.EnableOutline = true;
            vLyr.SRID = map.SRID;
            (vLyr.DataSource as ShapeFile).FilterDelegate = TextOnPathFilter;
            map.Layers.Add(vLyr);

            var labLyr = new LabelLayer("TextOnPath labels");
            labLyr.DataSource = vLyr.DataSource;
            labLyr.Enabled = true;
            labLyr.LabelColumn = "tenduong";
            labLyr.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
            labLyr.Style = new LabelStyle();
            labLyr.Style.ForeColor = Color.White;
            labLyr.Style.Font = new Font(FontFamily.GenericSerif, 9f, FontStyle.Bold);
            labLyr.Style.Halo = new Pen(Color.Black, 2f);
            labLyr.Style.IsTextOnPath = true;
            labLyr.Style.CollisionDetection = false;
            labLyr.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            labLyr.Style.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
            labLyr.SRID = 4326;
            map.Layers.Add(labLyr);
        }
        public static bool TextOnPathFilter(FeatureDataRow fdr)
        {
            return fdr[1].ToString() == "Trần Phú";
        }
        private void AddPathOnLabelLayers(Map map)
        {
            var fdt = new FeatureDataTable();
            fdt.Columns.Add(new DataColumn("ID", typeof(int)));
            fdt.Columns.Add(new DataColumn("LABEL", typeof(string)));

            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4236);
            var fdr = fdt.NewRow();
            fdr[0] = 1;
            fdr[1] = "Test Label";
            fdr.Geometry = factory.CreateLineString(new[]
            {
                new Coordinate(98.5, 12.5),
                new Coordinate(99.5, 13.5)
            });
            fdt.AddRow(fdr);
            
            var vLyr = new VectorLayer("Basic Line", new GeometryFeatureProvider(fdt));
            vLyr.Style.Line = new Pen(Color.DodgerBlue, 2f);
            map.Layers.Add(vLyr);

            var labLyr = new LabelLayer("Basic Line Labels")
            {
                DataSource = vLyr.DataSource,
                Enabled = true,
                LabelColumn = "LABEL",
            };
            labLyr.Style.IsTextOnPath = false;
            map.Layers.Add(labLyr);   
        }
    }
}
