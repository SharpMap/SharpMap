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
    internal double[] transform = new double[6];
    private double[] inverseTransform = new double[6];

    #region public properties

    /// <summary>
    /// returns value of the transform array
    /// </summary>
    /// <param name="i">place in array</param>
    /// <returns>value depedent on i</returns>
    public double this[int i]
    {
      get { return transform[i]; }
      set { transform[i] = value; }
    }

    public double[] Inverse
    {
      get { return inverseTransform; }
    }

    /// <summary>
    /// returns true if no values were fetched
    /// </summary>
    bool IsTrivial
    {
      get
      {
        return transform[0] == 0 && transform[1] == 1 &&
            transform[2] == 0 && transform[3] == 0 &&
            transform[4] == 0 && transform[5] == 1;
      }
    }

    /// <summary>
    /// left value of the image
    /// </summary>       
    public double Left
    {
      get { return transform[0]; }
    }


    /// <summary>
    /// right value of the image
    /// </summary>       
    //  public double Right
    //  {
    ///      get { return this.Left + (this.HorizontalPixelResolution * _GdalDataset.XSize); }
    //  }

    /// <summary>
    /// top value of the image
    /// </summary>
    public double Top
    {
      get { return transform[3]; }
    }

    /// <summary>
    /// bottom value of the image
    /// </summary>
    // public double Bottom
    // {
    //   get { return this.Top + (this.VerticalPixelResolution * _GdalDataset.YSize); }
    //}

    /// <summary>
    /// west to east pixel resolution
    /// </summary>
    public double HorizontalPixelResolution
    {
      get { return transform[1]; }
      set { transform[1] = value; }
    }

    /// <summary>
    /// north to south pixel resolution
    /// </summary>
    public double VerticalPixelResolution
    {
      get { return transform[5]; }
      set { transform[5] = value; }
    }

    public double XRotation
    {
      get { return transform[2]; }
      set { transform[2] = value; }
    }

    public double YRotation
    {
      get { return transform[4]; }
      set { transform[4] = value; }
    }


    #endregion

    #region constructors

    /// <summary>
    /// Constructor
    /// </summary>
    public GeoTransform()
    {
      transform = new double[6];
      transform[0] = 999.5; /* x */
      transform[1] = 1; /* w-e pixel resolution */
      transform[2] = 0; /* rotation, 0 if image is "north up" */
      transform[3] = 1000.5; /* y */
      transform[4] = 0; /* rotation, 0 if image is "north up" */
      transform[5] = -1; /* n-s pixel resolution */
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
      transform = array;
      CreateInverse();
    }

    private void CreateInverse()
    {
      double det;
      double[] tempSix = new double[6];

      // compute determinant
      det = transform[1] * transform[5] - transform[2] * transform[4];
      if (det == 0.0) return;

      // inverse rot/scale portion
      inverseTransform[1] = transform[5] / det;
      inverseTransform[2] = -transform[2] / det;
      inverseTransform[4] = -transform[4] / det;
      inverseTransform[5] = transform[1] / det;

      // compute translation elements
      inverseTransform[0] = -inverseTransform[1] * transform[0] - inverseTransform[2] * transform[3];
      inverseTransform[3] = -inverseTransform[4] * transform[0] - inverseTransform[5] * transform[3];

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
      return transform[0] + transform[1] * imgX + transform[2] * imgY;
    }

    /// <summary>
    /// converts image point into projected point
    /// </summary>
    /// <param name="imgX">image x value</param>
    /// <param name="imgY">image y value</param>
    /// <returns>projected y coordinate</returns>
    public double ProjectedY(double imgX, double imgY)
    {
      return transform[3] + transform[4] * imgX + transform[5] * imgY;
    }

    public Geometries.Point ImageToGround(Geometries.Point imagePoint)
    {
      Geometries.Point groundPoint = new Geometries.Point();

      groundPoint.X = transform[0] + transform[1] * imagePoint.X + transform[2] * imagePoint.Y;
      groundPoint.Y = transform[3] + transform[4] * imagePoint.X + transform[5] * imagePoint.Y;

      return groundPoint;
    }

    public Geometries.Point GroundToImage(Geometries.Point groundPoint)
    {
      Geometries.Point imagePoint = new Geometries.Point();

      imagePoint.X = inverseTransform[0] + inverseTransform[1] * groundPoint.X + inverseTransform[2] * groundPoint.Y;
      imagePoint.Y = inverseTransform[3] + inverseTransform[4] * groundPoint.X + inverseTransform[5] * groundPoint.Y;

      return imagePoint;
    }

    public double GndW(double ImgWidth, double ImgHeight)
    {
      // check for funky case
      if (transform[2] < 0 && transform[4] < 0 && transform[5] < 0)
        return Math.Abs((transform[1] * ImgWidth) + (transform[2] * ImgHeight));
      else
        return Math.Abs((transform[1] * ImgWidth) - (transform[2] * ImgHeight));
    }

    public double GndH(double ImgWidth, double ImgHeight)
    {
      // check for funky case
      if (transform[2] < 0 && transform[4] < 0 && transform[5] < 0)
        return Math.Abs((transform[4] * ImgWidth) - (transform[5] * ImgHeight));
      else
        return Math.Abs((transform[4] * ImgWidth) - (transform[5] * ImgHeight));
    }

    // finds leftmost pixel location (handles rotation)
    public double EnvelopeLeft(double ImgWidth, double ImgHeight)
    {
      double left;

      left = Math.Min(transform[0], transform[0] + (transform[1] * ImgWidth));
      left = Math.Min(left, transform[0] + (transform[2] * ImgHeight));
      left = Math.Min(left, transform[0] + (transform[1] * ImgWidth) + (transform[2] * ImgHeight));
      return left;
    }

    // finds rightmost pixel location (handles rotation)
    public double EnvelopeRight(double ImgWidth, double ImgHeight)
    {
      double right;

      right = Math.Max(transform[0], transform[0] + (transform[1] * ImgWidth));
      right = Math.Max(right, transform[0] + (transform[2] * ImgHeight));
      right = Math.Max(right, transform[0] + (transform[1] * ImgWidth) + (transform[2] * ImgHeight));
      return right;
    }

    // finds topmost pixel location (handles rotation)
    public double EnvelopeTop(double ImgWidth, double ImgHeight)
    {
      double top;

      top = Math.Max(transform[3], transform[3] + (transform[4] * ImgWidth));
      top = Math.Max(top, transform[3] + (transform[5] * ImgHeight));
      top = Math.Max(top, transform[3] + (transform[4] * ImgWidth) + (transform[5] * ImgHeight));
      return top;
    }

    // finds bottommost pixel location (handles rotation)
    public double EnvelopeBottom(double ImgWidth, double ImgHeight)
    {
      double bottom;

      bottom = Math.Min(transform[3], transform[3] + (transform[4] * ImgWidth));
      bottom = Math.Min(bottom, transform[3] + (transform[5] * ImgHeight));
      bottom = Math.Min(bottom, transform[3] + (transform[4] * ImgWidth) + (transform[5] * ImgHeight));
      return bottom;
    }

    // image was flipped horizontally
    public bool HorzFlip()
    {
      return transform[4] > 0;
    }

    // image was flipped vertically
    public bool VertFlip()
    {
      return transform[2] > 0;
    }

    public double PixelX(double lat)
    {
      return (transform[0] - lat) / (transform[1] - transform[2]);
    }

    public double PixelY(double lon)
    {
      return Math.Abs(transform[3] - lon) / (transform[4] + transform[5]);
    }

    public double PixelXwidth(double lat)
    {
      return Math.Abs(lat / (transform[1] - transform[2]));
    }

    public double PixelYwidth(double lon)
    {
      return Math.Abs(lon / (transform[5] + transform[4]));
    }

    public double RotationAngle()
    {
      if (transform[5] != 0)
        return Math.Atan(transform[2] / transform[5]) * 57.2957795;
      else
        return 0;
    }

    public bool IsFlipped()
    {
      if (transform[5] > 0)
        return true;
      else
        return false;
    }


    #endregion
  }
}
