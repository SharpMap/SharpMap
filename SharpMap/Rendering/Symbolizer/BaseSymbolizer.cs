using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Base;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Abstract base symbolizer class. 
    /// </summary>
    [Serializable]
    public abstract class BaseSymbolizer : DisposableObject, ISymbolizer
    {
        private SmoothingMode _oldSmootingMode;
        private PixelOffsetMode _oldPixelOffsetMode;

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="SmoothingMode"/> to <see cref="System.Drawing.Drawing2D.SmoothingMode.AntiAlias"/> and 
        /// <see cref="PixelOffsetMode"/> to <see cref="System.Drawing.Drawing2D.PixelOffsetMode.Default"/>.
        /// </remarks>
        protected BaseSymbolizer()
        {
            SmoothingMode = SmoothingMode.AntiAlias;
            PixelOffsetMode = PixelOffsetMode.Default;
        }

        /// <summary>
        /// Gets or sets a value indicating which <see cref="SmoothingMode"/> is to be used for rendering
        /// </summary>
        public SmoothingMode SmoothingMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which <see cref="PixelOffsetMode"/> is to be used for rendering
        /// </summary>
        public PixelOffsetMode PixelOffsetMode { get; set; }

        #region Implementation of ICloneable

        /// <summary>
        /// Creates a deep copy of this <see cref="ISymbolizer"/>.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        #endregion

        #region Implementation of ISymbolizer

        /// <summary>
        /// Method to perform preparatory work for symbilizing.
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        /// <param name="aproximateNumberOfGeometries">An approximate number of geometries to symbolize</param>
        public virtual void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            _oldSmootingMode = g.SmoothingMode;
            _oldPixelOffsetMode = g.PixelOffsetMode;

            g.SmoothingMode = SmoothingMode;
            g.PixelOffsetMode = PixelOffsetMode;
        }

        /// <summary>
        /// Method to perform symbolization
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        public virtual void Symbolize(Graphics g, Map map)
        {
        }

        /// <summary>
        /// Method to restore the state of the graphics object and do cleanup work.
        /// </summary>
        /// <param name="g">The graphics object to symbolize upon</param>
        /// <param name="map">The map</param>
        public virtual void End(Graphics g, Map map)
        {
            g.SmoothingMode = _oldSmootingMode;
            g.PixelOffsetMode = _oldPixelOffsetMode;
        }

        #endregion
    }
}