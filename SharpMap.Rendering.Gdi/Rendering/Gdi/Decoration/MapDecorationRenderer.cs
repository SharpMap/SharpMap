using System;
using System.Drawing;
using SharpMap.Rendering.Decoration;
using SharpMap.Utilities;

namespace SharpMap.Rendering.Gdi.Decoration
{
    public class MapDecorationRenderer : BaseDeviceRenderer<IMapDecoration, Graphics, GdiRenderingArguments>
    {
        protected override void OnRenderInternal(IMapDecoration @object, Graphics device, GdiRenderingArguments renderArgument,
            IProgressHandler handler)
        {
            if (@object == null)
                throw new ArgumentNullException("@object");

            //ToDo remove logic
            @object.Render(device, renderArgument.Map);
        }
    }
}