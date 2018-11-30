namespace WinFormSamples
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using BruTile;
    using BruTile.Cache;

    using GeoAPI.Geometries;

    using SharpMap;
    using SharpMap.Layers;

    internal class SharpMapTileSource : ITileSource
    {
        private readonly SharpMapTileProvider provider;

        public SharpMapTileSource(ITileSchema schema, params ILayer[] layers) : this(schema, null, layers) { }

        public SharpMapTileSource(ITileSchema schema, ITileCache<byte[]> cache, params ILayer[] layers)
        {
            Title = "SharpMap.Map";
            if (schema == null)
                throw new ArgumentNullException("schema");
            if (layers == null)
                throw new ArgumentNullException("layers");

            this.provider = new SharpMapTileProvider(schema, cache, layers);
        }

        public string Title { get; set; }

        public ITileProvider Provider
        {
            get { return this.provider; }
        }

        public ITileSchema Schema
        {
            get { return this.provider.Schema; }
        }

        public string Name { get; private set; }

        public Extent Extent
        {
            get { throw new NotImplementedException(); }
        }

        public Attribution Attribution => throw new NotImplementedException();

        public byte[] GetTile(TileInfo tileInfo)
        {
            return provider.GetTile(tileInfo);
        }
    }

    internal class SharpMapTileProvider : ITileProvider
    {
        private class NullCache : ITileCache<byte[]>
        {
            public void Add(TileIndex index, byte[] image) { }

            public void Remove(TileIndex index) { throw new NotImplementedException(); }

            public byte[] Find(TileIndex index) { return null; }
        }

        private readonly ITileCache<byte[]> cache;

        public ITileSchema Schema { get; private set; }

        private readonly LayerCollection layers;

        private ImageFormat format;

        public SharpMapTileProvider(ITileSchema schema, ITileCache<byte[]> cache, params ILayer[] layers)
        {            
            if (schema == null)
                throw new ArgumentNullException("schema");
            if (layers == null)
                throw new ArgumentNullException("layers");
            
            this.Schema = schema;
            this.cache = cache ?? new NullCache();

            LayerCollection collection = new LayerCollection();
            foreach (ILayer layer in layers)
                collection.Add(layer);
            this.layers = collection;
        }

        public byte[] GetTile(TileInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (this.format == null)
                this.format = GetFormat(this.Schema);

            TileIndex index = info.Index;
            byte[] bytes = this.cache.Find(index);
            if (bytes != null)
                return bytes;            

            using (Image tile = this.CreateTile(info))
            using (MemoryStream ms = new MemoryStream())
            {                
                tile.Save(ms, this.format);
                bytes = ms.ToArray();
                this.cache.Add(index, bytes);
                return ms.ToArray();
            }
        }

        private static ImageFormat GetFormat(ITileSchema schema)
        {
            string format = schema.Format;
            if (String.Equals("JPEG", format, StringComparison.InvariantCultureIgnoreCase))
                return ImageFormat.Jpeg;
            if (String.Equals("JPG", format, StringComparison.InvariantCultureIgnoreCase))
                return ImageFormat.Jpeg;
            if (String.Equals("GIF", format, StringComparison.InvariantCultureIgnoreCase))
                return ImageFormat.Gif;
            if (String.Equals("BMP", format, StringComparison.InvariantCultureIgnoreCase))
                return ImageFormat.Bmp;
            if (String.Equals("TIFF", format, StringComparison.InvariantCultureIgnoreCase))
                return ImageFormat.Tiff;
            if (String.Equals("TIF", format, StringComparison.InvariantCultureIgnoreCase))
                return ImageFormat.Tiff;
            return ImageFormat.Png;
        }

        private readonly object synclock = new object();

        private Image CreateTile(TileInfo info)        
        {
            lock (synclock)
            {
                var tileWidth = this.Schema.GetTileWidth(info.Index.Level);
                var tileHeight = this.Schema.GetTileHeight(info.Index.Level);
                Size size = new Size(tileWidth, tileHeight);
                Map map = new Map(size) { BackColor = Color.Transparent };
                map.Layers.AddCollection(this.layers);

                Extent ext = info.Extent;
                Envelope bbox = new Envelope(ext.MinX, ext.MaxX, ext.MinY, ext.MaxY);
                map.ZoomToBox(bbox);

                return map.GetMap();
            }
        }
    }    
}
