// Copyright 2012 - Peter Löfås
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
using System.Globalization;
using GeoAPI.Geometries;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;
using BoundingBox = GeoAPI.Geometries.Envelope;
using Geometry = GeoAPI.Geometries.IGeometry;
using Common.Logging;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Managed SpatiaLite Provider for SharpMap that does NOT require the libspatialite DLLs
    /// <para>
    /// Spatialite is a spatial extension for the popular SQLite database engine.
    /// </para>
    /// </summary>
    public class ManagedSpatiaLite : BaseProvider
    {
        static ILog logger = LogManager.GetLogger(typeof(ManagedSpatiaLite));

        private string _defintionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private readonly string _spatiaLiteIndexClause;
        private readonly SpatiaLiteIndex _spatiaLiteIndex;
        private string _table;

        private int _numOrdinateDimensions = 2;

        private Boolean _useSpatialIndex;

        /// <summary>
        /// Gets or sets whether geometry definition lookup should use sql LIKE operator for name comparison.
        /// </summary>
        public static bool UseLike { get; set; }

        /// <summary>
        /// Creates an instance of SpatiaLite provider
        /// </summary>
        /// <param name="connectionStr">Connection String to SQLite database file 
        /// ("http://www.connectionstrings.com/sqlite")</param>
        /// <param name="tablename">Name of the table with geometry information</param>
        /// <param name="geometryColumnName">Name of the desired geometry column</param>
        /// <param name="oidColumnName">Name of the object Id column</param>
        public ManagedSpatiaLite(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
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
        public BoundingBox CachedExtent 
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
            get { return _defintionQuery; }
            set
            {
                _defintionQuery = value;
            }
        }

        public override Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            var features = new Collection<Geometry>();
            using (var conn = GetConnection(ConnectionString))
            {
                var boxIntersect = GetBoxClause(bbox);

                var strSql = "SELECT " + GeometryColumn + " AS Geom ";
                strSql += "FROM " + Table + " WHERE ";
                strSql += boxIntersect;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += " AND " + DefinitionQuery;

                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var geom = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])dr[0], Factory);

                                //If we didn't have a spatial index we need to compare geometries manually
                                if (_spatiaLiteIndex != SpatiaLiteIndex.RTree && !bbox.Intersects(geom.EnvelopeInternal))
                                    continue;

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

        private SQLiteConnection GetConnection(string ConnectionString)
        {
            var cn = new SQLiteConnection(ConnectionString);
            cn.Open();
            return cn;
        }

        public override Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = GetConnection(ConnectionString))
            {
                string strSql = "SELECT " + ObjectIdColumn + " ";
                if (_spatiaLiteIndex != SpatiaLiteIndex.RTree)
                    strSql += ", " + _geometryColumn + " ";

                strSql += "FROM " + Table + " WHERE ";

                strSql += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += " AND " + DefinitionQuery + " AND ";

                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            

                            //If we didn't have a spatial index we need to compare geometries manually
                            if (_spatiaLiteIndex != SpatiaLiteIndex.RTree)
                            {
                                var geom = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])dr[1], Factory);
                                if (!bbox.Intersects(geom.EnvelopeInternal))
                                    continue;
                            }

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

        public override Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (var conn = GetConnection(ConnectionString))
            {
                string strSql = "SELECT " + GeometryColumn + " AS Geom FROM " + Table + " WHERE " +
                                ObjectIdColumn + "='" + oid + "'";
                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                geom = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])dr[0], Factory);
                            }
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            using (var conn = GetConnection(ConnectionString))
            {
                string cols = "*";
                //If using rowid as oid, we need to explicitly request it!
                if (string.Compare(ObjectIdColumn, "rowid", true) == 0)
                    cols = "rowid,*";

                string strSql = "SELECT " + cols + ", " + GeometryColumn + " AS sharpmap_tempgeometry ";
                strSql += "FROM " + Table + " WHERE ";
                //strSql += GetOverlapsClause(geom);
                strSql += GetBoxClause(geom.EnvelopeInternal);

                if (!String.IsNullOrEmpty(_defintionQuery))
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
                            IGeometry g = null;
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                g = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])dr["sharpmap_tempgeometry"], Factory);
                            if (g != null && geom.Overlaps(g))
                            {
                                var fdr = fdt.NewRow();
                                foreach (DataColumn col in ds2.Tables[0].Columns)
                                    if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" &&
                                        !col.ColumnName.StartsWith("Envelope_"))
                                        fdr[col.ColumnName] = dr[col];
                                if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                    fdr.Geometry = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])dr["sharpmap_tempgeometry"], Factory);
                                fdt.AddRow(fdr);
                            }
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        public override void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            using (var conn = GetConnection(ConnectionString))
            {
                string cols = "*";
                //If using rowid as oid, we need to explicitly request it!
                if (string.Compare(ObjectIdColumn, "rowid", true) == 0)
                    cols = "rowid,*";

                var strSql = "SELECT " + cols + ", " + GeometryColumn + " AS sharpmap_tempgeometry ";
                strSql += "FROM " + Table + " WHERE ";
                strSql += GetBoxClause(box);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += " AND " + DefinitionQuery;

                //using (var adapter = new SQLiteDataAdapter(strSql, conn))
                using (var cmd = new SQLiteCommand(strSql,conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var fdt = new FeatureDataTable();
                        List<int> colIdxToRead = new List<int>();
                        Dictionary<string, string> addedColumns = new Dictionary<string, string>();
                        for (int c = 0; c < reader.FieldCount; c++)
                        {
                            string name = reader.GetName(c);
                            if (name != GeometryColumn && name != "sharpmap_tempgeometry" &&
                                        !name.StartsWith("Envelope_") && !addedColumns.ContainsKey(name))
                            {
                                fdt.Columns.Add(name, reader.GetFieldType(c));
                                colIdxToRead.Add(c);
                                addedColumns.Add(name, name);
                            }
                        }


                        while (reader.Read())
                        {
                            IGeometry g = null;
                            if (reader["sharpmap_tempgeometry"] != DBNull.Value)
                                g = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])reader["sharpmap_tempgeometry"], Factory);

                            //If not using RTree index we need to filter in code
                            if (_spatiaLiteIndex != SpatiaLiteIndex.RTree && !box.Intersects(g.EnvelopeInternal))
                                continue;

                            var fdr = fdt.NewRow();
                            for (int c = 0; c < colIdxToRead.Count; c++)
                                fdr[c] = reader[colIdxToRead[c]];
                            fdr.Geometry = g;
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                        reader.Close();
                    }
                }
            }
        }

        public override int GetFeatureCount()
        {
            int count;
            using (var conn = GetConnection(ConnectionString))
            {
                string strSql = "SELECT COUNT(*) as numrecs FROM " + Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += " WHERE " + DefinitionQuery;
                using (var command = new SQLiteCommand(strSql, conn))
                {
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
            using (SQLiteConnection conn = GetConnection(ConnectionString))
            {
                string cols = "*";
                //If using rowid as oid, we need to explicitly request it!
                if (string.Compare(ObjectIdColumn, "rowid", true) == 0)
                    cols = "rowid,*";

                string strSQL = "SELECT " +cols + ", " + GeometryColumn + " AS sharpmap_tempgeometry FROM " + Table +
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
                                fdr.Geometry = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])dr["sharpmap_tempgeometry"], Factory);
                            return fdr;
                        }
                        return null;
                    }
                    return null;
                }
            }
        }

        private BoundingBox _cachedExtents;

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
        public override BoundingBox GetExtents()
        {
            if (_cachedExtents != null)
                return new Envelope(_cachedExtents);

            Envelope box = null;

            if (_spatiaLiteIndex == SpatiaLiteIndex.RTree)
            {
                using (var conn = GetConnection(ConnectionString))
                {
                    var strSQL = string.Format("SELECT \"data\" FROM \"idx_{0}_{1}_node\" WHERE \"nodeno\"=1;",
                                                _table, _geometryColumn);

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = strSQL;
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            var buffer = (byte[])result;
                            var node = new RTreeNode(buffer);

                            _cachedExtents = new Envelope(node.XMin, node.XMax, node.YMin, node.YMax);
                            return new Envelope(_cachedExtents);
                        }
                        throw new Exception();
                    }
                }
            }
            else
            {

                using (SQLiteConnection conn = GetConnection(ConnectionString))
                {
                    string strSQL;

                    strSQL = string.Format(
                        "SELECT {1} from {0}",
                        _table, _geometryColumn);

                    using (var command = new SQLiteCommand(strSQL, conn))
                    {
                        using (var dr = command.ExecuteReader())
                        {
                            double minx = double.MaxValue, miny = double.MaxValue, maxx = double.MinValue, maxy = double.MinValue;
                            while (dr.Read())
                            {
                                var geom = SharpMap.Converters.SpatiaLite.GeometryFromSpatiaLite.Parse((byte[])dr[0], Factory);

                                var env = geom.EnvelopeInternal;
                                if (minx > env.MinX)
                                    minx = env.MinX;
                                if (miny > env.MinY)
                                    miny = env.MinY;
                                if (maxx < env.MaxX)
                                    maxx = env.MaxX;
                                if (maxy < env.MaxY)
                                    maxy = env.MaxY;

                                box = new Envelope(minx, maxx, miny, maxy);
                            }
                            dr.Close();
                        }
                        conn.Close();
                    }
                    _cachedExtents = box;
                    return new Envelope(box);
                }
            }
        }

        private string FromClause(String table)
        {
            if (String.IsNullOrEmpty(DefinitionQuery))
                return table;

            return string.Format("{0} WHERE ({1})", table, _defintionQuery);
        }


        private string GetBoxClause(BoundingBox bbox)
        {
            if (UseSpatiaLiteIndex)
            {
                return string.Format(Map.NumberFormatEnUs, _spatiaLiteIndexClause, bbox.MaxX, bbox.MinX, bbox.MaxY, bbox.MinY);
            }
            
            /*Without index, no  db filtering... :-( */
            return "1=1";
        }

        private Geometry LineFromBbox(BoundingBox bbox)
        {
            var pointColl = new[] {bbox.Min(), bbox.Max()};
            return Factory.CreateLineString(pointColl);
        }

   

        /// <summary>
        /// Function to return an <see cref="IEnumerable{Spatialite}"/> for a given sqlite database.
        /// </summary>
        /// <param name="connectionString">The connection to the sqlite database.</param>
        /// <returns>
        /// An <see cref="IEnumerable{Spatialite}"/>.
        /// </returns>
        public static IEnumerable<ManagedSpatiaLite> GetSpatialTables(string connectionString)
        {
            IList<ManagedSpatiaLite> res = new List<ManagedSpatiaLite>();
            using (var cn = new SQLiteConnection(connectionString))
            {
                try
                {
                    cn.Open();
                    var cmd = new SQLiteCommand("SELECT * FROM geometry_columns;", cn);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            res.Add(new ManagedSpatiaLite(connectionString, (string)dr["f_table_name"],
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