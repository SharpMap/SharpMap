namespace ExampleCodeSnippets
{
    [NUnit.Framework.TestFixture]
    public class PolygonSymbolizerTest
    {
        private class ModifiedBasicPolygonSymbolizer : SharpMap.Rendering.Symbolizer.BasicPolygonSymbolizer
        {
            private System.Drawing.Point _oldRenderOrigin;

            public override void Begin(System.Drawing.Graphics g, SharpMap.MapViewport map, int aproximateNumberOfGeometries)
            {
                base.Begin(g, map, aproximateNumberOfGeometries);
                _oldRenderOrigin = g.RenderingOrigin;
            }
            protected override void OnRenderInternal(SharpMap.MapViewport map, GeoAPI.Geometries.IPolygon polygon, System.Drawing.Graphics g)
            {
                var pt = polygon.Centroid;
                g.RenderingOrigin = 
                    System.Drawing.Point.Truncate(map.WorldToImage(pt.Coordinate));
                base.OnRenderInternal(map, polygon, g);
            }
            public override void End(System.Drawing.Graphics g, SharpMap.MapViewport map)
            {
                g.RenderingOrigin = _oldRenderOrigin;
            }

        }

        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        { }

        [NUnit.Framework.Test]
        public void TestPlainPolygonSymbolizer()
        {
            var provider = new SharpMap.Data.Providers.ShapeFile(
                "..\\..\\..\\WinFormSamples\\GeoData\\World\\countries.shp", true);
            var l = new SharpMap.Layers.Symbolizer.PolygonalVectorLayer("Countries", provider);
            l.Symbolizer = new ModifiedBasicPolygonSymbolizer
                {
                    Fill = new System.Drawing.Drawing2D.HatchBrush(
                            System.Drawing.Drawing2D.HatchStyle.WideDownwardDiagonal, 
                            System.Drawing.Color.Red /*,
                            System.Drawing.Color.LightPink*/),
                    UseClipping = false,
                    //Outline = System.Drawing.Pens.AliceBlue
                };

            var m = new SharpMap.Map(new System.Drawing.Size(1440, 1080)) { BackColor = System.Drawing.Color.Cornsilk };
            m.Layers.Add(l);

            m.ZoomToExtents();

            var sw = new System.Diagnostics.Stopwatch();
            var img = m.GetMap();
            
            sw.Start();
            img = m.GetMap();
            img.Save("PolygonSymbolizer-1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method:{0}ms", sw.ElapsedMilliseconds));

            l.Symbolizer = new SharpMap.Rendering.Symbolizer.BasicPolygonSymbolizer()
            {
                Fill = new System.Drawing.Drawing2D.HatchBrush(
                        System.Drawing.Drawing2D.HatchStyle.WideDownwardDiagonal,
                        System.Drawing.Color.Red/*,
                        System.Drawing.Color.LightPink*/),
                UseClipping = false,
                //Outline = System.Drawing.Pens.AliceBlue
            };

            sw.Reset(); sw.Start();
            img = m.GetMap();
            img.Save("PolygonSymbolizer-2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method:{0}ms", sw.ElapsedMilliseconds));
        
        }
    }

}
