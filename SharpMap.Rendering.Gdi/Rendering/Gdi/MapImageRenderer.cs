using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using GeoAPI.Geometries;
using SharpMap.Layers;
using SharpMap.Rendering.Decoration;
using SharpMap.Rendering.Gdi.Decoration;
using SharpMap.Utilities;

namespace SharpMap.Rendering.Gdi
{
    public class MapImageRenderer : BaseMapRenderer<Image>
    {
        private readonly MapDecorationRenderer _mapDecorationRenderer = new MapDecorationRenderer();

        private Dictionary<Type, Func<Type, IDeviceRenderer<ILayer, Graphics, GdiRenderingArguments>>>
            _layerRendererRegistry;

        /// <summary>
        /// Gets or sets a value indicating the layers in layercollections could be rendered parallel
        /// </summary>
        public bool AllowParallel { get; set; }

        private readonly object _lock = new object();

        protected override Image OnRenderInternal(Envelope item, IProgressHandler handler)
        {
            if (Map == null)
                throw new SharpMapException("Map not set");

            if (Map.Envelope == null || Map.Envelope.IsNull)
                throw new SharpMapException("Map.Envelope is null");

            handler = handler ?? NoopProgressHandler.Instance.Value;

            Monitor.Enter(_lock);
            var img = new Bitmap(Map.Size.Width, Map.Size.Height);
            using (var g = Graphics.FromImage(img))
            {
                g.Clear(Map.BackColor);
                RenderLayerCollection(g, Map.BackgroundLayer, handler);
                RenderLayerCollection(g, Map.Layers, handler);
                RenderLayerCollection(g, Map.VariableLayers, handler);
                RenderMapDecorations(g, Map.Decorations, handler);
            }
            Monitor.Exit(_lock);

            return img;

        }

        private void RenderLayerCollection(Graphics g, LayerCollection layers, IProgressHandler handler)
        {
            OnRenderingLayerCollection(EventArgs.Empty);

            if (AllowParallel)
            {
                using (var lcr = new LayerCollectionRenderer(layers))
                    lcr.Render(g, Map);
            }
            else
            {
                foreach (var layer in layers)
                {
                    
                }
            }

            OnRenderedLayerCollection(EventArgs.Empty);

            //throw new NotImplementedException();
        }

        private void RenderMapDecorations(Graphics graphics, IEnumerable<IMapDecoration> decorations, IProgressHandler handler)
        {
            OnRenderingMapDecorations(EventArgs.Empty);
            foreach (var mapDecoration in decorations)
            {
                _mapDecorationRenderer.Render(mapDecoration, graphics, new GdiRenderingArguments {Map = Map});
            }
            OnRenderedMapDecorations(EventArgs.Empty);
        }
    }
}
