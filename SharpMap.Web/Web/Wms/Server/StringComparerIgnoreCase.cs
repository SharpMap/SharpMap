using System.Collections.Generic;

namespace SharpMap.Web.Wms.Server
{
    public class StringComparerIgnoreCase : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null || y == null)
                return false;
        
            return x.ToLowerInvariant() == y.ToLowerInvariant();
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }
    }
}
