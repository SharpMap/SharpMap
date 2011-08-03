using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Rendering.Thematics;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Warps a Pattern to a linestrings
    /// </summary>
    [Serializable]
    public class WarpedLineSymbolizer : LineSymbolizer
    {
        private GraphicsPath _pattern;

        private float _interval;

        /// <summary>
        /// Pattern to warp to
        /// </summary>
        public GraphicsPath Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }

        /// <summary>
        /// The interval between each pattern object
        /// </summary>
        public float Interval
        {
            get { return _interval; } 
            set { _interval = value; }
        }

        /// <summary>
        /// Creates a pattern symbolizing a linestring like this <c>&gt;&gt;&gt;&gt;&gt;</c>
        /// </summary>
        /// <param name="x">The length of the peak</param>
        /// <param name="y">the offset left and right from the original line</param>
        /// <returns>The pattern</returns>
        public static GraphicsPath GetGreaterSeries(float x, float y)
        {
            var gp = new GraphicsPath();
            gp.AddLine(new PointF(0.5f*x, y), new PointF(1.5f * x, 0f));
            gp.CloseFigure();
            gp.AddLine(new PointF(1.5f * x, 0f), new PointF(0.5f*x, -y));
            gp.CloseFigure();
            return gp;
        }

        /// <summary>
        /// Create a "zigzag" pattern, sort of like a rotated by 90 degree Z
        /// </summary>
        /// <param name="x">The width of a step op the linstring axis.</param>
        /// <param name="y">The offset left and right from the axis.</param>
        /// <returns>The pattern</returns>
        public static GraphicsPath GetZigZag(float x, float y)
        {
            var gp = new GraphicsPath();
            gp.AddLine(new PointF(0f, 0f), new PointF(0f, y));
            gp.CloseFigure();
            gp.AddLine(new PointF(0f, y), new PointF(2*x, -y));
            gp.CloseFigure();
            gp.AddLine(new PointF(2*x, -y), new PointF(2*x, 0));
            gp.CloseFigure();
            return gp;
        }

        /// <summary>
        /// Creates a triangle pattern
        /// </summary>
        /// <param name="x">The base length of the triangle</param>
        /// <param name="y">The location of the next triangle</param>
        /// <returns></returns>
        public static GraphicsPath GetTriangleSeries(float x, float y)
        {
            var gp = new GraphicsPath();
            gp.AddPolygon(new[] { new PointF(x, 0f), new PointF(0f, 0f), new PointF(0.5f*x, 2f*x/3f), new PointF(x, 0f) });
            gp.CloseFigure();
            
            //Just to move to a new position
            gp.AddEllipse(y, 0f, 0f, 0f);
            gp.CloseFigure();
            return gp;
        }

        /// <summary>
        /// Gets or sets the brush used to fill closed objects
        /// </summary>
        public Brush Fill { get; set; }

        protected override void OnRenderInternal(Map map, LineString linestring, Graphics g)
        {
            var clonedPattern = (GraphicsPath) Pattern.Clone();
            var graphicsPath = WarpPathToPath.Warp(LineStringToPath(linestring, map), clonedPattern, true, _interval);
            
            if (graphicsPath == null) return;

            // Fill?
            if (Fill != null)
                g.FillPath(Fill, graphicsPath);
            
            // Outline
            if (Line != null)
                g.DrawPath(Line, graphicsPath);
        }
    }
}