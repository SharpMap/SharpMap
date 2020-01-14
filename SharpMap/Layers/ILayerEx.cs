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
        /// <param name="affectedArea">The actual extent of rendered data inclusive of any labels or vector symbology</param>
        void Render(Graphics g, MapViewport  map, out Envelope affectedArea);
   }
}
