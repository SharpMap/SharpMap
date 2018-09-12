// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
using System.Data.SqlClient;
using SharpMap.Converters.WellKnownBinary;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Microsoft SQL data provider
    /// </summary>
    /// <remarks>
    /// <para>
    /// The SQL data table MUST contain five data columns: A binary or image column (a Geometry Column) for storing WKB formatted geometries, 
    /// and four real values holding the boundingbox of the geometry. These must be named: Envelope_MinX, Envelope_MinY, Envelope_MaxX and Envelope_MaxY.
    /// Any extra columns will be returns as feature data.
    /// </para>
    /// <para>For creating a valid MS SQL datatable for SharpMap, see <see cref="CreateDataTable"/> 
    /// for creating and uploading a datasource to MS SQL Server.</para>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    /// string ConnStr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|GeoDatabase.mdf;Integrated Security=True;User Instance=True";
    /// myLayer.DataSource = new SharpMap.Data.Providers.MsSql(ConnStr, "myTable");
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable]
    [Obsolete("Use MsSqlSpatial provider instead")]
    public class MsSql : IProvider
    {
        private string _ConnectionString;
        private string _definitionQuery;
        private string _GeometryColumn;
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private int _srid = -2;

        private string _Table;
        private IGeometryFactory _factory;

        /// <summary>
        /// Gets or sets the geometry factory used to create geometries
        /// </summary>
        public IGeometryFactory Factory
        {
            get { return _factory ?? (_factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(_srid)); }
            set { _factory = value; }
        }

        /// <summary>
        /// Initializes a new connection to MS Sql Server
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public MsSql(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
        {
            ConnectionString = ConnectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName; //Name of column to store geometry
            ObjectIdColumn = OID_ColumnName; //Name of object ID column
        }

        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }

        /// <summary>
        /// Name of geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _GeometryColumn; }
            set { _GeometryColumn = value; }
        }

        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _definitionQuery; }
            set { _definitionQuery = value; }
        }

        /// <summary>
        /// Gets a collection of columns in the dataset
        /// </summary>
        public DataColumnCollection Columns
        {
            get { throw new NotImplementedException(); }
        }

        #region IProvider Members

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


        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var features = new Collection<IGeometry>();
            using (var conn = new SqlConnection(_ConnectionString))
            {
                string BoxIntersect = GetBoxClause(bbox);

                string strSQL = "SELECT " + GeometryColumn + " AS Geom ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += BoxIntersect;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var geom = GeometryFromWKB.Parse((byte[]) dr[0], Factory);
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
        public IGeometry GetGeometryByID(uint oid)
        {
            IGeometry geom = null;
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT " + GeometryColumn + " AS Geom FROM " + Table + " WHERE " + ObjectIdColumn +
                                "='" + oid.ToString() + "'";
                conn.Open();
                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = GeometryFromWKB.Parse((byte[]) dr[0], Factory);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT " + ObjectIdColumn + " ";
                strSQL += "FROM " + Table + " WHERE ";

                strSQL += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " AND " + DefinitionQuery + " AND ";

                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
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

        /// <summary>
        /// Returns the features that intersects with 'geom' [NOT IMPLEMENTED]
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            int count = 0;
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int) command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        /// <summary>
        /// Spacial Reference ID
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set
            {
                if (value == _srid)
                    return;
                _srid = value;
                _factory = null;
            }
        }

        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint rowId)
        {
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT *, " + GeometryColumn + " AS sharpmap_tempgeometry FROM " + Table + " WHERE " +
                                ObjectIdColumn + "='" + rowId.ToString() + "'";
                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
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
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"], Factory);
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

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public Envelope GetExtents()
        {
            Envelope box = null;
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL =
                    "SELECT Min(Envelope_MinX) AS MinX, Min(Envelope_MinY) AS MinY, Max(Envelope_MaxX) AS MaxX, Max(Envelope_MaxY) AS MaxY FROM " +
                    Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                        if (dr.Read())
                        {
                            box = new Envelope(new Coordinate((float) dr[0], (float) dr[1]), new Coordinate((float) dr[2], (float) dr[3]));
                        }
                    conn.Close();
                }
                return box;
            }
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        public string ConnectionID
        {
            get { return _ConnectionString; }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            //List<Geometries.Geometry> features = new List<GeoAPI.Geometries.IGeometry>();
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT *, " + GeometryColumn + " AS sharpmap_tempgeometry ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
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
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"], Factory);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        #endregion

        #region Disposers and finalizers

        private bool _disposed;

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Close();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MsSql()
        {
            Dispose();
        }

        #endregion

        private static string GetBoxClause(Envelope bbox)
        {
            return String.Format(Map.NumberFormatEnUs,
                                 "(Envelope_MinX <= {0} AND Envelope_MaxX >= {1} AND Envelope_MinY <= {2} AND Envelope_MaxY >= {3})",
                                 bbox.MaxX, bbox.MinX, bbox.MaxY, bbox.MinY);
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
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                SqlCommand command = new SqlCommand();
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
                string sql = "CREATE TABLE " + tablename + " (oid INTEGER IDENTITY PRIMARY KEY, WKB_Geometry Image, " +
                             "Envelope_MinX real, Envelope_MinY real, Envelope_MaxX real, Envelope_MaxY real";
                foreach (DataColumn col in columns)
                    if (col.DataType != typeof (String))
                        sql += ", " + col.ColumnName + " " + Type2SqlType(col.DataType).ToString();
                    else
                        sql += ", " + col.ColumnName + " VARCHAR(256)";
                command.CommandText = sql + ");";
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

                        strSQL += "@WKB_Geometry,@Envelope_MinX,@Envelope_MinY, " +
                                  "@Envelope_MaxX,@Envelope_MaxY)";
                        strSQL = "INSERT INTO " + tablename + strSQL.Replace("@", "") + " VALUES" + strSQL;

                        command.CommandText = strSQL;
                        command.Parameters.Clear();
                        //Add datacolumn parameters
                        foreach (DataColumn col in feature.Table.Columns)
                            command.Parameters.Add("@" + col.ColumnName, Type2SqlType(col.DataType));

                        //Add geometry parameters
                        command.Parameters.Add("@WKB_Geometry", SqlDbType.VarBinary);
                        command.Parameters.Add("@Envelope_MinX", SqlDbType.Real);
                        command.Parameters.Add("@Envelope_MinY", SqlDbType.Real);
                        command.Parameters.Add("@Envelope_MaxX", SqlDbType.Real);
                        command.Parameters.Add("@Envelope_MaxY", SqlDbType.Real);
                    }
                    //Set values
                    foreach (DataColumn col in feature.Table.Columns)
                        command.Parameters["@" + col.ColumnName].Value = feature[col];
                    if (feature.Geometry != null)
                    {
                        command.Parameters["@WKB_Geometry"].Value = feature.Geometry.AsBinary();
                            //Add the geometry as Well-Known Binary
                        Envelope  box = feature.Geometry.EnvelopeInternal;
                        command.Parameters["@Envelope_MinX"].Value = box.MinX;
                        command.Parameters["@Envelope_MinY"].Value = box.MinY;
                        command.Parameters["@Envelope_MaxX"].Value = box.MaxX;
                        command.Parameters["@Envelope_MaxY"].Value = box.MaxY;
                    }
                    else
                    {
                        command.Parameters["@WKB_Geometry"].Value = DBNull.Value;
                        command.Parameters["@Envelope_MinX"].Value = DBNull.Value;
                        command.Parameters["@Envelope_MinY"].Value = DBNull.Value;
                        command.Parameters["@Envelope_MaxX"].Value = DBNull.Value;
                        command.Parameters["@Envelope_MaxY"].Value = DBNull.Value;
                    }
                    //Insert row
                    command.ExecuteNonQuery();
                    counter++;
                }
                //Create indexes
                command.Parameters.Clear();
                command.CommandText = "CREATE INDEX [IDX_Envelope_MinX] ON " + tablename + " (Envelope_MinX)";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE INDEX [IDX_Envelope_MinY] ON " + tablename + " (Envelope_MinY)";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE INDEX [IDX_Envelope_MaxX] ON " + tablename + " (Envelope_MaxX)";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE INDEX [IDX_Envelope_MaxY] ON " + tablename + " (Envelope_MaxY)";
                command.ExecuteNonQuery();

                conn.Close();
            }
            datasource.Close();
            return counter;
        }

        /// <summary>
        /// Returns the name of the SqlServer datatype based on a .NET datatype
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static SqlDbType Type2SqlType(Type t)
        {
            switch (t.ToString())
            {
                case "System.Boolean":
                    return SqlDbType.Bit;
                case "System.Single":
                    return SqlDbType.Real;
                case "System.Double":
                    return SqlDbType.Float;
                case "System.Int16":
                    return SqlDbType.SmallInt;
                case "System.Int32":
                    return SqlDbType.Int;
                case "System.Int64":
                    return SqlDbType.BigInt;
                case "System.DateTime":
                    return SqlDbType.DateTime;
                case "System.Byte[]":
                    return SqlDbType.Image;
                case "System.String":
                    return SqlDbType.VarChar;
                default:
                    throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
            }
        }
    }
}
