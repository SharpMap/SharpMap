using System;
using System.Drawing;
using GeoAPI.Geometries;

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
        protected override void OnRenderInternal(Map map, ILineString lineString, Graphics graphics)
        {
            graphics.DrawLines(Line, /*LimitValues(*/lineString.TransformToImage(map)/*)*/);
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
        protected override void OnRenderInternal(Map map, ILineString lineString, Graphics graphics)
        {
            var pts = /*LimitValues(*/ VectorRenderer.OffsetRight(lineString.TransformToImage(map), Offset) /*)*/;
            graphics.DrawLines(Line, pts);
        }

    }

}