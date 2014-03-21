using System;
using System.ComponentModel;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Features
{
    /// <summary>
    /// Sample implementation of a <see cref="IFeature{T}"/>.
    /// </summary>
    [Serializable]
    public class Feature<T> : Entity<T>, IFeature<T>, INotifyPropertyChanged
        where T : IComparable<T>, IEquatable<T>
    {
        private readonly FeatureFactory<T> _factory;
        private readonly FeatureAttributes<T> _attributes;
        private IGeometry _geometry;

        /// <summary>
        /// Creates a feature
        /// </summary>
        /// <param name="factory">The factory that created the feature</param>
        internal Feature(FeatureFactory<T> factory)
        {
            _factory = factory;
            _attributes = new FeatureAttributes<T>(this);
        }

        private Feature(FeatureFactory<T> factory, T oid, IGeometry geometry, FeatureAttributes<T> attributes)
        {
            _factory = factory;
            _attributes = attributes;
            Oid = oid;
            Geometry = geometry;
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

        public IGeometry Geometry
        {
            get { return _geometry; }
            set
            {
                if (ReferenceEquals(value, _geometry))
                    return;
                _geometry = value;
            }
        }

        public IFeatureAttributes Attributes
        {
            get { return _attributes; }
        }

        public void Dispose()
        {
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        internal int GetOrdinal(string key)
        {
            return _factory.AttributeIndex[key];
        }

        internal string GetFieldName(int index)
        {
            return _factory.AttributesDefinition[index].AttributeName;
        }
    }
}