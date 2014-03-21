using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// A collection of features
    /// </summary>
    /// <typeparam name="T">The type of the features object identifiers</typeparam>
    [Serializable]
    public class FeatureCollection<T> : Collection<IFeature<T>>, IFeatureCollection, IHasFeatureFactory where T : IComparable<T>, IEquatable<T>
    {
        private readonly IFeatureFactory<T> _factory;
        private readonly Dictionary<T, int> _oidToIndex = new Dictionary<T, int>();

        /// <summary>
        /// Creates an instance of this class, filled with the given <paramref name="features"/>.
        /// </summary>
        /// <param name="features">The features to add to the collection</param>
        public FeatureCollection(IEnumerable<IFeature<T>> features)
        {
            Name = "FC" + Guid.NewGuid();
            _factory = (IFeatureFactory<T>)features.First().Factory;
            foreach (var feature in features)
            {
                Add(feature);
            }

        }

        /// <summary>
        /// Creates a new feature collection based on the provided <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection"></param>
        protected FeatureCollection(FeatureCollection<T> collection)
            : base(collection)
        {
            Name = collection.Name;
            _factory = (IFeatureFactory<T>)collection.Factory;
        }

        /// <summary>
        /// Creates an instance of this class just initialized with the <paramref name="factory"/>
        /// </summary>
        /// <param name="factory">The feature factory</param>
        public FeatureCollection(IFeatureFactory<T> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Gets a value indicating the name of the feature collection
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a feature by its object identifier
        /// </summary>
        /// <param name="oid">The object identifier</param>
        /// <returns>The feature associated with the identifier, if present, otherwise <c>null</c></returns>
        IFeature IFeatureCollection.this[object oid]
        {
            get
            {
                int index;
                if (_oidToIndex.TryGetValue((T) oid, out index))
                    return this[index];
                return null;
            }
        }

        /// <summary>
        /// Gets a feature by an index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The feature at the index, if present, otherwise <c>null</c></returns>
        IFeature IFeatureCollection.this[int index]
        {
            get { return this[index]; }
        }

        /// <summary>
        /// Gets the factory to create new feature instances
        /// </summary>
        public IFeatureFactory Factory
        {
            get { return _factory; }
        }

        /// <summary>
        /// Method to get a feature by its object identifier
        /// </summary>
        /// <param name="oid">The object identifier</param>
        /// <returns>A feature if present, otherwise <c>null</c></returns>
        public IFeature GetFeatureByOid(object oid)
            {
                int index;
                if (_oidToIndex.TryGetValue((T) oid, out index))
                    return this[index];
                return null;
        }

        /// <summary>
        /// Returns a clone of this collection which is empty but may contain the same kind of features
        /// </summary>
        /// <returns>A feature collection</returns>
        public IFeatureCollection Clone()
        {
            return new FeatureCollection<T>((IFeatureFactory<T>)_factory.Clone());
        }

        public void AddRange(IEnumerable<IFeature> features)
        {
            foreach (var feature in features)
            {
                Add((IFeature<T>)feature);
            }
        }

        protected override void InsertItem(int index, IFeature<T> item)
        {
            if (!ReferenceEquals(Factory, item.Factory))
            {
                throw new ArgumentException("The item to insert was not created by this collections factory", "item");
            }
            base.InsertItem(index, item);
            _oidToIndex.Add(item.Oid, index);
        }

        protected override void SetItem(int index, IFeature<T> item)
        {
            if (!ReferenceEquals(Factory, item.Factory))
            {
                throw new ArgumentException("The item to insert was not created by this collections factory", "item");
            }
            if (_factory is FeatureFactory<T>)
            { ((FeatureFactory<T>)_factory).AssignOidIfNotAlreadyDone((Feature<T>)item); }
            
            base.SetItem(index, item);
            _oidToIndex[item.Oid] = index;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _oidToIndex.Clear();
        }

        protected override void RemoveItem(int index)
        {
            var f = this[index];
            base.RemoveItem(index);
            _oidToIndex.Remove(f.Oid);
        }

        IEnumerator<IFeature> IEnumerable<IFeature>.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<IFeature>.Add(IFeature item)
        {
            Add(item as IFeature<T>);
        }

        bool ICollection<IFeature>.Contains(IFeature item)
        {
            if (!(item is IFeature<T>))
                return false;
            return Contains(item as IFeature<T>);
        }

        void ICollection<IFeature>.CopyTo(IFeature[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException();
            
            if (arrayIndex <= 0)
                throw new ArgumentException("Negative array index", "arrayIndex");

            if (array.Length < Count + arrayIndex)
                throw new ArgumentException("Insufficient size", "array");

            for (var i = 0; i < Count; i++)
                array[i + arrayIndex] = this[i];
        }

        bool ICollection<IFeature>.Remove(IFeature item)
        {
            return Remove(item as IFeature<T>);
        }

        bool ICollection<IFeature>.IsReadOnly
        {
            get { return ((ICollection<IFeature<T>>)this).IsReadOnly; }
        }

        IList<IFeatureAttributeDefinition> IFeatureCollection.AttributesDefinition { get
        {
            return _factory.AttributesDefinition;
        }}
    }

    //public class FeatureSet<T> : HashSet<IFeature>, IFeatureSet where T : IComparable<T>, IEquatable<T>
    //{
    //    public FeatureSet(FeatureCollection<T> collection) 
    //        : base(collection)
    //    {
    //        Factory = collection.Factory;
    //    }

    //    public IFeatureFactory Factory { get; private set; }
    //}
}