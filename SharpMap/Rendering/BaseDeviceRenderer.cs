using System;
using Common.Logging;
using SharpMap.Utilities;

namespace SharpMap.Rendering
{
    /// <summary>
    /// An abstract base implementation for device renderers
    /// </summary>
    /// <typeparam name="TObject">The type of the object to render</typeparam>
    /// <typeparam name="TDevice">The type of the device to render upon</typeparam>
    /// <typeparam name="TRenderArgument">The arguments for the rendering process</typeparam>
    public abstract class BaseDeviceRenderer<TObject, TDevice, TRenderArgument> : IDeviceRenderer<TObject, TDevice, TRenderArgument>
    {
        /// <summary>
        /// Event raised when the map is about to be rendered
        /// </summary>
        public event EventHandler<DeviceRendererArgs<TObject, TDevice, TRenderArgument>> Rendering;

        /// <summary>
        /// Event raised when the map has been rendered
        /// </summary>
        public event EventHandler<DeviceRendererArgs<TObject, TDevice, TRenderArgument>> Rendered;

        /// <summary>
        /// Method to invoke the rendering process
        /// </summary>
        /// <param name="object">The object to render</param>
        /// <param name="device">The device to render to</param>
        /// <param name="renderArgument">The argument object needed for rendering</param>
        public virtual void Render(TObject @object, TDevice device, TRenderArgument renderArgument)
        {
            Render(@object, device, renderArgument, NoopProgressHandler.Instance.Value);
        }

        /// <summary>
        /// Method to invoke the rendering process
        /// </summary>
        /// <param name="object">The object to render</param>
        /// <param name="device">The device to render to</param>
        /// <param name="renderArgument">The argument object needed for rendering</param>
        /// <param name="handler">A handler to report progress</param>
        public virtual void Render(TObject @object, TDevice device, TRenderArgument renderArgument, IProgressHandler handler)
        {
            OnRendering(DeviceRenderingArgs.Create( @object, device, renderArgument));
            OnRenderInternal(@object, device, renderArgument, handler);
            OnRendered(DeviceRenderingArgs.Create( @object, device, renderArgument));
        }

        /// <summary>
        /// Event invoker for the <see cref="OnRendering"/> event
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnRendering(DeviceRendererArgs<TObject, TDevice, TRenderArgument> e)
        {
            LogManager.GetCurrentClassLogger().Debug("Rendering");
            if (Rendering != null)
                Rendering(this, e);
        }

        /// <summary>
        /// Event invoker for the <see cref="OnRendered"/> event
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnRendered(DeviceRendererArgs<TObject, TDevice, TRenderArgument> e)
        {
            LogManager.GetCurrentClassLogger().Debug("Rendered");
            if (Rendered != null)
                Rendered(this, e);
        }

        /// <summary>
        /// Method to implement actual rendering
        /// </summary>
        /// <param name="object">The object to render</param>
        /// <param name="device">The device to render upon</param>
        /// <param name="renderArgument">The rendering argument</param>
        /// <param name="handler">A progress handler</param>
        protected abstract void OnRenderInternal(TObject @object, TDevice device, TRenderArgument renderArgument,
            IProgressHandler handler);
    }
}