using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Features;

namespace SharpMap.Features
{
    public class FeatureCollection<T> : Collection<IFeature>, IFeatureCollection where T : IComparable<T>, IEquatable<T>
    {
        protected FeatureCollection(FeatureCollection<T> collection)
            : base(collection)
        {
            Factory = collection.Factory;
        }

        public FeatureCollection(IFeatureFactory factory)
        {
            Factory = factory;
        }
        
        public IFeatureFactory Factory { get; private set; }
        
        public IFeature New()
        {
            return Factory.Create();
        }

        protected override void InsertItem(int index, IFeature item)
        {
            if (!ReferenceEquals(Factory, item.Factory))
            {
                throw new ArgumentException("The item to insert was not created by this collections factory", "item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IFeature item)
        {
            if (!ReferenceEquals(Factory, item.Factory))
            {
                throw new ArgumentException("The item to insert was not created by this collections factory", "item");
            }
            base.SetItem(index, item);
        }
    }

    public class FeatureSet<T> : SortedSet<IFeature>, IFeatureSet where T : IComparable<T>, IEquatable<T>
    {
        public FeatureSet(FeatureCollection<T> collection) 
            : base(collection)
        {
            Factory = collection.Factory;
        }

        public IFeatureFactory Factory { get; private set; }
    }
}