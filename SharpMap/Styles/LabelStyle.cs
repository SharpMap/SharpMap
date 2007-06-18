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
	/// Defines a style used for rendering labels
	/// </summary>
	public class LabelStyle : Style
	{
		/// <summary>
		/// Initializes a new LabelStyle
		/// </summary>
		public LabelStyle()
		{
			_Font = new System.Drawing.Font("Times New Roman", 12f);
			_Offset = new System.Drawing.PointF(0, 0);
			_CollisionDetection = false;
			_CollisionBuffer = new System.Drawing.Size(0, 0);
			_ForeColor = System.Drawing.Color.Black;
			_HorisontalAlignment = HorizontalAlignmentEnum.Center;
			_VerticalAlignment = VerticalAlignmentEnum.Middle;
		}

		private System.Drawing.Font _Font;

		/// <summary>
		/// Label Font
		/// </summary>
		public System.Drawing.Font Font
		{
			get { return _Font; }
			set { _Font = value; }
		}

		private System.Drawing.Color _ForeColor;

		/// <summary>
		/// Font color
		/// </summary>
		public System.Drawing.Color ForeColor
		{
			get { return _ForeColor; }
			set { _ForeColor = value; }
		}

		private System.Drawing.Brush _BackColor;

		/// <summary>
		/// The background color of the label. Set to transparent brush or null if background isn't needed
		/// </summary>
		public System.Drawing.Brush BackColor
		{
			get { return _BackColor; }
			set { _BackColor = value; }
		}

		private System.Drawing.Pen _Halo;
		/// <summary>
		/// Creates a halo around the text
		/// </summary>
		public System.Drawing.Pen Halo
		{
			get { return _Halo; }
			set { _Halo = value; }
		}

	
		private System.Drawing.PointF _Offset;

		/// <summary>
		/// Specifies relative position of labels with respect to objects label point
		/// </summary>
		public System.Drawing.PointF Offset
		{
			get { return _Offset; }
			set { _Offset = value; }
		}
		private bool _CollisionDetection;

		/// <summary>
		/// Gets or sets whether Collision Detection is enabled for the labels.
		/// If set to true, label collision will be tested.
		/// </summary>
		public bool CollisionDetection
		{
			get { return _CollisionDetection; }
			set { _CollisionDetection = value; }
		}

		private System.Drawing.SizeF _CollisionBuffer;

		/// <summary>
		/// Distance around label where collision buffer is active
		/// </summary>
		public System.Drawing.SizeF CollisionBuffer
		{
			get { return _CollisionBuffer; }
			set { _CollisionBuffer = value; }
		}

		private HorizontalAlignmentEnum _HorisontalAlignment;
		private VerticalAlignmentEnum _VerticalAlignment;

		/// <summary>
		/// The horisontal alignment of the text in relation to the labelpoint
		/// </summary>
		public HorizontalAlignmentEnum HorizontalAlignment
		{
			get { return _HorisontalAlignment; }
			set { _HorisontalAlignment = value; }
		}
		/// <summary>
		/// The horisontal alignment of the text in relation to the labelpoint
		/// </summary>
		public VerticalAlignmentEnum VerticalAlignment
		{
			get { return _VerticalAlignment; }
			set { _VerticalAlignment = value; }
		}
		/// <summary>
		/// Label text alignment
		/// </summary>
		public enum HorizontalAlignmentEnum : short
		{ 
			/// <summary>
			/// Left oriented
			/// </summary>
			Left = 0,
			/// <summary>
			/// Right oriented
			/// </summary>
			Right = 2,
			/// <summary>
			/// Centered
			/// </summary>
			Center = 1
		}

		/// <summary>
		/// Label text alignment
		/// </summary>
		public enum VerticalAlignmentEnum : short
		{
			/// <summary>
			/// Left oriented
			/// </summary>
			Bottom = 0,
			/// <summary>
			/// Right oriented
			/// </summary>
			Top = 2,
			/// <summary>
			/// Centered
			/// </summary>
			Middle = 1
		}
	}
}
