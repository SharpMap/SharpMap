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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GeoAPI.Geometries;
using NetTopologySuite;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Decoration;
using SharpMap.Styles;
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
        /// <summary>
        /// Method to invoke the static constructor of this class
        /// </summary>
        public static void Configure()
        {
            // Methods sole purpose is to get the static constructor executed
        }

        /// <summary>
        /// Static constructor. Needed to get <see cref="GeoAPI.GeometryServiceProvider.Instance"/> set.
        /// </summary>
        static Map()
        {
            try
            {
                _logger.Debug("Trying to get GeoAPI.GeometryServiceProvider.Instance");
                var instance = GeoAPI.GeometryServiceProvider.Instance;
                if (instance == null)
                {
                    _logger.Debug("Returned null");
                    throw new InvalidOperationException();
                }
            }
            catch (InvalidOperationException)
            {
                _logger.Debug("Loading NetTopologySuite");
                Assembly.Load("NetTopologySuite");
                _logger.Debug("Loaded NetTopologySuite");
                _logger.Debug("Trying to get GeoAPI.GeometryServiceProvider.Instance");
                var instance = GeoAPI.GeometryServiceProvider.Instance;
                if (instance == null)
                {
                    _logger.Debug("Returned null");
                    throw new InvalidOperationException();
                }
            }

            // The following code did not seem to work in all cases.
            /*            
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                _logger.Debug("In design mode");
                Trace.WriteLine("In design mode");
                // We have to do this initialization with reflection due to the fact that NTS can reference an older version of GeoAPI and redirection 
                // is not available at design time..
                var ntsAssembly = Assembly.Load("NetTopologySuite");
                _logger.Debug("Loaded NetTopologySuite");
                Trace.WriteLine("Loaded NetTopologySuite");
                try
                {
                    _logger.Debug("Trying to access GeoAPI.GeometryServiceProvider.Instance");
                    Trace.WriteLine("Trying to access GeoAPI.GeometryServiceProvider.Instance");
                    if (GeoAPI.GeometryServiceProvider.Instance == null)
                    {
                        _logger.Debug("Returned null, setting it to default");
                        Trace.WriteLine("Returned null, setting it to default");
                        var ntsApiGeometryServices = ntsAssembly.GetType("NetTopologySuite.NtsGeometryServices");
                        GeoAPI.GeometryServiceProvider.Instance =
                            ntsApiGeometryServices.GetProperty("Instance").GetValue(null, null) as
                                GeoAPI.IGeometryServices;
                    }
                }

                catch (InvalidOperationException)
                {
                    _logger.Debug("InvalidOperationException thrown, setting it to default");
                    Trace.WriteLine("InvalidOperationException thrown, setting it to default");
                    var ntsApiGeometryServices = ntsAssembly.GetType("NetTopologySuite.NtsGeometryServices");
                    GeoAPI.GeometryServiceProvider.Instance =
                        ntsApiGeometryServices.GetProperty("Instance").GetValue(null, null) as
                            GeoAPI.IGeometryServices;
                }
                _logger.Debug("Exiting design mode handling");
                Trace.WriteLine("Exiting design mode handling");
            }
             */
        }

        static readonly ILog _logger = LogManager.GetLogger(typeof(Map));

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
#pragma warning disable 169
        // both fields redundant, but unable to remove without violating serialization
        private Matrix _mapTransform; 
        private Matrix _mapTransformInverted;
#pragma warning restore 169
        private readonly object _lockMapTransform = new object();
        private readonly object _lockMapTransformInverted = new object();
        private float[] _mapTransformElements; 
        private float[] _mapTransformInvertedElements;
        
        private readonly MapViewPortGuard _mapViewportGuard;
        private readonly Dictionary<object, List<ILayer>> _layersPerGroup = new Dictionary<object, List<ILayer>>();
        private ObservableCollection<ILayer> _replacingCollection;
        private Guid _id = Guid.NewGuid();

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

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                Factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(_srid);
            }
            _layers = new LayerCollection();
            _layersPerGroup.Add(_layers, new List<ILayer>());
            _backgroundLayers = new LayerCollection();
            _layersPerGroup.Add(_backgroundLayers, new List<ILayer>());
            _variableLayers = new VariableLayerCollection(_layers);
            _layersPerGroup.Add(_variableLayers, new List<ILayer>());
            BackColor = Color.Transparent;
            var matrix = new Matrix();
            _mapTransformElements = matrix.Elements;
            _mapTransformInvertedElements = matrix.Elements;
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
            _backgroundLayers.CollectionChanged += OnLayersCollectionChanged;
            _layers.CollectionChanged += OnLayersCollectionChanged;
        }

        /// <summary>
        /// Event handler to intercept when a new ITileAsymclayer is added to the Layers List and associate the MapNewTile Handler Event
        /// </summary>
        private void OnLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                var cloneList = _layersPerGroup[sender];
                IterUnHookEvents(sender, cloneList);
            }
            if (e.Action == NotifyCollectionChangedAction.Replace || e.Action == NotifyCollectionChangedAction.Remove)
            {
                IterUnHookEvents(sender, e.OldItems.Cast<ILayer>());
            }
            if (e.Action == NotifyCollectionChangedAction.Replace || e.Action == NotifyCollectionChangedAction.Add)
            {
                IterWireEvents(sender, e.NewItems.Cast<ILayer>());
            }

        }

        private void IterWireEvents(object owner, IEnumerable<ILayer> layers)
        {
            foreach (var layer in layers)
            {
                _layersPerGroup[owner].Add(layer);

                var tileAsyncLayer = layer as ITileAsyncLayer;
                if (tileAsyncLayer != null)
                {
                    WireTileAsyncEvents(tileAsyncLayer);
                }

                var group = layer as LayerGroup;
                if (group != null)
                {
                    group.LayersChanging += OnLayerGroupCollectionReplaching;
                    group.LayersChanged += OnLayerGroupCollectionReplached;

                    var nestedList = group.Layers;
                    if (group.Layers != null)
                    {
                        group.Layers.CollectionChanged += OnLayersCollectionChanged;
                        _layersPerGroup.Add(nestedList, new List<ILayer>());
                    }
                    else
                    {
                        _layersPerGroup.Add(nestedList, new List<ILayer>());
                    }

                    IterWireEvents(nestedList, nestedList);
                }
            }
        }

        private void IterUnHookEvents(object owner, IEnumerable<ILayer> layers)
        {
            var toBeRemoved = new List<ILayer>();

            foreach (var layer in layers)
            {
                toBeRemoved.Add(layer);

                var tileAsyncLayer = layer as ITileAsyncLayer;
                if (tileAsyncLayer != null)
                {
                    UnhookTileAsyncEvents(tileAsyncLayer);
                }

                var group = layer as LayerGroup;
                if (group != null)
                {
                    group.LayersChanging -= OnLayerGroupCollectionReplaching;
                    group.LayersChanged -= OnLayerGroupCollectionReplached;

                    var nestedList = group.Layers;

                    if (nestedList != null)
                    {
                        nestedList.CollectionChanged -= OnLayersCollectionChanged;

                        IterUnHookEvents(nestedList, nestedList);

                        _layersPerGroup.Remove(nestedList);
                    }
                }
            }

            var clonedList = _layersPerGroup[owner];
            toBeRemoved.ForEach(layer => clonedList.Remove(layer));
        }

        private void OnLayerGroupCollectionReplached(object sender, EventArgs eventArgs)
        {
            var layerGroup = (LayerGroup)sender;

            var newCollection = layerGroup.Layers;

            IterUnHookEvents(_replacingCollection, _replacingCollection);
            _layersPerGroup.Remove(_replacingCollection);
            _replacingCollection.CollectionChanged -= OnLayersCollectionChanged;

            if (newCollection != null)
            {
                IterWireEvents(newCollection, newCollection);

                _layersPerGroup.Add(newCollection, new List<ILayer>(newCollection));

                newCollection.CollectionChanged += OnLayersCollectionChanged;
            }
        }

        private void OnLayerGroupCollectionReplaching(object sender, EventArgs eventArgs)
        {
            var layerGroup = (LayerGroup)sender;

            _replacingCollection = layerGroup.Layers;
        }

        private void layer_DownloadProgressChanged(int tilesRemaining)
        {
            if (tilesRemaining <= 0)
            {
                OnRefreshNeeded(EventArgs.Empty);
            }
        }

        private void WireTileAsyncEvents(ITileAsyncLayer tileAsyncLayer)
        {
            if (tileAsyncLayer.OnlyRedrawWhenComplete)
            {
                tileAsyncLayer.DownloadProgressChanged += layer_DownloadProgressChanged;
            }
            else
            {
                tileAsyncLayer.MapNewTileAvaliable += MapNewTileAvaliableHandler;
            }
        }

        private void UnhookTileAsyncEvents(ITileAsyncLayer tileAsyncLayer)
        {
            tileAsyncLayer.DownloadProgressChanged -= layer_DownloadProgressChanged;
            tileAsyncLayer.MapNewTileAvaliable -= MapNewTileAvaliableHandler;
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
                    foreach (IDisposable disposable in Layers.OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                }
                if (BackgroundLayer != null)
                {
                    foreach (IDisposable disposable in BackgroundLayer.OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                }
                if (VariableLayers != null)
                {
                    foreach (IDisposable layer in VariableLayers.OfType<IDisposable>())
                        layer.Dispose();
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
                                                         new[] { new PointF(Size.Width, Size.Height) });

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
        public void MapNewTileAvaliableHandler(ITileAsyncLayer sender, Envelope bbox, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes)
        {
            var e = MapNewTileAvaliable;
            if (e != null)
                e(sender, bbox, bm, sourceWidth, sourceHeight, imageAttributes);
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
            _variableLayers.Pause = true;

            if ((Layers == null || Layers.Count == 0) && (BackgroundLayer == null || BackgroundLayer.Count == 0) && (_variableLayers == null || _variableLayers.Count == 0))
                throw new InvalidOperationException("No layers to render");

            g.Transform = MapTransform;
            
            var mvp = (MapViewport) this;

            g.Clear(BackColor);
            g.PageUnit = GraphicsUnit.Pixel;

            double zoom = mvp.Zoom;
            double scale = double.NaN; //will be resolved if needed

            ILayer[] layerList;
            if (_backgroundLayers != null && _backgroundLayers.Count > 0)
            {
                layerList = new ILayer[_backgroundLayers.Count];
                _backgroundLayers.CopyTo(layerList, 0);
                foreach (ILayer layer in layerList)
                {
                    if (layer.VisibilityUnits == VisibilityUnits.Scale && double.IsNaN(scale))
                    {
                        scale = mvp.GetMapScale((int)g.DpiX);
                    }
                    double visibleLevel = layer.VisibilityUnits == VisibilityUnits.ZoomLevel ? zoom : scale;

                    OnLayerRendering(layer, LayerCollectionType.Background);
                    if (layer.Enabled)
                    {
                        if (layer.MaxVisible >= visibleLevel && layer.MinVisible < visibleLevel)
                        {
                            LayerCollectionRenderer.RenderLayer(layer, g, mvp);
                        }
                    }
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
                    if (layer.VisibilityUnits == VisibilityUnits.Scale && double.IsNaN(scale))
                    {
                        scale = mvp.GetMapScale((int)g.DpiX);
                    }
                    double visibleLevel = layer.VisibilityUnits == VisibilityUnits.ZoomLevel ? zoom : scale;
                    OnLayerRendering(layer, LayerCollectionType.Static);
                    if (layer.Enabled && layer.MaxVisible >= visibleLevel && layer.MinVisible < visibleLevel)
                        LayerCollectionRenderer.RenderLayer(layer, g, mvp);
                        
                    OnLayerRendered(layer, LayerCollectionType.Static);
                }
            }

            if (_variableLayers != null && _variableLayers.Count > 0)
            {
                layerList = new ILayer[_variableLayers.Count];
                _variableLayers.CopyTo(layerList, 0);
                foreach (ILayer layer in layerList)
                {
                    if (layer.VisibilityUnits == VisibilityUnits.Scale && double.IsNaN(scale))
                    {
                        scale = mvp.GetMapScale((int)g.DpiX);
                    }
                    double visibleLevel = layer.VisibilityUnits == VisibilityUnits.ZoomLevel ? zoom : scale;
                    if (layer.Enabled && layer.MaxVisible >= visibleLevel && layer.MinVisible < visibleLevel)
                        LayerCollectionRenderer.RenderLayer(layer, g, mvp);
                    
                    OnLayerRendered(layer, LayerCollectionType.Variable);
                }
            }

#pragma warning disable 612,618
            RenderDisclaimer(g);
#pragma warning restore 612,618

            // Render all map decorations
            foreach (var mapDecoration in _decorations)
            {
                mapDecoration.Render(g, mvp);
            }
            //Resets the timer for VariableLayer
            _variableLayers.Pause = false;

            OnMapRendered(g);
        }

        /// <summary>
        /// Fires the RefreshNeeded event.
        /// </summary>
        /// <param name="e">EventArgs argument.</param>
        protected virtual void OnRefreshNeeded(EventArgs e)
        {
            var handler = RefreshNeeded;
            if (handler != null)
                handler(this, e);
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
        /// <param name="drawTransparent">Set whether to draw with transparent background or with BackColor as background</param>
        /// <exception cref="ArgumentNullException">if <see cref="Graphics"/> object is null.</exception>
        /// <exception cref="InvalidOperationException">if there are no layers to render.</exception>
        public void RenderMap(Graphics g, LayerCollectionType layerCollectionType, bool drawMapDecorations, bool drawTransparent)
        {
            if (g == null)
                throw new ArgumentNullException("g", "Cannot render map with null graphics object!");

            _variableLayers.Pause = true;

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

            if (lc == null || lc.Count == 0)
                throw new InvalidOperationException("No layers to render");

            var origTransform = g.Transform;

            g.Transform = MapTransform;

            var mvp = (MapViewport) this;

            if (!drawTransparent)
                g.Clear(BackColor);

            g.PageUnit = GraphicsUnit.Pixel;

            //LayerCollectionRenderer.AllowParallel = layerCollectionType == LayerCollectionType.Static;
            using (var lcr = new LayerCollectionRenderer(lc))
            {
                lcr.Render(g, mvp, layerCollectionType == LayerCollectionType.Static);
            }

            /*
            var layerList = new ILayer[lc.Count];
            lc.CopyTo(layerList, 0);

            foreach (ILayer layer in layerList)
            {
                if (layer.Enabled && layer.MaxVisible >= Zoom && layer.MinVisible < Zoom)
                    layer.Render(g, this);
            }

             */

            if (drawTransparent)
                g.Transform = origTransform;

            if (layerCollectionType == LayerCollectionType.Static)
            {
#pragma warning disable 612,618
                RenderDisclaimer(g);
#pragma warning restore 612,618
                if (drawMapDecorations)
                {
                    foreach (var mapDecoration in Decorations)
                    {
                        mapDecoration.Render(g, mvp);
                    }
                }
            }



            _variableLayers.Pause = false;

        }

        /// <summary>
        /// Returns a cloned copy of this map-object.
        /// Layers are not cloned. The same instances are referenced from the cloned copy as from the original.
        /// The property <see cref="DisposeLayersOnDispose"/> is however false on this object (which prevents layers beeing disposed and then not usable from the original map)
        /// </summary>
        /// <returns>Instance of <see cref="Map"/></returns>
        public Map Clone()
        {
            Map clone;
            lock (_lockMapTransform)
            {
                clone = new Map
                {
                    BackColor = BackColor,
#pragma warning disable 612,618
                    Disclaimer = Disclaimer,
                    DisclaimerLocation = DisclaimerLocation,
#pragma warning restore 612,618
                    MaximumZoom = MaximumZoom,
                    MinimumZoom = MinimumZoom,
                    MaximumExtents = MaximumExtents,
                    EnforceMaximumExtents = EnforceMaximumExtents,
                    PixelAspectRatio = PixelAspectRatio,
                    Zoom = Zoom,
                    DisposeLayersOnDispose = false,
                    SRID = SRID,
                    _id = ID
                };

#pragma warning disable 612,618
                if (DisclaimerFont != null)
                    clone.DisclaimerFont = (Font)DisclaimerFont.Clone();
#pragma warning restore 612,618
                if (MapTransform != null)
                    clone.MapTransform = MapTransform;
                if (!Size.IsEmpty)
                    clone.Size = new Size(Size.Width, Size.Height);
                if (Center != null)
                    clone.Center = Center.Copy();

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
            return Layers.Where(l => l.LayerName.Contains(layername));
        }

        /// <summary>
        /// Returns a layer by its name
        /// </summary>
        /// <param name="name">Name of layer</param>
        /// <returns>Layer</returns>
        public ILayer GetLayerByName(string name)
        {
            ILayer lay = null;
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
            ZoomToBox(GetExtents(), true);
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
        /// <param name="careAboutTransform">True if any map rotation should be taken into account (eg ZoomToExtents).
        /// False if rotation has already been accounted for (eg Zoom prev / next stack, or non-rotated views)</param>
        public void ZoomToBox(Envelope bbox, bool careAboutTransform = false)
        {
            if (bbox == null || bbox.IsNull) return;

            if (careAboutTransform && !MapTransformRotation.Equals(0f))
            {
                // adjust box for rotated views to ensure it is fully visible at maximum possible zoom
                var rad = NetTopologySuite.Utilities.Degrees.ToRadians(MapTransformRotation);
                var newWidth = bbox.Width * Math.Abs(Math.Cos(rad)) +
                               bbox.Height * Math.Abs(Math.Sin(rad));
                var newHeight = bbox.Width * Math.Abs(Math.Sin(rad)) +
                                bbox.Height * Math.Abs(Math.Cos(rad));
                bbox = new Envelope(
                    bbox.Centre.X - newWidth * 0.5,
                    bbox.Centre.X + newWidth * 0.5,
                    bbox.Centre.Y - newHeight * 0.5,
                    bbox.Centre.Y + newHeight * 0.5);
            }
            
            //Ensure aspect ratio
            var resX = Size.Width == 0 ? double.MaxValue : bbox.Width / Size.Width;
            var resY = Size.Height == 0 ? double.MaxValue : bbox.Height / Size.Height;
            var zoom = bbox.Width;
            if (resY > resX && resX > 0)
                zoom *= resY / resX;

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

        /// <summary>
        /// Converts an array of world coordinates to image coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="coordinates">Coordinate array in world coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. True for typical coordinate calcs,
        /// False when rendering to image as the Graphics object has already applied the MapTransform</param>
        /// <returns>PointF array in image coordinates</returns>
        public PointF[] WorldToImage(Coordinate[] coordinates, bool careAboutMapTransform = false)
        {
            if (MapTransformRotation.Equals(0f))
            {
                // simple case is non-rotated views (ie MapTransform does NOT need to be applied)
                var left = Center.X - Zoom * 0.5;   
                var top = Center.Y + MapHeight * 0.5; 
                return Transform.WorldToMap(coordinates, left, top, PixelWidth, PixelHeight);
            }

            // for rotated views, measurements must be performed in the rotated world coordinate space regardless
            // of careAboutTransform. The most efficient way is using affine transformation (matrix) to translate,
            // ROTATE, scale and flip coordinates, and finally translate to Image centre.
            // If careAboutMapTransform == false then the matrix must also apply a reverse rotation as the MapTransform
            // will be applied by the graphics object at completion of all drawing
            var matrix = Transform.WorldToMapMatrix(
                Center, PixelWidth, PixelHeight, MapTransformRotation, Size, careAboutMapTransform);
            return Transform.WorldToMap(coordinates, matrix);
        }
        
        /// <summary>
        /// Converts a point in world coordinates to image coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. When rendering to image,
        /// the Graphics object has usually applied MapTransform</param>
        /// <returns>PointF in image coordinates</returns>
        public PointF WorldToImage(Coordinate p, bool careAboutMapTransform = false)
        {
            var points = WorldToImage(new Coordinate[] {p}, careAboutMapTransform);
            return points[0];
        }

        /// <summary>
        /// Converts a point array from image coordinates to world coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="points">Point array in image coordinates. Note: if you wish to preserve the input values then
        /// you must clone the point array as it will be modified if a MapTransform is applied</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. </param>
        /// <returns>Point array in world coordinates</returns>
        public Point[] ImageToWorld(PointF[] points, bool careAboutMapTransform = false)
        {
            if (careAboutMapTransform && !MapTransformRotation.Equals(0f))
                using (var transformInv = MapTransformInverted)
                    transformInv.TransformPoints(points);

            return Transform.MapToWorld(points, this);
        }
        
        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current <see cref="Zoom"/>, <see cref="Center"/>,
        /// map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/>.
        /// </summary>
        /// <param name="p">Point in image coordinates. Note: if you wish to preserve the input value then
        /// you must clone the point as it will be modified if a MapTransform is applied</param>
        /// <param name="careAboutMapTransform">Indicates whether <see cref="MapTransform"/> should be applied. </param>
        /// <returns>Point in world coordinates</returns>
        public Point ImageToWorld(PointF p, bool careAboutMapTransform = false)
        {
            var pts = ImageToWorld(new PointF[] { p }, careAboutMapTransform);
            return pts[0];
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier of the map.
        /// </summary>
        public Guid ID
        {
            get { return _id; }
            set { _id = value; }
        }

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
        /// <para>Gets the rectilinear extents of the current map based on the current <see cref="Zoom"/>,
        /// <see cref="Center"/>, map <see cref="Size"/>, and (optionally) the <see cref="MapTransform"/></para>
        /// <para>If a <see cref="MapTransform"/> is applied, the envelope CONTAINING the rotated view
        /// will be returned (used by layers to spatially select data) and the aspect ratio will NOT be the
        /// same as map <see cref="Size"/>. If aspect ratio is important then refer to <see cref="Zoom"/>
        /// and <see cref="MapHeight"/></para> 
        /// </summary>
        public Envelope Envelope
        {
            get
            {
                // height has been adjusted for pixelRatio
                var height = MapHeight;
                if (double.IsNaN(height) || double.IsInfinity(height))
                    return new Envelope(0, 0, 0, 0);

                var ll = new Coordinate(Center.X - Zoom * .5, Center.Y - height * .5);
                var ur = new Coordinate(Center.X + Zoom * .5, Center.Y + height * .5);

                // simple, non-rotated view
               if (MapTransformRotation.Equals(0f))
                    return new Envelope(ll, ur);

               // otherwise derive envelope containing rotated view (required for layers to select data)
               if (Size.Width == 0 || Size.Height == 0)
                   return new Envelope(0, 0, 0, 0);

               var rad = (double) -MapTransformRotation * Math.PI / 180.0;
               var newWidth = (ur.X - ll.X) * Math.Abs(Math.Cos(rad)) +
                              (ur.Y - ll.Y) * Math.Abs(Math.Sin(rad));
               var newHeight = (ur.X - ll.X) * Math.Abs(Math.Sin(rad)) +
                               (ur.Y - ll.Y) * Math.Abs(Math.Cos(rad));

               ll = new Coordinate(Center.X - newWidth * .5, Center.Y - newHeight * .5);
               ur = new Coordinate(Center.X + newWidth * .5, Center.Y + newHeight * .5);

               return new Envelope(ll, ur);
           }
        }
        
        /// <summary>
        /// Using the <see cref="MapTransform"/> you can alter the coordinate system of the map rendering.
        /// This makes it possible to rotate the image, for instance to have another direction than north upwards.
        /// <para>The matrix elements are stored, and a new matrix is instantiated for every request</para>
        /// </summary>
        /// <example>
        /// Rotate the map output +45 degrees around its center (ie north arrow will point to the top-right corner):
        /// <code lang="C#">
        /// System.Drawing.Drawing2D.Matrix maptransform = new System.Drawing.Drawing2D.Matrix(); //Create transformation matrix
        ///	maptransform.RotateAt(45,new PointF(myMap.Size.Width/2,myMap.Size.Height/2)); //Apply 45 degrees rotation around the center of the map
        ///	myMap.MapTransform = maptransform; //Apply transformation to map
        /// </code>
        /// </example>
        public Matrix MapTransform
        {
            get
            {
                lock (_lockMapTransform)
                    return new Matrix(
                        _mapTransformElements[0],
                        _mapTransformElements[1],
                        _mapTransformElements[2],
                        _mapTransformElements[3],
                        _mapTransformElements[4],
                        _mapTransformElements[5]);
            }
            set
            {
                lock (_lockMapTransform)
                {
                    if (value == null)
                        value = new Matrix();

                    if (!value.IsInvertible)
                        throw new ArgumentException("Matrix not invertible", nameof(value));

                    _mapTransformElements = value.Elements;

                    lock (_lockMapTransformInverted)
                    {
                        var inverted = value.Clone();
                        inverted.Invert();
                        _mapTransformInvertedElements = inverted.Elements;
                    }
                    
                    if (value.IsIdentity)
                        MapTransformRotation = 0f;
                    else
                    {
                        var rad = value.Elements[1] >= 0 ? Math.Acos(value.Elements[0]) : -Math.Acos(value.Elements[0]);
                        if (rad < 0)
                            rad += 2 * Math.PI;
                        MapTransformRotation = (float)(rad * 180.0 / Math.PI);
                    }
                }

            }
        }

        /// <summary>
        /// The inverse of <see cref="MapTransform"/> used for calculations from Image to World.
        /// <para>The matrix elements are stored, and a new matrix is instantiated for every request</para>
        /// </summary>
        internal Matrix MapTransformInverted
        {
            get
            {
                lock (_lockMapTransformInverted)
                    return new Matrix(
                        _mapTransformInvertedElements[0],
                        _mapTransformInvertedElements[1],
                        _mapTransformInvertedElements[2],
                        _mapTransformInvertedElements[3],
                        _mapTransformInvertedElements[4],
                        _mapTransformInvertedElements[5]);
            }
        }
        
        /// <summary>
        /// MapTransform Rotation in degrees. Facilitates determining if map is rotated without locking MapTransform.
        /// Positive rotation is applied anti-clockwise, with the apparent effect of north arrow rotating clockwise. 
        /// </summary>
        public float MapTransformRotation { get; private set; }

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

        public Coordinate CenterOfInterest
        {
            get => _centerOfInterest?.Copy() ?? Center;
            set
            {
                if (value == _centerOfInterest)
                    return;

                _centerOfInterest = value;
            }
        }

        /// <summary>
        /// Center of map in WCS
        /// </summary>
        public Point Center
        {
            get { return _center.Copy(); }
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

        private static int? _dpiX;

        /// <summary>
        /// Gets or Sets the Scale of the map (related to current DPI-settings of rendering)
        /// </summary>
        public double MapScale
        {
            get
            {
                if (!_dpiX.HasValue)
                {
                    using (var g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        _dpiX = (int)g.DpiX;
                    }
                }

                return GetMapScale(_dpiX.Value);
            }
            set
            {
                if (!_dpiX.HasValue)
                {
                    using (var g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        _dpiX = (int)g.DpiX;
                    }
                }
                Zoom = GetMapZoomFromScale(value, _dpiX.Value);

            }
        }

        /// <summary>
        /// Calculate the Zoom value for a given Scale value
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public double GetMapZoomFromScale(double scale, int dpi)
        {
            return ScaleCalculations.GetMapZoomFromScaleNonLatLong(scale, 1, dpi, Size.Width);
        }

        /// <summary>
        /// Returns the mapscale if the map was to be rendered at the current <see cref="Zoom"/> with the specified DPI-settings
        /// </summary>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public double GetMapScale(int dpi)
        {
            return ScaleCalculations.CalculateScaleNonLatLong(Zoom, Size.Width, 1, dpi);
        }

        /// <summary>
        /// Gets or sets the zoom level of map.
        /// </summary>
        /// <remarks>
        /// <para>The zoom level corresponds to the apparent width of the map in WCS units, regardless of any <see cref="MapTransform"/>.
        /// Zoom will only equal <see cref="Envelope"/>.Width when <see cref="MapTransformRotation"/> is 0 or 180 degrees</para> 
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
            get { return Zoom / Size.Width; }
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
            get { return (Zoom * Size.Height) / Size.Width * PixelAspectRatio; }
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

        private static void ExtendBoxForCollection(IEnumerable<ILayer> layersCollection, ref Envelope bbox)
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
            set
            {
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
        private Coordinate _centerOfInterest;

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
            set { _disclaimerLocation = value % 4; }
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
