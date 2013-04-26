using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Features
{
    /// <summary>
    /// Utility class for the creation of common FeatureFactories
    /// </summary>
    public static class FeatureFactory
    {
        public static FeatureFactory<int> CreateInt32(IGeometryFactory factory,
            params FeatureAttributeDefinition[] attributes)
        {
            return new FeatureFactory<int>(-1, i => i + 1, factory, attributes);
        }

        public static FeatureFactory<long> CreateInt64(IGeometryFactory factory,
            params FeatureAttributeDefinition[] attributes)
        {
            return new FeatureFactory<long>(-1, i => i + 1, factory, attributes);
        }

        public static FeatureFactory<Guid> CreateGuid(IGeometryFactory factory,
            params FeatureAttributeDefinition[] attributes)
        {
            return new FeatureFactory<Guid>(Guid.Empty, i => Guid.NewGuid(), factory, attributes);
        }

        public static FeatureFactory<string> CreateString(Func<string, string> oidGenerator, IGeometryFactory factory,
            params FeatureAttributeDefinition[] attributes)
        {
            if (oidGenerator == null)
            {
                oidGenerator = delegate(string t)
                {
                    if (string.IsNullOrEmpty(t))
                        return "Oid0";

                    long nr;
                    if (!long.TryParse(t.Substring(3), NumberStyles.Any,
                                       NumberFormatInfo.InvariantInfo, out nr))
                    {
                        throw new InvalidOperationException();
                    }
                    return "Oid" + (++nr).ToString(NumberFormatInfo.InvariantInfo);
                };
            }
            return new FeatureFactory<string>(null, oidGenerator, factory, attributes);
        }
    }
    
    
    [Serializable]
    public class FeatureFactory<TEntity> : EntityOidGenerator<TEntity>, IFeatureFactory<TEntity>
        where TEntity: IComparable<TEntity>, IEquatable<TEntity>
    {
        private readonly ReadOnlyCollection<IFeatureAttributeDefinition> _indexToName;
        internal readonly Dictionary<string, int> AttributeIndex;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="factory">The geometry factory</param>
        /// <param name="attributes">The attribute definition</param>
        /// <param name="startOid">The value the last generated Oid is set to.</param>
        /// <param name="oidGenerator">A delegate function that produces a new oid, based on the last one provided</param>
        internal FeatureFactory(
            TEntity startOid, Func<TEntity, TEntity> oidGenerator,
            IGeometryFactory factory, 
            params FeatureAttributeDefinition[] attributes)
            :base(startOid, oidGenerator)
        {
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

        public IList<IFeatureAttributeDefinition> AttributesDefinition { get { return _indexToName; }}

        public IFeature Create()
        {
            var feature = new Feature<TEntity>(this);
            return feature;
        }

        public IFeature Create(IGeometry geometry)
        {
            var res = Create();
            res.Geometry = geometry;
            return res;
        }
    }
}