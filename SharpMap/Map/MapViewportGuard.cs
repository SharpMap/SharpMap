using System;
using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap
{
    /// <summary>
    /// Utility class that checks Viewport min/max Zoom and constraint
    /// </summary>
    [Serializable]
    public class MapViewPortGuard
    {
        private double _minimumZoom;
        private double _maximumZoom;
        private Envelope _maximumExtents;
        private double _pixelAspectRatio;
        const double MinMinZoomValue = 2d * Double.Epsilon;

        /// <summary>
        /// Gets or sets a value indicating the minimum zoom level.
        /// </summary>
        public double MinimumZoom
        {
            get { return _minimumZoom; }
            set
            {
                if (value < MinMinZoomValue)
                    value = MinMinZoomValue;
                _minimumZoom = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum zoom level.
        /// </summary>
        public double MaximumZoom
        {
            get { return _maximumZoom; }
            set
            {
                if (value < _minimumZoom)
                    value = _minimumZoom;
                _maximumZoom = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum extents
        /// </summary>
        public Envelope MaximumExtents
        {
            get { return _maximumExtents ?? (_maximumExtents = new Envelope()); }
            set { _maximumExtents = value; }
        }

        /// <summary>
        /// Gets or sets the size of the Map in device units (Pixel)
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the aspect-ratio of the pixel scales. A value less than 
        /// 1 will make the map streach upwards, and larger than 1 will make it smaller.
        /// </summary>
        /// <exception cref="ArgumentException">Throws an argument exception when value is 0 or less.</exception>
        public double PixelAspectRatio
        {
            get { return _pixelAspectRatio; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Invalid Pixel Aspect Ratio");
                _pixelAspectRatio = value;
            }
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        internal MapViewPortGuard(Size size, double minZoom, double maxZoom)
        {
            Size = size;
            MinimumZoom = minZoom;
            MaximumZoom = maxZoom;
            PixelAspectRatio = 1d;
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="Map.MaximumExtents"/> should be enforced or not.
        /// </summary>
        public bool EnforceMaximumExtents { get; set; }

        /// <summary>
        /// Verifies the zoom level and center of the map
        /// </summary>
        /// <param name="zoom">The zoom level to test</param>
        /// <param name="center">The center of the map. This coordinate might change so you <b>must</b> provide a copy if you want to preserve the old value</param>
        /// <returns>The zoom level, might have changed</returns>
        public double VerifyZoom(double zoom, Coordinate center)
        {
            // Zoom within valid region
            if (zoom < _minimumZoom)
                zoom = _minimumZoom;
            else if (zoom > _maximumZoom)
                zoom = _maximumZoom;

            if (EnforceMaximumExtents)
            {
                if (zoom > _maximumExtents.Width)
                    zoom = _maximumExtents.Width;
                zoom = VerifyValidViewport(zoom, center);
            }

            return zoom;
        }

        /// <summary>
        /// Verifies the valid viewport, makes adjustments if required
        /// </summary>
        /// <param name="zoom">The current zoom</param>
        /// <param name="center">The </param>
        /// <returns>The verified zoom level</returns>
        private double VerifyValidViewport(double zoom, Coordinate center)
        {
            var maxExtents = MaximumExtents ?? new Envelope();
            if (maxExtents.IsNull)
                return zoom;

            var halfWidth = 0.5d * zoom;
            var halfHeight = halfWidth * PixelAspectRatio * ((double)Size.Height / Size.Width);
            if (2 * halfHeight > _maximumExtents.Height)
            { 
                halfHeight = 0.5d * _maximumExtents.Height;
                halfWidth = halfHeight / (_pixelAspectRatio * ((double)Size.Height / Size.Width));
                zoom = 2 * halfWidth;
            }

            var testEnvelope = new Envelope(center.X - halfWidth, center.X + halfWidth,
                                            center.Y - halfHeight, center.Y + halfHeight);

            if (maxExtents.Contains(testEnvelope))
                return zoom;

            var dx = testEnvelope.MinX < maxExtents.MinX
                            ? maxExtents.MinX - testEnvelope.MinX
                            : testEnvelope.MaxX > maxExtents.MaxX 
                                ? maxExtents.MaxX - testEnvelope.MaxX 
                                : 0;

            var dy = testEnvelope.MinY < maxExtents.MinY
                            ? maxExtents.MinY - testEnvelope.MinY
                            : testEnvelope.MaxY > maxExtents.MaxY 
                                ? maxExtents.MaxY - testEnvelope.MaxY 
                                : 0;

            center.X += dx;
            center.Y += dy;

            return zoom;
        }
    }
}