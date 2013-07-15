using System;
using SharpMap.Utilities.SpatialIndexing;

namespace SharpMap.Utilities
{
    internal class NullCacheUtility : ICacheUtility
    {
        public bool IsWebContext { get { return false; } }

        public bool TryGetValue(string key, out QuadTree quadTree)
        {
            quadTree = null;
            return false;
        }

        public void TryAddValue(string key, QuadTree quadTree, TimeSpan fromDays) { }
    }
}