// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Data;
using SharpMap.Styles;

namespace SharpMap.Rendering.Thematics
{
    /// <summary>
    /// Gradient theme base class
    /// </summary>
    [Serializable]
    public abstract class GradientThemeBase : ITheme
    {
        private ColorBlend _fillColorBlend;
        private ColorBlend _lineColorBlend;
        private double _max;
        private IStyle _maxStyle;
        private double _min;
        private IStyle _minStyle;
        private ColorBlend _textColorBlend;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="minValue">The minimum value</param>
        /// <param name="maxValue">The maximum value</param>
        /// <param name="minStyle">The <see cref="IStyle">style</see> to apply for values equal to <paramref name="minValue"/></param>
        /// <param name="maxStyle">The <see cref="IStyle">style</see> to apply for values equal to <paramref name="maxValue"/></param>
        protected GradientThemeBase(double minValue, double maxValue, IStyle minStyle, IStyle maxStyle)
        {
            _min = minValue;
            _max = maxValue;
            _maxStyle = maxStyle;
            _minStyle = minStyle;
        }

        /// <summary>
        /// Gets or sets the minimum value of the gradient
        /// </summary>
        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Gets or sets the maximum value of the gradient
        /// </summary>
        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.IStyle">style</see> for the minimum value
        /// </summary>
        public IStyle MinStyle
        {
            get { return _minStyle; }
            set { _minStyle = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.IStyle">style</see> for the maximum value
        /// </summary>
        public IStyle MaxStyle
        {
            get { return _maxStyle; }
            set { _maxStyle = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Rendering.Thematics.ColorBlend"/> used on labels
        /// </summary>
        public ColorBlend TextColorBlend
        {
            get { return _textColorBlend; }
            set { _textColorBlend = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Rendering.Thematics.ColorBlend"/> used on lines
        /// </summary>
        public ColorBlend LineColorBlend
        {
            get { return _lineColorBlend; }
            set { _lineColorBlend = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Rendering.Thematics.ColorBlend"/> used as Fill
        /// </summary>
        public ColorBlend FillColorBlend
        {
            get { return _fillColorBlend; }
            set { _fillColorBlend = value; }
        }

        /// <summary>
        /// Function to compute a new <see cref="VectorStyle">style</see> for the given <paramref name="value"/>
        /// </summary>
        /// <param name="min">The minimum <see cref="VectorStyle">style</see></param>
        /// <param name="max">The maximum <see cref="VectorStyle">style</see></param>
        /// <param name="value">The value</param>
        /// <returns>A <see cref="VectorStyle">style</see></returns>
        protected VectorStyle CalculateVectorStyle(VectorStyle min, VectorStyle max, double value)
        {
            var style = new VectorStyle();
            double dFrac = Fraction(value);
            float fFrac = Convert.ToSingle(dFrac);
            style.Enabled = (dFrac > 0.5 ? min.Enabled : max.Enabled);
            style.EnableOutline = (dFrac > 0.5 ? min.EnableOutline : max.EnableOutline);
            if (_fillColorBlend != null)
                style.Fill = new SolidBrush(_fillColorBlend.GetColor(fFrac));
            else if (min.Fill != null && max.Fill != null)
                style.Fill = InterpolateBrush(min.Fill, max.Fill, value);

            if (min.Line != null && max.Line != null)
                style.Line = InterpolatePen(min.Line, max.Line, value);
            if (_lineColorBlend != null)
                style.Line.Color = _lineColorBlend.GetColor(fFrac);

            if (min.Outline != null && max.Outline != null)
                style.Outline = InterpolatePen(min.Outline, max.Outline, value);
            style.MinVisible = InterpolateDouble(min.MinVisible, max.MinVisible, value);
            style.MaxVisible = InterpolateDouble(min.MaxVisible, max.MaxVisible, value);
            style.Symbol = (dFrac > 0.5 ? min.Symbol : max.Symbol);
            style.SymbolOffset = (dFrac > 0.5 ? min.SymbolOffset : max.SymbolOffset);
            //We don't interpolate the offset but let it follow the symbol instead
            style.SymbolScale = InterpolateFloat(min.SymbolScale, max.SymbolScale, value);
            return style;
        }

        /// <summary>
        /// Function to compute a new <see cref="LabelStyle">style</see> for the given <paramref name="value"/>
        /// </summary>
        /// <param name="min">The minimum <see cref="LabelStyle">style</see></param>
        /// <param name="max">The maximum <see cref="LabelStyle">style</see></param>
        /// <param name="value">The value</param>
        /// <returns>A <see cref="LabelStyle">style</see></returns>
        protected LabelStyle CalculateLabelStyle(LabelStyle min, LabelStyle max, double value)
        {
            LabelStyle style = new LabelStyle();
            style.CollisionDetection = min.CollisionDetection;
            style.Enabled = InterpolateBool(min.Enabled, max.Enabled, value);
            float fontSize = InterpolateFloat(min.Font.Size, max.Font.Size, value);
            style.Font = new Font(min.Font.FontFamily, fontSize, min.Font.Style);
            if (min.BackColor != null && max.BackColor != null)
                style.BackColor = InterpolateBrush(min.BackColor, max.BackColor, value);

            if (_textColorBlend != null)
                style.ForeColor = _lineColorBlend.GetColor(Convert.ToSingle(Fraction(value)));
            else
                style.ForeColor = InterpolateColor(min.ForeColor, max.ForeColor, value);
            if (min.Halo != null && max.Halo != null)
                style.Halo = InterpolatePen(min.Halo, max.Halo, value);

            style.MinVisible = InterpolateDouble(min.MinVisible, max.MinVisible, value);
            style.MaxVisible = InterpolateDouble(min.MaxVisible, max.MaxVisible, value);
            style.Offset = new PointF(InterpolateFloat(min.Offset.X, max.Offset.X, value),
                                      InterpolateFloat(min.Offset.Y, max.Offset.Y, value));
            return style;
        }

        /// <summary>
        /// Function to compute the fraction for <paramref name="attr"/> based on <see cref="Min"/> and <see cref="Max"/>
        /// </summary>
        /// <param name="attr">The attribute value to compute the fraction for</param>
        /// <returns>A value in the &#x211d;[0, 1]</returns>
        protected double Fraction(double attr)
        {
            if (attr < _min) return 0;
            if (attr > _max) return 1;
            return (attr - _min) / (_max - _min);
        }

        /// <summary>
        /// Function to interpolate a <see cref="bool"/> value for <paramref name="attr"/>. 
        /// </summary>
        /// <param name="min">The value to return if <see cref="Fraction"/>/<paramref name="attr"/>) &lt;= <value>0.5d</value></param>
        /// <param name="max">The value to return if <see cref="Fraction"/>/<paramref name="attr"/>) &gt; <value>0.5d</value></param>
        /// <param name="attr">The value to test</param>
        /// <returns><paramref name="min"/>, if <see cref="Fraction"/>/<paramref name="attr"/>) &lt;= <value>0.5d</value>, else <paramref name="max"/></returns>
        protected bool InterpolateBool(bool min, bool max, double attr)
        {
            return Fraction(attr) > 0.5d ? max : min;
        }

        /// <summary>
        /// Function to interpolate a <see cref="float"/> value for <paramref name="attr"/>
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <param name="attr">The value to test</param>
        /// <returns>A value in the &#x211d;[<paramref name="min"/>, <paramref name="max"/>]</returns>
        protected float InterpolateFloat(float min, float max, double attr)
        {
            return Convert.ToSingle((max - min) * Fraction(attr) + min);
        }

        /// <summary>
        /// Function to interpolate a <see cref="float"/> value for <paramref name="attr"/>
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <param name="attr">The value to test</param>
        /// <returns>A value in the &#x211d;[<paramref name="min"/>, <paramref name="max"/>]</returns>
        protected double InterpolateDouble(double min, double max, double attr)
        {
            return (max - min) * Fraction(attr) + min;
        }

        /// <summary>
        /// Function to interpolate a <see cref="Brush"/> value for <paramref name="attr"/>
        /// </summary>
        /// <remarks>Currently only <see cref="SolidBrush"/>es are interpolated</remarks>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <param name="attr">The value to test</param>
        /// <returns>An interpolated <see cref="SolidBrush"/>.</returns>
        protected SolidBrush InterpolateBrush(Brush min, Brush max, double attr)
        {
            if (!(min is SolidBrush && max is SolidBrush))
                throw (new ArgumentException("Only SolidBrush brushes are supported in GradientTheme"));
            return new SolidBrush(InterpolateColor((min as SolidBrush).Color, (max as SolidBrush).Color, attr));
        }

        /// <summary>
        /// Function to interpolate a <see cref="Pen"/> value for <paramref name="attr"/>
        /// </summary>
        /// <remarks>Currently only <see cref="PenType.SolidColor"/> pens are interpolated</remarks>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <param name="attr">The value to test</param>
        /// <returns>An interpolated <see cref="Pen"/>.</returns>
        protected Pen InterpolatePen(Pen min, Pen max, double attr)
        {
            if (min.PenType != PenType.SolidColor || max.PenType != PenType.SolidColor)
                throw (new ArgumentException("Only SolidColor pens are supported in GradientTheme"));
            Pen pen = new Pen(InterpolateColor(min.Color, max.Color, attr), InterpolateFloat(min.Width, max.Width, attr));
            double frac = Fraction(attr);
            pen.MiterLimit = InterpolateFloat(min.MiterLimit, max.MiterLimit, attr);
            pen.StartCap = (frac > 0.5 ? max.StartCap : min.StartCap);
            pen.EndCap = (frac > 0.5 ? max.EndCap : min.EndCap);
            pen.LineJoin = (frac > 0.5 ? max.LineJoin : min.LineJoin);
            pen.DashStyle = (frac > 0.5 ? max.DashStyle : min.DashStyle);
            if (min.DashStyle == DashStyle.Custom && max.DashStyle == DashStyle.Custom)
                pen.DashPattern = (frac > 0.5 ? max.DashPattern : min.DashPattern);
            pen.DashOffset = (frac > 0.5 ? max.DashOffset : min.DashOffset);
            pen.DashCap = (frac > 0.5 ? max.DashCap : min.DashCap);
            if (min.CompoundArray.Length > 0 && max.CompoundArray.Length > 0)
                pen.CompoundArray = (frac > 0.5 ? max.CompoundArray : min.CompoundArray);
            pen.Alignment = (frac > 0.5 ? max.Alignment : min.Alignment);
            //pen.CustomStartCap = (frac > 0.5 ? max.CustomStartCap : min.CustomStartCap);  //Throws ArgumentException
            //pen.CustomEndCap = (frac > 0.5 ? max.CustomEndCap : min.CustomEndCap);  //Throws ArgumentException
            return pen;
        }

        /// <summary>
        /// Function to interpolate a <see cref="Color"/> value for <paramref name="attr"/>
        /// </summary>
        /// <param name="minCol">The minimum <see cref="Color"/></param>
        /// <param name="maxCol">The maximum <see cref="Color"/></param>
        /// <param name="attr">The value to test</param>
        /// <returns>An interpolated <see cref="Color"/>.</returns>
        protected Color InterpolateColor(Color minCol, Color maxCol, double attr)
        {
            double frac = Fraction(attr);
            
            if (frac == 1)
                return maxCol;
            
            if (frac == 0)
                return minCol;

            double r = (maxCol.R - minCol.R) * frac + minCol.R;
            double g = (maxCol.G - minCol.G) * frac + minCol.G;
            double b = (maxCol.B - minCol.B) * frac + minCol.B;
            double a = (maxCol.A - minCol.A) * frac + minCol.A;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            if (a > 255) a = 255;
            
            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        /// <summary>
        /// Returns the style based on a numeric DataColumn, where style
        /// properties are linearly interpolated between max and min values.
        /// </summary>
        /// <param name="row">Feature</param>
        /// <returns><see cref="SharpMap.Styles.IStyle">Style</see> calculated by a linear interpolation between the min/max styles</returns>
        public virtual IStyle GetStyle(FeatureDataRow row)
        {
            double attr;
            try
            {
                attr = GetAttributeValue(row);
            }
            catch
            {
                throw new ApplicationException(
                    "Invalid Attribute type in Gradient Theme - Couldn't parse attribute (must be numerical)");
            }
            if (_minStyle.GetType() != _maxStyle.GetType())
                throw new ArgumentException("MinStyle and MaxStyle must be of the same type");
            switch (MinStyle.GetType().FullName)
            {
                case "SharpMap.Styles.VectorStyle":
                    return CalculateVectorStyle(MinStyle as VectorStyle, MaxStyle as VectorStyle, attr);
                case "SharpMap.Styles.LabelStyle":
                    return CalculateLabelStyle(MinStyle as LabelStyle, MaxStyle as LabelStyle, attr);
                default:
                    throw new ArgumentException(
                        "Only SharpMap.Styles.VectorStyle and SharpMap.Styles.LabelStyle are supported for the gradient theme");
            }
        }

        /// <summary>
        /// Function to get a <see cref="double"/> value from <paramref name="row"/>.
        /// </summary>
        /// <param name="row">The row</param>
        /// <returns>A <see cref="double"/> value</returns>
        protected abstract double GetAttributeValue(FeatureDataRow row);
    }

    /// <summary>
    /// The GradientTheme class defines a gradient color thematic rendering of features based by a numeric attribute.
    /// </summary>
    [Serializable]
    public class GradientTheme : GradientThemeBase
    {
        private string _columnName;


        /// <summary>
        /// Initializes a new instance of the GradientTheme class
        /// </summary>
        /// <remarks>
        /// <para>The gradient theme interpolates linearly between two styles based on a numerical attribute in the datasource.
        /// This is useful for scaling symbols, line widths, line and fill colors from numerical attributes.</para>
        /// <para>Colors are interpolated between two colors, but if you want to interpolate through more colors (fx. a rainbow),
        /// set the <see cref="GradientThemeBase.TextColorBlend"/>, <see cref="GradientThemeBase.LineColorBlend"/> and <see cref="GradientThemeBase.FillColorBlend"/> properties
        /// to a custom <see cref="ColorBlend"/>.
        /// </para>
        /// <para>The following properties are scaled (properties not mentioned here are not interpolated):
        /// <list type="table">
        ///		<listheader><term>Property</term><description>Remarks</description></listheader>
        ///		<item><term><see cref="System.Drawing.Color"/></term><description>Red, Green, Blue and Alpha values are linearly interpolated.</description></item>
        ///		<item><term><see cref="System.Drawing.Pen"/></term><description>The color, width, color of pens are interpolated. MiterLimit,StartCap,EndCap,LineJoin,DashStyle,DashPattern,DashOffset,DashCap,CompoundArray, and Alignment are switched in the middle of the min/max values.</description></item>
        ///		<item><term><see cref="System.Drawing.SolidBrush"/></term><description>SolidBrush color are interpolated. Other brushes are not supported.</description></item>
        ///		<item><term><see cref="SharpMap.Styles.VectorStyle"/></term><description>MaxVisible, MinVisible, Line, Outline, Fill and SymbolScale are scaled linearly. Symbol, EnableOutline and Enabled switch in the middle of the min/max values.</description></item>
        ///		<item><term><see cref="SharpMap.Styles.LabelStyle"/></term><description>FontSize, BackColor, ForeColor, MaxVisible, MinVisible, Offset are scaled linearly. All other properties use min-style.</description></item>
        /// </list>
        /// </para>
        /// <example>
        /// Creating a rainbow colorblend showing colors from red, through yellow, green and blue depicting 
        /// the population density of a country.
        /// <code lang="C#">
        /// //Create two vector styles to interpolate between
        /// SharpMap.Styles.VectorStyle min = new SharpMap.Styles.VectorStyle();
        /// SharpMap.Styles.VectorStyle max = new SharpMap.Styles.VectorStyle();
        /// min.Outline.Width = 1f; //Outline width of the minimum value
        /// max.Outline.Width = 3f; //Outline width of the maximum value
        /// //Create a theme interpolating population density between 0 and 400
        /// SharpMap.Rendering.Thematics.GradientTheme popdens = new SharpMap.Rendering.Thematics.GradientTheme("PopDens", 0, 400, min, max);
        /// //Set the fill-style colors to be a rainbow blend from red to blue.
        /// popdens.FillColorBlend = SharpMap.Rendering.Thematics.ColorBlend.Rainbow5;
        /// myVectorLayer.Theme = popdens;
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="columnName">Name of column to extract the attribute</param>
        /// <param name="minValue">Minimum value</param>
        /// <param name="maxValue">Maximum value</param>
        /// <param name="minStyle">Color for minimum value</param>
        /// <param name="maxStyle">Color for maximum value</param>
        public GradientTheme(string columnName, double minValue, double maxValue, IStyle minStyle, IStyle maxStyle)
            : base(minValue, maxValue, minStyle, maxStyle)
        {
            _columnName = columnName;

        }

        /// <summary>
        /// Gets or sets the column name from where to get the attribute value
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        /// <summary>
        /// Function to get a <see cref="double"/> value for <see cref="ColumnName"/> from <paramref name="row"/>.
        /// </summary>
        /// <param name="row">The row</param>
        /// <returns>A <see cref="double"/> value</returns>
        protected override double GetAttributeValue(FeatureDataRow row)
        {
            return Convert.ToDouble(row[_columnName]);
        }
    }
}