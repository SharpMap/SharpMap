using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Geometries;
using Point = System.Drawing.Point;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Base class for all geometry symbolizers
    /// </summary>
    public abstract class PolygonSymbolizer : IPolygonSymbolizer
    {
        protected PolygonSymbolizer()
        {
            Fill = new SolidBrush(Utility.RandomKnownColor());
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

        public void Render(Map map, IPolygonal geometry, Graphics graphics)
        {
            var mp = geometry as MultiPolygon;
            if (mp != null)
            {
                foreach (Polygon poly in mp.Polygons)
                    OnRenderInternal(map, poly, graphics);
                return;
            }
            OnRenderInternal(map, (Polygon)geometry, graphics);
        }

        protected abstract void OnRenderInternal(Map mpa, Polygon polygon, Graphics g);

        private Point _renderOrigin;
        public virtual void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            _renderOrigin = g.RenderingOrigin;
            g.RenderingOrigin = RenderOrigin;
        }

        public virtual void Symbolize(Graphics g, Map map)
        {
        }

        public virtual void End(Graphics g, Map map)
        {
            g.RenderingOrigin = _renderOrigin;
        }

        protected static GraphicsPath PolygonToGraphicsPath(Map map, Polygon polygon)
        {
            var gp = new GraphicsPath(FillMode.Alternate);
            gp.AddPolygon(polygon.TransformToImage(map));
            return gp;
        }
    }
}