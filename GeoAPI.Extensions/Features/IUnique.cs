using System;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for objects that have an object identifier
    /// </summary>
    public interface IUnique
    {
        /// <summary>
        /// Gets or sets a value indicating the objects's identifier (Oid)
        /// </summary>
        /// <remarks>Can be interpreted as index in a collection of features that don't have a <i>unique</i> object identifier.</remarks>
        [FeatureAttribute(AttributeDescription = "This feature's object identifier")]
        object Oid { get; set; }

        /// <summary>
        /// Function to get the real entities type.
        /// <remarks>Required in case if object is wrapped with a proxy class.</remarks>
        /// </summary>
        /// <returns>The entity's type</returns>
        Type GetEntityType();

        /// <summary>
        /// Gets a value indicating that the <see cref="Oid"/> has been assigned at least once.
        /// </summary>
        bool HasOidAssigned { get; }
    }

    /// <summary>
    /// Interface for objects that have an object identifier
    /// </summary>
    /// <typeparam name="T">The type of the identifier</typeparam>
    public interface IUnique<T> : IUnique
        where T : /*struct, */IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Gets or sets a value indicating the objects's identifier (Oid)
        /// </summary>
        /// <remarks>Can be interpreted as index in a collection of features that don't have a <i>unique</i> object identifier.</remarks>
        [FeatureAttribute(AttributeDescription = "This feature's object identifier")]
        new T Oid { get; set; }
    }
}