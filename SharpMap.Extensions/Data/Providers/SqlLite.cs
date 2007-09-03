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

using SharpMap;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Converters.WellKnownBinary;

namespace SharpMap.Data.Providers
{
    public class SqlLite : IProvider, IDisposable
    {
        //string conStr = "Data Source=C:\\Workspace\\test.sqlite;Version=3;";
        public SqlLite(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
		{
			this.ConnectionString = ConnectionStr;
			this.Table = tablename;
			this.GeometryColumn = geometryColumnName; //Name of column to store geometry
			this.ObjectIdColumn = OID_ColumnName; //Name of object ID column
		}

        #region IProvider Members

        public System.Collections.ObjectModel.Collection<SharpMap.Geometries.Geometry> GetGeometriesInView(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string BoxIntersect = GetBoxClause(bbox);

                string strSQL = "SELECT " + this.GeometryColumn + " AS Geom ";
                strSQL += "FROM " + this.Table + " WHERE ";
                strSQL += BoxIntersect;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + this.DefinitionQuery;

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                SharpMap.Geometries.Geometry geom = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse((string)dr[0]);
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

        public System.Collections.ObjectModel.Collection<uint> GetObjectIDsInView(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string strSQL = "SELECT " + this.ObjectIdColumn + " ";
                strSQL += "FROM " + this.Table + " WHERE ";

                strSQL += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + this.DefinitionQuery + " AND ";

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = (uint)(int)dr[0];
                                objectlist.Add(ID);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
        {
            SharpMap.Geometries.Geometry geom = null;
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string strSQL = "SELECT " + this.GeometryColumn + " AS Geom FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + oid.ToString() + "'";
                conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse((string)dr[0]);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        public void ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom, FeatureDataSet ds)
        {
            throw new NotImplementedException();
        }

        public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox box, FeatureDataSet ds)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string strSQL = "SELECT *, " + this.GeometryColumn + " AS sharpmap_tempgeometry ";
                strSQL += "FROM " + this.Table + " WHERE ";
                strSQL += GetBoxClause(box);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + this.DefinitionQuery;

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    System.Data.DataSet ds2 = new System.Data.DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
                        {
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse((string)dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        public int GetFeatureCount()
        {
            int count = 0;
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + this.Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + this.DefinitionQuery;
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int)command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        public FeatureDataRow GetFeature(uint RowID)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string strSQL = "SELECT *, " + this.GeometryColumn + " AS sharpmap_tempgeometry FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + RowID.ToString() + "'";
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow dr = ds.Tables[0].Rows[0];
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse((string)dr["sharpmap_tempgeometry"]);
                            return fdr;
                        }
                        else
                            return null;

                    }
                    else
                        return null;
                }
            }
        }

        public SharpMap.Geometries.BoundingBox GetExtents()
        {
            SharpMap.Geometries.BoundingBox box = null;
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string strSQL = "SELECT Min(minx) AS MinX, Min(miny) AS MinY, Max(maxx) AS MaxX, Max(maxy) AS MaxY FROM " + this.Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + this.DefinitionQuery;
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                        if (dr.Read())
                        {
                            box = new SharpMap.Geometries.BoundingBox((double)dr[0], (double)dr[1], (double)dr[2], (double)dr[3]);
                        }
                    conn.Close();
                }
                return box;
            }
        }

        public string ConnectionID
        {
            get { return _ConnectionString; }
        }

        private bool _IsOpen;

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Don't really do anything. mssql's ConnectionPooling takes over here
            _IsOpen = true;
        }
        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. mssql's ConnectionPooling takes over here
            _IsOpen = false;
        }

        private int _srid = -2;

        /// <summary>
        /// Spatial Reference ID
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set { _srid = value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }

        //internal void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            //Close();
        //        }
        //        disposed = true;
        //    }
        //}
        #endregion

        #region Native Members

        private string _ConnectionString;

        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        private string _Table;

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }

        private string _GeometryColumn;

        /// <summary>
        /// Name of geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _GeometryColumn; }
            set { _GeometryColumn = value; }
        }

        private string _ObjectIdColumn;

        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }

        private string GetBoxClause(SharpMap.Geometries.BoundingBox bbox)
        {
            return String.Format(SharpMap.Map.numberFormat_EnUS,
                "(minx < {0} AND maxx > {1} AND miny < {2} AND maxy > {3})",
                bbox.Max.X, bbox.Min.X, bbox.Max.Y, bbox.Min.Y);
        }

        private string _defintionQuery;

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }
        
#endregion

        #region Static Members

		/// <summary>
		/// Creates a new table in a Microsoft SQL Server database and copies rows from an existing datasource.
		/// </summary>
		/// <remarks>
		/// <para>The datatable created will contain six extra columns besides the attribute data: "OID" (Object ID row), 
		/// "WKB_Geometry" (Geometry stored as WKB), and Envelope_MinX, Envelope_MinY, Envelope_MaxX, Envelope_MaxY
		/// for geometry bounding box.</para>
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
		///		string connstr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|GeoDatabase.mdf;Integrated Security=True;User Instance=True";
		///		int count = SharpMap.Data.Providers.MsSql.CreateDataTable(shp, tablename, connstr);
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
        public static int CreateDataTable(SharpMap.Data.Providers.IProvider datasource, string tablename, string connstr)
        {
            datasource.Open();
            FeatureDataRow geom = datasource.GetFeature(0);
            DataColumnCollection columns = geom.Table.Columns;
            int counter = -1;
            using (SQLiteConnection conn = new SQLiteConnection(connstr))
            {
                SQLiteCommand command = new SQLiteCommand();
                command.Connection = conn;

                conn.Open();
                //Try to drop table if it exists
                try
                {
                    command.CommandText = "DROP TABLE \"" + tablename + "\";";
                    command.ExecuteNonQuery();
                }
                catch { }
                //Create new table for storing the datasource
                string sql = "CREATE TABLE " + tablename + " (fid INTEGER PRIMARY KEY, geom TEXT, " +
                    "minx REAL, miny REAL, maxx REAL, maxy REAL, oid INTEGER";
                foreach (DataColumn col in columns)
                    if (col.DataType != typeof(String))
                        sql += ", " + col.ColumnName + " " + Type2SqlLiteTypeString(col.DataType);
                    else
                        sql += ", " + col.ColumnName + " TEXT";
                command.CommandText = sql + ");";
                //command.CommandText = sql;
                command.ExecuteNonQuery();
                counter++;
                Collection<uint> indexes = datasource.GetObjectIDsInView(datasource.GetExtents());
                //Select all indexes in shapefile, loop through each feature and insert them one-by-one
                foreach (uint idx in indexes)
                {
                    //Get feature from shapefile
                    SharpMap.Data.FeatureDataRow feature = datasource.GetFeature(idx);
                    if (counter == 0)
                    {

                        //Create insert script
                        string strSQL = " (";
                        foreach (DataColumn col in feature.Table.Columns)
                            strSQL += "@" + col.ColumnName + ",";

                        strSQL += "@geom,@minx,@miny, " +
                                    "@maxx,@maxy)";
                        strSQL = "INSERT INTO " + tablename + strSQL.Replace("@", "") + " VALUES" + strSQL;

                        command.CommandText = strSQL;
                        command.Parameters.Clear();
                        //Add datacolumn parameters
                        foreach (DataColumn col in feature.Table.Columns)
                            command.Parameters.Add("@" + col.ColumnName, Type2SqlType(col.DataType));

                        //Add geometry parameters
                        //command.Parameters.Add("@geom", DbType.Binary);
                        command.Parameters.Add("@minx", DbType.Double);
                        command.Parameters.Add("@miny", DbType.Double);
                        command.Parameters.Add("@maxx", DbType.Double);
                        command.Parameters.Add("@maxy", DbType.Double);
                    }
                    //Set values
                    foreach (DataColumn col in feature.Table.Columns)
                        command.Parameters["@" + col.ColumnName].Value = feature[col];
                    if (feature.Geometry != null)
                    {
                        System.Console.WriteLine(feature.Geometry.AsBinary().Length.ToString());
                        command.Parameters.AddWithValue("@geom", feature.Geometry.AsText()); //.AsBinary());
                        //command.Parameters["@geom"].Value = "X'" + ToHexString(feature.Geometry.AsBinary()) + "'"; //Add the geometry as Well-Known Binary
                        SharpMap.Geometries.BoundingBox box = feature.Geometry.GetBoundingBox();
                        command.Parameters["@minx"].Value = box.Left;
                        command.Parameters["@miny"].Value = box.Bottom;
                        command.Parameters["@maxx"].Value = box.Right;
                        command.Parameters["@maxy"].Value = box.Top;
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
                case "System.Boolean": return System.Data.SQLite.TypeAffinity.Int64;
                case "System.Single": return System.Data.SQLite.TypeAffinity.Double;
                case "System.Double": return System.Data.SQLite.TypeAffinity.Double;
                case "System.Int16": return System.Data.SQLite.TypeAffinity.Int64;
                case "System.Int32": return System.Data.SQLite.TypeAffinity.Int64;
                case "System.Int64": return System.Data.SQLite.TypeAffinity.Int64;
                case "System.DateTime": return System.Data.SQLite.TypeAffinity.DateTime;
                case "System.Byte[]": return System.Data.SQLite.TypeAffinity.Blob;
                case "System.String": return System.Data.SQLite.TypeAffinity.Text;
                default:
                    throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
            }
        }

        private static string Type2SqlLiteTypeString(Type t)
        {
            switch (t.ToString())
            {
                case "System.Boolean": return "INTEGER";
                case "System.Single": return "REAL";
                case "System.Double": return "REAL";
                case "System.Int16": return "INTEGER";
                case "System.Int32": return "INTEGER";
                case "System.Int64": return "INTEGER";
                case "System.DateTime": return "DATETIME";
                case "System.Byte[]": return "BLOB";
                case "System.String": return "TEXT";
                default:
                    throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
            }
        }

        private static DbType Type2SqlType(Type t)
        {
            switch (t.ToString())
            {
                case "System.Boolean": return System.Data.DbType.Int64;
                case "System.Single": return System.Data.DbType.Double;
                case "System.Double": return System.Data.DbType.Double;
                case "System.Int16": return System.Data.DbType.Int64;
                case "System.Int32": return System.Data.DbType.Int64;
                case "System.Int64": return System.Data.DbType.Int64;
                case "System.DateTime": return System.Data.DbType.DateTime;
                case "System.Byte[]": return System.Data.DbType.Binary;
                case "System.String": return System.Data.DbType.String;
                default:
                    throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
            }
        }

        public static string ToHexString(byte[] bytes)
        {
            char[] hexDigits = {'0', '1', '2', '3', '4', '5', '6', '7','8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        #endregion
    }
}
