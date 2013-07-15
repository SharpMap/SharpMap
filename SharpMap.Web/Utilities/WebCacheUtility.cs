using System;
using SharpMap.Utilities.SpatialIndexing;
using SharpMap.Web;

namespace SharpMap.Utilities
{
    public class WebCacheUtility : ICacheUtility
    {
        public bool IsWebContext { get
        {
            return HttpCacheUtility.IsWebContext;
        } 
        }
        public bool TryGetValue(string key, out QuadTree quadTree)
        {
            return HttpCacheUtility.TryGetValue(key, out quadTree);
        }

        public void TryAddValue(string key, QuadTree quadTree, TimeSpan fromDays)
        {
            HttpCacheUtility.TryAddValue(key, quadTree, fromDays);
        }
    }
}
