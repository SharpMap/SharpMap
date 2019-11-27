using System;
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using SharpMap.Utilities;

namespace SharpMap
{
    /// <summary>
    /// A <see cref="SharpMap.Map"/> utility class, that encapsulates all data required for rendering.
    /// </summary>
    /// <remarks>This is a value class</remarks>
    public class MapViewport
    {
        private readonly Envelope _envelope;
        private readonly Coordinate _center;
        private Coordinate _centerOfInterest;

        private readonly float[] _mapTransformElements;
        private readonly float[] _mapTransformInvertedElements;

        private readonly double[] _worldToMapElements;
        private readonly double[] _worldToMapElementsCareAboutTransform;

        private double _mapScale;
        private int _lastDpi;

        private readonly object _lockMapScale = new object();

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="mapId">The id of the map</param>
        /// <param name="srid">The spatial reference</param>
        /// <param name="zoom">current map zoom</param>
        /// <param name="mapHeight">map height</param>
        /// <param name="env">The envelope containing the viewport</param>
        /// <param name="size">The size of the viewport</param>
        /// <param name="pixelAspectRatio">A ratio between width and height</param>
        /// <param name="mapTransform">An affine map transform matrix</param>
        /// <param name="mapTransformInverted">The affine map transformation that inverts <paramref name="mapTransform"/></param>
        /// <param name="mapTransformRotation">The rotation in degrees applied by <paramref name="mapTransform"/></param>
        public MapViewport(Guid mapId, int srid, double zoom, double mapHeight, Envelope env, Size size, double pixelAspectRatio, 
            System.Drawing.Drawing2D.Matrix mapTransform, System.Drawing.Drawing2D.Matrix mapTransformInverted,
            float mapTransformRotation)
        {
            ID = mapId;
            SRID = srid;

            Zoom = zoom;
            MapHeight = mapHeight;

            _envelope = env.Copy();
            Size = size;
            _center = env.Centre;

            PixelAspectRatio = pixelAspectRatio;
            PixelWidth = Zoom / size.Width; 
            PixelHeight = PixelWidth * pixelAspectRatio;

            _mapTransformElements = mapTransform.Elements;
            _mapTransformInvertedElements = mapTransformInverted.Elements;
            MapTransformRotation = mapTransformRotation;

            // pre-calculated for use when MapTransformRotation == 0
            Left = Center.X - Zoom * 0.5;
            Top = Center.Y + mapHeight * 0.5;

            // pre-defined for use when MapTransformation != 0
            if (!mapTransformRotation.Equals(0f))
            {
                _worldToMapElements = Transform.WorldToMapMatrix(
                    Center, PixelWidth, PixelHeight, MapTransformRotation, Size, false).MatrixEntries;

                _worldToMapElementsCareAboutTransform =Transform.WorldToMapMatrix(
                    Center, PixelWidth, PixelHeight, MapTransformRotation, Size, true).MatrixEntries;
            }
        }

        /// <summary>
        /// Creates an instance of this class based on the provided map
        /// </summary>
        /// <param name="map">The Map</param>
        public MapViewport(Map map)
            : this(map.ID, map.SRID, map.Zoom, map.MapHeight, map.Envelope, map.Size, map.PixelAspectRatio,
                map.MapTransform, map.MapTransformInverted, map.MapTransformRotation)
        {
            CenterOfInterest = map.CenterOfInterest;
        }

        /// <summary>
        /// Gets a value indicating the which <see cref="Map"/> this viewport belongs to.
        /// </summary>
        public Guid ID { get; }

        /// <summary>
        /// Gets a value indicating the spatial reference id of the map
        /// </summary>
        public int SRID { get; /*set;*/ }

        /// <summary>
        /// Gets a value indicating the size of the map
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// <para>Gets the rectilinear extents of the map based on the current <see cref="Zoom"/>,
        /// <see cref="Center"/>, map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/></para>
        /// <para>If a <see cref="MapTransform"/> is applied, the envelope CONTAINING the rotated view
        /// will be returned (used by layers to spatially select data) and the aspect ratio will NOT be the
        /// same as map <see cref="Size"/>. If aspect ratio is important then refer to <see cref="Zoom"/>
        /// and <see cref="MapHeight"/></para> 
        /// </summary>
        public Envelope Envelope => _envelope.Copy();

        /// <summary>
        /// Gets a value indicating the transformation that has to be applied when
        /// rendering the map
        /// </summary>
        public System.Drawing.Drawing2D.Matrix MapTransform
        {
            get
            {
                return new System.Drawing.Drawing2D.Matrix(
                    _mapTransformElements[0],
                    _mapTransformElements[1],
                    _mapTransformElements[2],
                    _mapTransformElements[3],
                    _mapTransformElements[4],
                    _mapTransformElements[5]
                );
            }
        }

        /// <summary>
        /// Gets a value indicating the inverse transformation that is applied when
        /// rendering the map
        /// </summary>
        public System.Drawing.Drawing2D.Matrix MapTransformInverted
        {
            get
            {
                return new System.Drawing.Drawing2D.Matrix(
                    _mapTransformInvertedElements[0],
                    _mapTransformInvertedElements[1],
                    _mapTransformInvertedElements[2],
                    _mapTransformInvertedElements[3],
                    _mapTransformInvertedElements[4],
                    _mapTransformInvertedElements[5]
                );
            }
        }

        /// <summary>
        /// Map rotation in degrees (defined by <see cref="MapTransform"/>)
        /// </summary>
        public float MapTransformRotation { get; }

        /// <summary>
        /// Cached affine transformation used to transform world coordinates from apparent rotated coordinate frame (ie MapTransformRotation != 0)
        /// to image space. Unlike MapTransform, this matrix defines a complete transformation from World to Image taking into account MapRotation.
        /// 2 variants are available, depending on whether or not map rotation has already been applied.
        /// </summary>
        internal AffineTransformation WorldToMapTransform(bool careAboutTransform)
        {
            if (careAboutTransform)
                return new AffineTransformation(
                    _worldToMapElementsCareAboutTransform[0],
                    _worldToMapElementsCareAboutTransform[1],
                    _worldToMapElementsCareAboutTransform[2],
                    _worldToMapElementsCareAboutTransform[3],
                    _worldToMapElementsCareAboutTransform[4],
                    _worldToMapElementsCareAboutTransform[5]
                );
            else
                return new AffineTransformation(
                    _worldToMapElements[0],
                    _worldToMapElements[1],
                    _worldToMapElements[2],
                    _worldToMapElements[3],
                    _worldToMapElements[4],
                    _worldToMapElements[5]
                );
        }
        
        /// <summary>
        /// Gets a value indicating the center of the map viewport
        /// </summary>
        public Coordinate Center => _center.Copy();

        /// <summary>
        /// Gets a value indicating the center of the map viewport
        /// </summary>
        public Coordinate CenterOfInterest
        {
            get => _centerOfInterest?.Copy() ?? Center;
            set => _centerOfInterest = value;
        }

        /// <summary>
        /// Gets a value indicating the zoom of the map viewport
        /// </summary>
        /// <remarks>This value is identical to <see cref="MapWidth"/></remarks>
        public double Zoom { get; /*set;*/ }

        /// <summary>
        /// Gets a value indicating the height of the map viewport in world units
        /// </summary>
        public double MapHeight { get; }

        /// <summary>
        /// Gets a value indicating the width of the map viewport in world units
        /// </summary>
        /// <remarks>This value is equal to <see cref="Zoom"/></remarks>
        private double MapWidth => Zoom;

        /// <summary>
        /// Applicable to non-rotated views only, returning the minimum X value of the map viewport in world units
        /// </summary>
        public double Left { get; }
        
        /// <summary>
        /// Applicable to non-rotated views only, returning the maximum Y value of the map viewport in world units
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// Gets or sets the aspect-ratio of the pixel scales. A value less than 
        /// 1 will make the map stretch upwards, and larger than 1 will make it smaller.
        /// </summary>
        /// <exception cref="ArgumentException">Throws an argument exception when value is 0 or less.</exception>
        public double PixelAspectRatio { get; }

        /// <summary>
        /// Returns the width of a pixel in world coordinate units.
        /// </summary>
        public double PixelWidth { get; }

        /// <summary>
        /// Returns the height of a pixel in world coordinate units.
        /// </summary>
        public double PixelHeight { get; }

        /// <summary>
        /// Function to compute the denominator of the viewport's scale when using a given <paramref name="dpi"/> resolution.
        /// </summary>
        /// <param name="dpi">The resolution</param>
        /// <returns>The scale's denominator</returns>
        public double GetMapScale(int dpi)
        {
            if (_lastDpi != dpi)
            {
                lock (_lockMapScale)
                    if (_lastDpi != dpi)
                    {
                        _mapScale = ScaleCalculations.CalculateScaleNonLatLong(Zoom, Size.Width, 1, dpi);
                        _lastDpi = dpi;
                    }
            }

            return _mapScale;
        }

        /// <summary>
        /// Converts an array of world coordinates to image coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="coordinates">Coordinate array in world coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. True for typical coordinate calcs,
        /// False when rendering to image as the Graphics object has already applied the MapTransform</param>
        /// <returns>PointF array in image coordinates</returns>
        public PointF[] WorldToImage(Coordinate[] coordinates, bool careAboutMapTransform = false)
        {
            // see WorldToImage discussion in Map.cs. This is a slightly shortened form using cached values.
            if (MapTransformRotation.Equals(0f))
                return Transform.WorldToMap(coordinates, Left, Top, PixelWidth, PixelHeight);

            var matrix = WorldToMapTransform(careAboutMapTransform);
            return Transform.WorldToMap(coordinates, matrix);
        }
        
        /// <summary>
        /// Converts a point in world coordinates to image coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. When rendering to image,
        /// the Graphics object has usually applied MapTransform</param>
        /// <returns>PointF in image coordinates</returns>
        public PointF WorldToImage(Coordinate p, bool careAboutMapTransform = false)
        {
            var points = WorldToImage(new Coordinate[] {p}, careAboutMapTransform);
            return points[0];
        }

        /// <summary>
        /// Converts a point array from image coordinates to world coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="points">Point array in image coordinates. Note: if you wish to preserve the input values then
        /// you must clone the point array as it will be modified if a MapTransform is applied</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. </param>
        /// <returns>Point array in world coordinates</returns>
        public Coordinate[] ImageToWorld(PointF[] points, bool careAboutMapTransform = false)
        {
            if (careAboutMapTransform && !MapTransformRotation.Equals(0f))
                using (var transformInv = MapTransformInverted)
                    transformInv.TransformPoints(points);

            return Transform.MapToWorld(points, Center, Zoom, MapHeight, PixelWidth, PixelHeight);
        }

        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="p">Point in image coordinates. Note: if you wish to preserve the input value then
        /// you must clone the point as it will be modified if a MapTransform is applied</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. </param>
        /// <returns>Point in world coordinates</returns>
        public Coordinate ImageToWorld(PointF p, bool careAboutMapTransform = false)
        {
            var pts = ImageToWorld(new PointF[] {p}, careAboutMapTransform);
            return pts[0];
        }

        /// <summary>
        /// Creates a map viewport from a given map
        /// </summary>
        /// <param name="map">The map</param>
        /// <returns></returns>
        public static implicit operator MapViewport(Map map)
        {
            return new MapViewport(map);
        }
    }
}
