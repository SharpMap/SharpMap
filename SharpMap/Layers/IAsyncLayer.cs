using System.Drawing;
using System.Drawing.Imaging;
using GeoAPI.Geometries;

namespace SharpMap.Layers
{
    public delegate void MapNewTileAvaliabledHandler(TileLayer sender, Envelope bbox, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes);

    public interface ITileAsyncLayer
    {

        event MapNewTileAvaliabledHandler MapNewTileAvaliable;
    }
}
