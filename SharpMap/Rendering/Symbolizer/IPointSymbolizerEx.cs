using System.Drawing;

namespace SharpMap.Rendering.Symbolizer
{
    public interface IPointSymbolizerEx : IPointSymbolizer
    {
       
        /// <summary>
        /// Returns the rectilinear graphics extent of this symbol 
        /// </summary>
        RectangleF Bounds { get; }
    }
}
