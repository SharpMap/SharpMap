namespace ExampleCodeSnippets
{
    /// <summary>
    /// Test implemented PointSymbolizers
    /// </summary>
    public class PointSymbolizerTest
    {
        [NUnit.Framework.Test]
        public void TestCharacterPointSymbolizer()
        {
            var fdt = CreatingData.CreatePointFeatureDataTableFromArrays(
                CreatingData.GetRandomOrdinates(50, -180, 180), CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.FeatureProvider(fdt);
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
            map.GetMap().Save("CharacterPointSymbolizer1.bmp");

            cps.Rotation = -30;
            cps.Offset = new System.Drawing.PointF(4, 4);
            map.GetMap().Save("CharacterPointSymbolizer2.bmp");

            cps.Font = new System.Drawing.Font("Arial", 12);
            cps.Text = "ABC";
            cps.Offset = System.Drawing.PointF.Empty;
            cps.Rotation = -90;
            map.GetMap().Save("CharacterPointSymbolizer3.bmp");
        }


        [NUnit.Framework.Test]
        public void TestPathPointSymbolizer()
        {
            var fdt = CreatingData.CreatePointFeatureDataTableFromArrays(
                CreatingData.GetRandomOrdinates(50, -180, 180), CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.FeatureProvider(fdt);
            var layer = new SharpMap.Layers.VectorLayer("randompoints", geometryFeatureProvider);
            var pps =
                SharpMap.Rendering.Symbolizer.PathPointSymbolizer.CreateSquare(new System.Drawing.Pen(System.Drawing.Color.Red, 2),
                                                                    new System.Drawing.SolidBrush(
                                                                        System.Drawing.Color.DodgerBlue), 20);
            layer.Style.PointSymbolizer = pps;
            var map = new SharpMap.Map(new System.Drawing.Size(720, 360));
            map.Layers.Add(layer);
            map.ZoomToExtents();
            map.GetMap().Save("PathPointSymbolizer1.bmp");

            pps.Rotation = -30;
            map.GetMap().Save("PathPointSymbolizer2.bmp");

            pps.Rotation = 0f;
            pps.Offset = new System.Drawing.PointF(4, 4);
            map.GetMap().Save("PathPointSymbolizer3.bmp");

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
            map.GetMap().Save("PathPointSymbolizer4.bmp");
            pps.Rotation = 180;
            map.GetMap().Save("PathPointSymbolizer5.bmp");

        }

        [NUnit.Framework.Test]
        public void TestRasterPointSymbolizer()
        {
            var fdt = CreatingData.CreatePointFeatureDataTableFromArrays(
                CreatingData.GetRandomOrdinates(50, -180, 180), CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.FeatureProvider(fdt);
            var layer = new SharpMap.Layers.VectorLayer("randompoints", geometryFeatureProvider);
            var rps =
                new SharpMap.Rendering.Symbolizer.RasterPointSymbolizer {Symbol = new System.Drawing.Bitmap("women.png")};

            layer.Style.PointSymbolizer = rps;
            var map = new SharpMap.Map(new System.Drawing.Size(720, 360));
            map.Layers.Add(layer);
            map.ZoomToExtents();
            map.GetMap().Save("RasterPointSymbolizer1.bmp");

            rps.Rotation = 45;
            map.GetMap().Save("RasterPointSymbolizer2.bmp");
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
            map.GetMap().Save("RasterPointSymbolizer3.bmp");
        }

        [NUnit.Framework.Test]
        public void TestListPointSymbolizer()
        {
            var fdt = CreatingData.CreatePointFeatureDataTableFromArrays(
                CreatingData.GetRandomOrdinates(50, -180, 180), CreatingData.GetRandomOrdinates(50, -90, 90), null);
            var geometryFeatureProvider = new SharpMap.Data.Providers.FeatureProvider(fdt);
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
            map.GetMap().Save("ListPointSymbolizer1.bmp");
        }
    }
}
//D:\Development\Codeplex\_SharpMap\Repository\Trunk\DemoWinForm\Resources
//D:\Development\Codeplex\_SharpMap\Repository\Trunk\DemoWinForm\Resources\Women.png
//