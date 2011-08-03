using System;
using System.Drawing;
using SharpMap.Geometries;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Basic polygon symbolizer
    /// </summary>
    [Serializable]
    public class BasicPolygonSymbolizer : PolygonSymbolizer
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public BasicPolygonSymbolizer()
        {
            Outline = new Pen(Utility.RandomKnownColor(), 1);
        }

        /// <summary>
        /// Gets or sets the pen to render the outline of the polygon
        /// </summary>
        public Pen Outline { get; set; }

        protected override void OnRenderInternal(Map map, Polygon polygon, Graphics g)
        {
            // convert points
            var pts = /*LimitValues(*/polygon.TransformToImage(map)/*)*/;

            // clip
            if (UseClipping)
                pts = VectorRenderer.ClipPolygon(pts, map.Size.Width, map.Size.Height);
            
            // fill the polygon
            if (Fill != null)
                g.FillPolygon(Fill, pts);
            
            // outline the polygon
            if (Outline != null)
                g.DrawPolygon(Outline, pts);
        }
    }

    /// <summary>
    /// Polygon symbolizer class that uses <see cref="LineSymbolizer"/> to symbolize the outline
    /// </summary>
    [Serializable]
    public class PolygonSymbolizerUsingLineSymbolizer : PolygonSymbolizer
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public PolygonSymbolizerUsingLineSymbolizer()
        {
            Outline = new BasicLineSymbolizer();
        }
        /// <summary>
        /// Gets or sets the <see cref="LineSymbolizer"/> to draw the outline of the polygon
        /// </summary>
        public LineSymbolizer Outline { get; set; }

        protected override void OnRenderInternal(Map map, Polygon polygon, Graphics g)
        {
            // convert points
            var pts = /*LimitValues(*/polygon.TransformToImage(map)/*)*/;
            
            // clip
            if (UseClipping)
                pts = VectorRenderer.ClipPolygon(pts, map.Size.Width, map.Size.Height);

            // fill the polygon
            if (Fill != null)
                g.FillPolygon(Fill, pts);

            // outline the polygon
            if (Outline != null)
            {
                Outline.Render(map, polygon.ExteriorRing, g);
                foreach( var ls in polygon.InteriorRings )
                    Outline.Render(map, ls, g);
            }
        }

        public override void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            Outline.Begin(g, map, aproximateNumberOfGeometries);
            base.Begin(g, map, aproximateNumberOfGeometries);
        }

        public override void Symbolize(Graphics g, Map map)
        {
            Outline.Symbolize(g, map);
            base.Symbolize(g, map);
        }

        public override void End(Graphics g, Map map)
        {
            Outline.End(g, map);
            base.End(g, map);
        }
    }
}