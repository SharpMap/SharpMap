
using System;
using DotSpatial.Projections;
using GeoAPI.CoordinateSystems;

namespace SharpMap.CoordinateSystems
{
    public class DotSpatialCoordinateSystem : ICoordinateSystem
    {
        private readonly ProjectionInfo _projectionInfo;
        private string _wkt;
        private readonly string _alias;
        private readonly string _abbreviation;
        private readonly string _remarks;

        /// <summary>
        /// Creates an instance of this class using the provided projection info
        /// </summary>
        /// <param name="projecionInfo">The projection info</param>
        public DotSpatialCoordinateSystem(ProjectionInfo projecionInfo)
        {
            if (projecionInfo == null)
                throw new ArgumentNullException("projecionInfo");

            _projectionInfo = projecionInfo;
        }

        /// <summary>
        /// Creates an instance of this class using the provided projection info, enhanced by an alias, an abbreviation and a remark
        /// </summary>
        /// <param name="projecionInfo">The projection info</param>
        /// <param name="alias">The alias</param>
        /// <param name="abbreviation">The abbreviation</param>
        /// <param name="remarks">The remarks</param>
        public DotSpatialCoordinateSystem(ProjectionInfo projecionInfo, string alias = null, string abbreviation = null, string remarks = null)
            :this(projecionInfo)
        {
            _alias = alias;
            _abbreviation = abbreviation;
            _remarks = remarks;
        }

        /// <summary>
        /// Gets a value indicating the projection info
        /// </summary>
        public ProjectionInfo ProjectionInfo
        {
            get
            {
                // Copy an identical clone to make sure nothing changes this instance
                var res = new ProjectionInfo();
                res.CopyProperties(_projectionInfo);
                return res;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool EqualParams(object obj)
        {
            if (obj is DotSpatialCoordinateSystem)
                return _projectionInfo.Matches(((DotSpatialCoordinateSystem) obj)._projectionInfo);
            if (obj is ProjectionInfo)
                return _projectionInfo.Matches((ProjectionInfo)obj);
            return false;
        }

        /// <summary>Gets the name of the object.</summary>
        public string Name
        {
            get { return _projectionInfo.Name; }
        }

        /// <summary>
        /// Gets the authority name for this object, e.g., “POSC”,
        /// is this is a standard object with an authority specific
        /// identity code. Returns “CUSTOM” if this is a custom object.
        /// </summary>
        public string Authority
        {
            get { return _projectionInfo.Authority; }
        }

        /// <summary>
        /// Gets the authority specific identification code of the object
        /// </summary>
        public long AuthorityCode
        {
            get { return _projectionInfo.AuthorityCode; }
        }

        /// <summary>Gets the alias of the object.</summary>
        public string Alias
        {
            get { return _alias; }
        }

        /// <summary>Gets the abbreviation of the object.</summary>
        public string Abbreviation
        {
            get { return _abbreviation; }
        }

        /// <summary>
        /// Gets the provider-supplied remarks for the object.
        /// </summary>
        public string Remarks
        {
            get { return _remarks; }
        }

        /// <summary>
        /// Returns the Well-known text for this spatial reference object
        /// as defined in the simple features specification.
        /// </summary>
        public string WKT
        {
            get
            {
                return _wkt ?? (_wkt = _projectionInfo.ToEsriString());
            }
        }

        /// <summary>Gets an XML representation of this object.</summary>
        public string XML
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets axis details for dimension within coordinate system.
        /// </summary>
        /// <param name="dimension">Dimension</param>
        /// <returns>Axis info</returns>
        public AxisInfo GetAxis(int dimension)
        {
            throw new NotSupportedException();
        }

        /// <summary>Gets units for dimension within coordinate system.</summary>
        public IUnit GetUnits(int dimension)
        {
            throw new NotSupportedException();
        }

        /// <summary>Dimension of the coordinate system.</summary>
        public int Dimension
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>Gets default envelope of coordinate system.</summary>
        /// <remarks>
        /// Gets default envelope of coordinate system. Coordinate systems
        /// which are bounded should return the minimum bounding box of their
        /// domain. Unbounded coordinate systems should return a box which is
        /// as large as is likely to be used. For example, a (lon,lat)
        /// geographic coordinate system in degrees should return a box from
        /// (-180,-90) to (180,90), and a geocentric coordinate system could
        /// return a box from (-r,-r,-r) to (+r,+r,+r) where r is the
        /// approximate radius of the Earth.
        /// </remarks>
        public double[] DefaultEnvelope
        {
            get { throw new NotSupportedException(); }
        }
    }

}
