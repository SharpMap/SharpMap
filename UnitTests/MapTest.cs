using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Moq;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using SharpMap;
using SharpMap.Data.Providers;
using Geometry = GeoAPI.Geometries.IGeometry;
using SharpMap.Layers;
using SharpMap.Rendering.Decoration;
using SharpMap.Rendering.Decoration.ScaleBar;
using Point = GeoAPI.Geometries.Coordinate;
using BoundingBox = GeoAPI.Geometries.Envelope;

namespace UnitTests
{
    [TestFixture]
    public class MapTest
    {
        private static readonly IGeometryFactory Factory = new GeometryFactory();
        private static readonly WKTReader WktReader = new WKTReader(Factory);

        public static IGeometry GeomFromText(string wkt)
        {
            return WktReader.Read(wkt);
        }

        private static IProvider CreateDatasource()
        {
            var geoms = new Collection<Geometry>
                            {
                                GeomFromText("POINT EMPTY"),
                                GeomFromText(
                                    "GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))"),
                                GeomFromText(
                                    "MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), ((5 5, 7 5, 7 7, 5 7, 5 5)))"),
                                GeomFromText("LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)"),
                                GeomFromText(
                                    "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))"),
                                GeomFromText(
                                    "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 29 21, 29 29, 21 29, 21 21), (23 23, 23 27, 27 27, 27 23, 23 23))"),
                                GeomFromText("POINT (20.564 346.3493254)"),
                                GeomFromText("MULTIPOINT (20.564 346.3493254, 45 32, 23 54)"),
                                GeomFromText("MULTIPOLYGON EMPTY"),
                                GeomFromText("MULTILINESTRING EMPTY"),
                                GeomFromText("MULTIPOINT EMPTY"),
                                GeomFromText("LINESTRING EMPTY")
                            };
            return new GeometryProvider(geoms);
        }

        [Test]
        public void MapSridEqualsFactorySrid()
        {
            var m = new Map();
            Assert.AreEqual(m.SRID, m.Factory.SRID);
            m.SRID = 10;
            Assert.AreEqual(m.SRID, m.Factory.SRID);
        }

        [Test]
        public void FindLayer_ReturnEnumerable()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));
            map.Layers.Add(new VectorLayer("Layer 4"));

            int count = 0;
            foreach (ILayer lay in map.FindLayer("Layer 3"))
            {
                Assert.AreEqual("Layer 3", lay.LayerName);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void TestClone()
        {
            Map map = new Map(new Size(2, 1));
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));
            map.Layers.Add(new VectorLayer("Layer 4"));

            var clone = map.Clone();

            Assert.AreEqual(map.BackgroundLayer.Count, clone.BackgroundLayer.Count);
            Assert.AreEqual(map.Layers.Count, clone.Layers.Count);
            Assert.AreEqual(map.VariableLayers.Count, clone.VariableLayers.Count);
            Assert.AreEqual(map.Decorations.Count, clone.Decorations.Count);
        }

        [Test]
        public void GetExtents_EmptyMap_ThrowInvalidOperationException()
        {
            Map map = new Map(new Size(2, 1));
            Assert.Throws<InvalidOperationException>( () => map.ZoomToExtents() );
        }

        [Test]
        public void Map_InsertLayer()
        {
            Map m = new Map();
            VectorLayer vlay1 = new VectorLayer("1");
            m.Layers.Add(vlay1);
            VectorLayer vlay2 = new VectorLayer("2");
            m.Layers.Add(vlay2);

            VectorLayer vlay3 = new VectorLayer("3");
            m.Layers.Insert(1, vlay3);

            Assert.AreEqual("1", m.Layers[0].LayerName);
            Assert.AreEqual("3", m.Layers[1].LayerName);
            Assert.AreEqual("2", m.Layers[2].LayerName);
        }

        [Test]
        public void Map_GetLayerByNameInGroupLayer()
        {
            Map m = new Map();
            VectorLayer vlay1 = new VectorLayer("1");
            VectorLayer vlay2 = new VectorLayer("2");
            VectorLayer vlay3 = new VectorLayer("3");
            m.Layers.Add(vlay1);

            LayerGroup lg = new LayerGroup("Group");
            lg.Layers.Add(vlay2);
            lg.Layers.Add(vlay3);
            m.Layers.Add(lg);


            var lay = m.GetLayerByName("1");
            Assert.IsNotNull(lay);
            Assert.AreEqual("1", lay.LayerName);
            lay = m.GetLayerByName("2");
            Assert.IsNotNull(lay);
            Assert.AreEqual("2", lay.LayerName);
            lay = m.GetLayerByName("3");
            Assert.IsNotNull(lay);
            Assert.AreEqual("3", lay.LayerName);
            lay = m.GetLayerByName("Group");
            Assert.IsNotNull(lay);
            Assert.AreEqual("Group", lay.LayerName);

        }

        [Test]
        public void GetExtents_ValidDatasource()
        {
            Map map = new Map(new Size(400, 200));
            VectorLayer vLayer = new VectorLayer("Geom layer", CreateDatasource());
            map.Layers.Add(vLayer);
            BoundingBox box = map.GetExtents();
            Assert.AreEqual(new BoundingBox(0, 50, 0, 346.3493254), box);
        }

        [Test]
        public void GetLayerByName_Indexer()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));

            ILayer layer = map.Layers["Layer 2"];
            Assert.IsNotNull(layer);
            Assert.AreEqual("Layer 2", layer.LayerName);
        }

        [Test]
        public void GetLayerByName_ReturnCorrectLayer()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));

            ILayer layer = map.GetLayerByName("Layer 2");
            Assert.IsNotNull(layer);
            Assert.AreEqual("Layer 2", layer.LayerName);
        }

        [Test]
        public void GetMap_GeometryProvider_ReturnImage()
        {
            Map map = new Map(new Size(400, 200));
            VectorLayer vLayer = new VectorLayer("Geom layer", CreateDatasource());
            vLayer.Style.Outline = new Pen(Color.Red, 2f);
            vLayer.Style.EnableOutline = true;
            vLayer.Style.Line = new Pen(Color.Green, 2f);
            vLayer.Style.Fill = Brushes.Yellow;
            map.Layers.Add(vLayer);

            VectorLayer vLayer2 = new VectorLayer("Geom layer 2", vLayer.DataSource);
            vLayer2.Style.SymbolOffset = new PointF(3, 4);
            vLayer2.Style.SymbolRotation = 45;
            vLayer2.Style.SymbolScale = 0.4f;
            map.Layers.Add(vLayer2);

            VectorLayer vLayer3 = new VectorLayer("Geom layer 3", vLayer.DataSource);
            vLayer3.Style.SymbolOffset = new PointF(3, 4);
            vLayer3.Style.SymbolRotation = 45;
            map.Layers.Add(vLayer3);

            VectorLayer vLayer4 = new VectorLayer("Geom layer 4", vLayer.DataSource);
            vLayer4.Style.SymbolOffset = new PointF(3, 4);
            vLayer4.Style.SymbolScale = 0.4f;
            vLayer4.ClippingEnabled = true;
            map.Layers.Add(vLayer4);

            map.ZoomToExtents();

            Image img = map.GetMap();
            Assert.IsNotNull(img);
            map.Dispose();
            img.Dispose();
        }

        [Test]
        public void GetMap_RenderEmptyMap_ThrowInvalidOperationException()
        {
            Map map = new Map(new Size(2, 1));
            Assert.Throws<InvalidOperationException>(() => map.GetMap() );
        }

        [Test]
        //[ExpectedException(typeof (ApplicationException))]
        public void GetMap_RenderLayerWithoutDatasource_ThrowException()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.GetMap();
        }

        [Test]
        public void GetMapHeight_FixedZoom_Return1750()
        {
            Map map = new Map(new Size(400, 200));
            map.Zoom = 3500;
            Assert.AreEqual(1750, map.MapHeight);
        }

        [Test]
        public void GetPixelSize_FixedZoom_Return8_75()
        {
            Map map = new Map(new Size(400, 200));
            map.Zoom = 3500;
            Assert.AreEqual(8.75, map.PixelSize);
        }

        [Test]
        public void ImageToWorld()
        {
            Map map = new Map(new Size(1000, 500));
            map.Zoom = 360;
            map.Center = new Point(0, 0);
            Assert.AreEqual(new Point(0, 0), map.ImageToWorld(new PointF(500, 250)));
            Assert.AreEqual(new Point(-180, 90), map.ImageToWorld(new PointF(0, 0)));
            Assert.AreEqual(new Point(-180, -90), map.ImageToWorld(new PointF(0, 500)));
            Assert.AreEqual(new Point(180, 90), map.ImageToWorld(new PointF(1000, 0)));
            Assert.AreEqual(new Point(180, -90), map.ImageToWorld(new PointF(1000, 500)));
        }

        [Test]
        public void ImageToWorld_DefaultMap_ReturnValue()
        {
            Map map = new Map(new Size(500, 200));
            map.Center = new Point(23, 34);
            map.Zoom = 1000;
            Point p = map.ImageToWorld(new PointF(242.5f, 92));
            Assert.AreEqual(new Point(8, 50), p);
        }

        [Ignore("Benchmarking MapTransform in Map and MapViewport with(new) and without(old) Coordinate arrays")]
        [TestCase("roads_ugl.shp", 0)] 
        [TestCase("roads_ugl.shp", 45)] 
        [TestCase("SPATIAL_F_SKARVMUFF.shp", 0)] 
        [TestCase("SPATIAL_F_SKARVMUFF.shp", 45)] 
        public void WorldToImageTransform_Benchmark(string shapeFileName, float mapTransformRotation)
        {
            // previous World >> Image transform calculations were for each individual coordinate in an array 
            // new method transforms entire array (eg linestring, polygon ring, multipoint).
            // When there is no map rotation, a simplified calculation is used. When map is rotated, an affine transformation is used
            // (one affine transformation object instantiated per array, previously one affine transformation per coordinate). 

            // Hypothesis: There should be minimal change for point layers, but significant improvements for geometries with ILineString and IMultiPoint

            // New methods typically much faster as shown below from several tests:
            // roads_ugl: 3361 polylines (avg 52 vertices per feature)
            //    Rotn     OBJ   OLD (avg)  NEW (avg)    Improvement
            //    0deg     mAp     160        25          5x faster
            //    0deg     mVp     300        20          15x faster
            //    45deg    mAp    1500        20          75x faster - woo hoo!
            //    45deg    mVp    2000        10          200x faster - woo hoo squared!
            //
            // SKARVMUFF: 4342 Points
            //    Rotn    OBJ   OLD (avg)  NEW (avg)    Improvement
            //    0deg    mAp     3         3            no discernible change
            //    0deg    mVp     6         2            3x faster
            //    45deg   mAp    25        10            2.5x faster
            //    45deg   mVp    45         2            20x faster
            
            var map = new Map(new Size(1024, 1024)) {BackColor = System.Drawing.Color.LightSkyBlue};

            if (!mapTransformRotation.Equals(0f))
            {
                System.Drawing.Drawing2D.Matrix mapTransform = new System.Drawing.Drawing2D.Matrix();
                mapTransform.RotateAt(mapTransformRotation, new PointF(map.Size.Width * 0.5f, map.Size.Height * 0.5f));
                map.MapTransform = mapTransform;
            }

            var fn = TestUtility.GetPathToTestFile(shapeFileName);
            var prov = new SharpMap.Data.Providers.ShapeFile(fn, true);

            var vl = new VectorLayer(shapeFileName, prov);
            map.Layers.Add(vl);
            map.ZoomToExtents();

            var geoms = prov.GetGeometriesInView(map.Envelope);
            var sw = new System.Diagnostics.Stopwatch();

            var oldTimesMap = new System.Collections.Generic.List<long>();
            var newTimesMap = new System.Collections.Generic.List<long>();

            var oldTimesMvp = new System.Collections.Generic.List<long>();
            var newTimesMvp = new System.Collections.Generic.List<long>();

            var numTests = 20;
            
            // MAP tests
            for (var i = 0; i < numTests; i++)
            {
                // old
                sw.Reset();
                sw.Start();
                foreach (var geom in geoms)
                {
                    foreach (var p in geom.Coordinates)
                    {
                        var pt = SharpMap.Utilities.Transform.WorldToMap(p, map);
                        if (!map.MapTransformRotation.Equals(0f))
                        {
                            using (var transform = map.MapTransform)
                            {
                                var pts = new[] {pt};
                                transform.TransformPoints(pts);
                                pt = pts[0];
                            }
                        }
                    }
                }
                sw.Stop();
                oldTimesMap.Add(sw.ElapsedMilliseconds);

                // new
                sw.Reset();
                sw.Start();
                foreach (var geom in geoms)
                {
                    if (geom.Coordinates.Length == 0)
                    {
                        var pt = map.WorldToImage(geom.Coordinates[0], true);
                    }
                    else
                    {
                        var pts = map.WorldToImage(geom.Coordinates, true);
                    }
                }
                sw.Stop();
                newTimesMap.Add(sw.ElapsedMilliseconds);
            }
            
            // drop slowest 2 / fastest 2
            oldTimesMap.Sort();
            newTimesMap.Sort();

            var oldTimesAvgMap = oldTimesMap.Skip(2).Take(16).Average();
            var newTimesAvgMap = newTimesMap.Skip(2).Take(16).Average();
            
            Trace.WriteLine($"WorldToImageTransform_Benchmark {shapeFileName} {mapTransformRotation:000}deg MAP old: {oldTimesAvgMap}  MAP new: {newTimesAvgMap}");
            // allow a little bit of leeway
            Assert.LessOrEqual(newTimesAvgMap / oldTimesAvgMap,1.2,$"{shapeFileName}_{mapTransformRotation}deg_MAP" );

// NOTE: MapViewport Tests no longer relevant  due to redundant method WorldToImageOld being removed
// Section commented out AFTER confirming test results
//            // MapViewport Tests
//            var mvp = (MapViewport) map;
//            for (var i = 0; i < numTests; i++)
//            {
//                // old
//                sw.Reset();
//                sw.Start();
//                foreach (var geom in geoms)
//                {
//                    foreach (var p in geom.Coordinates)
//                    {
//                        var pt = mvp.WorldToImageOld(p, true);
//                        if (!mvp.MapTransformRotation.Equals(0f))
//                        {
//                            using (var transform = mvp.MapTransform)
//                            {
//                                var pts = new[] {pt};
//                                transform.TransformPoints(pts);
//                                pt = pts[0];
//                            }
//                        }
//                    }
//                }
//                sw.Stop();
//                oldTimesMvp.Add(sw.ElapsedMilliseconds);
//                
//                // new
//                sw.Reset();
//                sw.Start();
//                foreach (var geom in geoms)
//                {
//                    if (geom.Coordinates.Length == 0)
//                    {
//                        var pt = mvp.WorldToImage(geom.Coordinates[0], true);
//                    }
//                    else
//                    {
//                        var pts = mvp.WorldToImage(geom.Coordinates, true);
//                    }
//                }
//
//                sw.Stop();
//                newTimesMvp.Add(sw.ElapsedMilliseconds);
//            }
//            
//            oldTimesMvp.Sort();
//            newTimesMvp.Sort();
//            
//            var oldTimesAvgMvp = oldTimesMvp.Skip(2).Take(16).Average();
//            var newTimesAvgMvp = newTimesMvp.Skip(2).Take(16).Average();
//
//            
//            Trace.WriteLine($"WorldToImageTransform_Benchmark {shapeFileName} {mapTransformRotation:000}deg  MVP old: {oldTimesAvgMvp}  MVP new: {newTimesAvgMvp}");
//            // allow a little bit of leeway
//            Assert.LessOrEqual(newTimesAvgMvp/ oldTimesAvgMvp,1.2, $"{shapeFileName}_{mapTransformRotation}deg_MVP" );

            map.Dispose();
        }

        [TestCase(0), Category("RequiresWindows")]
        [TestCase(30), Category("RequiresWindows")]
        [TestCase(60), Category("RequiresWindows")]
        [TestCase(90), Category("RequiresWindows")]
        [TestCase(120), Category("RequiresWindows")]
        [TestCase(150), Category("RequiresWindows")]
        [TestCase(180), Category("RequiresWindows")]
        [TestCase(210), Category("RequiresWindows")]
        [TestCase(240), Category("RequiresWindows")]
        [TestCase(270), Category("RequiresWindows")]
        [TestCase(300), Category("RequiresWindows")]
        [TestCase(330), Category("RequiresWindows")]
        public void ImageToWorld_AndBack_Map_WithRotation(float rotationDeg)
        {
            using (var map = ConfigureTransformMap(rotationDeg))
            {
                var imagePts = GetImageCoordinates(map);
                var worldPts = GetWorldCoordinates(map, imagePts);
                // Test map transform, comparing Image>>World calcs with independent
                // Affine Transformation of image/world geometry and map properties
                ValidateTransformScenarios(false, map, imagePts, worldPts.Coordinates);
            }
        }

        [TestCase(0), Category("RequiresWindows")]
        [TestCase(30), Category("RequiresWindows")]
        [TestCase(60), Category("RequiresWindows")]
        [TestCase(90), Category("RequiresWindows")]
        [TestCase(120), Category("RequiresWindows")]
        [TestCase(150), Category("RequiresWindows")]
        [TestCase(180), Category("RequiresWindows")]
        [TestCase(210), Category("RequiresWindows")]
        [TestCase(240), Category("RequiresWindows")]
        [TestCase(270), Category("RequiresWindows")]
        [TestCase(300), Category("RequiresWindows")]
        [TestCase(330), Category("RequiresWindows")]
        public void ImageToWorld_AndBack_MapViewport_WithRotation(float rotationDeg)
        {
            // Similar to ImageToWorld_AndBack_Map_WithRotation but testing MapViewport and generating test images
            var map = ConfigureTransformMap(rotationDeg);
            map.Decorations.Add(new ScaleBar());
            map.Decorations.Add(new NorthArrow(){ForeColor = Color.Red});
            map.Decorations.Add(new EyeOfSight(){Anchor = MapDecorationAnchor.RightTop, ForeColor = Color.DarkBlue});
            var imagePts = GetImageCoordinates(map);
            var worldPts = GetWorldCoordinates(map, imagePts);
            ValidateTransformScenarios(true, map, imagePts, worldPts.Coordinates);
  
            // visual checks
            var vl = new VectorLayer("Test Viewport Outline");
            var gp = new GeometryProvider(worldPts);
            gp.Geometries.Add(new NetTopologySuite.Geometries.Point(map.Center));
            vl.DataSource = gp;

            map.Layers.Add(vl);

            // Polygon should always appear aligned with borders, with red dot should always be in lower left corner.
            // note buffer giving small margin around borders to be sure polygon isn't grossly larger than mapviewport. 
            var polygon = GetMapExtentPolygon(map.Center, map.Zoom,map.MapHeight,map.MapTransformRotation).Buffer(-50);
            vl = new VectorLayer("Test Viewport Inset");
            gp = new GeometryProvider(polygon);
            gp.Geometries.Add(new NetTopologySuite.Geometries.Point(map.Center));
            gp.Geometries.Add(new NetTopologySuite.Geometries.Point(polygon.Coordinates[0]));
            vl.DataSource = gp;
            map.Layers.Add(vl);
            
            string fn = $"MapRotation_{rotationDeg:000}.png";
            using (var img = map.GetMap(96))
                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), fn),System.Drawing.Imaging.ImageFormat.Png);

            map.Dispose();
        }
        
        private Map ConfigureTransformMap(float rotationDeg)
        {
            var map = new Map(new Size(600, 300)) {BackColor = System.Drawing.Color.LightSkyBlue};
            map.Zoom = 1000;
            map.Center = new Point(25000, 75000);
            var mapScale = map.MapScale;

            System.Drawing.Drawing2D.Matrix mapTransform = new System.Drawing.Drawing2D.Matrix();
            mapTransform.RotateAt(rotationDeg, new PointF(map.Size.Width * 0.5f, map.Size.Height * 0.5f));
            map.MapTransform = mapTransform;

            return map;
        }

        private PointF[] GetImageCoordinates(Map map)
        {
            return new PointF[]
            {
                new PointF((float) (map.Size.Width * 0.5),(float) (map.Size.Height * 0.5)), // centre
                new PointF(0, 0), // UL
                new PointF(map.Size.Width, 0), // UR
                new PointF(map.Size.Width, map.Size.Height), // LR
                new PointF(0, map.Size.Height) // LL
            };   
        }

        private LineString GetWorldCoordinates(Map map, PointF[] imagePts)
        {
            var affineTrans = GetIndependentTransform(map);
            // LineString equivalent of imagePts
            var geom = new LineString((Coordinate[])Array.ConvertAll(imagePts , p => new Coordinate(p.X, p.Y)));
            // independent transform to World coordinates            
//            NetTopologySuite.CoordinateSystems.Transformations.GeometryTransform.TransformLineString(
//                new GeometryFactory(new PrecisionModel()), geom, affineTrans);
            geom = (LineString)affineTrans.Transform(geom);
            geom.GeometryChangedAction();
            return geom;
        }
        
        //private ProjNet.CoordinateSystems.Transformations.AffineTransform GetIndependentTransform(Map map)
        private AffineTransformation GetIndependentTransform(Map map)
        {
            double scaleX = map.Zoom / map.Size.Width;
            double scaleY = map.MapHeight / map.Size.Height;
            
            // Affine Transformation: 
            // 1: Translate to mapViewPort centre
            // 2: Reflect in X-Axis
            // 3: Rotation about mapViewPort centre
            // 4: Scale to map units
            // 5: Translate to map centre
            
            //CLOCKWISE ProjNet affine transform (negate degrees)
            //double rad = -1 * deg * Math.PI / 180.0;
            //GeoAPI.CoordinateSystems.Transformations.IMathTransform trans =
            //    new ProjNet.CoordinateSystems.Transformations.AffineTransform(
            //        scaleX * Math.Cos(rad),
            //        -scaleX * Math.Sin(rad),
            //        -scaleX * Math.Cos(rad) * map.Size.Width / 2f + scaleX * Math.Sin(rad) * map.Size.Height / 2f + map.Center.X,
            //        -scaleY * Math.Sin(rad),
            //        -scaleY * Math.Cos(rad),
            //        scaleY * Math.Sin(rad) * map.Size.Width / 2f + scaleY * Math.Cos(rad) * map.Size.Height / 2f + map.Center.Y);

            //ANTICLCOCKWISE ProjNet affine transform 
            double rad = map.MapTransformRotation * Math.PI / 180.0;
//            var trans =
//                new ProjNet.CoordinateSystems.Transformations.AffineTransform(
//                    scaleX * Math.Cos(rad),
//                    scaleX * Math.Sin(rad),
//                    -scaleX * Math.Cos(rad) * map.Size.Width * 0.5 - scaleX * Math.Sin(rad) * map.Size.Height * 0.5 + map.Center.X,
//                    scaleY * Math.Sin(rad),
//                    -scaleY * Math.Cos(rad),
//                    -scaleY * Math.Sin(rad) * map.Size.Width * 0.5 + scaleY * Math.Cos(rad) * map.Size.Height * 0.5 + map.Center.Y);
//
//            return trans;

            var trans = new AffineTransformation();
            trans.Compose(AffineTransformation.TranslationInstance(-map.Size.Width * 0.5, -map.Size.Height * 0.5));
            trans.Compose(AffineTransformation.ScaleInstance(1, -1));
            trans.Compose(AffineTransformation.RotationInstance(rad));
            trans.Compose(AffineTransformation.ScaleInstance(scaleX, scaleY));
            trans.Compose(AffineTransformation.TranslationInstance(map.Center.X, map.Center.Y));
            return trans;

            // .Net Matrix
            //System.Drawing.Drawing2D.Matrix matrix;
            //matrix = new System.Drawing.Drawing2D.Matrix();
            //matrix.Translate(-map.Size.Width / 2f, -map.Size.Height / 2f);      // shift origin to viewport centre
            //matrix.Scale(1, -1, System.Drawing.Drawing2D.MatrixOrder.Append);   // reflect in X axis
            //matrix.Rotate(deg, System.Drawing.Drawing2D.MatrixOrder.Append);    // rotate about viewport centre
            //matrix.Scale((float)scaleX, (float)scaleY, System.Drawing.Drawing2D.MatrixOrder.Append); // scale
            //matrix.Translate((float)map.Center.X, (float)map.Center.Y, System.Drawing.Drawing2D.MatrixOrder.Append); // translate to map centre
        }

        private Polygon GetMapExtentPolygon(Coordinate mapCenter, double zoom, double mapHeight, float rotationDeg)
        {
            // height has been adjusted for pixelRatio
//            var height = map.MapHeight;
//            if (double.IsNaN(height) || double.IsInfinity(height) || map.Size.Width == 0 || map.Size.Height == 0)
//                return null;

            var poly = new Polygon(new LinearRing(new Coordinate[]
                {
                    new Coordinate(mapCenter.X - zoom * .5,mapCenter.Y - mapHeight * .5),
                    new Coordinate(mapCenter.X - zoom * .5,mapCenter.Y + mapHeight * .5),
                    new Coordinate(mapCenter.X + zoom * .5,mapCenter.Y + mapHeight * .5),
                    new Coordinate(mapCenter.X + zoom * .5,mapCenter.Y - mapHeight * .5),
                    new Coordinate(mapCenter.X -zoom * .5,mapCenter.Y - mapHeight * .5)
                }
            ));

            if (rotationDeg.Equals(0f))
                return poly;

            var rad = rotationDeg * Math.PI / 180.0;
            var at = AffineTransformation.RotationInstance(rad, mapCenter.X, mapCenter.Y);
            return (Polygon)at.Transform(poly);
        }

        private void ValidateTransformScenarios( bool useMapViewport, Map map, PointF[] ptsImage, Coordinate[] controlGeom)
        {
            Coordinate[] ptsWorld;
            Envelope worldEnv;
            Polygon worldPolygon;
            PointF[] andBack;
            string mode;

            var controlEnv = new Envelope(
                controlGeom.Min(c => c.X), 
                controlGeom.Max(c => c.X),
                controlGeom.Min(c => c.Y),
                controlGeom.Max(c => c.Y));

            var mvp = (MapViewport) map;
            
            if (!useMapViewport)
            {
                mode = "map";
                ptsWorld = map.ImageToWorld((PointF[]) ptsImage.Clone(), true);
                worldEnv = map.Envelope;
                worldPolygon = GetMapExtentPolygon(map.Center, map.Zoom,map.MapHeight,map.MapTransformRotation);
                andBack = map.WorldToImage(ptsWorld, true);
            }
            else
            {
                mode = "mvp";
                ptsWorld= mvp.ImageToWorld((PointF[]) ptsImage.Clone(), true);
                worldEnv = mvp.Envelope;
                worldPolygon = GetMapExtentPolygon(mvp.Center, mvp.Zoom,mvp.MapHeight,mvp.MapTransformRotation);;
                andBack = mvp.WorldToImage(ptsWorld, true);
            }

            // validate ImageToWorld calcs by comparison with control geom (independent affine transformation)
            Assert.IsTrue(controlGeom[0].Equals2D(ptsWorld[0], 0.001), $"{mode}Image2World Centre");
            Assert.IsTrue(controlGeom[1].Equals2D(ptsWorld[1], 0.001), $"{mode}Image2World TopLeft");
            Assert.IsTrue(controlGeom[2].Equals2D(ptsWorld[2], 0.001), $"{mode}Image2World TopRight");
            Assert.IsTrue(controlGeom[3].Equals2D(ptsWorld[3], 0.001), $"{mode}Image2World BottomRight");
            Assert.IsTrue(controlGeom[4].Equals2D(ptsWorld[4], 0.001), $"{mode}Image2World BottomLeft");

            // validate map envelope: lineString outline = image extents, so lineString.EnvelopeInternal should equal map.Envelope
            // this test found and resolved long-standing problem in Map.Envelope calcs when MapTransform is applied
            Assert.IsTrue(worldEnv.BottomLeft().Equals2D(controlEnv.BottomLeft(), 0.1), $"{mode}Envelope BottomLeft");
            Assert.IsTrue(worldEnv.TopLeft().Equals2D(controlEnv.TopLeft(), 0.1), $"{mode}Envelope TopLeft");
            Assert.IsTrue(worldEnv.TopRight().Equals2D(controlEnv.TopRight(), 0.1), $"{mode}Envelope TopRight");
            Assert.IsTrue(worldEnv.BottomRight().Equals2D(controlEnv.BottomRight(), 0.1), $"{mode}Envelope BottomRight");

            // validate map polygon
            Assert.IsTrue(worldPolygon.EnvelopeInternal.BottomLeft().Equals2D(controlEnv.BottomLeft(), 0.1), $"{mode}Polygon BottomLeft");
            Assert.IsTrue(worldPolygon.EnvelopeInternal.TopLeft().Equals2D(controlEnv.TopLeft(), 0.1), $"{mode}Polygon BottomLeft");
            Assert.IsTrue(worldPolygon.EnvelopeInternal.TopRight().Equals2D(controlEnv.TopRight(), 0.1), $"{mode}Polygon BottomLeft");
            Assert.IsTrue(worldPolygon.EnvelopeInternal.BottomRight().Equals2D(controlEnv.BottomRight(), 0.1), $"{mode}Polygon BottomLeft");
            
            // validate zoom
            map.Zoom = 1000;
            Assert.AreEqual(map.Zoom, mvp.Zoom, 0.001, $"{mode}MapZoom");
            
            Assert.AreEqual(map.Zoom, worldPolygon.Coordinates[1].Distance(worldPolygon.Coordinates[2]), 0.1, $"{mode}PolygonWidth");

            // validate MapScale
            Assert.AreEqual(map.MapScale, mvp.GetMapScale(96), 0.1, $"{mode}MapScale");
            
            // now convert WORLD >> IMAGE
            //var andBack = map.WorldToImage(ptsWorld, true);

            Assert.AreEqual(ptsImage[0].X,andBack[0].X, 0.02, $"{mode}World2Image Centre X");
            Assert.AreEqual(ptsImage[0].Y,andBack[0].Y, 0.02, $"{mode}World2Image Centre Y");
            Assert.AreEqual(ptsImage[1].X,andBack[1].X, 0.02, $"{mode}World2Image TopLeft X");
            Assert.AreEqual(ptsImage[1].Y,andBack[1].Y, 0.02, $"{mode}World2Image TopLeft Y");
            Assert.AreEqual(ptsImage[2].X,andBack[2].X, 0.02, $"{mode}World2Image TopRight X");
            Assert.AreEqual(ptsImage[2].Y,andBack[2].Y, 0.02, $"{mode}World2Image TopRight Y");
            Assert.AreEqual(ptsImage[3].X,andBack[3].X, 0.02, $"{mode}World2Image BottomRight X");
            Assert.AreEqual(ptsImage[3].Y,andBack[3].Y, 0.02, $"{mode}World2Image BottomRight Y");
            Assert.AreEqual(ptsImage[4].X,andBack[4].X, 0.02, $"{mode}World2Image BottomLeft X");
            Assert.AreEqual(ptsImage[4].Y,andBack[4].Y, 0.02, $"{mode}World2Image BottomLeft Y");

        }
        
        [Test]
        public void Initalize_MapInstance()
        {
            Map map = new Map(new Size(2, 1));
            Assert.IsNotNull(map);
            Assert.IsNotNull(map.Layers);
            Assert.AreEqual(2f, map.Size.Width);
            Assert.AreEqual(1f, map.Size.Height);
            Assert.AreEqual(Color.Transparent, map.BackColor);
            Assert.AreEqual(double.MaxValue, map.MaximumZoom);
            Assert.IsTrue(map.MinimumZoom > 0);
            Assert.AreEqual(new Point(0, 0), map.Center, "map.Center should be initialized to (0,0)");
            Assert.AreEqual(1, map.Zoom, "Map zoom should be initialized to 1.0");
        }

        [Test]
        public void SetMaximumZoom_ValueLessThanMinimumZoom()
        {
            Map map = new Map();
            map.MaximumZoom = -1;
            Assert.IsTrue(map.MaximumZoom >= map.MinimumZoom);
        }

        [Test]
        public void SetMaximumZoom_OKValue()
        {
            Map map = new Map();
            map.MaximumZoom = 100.3;
            Assert.AreEqual(100.3, map.MaximumZoom);
        }

        [Test]
        public void SetMinimumZoom_ValueLessThanTwoEpsilon()
        {
            Map map = new Map();
            map.MinimumZoom = -1;
            Assert.IsTrue(map.MinimumZoom > 0);
            map.MinimumZoom = Double.Epsilon;
            Assert.IsTrue(map.MinimumZoom > Double.Epsilon);
        }

        [Test]
        public void SetMinimumZoom_OKValue()
        {
            Map map = new Map();
            map.MinimumZoom = 100.3;
            Assert.AreEqual(100.3, map.MinimumZoom);
        }

        [Test]
        public void SetZoom_ValueBelowMin()
        {
            Map map = new Map();
            map.MinimumZoom = 100;
            map.Zoom = 50;
            Assert.AreEqual(100, map.MinimumZoom);
        }

        [Test]
        public void SetZoom_ValueOutsideMax()
        {
            Map map = new Map();
            map.MaximumZoom = 100;
            map.Zoom = 150;
            Assert.AreEqual(100, map.MaximumZoom);
        }

        [Test]
        public void ZoomToBoxWithEnforcedMaximumExtents()
        {
            Map map = new Map();
            //map.MaximumZoom = 100;
            map.MaximumExtents = new Envelope(-180, 180, -90, 90);
            map.EnforceMaximumExtents = true;
            map.ZoomToBox(new Envelope(-200, 200, -100, 100));
            Assert.IsTrue(map.MaximumExtents.Contains(map.Envelope));
            Assert.AreEqual(new BoundingBox(-120, 120, -90, 90), map.Envelope);
        }

        [Test]
        public void ZoomWithMapViewportLock()
        {
            Map map = new Map(new Size(100, 50));
            //map.MaximumZoom = 100;
            map.ZoomToBox(new Envelope(-200, 200, -100, 100));
            var vpl = new MapViewportLock(map);
            vpl.Lock();
            Assert.IsTrue(vpl.IsLocked);

            double zoom = map.Zoom;
            map.Zoom *= 1.1;
            Assert.That(map.Zoom, Is.EqualTo(zoom));

            map.Center = new Coordinate(10, 10);
            Assert.That(map.Center, Is.EqualTo(new Coordinate(0, 0)));
        }


        [Test]
        public void WorldToImage()
        {
            Map map = new Map(new Size(1000, 500));
            map.Zoom = 360;
            map.Center = new Point(0, 0);
            Assert.AreEqual(new PointF(500, 250), map.WorldToImage(new Point(0, 0)));
            Assert.AreEqual(new PointF(0, 0), map.WorldToImage(new Point(-180, 90)));
            Assert.AreEqual(new PointF(0, 500), map.WorldToImage(new Point(-180, -90)));
            Assert.AreEqual(new PointF(1000, 0), map.WorldToImage(new Point(180, 90)));
            Assert.AreEqual(new PointF(1000, 500), map.WorldToImage(new Point(180, -90)));
        }

        [Test]
        public void WorldToMap_DefaultMap_ReturnValue()
        {
            Map map = new Map(new Size(500, 200));
            map.Center = new Point(23, 34);
            map.Zoom = 1000;
            PointF p = map.WorldToImage(new Point(8, 50));
            Assert.AreEqual(new PointF(242.5f, 92), p);
        }

        [Test]
        public void ZoomToBox_NoAspectCorrection()
        {
            Map map = new Map(new Size(400, 200));
            map.ZoomToBox(new BoundingBox(20, 100, 50, 80));
            Assert.AreEqual(new Point(60, 65), map.Center);
            Assert.AreEqual(80, map.Zoom);
        }

        [Test]
        public void ZoomToBox_WithAspectCorrection()
        {
            Map map = new Map(new Size(400, 200));
            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.AreEqual(new Point(60, 95), map.Center);
            Assert.AreEqual(340, map.Zoom);
        }

        [TestCase(600, 300, 10000,10000)]
        [TestCase(600, 300, 5000,15000)]
        [TestCase(600, 300, 15000,5000)]
        [TestCase(300, 600, 10000,10000)]
        [TestCase(300, 600, 5000,15000)]
        [TestCase(300, 600, 15000,5000)]
        public void ZoomToBox_WithRotatedViewport(int mapSizeWidth, int mapSizeHeight, double dataWidthMetres, double dataHeightMetres)
        {
            // Tests to ensure ZoomToExtents shows map extents at maximum possible scale without clipping
            // Each test will work through series of MapTransform from 0-360 deg at 30deg increments/
            // The old/new image outputs demonstrate how the updates have fixed problems when viewport is rotated.
            var map = new Map(new Size(mapSizeWidth, mapSizeHeight));
            map.BackColor= Color.Azure;

            // create layer with single polygon centred on 700,000mE, 1,000,000mN
            var env = new Envelope(0, dataWidthMetres, 0, dataHeightMetres);
            env.Translate(700000 - dataWidthMetres * 0.5, 1000000 - dataHeightMetres * 0.5);
            var extentsPoly = new Polygon(new LinearRing(new Coordinate[]
            {
                new Coordinate(env.MinX, env.MinY),
                new Coordinate(env.MinX, env.MaxY),
                new Coordinate(env.MaxX, env.MaxY),
                new Coordinate(env.MaxX, env.MinY),
                new Coordinate(env.MinX, env.MinY)
            }));
            
            var vl = new VectorLayer("Test Points");
            var gp = new GeometryProvider(extentsPoly);
            gp.Geometries.Add(new NetTopologySuite.Geometries.Point(env.Centre));
            // red dot for lower left corner
            var lowerLeft = new Coordinate(env.BottomLeft());
            lowerLeft.X += 100;
            lowerLeft.Y += 100;
            gp.Geometries.Add(new NetTopologySuite.Geometries.Point(lowerLeft));
            vl.DataSource = gp;
            map.Layers.Add(vl);

            for (var degrees = 0; degrees < 360; degrees += 30)
            {
                var mapTransform = new System.Drawing.Drawing2D.Matrix();
                mapTransform.RotateAt(degrees, new PointF(map.Size.Width / 2, map.Size.Height / 2));
                map.MapTransform = mapTransform;

                var ext = map.GetExtents();
                Assert.IsTrue(ext.Equals(env));

                // reset view
                map.Center = new Coordinate(0, 0);
                map.Zoom = 1000;
                
                // OLD: zoom to box, ignoring map rotation: layer extents will overlap borders or have significant margins  
                map.ZoomToBox(ext);
                var fn = $"ZoomToBox_{mapSizeWidth}x{mapSizeHeight}_bbox_{env.Width}x{env.Height}_OLD_{degrees:000}deg.png";
                using (var img = map.GetMap(96))
                    img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), fn), System.Drawing.Imaging.ImageFormat.Bmp);

                // reset view
                map.Center = new Coordinate(0, 0);
                map.Zoom = 1000;

                // NEW: zoom to box, taking into account map rotation: map extents will fit perfectly between borders
                map.ZoomToBox(env, true);
                var polygon = GetMapExtentPolygon(map.Center, map.Zoom, map.MapHeight, map.MapTransformRotation);
                // allow small margin by buffering true outline of MapViewport in world coordinates
                Assert.IsTrue(polygon.Buffer(1).Contains(extentsPoly), $"{degrees:000}_contains");
                Assert.IsTrue(polygon.Buffer(-1).Intersects(extentsPoly), $"{degrees:000}_intersects");
                
                fn = $"ZoomToBox_{mapSizeWidth}x{mapSizeHeight}_bbox_{env.Width}x{env.Height}_NEW_{degrees:000}deg.png";
                using (var img = map.GetMap(96))
                    img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), fn), System.Drawing.Imaging.ImageFormat.Bmp);
            }

            map.Dispose();
        }

        [Test]
        public void TestZoomToBoxRaisesMapViewOnChange()
        {
            var raised = false;
            var map = new Map(new Size(400, 200));
            map.MapViewOnChange += () => raised = true;

            // ZoomToBox
            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.IsTrue(raised, "MapViewOnChange not fired when calling Map.ZoomToBox(...).");
            raised = false;
            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.IsFalse(raised, "MapViewOnChange fired when calling Map.ZoomToBox(map.Envelope).");

            // ZoomToExtents
            // Note: Not needed as Map.ZoomToExtents() calls Map.ZoomToBox(Map.GetExtents())
            //raised = false;
            //map.ZoomToExtents();
            //Assert.IsTrue(raised, "MapViewOnChange not fired when calling Map.ZoomToExtents()");
            //raised = false;
            //map.ZoomToExtents();
            //Assert.IsFalse(raised, "MapViewOnChange fired when calling Map.ZoomToExtents() twice");

            // Set Zoom
            map.Zoom = map.Zoom * 0.9;
            Assert.IsTrue(raised, "MapViewOnChange not fired when setting Map.Zoom.");
            raised = false;
            map.Zoom = map.Zoom;
            Assert.IsFalse(raised, "MapViewOnChange not fired when setting Map.Zoom = Map.Zoom");

            // Set Center
            map.Center = new Coordinate(map.Center.X + 1, map.Center.Y);
            Assert.IsTrue(raised, "MapViewOnChange not fired when setting Map.Center.");
            raised = false;
            map.Center = map.Center;
            Assert.IsFalse(raised, "MapViewOnChange fired when setting Map.Center = Map.Center.");

        }


        [TestCase(LayerCollectionType.Background, Description = "The map fires MapNewTileAvailable event when an ITileAsyncLayer added to background collection, fires the MapNewTileAvailable event")]
        [TestCase(LayerCollectionType.Static, Description = "The map fires MapNewTileAvailable event when an ITileAsyncLayer added to static collection, fires the MapNewTileAvailable event")]
        public async Task AddingTileAsyncLayers_HookItsMapNewTileAvaliableEvent(LayerCollectionType collectionType)
        {
            var map = new Map();

            var layer = CreateTileAsyncLayer();

            AddTileLayerToMap(layer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            RaiseMapNewtileAvailableOn(layer);
            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should not fire MapNewTileAvailable event for removed ITileAsyncLayers, case: layer from StaticLayers")]
        [TestCase(LayerCollectionType.Background, Description = "The map should not fire MapNewTileAvailable event for removed ITileAsyncLayers, case: layer from BackgroundLayers")]
        public async Task AfterRemovingTileAsyncLayer_MapDoesNotHookAnymoreItsMapNewTileAvailableEvent(LayerCollectionType collectionType)
        {
            var map = new Map();

            var tileAsyncLayer = CreateTileAsyncLayer();

            AddTileLayerToMap(tileAsyncLayer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            map.GetCollection(collectionType).RemoveAt(0);

            RaiseMapNewtileAvailableOn(tileAsyncLayer);

            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should not fire MapNewTileAvailable event for replaced TileAsyncLayers from Layer collection")]
        [TestCase(LayerCollectionType.Background, Description = "The map should not fire MapNewTileAvailable event for replaced TileAsyncLayers from BackgroundLayer collection")]
        public async Task MapDoesNotGenerateMapNewTile_ReplacedLayers(LayerCollectionType collectionType)
        {
            var map = new Map();

            var layer = CreateTileAsyncLayer();

            AddTileLayerToMap(layer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            var newLayer = CreateTileAsyncLayer();
            map.GetCollection(collectionType)[0] = newLayer.Item1.Object;

            RaiseMapNewtileAvailableOn(layer);

            bool res = await Task.Run(() => eventSource.IsEmpty()).Result;
            Assert.That(res, Is.EqualTo(true), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should fire MapNewTileAvailable event for new added by replace TileAsyncLayers, case: Layer")]
        [TestCase(LayerCollectionType.Background, Description = "The map should fire MapNewTileAvailable event for new added by replace TileAsyncLayers, case: BackgroundLayer")]
        public async Task MapGeneratesMapNewTile_NewReplacedLayers(LayerCollectionType collectionType)
        {
            var map = new Map();

            var tileAsyncLayer = CreateTileAsyncLayer();

            AddTileLayerToMap(tileAsyncLayer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            var newLayer = CreateTileAsyncLayer();
            map.GetCollection(collectionType)[0] = newLayer.Item1.Object;

            RaiseMapNewtileAvailableOn(newLayer);

            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should not fire MapNewTileAvailable event after the Layers are cleared from Layers collection")]
        [TestCase(LayerCollectionType.Background, Description = "The map should not fire MapNewTileAvailable event after the Layers are cleared from Background collection")]
        public async Task MapDoesNoGenerateMapNewTile_AfterClear(LayerCollectionType collectionType)
        {
            var map = new Map();

            var tileAsyncLayer = CreateTileAsyncLayer();

            AddTileLayerToMap(tileAsyncLayer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            map.GetCollection(collectionType).Clear();

            RaiseMapNewtileAvailableOn(tileAsyncLayer);

            bool res = await Task.Run(() => eventSource.IsEmpty()).Result;
            Assert.That(res, TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside of a LayerGroup, case Layer")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside of a LayerGroup, case BackgroundLayer")]
        public async Task Map_TileAsyncInsideGroup_FiresMapNewtileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            RaiseMapNewtileAvailableOn(tileLayer);

            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should NOT fire MapNewtileAvailable event for TileAsyncLayers removed from a group belonging to Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should NOT fire MapNewtileAvailable event for TileAsyncLayers removed from a group belonging to BackgroundLayers")]
        public async Task Map_TileAsyncRemovedFromGroup_DoesNotFiredMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            RemoveTileLayerFromGroup(group, tileLayer);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            RaiseMapNewtileAvailableOn(tileLayer);
            bool res = await Task.Run(() => eventSource.IsEmpty()).Result;
            Assert.That(res, TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewTileAvailable event for new TileAsyncLayer replaced from a group and not for the old layer, case: Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewTileAvailable event for new TileAsyncLayer replaced from a group and not for the old layer, case: BackgroundLayers")]
        public async Task Map_TileAsyncReplacedFromGroup_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            var newTileLayer = CreateTileAsyncLayer();
            ReplaceExistingAsyncLayerFromGroup(group, tileLayer, newTileLayer);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileLayer);

            bool res = await Task.Run(() => eventSource.IsEmpty()).Result;
            Assert.That(res, "Map should NOT fire MapNewTileAvailable event for TileAsyncLayers replaced from a group");

            RaiseMapNewtileAvailableOn(newTileLayer);

            int resI4 = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(resI4, Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayer belonging to a group that has been cleared, case: Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayer belonging to a group that has been cleared, case: BackgroundLayers")]
        public async Task Map_TileAsyncFromClearedGroup_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            group.Layers.Clear();

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileLayer);

            bool res = await Task.Run(() => eventSource.IsEmpty()).Result;
            Assert.That(res, TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers belonging to an added group, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers belonging to an added group, case BackgroundLayers")]
        public async Task Map_TileAsyncInsideAddedGroup_FiresMapNewTileAvail(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            map.GetCollection(collectionType).Add(group);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileLayer);

            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside nested group, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside nested group, case BackgroundLayers")]
        public async Task Map_TilAsyncInsideNephew_FiresMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var subGroup = CreateLayerGroup("subgroup");

            var tileAsyncLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsyncLayer, subGroup);

            group.Layers.Add(subGroup);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsyncLayer);
            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should not fire MapNewtileAvailable event for TileAsyncLayers removed from a nested group, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewtileAvailable event for TileAsyncLayers removed from a nested group, case BackgroundLayers")]
        public async Task Map_TileAsyncRemovedFromNephew_DoesNotFireMapNewtileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var subGroup = CreateLayerGroup("subgroup");

            var tileAsyncLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsyncLayer, subGroup);

            group.Layers.Add(subGroup);

            // test
            RemoveTileLayerFromGroup(subGroup, tileAsyncLayer);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsyncLayer);

            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayers belonging to a group collection replaced, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayers belonging to a group collection replaced, case BackgroundLayers")]
        public async Task Map_TileAsyncInsideReplacedCollection_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileAsync = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsync, group);

            group.Layers = new ObservableCollection<ILayer>();

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsync);
            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers added to a new replaced collection, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers added to a new replaced collection, case BackgroundLayers")]
        public async Task Map_TileAsyncAddedToReplacedCollection_FiresMapNewtileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            group.Layers = new ObservableCollection<ILayer>();

            var tileAsync = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsync, group);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsync);

            int res = await Task.Run(() => eventSource.Count()).Result;
            Assert.That(res, Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayers added to detached collections, case BackgroundLayers")]
        public async Task Map_TileAsyncAddedToDetachedCollection_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var detachedCollection = group.Layers;

            group.Layers = new ObservableCollection<ILayer>();

            var tileAsync = CreateTileAsyncLayer();
            detachedCollection.Add(tileAsync.Item1.Object);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsync);
            int count = await eventSource.Count();

            Assert.That(count, Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [Test(Description = "Removing a non empty group from layers empties the collection")]
        public void MapLayers_AfterRemovingNotEmptyGroup_IsEmpty()
        {
            var map = new Map();

            var group = CreateLayerGroup();
            group.Layers.Add(new LabelLayer("labels"));

            map.Layers.Add(group);

            map.Layers.Remove(group);

            Assert.That(map.Layers, Is.Empty);
        }

        [Test(Description = "The cloning should clone also the LayerGroups")]
        public void Clone_ShoulCloneTheGroups()
        {
            // LayerGroups must be cloned because we want that each instance of the map listens to only the notification events of its children.

            var map = new Map();

            var group = CreateLayerGroup();

            map.Layers.Add(group);

            var clonedMap = map.Clone();

            Assert.That(clonedMap.Layers[0], Is.Not.EqualTo(group), TestContext.CurrentContext.Test.GetDescription());
        }

        private Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> CreateTileAsyncLayer()
        {
            var tileAsync = new Mock<ITileAsyncLayer>();
            tileAsync.SetupAllProperties();
            var layer = tileAsync.As<ILayer>();
            layer.SetupAllProperties();

            return new Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>>(layer, tileAsync);
        }
        private void ReplaceExistingAsyncLayerFromGroup(LayerGroup group, Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> oldTileLayer,
            Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> newTileLayer)
        {
            var oldLayer = oldTileLayer.Item1.Object;
            var idx = group.Layers.IndexOf(oldLayer);
            group.Layers[idx] = newTileLayer.Item1.Object;
        }
        private void RemoveTileLayerFromGroup(LayerGroup group, Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileLayer)
        {
            group.Layers.Remove(tileLayer.Item1.Object);
        }
        private void AddTileLayerToLayerGroup(Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileLayer, LayerGroup group)
        {
            group.Layers.Add(tileLayer.Item1.Object);
        }
        private void AddTileLayerToMap(Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileLayer, Map map, LayerCollectionType collectionType = LayerCollectionType.Background)
        {
            var layer = tileLayer.Item1.Object;

            map.GetCollection(collectionType).Add(layer);
        }
        private LayerGroup CreateLayerGroup(string layerName = "group")
        {
            return new LayerGroup(layerName);
        }
        private void RaiseMapNewtileAvailableOn(Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileAsync)
        {
            tileAsync.Item2.Raise(tal => tal.MapNewTileAvaliable += null, (TileLayer)null, (Envelope)null, (Bitmap)null, 0,
                0, (ImageAttributes)null);
        }
    }
}
