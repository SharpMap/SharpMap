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

        private readonly ReadOnlyCollection<string> _indexToName;
        internal readonly Dictionary<string, int> AttributeIndex;
        private readonly HashSet<int> _givenIds = new HashSet<int>();

        public FeatureFactory(IGeometryFactory factory, params string[] names)
        {
            
            var list = new List<string>(names.Length + 1);
            list.Add("Oid");
            list.AddRange(names);
            _indexToName = new ReadOnlyCollection<string>(list);
            AttributeIndex = new Dictionary<string, int>(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                AttributeIndex.Add(names[i], i);
            }
            GeometryFactory = factory;
        }

        public IGeometryFactory GeometryFactory { get; private set; }

        public IList<string> AttributeNames { get { return _indexToName; }}

        public IFeature<int> Create()
        {
            var feature = new Feature(this) {Oid = -1};
            return feature;
        }

        public int GetNewId()
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