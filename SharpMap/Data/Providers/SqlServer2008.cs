// Copyright 2008 - William Dollins   
// SQL Server 2008 by William Dollins (dollins.bill@gmail.com)   
// Based on Oracle provider by Humberto Ferreira (humbertojdf@gmail.com)   
//   
// Date 2007-11-28   
//   
// This file is part of    
// is free software; you can redistribute it and/or modify   
// it under the terms of the GNU Lesser General Public License as published by   
// the Free Software Foundation; either version 2 of the License, or   
// (at your option) any later version.   
//   
// is distributed in the hope that it will be useful,   
// but WITHOUT ANY WARRANTY; without even the implied warranty of   
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the   
// GNU Lesser General Public License for more details.   
  
// You should have received a copy of the GNU Lesser General Public License   
// along with  if not, write to the Free Software   
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA    
  
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using SharpMap.Geometries;

  
namespace SharpMap.Data.Providers   
{   
    /// <summary>
    /// Possible spatial object types on SqlServer
    /// </summary>
    public enum SqlServerSpatialObjectType
    {
        /// <summary>
        /// Geometry
        /// </summary>
        Geometry,
        /// <summary>
        /// Geography
        /// </summary>
        Geography,
    }
    
    /// <summary>   
    /// SQL Server 2008 data provider   
    /// </summary>   
    /// <remarks>   
    /// <para>This provider was developed against the SQL Server 2008 November CTP. The platform may change significantly before release.</para>   
    /// <example>   
    /// Adding a datasource to a layer:   
    /// <code lang="C#">   
    /// Layers.VectorLayer myLayer = new Layers.VectorLayer("My layer");   
    /// string ConnStr = "Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=myDB;Data Source=myServer\myInstance";   
    /// myLayer.DataSource = new Data.Providers.Katmai(ConnStr, "myTable", "GeomColumn", "OidColumn");   
    /// </code>   
    /// </example>   
    /// <para>SQL Server 2008 provider by Bill Dollins (dollins.bill@gmail.com). Based on the Oracle provider written by Humberto Ferreira.</para>   
    /// </remarks>   
    [Serializable]   
    public class SqlServer2008 : IProvider   
    {   
        /// <summary>   
        /// Initializes a new connection to SQL Server   
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="geometryColumnName">Name of geometry column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        public SqlServer2008(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
            :this(connectionStr, tablename, geometryColumnName, oidColumnName, SqlServerSpatialObjectType.Geometry)
        {
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server   
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="geometryColumnName">Name of geometry column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        /// <param name="spatialObjectType">The type of the spatial object to use for spatial queries</param>
        public SqlServer2008(string connectionStr, string tablename, string geometryColumnName, string oidColumnName, SqlServerSpatialObjectType spatialObjectType)   
        {   
            //Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=ztTest;Data Source=<server>\<instance>   
            ConnectionString = connectionStr;   
            Table = tablename;   
            GeometryColumn = geometryColumnName;   
            ObjectIdColumn = oidColumnName;
            _spatialObjectType = spatialObjectType;
            switch (spatialObjectType)
            {
                case SqlServerSpatialObjectType.Geometry:
                    _spatialObject = "geometry";
                    break;
                //case SqlServerSpatialObjectType.Geography:
                default:
                    _spatialObject = "geography";
                    break;
            }
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server   
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        public SqlServer2008(string connectionStr, string tablename, string oidColumnName)
            : this(connectionStr, tablename, "shape", oidColumnName, SqlServerSpatialObjectType.Geometry)
        {
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server   
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>
        /// <param name="spatialObjectType">The type of the spatial object to use for spatial queries</param>
        public SqlServer2008(string connectionStr, string tablename, string oidColumnName, SqlServerSpatialObjectType spatialObjectType)
            : this(connectionStr,tablename,"shape",oidColumnName, spatialObjectType)
        {
        }

        private bool _isOpen;   
  
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
            //Don't really do anything.   
            _isOpen = true;   
        }   
        /// <summary>   
        /// Closes the datasource   
        /// </summary>   
        public void Close()   
        {   
            //Don't really do anything.   
           _isOpen = false;   
       }  

       #region Disposers and finalizers   
       private bool _disposed;   
 
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
           if (!_disposed)   
           {   
               if (disposing)   
               {   
                   //Close();   
               }   
               _disposed = true;   
           }   
       }   
 
       /// <summary>   
       /// Finalizer   
       /// </summary>   
       ~SqlServer2008()   
       {   
           Dispose();   
       }  
       #endregion   
 
       private string _connectionString;   
 
       /// <summary>   
       /// Connectionstring   
       /// </summary>   
       public string ConnectionString   
       {   
           get { return _connectionString; }   
           set { _connectionString = value; }   
       }   
 
       private string _table;   
 
       /// <summary>   
       /// Data table name   
       /// </summary>   
       public string Table   
       {   
           get { return _table; }   
           set { _table = value; }   
       }   
 
       private string _geometryColumn;   
 
       /// <summary>   
       /// Name of geometry column   
       /// </summary>   
       public string GeometryColumn   
       {   
           get { return _geometryColumn; }   
           set { _geometryColumn = value; }   
       }   
 
       private string _objectIdColumn;   
 
       /// <summary>   
       /// Name of column that contains the Object ID   
       /// </summary>   
       public string ObjectIdColumn   
       {   
           get { return _objectIdColumn; }   
           set { _objectIdColumn = value; }   
       }

        private bool _makeValid;
        /// <summary>
        /// Gets/Sets whether all <see cref="SharpMap.Geometries"/> passed to SqlServer2008 should me made valid using this function.
        /// </summary>
        public Boolean ValidateGeometries { get { return _makeValid; } set { _makeValid = value; } }

        private String MakeValidString
        {
            get { return _makeValid ? ".MakeValid()" : String.Empty; }
        }

        private readonly SqlServerSpatialObjectType _spatialObjectType;
        private readonly string _spatialObject;
        
        /// <summary>
        /// Spatial object type for  
        /// </summary>
        public SqlServerSpatialObjectType SpatialObjectType
        {
            get { return _spatialObjectType; }
        }


        /// <summary>   
       /// Returns geometries within the specified bounding box   
       /// </summary>   
       /// <param name="bbox"></param>   
       /// <returns></returns>   
       public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)   
       {   
           Collection<Geometry> features = new Collection<Geometry>();   
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
               //Get bounding box string   
               string strBbox = GetBoxFilterStr(bbox);   
 
               string strSQL = "SELECT g." + GeometryColumn +".STAsBinary() ";   
               strSQL += " FROM " + Table + " g WHERE ";   
 
               if (!String.IsNullOrEmpty(_defintionQuery))   
                   strSQL += DefinitionQuery + " AND ";   
 
               strSQL += strBbox;   
 
               using (SqlCommand command = new SqlCommand(strSQL, conn))   
               {   
                   conn.Open();   
                   using (SqlDataReader dr = command.ExecuteReader())   
                   {   
                       while (dr.Read())   
                       {   
                           if (dr[0] != DBNull.Value)   
                           {   
                               Geometry geom = Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr[0]);   
                               if(geom!=null)   
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
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
               string strSQL = "SELECT g." + GeometryColumn + ".STAsBinary() FROM " + Table + " g WHERE " + ObjectIdColumn + "='" + oid + "'";   
               conn.Open();   
               using (SqlCommand command = new SqlCommand(strSQL, conn))   
               {   
                   using (SqlDataReader dr = command.ExecuteReader())   
                   {   
                       while (dr.Read())   
                       {   
                           if (dr[0] != DBNull.Value)   
                               geom = Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr[0]);   
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
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
 
               //Get bounding box string   
               string strBbox = GetBoxFilterStr(bbox);   
 
               string strSQL = "SELECT g." + ObjectIdColumn + " ";   
               strSQL += "FROM " + Table + " g WHERE ";   
 
               if (!String.IsNullOrEmpty(_defintionQuery))   
                   strSQL += DefinitionQuery + " AND ";   
 
               strSQL += strBbox;                   
 
               using (SqlCommand command = new SqlCommand(strSQL, conn))   
               {   
                   conn.Open();   
                   using (SqlDataReader dr = command.ExecuteReader())   
                   {   
                       while (dr.Read())   
                       {   
                           if (dr[0] != DBNull.Value)   
                           {   
                               uint id = (uint)(decimal)dr[0];   
                               objectlist.Add(id);   
                           }   
                       }   
                   }   
                   conn.Close();   
               }   
           }   
           return objectlist;   
       }   
 
       /// <summary>   
       /// Returns the box filter string needed in SQL query   
       /// </summary>   
       /// <param name="bbox"></param>   
       /// <returns></returns>   
       private string GetBoxFilterStr(BoundingBox bbox) {   
           //geography::STGeomFromText('LINESTRING(47.656 -122.360, 47.656 -122.343)', 4326);   
           LinearRing lr = new LinearRing();   
           lr.Vertices.Add(new Point(bbox.Left, bbox.Bottom));   
           lr.Vertices.Add(new Point(bbox.Right, bbox.Bottom));   
           lr.Vertices.Add(new Point(bbox.Right, bbox.Top));   
           lr.Vertices.Add(new Point(bbox.Left, bbox.Top));   
           lr.Vertices.Add(new Point(bbox.Left, bbox.Bottom));   
           Polygon p = new Polygon(lr);   
           string bboxText = Converters.WellKnownText.GeometryToWKT.Write(p); // "";   
           //string whereClause = GeometryColumn + ".STIntersects(geometry::STGeomFromText('" + bboxText + "', " + SRID + ")" + MakeValidString + ") = 1";   
           string whereClause = String.Format("{0}{1}.STIntersects({4}::STGeomFromText('{2}', {3})) = 1", 
               GeometryColumn, MakeValidString, bboxText, SRID, _spatialObject);
           return whereClause; // strBbox;   
       }   
 
       /// <summary>   
       /// Returns the features that intersects with 'geom'   
       /// </summary>   
       /// <param name="geom"></param>   
       /// <param name="ds">FeatureDataSet to fill data into</param>   
       public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)   
       {   
           //List<Geometry> features = new List<Geometry>();   
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
               //TODO: Convert to SQL Server   
               string strGeom = _spatialObject + "::STGeomFromText('" + geom.AsText() + "', #SRID#)";

               strGeom = strGeom.Replace("#SRID#", SRID > 0 ? SRID.ToString() : "0");
               strGeom = GeometryColumn + ".STIntersects(" + strGeom + ") = 1";   
 
               string strSQL = "SELECT g.* , g." + GeometryColumn + ").STAsBinary() As sharpmap_tempgeometry FROM " + Table + " g WHERE ";   
 
               if (!String.IsNullOrEmpty(_defintionQuery))   
                   strSQL += DefinitionQuery + " AND ";   
 
               strSQL += strGeom;   
 
               using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))   
               {   
                   conn.Open();   
                   adapter.Fill(ds);   
                   conn.Close();   
                   if (ds.Tables.Count > 0)   
                   {   
                       FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);   
                       foreach (System.Data.DataColumn col in ds.Tables[0].Columns)   
                           if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")   
                               fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);   
                       foreach (System.Data.DataRow dr in ds.Tables[0].Rows)   
                       {   
                           FeatureDataRow fdr = fdt.NewRow();   
                           foreach (System.Data.DataColumn col in ds.Tables[0].Columns)   
                               if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")   
                                   fdr[col.ColumnName] = dr[col];   
                           fdr.Geometry = Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);   
                           fdt.AddRow(fdr);   
                       }   
                       ds.Tables.Add(fdt);   
                   }   
               }   
           }   
       }   
 
       /*
       /// <summary>   
       /// Convert WellKnownText to linestrings   
       /// </summary>   
       /// <param name="wkt"></param>   
       /// <returns></returns>   
       private LineString WktToLineString(string wkt)   
       {   
           LineString line = new LineString();   
           wkt = wkt.Substring(wkt.LastIndexOf('(') + 1).Split(')')[0];   
           string[] strPoints = wkt.Split(',');   
           foreach (string strPoint in strPoints)   
           {   
               string[] coord = strPoint.Split(' ');   
               line.Vertices.Add(new Point(double.Parse(coord[0], Map.NumberFormatEnUs), double.Parse(coord[1], Map.NumberFormatEnUs)));   
           }   
           return line;   
       }
        */
 
       /// <summary>   
       /// Returns the number of features in the dataset   
       /// </summary>   
       /// <returns>number of features</returns>   
       public int GetFeatureCount()   
       {   
           int count;   
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
               string strSQL = "SELECT COUNT(*) FROM " + Table;   
               if (!String.IsNullOrEmpty(_defintionQuery))   
                   strSQL += " WHERE " + DefinitionQuery;   
               using (SqlCommand command = new SqlCommand(strSQL, conn))   
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
 
       /// <summary>   
       /// Gets a collection of columns in the dataset   
       /// </summary>   
       public System.Data.DataColumnCollection Columns   
       {   
           get {   
               throw new NotImplementedException();   
           }   
       }   
 
       private int _srid;   
 
       /// <summary>   
       /// Spacial Reference ID   
       /// </summary>   
       public int SRID   
       {   
           get {   
               return _srid;   
           }   
           set {   
               _srid = value;   
           }   
       }   
 
       /// <summary>   
       /// Returns a datarow based on a RowID   
       /// </summary>   
       /// <param name="rowId"></param>   
       /// <returns>datarow</returns>   
       public FeatureDataRow GetFeature(uint rowId)   
       {   
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
               string strSQL = "select g.* , g." + GeometryColumn + ".STAsBinary() As sharpmap_tempgeometry from " + Table + " g WHERE " + ObjectIdColumn + "=" + rowId + "";   
               using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))   
               {   
                   FeatureDataSet ds = new FeatureDataSet();   
                   conn.Open();   
                   adapter.Fill(ds);   
                   conn.Close();   
                   if (ds.Tables.Count > 0)   
                   {   
                       FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);   
                       foreach (System.Data.DataColumn col in ds.Tables[0].Columns)   
                           if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")   
                               fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);   
                       if(ds.Tables[0].Rows.Count>0)   
                       {   
                           System.Data.DataRow dr = ds.Tables[0].Rows[0];   
                           FeatureDataRow fdr = fdt.NewRow();   
                           foreach (System.Data.DataColumn col in ds.Tables[0].Columns)   
                               if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")   
                                   fdr[col.ColumnName] = dr[col];   
                           fdr.Geometry = Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);   
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
       public BoundingBox GetExtents()   
       {   
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
               //string strSQL = "SELECT g." + GeometryColumn + ".STEnvelope().STAsText() FROM " + Table + " g ";   
               var strSQL = String.Format("SELECT g.{0}{1}.STEnvelope().STAsText() FROM {2} g ",
                   GeometryColumn, MakeValidString, Table);

               if (!String.IsNullOrEmpty(_defintionQuery))   
                   strSQL += " WHERE " + DefinitionQuery;   
               using (SqlCommand command = new SqlCommand(strSQL, conn))   
               {   
                   conn.Open();   
                   //Geometry geom = null;   
                   BoundingBox bx = null;   
                   SqlDataReader dr = command.ExecuteReader();   
                   while (dr.Read())   
                   {   
                       string wkt = dr.GetString(0); //[GeometryColumn];   
                       Geometry g = Converters.WellKnownText.GeometryFromWKT.Parse(wkt);   
                       BoundingBox bb = g.GetBoundingBox();   
                       bx = bx == null ? bb : bx.Join(bb);   
                   }   
                   dr.Close();   
                   conn.Close();   
                   return bx;   
               }   
           }   
       }   
 
       /// <summary>   
       /// Gets the connection ID of the datasource   
       /// </summary>   
       public string ConnectionID   
       {   
           get { return _connectionString; }   
       }  

       #endregion  

       #region IProvider Members   
 
       /// <summary>   
       /// Returns all features with the view box   
       /// </summary>   
       /// <param name="bbox">view box</param>   
       /// <param name="ds">FeatureDataSet to fill data into</param>   
       public void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)   
       {   
           //List<Geometry> features = new List<Geometry>();   
           using (SqlConnection conn = new SqlConnection(_connectionString))   
           {   
               //Get bounding box string   
               string strBbox = GetBoxFilterStr(bbox);   
 
               //string strSQL = "SELECT g.*, g." + GeometryColumn + ".STAsBinary() AS sharpmap_tempgeometry ";   
               string strSQL = String.Format(
                   "SELECT g.*, g.{0}{1}.STAsBinary() AS sharpmap_tempgeometry FROM {2} g WHERE ",
                   GeometryColumn, MakeValidString, Table);
 
               if (!String.IsNullOrEmpty(_defintionQuery))   
                   strSQL += DefinitionQuery + " AND ";   
 
               strSQL += strBbox;   
 
               using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))   
               {   
                   conn.Open();   
                   System.Data.DataSet ds2 = new System.Data.DataSet();   
                   adapter.Fill(ds2);   
                   conn.Close();   
                   if (ds2.Tables.Count > 0)   
                   {   
                       FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);   
                       foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)   
                           if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")   
                               fdt.Columns.Add(col.ColumnName,col.DataType,col.Expression);   
                       foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)   
                       {   
                           FeatureDataRow fdr = fdt.NewRow();   
                           foreach(System.Data.DataColumn col in ds2.Tables[0].Columns)   
                               if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")   
                                   fdr[col.ColumnName] = dr[col];   
                           fdr.Geometry = Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr["sharpmap_tempgeometry"]);   
                           fdt.AddRow(fdr);   
                       }   
                       ds.Tables.Add(fdt);   
                   }   
               }   
           }   
       }  
       #endregion   
   }   
}