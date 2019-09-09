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

using Common.Logging;
using GeoAPI.Geometries;
using SharpMap.Converters.SqlServer2008SpatialObjects;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Text;
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
        static readonly ILog _logger = LogManager.GetLogger(typeof(SqlServer2008Ex));

        private static bool _isSqlTypesLoaded;
        private static readonly object _loadLock = new object();
        
        /// <summary>
        /// Ensure SqlServerTypes (x32 or x64) are loaded at runtime
        /// </summary>
        static  SqlServer2008Ex()
        {
            if (!_isSqlTypesLoaded)
                lock(_loadLock)
                    if (!_isSqlTypesLoaded)
                    {
                        try
                        {
                            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
                            _isSqlTypesLoaded = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
        }

        /// <summary>
        /// placeholder method to be called by related classes causing static ctor to load types
        /// </summary>
        public static void LoadSqlServerTypes() {}
        
        /// <summary>
        /// Always <code>true</code>. Both queries and client-side conversion will always attempt to repair invalid spatial objects 
        /// </summary>
        public override bool ValidateGeometries
        {
            get => true;
            set
            {
                if (value != true)
                    throw new ArgumentOutOfRangeException("SqlServer2008Ex converters ALWAYS attempt to repair invalid spatial objects");
            }
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server for <see cref="SqlServerSpatialObjectType"/>.Geometry with default <see cref="SqlServer2008ExtentsMode" />    
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="geometryColumnName">Name of geometry column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        [Obsolete]
        public SqlServer2008Ex(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
            : base(connectionStr, tablename, geometryColumnName, oidColumnName, SqlServerSpatialObjectType.Geometry)
        {
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server with default <see cref="SqlServer2008ExtentsMode" /> 
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="spatialColumnName">Name of spatial column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        /// <param name="spatialObjectType">spatial type (Geometry or Geography)</param>
        [Obsolete]
        public SqlServer2008Ex(string connectionStr, string tablename, string spatialColumnName, string oidColumnName,
            SqlServerSpatialObjectType spatialObjectType)
            : base(connectionStr, tablename, spatialColumnName, oidColumnName, spatialObjectType)
        {
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="spatialColumnName">Name of spatial column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        /// <param name="spatialObjectType">spatial type (Geometry or Geography)</param>
        /// <param name="srid">Spatial Reference ID</param>
        /// <param name="extentsMode">method for reading data extents. Geography does not support SqlServer2008ExtentsMode::SpatialIndex</param>
        public SqlServer2008Ex(string connectionStr, string tablename, string spatialColumnName, string oidColumnName,
            SqlServerSpatialObjectType spatialObjectType, int srid, SqlServer2008ExtentsMode extentsMode)
            : base(connectionStr, tablename, spatialColumnName, oidColumnName, spatialObjectType, srid, extentsMode)
        {
        }

        /// <summary>   
        /// Returns geometries within the specified bounding box   
        /// </summary>   
        /// <param name="bbox"></param>   
        /// <returns></returns>   
        public override Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            var features = new Collection<Geometry>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                var sb = new StringBuilder($"SELECT {GeometryColumn}{GetMakeValidString()} AS {GeometryColumn} " +
                                           $"FROM {QualifiedTable} {BuildTableHints()} WHERE ");

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    sb.Append($"{DefinitionQuery} AND ");

                if (!ValidateGeometries ||
                    (SpatialObjectType == SqlServerSpatialObjectType.Geometry && (ForceSeekHint || !string.IsNullOrEmpty(ForceIndex))))
                    // Geometry sensitive to invalid geometries, and BuildTableHints (ForceSeekHint, ForceIndex) do not suppport .MakeValid() in GetBoxFilterStr
                    sb.Append($"{GeometryColumn}.STIsValid() = 1 AND ");

                sb.Append($"{GetBoxFilterStr(bbox)} {GetExtraOptions()}");

                if (_logger.IsDebugEnabled) _logger.DebugFormat("GetGeometriesInView {0}", sb.ToString());

                using (var command = new SqlCommand(sb.ToString(), conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != null && dr[0] != DBNull.Value)
                            {
                                var geom = SqlBytesToGeometry(dr[0]);
                                if (geom != null)
                                    features.Add(geom);
                            }
                        }
                    }
                }
            }
            return features;
        }

        /// <summary>   
        /// Returns the geometry corresponding to the Object ID   
        /// </summary>   
        /// <param name="oid">Object ID</param>   
        /// <returns>geometry</returns>   
        public override Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (var conn = new SqlConnection(ConnectionString))
            {
                string strSql = $"SELECT {GeometryColumn}{GetMakeValidString()} AS {GeometryColumn} " +
                                $"FROM {QualifiedTable} WHERE {ObjectIdColumn} = {oid}";

                if (_logger.IsDebugEnabled) _logger.DebugFormat("GetGeometryByID {0}", strSql);

                using (var command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != null && dr[0] != DBNull.Value)
                                geom = SqlBytesToGeometry(dr[0]);
                        }
                    }
                }
            }
            return geom;
        }

        /// <summary>   
        /// Returns the features that intersects with 'geom'   
        /// </summary>   
        /// <param name="geom"></param>   
        /// <param name="fds">FeatureDataSet to fill data into</param>   
        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet fds)
        {
            if (SpatialObjectType == SqlServerSpatialObjectType.Geography)
            {
                // Define Ring with Clockwise orientation, to be reoriented in query
                var maxExentsPoly = Factory.CreatePolygon(new Coordinate[] {
                            GeogMaxExtents.BottomLeft(), GeogMaxExtents.TopLeft(),
                            GeogMaxExtents.TopRight(), GeogMaxExtents.BottomRight(),
                            GeogMaxExtents.BottomLeft()});
                geom = geom.Intersection(maxExentsPoly);
            }

            var sb = new StringBuilder($"SELECT {GetAttributeColumnNames()}, {GeometryColumn}{GetMakeValidString()} AS {GeometryColumn} " +
                                       $"FROM {QualifiedTable} {BuildTableHints()} WHERE ");

            if (!ValidateGeometries ||
                (SpatialObjectType == SqlServerSpatialObjectType.Geometry && (ForceSeekHint || !string.IsNullOrEmpty(ForceIndex))))
                // Geometry sensitive to invalid geometries, and BuildTableHints (ForceSeekHint, ForceIndex) do not suppport .MakeValid() in GetBoxFilterStr
                sb.Append($"{GeometryColumn}.STIsValid() = 1 AND ");

            if (!String.IsNullOrEmpty(DefinitionQuery))
                sb.Append($"{DefinitionQuery} AND ");

            // .MakeValid() in WHERE clause is not compatible certain BuildHints, resulting in error:
            // The query processor could not produce a query plan for a query with a spatial index hint.  Reason: Could not find required binary spatial method in a condition.  Try removing the index hints or removing SET FORCEPLAN.
            var makeValid = (ForceSeekHint || !string.IsNullOrEmpty(ForceIndex)) ? "" : GetMakeValidString(); //".MakeValid()"

            sb.Append($"{GeometryColumn}{makeValid}.STIntersects({_spatialTypeString}::STGeomFromText('{geom.AsText()}', {SRID}){_reorientObject})=1 {GetExtraOptions()}");

            if (_logger.IsDebugEnabled) _logger.DebugFormat("OnExecuteIntersectionQuery {0}", sb.ToString());

            ExecuteIntersectionQuery(sb.ToString(), fds);
        }



        /// <summary>   
        /// Returns a datarow based on a RowID   
        /// </summary>   
        /// <param name="rowId"></param>   
        /// <returns>datarow</returns>   
        public override FeatureDataRow GetFeature(uint rowId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var strSql = $"SELECT {GetAttributeColumnNames()}, {GeometryColumn}{GetMakeValidString()} AS {GeometryColumn} " +
                             $"FROM {QualifiedTable} WHERE {ObjectIdColumn}={rowId}";

                if (_logger.IsDebugEnabled) _logger.DebugFormat("GetFeature {0}", strSql);

                using (var adapter = new SqlDataAdapter(strSql, conn))
                {
                    var ds = new System.Data.DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = SqlBytesToGeometry(dr[GeometryColumn]);
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
        /// <param name="fds">FeatureDataSet to fill data into</param>   
        public override void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet fds)
        {
            var sb = new StringBuilder($"SELECT {GetAttributeColumnNames()}, {GeometryColumn}{GetMakeValidString()} AS {GeometryColumn} " +
                                       $"FROM {QualifiedTable} {BuildTableHints()} WHERE ");

            if (!String.IsNullOrEmpty(DefinitionQuery))
                sb.Append($"{DefinitionQuery} AND ");

            if (!ValidateGeometries ||
                (SpatialObjectType == SqlServerSpatialObjectType.Geometry && (ForceSeekHint || !string.IsNullOrEmpty(ForceIndex))))
                // Geometry sensitive to invalid geometries, and BuildTableHints (ForceSeekHint, ForceIndex) do not suppport .MakeValid() in GetBoxFilterStr
                sb.Append($"{GeometryColumn}.STIsValid() = 1 AND ");

            sb.Append($"{GetBoxFilterStr(bbox)} {GetExtraOptions()}");

            if (_logger.IsDebugEnabled) _logger.DebugFormat("ExecuteIntersectionQuery {0}", sb.ToString());

            ExecuteIntersectionQuery(sb.ToString(), fds);
        }

        /// <summary>
        /// Select records from database and return FeatureDataSet with SharpMap geometries
        /// </summary>
        protected override void ExecuteIntersectionQuery(string sql, FeatureDataSet fds)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var adapter = new SqlDataAdapter(sql, conn))
                {
                    var ds = new System.Data.DataSet();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        var fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);

                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn)
                                    fdr[col.ColumnName] = dr[col];

                            fdr.Geometry = SqlBytesToGeometry(dr[GeometryColumn]);
                            fdt.AddRow(fdr);
                        }
                        fds.Tables.Add(fdt);
                    }
                }
            }
        }

        private Geometry SqlBytesToGeometry(Object sqlBytes)
        {
            Geometry geom = null;
            if (sqlBytes != null && sqlBytes != DBNull.Value)
                if (SpatialObjectType == SqlServerSpatialObjectType.Geometry)
                    geom = SqlGeometryConverter.ToSharpMapGeometry((Microsoft.SqlServer.Types.SqlGeometry)sqlBytes, Factory);
                else
                    geom = SqlGeographyConverter.ToSharpMapGeometry((Microsoft.SqlServer.Types.SqlGeography)sqlBytes, Factory);
            return geom;
        }

    }
}
