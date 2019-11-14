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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
//using System.Diagnostics;
using System.Data.SQLite;
using System.Globalization;
using Common.Logging;
using GeoAPI.Geometries;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// SpatiaLite Provider for SharpMap
    /// <para>
    /// Spatialite is a spatial extension for the popular SQLite database engine.
    /// </para>
    /// <para>
    /// In order to use this provider with SharpMap, you need to 
    /// <list type="bullet">
    /// <item>get your copy of the native spatialite binaries from http://www.gaia-gis.it/spatialite/ ,</item>
    /// <item>copy them all in <strong>one</strong> directory,</item>
    /// <item>add a (or modify your) app.config- or web.config-file with key/value pairs SpatiaLitePath and SpatiaLiteNativeDll</item>
    /// </list>
    /// </para>
    /// </summary>
    public class SpatiaLite : BaseProvider
    {
        static ILog logger = LogManager.GetLogger(typeof(SpatiaLite));

        private string _definitionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private readonly string _spatiaLiteIndexClause;
        private readonly SpatiaLiteIndex _spatiaLiteIndex;
        private string _table;

        private int _numOrdinateDimensions = 2;

        private Boolean _useSpatialIndex;

        private static readonly string SpatiaLitePath;
        private static readonly string SpatiaLiteNativeDll = "libspatialite-2.dll";

        ///// <summary>
        ///// Initializes Provider with information about where to find native spatialite library and how
        ///// it is named.
        ///// </summary>
        //static SpatiaLite()
        //{
        //    try
        //    {
        //        var slBin = ConfigurationManager.AppSettings["SpatiaLiteNativeDll"];
        //        if (!String.IsNullOrEmpty(slBin))
        //        {
        //            SpatiaLiteNativeDll = slBin;
        //        }
        //        else
        //        {
        //            logger.Warn("Path to native SpatiaLite binaries not configured, assuming they are in applications directory");
        //        }


        //        var slPath = ConfigurationManager.AppSettings["SpatiaLitePath"];
        //        if (slPath == null)
        //            slPath = "";
        //        SpatiaLitePath = slPath;
        //        if (!string.IsNullOrEmpty(slPath))
        //        {
        //            var path = Environment.GetEnvironmentVariable("path") ?? "";
        //            if (!path.ToLowerInvariant().Contains(slPath.ToLowerInvariant()))
        //                Environment.SetEnvironmentVariable("path", slPath + ";" + path);
        //        }
        //    }
        //    catch (Exception ee)
        //    {
        //        SpatiaLitePath = "";
        //        logger.Error("Error ininitializing SpatialLite", ee);
        //    }

        //    if (!System.IO.File.Exists(System.IO.Path.Combine(SpatiaLitePath, SpatiaLiteNativeDll)))
        //        throw new System.IO.FileNotFoundException("SpatiaLite binaries not found under given path and filename");
        //}

        /// <summary>
        /// Gets or sets whether geometry definition lookup should use sql LIKE operator for name comparison.
        /// </summary>
        public static bool UseLike { get; set; }

        /// <summary>
        /// Function to provide an SqLiteConnection with SpatiaLite extension loaded.
        /// </summary>
        /// <param name="connectionString">Connection string to connect to SQLite database file.</param>
        /// <returns>Opened <see cref="SQLiteConnection"/></returns>
        private static SQLiteConnection SpatiaLiteConnection(String connectionString)
        {
            try
            {
                var cn = new SQLiteConnection(connectionString);
                cn.Open();
                var cm = new SQLiteCommand(String.Format("SELECT load_extension('{0}');", SpatiaLiteNativeDll), cn);
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
            :base(-2)
        {
            ConnectionString = connectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName; //Name of column to store geometry
            ObjectIdColumn = oidColumnName; //Name of object ID column

            try
            {
                var op = UseLike ? "LIKE" : "=";
                using (var cn = new SQLiteConnection(connectionStr))
                {
                    cn.Open();
                    var cm = new SQLiteCommand(
                        String.Format(
                            "SELECT \"srid\", \"coord_dimension\", \"spatial_index_enabled\" FROM \"geometry_columns\" WHERE(\"f_table_name\" {2} '{0}' AND \"f_geometry_column\" {2} '{1}');",
                            tablename, geometryColumnName, op), cn);
                    var dr = cm.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        SRID = dr.GetInt32(0);

                        var coordDim = dr.GetFieldType(1) == typeof(long) 
                            ? dr.GetInt64(1).ToString(NumberFormatInfo.InvariantInfo) 
                            : dr.GetString(1);
                        
                        switch (coordDim)
                        {
                            case "2":
                            case "XY":
                                _numOrdinateDimensions = 2;
                                break;
                            case "3":
                            case "XYZ":
                            case "XYM":
                                _numOrdinateDimensions = 3;
                                break;
                            case "4":
                            case "XYZM":
                                _numOrdinateDimensions = 4;
                                break;
                            default:
                                throw new Exception("Cannot evaluate number of ordinate dimensions");

                        }

                        switch (dr.GetInt32(2))
                        {
                            case 1: //RTree
                                var indexName = string.Format(@"idx_{0}_{1}", tablename, geometryColumnName);
                                var whereClause = @"xmin < {0} AND xmax > {1} AND ymin < {2} AND ymax > {3}";
                                _spatiaLiteIndexClause = string.Format(@"ROWID IN (SELECT pkid FROM {0} WHERE {1})", indexName, whereClause);
                                _spatiaLiteIndex = SpatiaLiteIndex.RTree;
                                _useSpatialIndex = true;
                                break;
                            case 2: //MBRCache
                                indexName = string.Format(@"cache_{0}_{1}", tablename, geometryColumnName);
                                whereClause = "mbr=FilterMbrIntersects({1}, {3}, {0}, {2})";
                                _spatiaLiteIndexClause = string.Format(@"ROWID IN (SELECT ROWID FROM {0} WHERE {1})", indexName, whereClause);
#pragma warning disable 612,618
                                _spatiaLiteIndex = SpatiaLiteIndex.MbrCache;
#pragma warning restore 612,618
                                _useSpatialIndex = true;
                                break;
                        }
                    }
                    dr.Close();
                }
            }
            catch (Exception)
            {
                SRID = -1;
                _spatiaLiteIndex = SpatiaLiteIndex.None;
                _useSpatialIndex = false;
            }
        }

        /// <summary>
        /// Gets or sets the extent for this data source
        /// </summary>
        public Envelope CachedExtent 
        {
            get
            {
                return _cachedExtents ?? (_cachedExtents = GetExtents());
            }
            set
            {
                _cachedExtents = value;
            }
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
            get { return _definitionQuery; }
            set
            {
                _definitionQuery = value;
            }
        }

        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var features = new Collection<IGeometry>();
            using (var conn = SpatiaLiteConnection(ConnectionString))
            {
                var boxIntersect = GetBoxClause(bbox);

                var strSql = "SELECT AsBinary(" + GeometryColumn + ") AS Geom ";
                strSql += "FROM " + Table + " WHERE ";
                strSql += boxIntersect;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " AND " + DefinitionQuery;

                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
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

        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = SpatiaLiteConnection(ConnectionString))
            {
                string strSql = "SELECT " + ObjectIdColumn + " ";
                strSql += "FROM " + Table + " WHERE ";

                strSql += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " AND " + DefinitionQuery + " AND ";

                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] == DBNull.Value) 
                                continue;
                            var id = Convert.ToUInt32(dr[0]);
                            objectlist.Add(id);
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        public override IGeometry GetGeometryByID(uint oid)
        {
            IGeometry geom = null;
            using (var conn = SpatiaLiteConnection(ConnectionString))
            {
                string strSql = "SELECT AsBinary(" + GeometryColumn + ") AS Geom FROM " + Table + " WHERE " +
                                ObjectIdColumn + "='" + oid + "'";
                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
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

        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            using (var conn = SpatiaLiteConnection(ConnectionString))
            {
                string cols = "*";
                //If using rowid as oid, we need to explicitly request it!
                if (string.Compare(ObjectIdColumn, "rowid", true) == 0)
                    cols = "rowid,*";

                string strSql = "SELECT " + cols + ", AsBinary(" + GeometryColumn + ") AS sharpmap_tempgeometry ";
                strSql += "FROM " + Table + " WHERE ";
                strSql += GetOverlapsClause(geom);

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " AND " + DefinitionQuery;

                using (var adapter = new SQLiteDataAdapter(strSql, conn))
                {
                    var ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" &&
                                !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            var fdr = fdt.NewRow();
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

        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            using (var conn = SpatiaLiteConnection(ConnectionString))
            {
                string cols = "*";
                //If using rowid as oid, we need to explicitly request it!
                if (string.Compare(ObjectIdColumn, "rowid", true) == 0)
                    cols = "rowid,*";

                var strSql = "SELECT " + cols + ", AsBinary(" + GeometryColumn + ") AS sharpmap_tempgeometry ";
                strSql += "FROM " + Table + " WHERE ";
                strSql += GetBoxClause(box);

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " AND " + DefinitionQuery;

                using (var adapter = new SQLiteDataAdapter(strSql, conn))
                {
                    var ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" &&
                                !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            var fdr = fdt.NewRow();
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

        public override int GetFeatureCount()
        {
            int count;
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                string strSql = "SELECT COUNT(*) as numrecs FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += " WHERE " + DefinitionQuery;
                using (var command = new SQLiteCommand(strSql, conn))
                {
                    conn.Open();
                    var dtr = command.ExecuteReader();
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

        public override FeatureDataRow GetFeature(uint rowId)
        {
            using (SQLiteConnection conn = SpatiaLiteConnection(ConnectionString))
            {
                string cols = "*";
                //If using rowid as oid, we need to explicitly request it!
                if (string.Compare(ObjectIdColumn, "rowid", true) == 0)
                    cols = "rowid,*";

                string strSQL = "SELECT " +cols + ", AsBinary(" + GeometryColumn + ") AS sharpmap_tempgeometry FROM " + Table +
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
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"], Factory);
                            return fdr;
                        }
                        return null;
                    }
                    return null;
                }
            }
        }

        private Envelope _cachedExtents;

        private struct RTreeNodeEntry
        {
            public readonly long NodeId;
            public readonly Single XMin;
            public readonly Single XMax;
            public readonly Single YMin;
            public readonly Single YMax;

            public RTreeNodeEntry(byte[] buffer)
            {
                Array.Reverse(buffer, 0, 8);
                NodeId = BitConverter.ToInt64(buffer, 0);
                Array.Reverse(buffer, 8, 4);
                XMin = BitConverter.ToSingle(buffer, 8);
                Array.Reverse(buffer, 12, 4);
                XMax = BitConverter.ToSingle(buffer, 12);
                Array.Reverse(buffer, 16, 4);
                YMin = BitConverter.ToSingle(buffer, 16);
                Array.Reverse(buffer, 20, 4);
                YMax = BitConverter.ToSingle(buffer, 20);
            }

        }

        private class RTreeNode
        {
            private readonly short _treedepth;
            public readonly short NodesCount;
            public readonly RTreeNodeEntry[] Entries;

            public readonly Single XMin;
            public readonly Single XMax;
            public readonly Single YMin;
            public readonly Single YMax;

            public RTreeNode(byte[] buffer)
            {
                System.Diagnostics.Debug.Assert(buffer.Length == 960, "buffer.Length == 960");

                Array.Reverse(buffer, 0, 2);
                _treedepth = BitConverter.ToInt16(buffer, 0);
                Array.Reverse(buffer, 2, 2);
                NodesCount = BitConverter.ToInt16(buffer, 2);
                if (NodesCount == 0)
                {
                    XMin = YMin = float.NaN;
                    XMax = YMax = float.NaN;
                    return;
                }


                Entries = new RTreeNodeEntry[NodesCount];
                var entry = new byte[24];
                Buffer.BlockCopy(buffer, 4, entry, 0, 24);
                Entries[0] = new RTreeNodeEntry(entry);
                XMin = Entries[0].XMin;
                XMax = Entries[0].XMax;
                YMin = Entries[0].YMin;
                YMax = Entries[0].YMax;
                
                var offset = 28;
                for (var i = 1; i < NodesCount; i++)
                {
                    Buffer.BlockCopy(buffer, offset, entry, 0, 24);
                    Entries[i] = new RTreeNodeEntry(entry);

                    if (Entries[i].XMin < XMin) XMin = Entries[i].XMin;
                    if (Entries[i].XMax > XMax) XMax = Entries[i].XMax;
                    if (Entries[i].YMin < YMin) YMin = Entries[i].YMin;
                    if (Entries[i].YMax > YMax) YMax = Entries[i].YMax;
                    
                    offset += 24;
                }
            }
        }
        public override Envelope GetExtents()
        {
            if (_cachedExtents != null)
                return new Envelope(_cachedExtents);

            Envelope box = null;

            if (_spatiaLiteIndex == SpatiaLiteIndex.RTree)
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {   
                    conn.Open();
                    var strSQL = string.Format("SELECT \"data\" FROM \"idx_{0}_{1}_node\" WHERE \"nodeno\"=1;", 
                                                _table, _geometryColumn);
                    
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = strSQL;
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            var buffer = (byte[]) result;
                            var node = new RTreeNode(buffer);

                            _cachedExtents = float.IsNaN(node.XMin)
                                ? new Envelope()
                                : new Envelope(node.XMin, node.XMax, node.YMin, node.YMax);
                            return new Envelope(_cachedExtents);

                            //var entities = new float[2*_numOrdinateDimensions];
                            //var offset = 12;
                            //for (var i = 0; i < 2*_numOrdinateDimensions; i++)
                            //{
                            //    Array.Reverse(buffer, offset, 4);
                            //    entities[i] = BitConverter.ToSingle(buffer, offset);
                            //    offset += 4;
                            //}
                            //return new Envelope(entities[0], entities[2], entities[1], entities[3]);

                        }
                        throw new Exception();
                    }
                }
            }

            using (SQLiteConnection conn = SpatiaLiteConnection(ConnectionString))
            {
                //string strSQL = "SELECT Min(minx) AS MinX, Min(miny) AS MinY, Max(maxx) AS MaxX, Max(maxy) AS MaxY FROM " + this.Table;
                string strSQL;
                if (_spatiaLiteIndex == SpatiaLiteIndex.RTree && String.IsNullOrEmpty(_definitionQuery))
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

                using (var command = new SQLiteCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                        if (dr.Read())
                        {
                            if (dr.IsDBNull(0))
                                return new Envelope(0,0,0,0);

                            box = new Envelope(
                                (double) dr["minx"], (double) dr["maxx"], 
                                (double) dr["miny"], (double) dr["maxy"]);
                        }
                    conn.Close();
                }
                _cachedExtents = box;
                return new Envelope(box);
            }
        }

        private string FromClause(String table)
        {
            if (String.IsNullOrEmpty(DefinitionQuery))
                return table;

            return string.Format("{0} WHERE ({1})", table, _definitionQuery);
        }


        private string GetBoxClause(Envelope bbox)
        {
            if (UseSpatiaLiteIndex)
            {
                return string.Format(Map.NumberFormatEnUs, _spatiaLiteIndexClause, bbox.MaxX, bbox.MinX, bbox.MaxY, bbox.MinY);
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

        private IGeometry LineFromBbox(Envelope bbox)
        {
            var pointColl = new[] {bbox.Min(), bbox.Max()};
            return Factory.CreateLineString(pointColl);
        }

        public string GetOverlapsClause(IGeometry geom)
        {
            string wkt = GeometryToWKT.Write(geom);
            string retval = "Intersects(GeomFromText('" + wkt + "')," + _geometryColumn + ")=1";
            return retval;
        }

        /// <summary>
        /// Function to return an <see cref="IEnumerable{Spatialite}"/> for a given sqlite database.
        /// </summary>
        /// <param name="connectionString">The connection to the sqlite database.</param>
        /// <returns>
        /// An <see cref="IEnumerable{Spatialite}"/>.
        /// </returns>
        public static IEnumerable<SpatiaLite> GetSpatialTables(string connectionString)
        {
            IList<SpatiaLite> res = new List<SpatiaLite>();
            using (var cn = new SQLiteConnection(connectionString))
            {
                try
                {
                    cn.Open();
                    var cmd = new SQLiteCommand("SELECT * FROM geometry_columns;", cn);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            res.Add(new SpatiaLite(connectionString, (string)dr["f_table_name"],
                                                   (string)dr["f_geometry_column"], "ROWID"));
                    }

                    /*
                     * Need to check
                     *
                    cmd = new SQLiteCommand("SELECT * FROM geometry_views;", cn);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            res.Add(new SpatiaLite(connectionString, (string)dr["f_view_name"],
                                                   (string)dr["f_geometry_column"], "ROWID"));
                    }
                     */
                }
                catch (Exception ee)
                {
                    logger.Error(ee.Message, ee);
                }
                return res;
            }
        }

    }
}
