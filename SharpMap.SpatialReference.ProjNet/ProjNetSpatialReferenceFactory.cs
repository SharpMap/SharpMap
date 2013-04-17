using System;
using GeoAPI.SpatialReference;
using ProjNet.CoordinateSystems;

namespace SharpMap.SpatialReference
{
    public class ProjNetSpatialReferenceFactory : ISpatialReferenceFactory
    {
        private readonly CoordinateSystemFactory _factory = new CoordinateSystemFactory();

        public ISpatialReference Create(string definition)
        {
            var cs = _factory.CreateFromWkt(definition);
            return new ProjNetSpatialReference(cs);
        }

        public ISpatialReference Create(string oid, string definition)
        {
            var cs = _factory.CreateFromWkt(definition);
            return new ProjNetSpatialReference(oid, cs);
        }
    }
}