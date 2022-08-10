using NetTopologySuite.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;

//using KnownColor = System.Drawing.KnownColor
namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Abstract base class for all line symbolizer classes
    /// </summary>
    public abstract class LineSymbolizer : BaseSymbolizer, ILineSymbolizer
    {
#if !NETSTANDARD2_0
        /// <summary>
        /// Creates an instance of this class. <see cref="Line"/> is set to a random
        /// <see cref="System.Drawing.KnownColor"/>.
        /// </summary>
#else
        /// <summary>
        /// Creates an instance of this class. <see cref="Line"/> is set to a random
        /// <see cref="SharpMap.Drawing.KnownColor"/>.
        /// </summary>
#endif
        protected LineSymbolizer()
        {
            Line = new Pen(Utility.RandomKnownColor(), 1);
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            CheckDisposed();

            if (Line != null)
                Line.Dispose();

            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Method to render a LineString to the <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="map">The map object</param>
        /// <param name="lineal">Linestring to symbolize</param>
        /// <param name="g">The graphics object to use.</param>
        public void Render(MapViewport map, ILineal lineal, Graphics g)
        {
            var ms = lineal as MultiLineString;
            if (ms != null)
            {
                for (var i = 0; i < ms.NumGeometries; i++)
                {
                    var lineString = (LineString)ms[i];
                    OnRenderInternal(map, lineString, g);
                }
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
        protected abstract void OnRenderInternal(MapViewport map, LineString lineString, Graphics graphics);

        /// <summary>
        /// Function to transform a linestring to a graphics path for further processing
        /// </summary>
        /// <param name="lineString">The Linestring</param>
        /// <param name="map">The map</param>
        /// <!--<param name="useClipping">A value indicating whether clipping should be applied or not</param>-->
        /// <returns>A GraphicsPath</returns>
        public static GraphicsPath LineStringToPath(LineString lineString, MapViewport map)
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

        /// <summary>
        /// Method to perform symbolization
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        public override void Symbolize(Graphics g, MapViewport map)
        {
        }

        #endregion
    }
}
