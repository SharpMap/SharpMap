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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Common.Logging;
using GeoAPI;
using GeoAPI.Geometries;
using OSGeo.GDAL;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using SharpMap.CoordinateSystems;
using SharpMap.Data;
using SharpMap.Extensions.Data;
using Geometry = GeoAPI.Geometries.IGeometry;
using SharpMap.Rendering.Thematics;
using Point = System.Drawing.Point;

using Polygon = GeoAPI.Geometries.IPolygon;
using SharpMap.Utilities;

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
    [Serializable]
    public class GdalRasterLayer : Layer, ICanQueryLayer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof (GdalRasterLayer));

        static GdalRasterLayer()
        {
            GdalConfiguration.ConfigureGdal();
        }

        private IGeometryFactory _factory;
        protected IGeometryFactory Factory
        {
            get
            {
                return _factory ?? (_factory = GeometryServiceProvider.Instance.CreateGeometryFactory());
            }
            set { _factory = value; }
        }

        protected Envelope _envelope;
        protected Dataset _gdalDataset;
        //internal GeoTransform _geoTransform;
        
        //private double _histoBrightness, _histoContrast;
        //private double[] _histoMean;
        protected Size _imageSize;

        // outer radius is feather between inner radius and rest of image

        // outer radius is feather between inner radius and rest of image

        private Point _stretchPoint;
        protected bool _useRotation = true; // use geographic information

        #region accessors

        /// <summary>
        ///  Gets the version of fwTools that was used to compile and test this GdalRasterLayer
        /// </summary>
        public static string FWToolsVersion
        {
#pragma warning disable 612,618
            get { return FwToolsHelper.FwToolsVersion; }
#pragma warning restore 612,618
        }

        /// <summary>
        /// Gets or sets the filename of the raster file
        /// </summary>
        public string Filename { get; protected internal set; }

        /// <summary>
        /// Gets or sets a rectangle that is used to tile the image when rendering
        /// </summary>
        public Size TilingSize { get; set; }
        
        /// <summary>
        /// Gets or sets the bit depth of the raster file
        /// </summary>
        public int BitDepth { get; set; }

        /// <summary>
        /// Gets or set the projection of the raster file
        /// </summary>
        public string Projection { get; set; }

        /// <summary>
        /// Gets or sets to display IR Band
        /// </summary>
        public bool DisplayIR { get; set; }

        /// <summary>
        /// Gets or sets to display color InfraRed
        /// </summary>
        public bool DisplayCIR { get; set; }

        /// <summary>
        /// Gets or sets to display clip
        /// </summary>
        public bool ShowClip { get; set; }

        /// <summary>
        /// Gets or sets to display gamma
        /// </summary>
        public double Gamma { get; set; }

        /// <summary>
        /// Gets or sets to display gamma for Spot spot
        /// </summary>
        public double SpotGamma { get; set; }

        /// <summary>
        /// Gets or sets to display gamma for NonSpot
        /// </summary>
        public double NonSpotGamma { get; set; }

        /// <summary>
        /// Gets or sets to display red Gain
        /// </summary>
        public double[] Gain { get; set; }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] SpotGain { get; set; }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] NonSpotGain { get; set; }

        /// <summary>
        /// Gets or sets to display curve lut
        /// </summary>
        public List<int[]> CurveLut { get; set; }

        /// <summary>
        /// Correct Spot spot
        /// </summary>
        public bool HaveSpot { get; set; }

        /// <summary>
        /// Gets or sets to display curve lut for Spot spot
        /// </summary>
        public List<int[]> SpotCurveLut { get; set; }

        /// <summary>
        /// Gets or sets to display curve lut for NonSpot
        /// </summary>
        public List<int[]> NonSpotCurveLut { get; set; }

        /// <summary>
        /// Gets or sets the center point of the Spot spot
        /// </summary>
        public PointF SpotPoint { get; set; }

        /// <summary>
        /// Gets or sets the inner radius for the spot
        /// </summary>
        public double InnerSpotRadius { get; set; }

        /// <summary>
        /// Gets or sets the outer radius for the spot (feather zone)
        /// </summary>
        public double OuterSpotRadius { get; set; }

        /// <summary>
        /// Gets the true histogram
        /// </summary>
        public List<int[]> Histogram { get; private set; }

        /// <summary>
        /// Gets the quick histogram mean
        /// </summary>
        public double[] HistoMean
        {
            get
            {
                return null;// _histoMean;
            }
        }

        /// <summary>
        /// Gets the quick histogram brightness
        /// </summary>
        public double HistoBrightness
        {
            get
            {
                return double.NaN;
                //_histoBrightness;
            }
        }

        /// <summary>
        /// Gets the quick histogram contrast
        /// </summary>
        public double HistoContrast
        {
            get
            {
                return double.NaN;
                //_histoContrast; 
            }
        }

        /// <summary>
        /// Gets the number of bands
        /// </summary>
        public int Bands { get; internal set; }

        /// <summary>
        /// Gets the GSD (Horizontal)
        /// </summary>
        public double GSD
        {
            get { return new GeoTransform(_gdalDataset).HorizontalPixelResolution; }
        }

        ///<summary>
        /// Use rotation information
        /// </summary>
        public bool UseRotation
        {
            get { return _useRotation; }
            set
            {
                _useRotation = value;
                _envelope = GetExtent();
            }
        }

        public Size Size
        {
            get { return _imageSize; }
        }

        public bool ColorCorrect { get; set; }

        public Rectangle HistoBounds { get; set; }

        [Obsolete("Use CoordinateTransformation instead")]
        public ICoordinateTransformation Transform
        {
            get { return CoordinateTransformation; }
            protected set
            {
                CoordinateTransformation = value;
            }
        }

        public Color TransparentColor { get; set; }

        /// <summary>
        /// Gets or sets the minimum- and maximum pixel byte values of all raster bands.
        /// <para/>Knowing these, you can scale the color range
        /// </summary>
        public Point StretchPoint
        {
            get
            {
                if (_stretchPoint.IsEmpty)
                {
                    ComputeStretch();
                }

                return _stretchPoint;
            }
            set
            {
                _stretchPoint = value;
            }
        }

        #endregion

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The name of the layer</param>
        protected GdalRasterLayer(string layerName)
        {
            SpotPoint = new PointF(0, 0);
            Projection = "";
            BitDepth = 8;
            NonSpotGain = new double[] { 1, 1, 1, 1 };
            SpotGain = new double[] { 1, 1, 1, 1 };
            Gain = new double[] { 1, 1, 1, 1 };
            NonSpotGamma = 1;
            SpotGamma = 1;
            Gamma = 1;
            TransparentColor = Color.Empty;
            ColorCorrect = true;

            LayerName = layerName;

            Gdal.AllRegister();
        }

        /// <summary>
        /// initialize a Gdal based raster layer - from file
        /// </summary>
        /// <param name="strLayerName">Name of layer</param>
        /// <param name="imageFilename">location of image</param>
        public GdalRasterLayer(string strLayerName, string imageFilename) : this(strLayerName)
        {
            Filename = imageFilename;
            OpenDataset(imageFilename);
        }

        /// <summary>
        /// initialize a Gdal based raster layer - from buffer
        /// </summary>
        /// <param name="strLayerName">Name of layer</param>
        /// <param name="imageBuffer">image buffer</param>
        /// 
        public GdalRasterLayer(string strLayerName, byte[] imageBuffer) : this(strLayerName)
        {
            // a layer data name
            string memoryFileName = $"/vsimem/{strLayerName}Data";

            Filename = memoryFileName;
            OpenDataset(imageBuffer, memoryFileName);
        }

        /// <summary>
        /// OpenDataset for buffer
        /// </summary>
        /// <param name="imageBuffer">image buffer</param>
        /// <param name="memoryFileName">File Name in Memory</param>
        protected void OpenDataset(byte[] imageBuffer, string memoryFileName)
        {
            try
            {
                Gdal.FileFromMemBuffer(memoryFileName, imageBuffer);

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Opening dataset " + memoryFileName);

                _gdalDataset = Gdal.Open(memoryFileName, Access.GA_ReadOnly);

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Reading projection for " + memoryFileName);

                // have gdal read the projection
                Projection = _gdalDataset.GetProjectionRef();

                if (!string.IsNullOrEmpty(Projection))
                {
                    using (var p = new OSGeo.OSR.SpatialReference(null))
                    {
                        string wkt = Projection;
                        int res = p.ImportFromWkt(ref wkt);
                        int srid = 0;
                        if (res == 0)
                        {
                            if (Projection.StartsWith("PROJCS"))
                                int.TryParse(p.GetAuthorityCode("PROJCS"), out srid);
                            else if (Projection.StartsWith("GEOGCS"))
                                int.TryParse(p.GetAuthorityCode("GEOGCS"), out srid);
                            else
                                srid = p.AutoIdentifyEPSG();
                        }
                        SRID = srid;
                    }
                }

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Read sizes" + memoryFileName);

                _imageSize = new Size(_gdalDataset.RasterXSize, _gdalDataset.RasterYSize);
                _envelope = GetExtent();

                HistoBounds = new Rectangle((int)_envelope.MinX, (int)_envelope.MinY, (int)_envelope.Width,
                    (int)_envelope.Height);
                Bands = _gdalDataset.RasterCount;

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Dataset open, " + memoryFileName);

            }
            catch (Exception ex)
            {
                _gdalDataset = null;
                throw new Exception("Couldn't load " + memoryFileName + "\n\n" + ex.Message + ex.InnerException);
            }
            finally
            {
                Gdal.Unlink(memoryFileName);
            }
        }

        /// <summary>
        /// OpenDataset for imageFile
        /// </summary>
        /// <param name="imageFilename">location of image</param>
        protected void OpenDataset(string imageFilename)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug("Opening dataset " + imageFilename);
                _gdalDataset = Gdal.OpenShared(imageFilename, Access.GA_ReadOnly);

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Reading projection for " + imageFilename);
                
                // have gdal read the projection
                Projection = _gdalDataset.GetProjectionRef();

                // no projection info found in the image...check for a prj
                if (Projection == "" &&
                    File.Exists(imageFilename.Substring(0, imageFilename.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase)) + ".prj"))
                {
                    Projection =
                        File.ReadAllText(imageFilename.Substring(0, imageFilename.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase)) + ".prj");
                }

                if (!String.IsNullOrEmpty(Projection))
                {
                    using (var p = new OSGeo.OSR.SpatialReference(null))
                    {
                        var wkt = Projection;
                        var res = p.ImportFromWkt(ref wkt);
                        var srid = 0;
                        if (res == 0)
                        {
                            if (Projection.StartsWith("PROJCS"))
                                int.TryParse(p.GetAuthorityCode("PROJCS"), out srid);
                            else if (Projection.StartsWith("GEOGCS"))
                                int.TryParse(p.GetAuthorityCode("GEOGCS"), out srid);
                            else
                                srid = p.AutoIdentifyEPSG();
                        }
                        SRID = srid;
                    }
                }

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Read sizes" + imageFilename);

                _imageSize = new Size(_gdalDataset.RasterXSize, _gdalDataset.RasterYSize);
                _envelope = GetExtent();

                HistoBounds = new Rectangle((int) _envelope.MinX, (int) _envelope.MinY, (int) _envelope.Width,
                    (int) _envelope.Height);
                Bands = _gdalDataset.RasterCount;

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Dataset open, " + imageFilename);

            }
            catch (Exception ex)
            {
                _gdalDataset = null;
                throw new Exception("Couldn't load " + imageFilename + "\n\n" + ex.Message + ex.InnerException);
            }
        }

        protected override void ReleaseManagedResources()
        {
            _factory = null;
            //_geoTransform = null;
            if (_gdalDataset != null)
            {
                _gdalDataset.Dispose();
                _gdalDataset = null;
            }
            
            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override Envelope Envelope
        {
            get { return new Envelope(_envelope); }
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, MapViewport map)
        {
            CheckDisposed();
            if (TilingSize.IsEmpty || (TilingSize.Width > map.Size.Width && TilingSize.Height > map.Size.Height))
            {
                GetPreview(_gdalDataset, map.Size, g, map.Envelope, null, map);
            }
            else
            {
                foreach (var tileEnvelope in Tile(map))
                {
                    GetPreview(_gdalDataset, map.Size, g, tileEnvelope, null, map);
                }
            }
            //base.Render(g, map);
        }

        private IEnumerable<Envelope> Tile(MapViewport map)
        {
            var lt = Point.Truncate(map.WorldToImage(map.Envelope.TopLeft()));
            var rb = Point.Ceiling(map.WorldToImage(map.Envelope.BottomRight()));
            
            var size = new Size(rb.X - lt.X, rb.Y - lt.Y);
            var fullRect = new Rectangle(lt, size);
            
            var overlapSize = new Size(2, 2);

            for (var top = lt.Y; top < fullRect.Bottom; top+= TilingSize.Height )
            {
                for (var left = lt.X; left < fullRect.Right; left += TilingSize.Width)
                {
                    var partialRect = new Rectangle(new Point(left - overlapSize.Width, top - overlapSize.Height),
                                                    Size.Add(TilingSize, overlapSize));
                    var res = new Envelope(map.ImageToWorld(partialRect.Location));
                    res.ExpandToInclude(map.ImageToWorld(new PointF(partialRect.Right, partialRect.Bottom)));
                    yield return res;
                }
            }
        }

        // get raster projection
        public ICoordinateSystem GetProjection()
        {

            try
            {
                return Session.Instance.CoordinateSystemServices.GetCoordinateSystem(SRID);
            }
            catch (Exception ee)
            {
                if (_logger.IsErrorEnabled)
                    _logger.Error("Error parsing projection", ee);
            }

            return null;
        }

        /// <summary>
        /// Zoom to the native resolution
        /// </summary>
        /// <param name="map">The map object</param>
        /// <returns>The zoom factor for a 1:1 Scale</returns>
        public double GetOneToOne(Map map)
        {
            double dsWidth = _imageSize.Width;
            double dsHeight = _imageSize.Height;
            //double left, top, right, bottom;
            double dblImgEnvW, dblImgEnvH, dblWindowGndW, dblWindowGndH, dblImginMapW, dblImginMapH;

            var bbox = map.Envelope;
            Size size = map.Size;

            //// bounds of section of image to be displayed
            //left = Math.Max(bbox.MinX, _envelope.MinX);
            //top = Math.Min(bbox.MaxY, _envelope.MaxY);
            //right = Math.Min(bbox.MaxX, _envelope.MaxX);
            //bottom = Math.Max(bbox.MinY, _envelope.MinY);

            // height and width of envelope of transformed image in ground space
            dblImgEnvW = _envelope.Width;
            dblImgEnvH = _envelope.Height;

            // height and width of display window in ground space
            dblWindowGndW = bbox.Width;
            dblWindowGndH = bbox.Height;

            // height and width of transformed image in pixel space
            dblImginMapW = size.Width * (dblImgEnvW / dblWindowGndW);
            dblImginMapH = size.Height * (dblImgEnvH / dblWindowGndH);

            // image was not turned on its side
            if ((dblImginMapW > dblImginMapH && dsWidth > dsHeight) ||
                (dblImginMapW < dblImginMapH && dsWidth < dsHeight))
                return map.Zoom * (dblImginMapW / dsWidth);
            // image was turned on its side
            else
                return map.Zoom * (dblImginMapH / dsWidth);
        }

        // zooms to nearest tiff internal resolution set
        public double GetZoomNearestRSet(Map map, bool bZoomIn)
        {
            double dsWidth = _imageSize.Width;
            double dsHeight = _imageSize.Height;
            //double left, top, right, bottom;
            double dblImgEnvW, dblImgEnvH, dblWindowGndW, dblWindowGndH, dblImginMapW, dblImginMapH;
            double dblTempWidth;

            Envelope bbox = map.Envelope;
            Size size = map.Size;

            //// bounds of section of image to be displayed
            //left = Math.Max(bbox.MinX, _envelope.MinX);
            //top = Math.Min(bbox.MaxY, _envelope.MaxY);
            //right = Math.Min(bbox.MaxX, _envelope.MaxX);
            //bottom = Math.Max(bbox.MinY, _envelope.MinY);

            // height and width of envelope of transformed image in ground space
            dblImgEnvW = _envelope.Width;
            dblImgEnvH = _envelope.Height;

            // height and width of display window in ground space
            dblWindowGndW = bbox.Width;
            dblWindowGndH = bbox.Height;

            // height and width of transformed image in pixel space
            dblImginMapW = size.Width * (dblImgEnvW / dblWindowGndW);
            dblImginMapH = size.Height * (dblImgEnvH / dblWindowGndH);

            // image was not turned on its side
            if ((dblImginMapW > dblImginMapH && dsWidth > dsHeight) ||
                (dblImginMapW < dblImginMapH && dsWidth < dsHeight))
                dblTempWidth = dblImginMapW;
            else
                dblTempWidth = dblImginMapH;

            // zoom level is within the r sets
            if (dsWidth > dblTempWidth && (dsWidth / Math.Pow(2, 8)) < dblTempWidth)
            {
                if (bZoomIn)
                {
                    for (int i = 0; i <= 8; i++)
                    {
                        if (dsWidth / Math.Pow(2, i) > dblTempWidth)
                        {
                            if (dsWidth / Math.Pow(2, i + 1) < dblTempWidth)
                                return map.Zoom * (dblTempWidth / (dsWidth / Math.Pow(2, i)));
                        }
                    }
                }
                else
                {
                    for (int i = 8; i >= 0; i--)
                    {
                        if (dsWidth / Math.Pow(2, i) < dblTempWidth)
                        {
                            if (dsWidth / Math.Pow(2, i - 1) > dblTempWidth)
                                return map.Zoom * (dblTempWidth / (dsWidth / Math.Pow(2, i)));
                        }
                    }
                }
            }


            return map.Zoom;
        }

        public void ResetHistoRectangle()
        {
            HistoBounds = new Rectangle((int)_envelope.MinX, (int)_envelope.MinY, 
                                         (int)_envelope.Width, (int)_envelope.Height);
        }

        // gets transform between raster's native projection and the map projection
        private void GetTransform(ICoordinateSystem mapProjection)
        {
            if (mapProjection == null || Projection == "")
            {
                CoordinateTransformation = null;
                return;
            }

            // get our two projections
            ICoordinateSystem srcCoord = GetProjection();
            ICoordinateSystem tgtCoord = mapProjection;

            // raster and map are in same projection, no need to transform
            if (srcCoord.WKT == tgtCoord.WKT)
            {
                CoordinateTransformation = null;
                return;
            }

            // create transform
            CoordinateTransformation = Session.Instance.CoordinateSystemServices.CreateTransformation(srcCoord, tgtCoord);
            ReverseCoordinateTransformation = Session.Instance.CoordinateSystemServices.CreateTransformation(tgtCoord, srcCoord);
        }
        // get boundary of raster
        private Envelope GetExtent()
        {
            if (_envelope != null)
                return _envelope;

            if (_gdalDataset != null)
            {
                var geoTrans = new double[6];
                _gdalDataset.GetGeoTransform(geoTrans);

                // no rotation...use default transform
                if (!_useRotation && !HaveSpot || (DoublesAreEqual(geoTrans[0], 0) && DoublesAreEqual(geoTrans[3] ,0)))
                    geoTrans = new[] { 999.5, 1, 0, 1000.5, 0, -1 };

                var geoTransform = new GeoTransform(geoTrans);

                // image pixels
                var dblW = (double)_imageSize.Width;
                var dblH = (double)_imageSize.Height;

                var left = geoTransform.EnvelopeLeft(dblW, dblH);
                var right = geoTransform.EnvelopeRight(dblW, dblH);
                var top = geoTransform.EnvelopeTop(dblW, dblH);
                var bottom = geoTransform.EnvelopeBottom(dblW, dblH);

                return new Envelope(left, right, bottom, top);
            }

            return null;
        }

        // get 4 corners of image
        public Coordinate[] GetFourCorners()
        {
            var points = new Coordinate[4];

            if (_gdalDataset != null)
            {
                var geoTrans = new double[6];
                _gdalDataset.GetGeoTransform(geoTrans);

                // no rotation...use default transform
                if (!_useRotation && !HaveSpot || (DoublesAreEqual(geoTrans[0],0) && DoublesAreEqual(geoTrans[3],0)))
                    geoTrans = new[] { 999.5, 1, 0, 1000.5, 0, -1 };

                points[0] = new Coordinate(geoTrans[0], geoTrans[3]);
                points[1] = new Coordinate(geoTrans[0] + (geoTrans[1] * _imageSize.Width),
                                         geoTrans[3] + (geoTrans[4] * _imageSize.Width));
                points[2] = new Coordinate(geoTrans[0] + (geoTrans[1] * _imageSize.Width) + (geoTrans[2] * _imageSize.Height),
                                         geoTrans[3] + (geoTrans[4] * _imageSize.Width) + (geoTrans[5] * _imageSize.Height));
                points[3] = new Coordinate(geoTrans[0] + (geoTrans[2] * _imageSize.Height),
                                         geoTrans[3] + (geoTrans[5] * _imageSize.Height));

                // transform to map's projection
                if (CoordinateTransformation != null)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        double[] dblPoint = CoordinateTransformation.MathTransform.Transform(new[] { points[i].X, points[i].Y });
                        points[i] = new Coordinate(dblPoint[0], dblPoint[1]);
                    }
                }
            }

            return points;
        }

        public Polygon GetFootprint()
        {
            var myRing = Factory.CreateLinearRing(GetFourCorners());
            return Factory.CreatePolygon(myRing, null);
        }

        // applies map projection transfrom to get reprojected envelope
        private void ApplyTransformToEnvelope()
        {
            double[] leftBottom, leftTop, rightTop, rightBottom;
            double left, right, bottom, top;

            _envelope = GetExtent();

            if (CoordinateTransformation == null)
                return;

            // set envelope
            _envelope = GeometryTransform.TransformBox(_envelope, CoordinateTransformation.MathTransform);

            // do same to histo rectangle
            leftBottom = new double[] { HistoBounds.Left, HistoBounds.Bottom };
            leftTop = new double[] { HistoBounds.Left, HistoBounds.Top };
            rightBottom = new double[] { HistoBounds.Right, HistoBounds.Bottom };
            rightTop = new double[] { HistoBounds.Right, HistoBounds.Top };

            // transform corners into new projection
            var mt = CoordinateTransformation.MathTransform;
            leftBottom = mt.Transform(leftBottom);
            leftTop = mt.Transform(leftTop);
            rightBottom = mt.Transform(rightBottom);
            rightTop = mt.Transform(rightTop);

            // find extents
            left = Math.Min(leftBottom[0], Math.Min(leftTop[0], Math.Min(rightBottom[0], rightTop[0])));
            right = Math.Max(leftBottom[0], Math.Max(leftTop[0], Math.Max(rightBottom[0], rightTop[0])));
            bottom = Math.Min(leftBottom[1], Math.Min(leftTop[1], Math.Min(rightBottom[1], rightTop[1])));
            top = Math.Max(leftBottom[1], Math.Max(leftTop[1], Math.Max(rightBottom[1], rightTop[1])));

            // set histo rectangle
            HistoBounds = new Rectangle((int)left, (int)bottom, (int)right, (int)top);
        }

        // public method to set envelope and transform to new projection
        public void ReprojectToCoordinateSystem(ICoordinateSystem cs)
        {
            GetTransform(cs);
            ApplyTransformToEnvelope();
        }

        // public method to set envelope and transform to new projection
        /// <summary>
        /// Method to set <see cref="Envelope"/> and <see cref="Layer.CoordinateTransformation"/> to the projection of the map
        /// </summary>
        /// <param name="map">The map</param>
        public void ReprojectToMap(Map map)
        {
            ICoordinateSystem cs = null;
            if (map.SRID > 0)
            {
                using (var p = new OSGeo.OSR.SpatialReference(null))
                {
                    string wkt;
                    p.ImportFromEPSG(map.SRID);
                    p.ExportToWkt(out wkt);
                    cs = map.GetCoordinateSystem();
                }
            }
            ReprojectToCoordinateSystem(cs);
        }

        // add image pixels to the map
        
        protected virtual void GetPreview(Dataset dataset, Size size, Graphics g,
                                          Envelope displayBbox, ICoordinateSystem mapProjection, MapViewport map)
        {
            DateTime drawStart = DateTime.Now;
            double totalReadDataTime = 0;
            double totalTimeSetPixel = 0;

            if (!NeedRotation(dataset))
            {
                GetNonRotatedPreview(dataset, size, g, displayBbox, mapProjection);
                return;
            }

            var geoTransform = new GeoTransform(dataset);
            
            Bitmap bitmap = null;
            var bitmapTl = new Point();
            var bitmapSize = new Size();
            Rectangle rect = new Rectangle();
            const int pixelSize = 3; //Format24bppRgb = byte[b,g,r] 

            if (dataset != null)
            {
                //check if image is in bounding box
                if ((displayBbox.MinX > _envelope.MaxX) || (displayBbox.MaxX < _envelope.MinX)
                    || (displayBbox.MaxY < _envelope.MinY) || (displayBbox.MinY > _envelope.MaxY))
                    return;

                // init histo
                var histogram = new List<int[]>();
                for (int i = 0; i < Bands + 1; i++)
                    histogram.Add(new int[256]);

                var trueImageBbox = displayBbox.Intersection(_envelope);

                // put display bounds into current projection
                Envelope shownImageBbox = trueImageBbox.Copy();
                if (ReverseCoordinateTransformation != null)
                {
                    shownImageBbox = GeometryTransform.TransformBox(trueImageBbox, ReverseCoordinateTransformation.MathTransform);
                }

                // find min/max x and y pixels needed from image
                var g2I = geoTransform.GroundToImage(shownImageBbox).Intersection(new Envelope(0, _imageSize.Width, 0, _imageSize.Height));
                var gdalImageRect = ToRectangle(g2I);
                var displayImageSize = gdalImageRect.Size;

                // convert ground coordinates to map coordinates to figure out where to place the bitmap
                var pos1 = Point.Truncate(map.WorldToImage(trueImageBbox.TopLeft()));
                var pos2 = Point.Truncate(map.WorldToImage(trueImageBbox.BottomRight()));
                rect = Rectangle.FromLTRB(pos1.X, pos1.Y, pos2.X, pos2.Y);

                bitmapTl = rect.Location;
                bitmapSize = Size.Add(rect.Size, new Size(1,1));

                double disRatio = (double) displayImageSize.Width / displayImageSize.Height;
                double bmsRatio = (double) bitmapSize.Width / bitmapSize.Height;

                // check to see if image is on its side
                if (bitmapSize.Width > bitmapSize.Height && displayImageSize.Width < displayImageSize.Height)
                {
                    displayImageSize.Width = bitmapSize.Height;
                    displayImageSize.Height = bitmapSize.Width;
                }
                else
                {
                    displayImageSize.Width = bitmapSize.Width;
                    displayImageSize.Height = bitmapSize.Height;
                    if (Math.Abs(disRatio - bmsRatio) > 1e-5)
                        displayImageSize.Height = (int) (bmsRatio * displayImageSize.Height);
                }

                /*
                // scale
                var bitScalar = GetBitScalar();
                */

                // 0 pixels in length or height, nothing to display
                if (bitmapSize.Width < 1 || bitmapSize.Height < 1)
                    return;

                //initialize bitmap
                BitmapData bitmapData;
                bitmap = InitializeBitmap(bitmapSize, PixelFormat.Format24bppRgb, out bitmapData);
                
                try
                {
                    unsafe
                    {
                        byte cr = _noDataInitColor.R;
                        byte cg = _noDataInitColor.G;
                        byte cb = _noDataInitColor.B;

                        // create 3 dimensional buffer [band][x pixel][y pixel]
                        var buffer = new double[Bands][][];
                        for (int i = 0; i < Bands; i++)
                        {
                            buffer[i] = new double[displayImageSize.Width][];
                            for (int j = 0; j < displayImageSize.Width; j++)
                                buffer[i][j] = new double[displayImageSize.Height];
                        }

                        var ch = new int[Bands];
                        var bitScales = new double[Bands];

                        var noDataValues = new Double[Bands];
                        var scales = new Double[Bands];
                        ColorTable colorTable = null;
                        short[][] colorTableCache = null;

                        var imageRect = gdalImageRect;

                        ColorBlend colorBlend = null;
                        var intermediateValue = new double[Bands];

                        // get data from image
                        for (int i = 0; i < Bands; i++)
                        {
                            using (var band = dataset.GetRasterBand(i + 1))
                            {
                                //get nodata value if present
                                band.GetNoDataValue(out noDataValues[i], out int hasVal);
                                if (hasVal == 0) noDataValues[i] = double.NaN;
                                
                                //Get the scale value if present
                                band.GetScale(out scales[i], out hasVal);
                                if (hasVal == 0) scales[i] = 1.0;

                                //
                                bitScales[i] = GetBitScale(band.DataType);
                                switch (band.GetRasterColorInterpretation())
                                {
                                    case ColorInterp.GCI_BlueBand:
                                        ch[i] = 0;
                                        break;
                                    case ColorInterp.GCI_GreenBand:
                                        ch[i] = 1;
                                        break;
                                    case ColorInterp.GCI_RedBand:
                                        ch[i] = 2;
                                        break;
                                    case ColorInterp.GCI_Undefined:
                                        if (Bands > 1)
                                            ch[i] = 3; // infrared
                                        else
                                        {
                                            ch[i] = 4;
                                            colorBlend = GetColorBlend(band);
                                            intermediateValue = new Double[3];
                                        }
                                        break;
                                    case ColorInterp.GCI_GrayIndex:
                                        ch[i] = 0;
                                        break;
                                    case ColorInterp.GCI_PaletteIndex:
                                        if (colorTable != null)
                                        {
                                            //this should not happen
                                            colorTable.Dispose();
                                        }
                                        colorTable = band.GetRasterColorTable();

                                        int numColors = colorTable.GetCount();
                                        colorTableCache = new short[numColors][];
                                        for (int col = 0; col < numColors; col++)
                                        {
                                            using (var colEntry = colorTable.GetColorEntry(col))
                                            {
                                                colorTableCache[col] = new [] {
                                                    colEntry.c1, colEntry.c2, colEntry.c3
                                                };
                                            }
                                        }

                                        ch[i] = 5;
                                        intermediateValue = new Double[3];
                                        break;
                                    default:
                                        ch[i] = -1;
                                        break;
                                }
                            }
                        }

                        // store these values to keep from having to make slow method calls
                        double imageTop = g2I.MinY;
                        double imageLeft = g2I.MinX;


                        // get inverse values
                        var geoTop = geoTransform.Inverse[3];
                        var geoLeft = geoTransform.Inverse[0];
                        var geoHorzPixRes = geoTransform.Inverse[1];
                        var geoVertPixRes = geoTransform.Inverse[5];
                        var geoXRot = geoTransform.Inverse[2];
                        var geoYRot = geoTransform.Inverse[4];

                        double dblXScale = (displayImageSize.Width - 1) / (g2I.Width);
                        double dblYScale = (displayImageSize.Height - 1) / (g2I.Height);

                        // get inverse transform  
                        // NOTE: calling transform.MathTransform.Inverse() once and storing it
                        // is much faster than having to call every time it is needed
                        IMathTransform inverseTransform = null;
                        if (ReverseCoordinateTransformation != null)
                            inverseTransform = ReverseCoordinateTransformation.MathTransform;

                        int rowsRead = 0;
                        int displayImageStep = displayImageSize.Height;
                        while (rowsRead < displayImageStep)
                        {
                            int rowsToRead = displayImageStep;
                            if (rowsRead + rowsToRead > displayImageStep)
                                rowsToRead = displayImageStep - rowsRead;

                            var tempBuffer = new double[displayImageSize.Width * rowsToRead];
                            for (int i = 0; i < Bands; i++)
                            {
                                // read the buffer
                                using (var band = dataset.GetRasterBand(i + 1))
                                {
                                    DateTime start = DateTime.Now;
                                    band.ReadRaster(imageRect.Left, imageRect.Top,
                                                    imageRect.Width, imageRect.Height,
                                                    tempBuffer, displayImageSize.Width, rowsToRead, 0, 0);

                                    if (_logger.IsDebugEnabled)
                                    {
                                        TimeSpan took = DateTime.Now - start;
                                        totalReadDataTime += took.TotalMilliseconds;
                                    }
                                }

                                // parse temp buffer into the image x y value buffer
                                long pos = 0;
                                int newRowsRead = rowsRead + rowsToRead;
                                for (int y = rowsRead; y < newRowsRead; y++)
                                {
                                    for (int x = 0; x < displayImageSize.Width; x++)
                                    {
                                        buffer[i][x][y] = tempBuffer[pos++];
                                    }
                                }
                            }
                            rowsRead += rowsToRead;

                            double mapPixelWidth = map.PixelWidth;
                            double mapPixelHeight = map.PixelHeight;

                            for (int pixY = 0; pixY < bitmapData.Height; pixY++)
                            {
                                var rowPtr =  bitmapData.Scan0 + pixY * bitmapData.Stride;
                                var row = (byte*) rowPtr;

                                double gndY = Envelope.MaxY - pixY * mapPixelHeight;
                                //if (gndY > map.Envelope.MaxY) continue;
                                double gndX = Envelope.MinX;

                                for (int pixX = 0; pixX < bitmap.Width; pixX++)
                                {

                                    // transform ground point if needed
                                    if (inverseTransform != null)
                                    {
                                        var dblPoint = inverseTransform.Transform(new[] {gndX, gndY});
                                        gndX = dblPoint[0];
                                        gndY = dblPoint[1];
                                    }

                                    // same as GeoTransform.GroundToImage(), but much faster using stored values...
                                    var imageCoord = new Coordinate(
                                        geoLeft + geoHorzPixRes*gndX + geoXRot*gndY,
                                        geoTop + geoYRot*gndX + geoVertPixRes*gndY);

                                    if (!g2I.Contains(imageCoord)) 
                                        goto Incrementation;

                                    var imagePt = new Point((int)((imageCoord.X - imageLeft) * dblXScale),
                                                            (int)((imageCoord.Y - imageTop) * dblYScale));

                                    // Apply color correction
                                    for (var i = 0; i < Bands; i++)
                                    {
                                        intermediateValue[i] = buffer[i][imagePt.X][imagePt.Y];

                                        // apply scale
                                        intermediateValue[i] *= scales[i];

                                        double spotVal;
                                        var imageVal = spotVal = intermediateValue[i] = intermediateValue[i] * bitScales[i];

                                        if (ch[i] == 4)
                                        {
                                            if (!DoublesAreEqual(imageVal,noDataValues[i]))
                                            {
                                                var color = colorBlend.GetColor(Convert.ToSingle(imageVal));
                                                intermediateValue[0] = color.B;
                                                intermediateValue[1] = color.G;
                                                intermediateValue[2] = color.R;
                                                //intVal[3] = ce.c4;
                                            }
                                            else
                                            {
                                                intermediateValue[0] = cb;
                                                intermediateValue[1] = cg;
                                                intermediateValue[2] = cr;
                                            }
                                        }

                                        else if (ch[i] == 5 && colorTable != null)
                                        {
                                            if (double.IsNaN(noDataValues[i]) || !DoublesAreEqual(imageVal, noDataValues[i]))
                                            {
                                                var ce = colorTableCache[Convert.ToInt32(imageVal)];

                                                intermediateValue[0] = ce[2];
                                                intermediateValue[1] = ce[1];
                                                intermediateValue[2] = ce[0];
                                            }
                                            else
                                            {
                                                intermediateValue[0] = cb;
                                                intermediateValue[1] = cg;
                                                intermediateValue[2] = cr;
                                            }
                                        }

                                        else
                                        {
                                            if (ColorCorrect)
                                            {
                                                intermediateValue[i] = ApplyColorCorrection(imageVal, spotVal, ch[i],
                                                                                            gndX,
                                                                                            gndY);

                                                // if pixel is within ground boundary, add its value to the histogram
                                                if (ch[i] != -1 && intermediateValue[i] > 0 &&
                                                    (HistoBounds.Bottom >= (int) gndY) &&
                                                    HistoBounds.Top <= (int) gndY &&
                                                    HistoBounds.Left <= (int) gndX && HistoBounds.Right >= (int) gndX)
                                                {
                                                    histogram[ch[i]][(int) intermediateValue[i]]++;
                                                }
                                            }

                                            if (intermediateValue[i] > 255)
                                                intermediateValue[i] = 255;
                                        }
                                    }

                                    // luminosity
                                    if (Bands >= 3)
                                        histogram[Bands][
                                            (int)
                                            (intermediateValue[2]*0.2126 + intermediateValue[1]*0.7152 +
                                             intermediateValue[0]*0.0722)]
                                            ++;

                                    DateTime writeStart = DateTime.MinValue;
                                    if (_logger.IsDebugEnabled)
                                        writeStart = DateTime.Now;
                                    WritePixel(pixX, intermediateValue, pixelSize, ch, row);
                                    if (_logger.IsDebugEnabled)
                                    {
                                        TimeSpan took = DateTime.Now - writeStart;
                                        totalTimeSetPixel += took.TotalMilliseconds;
                                    }

                                    Incrementation:
                                    gndX += mapPixelWidth;
                                }
                            }
                        }

                        if (colorTable != null)
                        {
                            colorTable.Dispose();
                        }
                    }
                }

                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                // Update the histogram
                Histogram = histogram;
            }


            DateTime drawGdiStart = DateTime.Now;
            bitmap.MakeTransparent(_noDataInitColor);
            if (TransparentColor != Color.Empty)
                bitmap.MakeTransparent(TransparentColor);

            g.DrawImage(bitmap, bitmapTl);
            using (var p = new Pen(new SolidBrush(Color.Crimson), 4f))
                g.DrawRectangle(p, rect);

            if (_logger.IsDebugEnabled)
            {
                TimeSpan took = DateTime.Now - drawStart;
                TimeSpan drawGdiTook = DateTime.Now - drawGdiStart;
                _logger.DebugFormat("Draw GdalImage in {0}ms, readTime: {1}, setPixelTime: {2}, drawTime: {3}", took.TotalMilliseconds, totalReadDataTime,
                    totalTimeSetPixel, drawGdiTook.TotalMilliseconds);
            }
        }

 
        #region private helper methods
        private Rectangle ToRectangle(Envelope g2I)
        {
            var left = Convert.ToInt32(g2I.MinX);
            var top = Convert.ToInt32(g2I.MinY);
            var right = Convert.ToInt32(g2I.MaxX) + 1;
            var bottom = Convert.ToInt32(g2I.MaxY) + 1;

            //Stay within the bounds of the image
            if (left < 0) left = 0;
            if (top < 0) top = 0;
            if (right > _imageSize.Width) right = _imageSize.Width;
            if (bottom > _imageSize.Height) bottom = _imageSize.Height;

            return new Rectangle(new Point(left, top),
                                 new Size(right - left, bottom - top));

            /*
            var imageBR = g2i.Max();
            imageBR.X += 1;
            imageBR.Y += 1;
            var imageTL = g2i.Min();

            // stay within image
            if (imageBR.X > _imageSize.Width)
                imageBR.X = _imageSize.Width;
            if (imageBR.Y > _imageSize.Height)
                imageBR.Y = _imageSize.Height;
            if (imageTL.Y < 0)
                imageTL.Y = 0;
            if (imageTL.X < 0)
                imageTL.X = 0;

            return new Rectangle(new point);
             */
        }

        private bool NeedRotation(Dataset dataset)
        {
            if (UseRotation)
                return true;
            if (CoordinateTransformation != null)
                return true;
            if (HaveSpot)
                return true;

            var geoTransform = new GeoTransform(dataset);

            return geoTransform.IsScaling || geoTransform.IsRotating;

        }

        private ColorBlend GetColorBlend(Band band)
        {
            if (_colorBlend != null)
                return _colorBlend;

            int hasVal;

            //Get minimum raster value
            double dblMin;
            band.GetMinimum(out dblMin, out hasVal);
            if (hasVal == 0) dblMin = Double.NaN;

            //Get maximum raster value
            double dblMax;
            band.GetMaximum(out dblMax, out hasVal);
            if (hasVal == 0) dblMax = double.NaN;

            if (double.IsNaN(dblMin) && double.IsNaN(dblMax))
            {

                double dblMean, dblStdDev;
                band.GetStatistics(0, 1, out dblMin, out dblMax, out dblMean,
                                       out dblStdDev);
            }
            // ToDo: Colorblend positions
            var minmax = new[]
                {
                    Convert.ToSingle(dblMin),
                    0.5f*Convert.ToSingle(dblMin + dblMax),
                    Convert.ToSingle(dblMax)
                };

            _colorBlend = new ColorBlend(new[] { Color.Blue, Color.Yellow, Color.Red }, minmax);
            
            return _colorBlend;

        }

        private Bitmap InitializeBitmap(Size bitmapSize, PixelFormat pixelFormat, out BitmapData bitmapData)
        {
            var res = new Bitmap(bitmapSize.Width, bitmapSize.Height, pixelFormat);
            using (var g = Graphics.FromImage(res))
            {
                g.Clear(NoDataInitColor);
            }

            bitmapData = res.LockBits(new Rectangle(new Point(0, 0), bitmapSize), 
                                      ImageLockMode.WriteOnly, pixelFormat);
            return res;
        }
        #endregion

        // faster than rotated display
        private void GetNonRotatedPreview(Dataset dataset, Size size, Graphics g,
                                          Envelope bbox, ICoordinateSystem mapProjection)
        {
            var geoTrans = new double[6];
            dataset.GetGeoTransform(geoTrans);

            // default transform
            if (!_useRotation && !HaveSpot || (DoublesAreEqual(geoTrans[0],0) && DoublesAreEqual(geoTrans[3],0)))
                geoTrans = new[] { 999.5, 1, 0, 1000.5, 0, -1 };
            Bitmap bitmap = null;
            var geoTransform = new GeoTransform(geoTrans);
            BitmapData bitmapData = null;
            var intVal = new double[Bands];

            double dblLocX = 0, dblLocY = 0;

            const int iPixelSize = 3; //Format24bppRgb = byte[b,g,r] 

            if (dataset != null)
            {
                //check if image is in bounding box
                if ((bbox.MinX > _envelope.MaxX) || (bbox.MaxX < _envelope.MinX)
                    || (bbox.MaxY < _envelope.MinY) || (bbox.MinY > _envelope.MaxY))
                    return;

                Histogram = new List<int[]>();
                for (int i = 0; i < Bands + 1; i++)
                    Histogram.Add(new int[256]);

                double left = Math.Max(bbox.MinX, _envelope.MinX);
                double top = Math.Min(bbox.MaxY, _envelope.MaxY);
                double right = Math.Min(bbox.MaxX, _envelope.MaxX);
                double bottom = Math.Max(bbox.MinY, _envelope.MinY);

                double x1 = Math.Abs(geoTransform.PixelX(left));
                double y1 = Math.Abs(geoTransform.PixelY(top));
                double imgPixWidth = geoTransform.PixelXwidth(right - left);
                double imgPixHeight = geoTransform.PixelYwidth(bottom - top);

                //get screen pixels image should fill 
                double dblBBoxW = bbox.Width;
                double dblBBoxtoImgPixX = imgPixWidth / dblBBoxW;
                double dblImginMapW = size.Width * dblBBoxtoImgPixX * geoTransform.HorizontalPixelResolution;


                double dblBBoxH = bbox.Height;
                double dblBBoxtoImgPixY = imgPixHeight / dblBBoxH;
                double dblImginMapH = size.Height * dblBBoxtoImgPixY * -geoTransform.VerticalPixelResolution;

                if ((DoublesAreEqual(dblImginMapH, 0)) || (DoublesAreEqual(dblImginMapW,0)))
                    return;

                // ratios of bounding box to image ground space
                double dblBBoxtoImgX = size.Width / dblBBoxW;
                double dblBBoxtoImgY = size.Height / dblBBoxH;

                // set where to display bitmap in Map
                if (!DoublesAreEqual(bbox.MinX,left))
                {
                    if (!DoublesAreEqual(bbox.MaxX,right))
                        dblLocX = (_envelope.MinX - bbox.MinX) * dblBBoxtoImgX;
                    else
                        dblLocX = size.Width - dblImginMapW;
                }
                if (!DoublesAreEqual(bbox.MaxY,top))
                {
                    if (!DoublesAreEqual(bbox.MinY,bottom))
                        dblLocY = (bbox.MaxY - _envelope.MaxY) * dblBBoxtoImgY;
                    else
                        dblLocY = size.Height - dblImginMapH;
                }

                //double bitScalar = GetBitScalar();

                try
                {
                    bitmap = new Bitmap((int)Math.Round(dblImginMapW), (int)Math.Round(dblImginMapH),
                                        PixelFormat.Format24bppRgb);
                    bitmapData =
                        bitmap.LockBits(
                            new Rectangle(0, 0, (int)Math.Round(dblImginMapW), (int)Math.Round(dblImginMapH)),
                            ImageLockMode.ReadWrite, bitmap.PixelFormat);

                    byte cr = _noDataInitColor.R;
                    byte cg = _noDataInitColor.G;
                    byte cb = _noDataInitColor.B;

                    //
                    var noDataValues = new double[Bands];
                    var scales = new double[Bands];

                    ColorTable colorTable = null;
                    unsafe
                    {
                        var buffer = new double[Bands][];
                        var band = new Band[Bands];
                        var ch = new int[Bands];
                        var bitScales = new double[Bands];

                        // get data from image
                        for (int i = 0; i < Bands; i++)
                        {
                            buffer[i] = new double[(int)Math.Round(dblImginMapW) * (int)Math.Round(dblImginMapH)];
                            band[i] = dataset.GetRasterBand(i + 1);

                            //get nodata value if present
                            Int32 hasVal;
                            band[i].GetNoDataValue(out noDataValues[i], out hasVal);
                            if (hasVal == 0) noDataValues[i] = Double.NaN;
                            band[i].GetScale(out scales[i], out hasVal);
                            if (hasVal == 0) scales[i] = 1.0;

                            bitScales[i] = GetBitScale(band[i].DataType);

                            band[i].ReadRaster((int)Math.Round(x1), (int)Math.Round(y1), (int)Math.Round(imgPixWidth),
                                               (int)Math.Round(imgPixHeight),
                                               buffer[i], (int)Math.Round(dblImginMapW), (int)Math.Round(dblImginMapH),
                                               0, 0);

                            switch (band[i].GetRasterColorInterpretation())
                            {
                                case ColorInterp.GCI_BlueBand:
                                    ch[i] = 0;
                                    break;
                                case ColorInterp.GCI_GreenBand:
                                    ch[i] = 1;
                                    break;
                                case ColorInterp.GCI_RedBand:
                                    ch[i] = 2;
                                    break;
                                case ColorInterp.GCI_Undefined:
                                    if (Bands > 1)
                                        ch[i] = 3; // infrared
                                    else
                                    {
                                        ch[i] = 4;
                                        if (_colorBlend == null)
                                        {
                                            Double dblMin, dblMax;
                                            band[i].GetMinimum(out dblMin, out hasVal);
                                            if (hasVal == 0) dblMin = Double.NaN;
                                            band[i].GetMaximum(out dblMax, out hasVal);
                                            if (hasVal == 0) dblMax = double.NaN;
                                            if (Double.IsNaN(dblMin) || Double.IsNaN(dblMax))
                                            {
                                                double dblMean, dblStdDev;
                                                band[i].GetStatistics(0, 1, out dblMin, out dblMax, out dblMean, out dblStdDev);
                                                //double dblRange = dblMax - dblMin;
                                                //dblMin -= 0.1*dblRange;
                                                //dblMax += 0.1*dblRange;
                                            }
                                            var minmax = new float[] { Convert.ToSingle(dblMin), 0.5f * Convert.ToSingle(dblMin + dblMax), Convert.ToSingle(dblMax) };
                                            var colors = new Color[] { Color.Blue, Color.Yellow, Color.Red };
                                            _colorBlend = new ColorBlend(colors, minmax);
                                        }
                                        intVal = new Double[3];
                                    }
                                    break;
                                case ColorInterp.GCI_GrayIndex:
                                    ch[i] = 0;
                                    break;
                                case ColorInterp.GCI_PaletteIndex:
                                    colorTable = band[i].GetRasterColorTable();
                                    ch[i] = 5;
                                    intVal = new Double[3];
                                    break;
                                default:
                                    ch[i] = -1;
                                    break;
                            }
                        }

                        if (BitDepth == 32)
                            ch = new[] { 0, 1, 2 };

                        int pIndx = 0;
                        for (int y = 0; y < Math.Round(dblImginMapH); y++)
                        {
                            byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                            for (int x = 0; x < Math.Round(dblImginMapW); x++, pIndx++)
                            {
                                for (int i = 0; i < Bands; i++)
                                {
                                    var imageVal = intVal[i] = buffer[i][pIndx]*bitScales[i];
                                    //Double imageVal = intVal[i] = intVal[i]*bitScales[i];
                                    if (ch[i] == 4)
                                    {
                                        if (!DoublesAreEqual(imageVal, noDataValues[i]))
                                        {
                                            Color color = _colorBlend.GetColor(Convert.ToSingle(imageVal));
                                            intVal[0] = color.B;
                                            intVal[1] = color.G;
                                            intVal[2] = color.R;
                                            //intVal[3] = ce.c4;
                                        }
                                        else
                                        {
                                            intVal[0] = cb;
                                            intVal[1] = cg;
                                            intVal[2] = cr;
                                        }
                                    }

                                    else if (ch[i] == 5 && colorTable != null)
                                    {
                                        if (!DoublesAreEqual(imageVal,noDataValues[i]))
                                        {
                                            ColorEntry ce = colorTable.GetColorEntry(Convert.ToInt32(imageVal));
                                            intVal[0] = ce.c3;
                                            intVal[1] = ce.c2;
                                            intVal[2] = ce.c1;
                                            //intVal[3] = ce.c4;
                                        }
                                        else
                                        {
                                            intVal[0] = cb;
                                            intVal[1] = cg;
                                            intVal[2] = cr;
                                        }
                                    }
                                    else
                                    {
                                        if (ColorCorrect)
                                        {
                                            intVal[i] = ApplyColorCorrection(intVal[i], 0, ch[i], 0, 0);

                                            if (Bands >= 3)
                                                Histogram[Bands][
                                                    (int) (intVal[2]*0.2126 + intVal[1]*0.7152 + intVal[0]*0.0722)]++;
                                        }
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
            if (TransparentColor != Color.Empty)
                bitmap.MakeTransparent(TransparentColor);
            g.DrawImage(bitmap, new Point((int)Math.Round(dblLocX), (int)Math.Round(dblLocY)));
        }

        /// <summary>
        /// Method to get a scalar by which to scale raster values
        /// </summary>
        /// <returns>A scale value dependant on <see cref="BitDepth"/></returns>
        private double GetBitScalar()
        {
            switch (BitDepth)
            {
                case 12:
                    return 16.0;
                case 16:
                    return 256.0;
                case 32:
                    return 16777216.0;
            }
            return 1;
        }

        private static double GetBitScale(DataType dataType)
        {
            switch (dataType)
            {
                //case DataType.GDT_Byte:
                //    return 1d;
                case DataType.GDT_UInt16:
                case DataType.GDT_Int16:
                    return 1d/256d;
                case DataType.GDT_Int32:
                case DataType.GDT_UInt32:
                    return 1d/16777216d;
            }
            return 1d;
        }
        protected unsafe void WritePixel(double x, double[] intVal, int iPixelSize, int[] ch, byte* row)
        {
            // write out pixels
            // black and white
            Int32 offsetX = (int)Math.Round(x) * iPixelSize;
            if (Bands == 1 && BitDepth != 32)
            {
                if (ch[0] < 4)
                {
                    if (ShowClip)
                    {
                        if (DoublesAreEqual(intVal[0] ,0))
                        {
                            row[offsetX++] = 255;
                            row[offsetX++] = 0;
                            row[offsetX] = 0;
                        }
                        else if (DoublesAreEqual(intVal[0],255))
                        {
                            row[offsetX++] = 0;
                            row[offsetX++] = 0;
                            row[offsetX] = 255;
                        }
                        else
                        {
                            row[offsetX++] = (byte)intVal[0];
                            row[offsetX++] = (byte)intVal[0];
                            row[offsetX] = (byte)intVal[0];
                        }
                    }
                    else
                    {
                        row[offsetX++] = (byte)intVal[0];
                        row[offsetX++] = (byte)intVal[0];
                        row[offsetX] = (byte)intVal[0];
                    }
                }
                else
                {
                    row[offsetX++] = (byte)intVal[0];
                    row[offsetX++] = (byte)intVal[1];
                    row[offsetX] = (byte)intVal[2];
                }
            }
            // IR grayscale
            else if (DisplayIR && Bands == 4)
            {
                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] == 3)
                    {
                        if (ShowClip)
                        {
                            if (DoublesAreEqual(intVal[3],0))
                            {
                                row[(int)Math.Round(x) * iPixelSize] = 255;
                                row[(int)Math.Round(x) * iPixelSize + 1] = 0;
                                row[(int)Math.Round(x) * iPixelSize + 2] = 0;
                            }
                            else if (DoublesAreEqual(intVal[3],255))
                            {
                                row[(int)Math.Round(x) * iPixelSize] = 0;
                                row[(int)Math.Round(x) * iPixelSize + 1] = 0;
                                row[(int)Math.Round(x) * iPixelSize + 2] = 255;
                            }
                            else
                            {
                                row[(int)Math.Round(x) * iPixelSize] = (byte)intVal[i];
                                row[(int)Math.Round(x) * iPixelSize + 1] = (byte)intVal[i];
                                row[(int)Math.Round(x) * iPixelSize + 2] = (byte)intVal[i];
                            }
                        }
                        else
                        {
                            row[(int)Math.Round(x) * iPixelSize] = (byte)intVal[i];
                            row[(int)Math.Round(x) * iPixelSize + 1] = (byte)intVal[i];
                            row[(int)Math.Round(x) * iPixelSize + 2] = (byte)intVal[i];
                        }
                    }
                }
            }
            // CIR
            else if (DisplayCIR && Bands == 4)
            {
                if (ShowClip)
                {
                    if (DoublesAreEqual(intVal[0], 0) && DoublesAreEqual(intVal[1], 0) && DoublesAreEqual(intVal[3], 0))
                    {
                        intVal[3] = intVal[0] = 0;
                        intVal[1] = 255;
                    }
                    else if (DoublesAreEqual(intVal[0], 255) && DoublesAreEqual(intVal[1], 255) && DoublesAreEqual(intVal[3], 255))
                        intVal[1] = intVal[0] = 0;
                }

                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] != 0 && ch[i] != -1)
                        row[(int)Math.Round(x) * iPixelSize + ch[i] - 1] = (byte)intVal[i];
                }
            }
            // RGB
            else
            {
                {
                if (ShowClip)
                    if (DoublesAreEqual(intVal[0], 0) && DoublesAreEqual(intVal[1], 0) && DoublesAreEqual(intVal[2], 0))
                    {
                        intVal[0] = intVal[1] = 0;
                        intVal[2] = 255;
                    }
                    else if (DoublesAreEqual(intVal[0], 255) && DoublesAreEqual(intVal[1], 255) && DoublesAreEqual(intVal[2], 255))
                        intVal[1] = intVal[2] = 0;
                }

                for (int i = 0; i < 3; i++)
                {
                    if (ch[i] != 3 && ch[i] != -1)
                        row[(int)Math.Round(x) * iPixelSize + ch[i]] = (byte)intVal[i];
                }
            }
        }

        // apply any color correction to pixel
        private double ApplyColorCorrection(double imageVal, double spotVal, int channel, double gndX, double gndY)
        {
            double finalVal;

            finalVal = imageVal;

            if (HaveSpot)
            {
                // gamma
                if (!DoublesAreEqual(NonSpotGamma,1))
                    imageVal = 256 * Math.Pow((imageVal / 256), NonSpotGamma);

                // gain
                if (channel == 2)
                    imageVal = imageVal * NonSpotGain[0];
                else if (channel == 1)
                    imageVal = imageVal * NonSpotGain[1];
                else if (channel == 0)
                    imageVal = imageVal * NonSpotGain[2];
                else if (channel == 3)
                    imageVal = imageVal * NonSpotGain[3];

                if (imageVal > 255)
                    imageVal = 255;

                // curve
                if (NonSpotCurveLut != null)
                    if (NonSpotCurveLut.Count != 0)
                    {
                        if (channel == 2 || channel == 4)
                            imageVal = NonSpotCurveLut[0][(int)imageVal];
                        else if (channel == 1)
                            imageVal = NonSpotCurveLut[1][(int)imageVal];
                        else if (channel == 0)
                            imageVal = NonSpotCurveLut[2][(int)imageVal];
                        else if (channel == 3)
                            imageVal = NonSpotCurveLut[3][(int)imageVal];
                    }

                finalVal = imageVal;

                double distance = Math.Sqrt(Math.Pow(gndX - SpotPoint.X, 2) + Math.Pow(gndY - SpotPoint.Y, 2));

                if (distance <= InnerSpotRadius + OuterSpotRadius)
                {
                    // gamma
                    if (!DoublesAreEqual(SpotGamma,1))
                        spotVal = 256 * Math.Pow((spotVal / 256), SpotGamma);

                    // gain
                    if (channel == 2)
                        spotVal = spotVal * SpotGain[0];
                    else if (channel == 1)
                        spotVal = spotVal * SpotGain[1];
                    else if (channel == 0)
                        spotVal = spotVal * SpotGain[2];
                    else if (channel == 3)
                        spotVal = spotVal * SpotGain[3];

                    if (spotVal > 255)
                        spotVal = 255;

                    // curve
                    if (SpotCurveLut != null)
                        if (SpotCurveLut.Count != 0)
                        {
                            if (channel == 2 || channel == 4)
                                spotVal = SpotCurveLut[0][(int)spotVal];
                            else if (channel == 1)
                                spotVal = SpotCurveLut[1][(int)spotVal];
                            else if (channel == 0)
                                spotVal = SpotCurveLut[2][(int)spotVal];
                            else if (channel == 3)
                                spotVal = SpotCurveLut[3][(int)spotVal];
                        }

                    if (distance < InnerSpotRadius)
                        finalVal = spotVal;
                    else
                    {
                        double imagePct = (distance - InnerSpotRadius) / OuterSpotRadius;
                        double spotPct = 1 - imagePct;

                        finalVal = (Math.Round((spotVal * spotPct) + (imageVal * imagePct)));
                    }
                }
            }

            // gamma
            if (!DoublesAreEqual(Gamma, 1))
                finalVal = (256*Math.Pow((finalVal/256), Gamma));


            switch (channel)
            {
                case 2:
                    finalVal = finalVal * Gain[0];
                    break;
                case 1:
                    finalVal = finalVal * Gain[1];
                    break;
                case 0:
                    finalVal = finalVal * Gain[2];
                    break;
                case 3:
                    finalVal = finalVal * Gain[3];
                    break;
            }

            if (finalVal > 255)
                finalVal = 255;

            // curve
            if (CurveLut != null)
                if (CurveLut.Count != 0)
                {
                    if (channel == 2 || channel == 4)
                        finalVal = CurveLut[0][(int)finalVal];
                    else if (channel == 1)
                        finalVal = CurveLut[1][(int)finalVal];
                    else if (channel == 0)
                        finalVal = CurveLut[2][(int)finalVal];
                    else if (channel == 3)
                        finalVal = CurveLut[3][(int)finalVal];
                }

            return finalVal;
        }
        /*
        /// <summary>
        /// Build histogram and statistics
        /// </summary>
        /// <param name="bQuick">If true, build histogram off of smaller subsample of image</param>
        public void BuildHisto(bool bQuick)
        {
            Dataset dataset = _gdalDataset;
            int height, width, Bands;
            int p_indx = 0;
            int intVal;
            double[] stdDev = new double[4];
            int maxVal;

            if (bQuick)
            {
                height = 20;
                width = (int) (20*(dataset.RasterXSize/(double) dataset.RasterYSize));
            }
            else
            {
                height = 3000; // dataset.RasterYSize;
                width = (int) (3000*(dataset.RasterXSize/(double) dataset.RasterYSize));
                // dataset.RasterXSize;
            }

            Bands = dataset.RasterCount;

            _histogram = new List<int[]>();
            _histoMean = new double[Bands];

            for (int band = 1; band <= Bands; band++)
            {
                List<object> lstObj = new List<object>();

                if (_bitDepth == 8)
                    _histogram.Add(new int[256]);
                else if (_bitDepth == 12)
                    _histogram.Add(new int[4096]);
                else
                    _histogram.Add(new int[65536]);

                for (int i = 0; i < _histogram[band - 1].Length; i++)
                    _histogram[band - 1][i] = 0;

                maxVal = _histogram[0].Length - 1;

                Band RBand = dataset.GetRasterBand(band);
                double[] buffer = new double[width*height];
                RBand.ReadRaster(0, 0, dataset.RasterXSize, dataset.RasterYSize, buffer, width, height, 0, 0);

                p_indx = 0;

                _histoMean[band - 1] = 0;

                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++, p_indx++)
                    {
                        intVal = (int) buffer[p_indx];

                        // gamma
                        if (_nonSpotGamma != 1)
                            intVal = (int) (256*Math.Pow(((double) intVal/256), _nonSpotGamma));

                        // gain
                        intVal = (int) (intVal*_gain[band - 1]);

                        if (intVal > maxVal)
                            intVal = maxVal;

                        // curves
                        if (_nonSpotCurveLut != null)
                            if (_nonSpotCurveLut.Count != 0)
                                intVal = _nonSpotCurveLut[band - 1][intVal];

                        buffer[p_indx] = (byte) intVal;

                        _histogram[band - 1][intVal]++;
                        _histoMean[band - 1] += intVal;
                    }
                }
                _histoMean[band - 1] /= buffer.Length;
                stdDev[band - 1] = CalcStandardDeviation(buffer, _histoMean[band - 1]);
            }

            // set brightness and contrast
            if (Bands > 1)
            {
                _histoBrightness = (_histoMean[0]*0.2126 + _histoMean[1]*0.7152 + _histoMean[2]*0.0722)/2.55;
                _histoContrast = (stdDev[0]*0.2126 + stdDev[1]*0.7152 + stdDev[2]*0.0722)/1.28;
            }
            else
            {
                _histoBrightness = _histoMean[0]/2.55;
                _histoContrast = stdDev[0]/1.28;
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
        */

        /// <summary>
        /// Find min and max pixel values of the image
        /// </summary>
        private void ComputeStretch()
        {
            double min = 99999999, max = -99999999;
            int width, height;

            if (_gdalDataset.RasterYSize < 4000)
            {
                height = _gdalDataset.RasterYSize;
                width = _gdalDataset.RasterXSize;
            }
            else
            {
                height = 4000;
                width = (int)(4000 * (_gdalDataset.RasterXSize / (double)_gdalDataset.RasterYSize));
            }

            var buffer = new double[width * height];

            for (var band = 1; band <= Bands; band++)
            {
                using (var rasterBand = _gdalDataset.GetRasterBand(band))
                {
                    var err = rasterBand.ReadRaster(0, 0, _gdalDataset.RasterXSize, _gdalDataset.RasterYSize, 
                                                    buffer, 
                                                    width, height, 0, 0);
                    if (err != CPLErr.CE_None)
                        Console.WriteLine("err {0}", err);
                }

                foreach (double t in buffer)
                {
                    if (t < min)
                        min = t;
                    if (t > max)
                        max = t;
                }
            }

            var bitScalar = GetBitScalar();
            min /= bitScalar;
            max /= bitScalar;

            if (max > 255)
                max = 255;

            _stretchPoint = new Point((int)min, (int)max);
        }

        #region Disposers and finalizers

        /// <summary>
        /// Finalizer
        /// </summary>
        ~GdalRasterLayer()
        {
            ReleaseUnmanagedResources();
        }

        #endregion

        #region Implementation of ICanQueryLayer

        /// <summary>
        /// Returns the data associated with the centroid of the bounding box.
        /// </summary>
        /// <param name="box">Envelope to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            var pt = new Coordinate(box.MinX + 0.5 * box.Width,
                                  box.MaxY - 0.5 * box.Height);
            ExecuteIntersectionQuery(pt, ds);
        }

        private void ExecuteIntersectionQuery(Coordinate pt, FeatureDataSet ds)
        {

            if (CoordinateTransformation != null)
            {
                if (ReverseCoordinateTransformation != null)
                {
                    pt = GeometryTransform.TransformCoordinate(pt, ReverseCoordinateTransformation.MathTransform);
                }
                else
                {
                    CoordinateTransformation.MathTransform.Invert();
                    pt = GeometryTransform.TransformCoordinate(pt, CoordinateTransformation.MathTransform);
                    CoordinateTransformation.MathTransform.Invert();
                }
            }
            
            //Setup resulting Table
            var dt = new FeatureDataTable {TableName = LayerName};
            dt.Columns.Add("Ordinate X", typeof(Double));
            dt.Columns.Add("Ordinate Y", typeof(Double));
            for (int i = 1; i <= Bands; i++)
                dt.Columns.Add(string.Format("Value Band {0}", i), typeof(Double));

            //Get location on raster
            var buffer = new double[1];
            var bandMap = new int[Bands];
            for (int i = 1; i <= Bands; i++) bandMap[i - 1] = i;

            var geoTransform = new GeoTransform(_gdalDataset);
            var imgPt = geoTransform.GroundToImage(pt);
            Int32 x = Convert.ToInt32(imgPt.X);
            Int32 y = Convert.ToInt32(imgPt.Y);

            //Test if raster ordinates are within bounds
            if (x < 0) return;
            if (y < 0) return;
            if (x >= _imageSize.Width) return;
            if (y >= _imageSize.Height) return;

            //Create new row, add ordinates and location geometry
            FeatureDataRow dr = dt.NewRow();
            dr.Geometry = Factory.CreatePoint(pt);
            dr[0] = pt.X;
            dr[1] = pt.Y;

            //Add data from raster
            for (int i = 1; i <= Bands; i++)
            {
                Band band = _gdalDataset.GetRasterBand(i);
                //DataType dtype = band.DataType;
                CPLErr res = band.ReadRaster(x, y, 1, 1, buffer, 1, 1, 0, 0);
                if (res == CPLErr.CE_None)
                {
                    dr[1 + i] = buffer[0];
                }
                else
                {
                    dr[1 + i] = Double.NaN;
                }
            }
            //Add new row to table
            dt.Rows.Add(dr);

            //Add table to dataset
            ds.Tables.Add(dt);
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geometry, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(geometry.EnvelopeInternal, ds);
        }

        private bool _isQueryEnabled = true;
        public bool IsQueryEnabled
        {
            get
            {
                return _isQueryEnabled;
            }
            set
            {
                _isQueryEnabled = value;
            }
        }

        #endregion

        private Color _noDataInitColor = Color.Magenta;
        public Color NoDataInitColor
        {
            get { return _noDataInitColor; }
            set { _noDataInitColor = value; }
        }

        private ColorBlend _colorBlend;
        public ColorBlend ColorBlend
        {
            get { return _colorBlend; }
            set { _colorBlend = value; }
        }

        private bool DoublesAreEqual(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < double.Epsilon;
        }
    }
}
