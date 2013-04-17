using GeoAPI.SpatialReference;

namespace SharpMap.SpatialReference
{
    public class DotSpatialProjectionsSpatialReferenceFactory : ISpatialReferenceFactory
    {
        public ISpatialReference Create(string definition)
        {
            return Create(definition, definition);
        }

        public ISpatialReference Create(string oid, string definition)
        {
            return new DotSpatialSpatialReference(definition, definition);
        }
    }
}