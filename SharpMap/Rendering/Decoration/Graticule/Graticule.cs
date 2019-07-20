using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Common.Logging;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;
using SharpMap.Styles;
using SharpMap.Utilities;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Point = NetTopologySuite.Geometries.Point;

namespace SharpMap.Rendering.Decoration.Graticule
{
    /// <summary>
    /// Map Decoration to draw projected and/or geographic graticule on the map.
    /// The Map.SRID MUST be set to a valid ID for the graticule to render, as
    /// appropriate units are derived from the map's coordinate reference system
    /// (eg degrees-minutes-seconds, or projected units such as metres).
    /// <para>
    /// The Projected Coordinate System graticule is managed via <see cref="PcsGraticuleStyle"/>,
    /// and the Geographic Coordinate System graticule via <see cref="GcsGraticuleStyle"/> 
    /// </para>
    /// Web Mercator Projected Coordinate System graticule can be rendered as a standard 
    /// rectilinear graticule, or as meridian scale distortion lines via <see cref="PcsGraticuleMode"/> 
    /// </summary>
    public class Graticule : MapDecoration
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(Graticule));

        private const double OneSecond = 1 / 3600.0;
        private const double GeographicTolerance = 1e-9; // 1dp more than typical precision
        private const double ProjectedTolerance = 1e-4; // 1dp more than typical precision
        private const double SecondOfArcDecimalPlaces = 1e-4; // approx. 1mm at the equator

        private const int PcsPowerRangeMin = -5;
        private const int PcsPowerRangeMax = 10;
        private readonly double[] _pcsPreferredMultiples = {5, 2.5, 2, 1}; // MUST be in descending order

        private const int GcsPowerRangeMin = -5;
        private const int GcsPowerRangeMax = 0;

        private readonly double[]
            _gcsPreferredMultiples = {90, 60, 30, 20, 15, 10, 5, 2.5, 2, 1}; // MUST be in descending order

        private readonly Polygon _webMercatorClipPolygon = new Polygon(
            new LinearRing(new[]
            {
                GeoSpatialMath.WebMercatorEnv.BottomLeft(),
                GeoSpatialMath.WebMercatorEnv.TopLeft(),
                GeoSpatialMath.WebMercatorEnv.TopRight(),
                GeoSpatialMath.WebMercatorEnv.BottomRight(),
                GeoSpatialMath.WebMercatorEnv.BottomLeft(),
            }));

        private int _srid;
        private Envelope _oldViewExtents;
        private Polygon _viewClipPolygon;
        private ICoordinateSystem _coordinateSystem;
        private string _pcsUnitSuffix;
        private double _mapScale;

        private Envelope _pcsDomain;
        private Envelope _gcsDomain;
        private IMathTransform _unProject;
        private IMathTransform _project;
        private Envelope _pcsConstrExtents;
        private Envelope _gcsConstrExtents;
        private int _oldPcsNumSubdivisions;
        private int _oldGcsNumSubdivisions;
        private PcsGraticuleMode _oldPcsGraticuleMode;

        /// <summary>
        /// Helper class for managing graticule geometry
        /// </summary>
        private class GraticuleDef
        {
            public bool IsGeographicGraticule;
            public string Label;
            public bool IsPrimary;
            public bool IsParallel;
            public LineString WcsGraticule;
            public MultiPoint WcsTickMarks;
            public PointF[] ImageGraticule;
            public PointF[] ImageTickMarks;
            public bool[] ImageTickStyle;
        }

        /// <summary>
        /// Enumerator for indicating which end(s) of a line should be labelled
        /// </summary>
        [Flags]
        private enum LabelEnds
        {
            None = 0,
            Start = 1, // label start of line
            End = 2, // label end of line
            Both = Start + End
        }

        /// <summary>
        /// Graticule style definition for the Projected Coordinate System
        /// </summary>    
        public GraticuleStyle PcsGraticuleStyle { get; set; }

        /// <summary>
        /// Defines how Web Mercator Projected Graticule will be rendered.
        /// Either as a standard rectilinear graticule, or as meridian scale distortion lines
        /// </summary>    
        public PcsGraticuleMode PcsGraticuleMode { get; set; } =
            PcsGraticuleMode.Standard;

        /// <summary>
        /// Graticule style definition for the Geographic Coordinate System
        /// </summary>    
        public GraticuleStyle GcsGraticuleStyle { get; set; }

        public Graticule()
        {
            Location = new System.Drawing.Point(0, 0);
            Padding = new Size(0, 0);
            Anchor = MapDecorationAnchor.LeftTop;
            BorderColor = Color.Transparent;
            BackgroundColor = Color.Transparent;

            PcsGraticuleStyle = new GraticuleStyle(GraticuleStyle.GraticuleTheme.Bold,
                GraticuleLineStyle.Continuous, true,
                GraticuleBorders.LeftBottom)
            {
                SecondaryPen = {DashStyle = DashStyle.Dash}
            };

            GcsGraticuleStyle = new GraticuleStyle(GraticuleStyle.GraticuleTheme.Subtle,
                GraticuleLineStyle.HollowTick, false,
                GraticuleBorders.RightTop);
        }

        protected override Size InternalSize(Graphics g, MapViewport map)
        {
            return new Size(map.Size.Width, map.Size.Height);
        }

        protected override void OnRender(Graphics g, MapViewport map)
        {
            try
            {
                if (_srid != map.SRID)
                    InitCoordinateSystem(map.SRID);

                if (_coordinateSystem == null) return;

                var webMercatorScaleLinesActive = _srid == GeoSpatialMath.WebMercatorSrid &&
                                                  PcsGraticuleMode == PcsGraticuleMode
                                                      .WebMercatorScaleLines;

                if (_oldViewExtents is null || !_oldViewExtents.Equals(map.Envelope) ||
                    PcsGraticuleStyle.NumSubdivisions != _oldPcsNumSubdivisions ||
                    GcsGraticuleStyle.NumSubdivisions != _oldGcsNumSubdivisions ||
                    PcsGraticuleMode != _oldPcsGraticuleMode)
                    CalculateMetrics(g, map, webMercatorScaleLinesActive);

                if (_coordinateSystem is IProjectedCoordinateSystem)
                {
                    OnRenderInternal(g, map, PcsGraticuleStyle, _pcsConstrExtents, _pcsDomain,
                        webMercatorScaleLinesActive);
                    OnRenderInternal(g, map, GcsGraticuleStyle, _gcsConstrExtents, _gcsDomain, false);
                }
                else
                    OnRenderInternal(g, map, GcsGraticuleStyle, _gcsConstrExtents, _gcsDomain, false);
            }
            catch (NullReferenceException ex)
            {
                _logger.WarnFormat("SRID {0} Env {1} Rotation {2}", _srid, map.Envelope, map.MapTransformRotation);
                _logger.Error(ex);
            }
        }

        /// <summary>
        /// Fully render a graticule according to the given style 
        /// (ie Projected graticule is rendered separately to Geographic graticule)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <param name="style"></param>
        /// <param name="constrExtents"></param>
        /// <param name="crsDomain"></param>
        /// <param name="webMercatorScaleLinesActive">Refer to &lt;see cref="PcsGraticuleMode"/&gt;</param>
        private void OnRenderInternal(Graphics g, MapViewport map, GraticuleStyle style,
            Envelope constrExtents, Envelope crsDomain, bool webMercatorScaleLinesActive)
        {
            var visibleRef = style.VisibilityUnits == VisibilityUnits.Scale ? _mapScale : map.Zoom;

            if (style.Division <= 0 || style.PrimaryLineStyle == GraticuleLineStyle.None ||
                visibleRef <= style.MinVisible || visibleRef > style.MaxVisible ||
                constrExtents == null || constrExtents.IsNull)
                return;

            var isGcsStyle = style == GcsGraticuleStyle;
            var targetFactory = new GeometryFactory(new PrecisionModel());
            var tolerance = style == GcsGraticuleStyle ? GeographicTolerance : ProjectedTolerance;

            var graticuleDefs = new List<GraticuleDef>();
            // calculate geometry for Meridians and Parallels in world coordinate system
            graticuleDefs.AddRange(CalculateParallels(style, constrExtents, crsDomain, webMercatorScaleLinesActive));
            graticuleDefs.AddRange(CalculateMeridians(style, constrExtents, crsDomain, webMercatorScaleLinesActive));

            // project / simplify / clip / transform to image space
            foreach (var def in graticuleDefs)
            {
                // geographic >> projection
                if (isGcsStyle && _project != null)
                {
                    def.WcsGraticule =
                        (LineString) GeometryTransform.TransformLineString(def.WcsGraticule, _project, targetFactory);
                    def.WcsTickMarks = new MultiPoint(TransformPreserveZ(_project, def.WcsTickMarks.Coordinates));
                }

                if (def.WcsGraticule == null || def.WcsGraticule.IsEmpty) continue;

                // Simplify (AFTER doing any projection)
                if (def.WcsGraticule.NumPoints > 2)
                {
                    int same;
                    if (def.IsParallel)
                    {
                        var initialY = def.WcsGraticule .Coordinates[0].Y;
                        same = def.WcsGraticule.Coordinates.Count(c => Math.Abs(c.Y - initialY) < tolerance);
                    }
                    else
                    {
                        var initialX = def.WcsGraticule .Coordinates[0].X;
                        same = def.WcsGraticule.Coordinates.Count(c => Math.Abs(c.X - initialX) < tolerance);
                    }

                    if (def.WcsGraticule.Coordinates.Length == same)
                    {
                        // simplify to single line segment
                        def.WcsGraticule  = new LineString(new[]
                        {
                            def.WcsGraticule .Coordinates.First(),
                            def.WcsGraticule .Coordinates.Last()
                        });
                    }
                }

                // Clip + Transform to image. See CalculateMetrics for special handling of _viewClipPolygon and WebMercator
                def.ImageGraticule = map.WorldToImage(_viewClipPolygon.Intersection(def.WcsGraticule).Coordinates, true);

                var clippedTicks = _viewClipPolygon.Intersection(def.WcsTickMarks).Coordinates;
                // transfer Z ordinate to bool[] to indicate Primary or Secondary pen when rendering 
                def.ImageTickMarks = map.WorldToImage(clippedTicks, true);
                def.ImageTickStyle = new bool[clippedTicks.Length];
                for (var j = 0; j < clippedTicks.Length; j++)
                    def.ImageTickStyle[j] = Convert.ToInt32(clippedTicks[j].Z) == 1;
            }

            // Render
            var ticksAndEdgeCuts = graticuleDefs.Where(def =>
            {
                var s = def.IsGeographicGraticule
                    ? GcsGraticuleStyle
                    : PcsGraticuleStyle;

                if (webMercatorScaleLinesActive) return false;

                if (def.IsPrimary)
                    return s.PrimaryLineStyle == GraticuleLineStyle.SolidTick ||
                           s.PrimaryLineStyle == GraticuleLineStyle.HollowTick;

                return s.SecondaryLineStyle == GraticuleLineStyle.SolidTick ||
                       s.SecondaryLineStyle == GraticuleLineStyle.HollowTick;
            }).ToArray();

            var linesOnly = graticuleDefs.Except(ticksAndEdgeCuts).ToArray();

            if (ticksAndEdgeCuts.Length > 0)
            {
                RenderGraticuleTicks(g, ticksAndEdgeCuts);
                RenderEdgeCuts(g, ticksAndEdgeCuts);
            }

            if (linesOnly.Length > 0)
            {
                RenderGraticuleLines(g, linesOnly);
            }

            if (webMercatorScaleLinesActive)
                RenderScaleLineLabels(g, graticuleDefs, map);
            else
                RenderLabels(g, graticuleDefs, map.MapTransformRotation);
        }

        /// <summary>
        /// Progressing from South to North, calculate geometry for each parallel at regular intervals from west to east,
        /// constraining to CRS domain where applicable.  
        /// </summary>
        /// <param name="style"></param>
        /// <param name="constrExtents"></param>
        /// <param name="crsDomain"></param>
        /// <param name="webMercatorScaleLinesActive">Refer to &lt;see cref="PcsGraticuleMode"/&gt;</param>
        /// <returns> List of graticule geometries for rendering</returns>
        private List<GraticuleDef> CalculateParallels(GraticuleStyle style, Envelope constrExtents,
            Envelope crsDomain, bool webMercatorScaleLinesActive)
        {
            var parallels = new List<GraticuleDef>();
            var coordList = new List<Coordinate>();
            var isGeographicGraticule = style == GcsGraticuleStyle;
            var tolerance = isGeographicGraticule ? GeographicTolerance : ProjectedTolerance;

            // Parallels
            var iterateX = 1 + Convert.ToInt32((constrExtents.MaxX - constrExtents.MinX) / style.Subdivision);
            var iterateY = 1 + Convert.ToInt32((constrExtents.MaxY - constrExtents.MinY) / style.Subdivision);

            for (var i = 0; i < iterateY; i++)
            {
                var thisY = constrExtents.MinY + i * style.Subdivision;

                if (webMercatorScaleLinesActive)
                    // skip if NOT equator
                    if (!(Math.Abs(thisY) < tolerance ||
                          // or NOT "poles" (85 deg N/S)
                          crsDomain != null && Math.Abs(Math.Abs(thisY) - crsDomain.MaxY) < tolerance))
                        continue;

                var isPrimaryParallel = IsPrimaryInterval(thisY, style.Division, tolerance);

                if (!isPrimaryParallel && style.SecondaryLineStyle == GraticuleLineStyle.None) continue;

                if (ExceedsResolution(style, isPrimaryParallel)) continue;

                if (crsDomain != null && !crsDomain.IsNull)
                    // Ensure parallel Y is within CRS domain 
                    if (thisY < crsDomain.MinY - tolerance || thisY > crsDomain.MaxY + tolerance)
                        continue;

                coordList.Clear();
                for (var j = 0; j < iterateX; j++)
                {
                    var thisX = constrExtents.MinX + j * style.Subdivision;
                    var x = thisX;

                    // Trim parallel east-west extent to CRS domain
                    if (crsDomain != null && !crsDomain.IsNull)
                        //test for difference of 1 full increment or more
                        if (x < crsDomain.MinX - tolerance)
                            if (x < crsDomain.MinX - style.Subdivision + tolerance)
                                continue;
                            else
                                x = crsDomain.MinX;

                        else if (x > crsDomain.MaxX + tolerance)
                            if (x > crsDomain.MaxX + style.Subdivision - tolerance)
                                continue;
                            else
                                x = crsDomain.MaxX;

                    coordList.Add(new Coordinate(x, thisY));
                }

                if (coordList.Count < 2) continue;

                parallels.Add(new GraticuleDef()
                {
                    IsGeographicGraticule = isGeographicGraticule,
                    IsParallel = true,
                    IsPrimary = isPrimaryParallel,
                    Label = GetFormattedLabel(isGeographicGraticule, thisY, AxisOrientationEnum.North),
                    WcsGraticule = new LineString(coordList.ToArray()),
                    WcsTickMarks = (MultiPoint) MultiPoint.Empty // new List<IPoint>().ToArray())
                });
            }

            return parallels;
        }

        /// <summary>
        /// Progressing from West to East, calculate geometry for each meridian at regular intervals from south to north,
        /// constraining to CRS domain where applicable. Graticule intersections (ticks) are also computed if required by the style  
        /// </summary>
        /// <param name="style"></param>
        /// <param name="constrExtents"></param>
        /// <param name="crsDomain"></param>
        /// <param name="webMercatorScaleLinesActive">Refer to &lt;see cref="PcsGraticuleMode"/&gt;</param>
        /// <returns> List of graticule geometries for rendering</returns>
        private List<GraticuleDef> CalculateMeridians(GraticuleStyle style, Envelope constrExtents,
            Envelope crsDomain, bool webMercatorScaleLinesActive)
        {
            var meridians = new List<GraticuleDef>();
            var coordList = new List<Coordinate>();
            var tickList = new List<IPoint>();
            var isGeographicGraticule = style == GcsGraticuleStyle;
            var tolerance = isGeographicGraticule ? GeographicTolerance : ProjectedTolerance;

            // Meridians
            var iterateX = 1 + Convert.ToInt32((constrExtents.MaxX - constrExtents.MinX) / style.Subdivision);
            var iterateY = 1 + Convert.ToInt32((constrExtents.MaxY - constrExtents.MinY) / style.Subdivision);

            var factor = 1.0;
            if (webMercatorScaleLinesActive)
            {
                // increase resolution for Web Mercator scales lines for nice smooth line when zoomed out
                factor = 0.5;
                iterateY *= 2;
            }

            for (var i = 0; i < iterateX; i++)
            {
                var thisX = constrExtents.MinX + i * style.Subdivision;
                var isPrimaryMeridian = IsPrimaryInterval(thisX, style.Division, tolerance);

                if (!isPrimaryMeridian && style.SecondaryLineStyle == GraticuleLineStyle.None) continue;

                if (ExceedsResolution(style, isPrimaryMeridian)) continue;

                if (crsDomain != null && !crsDomain.IsNull)
                    // Ensure meridian X is within CRS domain 
                    if (thisX < crsDomain.MinX - tolerance || thisX > crsDomain.MaxX + tolerance)
                        continue;

                coordList.Clear();
                tickList.Clear();
                for (var j = 0; j < iterateY; j++)
                {
                    var thisY = constrExtents.MinY + j * style.Subdivision * factor;
                    var y = thisY;

                    // Trim meridian north-south extent to CRS domain
                    if (crsDomain != null && !crsDomain.IsNull)
                        // test for difference of 1 full increment or more
                        if (y < crsDomain.MinY - tolerance)
                            if (y < crsDomain.MinY - style.Subdivision + tolerance
                            ) //(y + style.Subdivision <= crsDomain.MinY)
                                continue;
                            else
                                y = crsDomain.MinY;

                        else if (y > crsDomain.MaxY + tolerance)
                            if (y > crsDomain.MaxY + style.Subdivision - tolerance
                            ) //(y - style.Subdivision >= crsDomain.MaxY)
                                continue;
                            else
                                y = crsDomain.MaxY;

                    // special handling for Web Mercator scale lines
                    coordList.Add(webMercatorScaleLinesActive
                        ? new Coordinate(CalcScaleCorrectedX(thisX, y), y)
                        : new Coordinate(thisX, y));

                    if (webMercatorScaleLinesActive) continue;

                    // if regular interval has been adjusted to fit CRS bounds
                    if (Math.Abs(thisY - y) > tolerance) continue;

                    // ticks
                    var isPrimaryParallel = IsPrimaryInterval(thisY, style.Division, tolerance);

                    if (!style.IsTickRequired(isPrimaryMeridian, isPrimaryParallel)) continue;

                    if (ExceedsResolution(style, isPrimaryParallel)) continue;

                    if (crsDomain != null && !crsDomain.IsNull)
                        // skip this tick if coincident with CRS domain envelope
                        if (Math.Abs(thisX - crsDomain.MinX) < tolerance ||
                            Math.Abs(thisX - crsDomain.MaxX) < tolerance ||
                            Math.Abs(thisY - crsDomain.MinY) < tolerance ||
                            Math.Abs(thisY - crsDomain.MaxY) < tolerance)
                            continue;

                    // use Z ordinate to indicate intersection of primary parallel and primary meridian
                    tickList.Add(new Point(
                        coordList.Last().X,
                        coordList.Last().Y,
                        isPrimaryMeridian && isPrimaryParallel ? 1 : 0));
                }

                if (coordList.Count < 2) continue;

                meridians.Add(new GraticuleDef()
                {
                    IsGeographicGraticule = isGeographicGraticule,
                    IsParallel = false,
                    IsPrimary = isPrimaryMeridian,
                    Label = GetFormattedLabel(isGeographicGraticule, thisX, AxisOrientationEnum.East),
                    WcsGraticule = new LineString(coordList.ToArray()),
                    WcsTickMarks = new MultiPoint(tickList.ToArray())
                });
            }

            return meridians;
        }

        /// <summary>
        /// Returns true if value is a multiple of the primaryInterval
        /// </summary>
        /// <param name="value">Number to be check for Primary interval</param>
        /// <param name="primaryInterval"></param>
        /// <param name="tolerance">Tolerance in Geographic or Projected units</param>
        /// <returns></returns>
        private bool IsPrimaryInterval(double value, double primaryInterval, double tolerance)
        {
            var modulo = Math.Abs(value) % primaryInterval;
            return modulo < tolerance || primaryInterval - modulo < tolerance;
        }

        /// <summary>
        /// Test to see if this increment is getting too small to be of practical use.
        /// This allows secondary graticule to be filtered out independently of primary graticule
        /// as user zooms in 
        /// </summary>
        /// <param name="style"></param>
        /// <param name="isPrimaryIncrement"></param>
        /// <returns></returns>
        private bool ExceedsResolution(GraticuleStyle style, bool isPrimaryIncrement)
        {
            if (style == GcsGraticuleStyle)
            {
                // geographic
                if (isPrimaryIncrement)
                    return style.Division < OneSecond;
                else
                    return style.Subdivision < OneSecond;
            }

            // projected 
            if (isPrimaryIncrement)
                return style.Division < 1;
            else
                return style.Subdivision < 1;
        }

        /// <summary>
        /// Return a formatted distance label
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetFormattedDistanceLabel(double value)
        {
            if (value > 1000 && _pcsUnitSuffix == "m")
                return GetFormattedLabel(false, value / 1000,
                    AxisOrientationEnum.Other).Replace("m", "km");

            return GetFormattedLabel(false, value, AxisOrientationEnum.Other);
        }

        /// <summary>
        /// Format a value with appropriate units and axis suffix 
        /// </summary>
        /// <param name="isGeographicCoordinateSystem"></param>
        /// <param name="value"></param>
        /// <param name="axis"></param>
        /// <returns>Formatted label, such as 7,000,000mN or 8°15'20"E</returns>
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
                return Math.Abs(value).ToString($"N{dp}") + _pcsUnitSuffix + axisSuffix;
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

            // -101.8, 0 >> -102
            var strValue = value.ToString(fmt);

            return strValue.Contains('.') ? strValue.Reverse().TakeWhile(c => c != '.').Count() : 0;
        }

        /// <summary>
        /// Transform coordinates between coordinate systems, preserving Z ordinate.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="coords"></param>
        /// <returns>Transformed array with z ordinate preserved</returns>
        private IPoint[] TransformPreserveZ(IMathTransform transform, Coordinate[] coords)
        {
            var transformed = new IPoint[coords.Length];
            for (var i = 0; i < coords.Length; i++)
            {
                var pt = transform.Transform(coords[i].ToDoubleArray());
                transformed[i] = new Point(pt[0], pt[1], coords[i].Z);
            }

            return transformed;
        }

        /// <summary>
        /// Render giving graticule definitions as a simple line according to the associated style 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="graticuleDefs"></param>
        private void RenderGraticuleLines(Graphics g, IEnumerable<GraticuleDef> graticuleDefs)
        {
            foreach (var def in graticuleDefs)
            {
                if (def.ImageGraticule == null || def.ImageGraticule.Length < 2) continue;

                var style = def.IsGeographicGraticule
                    ? GcsGraticuleStyle
                    : PcsGraticuleStyle;

                g.DrawLines(def.IsPrimary ? style.PrimaryPen : style.SecondaryPen, def.ImageGraticule);
            }
        }

        /// <summary>
        /// Render ticks for ALL graticule definitions according to the associated style
        /// </summary>
        /// <param name="g"></param>
        /// <param name="graticuleDefs"></param>
        private void RenderGraticuleTicks(Graphics g, IEnumerable<GraticuleDef> graticuleDefs)
        {
            var oldTransform = g.Transform;

            using (var matrix = new Matrix())
            {
                foreach (var def in graticuleDefs)
                {
                    if (def.ImageTickMarks == null || def.ImageTickMarks.Length == 0) continue;

                    var style = def.IsGeographicGraticule
                        ? GcsGraticuleStyle
                        : PcsGraticuleStyle;

                    var tolerance = def.IsGeographicGraticule ? GeographicTolerance : ProjectedTolerance;

                    var orientOn = PointF.Empty;
                    for (var i = 0; i < def.ImageTickMarks.Length; i++)
                    {
                        var tick = def.ImageTickMarks[i];
                        if (orientOn.IsEmpty)
                        {
                            if (i + 1 < def.ImageTickMarks.Length)
                            {
                                orientOn.X = def.ImageTickMarks[i + 1].X;
                                orientOn.Y = def.ImageTickMarks[i + 1].Y;
                            }
                            else if (def.ImageTickMarks.Length >= 2)
                            {
                                orientOn.X = def.ImageTickMarks[i - 1].X;
                                orientOn.Y = def.ImageTickMarks[i - 1].Y;
                            }
                            else if (def.ImageGraticule.Length > 0)
                            {
                                orientOn = def.ImageGraticule.FirstOrDefault(
                                    p => !(Math.Abs(p.X - tick.X) < tolerance &&
                                           Math.Abs(p.Y - tick.Y) < tolerance));
                            }

                            // final catch
                            if (orientOn.IsEmpty) orientOn = tick;
                        }

                        var deg = Math.Atan2(tick.Y - orientOn.Y, tick.X - orientOn.X) * 180 / Math.PI;
                        if (!def.IsParallel) deg += 90;
                        if (deg < 0) deg += 360;

                        matrix.Reset();
                        matrix.Translate(tick.X, tick.Y, MatrixOrder.Append);
                        matrix.RotateAt((float) deg, tick, MatrixOrder.Append);
                        g.Transform = matrix;

                        g.DrawPath(def.ImageTickStyle[i] ? style.PrimaryPen : style.SecondaryPen,
                            style.GetTick(def.ImageTickStyle[i]));

                        orientOn = tick;
                        System.Diagnostics.Debug.WriteLine($"x={tick.X} y={tick.Y}");
                    }
                }
            }

            g.Transform = oldTransform;
        }

        /// <summary>
        /// Edge cuts are the short lines where a graticule intersects the border
        /// </summary>
        /// <param name="g"></param>
        /// <param name="graticuleDefs"></param>
        private void RenderEdgeCuts(Graphics g, IEnumerable<GraticuleDef> graticuleDefs)
        {
            var pt = PointF.Empty;

            foreach (var def in graticuleDefs)
            {
                if (def.ImageGraticule == null || def.ImageGraticule.Length < 2) continue;

                var style = def.IsGeographicGraticule
                    ? GcsGraticuleStyle
                    : PcsGraticuleStyle;

                var margin = def.IsPrimary ? style.PrimaryMargin : style.SecondaryMargin;

                if (margin == 0) continue;

                // starting cut
                var dX = def.ImageGraticule[1].X - def.ImageGraticule[0].X;
                var dY = def.ImageGraticule[1].Y - def.ImageGraticule[0].Y;
                var ratio = (float) (margin / Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2)));

                pt.X = def.ImageGraticule[0].X + dX * ratio;
                pt.Y = def.ImageGraticule[0].Y + dY * ratio;

                g.DrawLine(def.IsPrimary ? style.PrimaryPen : style.SecondaryPen, def.ImageGraticule[0], pt);

                // ending cut
                var len = def.ImageGraticule.Length;
                dX = def.ImageGraticule[len - 2].X - def.ImageGraticule[len - 1].X;
                dY = def.ImageGraticule[len - 2].Y - def.ImageGraticule[len - 1].Y;
                ratio = (float) (margin / Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2)));

                pt.X = def.ImageGraticule[len - 1].X + dX * ratio;
                pt.Y = def.ImageGraticule[len - 1].Y + dY * ratio;

                g.DrawLine(def.IsPrimary ? style.PrimaryPen : style.SecondaryPen, def.ImageGraticule[len - 1], pt);
            }
        }

        /// <summary>
        /// Render labels for ALL graticule definitions according to the associated style
        /// </summary>
        /// <param name="g"></param>
        /// <param name="graticuleDefs">Graticule parameters and geometry</param>
        /// <param name="mapTransformRotation"></param>
        private void RenderLabels(Graphics g, IEnumerable<GraticuleDef> graticuleDefs, float mapTransformRotation)
        {
            var pts = new PointF[2];

            var originalTransform = g.Transform;

            using (var matrix = new Matrix())
            {
                foreach (var def in graticuleDefs)
                {
                    if (def.ImageGraticule == null || def.ImageGraticule.Length < 2) continue;

                    var style = def.IsGeographicGraticule
                        ? GcsGraticuleStyle
                        : PcsGraticuleStyle;

                    var labelEnds = GetLabelEnds(style, def, mapTransformRotation);

                    if (labelEnds == LabelEnds.None) continue;

                    var labelSize = g.MeasureString(def.Label,
                        def.IsPrimary ? style.PrimaryLabelFont : style.SecondaryLabelFont);

                    pts[0] = def.ImageGraticule.First();
                    pts[1] = def.ImageGraticule.Last();

                    var lineLen = Math.Sqrt(Math.Pow(pts[0].X - pts[1].X, 2) +
                                            Math.Pow(pts[0].Y - pts[1].Y, 2));

                    // is entire graticule long enough to label?
                    if (labelSize.Width + style.PrimaryLabelOffset.X >= lineLen) continue;

                    if ((labelEnds & LabelEnds.Start) != 0)
                        RenderLabel(g, matrix, def, style, labelSize, LabelEnds.Start);
                    if ((labelEnds & LabelEnds.End) != 0)
                        RenderLabel(g, matrix, def, style, labelSize, LabelEnds.End);
                }
            }

            g.Transform = originalTransform;
        }

        /// <summary>
        /// Determine which ends of the graticule line should be labelled as per the given <paramref name="style"/>,
        /// taking into account (any) map rotation and whether this graticule is a Parallel or Meridian 
        /// </summary>
        /// <param name="style">Graticule style</param>
        /// <param name="def">Graticule definition and geometry</param>
        /// <param name="mapTransformRotation"></param>
        /// <returns></returns>
        private LabelEnds GetLabelEnds(GraticuleStyle style, GraticuleDef def, float mapTransformRotation)
        {
            if (!def.IsPrimary && !style.LabelSubdivisions) return LabelEnds.None;

            if (style.LabelBorders == GraticuleBorders.All) return LabelEnds.Both;

            var ends = LabelEnds.None;

            if (def.IsParallel)
                // Parallels are constructed from WEST (left) to EAST (right)
                switch (mapTransformRotation)
                {
                    case var _ when mapTransformRotation.Equals((0f)):
                    case var _ when mapTransformRotation > 325f || mapTransformRotation <= 45f:

                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Left) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Right) > 0)
                            ends |= LabelEnds.End;
                        break;

                    case var _ when mapTransformRotation > 45f && mapTransformRotation <= 135f:
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Top) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Bottom) > 0)
                            ends |= LabelEnds.End;
                        break;

                    case var _ when mapTransformRotation > 135 && mapTransformRotation <= 225f:
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Right) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Left) > 0)
                            ends |= LabelEnds.End;
                        break;

                    case var _ when mapTransformRotation > 225 && mapTransformRotation <= 325:
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Bottom) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Top) > 0)
                            ends |= LabelEnds.End;
                        break;
                }
            else
                // Meridians are constructed from SOUTH (bottom) to NORTH (top)
                switch (mapTransformRotation)
                {
                    case var _ when mapTransformRotation.Equals((0f)):
                    case var _ when mapTransformRotation > 325f || mapTransformRotation <= 45f:

                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Bottom) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Top) > 0)
                            ends |= LabelEnds.End;
                        break;

                    case var _ when mapTransformRotation > 45f && mapTransformRotation <= 135f:
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Left) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Right) > 0)
                            ends |= LabelEnds.End;
                        break;

                    case var _ when mapTransformRotation > 135 && mapTransformRotation <= 225f:
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Top) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Bottom) > 0)
                            ends |= LabelEnds.End;
                        break;

                    case var _ when mapTransformRotation > 225 && mapTransformRotation <= 325:
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Right) > 0)
                            ends |= LabelEnds.Start;
                        if (((int) style.LabelBorders & (int) MapDecorationAnchorFlags.Left) > 0)
                            ends |= LabelEnds.End;
                        break;
                }

            return ends;
        }

        /// <summary>
        /// Draw the label
        /// </summary>
        /// <param name="g"></param>
        /// <param name="matrix">cached matrix to be re-used for each label</param>
        /// <param name="def">Graticule definition and geometry</param>
        /// <param name="style">Graticule Style</param>
        /// <param name="labelSize">estimated label size from g.MeasureString</param>
        /// <param name="thisEnd">the end of the line to be labelled</param>
        private void RenderLabel(Graphics g, Matrix matrix, GraticuleDef def, GraticuleStyle style, 
            SizeF labelSize, LabelEnds thisEnd)
        {
            var origin = thisEnd == LabelEnds.Start ? def.ImageGraticule.First() : def.ImageGraticule.Last();
            var orientOn = thisEnd == LabelEnds.Start ? def.ImageGraticule.Last() : def.ImageGraticule.First();

            // NB invert Y axis
            var rad = Math.Atan2(-1 * (orientOn.Y - origin.Y), orientOn.X - origin.X);
            if (rad < 0) rad += 2 * Math.PI;
            var deg = (float) Radians.ToDegrees(rad);
            if (deg < 0) deg += 360f;

            var offset = def.IsPrimary ? style.PrimaryLabelOffset : style.SecondaryLabelOffset;

            matrix.Reset();

            // rotate if required, then apply offsets from origin to top left of text
            if (deg <= 90 || deg > 270)
            {
                // Quadrants 1 & 4
                if (!deg.Equals(0f)) matrix.RotateAt(-deg, origin);
                matrix.Translate(offset.X, -offset.Y - labelSize.Height);
            }
            else
            {
                // Quadrants 2 & 3
                deg -= 180f;
                if (deg < 0) deg += 360;
                if (!deg.Equals(0f)) matrix.RotateAt(-deg, origin);
                matrix.Translate(-offset.X - labelSize.Width, -offset.Y - labelSize.Height);
            }

            g.Transform = matrix;

            if (style.LabelHalo != null)
                // Y-1 to centre box around glyph
                g.FillRectangle(style.LabelHalo, origin.X, origin.Y - 1, labelSize.Width, labelSize.Height);

            g.DrawString(def.Label,
                def.IsPrimary ? style.PrimaryLabelFont : style.SecondaryLabelFont,
                def.IsPrimary ? style.PrimaryLabelColor : style.SecondaryLabelColor,
                origin);
        }
        
        /// <summary>
        /// Render web mercator scale distortion meridians 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="graticuleDefs"></param>
        /// <param name="map"></param>
        private void RenderScaleLineLabels(Graphics g, List<GraticuleDef> graticuleDefs, MapViewport map)
        {
            // drop any parallels
            var filteredDefs = graticuleDefs.Where(d => d.IsParallel == false).ToList();
            if (filteredDefs.Count < 2) return;

            // the 4 sides in image space in preferred labelling sequence
            var sides = new[]
            {
                MapDecorationAnchorFlags.Bottom,
                MapDecorationAnchorFlags.Top,
                MapDecorationAnchorFlags.Left,
                MapDecorationAnchorFlags.Right
            };

            // real-world lines, inset from view borders by specified amount
            var intersectionLines = GetIntersectionLines(g, map, sides,
                PcsGraticuleStyle.PrimaryMargin + PcsGraticuleStyle.PrimaryLabelOffset.X);

            var dictIntersections = new Dictionary<MapDecorationAnchorFlags, List<Coordinate>>();
            
            for (var i = 0; i < sides.Length; i++)
            {
               var side = sides[i];

                // check to see if this is one of the borders to be labelled
                if (((int) side & (int) PcsGraticuleStyle.LabelBorders) == 0) continue;

                // perform intersection in world units
                dictIntersections[side] = CalculateScaleLineIntersections(filteredDefs, (LineString) intersectionLines[i]);
            }

            // determine label size in world units
            var label = GetFormattedDistanceLabel(PcsGraticuleStyle.Subdivision);
            var labelSize = g.MeasureString(label, PcsGraticuleStyle.PrimaryLabelFont);
            var labelDist = labelSize.Width * map.PixelWidth;

            // insert labels at every 2nd gap
            var oldTransform = g.Transform;
            using (var matrix = new Matrix())
                foreach (var key in dictIntersections.Keys)
                {
                    var intPoints = dictIntersections[key];
                    if (intPoints.Count < 2) continue;

                    // iterate through intersection points, labelling every 2nd gap
                    for (var j = 0; j < intPoints.Count - 1; j++)
                    {
                        if (j % 2 != 0) continue;

                        matrix.Reset();
                        var ptA = intPoints[j];
                        var ptB = intPoints[j + 1];
                        var dist = ptA.Distance(ptB);
                        if (dist < labelDist * 0.75) continue;

                        var midPtWcs = new Coordinate((ptA.X + ptB.X) * 0.5, (ptA.Y + ptB.Y) * 0.5);
                        var midPtF = map.WorldToImage(midPtWcs, true);

                        if ((key & MapDecorationAnchorFlags.Left) == MapDecorationAnchorFlags.Left ||
                            (key & MapDecorationAnchorFlags.Right) == MapDecorationAnchorFlags.Right)
                            matrix.RotateAt(-90, midPtF);

                        switch (key)
                        {
                            case MapDecorationAnchorFlags.Left:
                            case MapDecorationAnchorFlags.Top:
                                matrix.Translate(labelSize.Width * -0.5f, 0);
                                break;
                            case MapDecorationAnchorFlags.Right:
                            case MapDecorationAnchorFlags.Bottom:
                                matrix.Translate(labelSize.Width * -0.5f, -labelSize.Height);
                                break;
                        }

                        g.Transform = matrix;
                        if (PcsGraticuleStyle.LabelHalo != null)
                            // Y-1 to centre box around glyph
                            g.FillRectangle(PcsGraticuleStyle.LabelHalo, midPtF.X, midPtF.Y - 1, labelSize.Width,
                                labelSize.Height);

                        g.DrawString(label, PcsGraticuleStyle.PrimaryLabelFont, PcsGraticuleStyle.PrimaryLabelColor,
                            midPtF);
                    }
                }

            g.Transform = oldTransform;
        }
        
        /// <summary>
        /// Construct lines inset from image border, then convert to world units to
        /// be used for calculating intersections with graticule lines in world units
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <param name="sides"></param>
        /// <param name="margin"></param>
        /// <returns></returns>
        private MultiLineString GetIntersectionLines(Graphics g, MapViewport map, MapDecorationAnchorFlags[] sides,
            int margin)
        {
            // Image space
            var pts = new []
            {
                new PointF(g.ClipBounds.Left + margin, g.ClipBounds.Bottom), // left side
                new PointF(g.ClipBounds.Left + margin, g.ClipBounds.Top),
                new PointF(g.ClipBounds.Left, g.ClipBounds.Top + margin), // top side
                new PointF(g.ClipBounds.Right, g.ClipBounds.Top + margin),
                new PointF(g.ClipBounds.Right - margin, g.ClipBounds.Top), // right side
                new PointF(g.ClipBounds.Right - margin, g.ClipBounds.Bottom),
                new PointF(g.ClipBounds.Left, g.ClipBounds.Bottom - margin), // bottom side
                new PointF(g.ClipBounds.Right - margin, g.ClipBounds.Bottom - margin)
            };

            // to world coordinates
            var coords = map.ImageToWorld(pts, true);

            // construct world lines
            var lines = new ILineString[4]; 
            for (var i = 0; i < sides.Length; i++)
            {
                var side = sides[i];
                switch (side)
                {
                    case MapDecorationAnchorFlags.Left:
                        lines[i] = new LineString(new[] {coords[0], coords[1]}); // left
                        break;
                    case MapDecorationAnchorFlags.Top:
                        lines[i] = new LineString(new[] {coords[2], coords[3]}); // top
                        break;
                    case MapDecorationAnchorFlags.Right:
                        lines[i] = new LineString(new[] {coords[4], coords[5]}); // right
                        break;
                    case MapDecorationAnchorFlags.Bottom:
                        lines[i] = new LineString(new[] {coords[6], coords[7]}); // bottom
                        break;
                }
            }

            return new MultiLineString(lines);
        }

        /// <summary>
        /// Calculate intersections of given line with all of the provided graticule lines 
        /// </summary>
        /// <param name="filteredDefs"></param>
        /// <param name="line"></param>
        /// <returns>Coordinate list, ordered West to East, South to North</returns>
        private List<Coordinate> CalculateScaleLineIntersections(List<GraticuleDef> filteredDefs, LineString line)
        {
            var coords = new List<Coordinate>();

            foreach (var def in filteredDefs)
            {
                var geom = def.WcsGraticule.Intersection(line);
                if (geom.IsEmpty) continue;

                // typically 0, 1, or sometimes 2
                foreach (var thisCoord in geom.Coordinates)
                {
                    if (coords.Count == 0)
                        coords.Add(thisCoord);
                    else
                        for (var i = 0; i < coords.Count; i++)
                        {
                            var otherC = coords[i];
                            if (!(thisCoord.X < otherC.X + ProjectedTolerance) ||
                                !(thisCoord.Y < otherC.Y + ProjectedTolerance))
                            {
                                if (i != coords.Count - 1) continue;

                                coords.Add(thisCoord);
                                break;
                            }
                            coords.Insert(i, thisCoord);
                            break;
                        }
                }
            }
            return coords;
        }

        /// <summary>
        /// Configure coordinate systems and transformations to be used for constructing graticule lines
        /// </summary>
        /// <param name="mapSrid"></param>
        private void InitCoordinateSystem(int mapSrid)
        {
            _srid = mapSrid;
            _coordinateSystem = null;
            _pcsDomain = null;
            _gcsDomain = null;
            _unProject = null;
            _project = null;
            _oldViewExtents = null;
            _pcsUnitSuffix = string.Empty;

            if (_srid == 0) return;

            _coordinateSystem = Session.Instance.CoordinateSystemServices.GetCoordinateSystem(mapSrid);

            switch (_coordinateSystem)
            {
                case null:
                    return;

                case IGeographicCoordinateSystem _:
                    _gcsDomain = GetCrsDomain(_coordinateSystem);
                    return;

                case IProjectedCoordinateSystem pcs:
                    _pcsDomain = GetCrsDomain(pcs);

                    _gcsDomain = _srid == GeoSpatialMath.WebMercatorSrid
                        // ok - technically it's 85.06, but 85 good for rendering aesthetics
                        ? new Envelope(-180, 180, -85, 85) //new Envelope(-180,180,-85.06,85.06)
                        : GetCrsDomain(pcs.GeographicCoordinateSystem);

                    _pcsUnitSuffix = !string.IsNullOrEmpty(pcs.LinearUnit.Abbreviation) ? pcs.LinearUnit.Abbreviation :
                        !string.IsNullOrEmpty(pcs.LinearUnit.Alias) ? pcs.LinearUnit.Alias : pcs.LinearUnit.Name;

                    if (_pcsUnitSuffix == "metre" || _pcsUnitSuffix == "meter") _pcsUnitSuffix = "m";

                    var transform =
                        Session.Instance.CoordinateSystemServices.CreateTransformation(
                            pcs.GeographicCoordinateSystem, pcs);

                    if (transform == null) return;

                    // Basic Project / Un-project transforms 
                    _project = transform.MathTransform;
                    _unProject = transform.MathTransform.Inverse();
                    return;
            }
        }

        /// <summary>
        /// Coordinate Reference Domain defining appropriate area of use 
        /// </summary>
        /// <param name="crs"></param>
        /// <returns>Crs Domain envelope, or null Envelope if not defined and cannot be derived</returns>
        private Envelope GetCrsDomain(ICoordinateSystem crs)
        {
            if (crs.DefaultEnvelope != null && crs.DefaultEnvelope.Length == 4)
                // supplied PCS constraints (currently not defined on any coordinate systems)
                return new Envelope(
                    new Coordinate(crs.DefaultEnvelope[0], crs.DefaultEnvelope[1]),
                    new Coordinate(crs.DefaultEnvelope[2], crs.DefaultEnvelope[3])
                );

            if (crs.AuthorityCode == GeoSpatialMath.WebMercatorSrid)
                return GeoSpatialMath.WebMercatorEnv;

            if (crs is IGeographicCoordinateSystem)
                return new Envelope(-180, 180, -90, 90);

            return new Envelope();
        }

        /// <summary>
        /// Calculate the envelopes used to constructed projected and geographic graticule lines
        /// and the clipping polygon to trim lines to exact view extent
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <param name="webMercatorScaleLinesActive">Refer to &lt;see cref="PcsGraticuleMode"/&gt;</param>
        private void CalculateMetrics(Graphics g, MapViewport map, bool webMercatorScaleLinesActive)
        {
            _pcsConstrExtents = null;
            _gcsConstrExtents = null;
            _viewClipPolygon = null;

            _oldViewExtents = map.Envelope;
            _oldPcsNumSubdivisions = PcsGraticuleStyle.NumSubdivisions;
            _oldGcsNumSubdivisions = GcsGraticuleStyle.NumSubdivisions;
            _oldPcsGraticuleMode = PcsGraticuleMode;

            _mapScale = map.GetMapScale((int) g.DpiX);

            if (_coordinateSystem is IProjectedCoordinateSystem)
            {
                // pcsConstrExtents is expanded to the next multiple of division 
                _pcsConstrExtents = CalcPcsConstrExtents(_oldViewExtents, webMercatorScaleLinesActive);

                // un-project viewEnv - all 4 corners essential for WebMercator
                try
                {
                    var coords = _unProject.TransformList(
                        new List<Coordinate>()
                        {
                            _oldViewExtents.BottomLeft(),
                            _oldViewExtents.TopLeft(),
                            _oldViewExtents.TopRight(),
                            _oldViewExtents.BottomRight()
                        });

                    _gcsConstrExtents = CalcGcsConstrExtents(
                        new Envelope(
                            coords.Min(c => c.X),
                            coords.Max(c => c.X),
                            coords.Min(c => c.Y),
                            coords.Max(c => c.Y)
                        ));
                }
                catch (Exception ex)
                {
                    _logger.WarnFormat("SRID {0} Env {1} Rotation {2}", _srid, map.Envelope, map.MapTransformRotation);
                    _logger.Error(ex);
                }
            }
            else
                _gcsConstrExtents = CalcGcsConstrExtents(_oldViewExtents);

            // used to trim graticule lines immediately prior to rendering
            _viewClipPolygon = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(map.Center.X - map.Zoom * .5, map.Center.Y - map.MapHeight * .5),
                    new Coordinate(map.Center.X - map.Zoom * .5, map.Center.Y + map.MapHeight * .5),
                    new Coordinate(map.Center.X + map.Zoom * .5, map.Center.Y + map.MapHeight * .5),
                    new Coordinate(map.Center.X + map.Zoom * .5, map.Center.Y - map.MapHeight * .5),
                    new Coordinate(map.Center.X - map.Zoom * .5, map.Center.Y - map.MapHeight * .5)
                }
            ));

            if (!map.MapTransformRotation.Equals(0f))
            {
                var at = AffineTransformation.RotationInstance(
                    Degrees.ToRadians(map.MapTransformRotation), map.Center.X, map.Center.Y);

                _viewClipPolygon = (Polygon) at.Transform(_viewClipPolygon);
            }

            // special handling for Web Mercator to ensure curved meridian lines are correctly trimmed
            if (_srid == GeoSpatialMath.WebMercatorSrid && _viewClipPolygon.Intersects(_webMercatorClipPolygon))
                _viewClipPolygon = (Polygon) _viewClipPolygon.Intersection(_webMercatorClipPolygon);
        }

        /// <summary>
        /// Calculate the primary and secondary intervals based upon the dimensions of <paramref name="viewEnv"/>
        /// </summary>
        /// <param name="viewEnv"></param>
        /// <param name="webMercatorScaleLinesActive">Refer to &lt;see cref="PcsGraticuleMode"/&gt;</param>
        /// <returns>Envelope snapped to multiples of the calculated secondary interval</returns>
        private Envelope CalcPcsConstrExtents(Envelope viewEnv, bool webMercatorScaleLinesActive)
        {
            PcsGraticuleStyle.Division = CalcDivisor(10, PcsPowerRangeMin, PcsPowerRangeMax,
                _pcsPreferredMultiples, viewEnv.MinExtent / 2);
            PcsGraticuleStyle.Subdivision = PcsGraticuleStyle.Division / PcsGraticuleStyle.NumSubdivisions;

            return webMercatorScaleLinesActive
                ? CalcConstExtentsWebMercator(viewEnv, PcsGraticuleStyle.Subdivision)
                : CalcConstrExtentsRectilinear(viewEnv, PcsGraticuleStyle.Subdivision);
        }

        /// <summary>
        /// Calculate the primary and secondary intervals based upon the dimensions of <paramref name="viewEnvDegrees"/>
        /// </summary>
        /// <param name="viewEnvDegrees"></param>
        /// <returns>Envelope snapped to multiples of the calculated secondary interval</returns>
        private Envelope CalcGcsConstrExtents(Envelope viewEnvDegrees)
        {
            // special handling for pseudo pole-to-pole data sets that are often trimmed around 85deg N/S 
            var halfMinExtent = viewEnvDegrees.MinExtent >= 170 ? 90 : viewEnvDegrees.MinExtent / 2;

            GcsGraticuleStyle.Division = CalcDivisor(60, GcsPowerRangeMin, GcsPowerRangeMax,
                _gcsPreferredMultiples, halfMinExtent);

            // this COULD be a nice subdivision (eg 1deg / 3 = 20"), or it could be lousy (eg 2deg / 3 = 40")
            var test1 = GcsGraticuleStyle.Division / GcsGraticuleStyle.NumSubdivisions;
            var test2 = CalcDivisor(60, GcsPowerRangeMin, GcsPowerRangeMax,
                _gcsPreferredMultiples, test1);

            if (Math.Abs(test1 - test2) < GeographicTolerance)
                GcsGraticuleStyle.Subdivision = test2;
            else
            {
                // Treat GcsNumSubdivisions as PREFERRED number of subdivisions and determine actual
                // number of subdivisions to give sensible units in base 60 (ie deg min sec)
                // If GcsNumSubdivisions == 4, then it is better to start at 3
                GcsGraticuleStyle.Subdivision = CalcDivisor(60, GcsPowerRangeMin, GcsPowerRangeMax,
                    _gcsPreferredMultiples, GcsGraticuleStyle.Division /
                                            (GcsGraticuleStyle.NumSubdivisions == 4
                                                ? 3
                                                : GcsGraticuleStyle.NumSubdivisions));
            }

            return CalcConstrExtentsRectilinear(viewEnvDegrees, GcsGraticuleStyle.Subdivision);
        }

        /// <summary>
        /// Calculate largest "nice number" increment less than or equal to <paramref name="maxValue"/>
        /// </summary>
        /// <param name="multiplierBase"></param>
        /// <param name="minPower"></param>
        /// <param name="maxPower"></param>
        /// <param name="preferredMultiples"></param>
        /// <param name="maxValue"></param>
        /// <returns>The secondary interval</returns>
        private double CalcDivisor(int multiplierBase, int minPower, int maxPower,
            double[] preferredMultiples, double maxValue)
        {
            var candidate = 0d;
            for (var y = maxPower; y >= minPower; y--)
            {
                double multiplier = Math.Pow(multiplierBase, y);
                foreach (var niceNumber in preferredMultiples)
                {
                    candidate = niceNumber * multiplier;

                    if (candidate <= maxValue)
                        return candidate;
                }
            }
            return candidate;
        }

        /// <summary>
        /// Returns envelope expanded to nearest intervals of <paramref name="secondaryInterval"/>.
        /// Calculated envelope will usually be slightly larger than <paramref name="viewEnv"/>
        /// </summary>
        /// <param name="viewEnv"></param>
        /// <param name="secondaryInterval"></param>
        /// <returns>Envelope snapped to multiples of the given secondary interval</returns>
        private Envelope CalcConstrExtentsRectilinear(Envelope viewEnv, double secondaryInterval)
        {
            // Y extents expanded to next increment of secondaryInterval
            var minY = Math.Floor(viewEnv.MinY / secondaryInterval) * secondaryInterval;
            if (viewEnv.MinY < 0) minY -= secondaryInterval;

            var maxY = Math.Ceiling(viewEnv.MaxY / secondaryInterval) * secondaryInterval;
            if (viewEnv.MaxY < 0) maxY += secondaryInterval;

            // X extents expanded to next increment of secondaryInterval
            var minX = Math.Floor(viewEnv.MinX / secondaryInterval) * secondaryInterval;
            if (viewEnv.MinX < 0) minX -= secondaryInterval;

            var maxX = Math.Ceiling(viewEnv.MaxX / secondaryInterval) * secondaryInterval;
            if (viewEnv.MaxX < 0) maxX += secondaryInterval;

            return new Envelope(minX, maxX, minY, maxY);
        }

        /// <summary>
        /// Adjust east-west extents of <paramref name="viewEnv"/> to ensure ALL  
        /// scale-distortion meridian lines passing through view extent will plot 
        /// </summary>
        /// <param name="viewEnv"></param>
        /// <param name="secondaryInterval"></param>
        /// <returns>Envelope snapped to multiples of the given secondary interval</returns>
        private Envelope CalcConstExtentsWebMercator(Envelope viewEnv, double secondaryInterval)
        {
            double minX, maxX;

            if (viewEnv.MinX < 0 && viewEnv.MinY <= 0 && viewEnv.MaxY >= 0)
                // when LHS lies to WEST of central meridian AND view spans equator 
                minX = viewEnv.MinX;
            else if (viewEnv.MinX < 0)
                // MIN when LHS lies to WEST of central meridian
                minX = CalcEquatorialX(new Coordinate(viewEnv.MinX,
                    Math.Min(Math.Abs(viewEnv.MinY), Math.Abs(viewEnv.MaxY))));
            else
                // MAX when LHS lies to EAST of central meridian
                minX = CalcEquatorialX(new Coordinate(viewEnv.MinX,
                    Math.Max(Math.Abs(viewEnv.MinY), Math.Abs(viewEnv.MaxY))));

            if (viewEnv.MaxX > 0 && viewEnv.MinY <= 0 && viewEnv.MaxY >= 0)
                // when RHS lies to EAST of central meridian AND view spans equator 
                maxX = viewEnv.MaxX;
            else if (viewEnv.MaxX > 0)
                // MIN when RHS lies to EAST of central meridian
                maxX = CalcEquatorialX(new Coordinate(viewEnv.MaxX,
                    Math.Min(Math.Abs(viewEnv.MinY), Math.Abs(viewEnv.MaxY))));
            else
                // MAX when RHS lies to WEST of central meridian
                maxX = CalcEquatorialX(new Coordinate(viewEnv.MaxX,
                    Math.Max(Math.Abs(viewEnv.MinY), Math.Abs(viewEnv.MaxY))));

            return CalcConstrExtentsRectilinear(new Envelope(minX, maxX, viewEnv.MinY, viewEnv.MaxY),
                secondaryInterval);
        }

        /// <summary>
        /// Returns the equivalent X value of this <paramref name="coord"/> at the equator,
        /// taking into account Web Mercator latitude-dependent scale factor. 
        /// </summary>
        /// <param name="coord"></param>
        /// <returns>Equatorial X ordinate</returns>
        private double CalcEquatorialX(Coordinate coord)
        {
            var scaleFactor = Math.Cosh(Math.Abs(coord.Y) / GeoSpatialMath.WebMercatorRadius);
            return coord.X / scaleFactor;
        }

        /// <summary>
        /// Applies the Web Mercator scale factor to the <param name="x"></param> ordinate based upon the latitude dependent <param name="y"></param> ordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Scale-corrected X ordinate</returns>
        private double CalcScaleCorrectedX(double x, double y)
        {
            var scaleFactor = Math.Cosh(Math.Abs(y) / GeoSpatialMath.WebMercatorRadius);
            return x * scaleFactor;
        }
    }
}
