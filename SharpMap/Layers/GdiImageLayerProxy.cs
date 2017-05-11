// Copyright 2014 -      Robert Smart (www.cnl-software), Felix Obermaier (www.ivv-aachen.de)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Common.Logging;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    /// <summary>
    /// Image manipulation proxy layer
    /// </summary>
    /// <remarks>This layer is not for layers implementing <see cref="ITileAsyncLayer"/>. Every </remarks>
    /// <typeparam name="T">The type of the proxy layer. <see cref="ITileAsyncLayer"/> are not excluded, but are not handled in any way.</typeparam>
    // ToDo: think of a better name:
    // - ImageManipulationProxyLayer
    // - What to do with large images
    // - 
    [Serializable]
    public class GdiImageLayerProxy<T> : ICanQueryLayer, IDisposable
        where T: class, ILayer
    {
        /// <summary>
        /// Creates a proxy class that transforms all colors to grey scale
        /// </summary>
        /// <param name="baseLayer">The layer to be proxied</param>
        /// <returns>A proxy layer</returns>
        public static GdiImageLayerProxy<T> CreateGreyScale(T baseLayer)
        {
            var colorMatrix = new ColorMatrix(
                new[]
                {
                    new[] {.3f, .3f, .3f, 0, 0},
                    new[] {.59f, .59f, .59f, 0, 0},
                    new[] {.11f, .11f, .11f, 0, 0},
                    new[] {0f, 0, 0, 1, 0},
                    new[] {0f, 0, 0, 0, 1}
                });
            return new GdiImageLayerProxy<T>(baseLayer, colorMatrix);
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(GdiImageLayerProxy<T>));

        private readonly ColorMatrix _colorMatrix;
        private readonly ColorMap[] _colorMap;
        private readonly T _baseLayer;

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="opacity"/>
        /// </summary>
        /// <param name="layer">The layer to be proxied</param>
        /// <param name="opacity">An opacity value in the range of [0f, 1f]. Values outside of that range will be clipped.</param>
        public GdiImageLayerProxy(T layer, float opacity)
            :this(layer, new ColorMatrix {Matrix33 = Math.Max(Math.Min(1f, opacity), 0f)})
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="colorMatrix"/>
        /// </summary>
        /// <param name="layer">The layer to be proxied</param>
        /// <param name="colorMatrix">A color matrix that is to be applied upon drawing</param>
        public GdiImageLayerProxy(T layer, ColorMatrix colorMatrix)
        {
            if (layer is ITileAsyncLayer)
            {
                Logger.Warn("ITileAsyncLayer is not a valid layer for GdiImageLayerProxy<T>");
            }
            _baseLayer = layer;
            _colorMatrix = colorMatrix;
            VisibilityUnits = VisibilityUnits.ZoomLevel;
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="colorMap"/>
        /// </summary>
        /// <param name="layer">The layer to be proxied</param>
        /// <param name="colorMap">The color map</param>
        public GdiImageLayerProxy(T layer, params ColorMap[] colorMap)
        {
            if (layer is ITileAsyncLayer)
            {
                Logger.Warn("ITileAsyncLayer is not a valid layer for GdiImageLayerProxy<T>");
            }
            _baseLayer = layer;
            _colorMap = colorMap;
            VisibilityUnits = VisibilityUnits.ZoomLevel;
        }

        /// <summary>
        /// Gets a value indicating the proxied base layer
        /// </summary>
        public T BaseLayer{ get { return _baseLayer; }}

        double ILayer.MinVisible
        {
            get { return _baseLayer.MinVisible; }
            set { _baseLayer.MinVisible = value; }
        }

        double ILayer.MaxVisible
        {
            get { return _baseLayer.MaxVisible; }
            set { _baseLayer.MaxVisible = value; }
        }

        public VisibilityUnits VisibilityUnits { get; set; }

        bool ILayer.Enabled
        {
            get { return _baseLayer.Enabled; }
            set { _baseLayer.Enabled = value; }
        }

        public string LayerTitle
        {
            get { return _baseLayer.LayerTitle; }
            set { _baseLayer.LayerTitle = value; }
        }

        string ILayer.LayerName
        {
            get { return _baseLayer.LayerName; }
            set { _baseLayer.LayerName = value; }
        }

        Envelope ILayer.Envelope
        {
            get { return _baseLayer.Envelope; }
        }

        int ILayer.SRID
        {
            get { return _baseLayer.SRID; }
            set { _baseLayer.SRID = value; }
        }

        int ILayer.TargetSRID
        {
            get { return _baseLayer.TargetSRID; }
        }

        string ILayer.Proj4Projection
        {
            get { return _baseLayer.Proj4Projection; }
            set { _baseLayer.Proj4Projection = value; }
        }

        void ILayer.Render(Graphics g, Map map)
        {
            ((ILayer)this).Render(g, (MapViewport)map);
        }

        void ILayer.Render(Graphics g, MapViewport map)
        {
            if (_baseLayer is ITileAsyncLayer)
            {
                Logger.Warn("ITileAsyncLayer is not a valid layer for GdiImageLayerProxy<T>. -> Skipping");
                _baseLayer.Render(g, map);
                return;
            }
            
            var s = map.Size;
            using (var img = new Bitmap(s.Width, s.Height, PixelFormat.Format32bppArgb))
            {
                using (var gg = Graphics.FromImage(img))
                {
                    _baseLayer.Render(gg, map);
                }

                using (var ia = CreateImageAttributes())
                {
                    g.DrawImage(img, new Rectangle(0, 0, s.Width, s.Height), 
                        0, 0, s.Width, s.Height, GraphicsUnit.Pixel, ia);
                }
            }
        }

        private ImageAttributes CreateImageAttributes()
        {
            var ia = new ImageAttributes();
            if (_colorMatrix != null)
            {
                ia.SetColorMatrix(_colorMatrix);
            }
            else if (_colorMap != null)
            {
                ia.SetRemapTable(_colorMap);
            }
            return ia;
        }

        public void Dispose()
        {
            if (_baseLayer is IDisposable)
            {
                ((IDisposable)_baseLayer).Dispose();
            }
        }

        void ICanQueryLayer.ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            if (_baseLayer is ICanQueryLayer)
            {
                ((ICanQueryLayer)_baseLayer).ExecuteIntersectionQuery(box, ds);
            }
        }

        void ICanQueryLayer.ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            if (_baseLayer is ICanQueryLayer)
            {
                ((ICanQueryLayer)_baseLayer).ExecuteIntersectionQuery(geometry, ds);
            }
        }

        bool ICanQueryLayer.IsQueryEnabled
        {
            get
            {
                if (_baseLayer is ICanQueryLayer)
                    return ((ICanQueryLayer) _baseLayer).IsQueryEnabled;
                return false;
            }
            set
            {
                if (_baseLayer is ICanQueryLayer)
                    ((ICanQueryLayer)_baseLayer).IsQueryEnabled = value;
            }
        }
    }
}
