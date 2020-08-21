using System.Drawing;

namespace SharpMap.Layers
{
    /// <summary>
    /// An extended layer interface
    /// </summary>
    public interface ILayerEx : ILayer
    {
        /// <summary>
        /// Renders the layer using the current viewport, returning a rectangle describing the area covered.
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="mvp">Map which is rendered</param>
        /// <returns>Rectangle enclosing the actual area rendered on the graphics canvas</returns>
        new Rectangle Render(Graphics g, MapViewport mvp);
   }
}
