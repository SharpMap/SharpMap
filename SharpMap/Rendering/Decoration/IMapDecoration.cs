using System.Drawing;

namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// Interface for all map decorations
    /// </summary>
    public interface IMapDecoration
    {
        /// <summary>
        /// Renders the map decoration to the graphics object
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        void Render(Graphics g, Map map);
    }
}