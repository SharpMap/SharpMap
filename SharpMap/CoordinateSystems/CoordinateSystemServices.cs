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
        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="coordinateSystemFactory"/>, 
        /// <paramref name="coordinateTransformationFactory"/> and enumeration of 
        /// </summary>
        /// <param name="coordinateSystemFactory">The factory to use for creating a coordinate system.</param>
        /// <param name="coordinateTransformationFactory">The factory to use for creating a coordinate transformation.</param>
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

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="coordinateSystemFactory"/>, 
        /// <paramref name="coordinateTransformationFactory"/> and enumeration of 
        /// </summary>
        /// <param name="coordinateSystemFactory">The factory to use for creating a coordinate system.</param>
        /// <param name="coordinateTransformationFactory">The factory to use for creating a coordinate transformation.</param>
        /// <param name="enumerable">An enumeration if spatial reference ids and coordinate system definition strings pairs</param>
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
                var wellKnownText = keyPair.Value;

                var coordinateSystem = CreateCoordinateSystem(wellKnownText);
                if (coordinateSystem == null) continue;
                
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

        /// <summary>
        /// Gets a value indicating that this coordinate system repository is readonly
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Method to create a coordinate system based on the <paramref name="wellKnownText"/> coordinate system definition.
        /// </summary>
        /// <param name="wellKnownText"></param>
        /// <returns>A coordinate system, <value>null</value> if no coordinate system could be created.</returns>
        public ICoordinateSystem CreateCoordinateSystem(string wellKnownText)
        {
            try
            {
                return _coordinateSystemFactory.CreateFromWkt(wellKnownText.Replace("ELLIPSOID", "SPHEROID"));
            }
            catch (Exception)
            {
                // as a fallback we ignore projections not supported
                return null;
            }
        }

        /// <summary>
        /// Method to remove a coordinate system form the service by its <paramref name="srid"/> identifier
        /// </summary>
        /// <param name="srid">The identifier of the coordinate system to remove</param>
        /// <returns><value>true</value> if the coordinate system was removed successfully, otherwise <value>false</value></returns>
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

        /// <summary>
        /// Returns the coordinate system by <paramref name="srid"/> identifier
        /// </summary>
        /// <param name="srid">The initialization for the coordinate system</param>
        /// <returns>
        /// The coordinate system.
        /// </returns>
        public virtual ICoordinateSystem GetCoordinateSystem(int srid)
        {
            ICoordinateSystem cs;
            return _csBySrid.TryGetValue(srid, out cs) ? cs : null;
        }

        /// <summary>
        /// Returns the coordinate system by <paramref name="authority"/> and <paramref name="code"/>.
        /// </summary>
        /// <param name="authority">The authority for the coordinate system</param><param name="code">The code assigned to the coordinate system by <paramref name="authority"/>.</param>
        /// <returns>
        /// The coordinate system.
        /// </returns>
        public virtual ICoordinateSystem GetCoordinateSystem(string authority, long code)
        {
            var srid = GetSRID(authority, code);
            return srid.HasValue ? GetCoordinateSystem(srid.Value) : null;
        }

        /// <summary>
        /// Method to get the identifier, by which this coordinate system can be accessed.
        /// </summary>
        /// <param name="authority">The authority name</param><param name="authorityCode">The code assigned by <paramref name="authority"/></param>
        /// <returns>
        /// The identifier or 
        /// <value>
        /// null
        /// </value>
        /// </returns>
        public virtual int? GetSRID(string authority, long authorityCode)
        {
            var key = new CoordinateSystemKey(authority, authorityCode);
            int srid;
            if (_sridByCs.TryGetValue(key, out srid))
                return srid;

            return null;
        }

        /// <summary>
        /// Method to create a coordinate tranformation between two spatial reference systems, defined by their identifiers
        /// </summary>
        /// <remarks>
        /// This is a convenience function for <see cref="M:GeoAPI.ICoordinateSystemServices.CreateTransformation(GeoAPI.CoordinateSystems.ICoordinateSystem,GeoAPI.CoordinateSystems.ICoordinateSystem)"/>.
        /// </remarks>
        /// <param name="sourceSrid">The identifier for the source spatial reference system.</param><param name="targetSrid">The identifier for the target spatial reference system.</param>
        /// <returns>
        /// A coordinate transformation, 
        /// <value>
        /// null
        /// </value>
        ///  if no transformation could be created.
        /// </returns>
        public ICoordinateTransformation CreateTransformation(int sourceSrid, int targetSrid)
        {
            return CreateTransformation(
                GetCoordinateSystem(sourceSrid),
                GetCoordinateSystem(targetSrid));
        }

        /// <summary>
        /// Method to create a coordinate tranformation between two spatial reference systems
        /// </summary>
        /// <param name="source">The source spatial reference system.</param><param name="target">The target spatial reference system.</param>
        /// <returns>
        /// A coordinate transformation, 
        /// <value>
        /// null
        /// </value>
        ///  if no transformation could be created.
        /// </returns>
        public ICoordinateTransformation CreateTransformation(ICoordinateSystem source, ICoordinateSystem target)
        {
            return _ctFactory.CreateFromCoordinateSystems(source, target);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public virtual IEnumerator<KeyValuePair<int, ICoordinateSystem>> GetEnumerator()
        {
            return _csBySrid.GetEnumerator();
        }

        /// <summary>
        /// Method to remove all coordinate systems from the service
        /// </summary>
        public virtual void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            _csBySrid.Clear();
            _sridByCs.Clear();
        }

        /// <summary>
        /// Gets a value indicating the number of unique coordinate systems in the repository
        /// </summary>
        public virtual int Count
        {
            get { return _sridByCs.Count; }
        }

        /// <summary>
        /// Method to add an enumeration of spatial reference id and coordinate system definition pairs to the repository.
        /// </summary>
        /// <param name="coordinateSystems">An enumeration of spatial reference id and coordinate system definition pairs.</param>
        public void AddCoordinateSystems(IEnumerable<KeyValuePair<int, string>> coordinateSystems)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            PrivAddCoordinateSystems(coordinateSystems);
        }

        /// <summary>
        /// Method to add <paramref name="coordinateSystem"/> to the service and register it with the <paramref name="srid"/> value.
        /// </summary>
        /// <param name="srid">The identifier for the <paramref name="coordinateSystem"/> in the store.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
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
        /// <param name="coordinateSystemFactory">A coordinate system factory</param>
        /// <param name="coordinateTransformationFactory">A coordinate transformation factory</param>
        /// <returns>A coordinate system services instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown, if either <paramref name="coordinateSystemFactory"/> or <paramref name="coordinateTransformationFactory"/> is null.</exception>
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
