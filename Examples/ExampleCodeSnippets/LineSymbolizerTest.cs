namespace ExampleCodeSnippets
{

/// <summary>
/// A (simple) street direction symbolizer
/// </summary>
public class StreetDirectionSymbolizer : SharpMap.Rendering.Symbolizer.BaseSymbolizer,
    SharpMap.Rendering.Symbolizer.ILineSymbolizer
{
    /// <summary>
    /// Creates an instance of this class
    /// </summary>
    public StreetDirectionSymbolizer()
    {
        RepeatInterval = 500;
        ArrowLength = 100;
        ArrowPen = new System.Drawing.Pen(System.Drawing.Color.Black, 2)
        {
            EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor
        };
    }

    /// <summary>
    /// Method to place the street direction symbols
    /// </summary>
    /// <param name="map"></param>
    /// <param name="lineString"></param>
    /// <param name="graphics"></param>
    private void OnRenderInternal(SharpMap.MapViewport map, GeoAPI.Geometries.ILineString lineString,
        System.Drawing.Graphics graphics)
    {

        var length = lineString.Length;
        var lil = new NetTopologySuite.LinearReferencing.LengthIndexedLine(lineString);
        if (length < RepeatInterval + ArrowLength)
        {
            var start = System.Math.Max(0, (length - ArrowLength)/2);
            var end = System.Math.Min(length, (length + ArrowLength)/2);
            var arrow = (GeoAPI.Geometries.ILineString) lil.ExtractLine(start, end);

            RenderArrow(map, graphics, arrow);

            return;
        }

        var numArrows = (int) ((lineString.Length - ArrowLength)/RepeatInterval);
        var offset = (lineString.Length - numArrows*RepeatInterval - ArrowLength)*0.5;

        while (offset + ArrowLength < lineString.Length)
        {
            var arrow = (GeoAPI.Geometries.ILineString) lil.ExtractLine(offset, offset + ArrowLength);
            RenderArrow(map, graphics, arrow);
            offset += RepeatInterval;
        }

    }

    /// <summary>
    /// Method to render the arrow
    /// </summary>
    /// <param name="map">The map</param>
    /// <param name="graphics">The graphics object</param>
    /// <param name="arrow">The arrow</param>
    private void RenderArrow(SharpMap.MapViewport map, System.Drawing.Graphics graphics, GeoAPI.Geometries.ILineString arrow)
    {
        var pts = new System.Drawing.PointF[arrow.Coordinates.Length];
        for (var i = 0; i < pts.Length; i++)
            pts[i] = map.WorldToImage(arrow.GetCoordinateN(i));
        graphics.DrawLines(ArrowPen, pts);
    }

    /// <summary>
    /// Gets or sets a value indicating at which distances a street direction marker should be drawn.
    /// </summary>
    public double RepeatInterval { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the length of the street direction arrow.
    /// </summary>
    public double ArrowLength { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the pen to use when drawing the marker
    /// </summary>
    public System.Drawing.Pen ArrowPen { get; set; }

    public override void Begin(System.Drawing.Graphics g, SharpMap.MapViewport map, int aproximateNumberOfGeometries)
    {
        base.Begin(g, map, aproximateNumberOfGeometries);

        //Adjust Arrow cap
        var size = (float) (ArrowLength/5/map.PixelWidth);
        ArrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(size, size);
    }

    public void Render(SharpMap.MapViewport map, GeoAPI.Geometries.ILineal geometry, System.Drawing.Graphics graphics)
    {
        if (geometry is GeoAPI.Geometries.IMultiLineString)
        {
            var mls = (GeoAPI.Geometries.IMultiLineString) geometry;
            for (var i = 0; i < mls.Count; i++)
            {
                OnRenderInternal(map, (GeoAPI.Geometries.ILineString) mls.GetGeometryN(i), graphics);
            }
            return;
        }
        OnRenderInternal(map, (GeoAPI.Geometries.ILineString) geometry, graphics);
    }

    protected override void ReleaseManagedResources()
    {
        base.ReleaseManagedResources();
        ArrowPen.Dispose();
    }

    public override object Clone()
    {
        var res = (StreetDirectionSymbolizer) MemberwiseClone();
        res.ArrowPen = new System.Drawing.Pen(((System.Drawing.SolidBrush) ArrowPen.Brush).Color, ArrowPen.Width);
        return res;
    }
}

    public class LineSymbolizerTest
    {
        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }
        [NUnit.Framework.Test] 
        public void TestBasicLineSymbolizer()
        {
            if (!System.IO.File.Exists("d:\\daten\\GeoFabrik\\roads.shp"))
                throw new NUnit.Framework.IgnoreException("");

            var p = new SharpMap.Data.Providers.ShapeFile(@"d:\\daten\\GeoFabrik\\roads.shp", false);
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
            sw.Reset();
            sw.Start();
            var bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering old method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("NDSRoads1.bmp");


            var cls = new SharpMap.Rendering.Symbolizer.CachedLineSymbolizer();
            //cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 5) });
            cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 1) });

            l.Style.LineSymbolizer = cls;
            sw.Reset(); sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("NDSRoads2.bmp");

        }

        [NUnit.Framework.Test]
        public void TestStreetDirectionSymbolizer()
        {
            if (!System.IO.File.Exists("d:\\daten\\GeoFabrik\\Aurich\\roads.shp"))
                throw new NUnit.Framework.IgnoreException("");

            var p = new SharpMap.Data.Providers.ShapeFile(@"d:\\daten\\GeoFabrik\\Aurich\\roads.shp", false);
            var l = new SharpMap.Layers.VectorLayer("roads", p);
            l.Style.Outline = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 7);
            l.Style.Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 3);
            l.Style.EnableOutline = true;
            var sd = new SharpMap.Layers.Symbolizer.LinealVectorLayer("streetd", p, new StreetDirectionSymbolizer()
            {
                ArrowLength = 100, RepeatInterval = 500
            });
            var m = new SharpMap.Map(new System.Drawing.Size(1440, 1080)) { BackColor = System.Drawing.Color.Cornsilk };
            m.Layers.Add(l);
            m.Layers.Add(sd);

            m.ZoomToExtents();
            m.Zoom *= 0.3;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("NDSRoadsSD.bmp");

        }
        [NUnit.Framework.Test]
        public void TestWarpedLineSymbolizer()
        {
            if (!System.IO.File.Exists("d:\\daten\\GeoFabrik\\Aurich\\roads.shp"))
                throw new NUnit.Framework.IgnoreException("");

            var p = new SharpMap.Data.Providers.ShapeFile(@"d:\\daten\\GeoFabrik\\Aurich\\roads.shp", false);

            var l = new SharpMap.Layers.VectorLayer("roads", p);
            
            var cls = new SharpMap.Rendering.Symbolizer.CachedLineSymbolizer();
            cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler
                                              {Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 2)});

            var wls = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
                          {
                              Pattern =
                                  SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.
                                  GetGreaterSeries(3, 3),
                              Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1)
                              , Interval = 20
                          };
            cls.LineSymbolizeHandlers.Add(wls);
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
                                                   Pattern =
                                                       SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.
                                                       GetTriangle(4, 0),
                                                   Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                                                   Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Firebrick)
                                                   ,Interval = 10
                                               };
            sw.Reset(); sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads2-0.bmp");

            cls.LineSymbolizeHandlers[1] = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
            {
                Pattern =
                    SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.
                    GetTriangle(4, 1),
                Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Firebrick)
                ,
                Interval = 10
            };
            sw.Reset(); sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads2-1.bmp");
            cls.LineSymbolizeHandlers[1] = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
            {
                Pattern =
                    SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.
                    GetTriangle(4, 2),
                Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Firebrick)
                ,
                Interval = 10
            };
            sw.Reset(); sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads2-2.bmp");

            cls.LineSymbolizeHandlers[1] = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
            {
                Pattern =
                    SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.
                    GetTriangle(4, 3),
                Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Firebrick)
                ,
                Interval = 10
            };
            sw.Reset(); sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads2-3.bmp");


            //cls.LineSymbolizeHandlers[0] = cls.LineSymbolizeHandlers[1];
            cls.LineSymbolizeHandlers[1] = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
                                               {
                                                   Pattern =
                                                       SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.GetZigZag(4, 4),
                                                   Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1),
                                                   //Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Firebrick)
                                               };
            sw.Reset(); sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads3.bmp");
        }

        [NUnit.Framework.Test]
        public void TestCachedLineSymbolizerInTheme()
        {
            if (!System.IO.File.Exists("d:\\daten\\GeoFabrik\\Aurich\\roads.shp"))
                throw new NUnit.Framework.IgnoreException("");
            var p = new SharpMap.Data.Providers.ShapeFile(@"d:\\daten\\GeoFabrik\\Aurich\\roads.shp", false);

            var l = new SharpMap.Layers.VectorLayer("roads", p);
            var theme = new ClsTheme(l.Style);
            l.Theme = theme;

            var m = new SharpMap.Map(new System.Drawing.Size(720, 540)) { BackColor = System.Drawing.Color.Cornsilk };
            m.Layers.Add(l);

            m.ZoomToExtents();

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            var bmp = m.GetMap();
            sw.Stop();
            System.Console.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save("AurichRoads1Theme.bmp");
        }

        internal class ClsTheme : SharpMap.Rendering.Thematics.ITheme
        {
            private SharpMap.Styles.VectorStyle _style;

            public ClsTheme(SharpMap.Styles.VectorStyle style)
            {
                _style = style;
            }

            #region Implementation of ITheme

            /// <summary>
            /// Returns the style based on a feature
            /// </summary>
            /// <param name="attribute">Set of attribute values to calculate the <see cref="SharpMap.Styles.IStyle"/> from</param>
            /// <returns>The style</returns>
            public SharpMap.Styles.IStyle GetStyle(SharpMap.Data.FeatureDataRow attribute)
            {
                var res = _style;

                var cls = new SharpMap.Rendering.Symbolizer.CachedLineSymbolizer();
                cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 2) });

                var wls = new SharpMap.Rendering.Symbolizer.WarpedLineSymbolizeHander
                {
                    Pattern =
                        SharpMap.Rendering.Symbolizer.WarpedLineSymbolizer.
                        GetGreaterSeries(3, 3),
                    Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 1)
                    ,
                    Interval = 20
                    
                };
                cls.LineSymbolizeHandlers.Add(wls);
                cls.ImmediateMode = true;

                res.LineSymbolizer = cls;

                return res;
            }

            #endregion
        }

        [NUnit.Framework.Test]
        public void TestTransformation()
        {
            var m = new SharpMap.Map(new System.Drawing.Size(640, 320));

            var l = new SharpMap.Layers.Symbolizer.PuntalVectorLayer("l",
            new SharpMap.Data.Providers.GeometryProvider(m.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(0, 51.478885))),
            SharpMap.Rendering.Symbolizer.PathPointSymbolizer.CreateCircle(System.Drawing.Pens.Aquamarine, System.Drawing.Brushes.BurlyWood, 24));

            var ctFact = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            l.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);
            l.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);

            m.Layers.Add(new SharpMap.Layers.TileLayer(BruTile.Predefined.KnownTileSources.Create(),"b"));
            m.Layers.Add(l);

            var e = new GeoAPI.Geometries.Envelope(-0.02, 0.02, 51.478885 - 0.01, 51.478885 + 0.01);
            e = GeoAPI.CoordinateSystems.Transformations.GeometryTransform.TransformBox(e,
                l.CoordinateTransformation.MathTransform);
            m.ZoomToBox(e);
            m.GetMap().Save("Greenwich.png", System.Drawing.Imaging.ImageFormat.Png);

        }
    }
}
