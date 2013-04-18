using System;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Features
{
    /// <summary>
    /// Sample implementation of a <see cref="IFeature{T}"/>.
    /// </summary>
    [Serializable]
    public class Feature<T> : Entity<T>, IFeature<T> where T : IComparable<T>, IEquatable<T>
    {
        private readonly FeatureFactory<T> _factory;
        private readonly FeatureAttributes<T> _attributes;

        /// <summary>
        /// Creates a feature
        /// </summary>
        /// <param name="factory">The factory that created the feature</param>
        internal Feature(FeatureFactory<T> factory)
        {
            _factory = factory;
            _attributes = new FeatureAttributes<T>(factory);
        }

        private Feature(FeatureFactory<T> factory, T oid, IGeometry geometry, FeatureAttributes<T> attributes)
        {
            _factory = factory;
            _attributes = attributes;
            Geometry = geometry;
            Oid = oid;
        }

        protected override void OnOidChanged(EventArgs e)
        {
            Attributes[0] = Oid;
            base.OnOidChanged(e);
        }

        public object Clone()
        {
            return new Feature<T>(_factory, Oid, Geometry, (FeatureAttributes<T>)_attributes.Clone());
        }

        public IFeatureFactory Factory
        {
            get { return _factory; }
        }

        public IGeometry Geometry { get; set; }
        
        public IFeatureAttributes Attributes
        {
            get { return _attributes; }
        }

        public void Dispose()
        {
        }
    }
}