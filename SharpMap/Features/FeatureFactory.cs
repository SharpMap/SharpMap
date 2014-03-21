using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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
            return new FeatureFactory<int>(0, i => i + 1, factory, attributes);
        }

        public static FeatureFactory<uint> CreateUInt32(IGeometryFactory factory,
            params FeatureAttributeDefinition[] attributes)
        {
            return new FeatureFactory<uint>(0, i => i + 1, factory, attributes);
        }
        public static FeatureFactory<long> CreateInt64(IGeometryFactory factory,
            params FeatureAttributeDefinition[] attributes)
        {
            return new FeatureFactory<long>(0, i => i + 1, factory, attributes);
        }

        public static FeatureFactory<ulong> CreateUInt64(IGeometryFactory factory,
            params FeatureAttributeDefinition[] attributes)
        {
            return new FeatureFactory<ulong>(0, i => i + 1, factory, attributes);
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
                AttributeIndex.Add(list[i].AttributeName, i);
            }
            GeometryFactory = factory;
        }

        public IGeometryFactory GeometryFactory { get; private set; }

        public IList<IFeatureAttributeDefinition> AttributesDefinition { get { return _indexToName; }}

        public IFeature Create()
        {
            var feature = new Feature<TEntity>(this);
            feature.Oid = GetNewOid();
            return feature;
        }

        public IFeature Create(IGeometry geometry)
        {
            var res = Create();
            res.Oid = GetNewOid();
            res.Geometry = geometry;
            return res;
        }

        /// <summary>
        /// Creates a clone of this feature factory using the same <see cref="EntityOidGenerator{T}"/>
        /// </summary>
        /// <returns>A clone of the feature factory</returns>
        public FeatureFactory<TEntity> Clone()
        {
            var attDef = new FeatureAttributeDefinition[_indexToName.Count - 1];
            for (var i = 1; i < _indexToName.Count; i++)
                attDef[i + 1] = new FeatureAttributeDefinition
                {
                    AttributeDescription = _indexToName[i].AttributeDescription,
                    AttributeName = _indexToName[i].AttributeName,
                    AttributeType = _indexToName[i].AttributeType,
                    IsNullable = _indexToName[i].IsNullable,
                    Default = _indexToName[i].Default
                };
            return new FeatureFactory<TEntity>(StartOid, NewOidGenerator, GeometryFactory, attDef.ToArray());
        }

        IFeatureFactory IFeatureFactory.Clone()
        {
            return Clone();
        }
    }
}