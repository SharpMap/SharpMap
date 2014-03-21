// Copyright 2008 - William Dollins, 2012 - Joris Wit   
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

using SharpMap.Converters.SqlServer2008SpatialObjects;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using BoundingBox = GeoAPI.Geometries.Envelope;
using Geometry = GeoAPI.Geometries.IGeometry;

namespace SharpMap.Data.Providers
{
    /// <summary>   
    /// SQL Server 2008 data provider   
    /// </summary>   
    /// <remarks>   
    /// <para>This is a modified version of the <see cref="SqlServer2008"/> provider. It might provide better performance 
    /// because it directly uses the SQL server spatial types, instead of using the STAsBinary method.</para>   
    /// <para>It currently does not support the geography sql server data type.</para>   
    /// <example>   
    /// Adding a datasource to a layer:   
    /// <code lang="C#">   
    /// Layers.VectorLayer myLayer = new Layers.VectorLayer("My layer");   
    /// string ConnStr = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=myDB;Data Source=myServer\myInstance";   
    /// myLayer.DataSource = new Data.Providers.SqlServer2008Ex(ConnStr, "myTable", "GeomColumn", "OidColumn");   
    /// </code>   
    /// </example>   
    /// </remarks>   
    [Serializable]
    public class SqlServer2008Ex : SqlServer2008
    {
        /// <summary>   
        /// Initializes a new connection to SQL Server   
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="geometryColumnName">Name of geometry column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        public SqlServer2008Ex(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
            : base(connectionStr, tablename, geometryColumnName, oidColumnName)
        {
        }

        private const string SpatialObject = "geometry";

        /// <summary>   
        /// Returns geometries within the specified bounding box   
        /// </summary>   
        /// <param name="bbox"></param>   
        /// <returns></returns>   
        public override Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            var features = new Collection<Geometry>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                //Get bounding box string   
                string strBbox = GetBoxFilterStr(bbox);

                string strSql = "SELECT g." + GeometryColumn;
                strSql += " FROM " + Table + " g " + BuildTableHints() + " WHERE ";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != null && dr[0] != DBNull.Value)
                            {
                                Geometry geom = SqlGeometryConverter.ToSharpMapGeometry((Microsoft.SqlServer.Types.SqlGeometry) dr[0]);
                                if (geom != null)
                                    features.Add(geom);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return features;
        }

        /// <summary>   
        /// Returns the geometry corresponding to the Object ID   
        /// </summary>   
        /// <param name="oid">Object ID</param>   
        /// <returns>geometry</returns>   
        public override Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (var conn = new SqlConnection(ConnectionString))
            {
                string strSql = "SELECT g." + GeometryColumn + " FROM " + Table + " g WHERE " + ObjectIdColumn + "='" + oid + "'";
                conn.Open();
                using (var command = new SqlCommand(strSql, conn))
                {
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != null && dr[0] != DBNull.Value)
                                geom = SqlGeometryConverter.ToSharpMapGeometry((Microsoft.SqlServer.Types.SqlGeometry)dr[0]);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        /// <summary>   
        /// Returns the features that intersects with 'geom'   
        /// </summary>   
        /// <param name="geom"></param>   
        /// <param name="ds">FeatureDataSet to fill data into</param>   
        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                string strGeom = SpatialObject + "::STGeomFromText('" + geom.AsText() + "', #SRID#)";

                strGeom = strGeom.Replace("#SRID#", SRID > 0 ? SRID.ToString(CultureInfo.InvariantCulture) : "0");
                strGeom = GeometryColumn + ".STIntersects(" + strGeom + ") = 1";

                string strSql = "SELECT g.* FROM " + Table + " g " + BuildTableHints() + " WHERE ";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strGeom;

                using (var adapter = new SqlDataAdapter(strSql, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];

                            var ogeom = dr[GeometryColumn];
                            Geometry sqlGeometry = null;
                            if (ogeom != null && ogeom != DBNull.Value)
                                sqlGeometry = SqlGeometryConverter.ToSharpMapGeometry((Microsoft.SqlServer.Types.SqlGeometry)ogeom);
                            fdr.Geometry = sqlGeometry;
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        /// <summary>   
        /// Returns a datarow based on a RowID   
        /// </summary>   
        /// <param name="rowId"></param>   
        /// <returns>datarow</returns>   
        public override FeatureDataRow GetFeature(uint rowId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                string strSql = "select g.* from " + Table + " g WHERE " + ObjectIdColumn + "=" + rowId + "";
                using (var adapter = new SqlDataAdapter(strSql, conn))
                {
                    var ds = new System.Data.DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];

                            var geom = dr[GeometryColumn];
                            Geometry sqlGeometry = null;
                            if (geom != null && geom != DBNull.Value)
                                sqlGeometry = SqlGeometryConverter.ToSharpMapGeometry((Microsoft.SqlServer.Types.SqlGeometry)geom);
                            fdr.Geometry = sqlGeometry;
                            return fdr;
                        }
                        return null;
                    }
                    return null;
                }
            }
        }

        /// <summary>   
        /// Returns all features with the view box   
        /// </summary>   
        /// <param name="bbox">view box</param>   
        /// <param name="ds">FeatureDataSet to fill data into</param>   
        public override void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                string strSql = String.Format(
                    "SELECT g.* FROM {0} g {1} WHERE ",
                    Table, BuildTableHints());

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var adapter = new SqlDataAdapter(strSql, conn))
                {
                    conn.Open();
                    var ds2 = new System.Data.DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();

                            foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];

                            var geom = dr[GeometryColumn];
                            Geometry sqlGeometry = null;
                            if (geom != null && geom != DBNull.Value)
                                sqlGeometry = SqlGeometryConverter.ToSharpMapGeometry((Microsoft.SqlServer.Types.SqlGeometry) geom);
                            fdr.Geometry = sqlGeometry;
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

    }
}