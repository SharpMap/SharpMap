namespace ExampleCodeSnippets
{
    public class LineSymbolizerTest
    {
        [NUnit.Framework.Test] 
        public void TestBasicLineSymbolizer()
        {
            var p = new SharpMap.Data.Providers.ShapeFile(@"d:\\daten\GeoFabrik\\roads.shp", false);
            var l = new SharpMap.Layers.VectorLayer("roads", p);
            //l.Style.Outline = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 5);
            l.Style.Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 1);
            l.Style.EnableOutline = false;
            var m = new SharpMap.Map(new System.Drawing.Size(1440,1080)) {BackColor = System.Drawing.Color.Cornsilk};
            m.Layers.Add(l);

            m.ZoomToExtents();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            m.GetMap();

            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering old method: {0}ms", sw.ElapsedMilliseconds));
            sw.Restart();
            var bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering old method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("NDSRoads1.bmp");


            var cls = new SharpMap.Rendering.Symbolizer.CachedLineSymbolizer();
            //cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 5) });
            cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 1) });

            l.Style.LineSymbolizer = cls;
            sw.Restart();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("NDSRoads2.bmp");

        }

        [NUnit.Framework.Test]
        public void TestWarpedLineSymbolizer()
        {
            var p = new SharpMap.Data.Providers.ShapeFile(@"d:\\daten\GeoFabrik\\Aurich\\roads.shp", false);

            var l = new SharpMap.Layers.VectorLayer("roads", p);
            var cls = new SharpMap.Rendering.Symbolizer.CachedLineSymbolizer();
            cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 2) });
            cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander { Pattern = SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.GetGreaterSeries(3, 3), Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1) });
            l.Style.LineSymbolizer = cls;

            var m = new SharpMap.Map(new System.Drawing.Size(720, 540)) {BackColor = System.Drawing.Color.Cornsilk};
            m.Layers.Add(l);

            m.ZoomToExtents();

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            var bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads1.bmp");

            cls.LineSymbolizeHandlers[1] = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
            {
                Pattern = SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.GetTriangleSeries(4, 7),
                Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Firebrick)
            };
            sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads2.bmp");

            //cls.LineSymbolizeHandlers[0] = cls.LineSymbolizeHandlers[1];
            cls.LineSymbolizeHandlers[1] = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
                                               {
                                                   Pattern = SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.GetZigZag(4, 4),
                                                   Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                                                   //Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Firebrick)
                                               };
            sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads3.bmp");

        }
    }
}