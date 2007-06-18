// Copyright 2006 - Humberto Ferreira
// Oracle provider by Humberto Ferreira (humbertojdf@hotmail.com)
//
// Date 2006-09-05
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
using System.Text;
using Oracle.DataAccess.Client;


namespace SharpMap.Data.Providers
{
	/// <summary>
	/// Oracle dataprovider
	/// </summary>
	/// <remarks>
	/// <para>This provider needs the Oracle software client installed on the PC where the application runs.
	/// If you need to connect to an Oracle database, it has to have oracle client (or Oracle Instant Client) installed. </para>
	/// <para>You can download Oracle Client here:
	/// http://www.oracle.com/technology/software/index.html</para>
	/// <para>If client don4t need an instance of Oracle, a better option is to use Oracle Instant client
	/// http://www.oracle.com/technology/tech/oci/instantclient/index.html</para>
	/// <example>
	/// Adding a datasource to a layer:
	/// <code lang="C#">
	/// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
	///	string ConnStr = "Server=127.0.0.1;Port=5432;User Id=userid;Password=password;Database=myGisDb;";
	/// myLayer.DataSource = new SharpMap.Data.Providers.Oracle(ConnStr, "myTable", "GeomColumn", "OidColumn");
	/// </code>
	/// </example>
	/// <para>SharpMap Oracle provider by Humberto Ferreira (humbertojdf at hotmail com).</para>
	/// </remarks>
	[Serializable]
	public class Oracle : SharpMap.Data.Providers.IProvider, IDisposable
	{
		/// <summary>
		/// Initializes a new connection to Oracle
		/// </summary>
		/// <param name="ConnectionStr">Connectionstring</param>
		/// <param name="tablename">Name of data table</param>
		/// <param name="geometryColumnName">Name of geometry column</param>
		/// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
		public Oracle(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
		{
			this.ConnectionString = ConnectionStr;
			this.Table = tablename;
			this.GeometryColumn = geometryColumnName;
			this.ObjectIdColumn = OID_ColumnName;
		}

        /// <summary>
		/// Initializes a new connection to Oracle
		/// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="datasource">Datasoure</param>
        /// <param name="tablename">Tablename</param>
        /// <param name="geometryColumnName">Geometry column name</param>
        /// <param name="OID_ColumnName">Object ID column</param>
        public Oracle(string username, string password, string datasource, string tablename, string geometryColumnName, string OID_ColumnName)
        : this("User Id=" + username + ";Password=" + password + ";Data Source=" + datasource, tablename, geometryColumnName, OID_ColumnName)
        {
            
        }


		/// <summary>
		/// Initializes a new connection to Oracle
		/// </summary>
		/// <param name="ConnectionStr">Connectionstring</param>
		/// <param name="tablename">Name of data table</param>
		/// <param name="OID_ColumnName">Name of column with unique identifier</param>
		public Oracle(string ConnectionStr, string tablename, string OID_ColumnName) : this(ConnectionStr,tablename,"",OID_ColumnName)
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
			//Don't really do anything. oracle's ConnectionPooling takes over here
			_IsOpen = true;
		}
		/// <summary>
		/// Closes the datasource
		/// </summary>
		public void Close()
		{
			//Don't really do anything. oracle's ConnectionPooling takes over here
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
		~Oracle()
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
			using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

				//string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom ";
				string strSQL = "SELECT g." + this.GeometryColumn +".Get_WKB() ";
				strSQL += " FROM " + this.Table + " g WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += strBbox;

				using (OracleCommand command = new OracleCommand(strSQL, conn))
				{
					conn.Open();
					using (OracleDataReader dr = command.ExecuteReader())
					{						
						while (dr.Read())
						{
							if (dr[0] != DBNull.Value)
							{
								SharpMap.Geometries.Geometry geom = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])dr[0]);                               
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
		public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
		{
			SharpMap.Geometries.Geometry geom = null;
			using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
                string strSQL = "SELECT g." + this.GeometryColumn + ".Get_WKB() FROM " + this.Table + " g WHERE " + this.ObjectIdColumn + "='" + oid.ToString() + "'";
				conn.Open();
				using (OracleCommand command = new OracleCommand(strSQL, conn))
				{
					using (OracleDataReader dr = command.ExecuteReader())
					{
						while (dr.Read())
						{
							if (dr[0] != DBNull.Value)
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
			using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{

                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

				string strSQL = "SELECT g." + this.ObjectIdColumn + " ";
				strSQL += "FROM " + this.Table + " g WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += strBbox;                

				using (OracleCommand command = new OracleCommand(strSQL, conn))
				{
					conn.Open();
					using (OracleDataReader dr = command.ExecuteReader())
					{
						while (dr.Read())
						{
							if (dr[0] != DBNull.Value)
							{
                                uint ID = (uint)(decimal)dr[0];
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
        /// Returns the box filter string needed in SQL query
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        private string GetBoxFilterStr(SharpMap.Geometries.BoundingBox bbox) {
            string strBbox = "SDO_FILTER(g." + this.GeometryColumn + ", mdsys.sdo_geometry(2003,#SRID#,NULL," +
                                   "mdsys.sdo_elem_info_array(1,1003,3)," +
                                   "mdsys.sdo_ordinate_array(" +
                                   bbox.Min.X.ToString(SharpMap.Map.numberFormat_EnUS) + ", " +
                                   bbox.Min.Y.ToString(SharpMap.Map.numberFormat_EnUS) + ", " +
                                   bbox.Max.X.ToString(SharpMap.Map.numberFormat_EnUS) + ", " +
                                   bbox.Max.Y.ToString(SharpMap.Map.numberFormat_EnUS) + ")), " +
                                   "'querytype=window') = 'TRUE'";

            if (this.SRID > 0) {
                strBbox = strBbox.Replace("#SRID#", this.SRID.ToString(Map.numberFormat_EnUS));
            } else {
                strBbox = strBbox.Replace("#SRID#", "NULL");
            }
            return strBbox;
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

            //List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
			using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
				string strGeom = "MDSYS.SDO_GEOMETRY('" + geom.AsText() + "', #SRID#)";

                if (this.SRID > 0) {
                    strGeom = strGeom.Replace("#SRID#", this.SRID.ToString(Map.numberFormat_EnUS));
                } else {
                    strGeom = strGeom.Replace("#SRID#", "NULL");
                }
                
                strGeom = "SDO_WITHIN_DISTANCE(g." + this.GeometryColumn + ", " + strGeom + ", 'distance = " + distance.ToString(Map.numberFormat_EnUS) + "') = 'TRUE'";

                string strSQL = "SELECT g.* , g." + this.GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry FROM " + this.Table + " g WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

                strSQL += strGeom;

				using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
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
								fdt.Columns.Add(col.ColumnName,col.DataType,col.Expression);
						foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
						{
							SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
							foreach(System.Data.DataColumn col in ds.Tables[0].Columns)
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
			List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
			using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
				
                string strGeom = "MDSYS.SDO_GEOMETRY('" + geom.AsText() + "', #SRID#)";

                if (this.SRID > 0) {
                    strGeom = strGeom.Replace("#SRID#", this.SRID.ToString(Map.numberFormat_EnUS));
                } else {
                    strGeom = strGeom.Replace("#SRID#", "NULL");
                }

                strGeom = "SDO_RELATE(g." + this.GeometryColumn + ", " + strGeom + ", 'mask=ANYINTERACT querytype=WINDOW') = 'TRUE'";

                string strSQL = "SELECT g.* , g." + this.GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry FROM " + this.Table + " g WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += strGeom;

				using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
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
		/// Convert WellKnownText to linestrings
		/// </summary>
		/// <param name="WKT"></param>
		/// <returns></returns>
		private SharpMap.Geometries.LineString WktToLineString(string WKT)
		{
			SharpMap.Geometries.LineString line = new SharpMap.Geometries.LineString();
			WKT = WKT.Substring(WKT.LastIndexOf('(') + 1).Split(')')[0];
			string[] strPoints = WKT.Split(',');
			foreach (string strPoint in strPoints)
			{
				string[] coord = strPoint.Split(' ');
				line.Vertices.Add(new SharpMap.Geometries.Point(double.Parse(coord[0], SharpMap.Map.numberFormat_EnUS), double.Parse(coord[1], SharpMap.Map.numberFormat_EnUS)));
			}
			return line;
		}

		/// <summary>
		/// Returns the number of features in the dataset
		/// </summary>
		/// <returns>number of features</returns>
		public int GetFeatureCount()
		{
			int count = 0;
			using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
				string strSQL = "SELECT COUNT(*) FROM " + this.Table;
				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;
				using (OracleCommand command = new OracleCommand(strSQL, conn))
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

		private int _srid=-2;

		/// <summary>
		/// Spacial Reference ID
		/// </summary>
		public int SRID
		{
			get {
				if (_srid == -2)
				{
					string strSQL = "select SRID from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + this.Table + "'";
                    
					using (OracleConnection conn = new OracleConnection(_ConnectionString))
					{
                        using (OracleCommand command = new OracleCommand(strSQL, conn))
						{
							try
							{								
								conn.Open();
								_srid = (int)(decimal)command.ExecuteScalar();
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
			set {
				throw (new ApplicationException("Spatial Reference ID cannot by set on a Oracle table"));
			}
		}

		/// <summary>
		/// Queries the Oracle database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
		/// </summary>
		/// <remarks></remarks>
		/// <returns>Name of column containing geometry</returns>
		private string GetGeometryColumn()
		{
            string strSQL = "select COLUMN_NAME from USER_SDO_GEOM_METADATA WHERE TABLE_NAME='" + this.Table + "'";
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            using (OracleCommand command = new OracleCommand(strSQL, conn))
				{
					conn.Open();
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
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
                string strSQL = "select g.* , g." + this.GeometryColumn + ").Get_WKB() As sharpmap_tempgeometry from " + this.Table + " g WHERE " + this.ObjectIdColumn + "='" + RowID.ToString() + "'";
                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
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
						if(ds.Tables[0].Rows.Count>0)
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
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
                string strSQL = "SELECT SDO_AGGR_MBR(g." + this.GeometryColumn + ").Get_WKT() FROM " + this.Table + " g ";
				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;
                using (OracleCommand command = new OracleCommand(strSQL, conn))
				{
					conn.Open();
					object result = command.ExecuteScalar();
					conn.Close();
					if (result == System.DBNull.Value)
						return null;
					string strBox = (string)result;
					if (strBox.StartsWith("POLYGON", StringComparison.InvariantCultureIgnoreCase))
					{
                        strBox = strBox.Replace("POLYGON", "");                    
                        strBox = strBox.Trim();
                        strBox = strBox.Replace("(", "");
                        strBox = strBox.Replace(")", "");

                        List<double> xX = new List<double>();
                        List<double> yY = new List<double>();

                        String[] points = strBox.Split(',');
                        String[] nums;
                        string point;

                        foreach (string s in points)
	                    {
                            point = s.Trim();
                            nums = point.Split(' ');
                            xX.Add(double.Parse(nums[0], SharpMap.Map.numberFormat_EnUS));
                            yY.Add(double.Parse(nums[1], SharpMap.Map.numberFormat_EnUS));
	                    }

                        double minX = Double.MaxValue;
                        double minY = Double.MaxValue;
                        double maxX = Double.MinValue;
                        double maxY = Double.MinValue;

                        foreach (double d in xX) {
                            if(d > maxX){
                                maxX = d;
                            }
                            if (d < minX) {
                                minX = d;
                            }
                        }

                        foreach (double d in yY) {
                            if (d > maxY) {
                                maxY = d;
                            }
                            if (d < minY) {
                                minY = d;
                            }
                        }

						return new SharpMap.Geometries.BoundingBox(minX, minY, maxX, maxY);
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

		#endregion

		#region IProvider Members

		/// <summary>
		/// Returns all features with the view box
		/// </summary>
		/// <param name="bbox">view box</param>
		/// <param name="ds">FeatureDataSet to fill data into</param>
		[Obsolete("Use ExecuteIntersectionQuery(box) instead")]
		public void GetFeaturesInView(SharpMap.Geometries.BoundingBox bbox, SharpMap.Data.FeatureDataSet ds)
		{
			GetFeaturesInView(bbox, ds);
		}

		/// <summary>
		/// Returns all features with the view box
		/// </summary>
		/// <param name="bbox">view box</param>
		/// <param name="ds">FeatureDataSet to fill data into</param>
		public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox bbox, SharpMap.Data.FeatureDataSet ds)
		{
			List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
			{
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                string strSQL = "SELECT g.*, g." + this.GeometryColumn + ".Get_WKB() AS sharpmap_tempgeometry ";
				strSQL += "FROM " + this.Table + " g WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += strBbox;

                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
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
								fdt.Columns.Add(col.ColumnName,col.DataType,col.Expression);
						foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
						{
							SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
							foreach(System.Data.DataColumn col in ds2.Tables[0].Columns)
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
	}
}

