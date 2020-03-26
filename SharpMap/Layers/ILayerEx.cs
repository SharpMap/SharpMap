using System.Drawing;

namespace SharpMap.Layers
{
    public interface ILayerEx : ILayer
    {
        /// <summary>
        /// Renders the layer using the current viewport
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        /// <returns>Rectangle enclosing the actual area rendered on the graphics canvas</returns>
        new Rectangle Render(Graphics g, MapViewport  map);
   }
}
