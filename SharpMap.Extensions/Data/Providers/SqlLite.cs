// Copyright 2007 - William Dollins
//
// This file is part of SqlLiteProvider.
// SqlLiteProvider is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SqlLiteProvider is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SqlLiteProvider; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Threading;
using GeoAPI.Features;
using SharpMap.Converters.WellKnownText;
using GeoAPI.Geometries;
using Common.Logging;

namespace SharpMap.Data.Providers
{
    public class SqlLite : PreparedGeometryProvider
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(SqlLite));

        //string conStr = "Data Source=C:\\Workspace\\test.sqlite;Version=3;";
        private string _definitionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private string _table;

        public SqlLite(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
        {
            ConnectionString = connectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName; //Name of column to store geometry
            ObjectIdColumn = oidColumnName; //Name of object ID column
        }

        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return ConnectionID; }
            set { ConnectionID = value; }
        }

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }

        /// <summary>
        /// Name of geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _geometryColumn; }
            set { _geometryColumn = value; }
        }

        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _objectIdColumn; }
            set { _objectIdColumn = value; }
        }

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _definitionQuery; }
            set { _definitionQuery = value; }
        }

        #region IProvider Members

        public override IEnumerable<IGeometry> GetGeometriesInView(Envelope view, CancellationToken? cancellationToken = null)
        {
            using (var conn = new SQLiteConnection(ConnectionID))
            {
                var boxIntersect = GetBoxClause(view);

                var strSQL = "SELECT " + GeometryColumn + " AS Geom ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += boxIntersect;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (var command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var geom = GeometryFromWKT.Parse((string) dr[0]);
                                if (geom != null)
                                    yield return geom;
                            }
                        }
                    }
                    conn.Close();
                }
            }
        }

        public override IEnumerable<object> GetOidsInView(Envelope bbox, CancellationToken? cancellationToken = null)
        {
            var objectlist = new Collection<uint>();
            using (var conn = new SQLiteConnection(ConnectionID))
            {
                var strSQL = "SELECT " + ObjectIdColumn + " ";
                strSQL += "FROM " + Table + " WHERE ";

                strSQL += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " AND " + DefinitionQuery + " AND ";

                using (var command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                yield return dr[0];
                            }
                        }
                    }
                    conn.Close();
                }
            }
        }

        public override IGeometry GetGeometryByOid(object oid)
        {
            IGeometry geom = null;
            using (var conn = new SQLiteConnection(ConnectionID))
            {
                string strSQL = "SELECT " + GeometryColumn + " AS Geom FROM " + Table + " WHERE " + ObjectIdColumn +
                                "=@POid";
                conn.Open();
                using (var command = new SQLiteCommand(strSQL, conn))
                {
                    command.Parameters.Add(new SQLiteParameter("POid", oid));
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = GeometryFromWKT.Parse((string) dr[0]);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        protected override void OnExecuteIntersectionQuery(IGeometry geom, IFeatureCollectionSet ds, CancellationToken? cancellationToken = null)
        {
            var fds = new FeatureDataSet();
            ExecuteIntersectionQuery(geom.EnvelopeInternal, fds);
            
            //index of last added feature data table
            var index = fds.Tables.Count - 1;
            if (index <= 0) return;

            var res = (FeatureDataTable)CloneTableStructure(fds.Tables[index].Clone());
            res.BeginLoadData();
            
            var fdt = fds.Tables[index];
            foreach (FeatureDataRow row in fdt.Rows)
            {
                if (PreparedGeometry.Intersects(row.Geometry))
                {
                    var fdr = (FeatureDataRow)res.LoadDataRow(row.ItemArray, true);
                    fdr.Geometry = row.Geometry;
                }
            }

            res.EndLoadData();
            ds.Add(res);
            fds.Dispose();
        }

        public override void ExecuteIntersectionQuery(Envelope box, IFeatureCollectionSet fcs, CancellationToken? cancellationToken = null)
        {
            using (var conn = new SQLiteConnection(ConnectionID))
            {
                string strSQL = "SELECT *, " + GeometryColumn + " AS sharpmap_tempgeometry ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += GetBoxClause(box);

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    DataSet ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" &&
                                !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" &&
                                    !col.ColumnName.StartsWith("Envelope_"))
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = GeometryFromWKT.Parse((string) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        fcs.Add(fdt);
                    }
                }
            }
        }

        public override int GetFeatureCount()
        {
            var count = 0;
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (var command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int) command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        public override IFeature GetFeatureByOid(object oid)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                string strSQL = "SELECT *, " + GeometryColumn + " AS sharpmap_tempgeometry FROM " + Table + " WHERE " +
                                ObjectIdColumn + "=@POid";
                using (var adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    adapter.SelectCommand.Parameters.Add(new SQLiteParameter("POid", oid));
                    var ds = new DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" &&
                                !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" &&
                                    !col.ColumnName.StartsWith("Envelope_"))
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = GeometryFromWKT.Parse((string) dr["sharpmap_tempgeometry"]);
                            return fdr;
                        }
                        return null;
                    }
                    return null;
                }
            }
        }

        public override Envelope GetExtents()
        {
            Envelope box = null;
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                string strSQL =
                    "SELECT Min(minx) AS MinX, Min(miny) AS MinY, Max(maxx) AS MaxX, Max(maxy) AS MaxY FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (var command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                        if (dr.Read())
                        {
                            box = new Envelope((double) dr[0], (double) dr[2], (double) dr[1], (double) dr[3]);
                        }
                    conn.Close();
                }
                return box;
            }
        }

        #endregion

        private static string GetBoxClause(Envelope bbox)
        {
            return String.Format(Map.NumberFormatEnUs,
                                 "(minx < {0} AND maxx > {1} AND miny < {2} AND maxy > {3})",
                                 bbox.MaxX, bbox.MinX, bbox.MaxY, bbox.MinY);
        }

        /// <summary>
        /// Creates a new table in a SQLite database and copies rows from an existing datasource.
        /// </summary>
        /// <remarks>
        /// <para>The datatable created will contain six extra columns besides the attribute data: "oid" (Object ID row), 
        /// "geom" (Geometry stored as WKT), and minx, miny, maxx, maxy for geometry bounding box.</para>
        /// <para>
        /// <example>
        /// Upload a ShapeFile to a database:
        /// <code>
        /// public void CreateDatabase(string shapeFile)
        /// {
        ///		if (!System.IO.File.Exists(shapeFile))
        ///		{
        ///			MessageBox.Show("File not found");
        ///			return;
        ///		}
        ///		ShapeFile shp = new ShapeFile(shapeFile, false);
        ///		//Create tablename from filename
        ///		string tablename = shapeFile.Substring(shapeFile.LastIndexOf('\\') + 1,
        ///			shapeFile.LastIndexOf('.') - shapeFile.LastIndexOf('\\') - 1);
        ///		//Create connectionstring
        ///		string connstr = @"Data Source=|DataDirectory|GeoDatabase.sqlite;Version=3";
        ///		int count = SharpMap.Data.Providers.SqlLite.CreateDataTable(shp, tablename, connstr);
        ///		MessageBox.Show("Uploaded " + count.ToString() + " features to datatable '" + tablename + "'");
        ///	}
        /// </code>
        /// </example>
        /// </para>
        /// </remarks>
        /// <param name="datasource">Datasource to upload</param>
        /// <param name="tablename">Name of table to create (existing table will be overwritten!)</param>
        /// <param name="connstr">Connection string to database</param>
        /// <returns>Number or rows inserted, -1 if failed and 0 if table created but no rows inserted.</returns>        
        public static int CreateDataTable(IProvider datasource, string tablename, string connstr)
        {
            datasource.Open();
            var geom = datasource.GetFeatureByOid(0);
            var columns = geom.Factory.AttributesDefinition;
            int counter = -1;
            using (var conn = new SQLiteConnection(connstr))
            {
                var command = new SQLiteCommand();
                command.Connection = conn;

                conn.Open();
                //Try to drop table if it exists
                try
                {
                    command.CommandText = "DROP TABLE \"" + tablename + "\";";
                    command.ExecuteNonQuery();
                }
                catch
                {
                }
                //Create new table for storing the datasource
                string sql = "CREATE TABLE " + tablename + " (fid INTEGER PRIMARY KEY, geom TEXT, " +
                             "minx REAL, miny REAL, maxx REAL, maxy REAL, oid INTEGER";
                foreach (var col in columns)
                    if (col.AttributeType != typeof (String))
                        sql += ", " + col.AttributeName + " " + Type2SqlLiteTypeString(col.AttributeType);
                    else
                        sql += ", " + col.AttributeName + " TEXT";
                command.CommandText = sql + ");";
                //command.CommandText = sql;
                command.ExecuteNonQuery();
                counter++;
                var indexes = datasource.GetOidsInView(datasource.GetExtents());
                //Select all indexes in shapefile, loop through each feature and insert them one-by-one
                foreach (var idx in indexes)
                {
                    //Get feature from provider
                    var feature = datasource.GetFeatureByOid(idx);
                    if (counter == 0)
                    {
                        //Create insert script
                        var strSQL = " (";
                        foreach (var col in columns)
                            strSQL += "@" + col.AttributeName + ",";

                        strSQL += "@geom,@minx,@miny, " +
                                  "@maxx,@maxy)";
                        strSQL = "INSERT INTO " + tablename + strSQL.Replace("@", "") + " VALUES" + strSQL;

                        command.CommandText = strSQL;
                        command.Parameters.Clear();
                        //Add datacolumn parameters
                        foreach (var col in columns)
                            command.Parameters.Add("@" + col.AttributeName, Type2SqlType(col.AttributeType));

                        //Add geometry parameters
                        //command.Parameters.Add("@geom", DbType.Binary);
                        command.Parameters.Add("@minx", DbType.Double);
                        command.Parameters.Add("@miny", DbType.Double);
                        command.Parameters.Add("@maxx", DbType.Double);
                        command.Parameters.Add("@maxy", DbType.Double);
                    }
                    //Set values
                    foreach (var col in columns)
                        command.Parameters["@" + col.AttributeName].Value = feature.Attributes[col.AttributeName];
                    if (feature.Geometry != null)
                    {
                        if (_logger.IsDebugEnabled)
                            _logger.Debug(feature.Geometry.AsBinary().Length.ToString(NumberFormatInfo.InvariantInfo));

                        command.Parameters.AddWithValue("@geom", feature.Geometry.AsText()); //.AsBinary());
                        //command.Parameters["@geom"].Value = "X'" + ToHexString(feature.Geometry.AsBinary()) + "'"; //Add the geometry as Well-Known Binary
                        var box = feature.Geometry.EnvelopeInternal;
                        command.Parameters["@minx"].Value = box.MinX;
                        command.Parameters["@miny"].Value = box.MinY;
                        command.Parameters["@maxx"].Value = box.MaxX;
                        command.Parameters["@maxy"].Value = box.MaxY;
                    }
                    else
                    {
                        command.Parameters["@geom"].Value = DBNull.Value;
                        command.Parameters["@minx"].Value = DBNull.Value;
                        command.Parameters["@miny"].Value = DBNull.Value;
                        command.Parameters["@maxx"].Value = DBNull.Value;
                        command.Parameters["@maxy"].Value = DBNull.Value;
                    }
                    //Insert row
                    command.ExecuteNonQuery();
                    counter++;
                }
                //Create indexes
                //command.Parameters.Clear();
                //command.CommandText = "CREATE INDEX [IDX_Envelope_MinX] ON " + tablename + " (Envelope_MinX)";
                //command.ExecuteNonQuery();
                //command.CommandText = "CREATE INDEX [IDX_Envelope_MinY] ON " + tablename + " (Envelope_MinY)";
                //command.ExecuteNonQuery();
                //command.CommandText = "CREATE INDEX [IDX_Envelope_MaxX] ON " + tablename + " (Envelope_MaxX)";
                //command.ExecuteNonQuery();
                //command.CommandText = "CREATE INDEX [IDX_Envelope_MaxY] ON " + tablename + " (Envelope_MaxY)";
                //command.ExecuteNonQuery();

                conn.Close();
            }
            datasource.Close();
            return counter;
        }

        private static TypeAffinity Type2SqlLiteType(Type t)
        {
            switch (t.ToString())
            {
                case "System.Boolean":
                    return TypeAffinity.Int64;
                case "System.Single":
                    return TypeAffinity.Double;
                case "System.Double":
                    return TypeAffinity.Double;
                case "System.Int16":
                    return TypeAffinity.Int64;
                case "System.Int32":
                    return TypeAffinity.Int64;
                case "System.Int64":
                    return TypeAffinity.Int64;
                case "System.DateTime":
                    return TypeAffinity.DateTime;
                case "System.Byte[]":
                    return TypeAffinity.Blob;
                case "System.String":
                    return TypeAffinity.Text;
                default:
                    throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
            }
        }

        private static string Type2SqlLiteTypeString(Type t)
        {
            switch (t.ToString())
            {
                case "System.Boolean":
                    return "INTEGER";
                case "System.Single":
                    return "REAL";
                case "System.Double":
                    return "REAL";
                case "System.Int16":
                    return "INTEGER";
                case "System.Int32":
                    return "INTEGER";
                case "System.Int64":
                    return "INTEGER";
                case "System.DateTime":
                    return "DATETIME";
                case "System.Byte[]":
                    return "BLOB";
                case "System.String":
                    return "TEXT";
                default:
                    throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
            }
        }

        private static DbType Type2SqlType(Type t)
        {
            switch (t.ToString())
            {
                case "System.Boolean":
                    return DbType.Int64;
                case "System.Single":
                    return DbType.Double;
                case "System.Double":
                    return DbType.Double;
                case "System.Int16":
                    return DbType.Int64;
                case "System.Int32":
                    return DbType.Int64;
                case "System.Int64":
                    return DbType.Int64;
                case "System.DateTime":
                    return DbType.DateTime;
                case "System.Byte[]":
                    return DbType.Binary;
                case "System.String":
                    return DbType.String;
                default:
                    throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
            }
        }

        public static string ToHexString(byte[] bytes)
        {
            char[] hexDigits = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
            char[] chars = new char[bytes.Length*2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i*2] = hexDigits[b >> 4];
                chars[i*2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }
    }
}