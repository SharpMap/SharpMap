using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    public class LocationProvider : ILocationProvider
    {
        private readonly Dictionary<ushort, IPoint> _points;

        public LocationProvider()
        {
            _points = new Dictionary<ushort, IPoint>();
        }

        public LocationProvider(IEnumerable<KeyValuePair<ushort, IPoint>> points)
            :this()
        {
            foreach (var keyValuePair in points)
            {
                _points.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public IEnumerator<KeyValuePair<ushort, IPoint>> GetEnumerator()
        {
            foreach (var point in _points)
                yield return new KeyValuePair<ushort, IPoint>(point.Key, point.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IPoint this[ushort id]
        {
            get { return _points[id]; }
            set
            {
                if (value == null)
                    throw new InvalidOperationException();

                _points[id] = (IPoint)value.Clone();
            }
        }

        public ICollection<IPoint> Locations
        {
            get { return _points.Values; }
        }

        public int Count
        {
            get { return _points.Count; }
        }

        public virtual void Add(ushort id, IPoint point)
        {
            if (_points.ContainsKey(id))
                throw new InvalidOperationException("Cannot add another location with this id");
            _points.Add(id, (IPoint)point.Clone());
        }

        public virtual void Remove(ushort id)
        {
            _points.Remove(id);
        }
    }
}