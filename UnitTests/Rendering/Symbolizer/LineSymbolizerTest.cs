using NUnit.Framework;

namespace UnitTests.Rendering.Symbolizer
{
    public class LineSymbolizerTest
    {
        static LineSymbolizerTest()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        [NUnit.Framework.Test] 
        public void TestBasicLineSymbolizer()
        {
            var p = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("roads_aur.shp"), false);
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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering old method: {0}ms", sw.ElapsedMilliseconds));
            sw.Reset();
            sw.Start();
            var bmp = m.GetMap();
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering old method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "NDSRoads1.bmp"));


            var cls = new SharpMap.Rendering.Symbolizer.CachedLineSymbolizer();
            //cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Firebrick, 5) });
            cls.LineSymbolizeHandlers.Add(new SharpMap.Rendering.Symbolizer.PlainLineSymbolizeHandler { Line = new System.Drawing.Pen(System.Drawing.Color.Gold, 1) });

            l.Style.LineSymbolizer = cls;
            sw.Reset(); sw.Start();
            bmp = m.GetMap();
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "NDSRoads2.bmp"));

        }

        [Test]
        public void TestWarpedLineSymbolizer()
        {
            var p = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("roads_aur.shp"), false);

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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "AurichRoads2.bmp"));

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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "AurichRoads2-0.bmp"));

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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this),"AurichRoads2-1.bmp"));
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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "AurichRoads2-2.bmp"));

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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "AurichRoads2-3.bmp"));


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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "AurichRoads3.bmp"));
        }

        [NUnit.Framework.Test]
        public void TestCachedLineSymbolizerInTheme()
        {
            var p = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("roads_aur.shp"), false);

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
            System.Diagnostics.Trace.WriteLine(string.Format("Rendering new method: {0}ms", sw.ElapsedMilliseconds));
            bmp.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "AurichRoads1Theme.bmp"));
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
    }
}
