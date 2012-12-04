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
using System.Data.OleDb;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// The OleDbPoint provider is used for rendering point data from an OleDb compatible datasource.
    /// </summary>
    /// <remarks>
    /// <para>The data source will need to have two double-type columns, xColumn and yColumn that contains the coordinates of the point,
    /// and an integer-type column containing a unique identifier for each row.</para>
    /// <para>To get good performance, make sure you have applied indexes on ID, xColumn and yColumns in your datasource table.</para>
    /// </remarks>
    [Serializable]
    public class OleDbPoint : PreparedGeometryProvider
    {
        private string _definitionQuery;

        /// <summary>
        /// Initializes a new instance of the OleDbPoint provider
        /// </summary>
        /// <param name="connectionStr"></param>
        /// <param name="tablename"></param>
        /// <param name="oidColumnName"></param>
        /// <param name="xColumn"></param>
        /// <param name="yColumn"></param>
        public OleDbPoint(string connectionStr, string tablename, string oidColumnName, string xColumn, string yColumn)
        {
            Table = tablename;
            XColumn = xColumn;
            YColumn = yColumn;
            ObjectIdColumn = oidColumnName;
            ConnectionString = connectionStr;
        }

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table { get; set; }


        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn { get; set; }

        /// <summary>
        /// Name of column that contains X coordinate
        /// </summary>
        public string XColumn { get; set; }

        /// <summary>
        /// Name of column that contains Y coordinate
        /// </summary>
        public string YColumn { get; set; }

        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return ConnectionID; }
            set { ConnectionID = value; }
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
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var features = new Collection<IGeometry>();
            using (var conn = new OleDbConnection(ConnectionString))
            {
                //open the connection
                conn.Open();

                var strSQL = "Select " + XColumn + ", " + YColumn + " FROM " + Table + " WHERE ";
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += _definitionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += XColumn + " BETWEEN " + bbox.Left().ToString(Map.NumberFormatEnUs) + " AND " +
                          bbox.Right().ToString(Map.NumberFormatEnUs) + " AND " +
                          YColumn + " BETWEEN " + bbox.Bottom().ToString(Map.NumberFormatEnUs) + " AND " +
                          bbox.Top().ToString(Map.NumberFormatEnUs);

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                if (dr[0] != DBNull.Value && dr[1] != DBNull.Value)
                                    features.Add(Factory.CreatePoint(new Coordinate((double) dr[0], (double) dr[1])));
                            }
                        }
                    }
                }
            }
            return features;
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = new OleDbConnection(ConnectionString))
            {
                //open the connection
                conn.Open();

                var strSQL = "Select " + ObjectIdColumn + " FROM " + Table + " WHERE ";
                if (!String.IsNullOrEmpty(_definitionQuery))
                    strSQL += _definitionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += XColumn + " BETWEEN " + bbox.Left().ToString(Map.NumberFormatEnUs) + " AND " +
                          bbox.Right().ToString(Map.NumberFormatEnUs) + " AND " + YColumn +
                          " BETWEEN " + bbox.Bottom().ToString(Map.NumberFormatEnUs) + " AND " +
                          bbox.Top().ToString(Map.NumberFormatEnUs);

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                                if (dr[0] != DBNull.Value)
                                    objectlist.Add((uint) (int) dr[0]);
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public override IGeometry GetGeometryByID(uint oid)
        {
            using (var conn = new OleDbConnection(ConnectionString))
            {
                var strSQL = "Select " + XColumn + ", " + YColumn + " FROM " + Table + " WHERE " + ObjectIdColumn +
                                "=" + oid.ToString(Map.NumberFormatEnUs);

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr != null && dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                //If the read row is OK, create a point geometry from the XColumn and YColumn and return it
                                if (dr[0] != DBNull.Value && dr[1] != DBNull.Value)
                                    return Factory.CreatePoint(new Coordinate((double) dr[0], (double) dr[1]));
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public override void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            //List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
            using (var conn = new OleDbConnection(ConnectionString))
            {
                var strSQL = "Select * FROM " + Table + " WHERE ";
                if (!String.IsNullOrEmpty(_definitionQuery))
                    //If a definition query has been specified, add this as a filter on the query
                    strSQL += _definitionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += XColumn + " BETWEEN " + bbox.Left().ToString(Map.NumberFormatEnUs) + " AND " +
                          bbox.Right().ToString(Map.NumberFormatEnUs) + " AND " + YColumn +
                          " BETWEEN " + bbox.Bottom().ToString(Map.NumberFormatEnUs) + " AND " +
                          bbox.Top().ToString(Map.NumberFormatEnUs);

                using (var adapter = new OleDbDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    var ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            IGeometry geom;
                            if (dr[XColumn] != DBNull.Value && dr[YColumn] != DBNull.Value)
                                geom = Factory.CreatePoint(new Coordinate((double)dr[XColumn], (double)dr[YColumn]));
                            else
                                continue;

                            if (bbox.Intersects(geom.Coordinate))
                            {
                                var fdr = fdt.NewRow();

                                foreach (DataColumn col in ds2.Tables[0].Columns)
                                    fdr[col.Ordinal] = dr[col];
                                fdr.Geometry = geom;

                                fdt.AddRow(fdr);
                            }
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>Total number of features</returns>
        public override int GetFeatureCount()
        {
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();

                var strSQL = "SELECT Count(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    //If a definition query has been specified, add this as a filter on the query
                    strSQL += " WHERE " + _definitionQuery;

                return (int)new OleDbCommand(strSQL, conn).ExecuteScalar();
            }
        }

        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public override FeatureDataRow GetFeature(uint rowId)
        {
            using (var conn = new OleDbConnection(ConnectionString))
            {
                var strSQL = "SELECT * FROM " + Table + " WHERE " + ObjectIdColumn + "=" + rowId.ToString(Map.NumberFormatEnUs);

                using (var adapter = new OleDbDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    var ds = new DataSet();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            var dr = ds.Tables[0].Rows[0];
                            var fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                fdr[col.Ordinal] = dr[col];
                            if (dr[XColumn] != DBNull.Value && dr[YColumn] != DBNull.Value)
                                fdr.Geometry =
                                    Factory.CreatePoint(new Coordinate((double) dr[XColumn], (double) dr[YColumn]));
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
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                var strSQL = "SELECT Min(" + XColumn + ") as MinX, Min(" + YColumn + ") As MinY, " +
                                "Max(" + XColumn + ") As MaxX, Max(" + YColumn + ") As MaxY FROM " + Table;
                if (!String.IsNullOrEmpty(_definitionQuery))
                    //If a definition query has been specified, add this as a filter on the query
                    strSQL += " WHERE " + _definitionQuery;

                using (var command = new OleDbCommand(strSQL, conn))
                {
                    using (var dr = command.ExecuteReader())
                    {
                        if (dr != null && dr.HasRows )
                        if (dr.Read())
                        {
                            //If the read row is OK, create a point geometry from the XColumn and YColumn and return it
                            if (dr[0] != DBNull.Value && dr[1] != DBNull.Value && dr[2] != DBNull.Value &&
                                dr[3] != DBNull.Value)
                                return new Envelope(new Coordinate((float)dr[0], (float)dr[1]), new Coordinate((float)dr[2], (float)dr[3]));
                        }
                    }
                    conn.Close();
                }
            }
            return null;
        }

        #region Disposers and finalizers

        #endregion
    }
}