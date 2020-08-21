using System.Drawing;

namespace SharpMap.Rendering.Symbolizer
{
    
    /// <summary>
    /// An extended interface for <see cref="IPointSymbolizer"/>
    /// </summary>
    public interface IPointSymbolizerEx : IPointSymbolizer
    {
        /// <summary>
        /// Gets a value indicating the rectangle enclosing the extent of this symbol 
        /// </summary>
        RectangleF CanvasArea { get; }
    }
}
