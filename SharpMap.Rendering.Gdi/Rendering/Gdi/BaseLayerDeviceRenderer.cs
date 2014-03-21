using System.Drawing;
using SharpMap.Layers;
using SharpMap.Rendering.Gdi.Decoration;

namespace SharpMap.Rendering.Gdi
{
    public abstract class BaseLayerDeviceRenderer<TLayer> : BaseDeviceRenderer<TLayer, Graphics, GdiRenderingArguments>
        where TLayer : ILayer
    {
    }
}