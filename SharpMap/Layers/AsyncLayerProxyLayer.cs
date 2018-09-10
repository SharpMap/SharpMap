// Copyright 2014 - Felix Obermaier (www.ivv-aachen.de)
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    /// <summary>
    /// A proxy class to allow async tile rendering for static layers
    /// </summary>
    /// <example lang="C#">
    /// var map = new SharpMap.Map(new System.Drawing.Size(1024, 786));
    /// var provider = new SharpMap.Data.Providers.Shapefile("&lt;Path to shapefile&gt;", true);
    /// var layer = new SharpMap.Layers.VectorLayer("LAYER1", provider);
    /// map.BackgroundLayer.Add(AsyncLayerProxyLayer.Create(layer));
    /// </example>
    [Serializable]
    public class AsyncLayerProxyLayer : ITileAsyncLayer, IDisposable, ICanQueryLayer
    {
        private readonly ILayer _baseLayer;
        private bool _onlyRedrawWhenComplete;
        [NonSerialized]
        private int _numPendingDownloads;
        private readonly Size _cellSize;
        private readonly Size _cellBuffer = new Size(5, 5);

        private ImageAttributes _imageAttributes = new ImageAttributes();
        [NonSerialized]
        private object _renderLock = new object();
        [NonSerialized]
        private Bitmap _bitmap;
        [NonSerialized]
        private Envelope _lastViewport = new Envelope();

        private class RenderTask
        {
            /// <summary>
            /// The token to cancel the task
            /// </summary>
            internal System.Threading.CancellationTokenSource CancellationToken;
            /// <summary>
            /// The task
            /// </summary>
            internal System.Threading.Tasks.Task Task;
        }

        [NonSerialized]
        private List<RenderTask> _currentTasks = new List<RenderTask>();

        /// <summary>
        /// Method to warp a usual layer in an async layer
        /// </summary>
        /// <param name="layer">The layer to wrap</param>
        /// <param name="tileSize">The size of the tile</param>
        /// <returns>A async tile layer</returns>
        public static ILayer Create(ILayer layer, Size? tileSize = null)
        {
            if (layer == null)
                throw new ArgumentNullException("layer");
            if (layer is ITileAsyncLayer)
                return layer;

            tileSize = tileSize ?? new Size(256, 256);

            return new AsyncLayerProxyLayer(layer, tileSize.Value);
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="baseLayer">The layer to proxy</param>
        /// <param name="cellSize">The size of the tile</param>
        private AsyncLayerProxyLayer(ILayer baseLayer, Size cellSize)
        {
            _baseLayer = baseLayer;
            _cellSize = cellSize;
            ((ILayer)this).VisibilityUnits = baseLayer.VisibilityUnits;
        }

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

        /// <summary>
        /// Gets or Sets what level-reference the Min/Max values are defined in
        /// </summary>
        VisibilityUnits ILayer.VisibilityUnits { get; set; }

        /// <summary>
        /// Specifies whether this layer should be rendered or not
        /// </summary>
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

        /// <summary>
        /// Name of layer
        /// </summary>
        string ILayer.LayerName
        {
            get { return _baseLayer.LayerName; }
            set { _baseLayer.LayerName = value; }
        }

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        Envelope ILayer.Envelope
        {
            get { return _baseLayer.Envelope; }
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        int ILayer.SRID
        {
            get { return _baseLayer.SRID; }
            set { _baseLayer.SRID = value; }
        }

        /// <summary>
        /// The spatial reference ID (CRS) that can be exposed externally.
        /// </summary>
        /// <remarks>
        /// TODO: explain better why I need this property
        /// </remarks>
        int ILayer.TargetSRID
        {
            get { return _baseLayer.TargetSRID; }
        }

        /// <summary>
        /// Proj4 String Projection
        /// </summary>
        string ILayer.Proj4Projection
        {
            get { return _baseLayer.Proj4Projection; }
            set { _baseLayer.Proj4Projection = value; }
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        void ILayer.Render(Graphics g, Map map)
        {
            ((ILayer)this).Render(g, (MapViewport)map);
        }

        void ILayer.Render(Graphics g, MapViewport map)
        {
            // We don't need to regenerate the tiles
            if (map.Envelope.Equals(_lastViewport) && _numPendingDownloads == 0)
            {
                g.DrawImage(_bitmap, Point.Empty);
                return;
            }

            // Create a backbuffer
            lock (_renderLock)
            {
                if (_bitmap == null || _bitmap.Size != map.Size)
                {
                    _bitmap = new Bitmap(map.Size.Width, map.Size.Height, PixelFormat.Format32bppArgb);
                    using (var tmpGraphics = Graphics.FromImage(_bitmap))
                        tmpGraphics.Clear(Color.Transparent);
                }
            }
            // Save the last viewport
            _lastViewport = map.Envelope;

            // Cancel old rendercycle
            ((ITileAsyncLayer)this).Cancel();

            var mapViewport = map.Envelope;
            var mapSize = map.Size;

            var mapColumnWidth = _cellSize.Width+_cellBuffer.Width;
            var mapColumnHeight = _cellSize.Height + _cellBuffer.Width;
            var columns = (int)Math.Ceiling((double) mapSize.Width/mapColumnWidth);
            var rows = (int) Math.Ceiling((double) mapSize.Height/mapColumnHeight);

            var renderMapSize = new Size(columns * _cellSize.Width + _cellBuffer.Width, 
                                         rows * _cellSize.Height + _cellBuffer.Height);
            var horizontalFactor = (double) renderMapSize.Width/mapSize.Width;
            var verticalFactor = (double) renderMapSize.Height/mapSize.Height;

            var diffX = 0.5d*(horizontalFactor*mapViewport.Width - mapViewport.Width);
            var diffY = 0.5d*(verticalFactor*mapViewport.Height-mapViewport.Height);

            var totalRenderMapViewport = mapViewport.Grow(diffX, diffY);
            var columnWidth = totalRenderMapViewport.Width/columns;
            var rowHeight = totalRenderMapViewport.Height/rows;

            var rmdx = (int)((mapSize.Width-renderMapSize.Width) * 0.5f);
            var rmdy = (int)((mapSize.Height - renderMapSize.Height) * 0.5f);

            var tileSize = Size.Add(_cellSize, Size.Add(_cellBuffer, _cellBuffer));

            var miny = totalRenderMapViewport.MinY;
            var pty = rmdy + renderMapSize.Height - tileSize.Height;

            for (var i = 0; i < rows; i ++)
            {
                var minx = totalRenderMapViewport.MinX;
                var ptx = rmdx;
                for (var j = 0; j < columns; j++)
                {
                    var tmpMap = new Map(_cellSize);
                    
                    tmpMap.Layers.Add(_baseLayer);
                    tmpMap.DisposeLayersOnDispose = false;
                    tmpMap.ZoomToBox(new Envelope(minx, minx + columnWidth, miny, miny + rowHeight));

                    var cancelToken = new System.Threading.CancellationTokenSource();
                    var token = cancelToken.Token;
                    var pt = new Point(ptx, pty);
                    var t = new System.Threading.Tasks.Task(delegate
                    {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();

                        var res = RenderCellOnThread(token, pt, tmpMap);
                        if (res)
                        {
                            System.Threading.Interlocked.Decrement(ref _numPendingDownloads);
                            var e = DownloadProgressChanged;
                            if (e != null)
                                e(_numPendingDownloads);
                        }

                    }, token);
                    var dt = new RenderTask {CancellationToken = cancelToken, Task = t};
                    lock (_currentTasks)
                    {
                        _currentTasks.Add(dt);
                        _numPendingDownloads++;
                    }
                    t.Start();
                    minx += columnWidth;
                    ptx += _cellSize.Width;
                }
                miny += rowHeight;
                pty -= _cellSize.Height;
            }
        }

        /// <summary>
        /// Event raised when a new tile has been rendered an is now avalable
        /// </summary>
        public event MapNewTileAvaliabledHandler MapNewTileAvaliable;
        
        /// <summary>
        /// Event raised when the rendering of tiles has made progress
        /// </summary>
        public event DownloadProgressHandler DownloadProgressChanged;

        bool ITileAsyncLayer.OnlyRedrawWhenComplete
        {
            get { return _onlyRedrawWhenComplete; }
            set { _onlyRedrawWhenComplete = value; }
        }

        void ITileAsyncLayer.Cancel()
        {
            lock (_currentTasks)
            {
                foreach (var t in _currentTasks)
                {
                    if (!t.Task.IsCompleted)
                        t.CancellationToken.Cancel();
                }
                _currentTasks.Clear();
                _numPendingDownloads = 0;
            }
        }

        int ITileAsyncLayer.NumPendingDownloads
        {
            get { return _numPendingDownloads; }
        }

        private bool RenderCellOnThread(System.Threading.CancellationToken token, Point ptInsert, Map map)
        {
            var tile = new Bitmap(map.Size.Width, map.Size.Height, PixelFormat.Format32bppArgb);
            var mvp = new MapViewport(map);
            using (var g = Graphics.FromImage(tile))
            {
                g.Clear(Color.Transparent);
                _baseLayer.Render(g, mvp);
                map.Layers.Clear();
            }

            if (!token.IsCancellationRequested)
            {
                OnTileRendered(ptInsert, map.Envelope, tile);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method to raise the map tile available event
        /// </summary>
        /// <param name="ptInsert"></param>
        /// <param name="env"></param>
        /// <param name="bmp"></param>
        protected virtual void OnTileRendered(Point ptInsert, Envelope env, Bitmap bmp)
        {
            if (!env.Equals(_lastViewport))
            {
                System.Threading.Monitor.Enter(_renderLock);
                using (var g = Graphics.FromImage(_bitmap))
                    g.DrawImageUnscaled(bmp, ptInsert);
                System.Threading.Monitor.Exit(_renderLock);
            }

            var h1 = MapNewTileAvaliable;
            if (h1 != null)
            {
                h1(null, env, bmp, bmp.Width, bmp.Height, _imageAttributes);
            }

            var h2 = DownloadProgressChanged;
            if (h2 != null)
            {
                System.Threading.Interlocked.Decrement(ref _numPendingDownloads);
                h2(_numPendingDownloads);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            ((ITileAsyncLayer)this).Cancel();
            _imageAttributes.Dispose();
        }

        void ICanQueryLayer.ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            if (!((ICanQueryLayer)this).IsQueryEnabled)
                return;
            ((ICanQueryLayer)_baseLayer).ExecuteIntersectionQuery(box, ds);
        }

        void ICanQueryLayer.ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            if (!((ICanQueryLayer)this).IsQueryEnabled)
                return;
            ((ICanQueryLayer)_baseLayer).ExecuteIntersectionQuery(geometry, ds);
        }

        bool ICanQueryLayer.IsQueryEnabled
        {
            get
            {
                var cql = _baseLayer as ICanQueryLayer;
                if (cql != null && cql.IsQueryEnabled)
                    return true;
                return false;
            }
            set
            {
                var cql = _baseLayer as ICanQueryLayer;
                if (cql != null )
                    cql.IsQueryEnabled = value;
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _imageAttributes = new ImageAttributes();
            _currentTasks = new List<RenderTask>();
            _renderLock = new object();
            _lastViewport = new Envelope();
        }
    }
}
