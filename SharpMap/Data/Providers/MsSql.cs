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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Converters.WellKnownBinary;

namespace SharpMap.Data.Providers
{
	/// <summary>
	/// Microsoft SQL data provider
	/// </summary>
	/// <remarks>
	/// <para>
	/// The SQL data table MUST contain five data columns: A binary or image column (a Geometry Column) for storing WKB formatted geometries, 
	/// and four real values holding the boundingbox of the geometry. These must be named: Envelope_MinX, Envelope_MinY, Envelope_MaxX and Envelope_MaxY.
	/// Any extra columns will be returns as feature data.
	/// </para>
	/// <para>For creating a valid MS SQL datatable for SharpMap, see <see cref="CreateDataTable"/> 
	/// for creating and uploading a datasource to MS SQL Server.</para>
	/// <example>
	/// Adding a datasource to a layer:
	/// <code lang="C#">
	/// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
	/// string ConnStr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|GeoDatabase.mdf;Integrated Security=True;User Instance=True";
	/// myLayer.DataSource = new SharpMap.Data.Providers.MsSql(ConnStr, "myTable");
	/// </code>
	/// </example>
	/// </remarks>
	[Serializable]
	public class MsSql : IProvider, IDisposable
	{
		/// <summary>
		/// Initializes a new connection to MS Sql Server
		/// </summary>
		/// <param name="ConnectionStr">Connectionstring</param>
		/// <param name="tablename">Name of data table</param>
		/// <param name="geometryColumnName">Name of geometry column</param>
		/// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
		public MsSql(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
		{
			this.ConnectionString = ConnectionStr;
			this.Table = tablename;
			this.GeometryColumn = geometryColumnName; //Name of column to store geometry
			this.ObjectIdColumn = OID_ColumnName; //Name of object ID column
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
		~MsSql()
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
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string BoxIntersect = GetBoxClause(bbox);
				
				string strSQL = "SELECT " + this.GeometryColumn + " AS Geom ";
				strSQL += "FROM " + this.Table + " WHERE ";
				strSQL += BoxIntersect;
				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " AND " + this.DefinitionQuery;

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
		public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
		{
			SharpMap.Geometries.Geometry geom = null;
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT " + this.GeometryColumn + " AS Geom FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + oid.ToString() + "'";
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
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT " + this.ObjectIdColumn + " ";
				strSQL += "FROM " + this.Table + " WHERE ";

				strSQL += GetBoxClause(bbox);

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " AND " + this.DefinitionQuery + " AND ";

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
		/// Returns the features that intersects with 'geom' [NOT IMPLEMENTED]
		/// </summary>
		/// <param name="geom"></param>
		/// <param name="ds">FeatureDataSet to fill data into</param>
		public void ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom, FeatureDataSet ds)
		{
			throw new NotImplementedException();
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
				if (!String.IsNullOrEmpty(_defintionQuery))
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

		private string _defintionQuery;

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
		/// Gets a collection of columns in the dataset
		/// </summary>
		public System.Data.DataColumnCollection Columns
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		private int _srid = -2;

		/// <summary>
		/// Spacial Reference ID
		/// </summary>
		public int SRID
		{
			get { return _srid; }
			set { _srid = value; }
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
				string strSQL = "SELECT *, " + this.GeometryColumn + " AS sharpmap_tempgeometry FROM " + this.Table + " WHERE " + this.ObjectIdColumn + "='" + RowID.ToString() + "'";
				using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
				{
					DataSet ds = new DataSet();
					conn.Open();
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
							if(dr["sharpmap_tempgeometry"] != DBNull.Value)
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
			SharpMap.Geometries.BoundingBox box = null;
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT Min(Envelope_MinX) AS MinX, Min(Envelope_MinY) AS MinY, Max(Envelope_MaxX) AS MaxX, Max(Envelope_MaxY) AS MaxY FROM " + this.Table;
				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " WHERE " + this.DefinitionQuery;
				using (SqlCommand command = new SqlCommand(strSQL, conn))
				{
					conn.Open();
					using (SqlDataReader dr = command.ExecuteReader())
						if (dr.Read())
						{
							box = new SharpMap.Geometries.BoundingBox((float)dr[0], (float)dr[1], (float)dr[2], (float)dr[3]);
						}
					conn.Close();
				}
				return box;
			}
		}

		/// <summary>
		/// Gets the connection ID of the datasource
		/// </summary>
		public string ConnectionID
		{
			get { return _ConnectionString; }
		}

		private string GetBoxClause(SharpMap.Geometries.BoundingBox bbox)
		{
			return String.Format(SharpMap.Map.numberFormat_EnUS,
				"(Envelope_MinX < {0} AND Envelope_MaxX > {1} AND Envelope_MinY < {2} AND Envelope_MaxY > {3})",
				bbox.Max.X, bbox.Min.X, bbox.Max.Y, bbox.Min.Y);
		}

		/// <summary>
		/// Returns all features with the view box
		/// </summary>
		/// <param name="bbox">view box</param>
		/// <param name="ds">FeatureDataSet to fill data into</param>
		public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox bbox, SharpMap.Data.FeatureDataSet ds)
		{
			//List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
			using (SqlConnection conn = new SqlConnection(_ConnectionString))
			{
				string strSQL = "SELECT *, " + this.GeometryColumn + " AS sharpmap_tempgeometry ";
				strSQL += "FROM " + this.Table + " WHERE ";
				strSQL += GetBoxClause(bbox);

				if (!String.IsNullOrEmpty(_defintionQuery))
					strSQL += " AND " + this.DefinitionQuery;

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
		#endregion

		/// <summary>
		/// Creates a new table in a Microsoft SQL Server database and copies rows from an existing datasource.
		/// </summary>
		/// <remarks>
		/// <para>The datatable created will contain six extra columns besides the attribute data: "OID" (Object ID row), 
		/// "WKB_Geometry" (Geometry stored as WKB), and Envelope_MinX, Envelope_MinY, Envelope_MaxX, Envelope_MaxY
		/// for geometry bounding box.</para>
		/// <para>
		/// <example>
		/// Upload a ShapeFile to a database:
		/// <code>
		/// public void CreateDatabase(string shapeFile)
		/// {
		///		if (!System.IO.File.Exists(shapeFile))
		///		{
		///			MessageBox.Show("File not found");
		///			return;
		///		}
		///		ShapeFile shp = new ShapeFile(shapeFile, false);
		///		//Create tablename from filename
		///		string tablename = shapeFile.Substring(shapeFile.LastIndexOf('\\') + 1,
		///			shapeFile.LastIndexOf('.') - shapeFile.LastIndexOf('\\') - 1);
		///		//Create connectionstring
		///		string connstr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|GeoDatabase.mdf;Integrated Security=True;User Instance=True";
		///		int count = SharpMap.Data.Providers.MsSql.CreateDataTable(shp, tablename, connstr);
		///		MessageBox.Show("Uploaded " + count.ToString() + " features to datatable '" + tablename + "'");
		///	}
		/// </code>
		/// </example>
		/// </para>
		/// </remarks>
		/// <param name="datasource">Datasource to upload</param>
		/// <param name="tablename">Name of table to create (existing table will be overwritten!)</param>
		/// <param name="connstr">Connection string to database</param>
		/// <returns>Number or rows inserted, -1 if failed and 0 if table created but no rows inserted.</returns>
		public static int CreateDataTable(SharpMap.Data.Providers.IProvider datasource, string tablename, string connstr)
		{
			datasource.Open();
			FeatureDataRow geom = datasource.GetFeature(0);
			DataColumnCollection columns = geom.Table.Columns;
			int counter = -1;
			using (SqlConnection conn = new SqlConnection(connstr))
			{
				SqlCommand command = new SqlCommand();
				command.Connection = conn;

				conn.Open();
				//Try to drop table if it exists
				try
				{
					command.CommandText = "DROP TABLE \"" + tablename + "\";";
					command.ExecuteNonQuery();
				}
				catch { }
				//Create new table for storing the datasource
				string sql = "CREATE TABLE " + tablename + " (oid INTEGER IDENTITY PRIMARY KEY, WKB_Geometry Image, " +
					"Envelope_MinX real, Envelope_MinY real, Envelope_MaxX real, Envelope_MaxY real";
				foreach (DataColumn col in columns)
					if (col.DataType != typeof(String))
						sql += ", " + col.ColumnName + " " + Type2SqlType(col.DataType).ToString();
					else
						sql += ", " + col.ColumnName + " VARCHAR(256)";
				command.CommandText = sql + ");";
				command.ExecuteNonQuery();
				counter++;
				Collection<uint> indexes = datasource.GetObjectIDsInView(datasource.GetExtents());
				//Select all indexes in shapefile, loop through each feature and insert them one-by-one
				foreach (uint idx in indexes)
				{
					//Get feature from shapefile
					SharpMap.Data.FeatureDataRow feature = datasource.GetFeature(idx);					
					if (counter == 0)
					{

						//Create insert script
						string strSQL = " (";
						foreach (DataColumn col in feature.Table.Columns)
							strSQL += "@" + col.ColumnName + ",";

						strSQL += "@WKB_Geometry,@Envelope_MinX,@Envelope_MinY, " +
									"@Envelope_MaxX,@Envelope_MaxY)";
						strSQL = "INSERT INTO " + tablename + strSQL.Replace("@", "") + " VALUES" + strSQL;

						command.CommandText = strSQL;
						command.Parameters.Clear();
						//Add datacolumn parameters
						foreach (DataColumn col in feature.Table.Columns)
							command.Parameters.Add("@" + col.ColumnName, Type2SqlType(col.DataType));

						//Add geometry parameters
						command.Parameters.Add("@WKB_Geometry", SqlDbType.VarBinary);
						command.Parameters.Add("@Envelope_MinX", SqlDbType.Real);
						command.Parameters.Add("@Envelope_MinY", SqlDbType.Real);
						command.Parameters.Add("@Envelope_MaxX", SqlDbType.Real);
						command.Parameters.Add("@Envelope_MaxY", SqlDbType.Real);
					}
					//Set values
					foreach (DataColumn col in feature.Table.Columns)
						command.Parameters["@" + col.ColumnName].Value = feature[col];
					if (feature.Geometry != null)
					{
						command.Parameters["@WKB_Geometry"].Value = feature.Geometry.AsBinary(); //Add the geometry as Well-Known Binary
						SharpMap.Geometries.BoundingBox box = feature.Geometry.GetBoundingBox();
						command.Parameters["@Envelope_MinX"].Value = box.Left;
						command.Parameters["@Envelope_MinY"].Value = box.Bottom;
						command.Parameters["@Envelope_MaxX"].Value = box.Right;
						command.Parameters["@Envelope_MaxY"].Value = box.Top;
					}
					else
					{
						command.Parameters["@WKB_Geometry"].Value = DBNull.Value;
						command.Parameters["@Envelope_MinX"].Value = DBNull.Value;
						command.Parameters["@Envelope_MinY"].Value = DBNull.Value;
						command.Parameters["@Envelope_MaxX"].Value = DBNull.Value;
						command.Parameters["@Envelope_MaxY"].Value = DBNull.Value;
					}
					//Insert row
					command.ExecuteNonQuery();
					counter++;
				}
				//Create indexes
				command.Parameters.Clear();
				command.CommandText = "CREATE INDEX [IDX_Envelope_MinX] ON " + tablename + " (Envelope_MinX)";
				command.ExecuteNonQuery();
				command.CommandText = "CREATE INDEX [IDX_Envelope_MinY] ON " + tablename + " (Envelope_MinY)";
				command.ExecuteNonQuery();
				command.CommandText = "CREATE INDEX [IDX_Envelope_MaxX] ON " + tablename + " (Envelope_MaxX)";
				command.ExecuteNonQuery();
				command.CommandText = "CREATE INDEX [IDX_Envelope_MaxY] ON " + tablename + " (Envelope_MaxY)";
				command.ExecuteNonQuery();

				conn.Close();
			}
			datasource.Close();
			return counter;
		}
		/// <summary>
		/// Returns the name of the SqlServer datatype based on a .NET datatype
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		private static SqlDbType Type2SqlType(Type t)
		{
			switch (t.ToString())
			{
				case "System.Boolean": return System.Data.SqlDbType.Bit;
				case "System.Single": return System.Data.SqlDbType.Real;
				case "System.Double": return System.Data.SqlDbType.Float;
				case "System.Int16": return System.Data.SqlDbType.SmallInt;
				case "System.Int32": return System.Data.SqlDbType.Int;
				case "System.Int64": return System.Data.SqlDbType.BigInt;
				case "System.DateTime": return System.Data.SqlDbType.DateTime;
				case "System.Byte[]": return System.Data.SqlDbType.Image;
				case "System.String": return System.Data.SqlDbType.VarChar;
				default:
					throw (new NotSupportedException("Unsupported datatype '" + t.Name + "' found in datasource"));
			}
		}
	}
}
