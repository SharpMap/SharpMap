using System;
using System.Collections.ObjectModel;
using System.Linq;
using GeoAPI.Features;

namespace SharpMap.Features
{
    [Serializable]
    public class FeatureCollectionSet : Collection<IFeatureCollection>, IFeatureCollectionSet
    {
        public IFeatureCollection this[string name]
        {
            get
            {
                return this.FirstOrDefault(t => t.Name == name);
            }
        }

        public bool Contains(string name)
        {
            return this[name] != null;
        }

        public bool Remove(string name)
        {
            var fcs = this[name];
            if (fcs != null)
                return Remove(fcs);
            return false;
        }
    }
}