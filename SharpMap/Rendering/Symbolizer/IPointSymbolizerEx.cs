using System.Drawing;

namespace SharpMap.Rendering.Symbolizer
{
    public interface IPointSymbolizerEx : IPointSymbolizer
    {
       
        /// <summary>
        /// Returns the rectangle enclosing the extent of this symbol 
        /// </summary>
        RectangleF CanvasArea { get; }
    }
}
