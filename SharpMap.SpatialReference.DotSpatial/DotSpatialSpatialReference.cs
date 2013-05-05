using System;
using DotSpatial.Projections;
using GeoAPI.Features;
using GeoAPI.SpatialReference;

namespace SharpMap.SpatialReference
{
    /// <summary>
    /// DotSpatial.Projections.Projection info wrapper
    /// </summary>
    public class DotSpatialSpatialReference : ISpatialReference
    {
        private readonly string _oid;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public DotSpatialSpatialReference(ProjectionInfo projectionInfo)
        {
            _oid = projectionInfo.ToProj4String();
            Definition = projectionInfo.ToProj4String();
            ProjectionInfo = projectionInfo;

        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="definition"></param>
        public DotSpatialSpatialReference(string oid, string definition)
        {
            _oid = oid;
            Definition = definition;
            ProjectionInfo = ProjectionInfo.FromProj4String(definition);

        }

        object IEntity.Oid
        {
            get { return Oid; }
            set { Oid = (string)value; }
        }

        public string Oid
        {
            get { return _oid; }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool HasOidAssigned { get { return true;}}
        
        public Type GetEntityType()
        {
            return GetType();
        }

        public string Definition { get; private set; }

        public SpatialReferenceDefinitionType DefinitionType
        {
            get { return SpatialReferenceDefinitionType.Proj4; }
        }

        /// <summary>
        /// Gets the projection info
        /// </summary>
        public ProjectionInfo ProjectionInfo { get; private set; }
    }
}