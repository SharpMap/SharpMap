#region License

/*
 *  The attached / following is part of SharpMap.SqlServerSpatialObjects.
 *  
 *  SharpMap.SqlServerSpatialObjects is free software © 2010 Ingenieurgruppe IVV GmbH & Co. KG, 
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
using Geometry = GeoAPI.Geometries.IGeometry;

namespace SharpMap.Geometries
{
    /// <summary>
    /// Class defining a set of named spatial operations for geometric shape objects.
    /// </summary>
    public static class SpatialOperationsEx
    {
        public static Geometry Buffer(Geometry g, Double distance)
        {
            SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
            SqlGeometry sgBuffer = sg.STBuffer(distance);
            return SqlGeometryConverter.ToSharpMapGeometry(sgBuffer);
        }

        public static Geometry Union(Geometry g, params Geometry[] geometries)
        {
            SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
            foreach (SqlGeometry sgUnion in SqlGeometryConverter.ToSqlGeometries(geometries))
            {
                sg = sg.STUnion(sgUnion);
            }
            return SqlGeometryConverter.ToSharpMapGeometry(sg);
        }

        public static Geometry Difference(Geometry g1, Geometry g2)
        {
            SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
            SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
            SqlGeometry sgDifference = sg1.STDifference(sg2);
            return SqlGeometryConverter.ToSharpMapGeometry(sgDifference);
        }

        public static Geometry SymDifference(Geometry g1, Geometry g2)
        {
            SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
            SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
            SqlGeometry sgSymDifference = sg1.STSymDifference(sg2);
            return SqlGeometryConverter.ToSharpMapGeometry(sgSymDifference);
        }

        public static Geometry Intersection(Geometry g1, Geometry g2)
        {
            SqlGeometry sg1 = SqlGeometryConverter.ToSqlGeometry(g1);
            SqlGeometry sg2 = SqlGeometryConverter.ToSqlGeometry(g2);
            SqlGeometry sgIntersection = sg1.STIntersection(sg2);
            return SqlGeometryConverter.ToSharpMapGeometry(sgIntersection);
        }

        public static Geometry ConvexHull(Geometry g)
        {
            SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
            SqlGeometry sgConvexHull = sg.STConvexHull();
            return SqlGeometryConverter.ToSharpMapGeometry(sgConvexHull);
        }

        public static Geometry Boundary(Geometry g)
        {
            SqlGeometry sg = SqlGeometryConverter.ToSqlGeometry(g);
            SqlGeometry sgBoundary = sg.STBoundary();
            return SqlGeometryConverter.ToSharpMapGeometry(sgBoundary);
        }

    }
}