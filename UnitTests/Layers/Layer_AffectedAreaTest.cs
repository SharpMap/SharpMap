using System.Collections.Generic;
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
            PathOnLabel,
            SineCurve,
            SineCurveClipped,
            SineCurveExtended
        }

        private readonly float[] _rotations = new float[]
            {0f, 30f, 60f, 90f, 120f, 150f, 180f, 210f, 240f, 270f, 310f, 330f};

        /// <summary>
        /// Validate calculated affectedArea on 3 primary code paths of LabelLayer
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="testRotations"></param>
        [NUnit.Framework.TestCase(LabelLayerMode.BasicLabel, true)]
        [NUnit.Framework.TestCase(LabelLayerMode.BasicLabelRot, true)]
        [NUnit.Framework.TestCase(LabelLayerMode.TextOnPath, true)]
        [NUnit.Framework.TestCase(LabelLayerMode.PathOnLabel, true)]
        public void LabelLayer_AffectedArea(LabelLayerMode mode, bool testRotations)
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

                    var affectedArea = GetAffectedArea(map, (Layer) map.Layers[1]);
                    AddAffectedAreaLayer(map, affectedArea);

                    using (var img = map.GetMap())
                        img.Save(
                            Path.Combine(UnitTestsFixture.GetImageDirectory(this),
                                $"LabelLayer_{mode.ToString()}_{rot:000}.png"),
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

        private void AddAffectedAreaLayer(Map map, Envelope affectedAreaEnv)
        {
            var coords = new Coordinate[]
            {
                new Coordinate(affectedAreaEnv.TopLeft()),
                new Coordinate(affectedAreaEnv.TopRight()),
                new Coordinate(affectedAreaEnv.BottomRight()),
                new Coordinate(affectedAreaEnv.BottomLeft()),
                new Coordinate(affectedAreaEnv.TopLeft())
            };
            
            var gp = new GeometryProvider(new Polygon(new LinearRing(coords)));
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

            var lLyr = new LabelLayer("Basic Point Labels")
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
                lLyr.Style.Rotation = 315f;

            map.Layers.Add(lLyr);
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

            var lLyr = new LabelLayer("TextOnPath labels");
            lLyr.DataSource = vLyr.DataSource;
            lLyr.Enabled = true;
            lLyr.LabelColumn = "tenduong";
            lLyr.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
            lLyr.Style = new LabelStyle();
            lLyr.Style.ForeColor = Color.White;
            lLyr.Style.Font = new Font(FontFamily.GenericSerif, 9f, FontStyle.Bold);
            lLyr.Style.Halo = new Pen(Color.Black, 2f);
            lLyr.Style.IsTextOnPath = true;
            lLyr.Style.CollisionDetection = false;
            lLyr.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            lLyr.Style.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
            lLyr.SRID = 4326;
            map.Layers.Add(lLyr);
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

            var lLyr = new LabelLayer("Basic Line Labels")
            {
                DataSource = vLyr.DataSource,
                Enabled = true,
                LabelColumn = "LABEL",
            };
            lLyr.Style.IsTextOnPath = false;
            map.Layers.Add(lLyr);
        }

        /// <summary>
        /// Validate PathLabel using NTS LengthIndexedLine (ie NOT using old TextOnPath)
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="testRotations"></param>
        [NUnit.Framework.TestCase(LabelLayerMode.SineCurve, true, false, true)]
        [NUnit.Framework.TestCase(LabelLayerMode.SineCurveClipped, true, false, false)]
        [NUnit.Framework.TestCase(LabelLayerMode.SineCurveExtended, true, false, false)]
        public void PathLabel_AffectedArea(LabelLayerMode mode, bool testRotations, bool testHzAlign, bool testVtAlign)
        {
            var hzAlgins = new[]
            {
                LabelStyle.HorizontalAlignmentEnum.Center,
                LabelStyle.HorizontalAlignmentEnum.Left,
                LabelStyle.HorizontalAlignmentEnum.Right
            };

            var vtAlgins = new[]
            {
                LabelStyle.VerticalAlignmentEnum.Top, 
                LabelStyle.VerticalAlignmentEnum.Middle,
                LabelStyle.VerticalAlignmentEnum.Bottom
            };

            foreach (var hzAlign in hzAlgins)
            {
                foreach (var vtAlign in vtAlgins)
                {
                    using (var map = new Map())
                    {
                        ConfigureMap(map);

                        AddSineCurveLayers(map, mode, hzAlign, vtAlign);

                        if (mode == LabelLayerMode.SineCurveClipped)
                        {
                            map.ZoomToExtents();
                            map.Center = new Coordinate(map.Center.X - map.Envelope.Width * 0.25, map.Center.Y);
                            map.Zoom *= 0.65;
                        }

                        foreach (var rot in _rotations)
                        {
                            SetMapTransform(map, rot);

                            switch (mode)
                            {
                                case LabelLayerMode.SineCurve:
                                    map.ZoomToExtents();
                                    map.Zoom *= 1.25;
                                    break;
                                case LabelLayerMode.SineCurveClipped:
//                            map.ZoomToExtents();
//                            map.Zoom *= 0.6;
                                    break;
                                case LabelLayerMode.SineCurveExtended:
                                    map.ZoomToExtents();
                                    map.Zoom *= 1.25;
                                    break;
                            }

                            var affectedArea = GetAffectedArea(map, (Layer) map.Layers[1]);
                            if (affectedArea == null)
                                continue;
                            
                            AddAffectedAreaLayer(map, affectedArea);

                            using (var img = map.GetMap())
                                img.Save(
                                    Path.Combine(UnitTestsFixture.GetImageDirectory(this),
                                        $"PathLabel_{mode.ToString()}_Hz{hzAlign.ToString()}_Vt{vtAlign.ToString()}_{rot:000}.png"),
                                    System.Drawing.Imaging.ImageFormat.Png);

                            // remove affected area layer
                            map.Layers.RemoveAt(2);
                            if (!testRotations) break;
                        }
                    }
                    if (!testVtAlign) break;
                }
                if (!testHzAlign) break;
            }
        }

        private void AddSineCurveLayers(Map map, LabelLayerMode mode,  
            LabelStyle.HorizontalAlignmentEnum hzAlign, 
            LabelStyle.VerticalAlignmentEnum vtAlign)
        {
            string text;
            switch (mode)
            {
                case LabelLayerMode.SineCurveExtended:
                    text =
                        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam";//", quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
                    break;
                default:
                    text = "Lorem ipsum olor sit amet, consectetur adipisici elit";
                    break;
            }

            var fdt = new SharpMap.Data.FeatureDataTable();
            fdt.BeginInit();
            fdt.Columns.Add(new System.Data.DataColumn("ID", typeof(int)));
            fdt.Columns.Add(new System.Data.DataColumn("LABEL", typeof(string)));
            fdt.PrimaryKey = new[] {fdt.Columns[0]};
            fdt.EndInit();
            fdt.BeginLoadData();
            var fdr = (SharpMap.Data.FeatureDataRow) fdt.LoadDataRow(new object[] {1, text}, true);
            fdr.Geometry = CreateSineLine(new GeoAPI.Geometries.Coordinate(10, 10));
            fdt.EndLoadData();

            var vLyr = new SharpMap.Layers.VectorLayer("Geometry", new GeometryFeatureProvider(fdt));
            vLyr.Style.Line = new System.Drawing.Pen(System.Drawing.Color.Black, 4);
            vLyr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            vLyr.SRID = map.SRID;
            map.Layers.Add(vLyr);

            var lLyr = new SharpMap.Layers.LabelLayer("Label") {DataSource = vLyr.DataSource, LabelColumn = "LABEL"};
            lLyr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            lLyr.Style.ForeColor = System.Drawing.Color.Cyan;
            lLyr.Style.BackColor = new SolidBrush(Color.FromArgb(128,Color.LightSlateGray));
            //lLyr.Style.Halo = new Pen(Color.Yellow, 4);
            lLyr.Style.IgnoreLength = mode==LabelLayerMode.SineCurveExtended;
            lLyr.Style.HorizontalAlignment = hzAlign;
            lLyr.Style.VerticalAlignment = vtAlign;
            //ll.Style.IsTextOnPath = textOnPath;
            map.Layers.Add(lLyr);
        }

        private static GeoAPI.Geometries.ILineString CreateSineLine(GeoAPI.Geometries.Coordinate offset,
            double scaleY = 100)
        {
            var factory = new NetTopologySuite.Geometries.GeometryFactory();
            var cs = factory.CoordinateSequenceFactory.Create(181, 2);
            for (int i = 0; i <= 180; i++)
            {
                cs.SetOrdinate(i, GeoAPI.Geometries.Ordinate.X, offset.X + 2 * i);
                cs.SetOrdinate(i, GeoAPI.Geometries.Ordinate.Y, offset.Y + scaleY * System.Math.Sin(2d * i * System.Math.PI/180d));
            }
            return factory.CreateLineString(cs);        
        }
    }
}
