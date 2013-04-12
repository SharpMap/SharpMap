using System;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for objects that have an object identifier
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnique<T>
        where T : IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Gets a value indicating the objects's identifier (Oid)
        /// </summary>
        T Oid { get; }
    }
}