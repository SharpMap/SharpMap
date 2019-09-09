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

using System;
using Microsoft.SqlServer.Types;
using SharpMap.Converters.SqlServer2008SpatialObjects;
using SharpMap.Data.Providers;
using Geometry = GeoAPI.Geometries.IGeometry;

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
        
        [Obsolete]
        public static Geometry Buffer(Geometry g, Double distance)
        {
            return Buffer(g, distance, SqlServerSpatialObjectType.Geometry);
        }

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

        [Obsolete]
        public static Geometry Union(Geometry g, params Geometry[] geometries)
        {
            return Union(g, SqlServerSpatialObjectType.Geometry, geometries);
        }

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

        [Obsolete]
        public static Geometry Difference(Geometry g1, Geometry g2)
        {
            return Difference(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

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

        [Obsolete]
        public static Geometry SymDifference(Geometry g1, Geometry g2)
        {
            return SymDifference(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

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

        [Obsolete]
        public static Geometry Intersection(Geometry g1, Geometry g2)
        {
            return Intersection(g1, g2, SqlServerSpatialObjectType.Geometry);
        }

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

        [Obsolete]
        public static Geometry ConvexHull(Geometry g)
        {
            return ConvexHull(g, SqlServerSpatialObjectType.Geometry);
        }

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

        [Obsolete]
        public static Geometry Boundary(Geometry g)
        {
            return Boundary(g, SqlServerSpatialObjectType.Geometry);
        }

        public static Geometry Boundary(Geometry g, SqlServerSpatialObjectType spatialMode)
        {
            if (spatialMode == SqlServerSpatialObjectType.Geometry)
            {
                SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
                SqlGeometry sgBoundary = sg.STBoundary();
                return SqlGeometryConverter.ToSharpMapGeometry(sgBoundary);
            }
            else
            {
                throw new ArgumentOutOfRangeException ("Geography does not support STBoundary");
            }
        }

    }
}
