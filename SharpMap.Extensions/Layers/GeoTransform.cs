// Copyright 2007: Christian Graefe
// Copyright 2008: Dan Brecht and Joel Wilson
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
using GeoAPI.Geometries;
using Point = GeoAPI.Geometries.Coordinate;

namespace SharpMap.Layers
{
    /// <summary>
    /// The coefficients for transforming between pixel/line (X,Y) raster space, and projection coordinates (Xp,Yp) space.<br/>
    /// Xp = T[0] + T[1]*X + T[2]*Y<br/>
    /// Yp = T[3] + T[4]*X + T[5]*Y<br/>
    /// In a north up image, T[1] is the pixel width, and T[5] is the pixel height.
    /// The upper left corner of the upper left pixel is at position (T[0],T[3]).
    /// </summary>
    [Serializable]
    internal class GeoTransform
    {
        private readonly double[] _inverseTransform = new double[6];
        internal double[] Transform = new double[6];

        #region public properties

        /// <summary>
        /// returns value of the transform array
        /// </summary>
        /// <param name="i">place in array</param>
        /// <returns>value dependent on i</returns>
        public double this[int i]
        {
            get { return Transform[i]; }
            set { Transform[i] = value; }
        }

        public double[] Inverse
        {
            get { return _inverseTransform; }
        }

        /// <summary>
        /// Returns true if no values were fetched
        /// </summary>
        public bool IsTrivial
        {
            get
            {
                return Transform[0] == 0 && Transform[1] == 1 &&
                       Transform[2] == 0 && Transform[3] == 0 &&
                       Transform[4] == 0 && Transform[5] == 1;
            }
        }

        /// <summary>
        /// left value of the image
        /// </summary>       
        public double Left
        {
            get { return Transform[0]; }
        }


        /*
        /// <summary>
        /// right value of the image
        /// </summary>       
        public double Right
        {
              get { return this.Left + (this.HorizontalPixelResolution * _gdalDataset.XSize); }
        }
        */

        /// <summary>
        /// top value of the image
        /// </summary>
        public double Top
        {
            get { return Transform[3]; }
        }

        /// <summary>
        /// bottom value of the image
        /// </summary>
        // public double Bottom
        // {
        //   get { return this.Top + (this.VerticalPixelResolution * _gdalDataset.YSize); }
        //}
        /// <summary>
        /// west to east pixel resolution
        /// </summary>
        public double HorizontalPixelResolution
        {
            get { return Transform[1]; }
            set { Transform[1] = value; }
        }

        /// <summary>
        /// north to south pixel resolution
        /// </summary>
        public double VerticalPixelResolution
        {
            get { return Transform[5]; }
            set { Transform[5] = value; }
        }

        public double XRotation
        {
            get { return Transform[2]; }
            set { Transform[2] = value; }
        }

        public double YRotation
        {
            get { return Transform[4]; }
            set { Transform[4] = value; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public GeoTransform()
        {
            Transform = new double[6];
            Transform[0] = 999.5; /* x */
            Transform[1] = 1; /* w-e pixel resolution */
            Transform[2] = 0; /* rotation, 0 if image is "north up" */
            Transform[3] = 1000.5; /* y */
            Transform[4] = 0; /* rotation, 0 if image is "north up" */
            Transform[5] = -1; /* n-s pixel resolution */
            CreateInverse();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array"></param>
        public GeoTransform(double[] array)
        {
            if (array.Length != 6)
                throw new ApplicationException("GeoTransform constructor invoked with invalid sized array");
            Transform = array;
            CreateInverse();
        }

        private void CreateInverse()
        {
            // compute determinant
            var det = Transform[1] * Transform[5] - Transform[2] * Transform[4];
            if (det == 0.0) return;

            // inverse rot/scale portion
            _inverseTransform[1] = Transform[5] / det;
            _inverseTransform[2] = -Transform[2] / det;
            _inverseTransform[4] = -Transform[4] / det;
            _inverseTransform[5] = Transform[1] / det;

            // compute translation elements
            _inverseTransform[0] = -_inverseTransform[1] * Transform[0] - _inverseTransform[2] * Transform[3];
            _inverseTransform[3] = -_inverseTransform[4] * Transform[0] - _inverseTransform[5] * Transform[3];
        }

        #endregion

        #region public methods

        /// <summary>
        /// converts image point into projected point
        /// </summary>
        /// <param name="imgX">image x value</param>
        /// <param name="imgY">image y value</param>
        /// <returns>projected x coordinate</returns>
        public double ProjectedX(double imgX, double imgY)
        {
            return Transform[0] + Transform[1] * imgX + Transform[2] * imgY;
        }

        /// <summary>
        /// converts image point into projected point
        /// </summary>
        /// <param name="imgX">image x value</param>
        /// <param name="imgY">image y value</param>
        /// <returns>projected y coordinate</returns>
        public double ProjectedY(double imgX, double imgY)
        {
            return Transform[3] + Transform[4] * imgX + Transform[5] * imgY;
        }

        public Coordinate ImageToGround(Coordinate imagePoint)
        {
            return new Point
                       {
                           X = Transform[0] + Transform[1]*imagePoint.X + Transform[2]*imagePoint.Y,
                           Y = Transform[3] + Transform[4]*imagePoint.X + Transform[5]*imagePoint.Y
                       };
        }

        public Point GroundToImage(Point groundPoint)
        {
            return new Point
                       {
                           X = _inverseTransform[0] + _inverseTransform[1]*groundPoint.X +
                               _inverseTransform[2]*groundPoint.Y,
                           Y = _inverseTransform[3] + _inverseTransform[4]*groundPoint.X +
                               _inverseTransform[5]*groundPoint.Y
                       };
        }

        public double GndW(double imgWidth, double imgHeight)
        {
            // check for funky case
            if (Transform[2] < 0 && Transform[4] < 0 && Transform[5] < 0)
                return Math.Abs((Transform[1] * imgWidth) + (Transform[2] * imgHeight));
            return Math.Abs((Transform[1] * imgWidth) - (Transform[2] * imgHeight));
        }

        public double GndH(double imgWidth, double imgHeight)
        {
            // check for funky case
            if (Transform[2] < 0 && Transform[4] < 0 && Transform[5] < 0)
                return Math.Abs((Transform[4] * imgWidth) - (Transform[5] * imgHeight));
            return Math.Abs((Transform[4] * imgWidth) - (Transform[5] * imgHeight));
        }

        // finds leftmost pixel location (handles rotation)
        public double EnvelopeLeft(double imgWidth, double imgHeight)
        {
            var left = Math.Min(Transform[0], Transform[0] + (Transform[1] * imgWidth));
            left = Math.Min(left, Transform[0] + (Transform[2] * imgHeight));
            left = Math.Min(left, Transform[0] + (Transform[1] * imgWidth) + (Transform[2] * imgHeight));
            return left;
        }

        // finds rightmost pixel location (handles rotation)
        public double EnvelopeRight(double imgWidth, double imgHeight)
        {
            var right = Math.Max(Transform[0], Transform[0] + (Transform[1] * imgWidth));
            right = Math.Max(right, Transform[0] + (Transform[2] * imgHeight));
            right = Math.Max(right, Transform[0] + (Transform[1] * imgWidth) + (Transform[2] * imgHeight));
            return right;
        }

        // finds topmost pixel location (handles rotation)
        public double EnvelopeTop(double imgWidth, double imgHeight)
        {
            var top = Math.Max(Transform[3], Transform[3] + (Transform[4] * imgWidth));
            top = Math.Max(top, Transform[3] + (Transform[5] * imgHeight));
            top = Math.Max(top, Transform[3] + (Transform[4] * imgWidth) + (Transform[5] * imgHeight));
            return top;
        }

        // finds bottommost pixel location (handles rotation)
        public double EnvelopeBottom(double imgWidth, double imgHeight)
        {
            var bottom = Math.Min(Transform[3], Transform[3] + (Transform[4] * imgWidth));
            bottom = Math.Min(bottom, Transform[3] + (Transform[5] * imgHeight));
            bottom = Math.Min(bottom, Transform[3] + (Transform[4] * imgWidth) + (Transform[5] * imgHeight));
            return bottom;
        }

        // image was flipped horizontally
        public bool HorzFlip()
        {
            return Transform[4] > 0;
        }

        // image was flipped vertically
        public bool VertFlip()
        {
            return Transform[2] > 0;
        }

        public double PixelX(double lat)
        {
            return (Transform[0] - lat) / (Transform[1] - Transform[2]);
        }

        public double PixelY(double lon)
        {
            return Math.Abs(Transform[3] - lon) / (Transform[4] + Transform[5]);
        }

        public double PixelXwidth(double lat)
        {
            return Math.Abs(lat / (Transform[1] - Transform[2]));
        }

        public double PixelYwidth(double lon)
        {
            return Math.Abs(lon / (Transform[5] + Transform[4]));
        }

        public double RotationAngle()
        {
            if (Transform[5] != 0)
                return Math.Atan(Transform[2] / Transform[5]) * 57.2957795;

            return 0;
        }

        public bool IsFlipped()
        {
            return Transform[5] > 0;
        }

        #endregion
    }
}