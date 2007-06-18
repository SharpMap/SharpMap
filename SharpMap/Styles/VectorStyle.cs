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

namespace SharpMap.Styles
{
	/// <summary>
	/// Defines a style used for rendering vector data
	/// </summary>
	public class VectorStyle : Style
	{
		#region Privates
		private System.Drawing.Pen _LineStyle;
		private System.Drawing.Pen _OutlineStyle;
		private bool _Outline;
		private System.Drawing.Brush _FillStyle;
		private System.Drawing.Bitmap _Symbol;
		#endregion

		/// <summary>
		/// Initializes a new VectorStyle and sets the default values
		/// </summary>
		/// <remarks>
		/// Default style values when initialized:<br/>
		/// *LineStyle: 1px solid black<br/>
		/// *FillStyle: Solid black<br/>
		/// *Outline: No Outline
		/// *Symbol: null-reference
		/// </remarks>
		public VectorStyle()
		{
			this.Outline = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
			this.Line = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
			this.Fill = System.Drawing.Brushes.Black;
			this.EnableOutline = false;
			this.SymbolScale = 1f;
		}

		#region Properties

		/// <summary>
		/// Linestyle for line geometries
		/// </summary>
		public System.Drawing.Pen Line
		{
			get { return _LineStyle; }
			set { _LineStyle = value; }
		}

		/// <summary>
		/// Outline style for line and polygon geometries
		/// </summary>
		public System.Drawing.Pen Outline
		{
			get { return _OutlineStyle; }
			set { _OutlineStyle = value; }
		}

		/// <summary>
		/// Specified whether the objects are rendered with or without outlining
		/// </summary>
		public bool EnableOutline
		{
			get { return _Outline; }
			set { _Outline = value; }
		}

		/// <summary>
		/// Fillstyle for Polygon geometries
		/// </summary>
		public System.Drawing.Brush Fill
		{
			get { return _FillStyle; }
			set { _FillStyle = value; }
		}
		
		/// <summary>
		/// Symbol used for rendering points
		/// </summary>
		public System.Drawing.Bitmap Symbol
		{
			get { return _Symbol; }
			set { _Symbol = value; }
		}
		private float _SymbolScale;

		/// <summary>
		/// Scale of the symbol (defaults to 1)
		/// </summary>
		/// <remarks>
		/// Setting the symbolscale to '2.0' doubles the size of the symbol, where a scale of 0.5 makes the scale half the size of the original image
		/// </remarks>
		public float SymbolScale
		{
			get { return _SymbolScale; }
			set { _SymbolScale = value; }
		}

		private System.Drawing.PointF _SymbolOffset;

		/// <summary>
		/// Gets or sets the offset in pixels of the symbol.
		/// </summary>
		/// <remarks>
		/// The symbol offset is scaled with the <see cref="SymbolScale"/> property and refers to the offset af <see cref="SymbolScale"/>=1.0.
		/// </remarks>
		public System.Drawing.PointF SymbolOffset
		{
			get { return _SymbolOffset; }
			set { _SymbolOffset = value; }
		}

		private float _SymbolRotation;

		/// <summary>
		/// Gets or sets the rotation of the symbol in degrees (clockwise is positive)
		/// </summary>
		public float SymbolRotation
		{
			get { return _SymbolRotation; }
			set { _SymbolRotation = value; }
		}
	

		#endregion
	}
}
