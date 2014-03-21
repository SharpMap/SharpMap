using System;
using SharpMap.Utilities;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Generic renderer interface
    /// </summary>
    /// <typeparam name="TDevice">The type of the device to render to</typeparam>
    /// <typeparam name="TRenderArgument">The type of the arguments</typeparam>
    /// <typeparam name="TObject">The type of the object to render</typeparam>
    public interface IDeviceRenderer<TObject, TDevice, TRenderArgument>
    {
        /// <summary>
        /// Event raised when the map is about to be rendered
        /// </summary>
        event EventHandler<DeviceRendererArgs<TObject, TDevice, TRenderArgument>> Rendering;

        /// <summary>
        /// Event raised when the map has been rendered
        /// </summary>
        event EventHandler<DeviceRendererArgs<TObject, TDevice, TRenderArgument>> Rendered;

        /// <summary>
        /// Method to invoke the rendering process
        /// </summary>
        /// <param name="object">The object to render</param>
        /// <param name="device">The device to render to</param>
        /// <param name="renderArgument">The argument object needed for rendering</param>
        void Render(TObject @object, TDevice device, TRenderArgument renderArgument);

        /// <summary>
        /// Method to invoke the rendering process
        /// </summary>
        /// <param name="object">The object to render</param>
        /// <param name="device">The device to render to</param>
        /// <param name="renderArgument">The argument object needed for rendering</param>
        /// <param name="handler">A handler to report progress</param>
        void Render(TObject @object, TDevice device, TRenderArgument renderArgument, IProgressHandler handler);
    }

    public static class DeviceRenderingArgs
    {
        public static DeviceRendererArgs<T1, T2, T3> Create<T1, T2, T3>(T1 @object, T2 device, T3 args)
        {
            return new DeviceRendererArgs<T1, T2, T3>(@object, device, args);
        }
    }

    public class DeviceRendererArgs<TObject, TDevice, TRenderArgument> : EventArgs
    {
        public DeviceRendererArgs(TObject o, TDevice device, TRenderArgument arg)
        {
            Object = o;
            Device = device;
            Argument = arg;
        }

        public TObject Object { get; private set; }
        public TDevice Device { get; private set; }
        public TRenderArgument Argument { get; private set; }
    }
}