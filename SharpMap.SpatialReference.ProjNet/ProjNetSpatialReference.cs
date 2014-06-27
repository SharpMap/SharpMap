using System;
using GeoAPI.CoordinateSystems;
using GeoAPI.Features;
using GeoAPI.SpatialReference;

namespace SharpMap.SpatialReference
{
    [Serializable]
    public class ProjNetSpatialReference : ISpatialReference
    {
        private static volatile int _newId = 1000000;
        private readonly string _oid;

        public ProjNetSpatialReference(string oid, ICoordinateSystem cs)
        {
            _oid = oid;
            Definition = cs.WKT;
            CoordinateSystem = cs;
        }

        public ProjNetSpatialReference(ICoordinateSystem cs)
        {
            var authorityCode = cs.AuthorityCode;
            if (authorityCode <= 0)
            {
                authorityCode = ++_newId;
            }
            _oid = cs.Authority ?? "SM" + ":" + authorityCode;
            Definition = cs.WKT;
            CoordinateSystem = cs;
        }

        object IUnique.Oid 
        { 
            get { return Oid; }
            set { Oid = (string) value; }
        }

        public string Oid
        {
            get { return _oid; }
            set {  }
        }

        public bool HasOidAssigned { get { return true;}}

        public Type GetEntityType()
        {
            return GetType();
        }

        public string Definition { get; internal set; }
        
        public SpatialReferenceDefinitionType DefinitionType { get{ return SpatialReferenceDefinitionType.WellKnownText; } }

        public ICoordinateSystem CoordinateSystem { get; private set; }
    }
}