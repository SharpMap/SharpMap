using GeoAPI.Geometries;

namespace RoutingExample.RoutingSystem
{
    /// <summary>
    /// This Class Store The End Points Of A Line.
    /// </summary>
    public class LineEndPoints
    {
        /// <summary>
        /// The First Point Of The Line
        /// </summary>
        public Coordinate First { get; set; }

        /// <summary>
        /// The End Point Of The Line
        /// </summary>
        public Coordinate Last { get; set; }

        /// <summary>
        /// The Length Of The Line
        /// </summary>
        public double Length { get; set; }

        public LineEndPoints() { }

        public LineEndPoints(Coordinate pFirst, Coordinate pLast, double pLength)
        {
            First = pFirst;
            Last = pLast;
            Length = pLength;
        }
    }
}
