using System;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// Entity implementation
    /// </summary>
    /// <typeparam name="T">The type of the objects identifier</typeparam>
    [Serializable]
    public class Entity<T> : IEntity<T> where T : IComparable<T>, IEquatable<T>
    {
        private int? _requestedHashCode;
        private T _oid;

        public Entity()
        {}

        protected Entity(Entity<T> entity)
        {
            _oid = entity.Oid;
        }

        /// <summary>
        /// Event raised when the <see cref="Oid"/> has changed
        /// </summary>
        public event EventHandler OidChanged;

        /// <summary>
        /// Gets or sets a value indicating the objects's identifier (Oid)
        /// </summary>
        public T Oid
        {
            get { return _oid; }
            set
            {
                if (Equals(value, default(T)))
                {
                    throw new ArgumentException("value");
                }

                if (value.Equals(_oid))
                {
                    return;
                }

                _oid = value;
                OnOidChanged(EventArgs.Empty);
            }
        }

        [FeatureAttribute(Ignore = true)]
        object IEntity.Oid { get { return Oid; } set { Oid = (T) value; } }


        /// <summary>
        /// Event invoker for the <see cref="OidChanged"/> event
        /// </summary>
        /// <param name="e">The events argument</param>
        protected virtual void OnOidChanged(EventArgs e)
        {
            if (OidChanged != null)
            {
                OidChanged(this, e);
            }
        }

        /// <summary>
        /// Function to get the real entity's type.
        /// <remarks>This is required in case if object is wrapped with a proxy class.</remarks>
        /// </summary>
        /// <returns>The entity's type</returns>
        public virtual Type GetEntityType()
        {
            return GetType();
        }

        /// <summary>
        /// Gets a value indicating that the <see cref="IEntity.Oid"/> has been assigned at least once.
        /// </summary>
        public bool HasOidAssigned
        {
            get { return !Equals(Oid, default(T)); }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj is IEntity<T>)
            {
                return Equals((IEntity<T>) obj);
            }
            return false;
        }

        public virtual bool Equals(IEntity<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (HasOidAssigned && other.HasOidAssigned && Oid.Equals(other.Oid))
            {
                var otherType = other.GetEntityType();
                var thisType = GetEntityType();

                return thisType.IsAssignableFrom(otherType) || otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            // The value of GetHashCode should not be allowed to change during an objects lifetime!
            // If it does, it leads to crazy problems with Dictionaries etc (as they cache the value)

// ReSharper disable NonReadonlyFieldInGetHashCode
// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
            if (_requestedHashCode.HasValue)
            {
                return _requestedHashCode.Value;
            }

            var hashCode = HasOidAssigned ? Oid.GetHashCode() : base.GetHashCode();
            _requestedHashCode = hashCode;
            return hashCode;
// ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
// ReSharper restore NonReadonlyFieldInGetHashCode
        }

        public static bool operator ==(Entity<T> left, Entity<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Entity<T> left, Entity<T> right)
        {
            return !Equals(left, right);
        }
    }
}