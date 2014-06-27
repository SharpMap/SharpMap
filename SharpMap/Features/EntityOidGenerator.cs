using System;
using System.Collections.Generic;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// Class that can produce unique values for use as <see cref="IUnique{T}.Oid"/> values
    /// </summary>
    /// <typeparam name="TEntity">The type of the object identifier (Oid)</typeparam>
    [Serializable]
    public class EntityOidGenerator<TEntity> where TEntity : IComparable<TEntity>, IEquatable<TEntity>
    {
        protected readonly TEntity StartOid;
        private TEntity _lastOid;
        protected readonly Func<TEntity, TEntity> NewOidGenerator;
        private readonly HashSet<TEntity> _givenIds = new HashSet<TEntity>();

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="newOidGenerator">A delegate function that produces a new oid, based on the last one provided</param>
        public EntityOidGenerator(Func<TEntity, TEntity> newOidGenerator)
            :this(default(TEntity), newOidGenerator)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="startOid">The value the last generated Oid is set to.</param>
        /// <param name="newOidGenerator">A delegate function that produces a new oid, based on the last one provided</param>
        public EntityOidGenerator(TEntity startOid, Func<TEntity, TEntity> newOidGenerator)
        {
            if (newOidGenerator == null)
            {
                throw new ArgumentNullException("newOidGenerator");
            }
            if (newOidGenerator(startOid).Equals(startOid))
            {
                throw new ArgumentException("The provided generator does not produce new oid's", "newOidGenerator");
            }
            StartOid = startOid;
            _lastOid = startOid;
            NewOidGenerator = newOidGenerator;
        }

        /// <summary>
        /// Function to generate a new Oid value
        /// </summary>
        /// <returns>A new Oid, one that this generator has not generated yet.</returns>
        public TEntity GetNewOid()
        {
            while (_givenIds.Contains(_lastOid = NewOidGenerator(_lastOid))) 
            {}
            return _lastOid;
        }

        public void AssignOidIfNotAlreadyDone(Unique<TEntity> item)
        {
            if (item.HasOidAssigned)
                return;
            item.Oid = GetNewOid();
        }
    }
}