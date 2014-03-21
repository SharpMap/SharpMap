using System;
using GeoAPI.Geometries;
using SharpMap.Utilities;

namespace SharpMap.Rendering
{
    
    /// <summary>
    /// Interface for all map renderers
    /// </summary>
    public interface IMapRenderer<out T>
    {
        /// <summary>
        /// Event raised when the map is about to be rendered
        /// </summary>
        event EventHandler RenderingMap;
        /// <summary>
        /// Event raised when the map was rendered
        /// </summary>
        event EventHandler RenderedMap;
        /// <summary>
        /// Event raised when the map decorations are about to be rendered
        /// </summary>
        event EventHandler RenderingMapDecorations;

        /// <summary>
        /// Event raised when the map decorations were rendered
        /// </summary>
        event EventHandler RenderedMapDecorations;
        
        /// <summary>
        /// Gets or sets the map to render
        /// </summary>
        Map Map { get; set; }

        /// <summary>
        /// Method to invoke the rendering process using the provided <paramref name="envelope"/>
        /// </summary>
        /// <param name="envelope">The envelope</param>
        T Render(Envelope envelope);

        /// <summary>
        /// Method to invoke the rendering process
        /// </summary>
        /// <param name="handler">A handler to report progress</param>
        /// <param name="envelope">The envelope</param>
        T Render(Envelope envelope, IProgressHandler handler);
    }
}