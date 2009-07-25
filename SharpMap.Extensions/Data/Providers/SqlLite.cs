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
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;

namespace SharpMap.Data.Providers
{
    public class SqlLite : IProvider, IDisposable
    {
        //string conStr = "Data Source=C:\\Workspace\\test.sqlite;Version=3;";
        private string _connectionString;
        private string _defintionQuery;
        private string _geometryColumn;
        private bool _isOpen;
        private string _objectIdColumn;
        private int _srid = -2;
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
            get { return _connectionString; }
            set { _connectionString = value; }
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
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }

        #region IProvider Members

        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            Collection<Geometry> features = new Collection<Geometry>();
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string BoxIntersect = GetBoxClause(bbox);

                string strSQL = "SELECT " + GeometryColumn + " AS Geom ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += BoxIntersect;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                Geometry geom = GeometryFromWKT.Parse((string) dr[0]);
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

        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string strSQL = "SELECT " + ObjectIdColumn + " ";
                strSQL += "FROM " + Table + " WHERE ";

                strSQL += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + DefinitionQuery + " AND ";

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = (uint) (int) dr[0];
                                objectlist.Add(ID);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        public Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string strSQL = "SELECT " + GeometryColumn + " AS Geom FROM " + Table + " WHERE " + ObjectIdColumn +
                                "='" + oid.ToString() + "'";
                conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader())
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

        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            throw new NotImplementedException();
        }

        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string strSQL = "SELECT *, " + GeometryColumn + " AS sharpmap_tempgeometry ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += GetBoxClause(box);

                if (!String.IsNullOrEmpty(_defintionQuery))
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
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        public int GetFeatureCount()
        {
            int count = 0;
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int) command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        public FeatureDataRow GetFeature(uint RowID)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string strSQL = "SELECT *, " + GeometryColumn + " AS sharpmap_tempgeometry FROM " + Table + " WHERE " +
                                ObjectIdColumn + "='" + RowID.ToString() + "'";
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
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
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }
        }

        public BoundingBox GetExtents()
        {
            BoundingBox box = null;
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string strSQL =
                    "SELECT Min(minx) AS MinX, Min(miny) AS MinY, Max(maxx) AS MaxX, Max(maxy) AS MaxY FROM " + Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                        if (dr.Read())
                        {
                            box = new BoundingBox((double) dr[0], (double) dr[1], (double) dr[2], (double) dr[3]);
                        }
                    conn.Close();
                }
                return box;
            }
        }

        public string ConnectionID
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Don't really do anything. mssql's ConnectionPooling takes over here
            _isOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. mssql's ConnectionPooling takes over here
            _isOpen = false;
        }

        /// <summary>
        /// Spatial Reference ID
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set { _srid = value; }
        }

        public void Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

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

        private string GetBoxClause(BoundingBox bbox)
        {
            return String.Format(Map.NumberFormatEnUs,
                                 "(minx < {0} AND maxx > {1} AND miny < {2} AND maxy > {3})",
                                 bbox.Max.X, bbox.Min.X, bbox.Max.Y, bbox.Min.Y);
        }

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
        public static int CreateDataTable(IProvider datasource, string tablename, string connstr)
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
                catch
                {
                }
                //Create new table for storing the datasource
                string sql = "CREATE TABLE " + tablename + " (fid INTEGER PRIMARY KEY, geom TEXT, " +
                             "minx REAL, miny REAL, maxx REAL, maxy REAL, oid INTEGER";
                foreach (DataColumn col in columns)
                    if (col.DataType != typeof (String))
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
                    FeatureDataRow feature = datasource.GetFeature(idx);
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
                        Console.WriteLine(feature.Geometry.AsBinary().Length.ToString());
                        command.Parameters.AddWithValue("@geom", feature.Geometry.AsText()); //.AsBinary());
                        //command.Parameters["@geom"].Value = "X'" + ToHexString(feature.Geometry.AsBinary()) + "'"; //Add the geometry as Well-Known Binary
                        BoundingBox box = feature.Geometry.GetBoundingBox();
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