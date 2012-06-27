using System;
using System.Drawing;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Rendering.Thematics;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// A very basic line symbolizer class without any fuzzy things. It simply draws the
    /// linestring.
    /// </summary>
    [Serializable]
    public class BasicLineSymbolizer : LineSymbolizer
    {
        public override object Clone()
        {
            return new BasicLineSymbolizer {Line = (Pen) Line.Clone()};
        }

        protected override void OnRenderInternal(Map map, ILineString linestring, Graphics g)
        {
            g.DrawLines(Line, /*LimitValues(*/linestring.TransformToImage(map)/*)*/);
        }

    }

    /// <summary>
    /// A very basic line symbolizer class without any fuzzy things. It simply draws the
    /// linestring.
    /// </summary>
    [Serializable]
    public class BasicLineSymbolizerWithOffset : LineSymbolizer
    {
        public override object Clone()
        {
            return new BasicLineSymbolizerWithOffset {Line = (Pen) Line.Clone(), Offset = Offset};
        }

        /// <summary>
        /// Gets or sets the affset to the right (as defined by its coordinate sequence) of the lines
        /// </summary>
        public float Offset { get; set; }

        protected override void OnRenderInternal(Map map, ILineString linestring, Graphics g)
        {
            var pts = /*LimitValues(*/ VectorRenderer.OffsetRight(linestring.TransformToImage(map), Offset) /*)*/;
            g.DrawLines(Line, pts);
        }

    }

}