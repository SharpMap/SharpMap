using DotSpatial.Projections;
using GeoAPI.SpatialReference;

namespace SharpMap.SpatialReference
{
    /// <summary>
    /// 
    /// </summary>
    public class DotSpatialSpatialReference : ISpatialReference
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="definition"></param>
        public DotSpatialSpatialReference(string oid, string definition)
        {
            Oid = oid;
            Definition = definition;
            ProjectionInfo = ProjectionInfo.FromProj4String(definition);

        }

        public string Oid { get; private set; }
        public string Definition { get; private set; }
        
        /// <summary>
        /// Gets the projection info
        /// </summary>
        public ProjectionInfo ProjectionInfo { get; private set; }

        public int CompareTo(string other)
        {
            throw new System.NotImplementedException();
        }

        public bool Equals(string other)
        {
            
        }
    }
}