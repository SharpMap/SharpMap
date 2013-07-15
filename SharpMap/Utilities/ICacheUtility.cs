using System;
using SharpMap.Utilities.SpatialIndexing;

namespace SharpMap.Utilities
{
    public interface ICacheUtility
    {
        bool IsWebContext { get; }
        bool TryGetValue(string s, out QuadTree quadTree);
        void TryAddValue(string s, QuadTree quadTree, TimeSpan fromDays);
    }
}
