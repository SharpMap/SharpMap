using System.Drawing;
using SharpDX.Direct2D1;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Interface for classes that can create, prepare and dispose a <see cref="RenderTarget"/>.
    /// </summary>
    public interface IRenderTargetFactory
    {
        /// <summary>
        /// Function to create a render target for the SharpDXVectorLayer
        /// </summary>
        /// <remarks>NOTE: Implementations have to call <see cref="RenderTarget.BeginDraw()"/>.</remarks>
        /// <param name="factory">A DirectDraw2D factory</param>
        /// <param name="g">The Graphics object</param>
        /// <param name="map">The Map</param>
        /// <returns>A render target</returns>
        RenderTarget Create(Factory factory, Graphics g, Map map);
        

        /// <summary>
        /// Method to clean up the <see cref="T:SharpDX.Direct2D1.RenderTarget"/> created by <see cref="Create"/>
        /// </summary>
        /// <remarks>NOTE: 
        /// <list type="Bullet">
        /// <item>
        /// Implementations have to call <see cref="RenderTarget.EndDraw()"/>.
        /// </item>
        /// <item>Implementations have to call <see cref="RenderTarget.Dispose()"/>.</item>
        /// </list>
        /// </remarks>
        /// <param name="target">The render target</param>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        void CleanUp(RenderTarget target, Graphics g, Map map);
    }
}