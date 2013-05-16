using System.Drawing;
using System.Drawing.Imaging;
using GeoAPI.Geometries;

namespace SharpMap.Layers
{
    /// <summary>
    /// Delegate function definition for handling <see cref="T:SharpMap.Layers.ITileAsyncLayer"/>s <see cref="E:SharpMap.Layers.ITileAsyncLayer.MapNewTileAvaliable"/> event
    /// </summary>
    /// <param name="sender">The sender</param>
    /// <param name="bbox">The bounding box of the tile</param>
    /// <param name="bm">The tile bitmap</param>
    /// <param name="sourceWidth">The tiles width</param>
    /// <param name="sourceHeight">The tiles height</param>
    /// <param name="imageAttributes">The <see cref="ImageAttributes"/> to use when rendering the tile</param>
    public delegate void MapNewTileAvaliabledHandler(TileLayer sender, Envelope bbox, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes);

    /// <summary>
    /// Delegate for notifying download of tiles
    /// </summary>
    /// <param name="tilesRemaining"></param>
    public delegate void DownloadProgressHandler(int tilesRemaining);

    /// <summary>
    /// Interface for async tile layers
    /// </summary>
    public interface ITileAsyncLayer
    {
        /// <summary>
        /// Event raised when a new tile is available
        /// </summary>
        event MapNewTileAvaliabledHandler MapNewTileAvaliable;

        /// <summary>
        /// Event raised when downloadprogress of tiles changed
        /// </summary>
        event DownloadProgressHandler DownloadProgressChanged;

        /// <summary>
        /// Gets or Sets a value indicating if to redraw the map only when all tiles are downloaded
        /// </summary>
        bool OnlyRedrawWhenComplete { get; set; }

        /// <summary>
        /// Method to cancel the async layer
        /// </summary>
        void Cancel();

        /// <summary>
        /// Returns the number of tiles that are in queue for download
        /// </summary>
        int NumPendingDownloads { get; }
    }
}
