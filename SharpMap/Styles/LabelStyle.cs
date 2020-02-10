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
using System.Net.Mime;
using System.Runtime.CompilerServices;
using Common.Logging;
using SharpMap.Rendering.Thematics;

namespace SharpMap.Styles
{
    /// <summary>
    /// Defines a style used for rendering labels
    /// </summary>
    [Serializable]
    public class LabelStyle : Style, ICloneable
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof (LabelStyle));
        
        #region HorizontalAlignmentEnum enum

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

        #endregion

        #region VerticalAlignmentEnum enum

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

        #endregion

        private Brush _BackColor;
        private SizeF _CollisionBuffer;
        private bool _CollisionDetection;

        private Font _Font;

        private Color _ForeColor;
        private Pen _Halo;
        private HorizontalAlignmentEnum _HorizontalAlignment;
        private PointF _Offset;
        private VerticalAlignmentEnum _VerticalAlignment;
        private float _rotation;
        private bool _ignoreLength;
        private bool _isTextOnPath = false;
        /// <summary>
        /// get or set label on path
        /// </summary>
        [Obsolete("TextOnPath has been deprecated in favor of more efficient rendering techniques")]
        public bool IsTextOnPath
        {
            get { return _isTextOnPath; }
            set { _isTextOnPath = value; }
        }
        /// <summary>
        /// Initializes a new LabelStyle
        /// </summary>
        public LabelStyle()
        {
            _Font = new Font(FontFamily.GenericSerif, 12f);
            _Offset = new PointF(0, 0);
            _CollisionDetection = false;
            _CollisionBuffer = new Size(0, 0);
            _ForeColor = Color.Black;
            _HorizontalAlignment = HorizontalAlignmentEnum.Center;
            _VerticalAlignment = VerticalAlignmentEnum.Middle;
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (IsDisposed)
                return;

            if (_Font != null) _Font.Dispose();
            if (_Halo != null) _Halo.Dispose();
            if (_BackColor != null) _BackColor.Dispose();

            base.ReleaseManagedResources();
        }
        /// <summary>
        /// Method to create a deep copy of this <see cref="LabelStyle"/>
        /// </summary>
        /// <returns>A LabelStyle resembling this instance.</returns>
        public LabelStyle Clone()
        {
            var res = (LabelStyle) MemberwiseClone();

            lock (this)
            {
                if (_Font != null)
                    res.Font = (Font)_Font.Clone();

                if (_Halo != null)
                    res._Halo = (Pen)_Halo.Clone();

                if (_BackColor != null)
                    res._BackColor = (Brush) _BackColor.Clone();
            }

            return res;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Function to get a <see cref="StringFormat"/> with <see cref="StringFormat.Alignment"/> and <see cref="StringFormat.LineAlignment"/> properties according to 
        /// <see cref="HorizontalAlignment"/> and <see cref="VerticalAlignment"/>
        /// </summary>
        /// <returns>A <see cref="StringFormat"/></returns>
        internal StringFormat GetStringFormat()
        {
            var r = (StringFormat)StringFormat.GenericTypographic.Clone();
            switch (HorizontalAlignment)
            {
                case HorizontalAlignmentEnum.Center:
                    r.Alignment = StringAlignment.Center;
                    break;
                case HorizontalAlignmentEnum.Left:
                    r.Alignment = StringAlignment.Near;
                    break;
                case HorizontalAlignmentEnum.Right:
                    r.Alignment = StringAlignment.Far;
                    break;
            }
            switch (VerticalAlignment)
            {
                case VerticalAlignmentEnum.Middle:
                    r.LineAlignment = StringAlignment.Center;
                    break;
                case VerticalAlignmentEnum.Top:
                    r.LineAlignment = StringAlignment.Near;
                    break;
                case VerticalAlignmentEnum.Bottom:
                    r.LineAlignment = StringAlignment.Far;
                    break;
            }
            return r;
        }

        /// <summary>
        /// Label Font
        /// </summary>
        public Font Font
        {
            get
            {
                return _Font;
            }
            set
            {
                if (value.Unit != GraphicsUnit.Point)
                    _logger.Error( fmh => fmh("Only assign fonts with size in Points"));
                _Font = value;

                // we can't dispose the previous font 
                // because it could still be used by 
                // the rendering engine, so we just let it be GC'ed.
                _cachedFontForGraphics = null;
            }
        }

        [NonSerialized] 
        private float _cachedDpiY = 0f;
        [NonSerialized]
        private Font _cachedFontForGraphics = null;

        /// <summary>
        /// Method to create a font that ca
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Font GetFontForGraphics(Graphics g)
        {
            if (_cachedFontForGraphics == null || g.DpiY != _cachedDpiY)
            {
                _cachedDpiY = g.DpiY;
                if (_cachedFontForGraphics != null) _cachedFontForGraphics.Dispose();
                _cachedFontForGraphics = new Font(_Font.FontFamily, _Font.GetHeight(g), _Font.Style, GraphicsUnit.Pixel);
                /*
                var textHeight = _Font.Size;
                switch (_Font.Unit)
                {
                    case GraphicsUnit.Display:
                        textHeight *= g.DpiY / 75;
                        break;
                    case GraphicsUnit.Document:
                        textHeight *= g.DpiY / 300;
                        break;
                    case GraphicsUnit.Inch:
                        textHeight *= g.DpiY;
                        break;
                    case GraphicsUnit.Millimeter:
                        textHeight /= 25.4f * g.DpiY;
                        break;
                    case GraphicsUnit.Pixel:
                        //do nothing
                        break;
                    case GraphicsUnit.Point:
                        textHeight *= g.DpiY / 72;
                        break;
                }                
                _cachedFontForGraphics = new Font(_Font.FontFamily, textHeight, _Font.Style, GraphicsUnit.Pixel);
                 */
            }
            return _cachedFontForGraphics;
        }

        /// <summary>
        /// Font color
        /// </summary>
        public Color ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; }
        }

        /// <summary>
        /// The background color of the label. Set to transparent brush or null if background isn't needed
        /// </summary>
        public Brush BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }

        /// <summary>
        /// Creates a halo around the text
        /// </summary>
        [System.ComponentModel.Category("Uneditable")]
        [System.ComponentModel.Editor()]
        public Pen Halo
        {
            get { return _Halo; }
            set
            {
                _Halo = value;
                //if (_Halo != null)
                //    _Halo.LineJoin = LineJoin.Round;
            }
        }


        /// <summary>
        /// Specifies relative position of labels with respect to objects label point
        /// </summary>
        [System.ComponentModel.Category("Uneditable")]
        public PointF Offset
        {
            get { return _Offset; }
            set { _Offset = value; }
        }

        /// <summary>
        /// Gets or sets whether Collision Detection is enabled for the labels.
        /// If set to true, label collision will be tested.
        /// </summary>
        /// <remarks>Just setting this property in a <see cref="ITheme.GetStyle"/> method does not lead to the desired result. You must set it to for the whole layer using the default Style.</remarks>
        [System.ComponentModel.Category("Collision Detection")]
        public bool CollisionDetection
        {
            get { return _CollisionDetection; }
            set { _CollisionDetection = value; }
        }

        /// <summary>
        /// Distance around label where collision buffer is active
        /// </summary>
        [System.ComponentModel.Category("Collision Detection")]
        public SizeF CollisionBuffer
        {
            get { return _CollisionBuffer; }
            set { _CollisionBuffer = value; }
        }

        /// <summary>
        /// The horizontal alignment of the text in relation to the labelpoint
        /// </summary>
        [System.ComponentModel.Category("Alignment")]
        public HorizontalAlignmentEnum HorizontalAlignment
        {
            get { return _HorizontalAlignment; }
            set { _HorizontalAlignment = value; }
        }

        /// <summary>
        /// The horizontal alignment of the text in relation to the labelpoint
        /// </summary>
        [System.ComponentModel.Category("Alignment")]
        public VerticalAlignmentEnum VerticalAlignment
        {
            get { return _VerticalAlignment; }
            set { _VerticalAlignment = value; }
        }

        /// <summary>
        /// The Rotation of the text
        /// </summary>
        [System.ComponentModel.Category("Alignment")]
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value % 360f; }
        }

        /// <summary>
        /// Gets or sets if length of linestring should be ignored
        /// </summary>
        [System.ComponentModel.Category("Alignment")]
        public bool IgnoreLength
        {
            get { return _ignoreLength; }
            set { _ignoreLength = value; }
        }

    }
}
