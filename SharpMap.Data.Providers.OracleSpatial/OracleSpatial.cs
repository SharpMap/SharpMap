// Copyright 2014 - Peter Löfås
// Oracle Spatial provider by Peter Löfås ( peter.lofas@triona.se)
//
// Date 2014-01-14
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

using GeoAPI.Geometries;
using Oracle.DataAccess.Client;
using SharpMap.Data.Providers.OracleUDT;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using Geometry = GeoAPI.Geometries.IGeometry;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Oracle Spatial Data Provider
    /// Uses ODP.NET, ODAC and UserDefinedTypes to access OracleSpatial data to get enhanced performance compared to Managed Data Access
    /// 
    /// Deployment need to have the same version of Oracle DataAccess and ODAC as built against for UDT's to work properly. 
    /// XCopy 
    /// Oracle.DataAccess.dll
    /// oci.dll
    /// orannzsbb11.dll
    /// oraociicus11.dll
    /// OraOps11W.dll 
    /// 
    /// from build-directoy to deployment directory to run
    /// </summary>
    /// <remarks>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    ///	string ConnStr = "Server=127.0.0.1;Port=5432;User Id=userid;Password=password;Database=myGisDb;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.Oracle(ConnStr, "myTable", "GeomColumn", "OidColumn");
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable]
    public class OracleSpatial : BaseProvider
    {
        private string _definitionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private string _table;

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="connectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="oidColumnName">Name of column with unique identifier</param>
        public OracleSpatial(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
            : base(-2)
        {
            ConnectionString = connectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = oidColumnName;
        }

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="datasource">Datasoure</param>
        /// <param name="tablename">Tablename</param>
        /// <param name="geometryColumnName">Geometry column name</param>
        /// <param name="oidColumnName">Object ID column</param>
        public OracleSpatial(string username, string password, string datasource, string tablename, string geometryColumnName,
                      string oidColumnName)
            : this(
                "User Id=" + username + ";Password=" + password + ";Data Source=" + datasource, tablename,
                geometryColumnName, oidColumnName)
        {
        }


        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="connectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="oidColumnName">Name of column with unique identifier</param>
        public OracleSpatial(string connectionStr, string tablename, string oidColumnName)
            : this(connectionStr, tablename, "", oidColumnName)
        {
            GeometryColumn = GetGeometryColumn();
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

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<Geometry> GetGeometriesInView(Envelope bbox)
        {
            var features = new Collection<Geometry>();
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                //string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom ";
                string strSql = "SELECT g." + GeometryColumn + " as Geom ";
                strSql += " FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var geom = dr[0] as SdoGeometry;
                                if (geom != null)
                                    features.Add(geom.AsGeometry());
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
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "SELECT g." + GeometryColumn + " as Geom FROM " + Table + " g WHERE " + ObjectIdColumn +
                                "='" + oid.ToString(CultureInfo.InvariantCulture) + "'";
                conn.Open();
                using (var command = new OracleCommand(strSql, conn))
                {
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var sdoGeom = dr[0] as SdoGeometry;
                                if (sdoGeom != null)
                                    geom = sdoGeom.AsGeometry();
                            }
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
        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                string strSql = "SELECT g." + ObjectIdColumn + " ";
                strSql += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var id = (uint)(decimal)dr[0];
                                objectlist.Add(id);
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
        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strGeom = "MDSYS.SDO_GEOMETRY('" + geom.AsText() + "', #SRID#)";

                strGeom = strGeom.Replace("#SRID#", SRID > 0 ? SRID.ToString(Map.NumberFormatEnUs) : "NULL");

                strGeom = "SDO_RELATE(g." + GeometryColumn + ", " + strGeom +
                          ", 'mask=ANYINTERACT querytype=WINDOW') = 'TRUE'";

                string strSql = "SELECT * FROM " +
                                Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strGeom;

                using (var adapter = new OracleDataAdapter(strSql, conn))
                {
                    using (var sourceDataSet = new DataSet())
                    {
                        conn.Open();
                        adapter.Fill(sourceDataSet);
                        conn.Close();
                        if (sourceDataSet.Tables.Count > 0)
                        {
                            var fdt = new FeatureDataTable(sourceDataSet.Tables[0]);
                            foreach (DataColumn col in sourceDataSet.Tables[0].Columns)
                                if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                    fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                            foreach (DataRow dr in sourceDataSet.Tables[0].Rows)
                            {
                                var fdr = fdt.NewRow();
                                foreach (DataColumn col in sourceDataSet.Tables[0].Columns)
                                    if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                        fdr[col.ColumnName] = dr[col];
                                var sdoGeometry = dr[GeometryColumn] as SdoGeometry;

                                if (sdoGeometry != null)
                                {
                                    fdr.Geometry = sdoGeometry.AsGeometry();
                                }

                                fdt.AddRow(fdr);
                            }
                            ds.Tables.Add(fdt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public override int GetFeatureCount()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " WHERE " + DefinitionQuery;
                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
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
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "select * from " +
                                Table + " g WHERE " + ObjectIdColumn + "='" + rowId.ToString(NumberFormatInfo.InvariantInfo) + "'";
                using (var adapter = new OracleDataAdapter(strSql, conn))
                {
                    var sourceDataset = new DataSet();
                    conn.Open();
                    adapter.Fill(sourceDataset);
                    conn.Close();
                    if (sourceDataset.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(sourceDataset.Tables[0]);
                        foreach (DataColumn col in sourceDataset.Tables[0].Columns)
                            if (string.Compare(col.ColumnName, GeometryColumn,CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);

                        if (sourceDataset.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = sourceDataset.Tables[0].Rows[0];
                            var fdr = fdt.NewRow();
                            foreach (DataColumn col in sourceDataset.Tables[0].Columns)
                                if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                    fdr[col.ColumnName] = dr[col];

                            var sdoGeom = fdr[GeometryColumn] as SdoGeometry;
                            if (sdoGeom != null)
                                fdr.Geometry = sdoGeom.AsGeometry();
                            return fdr;
                        }

                        return null;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public override Envelope GetExtents()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "SELECT SDO_AGGR_MBR(g." + GeometryColumn + ") FROM " + Table + " g ";
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " WHERE " + DefinitionQuery;
                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    var result = command.ExecuteScalar();
                    conn.Close();
                    if (result == null || result == DBNull.Value || !(result is SdoGeometry))
                        return null;

                    var geom = (result as SdoGeometry).AsGeometry();
                    return geom.EnvelopeInternal;
                }
            }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public override void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                var strBbox = GetBoxFilterStr(bbox);

                var strSql = "SELECT * ";
                strSql += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var adapter = new OracleDataAdapter(strSql, conn))
                {
                    conn.Open();
                    var ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                    fdr[col.ColumnName] = dr[col];

                            var sdoGeom = dr[GeometryColumn] as SdoGeometry;
                            if (sdoGeom != null)
                                fdr.Geometry = sdoGeom.AsGeometry();

                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns the box filter string needed in SQL query
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        protected string GetBoxFilterStr(Envelope bbox)
        {
            string strBbox = "SDO_FILTER(g." + GeometryColumn + ", mdsys.sdo_geometry(2003,#SRID#,NULL," +
                             "mdsys.sdo_elem_info_array(1,1003,3)," +
                             "mdsys.sdo_ordinate_array(" +
                             bbox.MinX.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.MinY.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.MaxX.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.MaxY.ToString(Map.NumberFormatEnUs) + ")), " +
                             "'querytype=window') = 'TRUE'";

            strBbox = strBbox.Replace("#SRID#", SRID > 0 ? SRID.ToString(Map.NumberFormatEnUs) : "NULL");
            return strBbox;
        }

        /// <summary>
        /// Queries the Oracle database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSql = "select COLUMN_NAME from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + Table + "'";
            using (var conn = new OracleConnection(ConnectionString))
            using (var command = new OracleCommand(strSql, conn))
            {
                conn.Open();
                object columnname = command.ExecuteScalar();
                conn.Close();
                if (columnname == DBNull.Value)
                    throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                return (string)columnname;
            }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        [Obsolete("Use ExecuteIntersectionQuery(box) instead")]
        public void GetFeaturesInView(Envelope bbox, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }
    }

    /// <summary>
    /// Oracle Spatial Data Provider
    /// Uses ODP.NET, ODAC and UserDefinedTypes to access OracleSpatial data to get enhanced performance compared to Managed Data Access
    /// 
    /// Deployment need to have the same version of Oracle DataAccess and ODAC as built against for UDT's to work properly. 
    /// XCopy 
    /// Oracle.DataAccess.dll
    /// oci.dll
    /// orannzsbb11.dll
    /// oraociicus11.dll
    /// OraOps11W.dll 
    /// 
    /// from build-directoy to deployment directory to run
    /// </summary>
    /// <remarks>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    ///	string ConnStr = "Server=127.0.0.1;Port=5432;User Id=userid;Password=password;Database=myGisDb;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.Oracle(ConnStr, "myTable", "GeomColumn", "OidColumn");
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable]
    public class OracleSpatial<TOid>  : BaseProvider<TOid> where TOid:IComparable<TOid>
    {
        private string _definitionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private string _table;

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="connectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="oidColumnName">Name of column with unique identifier</param>
        public OracleSpatial(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
            : base(-2)
        {
            ConnectionString = connectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = oidColumnName;
        }

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="datasource">Datasoure</param>
        /// <param name="tablename">Tablename</param>
        /// <param name="geometryColumnName">Geometry column name</param>
        /// <param name="oidColumnName">Object ID column</param>
        public OracleSpatial(string username, string password, string datasource, string tablename, string geometryColumnName,
                      string oidColumnName)
            : this(
                "User Id=" + username + ";Password=" + password + ";Data Source=" + datasource, tablename,
                geometryColumnName, oidColumnName)
        {
        }


        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="connectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="oidColumnName">Name of column with unique identifier</param>
        public OracleSpatial(string connectionStr, string tablename, string oidColumnName)
            : this(connectionStr, tablename, "", oidColumnName)
        {
            GeometryColumn = GetGeometryColumn();
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

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<Geometry> GetGeometriesInView(Envelope bbox)
        {
            var features = new Collection<Geometry>();
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                //string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom ";
                string strSql = "SELECT g." + GeometryColumn + " as Geom ";
                strSql += " FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var geom = dr[0] as SdoGeometry;
                                if (geom != null)
                                    features.Add(geom.AsGeometry());
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
        public override Geometry GetGeometryByID(TOid oid)
        {
            Geometry geom = null;
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "SELECT g." + GeometryColumn + " as Geom FROM " + Table + " g WHERE " + ObjectIdColumn +
                                "='" + oid + "'";
                conn.Open();
                using (var command = new OracleCommand(strSql, conn))
                {
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var sdoGeom = dr[0] as SdoGeometry;
                                if (sdoGeom != null)
                                    geom = sdoGeom.AsGeometry();
                            }
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
        public override Collection<TOid> GetObjectIDsInView(Envelope bbox)
        {
            var objectlist = new Collection<TOid>();
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                string strSql = "SELECT g." + ObjectIdColumn + " ";
                strSql += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var id = (TOid)dr[0];
                                objectlist.Add(id);
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
        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strGeom = "MDSYS.SDO_GEOMETRY('" + geom.AsText() + "', #SRID#)";

                strGeom = strGeom.Replace("#SRID#", SRID > 0 ? SRID.ToString(Map.NumberFormatEnUs) : "NULL");

                strGeom = "SDO_RELATE(g." + GeometryColumn + ", " + strGeom +
                          ", 'mask=ANYINTERACT querytype=WINDOW') = 'TRUE'";

                string strSql = "SELECT * FROM " +
                                Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strGeom;

                using (var adapter = new OracleDataAdapter(strSql, conn))
                {
                    using (var sourceDataSet = new DataSet())
                    {
                        conn.Open();
                        adapter.Fill(sourceDataSet);
                        conn.Close();
                        if (sourceDataSet.Tables.Count > 0)
                        {
                            var fdt = new FeatureDataTable(sourceDataSet.Tables[0]);
                            foreach (DataColumn col in sourceDataSet.Tables[0].Columns)
                                if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                    fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                            foreach (DataRow dr in sourceDataSet.Tables[0].Rows)
                            {
                                var fdr = fdt.NewRow();
                                foreach (DataColumn col in sourceDataSet.Tables[0].Columns)
                                    if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                        fdr[col.ColumnName] = dr[col];
                                var sdoGeometry = dr[GeometryColumn] as SdoGeometry;

                                if (sdoGeometry != null)
                                {
                                    fdr.Geometry = sdoGeometry.AsGeometry();
                                }

                                fdt.AddRow(fdr);
                            }
                            ds.Tables.Add(fdt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public override int GetFeatureCount()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " WHERE " + DefinitionQuery;
                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public override FeatureDataRow GetFeature(TOid rowId)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "select * from " +
                                Table + " g WHERE " + ObjectIdColumn + "='" + rowId + "'";
                using (var adapter = new OracleDataAdapter(strSql, conn))
                {
                    var sourceDataset = new DataSet();
                    conn.Open();
                    adapter.Fill(sourceDataset);
                    conn.Close();
                    if (sourceDataset.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(sourceDataset.Tables[0]);
                        foreach (DataColumn col in sourceDataset.Tables[0].Columns)
                            if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);

                        if (sourceDataset.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = sourceDataset.Tables[0].Rows[0];
                            var fdr = fdt.NewRow();
                            foreach (DataColumn col in sourceDataset.Tables[0].Columns)
                                if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                    fdr[col.ColumnName] = dr[col];

                            var sdoGeom = fdr[GeometryColumn] as SdoGeometry;
                            if (sdoGeom != null)
                                fdr.Geometry = sdoGeom.AsGeometry();
                            return fdr;
                        }

                        return null;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public override Envelope GetExtents()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSql = "SELECT SDO_AGGR_MBR(g." + GeometryColumn + ") FROM " + Table + " g ";
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " WHERE " + DefinitionQuery;
                using (var command = new OracleCommand(strSql, conn))
                {
                    conn.Open();
                    var result = command.ExecuteScalar();
                    conn.Close();
                    if (result == null || result == DBNull.Value || !(result is SdoGeometry))
                        return null;

                    var geom = (result as SdoGeometry).AsGeometry();
                    return geom.EnvelopeInternal;
                }
            }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public override void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                var strBbox = GetBoxFilterStr(bbox);

                var strSql = "SELECT * ";
                strSql += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += strBbox;

                using (var adapter = new OracleDataAdapter(strSql, conn))
                {
                    conn.Open();
                    var ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (string.Compare(col.ColumnName, GeometryColumn, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) != 0)
                                    fdr[col.ColumnName] = dr[col];

                            var sdoGeom = dr[GeometryColumn] as SdoGeometry;
                            if (sdoGeom != null)
                                fdr.Geometry = sdoGeom.AsGeometry();

                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns the box filter string needed in SQL query
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        protected string GetBoxFilterStr(Envelope bbox)
        {
            string strBbox = "SDO_FILTER(g." + GeometryColumn + ", mdsys.sdo_geometry(2003,#SRID#,NULL," +
                             "mdsys.sdo_elem_info_array(1,1003,3)," +
                             "mdsys.sdo_ordinate_array(" +
                             bbox.MinX.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.MinY.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.MaxX.ToString(Map.NumberFormatEnUs) + ", " +
                             bbox.MaxY.ToString(Map.NumberFormatEnUs) + ")), " +
                             "'querytype=window') = 'TRUE'";

            strBbox = strBbox.Replace("#SRID#", SRID > 0 ? SRID.ToString(Map.NumberFormatEnUs) : "NULL");
            return strBbox;
        }

        /// <summary>
        /// Queries the Oracle database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSql = "select COLUMN_NAME from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + Table + "'";
            using (var conn = new OracleConnection(ConnectionString))
            using (var command = new OracleCommand(strSql, conn))
            {
                conn.Open();
                object columnname = command.ExecuteScalar();
                conn.Close();
                if (columnname == DBNull.Value)
                    throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                return (string)columnname;
            }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        [Obsolete("Use ExecuteIntersectionQuery(box) instead")]
        public void GetFeaturesInView(Envelope bbox, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }
    }

}