using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using SharpMap.Base;

namespace SharpMap.Rendering.Symbolizer
{

    /// <summary>
    /// Interface for all classes providing Line symbolization handling routine
    /// </summary>
    public interface ILineSymbolizeHandler : IDisposableEx
    {
        /// <summary>
        /// Function to symbolize the graphics path to the graphics object
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="paths">The Paths</param>
        void SymbolizePaths(Graphics g, IEnumerable<GraphicsPath> paths);
    }

    /// <summary>
    /// Line symbolize helper class that plainly draws a line.
    /// </summary>
    public class PlainLineSymbolizeHandler : DisposableObject, ILineSymbolizeHandler
    {
        /// <summary>
        /// Gets or sets the <see cref="Pen"/> to use
        /// </summary>
        public Pen Line { get; set; }


        /// <summary>
        /// Function to symbolize the graphics path to the graphics object
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="path">The Path</param>
        public void SymbolizePaths(Graphics g, IEnumerable<GraphicsPath> path)
        {
            foreach (var graphicsPath in path)
                g.DrawPath(Line, graphicsPath);
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (Line != null)
                Line.Dispose();

            base.ReleaseManagedResources();
        }
    }

    /// <summary>
    /// Class that symbolizes a path by warping a <see cref="Pattern"/> to the provided graphics path.
    /// </summary>
    public class WarpedLineSymbolizeHander : DisposableObject, ILineSymbolizeHandler
    {
        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (Line != null)
                Line.Dispose();
            if (Fill != null)
                Fill.Dispose();
            if (Pattern != null)
                Pattern.Dispose();
            
            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Gets or sets the <see cref="Pen"/> to draw the graphics path
        /// </summary>
        public Pen Line { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to fill the graphics path
        /// </summary>
        public Brush Fill { get; set; }

        /// <summary>
        /// The pattern to warp.
        /// </summary>
        public GraphicsPath Pattern { get; set; }

        /// <summary>
        /// Gets or sets the interval with witch to repeat the pattern
        /// </summary>
        public float Interval { get; set; }

        /// <summary>
        /// Function to symbolize the graphics path to the graphics object
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="paths">The paths</param>
        public void SymbolizePaths(Graphics g, IEnumerable<GraphicsPath> paths)
        {
            foreach (var graphicsPath in paths)
            {
                var clonedPattern = (GraphicsPath)Pattern.Clone();
                var warpedPath = WarpPathToPath.Warp(graphicsPath, clonedPattern, true, Interval);
                
                if (warpedPath == null) continue;

                if (Fill != null)
                    g.FillPath(Fill, warpedPath);
                if (Line != null)
                    g.DrawPath(Line, warpedPath);
            }
        }
    }

    /// <summary>
    /// A Line symbolizer that creates <see cref="GraphicsPath"/>objects in the <see cref="OnRenderInternal"/> function.
    /// These graphic paths are symbolized in subsequent symbolize calls.
    /// </summary>
    public class CachedLineSymbolizer : LineSymbolizer
    {
        private List<GraphicsPath> _graphicsPaths;
        private readonly List<ILineSymbolizeHandler> _lineSymbolizeHandlers;
        private readonly ILineSymbolizeHandler _fallback = new PlainLineSymbolizeHandler {Line = new Pen(Color.Black)};
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public CachedLineSymbolizer()
        {
            _graphicsPaths = new List<GraphicsPath>();
            _lineSymbolizeHandlers = new List<ILineSymbolizeHandler>();
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (_graphicsPaths != null)
            {
                foreach (var graphicsPath in Paths)
                    graphicsPath.Dispose();
                _graphicsPaths = null;
            }

            if (_lineSymbolizeHandlers != null)
            {
                foreach (var lineSymbolizeHandler in _lineSymbolizeHandlers)
                    lineSymbolizeHandler.Dispose();
                _lineSymbolizeHandlers.Clear();
            }

            if (_fallback != null)
                _fallback.Dispose();
            
            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The cached path
        /// </summary>
        public List<GraphicsPath> Paths
        {
            get { return _graphicsPaths; }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _graphicsPaths = value;
            }
        }

        /// <summary>
        /// The line symbolizers to apply to the <see cref="Paths"/>.
        /// </summary>
        public List<ILineSymbolizeHandler> LineSymbolizeHandlers
        {
            get { return _lineSymbolizeHandlers; }
        }

        /// <summary>
        /// Function that actually renders the linestring
        /// </summary>
        /// <param name="map"></param>
        /// <param name="lineString"></param>
        /// <param name="graphics"></param>
        protected override void OnRenderInternal(Map map, ILineString lineString, Graphics graphics)
        {
            var gp = new GraphicsPath();
            gp.AddLines(/*LimitValues(*/lineString.TransformToImage(map)/*)*/);
            if (ImmediateMode)
            {
                var tmp = new List<GraphicsPath>(new[] {gp});
                Symbolize(graphics, map, tmp);
            }
            else
                _graphicsPaths.Add(gp);
        }

        /// <summary>
        /// Do not cache the geometries paths
        /// </summary>
        public bool ImmediateMode { get; set; }

        /// <summary>
        /// Method to indicate that the symbolizer has to be prepared.
        /// </summary>
        public override void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            _graphicsPaths = new List<GraphicsPath>(aproximateNumberOfGeometries);
            base.Begin(g, map, aproximateNumberOfGeometries);
        }


        /// <summary>
        /// Method to indicate that the symbolizer should do its symbolizer work.
        /// </summary>
        public override void Symbolize(Graphics g, Map map)
        {
            Symbolize(g, map, Paths);
        }

        private void Symbolize(Graphics graphics, Map map, List<GraphicsPath> paths)
        {
            if (_lineSymbolizeHandlers.Count == 0)
                _fallback.SymbolizePaths(graphics, paths);
            else
            {
                foreach (var lineSymbolizeHandler in _lineSymbolizeHandlers)
                    lineSymbolizeHandler.SymbolizePaths(graphics, paths);
            }
        }

        /// <summary>
        /// Method to indicate that the symbolizers work is done and it can clean up.
        /// </summary>
        public override void End(Graphics g, Map map)
        {
            if (_graphicsPaths.Count > 0)
                _graphicsPaths.Clear();
            base.End(g, map);
        }
    }
}