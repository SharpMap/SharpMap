// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Copyright 2010       - Felix Obermaier
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using Microsoft.SqlServer.Types;
using SharpMap.Converters.SqlServer2008SpatialObjects;
using SharpMap.Data.Providers;
using Geometry = GeoAPI.Geometries.IGeometry;

namespace SharpMap.Geometries
{
    /// <summary>
    /// Class defining a set of named spatial relationship operators for geometric shape objects.
    /// </summary>
    public static class SpatialRelationsEx
    {
        static SpatialRelationsEx()
        {
            SqlServer2008Ex.LoadSqlServerTypes();
        }
        
        /// <summary>
        /// Returns true if otherGeometry is wholly contained within the source geometry. This is the same as
        /// reversing the primary and comparison shapes of the Within operation.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <returns>True if otherGeometry is wholly contained within the source geometry.</returns>
        [Obsolete]
        public static bool Contains(Geometry g1, Geometry g2)
        {
            return Contains(g1, g2, Data.Providers.SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if otherGeometry is wholly contained within the source geometry. This is the same as
        /// reversing the primary and comparison shapes of the Within operation.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns>True if otherGeometry is wholly contained within the source geometry.</returns>
        public static bool Contains(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == Data.Providers.SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)(sg1.STContains(sg2));
            }
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                return (bool)(sg1.STContains(sg2));
            }
        }

        /// <summary>
        /// Returns true if the intersection of the two geometries results in a geometry whose dimension is less than
        /// the maximum dimension of the two geometries and the intersection geometry is not equal to either.
        /// geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <returns></returns>
        [Obsolete]
        public static bool Crosses(Geometry g1, Geometry g2)
        {
            return Crosses(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if the intersection of the two geometries results in a geometry whose dimension is less than
        /// the maximum dimension of the two geometries and the intersection geometry is not equal to either.
        /// geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static bool Crosses(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == Data.Providers.SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)sg1.STCrosses(sg2);
            }
            else
            {
               throw new ArgumentOutOfRangeException ("Geography does not support STCrosses. You must first convert Geography to Geometry using WKB");
            }
        }

        /// <summary>
        /// Returns true if otherGeometry is disjoint from the source geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <returns></returns>
        [Obsolete]
        public static bool Disjoint(Geometry g1, Geometry g2)
        {
            return Disjoint(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if otherGeometry is disjoint from the source geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static bool Disjoint(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
                return !g2.Intersects(g1);
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                return (bool)sg1.STDisjoint(sg2);
            }
        }

        /// <summary>
        /// Returns true if otherGeometry is of the same type and defines the same point set as the source geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other Geometry</param>
        /// <returns></returns>
        [Obsolete]
        public static bool Equals(Geometry g1, Geometry g2)
        {
            return Equals(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if otherGeometry is of the same type and defines the same point set as the source geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other Geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static bool Equals(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)sg1.STEquals(sg2);
            }
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                return (bool)sg1.STEquals(sg2);
            }
        }

        /// <summary>
        /// Returns true if there is any intersection between the two geometries.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <returns></returns>
        [Obsolete]
        public static bool Intersects(Geometry g1, Geometry g2)
        {
            return Intersects(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if there is any intersection between the two geometries.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static bool Intersects(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)sg1.STIntersects(sg2);
            }
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                return (bool)sg1.STIntersects(sg2);
            }
        }

        /// <summary>
        /// Returns true if the intersection of the two geometries results in an object of the same dimension as the
        /// input geometries and the intersection geometry is not equal to either geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <returns></returns>
        [Obsolete]
        public static bool Overlaps(Geometry g1, Geometry g2)
        {
            return Overlaps(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if the intersection of the two geometries results in an object of the same dimension as the
        /// input geometries and the intersection geometry is not equal to either geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static bool Overlaps(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)sg1.STOverlaps(sg2);
            }
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                return (bool)sg1.STOverlaps(sg2);
            }
        }

        /// <summary>
        /// Returns true if the only points in common between the two geometries lie in the union of their boundaries.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <returns></returns>
        [Obsolete]
        public static bool Touches(Geometry g1, Geometry g2)
        {
            return Touches(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if the only points in common between the two geometries lie in the union of their boundaries.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static bool Touches(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)sg1.STTouches(sg2);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Geography does not support STTouches. You must first convert Geography to Geometry using WKB");
            }
        }

        /// <summary>
        /// Returns true if the primary geometry is wholly contained within the comparison geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <returns></returns>
        [Obsolete]
        public static bool Within(Geometry g1, Geometry g2)
        {
            return Within(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if the primary geometry is wholly contained within the comparison geometry.
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static bool Within(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
        if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)sg1.STWithin(sg2);
            }
        else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                return (bool)sg1.STWithin(sg2);
            }
            
        }

        /// <summary>
        /// Returns true if the given geometries relate according to the provided intersection pattern Matrix
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="intersectionPatternMatrix">Intersection pattern Matrix</param>
        /// <returns></returns>
        [Obsolete]
        public static Boolean Relate(Geometry g1, Geometry g2, string intersectionPatternMatrix)
        {
            return Relate(g1, g2, intersectionPatternMatrix, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Returns true if the given geometries relate according to the provided intersection pattern Matrix
        /// </summary>
        /// <param name="g1">Source geometry</param>
        /// <param name="g2">Other geometry</param>
        /// <param name="intersectionPatternMatrix">Intersection pattern Matrix</param>
        /// <param name="spatialMode">calculation library - some methods have different behaviour depending on geometry or geography</param>
        /// <returns></returns>
        public static Boolean Relate(Geometry g1, Geometry g2, string intersectionPatternMatrix, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                return (bool)sg1.STRelate(sg2, intersectionPatternMatrix);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Geography does not support STRelate. You must first convert Geography to Geometry using WKB");
            }
        }
    }
}
