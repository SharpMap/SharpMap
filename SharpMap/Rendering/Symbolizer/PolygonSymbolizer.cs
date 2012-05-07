using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using SharpMap.Base;
using Point = System.Drawing.Point;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Base class for all geometry symbolizers
    /// </summary>
    public abstract class PolygonSymbolizer : DisposableObject, IPolygonSymbolizer
    {
        protected PolygonSymbolizer()
        {
            Fill = new SolidBrush(Utility.RandomKnownColor());
        }

        protected override void ReleaseManagedResources()
        {
            if (Fill != null)
            {
                Fill.Dispose();
                Fill = null;
            }

            base.ReleaseManagedResources();
        }

        public abstract object Clone();

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
            var mp = geometry as IMultiPolygon;
            if (mp != null)
            {
                for (var i = 0; i < mp.NumGeometries;i++)
                {
                    var poly = (IPolygon) mp[0];
                    OnRenderInternal(map, poly, graphics);
                }
                return;
            }
            OnRenderInternal(map, (IPolygon)geometry, graphics);
        }

        protected abstract void OnRenderInternal(Map mpa, IPolygon polygon, Graphics g);

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

        protected static GraphicsPath PolygonToGraphicsPath(Map map, IPolygon polygon)
        {
            var gp = new GraphicsPath(FillMode.Alternate);
            gp.AddPolygon(polygon.TransformToImage(map));
            return gp;
        }
    }
}