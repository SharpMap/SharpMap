using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Features;

namespace SharpMap.Features
{
    public class FeatureCollection : Collection<IFeature<int>>, IFeatureCollection<int>
    {
        protected FeatureCollection(FeatureCollection collection)
            : base(collection)
        {
            Factory = collection.Factory;
        }

        public FeatureCollection(IFeatureFactory<int> factory)
        {
            Factory = factory;
        }
        
        public IFeatureFactory<int> Factory { get; private set; }
        
        public IFeature<int> New()
        {
            return Factory.Create();
        }

        protected override void InsertItem(int index, IFeature<int> item)
        {
            if (!ReferenceEquals(Factory, item.Factory))
            {
                throw new ArgumentException("The item to insert was not created by this collections factory", "item");
            }
            base.InsertItem(index, item);
            item.Oid = index;
        }

        protected override void SetItem(int index, IFeature<int> item)
        {
            if (!ReferenceEquals(Factory, item.Factory))
            {
                throw new ArgumentException("The item to insert was not created by this collections factory", "item");
            }
            base.SetItem(index, item);
            item.Oid = index;
        }
    }

    public class FeatureSet : SortedSet<IFeature<int>>, IFeatureSet<int>
    {
        public FeatureSet(FeatureCollection collection) 
            : base(collection)
        {
            Factory = collection.Factory;
        }

        public IFeatureFactory<int> Factory { get; private set; }
    }
}