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
        public static double Get_Y(this Coordinate coord)
        {
            return coord.Y;
        }
        public static double Get_Z(this Coordinate coord)
        {
            return coord.Z;
        }
#pragma warning disable 618
        public static double Get_M(this ICoordinate coord)
#pragma warning restore 618
        {
            return coord.M;
        }
    }
}
