using System.Drawing;
using BruTile.Web;

namespace MapWindowConference
{
    public static class Examples
    {

        public static void Example1()
        {
            var map = new SharpMap.Map(new Size(1280, 1084));

            SharpMap.Data.Providers.IProvider provider =
                new SharpMap.Data.Providers.ShapeFile(
                    @"C:\temp\Data\niedersachsen.shp\railways.shp");
            var vl = new SharpMap.Layers.VectorLayer("Railways", provider);

            map.Layers.Add(vl);

            map.ZoomToBox(vl.Envelope);

            var mapImage = map.GetMap();
            mapImage.Save("Example1.png",
                System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void Example2()
        {
            var map = new SharpMap.Map(new Size(1280, 1084));

            SharpMap.Data.Providers.IProvider provider =
                new SharpMap.Data.Providers.ShapeFile(
                    @"C:\temp\Data\niedersachsen.shp\railways.shp");

            var style = new SharpMap.Styles.VectorStyle();
            style.Line.Brush = Brushes.White;
            style.Line.DashPattern = new float[] { 4f, 4f };
            style.Line.Width = 4;
            style.EnableOutline = true;
            style.Outline.Brush = Brushes.Black;
            style.Outline.Width = 6;

            var vl = new SharpMap.Layers.VectorLayer("Railways", provider)
                         {Style = style};


            map.Layers.Add(vl);

            var env = vl.Envelope;
            env.ExpandBy(-0.45f *env.Width, -0.45 * env.Height);
            
            map.ZoomToBox(env);

            var mapImage = map.GetMap();
            mapImage.Save("Example2.png",
                System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void Example3()
        {
            var map = new SharpMap.Map(new Size(1280, 1084));

            SharpMap.Data.Providers.IProvider provider =
                new SharpMap.Data.Providers.ShapeFile(
                    @"C:\temp\Data\niedersachsen.shp\railways.shp");

            var cls = new SharpMap.Rendering.Symbolizer.CachedLineSymbolizer();
            cls.LineSymbolizeHandlers.Add(
                new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler 
                { Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 2) });

            var wls = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
            {
                Pattern =
                    SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.
                    GetGreaterSeries(3, 3),
                Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                Interval = 20
            };
            cls.LineSymbolizeHandlers.Add(wls);

            var vl = new SharpMap.Layers.Symbolizer.LinealVectorLayer(
                "Railways", provider);
            vl.Symbolizer = cls;

            map.Layers.Add(vl);

            var env = vl.Envelope;
            env.ExpandBy(-0.45f * env.Width, -0.45 * env.Height);

            map.ZoomToBox(env);

            var mapImage = map.GetMap();
            mapImage.Save("Example3.png",
                System.Drawing.Imaging.ImageFormat.Png);
        }

    }
}