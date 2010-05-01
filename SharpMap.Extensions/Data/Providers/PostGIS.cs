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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using Npgsql;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Geometries;
#if DEBUG

#endif

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// PostGreSQL / PostGIS dataprovider
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
    public class PostGIS : IProvider, IDisposable
    {
        private string _ConnectionString;
        private string _defintionQuery;
        private string _GeometryColumn;
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private string _Schema = "public";
        private int _srid = -2;
        private string _Table;

        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
        {
            ConnectionString = ConnectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = OID_ColumnName;
        }

        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS(string ConnectionStr, string tablename, string OID_ColumnName)
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
            set
            {
                _Table = value;
                qualifyTable();
            }
        }

        /// <summary>
        /// Schema Name
        /// </summary>
        public string Schema
        {
            get { return _Schema; }
            set { _Schema = value; }
        }

        /// <summary>
        /// Qualified Table Name
        /// </summary>
        public string QualifiedTable
        {
            get { return string.Format("\"{0}\".\"{1}\"", _Schema, _Table); }
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
            //Don't really do anything. npgsql's ConnectionPooling takes over here
            _IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. npgsql's ConnectionPooling takes over here
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
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.Min.X.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.Min.Y.ToString(Map.NumberFormatEnUs) + "," +
                                 bbox.Max.X.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.Max.Y.ToString(Map.NumberFormatEnUs) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(Map.NumberFormatEnUs) + ")";

                string strSQL = "SELECT AsBinary(\"" + GeometryColumn + "\") AS Geom ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strBbox;

#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetGeometriesInView: executing sql:", strSQL));
#endif
                using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (NpgsqlDataReader dr = command.ExecuteReader())
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
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT AsBinary(\"" + GeometryColumn + "\") AS Geom FROM " + QualifiedTable +
                                " WHERE \"" + ObjectIdColumn + "\"='" + oid + "'";
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetGeometryByID: executing sql:", strSQL));
#endif
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
                {
                    using (NpgsqlDataReader dr = command.ExecuteReader())
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
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.Min.X.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.Min.Y.ToString(Map.NumberFormatEnUs) + "," +
                                 bbox.Max.X.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.Max.Y.ToString(Map.NumberFormatEnUs) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(Map.NumberFormatEnUs) + ")";

                string strSQL = "SELECT \"" + ObjectIdColumn + "\" ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strBbox;
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetObjectIDsInView: executing sql:", strSQL));
#endif

                using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (NpgsqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = (uint) (int) dr[0];
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
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
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

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(strSQL, conn))
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
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + QualifiedTable;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    count = Convert.ToInt32(command.ExecuteScalar());
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
                    string strSQL = "select srid from geometry_columns WHERE f_table_schema='" + _Schema +
                                    "' AND f_table_name='" + _Table + "'";

                    using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
                        {
                            try
                            {
                                conn.Open();
                                _srid = (int) command.ExecuteScalar();
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
            set { throw (new ApplicationException("Spatial Reference ID cannot by set on a PostGIS table")); }
        }


        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strSQL = "select * , AsBinary(\"" + GeometryColumn + "\") As sharpmap_tempgeometry from " +
                                QualifiedTable + " WHERE \"" + ObjectIdColumn + "\"='" + RowID + "'";
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(strSQL, conn))
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
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT EXTENT(\"" + GeometryColumn + "\") FROM " + QualifiedTable;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    object result = command.ExecuteScalar();
                    conn.Close();
                    if (result == DBNull.Value)
                        return null;
                    string strBox = (string) result;
                    if (strBox.StartsWith("BOX("))
                    {
                        string[] vals = strBox.Substring(4, strBox.IndexOf(")") - 4).Split(new char[2] {',', ' '});
                        return new BoundingBox(
                            double.Parse(vals[0], Map.NumberFormatEnUs),
                            double.Parse(vals[1], Map.NumberFormatEnUs),
                            double.Parse(vals[2], Map.NumberFormatEnUs),
                            double.Parse(vals[3], Map.NumberFormatEnUs));
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
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.Min.X.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.Min.Y.ToString(Map.NumberFormatEnUs) + "," +
                                 bbox.Max.X.ToString(Map.NumberFormatEnUs) + " " +
                                 bbox.Max.Y.ToString(Map.NumberFormatEnUs) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(Map.NumberFormatEnUs) + ")";

                string strSQL = "SELECT *, AsBinary(\"" + GeometryColumn + "\") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strBbox;
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}\n", "ExecuteIntersectionQuery: executing sql:", strSQL));
#endif
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(strSQL, conn))
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

        private bool disposed;

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
        ~PostGIS()
        {
            Dispose();
        }

        #endregion

        private void qualifyTable()
        {
            int dotPos = _Table.IndexOf(".");
            if (dotPos == -1)
            {
                _Schema = "public";
            }
            else
            {
                _Schema = _Table.Substring(0, dotPos);
                _Schema = _Schema.Replace('"', ' ').Trim();
            }
            _Table = _Table.Substring(dotPos + 1);
            _Table = _Table.Replace('"', ' ').Trim();
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
            //Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strGeom = "GeomFromText('" + geom.AsText() + "')";
                if (SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + SRID + ")";

                string strSQL = "SELECT * , AsBinary(\"" + GeometryColumn + "\") As sharpmap_tempgeometry FROM " +
                                QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + "buffer(" + strGeom + "," +
                          distance.ToString(Map.NumberFormatEnUs) + ")";
                strSQL += " AND distance(\"" + GeometryColumn + "\", " + strGeom + ")<" +
                          distance.ToString(Map.NumberFormatEnUs);

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(strSQL, conn))
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
        /// Queries the PostGIS database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSQL = "SELECT f_geometry_column from geometry_columns WHERE f_table_schema='" + _Schema +
                            "' and f_table_name='" + _Table + "'";
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
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
        [Obsolete("Use ExecuteIntersectionQuery")]
        public void GetFeaturesInView(BoundingBox bbox, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }
    }
}