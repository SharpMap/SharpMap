using System;
using System.Drawing;

namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// Interface for all map decorations
    /// </summary>
    public interface IMapDecoration
    {
        /// <summary>
        /// Draw the map decoration.
        /// <para>Note that base class <see cref="MapDecoration"/> implementation resets <paramref name="g"/>.Transform
        /// prior to raising event OnRendering, and restore the <paramref name="g"/>.Transform prior to
        /// raising event OnRendered.</para>
        /// Likewise, <paramref name="g"/>.Clip is reset prior to rendering map decoration, and restored
        /// immediately after rendering.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        void Render(Graphics g, MapViewport map);

        [Obsolete("Use Render (Graphics, MapViewport)")]
        void Render(Graphics g, Map map);
    }
}
