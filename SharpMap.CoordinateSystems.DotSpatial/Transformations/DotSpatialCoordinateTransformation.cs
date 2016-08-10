using System;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;

namespace SharpMap.CoordinateSystems.Transformations
{
    /// <summary>
    /// A coordinate transformation base on two <see cref="DotSpatialCoordinateSystem"/>s.
    /// </summary>
    public class DotSpatialCoordinateTransformation : ICoordinateTransformation
    {
        private readonly DotSpatialCoordinateSystem _source;
        private readonly DotSpatialCoordinateSystem _target;
        private readonly DotSpatialMathTransform _transform;

        /// <summary>
        /// Creates an instance of this class using the provided coordinate systems
        /// </summary>
        /// <param name="source">The source coordinate system</param>
        /// <param name="target">The target coordinate system</param>
        public DotSpatialCoordinateTransformation(DotSpatialCoordinateSystem source, DotSpatialCoordinateSystem target)
        {
            _source = source;
            _target = target;
            _transform = new DotSpatialMathTransform(source.ProjectionInfo, target.ProjectionInfo);
        }


        /// <summary>
        /// Human readable description of domain in source coordinate system.
        /// </summary>
        public string AreaOfUse
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Authority which defined transformation and parameter values.
        /// </summary>
        /// <remarks>
        /// An Authority is an organization that maintains definitions of Authority Codes. For example the European Petroleum Survey Group (EPSG) maintains a database of coordinate systems, and other spatial referencing objects, where each object has a code number ID. For example, the EPSG code for a WGS84 Lat/Lon coordinate system is ‘4326’
        /// </remarks>
        public string Authority
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Code used by authority to identify transformation. An empty string is used for no code.
        /// </summary>
        /// <remarks>The AuthorityCode is a compact string defined by an Authority to reference a particular spatial reference object. For example, the European Survey Group (EPSG) authority uses 32 bit integers to reference coordinate systems, so all their code strings will consist of a few digits. The EPSG code for WGS84 Lat/Lon is ‘4326’.</remarks>
        public long AuthorityCode
        {
            get { return -1; }
        }

        /// <summary>Gets math transform.</summary>
        public IMathTransform MathTransform
        {
            get { return _transform; }
        }

        /// <summary>Name of transformation.</summary>
        public string Name
        {
            get { return string.Empty; }
        }

        /// <summary>Gets the provider-supplied remarks.</summary>
        public string Remarks
        {
            get { return string.Empty; }
        }

        /// <summary>Source coordinate system.</summary>
        public ICoordinateSystem SourceCS
        {
            get { return _source; }
        }

        /// <summary>Target coordinate system.</summary>
        public ICoordinateSystem TargetCS
        {
            get { return _target; }
        }

        /// <summary>
        /// Semantic type of transform. For example, a datum transformation or a coordinate conversion.
        /// </summary>
        public TransformType TransformType
        {
            get { throw new NotSupportedException(); }
        }
    }
}