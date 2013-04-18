using System.Collections.Generic;

namespace GeoAPI.Features
{
    public interface IFeatureSet : ISet<IFeature>
    {
        IFeatureFactory Factory { get; }
    }
}