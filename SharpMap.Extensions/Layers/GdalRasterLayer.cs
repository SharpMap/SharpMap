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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OSGeo.GDAL;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Geometries;
using Point=System.Drawing.Point;

namespace SharpMap.Layers
{
    /// <summary>
    /// Gdal raster image layer
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code lang="C#">
    /// myMap = new SharpMap.Map(new System.Drawing.Size(500,250);
    /// SharpMap.Layers.GdalRasterLayer layGdal = new SharpMap.Layers.GdalRasterLayer("Blue Marble", @"C:\data\bluemarble.ecw");
    /// myMap.Layers.Add(layGdal);
    /// myMap.ZoomToExtents();
    /// </code>
    /// </example>
    /// </remarks>
    public class GdalRasterLayer : Layer, IDisposable
    {
        protected BoundingBox _Envelope;
        protected Dataset _GdalDataset;
        private bool bColorCorrect = true; // apply color correction values
        private bool bHaveSpot = false; // spot correction
        private bool bShowClip = false;
        protected bool bUseRotation = true; // use geographic information
        private double[] dblGain = {1, 1, 1, 1};
        private double[] dblNonSpotGain = {1, 1, 1, 1};
        private double[] dblSpotGain = {1, 1, 1, 1};
        private bool DisplayCIR = false;
        private bool DisplayIR = false;
        private double gamma = 1;
        internal GeoTransform GT;
        private double histoBrightness, histoContrast;
        private List<int[]> histogram; // histogram of image
        private double[] histoMean;
        protected Size imagesize;

        private double innerSpotRadius = 0;
                       // outer radius is feather between inner radius and rest of image

        private int intBitDepth = 8;
        internal int lbands;

        private List<int[]> lstCurveLut;
        private List<int[]> lstNonSpotCurveLut;
        private List<int[]> lstSpotCurveLut;
        private double NonSpotGamma = 1;

        private double outerSpotRadius = 0;
                       // outer radius is feather between inner radius and rest of image

        private PointF pntSpot = new PointF(0, 0);

        private Rectangle rectHistoBounds;
        private double SpotGamma = 1;
        private Point stretchPoint;
        private string strProjection = "";
        protected ICoordinateTransformation transform = null;
        private Color transparentColor = Color.Empty; // color in image to make transparent (i.e. for black fill)

        #region accessors

        private string _Filename;

        /// <summary>
        ///  Gets the version of fwTools that was used to compile and test this GdalRasterLayer
        /// </summary>
        public static string FWToolsVersion
        {
            get { return "2.2.0"; }
        }

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
        public int BitDepth
        {
            get { return intBitDepth; }
            set { intBitDepth = value; }
        }

        /// <summary>
        /// Gets or set the projection of the raster file
        /// </summary>
        public string Projection
        {
            get { return strProjection; }
            set { strProjection = value; }
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
        /// Gets or sets to display clip
        /// </summary>
        public bool ShowClip
        {
            get { return bShowClip; }
            set { bShowClip = value; }
        }

        /// <summary>
        /// Gets or sets to display gamma
        /// </summary>
        public double dblGamma
        {
            get { return gamma; }
            set { gamma = value; }
        }

        /// <summary>
        /// Gets or sets to display gamma for Spot spot
        /// </summary>
        public double dblSpotGamma
        {
            get { return SpotGamma; }
            set { SpotGamma = value; }
        }

        /// <summary>
        /// Gets or sets to display gamma for NonSpot
        /// </summary>
        public double dblNonSpotGamma
        {
            get { return NonSpotGamma; }
            set { NonSpotGamma = value; }
        }

        /// <summary>
        /// Gets or sets to display red Gain
        /// </summary>
        public double[] Gain
        {
            get { return dblGain; }
            set { dblGain = value; }
        }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] SpotGain
        {
            get { return dblSpotGain; }
            set { dblSpotGain = value; }
        }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] NonSpotGain
        {
            get { return dblNonSpotGain; }
            set { dblNonSpotGain = value; }
        }

        /// <summary>
        /// Gets or sets to display curve lut
        /// </summary>
        public List<int[]> LstCurveLut
        {
            get { return lstCurveLut; }
            set { lstCurveLut = value; }
        }

        /// <summary>
        /// Correct Spot spot
        /// </summary>
        public bool HaveSpot
        {
            get { return bHaveSpot; }
            set { bHaveSpot = value; }
        }

        /// <summary>
        /// Gets or sets to display curve lut for Spot spot
        /// </summary>
        public List<int[]> LstSpotCurveLut
        {
            get { return lstSpotCurveLut; }
            set { lstSpotCurveLut = value; }
        }

        /// <summary>
        /// Gets or sets to display curve lut for NonSpot
        /// </summary>
        public List<int[]> LstNonSpotCurveLut
        {
            get { return lstNonSpotCurveLut; }
            set { lstNonSpotCurveLut = value; }
        }

        /// <summary>
        /// Gets or sets the center point of the Spot spot
        /// </summary>
        public PointF SpotPoint
        {
            get { return pntSpot; }
            set { pntSpot = value; }
        }

        /// <summary>
        /// Gets or sets the inner radius for the spot
        /// </summary>
        public double InnerSpotRadius
        {
            get { return innerSpotRadius; }
            set { innerSpotRadius = value; }
        }

        /// <summary>
        /// Gets or sets the outer radius for the spot (feather zone)
        /// </summary>
        public double OuterSpotRadius
        {
            get { return outerSpotRadius; }
            set { outerSpotRadius = value; }
        }

        /// <summary>
        /// Gets the true histogram
        /// </summary>
        public List<int[]> Histogram
        {
            get { return histogram; }
        }

        /// <summary>
        /// Gets the quick histogram mean
        /// </summary>
        public double[] HistoMean
        {
            get { return histoMean; }
        }

        /// <summary>
        /// Gets the quick histogram brightness
        /// </summary>
        public double HistoBrightness
        {
            get { return histoBrightness; }
        }

        /// <summary>
        /// Gets the quick histogram contrast
        /// </summary>
        public double HistoContrast
        {
            get { return histoContrast; }
        }

        /// <summary>
        /// Gets the number of bands
        /// </summary>
        public int Bands
        {
            get { return lbands; }
        }

        /// <summary>
        /// Gets the GSD (Horizontal)
        /// </summary>
        public double dblGSD
        {
            get { return GT.HorizontalPixelResolution; }
        }

        ///<summary>
        /// Use rotation information
        /// </summary>
        public bool UseRotation
        {
            get { return bUseRotation; }
            set
            {
                bUseRotation = value;
                _Envelope = GetExtent();
            }
        }

        public Size Size
        {
            get { return imagesize; }
        }

        public bool ColorCorrect
        {
            get { return bColorCorrect; }
            set { bColorCorrect = value; }
        }

        public Rectangle HistoBounds
        {
            get { return rectHistoBounds; }
            set { rectHistoBounds = value; }
        }

        public ICoordinateTransformation Transform
        {
            get { return transform; }
        }

        public Color TransparentColor
        {
            get { return transparentColor; }
            set { transparentColor = value; }
        }

        public Point StretchPoint
        {
            get
            {
                if (stretchPoint.Y == 0)
                    ComputeStretch();

                return stretchPoint;
            }
            set { stretchPoint = value; }
        }

        #endregion

        /// <summary>
        /// initialize a Gdal based raster layer
        /// </summary>
        /// <param name="strLayerName">Name of layer</param>
        /// <param name="imageFilename">location of image</param>
        public GdalRasterLayer(string strLayerName, string imageFilename)
        {
            LayerName = strLayerName;
            Filename = imageFilename;
            disposed = false;

            Gdal.AllRegister();

            try
            {
                _GdalDataset = Gdal.OpenShared(_Filename, Access.GA_ReadOnly);

                // have gdal read the projection
                strProjection = _GdalDataset.GetProjectionRef();

                // no projection info found in the image...check for a prj
                if (strProjection == "" &&
                    File.Exists(imageFilename.Substring(0, imageFilename.LastIndexOf(".")) + ".prj"))
                {
                    strProjection = File.ReadAllText(imageFilename.Substring(0, imageFilename.LastIndexOf(".")) + ".prj");
                }

                imagesize = new Size(_GdalDataset.RasterXSize, _GdalDataset.RasterYSize);
                _Envelope = GetExtent();
                rectHistoBounds = new Rectangle((int) _Envelope.Left, (int) _Envelope.Bottom, (int) _Envelope.Width,
                                                (int) _Envelope.Height);
                lbands = _GdalDataset.RasterCount;
            }
            catch (Exception ex)
            {
                _GdalDataset = null;
                throw new Exception("Couldn't load " + imageFilename + "\n\n" + ex.Message + ex.InnerException);
            }
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override BoundingBox Envelope
        {
            get { return _Envelope; }
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            if (disposed)
                throw (new ApplicationException("Error: An attempt was made to render a disposed layer"));

            GetPreview(_GdalDataset, map.Size, g, map.Envelope, null, map);
            base.Render(g, map);
        }

        // get raster projection
        public ICoordinateSystem GetProjection()
        {
            CoordinateSystemFactory cFac = new CoordinateSystemFactory();

            try
            {
                if (strProjection != "")
                    return cFac.CreateFromWkt(strProjection);
            }
            catch
            {
            }

            return null;
        }

        // zoom to native resolution
        public double GetOneToOne(Map map)
        {
            double DsWidth = imagesize.Width;
            double DsHeight = imagesize.Height;
            double left, top, right, bottom;
            double dblImgEnvW, dblImgEnvH, dblWindowGndW, dblWindowGndH, dblImginMapW, dblImginMapH;

            BoundingBox bbox = map.Envelope;
            Size size = map.Size;

            // bounds of section of image to be displayed
            left = Math.Max(bbox.Left, _Envelope.Left);
            top = Math.Min(bbox.Top, _Envelope.Top);
            right = Math.Min(bbox.Right, _Envelope.Right);
            bottom = Math.Max(bbox.Bottom, _Envelope.Bottom);

            // height and width of envelope of transformed image in ground space
            dblImgEnvW = _Envelope.Right - _Envelope.Left;
            dblImgEnvH = _Envelope.Top - _Envelope.Bottom;

            // height and width of display window in ground space
            dblWindowGndW = bbox.Right - bbox.Left;
            dblWindowGndH = bbox.Top - bbox.Bottom;

            // height and width of transformed image in pixel space
            dblImginMapW = size.Width*(dblImgEnvW/dblWindowGndW);
            dblImginMapH = size.Height*(dblImgEnvH/dblWindowGndH);

            // image was not turned on its side
            if ((dblImginMapW > dblImginMapH && DsWidth > DsHeight) ||
                (dblImginMapW < dblImginMapH && DsWidth < DsHeight))
                return map.Zoom*(dblImginMapW/DsWidth);
                // image was turned on its side
            else
                return map.Zoom*(dblImginMapH/DsWidth);
        }

        // zooms to nearest tiff internal resolution set
        public double GetZoomNearestRSet(Map map, bool bZoomIn)
        {
            double DsWidth = imagesize.Width;
            double DsHeight = imagesize.Height;
            double left, top, right, bottom;
            double dblImgEnvW, dblImgEnvH, dblWindowGndW, dblWindowGndH, dblImginMapW, dblImginMapH;
            double dblTempWidth = 0;

            BoundingBox bbox = map.Envelope;
            Size size = map.Size;

            // bounds of section of image to be displayed
            left = Math.Max(bbox.Left, _Envelope.Left);
            top = Math.Min(bbox.Top, _Envelope.Top);
            right = Math.Min(bbox.Right, _Envelope.Right);
            bottom = Math.Max(bbox.Bottom, _Envelope.Bottom);

            // height and width of envelope of transformed image in ground space
            dblImgEnvW = _Envelope.Right - _Envelope.Left;
            dblImgEnvH = _Envelope.Top - _Envelope.Bottom;

            // height and width of display window in ground space
            dblWindowGndW = bbox.Right - bbox.Left;
            dblWindowGndH = bbox.Top - bbox.Bottom;

            // height and width of transformed image in pixel space
            dblImginMapW = size.Width*(dblImgEnvW/dblWindowGndW);
            dblImginMapH = size.Height*(dblImgEnvH/dblWindowGndH);

            // image was not turned on its side
            if ((dblImginMapW > dblImginMapH && DsWidth > DsHeight) ||
                (dblImginMapW < dblImginMapH && DsWidth < DsHeight))
                dblTempWidth = dblImginMapW;
            else
                dblTempWidth = dblImginMapH;

            // zoom level is within the r sets
            if (DsWidth > dblTempWidth && (DsWidth/Math.Pow(2, 8)) < dblTempWidth)
            {
                if (bZoomIn)
                {
                    for (int i = 0; i <= 8; i++)
                    {
                        if (DsWidth/Math.Pow(2, i) > dblTempWidth)
                        {
                            if (DsWidth/Math.Pow(2, i + 1) < dblTempWidth)
                                return map.Zoom*(dblTempWidth/(DsWidth/Math.Pow(2, i)));
                        }
                    }
                }
                else
                {
                    for (int i = 8; i >= 0; i--)
                    {
                        if (DsWidth/Math.Pow(2, i) < dblTempWidth)
                        {
                            if (DsWidth/Math.Pow(2, i - 1) > dblTempWidth)
                                return map.Zoom*(dblTempWidth/(DsWidth/Math.Pow(2, i)));
                        }
                    }
                }
            }


            return map.Zoom;
        }

        /// <summary>
        /// Clones the object
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public void ResetHistoRectangle()
        {
            rectHistoBounds = new Rectangle((int) _Envelope.Left, (int) _Envelope.Bottom, (int) _Envelope.Width,
                                            (int) _Envelope.Height);
        }

        // gets transform between raster's native projection and the map projection
        private void GetTransform(ICoordinateSystem mapProjection)
        {
            if (mapProjection == null || strProjection == "")
            {
                transform = null;
                return;
            }

            CoordinateSystemFactory cFac = new CoordinateSystemFactory();

            // get our two projections
            ICoordinateSystem srcCoord = cFac.CreateFromWkt(strProjection);
            ICoordinateSystem tgtCoord = mapProjection;

            // raster and map are in same projection, no need to transform
            if (srcCoord.WKT == tgtCoord.WKT)
            {
                transform = null;
                return;
            }

            // create transform
            transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(srcCoord, tgtCoord);
        }

        // get boundary of raster
        private BoundingBox GetExtent()
        {
            if (_GdalDataset != null)
            {
                double right = 0, left = 0, top = 0, bottom = 0;
                double dblW, dblH;

                double[] geoTrans = new double[6];


                _GdalDataset.GetGeoTransform(geoTrans);

                // no rotation...use default transform
                if (!bUseRotation && !bHaveSpot || (geoTrans[0] == 0 && geoTrans[3] == 0))
                    geoTrans = new double[] {999.5, 1, 0, 1000.5, 0, -1};

                GT = new GeoTransform(geoTrans);

                // image pixels
                dblW = imagesize.Width;
                dblH = imagesize.Height;

                left = GT.EnvelopeLeft(dblW, dblH);
                right = GT.EnvelopeRight(dblW, dblH);
                top = GT.EnvelopeTop(dblW, dblH);
                bottom = GT.EnvelopeBottom(dblW, dblH);

                return new BoundingBox(left, bottom, right, top);
            }

            return null;
        }

        // get 4 corners of image
        public Collection<Geometries.Point> GetFourCorners()
        {
            Collection<Geometries.Point> points = new Collection<Geometries.Point>();
            double[] dblPoint;

            if (_GdalDataset != null)
            {
                double[] geoTrans = new double[6];
                _GdalDataset.GetGeoTransform(geoTrans);

                // no rotation...use default transform
                if (!bUseRotation && !bHaveSpot || (geoTrans[0] == 0 && geoTrans[3] == 0))
                    geoTrans = new double[] {999.5, 1, 0, 1000.5, 0, -1};

                points.Add(new Geometries.Point(geoTrans[0], geoTrans[3]));
                points.Add(new Geometries.Point(geoTrans[0] + (geoTrans[1]*imagesize.Width),
                                                geoTrans[3] + (geoTrans[4]*imagesize.Width)));
                points.Add(
                    new Geometries.Point(geoTrans[0] + (geoTrans[1]*imagesize.Width) + (geoTrans[2]*imagesize.Height),
                                         geoTrans[3] + (geoTrans[4]*imagesize.Width) + (geoTrans[5]*imagesize.Height)));
                points.Add(new Geometries.Point(geoTrans[0] + (geoTrans[2]*imagesize.Height),
                                                geoTrans[3] + (geoTrans[5]*imagesize.Height)));

                // transform to map's projection
                if (transform != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        dblPoint = transform.MathTransform.Transform(new double[] {points[i].X, points[i].Y});
                        points[i] = new Geometries.Point(dblPoint[0], dblPoint[1]);
                    }
                }
            }

            return points;
        }

        public Polygon GetFootprint()
        {
            LinearRing myRing = new LinearRing(GetFourCorners());
            return new Polygon(myRing);
        }

        // applies map projection transfrom to get reprojected envelope
        private void ApplyTransformToEnvelope()
        {
            double[] leftBottom, leftTop, rightTop, rightBottom;
            double left, right, bottom, top;

            _Envelope = GetExtent();

            if (transform == null)
                return;

            // set envelope
            _Envelope = GeometryTransform.TransformBox(_Envelope, transform.MathTransform);

            // do same to histo rectangle
            leftBottom = new double[] {rectHistoBounds.Left, rectHistoBounds.Bottom};
            leftTop = new double[] {rectHistoBounds.Left, rectHistoBounds.Top};
            rightBottom = new double[] {rectHistoBounds.Right, rectHistoBounds.Bottom};
            rightTop = new double[] {rectHistoBounds.Right, rectHistoBounds.Top};

            // transform corners into new projection
            leftBottom = transform.MathTransform.Transform(leftBottom);
            leftTop = transform.MathTransform.Transform(leftTop);
            rightBottom = transform.MathTransform.Transform(rightBottom);
            rightTop = transform.MathTransform.Transform(rightTop);

            // find extents
            left = Math.Min(leftBottom[0], Math.Min(leftTop[0], Math.Min(rightBottom[0], rightTop[0])));
            right = Math.Max(leftBottom[0], Math.Max(leftTop[0], Math.Max(rightBottom[0], rightTop[0])));
            bottom = Math.Min(leftBottom[1], Math.Min(leftTop[1], Math.Min(rightBottom[1], rightTop[1])));
            top = Math.Max(leftBottom[1], Math.Max(leftTop[1], Math.Max(rightBottom[1], rightTop[1])));

            // set histo rectangle
            rectHistoBounds = new Rectangle((int) left, (int) bottom, (int) right, (int) top);
        }

        // public method to set envelope and transform to new projection
        public void ReprojectToMap(Map map)
        {
            GetTransform(null);
            ApplyTransformToEnvelope();
        }

        // add image pixels to the map
        protected virtual void GetPreview(Dataset dataset, Size size, Graphics g,
                                          BoundingBox displayBbox, ICoordinateSystem mapProjection, Map map)
        {
            double[] geoTrans = new double[6];
            _GdalDataset.GetGeoTransform(geoTrans);

            // not rotated, use faster display method
            if ((!bUseRotation ||
                 (geoTrans[1] == 1 && geoTrans[2] == 0 && geoTrans[4] == 0 && Math.Abs(geoTrans[5]) == 1))
                && !bHaveSpot && transform == null)
            {
                GetNonRotatedPreview(dataset, size, g, displayBbox, mapProjection);
                return;
            }
                // not rotated, but has spot...need default rotation
            else if ((geoTrans[0] == 0 && geoTrans[3] == 0) && bHaveSpot)
                geoTrans = new double[] {999.5, 1, 0, 1000.5, 0, -1};

            GT = new GeoTransform(geoTrans);
            double DsWidth = imagesize.Width;
            double DsHeight = imagesize.Height;
            double left, top, right, bottom;
            double GndX = 0, GndY = 0, ImgX = 0, ImgY = 0, PixX, PixY;
            double[] intVal = new double[Bands];
            double imageVal = 0, SpotVal = 0;
            double bitScalar = 1.0;
            Bitmap bitmap = null;
            Point bitmapTL = new Point(), bitmapBR = new Point();
            Geometries.Point imageTL = new Geometries.Point(), imageBR = new Geometries.Point();
            BoundingBox shownImageBbox, trueImageBbox;
            int bitmapLength, bitmapHeight;
            int displayImageLength, displayImageHeight;

            int iPixelSize = 3; //Format24bppRgb = byte[b,g,r] 

            if (dataset != null)
            {
                //check if image is in bounding box
                if ((displayBbox.Left > _Envelope.Right) || (displayBbox.Right < _Envelope.Left)
                    || (displayBbox.Top < _Envelope.Bottom) || (displayBbox.Bottom > _Envelope.Top))
                    return;

                // init histo
                histogram = new List<int[]>();
                for (int i = 0; i < lbands + 1; i++)
                    histogram.Add(new int[256]);

                // bounds of section of image to be displayed
                left = Math.Max(displayBbox.Left, _Envelope.Left);
                top = Math.Min(displayBbox.Top, _Envelope.Top);
                right = Math.Min(displayBbox.Right, _Envelope.Right);
                bottom = Math.Max(displayBbox.Bottom, _Envelope.Bottom);

                trueImageBbox = new BoundingBox(left, bottom, right, top);

                // put display bounds into current projection
                if (transform != null)
                    shownImageBbox = GeometryTransform.TransformBox(trueImageBbox, transform.MathTransform.Inverse());
                else
                    shownImageBbox = trueImageBbox;

                // find min/max x and y pixels needed from image
                imageBR.X =
                    (int)
                    (Math.Max(GT.GroundToImage(shownImageBbox.TopLeft).X,
                              Math.Max(GT.GroundToImage(shownImageBbox.TopRight).X,
                                       Math.Max(GT.GroundToImage(shownImageBbox.BottomLeft).X,
                                                GT.GroundToImage(shownImageBbox.BottomRight).X))) + 1);
                imageBR.Y =
                    (int)
                    (Math.Max(GT.GroundToImage(shownImageBbox.TopLeft).Y,
                              Math.Max(GT.GroundToImage(shownImageBbox.TopRight).Y,
                                       Math.Max(GT.GroundToImage(shownImageBbox.BottomLeft).Y,
                                                GT.GroundToImage(shownImageBbox.BottomRight).Y))) + 1);
                imageTL.X =
                    (int)
                    Math.Min(GT.GroundToImage(shownImageBbox.TopLeft).X,
                             Math.Min(GT.GroundToImage(shownImageBbox.TopRight).X,
                                      Math.Min(GT.GroundToImage(shownImageBbox.BottomLeft).X,
                                               GT.GroundToImage(shownImageBbox.BottomRight).X)));
                imageTL.Y =
                    (int)
                    Math.Min(GT.GroundToImage(shownImageBbox.TopLeft).Y,
                             Math.Min(GT.GroundToImage(shownImageBbox.TopRight).Y,
                                      Math.Min(GT.GroundToImage(shownImageBbox.BottomLeft).Y,
                                               GT.GroundToImage(shownImageBbox.BottomRight).Y)));

                // stay within image
                if (imageBR.X > imagesize.Width)
                    imageBR.X = imagesize.Width;
                if (imageBR.Y > imagesize.Height)
                    imageBR.Y = imagesize.Height;
                if (imageTL.Y < 0)
                    imageTL.Y = 0;
                if (imageTL.X < 0)
                    imageTL.X = 0;

                displayImageLength = (int) (imageBR.X - imageTL.X);
                displayImageHeight = (int) (imageBR.Y - imageTL.Y);

                // find ground coordinates of image pixels
                Geometries.Point groundBR = GT.ImageToGround(imageBR);
                Geometries.Point groundTL = GT.ImageToGround(imageTL);

                // convert ground coordinates to map coordinates to figure out where to place the bitmap
                bitmapBR = new Point((int) map.WorldToImage(trueImageBbox.BottomRight).X + 1,
                                     (int) map.WorldToImage(trueImageBbox.BottomRight).Y + 1);
                bitmapTL = new Point((int) map.WorldToImage(trueImageBbox.TopLeft).X,
                                     (int) map.WorldToImage(trueImageBbox.TopLeft).Y);

                bitmapLength = bitmapBR.X - bitmapTL.X;
                bitmapHeight = bitmapBR.Y - bitmapTL.Y;

                // check to see if image is on its side
                if (bitmapLength > bitmapHeight && displayImageLength < displayImageHeight)
                {
                    displayImageLength = bitmapHeight;
                    displayImageHeight = bitmapLength;
                }
                else
                {
                    displayImageLength = bitmapLength;
                    displayImageHeight = bitmapHeight;
                }

                // scale
                if (intBitDepth == 12)
                    bitScalar = 16.0;
                else if (intBitDepth == 16)
                    bitScalar = 256.0;
                else if (intBitDepth == 32)
                    bitScalar = 16777216.0;

                // 0 pixels in length or height, nothing to display
                if (bitmapLength < 1 || bitmapHeight < 1)
                    return;

                //initialize bitmap
                bitmap = new Bitmap(bitmapLength, bitmapHeight, PixelFormat.Format24bppRgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmapLength, bitmapHeight),
                                                        ImageLockMode.ReadWrite, bitmap.PixelFormat);

                try
                {
                    unsafe
                    {
                        // turn everything yellow, so we can make fill transparent
                        for (int y = 0; y < bitmapHeight; y++)
                        {
                            byte* brow = (byte*) bitmapData.Scan0 + (y*bitmapData.Stride);
                            for (int x = 0; x < bitmapLength; x++)
                            {
                                brow[x*3 + 0] = 0;
                                brow[x*3 + 1] = 255;
                                brow[x*3 + 2] = 255;
                            }
                        }

                        // create 3 dimensional buffer [band][x pixel][y pixel]
                        double[][] tempBuffer = new double[Bands][];
                        double[][][] buffer = new double[Bands][][];
                        for (int i = 0; i < Bands; i++)
                        {
                            buffer[i] = new double[displayImageLength][];
                            for (int j = 0; j < displayImageLength; j++)
                                buffer[i][j] = new double[displayImageHeight];
                        }

                        Band[] band = new Band[Bands];
                        int[] ch = new int[Bands];

                        // get data from image
                        for (int i = 0; i < Bands; i++)
                        {
                            tempBuffer[i] = new double[displayImageLength*displayImageHeight];
                            band[i] = dataset.GetRasterBand(i + 1);

                            band[i].ReadRaster(
                                (int) imageTL.X,
                                (int) imageTL.Y,
                                (int) (imageBR.X - imageTL.X),
                                (int) (imageBR.Y - imageTL.Y),
                                tempBuffer[i], displayImageLength, displayImageHeight, 0, 0);

                            // parse temp buffer into the image x y value buffer
                            long pos = 0;
                            for (int y = 0; y < displayImageHeight; y++)
                            {
                                for (int x = 0; x < displayImageLength; x++, pos++)
                                    buffer[i][x][y] = tempBuffer[i][pos];
                            }

                            if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_BlueBand) ch[i] = 0;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GreenBand) ch[i] = 1;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_RedBand) ch[i] = 2;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_Undefined)
                                ch[i] = 3; // infrared
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GrayIndex) ch[i] = 0;
                            else ch[i] = -1;
                        }

                        // store these values to keep from having to make slow method calls
                        int bitmapTLX = bitmapTL.X;
                        int bitmapTLY = bitmapTL.Y;
                        double imageTop = imageTL.Y;
                        double imageLeft = imageTL.X;
                        double dblMapPixelWidth = map.PixelWidth;
                        double dblMapPixelHeight = map.PixelHeight;
                        double dblMapMinX = map.Envelope.Min.X;
                        double dblMapMaxY = map.Envelope.Max.Y;
                        double geoTop, geoLeft, geoHorzPixRes, geoVertPixRes, geoXRot, geoYRot;

                        // get inverse values
                        geoTop = GT.Inverse[3];
                        geoLeft = GT.Inverse[0];
                        geoHorzPixRes = GT.Inverse[1];
                        geoVertPixRes = GT.Inverse[5];
                        geoXRot = GT.Inverse[2];
                        geoYRot = GT.Inverse[4];

                        double dblXScale = (imageBR.X - imageTL.X)/(displayImageLength - 1);
                        double dblYScale = (imageBR.Y - imageTL.Y)/(displayImageHeight - 1);
                        double[] dblPoint;

                        // get inverse transform  
                        // NOTE: calling transform.MathTransform.Inverse() once and storing it
                        // is much faster than having to call every time it is needed
                        IMathTransform inverseTransform = null;
                        if (transform != null)
                            inverseTransform = transform.MathTransform.Inverse();

                        for (PixY = 0; PixY < bitmapBR.Y - bitmapTL.Y; PixY++)
                        {
                            byte* row = (byte*) bitmapData.Scan0 + ((int) Math.Round(PixY)*bitmapData.Stride);

                            for (PixX = 0; PixX < bitmapBR.X - bitmapTL.X; PixX++)
                            {
                                // same as Map.ImageToGround(), but much faster using stored values...rather than called each time
                                GndX = dblMapMinX + (PixX + (double) bitmapTLX)*dblMapPixelWidth;
                                GndY = dblMapMaxY - (PixY + (double) bitmapTLY)*dblMapPixelHeight;

                                // transform ground point if needed
                                if (transform != null)
                                {
                                    dblPoint = inverseTransform.Transform(new double[] {GndX, GndY});
                                    GndX = dblPoint[0];
                                    GndY = dblPoint[1];
                                }

                                // same as GeoTransform.GroundToImage(), but much faster using stored values...
                                ImgX = (geoLeft + geoHorzPixRes*GndX + geoXRot*GndY);
                                ImgY = (geoTop + geoYRot*GndX + geoVertPixRes*GndY);

                                if (ImgX < imageTL.X || ImgX > imageBR.X || ImgY < imageTL.Y || ImgY > imageBR.Y)
                                    continue;

                                // color correction
                                for (int i = 0; i < Bands; i++)
                                {
                                    intVal[i] =
                                        buffer[i][(int) ((ImgX - imageLeft)/dblXScale)][
                                            (int) ((ImgY - imageTop)/dblYScale)];

                                    imageVal = SpotVal = intVal[i] = intVal[i]/bitScalar;

                                    if (bColorCorrect)
                                    {
                                        intVal[i] = ApplyColorCorrection(imageVal, SpotVal, ch[i], GndX, GndY);

                                        // if pixel is within ground boundary, add its value to the histogram
                                        if (ch[i] != -1 && intVal[i] > 0 && (rectHistoBounds.Bottom >= (int) GndY) &&
                                            rectHistoBounds.Top <= (int) GndY &&
                                            rectHistoBounds.Left <= (int) GndX && rectHistoBounds.Right >= (int) GndX)
                                        {
                                            histogram[ch[i]][(int) intVal[i]]++;
                                        }
                                    }

                                    if (intVal[i] > 255)
                                        intVal[i] = 255;
                                }

                                // luminosity
                                if (lbands >= 3)
                                    histogram[lbands][(int) (intVal[2]*0.2126 + intVal[1]*0.7152 + intVal[0]*0.0722)]++;

                                WritePixel(PixX, intVal, iPixelSize, ch, row);
                            }
                        }
                    }
                }

                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            bitmap.MakeTransparent(Color.Yellow);
            if (transparentColor != Color.Empty)
                bitmap.MakeTransparent(transparentColor);
            g.DrawImage(bitmap, new Point(bitmapTL.X, bitmapTL.Y));
        }

        // faster than rotated display
        private void GetNonRotatedPreview(Dataset dataset, Size size, Graphics g,
                                          BoundingBox bbox, ICoordinateSystem mapProjection)
        {
            double[] geoTrans = new double[6];
            dataset.GetGeoTransform(geoTrans);

            // default transform
            if (!bUseRotation && !bHaveSpot || (geoTrans[0] == 0 && geoTrans[3] == 0))
                geoTrans = new double[] {999.5, 1, 0, 1000.5, 0, -1};
            Bitmap bitmap = null;
            GT = new GeoTransform(geoTrans);
            int DsWidth = 0;
            int DsHeight = 0;
            BitmapData bitmapData = null;
            double[] intVal = new double[Bands];
            int p_indx;
            double bitScalar = 1.0;

            double dblImginMapW = 0, dblImginMapH = 0, dblLocX = 0, dblLocY = 0;

            int iPixelSize = 3; //Format24bppRgb = byte[b,g,r] 

            if (dataset != null)
            {
                //check if image is in bounding box
                if ((bbox.Left > _Envelope.Right) || (bbox.Right < _Envelope.Left)
                    || (bbox.Top < _Envelope.Bottom) || (bbox.Bottom > _Envelope.Top))
                    return;

                DsWidth = imagesize.Width;
                DsHeight = imagesize.Height;

                histogram = new List<int[]>();
                for (int i = 0; i < lbands + 1; i++)
                    histogram.Add(new int[256]);

                double left = Math.Max(bbox.Left, _Envelope.Left);
                double top = Math.Min(bbox.Top, _Envelope.Top);
                double right = Math.Min(bbox.Right, _Envelope.Right);
                double bottom = Math.Max(bbox.Bottom, _Envelope.Bottom);

                double x1 = Math.Abs(GT.PixelX(left));
                double y1 = Math.Abs(GT.PixelY(top));
                double imgPixWidth = GT.PixelXwidth(right - left);
                double imgPixHeight = GT.PixelYwidth(bottom - top);

                //get screen pixels image should fill 
                double dblBBoxW = bbox.Right - bbox.Left;
                double dblBBoxtoImgPixX = (double) imgPixWidth/(double) dblBBoxW;
                dblImginMapW = (double) size.Width*dblBBoxtoImgPixX*GT.HorizontalPixelResolution;


                double dblBBoxH = bbox.Top - bbox.Bottom;
                double dblBBoxtoImgPixY = (double) imgPixHeight/(double) dblBBoxH;
                dblImginMapH = (double) size.Height*dblBBoxtoImgPixY*-GT.VerticalPixelResolution;

                if ((dblImginMapH == 0) || (dblImginMapW == 0))
                    return;

                // ratios of bounding box to image ground space
                double dblBBoxtoImgX = (double) size.Width/dblBBoxW;
                double dblBBoxtoImgY = (double) size.Height/dblBBoxH;

                // set where to display bitmap in Map
                if (bbox.Left != left)
                {
                    if (bbox.Right != right)
                        dblLocX = (_Envelope.Left - bbox.Left)*dblBBoxtoImgX;
                    else
                        dblLocX = (double) size.Width - dblImginMapW;
                }
                if (bbox.Top != top)
                {
                    if (bbox.Bottom != bottom)
                        dblLocY = (bbox.Top - _Envelope.Top)*dblBBoxtoImgY;
                    else
                        dblLocY = (double) size.Height - dblImginMapH;
                }

                // scale
                if (intBitDepth == 12)
                    bitScalar = 16.0;
                else if (intBitDepth == 16)
                    bitScalar = 256.0;
                else if (intBitDepth == 32)
                    bitScalar = 16777216.0;

                try
                {
                    bitmap = new Bitmap((int) Math.Round(dblImginMapW), (int) Math.Round(dblImginMapH),
                                        PixelFormat.Format24bppRgb);
                    bitmapData =
                        bitmap.LockBits(
                            new Rectangle(0, 0, (int) Math.Round(dblImginMapW), (int) Math.Round(dblImginMapH)),
                            ImageLockMode.ReadWrite, bitmap.PixelFormat);

                    unsafe
                    {
                        double[][] buffer = new double[Bands][];
                        Band[] band = new Band[Bands];
                        int[] ch = new int[Bands];

                        // get data from image
                        for (int i = 0; i < Bands; i++)
                        {
                            buffer[i] = new double[(int) Math.Round(dblImginMapW)*(int) Math.Round(dblImginMapH)];
                            band[i] = dataset.GetRasterBand(i + 1);

                            band[i].ReadRaster((int) Math.Round(x1), (int) Math.Round(y1), (int) Math.Round(imgPixWidth),
                                               (int) Math.Round(imgPixHeight),
                                               buffer[i], (int) Math.Round(dblImginMapW), (int) Math.Round(dblImginMapH),
                                               0, 0);

                            if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_BlueBand) ch[i] = 0;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GreenBand) ch[i] = 1;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_RedBand) ch[i] = 2;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_Undefined)
                                ch[i] = 3; // infrared
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GrayIndex) ch[i] = 4;
                            else ch[i] = -1;
                        }

                        if (intBitDepth == 32)
                            ch = new int[] {0, 1, 2};

                        p_indx = 0;
                        for (int y = 0; y < Math.Round(dblImginMapH); y++)
                        {
                            byte* row = (byte*) bitmapData.Scan0 + (y*bitmapData.Stride);
                            for (int x = 0; x < Math.Round(dblImginMapW); x++, p_indx++)
                            {
                                for (int i = 0; i < Bands; i++)
                                {
                                    intVal[i] = buffer[i][p_indx]/bitScalar;

                                    if (bColorCorrect)
                                    {
                                        intVal[i] = ApplyColorCorrection(intVal[i], 0, ch[i], 0, 0);

                                        if (lbands >= 3)
                                            histogram[lbands][
                                                (int) (intVal[2]*0.2126 + intVal[1]*0.7152 + intVal[0]*0.0722)]++;
                                    }

                                    if (intVal[i] > 255)
                                        intVal[i] = 255;
                                }

                                WritePixel(x, intVal, iPixelSize, ch, row);
                            }
                        }
                    }
                }
                catch
                {
                    return;
                }
                finally
                {
                    if (bitmapData != null)
                        bitmap.UnlockBits(bitmapData);
                }
            }
            if (transparentColor != Color.Empty)
                bitmap.MakeTransparent(transparentColor);
            g.DrawImage(bitmap, new Point((int) Math.Round(dblLocX), (int) Math.Round(dblLocY)));
        }

        protected unsafe void WritePixel(double x, double[] intVal, int iPixelSize, int[] ch, byte* row)
        {
            // write out pixels
            // black and white
            if (Bands == 1 && intBitDepth != 32)
            {
                if (bShowClip)
                {
                    if (intVal[0] == 0)
                    {
                        row[(int) Math.Round(x)*iPixelSize] = 255;
                        row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                        row[(int) Math.Round(x)*iPixelSize + 2] = 0;
                    }
                    else if (intVal[0] == 255)
                    {
                        row[(int) Math.Round(x)*iPixelSize] = 0;
                        row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                        row[(int) Math.Round(x)*iPixelSize + 2] = 255;
                    }
                    else
                    {
                        row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[0];
                        row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[0];
                        row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[0];
                    }
                }
                else
                {
                    row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[0];
                    row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[0];
                    row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[0];
                }
            }
                // IR grayscale
            else if (bDisplayIR && Bands == 4)
            {
                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] == 3)
                    {
                        if (bShowClip)
                        {
                            if (intVal[3] == 0)
                            {
                                row[(int) Math.Round(x)*iPixelSize] = 255;
                                row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                                row[(int) Math.Round(x)*iPixelSize + 2] = 0;
                            }
                            else if (intVal[3] == 255)
                            {
                                row[(int) Math.Round(x)*iPixelSize] = 0;
                                row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                                row[(int) Math.Round(x)*iPixelSize + 2] = 255;
                            }
                            else
                            {
                                row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[i];
                                row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[i];
                                row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[i];
                            }
                        }
                        else
                        {
                            row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[i];
                            row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[i];
                            row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[i];
                        }
                    }
                    else
                        continue;
                }
            }
                // CIR
            else if (bDisplayCIR && Bands == 4)
            {
                if (bShowClip)
                {
                    if (intVal[0] == 0 && intVal[1] == 0 && intVal[3] == 0)
                    {
                        intVal[3] = intVal[0] = 0;
                        intVal[1] = 255;
                    }
                    else if (intVal[0] == 255 && intVal[1] == 255 && intVal[3] == 255)
                        intVal[1] = intVal[0] = 0;
                }

                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] != 0 && ch[i] != -1)
                        row[(int) Math.Round(x)*iPixelSize + ch[i] - 1] = (byte) intVal[i];
                }
            }
                // RGB
            else
            {
                if (bShowClip)
                {
                    if (intVal[0] == 0 && intVal[1] == 0 && intVal[2] == 0)
                    {
                        intVal[0] = intVal[1] = 0;
                        intVal[2] = 255;
                    }
                    else if (intVal[0] == 255 && intVal[1] == 255 && intVal[2] == 255)
                        intVal[1] = intVal[2] = 0;
                }

                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] != 3 && ch[i] != -1)
                        row[(int) Math.Round(x)*iPixelSize + ch[i]] = (byte) intVal[i];
                }
            }
        }

        // apply any color correction to pixel
        private double ApplyColorCorrection(double imageVal, double spotVal, int channel, double GndX, double GndY)
        {
            double finalVal;
            double distance;
            double imagePct, spotPct;

            finalVal = imageVal;

            if (bHaveSpot)
            {
                // gamma
                if (NonSpotGamma != 1)
                    imageVal = 256*Math.Pow(((double) imageVal/256), NonSpotGamma);

                // gain
                if (channel == 2)
                    imageVal = imageVal*dblNonSpotGain[0];
                else if (channel == 1)
                    imageVal = imageVal*dblNonSpotGain[1];
                else if (channel == 0)
                    imageVal = imageVal*dblNonSpotGain[2];
                else if (channel == 3)
                    imageVal = imageVal*dblNonSpotGain[3];

                if (imageVal > 255)
                    imageVal = 255;

                // curve
                if (lstNonSpotCurveLut != null)
                    if (lstNonSpotCurveLut.Count != 0)
                    {
                        if (channel == 2 || channel == 4)
                            imageVal = lstNonSpotCurveLut[0][(int) imageVal];
                        else if (channel == 1)
                            imageVal = lstNonSpotCurveLut[1][(int) imageVal];
                        else if (channel == 0)
                            imageVal = lstNonSpotCurveLut[2][(int) imageVal];
                        else if (channel == 3)
                            imageVal = lstNonSpotCurveLut[3][(int) imageVal];
                    }

                finalVal = imageVal;

                distance = Math.Sqrt(Math.Pow(GndX - (double) SpotPoint.X, 2) + Math.Pow(GndY - (double) SpotPoint.Y, 2));

                if (distance <= innerSpotRadius + outerSpotRadius)
                {
                    // gamma
                    if (SpotGamma != 1)
                        spotVal = 256*Math.Pow((spotVal/256), SpotGamma);

                    // gain
                    if (channel == 2)
                        spotVal = spotVal*dblSpotGain[0];
                    else if (channel == 1)
                        spotVal = spotVal*dblSpotGain[1];
                    else if (channel == 0)
                        spotVal = spotVal*dblSpotGain[2];
                    else if (channel == 3)
                        spotVal = spotVal*dblSpotGain[3];

                    if (spotVal > 255)
                        spotVal = 255;

                    // curve
                    if (lstSpotCurveLut != null)
                        if (lstSpotCurveLut.Count != 0)
                        {
                            if (channel == 2 || channel == 4)
                                spotVal = lstSpotCurveLut[0][(int) spotVal];
                            else if (channel == 1)
                                spotVal = lstSpotCurveLut[1][(int) spotVal];
                            else if (channel == 0)
                                spotVal = lstSpotCurveLut[2][(int) spotVal];
                            else if (channel == 3)
                                spotVal = lstSpotCurveLut[3][(int) spotVal];
                        }

                    if (distance < innerSpotRadius)
                        finalVal = spotVal;
                    else
                    {
                        imagePct = (distance - innerSpotRadius)/outerSpotRadius;
                        spotPct = 1 - imagePct;

                        finalVal = (Math.Round((spotVal*spotPct) + (imageVal*imagePct)));
                    }
                }
            }

            // gamma
            if (gamma != 1)
                finalVal = (256*Math.Pow((finalVal/256), gamma));


            switch (channel)
            {
                case 2:
                    finalVal = finalVal*dblGain[0];
                    break;
                case 1:
                    finalVal = finalVal*dblGain[1];
                    break;
                case 0:
                    finalVal = finalVal*dblGain[2];
                    break;
                case 3:
                    finalVal = finalVal*dblGain[3];
                    break;
            }

            if (finalVal > 255)
                finalVal = 255;

            // curve
            if (lstCurveLut != null)
                if (lstCurveLut.Count != 0)
                {
                    if (channel == 2 || channel == 4)
                        finalVal = lstCurveLut[0][(int) finalVal];
                    else if (channel == 1)
                        finalVal = lstCurveLut[1][(int) finalVal];
                    else if (channel == 0)
                        finalVal = lstCurveLut[2][(int) finalVal];
                    else if (channel == 3)
                        finalVal = lstCurveLut[3][(int) finalVal];
                }

            return finalVal;
        }

        /// <summary>
        /// Build histogram and statistics
        /// </summary>
        /// <param name="bQuick">If true, build histogram off of smaller subsample of image</param>
        public void BuildHisto(bool bQuick)
        {
            Dataset dataset = _GdalDataset;
            int height, width, Bands;
            int p_indx = 0;
            int intVal;
            double[] stdDev = new double[4];
            int maxVal;

            if (bQuick)
            {
                height = 20;
                width = (int) ((double) 20*((double) dataset.RasterXSize/(double) dataset.RasterYSize));
            }
            else
            {
                height = 3000; // dataset.RasterYSize;
                width = (int) ((double) 3000*((double) dataset.RasterXSize/(double) dataset.RasterYSize));
                    // dataset.RasterXSize;
            }

            Bands = dataset.RasterCount;

            histogram = new List<int[]>();
            histoMean = new double[Bands];

            for (int band = 1; band <= Bands; band++)
            {
                List<object> lstObj = new List<object>();

                if (intBitDepth == 8)
                    histogram.Add(new int[256]);
                else if (intBitDepth == 12)
                    histogram.Add(new int[4096]);
                else
                    histogram.Add(new int[65536]);

                for (int i = 0; i < histogram[band - 1].Length; i++)
                    histogram[band - 1][i] = 0;

                maxVal = histogram[0].Length - 1;

                Band RBand = dataset.GetRasterBand(band);
                double[] buffer = new double[width*height];
                RBand.ReadRaster(0, 0, dataset.RasterXSize, dataset.RasterYSize, buffer, width, height, 0, 0);

                p_indx = 0;

                histoMean[band - 1] = 0;

                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++, p_indx++)
                    {
                        intVal = (int) buffer[p_indx];

                        // gamma
                        if (NonSpotGamma != 1)
                            intVal = (int) (256*Math.Pow(((double) intVal/256), NonSpotGamma));

                        // gain
                        intVal = (int) ((double) intVal*dblGain[band - 1]);

                        if (intVal > maxVal)
                            intVal = maxVal;

                        // curves
                        if (lstNonSpotCurveLut != null)
                            if (lstNonSpotCurveLut.Count != 0)
                                intVal = lstNonSpotCurveLut[band - 1][intVal];

                        buffer[p_indx] = (byte) intVal;

                        histogram[band - 1][intVal]++;
                        histoMean[band - 1] += intVal;
                    }
                }
                histoMean[band - 1] /= buffer.Length;
                stdDev[band - 1] = CalcStandardDeviation(buffer, histoMean[band - 1]);
            }

            // set brightness and contrast
            if (Bands > 1)
            {
                histoBrightness = (histoMean[0]*0.2126 + histoMean[1]*0.7152 + histoMean[2]*0.0722)/2.55;
                histoContrast = (stdDev[0]*0.2126 + stdDev[1]*0.7152 + stdDev[2]*0.0722)/1.28;
            }
            else
            {
                histoBrightness = histoMean[0]/2.55;
                histoContrast = stdDev[0]/1.28;
            }
        }

        private double CalcStandardDeviation(double[] buffer, double mean)
        {
            double dblAccum = 0;

            for (int i = 0; i < buffer.Length; i++)
                dblAccum += Math.Pow((buffer[i] - mean), 2);

            dblAccum = dblAccum/buffer.Length;

            return Math.Sqrt(dblAccum);
        }

        // find min and max pixel values of the image
        private void ComputeStretch()
        {
            double min = 99999999, max = -99999999;
            int width, height;

            if (_GdalDataset.RasterYSize < 4000)
            {
                height = _GdalDataset.RasterYSize;
                width = _GdalDataset.RasterXSize;
            }
            else
            {
                height = 4000;
                width = (int) ((double) 4000*((double) _GdalDataset.RasterXSize/(double) _GdalDataset.RasterYSize));
            }

            double[] buffer = new double[width*height];

            for (int band = 1; band <= lbands; band++)
            {
                Band RBand = _GdalDataset.GetRasterBand(band);
                RBand.ReadRaster(0, 0, _GdalDataset.RasterXSize, _GdalDataset.RasterYSize, buffer, width, height, 0, 0);

                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] < min)
                        min = buffer[i];
                    if (buffer[i] > max)
                        max = buffer[i];
                }
            }

            if (intBitDepth == 12)
            {
                min /= 16;
                max /= 16;
            }
            else if (intBitDepth == 16)
            {
                min /= 256;
                max /= 256;
            }

            if (max > 255)
                max = 255;

            stretchPoint = new Point((int) min, (int) max);
        }

        #region Disposers and finalizers

        private bool disposed = false;

        /// <summary>
        /// Disposes the GdalRasterLayer and release the raster file
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
                        finally
                        {
                            _GdalDataset = null;
                        }
                    }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~GdalRasterLayer()
        {
            Dispose(true);
        }

        #endregion
    }
}