using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;

namespace SharpMap.UI.WPF
{
    public static class PropertyExtensions
    {
        public static double Get_X(this Coordinate coord)
        {
            return coord.X;
        }

        
    }
}
