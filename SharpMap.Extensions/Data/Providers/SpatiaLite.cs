// Copyright 2009 - William Dollins
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
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Text;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Defines possible spatial indices
    /// </summary>
    public enum SpatiaLiteIndex
    {
        /// <summary>
        /// No spatial index defined
        /// </summary>
        None = 0,
        /// <summary>
        /// RTree
        /// </summary>
        RTree = 1,
        /// <summary>
        /// Cache of minimum bounding rectangles (MBR)
        /// </summary>
        MbrCache = 2
    }

    /// <summary>
    /// SpatiaLite Provider for SharpMap
    /// <para>
    /// Spatialite is a spatial extension for the popular SQLite database engine.
    /// </para>
    /// <para>
    /// In order to use this provider with SharpMap, you need to 
    /// <list type="bullet">
    /// <item>get your copy of the native spatialite binaries from http://www.gaia-gis.it/spatialite/,</item>
    /// <item>copy them all in <strong>one</strong> directory,</item>
    /// <item>add a (or modify your) app.config- or web.config-file with key/value pairs SpatiaLitePath and SpatiaLiteNativeDll</item>
    /// </list>
    /// </para>
    /// </summary>
    public class SpatiaLite : IProvider
    {
        private string _connectionString;
        private string _defintionQuery;
        private string _geometryColumn;
        private bool _isOpen;
        private string _objectIdColumn;
        //private string _spatialIndex;
        private readonly string _spatiaLiteIndexClause;
        private readonly SpatiaLiteIndex _spatiaLiteIndex;
        private readonly int _srid = -2;
        private string _table;

        private Boolean _useSpatialIndex = false;

        private static readonly string SpatiaLitePath;
        private static readonly string SpatiaLiteNativeDll = "libspatialite-2.dll";

        /// <summary>
        /// Initializes Provider with information about where to find native spatialite library and how
        /// it is named.
        /// </summary>
        static SpatiaLite()
        {
            AppSettingsReader asr = new AppSettingsReader();
            try
            {
                String slBin = (String)asr.GetValue("SpatiaLiteNativeDll", typeof(String));
                if (!String.IsNullOrEmpty(slBin))
                    SpatiaLiteNativeDll = slBin;
            }
            catch
            {
                System.Diagnostics.Trace.WriteLine("Path to native SpatiaLite binaries not configured, assuming they are in applications directory");
            }

            try
            {
                String slPath = (String)asr.GetValue("SpatiaLitePath", typeof (String));
                SpatiaLitePath = slPath;
                String path = Environment.GetEnvironmentVariable("path");
                if (path == null) path = "";
                if (!path.ToLowerInvariant().Contains(slPath.ToLowerInvariant()))
                    Environment.SetEnvironmentVariable("path", slPath + ";" + path);
            }
            catch
            {
                SpatiaLitePath = "";
            }

            if (!System.IO.File.Exists(System.IO.Path.Combine(SpatiaLitePath, SpatiaLiteNativeDll)))
                throw new System.IO.FileNotFoundException("SpatiaLite binaries not found under given path and filename");
        }

        /// <summary>
        /// Function to provide an SqLiteConnection with SpatiaLite extension loaded.
        /// </summary>
        /// <param name="connectionString">Connection string to connect to SQLite database file.</param>
        /// <returns>Opened <see cref="SQLiteConnection"/></returns>
        private static SQLiteConnection SpatiaLiteConnection(String connectionString)
        {
            try
            {
                SQLiteConnection cn = new SQLiteConnection(connectionString);
                cn.Open();
                SQLiteCommand cm = new SQLiteCommand(String.Format("SELECT load_extension('{0}');", SpatiaLiteNativeDll), cn);
                cm.ExecuteNonQuery();
                //if ((Int32)cm.ExecuteScalar() != 0)
                //    throw new Exception();
                return cn;
            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Creates an instance of SpatiaLite provider
        /// </summary>
        /// <param name="connectionStr">Connection String to SQLite database file 
        /// ("http://www.connectionstrings.com/sqlite")</param>
        /// <param name="tablename">Name of the table with geometry information</param>
        /// <param name="geometryColumnName">Name of the desired geometry column</param>
        /// <param name="oidColumnName">Name of the object Id column</param>
        public SpatiaLite(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
        {
            ConnectionString = connectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName; //Name of column to store geometry
            ObjectIdColumn = oidColumnName; //Name of object ID column

            try
            {

                using (SQLiteConnection cn = new SQLiteConnection(connectionStr))
                {
                    cn.Open();
                    SQLiteCommand cm = new SQLiteCommand(
                        String.Format(
                            "SELECT srid, spatial_index_enabled FROM geometry_columns WHERE(f_table_name='{0}' AND f_geometry_column='{1}');",
                            tablename, geometryColumnName), cn);
                    SQLiteDataReader dr = cm.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        _srid = dr.GetInt32(0);

                        switch (dr.GetInt32(1))
                        {
                            case 1: //RTree
                                String indexName = string.Format(@"idx_{0}_{1}", tablename, geometryColumnName);
                                String whereClause = @"xmin < {0} AND xmax > {1} AND ymin < {2} AND ymax > {3}";
                                _spatiaLiteIndexClause = string.Format(@"ROWID IN (SELECT pkid FROM {0} WHERE {1})", indexName, whereClause);
                                _spatiaLiteIndex = SpatiaLiteIndex.RTree;
                                _useSpatialIndex = true;
                                break;
                            case 2: //MBRCache
                                indexName = string.Format(@"cache_{0}_{1}", tablename, geometryColumnName);
                                whereClause = "mbr=FilterMbrIntersects({1}, {3}, {0}, {2})";
                                _spatiaLiteIndexClause = string.Format(@"ROWID IN (SELECT ROWID FROM {0} WHERE {1})", indexName, whereClause);
                                _spatiaLiteIndex = SpatiaLiteIndex.MbrCache;
                                _useSpatialIndex = true;
                                break;
                        }
                    }
                    dr.Close();
                }
            }
            catch (Exception)
            {
                _srid = -1;
                _spatiaLiteIndex = SpatiaLiteIndex.None;
                _useSpatialIndex = false;
            }
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

        public bool UseSpatiaLiteIndex
        {
            get { return _useSpatialIndex && _spatiaLiteIndex != SpatiaLiteIndex.None; }
            set 
            {
                if (_spatiaLiteIndex != SpatiaLiteIndex.None)
                    _useSpatialIndex = value;
            }
        }

        /// <summary>
        /// Name of the spatial index
        /// </summary>
        public string SpatialIndex
        {
            get
            {
                return _spatiaLiteIndex.ToString();
            }
            set 
            { 
                System.Diagnostics.Debug.WriteLine("SpatialIndex is obsolete, use UseSpatiaLiteIndex property");
                //_spatialIndex = value;
            }
        }

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set
            {
                if (value != _defintionQuery)
                {
                    _cachedExtents = null;
                    _defintionQuery = value;
                }
            }
        }

        #region IProvider Members

        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            Collection<Geometry> features = new Collection<Geometry>();
            using (SQLiteConnection conn = SpatiaLiteConnection(_connectionString))
            {
                string boxIntersect = GetBoxClause(bbox);

                string strSQL = "SELECT AsBinary(" + GeometryColumn + ") AS Geom ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += boxIntersect;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader())
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

        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (SQLiteConnection conn = SpatiaLiteConnection(_connectionString))
            {
                string strSQL = "SELECT " + ObjectIdColumn + " ";
                strSQL += "FROM " + Table + " WHERE ";

                strSQL += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + DefinitionQuery + " AND ";

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint id = Convert.ToUInt32(dr[0]);
                                objectlist.Add(id);
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
            using (SQLiteConnection conn = SpatiaLiteConnection(_connectionString))
            {
                string strSQL = "SELECT AsBinary(" + GeometryColumn + ") AS Geom FROM " + Table + " WHERE " +
                                ObjectIdColumn + "='" + oid + "'";
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                geom = GeometryFromWKB.Parse((byte[]) dr[0]);
                            }
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            using (SQLiteConnection conn = SpatiaLiteConnection(_connectionString))
            {
                string strSQL = "SELECT *, AsBinary(" + GeometryColumn + ") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += GetOverlapsClause(geom);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
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
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            using (SQLiteConnection conn = SpatiaLiteConnection(_connectionString))
            {
                string strSQL = "SELECT *, AsBinary(" + GeometryColumn + ") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + Table + " WHERE ";
                strSQL += GetBoxClause(box);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + DefinitionQuery;

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
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
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        public int GetFeatureCount()
        {
            int count;
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                string strSQL = "SELECT COUNT(*) as numrecs FROM " + Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    SQLiteDataReader dtr = command.ExecuteReader();
                    if (dtr["numrecs"] != null)
                    {
                        count = Convert.ToInt32(dtr["numrecs"]); // (int)command.ExecuteScalar();
                    }
                    else
                    {
                        count = -1;
                    }
                    conn.Close();
                }
            }
            return count;
        }

        public FeatureDataRow GetFeature(uint rowId)
        {
            using (SQLiteConnection conn = SpatiaLiteConnection(_connectionString))
            {
                string strSQL = "SELECT *, AsBinary(" + GeometryColumn + ") AS sharpmap_tempgeometry FROM " + Table +
                                " WHERE " + ObjectIdColumn + "='" + rowId + "'";
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
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
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            return fdr;
                        }
                        return null;
                    }
                    return null;
                }
            }
        }

        private BoundingBox _cachedExtents = null;

        public BoundingBox GetExtents()
        {
            if (_cachedExtents != null)
                return _cachedExtents;

            BoundingBox box = null;
            using (SQLiteConnection conn = SpatiaLiteConnection(_connectionString))
            {
                //string strSQL = "SELECT Min(minx) AS MinX, Min(miny) AS MinY, Max(maxx) AS MaxX, Max(maxy) AS MaxY FROM " + this.Table;
                string strSQL;
                if (_spatiaLiteIndex == SpatiaLiteIndex.RTree && String.IsNullOrEmpty(_defintionQuery))
                {
                    strSQL = string.Format(
                        "SELECT MIN(xmin) AS minx, MAX(xmax) AS maxx, MIN(ymin) AS miny, MAX(ymax) AS maxy from {0};",
                        string.Format("idx_{0}_{1}", _table, _geometryColumn));
                }
                else
                {
                    strSQL = string.Format(
                        "SELECT MIN(MbrMinX({0})) AS minx, MIN(MbrMinY({0})) AS miny, MAX(MbrMaxX({0})) AS maxx, MAX(MbrMaxY({0})) AS maxy FROM {1};",
                        _geometryColumn, FromClause(_table));
                }

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader())
                        if (dr.Read())
                        {
                            box = new BoundingBox(
                                (double) dr["minx"], (double) dr["miny"], 
                                (double) dr["maxx"], (double) dr["maxy"]);
                        }
                    conn.Close();
                }
                _cachedExtents = box;
                return box;
            }
        }

        private string FromClause(String table)
        {
            if (String.IsNullOrEmpty(DefinitionQuery))
                return table;

            return string.Format("{0} WHERE ({1})", table, _defintionQuery);
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
            set
            {
                System.Diagnostics.Debug.WriteLine("SRID property is read from geometry_columns");
            }
        }

        public void Dispose()
        {
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
            if (UseSpatiaLiteIndex)
            {
                return string.Format(Map.NumberFormatEnUs, _spatiaLiteIndexClause, bbox.Max.X, bbox.Min.X, bbox.Max.Y, bbox.Min.Y);
                /*
                StringBuilder sql = new StringBuilder("ROWID IN ( ");
                sql.Append("SELECT pkid FROM ");
                sql.Append(SpatialIndex);
                sql.Append(" WHERE ");
                sql.AppendFormat(Map.NumberFormatEnUs,
                                 "xmin < {0} AND xmax > {1} AND ymin < {2} AND ymax > {3} )",
                                 bbox.Max.X, bbox.Min.X, bbox.Max.Y, bbox.Min.Y);

                return sql.ToString();
                */
            }

            string wkt = GeometryToWKT.Write(LineFromBbox(bbox));
            return "MBRIntersects(GeomFromText('" + wkt + "')," + _geometryColumn + ")=1";
        }

        private static IGeometry LineFromBbox(BoundingBox bbox)
        {
            Collection<Point> pointColl = new Collection<Point> {bbox.Min, bbox.Max};
            return new LineString(pointColl);
        }

        public string GetOverlapsClause(Geometry geom)
        {
            string wkt = GeometryToWKT.Write(geom);
            string retval = "Intersects(GeomFromText('" + wkt + "')," + _geometryColumn + ")=1";
            return retval;
        }
    }
}