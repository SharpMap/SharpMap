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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA using System;
/* 
 * Author John Diss 2009 (www.newgrove.com)
 * 
 */

using System.Collections.Generic;
using System.Drawing;
#if !DotSpatialProjections
using ProjNet.CoordinateSystems.Transformations;
#else
using DotSpatial.Projections;
#endif
using SharpMap.Geometries;
using Point = System.Drawing.Point;

namespace SharpMap.Layers
{
    public class GdalRasterLayerCachingProxy : Layer
    {
        private readonly GdalRasterLayer _innerLayer;

        public GdalRasterLayerCachingProxy(GdalRasterLayer innerLayer)
        {
            _innerLayer = innerLayer;
            LayerName = innerLayer.LayerName;
        }

        public GdalRasterLayerCachingProxy(string strLayerName, string imageFilename)
        {
            LayerName = strLayerName;
            _innerLayer = new GdalRasterLayer(strLayerName, imageFilename);
        }

        public string Filename
        {
            get { return _innerLayer.Filename; }
            set
            {
                CheckUpdate(Filename, value);
                _innerLayer.Filename = value;
            }
        }

        /// <summary>
        /// Gets or sets the bit depth of the raster file
        /// </summary>
        public int BitDepth
        {
            get { return _innerLayer.BitDepth; }
            set
            {
                CheckUpdate(BitDepth, value);
                _innerLayer.BitDepth = value;
            }
        }

        /// <summary>
        /// Gets or set the projection of the raster file
        /// </summary>
        public string Projection
        {
            get { return _innerLayer.Projection; }
            set
            {
                CheckUpdate(Projection, value);
                _innerLayer.Projection = value;
            }
        }

        /// <summary>
        /// Gets or sets to display IR Band
        /// </summary>
        public bool DisplayIR
        {
            get { return _innerLayer.DisplayIR; }
            set
            {
                CheckUpdate(DisplayIR, value);
                _innerLayer.DisplayIR = value;
            }
        }

        /// <summary>
        /// Gets or sets to display color InfraRed
        /// </summary>
        public bool DisplayCIR
        {
            get { return _innerLayer.DisplayCIR; }
            set
            {
                CheckUpdate(DisplayCIR, value);
                _innerLayer.DisplayCIR = value;
            }
        }

        /// <summary>
        /// Gets or sets to display clip
        /// </summary>
        public bool ShowClip
        {
            get { return _innerLayer.ShowClip; }
            set
            {
                CheckUpdate(ShowClip, value);
                _innerLayer.ShowClip = value;
            }
        }

        /// <summary>
        /// Gets or sets to display gamma
        /// </summary>
        public double Gamma
        {
            get { return _innerLayer.Gamma; }
            set
            {
                CheckUpdate(Gamma, value);
                _innerLayer.Gamma = value;
            }
        }

        /// <summary>
        /// Gets or sets to display gamma for Spot spot
        /// </summary>
        public double SpotGamma
        {
            get { return _innerLayer.SpotGamma; }
            set
            {
                CheckUpdate(SpotGamma, value);
                _innerLayer.SpotGamma = value;
            }
        }

        /// <summary>
        /// Gets or sets to display gamma for NonSpot
        /// </summary>
        public double NonSpotGamma
        {
            get { return _innerLayer.NonSpotGamma; }
            set
            {
                CheckUpdate(NonSpotGamma, value);
                _innerLayer.NonSpotGamma = value;
            }
        }

        /// <summary>
        /// Gets or sets to display red Gain
        /// </summary>
        public double[] Gain
        {
            get { return _innerLayer.Gain; }
            set
            {
                CheckUpdate(Gain, value);
                _innerLayer.Gain = value;
            }
        }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] SpotGain
        {
            get { return _innerLayer.SpotGain; }
            set
            {
                CheckUpdate(SpotGain, value);
                _innerLayer.SpotGain = value;
            }
        }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] NonSpotGain
        {
            get { return _innerLayer.NonSpotGain; }
            set
            {
                CheckUpdate(NonSpotGain, value);
                _innerLayer.NonSpotGain = value;
            }
        }

        /// <summary>
        /// Gets or sets to display curve lut
        /// </summary>
        public List<int[]> CurveLut
        {
            get { return _innerLayer.CurveLut; }
            set
            {
                CheckUpdate(CurveLut, value);
                _innerLayer.CurveLut = value;
            }
        }

        /// <summary>
        /// Correct Spot spot
        /// </summary>
        public bool HaveSpot
        {
            get { return _innerLayer.HaveSpot; }
            set
            {
                CheckUpdate(HaveSpot, value);
                _innerLayer.HaveSpot = value;
            }
        }

        /// <summary>
        /// Gets or sets to display curve lut for Spot spot
        /// </summary>
        public List<int[]> SpotCurveLut
        {
            get { return _innerLayer.SpotCurveLut; }
            set
            {
                CheckUpdate(SpotCurveLut, value);
                _innerLayer.SpotCurveLut = value;
            }
        }

        /// <summary>
        /// Gets or sets to display curve lut for NonSpot
        /// </summary>
        public List<int[]> NonSpotCurveLut
        {
            get { return _innerLayer.NonSpotCurveLut; }
            set
            {
                CheckUpdate(NonSpotCurveLut, value);
                _innerLayer.NonSpotCurveLut = value;
            }
        }

        /// <summary>
        /// Gets or sets the center point of the Spot spot
        /// </summary>
        public PointF SpotPoint
        {
            get { return _innerLayer.SpotPoint; }
            set
            {
                CheckUpdate(SpotPoint, value);
                _innerLayer.SpotPoint = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner radius for the spot
        /// </summary>
        public double InnerSpotRadius
        {
            get { return _innerLayer.InnerSpotRadius; }
            set
            {
                CheckUpdate(InnerSpotRadius, value);
                _innerLayer.InnerSpotRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets the outer radius for the spot (feather zone)
        /// </summary>
        public double OuterSpotRadius
        {
            get { return _innerLayer.OuterSpotRadius; }
            set
            {
                CheckUpdate(OuterSpotRadius, value);
                _innerLayer.OuterSpotRadius = value;
            }
        }

        /// <summary>
        /// Gets the true histogram
        /// </summary>
        public List<int[]> Histogram
        {
            get { return _innerLayer.Histogram; }
        }

        /// <summary>
        /// Gets the quick histogram mean
        /// </summary>
        public double[] HistoMean
        {
            get { return _innerLayer.HistoMean; }
        }

        /// <summary>
        /// Gets the quick histogram brightness
        /// </summary>
        public double HistoBrightness
        {
            get { return _innerLayer.HistoBrightness; }
        }

        /// <summary>
        /// Gets the quick histogram contrast
        /// </summary>
        public double HistoContrast
        {
            get { return _innerLayer.HistoContrast; }
        }

        /// <summary>
        /// Gets the number of bands
        /// </summary>
        public int Bands
        {
            get { return _innerLayer.Bands; }
        }

        /// <summary>
        /// Gets the GSD (Horizontal)
        /// </summary>
        public double GSD
        {
            get { return _innerLayer.GSD; }
        }

        ///<summary>
        /// Use rotation information
        /// </summary>
        public bool UseRotation
        {
            get { return _innerLayer.UseRotation; }
            set
            {
                CheckUpdate(UseRotation, value);
                _innerLayer.UseRotation = value;
            }
        }

        public Size Size
        {
            get { return _innerLayer.Size; }
        }

        public bool ColorCorrect
        {
            get { return _innerLayer.ColorCorrect; }
            set
            {
                CheckUpdate(ColorCorrect, value);
                _innerLayer.ColorCorrect = value;
            }
        }

        public Rectangle HistoBounds
        {
            get { return _innerLayer.HistoBounds; }
            set
            {
                CheckUpdate(HistoBounds, value);
                _innerLayer.HistoBounds = value;
            }
        }

        public ICoordinateTransformation Transform
        {
            get { return _innerLayer.Transform; }
        }

        public Color TransparentColor
        {
            get { return _innerLayer.TransparentColor; }
            set
            {
                CheckUpdate(TransparentColor, value);
                _innerLayer.TransparentColor = value;
            }
        }

        public Point StretchPoint
        {
            get { return _innerLayer.StretchPoint; }
            set
            {
                CheckUpdate(StretchPoint, value);
                _innerLayer.StretchPoint = value;
            }
        }

        public override BoundingBox Envelope
        {
            get { return _innerLayer.Envelope; }
        }

        protected internal bool RequiresRedraw { get; set; }

        private Size? _lastRenderedSize;
        protected Size? LastRenderedSize
        {
            get { return _lastRenderedSize; }
            set
            {
                CheckUpdate(LastRenderedSize, value);
                _lastRenderedSize = value;
            }
        }

        private BoundingBox _lastRenderedExtents;
        protected BoundingBox LastRenderedExtents
        {
            get { return _lastRenderedExtents; }
            set
            {
                CheckUpdate(LastRenderedExtents, value);
                _lastRenderedExtents = value;
            }
        }

        private Bitmap _cachedBitmap;
        protected Bitmap CachedBitmap
        {
            get { return _cachedBitmap; }
            set { _cachedBitmap = value; }
        }

        private void CheckUpdate<T>(T currentValue, T newValue)
        {
            if (!Equals(currentValue, newValue))
                RequiresRedraw = true;
        }

        public override void Render(Graphics g, Map map)
        {
            LastRenderedSize = map.Size;
            LastRenderedExtents = map.Envelope;
            if (RequiresRedraw || CachedBitmap == null)
            {
                Bitmap bmp = new Bitmap(LastRenderedSize.Value.Width, LastRenderedSize.Value.Height);
                using (Graphics g2 = Graphics.FromImage(bmp))
                {
                    _innerLayer.Render(g2, map);
                    CachedBitmap = bmp;
                }
            }
            RequiresRedraw = false;
            g.DrawImageUnscaled(CachedBitmap, 0, 0);
            base.Render(g, map);
        }
    }
}