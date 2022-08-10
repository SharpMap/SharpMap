﻿/*
 * Copyright © 2017 - Felix Obermaier, Ingenieurgruppe IVV GmbH & Co. KG
 * 
 * This file is part of SharpMap.CoordinateSystems.DotSpatial.
 *
 * SharpMap.CoordinateSystems.DotSpatial is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharpMap.CoordinateSystems.DotSpatial is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with SharpMap; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
 *
 */
using DotSpatial.Projections;
using ProjNet.CoordinateSystems;
using System;

namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// A wrapper around a <see cref="DotSpatial.Projections.ProjectionInfo"/>
    /// </summary>
    public class DotSpatialCoordinateSystem
    {
        private static readonly string[] _latLonNames = { "Longitude", "Latitude" };
        private static readonly string[] _eastNorthNames = { "East", "North" };

        private static readonly AxisOrientationEnum[] _axisOrientation =
            {AxisOrientationEnum.East, AxisOrientationEnum.North,};

        private readonly ProjectionInfo _projectionInfo;
        private string _wkt;
        private readonly string _alias;
        private readonly string _abbreviation;
        private readonly string _remarks;

        /// <summary>
        /// Creates an instance of this class using the provided projection info
        /// </summary>
        /// <param name="projectionInfo">The projection info</param>
        public DotSpatialCoordinateSystem(ProjectionInfo projectionInfo)
        {
            _projectionInfo = projectionInfo ?? throw new ArgumentNullException("projectionInfo");
        }

        /// <summary>
        /// Creates an instance of this class using the provided projection info, enhanced by an alias, an abbreviation and a remark
        /// </summary>
        /// <param name="projecionInfo">The projection info</param>
        /// <param name="alias">The alias</param>
        /// <param name="abbreviation">The abbreviation</param>
        /// <param name="remarks">The remarks</param>
        public DotSpatialCoordinateSystem(ProjectionInfo projecionInfo, string alias = null, string abbreviation = null, string remarks = null)
            : this(projecionInfo)
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
            if (obj is DotSpatialCoordinateSystem dcs)
                return _projectionInfo.Matches(dcs._projectionInfo);
            if (obj is ProjectionInfo pi)
                return _projectionInfo.Matches(pi);
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
            return ToAxisInfo(_projectionInfo, dimension);
        }

        private AxisInfo ToAxisInfo(ProjectionInfo projectionInfo, int dimension)
        {
            if (projectionInfo.IsLatLon)
                return new AxisInfo(_latLonNames[dimension], _axisOrientation[dimension]);
            return new AxisInfo(_eastNorthNames[dimension], _axisOrientation[dimension]);
        }

        /// <summary>Gets units for dimension within coordinate system.</summary>
        public IUnit GetUnits(int dimension)
        {
            return ToUnit(_projectionInfo);
        }

        private static IUnit ToUnit(ProjectionInfo projDescriptor)
        {
            if (projDescriptor.IsLatLon)
                return new AngularUnitWrapper(projDescriptor.GeographicInfo.Unit);

            return new LinearUnitWrapper(projDescriptor.Unit);
        }

        private abstract class UnitWrapper : IUnit
        {
            protected readonly ProjDescriptor Descriptor;

            protected UnitWrapper(ProjDescriptor descriptor)
            {
                Descriptor = descriptor;
            }

            public abstract string Name { get; }

            public string Authority { get { return "DS"; } }
            public long AuthorityCode { get { return 20000000; } }
            public string Alias { get { return string.Empty; } }
            public string Abbreviation { get { return string.Empty; } }
            public string Remarks { get { return string.Empty; } }
            public abstract string WKT { get; }

            public string XML { get { throw new NotSupportedException(); } }

            public abstract bool EqualParams(object obj);
        }

        private class AngularUnitWrapper : UnitWrapper
        {
            private DotSpatial.Projections.AngularUnit Unit
            {
                get { return (DotSpatial.Projections.AngularUnit)Descriptor; }
            }

            public AngularUnitWrapper(DotSpatial.Projections.AngularUnit angularUnit)
                : base(angularUnit)
            {
            }

            public override string Name { get { return Unit.Name; } }

            public override string WKT { get { return Unit.ToEsriString(); } }

            public override bool EqualParams(object obj)
            {
                if (obj == null) return false;
                if (obj is AngularUnitWrapper auw)
                    return auw.RadiansPerUnit == RadiansPerUnit;
                return false;
            }


            public double RadiansPerUnit
            {
                get { return Unit.Radians; }
                set { Unit.Radians = value; }
            }
        }

        private class LinearUnitWrapper : UnitWrapper
        {
            private DotSpatial.Projections.LinearUnit Unit
            {
                get { return (DotSpatial.Projections.LinearUnit)Descriptor; }
            }

            public LinearUnitWrapper(DotSpatial.Projections.LinearUnit linearUnit)
                : base(linearUnit)
            {
            }

            public override string Name { get { return Unit.Name; } }

            public override string WKT { get { return Unit.ToEsriString(); } }

            public override bool EqualParams(object obj)
            {
                if (obj == null) return false;
                if (obj is LinearUnitWrapper luw)
                    return luw.MetersPerUnit == MetersPerUnit;
                return false;
            }


            public double MetersPerUnit
            {
                get { return Unit.Meters; }
                set { Unit.Meters = value; }
            }
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
