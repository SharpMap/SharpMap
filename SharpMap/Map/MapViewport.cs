using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Threading;
using GeoAPI.Geometries;
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

        //private readonly Matrix _mapTransform;
        //private readonly Matrix _mapTransformInverted;

        private readonly float[] _mapTransformElements;
        private readonly float[] _mapTransformInvertedElements;

        private readonly double _left;
        private readonly double _top;
        private double _mapScale;
        private int _lastDpi;

        // TODO: Reconsider computing map scale
        private readonly object _lockMapScale = new object();

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="mapId">The id of the map</param>
        /// <param name="srid">The spatial reference</param>
        /// <param name="env">The envelope of the viewport</param>
        /// <param name="size">The size of the viewport</param>
        /// <param name="pixelAspectRatio">A ratio between width and height</param>
        /// <param name="mapTransform">An affine map transform matrix</param>
        /// <param name="mapTransformInverted">The affine map transformation that inverts <paramref name="mapTransform"/></param>
        public MapViewport(Guid mapId, int srid, Envelope env, Size size, double pixelAspectRatio, Matrix mapTransform, Matrix mapTransformInverted)
        {
            ID = mapId;
            SRID = srid;

            _envelope = env.Copy();
            Size = size;
            _center = env.Centre;

            PixelAspectRatio = pixelAspectRatio;
            PixelWidth = /*PixelSize = */env.Width/size.Width;
            PixelHeight = PixelWidth*pixelAspectRatio;

            Zoom = env.Width;
            MapHeight = Zoom * pixelAspectRatio;

            // already cloned
            //_mapTransform = mapTransform;
            //_mapTransformInverted = mapTransformInverted;

            _mapTransformElements = mapTransform.Elements;
            _mapTransformInvertedElements = mapTransformInverted.Elements;

            double height = (Zoom * Size.Height) / Size.Width;
            _left = Center.X - Zoom * 0.5;
            _top = Center.Y + height * 0.5 * PixelAspectRatio;
        }

        /// <summary>
        /// Creates an instance of this class based on the provided map
        /// </summary>
        /// <param name="map">The Map</param>
        public MapViewport(Map map)
            :this(map.ID, map.SRID, map.Envelope, map.Size, map.PixelAspectRatio, map.MapTransform, map.MapTransformInverted)
        {
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
        /// Gets a value indicating the area covered by the map (in world units)
        /// </summary>
        public Envelope Envelope
        {
            get { return _envelope.Copy(); }
        }

        /// <summary>
        /// Gets a value indicating the transformation that has to be applied when
        /// rendering the map
        /// </summary>
        public Matrix MapTransform
        {
            get
            {
                //lock (_mapTransform)
                //{
                //    return _mapTransform.Clone();
                //}
                return new Matrix(
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
        public Matrix MapTransformInverted
        {
            get
            {
                return new Matrix(
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
        /// Gets a value indicating the center of the map viewport
        /// </summary>
        public Coordinate Center
        {
            get { return _center.Copy(); }
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
        public double MapWidth { get { return Zoom; } }

        /// <summary>
        /// Gets or sets the aspect-ratio of the pixel scales. A value less than 
        /// 1 will make the map stretch upwards, and larger than 1 will make it smaller.
        /// </summary>
        /// <exception cref="ArgumentException">Throws an argument exception when value is 0 or less.</exception>
        public double PixelAspectRatio { get; }

        ///// <summary>
        ///// Get Returns the size of a pixel in world coordinate units
        ///// </summary>
        //[Obsolete("Use PixelWidth or PixelHeight")]
        //public double PixelSize { get; private set; }

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
            // Why lock?
            lock (_lockMapScale)
            {
                if (_lastDpi != dpi)
                {
                    _mapScale = ScaleCalculations.CalculateScaleNonLatLong(Envelope.Width, Size.Width, 1, dpi);
                    _lastDpi = dpi;
                }
                return _mapScale;
            }
        }

        /// <summary>
        /// Converts a point from world coordinates to image coordinates based on the current
        /// <see cref="Zoom"/>, <see cref="Center"/> and <see cref="Size"/>.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether MapTransform should be taken into account</param>
        /// <returns>Point in image coordinates</returns>
        public PointF WorldToImage(Coordinate p, bool careAboutMapTransform)
        {
            var pTmp = WorldToImage(p);
            if (!careAboutMapTransform)
                return pTmp;

            var pts = new[] { pTmp };
            //Monitor.Enter(_mapTransform);
            using (var mapTransform = MapTransform)
            {
                if (mapTransform.IsIdentity == false)
                {
                    mapTransform.TransformPoints(pts);
                }
            }
            //Monitor.Exit(_mapTransform);

            return pts[0];
        }

        /// <summary>
        /// Converts a point from world coordinates to image coordinates based on the current
        /// <see cref="Zoom"/>, <see cref="Center"/> and <see cref="Size"/>.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <returns>Point in image coordinates</returns>
        public PointF WorldToImage(Coordinate p)
        {
            if (p.IsEmpty())
                return PointF.Empty;

            double x = (p.X - _left) / PixelWidth;
            if (double.IsNaN(x))
                return PointF.Empty;

            double y = (_top - p.Y) / PixelHeight;
            if (double.IsNaN(y))
                return PointF.Empty;

            return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current
        /// <see cref="Zoom"/>, <see cref="Center"/> and <see cref="Size"/>.
        /// </summary>
        /// <param name="p">Point in image coordinates</param>
        /// <returns>Point in world coordinates</returns>
        public Coordinate ImageToWorld(PointF p)
        {
            return ImageToWorld(p, false);
        }
        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current
        /// <see cref="Zoom"/>, <see cref="Center"/> and <see cref="Size"/>.
        /// </summary>
        /// <param name="p">Point in image coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether MapTransform should be taken into account</param>
        /// <returns>Point in world coordinates</returns>
        public Coordinate ImageToWorld(PointF p, bool careAboutMapTransform)
        {
            var pts = new[] { p };
            //Monitor.Enter(_mapTransformInverted);
            using (var mapTransformInverted = MapTransformInverted)
            {
                if (mapTransformInverted.IsIdentity == false)
                {
                    mapTransformInverted.TransformPoints(pts);
                }
            }
            //Monitor.Exit(_mapTransformInverted);

            return Transform.MapToWorld(pts[0], this);
        }

        /// <summary>
        /// Creates a map viewport from a given map
        /// </summary>
        /// <param name="map">The map</param>
        /// <returns></returns>
        public static implicit operator MapViewport(Map map)
        {
            return new MapViewport(map);
            //return new MapViewport(map.ID, map.SRID, map.Envelope, map.Size, map.PixelAspectRatio, 
            //                       map.MapTransform, map.MapTransformInverted);
        }

    }
}
