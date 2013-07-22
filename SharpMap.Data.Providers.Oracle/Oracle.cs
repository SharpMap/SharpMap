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
using System.Globalization;
using GeoAPI.Geometries;
using Oracle.ManagedDataAccess.Client;
using SharpMap.Converters.WellKnownBinary;

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
    public class OracleProvider : BaseProvider
    {
        private string _definitionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private string _table;

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="tablename">The name of data table</param>
        /// <param name="geometryColumnName">The name of geometry column</param>
        /// <param name="oidColumnName">The name of column with unique identifier</param>
        public OracleProvider(string connectionString, string tablename, string geometryColumnName, string oidColumnName)
            : base(-2)
        {
            ConnectionString = connectionString;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = oidColumnName;
        }

        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="datasource">The datasoure</param>
        /// <param name="tablename">The name of data table</param>
        /// <param name="geometryColumnName">The name of geometry column</param>
        /// <param name="oidColumnName">The name of column with unique identifier</param>
        public OracleProvider(string username, string password, string datasource, string tablename, string geometryColumnName,
                              string oidColumnName)
            : this(
                "User Id=" + username + ";Password=" + password + ";Data Source=" + datasource, tablename,
                geometryColumnName, oidColumnName)
        {
        }


        /// <summary>
        /// Initializes a new connection to Oracle
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="tablename">The name of data table</param>
        /// <param name="oidColumnName">The name of column with unique identifier</param>
        public OracleProvider(string connectionString, string tablename, string oidColumnName)
            : this(connectionString, tablename, string.Empty, oidColumnName)
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
        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var features = new Collection<IGeometry>();
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                //string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom ";
                string strSQL = "SELECT g." + GeometryColumn + ".Get_WKB() ";
                strSQL += " FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                using (var command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!dr.IsDBNull(0))
                            {
                                var geom = GeometryFromWKB.Parse((byte[]) dr[0], Factory);
                                if (geom != null) features.Add(geom);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return features;
        }

        /// <summary>
        /// Returns the IGeometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>IGeometry</returns>
        public override IGeometry GetGeometryByID(uint oid)
        {
            IGeometry geom = null;
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSQL = "SELECT g." + GeometryColumn + ".Get_WKB() FROM " + Table + " g WHERE " + ObjectIdColumn +
                                "='" + oid.ToString(NumberFormatInfo.InvariantInfo) + "'";
                conn.Open();
                using (var command = new OracleCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!dr.IsDBNull(0))
                            {
                                geom = GeometryFromWKB.Parse((byte[]) dr[0], Factory);
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

                string strSQL = "SELECT g." + ObjectIdColumn + " ";
                strSQL += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                using (var command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr.IsDBNull(0)) continue;

                            var id = (uint) (decimal) dr[0];
                            objectlist.Add(id);
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
        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                var strGeom = "MDSYS.SDO_GEOMETRY('" + geom.AsText() + "', #SRID#)";

                strGeom = strGeom.Replace("#SRID#", SRID > 0 ? SRID.ToString(Map.NumberFormatEnUs) : "NULL");

                strGeom = "SDO_RELATE(g." + GeometryColumn + ", " + strGeom +
                          ", 'mask=ANYINTERACT querytype=WINDOW') = 'TRUE'";

                var strSQL = "SELECT g.* , g." + GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry FROM " +
                             Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strGeom;

                using (var adapter = new OracleDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count <= 0)
                    {
                        return;
                    }
                    
                    var fdt = new FeatureDataTable(ds.Tables[0]);
                    foreach (DataColumn col in ds.Tables[0].Columns)
                    {
                        if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                        {
                            fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        }
                    }
                    
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var fdr = fdt.NewRow();
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdr[col.ColumnName] = dr[col];
                        fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"], Factory);
                        fdt.AddRow(fdr);
                    }
                    ds.Tables.Add(fdt);
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
                string strSQL = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (var command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    return (int) command.ExecuteScalar();
                }
            }
        }

        ///// <summary>
        ///// Spacial Reference ID
        ///// </summary>
        //public int SRID
        //{
        //    get
        //    {
        //        if (_srid == -2)
        //        {
        //            string strSQL = "select SRID from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + Table + "'";

        //            using (OracleConnection conn = new OracleConnection(ConnectionString))
        //            {
        //                using (OracleCommand command = new OracleCommand(strSQL, conn))
        //                {
        //                    try
        //                    {
        //                        conn.Open();
        //                        _srid = (int) (decimal) command.ExecuteScalar();
        //                        conn.Close();
        //                    }
        //                    catch
        //                    {
        //                        _srid = -1;
        //                    }
        //                }
        //            }
        //        }
        //        return _srid;
        //    }
        //    set { throw (new ApplicationException("Spatial Reference ID cannot by set on a Oracle table")); }
        //}


        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public override FeatureDataRow GetFeature(uint rowId)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSQL = "select g.* , g." + GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry from " +
                                Table + " g WHERE " + ObjectIdColumn + "='" +
                                rowId.ToString(NumberFormatInfo.InvariantInfo) + "'";
                using (var adapter = new OracleDataAdapter(strSQL, conn))
                {
                    var ds = new FeatureDataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            var fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"], Factory);
                            return fdr;
                        }
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Envelope of dataset
        /// </summary>
        /// <returns>Envelope</returns>
        public override Envelope GetExtents()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                string strSQL = "SELECT SDO_AGGR_MBR(g." + GeometryColumn + ").Get_WKT() FROM " + Table + " g ";
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (var command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    var result = command.ExecuteScalar();
                    conn.Close();
                    if (result == DBNull.Value)
                        return null;
                    var strBox = (string) result;
                    if (strBox.StartsWith("POLYGON", StringComparison.InvariantCultureIgnoreCase))
                    {
                        strBox = strBox.Replace("POLYGON", "");
                        strBox = strBox.Trim();
                        strBox = strBox.Replace("(", "");
                        strBox = strBox.Replace(")", "");

                        var xX = new List<double>();
                        var yY = new List<double>();

                        String[] points = strBox.Split(',');

                        foreach (string s in points)
                        {
                            var point = s.Trim();
                            var nums = point.Split(' ');
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

                        return new Envelope(minX, maxX, minY, maxY);
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
        public override void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                //Get bounding box string
                var strBbox = GetBoxFilterStr(bbox);

                var strSQL = "SELECT g.*, g." + GeometryColumn + ".Get_WKB() AS sharpmap_tempgeometry ";
                strSQL += "FROM " + Table + " g WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                using (var adapter = new OracleDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    var ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"], Factory);
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

        ///// <summary>
        ///// Convert WellKnownText to linestrings
        ///// </summary>
        ///// <param name="WKT"></param>
        ///// <returns></returns>
        //private LineString WktToLineString(string WKT)
        //{
        //    LineString line = new LineString();
        //    WKT = WKT.Substring(WKT.LastIndexOf('(') + 1).Split(')')[0];
        //    string[] strPoints = WKT.Split(',');
        //    foreach (string strPoint in strPoints)
        //    {
        //        string[] coord = strPoint.Split(' ');
        //        line.Vertices.Add(new Point(double.Parse(coord[0], Map.NumberFormatEnUs),
        //                                    double.Parse(coord[1], Map.NumberFormatEnUs)));
        //    }
        //    return line;
        //}

        /// <summary>
        /// Queries the Oracle database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSQL = "select COLUMN_NAME from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + Table + "'";
            using (var conn = new OracleConnection(ConnectionString))
            using (var command = new OracleCommand(strSQL, conn))
            {
                conn.Open();
                object columnname = command.ExecuteScalar();
                conn.Close();
                if (columnname == DBNull.Value)
                    throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                return (string) columnname;
            }
        }
    }
}
