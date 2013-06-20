// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using GeoAPI.Geometries;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Decoration;
using SharpMap.Utilities;
using Point = GeoAPI.Geometries.Coordinate;
using System.Drawing.Imaging;
using Common.Logging;
using System.Reflection;

namespace SharpMap
{
    /// <summary>
    /// Map class, the main holder for a MapObject in SharpMap
    /// </summary>
    /// <example>
    /// Creating a new map instance, adding layers and rendering the map:
    /// </example>
    [Serializable]
    public class Map : IDisposable
    {
        static Map()
        {
            if (!(System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime))
            {
                // We have to do this initialization with reflection due to the fact that NTS can reference an older version of GeoAPI and redirection 
                // is not available at design time..
                var ntsAssembly = Assembly.Load("NetTopologySuite");
                if (GeoAPI.GeometryServiceProvider.Instance == null)
                {
                    var ntsApiGeometryServices = ntsAssembly.GetType("NetTopologySuite.NtsGeometryServices");
                    GeoAPI.GeometryServiceProvider.Instance = ntsApiGeometryServices.GetProperty("Instance").GetValue(null, null) as GeoAPI.IGeometryServices;
                }
            }
        }

        readonly ILog _logger = LogManager.GetLogger(typeof(Map));

        /// <summary>
        /// Used for converting numbers to/from strings
        /// </summary>
        public static NumberFormatInfo NumberFormatEnUs = new CultureInfo("en-US", false).NumberFormat;

        #region Fields
        private readonly List<IMapDecoration> _decorations = new List<IMapDecoration>();

        private Color _backgroundColor;
        private int _srid = -1;
        private double _zoom;
        private Point _center;
        private readonly LayerCollection _layers;
        private readonly LayerCollection _backgroundLayers;
        private readonly VariableLayerCollection _variableLayers;
        private Matrix _mapTransform;
        internal Matrix MapTransformInverted;
        
        private readonly MapViewPortGuard _mapViewportGuard;
        #endregion


        /// <summary>
        /// Specifies whether to trigger a dispose on all layers (and their datasources) contained in this map when the map-object is disposed.
        /// The default behaviour is true unless the map is a result of a Map.Clone() operation in which case the value is false
        /// <para/>
        /// If you reuse your datasources or layers between many map-objects you should set this property to false in order for them to keep existing after a map.dispose()
        /// </summary>
        public bool DisposeLayersOnDispose = true;

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public Map() : this(new Size(640, 480))
        {

        }

        /// <summary>
        /// Initializes a new map
        /// </summary>
        /// <param name="size">Size of map in pixels</param>
        public Map(Size size)
        {
            _mapViewportGuard = new MapViewPortGuard(size, 0d, Double.MaxValue);

            if (!(System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime))
            {
                Factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(_srid);
            }
            _layers = new LayerCollection();
            _backgroundLayers = new LayerCollection();
            _variableLayers = new VariableLayerCollection(_layers);
            BackColor = Color.Transparent;
            _mapTransform = new Matrix();
            MapTransformInverted = new Matrix();
            _center = new Point(0, 0);
            _zoom = 1;

            WireEvents();

            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("Map initialized with size {0},{1}", size.Width, size.Height);
        }

        /// <summary>
        /// Wires the events
        /// </summary>
        private void WireEvents()
        {
            _backgroundLayers.ListChanged += HandleLayersCollectionChanged;
        }

        /// <summary>
        /// Event handler to intercept when a new ITileAsymclayer is added to the Layers List and associate the MapNewTile Handler Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleLayersCollectionChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (sender == _backgroundLayers)
            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded)
            {

                var l = _backgroundLayers[e.NewIndex];
                var tileAsyncLayer = l as ITileAsyncLayer;
                if (tileAsyncLayer != null)
                {
                    if (tileAsyncLayer.OnlyRedrawWhenComplete)
                    {
                        tileAsyncLayer.DownloadProgressChanged += new DownloadProgressHandler(layer_DownloadProgressChanged);
                    }
                    else
                    {
                        tileAsyncLayer.MapNewTileAvaliable += MapNewTileAvaliableHandler;
                    }
                }
            }
        }

        void layer_DownloadProgressChanged(int tilesRemaining)
        {
            if (tilesRemaining <= 0)
            {
                var e = RefreshNeeded;
                if (e != null)
                    e(this, new EventArgs());
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the map object
        /// </summary>
        public void Dispose()
        {
            if (DisposeLayersOnDispose)
            {
                if (Layers != null)
                {
                    foreach (ILayer layer in Layers)
                        if (layer is IDisposable)
                            ((IDisposable)layer).Dispose();
                }
                if (BackgroundLayer != null)
                {
                    foreach (ILayer layer in BackgroundLayer)
                        if (layer is IDisposable)
                            ((IDisposable)layer).Dispose();
                }
                if (VariableLayers != null)
                {
                    foreach (ILayer layer in VariableLayers)
                        if (layer is IDisposable)
                            ((IDisposable)layer).Dispose();
                }
            }
            if (Layers != null)
            {
                Layers.Clear();
            }
            if (BackgroundLayer != null)
            {
                BackgroundLayer.Clear();
            }
            if (VariableLayers != null)
            {
                VariableLayers.Clear();
            }

        }

        #endregion

        #region Events

        #region Delegates

        /// <summary>
        /// EventHandler for event fired when the maps layer list has been changed
        /// </summary>
        public delegate void LayersChangedEventHandler();

        /// <summary>
        /// EventHandler for event fired when all layers have been rendered
        /// </summary>
        public delegate void MapRenderedEventHandler(Graphics g);

        /// <summary>
        /// EventHandler for event fired when all layers are about to be rendered
        /// </summary>
        public delegate void MapRenderingEventHandler(Graphics g);

        /// <summary>
        /// EventHandler for event fired when the zoomlevel or the center point has been changed
        /// </summary>
        public delegate void MapViewChangedHandler();


        #endregion

        /// <summary>
        /// Event fired when the maps layer list have been changed
        /// </summary>
        [Obsolete("This event is never invoked since it has been made impossible to change the LayerCollection for a map instance.")]
#pragma warning disable 67
        public event LayersChangedEventHandler LayersChanged;
#pragma warning restore 67

        /// <summary>
        /// Event fired when the zoomlevel or the center point has been changed
        /// </summary>
        public event MapViewChangedHandler MapViewOnChange;


        /// <summary>
        /// Event fired when all layers are about to be rendered
        /// </summary>
        public event MapRenderedEventHandler MapRendering;

        /// <summary>
        /// Event fired when all layers have been rendered
        /// </summary>
        public event MapRenderedEventHandler MapRendered;

        /// <summary>
        /// Event fired when one layer have been rendered
        /// </summary>
        public event EventHandler<LayerRenderingEventArgs> LayerRendering;

        /// <summary>
        /// Event fired when one layer have been rendered
        /// </summary>
        public event EventHandler<LayerRenderingEventArgs> LayerRenderedEx;

        ///<summary>
        /// Event fired when a layer has been rendered
        ///</summary>
        [Obsolete("Use LayerRenderedEx")]
        public event EventHandler LayerRendered;

        /// <summary>
        /// Event fired when a new Tile is available in a TileAsyncLayer
        /// </summary>
        public event MapNewTileAvaliabledHandler MapNewTileAvaliable;

        /// <summary>
        /// Event that is called when a layer have changed and the map need to redraw
        /// </summary>
        public event EventHandler RefreshNeeded;

        #endregion

        #region Methods

        /// <summary>
        /// Renders the map to an image
        /// </summary>
        /// <returns>the map image</returns>
        public Image GetMap()
        {
            Image img = new Bitmap(Size.Width, Size.Height);
            Graphics g = Graphics.FromImage(img);
            RenderMap(g);
            g.Dispose();
            return img;
        }

        /// <summary>
        /// Renders the map to an image with the supplied resolution
        /// </summary>
        /// <param name="resolution">The resolution of the image</param>
        /// <returns>The map image</returns>
        public Image GetMap(int resolution)
        {
            Image img = new Bitmap(Size.Width, Size.Height);
            ((Bitmap)img).SetResolution(resolution, resolution);
            Graphics g = Graphics.FromImage(img);
            RenderMap(g);
            g.Dispose();
            return img;

        }

        /// <summary>
        /// Renders the map to a Metafile (Vectorimage).
        /// </summary>
        /// <remarks>
        /// A Metafile can be saved as WMF,EMF etc. or put onto the clipboard for paste in other applications such av Word-processors which will give
        /// a high quality vector image in that application.
        /// </remarks>
        /// <returns>The current map rendered as to a Metafile</returns>
        public Metafile GetMapAsMetafile()
        {
            return GetMapAsMetafile(String.Empty);
        }

        /// <summary>
        /// Renders the map to a Metafile (Vectorimage).
        /// </summary>
        /// <param name="metafileName">The filename of the metafile. If this is null or empty the metafile is not saved.</param>
        /// <remarks>
        /// A Metafile can be saved as WMF,EMF etc. or put onto the clipboard for paste in other applications such av Word-processors which will give
        /// a high quality vector image in that application.
        /// </remarks>
        /// <returns>The current map rendered as to a Metafile</returns>
        public Metafile GetMapAsMetafile(string metafileName)
        {
            Metafile metafile;
            var bm = new Bitmap(1, 1);
            using (var g = Graphics.FromImage(bm))
            {
                 var hdc = g.GetHdc();
                 using (var stream = new MemoryStream())
                 {
                     metafile = new Metafile(stream, hdc, new RectangleF(0, 0, Size.Width, Size.Height),
                                             MetafileFrameUnit.Pixel, EmfType.EmfPlusDual);

                     using (var metafileGraphics = Graphics.FromImage(metafile))
                     {
                         metafileGraphics.PageUnit = GraphicsUnit.Pixel;
                         metafileGraphics.TransformPoints(CoordinateSpace.Page, CoordinateSpace.Device,
                                                          new[] {new PointF(Size.Width, Size.Height)});

                         //Render map to metafile
                         RenderMap(metafileGraphics);
                     }

                     //Save metafile if desired
                     if (!String.IsNullOrEmpty(metafileName))
                         File.WriteAllBytes(metafileName, stream.ToArray());
                 }
                g.ReleaseHdc(hdc);
             }
            return metafile;
        }

        //ToDo: fill in the blanks
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bbox"></param>
        /// <param name="bm"></param>
        /// <param name="sourceWidth"></param>
        /// <param name="sourceHeight"></param>
        /// <param name="imageAttributes"></param>
        public void MapNewTileAvaliableHandler(TileLayer sender, Envelope bbox, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes)
        {
            var e = MapNewTileAvaliable;
            if (e != null)
                e(sender, bbox, bm,sourceWidth,sourceHeight,imageAttributes);
        }

        /// <summary>
        /// Renders the map using the provided <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="g">the <see cref="Graphics"/> object to use</param>
        /// <exception cref="ArgumentNullException">if <see cref="Graphics"/> object is null.</exception>
        /// <exception cref="InvalidOperationException">if there are no layers to render.</exception>
        public void RenderMap(Graphics g)
        {
            OnMapRendering(g);

            if (g == null)
                throw new ArgumentNullException("g", "Cannot render map with null graphics object!");

            //Pauses the timer for VariableLayer
            VariableLayerCollection.Pause = true;

            if ((Layers == null || Layers.Count == 0) && (BackgroundLayer == null || BackgroundLayer.Count == 0) && (_variableLayers == null || _variableLayers.Count == 0))
                throw new InvalidOperationException("No layers to render");

            lock (MapTransform)
            {
                g.Transform = MapTransform.Clone();
            }
            g.Clear(BackColor);
            g.PageUnit = GraphicsUnit.Pixel;


            //int srid = (Layers.Count > 0 ? Layers[0].SRID : -1); //Get the SRID of the first layer
            ILayer[] layerList;
            if (_backgroundLayers != null && _backgroundLayers.Count > 0)
            {
                layerList = new ILayer[_backgroundLayers.Count];
                _backgroundLayers.CopyTo(layerList, 0);
                foreach (ILayer layer in layerList)
                {
                    OnLayerRendering(layer, LayerCollectionType.Background);
                    if (layer.Enabled && layer.MaxVisible >= Zoom && layer.MinVisible < Zoom)
                        layer.Render(g, this);
                    OnLayerRendered(layer, LayerCollectionType.Background);
                }
            }

            if (_layers != null && _layers.Count > 0)
            {
                layerList = new ILayer[_layers.Count];
                _layers.CopyTo(layerList, 0);

                //int srid = (Layers.Count > 0 ? Layers[0].SRID : -1); //Get the SRID of the first layer
                foreach (ILayer layer in layerList)
                {
                    OnLayerRendering(layer, LayerCollectionType.Static);
                    if (layer.Enabled && layer.MaxVisible >= Zoom && layer.MinVisible < Zoom)
                        layer.Render(g, this);
                    OnLayerRendered(layer, LayerCollectionType.Static);
                }
            }

            if (_variableLayers != null && _variableLayers.Count > 0)
            {
                layerList = new ILayer[_variableLayers.Count];
                _variableLayers.CopyTo(layerList, 0);
                foreach (ILayer layer in layerList)
                {
                    if (layer.Enabled && layer.MaxVisible >= Zoom && layer.MinVisible < Zoom)
                        layer.Render(g, this);
                }
            }

#pragma warning disable 612,618
            RenderDisclaimer(g);
#pragma warning restore 612,618

            // Render all map decorations
            foreach (var mapDecoration in _decorations)
            {
                mapDecoration.Render(g, this);
            }
            //Resets the timer for VariableLayer
            VariableLayerCollection.Pause = false;

            OnMapRendered(g);
        }

        /// <summary>
        /// Fired when map is rendering
        /// </summary>
        /// <param name="g"></param>
        protected virtual void OnMapRendering(Graphics g)
        {
            var e = MapRendering;
            if (e != null) e(g);
        }

        /// <summary>
        /// Fired when Map is rendered
        /// </summary>
        /// <param name="g"></param>
        protected virtual void OnMapRendered(Graphics g)
        {
            var e = MapRendered;
            if (e != null) e(g); //Fire render event
        }

        /// <summary>
        /// Method called when starting to render <paramref name="layer"/> of <paramref name="layerCollectionType"/>. This fires the
        /// <see cref="E:SharpMap.Map.LayerRendering"/> event.
        /// </summary>
        /// <param name="layer">The layer to render</param>
        /// <param name="layerCollectionType">The collection type</param>
        protected virtual void OnLayerRendering(ILayer layer, LayerCollectionType layerCollectionType)
        {
            var e = LayerRendering;
            if (e != null) e(this, new LayerRenderingEventArgs(layer, layerCollectionType));
        }

#pragma warning disable 612,618
        /// <summary>
        /// Method called when <paramref name="layer"/> of <paramref name="layerCollectionType"/> has been rendered. This fires the
        /// <see cref="E:SharpMap.Map.LayerRendered"/> and <see cref="E:SharpMap.Map.LayerRenderedEx"/> event.
        /// </summary>
        /// <param name="layer">The layer to render</param>
        /// <param name="layerCollectionType">The collection type</param>
        protected virtual void OnLayerRendered(ILayer layer, LayerCollectionType layerCollectionType)
        {
            var e = LayerRendered;
#pragma warning restore 612,618
            if (e != null)
            {
                e(this, EventArgs.Empty);
            }

            var eex = LayerRenderedEx;
            if (eex != null)
            {
                eex(this, new LayerRenderingEventArgs(layer, layerCollectionType));
            }
        }


        /// <summary>
        /// Renders the map using the provided <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="g">the <see cref="Graphics"/> object to use</param>
        /// <param name="layerCollectionType">the <see cref="LayerCollectionType"/> to use</param>
        /// <exception cref="ArgumentNullException">if <see cref="Graphics"/> object is null.</exception>
        /// <exception cref="InvalidOperationException">if there are no layers to render.</exception>
        public void RenderMap(Graphics g, LayerCollectionType layerCollectionType)
        {
            RenderMap(g, layerCollectionType, true, false);
        }

        /// <summary>
        /// Renders the map using the provided <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="g">the <see cref="Graphics"/> object to use</param>
        /// <param name="layerCollectionType">the <see cref="LayerCollectionType"/> to use</param>
        /// <param name="drawMapDecorations">Set whether to draw map decorations on the map (if such are set)</param>
        /// <param name="drawTransparent">Set wether to draw with transparent background or with BackColor as background</param>
        /// <exception cref="ArgumentNullException">if <see cref="Graphics"/> object is null.</exception>
        /// <exception cref="InvalidOperationException">if there are no layers to render.</exception>
        public void RenderMap(Graphics g, LayerCollectionType layerCollectionType, bool drawMapDecorations, bool drawTransparent)
        {
            if (g == null)
                throw new ArgumentNullException("g", "Cannot render map with null graphics object!");

            VariableLayerCollection.Pause = true;

            LayerCollection lc = null;
            switch (layerCollectionType)
            {
                case LayerCollectionType.Static:
                    lc = Layers;
                    break;
                case LayerCollectionType.Variable:
                    lc = VariableLayers;
                    break;
                case LayerCollectionType.Background:
                    lc = BackgroundLayer;
                    break;
            }

            if (lc== null || lc.Count == 0)
                throw new InvalidOperationException("No layers to render");

            Matrix transform = g.Transform;
            lock (MapTransform)
            {
                g.Transform = MapTransform.Clone();
            }
            if (!drawTransparent)
                g.Clear(BackColor);

            g.PageUnit = GraphicsUnit.Pixel;


            ILayer[] layerList = new ILayer[lc.Count];
            lc.CopyTo(layerList, 0);

            foreach (ILayer layer in layerList)
            {
                if (layer.Enabled && layer.MaxVisible >= Zoom && layer.MinVisible < Zoom)
                    layer.Render(g, this);
            }

            if (drawTransparent)
                

            g.Transform = transform;
            if (layerCollectionType == LayerCollectionType.Static)
            {
#pragma warning disable 612,618
                RenderDisclaimer(g);
#pragma warning restore 612,618
                if (drawMapDecorations)
                {
                    foreach (var mapDecoration in Decorations)
                    {
                        mapDecoration.Render(g, this);
                    }
                }
            }



            VariableLayerCollection.Pause = false;

        }

        /// <summary>
        /// Returns a cloned copy of this map-object.
        /// Layers are not cloned. The same instances are referenced from the cloned copy as from the original.
        /// The property <see cref="DisposeLayersOnDispose"/> is however false on this object (which prevents layers beeing disposed and then not usable from the original map)
        /// </summary>
        /// <returns>Instance of <see cref="Map"/></returns>
        public Map Clone()
        {
            Map clone = null;
            lock (MapTransform)
            {
                clone = new Map()
                {
                    BackColor = BackColor,
#pragma warning disable 612,618
                    Disclaimer = Disclaimer,
                    DisclaimerLocation = DisclaimerLocation,
#pragma warning restore 612,618
                    MaximumZoom = MaximumZoom,
                    MinimumZoom = MinimumZoom,
                    PixelAspectRatio = PixelAspectRatio,
                    Zoom = Zoom,
                    DisposeLayersOnDispose = false,
                    SRID = SRID
                };

#pragma warning disable 612,618
                if (DisclaimerFont != null)
                    clone.DisclaimerFont = (Font)DisclaimerFont.Clone();
#pragma warning restore 612,618
                if (MapTransform != null)
                    clone.MapTransform = MapTransform.Clone();
                if (!Size.IsEmpty)
                    clone.Size = new Size(Size.Width, Size.Height);
                if (Center != null)
                    clone.Center = (Coordinate)Center.Clone();

            }

            if (BackgroundLayer != null)
                clone.BackgroundLayer.AddCollection(BackgroundLayer.Clone());

            for (int i = 0; i < Decorations.Count; i++)
                clone.Decorations.Add(Decorations[i]);

            if (Layers != null)
                clone.Layers.AddCollection(Layers.Clone());

            if (VariableLayers != null)
                clone.VariableLayers.AddCollection(VariableLayers.Clone());

            return clone;
        }

        [Obsolete]
        private void RenderDisclaimer(Graphics g)
        {
            //Disclaimer
            if (!String.IsNullOrEmpty(_disclaimer))
            {
                var size = VectorRenderer.SizeOfString(g, _disclaimer, _disclaimerFont);
                size.Width = (Single)Math.Ceiling(size.Width);
                size.Height = (Single)Math.Ceiling(size.Height);
                StringFormat sf;
                switch (DisclaimerLocation)
                {
                    case 0: //Right-Bottom
                        sf = new StringFormat();
                        sf.Alignment = StringAlignment.Far;
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black,
                            g.VisibleClipBounds.Width,
                            g.VisibleClipBounds.Height - size.Height - 2, sf);
                        break;
                    case 1: //Right-Top
                        sf = new StringFormat();
                        sf.Alignment = StringAlignment.Far;
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black,
                            g.VisibleClipBounds.Width, 0f, sf);
                        break;
                    case 2: //Left-Top
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black, 0f, 0f);
                        break;
                    case 3://Left-Bottom
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black, 0f,
                            g.VisibleClipBounds.Height - size.Height - 2);
                        break;
                }
            }
        }

        /// <summary>
        /// Returns an enumerable for all layers containing the search parameter in the LayerName property
        /// </summary>
        /// <param name="layername">Search parameter</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<ILayer> FindLayer(string layername)
        {
            foreach (ILayer l in Layers)
                if (l.LayerName.Contains(layername))
                    yield return l;
        }

        /// <summary>
        /// Returns a layer by its name
        /// </summary>
        /// <param name="name">Name of layer</param>
        /// <returns>Layer</returns>
        public ILayer GetLayerByName(string name)
        {
            ILayer lay  = null;
            if (Layers != null)
            {
                lay = Layers.GetLayerByName(name);
            }
            if (lay == null && BackgroundLayer != null)
            {
                lay = BackgroundLayer.GetLayerByName(name);
            }
            if (lay == null && VariableLayers != null)
            {
                lay = VariableLayers.GetLayerByName(name);
            }

            return lay;
        }

        /// <summary>
        /// Zooms to the extents of all layers
        /// </summary>
        public void ZoomToExtents()
        {
            ZoomToBox(GetExtents());
        }

        /// <summary>
        /// Zooms the map to fit a bounding box
        /// </summary>
        /// <remarks>
        /// NOTE: If the aspect ratio of the box and the aspect ratio of the mapsize
        /// isn't the same, the resulting map-envelope will be adjusted so that it contains
        /// the bounding box, thus making the resulting envelope larger!
        /// </remarks>
        /// <param name="bbox"></param>
        public void ZoomToBox(Envelope bbox)
        {
            if (bbox != null && !bbox.IsNull)
            {
                //Ensure aspect ratio
                var resX = Size.Width == 0 ? double.MaxValue : bbox.Width / Size.Width;
                var resY = Size.Height == 0 ? double.MaxValue : bbox.Height / Size.Height;
                var zoom = bbox.Width;
                if (resY > resX && resX > 0)
                {
                    zoom *= resY / resX;
                }

                var center = new Coordinate(bbox.Centre);

                zoom = _mapViewportGuard.VerifyZoom(zoom, center);
                var changed = false;
                if (zoom != _zoom)
                {
                    _zoom = zoom;
                    changed = true;
                }

                if (!center.Equals2D(_center))
                {
                    _center = center;
                    changed = true;
                }

                if (changed && MapViewOnChange != null)
                    MapViewOnChange();
            }
        }

        /// <summary>
        /// Converts a point from world coordinates to image coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether MapTransform should be taken into account</param>
        /// <returns>Point in image coordinates</returns>
        public PointF WorldToImage(Coordinate p, bool careAboutMapTransform)
        {
            var pTmp = Transform.WorldtoMap(p, this);
            lock (MapTransform)
            {
                if (careAboutMapTransform && !MapTransform.IsIdentity)
                {
                    var pts = new[] { pTmp };
                    MapTransform.TransformPoints(pts);
                    pTmp = pts[0];
                }
            }
            return pTmp;
        }

        /// <summary>
        /// Converts a point from world coordinates to image coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <returns>Point in image coordinates</returns>
        public PointF WorldToImage(Point p)
        {
            return WorldToImage(p, false);
        }

        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in image coordinates</param>
        /// <returns>Point in world coordinates</returns>
        public Point ImageToWorld(PointF p)
        {
            return ImageToWorld(p, false);
        }
        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in image coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether MapTransform should be taken into account</param>
        /// <returns>Point in world coordinates</returns>
        public Point ImageToWorld(PointF p, bool careAboutMapTransform)
        {
            lock (MapTransform)
            {
                if (careAboutMapTransform && !MapTransform.IsIdentity)
                {
                    var pts = new[] { p };
                    MapTransformInverted.TransformPoints(pts);
                    p = pts[0];
                }
            }
            return Transform.MapToWorld(p, this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the SRID of the map
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set
            {
                if (_srid == value)
                    return;
                _srid = value;
                Factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(_srid);
            }
        }

        /// <summary>
        /// Factory used to create geometries
        /// </summary>
        public IGeometryFactory Factory { get; private set; }

        /// <summary>
        /// List of all map decorations
        /// </summary>
        public IList<IMapDecoration> Decorations
        {
            get { return _decorations; }
        }

        /// <summary>
        /// Gets the extents of the current map based on the current zoom, center and mapsize
        /// </summary>
        public Envelope Envelope
        {
            get
            {
                if (double.IsNaN(MapHeight) || double.IsInfinity(MapHeight))
                    return new Envelope(0, 0, 0, 0);

                var ll = new Coordinate(Center.X - Zoom * .5, Center.Y - MapHeight * .5);
                var ur = new Coordinate(Center.X + Zoom * .5, Center.Y + MapHeight * .5);
                
                var ptfll = WorldToImage(ll, true);
                ptfll = new PointF(Math.Abs(ptfll.X), Math.Abs(Size.Height - ptfll.Y));
                if (!ptfll.IsEmpty)
                {
                    ll.X = ll.X - ptfll.X * PixelWidth;
                    ll.Y = ll.Y - ptfll.Y * PixelHeight;
                    ur.X = ur.X + ptfll.X * PixelWidth;
                    ur.Y = ur.Y + ptfll.Y * PixelHeight;
                }
                return new Envelope(ll, ur);    
            }
        }

        /// <summary>
        /// Using the <see cref="MapTransform"/> you can alter the coordinate system of the map rendering.
        /// This makes it possible to rotate or rescale the image, for instance to have another direction than north upwards.
        /// </summary>
        /// <example>
        /// Rotate the map output 45 degrees around its center:
        /// <code lang="C#">
        /// System.Drawing.Drawing2D.Matrix maptransform = new System.Drawing.Drawing2D.Matrix(); //Create transformation matrix
        ///	maptransform.RotateAt(45,new PointF(myMap.Size.Width/2,myMap.Size.Height/2)); //Apply 45 degrees rotation around the center of the map
        ///	myMap.MapTransform = maptransform; //Apply transformation to map
        /// </code>
        /// </example>
        public Matrix MapTransform
        {
            get { return _mapTransform; }
            set
            {
                _mapTransform = value;
                if (_mapTransform.IsInvertible)
                {
                    MapTransformInverted = value.Clone();
                    MapTransformInverted.Invert();
                }
                else
                    MapTransformInverted.Reset();
            }
        }

        /// <summary>
        /// A collection of layers. The first layer in the list is drawn first, the last one on top.
        /// </summary>
        public LayerCollection Layers
        {
            get { return _layers; }
        }

        /// <summary>
        /// Collection of background Layers
        /// </summary>
        public LayerCollection BackgroundLayer
        {
            get { return _backgroundLayers; }
        }

        /// <summary>
        /// A collection of layers. The first layer in the list is drawn first, the last one on top.
        /// </summary>
        public VariableLayerCollection VariableLayers
        {
            get { return _variableLayers; }
        }

        /// <summary>
        /// Map background color (defaults to transparent)
        /// </summary>
        public Color BackColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                if (MapViewOnChange != null)
                    MapViewOnChange();
            }
        }

        /// <summary>
        /// Center of map in WCS
        /// </summary>
        public Point Center
        {
            get { return _center; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                var newZoom = _zoom;
                var newCenter = new Coordinate(value);

                newZoom = _mapViewportGuard.VerifyZoom(newZoom, newCenter);

                var changed = false;
                if (newZoom != _zoom)
                {
                    _zoom = newZoom;
                    changed = true;
                }

                if (!newCenter.Equals2D(_center))
                {
                    _center = newCenter;
                    changed = true;
                }
                
                if (changed && MapViewOnChange != null)
                    MapViewOnChange();
            }
        }

        /// <summary>
        /// Gets or sets the zoom level of map.
        /// </summary>
        /// <remarks>
        /// <para>The zoom level corresponds to the width of the map in WCS units.</para>
        /// <para>A zoomlevel of 0 will result in an empty map being rendered, but will not throw an exception</para>
        /// </remarks>
        public double Zoom
        {
            get { return _zoom; }
            set
            {
                var newCenter = new Coordinate(_center);
                value = _mapViewportGuard.VerifyZoom(value, newCenter);

                if (value.Equals(_zoom))
                    return;

                _zoom = value;
                if (!newCenter.Equals2D(_center))
                    _center = newCenter;

                if (MapViewOnChange != null)
                    MapViewOnChange();
            }
        }

        /// <summary>
        /// Get Returns the size of a pixel in world coordinate units
        /// </summary>
        public double PixelSize
        {
            get { return Zoom/Size.Width; }
        }

        /// <summary>
        /// Returns the width of a pixel in world coordinate units.
        /// </summary>
        /// <remarks>The value returned is the same as <see cref="PixelSize"/>.</remarks>
        public double PixelWidth
        {
            get { return PixelSize; }
        }

        /// <summary>
        /// Returns the height of a pixel in world coordinate units.
        /// </summary>
        /// <remarks>The value returned is the same as <see cref="PixelSize"/> unless <see cref="PixelAspectRatio"/> is different from 1.</remarks>
        public double PixelHeight
        {
            get { return PixelSize * _mapViewportGuard.PixelAspectRatio; }
        }

        /// <summary>
        /// Gets or sets the aspect-ratio of the pixel scales. A value less than 
        /// 1 will make the map stretch upwards, and larger than 1 will make it smaller.
        /// </summary>
        /// <exception cref="ArgumentException">Throws an argument exception when value is 0 or less.</exception>
        public double PixelAspectRatio
        {
            get { return _mapViewportGuard.PixelAspectRatio; }
            set
            {
                _mapViewportGuard.PixelAspectRatio = value;
            }
        }

        /// <summary>
        /// Height of map in world units
        /// </summary>
        /// <returns></returns>
        public double MapHeight
        {
            get { return (Zoom*Size.Height)/Size.Width*PixelAspectRatio; }
        }

        /// <summary>
        /// Size of output map
        /// </summary>
        public Size Size
        {
            get { return _mapViewportGuard.Size; }
            set { _mapViewportGuard.Size = value; }
        }

        /// <summary>
        /// Minimum zoom amount allowed
        /// </summary>
        public double MinimumZoom
        {
            get { return _mapViewportGuard.MinimumZoom; }
            set
            {
                _mapViewportGuard.MinimumZoom = value;
            }
        }

        /// <summary>
        /// Maximum zoom amount allowed
        /// </summary>
        public double MaximumZoom
        {
            get { return _mapViewportGuard.MaximumZoom; }
            set
            {
                _mapViewportGuard.MaximumZoom = value;
            }
        }

        /// <summary>
        /// Gets the extents of the map based on the extents of all the layers in the layers collection
        /// </summary>
        /// <returns>Full map extents</returns>
        public Envelope GetExtents()
        {
            if (!_mapViewportGuard.MaximumExtents.IsNull)
                return MaximumExtents;

            if ((Layers == null || Layers.Count == 0) &&
                (VariableLayers == null || VariableLayers.Count == 0) &&
                (BackgroundLayer == null || BackgroundLayer.Count == 0))
                throw (new InvalidOperationException("No layers to zoom to"));

            Envelope bbox = null;

            ExtendBoxForCollection(Layers, ref bbox);
            ExtendBoxForCollection(VariableLayers, ref bbox);
            ExtendBoxForCollection(BackgroundLayer, ref bbox);

            return bbox;
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum visible extent
        /// </summary>
        public Envelope MaximumExtents
        {
            get { return _mapViewportGuard.MaximumExtents; }
            set { _mapViewportGuard.MaximumExtents = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="MaximumExtents"/> should be enforced or not.
        /// </summary>
        public bool EnforceMaximumExtents
        {
            get { return _mapViewportGuard.EnforceMaximumExtents; }
            set { _mapViewportGuard.EnforceMaximumExtents = value; }
        }

        private static void ExtendBoxForCollection(LayerCollection layersCollection, ref Envelope bbox)
        {
            foreach (var l in layersCollection)
            {
                
                //Tries to get bb. Fails on some specific shapes and Mercator projects (World.shp)
                Envelope bb;
                try
                {
                    bb = l.Envelope;
                }
                catch (Exception)
                {
                    bb = new Envelope(new Coordinate(-20037508.342789, -20037508.342789), new Coordinate(20037508.342789, 20037508.342789));
                }

                if (bbox == null)
                    bbox = bb;
                else
                {
                    //FB: bb can be null on empty layers (e.g. temporary working layers with no data objects)
                    if (bb != null)
                        bbox.ExpandToInclude(bb);
                }

            }
        }

        #endregion

        #region Disclaimer

        private String _disclaimer;
        /// <summary>
        /// Copyright notice to be placed on the map
        /// </summary>
        [Obsolete("Use Disclaimer as MapDecoration instead!")]
        public String Disclaimer
        {
            get { return _disclaimer; }
            set {
                //only set disclaimer if not already done
                if (String.IsNullOrEmpty(_disclaimer))
                {
                    _disclaimer = value;
                    //Ensure that Font for disclaimer is set
                    if (_disclaimerFont == null)
                        _disclaimerFont = new Font(FontFamily.GenericSansSerif, 8f);
                }
            }
        }        

        private Font _disclaimerFont;
        /// <summary>
        /// Font to use for the Disclaimer
        /// </summary>
        [Obsolete("Use Disclaimer as MapDecoration instead!")]
        public Font DisclaimerFont
        {
            get { return _disclaimerFont; }
            set
            {
                if (value == null) return;
                _disclaimerFont = value;
            }
        }

        private Int32 _disclaimerLocation;

        /// <summary>
        /// Location for the disclaimer
        /// 2|1
        /// -+-
        /// 3|0
        /// </summary>
        [Obsolete("Use Disclaimer as MapDecoration instead!")]
        public Int32 DisclaimerLocation
        {
            get { return _disclaimerLocation; }
            set { _disclaimerLocation = value%4; }
        }


        #endregion
    }

    /// <summary>
    /// Layer rendering event arguments class
    /// </summary>
    public class LayerRenderingEventArgs : EventArgs
    {
        /// <summary>
        /// The layer that is being or has been rendered
        /// </summary>
        public readonly ILayer Layer;

        /// <summary>
        /// The layer collection type the layer belongs to.
        /// </summary>
        public readonly LayerCollectionType LayerCollectionType;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layer">The layer that is being or has been rendered</param>
        /// <param name="layerCollectionType">The layer collection type the layer belongs to.</param>
        public LayerRenderingEventArgs(ILayer layer, LayerCollectionType layerCollectionType)
        {
            Layer = layer;
            LayerCollectionType = layerCollectionType;
        }
    }

}
