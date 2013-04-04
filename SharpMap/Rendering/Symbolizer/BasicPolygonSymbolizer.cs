using System;
using System.Drawing;
using GeoAPI.Geometries;

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
        /// Method to release all managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            CheckDisposed();

            if (Outline != null)
            {
                Outline.Dispose();
                Outline = null;
            }

            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Gets or sets the pen to render the outline of the polygon
        /// </summary>
        public Pen Outline { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object Clone()
        {
            return new BasicPolygonSymbolizer
                       {
                           Fill = (Brush) Fill.Clone(),
                           Outline = (Pen) Outline.Clone(),
                           RenderOrigin = RenderOrigin,
                           UseClipping = UseClipping,
                       };
        }

        /// <summary>
        /// Method that does the actual rendering of geometries
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="polygon">The feature</param>
        /// <param name="g">The graphics object</param>
        protected override void OnRenderInternal(Map map, IPolygon polygon, Graphics g)
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
        /// Method that releases all managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            CheckDisposed();

            if (Outline != null)
            {
                Outline.Dispose();
                Outline = null;
            }

            base.ReleaseManagedResources();
        }
        /// <summary>
        /// Gets or sets the <see cref="LineSymbolizer"/> to draw the outline of the polygon
        /// </summary>
        public LineSymbolizer Outline { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object Clone()
        {
            return new PolygonSymbolizerUsingLineSymbolizer
                       {
                           Fill = (Brush) Fill.Clone(),
                           Outline = (LineSymbolizer) Outline.Clone(),
                           RenderOrigin = RenderOrigin,
                           UseClipping = UseClipping
                       };
        }

        /// <summary>
        /// Method to perform actual rendering 
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="polygon">The polygon to render</param>
        /// <param name="g">The graphics object to use</param>
        protected override void OnRenderInternal(Map map, IPolygon polygon, Graphics g)
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

        /// <summary>
        /// Method to perform preparatory work for symbilizing.
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        /// <param name="aproximateNumberOfGeometries">An approximate number of geometries to symbolize</param>
        public override void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            Outline.Begin(g, map, aproximateNumberOfGeometries);
            base.Begin(g, map, aproximateNumberOfGeometries);
        }

        /// <summary>
        /// Method to perform symbolization
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        public override void Symbolize(Graphics g, Map map)
        {
            Outline.Symbolize(g, map);
            base.Symbolize(g, map);
        }

        /// <summary>
        /// Method to restore the state of the graphics object and do cleanup work.
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        public override void End(Graphics g, Map map)
        {
            Outline.End(g, map);
            base.End(g, map);
        }
    }
}