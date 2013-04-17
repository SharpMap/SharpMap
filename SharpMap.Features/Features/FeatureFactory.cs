using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Features
{
    public class FeatureFactory : IFeatureFactory<int>
    {
        private static int _newOid = -1;

        private readonly ReadOnlyCollection<IFeatureAttributeDefinition> _indexToName;
        internal readonly Dictionary<string, int> AttributeIndex;
        private readonly HashSet<int> _givenIds = new HashSet<int>();

        public FeatureFactory(IGeometryFactory factory, params IFeatureAttributeDefinition[] attributes)
        {

            var list = new List<IFeatureAttributeDefinition>(attributes.Length + 1);
            list.Add(new FeatureAttributeDefinition { AttributeName = "Oid", AttributeType = typeof(int), AttributeDescription = "Object Id", IsNullable = false });
            list.AddRange(attributes);
            _indexToName = new ReadOnlyCollection<IFeatureAttributeDefinition>(list);
            AttributeIndex = new Dictionary<string, int>(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                AttributeIndex.Add(attributes[i].AttributeName, i);
            }
            GeometryFactory = factory;
        }

        public IGeometryFactory GeometryFactory { get; private set; }

        public IList<IFeatureAttributeDefinition> Attributes { get { return _indexToName; }}

        public IFeature<int> Create()
        {
            var feature = new Feature(this) {Oid = -1};
            return feature;
        }

        internal int GetNewId()
        {
            Interlocked.Increment(ref _newOid);
            while (_givenIds.Contains(_newOid))
                Interlocked.Increment(ref _newOid);
            _givenIds.Add(_newOid);
            return _newOid;
        }

        public IFeature<int> Create(IGeometry geometry)
        {
            var res = Create();
            res.Geometry = geometry;
            return res;
        }
    }
}