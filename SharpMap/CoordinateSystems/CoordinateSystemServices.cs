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
    public class CoordinateSystemServices : ICoordinateSystemServices
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
        private CoordinateSystemServices(ICoordinateSystemFactory coordinateSystemFactory,
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

        public CoordinateSystemServices(ICoordinateSystemFactory coordinateSystemFactory,
            ICoordinateTransformationFactory coordinateTransformationFactory,
            IEnumerable<KeyValuePair<int, string>> enumeration)
            : this(coordinateSystemFactory, coordinateTransformationFactory)
        {
            FromEnumeration(this, enumeration);
        } 
        #endregion

        #region private members
        private static ICoordinateSystem CreateCoordinateSystem(ICoordinateSystemFactory coordinateSystemFactory, string wkt)
        {
            try
            {
                return coordinateSystemFactory.CreateFromWkt(wkt.Replace("ELLIPSOID", "SPHEROID"));
            }
            catch (Exception)
            {
                // as a fallback we ignore projections not supported
                return null;
            }
        }

        private static void FromEnumeration(CoordinateSystemServices css,
            IEnumerable<KeyValuePair<int, ICoordinateSystem>> enumeration)
        {
            foreach (var sridCs in enumeration)
            {
                css.AddCoordinateSystem(sridCs.Key, sridCs.Value);
            }
        }

        private static IEnumerable<KeyValuePair<int, ICoordinateSystem>> CreateCoordinateSystems(
            ICoordinateSystemFactory factory,
            IEnumerable<KeyValuePair<int, string>> enumeration)
        {
            foreach (var sridWkt in enumeration)
            {
                var cs = CreateCoordinateSystem(factory, sridWkt.Value);
                if (cs != null)
                    yield return new KeyValuePair<int, ICoordinateSystem>(sridWkt.Key, cs);
            }
        }

        private static void FromEnumeration(CoordinateSystemServices css,
            IEnumerable<KeyValuePair<int, string>> enumeration)
        {
            FromEnumeration(css, CreateCoordinateSystems(css._coordinateSystemFactory, enumeration));
        }

        #endregion

        #region public members

        public ICoordinateSystem GetCoordinateSystem(int srid)
        {
            ICoordinateSystem cs;
            return _csBySrid.TryGetValue(srid, out cs) ? cs : null;
        }

        public ICoordinateSystem GetCoordinateSystem(string authority, long code)
        {
            var srid = GetSRID(authority, code);
            return srid.HasValue ? GetCoordinateSystem(srid.Value) : null;
        }

        public int? GetSRID(string authority, long authorityCode)
        {
            var key = new CoordinateSystemKey(authority, authorityCode);
            int srid;
            if (_sridByCs.TryGetValue(key, out srid))
                return srid;

            return null;
        }

        public ICoordinateTransformation CreateTransformation(int sourceSrid, int targetSrid)
        {
            return CreateTransformation(GetCoordinateSystem(sourceSrid),
                GetCoordinateSystem(targetSrid));
        }

        public ICoordinateTransformation CreateTransformation(ICoordinateSystem src, ICoordinateSystem tgt)
        {
            return _ctFactory.CreateFromCoordinateSystems(src, tgt);
        }

        public IEnumerator<KeyValuePair<int, ICoordinateSystem>> GetEnumerator()
        {
            return _csBySrid.GetEnumerator();
        }

        #region obsolete members
        [Obsolete]
        public string GetCoordinateSystemInitializationString(int srid)
        {
            ICoordinateSystem cs;
            if (_csBySrid.TryGetValue(srid, out cs))
                return cs.WKT;
            throw new ArgumentOutOfRangeException("srid");
        }

        [Obsolete]
        public ICoordinateSystemFactory CoordinateSystemFactory
        {
            get { return _coordinateSystemFactory; }
        }

        [Obsolete]
        public ICoordinateTransformationFactory CoordinateTransformationFactory
        {
            get { return _ctFactory; }
        }

        #endregion

        #endregion

        #region protected members

        protected void AddCoordinateSystem(int srid, ICoordinateSystem coordinateSystem)
        {
            lock (((IDictionary)_csBySrid).SyncRoot)
            {
                lock (((IDictionary)_sridByCs).SyncRoot)
                {
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
        }

        protected virtual int AddCoordinateSystem(ICoordinateSystem coordinateSystem)
        {
            var srid = (int)coordinateSystem.AuthorityCode;
            AddCoordinateSystem(srid, coordinateSystem);

            return srid;
        }

        protected virtual void Clear()
        {
            _csBySrid.Clear();
            _sridByCs.Clear();
        }

        protected int Count
        {
            get
            {
                return _sridByCs.Count;
            }
        } 

        #endregion

        public virtual bool RemoveCoordinateSystem(int srid)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a CoordinateSystemServices built with all the values coming from the SpatialRefSys.xml
        /// </summary>
        /// <param name="coordinateSystemFactory"></param>
        /// <param name="coordinateTransformationFactory"></param>
        /// <returns></returns>
        public static CoordinateSystemServices FromSpatialRefSys(ICoordinateSystemFactory coordinateSystemFactory, ICoordinateTransformationFactory coordinateTransformationFactory)
        {
            if (coordinateSystemFactory == null)
                throw new ArgumentNullException("coordinateSystemFactory");

            if (coordinateTransformationFactory == null)
                throw new ArgumentNullException("coordinateTransformationFactory");

            return new CoordinateSystemServices(coordinateSystemFactory, coordinateTransformationFactory, SpatialReference.GetAllReferenceSystems());
        }
    }
}
