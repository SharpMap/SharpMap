using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Features
{
    [Serializable]
    public class FeatureFactory<TEntity> : IFeatureFactory<TEntity>
        where TEntity: IComparable<TEntity>, IEquatable<TEntity>
    {
        private readonly EntityOidGenerator<TEntity> _oidGenerator;
 
        private readonly ReadOnlyCollection<IFeatureAttributeDefinition> _indexToName;
        internal readonly Dictionary<string, int> AttributeIndex;

        public FeatureFactory(EntityOidGenerator<TEntity> oidGenerator, IGeometryFactory factory, params IFeatureAttributeDefinition[] attributes)
        {
            if (oidGenerator == null)
            {
                throw new ArgumentNullException("oidGenerator");
            }
            _oidGenerator = oidGenerator;

            var list = new List<IFeatureAttributeDefinition>(attributes.Length + 1);
            list.Add(new FeatureAttributeDefinition { AttributeName = "Oid", AttributeType = typeof(TEntity), AttributeDescription = "Object Id", IsNullable = false });
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

        public IFeature Create()
        {
            var feature = new Feature<TEntity>(this) { Oid = UnassignedOid };
            return feature;
        }

        public IFeature Create(IGeometry geometry)
        {
            var res = Create();
            res.Geometry = geometry;
            return res;
        }

        public TEntity GetNewOid()
        {
            return _oidGenerator.GetNewOid();
        }

        public TEntity UnassignedOid
        {
            get { return _oidGenerator.UnassignedOid; }
        }
    }
}