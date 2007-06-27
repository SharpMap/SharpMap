// Copyright 2006 - Ricardo Stuven (rstuven@gmail.com)
// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
// MsSqlSpatial provider by Ricardo Stuven.
// Based on PostGIS provider by Morten Nielsen.
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
using System.Data.SqlClient;

namespace SharpMap.Data.Providers
{
	/// <summary>
	/// Microsoft SQL Server 2005 / MsSqlSpatial dataprovider
	/// </summary>
	/// <example>
	/// Adding a datasource to a layer:
	/// <code lang="C#">
	/// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
	/// string ConnStr = @"Data Source=localhost\sqlexpress;Initial Catalog=myGisDb;Integrated Security=SSPI;";
	/// myLayer.DataSource = new SharpMap.Data.Providers.MsSqlSpatial(ConnStr, "myTable", "myId");
	/// </code>
	/// </example>
	[Serializable]
	public class MsSqlSpatial : SharpMap.Data.Providers.IProvider, IDisposable
	{
		/// <summary>
		/// Initializes a new connection to MsSqlSpatial
		/// </summary>
		/// <param name="connectionString">Connectionstring</param>
		/// <param name="tableName">Name of data table</param>
		/// <param name="geometryColumnName">Name of geometry column</param>
		/// /// <param name="identifierColumnName">Name of column with unique identifier</param>
		public MsSqlSpatial(string connectionString, string tableName, string geometryColumnName, string identifierColumnName)
		{
			this.ConnectionString = connectionString;
			this.Table = tableName;
			this.GeometryColumn = geometryColumnName;
			this.ObjectIdColumn = identifierColumnName;
		}

		/// <summary>
		/// Initializes a new connection to MsSqlSpatial
		/// </summary>
		/// <param name="ConnectionStr">Connectionstring</param>
		/// <param name="tablename">Name of data table</param>
		/// <param name="OID_ColumnName">Name of column with unique identifier</param>
		public MsSqlSpatial(string connectionString, string tableName, string identifierColumnName)
			: this(connectionString, tableName, "", identifierColumnName)
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
			//Don't really do anything. SqlClient's ConnectionPooling takes over here
			_IsOpen = true;
		}
		/// <summary>
		/// Closes the datasource
		/// </summary>
		public void Close()
		{
			//Don't really do anything. SqlClient's ConnectionPooling takes over here
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
		~MsSqlSpatial()
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

		private string _GeometryExpression = "{0}";
		/// <summary>
		/// Expression template for geometry column evaluation.
		/// </summary>
		/// <example>
		/// You could, for instance, simplify your geometries before they're displayed.
		/// Simplification helps to speed the rendering of big geometries.
		/// Here's a sample code to simplify geometries using 100 meters of threshold.
		/// <code>
		/// datasource.GeometryExpression = "ST.Simplify({0}, 100)";
		/// </code>
		/// Also you could draw a 20 meters buffer around those little points:
		/// <code>
		/// datasource.GeometryExpression = "ST.Buffer({0}, 20)";
		/// </code>
		/// </example>
		public string GeometryExpression
		{
			get { return _GeometryExpression; }
			set { _GeometryExpression = value; }
		}

		private string _FeatureColumns = "*";
		/// <summary>
		/// List of columns or T-SQL expressions separated by comma.
		/// Using "*" (the value by default), all columns are selected.
		/// </summary>
		public string FeatureColumns
		{
			get { return _FeatureColumns; }
			set { _FeatureColumns = value; }
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

		private string _DefinitionQuery = String.Empty;
		/// <summary>
		/// Definition query used for limiting dataset (WHERE clause)
		/// </summary>
		public string DefinitionQuery
		{
			get { return _DefinitionQuery; }
			set { _DefinitionQuery = value; }
		}

		private string _OrderQuery = String.Empty;
		/// <summary>
		/// Columns or T-SQL expressions for sorting (ORDER BY clause)
		/// </summary>
		public string OrderQuery
		{
			get { return _OrderQuery; }
			set { _OrderQuery = value; }
		}


		private int _TargetSRID = -1;
		/// <summary>
		/// The target spatial reference ID (SRID). 
		/// It allows on-the-fly transformations in the server-side.
		/// </summary>
		public int TargetSRID
		{
			get { return _TargetSRID; }
			set { _TargetSRID = value; }
		}

		private string TargetGeometryColumn
		{
			get
			{
				if (this.SRID > 0 && this.TargetSRID > 0 && this.SRID != this.TargetSRID)
					return "ST.Transform(" + this.GeometryColumn + "," + this.TargetSRID + ")";
				else
					return this.GeometryColumn;
			}
		}

		/// <summary>
		/// Returns geometries within the specified bounding box
		/// </summary>
		/// <param name="bbox"></param>
		/// <returns></returns>
		public Collection<Geometries.Geometry> GetGeometriesInView(SharpMap.Geometries.BoundingBox bbox)
		{
			Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT ST.AsBinary(" + this.BuildGeometryExpression() + ") ";
				strSQL += "FROM ST.FilterQuery" + this.BuildSpatialQuerySuffix() + "(" + this.BuildEnvelope(bbox) + ")";

				if (!String.IsNullOrEmpty(this.DefinitionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;

				if (!String.IsNullOrEmpty(this.OrderQuery))
					strSQL += " ORDER BY " + this.OrderQuery;

				using (SqlCommand command = new SqlCommand(strSQL, conn))
				{
					conn.Open();
					using (SqlDataReader dr = command.ExecuteReader())
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
			using (SqlConnection conn = new SqlConnection(this.ConnectionString))
			{
				string strSQL = "SELECT ST.AsBinary(" + this.BuildGeometryExpression() + ") AS Geom FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + oid.ToString() + "'";
				conn.Open();
				using (SqlCommand command = new SqlCommand(strSQL, conn))
				{
					using (SqlDataReader dr = command.ExecuteReader())
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
			using (SqlConnection conn = new SqlConnection(this.ConnectionString))
			{
				string strSQL = "SELECT * FROM ST.FilterQuery('" + this.Table + "', '" + this.GeometryColumn + "', " + this.BuildEnvelope(bbox) + ")";

				if (!String.IsNullOrEmpty(this.DefinitionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;

				if (!String.IsNullOrEmpty(this.OrderQuery))
					strSQL += " ORDER BY " + this.OrderQuery;

				using (SqlCommand command = new SqlCommand(strSQL, conn))
				{
					conn.Open();
					using (SqlDataReader dr = command.ExecuteReader())
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
			//List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
			using (SqlConnection conn = new SqlConnection(this.ConnectionString))
			{
				string strGeom;
				if (this.TargetSRID > 0 && this.SRID > 0 && this.SRID != this.TargetSRID)
					strGeom = "ST.Transform(ST.GeomFromText('" + geom.AsText() + "'," + this.TargetSRID.ToString() + ")," + this.SRID.ToString() + ")";
				else
					strGeom = "ST.GeomFromText('" + geom.AsText() + "', " + this.SRID.ToString() + ")";

				string strSQL = "SELECT " + this.FeatureColumns + ", ST.AsBinary(" + this.BuildGeometryExpression() + ") As sharpmap_tempgeometry ";
				strSQL += "FROM ST.IsWithinDistanceQuery" + this.BuildSpatialQuerySuffix() + "(" + strGeom + ", " + distance.ToString(Map.numberFormat_EnUS) + ")";

				if (!String.IsNullOrEmpty(this.DefinitionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;

				if (!String.IsNullOrEmpty(this.OrderQuery))
					strSQL += " ORDER BY " + this.OrderQuery;

				using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
				{
					System.Data.DataSet ds = new System.Data.DataSet();
					conn.Open();
					adapter.Fill(ds);
					conn.Close();
					if (ds.Tables.Count > 0)
					{
						FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
						foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
							if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
								fdt.Columns.Add(col.ColumnName,col.DataType,col.Expression);
						foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
						{
							SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
							foreach(System.Data.DataColumn col in ds.Tables[0].Columns)
								if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
									fdr[col.ColumnName] = dr[col];
							if (dr["sharpmap_tempgeometry"] != DBNull.Value)
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
			using (SqlConnection conn = new SqlConnection(this.ConnectionString))
			{
				string strGeom;
				if (this.TargetSRID > 0 && this.SRID > 0 && this.SRID != this.TargetSRID)
					strGeom = "ST.Transform(ST.GeomFromText('" + geom.AsText() + "'," + this.TargetSRID.ToString() + ")," + this.SRID.ToString() + ")";
				else
					strGeom = "ST.GeomFromText('" + geom.AsText() + "', " + this.SRID.ToString() + ")";

				string strSQL = "SELECT " + this.FeatureColumns + ", ST.AsBinary(" + this.BuildGeometryExpression() + ") As sharpmap_tempgeometry ";
				strSQL += "FROM ST.RelateQuery" + this.BuildSpatialQuerySuffix() + "(" + strGeom + ", 'intersects')";

				if (!String.IsNullOrEmpty(this.DefinitionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;

				if (!String.IsNullOrEmpty(this.OrderQuery))
					strSQL += " ORDER BY " + this.OrderQuery;

				using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
				{
					conn.Open();
					adapter.Fill(ds);
					conn.Close();
					if (ds.Tables.Count > 0)
					{
						FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
						foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
							if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
								fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
						foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
						{
							SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
							foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
								if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
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

		/// <summary>
		/// Returns the number of features in the dataset
		/// </summary>
		/// <returns>number of features</returns>
		public int GetFeatureCount()
		{
			int count = 0;
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT COUNT(*) FROM " + this.Table;
				if (!String.IsNullOrEmpty(this.DefinitionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;
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

		/// <summary>
		/// Gets a collection of columns in the dataset
		/// </summary>
		public System.Data.DataColumnCollection Columns
		{
			get {
				throw new NotImplementedException();
				//using (SqlConnection conn = new SqlConnection(this.ConnectionString))
				//{
				//    System.Data.DataColumnCollection columns = new System.Data.DataColumnCollection();
				//    string strSQL = "SELECT column_name, udt_name FROM information_schema.columns WHERE table_name='" + this.Table + "' ORDER BY ordinal_position";
				//    using (SqlCommand command = new SqlCommand(strSQL, conn))
				//    {
				//        conn.Open();
				//        using (SqlDataReader dr = command.ExecuteReader())
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
						strSQL = "select SRID from ST.GEOMETRY_COLUMNS WHERE F_TABLE_NAME='" + this.Table + "'";
					else
					{
						string schema = this.Table.Substring(0, dotPos);
						string table = this.Table.Substring(dotPos + 1);
						strSQL = "select SRID from ST.GEOMETRY_COLUMNS WHERE F_TABLE_SCHEMA='" + schema + "' AND F_TABLE_NAME='" + table + "'";
					}

					using (SqlConnection conn = new SqlConnection(_ConnectionString))
					{
						using (SqlCommand command = new SqlCommand(strSQL, conn))
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
			set
			{
				// SRID can be set in order to support views.
				_srid = value;
			}
		}

		/// <summary>
		/// Queries the MsSqlSpatial database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
		/// </summary>
		/// <remarks></remarks>
		/// <returns>Name of column containing geometry</returns>
		private string GetGeometryColumn()
		{
			string strSQL = "select F_GEOMETRY_COLUMN from ST.GEOMETRY_COLUMNS WHERE F_TABLE_NAME='" + this.Table + "'";
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
				using (SqlCommand command = new SqlCommand(strSQL, conn))
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
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "select " + this.FeatureColumns + ", ST.AsBinary(" + this.BuildGeometryExpression() + ") As sharpmap_tempgeometry from " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + RowID.ToString() + "'";
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
							if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
								fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
						if(ds.Tables[0].Rows.Count>0)
						{
							System.Data.DataRow dr = ds.Tables[0].Rows[0];
							SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
							foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
								if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
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

		/// <summary>
		/// Boundingbox of dataset
		/// </summary>
		/// <returns>boundingbox</returns>
		public SharpMap.Geometries.BoundingBox GetExtents()
		{
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = string.Format("SELECT ST.AsBinary(ST.EnvelopeQueryWhere('{0}', '{1}', '{2}'))", this.Table, this.GeometryColumn, this.DefinitionQuery.Replace("'", "''"));
				using (SqlCommand command = new SqlCommand(strSQL, conn))
				{
					conn.Open();
					object result = command.ExecuteScalar();
					conn.Close();
					if (result == System.DBNull.Value)
						return null;
					SharpMap.Geometries.BoundingBox bbox = SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse((byte[])result).GetBoundingBox();
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
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT " + this.FeatureColumns + ", ST.AsBinary(" + this.BuildGeometryExpression() + ") AS sharpmap_tempgeometry ";
				strSQL += "FROM ST.FilterQuery" + this.BuildSpatialQuerySuffix() + "(" + this.BuildEnvelope(bbox) + ")";

				if (!String.IsNullOrEmpty(this.DefinitionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;

				if (!String.IsNullOrEmpty(this.OrderQuery))
					strSQL += " ORDER BY " + this.OrderQuery;

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
							if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
								fdt.Columns.Add(col.ColumnName,col.DataType,col.Expression);
						foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
						{
							SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();
							foreach(System.Data.DataColumn col in ds2.Tables[0].Columns)
								if (col.ColumnName != this.GeometryColumn && !col.ColumnName.StartsWith(this.GeometryColumn + "_Envelope_") && col.ColumnName != "sharpmap_tempgeometry")
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
		#endregion

		private string BuildSpatialQuerySuffix()
		{
			string schema;
			string table = this.Table;
			int dotPosition = table.IndexOf('.');
			if (dotPosition == -1)
			{
				schema = "dbo";
			}
			else
			{
				schema = table.Substring(0, dotPosition);
				table = table.Substring(dotPosition + 1);
			}
			return "#" + schema + "#" + table + "#" + this.GeometryColumn;
		}

		private string BuildGeometryExpression()
		{
			return string.Format(this.GeometryExpression, this.TargetGeometryColumn);
		}

		private string BuildEnvelope(SharpMap.Geometries.BoundingBox bbox)
		{
			if (this.TargetSRID > 0 && this.SRID > 0 && this.SRID != this.TargetSRID)
				return string.Format(SharpMap.Map.numberFormat_EnUS,
					"ST.Transform(ST.MakeEnvelope({0},{1},{2},{3},{4}),{5})",
						bbox.Min.X,
						bbox.Min.Y,
						bbox.Max.X,
						bbox.Max.Y,
						this.TargetSRID,
						this.SRID);
			else
				return string.Format(SharpMap.Map.numberFormat_EnUS, 
					"ST.MakeEnvelope({0},{1},{2},{3},{4})",
					bbox.Min.X,
					bbox.Min.Y,
					bbox.Max.X,
					bbox.Max.Y,
					this.SRID);
		}
	}
}

