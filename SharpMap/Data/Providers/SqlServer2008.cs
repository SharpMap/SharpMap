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

// Note - Supports both Geometry AND Geography types for SQL Server 2008 onwards. 
// The '2008' suffix in the class name is to distinguish from SharpMap.Data.Providers.MsSqlSpatial provider (Sql Server 2005).
// SqlServer2008 requests WKB from the database (hence will work with Sql Server 2008, 2012, 2016 etc), 
// and WKB is then parsed to an IGeometry instance using NetTopologySuite.IO.WkbReader
//
// Alternatively, to work with native Sql Spatial types, see SharpMap.SqlServerSpatialObjects which requests
// raw spatial bytes from the database and uses Microsoft.SqlServer.Types to convert Sql bytes on the client.
//
// By default, the provider ignores invalid spatial objects. This behaviour can be changed by setting 
// SqlServer2008.ValidateGeometries = True BUT take note of comments on this property.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using GeoAPI.Geometries;
using Common.Logging;
using GeoAPI;

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
    /// Method used to determine extents of all features
    /// </summary>
    public enum SqlServer2008ExtentsMode
    {
        /// <summary>
        /// Client retrieves and reads through all features in the table to determine extents
        /// </summary>
        QueryIndividualFeatures,

        /// <summary>
        /// Supported by <see cref="SqlServerSpatialObjectType" />.Geometry only, reading the bounds of the spatial index directly from the system tables (very fast, but does not take into account actual data extents or <see cref="SqlServer2008.DefinitionQuery"/>)
        /// </summary>
        SpatialIndex,

        /// <summary>
        /// Uses Aggregate functions introduced in SQL Server 2012 (recommended, server aggregates data and returns single record to client)
        /// </summary>
        EnvelopeAggregate
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
    /// string ConnStr = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=myDB;Data Source=myServer\myInstance";   
    /// myLayer.DataSource = new Data.Providers.SqlServer2008(ConnStr, "myTable", "GeomColumn", "OidColumn");   
    /// </code>   
    /// </example>   
    /// <para>SQL Server 2008 provider by Bill Dollins (dollins.bill@gmail.com). Based on the Oracle provider written by Humberto Ferreira.</para>   
    /// </remarks>   
    [Serializable]
    public class SqlServer2008 : BaseProvider
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(SqlServer2008));

        // column name used in queries for retrieving spatial column as WKB
        private const string SharpMapWkb = "sharpmapwkb";

        // required for restricting extents of WKT (eg bbox) used to query SqlGeography to work around error 
        // 24206 "The specified input cannot be accepted because it contains an edge with antipodal points"
        // FullGlobe will be used when map extents exceed this envelope.
        protected static readonly Envelope GeogMaxExtents = new Envelope(-179.999999999, 180.0, -89.999999000, 89.999999000);

        // SqlGeography : polygon interior defined by left hand/foot rule (anti-clockwise orientation)
        // SqlGeometry  : orientation is irrelevant
        // GeometryToWKT returns Envelope with clockwise ring, so need to call .ReorientObject() for WKT used to query SqlGeography
        protected readonly string _reorientObject;

        // used for static spatial methods in SQL string
        protected readonly string _spatialTypeString;

        private SqlServer2008ExtentsMode _extentsMode;

        // List of columns EXCLUDING the spatial column eg: [Id], [Name], [Geom4326] >> [Id], [Name]
        // _attributeColumnNames is used when  feature "attributes" should be returned (eg OnExecuteIntersectionQuery, GetFeature). 
        // The Spatial column should NOT be retrieved directly without reference to Microsoft.SqlServerTypes,
        // as it will cause DataAdaptor.Fill() to throw an error when attempting to determine type for spatial column.
        private string _attributeColumnNames;

        /// <summary>   
        /// Data table schema   
        /// </summary>   
        public string TableSchema { get; private set; }

        /// <summary>   
        /// Data table name   
        /// </summary>   
        public string Table { get; private set; }

        /// <summary>
        /// Gets a value indicating the qualified schema table name in square brackets
        /// </summary>
        protected string QualifiedTable { get; private set; }

        /// <summary>   
        /// Name of column that contains the Object ID   
        /// </summary>   
        public string ObjectIdColumn { get; private set; }

        /// <summary>   
        /// Name of geometry column   
        /// </summary>   
        public string GeometryColumn { get; private set; }

        /// <summary>
        /// Spatial object type for  
        /// </summary>
        public SqlServerSpatialObjectType SpatialObjectType { get; private set; }

        /// <summary>
        /// When <c>true</c>, attempts to repair invalid SqlServer spatial objects by appending .MakeValid() in SQL statements. 
        /// Ignored for <see cref="SqlServerSpatialObjectType"/>.Geometry 
        /// when <see cref="ForceSeekHint"/> or <see cref="ForceIndex"/> is enabled due to SQL Server query execution plan.
        /// </summary>
        public virtual Boolean ValidateGeometries { get; set; }

        /// <summary>
        /// When <code>true</code>, uses the FORCESEEK table hint, possibly over-riding <see cref="ValidateGeometries"/>. 
        /// </summary>   
        public bool ForceSeekHint { get; set; }

        /// <summary>
        /// When <code>true</code>, uses the NOLOCK table hint.
        /// </summary>   
        public bool NoLockHint { get; set; }

        /// <summary>
        /// When set, forces use of the specified index, possibly over-riding <see cref="ValidateGeometries"/>. 
        /// </summary>   
        public string ForceIndex { get; set; }

        /// <summary>
        /// If set, sends an Option MaxDop to the SQL-Server to override the Parallel Execution of indexes
        /// This can be used if Spatial indexes are not used on SQL-Servers with many processors.
        /// 
        /// MaxDop = 0 // Default behaviour
        /// MaxDop = 1 // Suppress Parallel execution of Queryplan
        /// MaxDop = [2..n] // Use X cores in in execution plan
        /// </summary>
        public int MaxDop { get; set; }

        /// <summary>   
        /// Initializes a new connection to SQL Server for <see cref="SqlServerSpatialObjectType"/>.Geometry in column named SHAPE with default <see cref="ExtentsMode" /> 
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        [Obsolete]
        public SqlServer2008(string connectionStr, string tablename, string oidColumnName)
            : this(connectionStr, tablename, "SHAPE", oidColumnName, SqlServerSpatialObjectType.Geometry)
        {
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server for spatial column named SHAPE with default <see cref="ExtentsMode" /> 
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>
        /// <param name="spatialObjectType">The type of the spatial object to use for spatial queries</param>
        [Obsolete]
        public SqlServer2008(string connectionStr, string tablename, string oidColumnName,
            SqlServerSpatialObjectType spatialObjectType)
            : this(connectionStr, tablename, "SHAPE", oidColumnName, spatialObjectType)
        {
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server for <see cref="SqlServerSpatialObjectType"/>.Geometry with default <see cref="ExtentsMode" />
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="geometryColumnName">Name of geometry column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        [Obsolete]
        public SqlServer2008(string connectionStr, string tablename, string geometryColumnName, string oidColumnName)
            : this(connectionStr, tablename, geometryColumnName, oidColumnName, SqlServerSpatialObjectType.Geometry)
        {
        }

        /// <summary>   
        /// Initializes a new connection to SQL Server with default <see cref="ExtentsMode" />
        /// </summary>   
        /// <param name="connectionStr">Connectionstring</param>   
        /// <param name="tablename">Name of data table</param>   
        /// <param name="spatialColumnName">Name of spatial column</param>   
        /// <param name="oidColumnName">Name of column with unique identifier</param>   
        /// <param name="spatialObjectType">spatial type (Geometry or Geography)</param>
        [Obsolete]
        public SqlServer2008(string connectionStr, string tablename, string spatialColumnName, string oidColumnName,
            SqlServerSpatialObjectType spatialObjectType)
            : this(connectionStr, tablename, spatialColumnName, oidColumnName, spatialObjectType, false)
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
        /// <param name="useSpatialIndexExtentAsExtent">True sets ExtentsMode to SqlServer2008ExtentsMode.SpatialIndex</param>
        [Obsolete]
        public SqlServer2008(string connectionStr, string tablename, string spatialColumnName, string oidColumnName,
            SqlServerSpatialObjectType spatialObjectType, bool useSpatialIndexExtentAsExtent)
            : this(
                connectionStr, tablename, spatialColumnName, oidColumnName, spatialObjectType,
                useSpatialIndexExtentAsExtent, 0)
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
        /// <param name="useSpatialIndexExtentAsExtent">True sets ExtentsMode to SqlServer2008ExtentsMode.SpatialIndex</param>
        /// <param name="srid">The spatial reference id</param>
        [Obsolete]
        public SqlServer2008(string connectionStr, string tablename, string spatialColumnName, string oidColumnName,
            SqlServerSpatialObjectType spatialObjectType, bool useSpatialIndexExtentAsExtent, int srid)
             : this(
                connectionStr, tablename, spatialColumnName, oidColumnName, spatialObjectType, srid,
                (useSpatialIndexExtentAsExtent ?
                   SqlServer2008ExtentsMode.SpatialIndex :
                   SqlServer2008ExtentsMode.QueryIndividualFeatures))
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
        /// <param name="srid">The spatial reference id</param>
        /// <param name="extentsMode">Mode for calculating full extents of the data</param>
        public SqlServer2008(string connectionStr, string tablename, string spatialColumnName, string oidColumnName,
            SqlServerSpatialObjectType spatialObjectType, int srid, SqlServer2008ExtentsMode extentsMode)
        {
            ConnectionString = connectionStr;

            ParseTablename(tablename);

            GeometryColumn = spatialColumnName;
            ObjectIdColumn = oidColumnName;
            SpatialObjectType = spatialObjectType;
            switch (spatialObjectType)
            {
                case SqlServerSpatialObjectType.Geometry:
                    _spatialTypeString = "geometry";
                    _reorientObject = string.Empty;
                    break;

                //case SqlServerSpatialObjectType.Geography:
                default:
                    _spatialTypeString = "geography";
                    _reorientObject = ".ReorientObject()";
                    break;
            }

            SRID = srid;

            ExtentsMode = extentsMode;

            if (!string.IsNullOrEmpty(TableSchema))
                QualifiedTable = $"[{TableSchema}].[{Table}]";
            else
                QualifiedTable = $"[{Table}]";

        }

        /// <summary>
        /// Gets or sets the method used in the <see cref="GetExtents"/> method.
        /// </summary>
        public SqlServer2008ExtentsMode ExtentsMode
        {
            get => _extentsMode;
            set
            {
                if (SpatialObjectType == SqlServerSpatialObjectType.Geography && value == SqlServer2008ExtentsMode.SpatialIndex)
                    throw new ArgumentOutOfRangeException("ExtentsMode", "Geography type does not support extents by Spatial Index");

                _extentsMode = value;
            }
        }

        /// <summary>   
        /// Connectionstring   
        /// </summary>   
        public string ConnectionString
        {
            get { return ConnectionID; }
            set { ConnectionID = value; }
        }

        protected string GetMakeValidString()
        { return ValidateGeometries ? ".MakeValid()" : String.Empty; }

        /// <summary>
        /// Method to parse TableSchema and Table from a (fully qualified) tablename
        /// </summary>
        /// <param name="tablename">The table name</param>
        protected void ParseTablename(string tablename)
        {
            bool open = false;
            var sb = new StringBuilder(tablename.Length);
            var lastChar = char.MinValue;

            foreach (var c in tablename)
            {
                switch (c)
                {
                    case '[':
                        if (open)
                            throw new ArgumentException("tablename");
                        open = true;
                        break;
                    case ']':
                        if (!open)
                            throw new ArgumentException("tablename");
                        open = false;
                        break;
                    case '.':
                        if (lastChar == char.MinValue)
                            throw new ArgumentException("tablename");
                        if (open)
                            sb.Append(c);
                        else
                        {
                            if (string.IsNullOrEmpty(TableSchema))
                                TableSchema = sb.ToString();
                            else
                                TableSchema += "." + sb.ToString();

                            sb.Clear();
                        }
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
                lastChar = c;
            }

            if (open)
                throw new ArgumentException("tablename");

            Table = sb.ToString();
        }

        protected string GetAttributeColumnNames()
        {
            if (string.IsNullOrEmpty(_attributeColumnNames))
            {
                // select csv list of OID + attribute columns (each column in square brackets)
                var strSql = "SELECT STUFF (" +
                     $"(SELECT DISTINCT '], [' + name FROM sys.columns WHERE object_id = OBJECT_ID('{QualifiedTable}') " +
                     $"AND name NOT IN ('{GeometryColumn}') " +
                     "FOR XML PATH('')), 1, 2, '') + ']';";

                if (_logger.IsDebugEnabled) _logger.DebugFormat("GetAttributeColumnNames {0}", strSql);

                using (var conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(strSql, conn))
                    {
                        _attributeColumnNames = (string)cmd.ExecuteScalar();
                    }
                }
            }

            return _attributeColumnNames;
        }

        /// <summary>
        /// Function to transform <see cref="MaxDop"/> to sql for the query
        /// </summary>
        /// <returns>MAXDOP option striong</returns>
        protected string GetExtraOptions()
        {
            if (MaxDop != 0)
            {
                return "OPTION (MAXDOP " + MaxDop + ")";
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Builds the WITH clause containing all specified table hints
        /// </summary>
        /// <returns>The WITH clause</returns>
        protected string BuildTableHints()
        {
            if (ForceSeekHint || NoLockHint || !string.IsNullOrEmpty(ForceIndex))
            {
                var hints = new List<string>(3);
                if (!string.IsNullOrEmpty(ForceIndex))
                {
                    hints.Add("INDEX(" + ForceIndex + ")");
                }
                if (NoLockHint)
                {
                    hints.Add("NOLOCK");
                }
                if (ForceSeekHint)
                {
                    hints.Add("FORCESEEK");
                }
                return "WITH (" + string.Join(",", hints.ToArray()) + ")";
            }
            return string.Empty;
        }

        /// <summary>   
        /// Returns geometries within the specified bounding box   
        /// </summary>   
        /// <param name="bbox"></param>   
        /// <returns></returns>   
        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var features = new Collection<IGeometry>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                var sb = new StringBuilder($"SELECT {GeometryColumn}{GetMakeValidString()}.STAsBinary() FROM {QualifiedTable} {BuildTableHints()} WHERE ");

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
                        var wkbReader = new NetTopologySuite.IO.WKBReader(GeometryServiceProvider.Instance);
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                var geom = wkbReader.Read((byte[])dr[0]);
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
        public override IGeometry GetGeometryByID(uint oid)
        {
            IGeometry geom = null;
            using (var conn = new SqlConnection(ConnectionString))
            {
                string strSql = $"SELECT {GeometryColumn}{GetMakeValidString()}.STAsBinary() FROM {QualifiedTable} " +
                                $"WHERE {ObjectIdColumn} = {oid}";

                if (_logger.IsDebugEnabled) _logger.DebugFormat("GetGeometryByID {0}", strSql);

                using (var command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        var wkbReader = new NetTopologySuite.IO.WKBReader(GeometryServiceProvider.Instance);
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = wkbReader.Read((byte[])dr[0]);
                        }
                    }
                }
            }
            return geom;
        }

        /// <summary>   
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'   
        /// </summary>   
        /// <param name="bbox"></param>   
        /// <returns></returns>   
        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var objectlist = new Collection<uint>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                var sb = new StringBuilder($"SELECT {ObjectIdColumn} FROM {QualifiedTable} {BuildTableHints()} WHERE ");

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    sb.Append(DefinitionQuery + " AND ");

                if (!ValidateGeometries ||
                    (SpatialObjectType == SqlServerSpatialObjectType.Geometry && (ForceSeekHint || !string.IsNullOrEmpty(ForceIndex))))
                    // Geometry sensitive to invalid geometries, and BuildTableHints (ForceSeekHint, ForceIndex) do not suppport .MakeValid() in GetBoxFilterStr
                    sb.Append($"{GeometryColumn}.STIsValid() = 1 AND ");
                
                sb.Append($"{GetBoxFilterStr(bbox)} {GetExtraOptions()}");

                if (_logger.IsDebugEnabled) _logger.DebugFormat("GetObjectIDsInView {0}", sb.ToString());

                using (var command = new SqlCommand(sb.ToString(), conn))
                {
                    conn.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint id = Convert.ToUInt32(dr[0]);
                                objectlist.Add(id);
                            }
                        }
                    }
                }
            }
            return objectlist;
        }

        /// <summary>   
        /// Returns the box filter string needed in SQL query   
        /// </summary>   
        /// <param name="bbox"></param>   
        /// <returns></returns>   
        protected string GetBoxFilterStr(Envelope bbox)
        {
            var bboxText = Factory.ToGeometry(bbox).ToString();

            if (SpatialObjectType == SqlServerSpatialObjectType.Geography && !GeogMaxExtents.Contains(bbox))
                // ReorientObject() has no effect on FullGlobe
                bboxText = "FullGlobe";

            // .MakeValid() in WHERE clause is not compatible certain BuildHints, resulting in error:
            // The query processor could not produce a query plan for a query with a spatial index hint.  Reason: Could not find required binary spatial method in a condition.  Try removing the index hints or removing SET FORCEPLAN.
            var makeValid = (ForceSeekHint || !string.IsNullOrEmpty(ForceIndex)) ? "" : GetMakeValidString(); //".MakeValid()"

            // STGeomFromText applicable to both Geometry AND Geography (ie x,y ordinate order) 
            return $"{GeometryColumn}{makeValid}.STIntersects({_spatialTypeString}::STGeomFromText('{bboxText}', {SRID}){_reorientObject})=1";
        }

        /// <summary>   
        /// Returns the features that intersects with 'geom'   
        /// </summary>   
        /// <param name="geom"></param>   
        /// <param name="ds">FeatureDataSet to fill data into</param>   
        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet fds)
        {
            var sb = new StringBuilder($"SELECT {GetAttributeColumnNames()}, {GeometryColumn}{GetMakeValidString()}.STAsBinary() As {SharpMapWkb} " +
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

            var geomText = geom.AsText();

            if (SpatialObjectType == SqlServerSpatialObjectType.Geography && !GeogMaxExtents.Contains(geom.EnvelopeInternal))
                geomText = "FullGlobe";

            sb.Append($"{GeometryColumn}{makeValid}.STIntersects({_spatialTypeString}::STGeomFromText('{geomText}', {SRID}){_reorientObject})=1 {GetExtraOptions()}");

            if (_logger.IsDebugEnabled) _logger.DebugFormat("OnExecuteIntersectionQuery {0}", sb.ToString());

            ExecuteIntersectionQuery(sb.ToString(), fds);
        }

        /// <summary>   
        /// Returns the number of records in the dataset (including NULL, EMPTY, and Invalid geometries)
        /// </summary>   
        /// <returns>number of features</returns>   
        public override int GetFeatureCount()
        {
            int count;
            using (var conn = new SqlConnection(ConnectionString))
            {
                var strSql = $"SELECT COUNT({ObjectIdColumn}) FROM {QualifiedTable}";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSql += $" WHERE {DefinitionQuery}";

                using (var command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    count = (int)command.ExecuteScalar();
                }
            }
            return count;
        }

        #region IProvider Members   

        /// <summary>   
        /// Definition query used for limiting dataset   
        /// </summary>   
        public string DefinitionQuery { get; set; }

        /// <summary>   
        /// Gets a collection of columns in the dataset   
        /// </summary>   
        public System.Data.DataColumnCollection Columns
        {
            get { throw new NotImplementedException(); }
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
                var strSql = $"SELECT {GetAttributeColumnNames()}, {GeometryColumn}{GetMakeValidString()}.STAsBinary() As {SharpMapWkb} " +
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
                            if (col.ColumnName != GeometryColumn && col.ColumnName != SharpMapWkb)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != SharpMapWkb)
                                    fdr[col.ColumnName] = dr[col];

                            if (dr[SharpMapWkb] != null && dr[SharpMapWkb] != DBNull.Value)
                            {
                                var wkbReader = new NetTopologySuite.IO.WKBReader(GeometryServiceProvider.Instance);
                                fdr.Geometry = wkbReader.Read((byte[])dr[SharpMapWkb]);
                            }

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
        public override Envelope GetExtents()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var wkbReader = new NetTopologySuite.IO.WKBReader(GeometryServiceProvider.Instance);
                string sql;
                switch (ExtentsMode)
                {
                    case SqlServer2008ExtentsMode.SpatialIndex:
                        // Applicable to GEOMETRY only. Reads extents of Spatial Index GRID (not feature table) and DefinitionQuery is NOT applied.
                        sql = "SELECT bounding_box_xmin, bounding_box_xmax, bounding_box_ymin, bounding_box_ymax " +
                              "FROM sys.spatial_index_tessellations " +
                              $"WHERE object_id = OBJECT_ID('{QualifiedTable}')";

                        if (_logger.IsDebugEnabled) _logger.DebugFormat("GetExtents {0} {1}", ExtentsMode, sql);

                        using (var command = new SqlCommand(sql, conn))
                        {
                            //Geometry geom = null;   
                            using (var dr = command.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    return new Envelope(
                                        Convert.ToDouble(dr["bounding_box_xmin"]),
                                        Convert.ToDouble(dr["bounding_box_xmax"]),
                                        Convert.ToDouble(dr["bounding_box_ymin"]),
                                        Convert.ToDouble(dr["bounding_box_ymax"]));
                                }
                            }
                        }
                        break;

                    case SqlServer2008ExtentsMode.QueryIndividualFeatures:

                        // MUST call MakeValid regardless of ValidateGeometries
                        if (SpatialObjectType == SqlServerSpatialObjectType.Geometry)
                            // GEOMETRY returns 1 row for each feature
                            sql = $"SELECT {GeometryColumn}.MakeValid().STEnvelope().STAsBinary() FROM {QualifiedTable}";
                        else
                            // GEOGRAPHY returns single row with multi-geometry containing all features
                            sql = $"SELECT {_spatialTypeString}::CollectionAggregate({GeometryColumn}.MakeValid()).STAsBinary() FROM {QualifiedTable}";

                        if (!String.IsNullOrEmpty(DefinitionQuery))
                            sql += $" WHERE {DefinitionQuery}";

                        if (!ValidateGeometries)
                        {
                            // explicitly exclude any Invalid geoms                             
                            sql += String.IsNullOrEmpty(DefinitionQuery) ? " WHERE " : " AND ";
                            sql += $"{GeometryColumn}.STIsValid()=1";
                        }

                        if (_logger.IsDebugEnabled) _logger.DebugFormat("GetExtents {0} {1}", ExtentsMode, sql);

                        using (var command = new SqlCommand(sql, conn))
                        {
                            var bx = new Envelope();
                            using (var dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    if (dr[0] != DBNull.Value)
                                    {
                                        var g = wkbReader.Read((byte[])dr[0]);
                                        bx.ExpandToInclude(g.EnvelopeInternal);
                                    }
                                }
                            }
                            return bx;
                        }

                    case SqlServer2008ExtentsMode.EnvelopeAggregate:

                        if (SpatialObjectType == SqlServerSpatialObjectType.Geometry)
                            // GEOMETRY EnvelopeAggregate returns RECTILINEAR polygon.
                            sql = $"SELECT {_spatialTypeString}::EnvelopeAggregate({GeometryColumn}{GetMakeValidString()}).STAsBinary() FROM {QualifiedTable}";
                        else
                            // GEOGRAPHY EnvelopeAggregate returns CURVED polygon (not supported by SharpMap), 
                            // so use ConvextHullAggregate to return POLYGON and FORCE .MakeValid
                            sql = $"SELECT {_spatialTypeString}::ConvexHullAggregate({GeometryColumn}{GetMakeValidString()}).STAsBinary() FROM {QualifiedTable}";

                        if (!String.IsNullOrEmpty(DefinitionQuery))
                            sql += $" WHERE {DefinitionQuery}";

                        // Note Geometry limitiation - ALWAYS have to filter Invalid geometries (otherwise EnvelopeAggregate will return NULL)
                        if (!ValidateGeometries || SpatialObjectType == SqlServerSpatialObjectType.Geometry)
                        {
                            // explicitly exclude any Invalid geoms                             
                            sql += String.IsNullOrEmpty(DefinitionQuery) ? " WHERE " : " AND ";
                            sql += $"{GeometryColumn}.STIsValid()=1";
                        }

                        if (_logger.IsDebugEnabled) _logger.DebugFormat("GetExtents {0} {1}", ExtentsMode, sql);

                        using (var command = new SqlCommand(sql, conn))
                        {
                            using (var dr = command.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    if (dr[0] != DBNull.Value)
                                    {
                                        var g = wkbReader.Read((byte[])dr[0]);
                                        return g.EnvelopeInternal;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            throw new InvalidOperationException();
        }

        #endregion

        #region IProvider Members   

        /// <summary>   
        /// Returns all features with the view box   
        /// </summary>   
        /// <param name="bbox">view box</param>   
        /// <param name="ds">FeatureDataSet to fill data into</param>   
        public override void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet fds)
        {
            var sb = new StringBuilder($"SELECT {GetAttributeColumnNames()}, {GeometryColumn}{GetMakeValidString()}.STAsBinary() AS {SharpMapWkb} " +
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

        #endregion

        protected virtual void ExecuteIntersectionQuery(string sql, FeatureDataSet fds)
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
                            if (col.ColumnName != GeometryColumn && col.ColumnName != SharpMapWkb)
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);

                        var wkbReader = new NetTopologySuite.IO.WKBReader(GeometryServiceProvider.Instance);
                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (System.Data.DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != SharpMapWkb)
                                    fdr[col.ColumnName] = dr[col];

                            if (dr[SharpMapWkb] != DBNull.Value)
                                fdr.Geometry = wkbReader.Read((byte[])dr[SharpMapWkb]);

                            fdt.AddRow(fdr);
                        }
                        fds.Tables.Add(fdt);
                    }
                }
            }

        }

    }
}
