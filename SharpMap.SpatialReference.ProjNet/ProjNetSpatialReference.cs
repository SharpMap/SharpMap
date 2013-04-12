using GeoAPI.CoordinateSystems;
using GeoAPI.SpatialReference;

namespace SharpMap.SpatialReference
{
    public class ProjNetSpatialReference : ISpatialReference
    {
        private volatile int _newId = 1000000;
        
        public ProjNetSpatialReference(ICoordinateSystem cs)
        {
            var authorityCode = cs.AuthorityCode;
            if (authorityCode <= 0) authorityCode = ++_newId;
            Oid = cs.Authority ?? "SM" + ":" + authorityCode;
            Definition = cs.WKT;
            CoordinateSystem = cs;
        }
        
        public string Oid { get; private set; }
        
        public string Definition { get; private set; }
        
        public SpatialReferenceDefinitionType DefinitionType { get{ return SpatialReferenceDefinitionType.WellKnownText; } }

        public ICoordinateSystem CoordinateSystem { get; private set; }
    }
}