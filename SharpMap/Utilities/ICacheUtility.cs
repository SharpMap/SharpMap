using System;
using SharpMap.Utilities.SpatialIndexing;

namespace SharpMap.Utilities
{
    public interface ICacheUtility
    {
        bool IsWebContext { get; }

        bool TryGetValue(string key, out QuadTree quadTree);

        void TryAddValue(string key, QuadTree quadTree, TimeSpan fromDays);
    }
}
