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
using System.Collections.Generic;
using System.Text;
using SharpMap.Styles;
namespace SharpMap.Rendering.Thematics
{
	/// <summary>
	/// The GradientTheme class defines a gradient color thematic rendering of features based by a numeric attribute.
	/// </summary>
	public class GradientTheme : ITheme
	{
		/// <summary>
		/// Initializes a new instance of the GradientTheme class
		/// </summary>
		/// <remarks>
		/// <para>The gradient theme interpolates linearly between two styles based on a numerical attribute in the datasource.
		/// This is useful for scaling symbols, line widths, line and fill colors from numerical attributes.</para>
		/// <para>Colors are interpolated between two colors, but if you want to interpolate through more colors (fx. a rainbow),
		/// set the <see cref="TextColorBlend"/>, <see cref="LineColorBlend"/> and <see cref="FillColorBlend"/> properties
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
		public GradientTheme(string columnName, double minValue, double maxValue, SharpMap.Styles.IStyle minStyle, SharpMap.Styles.IStyle maxStyle)
		{
			_ColumnName = columnName;
			_min = minValue;
			_max = maxValue;
			_maxStyle = maxStyle;
			_minStyle = minStyle;
		}

		private string _ColumnName;

		/// <summary>
		/// Gets or sets the column name from where to get the attribute value
		/// </summary>
		public string ColumnName
		{
			get { return _ColumnName; }
			set { _ColumnName = value; }
		}

		private double _min;

		/// <summary>
		/// Gets or sets the minimum value of the gradient
		/// </summary>
		public double Min
		{
			get { return _min; }
			set { _min = value; }
		}

		private double _max;

		/// <summary>
		/// Gets or sets the maximum value of the gradient
		/// </summary>
		public double Max
		{
			get { return _max; }
			set { _max = value; }
		}

		private SharpMap.Styles.IStyle _minStyle;

		/// <summary>
		/// Gets or sets the <see cref="SharpMap.Styles.IStyle">style</see> for the minimum value
		/// </summary>
		public SharpMap.Styles.IStyle MinStyle
		{
			get { return _minStyle; }
			set { _minStyle = value; }
		}

		private SharpMap.Styles.IStyle _maxStyle;

		/// <summary>
		/// Gets or sets the <see cref="SharpMap.Styles.IStyle">style</see> for the maximum value
		/// </summary>
		public SharpMap.Styles.IStyle MaxStyle
		{
			get { return _maxStyle; }
			set { _maxStyle = value; }
		}

		private ColorBlend _TextColorBlend;

		/// <summary>
		/// Gets or sets the <see cref="SharpMap.Rendering.Thematics.ColorBlend"/> used on labels
		/// </summary>
		public ColorBlend TextColorBlend
		{
			get { return _TextColorBlend; }
			set { _TextColorBlend = value; }
		}

		private ColorBlend _LineColorBlend;

		/// <summary>
		/// Gets or sets the <see cref="SharpMap.Rendering.Thematics.ColorBlend"/> used on lines
		/// </summary>
		public ColorBlend LineColorBlend
		{
			get { return _LineColorBlend; }
			set { _LineColorBlend = value; }
		}

		private ColorBlend _FillColorBlend;

		/// <summary>
		/// Gets or sets the <see cref="SharpMap.Rendering.Thematics.ColorBlend"/> used as Fill
		/// </summary>
		public ColorBlend FillColorBlend
		{
			get { return _FillColorBlend; }
			set { _FillColorBlend = value; }
		}	

		#region ITheme Members

		/// <summary>
		/// Returns the style based on a numeric DataColumn, where style
		/// properties are linearly interpolated between max and min values.
		/// </summary>
		/// <param name="row">Feature</param>
		/// <returns><see cref="SharpMap.Styles.IStyle">Style</see> calculated by a linear interpolation between the min/max styles</returns>
		public SharpMap.Styles.IStyle GetStyle(SharpMap.Data.FeatureDataRow row)
		{
			double attr = 0;
			try { attr = Convert.ToDouble(row[this._ColumnName]); }
			catch { throw new ApplicationException("Invalid Attribute type in Gradient Theme - Couldn't parse attribute (must be numerical)"); }
			if (_minStyle.GetType() != _maxStyle.GetType())
				throw new ArgumentException("MinStyle and MaxStyle must be of the same type");
			switch (MinStyle.GetType().FullName)
			{
				case "SharpMap.Styles.VectorStyle":
					return CalculateVectorStyle(MinStyle as VectorStyle, MaxStyle as VectorStyle, attr);
				case "SharpMap.Styles.LabelStyle":
					return CalculateLabelStyle(MinStyle as LabelStyle, MaxStyle as LabelStyle, attr);
				default:
					throw new ArgumentException("Only SharpMap.Styles.VectorStyle and SharpMap.Styles.LabelStyle are supported for the gradient theme");
			}			
		}

		private VectorStyle CalculateVectorStyle(SharpMap.Styles.VectorStyle min, SharpMap.Styles.VectorStyle max, double value)
		{
			VectorStyle style = new VectorStyle();
			double dFrac = Fraction(value);
			float fFrac = Convert.ToSingle(dFrac);
			style.Enabled = (dFrac>0.5 ? min.Enabled : max.Enabled);
			style.EnableOutline = (dFrac>0.5 ? min.EnableOutline : max.EnableOutline);
			if (_FillColorBlend != null)
				style.Fill = new System.Drawing.SolidBrush(_FillColorBlend.GetColor(fFrac));
			else if (min.Fill != null && max.Fill != null)
				style.Fill = InterpolateBrush(min.Fill, max.Fill, value);
			
			if (min.Line != null && max.Line != null)
				style.Line = InterpolatePen(min.Line, max.Line, value);
			if (_LineColorBlend != null)
				style.Line.Color = _LineColorBlend.GetColor(fFrac);

			if(min.Outline!=null && max.Outline != null)
				style.Outline = InterpolatePen(min.Outline, max.Outline,value);
			style.MinVisible = InterpolateDouble(min.MinVisible, max.MinVisible, value);
			style.MaxVisible = InterpolateDouble(min.MaxVisible, max.MaxVisible, value);
			style.Symbol = (dFrac > 0.5 ? min.Symbol : max.Symbol);
			style.SymbolOffset = (dFrac > 0.5 ? min.SymbolOffset : max.SymbolOffset); //We don't interpolate the offset but let it follow the symbol instead
			style.SymbolScale = InterpolateFloat(min.SymbolScale, max.SymbolScale, value);
			return style;
		}

		private LabelStyle CalculateLabelStyle(SharpMap.Styles.LabelStyle min, SharpMap.Styles.LabelStyle max, double value)
		{
			LabelStyle style = new LabelStyle();
			style.CollisionDetection = min.CollisionDetection;
			style.Enabled = InterpolateBool(min.Enabled, max.Enabled,value);
			float FontSize = InterpolateFloat(min.Font.Size,max.Font.Size,value);
			style.Font = new System.Drawing.Font(min.Font.FontFamily,FontSize,min.Font.Style);
			if (min.BackColor != null && max.BackColor != null)
				style.BackColor = InterpolateBrush(min.BackColor, max.BackColor, value);

			if (_TextColorBlend != null)
				style.ForeColor = _LineColorBlend.GetColor(Convert.ToSingle(Fraction(value)));
			else 
				style.ForeColor = InterpolateColor(min.ForeColor, max.ForeColor, value);
			if (min.Halo != null && max.Halo != null)
				style.Halo = InterpolatePen(min.Halo,max.Halo, value);

			style.MinVisible = InterpolateDouble(min.MinVisible,max.MinVisible,value);
			style.MaxVisible = InterpolateDouble(min.MaxVisible, max.MaxVisible, value);
			style.Offset = new System.Drawing.PointF(InterpolateFloat(min.Offset.X, max.Offset.X, value), InterpolateFloat(min.Offset.Y, max.Offset.Y, value));
			return style;
		}

		private double Fraction(double attr)
		{
			if (attr < _min) return 0;
			if (attr > _max) return 1;
			return (attr - _min) / (_max - _min);
		}

		private bool InterpolateBool(bool min, bool max, double attr)
		{
			double frac = Fraction(attr);
			if (frac > 0.5) return max;
			else return min;
		}

		private float InterpolateFloat(float min, float max, double attr)
		{
			return Convert.ToSingle((max - min) * Fraction(attr) + min);
		}

		private double InterpolateDouble(double min, double max, double attr)
		{
			return (max - min) * Fraction(attr) + min;
		}

		private System.Drawing.SolidBrush InterpolateBrush(System.Drawing.Brush min, System.Drawing.Brush max, double attr)
		{
			if (min.GetType() != typeof(System.Drawing.SolidBrush) || max.GetType() != typeof(System.Drawing.SolidBrush))
				throw (new ArgumentException("Only SolidBrush brushes are supported in GradientTheme"));
			return new System.Drawing.SolidBrush(InterpolateColor((min as System.Drawing.SolidBrush).Color, (max as System.Drawing.SolidBrush).Color, attr));
		}

		private System.Drawing.Pen InterpolatePen(System.Drawing.Pen min, System.Drawing.Pen max, double attr)
		{
			if (min.PenType!= System.Drawing.Drawing2D.PenType.SolidColor|| max.PenType != System.Drawing.Drawing2D.PenType.SolidColor)
				throw (new ArgumentException("Only SolidColor pens are supported in GradientTheme"));		
			System.Drawing.Pen pen = new System.Drawing.Pen(InterpolateColor(min.Color, max.Color, attr),InterpolateFloat(min.Width, max.Width,attr));
			double frac = Fraction(attr);
			pen.MiterLimit = InterpolateFloat(min.MiterLimit, max.MiterLimit, attr);
			pen.StartCap = (frac > 0.5 ? max.StartCap : min.StartCap);
			pen.EndCap = (frac > 0.5 ? max.EndCap : min.EndCap);
			pen.LineJoin = (frac > 0.5 ? max.LineJoin : min.LineJoin);
			pen.DashStyle = (frac > 0.5 ? max.DashStyle : min.DashStyle);
			if(min.DashStyle==System.Drawing.Drawing2D.DashStyle.Custom && max.DashStyle==System.Drawing.Drawing2D.DashStyle.Custom)
				pen.DashPattern = (frac > 0.5 ? max.DashPattern : min.DashPattern);
			pen.DashOffset = (frac > 0.5 ? max.DashOffset : min.DashOffset);
			pen.DashCap = (frac > 0.5 ? max.DashCap : min.DashCap);
			if(min.CompoundArray.Length>0 && max.CompoundArray.Length>0)
				pen.CompoundArray = (frac > 0.5 ? max.CompoundArray : min.CompoundArray);
			pen.Alignment = (frac > 0.5 ? max.Alignment : min.Alignment);
			//pen.CustomStartCap = (frac > 0.5 ? max.CustomStartCap : min.CustomStartCap);  //Throws ArgumentException
			//pen.CustomEndCap = (frac > 0.5 ? max.CustomEndCap : min.CustomEndCap);  //Throws ArgumentException
			return pen;
		}

		private System.Drawing.Color InterpolateColor(System.Drawing.Color minCol, System.Drawing.Color maxCol, double attr)
		{
			double frac = Fraction(attr);
			if (frac==1)
				return maxCol;
			else if (frac==0)
				return minCol;
			else
			{
				double r = (maxCol.R - minCol.R) * frac + minCol.R;
				double g = (maxCol.G - minCol.G) * frac + minCol.G;
				double b = (maxCol.B - minCol.B) * frac + minCol.B;
				double a = (maxCol.A - minCol.A) * frac + minCol.A;
				if (r > 255) r = 255;
				if (g > 255) g = 255;
				if (b > 255) b = 255;
				if (a > 255) a = 255;
				return System.Drawing.Color.FromArgb((int)a, (int)r, (int)g, (int)b);
			}
		}
		#endregion
}
}
