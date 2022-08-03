#region License

/*
 *  The attached / following is part of SharpMap.SqlServerSpatialObjects.
 *  
 *  SharpMap.SqlServerSpatialObjects is free software � 2010 Ingenieurgruppe IVV GmbH & Co. KG, 
 *  www.ivv-aachen.de; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/.
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: Felix Obermaier 2010
 *  
 */

#endregion

using Microsoft.SqlServer.Types;
using SharpMap.Converters.SqlServer2008SpatialObjects;
using SharpMap.Data.Providers;
using System;
using Geometry = NetTopologySuite.Geometries.Geometry;

namespace SharpMap.Geometries
{
    /// <summary>
    /// Class defining a set of named spatial operations for geometric shape objects.
    /// </summary>
    public static class SpatialOperationsEx
    {
        static SpatialOperationsEx()
        {
            SqlServer2008Ex.LoadSqlServerTypes();
        }

        /// <summary>
        /// Computes a buffer around <paramref name="g"/> with a given <paramref name="distance"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry</param>
        /// <param name="distance">A distance</param>
        /// <returns>The buffered geometry</returns>        [Obsolete]
        [Obsolete]
        public static Geometry Buffer(Geometry g, Double distance)
        {
            return Buffer(g, distance, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Computes a buffer around <paramref name="g"/> with a given <paramref name="distance"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry/geography</param>
        /// <param name="distance">A distance</param>
        /// <param name="spatialMode">Flag indicating if <paramref name="g"/> is a geometry or geography objects</param>
        /// <returns>The buffered geometry</returns>
        public static Geometry Buffer(Geometry g, Double distance, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
                SqlGeometry sgBuffer = sg.STBuffer(distance);
                return SqlGeometryConverter.ToSharpMapGeometry(sgBuffer);
            }
            else
            {
                SqlGeography sg = SqlGeographyConverter.ToSqlGeography(g);
                SqlGeography sgBuffer = sg.STBuffer(distance);
                return SqlGeographyConverter.ToSharpMapGeometry(sgBuffer);
            }
        }

        /// <summary>
        /// Computes the union of <paramref name="g"/> and <paramref name="geometries"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry</param>
        /// <param name="geometries">A (series of) geometry objects</param>
        /// <returns>The union</returns>
        [Obsolete]
        public static Geometry Union(Geometry g, params Geometry[] geometries)
        {
            return Union(g, SqlServerSpatialObjectType.Geometry, geometries);
        }

        /// <summary>
        /// Computes the union of <paramref name="g"/> and <paramref name="geometries"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry/geography</param>
        /// <param name="geometries">A (series of) geometry/geography objects</param>
        /// <param name="spatialMode">Flag indicating if <paramref name="g"/> and <paramref name="geometries"/> are geometry or geography objects</param>
        /// <returns>The union</returns>
        public static Geometry Union(Geometry g, SqlServerSpatialObjectType spatialMode, params Geometry[] geometries)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
                foreach (SqlGeometry sgUnion in SqlGeometryConverter.ToSqlGeometries(geometries))
                {
                    sg = sg.STUnion(sgUnion);
                }
                return SqlGeometryConverter.ToSharpMapGeometry(sg);
            }
            else
            {
                SqlGeography sg = SqlGeographyConverter.ToSqlGeography(g);
                foreach (SqlGeography sgUnion in SqlGeographyConverter.ToSqlGeographies(geometries))
                {
                    sg = sg.STUnion(sgUnion);
                }
                return SqlGeographyConverter.ToSharpMapGeometry(sg);
            }

        }

        /// <summary>
        /// Computes the difference of <paramref name="g1"/> and <paramref name="g2"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <returns>The difference</returns>
        [Obsolete]
        public static Geometry Difference(Geometry g1, Geometry g2)
        {
            return Difference(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Computes the difference of <paramref name="g1"/> and <paramref name="g2"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g1">A geometry/geography</param>
        /// <param name="g2">A geometry/geography</param>
        /// <param name="spatialMode">Flag indicating if <paramref name="g1"/> and <paramref name="g2"/> are geometry or geography objects</param>
        /// <returns>The difference</returns>
        public static Geometry Difference(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                SqlGeometry sgDifference = sg1.STDifference(sg2);
                return SqlGeometryConverter.ToSharpMapGeometry(sgDifference);
            }
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                SqlGeography sgDifference = sg1.STDifference(sg2);
                return SqlGeographyConverter.ToSharpMapGeometry(sgDifference);
            }
        }

        /// <summary>
        /// Computes the symmetric-difference of <paramref name="g1"/> and <paramref name="g2"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <returns>The symmetric difference</returns>
        [Obsolete]
        public static Geometry SymDifference(Geometry g1, Geometry g2)
        {
            return SymDifference(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Computes the symmetric-difference of <paramref name="g1"/> and <paramref name="g2"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g1">A geometry/geography</param>
        /// <param name="g2">A geometry/geography</param>
        /// <param name="spatialMode">Flag indicating if <paramref name="g1"/> and <paramref name="g2"/> are geometry or geography objects</param>
        /// <returns>The symmetric difference</returns>
        public static Geometry SymDifference(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                SqlGeometry sgSymDifference = sg1.STSymDifference(sg2);
                return SqlGeometryConverter.ToSharpMapGeometry(sgSymDifference);
            }
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                SqlGeography sgSymDifference = sg1.STSymDifference(sg2);
                return SqlGeographyConverter.ToSharpMapGeometry(sgSymDifference);
            }
        }

        /// <summary>
        /// Computes the intersection of <paramref name="g1"/> and <paramref name="g2"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <returns>The intersection</returns>
        [Obsolete]
        public static Geometry Intersection(Geometry g1, Geometry g2)
        {
            return Intersection(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Computes the intersection of <paramref name="g1"/> and <paramref name="g2"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g1">A geometry/geography</param>
        /// <param name="g2">A geometry/geography</param>
        /// <param name="spatialMode">Flag indicating if <paramref name="g1"/> and <paramref name="g2"/> are geometry or geography objects</param>
        /// <returns>The intersection</returns>
        public static Geometry Intersection(Geometry g1, Geometry g2, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
                SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
                SqlGeometry sgIntersection = sg1.STIntersection(sg2);
                return SqlGeometryConverter.ToSharpMapGeometry(sgIntersection);
            }
            else
            {
                SqlGeography sg1 = SqlGeographyConverter.ToSqlGeography(g1);
                SqlGeography sg2 = SqlGeographyConverter.ToSqlGeography(g2);
                SqlGeography sgIntersection = sg1.STIntersection(sg2);
                return SqlGeographyConverter.ToSharpMapGeometry(sgIntersection);
            }
        }

        /// <summary>
        /// Computes the convex-hull of <paramref name="g"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry</param>
        /// <returns>The convex-hull</returns>
        [Obsolete]
        public static Geometry ConvexHull(Geometry g)
        {
            return ConvexHull(g, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Computes the convex-hull of <paramref name="g"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry/geography</param>
        /// <param name="spatialMode">Flag indicating if <paramref name="g"/> is geometry or geography</param>
        /// <returns>The convex-hull</returns>
        public static Geometry ConvexHull(Geometry g, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
                SqlGeometry sgConvexHull = sg.STConvexHull();
                return SqlGeometryConverter.ToSharpMapGeometry(sgConvexHull);
            }
            else
            {
                SqlGeography sg = SqlGeographyConverter.ToSqlGeography(g);
                SqlGeography sgConvexHull = sg.STConvexHull();
                return SqlGeographyConverter.ToSharpMapGeometry(sgConvexHull);
            }
        }

        /// <summary>
        /// Computes the boundary of <paramref name="g"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry</param>
        /// <returns>The boundary</returns>
        [Obsolete]
        public static Geometry Boundary(Geometry g)
        {
            return Boundary(g, SqlServerSpatialObjectType.Geometry);
        }

        /// <summary>
        /// Computes the boundary of <paramref name="g"/> using SqlServer spatial object algorithms
        /// </summary>
        /// <param name="g">A geometry/geography</param>
        /// <param name="spatialMode">Flag indicating if <paramref name="g"/> is geometry or geography</param>
        /// <returns>The boundary</returns>
        public static Geometry Boundary(Geometry g, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
                SqlGeometry sgBoundary = sg.STBoundary();
                return SqlGeometryConverter.ToSharpMapGeometry(sgBoundary);
            }

            throw new ArgumentOutOfRangeException("Geography does not support STBoundary");
        }

    }
}
