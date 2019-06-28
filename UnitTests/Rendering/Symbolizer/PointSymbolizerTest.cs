namespace UnitTests.Rendering.Symbolizer
{
    /// <summary>
    /// Test implemented PointSymbolizers
    /// </summary>
    public class PointSymbolizerTest
    {
        [NUnit.Framework.Test]
        public void TestCharacterPointSymbolizer()
        {
            var fdt = TestData.CreatingData.CreatePointFeatureDataTableFromArrays(
                TestData.CreatingData.GetRandomOrdinates(50, -180, 180), TestData.CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
            var layer = new SharpMap.Layers.VectorLayer("randompoints", geometryFeatureProvider);
            var cps  = new SharpMap.Rendering.Symbolizer.CharacterPointSymbolizer
                                              {
                                                  Halo = 1,
                                                  HaloBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Wheat),
                                                  //Font = new System.Drawing.Font("Arial", 12),
                                                  CharacterIndex = 0xcc,
                                              };
            layer.Style.PointSymbolizer = cps;
            var map = new SharpMap.Map(new System.Drawing.Size(720, 360));
            
            map.Layers.Add(layer);
            map.ZoomToExtents();
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "CharacterPointSymbolizer1.bmp"));

            cps.Rotation = -30;
            cps.Offset = new System.Drawing.PointF(4, 4);
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "CharacterPointSymbolizer2.bmp"));

            cps.Font = new System.Drawing.Font("Arial", 12);
            cps.Text = "ABC";
            cps.Offset = System.Drawing.PointF.Empty;
            cps.Rotation = -90;
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "CharacterPointSymbolizer3.bmp"));
        }


        [NUnit.Framework.Test]
        public void TestPathPointSymbolizer()
        {
            var fdt = TestData.CreatingData.CreatePointFeatureDataTableFromArrays(
                TestData.CreatingData.GetRandomOrdinates(50, -180, 180), TestData.CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
            var layer = new SharpMap.Layers.VectorLayer("randompoints", geometryFeatureProvider);
            var pps =
                SharpMap.Rendering.Symbolizer.PathPointSymbolizer.CreateSquare(new System.Drawing.Pen(System.Drawing.Color.Red, 2),
                                                                    new System.Drawing.SolidBrush(
                                                                        System.Drawing.Color.DodgerBlue), 20);
            layer.Style.PointSymbolizer = pps;
            var map = new SharpMap.Map(new System.Drawing.Size(720, 360));
            map.Layers.Add(layer);
            map.ZoomToExtents();
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "PathPointSymbolizer1.bmp"));

            pps.Rotation = -30;
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "PathPointSymbolizer2.bmp"));

            pps.Rotation = 0f;
            pps.Offset = new System.Drawing.PointF(4, 4);
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "PathPointSymbolizer3.bmp"));

            var gpTriangle1 = new System.Drawing.Drawing2D.GraphicsPath();
            gpTriangle1.AddPolygon(new [] { new System.Drawing.Point(0, 0), new System.Drawing.Point(5, 10), new System.Drawing.Point(10, 0), new System.Drawing.Point(0, 0), });
            var gpTriangle2 = new System.Drawing.Drawing2D.GraphicsPath();
            gpTriangle2.AddPolygon(new[] { new System.Drawing.Point(0, 0), new System.Drawing.Point(-5, -10), new System.Drawing.Point(-10, 0), new System.Drawing.Point(0, 0), });
            pps = new
                SharpMap.Rendering.Symbolizer.PathPointSymbolizer(new[]
                                                        {
                                                            new SharpMap.Rendering.Symbolizer.PathPointSymbolizer.PathDefinition
                                                                {
                                                                    Path = gpTriangle1,
                                                                    Line =
                                                                        new System.Drawing.Pen(
                                                                        System.Drawing.Color.Red, 2),
                                                                    Fill =
                                                                        new System.Drawing.SolidBrush(
                                                                        System.Drawing.Color.DodgerBlue)
                                                                },
                                                            new SharpMap.Rendering.Symbolizer.PathPointSymbolizer.PathDefinition
                                                                {
                                                                    Path = gpTriangle2,
                                                                    Line =
                                                                        new System.Drawing.Pen(
                                                                        System.Drawing.Color.DodgerBlue, 2),
                                                                    Fill =
                                                                        new System.Drawing.SolidBrush(
                                                                        System.Drawing.Color.Red)
                                                                }

                                                        }){ Rotation = 45 };

            layer.Style.PointSymbolizer = pps;
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "PathPointSymbolizer4.bmp"));
            pps.Rotation = 180;
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "PathPointSymbolizer5.bmp"));

        }

        [NUnit.Framework.Test]
        public void TestRasterPointSymbolizer()
        {
            var fdt = TestData.CreatingData.CreatePointFeatureDataTableFromArrays(
                TestData.CreatingData.GetRandomOrdinates(50, -180, 180), TestData.CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
            var layer = new SharpMap.Layers.VectorLayer("randompoints", geometryFeatureProvider);

            
            var wmnStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("UnitTests.Resources.Women.png");
            var wmnBmp = new System.Drawing.Bitmap(wmnStream);

            var rps =
                new SharpMap.Rendering.Symbolizer.RasterPointSymbolizer {Symbol = wmnBmp};

            layer.Style.PointSymbolizer = rps;
            var map = new SharpMap.Map(new System.Drawing.Size(720, 360));
            map.Layers.Add(layer);
            map.ZoomToExtents();
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "RasterPointSymbolizer1.bmp"));

            rps.Rotation = 45;
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "RasterPointSymbolizer2.bmp"));
            rps.Rotation = 0;

            var cps = new SharpMap.Rendering.Symbolizer.CharacterPointSymbolizer
                          {
                              Halo = 1,
                              HaloBrush = new System.Drawing.SolidBrush(System.Drawing.Color.WhiteSmoke),
                              Foreground = new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                              Font = new System.Drawing.Font("Arial", 12),
                              Text = "Anne",
                              Offset = new System.Drawing.PointF(0, rps.Size.Height*0.5f)
                
            };

            var lps = new SharpMap.Rendering.Symbolizer.ListPointSymbolizer { rps, cps };

            layer.Style.PointSymbolizer = lps;
            map.Layers.Add(layer);
            map.ZoomToExtents();
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "RasterPointSymbolizer3.bmp"));
        }

        [NUnit.Framework.Test]
        public void TestListPointSymbolizer()
        {
            var fdt = TestData.CreatingData.CreatePointFeatureDataTableFromArrays(
                TestData.CreatingData.GetRandomOrdinates(50, -180, 180), TestData.CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
            var layer = new SharpMap.Layers.VectorLayer("randompoints", geometryFeatureProvider);
            var pps =
                SharpMap.Rendering.Symbolizer.PathPointSymbolizer.CreateSquare(new System.Drawing.Pen(System.Drawing.Color.Red, 2),
                                                                    new System.Drawing.SolidBrush(
                                                                        System.Drawing.Color.DodgerBlue), 20);

            var cps = new SharpMap.Rendering.Symbolizer.CharacterPointSymbolizer
            {
                Halo = 1,
                HaloBrush = new System.Drawing.SolidBrush(System.Drawing.Color.WhiteSmoke),
                Foreground = new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                Font = new System.Drawing.Font("Arial", 12),
                CharacterIndex = 65
            };

            var lps = new SharpMap.Rendering.Symbolizer.ListPointSymbolizer { pps, cps };

            layer.Style.PointSymbolizer = lps;
            var map = new SharpMap.Map(new System.Drawing.Size(720, 360));
            map.Layers.Add(layer);
            map.ZoomToExtents();
            map.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "ListPointSymbolizer1.bmp"));
        }
    }
}
