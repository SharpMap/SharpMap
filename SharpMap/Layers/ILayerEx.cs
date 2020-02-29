using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap.Layers
{
    public interface ILayerEx : ILayer
    {
        /// <summary>
        /// Renders the layer using the current viewport
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        /// <returns>The extent of the actual area rendered in world units</returns>
        new Envelope Render(Graphics g, MapViewport  map);
   }
}
