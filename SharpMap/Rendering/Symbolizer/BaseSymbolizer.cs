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

        public abstract object Clone();

        #endregion

        #region Implementation of ISymbolizer

        public virtual void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            _oldSmootingMode = g.SmoothingMode;
            _oldPixelOffsetMode = g.PixelOffsetMode;

            g.SmoothingMode = SmoothingMode;
            g.PixelOffsetMode = PixelOffsetMode;
        }

        public virtual void Symbolize(Graphics g, Map map)
        {
        }

        /// <summary>
        /// Restores the gra
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        public virtual void End(Graphics g, Map map)
        {
            g.SmoothingMode = _oldSmootingMode;
            g.PixelOffsetMode = _oldPixelOffsetMode;
        }

        #endregion
    }
}