using System;
using System.Collections.Generic;

namespace GeoAPI.Features
{
    
    public interface IFeatureCollection<T> : ICollection<IFeature<T>> where T : IComparable<T>, IEquatable<T>
    {
        IFeatureFactory<T> Factory { get; }

        IFeature<T> New();
    }
}