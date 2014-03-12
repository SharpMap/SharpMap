using GeoAPI.Features;

namespace SharpMap.Features.Poco
{
    public enum PoIKind
    {
        Undefined, Hotel, Bar, Sight, Restaurant,
    }
    
    public class PoIFeature : PocoFeature
    {
        private static readonly EntityOidGenerator<long> _oidGenerator = new EntityOidGenerator<long>(-1, t => t + 1);
        
        private string _name;
        private PoIKind _kind;

        public PoIFeature()
        {
            //Oid = _oidGenerator.UnassignedOid;
        }

        public PoIFeature(PoIFeature feature)
            :base(feature)
        {
            Name = feature.Name;
            Kind = feature.Kind;
        }


        public override object Clone()
        {
            return new PoIFeature(this);
        }

        [FeatureAttribute(AttributeDescription = "Name of the feature")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [FeatureAttribute(AttributeDescription = "Kind of the feature")]
        public PoIKind Kind 
        {
            get { return _kind; }
            set
            {
                if (value == _kind) return;
                _kind = value;
                OnPropertyChanged("Kind");
            }
        }

        public override IFeature Create()
        {
            return new PoIFeature();
        }

        public override long GetNewOid()
        {
            return _oidGenerator.GetNewOid();
        }
    }
}