using System.Collections.Generic;

namespace GeoAPI.Features
{
    
    public interface IFeatureCollection : ICollection<IFeature>
    {
        IFeatureFactory Factory { get; }
    }
}