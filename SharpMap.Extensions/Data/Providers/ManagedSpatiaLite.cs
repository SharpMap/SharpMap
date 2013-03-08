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
using System.Data.Common;
using System.Globalization;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using Common.Logging;
using System.Data.SQLite;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Managed SpatiaLite Provider for SharpMap that does NOT require the libspatialite DLLs
    /// <para>
    /// Spatialite is a spatial extension for the popular SQLite database engine.
    /// </para>
    /// </summary>
    [Serializable]
    public class ManagedSpatiaLite : BaseProvider
    {
        static readonly ILog Logger = LogManager.GetLogger(typeof(ManagedSpatiaLite));

        private string _definitionQuery;
        private string _geometryColumn;
        private string _objectIdColumn;
        private readonly string _spatiaLiteIndexClause;
        private readonly SpatiaLiteIndex _spatiaLiteIndex;
        private string _table;
        private string _columns;
        private Envelope _cachedExtents;

        private readonly Ordinates _ordinates;

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
                                _ordinates = Ordinates.XY;
                                break;
                            case "3":
                            case "XYZ":
                                _ordinates = Ordinates.XYZ;
                                break;
                            case "XYM":
                                _ordinates = Ordinates.XYM;
                                break;
                            case "4":
                            case "XYZM":
                                _ordinates = Ordinates.XYZM;
                                break;
                            default:
                                throw new Exception("Cannot evaluate number of ordinate dimensions");

                        }

                        switch (dr.GetInt32(2))
                        {
                            case 1: //RTree
                                var indexName = string.Format(@"idx_{0}_{1}", tablename, geometryColumnName);
                                const string whereClause = @"xmin < {0} AND xmax > {1} AND ymin < {2} AND ymax > {3}";
                                _spatiaLiteIndexClause = string.Format(@"ROWID IN (SELECT pkid FROM {0} WHERE {1})", indexName, whereClause);
                                _spatiaLiteIndex = SpatiaLiteIndex.RTree;
                                _useSpatialIndex = true;
                                break;
                        }
                    }
                    dr.Close();
                }

                GetNonSpatialColumns();
            }
            catch (Exception)
            {
                SRID = -1;
                _spatiaLiteIndex = SpatiaLiteIndex.None;
                _useSpatialIndex = false;
            }
        }

        /// <summary>
        /// Get the non-spatial columns
        /// </summary>
        private void GetNonSpatialColumns()
        {
            if (!string.IsNullOrEmpty(_columns))
                return;

            if (string.IsNullOrEmpty(ConnectionID))
                return;

            using (var cn = GetConnection(ConnectionString))
            {
                using (var dr = new SQLiteCommand(string.Format("PRAGMA table_info('{0}');", Table), cn).ExecuteReader())
                {
                    if (!dr.HasRows)
                        throw new InvalidOperationException("Provider configuration incomplete or wrong!");

                    var columns = new List<string>
                        {
                            string.Equals(ObjectIdColumn, "rowid", StringComparison.OrdinalIgnoreCase)
                                ? "\"ROWID\" AS \"ROWID\""
                                : string.Format("\"{0}\"", ObjectIdColumn)
                        };

                    while (dr.Read())
                    {
                        var column = dr.GetString(1);
                        if (string.Equals(column, ObjectIdColumn)) continue;
                        if (string.Equals(column, GeometryColumn)) continue;
                        columns.Add(string.Format("\"{0}\"", column));
                    }

                    _columns = string.Join(", ", columns);
                }
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
            set
            {
                if (string.Equals(value, ConnectionID))
                    return;

                ConnectionID = value;
                _columns = string.Empty;
            }
        }

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _table; }
            set
            {
                if (string.Equals(value, _table))
                    return;
                _table = value;
                _columns = string.Empty;
            }
        }

        /// <summary>
        /// Name of geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _geometryColumn; }
            set
            {
                if (string.Equals(value, _geometryColumn))
                    return;
                _geometryColumn = value;
                _columns = string.Empty;
            }
        }

        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _objectIdColumn; }
            set
            {
                if (string.Equals(value, _objectIdColumn))
                    return;
                _objectIdColumn = value;
                _columns = string.Empty;
            }
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
        [Obsolete("SpatialIndex is obsolete, use UseSpatiaLiteIndex property")]
        public string SpatialIndex
        {
            get
            {
                return _spatiaLiteIndex.ToString();
            }
            set 
            {
                Logger.Debug("SpatialIndex is obsolete, use UseSpatiaLiteIndex property");
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
            using (var conn = GetConnection(ConnectionString))
            {
                var strSql = "SELECT \"" + GeometryColumn + "\" AS \"_smtmp_\" ";
                strSql += "FROM " + Table + " WHERE ";
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";
                strSql += GetBoxClause(bbox);

                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        var geoReader = new GaiaGeoReader(Factory.CoordinateSequenceFactory, Factory.PrecisionModel,
                                                          _ordinates);
                        while (dr.Read())
                        {
                            if (dr.IsDBNull(0))
                                continue;

                            var geom = geoReader.Read((byte[])dr.GetValue(0));

                            //No geometry to add
                            if (geom == null)
                                continue;

                            //If we didn't have a spatial index we need to compare geometries manually
                            if (_spatiaLiteIndex != SpatiaLiteIndex.RTree && !bbox.Intersects(geom.EnvelopeInternal))
                                continue;

                            features.Add(geom);
                        }
                    }
                }
            }
            return features;
        }

        private static SQLiteConnection GetConnection(string connectionString)
        {
            var cn = new SQLiteConnection(connectionString);
            cn.Open();
            return cn;
        }

        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = GetConnection(ConnectionString))
            {
                string strSql = "SELECT \"" + ObjectIdColumn + "\" ";
                if (_spatiaLiteIndex != SpatiaLiteIndex.RTree)
                    strSql += ", \"" + _geometryColumn + "\" ";

                strSql += "FROM " + Table + " WHERE ";

                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += GetBoxClause(bbox);


                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        var geoReader = new GaiaGeoReader(Factory.CoordinateSequenceFactory, Factory.PrecisionModel,
                                                          _ordinates);
                        while (dr.Read())
                        {
                            if (dr.IsDBNull(0))
                                continue;

                            //If we didn't have a spatial index we need to compare geometries manually
                            if (_spatiaLiteIndex != SpatiaLiteIndex.RTree)
                            {
                                if (dr.IsDBNull(1))
                                    continue;

                                var geom = geoReader.Read((byte[])dr.GetValue(1));
                                if (geom == null)
                                    continue;

                                if (!bbox.Intersects(geom.EnvelopeInternal))
                                    continue;
                            }

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
            var reader = new GaiaGeoReader(Factory.CoordinateSequenceFactory, Factory.PrecisionModel, _ordinates);
            using (var conn = GetConnection(ConnectionString))
            {
                string strSql = "SELECT \"" + GeometryColumn + "\" AS Geom FROM \"" + Table + "\" WHERE \"" +
                                ObjectIdColumn + "\" = " + oid;
                using (var command = new SQLiteCommand(strSql, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!dr.IsDBNull(0))
                            {
                                geom = reader.Read((byte[]) dr[0]);
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
            var prepGeom = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            GetNonSpatialColumns();

            using (var conn = GetConnection(ConnectionString))
            {
                var strSql = "SELECT " + _columns + ", \"" + GeometryColumn + "\" AS \"_smtmp_\" ";
                strSql += "FROM " + Table + " WHERE ";

                // Attribute constraint
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";

                // Spatial constraint
                strSql += GetBoxClause(geom.EnvelopeInternal);

                using (var cmd = new SQLiteCommand(strSql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var geomIndex = reader.FieldCount - 1;
                        var fdt = CreateTableFromReader(reader, geomIndex);

                        var dataTransfer = new object[geomIndex];
                        var geoReader = new GaiaGeoReader(Factory.CoordinateSequenceFactory, Factory.PrecisionModel,
                                                          _ordinates);
                        fdt.BeginLoadData();
                        while (reader.Read())
                        {
                            IGeometry g = null;
                            if (!reader.IsDBNull(geomIndex))
                                g = geoReader.Read((byte[]) reader.GetValue(geomIndex));

                            //No geometry, no feature!
                            if (g == null)
                                continue;

                            //If not intersecting
                            if (!prepGeom.Intersects(g))
                                continue;

                            //Get all the attribute data
                            var count = reader.GetValues(dataTransfer);
                            System.Diagnostics.Debug.Assert(count == dataTransfer.Length);
                            
                            var fdr = (FeatureDataRow) fdt.LoadDataRow(dataTransfer, true);
                            fdr.Geometry = g;
                        }
                        fdt.EndLoadData();
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        private FeatureDataTable CreateTableFromReader(DbDataReader reader, int geomIndex)
        {
            var res = new FeatureDataTable {TableName = Table};
            for (var c = 0; c < geomIndex; c++)
            {
                var fieldType = reader.GetFieldType(c);
                if (fieldType == null)
                    throw new Exception("Unable to retrieve field type for column " + c);
                res.Columns.Add(DequoteIdentifier(reader.GetName(c)), fieldType);
            }
            return res;
        }

        /// <summary>
        /// Function to remove double quotes from identifiers. SQLite returns quoted column names when querying a view.
        /// </summary>
        /// <param name="item">The identifier</param>
        /// <returns>The unquoted <paramref name="item"/></returns>
        private static string DequoteIdentifier(string item)
        {
            if (item.StartsWith("\"") && item.EndsWith("\""))
                return item.Substring(1, item.Length - 2);
            return item;
        }

        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            GetNonSpatialColumns();
            using (var conn = GetConnection(ConnectionString))
            {
                var strSql = "SELECT " + _columns + ", \"" + GeometryColumn + "\" AS \"_smtmp_\" ";
                strSql += "FROM " + Table + " WHERE ";

                // Attribute constraint
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSql += DefinitionQuery + " AND ";
                
                // Spatial constraint
                strSql += GetBoxClause(box);

                using (var cmd = new SQLiteCommand(strSql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var geomIndex = reader.FieldCount - 1;
                        var fdt = CreateTableFromReader(reader, geomIndex);

                        var dataTransfer = new object[geomIndex];
                        var geoReader = new GaiaGeoReader(Factory.CoordinateSequenceFactory, Factory.PrecisionModel,
                                                          _ordinates);
                        fdt.BeginLoadData();
                        while (reader.Read())
                        {
                            IGeometry g = null;
                            if (!reader.IsDBNull(geomIndex))
                                g = geoReader.Read((byte[])reader.GetValue(geomIndex));

                            //No geometry, no feature!
                            if (g == null)
                                continue;

                            //If not using RTree index we need to filter in code
                            if (_spatiaLiteIndex != SpatiaLiteIndex.RTree && !box.Intersects(g.EnvelopeInternal))
                                continue;

                            //Get all the attribute data
                            var count = reader.GetValues(dataTransfer);
                            System.Diagnostics.Debug.Assert(count == dataTransfer.Length);

                            var fdr = (FeatureDataRow)fdt.LoadDataRow(dataTransfer, true);
                            fdr.Geometry = g;
                        }
                        fdt.EndLoadData();
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        public override int GetFeatureCount()
        {
            int count;
            using (var conn = GetConnection(ConnectionString))
            {
                var strSql = "SELECT COUNT(*) as numrecs FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
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
            GetNonSpatialColumns();
            using (var conn = GetConnection(ConnectionString))
            {
                var strSql = "SELECT " + _columns + ", \"" + GeometryColumn + "\" AS \"_smtmp_\" ";
                strSql += "FROM " + Table + " WHERE \"" + ObjectIdColumn +"\" = " + rowId;

                using (var cmd = new SQLiteCommand(strSql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var geomIndex = reader.FieldCount - 1;
                        var fdt = CreateTableFromReader(reader, geomIndex);

                        var dataTransfer = new object[geomIndex];
                        var geoReader = new GaiaGeoReader(Factory.CoordinateSequenceFactory, Factory.PrecisionModel,
                                                          _ordinates);
                        fdt.BeginLoadData();
                        while (reader.Read())
                        {
                            IGeometry g = null;
                            if (!reader.IsDBNull(geomIndex))
                                g = geoReader.Read((byte[])reader.GetValue(geomIndex));

                            //No geometry, no feature!
                            if (g == null)
                                continue;

                            //Get all the attribute data
                            var count = reader.GetValues(dataTransfer);
                            System.Diagnostics.Debug.Assert(count == dataTransfer.Length);

                            var fdr = (FeatureDataRow)fdt.LoadDataRow(dataTransfer, true);
                            fdr.Geometry = g;
                        }
                        fdt.EndLoadData();
                        return (FeatureDataRow)fdt.Rows[0];
                    }
                }
            }
        }

        private struct RTreeNodeEntry
        {
// ReSharper disable NotAccessedField.Local
            public readonly long NodeId;
// ReSharper restore NotAccessedField.Local
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
                    XMin = XMax = YMin = YMax = Single.NaN;
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
                            _cachedExtents = float.IsNaN(node.XMin) 
                                ? new Envelope() 
                                : new Envelope(node.XMin, node.XMax, node.YMin, node.YMax);
                            return new Envelope(_cachedExtents);
                        }
                        throw new Exception();
                    }
                }
            }
            using (var conn = GetConnection(ConnectionString))
            {
                var strSQL = string.Format("SELECT \"{1}\" AS \"_smtmp_\" FROM \"{0}\"",
                                           _table, _geometryColumn);

                using (var command = new SQLiteCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            double minx = double.MaxValue,
                                   miny = double.MaxValue,
                                   maxx = double.MinValue,
                                   maxy = double.MinValue;
                            var geoReader = new GaiaGeoReader(Factory.CoordinateSequenceFactory, Factory.PrecisionModel,
                                                              _ordinates);

                            while (dr.Read())
                            {
                                var geom = geoReader.Read((byte[]) dr.GetValue(0));

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
                        else
                        {
                            box = new Envelope();
                        }
                    }
                    conn.Close();
                }
                _cachedExtents = box;
                return new Envelope(box);
            }
        }

        private string GetBoxClause(Envelope bbox)
        {
            if (UseSpatiaLiteIndex)
            {
                return string.Format(Map.NumberFormatEnUs, 
                    _spatiaLiteIndexClause, bbox.MaxX, bbox.MinX, bbox.MaxY, bbox.MinY);
            }
            
            /*Without index, no  db filtering... :-( */
            return "1=1";
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
            var res = new List<ManagedSpatiaLite>();
            using (var cn = new SQLiteConnection(connectionString))
            {
                try
                {
                    cn.Open();
                    var cmd = new SQLiteCommand("SELECT * FROM \"geometry_columns\";", cn);
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
                    Logger.Error(ee.Message, ee);
                }
                return res;
            }
        }
    }


}