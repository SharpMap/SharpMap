using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    public class RelationProvider<T> : IRelationProvider
        where T : ILocationProvider
    {
        static RelationProvider()
        {
            Factory = new NetTopologySuite.Geometries.GeometryFactory();
        }

        private readonly T _locationProvider;

        public RelationProvider(T locationProvider)
        {
            _locationProvider = locationProvider;
        }

        protected static IGeometryFactory Factory { get; private set; }

        public IEnumerable<KeyValuePair<KeyValuePair<ushort, IPoint>, KeyValuePair<ushort, IPoint>>> Relations(ushort? restrict)
        {
            if (!restrict.HasValue)
            {
                var skip = 0;
                foreach (var origin in this)
                {
                    foreach (var destination in this.Skip(skip))
                        yield return
                            new KeyValuePair<KeyValuePair<ushort, IPoint>, KeyValuePair<ushort, IPoint>>(origin,
                                                                                                             destination);
                    skip++;
                }
                yield break;
            }

            var id = restrict.Value;
            var pt = this[id];
            var fixOrigin = new KeyValuePair<ushort, IPoint>(id, pt);
            foreach (var destin in this)
            {
                if (destin.Key == id)
                {
                    foreach (var origin in this)
                    {
                        yield return new KeyValuePair<KeyValuePair<ushort, IPoint>,
                            KeyValuePair<ushort, IPoint>>(origin, destin);
                    }
                }
                else
                    yield return new KeyValuePair<KeyValuePair<ushort, IPoint>, KeyValuePair<ushort, IPoint>>(fixOrigin, destin);
            }
        }

        public IEnumerator<KeyValuePair<ushort, IPoint>> GetEnumerator()
        {
            return _locationProvider.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IPoint this[ushort id]
        {
            get { return _locationProvider[id]; }
            set { _locationProvider[id] = value; }
        }

        public ICollection<IPoint> Locations
        {
            get { return _locationProvider.Locations; }
        }

        public int Count
        {
            get { return _locationProvider.Count; }
        }

        public virtual void Add(ushort id, IPoint point)
        {
            _locationProvider.Add(id, point);
        }

        public virtual void Remove(ushort id)
        {
            _locationProvider.Remove(id);
        }
    }
}