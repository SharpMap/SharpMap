using System;
using System.Collections.Generic;
using System.Globalization;
using DotSpatial.Projections;
using GeoAPI.CoordinateSystems;
using DatumType = GeoAPI.CoordinateSystems.DatumType;

namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// An implementation for a coordinate system factory that creates <see cref="DotSpatialCoordinateSystem"/>s.
    /// </summary>
    /// <remarks>The only supported method is <see cref="CreateFromWkt"/></remarks>
    public class DotSpatialCoordinateSystemFactory : ICoordinateSystemFactory
    {
        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.ICompoundCoordinateSystem" />.
        /// </summary>
        /// <param name="name">Name of compound coordinate system.</param>
        /// <param name="head">Head coordinate system</param>
        /// <param name="tail">Tail coordinate system</param>
        /// <returns>Compound coordinate system</returns>
        /// <exception cref="NotSupportedException"></exception>
        public ICompoundCoordinateSystem CreateCompoundCoordinateSystem(string name, ICoordinateSystem head, ICoordinateSystem tail)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates an <see cref="T:GeoAPI.CoordinateSystems.IEllipsoid" /> from radius values.
        /// </summary>
        /// <seealso cref="M:GeoAPI.CoordinateSystems.ICoordinateSystemFactory.CreateFlattenedSphere(System.String,System.Double,System.Double,GeoAPI.CoordinateSystems.ILinearUnit)" />
        /// <param name="name">Name of ellipsoid</param>
        /// <param name="semiMajorAxis"></param>
        /// <param name="semiMinorAxis"></param>
        /// <param name="linearUnit"></param>
        /// <returns>Ellipsoid</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IEllipsoid CreateEllipsoid(string name, double semiMajorAxis, double semiMinorAxis, ILinearUnit linearUnit)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.IFittedCoordinateSystem" />.
        /// </summary>
        /// <remarks>The units of the axes in the fitted coordinate system will be
        /// inferred from the units of the base coordinate system. If the affine map
        /// performs a rotation, then any mixed axes must have identical units. For
        /// example, a (lat_deg,lon_deg,height_feet) system can be rotated in the
        /// (lat,lon) plane, since both affected axes are in degrees. But you
        /// should not rotate this coordinate system in any other plane.</remarks>
        /// <param name="name">Name of coordinate system</param>
        /// <param name="baseCoordinateSystem">Base coordinate system</param>
        /// <param name="toBaseWkt"></param>
        /// <param name="arAxes"></param>
        /// <returns>Fitted coordinate system</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IFittedCoordinateSystem CreateFittedCoordinateSystem(string name, ICoordinateSystem baseCoordinateSystem,
            string toBaseWkt, List<AxisInfo> arAxes)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates an <see cref="T:GeoAPI.CoordinateSystems.IEllipsoid" /> from an major radius, and inverse flattening.
        /// </summary>
        /// <seealso cref="M:GeoAPI.CoordinateSystems.ICoordinateSystemFactory.CreateEllipsoid(System.String,System.Double,System.Double,GeoAPI.CoordinateSystems.ILinearUnit)" />
        /// <param name="name">Name of ellipsoid</param>
        /// <param name="semiMajorAxis">Semi major-axis</param>
        /// <param name="inverseFlattening">Inverse flattening</param>
        /// <param name="linearUnit">Linear unit</param>
        /// <returns>Ellipsoid</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IEllipsoid CreateFlattenedSphere(string name, double semiMajorAxis, double inverseFlattening, ILinearUnit linearUnit)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a coordinate system object from an XML string.
        /// </summary>
        /// <param name="xml">XML representation for the spatial reference</param>
        /// <returns>The resulting spatial reference object</returns>
        /// <exception cref="NotSupportedException"></exception>
        public ICoordinateSystem CreateFromXml(string xml)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a spatial reference object given its Well-known text representation.
        /// The output object may be either a <see cref="T:GeoAPI.CoordinateSystems.IGeographicCoordinateSystem" /> or
        /// a <see cref="T:GeoAPI.CoordinateSystems.IProjectedCoordinateSystem" />.
        /// </summary>
        /// <param name="wkt">The Well-known text representation for the spatial reference</param>
        /// <returns>The resulting spatial reference object</returns>
        public ICoordinateSystem CreateFromWkt(string wkt)
        {
            //Hack: DotSpatial.Projections does not handle Authority and AuthorityCode
            var pos1 = 10 + wkt.LastIndexOf("AUTHORITY[", StringComparison.InvariantCulture);
            var pos2 = wkt.IndexOf("]", pos1, StringComparison.InvariantCulture) - 1;
            var parts = wkt.Substring(pos1, pos2 - pos1 + 1).Split(',');

            var auth = parts[0].Replace("\"", "").Trim();
            var code =  int.Parse(parts[1].Replace("\"", ""), NumberStyles.Integer,
                NumberFormatInfo.InvariantInfo);

            ProjectionInfo pi = null;
            try
            {
                pi = ProjectionInfo.FromEsriString(wkt);
            }
            catch (Exception)
            {
                try
                {
                    pi = ProjectionInfo.FromAuthorityCode(auth, code);
                }
                catch (Exception)
                {
                }
            }

            if (pi == null) return null;

            pi.Authority = auth;
            pi.AuthorityCode = code;

            return new DotSpatialCoordinateSystem(pi);
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.IGeographicCoordinateSystem" />, which could be Lat/Lon or Lon/Lat.
        /// </summary>
        /// <param name="name">Name of geographical coordinate system</param>
        /// <param name="angularUnit">Angular units</param>
        /// <param name="datum">Horizontal datum</param>
        /// <param name="primeMeridian">Prime meridian</param>
        /// <param name="axis0">First axis</param>
        /// <param name="axis1">Second axis</param>
        /// <returns>Geographic coordinate system</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IGeographicCoordinateSystem CreateGeographicCoordinateSystem(string name, IAngularUnit angularUnit,
            IHorizontalDatum datum, IPrimeMeridian primeMeridian, AxisInfo axis0, AxisInfo axis1)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates <see cref="T:GeoAPI.CoordinateSystems.IHorizontalDatum" /> from ellipsoid and Bursa-World parameters.
        /// </summary>
        /// <remarks>
        /// Since this method contains a set of Bursa-Wolf parameters, the created
        /// datum will always have a relationship to WGS84. If you wish to create a
        /// horizontal datum that has no relationship with WGS84, then you can
        /// either specify a <see cref="T:GeoAPI.CoordinateSystems.DatumType">horizontalDatumType</see> of <see cref="F:GeoAPI.CoordinateSystems.DatumType.HD_Other" />, or create it via WKT.
        /// </remarks>
        /// <param name="name">Name of ellipsoid</param>
        /// <param name="datumType">Type of datum</param>
        /// <param name="ellipsoid">Ellipsoid</param>
        /// <param name="toWgs84">Wgs84 conversion parameters</param>
        /// <returns>Horizontal datum</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IHorizontalDatum CreateHorizontalDatum(string name, DatumType datumType, IEllipsoid ellipsoid,
            Wgs84ConversionInfo toWgs84)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.ILocalCoordinateSystem">local coordinate system</see>.
        /// </summary>
        /// <remarks>
        ///  The dimension of the local coordinate system is determined by the size of
        /// the axis array. All the axes will have the same units. If you want to make
        /// a coordinate system with mixed units, then you can make a compound
        /// coordinate system from different local coordinate systems.
        /// </remarks>
        /// <param name="name">Name of local coordinate system</param>
        /// <param name="datum">Local datum</param>
        /// <param name="unit">Units</param>
        /// <param name="axes">Axis info</param>
        /// <returns>Local coordinate system</returns>
        /// <exception cref="NotSupportedException"></exception>
        public ILocalCoordinateSystem CreateLocalCoordinateSystem(string name, ILocalDatum datum, IUnit unit, List<AxisInfo> axes)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.ILocalDatum" />.
        /// </summary>
        /// <param name="name">Name of datum</param>
        /// <param name="datumType">Datum type</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public ILocalDatum CreateLocalDatum(string name, DatumType datumType)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.IPrimeMeridian" />, relative to Greenwich.
        /// </summary>
        /// <param name="name">Name of prime meridian</param>
        /// <param name="angularUnit">Angular unit</param>
        /// <param name="longitude">Longitude</param>
        /// <returns>Prime meridian</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IPrimeMeridian CreatePrimeMeridian(string name, IAngularUnit angularUnit, double longitude)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.IProjectedCoordinateSystem" /> using a projection object.
        /// </summary>
        /// <param name="name">Name of projected coordinate system</param>
        /// <param name="gcs">Geographic coordinate system</param>
        /// <param name="projection">Projection</param>
        /// <param name="linearUnit">Linear unit</param>
        /// <param name="axis0">Primary axis</param>
        /// <param name="axis1">Secondary axis</param>
        /// <returns>Projected coordinate system</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IProjectedCoordinateSystem CreateProjectedCoordinateSystem(string name, IGeographicCoordinateSystem gcs,
            IProjection projection, ILinearUnit linearUnit, AxisInfo axis0, AxisInfo axis1)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.IProjection" />.
        /// </summary>
        /// <param name="name">Name of projection</param>
        /// <param name="wktProjectionClass">Projection class</param>
        /// <param name="parameters">Projection parameters</param>
        /// <returns>Projection</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IProjection CreateProjection(string name, string wktProjectionClass, List<ProjectionParameter> parameters)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.IVerticalCoordinateSystem" /> from a <see cref="T:GeoAPI.CoordinateSystems.IVerticalDatum">datum</see> and <see cref="T:GeoAPI.CoordinateSystems.ILinearUnit">linear units</see>.
        /// </summary>
        /// <param name="name">Name of vertical coordinate system</param>
        /// <param name="datum">Vertical datum</param>
        /// <param name="verticalUnit">Unit</param>
        /// <param name="axis">Axis info</param>
        /// <returns>Vertical coordinate system</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IVerticalCoordinateSystem CreateVerticalCoordinateSystem(string name, IVerticalDatum datum, ILinearUnit verticalUnit,
            AxisInfo axis)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a <see cref="T:GeoAPI.CoordinateSystems.IVerticalDatum" /> from an enumerated type value.
        /// </summary>
        /// <param name="name">Name of datum</param>
        /// <param name="datumType">Type of datum</param>
        /// <returns>Vertical datum</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IVerticalDatum CreateVerticalDatum(string name, DatumType datumType)
        {
            throw new NotSupportedException();
        }
    }
}