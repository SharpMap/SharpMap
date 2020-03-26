// Copyright 2013 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Layers.HeatLayer.
// SharpMap.Layers.HeatLayer is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Layers.HeatLayer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// *******************************************************************************
//
// Based on idea by Corey Fournier
// http://www.codeproject.com/Articles/88956/GHeat-NET
//
// *******************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using ColorBlend = SharpMap.Rendering.Thematics.ColorBlend;

namespace SharpMap.Layers
{
    [Serializable]
    public class HeatLayer : Layer, ICanQueryLayer
    {
        /// <summary>
        /// A delegate function definition that computes the heat value from a <seealso cref="FeatureDataRow"/>
        /// </summary>
        /// <returns>A value in the range &#x211d;[0, 1f] </returns>
        public Func<FeatureDataRow, float> HeatValueComputer { get; set; }

        /// <summary>
        /// A color blend that transforms heat values into colors.
        /// <para>Note: in order to make non hot areas transparent, be sure to start your
        /// <seealso cref="ColorBlend"/> with a <seealso cref="Color"/> that has an 
        /// <seealso cref="Color.A"/> value of 0.</para>
        /// </summary>
        public ColorBlend HeatColorBlend { get; set; }
        
        /// <summary>
        /// A list of dots, that are used as heat value markers
        /// </summary>
        private readonly Bitmap[] _bitmaps = new Bitmap[32];
        private readonly float [] _opacity = new float[32];

        private double _opacityMin;
        private double _opacityMax;
        private double _zoomMin;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        private HeatLayer()
        {
            ZoomMin = Double.NaN;
            _bitmaps = GenerateDots();
            HeatColorBlend = Fire;
            _opacityMax = 0.7;
            _opacityMin = 1f;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="vectorLayer">The base layer</param>
        /// <param name="heatValueColumn">The name of the column that contains the heat value</param>
        /// <param name="heatValueScale">A value that is responsible to scale the heat value to the range &#x211d;[0, 1f]</param>
        public HeatLayer(VectorLayer vectorLayer, string heatValueColumn, float heatValueScale = 1f)
            : this()
        {
            BaseLayer = vectorLayer;
            LayerName = "heat_" + vectorLayer.LayerName;
            DataSource = vectorLayer.DataSource;
            HeatValueComputer = GetHeatValueFromColumn;
            HeatValueColumn = heatValueColumn;
            HeatValueScale = heatValueScale;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="provider">The provider</param>
        /// <param name="heatValueColumn">The name of the column that contains the heat value</param>
        /// <param name="heatValueScale">A value that is responsible to scale the heat value to the range &#x211d;[0, 1f]</param>
        public HeatLayer(IProvider provider, string heatValueColumn, float heatValueScale = 1f)
            :this()
        {
            DataSource = provider;
            LayerName = "heat_" + provider.ConnectionID + heatValueColumn;
            HeatValueComputer = GetHeatValueFromColumn;
            HeatValueColumn = heatValueColumn;
            HeatValueScale = heatValueScale;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="vectorLayer">The base layer</param>
        /// <param name="heatComputer">A function to compute the heat value from a <seealso cref="FeatureDataRow"/></param>
        public HeatLayer(VectorLayer vectorLayer, Func<FeatureDataRow, float> heatComputer)
            : this()
        {
            BaseLayer = vectorLayer;
            LayerName = "heat_" + vectorLayer.LayerName;
            DataSource = vectorLayer.DataSource;
            HeatValueComputer = heatComputer;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="provider">The provider</param>
        /// <param name="heatComputer">A function to compute the heat value from a <seealso cref="FeatureDataRow"/></param>
        public HeatLayer(IProvider provider, Func<FeatureDataRow, float> heatComputer)
            : this()
        {
            DataSource = provider;
            LayerName = "heat_" + provider.ConnectionID;
            HeatValueComputer = heatComputer;
        }

        /// <summary>
        /// Gets or sets the name of the column that contains the heat value
        /// </summary>
        public string HeatValueColumn { get; set; }

        /// <summary>
        /// Gets or sets a scale value to get a heat value in the range &#x211d;[0, 1].
        /// </summary>
        public float HeatValueScale { get; set; }

        /// <summary>
        /// Gets the base layer
        /// </summary>
        public VectorLayer BaseLayer { get; private set; }
        
        /// <summary>
        /// Gets the provider that serves the heat value features
        /// </summary>
        public IBaseProvider DataSource { get; private set; }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, MapViewport map)
        {
            if (BaseLayer != null)
            {
                BaseLayer.Render(g, map);
            }
            
            var fds = new FeatureDataSet();
            var box = map.Envelope;
            ExecuteIntersectionQuery(box, fds);
            
            if (fds.Tables.Count == 0 || fds.Tables[0].Rows.Count == 0)
            {
                //base.Render(g, map);
                return;
            }

            var zoomIndex = GetZoomIndex(map.Zoom);
            var dot = (Bitmap)_bitmaps[zoomIndex].Clone();
            var opacity = _opacity[zoomIndex];

            using (var image = new Bitmap(map.Size.Width + dot.Width, map.Size.Height + dot.Height, PixelFormat.Format32bppArgb))
            {
                using (var gr = Graphics.FromImage(image))
                {
                    gr.Clear(Color.White);
                }

                DrawPoints(map, fds.Tables[0].Select(), dot, image);
                Colorize(image, HeatColorBlend, opacity);
                
                g.DrawImage(image, -dot.Width/2, -dot.Height/2);
            }
            dot.Dispose();

            // Invoke the LayerRendered event.
            OnLayerRendered(g);
        }

        /// <summary>
        /// Gets a linear gradient scale with 5 colors making a fire-like color blend
        /// </summary>
        public static ColorBlend Fire
        {
            get
            {
                return new ColorBlend(new[] { Color.FromArgb(0, Color.White), Color.Yellow, Color.Orange, Color.Red, Color.Gray },
                                      new[] { 0f, 0.1f, 0.4f, 0.8f, 1f });
            }
        }
        /// <summary>
        /// Gets a linear gradient scale with seven colours making a rainbow from red to violet.
        /// </summary>
        /// <remarks>
        /// Colors span the following with an interval of 1/6:
        /// { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet }
        /// </remarks>
        public static ColorBlend Classic
        {
            get
            {
                return new ColorBlend(
                    new[] { Color.FromArgb(0, Color.White), Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Indigo},
                    new[] { 0f,                             0.125f,    0.25f,        0.375f,       0.5f,        0.75f,      0.9f,       1 });
            }
        }


        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override Envelope Envelope
        {
            get
            {
                var res = DataSource.GetExtents();
                if (CoordinateTransformation != null)
                {
                    return GeometryTransform.TransformBox(res, CoordinateTransformation.MathTransform);
                }
                return res;
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// 
        /// Note! The table added should be named according to the LayerName!
        /// </summary>
        /// <param name="box">Bounding box to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            if (CoordinateTransformation != null)
            {
                box = GeometryTransform.TransformBox(box, CoordinateTransformation.MathTransform.Inverse());
            }
            DataSource.ExecuteIntersectionQuery(box, ds);
            if (ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = LayerName;
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// 
        /// Note! The table added should be named according to the LayerName!
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            if (CoordinateTransformation != null)
            {
                geometry = GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.MathTransform.Inverse(), geometry.Factory);
            }
            DataSource.ExecuteIntersectionQuery(geometry, ds);
            if (ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = LayerName;
            }
        }

        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, 
        /// ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE.
        /// This property currently only applies to WMS and should perhaps be moved to a WMS
        /// specific class.
        /// </summary>
        public bool IsQueryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Map.Zoom"/> value, at which the biggest heat value symbol should be drawn
        /// </summary>
        public double ZoomMin
        {
            get
            {
                if (double.IsNaN(_zoomMin))
                    CalculateZoomMinMax();
                return _zoomMin;
            }
            set { _zoomMin = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Map.Zoom"/> value, at which the smallest heat value symbol should be drawn
        /// </summary>
        public double ZoomMax { get; set; }

        /// <summary>
        /// Gets or sets an opacity value for the <seealso cref="ZoomMin"/> value
        /// </summary>
        public double OpacityMin
        {
            get { return _opacityMin; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _opacityMin = value;
                InterpolateOpacityValues();
            }
        }

        /// <summary>
        /// Gets or sets an opacity value for the <seealso cref="ZoomMax"/> value
        /// </summary>
        public double OpacityMax
        {
            get { return _opacityMax; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _opacityMax = value;
                InterpolateOpacityValues();
            }
        }

        /// <summary>
        /// Method compute the <seealso cref="ZoomMin"/> and <seealso cref="ZoomMax"/> values
        /// </summary>
        /// <param name="portion">A protion that is to be cut off. Value must be less than 0.5f</param>
        public void CalculateZoomMinMax(double portion = 0.1d)
        {
            if (portion >= 0.5d)
            {
                throw new ArgumentException("portion");
            }

            var ext = DataSource.GetExtents();
            ZoomMin = ext.MaxExtent*portion;
            ZoomMax = ext.MaxExtent - ZoomMin;
        }

        #region private helper methods

        /// <summary>
        /// Method to generate the base markers for different zoom levels
        /// </summary>
        /// <returns>An array of bitmaps</returns>
        private static Bitmap[] GenerateDots()
        {
            var res = new Bitmap[32];
            var size = 6;
            for (var i = 0; i < 32; i++)
            {
                var bmp = new Bitmap(size, size, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(0, 0, size-1, size-1);
                        var brush = new PathGradientBrush(path)
                        {
                            CenterColor = Color.Gray,
                            SurroundColors = new[] { Color.White },
                        };
                        g.FillEllipse(brush, 0, 0, size-1, size-1);
                    }
                }
                res[i] = bmp;
                //bmp.Save("dot" + i + ".png");
                size += 3;
            }
            return res;
        }

        /// <summary>
        /// Change the intensity of the image
        /// </summary>
        /// <param name="image">The dot image</param>
        /// <param name="weight">The weight to apply</param>
        /// <returns>The weighted image</returns>
        private static Bitmap ApplyHeatValueToImage(Bitmap image, float weight)
        {
            var tempImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(tempImage))
            {
                //I want to make the color more intense (White/bright)
                if (weight < 0.02f) weight = 0.02f;
                weight *= 5f;
                if (weight > 5f) weight = 5f;

                // Create ImageAttributes
                var ia = new ImageAttributes();

                //Gamma values range from 0.1 to 5.0 (normally 0.1 to 2.2), with 0.1 being the brightest and 5.0 the darkest.
                //Convert the 100% to a range of 0.1-5 by multiplying it by 5
                ia.SetGamma(weight, ColorAdjustType.Bitmap);

                // Draw Image with the attributes
                g.DrawImage(image,
                            new Rectangle(0, 0, image.Width, image.Height),
                            0, 0, image.Width, image.Height,
                            GraphicsUnit.Pixel, ia);
            }
            //New dot with a different intensity
            return tempImage;
        }

        private float GetHeatValueFromColumn(FeatureDataRow row)
        {
            if (row == null)
                return 0;

            if (row[HeatValueColumn] == DBNull.Value)
                return 0;

            var res = HeatValueScale * Convert.ToSingle(row[HeatValueColumn]);
            return res < 0f ? 0f : res > 1f ? 1f : res;

        }

        /// <summary>
        /// Method to get the symbol index for the <paramref name="zoom"/>
        /// </summary>
        /// <param name="zoom">The zoom</param>
        /// <returns>The symbol's index</returns>
        private int GetZoomIndex(double zoom)
        {
            if (zoom <= ZoomMin)
                return 31;
            if (zoom >= ZoomMax)
                return 0;

            var dz = ZoomMax - ZoomMin;
            zoom -= ZoomMin;
            var res = 0;
            while (dz > zoom)
            {
                res += 1;
                dz /= 2;
            }
            if (res > 31) res = 31;
            return res;
        }

        private static void Colorize(Bitmap image, ColorBlend heatPalette, float f)
        {
            if (f < 0) f = 0;
            if (f > 1) f = 1;

            var imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite,
                                           PixelFormat.Format32bppPArgb);

            var imageStride = imageData.Stride;
            var imageScan0 = imageData.Scan0;

            for (var y = 0; y < image.Height; y++)
            {
                var buffer = new byte[imageStride];
                System.Runtime.InteropServices.Marshal.Copy(imageScan0 + y * imageStride, buffer, 0, imageStride);

                for (var x = 0; x < image.Width * 4; x += 4)
                {
                    var colorIndex = (255 - buffer[x+2]) / 255f;
                    var color = heatPalette.GetColor(colorIndex);
                    var alpha = Convert.ToInt32(255f * (color.A / 255f) * (buffer[x+3] / 255f) * f);
                    color = Color.FromArgb(alpha, color);
                    buffer[x + 3] = color.A;
                    buffer[x + 2] = color.R;
                    buffer[x + 1] = color.G;
                    buffer[x + 0] = color.B;
                }

                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, imageScan0 + y * imageStride, imageStride);

            }

            image.UnlockBits(imageData);
        }

        private void DrawPoints(MapViewport map, IEnumerable<DataRow> features, Bitmap dot, Bitmap image)
        {
            var size = new Size(dot.Width, dot.Height);

            foreach (FeatureDataRow row in features)
            {
                var heatValue = HeatValueComputer(row);
                if (heatValue <= 0) continue;
                if (heatValue >= 1f) heatValue = 1;

                var c = row.Geometry.PointOnSurface.Coordinate;
                if (CoordinateTransformation != null)
                {
                    c = GeometryTransform.TransformCoordinate(c, CoordinateTransformation.MathTransform);
                }
                var posF = map.WorldToImage(c);
                var pos = Point.Round(posF);
                //var pos = Point.Round(PointF.Subtract(posF, halfSize));

                using (var tmpDot = ApplyHeatValueToImage(dot, heatValue))
                {
                    ImageBlender.BlendImages(image, pos.X, pos.Y, size.Width, size.Height,
                                             tmpDot, 0, 0, BlendOperation.BlendMultiply);
                }
            }
        }

        private void InterpolateOpacityValues()
        {
            _opacity[0] = (float)_opacityMax;
            var dOpacity = (float)(_opacityMin - _opacityMax)/31f;
            for (var i = 1; i < 31; i++)
                _opacity[i] = _opacity[i - 1] + dOpacity;
            _opacity[31] = (float)_opacityMin;
        }

        #endregion

    }
}
