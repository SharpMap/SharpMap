using System;
using System.Drawing;
using SharpMap.Data;
using SharpMap.Geometries;
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
        protected override void OnRenderInternal(Map map, LineString linestring, Graphics g)
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
        /// <summary>
        /// Gets or sets the Offset 
        /// </summary>
        public float Offset { get; set; }

        protected override void OnRenderInternal(Map map, LineString linestring, Graphics g)
        {
            var pts = /*LimitValues(*/ linestring.TransformToImage(map) /*)*/;
            g.DrawLines(Line, pts);
        }

    }

}