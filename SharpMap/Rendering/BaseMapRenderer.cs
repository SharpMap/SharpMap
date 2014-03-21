using System;
using Common.Logging;
using GeoAPI.Geometries;
using SharpMap.Utilities;

namespace SharpMap.Rendering
{
    /// <summary>
    /// An abstract base map renderer class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseMapRenderer<T> : IMapRenderer<T>
    {
        /// <summary>
        /// Event raised when the map is about to be rendered
        /// </summary>
        public event EventHandler RenderingMap;

        /// <summary>
        /// Event raised when the map has been rendered
        /// </summary>
        public event EventHandler RenderedMap;

        /// <summary>
        /// Event raised when the map decorations are about to be rendered
        /// </summary>
        public event EventHandler RenderingMapDecorations;

        /// <summary>
        /// Event raised when the map decorations have been rendered
        /// </summary>
        public event EventHandler RenderedMapDecorations;

        /// <summary>
        /// Gets or sets a value indicating the map
        /// </summary>
        public Map Map { get; set; }

        /// <summary>
        /// Renders the <see cref="Map"/> using <see cref="SharpMap.Map.Envelope"/>.
        /// </summary>
        public T Render()
        {
            return Render(Map.Envelope);
        }

        /// <summary>
        /// Renders the <see cref="Map"/> using <see cref="SharpMap.Map.Envelope"/>.
        /// </summary>
        /// <param name="handler"/>
        public T Render(IProgressHandler handler)
        {
            return Render(Map.Envelope);
        }
        /// <summary>
        /// Renders the <see cref="Map"/> using <paramref name="envelope"/> viewport.
        /// </summary>
        public T Render(Envelope envelope)
        {
            return Render(envelope, NoopProgressHandler.Instance.Value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="handler"></param>
        public T Render(Envelope item, IProgressHandler handler)
        {
            OnRenderingMap(EventArgs.Empty);
            var res = OnRenderInternal(item, handler);
            OnRenderedMap(EventArgs.Empty);

            return res;
        }

        /// <summary>
        /// Event invoker method for the <see cref="RenderingMap"/> event.
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnRenderingMap(EventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("OnRenderingMap");
            if (RenderingMap != null)
                RenderingMap(this, e);
        }

        /// <summary>
        /// Event invoker method for the <see cref="RenderedMap"/> event.
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnRenderedMap(EventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("OnRenderedMap");
            if (RenderedMap != null)
                RenderedMap(this, e);
        }

        /// <summary>
        /// Event invoker method for the <see cref="RenderingMapDecorations"/> event.
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnRenderingMapDecorations(EventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("OnRenderingMapDecorations");
            if (RenderingMapDecorations != null)
                RenderingMapDecorations(this, e);
        }

        /// <summary>
        /// Event invoker method for the <see cref="RenderedMapDecorations"/> event.
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnRenderedMapDecorations(EventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("OnRenderedMapDecorations");
            if (RenderedMapDecorations != null)
                RenderedMapDecorations(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        protected virtual T OnRenderInternal(Envelope item, IProgressHandler handler)
        {
            return default (T);
        }
    }
}