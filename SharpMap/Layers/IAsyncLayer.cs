using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace SharpMap.Layers
{
    public delegate void MapNewTileAvaliabledHandler(TileLayer sender, SharpMap.Geometries.BoundingBox bbox, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes);

    public interface ITileAsyncLayer
    {

        event MapNewTileAvaliabledHandler MapNewTileAvaliable;
    }
}
