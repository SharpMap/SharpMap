using System;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Features
{
    /// <summary>
    /// Sample implementation of a <see cref="IFeature{T}"/>.
    /// </summary>
    public class Feature : Entity<int>, IFeature<int>
    {
        /// <summary>
        /// Creates a feature
        /// </summary>
        /// <param name="factory">The factory that created the feature</param>
        internal Feature(FeatureFactory factory)
        {
            Factory = factory;
            Attributes = new FeatureAttributes(factory);
        }

        private Feature(IFeatureFactory<int> factory, int oid, IGeometry geometry, IFeatureAttributes attributes)
        {
            Factory = factory;
            Attributes = attributes;
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
            return new Feature(Factory, Oid, Geometry, (IFeatureAttributes)Attributes.Clone());
        }

        public IFeatureFactory<int> Factory { get; private set; }
        
        public IGeometry Geometry { get; set; }
        
        public IFeatureAttributes Attributes { get; private set; }
        
        public void Dispose()
        {
        }
    }
}