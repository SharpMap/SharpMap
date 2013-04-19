using System;
using System.Collections.Generic;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// Class that can produce unique values for use as <see cref="IEntity{T}.Oid"/> values
    /// </summary>
    /// <typeparam name="TEntity">The type of the object identifier (Oid)</typeparam>
    [Serializable]
    public class EntityOidGenerator<TEntity> where TEntity : IComparable<TEntity>, IEquatable<TEntity>
    {
        private TEntity _lastOid;
        private readonly Func<TEntity, TEntity> _newOidGenerator;
        private readonly HashSet<TEntity> _givenIds = new HashSet<TEntity>();

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="unassignedOid">The value an entities Oid should have to mark it as unset</param>
        /// <param name="newOidGenerator">A delegate function that produces a new oid, based on the last one provided</param>
        public EntityOidGenerator(TEntity unassignedOid, Func<TEntity, TEntity> newOidGenerator)
            :this(unassignedOid, unassignedOid, newOidGenerator)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="unassignedOid">The value an entities Oid should have to mark it as unset</param>
        /// <param name="startOid">The value the last generated Oid is set to.</param>
        /// <param name="newOidGenerator">A delegate function that produces a new oid, based on the last one provided</param>
        public EntityOidGenerator(TEntity unassignedOid, TEntity startOid, Func<TEntity, TEntity> newOidGenerator)
        {
            if (newOidGenerator == null)
            {
                throw new ArgumentNullException("newOidGenerator");
            }
            if (newOidGenerator(startOid).Equals(startOid))
            {
                throw new ArgumentException("The provided generator does not produce new oid's", "newOidGenerator");
            }

            UnassignedOid = unassignedOid;
            _givenIds.Add(unassignedOid);
            _lastOid = startOid;
            _newOidGenerator = newOidGenerator;
        }

        /// <summary>
        /// Gets the "unassigned" Oid value
        /// </summary>
        public TEntity UnassignedOid { get; private set; }

        /// <summary>
        /// Function to generate a new Oid value
        /// </summary>
        /// <returns>A new Oid, one that this generator has not generated yet.</returns>
        public TEntity GetNewOid()
        {
            while (_givenIds.Contains(_lastOid = _newOidGenerator(_lastOid))) 
            {}
            return _lastOid;
        }

        public void AssignOid(Entity<TEntity> item)
        {
            // Test for class Oids
            if (typeof (TEntity).IsClass && ReferenceEquals(UnassignedOid, null))
            {
                return;
            }

            // Test for value
            if (UnassignedOid.Equals(item.Oid))
            {
                return;
            }

            item.Oid = GetNewOid();
        }
    }
}