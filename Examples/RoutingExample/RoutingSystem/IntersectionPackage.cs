using GeoAPI.Geometries;

namespace RoutingExample.RoutingSystem
{
    /// <summary>
    /// A class that groups together the point the user clicks, the cloest line to that point and the intersection
    /// point on the line
    /// </summary>
    public class IntersectionPackage
    {
        /// <summary>
        /// The Closest line to the user click point
        /// </summary>
        private readonly ILineString _cloestLine;

        /// <summary>
        /// The point the user clicks on
        /// </summary>
        private readonly IPoint _clickPoint;

        /// <summary>
        /// The intersection point on cloestline to clickpoint
        /// </summary>
        private readonly IPoint _intersectionPoint;

        /// <summary>
        /// The Cloest Line.
        /// </summary>
        public ILineString ClosestLine
        {
            get
            {
                return _cloestLine;
            }
        }

        /// <summary>
        /// The Intersection Point On A Line.
        /// </summary>
        public IPoint IntersectionPoint
        {
            get
            {
                return _intersectionPoint;
            }
        }

        /// <summary>
        /// Where The User Selected As A End Point
        /// </summary>
        public IPoint ClickPoint
        {
            get
            {
                return _clickPoint;
            }
        }

        public IntersectionPackage() { }



        public IntersectionPackage(IPoint pClick, IPoint pIntersect, ILineString pLine)
        {
            _clickPoint = pClick;
            _intersectionPoint = pIntersect;
            _cloestLine = pLine;
        }
    }
}
