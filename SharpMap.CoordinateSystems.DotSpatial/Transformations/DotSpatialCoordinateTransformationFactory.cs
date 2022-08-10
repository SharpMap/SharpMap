using DotSpatial.Projections;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace SharpMap.CoordinateSystems.Transformations
{
    /// <summary>
    /// An implementation for a coordinate transformation factory, that creates 
    /// coordinate transformations based on <see cref="DotSpatialCoordinateSystem"/>s.
    /// </summary>
    public class DotSpatialCoordinateTransformationFactory : CoordinateTransformationFactory
    {
        /// <summary>
        /// Creates a transformation between two coordinate systems.
        /// </summary>
        /// <remarks>
        /// This method will examine the coordinate systems in order to construct
        /// a transformation between them. This method may fail if no path between
        /// the coordinate systems is found, using the normal failing behavior of
        /// the DCP (e.g. throwing an exception).</remarks>
        /// <param name="sourceCS">Source coordinate system</param>
        /// <param name="targetCS">Target coordinate system</param>
        /// <returns>A coordinate transformation object</returns>
        /// <remarks>
        /// For this method to work properly, the input coordinate systems must either be 
        /// <see cref="DotSpatialCoordinateSystem"/>s or return valid <see cref="IInfo.WKT"/> values.
        /// </remarks>
        public new ICoordinateTransformation CreateFromCoordinateSystems(CoordinateSystem sourceCS, CoordinateSystem targetCS)
        {
            if (sourceCS == null || targetCS == null)
                return null;

            var source = new DotSpatialCoordinateSystem(ProjectionInfo.FromEsriString(sourceCS.WKT));

            var target = new DotSpatialCoordinateSystem(ProjectionInfo.FromEsriString(targetCS.WKT));

            return new DotSpatialCoordinateTransformation(source, target);
        }
    }
}
