/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Christian
 * Datum: 11.01.2007
 * Zeit: 20:38
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

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

		#region public properties

        /// <summary>
        /// returns value of the transform array
        /// </summary>
        /// <param name="i">place in array</param>
        /// <returns>value depedent on i</returns>
		public double this[int i]
		{
			get { return transform[i]; }
			set	{ transform[i] = value; }
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
        /// top value of the image
        /// </summary>
        public double Top
        {
            get { return transform[3]; }
        }       

        /// <summary>
        /// west to east pixel resolution
        /// </summary>
        public double HorizontalPixelResolution
        {
            get { return transform[1]; }
        }

        /// <summary>
        /// north to south pixel resolution
        /// </summary>
        public double VerticalPixelResolution
        {
            get { return transform[5]; }
        }


		#endregion

		#region constructors

        /// <summary>
        /// Constructor
        /// </summary>
		public GeoTransform()
        {
            transform = new double[6];
            transform[0] = 0; /* top left x */
            transform[1] = 1; /* w-e pixel resolution */
            transform[2] = 0; /* rotation, 0 if image is "north up" */
            transform[3] = 0; /* top left y */
            transform[4] = 0; /* rotation, 0 if image is "north up" */
            transform[5] = 1; /* n-s pixel resolution */
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

        /// <summary>
        /// converts latitude into pixel
        /// </summary>
        /// <param name="lat"></param>
        /// <returns></returns>
        public double PixelX(double lat)
        {
            return Math.Abs(transform[0] - lat) / transform[1];
        }
		
        /// <summary>
        /// converts longitude into pixel
        /// </summary>
        /// <param name="lon"></param>
        /// <returns></returns>
        public double PixelY(double lon)
        {
            return Math.Abs(Math.Abs(transform[3] - lon) / transform[5]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <returns></returns>
        public double PixelXwidth(double lat)
        {
            return Math.Abs(lat / transform[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lon"></param>
        /// <returns></returns>
        public double PixelYwidth(double lon)
        {
            return Math.Abs(lon / transform[5]);
        }


		#endregion
	}
}
