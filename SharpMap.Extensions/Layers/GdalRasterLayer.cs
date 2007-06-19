using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SharpMap.Layers
{
    /// <summary>
    /// Gdal raster image layer
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code lang="C#">
    /// myMap = new SharpMap.Map(new System.Drawing.Size(500,250);
    /// SharpMap.Layers.GdalRasterLayer layGdal = new SharpMap.Layers.GdalRasterLayer("Blue Marble", @"C:\data\srtm30plus.tif");
    /// myMap.Layers.Add(layGdal);
    /// myMap.ZoomToExtents();
    /// </code>
    /// </example>
    /// </remarks>

    public class GdalRasterLayer : SharpMap.Layers.Layer, IDisposable
    {
        private SharpMap.Geometries.BoundingBox _Envelope;
        private OSGeo.GDAL.Dataset _GdalDataset;
        private System.Drawing.Size imagesize;

        private string _Filename;
        /// <summary>
        /// Gets or sets the filename of the raster file
        /// </summary>
        public string Filename
        {
            get { return _Filename; }
            set { _Filename = value; }
        }

        /// <summary>
        /// initialize a Gdal based raster layer
        /// </summary>
        /// <param name="strLayerName">Name of layer</param>
        /// <param name="imageFilename">location of image</param>
        public GdalRasterLayer(string strLayerName, string imageFilename)
        {
            this.LayerName = strLayerName;
            this.Filename = imageFilename;
            disposed = false;

            OSGeo.GDAL.Gdal.AllRegister();
            try
            {
                _GdalDataset = OSGeo.GDAL.Gdal.Open(_Filename, OSGeo.GDAL.Access.GA_ReadOnly);
                imagesize = new Size(_GdalDataset.RasterXSize, _GdalDataset.RasterYSize);

                _Envelope = this.GetExtent();
            }
            catch (Exception ex)
            {
                _GdalDataset = null;
                throw new Exception("Couldn't load dataset. " + ex.Message + ex.InnerException);
            }

        }

        /// <summary>
        /// initialize a Gdal based raster layer
        /// </summary>
        /// <param name="strLayerName">Name of layer</param>
        /// <param name="imageFilename">location of image</param>
        /// <param name="srid">sets the SRID of the data set</param>
        public GdalRasterLayer(string strLayerName, string imageFilename, int srid)
            : this(strLayerName, imageFilename)
        {
            this.SRID = srid;
        }


        #region ILayer Members

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            if (disposed)
                throw (new ApplicationException("Error: An attempt was made to render a disposed layer"));

            //if (this.Envelope.Intersects(map.Envelope))
            //{
            this.GetPreview(_GdalDataset, map.Size, g, map.Envelope);
            //}
            base.Render(g, map);
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>

        public override SharpMap.Geometries.BoundingBox Envelope
        {
            get { return _Envelope; }
        }

        private int _SRID = -1;
        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public override int SRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clones the object
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Disposers and finalizers

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    if (_GdalDataset != null)
                    {
                        try
                        {
                            _GdalDataset.Dispose();
                        }
                        finally { _GdalDataset = null; }
                    }
                disposed = true;
            }
        }
        /// <summary>
        /// Disposes the GdalRasterLayer and release the raster file
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Finalizer
        /// </summary>
        ~GdalRasterLayer()
        {
            this.Dispose(true);
        }


        #endregion

        private SharpMap.Geometries.BoundingBox GetExtent()
        {
            if (_GdalDataset != null)
            {
                double[] geoTrans = new double[6];
                _GdalDataset.GetGeoTransform(geoTrans);
                GeoTransform GT = new GeoTransform(geoTrans);

                return new SharpMap.Geometries.BoundingBox(GT.Left,
                                                            GT.Top + (GT.VerticalPixelResolution * _GdalDataset.RasterYSize),
                                                            GT.Left + (GT.HorizontalPixelResolution * _GdalDataset.RasterXSize),
                                                            GT.Top);
            }

            return null;
        }

        private void GetPreview(OSGeo.GDAL.Dataset dataset, System.Drawing.Size size, Graphics g, SharpMap.Geometries.BoundingBox bbox)
        {
            double[] geoTrans = new double[6];
            dataset.GetGeoTransform(geoTrans);
            GeoTransform GT = new GeoTransform(geoTrans);

            int DsWidth = dataset.RasterXSize;
            int DsHeight = dataset.RasterYSize;

            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
            int iPixelSize = 3; //Format24bppRgb = byte[b,g,r]

            if (dataset != null)
            {
                /*
                if ((float)size.Width / (float)size.Height > (float)DsWidth / (float)DsHeight)
                    size.Width = size.Height * DsWidth / DsHeight;
                else
                    size.Height = size.Width * DsHeight / DsWidth;
                */


                double left = Math.Max(bbox.Left, _Envelope.Left);
                double top = Math.Min(bbox.Top, _Envelope.Top);
                double right = Math.Min(bbox.Right, _Envelope.Right);
                double bottom = Math.Max(bbox.Bottom, _Envelope.Bottom);


                int x1 = (int)GT.PixelX(left);
                int y1 = (int)GT.PixelY(top);
                int x1width = (int)GT.PixelXwidth(right - left);

                int y1height = (int)GT.PixelYwidth(bottom - top);


                bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, size.Width, size.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                try
                {
                    unsafe
                    {
                        for (int i = 1; i <= (dataset.RasterCount > 3 ? 3 : dataset.RasterCount); ++i)
                        {
                            byte[] buffer = new byte[size.Width * size.Height];
                            OSGeo.GDAL.Band band = dataset.GetRasterBand(i);

                            //band.ReadRaster(x1, y1, x1width, y1height, buffer, size.Width, size.Height, (int)GT.HorizontalPixelResolution, (int)GT.VerticalPixelResolution);
                            band.ReadRaster(x1, y1, x1width, y1height, buffer, size.Width, size.Height, 0, 0);

                            int p_indx = 0;
                            int ch = 0;

                            //#warning Check correspondance between enum and integer values
                            if (band.GetRasterColorInterpretation() == OSGeo.GDAL.ColorInterp.GCI_BlueBand) ch = 0;
                            if (band.GetRasterColorInterpretation() == OSGeo.GDAL.ColorInterp.GCI_GreenBand) ch = 1;
                            if (band.GetRasterColorInterpretation() == OSGeo.GDAL.ColorInterp.GCI_RedBand) ch = 2;
                            if (band.GetRasterColorInterpretation() != OSGeo.GDAL.ColorInterp.GCI_PaletteIndex)
                            {
                                for (int y = 0; y < size.Height; y++)
                                {
                                    byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                    for (int x = 0; x < size.Width; x++, p_indx++)
                                    {
                                        row[x * iPixelSize + ch] = buffer[p_indx];
                                    }
                                }
                            }
                            else //8bit Grayscale
                            {
                                for (int y = 0; y < size.Height; y++)
                                {
                                    byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                    for (int x = 0; x < size.Width; x++, p_indx++)
                                    {
                                        row[x * iPixelSize] = buffer[p_indx];
                                        row[x * iPixelSize + 1] = buffer[p_indx];
                                        row[x * iPixelSize + 2] = buffer[p_indx];
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            g.DrawImage(bitmap, new System.Drawing.Point(0, 0));
        }

    }



    /*   
        /// <summary>
        /// Gdal raster image layer
        /// </summary>
        /// <remarks>
        /// Before you can use the Gdal raster layer, you have to install the FwTools
        /// from http://fwtools.maptools.org/ . This package provides all the nessesary
        /// libraries for correct use.
        /// <example>
        /// <code lang="C#">
        /// myMap = new SharpMap.Map(new System.Drawing.Size(500,250);
        /// SharpMap.Layers.GdalRasterLayer layGdal = new SharpMap.Layers.GdalRasterLayer("Blue Marble", @"C:\data\bluemarble.ecw");
        /// myMap.Layers.Add(layGdal);
        /// myMap.ZoomToExtents();
        /// </code>
        /// </example>
        /// </remarks>
        public class GdalRasterLayer : SharpMap.Layers.Layer, IDisposable
        {
            private SharpMap.Geometries.BoundingBox _Envelope;
            private GDAL.Dataset _GdalDataset;
            private System.Drawing.Size imagesize;
            private int BitDepth = 8;
            private bool DisplayIR = false;
            private bool DisplayCIR = false;
            private double redGain = 1;
            private double greenGain = 1;
            private double blueGain = 1;

            private string _Filename;
            /// <summary>
            /// Gets or sets the filename of the raster file
            /// </summary>
            public string Filename
            {
                get { return _Filename; }
                set { _Filename = value; }
            }
            /// <summary>
            /// Gets or sets the bit depth of the raster file
            /// </summary>
            public int intBitDepth
            {
                get { return BitDepth; }
                set { BitDepth = value; }
            }
            /// <summary>
            /// Gets or sets to display IR Band
            /// </summary>
            public bool bDisplayIR
            {
                get { return DisplayIR; }
                set { DisplayIR = value; }
            }
            /// <summary>
            /// Gets or sets to display color InfraRed
            /// </summary>
            public bool bDisplayCIR
            {
                get { return DisplayCIR; }
                set { DisplayCIR = value; }
            }
            /// <summary>
            /// Gets or sets to display red Gain
            /// </summary>
            public double dblRedGain
            {
                get { return redGain; }
                set { redGain = value; }
            }
            /// <summary>
            /// Gets or sets to display green Gain
            /// </summary>
            public double dblGreenGain
            {
                get { return greenGain; }
                set { greenGain = value; }
            }
            /// <summary>
            /// Gets or sets to display blue Gain
            /// </summary>
            public double dblBlueGain
            {
                get { return blueGain; }
                set { blueGain = value; }
            }

            /// <summary>
            /// initialize a Gdal based raster layer
            /// </summary>
            /// <param name="strLayerName">Name of layer</param>
            /// <param name="imageFilename">location of image</param>
            public GdalRasterLayer(string strLayerName, string imageFilename)
            {
                this.LayerName = strLayerName;
                this.Filename = imageFilename;
                disposed = false;

                GDAL.gdal.AllRegister();
                try
                {
                    _GdalDataset = GDAL.gdal.Open(_Filename, 1);
                    imagesize = new Size(_GdalDataset.RasterXSize, _GdalDataset.RasterYSize);
                    _Envelope = this.GetExtent();   
                }
                catch (Exception ex) { 
                    _GdalDataset = null;
                    throw new Exception("Couldn't load dataset. " + ex.Message + ex.InnerException);
                }
 
            }


            #region ILayer Members

            /// <summary>
            /// Renders the layer
            /// </summary>
            /// <param name="g">Graphics object reference</param>
            /// <param name="map">Map which is rendered</param>
            public override void Render(System.Drawing.Graphics g, Map map)
            {
                if (disposed)
                    throw (new ApplicationException("Error: An attempt was made to render a disposed layer"));
            
                //if (this.Envelope.Intersects(map.Envelope))
                //{
                    this.GetPreview(_GdalDataset, map.Size, g, map.Envelope);
                //}
                base.Render(g, map);
            }
       
            /// <summary>
            /// Returns the extent of the layer
            /// </summary>
            /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>

            public override SharpMap.Geometries.BoundingBox Envelope
            {
                get { return _Envelope; }
            }

            #endregion

            #region ICloneable Members

            /// <summary>
            /// Clones the object
            /// </summary>
            /// <returns></returns>
            public override object Clone()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Disposers and finalizers

            private bool disposed = false;

            private void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                        if (_GdalDataset != null)
                        {
                            try {
                            _GdalDataset.Dispose();
                            }
                            finally { _GdalDataset = null; }
                        }
                    disposed = true;
                }
            }
            /// <summary>
            /// Disposes the GdalRasterLayer and release the raster file
            /// </summary>
            public void Dispose ()
            {
                this.Dispose (true);
                GC.SuppressFinalize(this);
            }
            /// <summary>
            /// Finalizer
            /// </summary>
            ~GdalRasterLayer()
            {
                this.Dispose (true);
            }


            #endregion

            private SharpMap.Geometries.BoundingBox GetExtent()
            {
                if(_GdalDataset!=null)
                {
                    double [] geoTrans = new double[6];
                    _GdalDataset.GetGeoTransform(geoTrans);        	
                    GeoTransform GT = new GeoTransform(geoTrans);

                    return new SharpMap.Geometries.BoundingBox( GT.Left,
                                                                GT.Top + (GT.VerticalPixelResolution * _GdalDataset.RasterYSize),
                                                                GT.Left + (GT.HorizontalPixelResolution * _GdalDataset.RasterXSize),
                                                                GT.Top);
                }

                return null;
            }

            private void Get8BitPreview(GDAL.Dataset dataset, System.Drawing.Size size, System.Drawing.Graphics g, SharpMap.Geometries.BoundingBox bbox)
            {
                double [] geoTrans = new double[6];        	
                dataset.GetGeoTransform(geoTrans);
                GeoTransform GT = new GeoTransform(geoTrans);
                int DsWidth = imagesize.Width; // dataset.ImageSize.Width;
                int DsHeight = imagesize.Height; // dataset.ImageSize.Height;
            
                int intImginMapW = 0, intImginMapH = 0, intLocX = 0, intLocY = 0;
            
                Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
            
                int iPixelSize = 3; //Format24bppRgb = byte[b,g,r] 

                if (dataset != null)
                {
                    //check if image is in bounding box
                    if ((bbox.Left > _Envelope.Right) || (bbox.Right < _Envelope.Left)
                        || (bbox.Top < _Envelope.Bottom) || (bbox.Bottom > _Envelope.Top))
                        return;

                    double left = Math.Max(bbox.Left, _Envelope.Left);
                    double top = Math.Min(bbox.Top, _Envelope.Top);
                    double right = Math.Min(bbox.Right, _Envelope.Right);
                    double bottom = Math.Max(bbox.Bottom, _Envelope.Bottom);

                    int x1 = (int)Math.Round(GT.PixelX(left));
                    int y1 = (int)Math.Round(GT.PixelY(top));
                    int imgPixWidth = (int)Math.Round(GT.PixelXwidth(right - left));
                    int imgPixHeight = (int)Math.Round(GT.PixelYwidth(bottom - top));
                
                    //get screen pixels image should fill 
                    double dblBBoxW = bbox.Right - bbox.Left;
                    double dblBBoxtoImgPixX = (double)imgPixWidth / (double)dblBBoxW;
                    intImginMapW = (int)Math.Round(size.Width * dblBBoxtoImgPixX * GT.HorizontalPixelResolution);
                

                    double dblBBoxH = bbox.Top - bbox.Bottom;
                    double dblBBoxtoImgPixY = (double)imgPixHeight / (double)dblBBoxH;
                    intImginMapH = (int)Math.Round(size.Height * dblBBoxtoImgPixY * -GT.VerticalPixelResolution);

                    if((intImginMapH == 0) || (intImginMapW == 0))
                        return;

                    // ratios of bounding box to image ground space
                    double dblBBoxtoImgX = size.Width / dblBBoxW;
                    double dblBBoxtoImgY = size.Height / dblBBoxH;
                
                    // set where to display bitmap in Map
                    if (bbox.Left != left)
                    {
                        if (bbox.Right != right)
                            intLocX = (int)Math.Round((_Envelope.Left - bbox.Left) * dblBBoxtoImgX);
                        else
                            intLocX = size.Width - intImginMapW;
                    }
                    if (bbox.Top != top)
                    {
                        if (bbox.Bottom != bottom)
                            intLocY = (int)Math.Round((bbox.Top - _Envelope.Top) * dblBBoxtoImgY);
                        else
                            intLocY = size.Height - intImginMapH;
                    }

                    bitmap = new Bitmap(intImginMapW, intImginMapH, PixelFormat.Format24bppRgb);
                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, intImginMapW, intImginMapH), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                    try
                    {
                        unsafe
                        {
                            if ((DisplayIR) && (dataset.RasterCount == 4))
                            {
                                byte[] buffer = new byte[intImginMapW * intImginMapH];
                                GDAL.Band band = dataset.GetRasterBand(4);

                                band.ReadRaster(x1, y1, imgPixWidth, imgPixHeight, buffer, size.Width, size.Height, (int)GT.HorizontalPixelResolution, (int)GT.VerticalPixelResolution);
                            
                                int p_indx = 0;
                                for (int y = 0; y < intImginMapH; y++)
                                {
                                    byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                    for (int x = 0; x < intImginMapW; x++, p_indx++)
                                    {
                                        row[x * iPixelSize] = buffer[p_indx];
                                        row[x * iPixelSize + 1] = buffer[p_indx];
                                        row[x * iPixelSize + 2] = buffer[p_indx];
                                    }
                                }
                            }
                            else if ((DisplayCIR) && (dataset.RasterCount == 4))
                            {
                                for (int i = 1; i <= 4; ++i)
                                {
                                    if (i == 3) continue;

                                    byte[] buffer = new byte[intImginMapW * intImginMapH];
                                    GDAL.Band band = dataset.GetRasterBand(i);

                                    //band.RasterIO(RWFlag.Read, x1, y1, imgPixWidth, imgPixHeight, buffer, intImginMapW, intImginMapH);
                                    band.ReadRaster(x1, y1, imgPixWidth, imgPixHeight, buffer, size.Width, size.Height, (int)GT.HorizontalPixelResolution, (int)GT.VerticalPixelResolution);                                

                                    int p_indx = 0;
                                    int ch = 0;
                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.BlueBand)) ch = 2;
                                    else if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.GreenBand)) ch = 0;
                                    else if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.RedBand)) ch = 1;
                                    else ch = 2;
                                    for (int y = 0; y < intImginMapH; y++)
                                    {
                                        byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                        for (int x = 0; x < intImginMapW; x++, p_indx++)
                                        {
                                            row[x * iPixelSize + ch] = (byte)(buffer[p_indx]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i <= (dataset.RasterCount > 3 ? 3 : dataset.RasterCount); ++i)
                                {
                                    byte[] buffer = new byte[intImginMapW * intImginMapH];
                                    GDAL.Band band = dataset.GetRasterBand(i);

                                    //band.RasterIO(RWFlag.Read, x1, y1, imgPixWidth, imgPixHeight, buffer, intImginMapW, intImginMapH);
                                    band.ReadRaster(x1, y1, imgPixWidth, imgPixHeight, buffer, size.Width, size.Height, (int)GT.HorizontalPixelResolution, (int)GT.VerticalPixelResolution);

                                    int p_indx = 0;
                                    int ch = 0;

                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.BlueBand)) ch = 0;
                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.GreenBand)) ch = 1;
                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.RedBand)) ch = 2;
                                    if ((!int.Equals(band.GetRasterColorInterpretation(), ColorInterp.PaletteIndex)) && (!int.Equals(band.GetRasterColorInterpretation(), ColorInterp.GrayIndex)))
                                    {
                                        for (int y = 0; y < intImginMapH; y++)
                                        {
                                            byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                            for (int x = 0; x < intImginMapW; x++, p_indx++)
                                            {
                                                if(ch == 2)
                                                    row[x * iPixelSize + ch] = (byte)((buffer[p_indx]) * redGain);
                                                else if( ch == 1)
                                                    row[x * iPixelSize + ch] = (byte)((buffer[p_indx]) * greenGain);
                                                else if (ch == 0)
                                                    row[x * iPixelSize + ch] = (byte)((buffer[p_indx]) * blueGain);
                                            }
                                        }
                                    }

                                    else //8bit Grayscale
                                    {
                                        for (int y = 0; y < intImginMapH; y++)
                                        {
                                            byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                            for (int x = 0; x < intImginMapW; x++, p_indx++)
                                            {
                                                row[x * iPixelSize] = buffer[p_indx];
                                                row[x * iPixelSize + 1] = buffer[p_indx];
                                                row[x * iPixelSize + 2] = buffer[p_indx];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                }
                g.DrawImage(bitmap, new System.Drawing.Point(intLocX, intLocY));
            }

            private void GetPreview(GDAL.Dataset dataset, System.Drawing.Size size, System.Drawing.Graphics g, SharpMap.Geometries.BoundingBox bbox)
            {
                double [] geoTrans = new double[6];        	
                dataset.GetGeoTransform(geoTrans);
                GeoTransform GT = new GeoTransform(geoTrans);
            
                int DsWidth = imagesize.Width; //dataset.ImageSize.Width;
                int DsHeight = imagesize.Height; //dataset.ImageSize.Height;
                int intImginMapW = 0, intImginMapH = 0, intLocX = 0, intLocY = 0, intVal = 0;

                if (intBitDepth == 8)
                {
                    Get8BitPreview(dataset, size, g, bbox);
                    return;
                }


                Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

                int iPixelSize = 3; //Format24bppRgb = byte[b,g,r] 

                if (dataset != null)
                {
                    //check if image is in bounding box
                    if ((bbox.Left > _Envelope.Right) || (bbox.Right < _Envelope.Left)
                        || (bbox.Top < _Envelope.Bottom) || (bbox.Bottom > _Envelope.Top))
                        return;

                    double left = Math.Max(bbox.Left, _Envelope.Left);
                    double top = Math.Min(bbox.Top, _Envelope.Top);
                    double right = Math.Min(bbox.Right, _Envelope.Right);
                    double bottom = Math.Max(bbox.Bottom, _Envelope.Bottom);

                    int x1 = (int)Math.Round(GT.PixelX(left));
                    int y1 = (int)Math.Round(GT.PixelY(top));
                    int imgPixWidth = (int)Math.Round(GT.PixelXwidth(right - left));
                    int imgPixHeight = (int)Math.Round(GT.PixelYwidth(bottom - top));

                    //get screen pixels image should fill 
                    double dblBBoxW = bbox.Right - bbox.Left;
                    double dblBBoxtoImgPixX = (double)imgPixWidth / (double)dblBBoxW;
                    intImginMapW = (int)Math.Round(size.Width * dblBBoxtoImgPixX * GT.HorizontalPixelResolution);

                    double dblBBoxH = bbox.Top - bbox.Bottom;
                    double dblBBoxtoImgPixY = (double)imgPixHeight / (double)dblBBoxH;
                    intImginMapH = (int)Math.Round(size.Height * dblBBoxtoImgPixY * -GT.VerticalPixelResolution);

                    // ratios of bounding box to image ground space
                    double dblBBoxtoImgX = size.Width / dblBBoxW;
                    double dblBBoxtoImgY = size.Height / dblBBoxH;

                    // set where to display bitmap in Map
                    if (bbox.Left != left)
                    {
                        if (bbox.Right != right)
                            intLocX = (int)Math.Round((_Envelope.Left - bbox.Left) * dblBBoxtoImgX);
                        else
                            intLocX = size.Width - intImginMapW;
                    }
                    if (bbox.Top != top)
                    {
                        if (bbox.Bottom != bottom)
                            intLocY = (int)Math.Round((bbox.Top - _Envelope.Top) * dblBBoxtoImgY);
                        else
                            intLocY = size.Height - intImginMapH;
                    }

                    bitmap = new Bitmap(intImginMapW, intImginMapH, PixelFormat.Format24bppRgb);
                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, intImginMapW, intImginMapH), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                    try
                    {
                        unsafe
                        {
                            if ((DisplayIR)&&(dataset.RasterCount == 4))
                            {
                                //UInt16[] buffer = new UInt16[intImginMapW * intImginMapH];
                                Int16[] buffer = new Int16[intImginMapW * intImginMapH];
                                GDAL.Band band = dataset.GetRasterBand(4);

                                //band.RasterIO(RWFlag.Read, x1, y1, imgPixWidth, imgPixHeight, buffer, intImginMapW, intImginMapH);
                                band.ReadRaster(x1, y1, imgPixWidth, imgPixHeight, (short[])buffer, size.Width, size.Height, (int)GT.HorizontalPixelResolution, (int)GT.VerticalPixelResolution);
                            
                                int p_indx = 0;

                                for (int y = 0; y < intImginMapH; y++)
                                {
                                    byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                    for (int x = 0; x < intImginMapW; x++, p_indx++)
                                    {
                                        if (intBitDepth == 16)
                                        {
                                            intVal = (buffer[p_indx] / 256);
                                            if(intVal > 255)intVal = 255;
                                        }
                                        else if (intBitDepth == 12)
                                        {
                                            intVal = (buffer[p_indx] / 16);
                                            if (intVal > 255) intVal = 255;
                                        }
                                        row[x * iPixelSize] = (byte)intVal;
                                        row[x * iPixelSize + 1] = (byte)intVal;
                                        row[x * iPixelSize + 2] = (byte)intVal;
                                    }
                                }
                            }
                            else if ((DisplayCIR) && (dataset.RasterCount == 4))
                            {
                                for (int i = 1; i <= 4; ++i)
                                {
                                    if (i == 3) continue;

                                    //UInt16[] buffer = new UInt16[intImginMapW * intImginMapH];
                                    Int16[] buffer = new Int16[intImginMapW * intImginMapH];
                                    GDAL.Band band = dataset.GetRasterBand(i);

                                    //band.RasterIO(RWFlag.Read, x1, y1, imgPixWidth, imgPixHeight, buffer, intImginMapW, intImginMapH);
                                    band.ReadRaster(x1, y1, imgPixWidth, imgPixHeight, (short[])buffer, size.Width, size.Height, (int)GT.HorizontalPixelResolution, (int)GT.VerticalPixelResolution);

                                    int p_indx = 0;
                                    int ch = 0;
                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.BlueBand)) ch = 2;
                                    else if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.GreenBand)) ch = 0;
                                    else if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.RedBand)) ch = 1;
                                    else ch = 2;
                                    for (int y = 0; y < intImginMapH; y++)
                                    {
                                        byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                        for (int x = 0; x < intImginMapW; x++, p_indx++)
                                        {
                                            if (intBitDepth == 16)
                                            {
                                                intVal = (buffer[p_indx] / 256);
                                                if (intVal > 255)intVal = 255;
                                            }

                                            else if (intBitDepth == 12)
                                            {
                                                intVal = (buffer[p_indx] / 16);
                                                if (intVal > 255)intVal = 255;
                                            }
                                            row[x * iPixelSize + ch] = (byte)intVal;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i <= (dataset.RasterCount > 3 ? 3 : dataset.RasterCount); ++i)
                                {
                                    //UInt16[] buffer = new UInt16[intImginMapW * intImginMapH];
                                    Int16[] buffer = new Int16[intImginMapW * intImginMapH];                                
                                    GDAL.Band band = dataset.GetRasterBand(i);

                                    //band.RasterIO(RWFlag.Read, x1, y1, imgPixWidth, imgPixHeight, buffer, intImginMapW, intImginMapH);
                                    band.ReadRaster(x1, y1, imgPixWidth, imgPixHeight, (short[])buffer, size.Width, size.Height, (int)GT.HorizontalPixelResolution, (int)GT.VerticalPixelResolution);
                            	

                                    int p_indx = 0;
                                    int ch = 0;

                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.BlueBand)) ch = 0;
                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.GreenBand)) ch = 1;
                                    if (int.Equals(band.GetRasterColorInterpretation(), ColorInterp.RedBand)) ch = 2;
                                    if ((!int.Equals(band.GetRasterColorInterpretation(), ColorInterp.PaletteIndex))&& (!int.Equals(band.GetRasterColorInterpretation(), ColorInterp.GrayIndex)))
                                    {
                                        for (int y = 0; y < intImginMapH; y++)
                                        {
                                            byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                            for (int x = 0; x < intImginMapW; x++, p_indx++)
                                            {
                                                if (intBitDepth == 16)
                                                {
                                                    if (ch == 2)
                                                    {
                                                        intVal = (int)((buffer[p_indx] / 256) * redGain);
                                                        if (intVal > 255) intVal = 255;
                                                    }
                                                    else if (ch == 1)
                                                    {
                                                        intVal = (int)((buffer[p_indx] / 256) * greenGain);
                                                        if (intVal > 255) intVal = 255;
                                                    }
                                                    else if (ch == 0)
                                                    {
                                                        intVal = (int)((buffer[p_indx] / 256) * blueGain);
                                                        if (intVal > 255) intVal = 255;
                                                    }
                                                    row[x * iPixelSize + ch] = (byte)intVal;
                                                }
                                                else if (intBitDepth == 12)
                                                {
                                                    if (ch == 2)
                                                    {
                                                        intVal = (int)((buffer[p_indx] / 16) * redGain);
                                                        if (intVal > 255) intVal = 255;
                                                    }
                                                    else if (ch == 1)
                                                    {
                                                        intVal = (int)((buffer[p_indx] / 16) * greenGain);
                                                        if (intVal > 255) intVal = 255;
                                                    }
                                                    else if (ch == 0)
                                                    {
                                                        intVal = (int)((buffer[p_indx] / 16) * blueGain);
                                                        if (intVal > 255) intVal = 255;
                                                    }
                                                    row[x * iPixelSize + ch] = (byte)intVal;
                                                }
                                            }
                                        }
                                    }

                                    else //Grayscale
                                    {
                                        for (int y = 0; y < intImginMapH; y++)
                                        {
                                            byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                                            for (int x = 0; x < intImginMapW; x++, p_indx++)
                                            {
                                                if (intBitDepth == 16)
                                                {
                                                    intVal = (buffer[p_indx] / 256);
                                                    if (intVal > 255) intVal = 255;
                                                }
                                                else if (intBitDepth == 12)
                                                {
                                                    intVal = (buffer[p_indx] / 16);
                                                    if (intVal > 255) intVal = 255;
                                                }
                                                row[x * iPixelSize] = (byte)intVal;
                                                row[x * iPixelSize + 1] = (byte)intVal;
                                                row[x * iPixelSize + 2] = (byte)intVal;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                }
                g.DrawImage(bitmap, new System.Drawing.Point(intLocX, intLocY));
            }
        }   
   
    */





    /// <summary>
    /// Types of color interpretation for raster bands.
    /// </summary>
    public enum ColorInterp
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Greyscale
        /// </summary>
        GrayIndex = 1,
        /// <summary>
        /// Paletted (see associated color table)
        /// </summary>
        PaletteIndex = 2,
        /// <summary>
        /// Red band of RGBA image
        /// </summary>               
        RedBand = 3,
        /// <summary>
        /// Green band of RGBA image
        /// </summary>
        GreenBand = 4,
        /// <summary>
        /// Blue band of RGBA image
        /// </summary>                       
        BlueBand = 5,
        /// <summary>
        /// Alpha (0=transparent, 255=opaque)
        /// </summary>   
        AlphaBand = 6,
        /// <summary>
        /// Hue band of HLS image 
        /// </summary>                 
        HueBand = 7,
        /// <summary>
        /// Saturation band of HLS image 
        /// </summary>       
        SaturationBand = 8,
        /// <summary>
        /// Lightness band of HLS image
        /// </summary>            
        LightnessBand = 9,
        /// <summary>
        /// Cyan band of CMYK image
        /// </summary>                
        CyanBand = 10,
        /// <summary>
        /// Magenta band of CMYK image
        /// </summary>             
        MagentaBand = 11,
        /// <summary>
        /// Yellow band of CMYK image
        /// </summary>             
        YellowBand = 12,
        /// <summary>
        /// Black band of CMYK image
        /// </summary>                      
        BlackBand = 13,
        /// <summary>
        /// Y Luminance
        /// </summary>                             
        YCbCr_YBand = 14,
        /// <summary>
        /// Cb Chroma
        /// </summary>                              
        YCbCr_CbBand = 15,
        /// <summary>
        /// Cr Chroma
        /// </summary>                                          
        YCbCr_CrBand = 16,
        /// <summary>
        /// Max current value
        /// </summary>
        Max = 16
    };

    /// <summary>
    /// Type for Read Write state
    /// </summary>
    public enum RWFlag
    {
        /// <summary>
        /// Read dataset
        /// </summary>
        Read = 0,
        /// <summary>
        /// Write dataset
        /// </summary>
        Write = 1
    };

    /// <summary>
    /// Field data type
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// unknown data type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// byte
        /// </summary>
        Byte = 1,
        /// <summary>
        /// unsigned int 16 bit
        /// </summary>
        UInt16 = 2,
        /// <summary>
        /// signed int 16 bit
        /// </summary>
        Int16 = 3,
        /// <summary>
        /// unsigned int 32 bit
        /// </summary>
        UInt32 = 4,
        /// <summary>
        /// signed int 32 bit
        /// </summary>
        Int32 = 5,
        /// <summary>
        /// float 32 bit
        /// </summary>
        Float32 = 6,
        /// <summary>
        /// float 64 bit
        /// </summary>
        Float64 = 7,
        /// <summary>
        /// c int 16 bit
        /// </summary>
        CInt16 = 8,
        /// <summary>
        /// c int 32 bit
        /// </summary>
        CInt32 = 9,
        /// <summary>
        /// c float 16 bit
        /// </summary>
        CFloat32 = 10,
        /// <summary>
        /// c float 64 bit
        /// </summary>
        CFloat64 = 11,
        /// <summary>
        /// type count
        /// </summary>
        TypeCount = 12
    };
}
