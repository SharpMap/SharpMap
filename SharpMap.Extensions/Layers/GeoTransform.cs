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
using SharpMap.Geometries;

namespace SharpMap.Layers
{
    /// <summary>
    /// The coefficients for transforming between pixel/line (X,Y) raster space, and projection coordinates (Xp,Yp) space.<br/>
    /// Xp = T[0] + T[1]*X + T[2]*Y<br/>
    /// Yp = T[3] + T[4]*X + T[5]*Y<br/>
    /// In a north up image, T[1] is the pixel width, and T[5] is the pixel height.
    /// The upper left corner of the upper left pixel is at position (T[0],T[3]).
    /// </summary>
    internal class GeoTransform
    {
        private readonly double[] _inverseTransform = new double[6];
        internal double[] _transform = new double[6];

        #region public properties

        /// <summary>
        /// returns value of the transform array
        /// </summary>
        /// <param name="i">place in array</param>
        /// <returns>value depedent on i</returns>
        public double this[int i]
        {
            get { return _transform[i]; }
            set { _transform[i] = value; }
        }

        public double[] Inverse
        {
            get { return _inverseTransform; }
        }

        /// <summary>
        /// returns true if no values were fetched
        /// </summary>
        private bool IsTrivial
        {
            get
            {
                return _transform[0] == 0 && _transform[1] == 1 &&
                       _transform[2] == 0 && _transform[3] == 0 &&
                       _transform[4] == 0 && _transform[5] == 1;
            }
        }

        /// <summary>
        /// left value of the image
        /// </summary>       
        public double Left
        {
            get { return _transform[0]; }
        }


        /// <summary>
        /// right value of the image
        /// </summary>       
        //  public double Right
        //  {
        ///      get { return this.Left + (this.HorizontalPixelResolution * _gdalDataset.XSize); }
        //  }
        /// <summary>
        /// top value of the image
        /// </summary>
        public double Top
        {
            get { return _transform[3]; }
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
            get { return _transform[1]; }
            set { _transform[1] = value; }
        }

        /// <summary>
        /// north to south pixel resolution
        /// </summary>
        public double VerticalPixelResolution
        {
            get { return _transform[5]; }
            set { _transform[5] = value; }
        }

        public double XRotation
        {
            get { return _transform[2]; }
            set { _transform[2] = value; }
        }

        public double YRotation
        {
            get { return _transform[4]; }
            set { _transform[4] = value; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public GeoTransform()
        {
            _transform = new double[6];
            _transform[0] = 999.5; /* x */
            _transform[1] = 1; /* w-e pixel resolution */
            _transform[2] = 0; /* rotation, 0 if image is "north up" */
            _transform[3] = 1000.5; /* y */
            _transform[4] = 0; /* rotation, 0 if image is "north up" */
            _transform[5] = -1; /* n-s pixel resolution */
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
            _transform = array;
            CreateInverse();
        }

        private void CreateInverse()
        {
            double det;
            double[] tempSix = new double[6];

            // compute determinant
            det = _transform[1] * _transform[5] - _transform[2] * _transform[4];
            if (det == 0.0) return;

            // inverse rot/scale portion
            _inverseTransform[1] = _transform[5] / det;
            _inverseTransform[2] = -_transform[2] / det;
            _inverseTransform[4] = -_transform[4] / det;
            _inverseTransform[5] = _transform[1] / det;

            // compute translation elements
            _inverseTransform[0] = -_inverseTransform[1] * _transform[0] - _inverseTransform[2] * _transform[3];
            _inverseTransform[3] = -_inverseTransform[4] * _transform[0] - _inverseTransform[5] * _transform[3];
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
            return _transform[0] + _transform[1] * imgX + _transform[2] * imgY;
        }

        /// <summary>
        /// converts image point into projected point
        /// </summary>
        /// <param name="imgX">image x value</param>
        /// <param name="imgY">image y value</param>
        /// <returns>projected y coordinate</returns>
        public double ProjectedY(double imgX, double imgY)
        {
            return _transform[3] + _transform[4] * imgX + _transform[5] * imgY;
        }

        public Point ImageToGround(Point imagePoint)
        {
            Point groundPoint = new Point();

            groundPoint.X = _transform[0] + _transform[1] * imagePoint.X + _transform[2] * imagePoint.Y;
            groundPoint.Y = _transform[3] + _transform[4] * imagePoint.X + _transform[5] * imagePoint.Y;

            return groundPoint;
        }

        public Point GroundToImage(Point groundPoint)
        {
            Point imagePoint = new Point();

            imagePoint.X = _inverseTransform[0] + _inverseTransform[1] * groundPoint.X +
                           _inverseTransform[2] * groundPoint.Y;
            imagePoint.Y = _inverseTransform[3] + _inverseTransform[4] * groundPoint.X +
                           _inverseTransform[5] * groundPoint.Y;

            return imagePoint;
        }

        public double GndW(double imgWidth, double imgHeight)
        {
            // check for funky case
            if (_transform[2] < 0 && _transform[4] < 0 && _transform[5] < 0)
                return Math.Abs((_transform[1] * imgWidth) + (_transform[2] * imgHeight));
            else
                return Math.Abs((_transform[1] * imgWidth) - (_transform[2] * imgHeight));
        }

        public double GndH(double imgWidth, double imgHeight)
        {
            // check for funky case
            if (_transform[2] < 0 && _transform[4] < 0 && _transform[5] < 0)
                return Math.Abs((_transform[4] * imgWidth) - (_transform[5] * imgHeight));
            else
                return Math.Abs((_transform[4] * imgWidth) - (_transform[5] * imgHeight));
        }

        // finds leftmost pixel location (handles rotation)
        public double EnvelopeLeft(double imgWidth, double imgHeight)
        {
            double left;

            left = Math.Min(_transform[0], _transform[0] + (_transform[1] * imgWidth));
            left = Math.Min(left, _transform[0] + (_transform[2] * imgHeight));
            left = Math.Min(left, _transform[0] + (_transform[1] * imgWidth) + (_transform[2] * imgHeight));
            return left;
        }

        // finds rightmost pixel location (handles rotation)
        public double EnvelopeRight(double imgWidth, double imgHeight)
        {
            double right;

            right = Math.Max(_transform[0], _transform[0] + (_transform[1] * imgWidth));
            right = Math.Max(right, _transform[0] + (_transform[2] * imgHeight));
            right = Math.Max(right, _transform[0] + (_transform[1] * imgWidth) + (_transform[2] * imgHeight));
            return right;
        }

        // finds topmost pixel location (handles rotation)
        public double EnvelopeTop(double imgWidth, double imgHeight)
        {
            double top;

            top = Math.Max(_transform[3], _transform[3] + (_transform[4] * imgWidth));
            top = Math.Max(top, _transform[3] + (_transform[5] * imgHeight));
            top = Math.Max(top, _transform[3] + (_transform[4] * imgWidth) + (_transform[5] * imgHeight));
            return top;
        }

        // finds bottommost pixel location (handles rotation)
        public double EnvelopeBottom(double imgWidth, double imgHeight)
        {
            double bottom;

            bottom = Math.Min(_transform[3], _transform[3] + (_transform[4] * imgWidth));
            bottom = Math.Min(bottom, _transform[3] + (_transform[5] * imgHeight));
            bottom = Math.Min(bottom, _transform[3] + (_transform[4] * imgWidth) + (_transform[5] * imgHeight));
            return bottom;
        }

        // image was flipped horizontally
        public bool HorzFlip()
        {
            return _transform[4] > 0;
        }

        // image was flipped vertically
        public bool VertFlip()
        {
            return _transform[2] > 0;
        }

        public double PixelX(double lat)
        {
            return (_transform[0] - lat) / (_transform[1] - _transform[2]);
        }

        public double PixelY(double lon)
        {
            return Math.Abs(_transform[3] - lon) / (_transform[4] + _transform[5]);
        }

        public double PixelXwidth(double lat)
        {
            return Math.Abs(lat / (_transform[1] - _transform[2]));
        }

        public double PixelYwidth(double lon)
        {
            return Math.Abs(lon / (_transform[5] + _transform[4]));
        }

        public double RotationAngle()
        {
            if (_transform[5] != 0)
                return Math.Atan(_transform[2] / _transform[5]) * 57.2957795;

            return 0;
        }

        public bool IsFlipped()
        {
            if (_transform[5] > 0)
                return true;

            return false;
        }

        #endregion
    }
}