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

using SharpMap;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Converters.WellKnownBinary;

namespace SharpMap.Data.Providers
{
    public class SpatiaLite : IProvider, IDisposable
    {
        //string conStr = "Data Source=C:\\Workspace\\test.sqlite;Version=3;";
        public SpatiaLite(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
        {
            //Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", new SQLiteConnection(ConnectionStr)).ExecuteScalar();
            this.ConnectionString = ConnectionStr;
            this.Table = tablename;
            this.GeometryColumn = geometryColumnName; //Name of column to store geometry
            this.ObjectIdColumn = OID_ColumnName; //Name of object ID column
        }

        #region IProvider Members

        public System.Collections.ObjectModel.Collection<SharpMap.Geometries.Geometry> GetGeometriesInView(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                //conn.Open();
                //Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                string BoxIntersect = GetBoxClause(bbox);

                string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom ";
                strSQL += "FROM " + this.Table + " WHERE ";
                strSQL += BoxIntersect;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + this.DefinitionQuery;

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                SharpMap.Geometries.Geometry geom = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr[0]);
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

        public System.Collections.ObjectModel.Collection<uint> GetObjectIDsInView(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {
                string strSQL = "SELECT " + this.ObjectIdColumn + " ";
                strSQL += "FROM " + this.Table + " WHERE ";

                strSQL += GetBoxClause(bbox);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + this.DefinitionQuery + " AND ";

                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = Convert.ToUInt32(dr[0]);
                                objectlist.Add(ID);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
        {
            SharpMap.Geometries.Geometry geom = null;
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {

                string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + oid.ToString() + "'";
                conn.Open();
                Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                //byte[] b = dr[0] as byte[];
                                geom = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr[0]);
                            }
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        public void ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom, FeatureDataSet ds)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {

                string strSQL = "SELECT *, AsBinary(" + this.GeometryColumn + ") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + this.Table + " WHERE ";
                strSQL += GetOverlapsClause(geom);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + this.DefinitionQuery;

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                    System.Data.DataSet ds2 = new System.Data.DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
                        {
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }
        public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox box, FeatureDataSet ds)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {

                string strSQL = "SELECT *, AsBinary(" + this.GeometryColumn + ") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + this.Table + " WHERE ";
                strSQL += GetBoxClause(box);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " AND " + this.DefinitionQuery;

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                    System.Data.DataSet ds2 = new System.Data.DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
                        {
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);
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
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {

                string strSQL = "SELECT COUNT(*) as numrecs FROM " + this.Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + this.DefinitionQuery;
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

        public FeatureDataRow GetFeature(uint RowID)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {

                string strSQL = "SELECT *, AsBinary(" + this.GeometryColumn + ") AS sharpmap_tempgeometry FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + RowID.ToString() + "'";
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
                    conn.Open();
                    Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow dr = ds.Tables[0].Rows[0];
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry" && !col.ColumnName.StartsWith("Envelope_"))
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);
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

        public SharpMap.Geometries.BoundingBox GetExtents()
        {
            //TODO: Update GetExtents
            SharpMap.Geometries.BoundingBox box = null;
            using (SQLiteConnection conn = new SQLiteConnection(_ConnectionString))
            {

                //string strSQL = "SELECT Min(minx) AS MinX, Min(miny) AS MinY, Max(maxx) AS MaxX, Max(maxy) AS MaxY FROM " + this.Table;
                string strSQL = string.Format("SELECT max(MbrMaxY({0})) as maxy, max(MbrMaxX({0})) as maxx, min(MbrMinY({0})) as miny, min(MbrMinX({0})) as minx from {1};", _GeometryColumn, _Table);
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + this.DefinitionQuery;
                using (SQLiteCommand command = new SQLiteCommand(strSQL, conn))
                {
                    conn.Open();
                    Object retVal = new SQLiteCommand("SELECT load_extension('libspatialite-2.dll');", conn).ExecuteScalar();
                    using (SQLiteDataReader dr = command.ExecuteReader())
                        if (dr.Read())
                        {
                            box = new SharpMap.Geometries.BoundingBox((double)dr["minx"], (double)dr["miny"], (double)dr["maxx"], (double)dr["maxy"]);
                        }
                    conn.Close();
                }
                return box;
            }
        }

        public string ConnectionID
        {
            get { return _ConnectionString; }
        }

        private bool _IsOpen;

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

        private int _srid = -2;

        /// <summary>
        /// Spatial Reference ID
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set { _srid = value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }

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
        #endregion

        #region Native Members

        private string _ConnectionString;

        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        private string _Table;

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }

        private string _GeometryColumn;

        /// <summary>
        /// Name of geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _GeometryColumn; }
            set { _GeometryColumn = value; }
        }

        private string _ObjectIdColumn;

        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }
        private string _SpatialIndex;

        /// <summary>
        /// Name of the spatial index table
        /// </summary>
        public string SpatialIndex
        {
            get { return _SpatialIndex; }
            set { _SpatialIndex = value; }
        }

        private string GetBoxClause(SharpMap.Geometries.BoundingBox bbox)
        {
            if (!string.IsNullOrEmpty(SpatialIndex))
            {
                StringBuilder sql = new StringBuilder("ROWID IN ( ");
                sql.Append("SELECT pkid FROM ");
                sql.Append(SpatialIndex);
                sql.Append(" WHERE ");
                sql.AppendFormat(SharpMap.Map.NumberFormatEnUs,
                "xmin < {0} AND xmax > {1} AND ymin < {2} AND ymax > {3} )",
                 bbox.Max.X, bbox.Min.X, bbox.Max.Y, bbox.Min.Y);

                return sql.ToString();
            }

            string wkt = SharpMap.Converters.WellKnownText.GeometryToWKT.Write(LineFromBbox(bbox));
            return "MBRIntersects(GeomFromText('" + wkt + "')," + _GeometryColumn + ")=1";
        }

        private SharpMap.Geometries.IGeometry LineFromBbox(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<SharpMap.Geometries.Point> PointColl = new Collection<SharpMap.Geometries.Point>();
            PointColl.Add(bbox.Min);
            PointColl.Add(bbox.Max);

            return (SharpMap.Geometries.IGeometry)new SharpMap.Geometries.LineString(PointColl);
        }

        public string GetOverlapsClause(SharpMap.Geometries.Geometry geom)
        {
            string wkt = SharpMap.Converters.WellKnownText.GeometryToWKT.Write((SharpMap.Geometries.IGeometry)geom);
            string retval = "Intersects(GeomFromText('" + wkt + "')," + _GeometryColumn + ")=1";
            return retval;
            //return String.Format(SharpMap.Map.numberFormat_EnUS,
            //    "(minx < {0} AND maxx > {1} AND miny < {2} AND maxy > {3})",
            //    bbox.Max.X, bbox.Min.X, bbox.Max.Y, bbox.Min.Y);
        }

        private string _defintionQuery;

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }

        #endregion

    }
}
