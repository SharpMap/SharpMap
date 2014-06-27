using System;
using GeoAPI.Features;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;

namespace SharpMap.SpatialReference
{
    /// <summary>
    /// An <see cref="IReprojector"/> implementation, that does not reproject at all
    /// </summary>
    public class NoopReprojector : IReprojectorCore
    {
        private class NoopSpatialReferenceFactory : ISpatialReferenceFactory
        {
            public ISpatialReference Create(string definition)
            {
                return new NoopSpatialReference(definition);
            }
        }

        private struct NoopSpatialReference : ISpatialReference
        {
            private readonly string _definition;
            public NoopSpatialReference(string definition)
            {
                _definition = definition;
            }

            object IUnique.Oid
            {
                get { return _definition; }
                set { }
            }

            public string Oid { get { return _definition; } set {}}

            public bool HasOidAssigned { get { return true; } }

            public Type GetEntityType()
            {
                return GetType();
            }

            public string Definition { get { return _definition; } }

            public SpatialReferenceDefinitionType DefinitionType { get { return SpatialReferenceDefinitionType.Unknown;} }
        }

        public Coordinate Reproject(Coordinate coordinate, ISpatialReference @from, ISpatialReference to)
        {
            return coordinate;
        }

        public Envelope Reproject(Envelope envelope, ISpatialReference @from, ISpatialReference to)
        {
            return envelope;
        }

        public ICoordinateSequence Reproject(ICoordinateSequence sequence, ISpatialReference @from, ISpatialReference to)
        {
            return sequence;
        }

        public ISpatialReferenceFactory Factory { get { return new NoopSpatialReferenceFactory(); } }
    }
}