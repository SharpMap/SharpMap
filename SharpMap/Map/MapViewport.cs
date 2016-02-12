using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using SharpMap.Utilities;

namespace SharpMap
{
    /// <summary>
    /// A <see cref="SharpMap.Map"/> utility class, that encapuslates all data required for rendering.
    /// </summary>
    /// <remarks>This is a value class</remarks>
    public class MapViewport
    {
        private readonly Envelope _envelope;
        private readonly Matrix _mapTransform;
        private readonly Matrix _mapTransformInverted;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="mapId">The id of the map</param>
        /// <param name="srid">The spatial reference</param>
        /// <param name="env">The envelope of the viewport</param>
        /// <param name="size">The size of the viewport</param>
        /// <param name="pixelAspectRatio">A ratio between width and height</param>
        /// <param name="mapTransform">An affine map transform matrix</param>
        public MapViewport(Guid mapId, int srid, Envelope env, Size size, double pixelAspectRatio, Matrix mapTransform, Matrix mapTransformInverted)
        {
            ID = mapId;
            SRID = srid;

            _envelope = new Envelope(env);
            Size = size;
            Center = env.Centre;

            PixelAspectRatio = pixelAspectRatio;
            PixelWidth = PixelSize = env.Width/size.Width;
            PixelHeight = PixelWidth*pixelAspectRatio;

            Zoom = env.Width;
            MapHeight = Zoom*pixelAspectRatio;

            _mapTransform = mapTransform.Clone();
            _mapTransformInverted = mapTransformInverted.Clone();
        }

        /// <summary>
        /// Gets a value indicating the which <see cref="Map"/> this viewport belongs to.
        /// </summary>
        public Guid ID { get; private set; }

        public int SRID { get; set; }

        public Size Size { get; private set; }

        public Envelope Envelope
        {
            get { return new Envelope(_envelope); }
        }

        public Matrix MapTransform
        {
            get { return _mapTransform.Clone(); }
        }

        public Coordinate Center { get; private set; }


        public double Zoom { get; set; }
        public double MapHeight { get; private set; }
        public double MapWidth { get { return Zoom; } }

        /// <summary>
        /// Gets or sets the aspect-ratio of the pixel scales. A value less than 
        /// 1 will make the map streach upwards, and larger than 1 will make it smaller.
        /// </summary>
        /// <exception cref="ArgumentException">Throws an argument exception when value is 0 or less.</exception>
        public double PixelAspectRatio
        {
            get; private set;
        }

        /// <summary>
        /// Get Returns the size of a pixel in world coordinate units
        /// </summary>
        [Obsolete("Use PixelWidth or PixelHeight")]
        public double PixelSize { get; private set; }

        /// <summary>
        /// Returns the width of a pixel in world coordinate units.
        /// </summary>
        public double PixelWidth { get; private set; }

        /// <summary>
        /// Returns the height of a pixel in world coordinate units.
        /// </summary>
        public double PixelHeight { get; private set; }

        public double GetMapScale(int dpi)
        {
            return ScaleCalculations.CalculateScaleNonLatLong(Envelope.Width, Size.Width, 1, dpi);
        }

        /// <summary>
        /// Converts a point from world coordinates to image coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether MapTransform should be taken into account</param>
        /// <returns>Point in image coordinates</returns>
        public PointF WorldToImage(Coordinate p, bool careAboutMapTransform)
        {
            var pTmp = Transform.WorldtoMap(p, this);
            lock (_mapTransform)
            {
                if (careAboutMapTransform && !_mapTransform.IsIdentity)
                {
                    var pts = new[] { pTmp };
                    _mapTransform.TransformPoints(pts);
                    pTmp = pts[0];
                }
            }
            return pTmp;
        }

        /// <summary>
        /// Converts a point from world coordinates to image coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <returns>Point in image coordinates</returns>
        public PointF WorldToImage(Coordinate p)
        {
            return WorldToImage(p, false);
        }

        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in image coordinates</param>
        /// <returns>Point in world coordinates</returns>
        public Coordinate ImageToWorld(PointF p)
        {
            return ImageToWorld(p, false);
        }
        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in image coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether MapTransform should be taken into account</param>
        /// <returns>Point in world coordinates</returns>
        public Coordinate ImageToWorld(PointF p, bool careAboutMapTransform)
        {
            lock (_mapTransform)
            {
                if (careAboutMapTransform && !_mapTransformInverted.IsIdentity)
                {
                    var pts = new[] { p };
                    _mapTransformInverted.TransformPoints(pts);
                    p = pts[0];
                }
            }
            return Transform.MapToWorld(p, this);
        }
        /// <summary>
        /// Creates a map viewport from a given map
        /// </summary>
        /// <param name="map">The map</param>
        /// <returns></returns>
        public static implicit operator MapViewport(Map map)
        {
            return new MapViewport(map.ID, map.SRID, map.Envelope, map.Size, map.PixelAspectRatio, 
                                   map.MapTransform, map.MapTransformInverted);
        }

    }
}