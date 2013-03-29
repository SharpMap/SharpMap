using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Warps a Pattern to a linestrings
    /// </summary>
    [Serializable]
    public class WarpedLineSymbolizer : LineSymbolizer
    {
        /// <summary>
        /// Pattern to warp to
        /// </summary>
        public GraphicsPath Pattern { get; set; }

        /// <summary>
        /// The interval between each pattern object
        /// </summary>
        public float Interval { get; set; }

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
        /// <param name="x">The width of a step op the linestring axis.</param>
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
        /// <param name="size">The base length of the triangle</param>
        /// <param name="orientation">The orientation of the triangle
        /// <list type="Bullet"><item>0 ... Up</item>
        /// <item>1 ... Left</item>
        /// <item>2 ... Down</item>
        /// <item>3 ... Right</item>
        /// </list><para/>All other values are reduced using modulo operation</param>
        /// <returns></returns>
        public static GraphicsPath GetTriangle(float size, int orientation)
        {
            orientation = orientation%4;
            var half = 0.5f*size;
            var twoThirds = 2f*size/3f;
            var gp = new GraphicsPath();
            switch (orientation)
            {
                case 0:
                    gp.AddPolygon(new[] { 
                        new PointF(size, 0f), new PointF(0f, 0f), 
                        new PointF(half, twoThirds), new PointF(size, 0f) 
                    });
                    break;
                case 1:
                    gp.AddPolygon(new[] {
                        new PointF(0f, -half), new PointF(0f, half),
                        new PointF(twoThirds, 0), new PointF(0f, -half)
                    });
                    break;
                case 2:
                    gp.AddPolygon(new[] {
                        new PointF(size, 0f), new PointF(0f, 0f), 
                        new PointF(half, -twoThirds), new PointF(size, 0f)
                    });
                    break;
                case 3:
                    gp.AddPolygon(new[] {
                        new PointF(twoThirds, half), new PointF(twoThirds, -half),
                        new PointF(0, 0), new PointF(twoThirds, half)
                    });
                    break;
            }
            gp.CloseFigure();

            return gp;
        }

        /// <summary>
        /// Creates a triangle pattern
        /// </summary>
        /// <param name="x">The base length of the triangle</param>
        /// <param name="y">The location of the next triangle</param>
        /// <returns></returns>
        [Obsolete("Use GetTriangle along with Interval")]
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
        /// Creates a forward oriented triangle pattern
        /// </summary>
        /// <param name="x">The base length of the triangle</param>
        /// <param name="y">The location of the next triangle</param>
        /// <returns></returns>
        [Obsolete("Use GetTriangle along with Interval")]
        public static GraphicsPath GetTriangleSeriesForward(float x, float y)
        {
            var gp = new GraphicsPath();
            gp.AddPolygon(new[] { new PointF(0f, -0.5f*x), new PointF(0f, 0.5f*x), new PointF(2f * x / 3f, 0), new PointF(0f, -0.5f*x) });
            gp.CloseFigure();

            //Just to move to a new position
            gp.AddEllipse(y, 0f, 0f, 0f);
            gp.CloseFigure();
            return gp;
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (Fill != null)
            {
                Fill.Dispose();
                Fill = null;
            }

            if (Pattern != null)
            {
                Pattern.Dispose();
                Pattern = null;
            }

            base.ReleaseManagedResources();
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            var res = (WarpedLineSymbolizer)MemberwiseClone();
            res.Fill = (Brush) Fill.Clone();
            res.Line = (Pen) Line.Clone();
            res.Pattern = (GraphicsPath) Pattern.Clone();
            
            return res;
        }

        /// <summary>
        /// Gets or sets the brush used to fill closed objects
        /// </summary>
        public Brush Fill { get; set; }

        /// <summary>
        /// Function that actually renders the linestring
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="lineString">The line string to symbolize.</param>
        /// <param name="graphics">The graphics</param>
        protected override void OnRenderInternal(Map map, ILineString lineString, Graphics graphics)
        {
            var clonedPattern = (GraphicsPath) Pattern.Clone();
            var graphicsPath = WarpPathToPath.Warp(LineStringToPath(lineString, map), clonedPattern, true, Interval);
            
            if (graphicsPath == null) return;

            // Fill?
            if (Fill != null)
                graphics.FillPath(Fill, graphicsPath);
            
            // Outline
            if (Line != null)
                graphics.DrawPath(Line, graphicsPath);
        }
    }
}