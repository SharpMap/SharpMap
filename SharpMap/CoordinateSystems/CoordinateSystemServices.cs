using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GeoAPI;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using SharpMap.Converters.WellKnownText;

namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// A coordinate system services class
    /// </summary>
    public class CoordinateSystemServices : ICoordinateSystemServices, ICoordinateSystemRepository
    {
        private readonly Dictionary<int, ICoordinateSystem> _csBySrid;
        private readonly Dictionary<IInfo, int> _sridByCs;

        private readonly ICoordinateSystemFactory _coordinateSystemFactory;
        private readonly ICoordinateTransformationFactory _ctFactory;

        #region CsEqualityComparer class
        private class CsEqualityComparer : EqualityComparer<IInfo>
        {
            public override bool Equals(IInfo x, IInfo y)
            {
                return x.AuthorityCode == y.AuthorityCode &&
#if PCL
                    string.Compare(x.Authority, y.Authority, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) == 0;
#else
 string.Compare(x.Authority, y.Authority, true, CultureInfo.InvariantCulture) == 0;
#endif
            }

            public override int GetHashCode(IInfo obj)
            {
                if (obj == null) return 0;
                return Convert.ToInt32(obj.AuthorityCode) + (obj.Authority != null ? obj.Authority.GetHashCode() : 0);
            }
        }
        #endregion

        #region CoordinateSystemKey class

        private class CoordinateSystemKey : IInfo
        {
            public CoordinateSystemKey(string authority, long authorityCode)
            {
                Authority = authority;
                AuthorityCode = authorityCode;
            }

            public bool EqualParams(object obj)
            {
                throw new NotSupportedException();
            }

            public string Name { get { return null; } }
            public string Authority { get; private set; }
            public long AuthorityCode { get; private set; }
            public string Alias { get { return null; } }
            public string Abbreviation { get { return null; } }
            public string Remarks { get { return null; } }
            public string WKT { get { return null; } }
            public string XML { get { return null; } }
        }

        #endregion

        #region ctors
        public CoordinateSystemServices(ICoordinateSystemFactory coordinateSystemFactory,
            ICoordinateTransformationFactory coordinateTransformationFactory)
        {
            if (coordinateSystemFactory == null)
                throw new ArgumentNullException("coordinateSystemFactory");

            if (coordinateTransformationFactory == null)
                throw new ArgumentNullException("coordinateTransformationFactory");

            _coordinateSystemFactory = coordinateSystemFactory;
            _ctFactory = coordinateTransformationFactory;

            _csBySrid = new Dictionary<int, ICoordinateSystem>();
            _sridByCs = new Dictionary<IInfo, int>(new CsEqualityComparer());
        }

        public CoordinateSystemServices(
            ICoordinateSystemFactory coordinateSystemFactory,
            ICoordinateTransformationFactory coordinateTransformationFactory,
            IEnumerable<KeyValuePair<int, string>> enumerable)
            : this(coordinateSystemFactory, coordinateTransformationFactory)
        {
            PrivAddCoordinateSystems(enumerable);
        } 
        #endregion

        #region private members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void PrivAddCoordinateSystems(IEnumerable<KeyValuePair<int, string>> coordinateSystems)
        {
            foreach (var keyPair in coordinateSystems)
            {
                var srid = keyPair.Key;
                var WKT = keyPair.Value;

                var coordinateSystem = CreateCoordinateSystem(WKT);

                if (_csBySrid.ContainsKey(srid))
                {
                    if (ReferenceEquals(coordinateSystem, _csBySrid[srid]))
                        return;

                    _sridByCs.Remove(_csBySrid[srid]);
                    _csBySrid[srid] = coordinateSystem;
                    _sridByCs.Add(coordinateSystem, srid);
                }
                else
                {
                    _csBySrid.Add(srid, coordinateSystem);
                    _sridByCs.Add(coordinateSystem, srid);
                }
            }
        }
        #endregion

        #region public members

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public ICoordinateSystem CreateCoordinateSystem(string wkt)
        {
            try
            {
                return _coordinateSystemFactory.CreateFromWkt(wkt.Replace("ELLIPSOID", "SPHEROID"));
            }
            catch (Exception)
            {
                // as a fallback we ignore projections not supported
                return null;
            }
        }

        public virtual bool RemoveCoordinateSystem(int srid)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            var cs = GetCoordinateSystem(srid);
            if (cs == null)
                return false;

            _csBySrid.Remove(srid);
            _sridByCs.Remove(cs);

            return true;
        }

        public virtual ICoordinateSystem GetCoordinateSystem(int srid)
        {
            ICoordinateSystem cs;
            return _csBySrid.TryGetValue(srid, out cs) ? cs : null;
        }

        public virtual ICoordinateSystem GetCoordinateSystem(string authority, long code)
        {
            var srid = GetSRID(authority, code);
            return srid.HasValue ? GetCoordinateSystem(srid.Value) : null;
        }

        public virtual int? GetSRID(string authority, long authorityCode)
        {
            var key = new CoordinateSystemKey(authority, authorityCode);
            int srid;
            if (_sridByCs.TryGetValue(key, out srid))
                return srid;

            return null;
        }

        public ICoordinateTransformation CreateTransformation(int sourceSrid, int targetSrid)
        {
            return CreateTransformation(
                GetCoordinateSystem(sourceSrid),
                GetCoordinateSystem(targetSrid));
        }

        public ICoordinateTransformation CreateTransformation(ICoordinateSystem src, ICoordinateSystem tgt)
        {
            return _ctFactory.CreateFromCoordinateSystems(src, tgt);
        }

        public virtual IEnumerator<KeyValuePair<int, ICoordinateSystem>> GetEnumerator()
        {
            return _csBySrid.GetEnumerator();
        }


        public virtual void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            _csBySrid.Clear();
            _sridByCs.Clear();
        }

        public virtual int Count
        {
            get { return _sridByCs.Count; }
        }

        public void AddCoordinateSystems(IEnumerable<KeyValuePair<int, string>> coordinateSystems)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            PrivAddCoordinateSystems(coordinateSystems);
        }

        public virtual void AddCoordinateSystem(int srid, ICoordinateSystem coordinateSystem)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            if (_csBySrid.ContainsKey(srid))
            {
                if (ReferenceEquals(coordinateSystem, _csBySrid[srid]))
                    return;

                _sridByCs.Remove(_csBySrid[srid]);
                _csBySrid[srid] = coordinateSystem;
                _sridByCs.Add(coordinateSystem, srid);
            }
            else
            {
                _csBySrid.Add(srid, coordinateSystem);
                _sridByCs.Add(coordinateSystem, srid);
            }
        }

        /// <summary>
        /// Creates a CoordinateSystemServices built with all the values coming from the SpatialRefSys.xml
        /// </summary>
        /// <param name="coordinateSystemFactory"></param>
        /// <param name="coordinateTransformationFactory"></param>
        /// <returns></returns>
        public static ICoordinateSystemServices FromSpatialRefSys(ICoordinateSystemFactory coordinateSystemFactory, ICoordinateTransformationFactory coordinateTransformationFactory)
        {
            if (coordinateSystemFactory == null)
                throw new ArgumentNullException("coordinateSystemFactory");

            if (coordinateTransformationFactory == null)
                throw new ArgumentNullException("coordinateTransformationFactory");

            var css = new CoordinateSystemServices(coordinateSystemFactory, coordinateTransformationFactory);
            css.AddCoordinateSystems(SpatialReference.GetAllReferenceSystems());

            return css;
        }

        #endregion
    }
}
