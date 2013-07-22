// Copyright 2006 - Ricardo Stuven (rstuven@gmail.com)
// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
// MsSqlSpatial provider by Ricardo Stuven.
// Based on PostGIS provider by Morten Nielsen.
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
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using NetTopologySuite.IO;
using SharpMap.Converters.WellKnownBinary;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Microsoft SQL Server 2005 / MsSqlSpatial dataprovider
    /// </summary>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    /// string ConnStr = @"Data Source=localhost\sqlexpress;Initial Catalog=myGisDb;Integrated Security=SSPI;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.MsSqlSpatial(ConnStr, "myTable", "myId");
    /// </code>
    /// </example>
    [Serializable]
    public class MsSqlSpatial : SpatialDbProvider
    {
        /*
        private string _definitionQuery = String.Empty;
        private string _featureColumns = "*";
        private string _geometryColumn;
        private string _geometryExpression = "{0}";
        private bool _isOpen;
        private string _objectIdColumn;
        private string _orderQuery = String.Empty;
        //private int _srid = -2;
        private string _table;
        private int _targetSRID = -1;
        */
        /// <summary>
        /// Initializes a new connection to MsSqlSpatial
        /// </summary>
        /// <param name="connectionString">Connectionstring</param>
        /// <param name="tableName">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="identifierColumnName">Name of column with unique identifier</param>
        public MsSqlSpatial(string connectionString, string tableName, string geometryColumnName,
                            string identifierColumnName)
            : base(CreateSpatialDbUtility(), connectionString, GetSchemaName(tableName), GetTableName(tableName))
        {
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = identifierColumnName;
        }

        /// <summary>
        /// Creates a new <see cref="SpatialDbUtility"/> to handle database backends specifics
        /// </summary>
        /// <returns>A <see cref="SpatialDbUtility"/></returns>
        protected static SpatialDbUtility CreateSpatialDbUtility()
        {
            return new SpatialDbUtility();
        }



        private static string GetSchemaName(string qualifiedTable)
        {
            var tmp = qualifiedTable.Split(new [] {'.'}, 2);
            return tmp.Length == 1 
                ? "dbo" 
                : tmp[0].Replace("\"", "");
        }

        private static string GetTableName(string qualifiedTable)
        {
            var tmp = qualifiedTable.Split(new[] { '.' }, 2);
            return tmp.Length == 1 
                ? tmp[0].Replace("\"", "") 
                : tmp[1].Replace("\"", "");
        }

        /// <summary>
        /// Initializes a new connection to MsSqlSpatial
        /// </summary>
        /// <param name="connectionString">Connectionstring</param>
        /// <param name="tableName">Name of data table</param>
        /// <param name="identifierColumnName">Name of column with unique identifier</param>
        public MsSqlSpatial(string connectionString, string tableName, string identifierColumnName)
            : this(connectionString, tableName, "", identifierColumnName)
        {
            GeometryColumn = GetGeometryColumn();
        }

        //private string TargetGeometryColumn
        //{
        //    get
        //    {
        //        if (SRID > 0 && TargetSRID > 0 && SRID != TargetSRID)
        //            return "ST.Transform(" + GeometryColumn + "," + TargetSRID + ")";
        //        return GeometryColumn;
        //    }
        //}

        /// <summary>
        /// Gets a collection of columns in the dataset
        /// </summary>
        public DataColumnCollection Columns
        {
            get
            {
                throw new NotImplementedException();
                //using (SqlConnection conn = new SqlConnection(this.ConnectionString))
                //{
                //    System.Data.DataColumnCollection columns = new System.Data.DataColumnCollection();
                //    string strSQL = "SELECT column_name, udt_name FROM information_schema.columns WHERE table_name='" + this.Table + "' ORDER BY ordinal_position";
                //    using (SqlCommand command = new SqlCommand(strSQL, conn))
                //    {
                //        conn.Open();
                //        using (SqlDataReader dr = command.ExecuteReader())
                //        {
                //            while (dr.Read())
                //            {
                //                System.Data.DataColumn col = new System.Data.DataColumn((string)dr["column_name"]);
                //                switch((string)dr["udt_name"])
                //                {
                //                    case "int4":
                //                        col.DataType = typeof(Int32);
                //                        break;
                //                    case "int8":
                //                        col.DataType = typeof(Int64);
                //                        break;
                //                    case "varchar":
                //                        col.DataType = typeof(string);
                //                        break;
                //                    case "text":
                //                        col.DataType = typeof(string);
                //                        break;
                //                    case "bool":
                //                        col.DataType = typeof(bool);
                //                        break;
                //                    case "geometry":
                //                        col.DataType = typeof(SharpMap.Geometries.Geometry);
                //                        break;
                //                    default:
                //                        col.DataType = typeof(object);
                //                        break;
                //                }
                //                columns.Add(col);
                //            }
                //        }
                //    }
                //    return columns;
                //}
            }
        }

        #region IProvider Members

        /// <summary>
        /// Convenience function to create and open a connection to the database backend.
        /// </summary>
        /// <returns>An open connection to the database backend.</returns>
        protected override DbConnection CreateOpenDbConnection()
        {
 	        var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Convenience function to create a data adapter.
        /// </summary>
        /// <returns>An open connection to the database backend.</returns>
        protected override DbDataAdapter CreateDataAdapter()
        {
 	        return new SqlDataAdapter();
        }

        ///// <summary>
        ///// Returns geometries within the specified bounding box
        ///// </summary>
        ///// <param name="bbox"></param>
        ///// <returns></returns>
        //protected override Collection<IGeometry> GetGeometriesInViewInternal(Envelope bbox)
        //{
        //    var features = new Collection<IGeometry>();
        //    using (var conn = CreateOpenDbConnection())
        //    {
        //        var strSQL = "SELECT ST.AsBinary(" + BuildGeometryExpression() + ") ";
        //        strSQL += "FROM ST.FilterQuery" + BuildSpatialQuerySuffix() + "(" + BuildEnvelope(bbox) + ")";

        //        if (!String.IsNullOrEmpty(DefinitionQuery))
        //            strSQL += " WHERE " + DefinitionQuery;

        //        if (!String.IsNullOrEmpty(OrderQuery))
        //            strSQL += " ORDER BY " + OrderQuery;

        //        using (var command = new SqlCommand(strSQL, conn))
        //        {
        //            conn.Open();
        //            using (var dr = command.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    if (dr[0] != DBNull.Value)
        //                    {
        //                        var geom = GeometryFromWKB.Parse((byte[]) dr[0], Factory);
        //                        if (geom != null)
        //                            features.Add(geom);
        //                    }
        //                }
        //            }
        //            conn.Close();
        //        }
        //    }
        //    return features;
        //}

        ///// <summary>
        ///// Returns the geometry corresponding to the Object ID
        ///// </summary>
        ///// <param name="oid">Object ID</param>
        ///// <returns>geometry</returns>
        //protected override IGeometry GetGeometryByIDInternal(uint oid)
        //{
        //    using (var conn = CreateOpenDbConnection())
        //    {
        //        var strSQL = "SELECT ST.AsBinary(" + BuildGeometryExpression() + ") AS Geom FROM " + Table +
        //                     " WHERE " + ObjectIdColumn + "='" + oid.ToString() + "'";

        //        using (var command = new SqlCommand(strSQL, conn))
        //        {
        //            using (var dr = command.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    if (dr[0] != DBNull.Value)
        //                        return GeometryFromWKB.Parse((byte[]) dr[0], Factory);
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}


        /// <summary>
        /// Gets the object of features that lie within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox">The bounding box</param>
        /// <returns>A collection of object ids</returns>
        protected override Collection<uint> GetObjectIDsInViewInternal(Envelope bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = CreateOpenDbConnection())
            {
                using (var command = (SqlCommand)conn.CreateCommand())
                {
#pragma warning disable 612,618
                    var @where = !string.IsNullOrEmpty(DefinitionQuery)
                        ? DefinitionQuery 
                        : FeatureColumns.GetWhereClause(null);
#pragma warning restore 612,618
                    var strSQL = string.Format("SELECT _sm_.{4} FROM ST.FilterQueryWhere('{0}','{1}',{3},'{2}') AS _sm_;",
                                Table, GeometryColumn, @where, BuildEnvelope(bbox, command), ObjectIdColumn);

#pragma warning disable 612,618
                    if (!String.IsNullOrEmpty(OrderQuery))
                        strSQL += " ORDER BY " + OrderQuery;
#pragma restore 612,618

                    command.CommandText = strSQL;

                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!dr.IsDBNull(0))
                            {
                                var id = Convert.ToUInt32(dr[0]);
                                objectlist.Add(id);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        ///// <summary>
        ///// Returns the features that intersects with 'geom'
        ///// </summary>
        ///// <param name="geom"></param>
        ///// <param name="ds">FeatureDataSet to fill data into</param>
        //protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        //{
        //    var features = new List<IGeometry>();
        //    using (var conn = CreateOpenDbConnection())
        //    {
        //        string strGeom;
        //        if (TargetSRID > 0 && SRID > 0 && SRID != TargetSRID)
        //            strGeom = "ST.Transform(ST.GeomFromText('" + geom.AsText() + "'," + TargetSRID.ToString(Map.NumberFormatEnUs) + ")," +
        //                      SRID.ToString(Map.NumberFormatEnUs) + ")";
        //        else
        //            strGeom = "ST.GeomFromText('" + geom.AsText() + "', " + SRID.ToString(Map.NumberFormatEnUs) + ")";

        //        string strSQL = "SELECT " + FeatureColumns + ", ST.AsBinary(" + BuildGeometryExpression() +
        //                        ") As sharpmap_tempgeometry ";
        //        strSQL += "FROM ST.RelateQuery" + BuildSpatialQuerySuffix() + "(" + strGeom + ", 'intersects')";

        //        if (!String.IsNullOrEmpty(DefinitionQuery))
        //            strSQL += " WHERE " + DefinitionQuery;

        //        if (!String.IsNullOrEmpty(OrderQuery))
        //            strSQL += " ORDER BY " + OrderQuery;

        //        using (var adapter = new SqlDataAdapter(strSQL, conn))
        //        {
        //            conn.Open();
        //            adapter.Fill(ds);
        //            conn.Close();
        //            if (ds.Tables.Count > 0)
        //            {
        //                FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
        //                foreach (DataColumn col in ds.Tables[0].Columns)
        //                    if (col.ColumnName != GeometryColumn &&
        //                        !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
        //                        col.ColumnName != "sharpmap_tempgeometry")
        //                        fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
        //                foreach (DataRow dr in ds.Tables[0].Rows)
        //                {
        //                    FeatureDataRow fdr = fdt.NewRow();
        //                    foreach (DataColumn col in ds.Tables[0].Columns)
        //                        if (col.ColumnName != GeometryColumn &&
        //                            !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
        //                            col.ColumnName != "sharpmap_tempgeometry")
        //                            fdr[col.ColumnName] = dr[col];
        //                    if (dr["sharpmap_tempgeometry"] != DBNull.Value)
        //                        fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"], Factory);
        //                    fdt.AddRow(fdr);
        //                }
        //                ds.Tables.Add(fdt);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Spacial Reference ID handling
        ///// </summary>
        //protected override int SRID
        //{
        //    get { return base.SRID; }
        //    set
        //    {

        //        if (SRID == -2)
        //        return;
        //        {
        //            int dotPos = Table.IndexOf(".");
        //            string strSQL = "";
        //            if (dotPos == -1)
        //                strSQL = "select SRID from ST.GEOMETRY_COLUMNS WHERE F_TABLE_NAME='" + Table + "'";
        //            else
        //            {
        //                var schema = Table.Substring(0, dotPos);
        //                var table = Table.Substring(dotPos + 1);
        //                strSQL = "select SRID from ST.GEOMETRY_COLUMNS WHERE F_TABLE_SCHEMA='" + schema +
        //                         "' AND F_TABLE_NAME='" + table + "'";
        //            }

        //            using (var conn = (SqlConnection)CreateOpenDbConnection())
        //            {
        //                using (var command = new SqlCommand(strSQL, conn))
        //                {
        //                    try
        //                    {
        //                        conn.Open();
        //                        base.SRID = (int) command.ExecuteScalar();
        //                        conn.Close();
        //                    }
        //                    catch
        //                    {
        //                        base.SRID = -1;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}


        ///// <summary>
        ///// Returns a datarow based on a RowID
        ///// </summary>
        ///// <param name="rowId"></param>
        ///// <returns>datarow</returns>
        //protected override FeatureDataRow GetFeatureInternal(uint rowId)
        //{
        //    using (var conn = (SqlConnection)CreateOpenDbConnection())
        //    {
        //        string strSQL = "select " + FeatureColumns + ", ST.AsBinary(" + BuildGeometryExpression() +
        //                        ") As sharpmap_tempgeometry from " + Table + " WHERE " + ObjectIdColumn + "='" +
        //                        rowId.ToString() + "'";
        //        using (var adapter = new SqlDataAdapter(strSQL, conn))
        //        {
        //            FeatureDataSet ds = new FeatureDataSet();
        //            conn.Open();
        //            adapter.Fill(ds);
        //            conn.Close();
        //            if (ds.Tables.Count > 0)
        //            {
        //                FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
        //                foreach (DataColumn col in ds.Tables[0].Columns)
        //                    if (col.ColumnName != GeometryColumn &&
        //                        !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
        //                        col.ColumnName != "sharpmap_tempgeometry")
        //                        fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
        //                if (ds.Tables[0].Rows.Count > 0)
        //                {
        //                    DataRow dr = ds.Tables[0].Rows[0];
        //                    FeatureDataRow fdr = fdt.NewRow();
        //                    foreach (DataColumn col in ds.Tables[0].Columns)
        //                        if (col.ColumnName != GeometryColumn &&
        //                            !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
        //                            col.ColumnName != "sharpmap_tempgeometry")
        //                            fdr[col.ColumnName] = dr[col];
        //                    if (dr["sharpmap_tempgeometry"] != DBNull.Value)
        //                        fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
        //                    return fdr;
        //                }
        //                else
        //                    return null;
        //            }
        //            else
        //                return null;
        //        }
        //    }
        //}

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        protected override Envelope GetExtentsInternal()
        {
            using (var conn = (SqlConnection)CreateOpenDbConnection())
            {
#pragma warning disable 612,618
                var where = (String.IsNullOrEmpty(DefinitionQuery)
                                 ? FeatureColumns.GetWhereClause()
                                 : DefinitionQuery).Replace(" WHERE ", "").Replace("'", "''");
#pragma warning restore 612,618

                var strSQL = string.Format("SELECT ST.AsBinary(ST.EnvelopeQueryWhere('{0}', '{1}', '{2}'))", /*DbUtility.DecorateTable(Schema, Table)*/ /* Schema, */Table,
                                              GeometryColumn, where);
                
                using (var command = new SqlCommand(strSQL, conn))
                {
                    var result = command.ExecuteScalar();
                    return result == DBNull.Value 
                        ? null : 
                        GeometryFromWKB.Parse((byte[]) result, Factory).EnvelopeInternal;
                }
            }
        }

        ///// <summary>
        ///// Returns all features with the view box
        ///// </summary>
        ///// <param name="bbox">view box</param>
        ///// <param name="ds">FeatureDataSet to fill data into</param>
        //protected override void ExecuteIntersectionQueryInternal(Envelope bbox, FeatureDataSet ds)
        //{
        //    using (var conn = CreateOpenDbConnection())
        //    {
        //        string strSQL = "SELECT " + FeatureColumns + ", ST.AsBinary(" + BuildGeometryExpression() +
        //                        ") AS sharpmap_tempgeometry ";
        //        strSQL += "FROM ST.FilterQuery" + BuildSpatialQuerySuffix() + "(" + BuildEnvelope(bbox) + ")";

        //        if (!String.IsNullOrEmpty(DefinitionQuery))
        //            strSQL += " WHERE " + DefinitionQuery;

        //        if (!String.IsNullOrEmpty(OrderQuery))
        //            strSQL += " ORDER BY " + OrderQuery;

        //        using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
        //        {
        //            conn.Open();
        //            DataSet ds2 = new DataSet();
        //            adapter.Fill(ds2);
        //            conn.Close();
        //            if (ds2.Tables.Count > 0)
        //            {
        //                FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
        //                foreach (DataColumn col in ds2.Tables[0].Columns)
        //                    if (col.ColumnName != GeometryColumn &&
        //                        !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
        //                        col.ColumnName != "sharpmap_tempgeometry")
        //                        fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
        //                foreach (DataRow dr in ds2.Tables[0].Rows)
        //                {
        //                    FeatureDataRow fdr = fdt.NewRow();
        //                    foreach (DataColumn col in ds2.Tables[0].Columns)
        //                        if (col.ColumnName != GeometryColumn &&
        //                            !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
        //                            col.ColumnName != "sharpmap_tempgeometry")
        //                            fdr[col.ColumnName] = dr[col];
        //                    if (dr["sharpmap_tempgeometry"] != DBNull.Value)
        //                        fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
        //                    fdt.AddRow(fdr);
        //                }
        //                ds.Tables.Add(fdt);
        //            }
        //        }
        //    }
        //}

        #endregion

        #region Disposers and finalizers

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MsSqlSpatial()
        {
            Dispose();
        }

        #endregion

        /// <summary>
        /// Queries the MsSqlSpatial database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            var strSQL = "SELECT F_GEOMETRY_COLUMN from ST.GEOMETRY_COLUMNS WHERE F_TABLE_NAME='" + Table + "'";
            using (var conn = (SqlConnection)CreateOpenDbConnection())
            {
                using (var command = new SqlCommand(strSQL, conn))
                {
                    var columnname = command.ExecuteScalar();
                    if (columnname == DBNull.Value)
                        throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                    return (string) columnname;
                }
            }
        }

        private string BuildSpatialQuerySuffix()
        {
            return "#" + Schema + "#" + Table + "#" + GeometryColumn;
        }

        //private string BuildGeometryExpression()
        //{
        //    return string.Format(GeometryExpression, TargetGeometryColumn);
        //}

        /// <summary>
        /// Function to generate a spatial where clause for the intersection queries.
        /// </summary>
        /// <param name="bbox">The bounding box</param>
        /// <param name="command">The command object, that is supposed to execute the query.</param>
        /// <returns>The spatial component of a SQL where clause</returns>
        protected override string GetSpatialWhere(Envelope bbox, DbCommand command)
        {
            var sqlCommand = (SqlCommand) command;

#pragma warning disable 612,618
            var pwhere = new SqlParameter("@PWhere", !string.IsNullOrEmpty(DefinitionQuery)
                                                         ? DefinitionQuery
                                                         : FeatureColumns.GetWhereClause(null));
#pragma warning restore 612,618
            sqlCommand.Parameters.Add(pwhere);

            return string.Format("{3} IN (SELECT _tmp_.{3} FROM ST.FilterQueryWhere('{0}', '{1}', {2}, @PWhere) AS _tmp_)",
                                  Table, GeometryColumn, BuildEnvelope(bbox, sqlCommand), DbUtility.DecorateColumn(ObjectIdColumn));
        }

        private string BuildEnvelope(Envelope bbox, SqlCommand command)
        {
            var res = "ST.MakeEnvelope(@PMinX,@PMinY,@PMaxX,@PMaxY,@PTargetSrid)";

            var needsTransform = NeedsTransform;
            command.Parameters.AddRange(
                new[]
                    {
                        new SqlParameter("@PMinX", bbox.MinX),
                        new SqlParameter("@PMinY", bbox.MinY),
                        new SqlParameter("@PMaxX", bbox.MaxX),
                        new SqlParameter("@PMaxY", bbox.MaxY),
                        new SqlParameter("@PTargetSrid", needsTransform ? TargetSRID : SRID)
                    });

            if (needsTransform)
            {
                res = string.Format("ST.Transform({0}, @PSrid)", res);
                command.Parameters.AddWithValue("@PSrid", SRID);
            }

            return res;
        }

        private string BuildGeometry(IGeometry geometry, SqlCommand command)
        {
            var res = "ST.GeomFromWKB(@PGeom,@PTargetSrid)";

            var needsTransform = NeedsTransform;
            command.Parameters.AddRange(
                new[]
                    {
                        new SqlParameter("@PGeom", geometry.AsBinary()),
                        new SqlParameter("@PTargetSrid", needsTransform ? TargetSRID : SRID)
                    });

            if (needsTransform)
            {
                res = string.Format("ST.Transform({0}, @PSrid)", res);
                command.Parameters.AddWithValue("@PSrid", SRID);
            }

            return res;
        }

        /// <summary>
        /// Function to generate a spatial where clause for the intersection queries.
        /// </summary>
        /// <param name="bbox">The geometry</param>
        /// <param name="command">The command object, that is supposed to execute the query.</param>
        /// <returns>The spatial component of a SQL where clause</returns>
        protected override string GetSpatialWhere(IGeometry bbox, DbCommand command)
        {
            var sqlCommand = (SqlCommand)command;

#pragma warning disable 612,618
            var pwhere = new SqlParameter("@PWhere", !string.IsNullOrEmpty(DefinitionQuery)
                                                         ? DefinitionQuery
                                                         : FeatureColumns.GetWhereClause(null));
#pragma warning restore 612,618
            sqlCommand.Parameters.Add(pwhere);

            return string.Format("{3} IN (SELECT _tmp_.{3} FROM ST.RelateQueryWhere('{0}', '{1}', {2}, 'Intersects', @PWhere) AS _tmp_)",
                                  Table, GeometryColumn, BuildGeometry(bbox, sqlCommand), DbUtility.DecorateColumn(ObjectIdColumn));
        }
    }
}