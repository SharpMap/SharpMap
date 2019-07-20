using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
    [TestFixture]
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
            var x = GetFormattedLabel(isGeographic, map.Center.X, AxisOrientationEnum.East).Replace(",","");
            var y = GetFormattedLabel(isGeographic, map.Center.Y, AxisOrientationEnum.North).Replace(",","");
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
                BruTile.Predefined.KnownTileSources.Create(BruTile.Predefined.KnownTileSource.OpenStreetMap),
                "Tiles", Color.Transparent, true, cacheFolder)
            {
                SRID = 3857,
                TargetSRID = 3857
            };
            return lyrBruTile;


        }


        [Ignore ("Time consuming test that has already been validated")]
        [TestCase(7, AxisOrientationEnum.North )]
        public void TestDmsLabel(double degree, AxisOrientationEnum axis)
        {
            char[] split = {'°', '\'', '"', 'N', 'S', 'E', 'W'};

            var until = Math.Truncate(degree + 1);
            
            do
            {
                var dms = GetFormattedLabel(true, degree, axis);
                var tokens = dms.Split(split, StringSplitOptions.RemoveEmptyEntries);
                var dec = double.Parse(tokens[0]);
                if (tokens.Length >= 3)
                    dec += double.Parse(tokens[2]) / 3600;
                if (tokens.Length >= 2)
                    dec += double.Parse(tokens[1]) / 60;

                Assert.AreEqual(degree, dec, 2.8E-8d, $"{degree:N8} {dms} {dec:N8}");
                
                degree += 0.00000001;
            } while (degree < until);


        }
        
            private string GetFormattedLabel(bool isGeographicCoordinateSystem, double value, AxisOrientationEnum axis)
        {
            var axisSuffix = string.Empty;
            switch (axis)
            {
                case AxisOrientationEnum.North:
                    axisSuffix = value >= 0 ? "N" : "S";
                    break;

                case AxisOrientationEnum.East:
                    axisSuffix = value >= 0 ? "E" : "W";
                    break;
            }

            int dp;

            if (!isGeographicCoordinateSystem)
            {
                dp = GetDecimalPlaces(value, 3);
                return Math.Abs(value).ToString($"N{dp}") + "m" + axisSuffix;
            }

            // DMS: 8dp approx = 1mm (= 1" arc to 4dp) 
            var deg = Math.Round(Math.Abs(value), 8, MidpointRounding.AwayFromZero);
            var iDeg = (int) (deg); // equiv to Math.Truncate
            var dec = deg - iDeg;

            var mins = dec * 60;
            var iMin = (int) (mins); // equiv to Math.Truncate

            var secs = Math.Round(deg * 3600 - (iDeg * 3600) - (iMin * 60), 4, MidpointRounding.AwayFromZero);

            if (Math.Abs(secs - 60) < SecondOfArcDecimalPlaces)
            {
                secs = 0;
                iMin += 1;
            }

            if (iMin == 60)
            {
                iMin = 0;
                iDeg += 1;
            }

            if (iMin == 0 && secs < SecondOfArcDecimalPlaces)
                return $"{iDeg}°{axisSuffix}";

            if (secs < SecondOfArcDecimalPlaces)
                return $"{iDeg}°{iMin:00}'{axisSuffix}";

            dp = GetDecimalPlaces(secs, 4);
            var fmt = dp == 0 ? "00" : $"00.{new string('0', dp)}";
            return ($"{iDeg}°{iMin:00}'{secs.ToString(fmt)}\"{axisSuffix}");
        }

        /// <summary>
        /// Determine number of decimal places required for <paramref name="maxPrecision "/> without any trailing zeros
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxPrecision"></param>
        /// <returns>Number of decimal places without any trailing zeros</returns>
        private static int GetDecimalPlaces(double value, int maxPrecision)
        {
            if (maxPrecision <= 0) return 0;
            
            // eg 0.###
            var fmt = "0." + new string('#', maxPrecision);

            // implicit rounding away from zero???
            var strValue = value.ToString(fmt);
                
            return strValue.Contains('.') ? strValue.Reverse().TakeWhile(c => c !='.').Count() : 0;
        }

    }
}
