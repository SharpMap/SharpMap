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
using System.Text;

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
	public class PostGIS : SharpMap.Data.Providers.IProvider, IDisposable
	{
		/// <summary>
		/// Initializes a new connection to PostGIS
		/// </summary>
		/// <param name="ConnectionStr">Connectionstring</param>
		/// <param name="tablename">Name of data table</param>
		/// <param name="geometryColumnName">Name of geometry column</param>
		/// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
		public PostGIS(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
		{
			this.ConnectionString = ConnectionStr;
			this.Table = tablename;
			this.GeometryColumn = geometryColumnName;
			this.ObjectIdColumn = OID_ColumnName;
		}

		/// <summary>
		/// Initializes a new connection to PostGIS
		/// </summary>
		/// <param name="ConnectionStr">Connectionstring</param>
		/// <param name="tablename">Name of data table</param>
		/// <param name="OID_ColumnName">Name of column with unique identifier</param>
		public PostGIS(string ConnectionStr, string tablename, string OID_ColumnName) : this(ConnectionStr,tablename,"",OID_ColumnName)
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
		~PostGIS()
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
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strBbox = "box2d('BOX3D(" +
							bbox.Min.X.ToString(SharpMap.Map.numberFormat_EnUS) + " " +
							bbox.Min.Y.ToString(SharpMap.Map.numberFormat_EnUS) + "," +
							bbox.Max.X.ToString(SharpMap.Map.numberFormat_EnUS) + " " +
							bbox.Max.Y.ToString(SharpMap.Map.numberFormat_EnUS) + ")'::box3d)";
				if (this.SRID > 0)
					strBbox = "setSRID(" + strBbox + "," + this.SRID.ToString(Map.numberFormat_EnUS) + ")";

				string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom ";
				strSQL += "FROM " + this.Table + " WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += this.GeometryColumn + " && " + strBbox;

				using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
				{
					conn.Open();
					using (Npgsql.NpgsqlDataReader dr = command.ExecuteReader())
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
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT AsBinary(" + this.GeometryColumn + ") AS Geom FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + oid.ToString() + "'";
				conn.Open();
				using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
				{
					using (Npgsql.NpgsqlDataReader dr = command.ExecuteReader())
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
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strBbox = "box2d('BOX3D(" +
							bbox.Min.X.ToString(SharpMap.Map.numberFormat_EnUS) + " " +
							bbox.Min.Y.ToString(SharpMap.Map.numberFormat_EnUS) + "," +
							bbox.Max.X.ToString(SharpMap.Map.numberFormat_EnUS) + " " +
							bbox.Max.Y.ToString(SharpMap.Map.numberFormat_EnUS) + ")'::box3d)";
				if (this.SRID > 0)
					strBbox = "setSRID(" + strBbox + "," + this.SRID.ToString(Map.numberFormat_EnUS) + ")";

				string strSQL = "SELECT " + this.ObjectIdColumn + " ";
				strSQL += "FROM " + this.Table + " WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += this.GeometryColumn + " && " + strBbox;

				using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
				{
					conn.Open();
					using (Npgsql.NpgsqlDataReader dr = command.ExecuteReader())
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
			//Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strGeom = "GeomFromText('" + geom.AsText() + "')";
				if (this.SRID > 0)
					strGeom = "setSRID(" + strGeom + "," + this.SRID.ToString() + ")";

				string strSQL = "SELECT * , AsBinary(" + this.GeometryColumn + ") As sharpmap_tempgeometry FROM " + this.Table + " WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += this.GeometryColumn + " && " + "buffer(" + strGeom + "," + distance.ToString(Map.numberFormat_EnUS) + ")";
				strSQL += " AND distance(" + this.GeometryColumn + ", " + strGeom + ")<" + distance.ToString(Map.numberFormat_EnUS);

				using (Npgsql.NpgsqlDataAdapter adapter = new Npgsql.NpgsqlDataAdapter(strSQL, conn))
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
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strGeom = "GeomFromText('" + geom.AsText() + "')";
				if (this.SRID > 0)
					strGeom = "setSRID(" + strGeom + "," + this.SRID.ToString() + ")";

				string strSQL = "SELECT * , AsBinary(" + this.GeometryColumn + ") As sharpmap_tempgeometry FROM " + this.Table + " WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += this.GeometryColumn + " && " + strGeom + " AND distance(" + this.GeometryColumn + ", " + strGeom + ")<0";

				using (Npgsql.NpgsqlDataAdapter adapter = new Npgsql.NpgsqlDataAdapter(strSQL, conn))
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
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT COUNT(*) FROM " + this.Table;
				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;
				using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
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
				//using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(this.ConnectionString))
				//{
				//    System.Data.DataColumnCollection columns = new System.Data.DataColumnCollection();
				//    string strSQL = "SELECT column_name, udt_name FROM information_schema.columns WHERE table_name='" + this.Table + "' ORDER BY ordinal_position";
				//    using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
				//    {
				//        conn.Open();
				//        using (Npgsql.NpgsqlDataReader dr = command.ExecuteReader())
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

		private int _srid=-2;

		/// <summary>
		/// Spacial Reference ID
		/// </summary>
		public int SRID
		{
			get {
				if (_srid == -2)
				{
					int dotPos = this.Table.IndexOf(".");
					string strSQL = "";
					if (dotPos == -1)
						strSQL = "select srid from geometry_columns WHERE f_table_name='" + this.Table + "'";
					else
					{
						string schema = this.Table.Substring(0, dotPos);
						string table = this.Table.Substring(dotPos + 1);
						strSQL = "select srid from geometry_columns WHERE f_table_schema='" + schema + "' AND f_table_name='" + table + "'";
					}

					using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
					{
						using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
						{
							try
							{
								conn.Open();
								_srid = (int)command.ExecuteScalar();
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
			string strSQL = "select f_geometry_column from geometry_columns WHERE f_table_name='" + this.Table + "'";
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
				using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
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
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strSQL = "select * , AsBinary(" + this.GeometryColumn + ") As sharpmap_tempgeometry from " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + RowID.ToString() + "'";
				using (Npgsql.NpgsqlDataAdapter adapter = new Npgsql.NpgsqlDataAdapter(strSQL, conn))
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
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT EXTENT(" + this.GeometryColumn + ") FROM " + this.Table;
				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;
				using (Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(strSQL, conn))
				{
					conn.Open();
					object result = command.ExecuteScalar();
					conn.Close();
					if (result == System.DBNull.Value)
						return null;
					string strBox = (string)result;					
					if (strBox.StartsWith("BOX("))
					{
						string[] vals = strBox.Substring(4, strBox.IndexOf(")")-4).Split(new char[2] { ',', ' ' });
						return new SharpMap.Geometries.BoundingBox(
							double.Parse(vals[0], SharpMap.Map.numberFormat_EnUS),
							double.Parse(vals[1], SharpMap.Map.numberFormat_EnUS),
							double.Parse(vals[2], SharpMap.Map.numberFormat_EnUS),
							double.Parse(vals[3], SharpMap.Map.numberFormat_EnUS));
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
			List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
			using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(_ConnectionString))
			{
				string strBbox = "box2d('BOX3D(" +
							bbox.Min.X.ToString(SharpMap.Map.numberFormat_EnUS) + " " +
							bbox.Min.Y.ToString(SharpMap.Map.numberFormat_EnUS) + "," +
							bbox.Max.X.ToString(SharpMap.Map.numberFormat_EnUS) + " " +
							bbox.Max.Y.ToString(SharpMap.Map.numberFormat_EnUS) + ")'::box3d)";
				if (this.SRID > 0)
					strBbox = "setSRID(" + strBbox + "," + this.SRID.ToString(Map.numberFormat_EnUS) + ")";

				string strSQL = "SELECT *, AsBinary(" + this.GeometryColumn + ") AS sharpmap_tempgeometry ";
				strSQL += "FROM " + this.Table + " WHERE ";

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += this.DefinitionQuery + " AND ";

				strSQL += this.GeometryColumn + " && " + strBbox;

				using (Npgsql.NpgsqlDataAdapter adapter = new Npgsql.NpgsqlDataAdapter(strSQL, conn))
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

