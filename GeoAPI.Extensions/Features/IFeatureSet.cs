using System;
using System.Collections.Generic;

namespace GeoAPI.Features
{
    public interface IFeatureSet<T> : ISet<IFeature<T>> where T : IComparable<T>, IEquatable<T>
    {
        IFeatureFactory<T> Factory { get; }
    }
}