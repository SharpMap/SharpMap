// Copyright 2006 - Humberto Ferreira
// Oracle provider by Humberto Ferreira (humbertojdf@gmail.com)
//
// Date 2006-09-05
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Oracle.DataAccess.Client;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Oracle dataprovider
    /// </summary>
    /// <remarks>
    /// <para>This provider needs the Oracle software client installed on the PC where the application runs.
    /// If you need to connect to an Oracle database, it has to have oracle client (or Oracle Instant Client) installed. </para>
    /// <para>You can download Oracle Client here:
    /// http://www.oracle.com/technology/software/index.html</para>
    /// <para>If client don4t need an instance of Oracle, a better option is to use Oracle Instant client
    /// http://www.oracle.com/technology/tech/oci/instantclient/index.html</para>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    ///	string ConnStr = "Server=127.0.0.1;Port=5432;User Id=userid;Password=password;Database=myGisDb;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.Oracle(ConnStr, "myTable", "GeomColumn", "OidColumn");
    /// </code>
    /// </example>
    /// <para>SharpMap Oracle provider by Humberto Ferreira (humbertojdf at gmail com).</para>
    /// </remarks>
    [Serializable]
    public class Oracle : IProvider, IDisposable
    {
        private string _ConnectionString;
        private string _defintionQuery;
        private string _GeometryColumn;
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private int _srid = -2;
        private string _Table;

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public Oracle(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
        {
            ConnectionString = ConnectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = OID_ColumnName;
        }

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="datasource">Datasoure</param>
        /// <param name="tablename">Tablename</param>
        /// <param name="geometryColumnName">Geometry column name</param>
        /// <param name="OID_ColumnName">Object ID column</param>
        public Oracle(string username, string password, string datasource, string tablename, string geometryColumnName,
                      string OID_ColumnName)
            : this(
                "User Id=" + username + ";Password=" + password + ";Data Source=" + datasource, tablename,
                geometryColumnName, OID_ColumnName)
        {
        }


        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public Oracle(string ConnectionStr, string tablename, string OID_ColumnName)
            : this(ConnectionStr, tablename, "", OID_ColumnName)
        {
            GeometryColumn = GetGeometryColumn();
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
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
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
            //Don't really do anything. oracle's ConnectionPooling takes over here
            _IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. oracle's ConnectionPooling takes over here
            _IsOpen = false;
        }


        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            Collection<Geometry> features = new Collection<Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                //string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom ";
                string strSQL = "SELECT g." + GeometryColumn + ".Get_WKB() ";
                strSQL += " FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                Geometry geom = GeometryFromWKB.Parse((byte[]) dr[0]);
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
        public Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "SELECT g." + GeometryColumn + ".Get_WKB() FROM " + Table + " g WHERE " + ObjectIdColumn +
                                "='" + oid.ToString() + "'";
                conn.Open();
                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = GeometryFromWKB.Parse((byte[]) dr[0]);
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
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                string strSQL = "SELECT g." + ObjectIdColumn + " ";
                strSQL += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = (uint) (decimal) dr[0];
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
        /// Returns the features that intersects with 'geom'
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            List<Geometry> features = new List<Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strGeom = "MDSYS.SDO_GEOMETRY('" + geom.AsText() + "', #SRID#)";

                if (SRID > 0)
                {
                    strGeom = strGeom.Replace("#SRID#", SRID.ToString(Map.NumberFormatEnUs));
                }
                else
                {
                    strGeom = strGeom.Replace("#SRID#", "NULL");
                }

                strGeom = "SDO_RELATE(g." + GeometryColumn + ", " + strGeom +
                          ", 'mask=ANYINTERACT querytype=WINDOW') = 'TRUE'";

                string strSQL = "SELECT g.* , g." + GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry FROM " +
                                Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strGeom;

                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            int count = 0;
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (OracleCommand command = new OracleCommand(strSQL, conn))
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
            get
            {
                if (_srid == -2)
                {
                    string strSQL = "select SRID from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + Table + "'";

                    using (OracleConnection conn = new OracleConnection(_ConnectionString))
                    {
                        using (OracleCommand command = new OracleCommand(strSQL, conn))
                        {
                            try
                            {
                                conn.Open();
                                _srid = (int) (decimal) command.ExecuteScalar();
                                conn.Close();
                            }
                            catch
                            {
                                _srid = -1;
                            }
                        }
                    }
                }
                return _srid;
            }
            set { throw (new ApplicationException("Spatial Reference ID cannot by set on a Oracle table")); }
        }


        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "select g.* , g." + GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry from " +
                                Table + " g WHERE " + ObjectIdColumn + "='" + RowID.ToString() + "'";
                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    FeatureDataSet ds = new FeatureDataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
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
        public BoundingBox GetExtents()
        {
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "SELECT SDO_AGGR_MBR(g." + GeometryColumn + ").Get_WKT() FROM " + Table + " g ";
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    object result = command.ExecuteScalar();
                    conn.Close();
                    if (result == DBNull.Value)
                        return null;
                    string strBox = (string) result;
                    if (strBox.StartsWith("POLYGON", StringComparison.InvariantCultureIgnoreCase))
                    {
                        strBox = strBox.Replace("POLYGON", "");
                        strBox = strBox.Trim();
                        strBox = strBox.Replace("(", "");
                        strBox = strBox.Replace(")", "");

                        List<double> xX = new List<double>();
                        List<double> yY = new List<double>();

                        String[] points = strBox.Split(',');
                        String[] nums;
                        string point;

                        foreach (string s in points)
                        {
                            point = s.Trim();
                            nums = point.Split(' ');
                            xX.Add(double.Parse(nums[0], Map.NumberFormatEnUs));
                            yY.Add(double.Parse(nums[1], Map.NumberFormatEnUs));
                        }

                        double minX = Double.MaxValue;
                        double minY = Double.MaxValue;
                        double maxX = Double.MinValue;
                        double maxY = Double.MinValue;

                        foreach (double d in xX)
                        {
                            if (d > maxX)
                            {
                                maxX = d;
                            }
                            if (d < minX)
                            {
                                minX = d;
                            }
                        }

                        foreach (double d in yY)
                        {
                            if (d > maxY)
                            {
                                maxY = d;
                            }
                            if (d < minY)
                            {
                                minY = d;
                            }
                        }

                        return new BoundingBox(minX, minY, maxX, maxY);
                    }
                    else
                        return null;
                }
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
        public void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)
        {
            List<Geometry> features = new List<Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                string strSQL = "SELECT g.*, g." + GeometryColumn + ".Get_WKB() AS sharpmap_tempgeometry ";
                strSQL += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    DataSet ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        #endregion

        #region Disposers and finalizers

        private bool disposed = false;

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
            if (!disposed)
            {
                if (disposing)
                {
                    //Close();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~Oracle()
        {
            Dispose();
        }

        #endregion

        /// <summary>
        /// Returns the box filter string needed in SQL query
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        private string GetBoxFilterStr(BoundingBox bbox)
        {
            string strBbox = "SDO_FILTER(g." + GeometryColumn + ", mdsys.sdo_geometry(2003,#SRID#,NULL," +
                             "mdsys.sdo_elem_info_array(1,1003,3)," +
                             "mdsys.sdo_ordinate_array(" +
                             bbox.Min.X.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.Min.Y.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.Max.X.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.Max.Y.ToString(Map.NumberFormatEnUs) + ")), " +
                             "'querytype=window') = 'TRUE'";

            if (SRID > 0)
            {
                strBbox = strBbox.Replace("#SRID#", SRID.ToString(Map.NumberFormatEnUs));
            }
            else
            {
                strBbox = strBbox.Replace("#SRID#", "NULL");
            }
            return strBbox;
        }

        /// <summary>
        /// Returns all objects within a distance of a geometry
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [Obsolete("Use ExecuteIntersectionQuery instead")]
        public FeatureDataTable QueryFeatures(Geometry geom, double distance)
        {
            //List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strGeom = "MDSYS.SDO_GEOMETRY('" + geom.AsText() + "', #SRID#)";

                if (SRID > 0)
                {
                    strGeom = strGeom.Replace("#SRID#", SRID.ToString(Map.NumberFormatEnUs));
                }
                else
                {
                    strGeom = strGeom.Replace("#SRID#", "NULL");
                }

                strGeom = "SDO_WITHIN_DISTANCE(g." + GeometryColumn + ", " + strGeom + ", 'distance = " +
                          distance.ToString(Map.NumberFormatEnUs) + "') = 'TRUE'";

                string strSQL = "SELECT g.* , g." + GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry FROM " +
                                Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strGeom;

                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        return fdt;
                    }
                    else return null;
                }
            }
        }

        /// <summary>
        /// Convert WellKnownText to linestrings
        /// </summary>
        /// <param name="WKT"></param>
        /// <returns></returns>
        private LineString WktToLineString(string WKT)
        {
            LineString line = new LineString();
            WKT = WKT.Substring(WKT.LastIndexOf('(') + 1).Split(')')[0];
            string[] strPoints = WKT.Split(',');
            foreach (string strPoint in strPoints)
            {
                string[] coord = strPoint.Split(' ');
                line.Vertices.Add(new Point(double.Parse(coord[0], Map.NumberFormatEnUs),
                                            double.Parse(coord[1], Map.NumberFormatEnUs)));
            }
            return line;
        }

        /// <summary>
        /// Queries the Oracle database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSQL = "select COLUMN_NAME from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + Table + "'";
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            using (OracleCommand command = new OracleCommand(strSQL, conn))
            {
                conn.Open();
                object columnname = command.ExecuteScalar();
                conn.Close();
                if (columnname == DBNull.Value)
                    throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                return (string) columnname;
            }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        [Obsolete("Use ExecuteIntersectionQuery(box) instead")]
        public void GetFeaturesInView(BoundingBox bbox, FeatureDataSet ds)
        {
            GetFeaturesInView(bbox, ds);
        }
    }
}