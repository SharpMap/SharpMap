using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GeoAPI.Features;
using GeoAPI.Geometries;
using SharpMap.Annotations;

namespace SharpMap.Features.Poco
{
    public abstract class PocoFeature : Entity<long>, IFeature<long>, IFeatureFactory<long>, INotifyPropertyChanged
    {
        private static readonly object PocoLock = new object();
        protected static volatile PocoFeatureAttributesDefinition PocoFeatureAttributesDefinition;
        
        private IGeometry _geometry;

        public abstract object Clone();

        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets the factory that created this feature
        /// </summary>
        public IFeatureFactory Factory { get { return this; } }

        /// <summary>
        /// Gets or sets the geometry defining the feature
        /// </summary>
        public IGeometry Geometry
        {
            get { return _geometry; }
            set
            {
                if (ReferenceEquals(_geometry, value))
                    return;

                _geometry = value;
                OnPropertyChanged("Geometry");
            }
        }

        /// <summary>
        /// Gets the geometry factory to create features
        /// </summary>
        public IGeometryFactory GeometryFactory { get { return _geometry.Factory; } }

        [FeatureAttribute(Ignore = true)]
        public ReadOnlyCollection<PocoFeatureAttributeDefinition> AttributesDefinition
        {
            get
            {
                if (PocoFeatureAttributesDefinition == null)
                {
                    lock (PocoLock)
                    {
                        if (PocoFeatureAttributesDefinition == null)
                        {
                            var p = new PocoFeatureAttributesDefinition(GetType());
                            PocoFeatureAttributesDefinition = p;
                        }

                    }
                }
                return PocoFeatureAttributesDefinition.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets a list of attribute names
        /// </summary>
        [FeatureAttribute(Ignore = true)]
        IList<IFeatureAttributeDefinition> IFeatureFactory.Attributes
        {
            get
            {
                if (PocoFeatureAttributesDefinition == null)
                {
                    lock (PocoLock)
                    {
                        if (PocoFeatureAttributesDefinition == null)
                        {
                            var p = new PocoFeatureAttributesDefinition(GetType());
                            PocoFeatureAttributesDefinition = p;
                        }

                    }
                }
                return PocoFeatureAttributesDefinition.AsList();
            }
        }

        /// <summary>
        /// Creates a new feature
        /// </summary>
        /// <returns>A new feature with no geometry and attributes</returns>
        public abstract IFeature Create();

        /// <summary>
        /// Creates a new feature with <paramref name="geometry"/>, but no attributes
        /// </summary>
        /// <returns>A new feature with <paramref name="geometry"/>, but no attributes</returns>
        public virtual IFeature Create(IGeometry geometry)
        {
            var res = Create();
            res.Geometry = geometry;
            return res;
        }

        /// <summary>
        /// Gets the attributes associated with this feature
        /// </summary>
        IFeatureAttributes IFeature.Attributes { get { return new PocoFeatureAttributesProxy(this); } }

        public abstract long GetNewOid();

        public virtual long UnassignedOid { get { return -1; } }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}