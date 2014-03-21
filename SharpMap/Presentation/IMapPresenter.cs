using SharpMap.Rendering;

namespace SharpMap.Presentation
{
    /// <summary>
    /// Interface for all map presenters
    /// </summary>
    public interface IMapPresenter<T>
    {
        /// <summary>
        /// Gets a renderer
        /// </summary>
        IMapRenderer<T> Renderer { get; set; }

        /// <summary>
        /// Gets a value indicating the map
        /// </summary>
        Map Map { get; }


    }
}