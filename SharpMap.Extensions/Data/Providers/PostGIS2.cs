// Copyright 2005, 2006 - Christian Gräfe (www.sharptools.de)
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
using PostgreSql.Data.PgTypes;
using PostgreSql.Data.PostgreSqlClient;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Geometries;
// more info at http://sf.net/projects/pgsqlclient

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
    /// myLayer.DataSource = new SharpMap.Data.Providers.PostGIS2(ConnStr, "myTable");
    /// </code>
    /// </example>
    [Serializable]
    public class PostGIS2 : IProvider, IDisposable
    {
        private string _ConnectionString;
        private string _defintionQuery;
        private string _GeometryColumn;
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private int _srid = -2;
        private string _Table;

        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS2(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
        {
            ConnectionString = ConnectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = OID_ColumnName;
        }

        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionString">Connectionstring</param>
        /// <param name="TableName">Name of data table</param>
        /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS2(string ConnectionString, string TableName, string OIdColumnName)
            : this(ConnectionString, TableName, "", OIdColumnName)
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
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strBbox = GetBoundingBoxSql(bbox, SRID);

                String strSql = String.Format("SELECT AsBinary({0}) as geom FROM {1} WHERE ",
                                              GeometryColumn,
                                              Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += String.Format("{0} && {1}", GeometryColumn, strBbox);

                using (PgCommand command = new PgCommand(strSql, conn))
                {
                    conn.Open();
                    using (PgDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            //object obj = dr[0];
                            Geometry geom = null;

                            //							if ( typeof(PgPoint) == obj.GetType() )
                            //								geom = new SharpMap.Geometries.Point( ((PgPoint)obj).X, ((PgPoint)obj).Y );
                            //							else 
                            if (dr[0] != DBNull.Value)
                                geom = GeometryFromWKB.Parse((byte[]) dr[0]);


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
        public Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                String strSql = String.Format("SELECT AsBinary({0}) As Geom FROM {1} WHERE {2} = '{3}'",
                                              GeometryColumn,
                                              Table,
                                              ObjectIdColumn,
                                              oid);

                conn.Open();
                using (PgCommand command = new PgCommand(strSql, conn))
                {
                    using (PgDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            object obj = dr[0];
                            if (typeof (PgPoint) == obj.GetType())
                                geom = new Point(((PgPoint) obj).X, ((PgPoint) obj).Y);
                            else if (obj != DBNull.Value)
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
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strBbox = GetBoundingBoxSql(bbox, SRID);

                String strSql = String.Format("SELECT {0} FROM {1} WHERE ", ObjectIdColumn, Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += DefinitionQuery + " AND ";

                strSql += GeometryColumn + " && " + strBbox;

                using (PgCommand command = new PgCommand(strSql, conn))
                {
                    conn.Open();
                    using (PgDataReader dr = command.ExecuteReader())
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
            //List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strGeom = "GeomFromText('" + geom.AsText() + "')";
                if (SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + SRID.ToString() + ")";

                string strSQL = "SELECT * , AsBinary(" + GeometryColumn + ") As sharpmap_tempgeometry FROM " + Table +
                                " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += GeometryColumn + " && " + strGeom + " AND distance(" + GeometryColumn + ", " + strGeom + ")<0";

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
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
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + Table;

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;

                using (PgCommand command = new PgCommand(strSQL, conn))
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
                    string strSQL = "select srid from geometry_columns WHERE f_table_name = @Table";
                    using (PgConnection conn = new PgConnection(_ConnectionString))
                    using (PgCommand command = new PgCommand(strSQL, conn))
                    {
                        try
                        {
                            conn.Open();

                            command.Parameters.Add(new PgParameter("@Table", PgDbType.VarChar));
                            command.Parameters[0].Value = _Table;

                            _srid = (int) command.ExecuteScalar();
                            conn.Close();
                        }
                        catch
                        {
                            _srid = -1;
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
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strSQL =
                    String.Format("select * , AsBinary({0}) As sharpmap_tempgeometry from {1} WHERE {2} = '{3}'",
                                  GeometryColumn, Table, ObjectIdColumn, RowID);

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
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
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strSQL = String.Format("SELECT EXTENT({0}) FROM {1}",
                                              GeometryColumn,
                                              Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;


                strSQL += ";";

                using (PgCommand command = new PgCommand(strSQL, conn))
                {
                    conn.Open();

                    BoundingBox bbox = null;
                    try
                    {
                        PgBox2D result = (PgBox2D) command.ExecuteScalar();
                        bbox = new BoundingBox(result.LowerLeft.X, result.LowerLeft.Y, result.UpperRight.X,
                                               result.UpperRight.Y);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Box2d couldn't fetched from table. " + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    return bbox;
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
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strBbox = GetBoundingBoxSql(bbox, SRID);

                string strSQL = String.Format("SELECT *, AsBinary({0}) AS sharpmap_tempgeometry FROM {1} WHERE ",
                                              GeometryColumn,
                                              Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += GeometryColumn + " && " + strBbox;

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
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

        #region * Sql builder methods *

        /// <summary>
        /// returns the Bounding Box Sql part for PostGis Extension queries
        /// </summary>
        /// <param name="bbox">Bounding Box</param>
        /// <param name="iSRID">Spatial Reference Id</param>
        /// <returns>String</returns>
        private static string GetBoundingBoxSql(BoundingBox bbox, int iSRID)
        {
            string strBbox = String.Format("box2d('BOX3D({0} {1},{2} {3})'::box3d)",
                                           bbox.Min.X.ToString(Map.NumberFormatEnUs),
                                           bbox.Min.Y.ToString(Map.NumberFormatEnUs),
                                           bbox.Max.X.ToString(Map.NumberFormatEnUs),
                                           bbox.Max.Y.ToString(Map.NumberFormatEnUs));

            if (iSRID > 0)
                strBbox = String.Format(Map.NumberFormatEnUs, "SetSRID({0},{1})", strBbox, iSRID);

            return strBbox;
        }

        #endregion

        #region Disposers and finalizers

        private bool disposed = false;

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
        ~PostGIS2()
        {
            Dispose();
        }

        #endregion

        /// <summary>
        /// Returns all objects within a distance of a geometry
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [Obsolete("Use ExecuteIntersectionQuery instead")]
        public FeatureDataTable QueryFeatures(Geometry geom, double distance)
        {
            Collection<Geometry> features = new Collection<Geometry>();
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strGeom = "GeomFromText('" + geom.AsText() + "')";
                if (SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + SRID.ToString() + ")";

                string strSQL = "SELECT * , AsBinary(" + GeometryColumn + ") As sharpmap_tempgeometry FROM " + Table +
                                " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += GeometryColumn + " && " + "buffer(" + strGeom + "," + distance.ToString(Map.NumberFormatEnUs) +
                          ")";
                strSQL += " AND distance(" + GeometryColumn + ", " + strGeom + ")<" +
                          distance.ToString(Map.NumberFormatEnUs);

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
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
        /// Queries the PostGIS database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSQL = "select f_geometry_column from geometry_columns WHERE f_table_name = @Table'";

            using (PgConnection conn = new PgConnection(_ConnectionString))
            using (PgCommand command = new PgCommand(strSQL, conn))
            {
                conn.Open();

                command.Parameters.Add(new PgParameter("@Table", PgDbType.VarChar));
                command.Parameters[0].Value = _Table;

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