using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Base class for all geometry symbolizers
    /// </summary>
    public abstract class PolygonSymbolizer : BaseSymbolizer, IPolygonSymbolizer
    {
#if !NETSTANDARD2_0
        /// <summary>
        /// Creates an instance of his class. <see cref="Fill"/> is set to a <see cref="SolidBrush"/> with a random <see cref="System.Drawing.KnownColor"/>.
        /// </summary>
#else
        /// <summary>
        /// Creates an instance of his class. <see cref="Fill"/> is set to a <see cref="SolidBrush"/> with a random <see cref="SharpMap.Drawing.KnownColor"/>.
        /// </summary>
#endif
        protected PolygonSymbolizer()
        {
            Fill = new SolidBrush(Utility.RandomKnownColor());
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            CheckDisposed();

            if (Fill != null)
            {
                Fill.Dispose();
                Fill = null;
            }

            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Gets or sets the brush to fill the polygon
        /// </summary>
        public Brush Fill { get; set; }

        /// <summary>
        /// The render origin for brushes (Texture, Gradient, ...)
        /// </summary>
        public Point RenderOrigin { get; set; }

        /// <summary>
        /// Gets or sets if polygons should be clipped or not.
        /// </summary>
        public bool UseClipping { get; set; }

        /// <summary>
        /// Function to render the geometry
        /// </summary>
        /// <param name="map">The map object, mainly needed for transformation purposes.</param>
        /// <param name="geometry">The geometry to symbolize.</param>
        /// <param name="graphics">The graphics object to use.</param>
        public void Render(MapViewport map, IPolygonal geometry, Graphics graphics)
        {
            var mp = geometry as MultiPolygon;
            if (mp != null)
            {
                for (var i = 0; i < mp.NumGeometries; i++)
                {
                    var poly = (Polygon)mp[i];
                    OnRenderInternal(map, poly, graphics);
                }
                return;
            }
            OnRenderInternal(map, (Polygon)geometry, graphics);
        }

        /// <summary>
        /// Method to perform actual rendering 
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="polygon">The polygon to render</param>
        /// <param name="g">The graphics object to use</param>
        protected abstract void OnRenderInternal(MapViewport map, Polygon polygon, Graphics g);

        private Point _renderOrigin;

        /// <summary>
        /// Method to perform preparatory work for symbilizing.
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        /// <param name="aproximateNumberOfGeometries">An approximate number of geometries to symbolize</param>
        public override void Begin(Graphics g, MapViewport map, int aproximateNumberOfGeometries)
        {
            _renderOrigin = g.RenderingOrigin;
            g.RenderingOrigin = RenderOrigin;
            base.Begin(g, map, aproximateNumberOfGeometries);
        }

        /// <summary>
        /// Method to restore the state of the graphics object and do cleanup work.
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        public override void End(Graphics g, MapViewport map)
        {
            g.RenderingOrigin = _renderOrigin;
            base.End(g, map);
        }

        /// <summary>
        /// Conversion function for a polygon to a graphics path
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="polygon">The polygon</param>
        /// <returns>A graphics path</returns>
        protected static GraphicsPath PolygonToGraphicsPath(Map map, Polygon polygon)
        {
            return polygon.TransformToImage(map);
        }
    }
}
