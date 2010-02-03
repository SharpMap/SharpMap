// Copyright 2006 - Morten Nielsen (www.iter.dk)
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

namespace SharpMap.Rendering.Thematics
{
    /// <summary>
    /// Defines arrays of colors and positions used for interpolating color blending in a multicolor gradient.
    /// </summary>
    /// <seealso cref="SharpMap.Rendering.Thematics.GradientTheme"/>
    public class ColorBlend
    {
        private Color[] _Colors;

        private float[] _Positions;
        private float _maximum = float.NaN;
        private float _minimum = float.NaN;

        internal ColorBlend()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ColorBlend class.
        /// </summary>
        /// <param name="colors">An array of Color structures that represents the colors to use at corresponding positions along a gradient.</param>
        /// <param name="positions">An array of values that specify percentages of distance along the gradient line.</param>
        public ColorBlend(Color[] colors, float[] positions)
        {
            _Colors = colors;
            Positions = positions;
        }

        /// <summary>
        /// Gets or sets an array of colors that represents the colors to use at corresponding positions along a gradient.
        /// </summary>
        /// <value>An array of <see cref="System.Drawing.Color"/> structures that represents the colors to use at corresponding positions along a gradient.</value>
        /// <remarks>
        /// This property is an array of <see cref="System.Drawing.Color"/> structures that represents the colors to use at corresponding positions
        /// along a gradient. Along with the Positions property, this property defines a multicolor gradient.
        /// </remarks>
        public Color[] Colors
        {
            get { return _Colors; }
            set { _Colors = value; }
        }

        /// <summary>
        /// Gets or sets the positions along a gradient line.
        /// </summary>
        /// <value>An array of values that specify percentages of distance along the gradient line.</value>
        /// <remarks>
        /// <para>The elements of this array specify percentages of distance along the gradient line.
        /// For example, an element value of 0.2f specifies that this point is 20 percent of the total
        /// distance from the starting point. The elements in this array are represented by float
        /// values between 0.0f and 1.0f, and the first element of the array must be 0.0f and the
        /// last element must be 1.0f.</para>
        /// <pre>Along with the Colors property, this property defines a multicolor gradient.</pre>
        /// </remarks>
        public float[] Positions
        {
            get { return _Positions; }
            set 
            { 
                _Positions = value;
                if ( value == null )
                    _minimum = _maximum = float.NaN;
                else
                {
                    _minimum = value[0];
                    _maximum = value[value.GetUpperBound(0)];
                }
            }
        }

        /// <summary>
        /// Gets the color from the scale at position 'pos'.
        /// </summary>
        /// <remarks>If the position is outside the scale [0..1] only the fractional part
        /// is used (in other words the scale restarts for each integer-part).</remarks>
        /// <param name="pos">Position on scale between 0.0f and 1.0f</param>
        /// <returns>Color on scale</returns>
        public Color GetColor(float pos)
        {
            if (float.IsNaN(_minimum))
                throw (new ArgumentException("Positions not set"));
            if (_Colors.Length != _Positions.Length)
                throw (new ArgumentException("Colors and Positions arrays must be of equal length"));
            if (_Colors.Length < 2)
                throw (new ArgumentException("At least two colors must be defined in the ColorBlend"));
            /*
            if (_Positions[0] != 0f)
                throw (new ArgumentException("First position value must be 0.0f"));
            if (_Positions[_Positions.Length - 1] != 1f)
                throw (new ArgumentException("Last position value must be 1.0f"));
            if (pos > 1 || pos < 0) pos -= (float) Math.Floor(pos);
             */
            int i = 1;
            while (i < _Positions.Length && _Positions[i] < pos)
                i++;
            float frac = (pos - _Positions[i - 1])/(_Positions[i] - _Positions[i - 1]);
            frac = Math.Max(frac, 0.0f);
            frac = Math.Min(frac, 1.0f);
            int R = (int) Math.Round((_Colors[i - 1].R*(1 - frac) + _Colors[i].R*frac));
            int G = (int) Math.Round((_Colors[i - 1].G*(1 - frac) + _Colors[i].G*frac));
            int B = (int) Math.Round((_Colors[i - 1].B*(1 - frac) + _Colors[i].B*frac));
            int A = (int) Math.Round((_Colors[i - 1].A*(1 - frac) + _Colors[i].A*frac));
            return Color.FromArgb(A, R, G, B);
        }

        /// <summary>
        /// Converts the color blend to a gradient brush
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public LinearGradientBrush ToBrush(Rectangle rectangle, float angle)
        {
            LinearGradientBrush br = new LinearGradientBrush(rectangle, Color.Black, Color.Black, angle, true);
            System.Drawing.Drawing2D.ColorBlend cb = new System.Drawing.Drawing2D.ColorBlend();
            cb.Colors = _Colors;
            //scale and translate positions to range[0.0, 1.0]
            float[] positions = new float[_Positions.Length];
            float range = _maximum - _minimum;
            for (int i = 0; i < _Positions.Length; i++)
                positions[i] = (_Positions[i] - _minimum) / range;
            cb.Positions = positions;
            br.InterpolationColors = cb;
            return br;
        }

        #region Predefined color scales

        /// <summary>
        /// Gets a linear gradient scale with seven colours making a rainbow from red to violet.
        /// </summary>
        /// <remarks>
        /// Colors span the following with an interval of 1/6:
        /// { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet }
        /// </remarks>
        public static ColorBlend Rainbow7
        {
            get
            {
                ColorBlend cb = new ColorBlend();
                cb._Positions = new float[7];
                for (int i = 1; i < 7; i++)
                    cb.Positions[i] = i/6f;
                cb.Colors = new[]
                                {
                                    Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo,
                                    Color.Violet
                                };
                return cb;
            }
        }

        /// <summary>
        /// Gets a linear gradient scale with five colours making a rainbow from red to blue.
        /// </summary>
        /// <remarks>
        /// Colors span the following with an interval of 0.25:
        /// { Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue }
        /// </remarks>
        public static ColorBlend Rainbow5
        {
            get
            {
                return new ColorBlend(
                    new[] {Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue},
                    new[] {0f, 0.25f, 0.5f, 0.75f, 1f});
            }
        }

        /// <summary>
        /// Gets a linear gradient scale from black to white
        /// </summary>
        public static ColorBlend BlackToWhite
        {
            get { return new ColorBlend(new[] {Color.Black, Color.White}, new[] {0f, 1f}); }
        }

        /// <summary>
        /// Gets a linear gradient scale from white to black
        /// </summary>
        public static ColorBlend WhiteToBlack
        {
            get { return new ColorBlend(new[] {Color.White, Color.Black}, new[] {0f, 1f}); }
        }

        /// <summary>
        /// Gets a linear gradient scale from red to green
        /// </summary>
        public static ColorBlend RedToGreen
        {
            get { return new ColorBlend(new[] {Color.Red, Color.Green}, new[] {0f, 1f}); }
        }

        /// <summary>
        /// Gets a linear gradient scale from green to red
        /// </summary>
        public static ColorBlend GreenToRed
        {
            get { return new ColorBlend(new[] {Color.Green, Color.Red}, new[] {0f, 1f}); }
        }

        /// <summary>
        /// Gets a linear gradient scale from blue to green
        /// </summary>
        public static ColorBlend BlueToGreen
        {
            get { return new ColorBlend(new[] {Color.Blue, Color.Green}, new[] {0f, 1f}); }
        }

        /// <summary>
        /// Gets a linear gradient scale from green to blue
        /// </summary>
        public static ColorBlend GreenToBlue
        {
            get { return new ColorBlend(new[] {Color.Green, Color.Blue}, new[] {0f, 1f}); }
        }

        /// <summary>
        /// Gets a linear gradient scale from red to blue
        /// </summary>
        public static ColorBlend RedToBlue
        {
            get { return new ColorBlend(new[] {Color.Red, Color.Blue}, new[] {0f, 1f}); }
        }

        /// <summary>
        /// Gets a linear gradient scale from blue to red
        /// </summary>
        public static ColorBlend BlueToRed
        {
            get { return new ColorBlend(new[] {Color.Blue, Color.Red}, new[] {0f, 1f}); }
        }

        #endregion

        #region Constructor helpers

        /// <summary>
        /// Creates a linear gradient scale from two colors
        /// </summary>
        /// <param name="fromColor"></param>
        /// <param name="toColor"></param>
        /// <returns></returns>
        public static ColorBlend TwoColors(Color fromColor, Color toColor)
        {
            return new ColorBlend(new[] {fromColor, toColor}, new[] {0f, 1f});
        }

        /// <summary>
        /// Creates a linear gradient scale from three colors
        /// </summary>
        public static ColorBlend ThreeColors(Color fromColor, Color middleColor, Color toColor)
        {
            return new ColorBlend(new[] {fromColor, middleColor, toColor}, new[] {0f, 0.5f, 1f});
        }

        #endregion
    }
}