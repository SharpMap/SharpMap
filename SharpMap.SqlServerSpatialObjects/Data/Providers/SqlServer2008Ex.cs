// Copyright 2008 - William Dollins, 2012 - Joris Wit   
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
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using GeoAPI.Features;
using GeoAPI.IO;
using Microsoft.SqlServer.Types;
using NetTopologySuite.IO;
using BoundingBox = GeoAPI.Geometries.Envelope;
using Geometry = GeoAPI.Geometries.IGeometry;

namespace SharpMap.Data.Providers
{
    /// <summary>   
    /// SQL Server 2008 data provider   
    /// </summary>   
    /// <remarks>   
    /// <para>This is a modified version of the <see cref="SqlServer2008"/> provider. It might provide better performance 
    /// because it directly uses the SQL server spatial types, instead of using the STAsBinary method.</para>   
    /// <para>It currently does not support the geography sql server data type.</para>   
    /// <example>   
    /// Adding a datasource to a layer:   
    /// <code lang="C#">   
    /// Layers.VectorLayer myLayer = new Layers.VectorLayer("My layer");   
    /// string ConnStr = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=myDB;Data Source=myServer\myInstance";   
    /// myLayer.DataSource = new Data.Providers.SqlServer2008Ex(ConnStr, "myTable", "GeomColumn", "OidColumn");   
    /// </code>   
    /// </example>   
    /// </remarks>   
    [Serializable]
    public class SqlServer2008Ex : SqlServer2008
    {
        /// <summary>   
        /// Initializes a new connection to SQL Server   
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="geometryColumnName">Name of geometry column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        public SqlServer2008Ex(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
            : base(connectionStr, tablename, geometryColumnName, oidColumnName)
        {
        }

        //private const string SpatialObject = "geometry";

        /// <summary>   
        /// Returns geometries within the specified bounding box   
        /// </summary>   
        /// <param name="bbox"></param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>   
        public override IEnumerable<Geometry> GetGeometriesInView(BoundingBox bbox, CancellationToken? cancellationToken = null)
        {
            var features = new Collection<Geometry>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                //Get bounding box string   
                string strBbox = GetBoxFilterStr(bbox);

                string strSQL = "SELECT g." + GeometryColumn;
                strSQL += " FROM " + Table + " g " + BuildTableHints() + " WHERE ";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                string extraOptions = GetExtraOptions();
                if (!string.IsNullOrEmpty(extraOptions))
                    strSQL += " " + extraOptions;

                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        var spatialReader = CreateReader();
                        
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var geom = spatialReader.Read(dr[0]);
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

        private ISpatialObjectReader CreateReader()
        {
            switch (SpatialObjectType)
            {
                case SqlServerSpatialObjectType.Geography:
                    return new SpatialObjectReader<SqlGeography>( new MsSql2008GeographyReader());
                default:
                    return new SpatialObjectReader<SqlGeometry>(new MsSql2008GeometryReader());
            }
        }
        /// <summary>   
        /// Returns the geometry corresponding to the Object ID   
        /// </summary>   
        /// <param name="oid">Object ID</param>   
        /// <returns>geometry</returns>   
        public override Geometry GetGeometryByOid(object oid)
        {
            Geometry geom = null;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string strSQL = "SELECT g." + GeometryColumn + " FROM " + Table + " g WHERE " + ObjectIdColumn + "='" + oid + "'";
                conn.Open();
                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        var spatialReader = CreateReader();
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = spatialReader.Read(dr[0]);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        /// <summary>   
        /// Returns the features that intersects with 'geom'   
        /// </summary>   
        /// <param name="geom"></param>   
        /// <param name="fcs">FeatureDataSet to fill data into</param>
        /// <param name="cancellationToken">A cancellation token</param>   
        protected override void OnExecuteIntersectionQuery(Geometry geom, IFeatureCollectionSet fcs, CancellationToken? cancellationToken = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string strGeom = SpatialObjectType + "::STGeomFromText('" + geom.AsText() + "', #SRID#)";

                strGeom = strGeom.Replace("#SRID#", SRID > 0 ? SRID.ToString(NumberFormatInfo.InvariantInfo) : "0");
                strGeom = GeometryColumn + ".STIntersects(" + strGeom + ") = 1";

                string strSQL = "SELECT g.* FROM " + Table + " g " + BuildTableHints() + " WHERE ";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strGeom;

                string extraOptions = GetExtraOptions();
                if (!string.IsNullOrEmpty(extraOptions))
                    strSQL += " " + extraOptions;

                var ds = (System.Data.DataSet) new FeatureDataSet();
                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        var geometryReader = CreateReader();
                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = geometryReader.Read(dr[GeometryColumn]);
                            fdt.AddRow(fdr);
                        }
                        fcs.Add(fdt);
                    }
                }
            }
        }

        /// <summary>   
        /// Returns a datarow based on a RowID   
        /// </summary>   
        /// <param name="oid"></param>   
        /// <returns>datarow</returns>   
        public override IFeature GetFeatureByOid(object oid)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string strSQL = "select g.* from " + Table + " g WHERE " + ObjectIdColumn + "=:POid";
                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
                {
                    adapter.SelectCommand.Parameters.Add(new SqlParameter("POid", oid));
                    System.Data.DataSet ds = new System.Data.DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        var geometryReader = CreateReader();
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = geometryReader.Read(dr[GeometryColumn]);
                            return fdr;
                        }
                        return null;
                    }
                    return null;
                }
            }
        }

        /// <summary>   
        /// Returns all features with the view box   
        /// </summary>   
        /// <param name="bbox">view box</param>   
        /// <param name="fcs">FeatureDataSet to fill data into</param>
        /// <param name="cancellationToken">A cancellation token</param>   
        public override void ExecuteIntersectionQuery(BoundingBox bbox, IFeatureCollectionSet fcs, CancellationToken? cancellationToken = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                //Get bounding box string
                string strBbox = GetBoxFilterStr(bbox);

                string strSQL = String.Format(
                    "SELECT g.* FROM {0} g {1} WHERE ",
                    Table, BuildTableHints());

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += strBbox;

                string extraOptions = GetExtraOptions();
                if (!string.IsNullOrEmpty(extraOptions))
                    strSQL += " " + extraOptions;

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
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        var geometryReader = CreateReader();
                        foreach (System.Data.DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = geometryReader.Read(dr[GeometryColumn]);
                            fdt.AddRow(fdr);
                        }
                        fcs.Add(fdt);
                    }
                }
            }
        }

        #region nested utility classes
        private interface ISpatialObjectReader
        {
            Geometry Read(object obj);
        }

        private class SpatialObjectReader<T> : ISpatialObjectReader
        {
            private readonly IGeometryReader<T> _reader;

            public SpatialObjectReader(IGeometryReader<T> reader)
            {
                _reader = reader;
            }

            public Geometry Read(object obj)
            {
                if (obj.GetType() == typeof(T))
                    return _reader.Read((T)obj);
                return null;
            }
        }
        #endregion

    }
}