using System;
using System.Drawing;
using SharpMap.Base;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Styles;

namespace SharpMap.Rendering.Thematics
{
    /// <summary>
    /// A theme that calculates the font size dependant on the map's zoom
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// // Create a map
    /// var map = new SharpMap.Map();
    /// // Create the label layer   
    /// var layer = new SharpMap.Layers.LabelLayer("Label");
    /// // Assign data source
    /// layer.DataSource = myDataSource;
    /// // Assign theme
    /// layer.Theme = new SharpMap.Rendering.Thematics.FontSizeTheme(layer, map)
    /// {
    ///    // these values are both optional
    ///    MinFontSize = 6f,
    ///    FontSizeScale = 10f,
    ///    //BaseTheme = SomeOtherLabelStyleTheme
    /// };
    /// // Add layer to map
    /// map.Layers.Add(layer);
    /// </code>
    /// </example>
    [Serializable]
    public class FontSizeTheme : DisposableObject, ITheme
    {
        private readonly LabelLayer _labelLayer;
        private readonly Map _map;

        private double _currentZoom;
        private LabelStyle _currentStyle;

        private Func<Map, float, float> _calculateSize;
        private float _currentSize;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="map"></param>
        public FontSizeTheme(LabelLayer layer, Map map)
        {
            _labelLayer = layer;
            _map = map;

            _currentStyle = layer.Style.Clone();
            _currentSize = _currentStyle.Font.Size;
            _labelLayer.StyleChanged += HandleStyleChanged;
        }

        /// <summary>
        /// Event handler for the <see cref="Layer.StyleChanged"/> event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event's arguments</param>
        private void HandleStyleChanged(object sender, EventArgs e)
        {
            if (_currentStyle != null)
                _currentStyle.Dispose();
            
            _currentStyle = _labelLayer.Style.Clone();
            _currentSize = _currentStyle.Font.Size;
        }

        /// <summary>
        /// Returns the style based on a feature
        /// </summary>
        /// <param name="attribute">Set of attribute values to calculate the <see cref="IStyle"/> from</param>
        /// <returns>The style</returns>
        public IStyle GetStyle(FeatureDataRow attribute)
        {
            // Do we have a theme to evaluate first?
            if (BaseTheme != null)
            {
                var style = BaseTheme.GetStyle(attribute);
                if (!style.Enabled) return style;
                if (!(style is LabelStyle)) return style;

                var labelStyle = (LabelStyle) style;
                return UpdateStyle(labelStyle, CalculateSize(_map, labelStyle.Font.SizeInPoints));
            }

            // Test if zoom level has changed.
            if (_currentZoom != _map.Zoom)
            {
                // Get the new size
                var newSize = CalculateSize(_map, _currentSize);
                
                // Update the style
                _currentStyle = UpdateStyle(_currentStyle, newSize);
                _currentZoom = _map.Zoom;
            }

            // return the style
            return _currentStyle;
        }

        /// <summary>
        /// Method to update the style according to the new size
        /// </summary>
        /// <param name="labelStyle">The label style</param>
        /// <param name="newSize">A new size</param>
        /// <returns>The updated label style</returns>
        protected virtual LabelStyle UpdateStyle(LabelStyle labelStyle, float newSize)
        {
            if (MinFontSize.HasValue && newSize < MinFontSize)
            {
                labelStyle.Enabled = false;
                return labelStyle;
            }
            
            // Make sure label style is enabled
            labelStyle.Enabled = true;

            // Is there a significant change in the font size?
            if (Math.Round(newSize) != Math.Round(labelStyle.Font.Size))
            {
                // Store the old font
                var oldFont = labelStyle.Font;

                // Build a new font
                labelStyle.Font = new Font(oldFont.FontFamily, newSize, oldFont.Style, oldFont.Unit);

                // Dispose the old font
                if (oldFont != null) oldFont.Dispose();
            }
            return labelStyle;
        }

        /// <summary>
        /// Gets or sets an additional theme that has to be computed before this style is applied
        /// </summary>
        public ITheme BaseTheme { get; set; }

        /// <summary>
        /// Function to calculate the size of the font in <see cref="GraphicsUnit.Pixel"/>
        /// </summary>
        public Func<Map, float, float> CalculateSize
        {
            get { return _calculateSize ?? TreatSizeAsMapUnits; }
            set { _calculateSize = value; }
        }

        /// <summary>
        /// Default Implementation of a size conversion from map units to <see cref="GraphicsUnit.Pixel"/>.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="sizeInMapUnits">The size of the font in map units</param>
        /// <returns>A font size in pixel</returns>
        public float TreatSizeAsMapUnits(Map map, float sizeInMapUnits)
        {
            if (FontSizeScale.HasValue)
                sizeInMapUnits *= FontSizeScale.Value;
            return (float)(sizeInMapUnits/_map.PixelHeight);
        }

        /// <summary>
        /// Gets or sets a value indicating at which size the font is being drawn
        /// </summary>
        public float? MinFontSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the scale factor for the font size
        /// </summary>
        public float? FontSizeScale { get; set; }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            _currentStyle.Dispose();
            base.ReleaseManagedResources();
        }
    }
}