using System.Collections.ObjectModel;
using GeoAPI.Features;

namespace SharpMap.Features.Poco
{
    public class PocoFeatureAttributesProxy : IFeatureAttributes
    {
        private readonly PocoFeature _poco;
        private readonly ReadOnlyCollection<PocoFeatureAttributeDefinition> _att;
        
        public PocoFeatureAttributesProxy(PocoFeature poco)
        {
            _poco = poco;
            _att = poco.AttributesDefinition;
        }

        public object Clone()
        {
            return new PocoFeatureAttributesProxy((PocoFeature)_poco.Clone());
        }

        object IFeatureAttributes.this[int index]
        {
            get { return _att[index].GetValue(_poco); }
            set { _att[index].SetValue(_poco, value); }
        }

        object IFeatureAttributes.this[string key]
        {
            get { return ((IFeatureAttributes)this)[_poco.GetOrdinal(key)]; }
            set { ((IFeatureAttributes)this)[_poco.GetOrdinal(key)] = value; }
        }

        public object[] GetValues()
        {
            var res = new object[_att.Count];
            var i = 0;
            foreach (var attDef in _att)
            {
                res[i++] = attDef.GetValue(_poco);
            }
            return res;
        }
    }
}