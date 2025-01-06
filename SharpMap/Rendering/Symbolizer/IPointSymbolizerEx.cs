using System.Drawing;

namespace SharpMap.Rendering.Symbolizer
{
    
    /// <summary>
    /// An extended interface for <see cref="NetTopologySuite.Geometries.PointSymbolizer"/>
    /// </summary>
    public interface NetTopologySuite.Geometries.PointSymbolizerEx : NetTopologySuite.Geometries.PointSymbolizer
    {
        /// <summary>
        /// Gets a value indicating the rectangle enclosing the extent of this symbol 
        /// </summary>
        RectangleF CanvasArea { get; }
    }
}
