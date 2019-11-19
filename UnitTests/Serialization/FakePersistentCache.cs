using System;
using BruTile;
using BruTile.Cache;

namespace UnitTests.Serialization
{
    [Serializable]
    public class FakePersistentCache<T> : IPersistentCache<T>
    {
        readonly MemoryCache<T> _memoryCache = new MemoryCache<T>(100, 200);

        public void Add(TileIndex index, T tile)
        {
            _memoryCache.Add(index, tile);
        }

        public void Remove(TileIndex index)
        {
            _memoryCache.Remove(index);
        }

        public T Find(TileIndex index)
        {
            return _memoryCache.Find(index);
        }
    }
}
