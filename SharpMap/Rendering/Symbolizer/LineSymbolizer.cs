using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Geometries;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Abstract base class for all line symbolizer classes
    /// </summary>
    public abstract class LineSymbolizer : ILineSymbolizer
    {
        protected LineSymbolizer()
        {
            Line = new Pen(Utility.RandomKnownColor(), 1);
        }
        
        /// <summary>
        /// Method to render a LineString to the <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="map">The map object</param>
        /// <param name="lineal">Linestring to symbolize</param>
        /// <param name="g">The graphics object to use.</param>
        public void Render(Map map, ILineal lineal, Graphics g)
        {
            var ms = lineal as MultiLineString;
            if (ms != null)
            {
                foreach (LineString lineString in ms.LineStrings)
                    OnRenderInternal(map, lineString, g);
                return;
            }
            OnRenderInternal(map, (LineString)lineal, g);
        }

        /// <summary>
        /// Function that actually renders the linestring
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="lineString">The line string to symbolize.</param>
        /// <param name="graphics">The graphics</param>
        protected abstract void OnRenderInternal(Map map, LineString lineString, Graphics graphics);

        /// <summary>
        /// Function to transform a linestring to a graphics path for further processing
        /// </summary>
        /// <param name="lineString">The Linestring</param>
        /// <param name="map">The map</param>
        /// <!--<param name="useClipping">A value indicating whether clipping should be applied or not</param>-->
        /// <returns>A GraphicsPath</returns>
        public static GraphicsPath LineStringToPath(LineString lineString, Map map)
        {
            var gp = new GraphicsPath(FillMode.Alternate);
            gp.AddLines(lineString.TransformToImage(map));
            return gp;
        }

        /// <summary>
        /// Gets or sets the <see cref="Pen"/> to render the LineString
        /// </summary>
        public Pen Line { get; set; }

        #region ISymbolizer implementation

        public virtual void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
        }

        public virtual void Symbolize(Graphics g, Map map)
        {
        }

        public virtual void End(Graphics g, Map map)
        {
        }
        
        #endregion
    }
}