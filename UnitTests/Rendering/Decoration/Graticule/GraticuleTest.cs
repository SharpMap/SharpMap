using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Decoration;
using SharpMap.Rendering.Decoration.Graticule;
using SharpMap.Utilities;
using Point = NetTopologySuite.Geometries.Point;

namespace UnitTests.Rendering.Decoration.Graticule
{
    [TestFixture, Category("RequiresWindows")]
    public class GraticuleTest
    {
        private const double SecondOfArcDecimalPlaces = 1e-4; // approx. 1mm at the equator
        private readonly Envelope _gcsDomain = new Envelope(-180, 180, -90, 90);
        private readonly Envelope _gcsWebMercDomain = new Envelope(-180, 180, -85, 85);

        [TestCase(2500,0)]
        [TestCase(2500,60)]
        [TestCase(2500,120)]
        [TestCase(2500,180)]
        [TestCase(2500,240)]
        [TestCase(1000,0)]
        [TestCase(500,0)]
        [TestCase(100,0)]
        [TestCase(50,0)]
        [TestCase(20,0)]
        [TestCase(10,0)]
        [TestCase(5,0)]
        [TestCase(2,0)]
        [TestCase(1,0)]
        [TestCase(0.5,0)]
        [TestCase(0.25,0)]
        [TestCase(0.1,0)]
        [TestCase(0.05,0)]
        [TestCase(0.01,0)]
        public void TestGraticuleWgs84(double scale, float mapRotationDeg)
        {
            using (var map = GetMap(4326, new Coordinate(0.5,-0.5),mapRotationDeg))
            {
                map.MapScale = scale;
            
                var graticule = map.Decorations[0] as SharpMap.Rendering.Decoration.Graticule.Graticule;

                CalculateGraticule(map, graticule);

                var gcsStyle = graticule.GcsGraticuleStyle;
                if (gcsStyle.SecondaryLineStyle != GraticuleLineStyle.None)
                    map.Layers.Add(CreateLayer("Gcs Secondary", 4326, 4326, 
                        map.Envelope,_gcsDomain, Brushes.Orange, 6, gcsStyle.Subdivision));
            
                map.Layers.Add(CreateLayer("Gcs Primary", 4326,4326, 
                    map.Envelope, _gcsDomain,Brushes.Red, 10, gcsStyle.Division));

                SaveMap(map, true);
                
            }
        }

        [TestCase(500000,0)]
        [TestCase(2500,0)]
        [TestCase(2500,60)]
        [TestCase(2500,120)]
        [TestCase(2500,180)]
        [TestCase(2500,240)]
        [TestCase(1000,0)]
        [TestCase(500,0)]
        [TestCase(100,0)]
        public void TestGraticuleWebMercator(double scale, float mapRotationDeg)
        {
            using (var map = GetMap(3857, new Coordinate(19300000,-5010000),mapRotationDeg))
            {
                map.MapScale = scale;
            
                var graticule = map.Decorations[0] as SharpMap.Rendering.Decoration.Graticule.Graticule;
                CalculateGraticule(map, graticule);

                // Projected
                var pcsStyle = graticule.PcsGraticuleStyle;
                if (pcsStyle.SecondaryLineStyle != GraticuleLineStyle.None)
                    map.Layers.Add(CreateLayer("Pcs Secondary", map.SRID, map.SRID,
                        map.Envelope, GeoSpatialMath.WebMercatorEnv, Brushes.CadetBlue, 8, pcsStyle.Subdivision));

                map.Layers.Add(CreateLayer("Pcs Primary", map.SRID, map.SRID,
                    map.Envelope, GeoSpatialMath.WebMercatorEnv, Brushes.DodgerBlue, 12, pcsStyle.Division));

                // Geographic
                var unProjectedEnv = TransformBox(map.Envelope, map.SRID, 4326);
                var gcsStyle = graticule.GcsGraticuleStyle;
                if (gcsStyle.SecondaryLineStyle != GraticuleLineStyle.None)
                    map.Layers.Add(CreateLayer("Gcs Secondary", 4326, map.SRID,
                        unProjectedEnv, _gcsWebMercDomain, Brushes.Orange, 6, gcsStyle.Subdivision));

                map.Layers.Add(CreateLayer("Gcs Primary", 4326, map.SRID,
                    unProjectedEnv, _gcsWebMercDomain, Brushes.Red, 10, gcsStyle.Division));

                map.BackgroundLayer.Add(GetBruTileLayer());
                
                SaveMap(map, false);
            } 
        }

        [TestCase(500000,0)]
        [TestCase(2500,0)]
        [TestCase(2500,60)]
        [TestCase(2500,120)]
        [TestCase(2500,180)]
        [TestCase(2500,240)]
        [TestCase(1000,0)]
        [TestCase(500,0)]
        [TestCase(100,0)]
        public void TestGraticuleInd75Utm47N(double scale, float mapRotationDeg)
        {
            // PCS = 24047 and GCS = 4240
            using (var map = GetMap(24047, new Coordinate(700000,1000000),mapRotationDeg))
            {
                map.MapScale = scale;
            
                var graticule = map.Decorations[0] as SharpMap.Rendering.Decoration.Graticule.Graticule;
                CalculateGraticule(map, graticule);

                // Projected
                var pcsStyle = graticule.PcsGraticuleStyle;
                var pcsDomain = new Envelope(166021.44, 534994.66,0.00 ,9329005.18);
                if (pcsStyle.SecondaryLineStyle != GraticuleLineStyle.None)
                    map.Layers.Add(CreateLayer("Pcs Secondary", map.SRID, map.SRID,
                        map.Envelope, pcsDomain , Brushes.CadetBlue, 8, pcsStyle.Subdivision));

                map.Layers.Add(CreateLayer("Pcs Primary", map.SRID, map.SRID,
                    map.Envelope, pcsDomain, Brushes.DodgerBlue, 12, pcsStyle.Division));

                // Geographic
                var unProjectedEnv = TransformBox(map.Envelope, map.SRID, 4240);
                var gcsStyle = graticule.GcsGraticuleStyle;
                if (gcsStyle.SecondaryLineStyle != GraticuleLineStyle.None)
                    map.Layers.Add(CreateLayer("Gcs Secondary", 4240, map.SRID,
                        unProjectedEnv, _gcsWebMercDomain, Brushes.Orange, 6, gcsStyle.Subdivision));

                map.Layers.Add(CreateLayer("Gcs Primary", 4240, map.SRID,
                    unProjectedEnv, _gcsWebMercDomain, Brushes.Red, 10, gcsStyle.Division));

                // can't do this - it's 3857 and doesn't project to any other Map.SRID
                //map.BackgroundLayer.Add(GetBruTileLayer());
                
                SaveMap(map, false);
            } 
        }

        private void SaveMap(Map map, bool isGeographic)
        {
            var x = map.Center.X.ToString("N0");//GetFormattedLabel(isGeographic, map.Center.X, AxisOrientationEnum.East).Replace(",","");
            var y = map.Center.Y.ToString("N0");//GetFormattedLabel(isGeographic, map.Center.Y, AxisOrientationEnum.North).Replace(",","");
            var s = map.MapScale.ToString("00000.###").Replace(".", "-");
            var fn = $"Graticule SRID {map.SRID} Scale {s} Center {x} {y} Rotn {map.MapTransformRotation:N0}.png";
            using (var img = map.GetMap(96))
                img.Save(Path.Combine(UnitTestsFixture.GetImageDirectory(this), fn),ImageFormat.Png);
        }

        private void CalculateGraticule(MapViewport map, SharpMap.Rendering.Decoration.Graticule.Graticule graticule)
        {
            using (var img = new Bitmap(map.Size.Width, map.Size.Height, PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(img))
                    graticule.Render(g, map);
        }

        private Map GetMap(int srid, Coordinate center, float mapRotationDeg)
        {
            var map = new Map(new Size(800,640));
            map.SRID= srid;
            map.Center = center;
            map.BackColor = Color.BlanchedAlmond;

            if (!mapRotationDeg.Equals(0f))
            {
                var matrix = new System.Drawing.Drawing2D.Matrix();
                matrix.RotateAt(mapRotationDeg, new PointF(map.Size.Width * 0.5f, map.Size.Height* 0.5f));
                map.MapTransform = matrix;
            }
            
            map.Decorations.Add(new SharpMap.Rendering.Decoration.Graticule.Graticule
            {
                GcsGraticuleStyle = {PrimaryLineStyle = GraticuleLineStyle.SolidTick}
            });
            
            map.Decorations.Add(new EyeOfSight());
            
            map.Disclaimer = "FEATURE LAYER DOTS SHOULD COINCIDE WITH GRATICULE TICKS";
            
            return map;
        }

        private Envelope TransformBox(Envelope env, int sourceSrid, int targetSrid)
        {
            var transform = Session.Instance.CoordinateSystemServices.CreateTransformation(sourceSrid, targetSrid);

            var ll = transform.MathTransform.Transform(env.BottomLeft().ToDoubleArray());
            var ur = transform.MathTransform.Transform(env.TopRight().ToDoubleArray());
            
            return new Envelope(new Coordinate(ll[0], ll[1]), 
                                new Coordinate(ur[0], ur[1]));
        }

        private ILayer CreateLayer(string layerName, int sourceSrid, int targetSrid, Envelope viewExtents, Envelope crsDomain, Brush symColour, int symSize, double interval)
        {
            var gp = new GeometryProvider(Point.Empty);
            gp.SRID = sourceSrid;

            var originX = Math.Floor(viewExtents.MinX / interval) * interval;
            var originY = Math.Floor(viewExtents.MinY / interval) * interval;

            var coord = new Coordinate();

            var thisX = originX;
            while (thisX < viewExtents.MaxX)
            {
                var thisY = originY;
                while (thisY < viewExtents.MaxY)
                {
                    coord.X = thisX;
                    coord.Y = thisY;
                    if (crsDomain.Contains(coord))
                        gp.Geometries.Add(new Point(thisX, thisY));
                    thisY += interval;
                }
                thisX += interval;
            }

            var vl = new VectorLayer(layerName)
            {
                DataSource = gp, 
                TargetSRID = targetSrid, 
                Style = {PointColor = symColour, PointSize = symSize}
            };
            return vl;
        }

        private ILayer GetBruTileLayer()
        {
            var cacheFolder = "";//System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BruTileCache", "Osm");
            var lyrBruTile = new TileLayer(
                BruTile.Predefined.KnownTileSources.Create(BruTile.Predefined.KnownTileSource.StamenTonerLite),
                "Tiles", Color.Transparent, true, cacheFolder)
            {
                SRID = 3857,
                TargetSRID = 3857
            };
            return lyrBruTile;


        }

        [Ignore ("Time consuming test that has already been validated")]
        [TestCase(7.0, 8.0, 0.00000001, AxisOrientationEnum.North )]
        public void TestDmsLabel(  double fromVal, double toVal, double increment, AxisOrientationEnum axis)
        {
            // ok ok - testing private method suggests this method should be public and somewhere else
            // I think NTS would be good place, but at time of writing NTS was going through major version
            // upgrade, so have left this as is for next significant release of SharpMap
            
            var tolerance = 1.1E-8;

            char[] split = {'°', '\'', '"', 'N', 'S', 'E', 'W'};

            Type type = typeof (SharpMap.Rendering.Decoration.Graticule.Graticule);
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo method = type.GetMethod("GetFormattedLabel", bindingAttr);
            
            var gratitule = Activator.CreateInstance(type);

            var thisVal = fromVal;
            do
            {
                var dms = (string)method.Invoke(gratitule, new object [] {true, thisVal, axis});
                //var dms = GetFormattedLabel(true, degree, axis);
                var tokens = dms.Split(split, StringSplitOptions.RemoveEmptyEntries);
                var dec = double.Parse(tokens[0]);
                if (tokens.Length >= 3)
                    dec += double.Parse(tokens[2]) / 3600;
                if (tokens.Length >= 2)
                    dec += double.Parse(tokens[1]) / 60;

                Assert.AreEqual(Math.Round(thisVal,8), Math.Round(dec, 8), tolerance, $"{thisVal:N8} {dms} {dec:N8}");
                
                thisVal += increment;
            } while (thisVal < toVal);
        }
    }
}
