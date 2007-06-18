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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime;
using System.Text;

// more info at http://sf.net/projects/pgsqlclient
using PostgreSql.Data.PostgreSqlClient;
using PostgreSql.Data.PgTypes;

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
    public class PostGIS2 : SharpMap.Data.Providers.IProvider, IDisposable
    {
        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS2(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
        {
            this.ConnectionString = ConnectionStr;
            this.Table = tablename;
            this.GeometryColumn = geometryColumnName;
            this.ObjectIdColumn = OID_ColumnName;
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
            this.GeometryColumn = this.GetGeometryColumn();
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


        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<Geometries.Geometry> GetGeometriesInView(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strBbox = GetBoundingBoxSql(bbox, this.SRID);

                String strSql = String.Format("SELECT AsBinary({0}) as geom FROM {1} WHERE ",
                                              this.GeometryColumn,
                                              this.Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += this.DefinitionQuery + " AND ";

                strSql += String.Format("{0} && {1}", this.GeometryColumn, strBbox);

                using (PgCommand command = new PgCommand(strSql, conn))
                {
                    conn.Open();
                    using (PgDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            //object obj = dr[0];
                            SharpMap.Geometries.Geometry geom = null;

                            //							if ( typeof(PgPoint) == obj.GetType() )
                            //								geom = new SharpMap.Geometries.Point( ((PgPoint)obj).X, ((PgPoint)obj).Y );
                            //							else 
                            if (dr[0] != DBNull.Value)
                                geom = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr[0]);


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
        public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
        {
            SharpMap.Geometries.Geometry geom = null;
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                String strSql = String.Format("SELECT AsBinary({0}) As Geom FROM {1} WHERE {2} = '{3}'",
                                              this.GeometryColumn,
                                              this.Table,
                                              this.ObjectIdColumn,
                                              oid);

                conn.Open();
                using (PgCommand command = new PgCommand(strSql, conn))
                {
                    using (PgDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            object obj = dr[0];
                            if (typeof(PgPoint) == obj.GetType())
                                geom = new SharpMap.Geometries.Point(((PgPoint)obj).X, ((PgPoint)obj).Y);
                            else if (obj != DBNull.Value)
                                geom = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr[0]);
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
        public Collection<uint> GetObjectIDsInView(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strBbox = GetBoundingBoxSql(bbox, this.SRID);

                String strSql = String.Format("SELECT {0} FROM {1} WHERE ", this.ObjectIdColumn, this.Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSql += this.DefinitionQuery + " AND ";

                strSql += this.GeometryColumn + " && " + strBbox;

                using (PgCommand command = new PgCommand(strSql, conn))
                {
                    conn.Open();
                    using (PgDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = (uint)(int)dr[0];
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
        /// Returns all objects within a distance of a geometry
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [Obsolete("Use ExecuteIntersectionQuery instead")]
        public SharpMap.Data.FeatureDataTable QueryFeatures(SharpMap.Geometries.Geometry geom, double distance)
        {
            Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strGeom = "GeomFromText('" + geom.AsText() + "')";
                if (this.SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + this.SRID.ToString() + ")";

                string strSQL = "SELECT * , AsBinary(" + this.GeometryColumn + ") As sharpmap_tempgeometry FROM " + this.Table + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += this.DefinitionQuery + " AND ";

                strSQL += this.GeometryColumn + " && " + "buffer(" + strGeom + "," + distance.ToString(Map.numberFormat_EnUS) + ")";
                strSQL += " AND distance(" + this.GeometryColumn + ", " + strGeom + ")<" + distance.ToString(Map.numberFormat_EnUS);

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
                {
                    System.Data.DataSet ds = new System.Data.DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        return fdt;
                    }
                    else return null;
                }
            }
        }

        /// <summary>
        /// Returns the features that intersects with 'geom'
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom, FeatureDataSet ds)
        {
            //List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strGeom = "GeomFromText('" + geom.AsText() + "')";
                if (this.SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + this.SRID.ToString() + ")";

                string strSQL = "SELECT * , AsBinary(" + this.GeometryColumn + ") As sharpmap_tempgeometry FROM " + this.Table + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += this.DefinitionQuery + " AND ";

                strSQL += this.GeometryColumn + " && " + strGeom + " AND distance(" + this.GeometryColumn + ", " + strGeom + ")<0";

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);
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
                string strSQL = "SELECT COUNT(*) FROM " + this.Table;

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + this.DefinitionQuery;

                using (PgCommand command = new PgCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int)command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        #region IProvider Members

        private string _defintionQuery;

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }

        /*
		/// <summary>
		/// Gets a collection of columns in the dataset
		/// </summary>
		public System.Data.DataColumnCollection Columns1
		{
			get {
				throw new NotImplementedException();
				//using (PgConnection conn = new PgConnection(this.ConnectionString))
				//{
				//    System.Data.DataColumnCollection columns = new System.Data.DataColumnCollection();
				//    string strSQL = "SELECT column_name, udt_name FROM information_schema.columns WHERE table_name='" + this.Table + "' ORDER BY ordinal_position";
				//    using (PgCommand command = new PgCommand(strSQL, conn))
				//    {
				//        conn.Open();
				//        using (PgDataReader dr = command.ExecuteReader())
				//        {
				//            while (dr.Read())
				//            {
				//                System.Data.DataColumn col = new System.Data.DataColumn((string)dr["column_name"]);
				//                switch((string)dr["udt_name"])
				//                {
				//                    case "int4":
				//                        col.DataType = typeof(Int32);
				//                        break;
				//                    case "int8":
				//                        col.DataType = typeof(Int64);
				//                        break;
				//                    case "varchar":
				//                        col.DataType = typeof(string);
				//                        break;
				//                    case "text":
				//                        col.DataType = typeof(string);
				//                        break;
				//                    case "bool":
				//                        col.DataType = typeof(bool);
				//                        break;
				//                    case "geometry":
				//                        col.DataType = typeof(SharpMap.Geometries.Geometry);
				//                        break;
				//                    default:
				//                        col.DataType = typeof(object);
				//                        break;
				//                }
				//                columns.Add(col);
				//            }
				//        }
				//    }
				//    return columns;
				//}
			}
		}
        */

        private int _srid = -2;

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
                            command.Parameters[0].Value = this._Table;

                            _srid = (int)command.ExecuteScalar();
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
            set
            {
                throw (new ApplicationException("Spatial Reference ID cannot by set on a PostGIS table"));
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
                command.Parameters[0].Value = this._Table;

                object columnname = command.ExecuteScalar();
                conn.Close();

                if (columnname == System.DBNull.Value)
                    throw new ApplicationException("Table '" + this.Table + "' does not contain a geometry column");
                return (string)columnname;
            }
        }


        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns>datarow</returns>
        public SharpMap.Data.FeatureDataRow GetFeature(uint RowID)
        {
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strSQL = String.Format("select * , AsBinary({0}) As sharpmap_tempgeometry from {1} WHERE {2} = '{3}'",
                                              this.GeometryColumn, this.Table, this.ObjectIdColumn, RowID);

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
                {
                    FeatureDataSet ds = new FeatureDataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow dr = ds.Tables[0].Rows[0];
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
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

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public SharpMap.Geometries.BoundingBox GetExtents()
        {
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {
                string strSQL = String.Format("SELECT EXTENT({0}) FROM {1}",
                                              this.GeometryColumn,
                                              this.Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + this.DefinitionQuery;


                strSQL += ";";

                using (PgCommand command = new PgCommand(strSQL, conn))
                {
                    conn.Open();

                    SharpMap.Geometries.BoundingBox bbox = null;
                    try
                    {
                        PostgreSql.Data.PgTypes.PgBox2D result = (PostgreSql.Data.PgTypes.PgBox2D)command.ExecuteScalar();
                        bbox = new SharpMap.Geometries.BoundingBox(result.LowerLeft.X, result.LowerLeft.Y, result.UpperRight.X, result.UpperRight.Y);
                    }
                    catch (System.Exception ex)
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
        [Obsolete("Use ExecuteIntersectionQuery")]
        public void GetFeaturesInView(SharpMap.Geometries.BoundingBox bbox, SharpMap.Data.FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox bbox, SharpMap.Data.FeatureDataSet ds)
        {
            using (PgConnection conn = new PgConnection(_ConnectionString))
            {

                string strBbox = GetBoundingBoxSql(bbox, this.SRID);

                string strSQL = String.Format("SELECT *, AsBinary({0}) AS sharpmap_tempgeometry FROM {1} WHERE ",
                                              this.GeometryColumn,
                                              this.Table);

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += this.DefinitionQuery + " AND ";

                strSQL += this.GeometryColumn + " && " + strBbox;

                using (PgDataAdapter adapter = new PgDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    System.Data.DataSet ds2 = new System.Data.DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
                        {
                            SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != this.GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);
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
        private static string GetBoundingBoxSql(SharpMap.Geometries.BoundingBox bbox, int iSRID)
        {
            string strBbox = String.Format("box2d('BOX3D({0} {1},{2} {3})'::box3d)",
                                bbox.Min.X.ToString(SharpMap.Map.numberFormat_EnUS),
                                bbox.Min.Y.ToString(SharpMap.Map.numberFormat_EnUS),
                                bbox.Max.X.ToString(SharpMap.Map.numberFormat_EnUS),
                                bbox.Max.Y.ToString(SharpMap.Map.numberFormat_EnUS));

            if (iSRID > 0)
                strBbox = String.Format(SharpMap.Map.numberFormat_EnUS, "SetSRID({0},{1})", strBbox, iSRID);

            return strBbox;
        }

        #endregion
    }
}
