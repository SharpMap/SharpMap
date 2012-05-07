// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

// PostGIS references:
// PostGIS functions: http://www.01map.com/download/guide_utilisateur/node73.htm
// PostGIS manual: http://sun.calstatela.edu/~cysun/documentation/postgres/8/postgis/postgis.html
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using Npgsql;
using SharpMap.Converters.WellKnownBinary;
using BoundingBox = GeoAPI.Geometries.Envelope;
using Geometry = GeoAPI.Geometries.IGeometry;
using LineString = GeoAPI.Geometries.ILineString;
using System.Globalization;
#if DEBUG

#endif

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// PostGreSQL / PostGIS dataprovider
    /// Uses NPGSQL for communicating with database.
    /// Detect PostGIS version and uses ST_Intersects method for PG_Verions >= 1.3 and && (bbox comparison) for others)
    /// </summary>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    ///	string ConnStr = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=password;Database=myGisDb;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.PostGIS(ConnStr, "myTable");
    /// </code>
    /// </example>
    [Serializable]
    public class PostGIS : BaseProvider
    {
        private string _defintionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private string _schema = "public";
        private string _table;
        private readonly double _postGisVersion;
        private readonly bool _supportSTIntersects;

        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="connectionString">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="objectIdColumnName">Name of column with unique identifier</param>
        public PostGIS(string connectionString, string tablename, string geometryColumnName, string objectIdColumnName)
        {
            ConnectionString = connectionString;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            if (!string.IsNullOrEmpty(geometryColumnName))
                SRID = GetGeometrySrid();
            ObjectIdColumn = objectIdColumnName;
            _postGisVersion = GetPostGISVersion();
            _supportSTIntersects = _postGisVersion >= 1.3;
        }

        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="connectionString">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="objectIdColumnName">Name of column with unique identifier</param>
        public PostGIS(string connectionString, string tablename, string objectIdColumnName)
            : this(connectionString, tablename, "", objectIdColumnName)
        {
            int srid;
            GeometryColumn = GetGeometryColumn(out srid);
            SRID = srid;
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
            set
            {
                _table = value;
                QualifyTable();
            }
        }

        /// <summary>
        /// Schema Name
        /// </summary>
        public string Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }

        /// <summary>
        /// Qualified Table Name
        /// </summary>
        public string QualifiedTable
        {
            get { return string.Format("\"{0}\".\"{1}\"", _schema, _table); }
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

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            var features = new Collection<Geometry>();
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.MinX.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.MinY.ToString(Map.NumberFormatEnUs) + "," +
                                 bbox.MaxX.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.MaxY.ToString(Map.NumberFormatEnUs) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(Map.NumberFormatEnUs) + ")";

                string strSQL = "SELECT AsBinary(\"" + GeometryColumn + "\") AS Geom ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                if (_supportSTIntersects)
                {
                    strSQL += "ST_Intersects(\"" + GeometryColumn + "\"," + strBbox + ")";
                }
                else
                {
                    strSQL += "\"" + GeometryColumn + "\" && " + strBbox;
                }

#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetGeometriesInView: executing sql:", strSQL));
#endif
                using (var command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] == DBNull.Value) 
                                continue;
                            
                            var geom = GeometryFromWKB.Parse((byte[]) dr[0], Factory);
                            if (geom != null)
                                features.Add(geom);
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
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                var strSQL = "SELECT AsBinary(\"" + GeometryColumn + "\") AS Geom FROM " + QualifiedTable +
                             " WHERE \"" + ObjectIdColumn + "\"='" + oid + "'";
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetGeometryByID: executing sql:", strSQL));
#endif
                conn.Open();
                using (var command = new NpgsqlCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
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
        public override Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.MinX.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.MinY.ToString(Map.NumberFormatEnUs) + "," +
                                 bbox.MaxX.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.MaxY.ToString(Map.NumberFormatEnUs) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(Map.NumberFormatEnUs) + ")";

                var strSQL = "SELECT \"" + ObjectIdColumn + "\" ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strBbox;
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetObjectIDsInView: executing sql:", strSQL));
#endif

                using (var command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] == DBNull.Value) 
                                continue;
                            var id = (uint) (int) dr[0];
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
        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                string strGeom = "GeomFromText('" + geom.AsText() + "')";
                if (SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + SRID + ")";

                string strSQL = "SELECT * , AsBinary(\"" + GeometryColumn + "\") As sharpmap_tempgeometry FROM " +
                                QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strGeom + " AND distance(\"" + GeometryColumn + "\", " +
                          strGeom + ")<0";

#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "ExecuteIntersectionQuery: executing sql:", strSQL));
#endif

                using (var adapter = new NpgsqlDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
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
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public override int GetFeatureCount()
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + QualifiedTable;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (var command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
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
        //            string strSQL = "select srid from geometry_columns WHERE f_table_schema='" + _schema +
        //                            "' AND f_table_name='" + _table + "'";

        //            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
        //            {
        //                using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
        //                {
        //                    try
        //                    {
        //                        conn.Open();
        //                        _srid = (int) command.ExecuteScalar();
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
        //    set { throw (new ApplicationException("Spatial Reference ID cannot by set on a PostGIS table")); }
        //}


        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public override FeatureDataRow GetFeature(uint rowId)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                var strSQL = "select * , AsBinary(\"" + GeometryColumn + "\") As sharpmap_tempgeometry from " +
                                QualifiedTable + " WHERE \"" + ObjectIdColumn + "\"='" + rowId + "'";
                using (var adapter = new NpgsqlDataAdapter(strSQL, conn))
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
                            FeatureDataRow fdr = fdt.NewRow();
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
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public override BoundingBox GetExtents()
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                string strSQL = "SELECT EXTENT(\"" + GeometryColumn + "\") FROM " + QualifiedTable;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (var command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    object result = command.ExecuteScalar();
                    conn.Close();
                    if (result == DBNull.Value)
                        return null;
                    string strBox = (string) result;
                    if (strBox.StartsWith("BOX("))
                    {
                        var vals = strBox.Substring(4, strBox.IndexOf(")", StringComparison.InvariantCultureIgnoreCase) - 4).Split(new [] {',', ' '});
                        return new BoundingBox(
                            double.Parse(vals[0], Map.NumberFormatEnUs),
                            double.Parse(vals[2], Map.NumberFormatEnUs),
                            double.Parse(vals[1], Map.NumberFormatEnUs),
                            double.Parse(vals[3], Map.NumberFormatEnUs));
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
            //List<Geometry> features = new List<Geometry>();
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                var strBbox = "box2d('BOX3D(" +
                                 bbox.MinX.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.MinY.ToString(Map.NumberFormatEnUs) + "," +
                                 bbox.MaxX.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.MaxY.ToString(Map.NumberFormatEnUs) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(Map.NumberFormatEnUs) + ")";

                string strSQL = "SELECT *, AsBinary(\"" + GeometryColumn + "\") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                if (_supportSTIntersects)
                {
                    strSQL += "ST_Intersects(\"" + GeometryColumn + "\"," + strBbox + ")";
                }
                else
                {
                    strSQL += "\"" + GeometryColumn + "\" && " + strBbox;
                }
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}\n", "ExecuteIntersectionQuery: executing sql:", strSQL));
#endif
                using (var adapter = new NpgsqlDataAdapter(strSQL, conn))
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

        #endregion

        private void QualifyTable()
        {
            var dotPos = _table.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);
            if (dotPos == -1)
            {
                _schema = "public";
            }
            else
            {
                _schema = _table.Substring(0, dotPos);
                _schema = _schema.Replace('"', ' ').Trim();
            }
            _table = _table.Substring(dotPos + 1);
            _table = _table.Replace('"', ' ').Trim();
        }

        /// <summary>
        /// Queries the PostGIS database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn(out int srid)
        {
            string strSQL = "SELECT \"f_geometry_column\", \"srid\" from public.\"geometry_columns\" WHERE \"f_table_schema\"='" + _schema +
                            "' and \"f_table_name\"='" + _table + "'";
            
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr != null && dr.Read())
                        {
                            srid = dr.GetInt32(1);
                            return dr.GetString(0);
                        }
                    }
                }
            }
            throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
        }

        /// <summary>
        /// Queries the PostGIS database to get the srid of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private int GetGeometrySrid()
        {
            var strSQL = "SELECT \"srid\" FROM public.\"geometry_columns\" WHERE \"f_table_schema\"='" + _schema +
                            "' and \"f_table_name\"='" + _table + "' AND \"f_geometry_column\"='"+ GeometryColumn +"';";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr != null && dr.Read())
                            return dr.GetInt32(0);
                    }
                }
            }
            throw new ApplicationException("Table '" + Table + "' does not contain a geometry column named '"+ GeometryColumn + "')");
        }

        /// <summary>
        /// Reads the postgis version installed on the server
        /// </summary>
        /// <returns></returns>
        private double GetPostGISVersion()
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand("SELECT postgis_version();", conn))
                {
                    var version = command.ExecuteScalar();
                    conn.Close();
                    if (version == DBNull.Value)
                        return 0;
                    var vPart = version.ToString();
                    if (vPart.Contains(" "))
                        vPart = vPart.Split(' ')[0];
                    return Convert.ToDouble(vPart, CultureInfo.InvariantCulture);
                }
            }
        }
    }
}