using NetTopologySuite.Geometries;
using System;
using System.Drawing;


namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// A very basic line symbolizer class without any fuzzy things. It simply draws the
    /// linestring.
    /// </summary>
    [Serializable]
    public class BasicLineSymbolizer : LineSymbolizer
    {
        /// <summary>
        /// Creates a clone of this symbolizer
        /// </summary>
        /// <returns>A symbolizer exactly like this one</returns>
        public override object Clone()
        {
            return new BasicLineSymbolizer {Line = (Pen) Line.Clone()};
        }

        /// <summary>
        /// Method that does the actual rendering of individual features.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="lineString">The linestring</param>
        /// <param name="graphics">The graphics object</param>
        protected override void OnRenderInternal(MapViewport map, LineString lineString, Graphics graphics)
        {
            graphics.DrawLines(Line, VectorRenderer.LimitValues(lineString.TransformToImage(map), VectorRenderer.ExtremeValueLimit));
        }

    }

    /// <summary>
    /// A very basic line symbolizer class without any fuzzy things. It simply draws the
    /// linestring.
    /// </summary>
    [Serializable]
    public class BasicLineSymbolizerWithOffset : LineSymbolizer
    {
        /// <summary>
        /// Creates a clone of this symbolizer
        /// </summary>
        /// <returns>A symbolizer exactly like this one</returns>
        public override object Clone()
        {
            return new BasicLineSymbolizerWithOffset {Line = (Pen) Line.Clone(), Offset = Offset};
        }

        /// <summary>
        /// Gets or sets the affset to the right (as defined by its coordinate sequence) of the lines
        /// </summary>
        public float Offset { get; set; }

        /// <summary>
        /// Method that does the actual rendering of individual features.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="lineString">The linestring</param>
        /// <param name="graphics">The graphics object</param>
        protected override void OnRenderInternal(MapViewport map, LineString lineString, Graphics graphics)
        {
            var pts = VectorRenderer.LimitValues(VectorRenderer.OffsetRight(lineString.TransformToImage(map), Offset), VectorRenderer.ExtremeValueLimit);
            graphics.DrawLines(Line, pts);
        }

    }

}
