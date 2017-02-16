using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using ICoordinateSystem = GeoAPI.CoordinateSystems.ICoordinateSystem;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    public enum GpkgOrdinateOption : byte
    {
        Undefined = 0,
        Prohibited,
        Mandatory,
        Optional,
    }
    
    /// <summary>
    /// Class representing the content of a GeoPackage
    /// </summary>
    [Serializable]
    public class GpkgContent
    {
        private readonly string _tableName;
        private readonly string _dataType;
        private readonly string _identifier;
        private readonly string _description;
        private readonly Envelope _extent;
        private readonly DateTime _lastChange;
        private readonly int _srid;
        private readonly string _connectionString;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="rdr">The data record</param>
        /// <param name="connectionString">The connection string</param>
        public GpkgContent(IDataRecord rdr, string connectionString)
        {
            int index;
            _tableName = rdr.IsDBNull(index=rdr.GetOrdinal("table_name")) ? string.Empty : rdr.GetString(index);
            _dataType = rdr.IsDBNull(index=rdr.GetOrdinal("data_type")) ? string.Empty : rdr.GetString(index);
            _identifier = rdr.IsDBNull(index = rdr.GetOrdinal("identifier")) ? string.Empty : rdr.GetString(index);
            _description = rdr.IsDBNull(index = rdr.GetOrdinal("description")) ? string.Empty : rdr.GetString(index);
            _lastChange = rdr.IsDBNull(index = rdr.GetOrdinal("last_change")) ? DateTime.MinValue : rdr.GetDateTime(index);
            _extent = new Envelope(
                rdr.GetDouble(rdr.GetOrdinal("min_x")), rdr.GetDouble(rdr.GetOrdinal("max_x")), 
                rdr.GetDouble(rdr.GetOrdinal("min_y")), rdr.GetDouble(rdr.GetOrdinal("max_y")));
            _srid = rdr.GetInt32(rdr.GetOrdinal("srs_id"));
            _connectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        public string TableName
        {
            get { return _tableName; }
        }

        /// <summary>
        /// Gets the data type, currently 'features' or 'tiles'
        /// </summary>
        public string DataType
        {
            get { return _dataType; }
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public string Description
        {
            get { return _description; }
        }

        public Envelope Extent
        {
            get { return _extent; }
        }

        public DateTime LastChange
        {
            get { return _lastChange; }
        }

        public int SRID
        {
            get { return _srid; }
        }

        internal string ConnectionString { get { return _connectionString; } }

        internal string OidColumn { get; private set; }

        internal string GeometryColumn { get; private set; }

        internal int GeometryType { get; private set; }

        private ICoordinateSystem _spatialReference;

        internal ICoordinateSystem SpatialReference
        {
            get { return _spatialReference ?? (_spatialReference = GetSpatialRefernce()); }
        }

        private ICoordinateSystem GetSpatialRefernce()
        {
            return null;
        }


        internal void GetGeometryColumnDefinition()
        {
            const string sqlSelectColumnData =
                "SELECT * FROM \"gpkg_geometry_columns\" WHERE \"table_name\" = ?;";

            using (var cn = new SQLiteConnection(_connectionString).OpenAndReturn())
            {
                var cmd = new SQLiteCommand(sqlSelectColumnData, cn);
                cmd.Parameters.AddWithValue(null, TableName);
                var rdr = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (!rdr.HasRows)
                    throw new GeoPackageException(
                        string.Format("No geometry column defined for '{0}'", TableName));
                rdr.Read();
                GeometryColumn = rdr.GetString(rdr.GetOrdinal("column_name"));
                GeometryType = GetGeometryType(rdr.GetString(rdr.GetOrdinal("geometry_type_name")));
                Debug.Assert(rdr.GetInt32(rdr.GetOrdinal("srs_id")) == SRID);
                ZOption = (GpkgOrdinateOption)(rdr.GetByte(rdr.GetOrdinal("z")) + 1);
                MOption = (GpkgOrdinateOption)(rdr.GetByte(rdr.GetOrdinal("m")) + 1);
            }
        }

        private static int GetGeometryType(string geometryTypeName)
        {
            switch (geometryTypeName.ToUpperInvariant())
            {
                case "GEOMETRY":
                    return 0; //OgcGeometryType.Geometry;
                case "POINT":
                    return 1; //OgcGeometryType.Geometry;
                case "LINESTRING":
                    return 2; //OgcGeometryType.Geometry;
                case "POLYGON":
                    return 3; //OgcGeometryType.Geometry;
                case "MULTIPOINT":
                    return 4; //OgcGeometryType.Geometry;
                case "MULTILINESTRING":
                    return 5; //OgcGeometryType.Geometry;
                case "MULTIPOLYGON":
                    return 6; //OgcGeometryType.Geometry;
                case "GEOMETRYCOLLECTION":
                    return 7; //OgcGeometryType.Geometry;
            }
            throw new NotSupportedException(
                string.Format("Geometry type '{0} is not supported'", geometryTypeName));
        }

        internal GpkgOrdinateOption ZOption { get; private set; }

        internal GpkgOrdinateOption MOption { get; private set; }

        internal FeatureDataTable GetBaseTable()
        {
            const string sqlPragmaTableInfo ="PRAGMA table_info('{0}');";

            const string sqlSelectHasColumnData =
                "SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"type\"='table' AND \"name\"='gpkg_column_data';";
            const string sqlSelectColumnData =
                "SELECT * FROM \"gpkg_column_data\" WHERE \"table_name\"=? AND \"column_name\"=?;";

            const string sqlSelectColumnConstraint =
                "SELECT * FROM \"gpkg_column_constraint\" WHERE \"constraint_name\"=?;";

            var fdt = new FeatureDataTable();
            
            // Get the geometry column definition if not previously done
            if (string.IsNullOrEmpty(GeometryColumn))
                GetGeometryColumnDefinition();

            using (var cnCI = new SQLiteConnection(_connectionString).OpenAndReturn())
            using (var cnCD = new SQLiteConnection(_connectionString).OpenAndReturn())
            using (var cnCC = new SQLiteConnection(_connectionString).OpenAndReturn())
            {
                var rdrCI = new SQLiteCommand(string.Format(sqlPragmaTableInfo, TableName), cnCI).ExecuteReader();
                if (!rdrCI.HasRows)
                    throw new GeoPackageException("The table '{0}' does not exist in database!");

                // has additional column data?
                var cmdCD = new SQLiteCommand(sqlSelectHasColumnData, cnCD);
                var hasCD = Convert.ToInt32(cmdCD.ExecuteScalar()) == 1;

                // additional column data
                cmdCD = new SQLiteCommand(sqlSelectColumnData, cnCD);
                var parCD0 = cmdCD.Parameters.Add("table_name", DbType.String);
                parCD0.Value = TableName;
                var parCD1 = cmdCD.Parameters.Add("column_name", DbType.String);

                // additional column constaint(s)
                var cmdCC = new SQLiteCommand(sqlSelectColumnConstraint, cnCC);
                var parCC0 = cmdCC.Parameters.Add("pcc", DbType.String);

                while (rdrCI.Read())
                {
                    // Get the column name
                    var columnName = rdrCI.GetString(1);
                    
                    // We don't want the geometry to appear as an attribute in the feature data table;
                    if (columnName == GeometryColumn) continue;
                    
                    // Set up the column
                    // column name and type
                    var dc = new DataColumn(rdrCI.GetString(1), GpkgUtility.GetTypeForDataTypeString(rdrCI.GetString(2)));

                    // Allow DBNull?
                    if (rdrCI.GetInt32(3) == 0) dc.AllowDBNull = true;

                    // Assign default value
                    if (!rdrCI.IsDBNull(4)) dc.DefaultValue = rdrCI.GetValue(4);

                    // Add the column
                    fdt.Columns.Add(dc);

                    // Get additional information
                    if (hasCD)
                    {
                        parCD1.Value = columnName;
                        var rdrCD = cmdCD.ExecuteReader(CommandBehavior.SingleRow);
                        if (rdrCD.HasRows)
                        {
                            rdrCD.Read();
                            if (!rdrCD.IsDBNull(2)) dc.Caption = rdrCD.GetString(2);

                            if (!rdrCD.IsDBNull(3))
                                dc.ExtendedProperties.Add("Title", rdrCD.GetString(3));
                            if (!rdrCD.IsDBNull(4))
                                dc.ExtendedProperties.Add("Description", rdrCD.GetString(4));
                            if (!rdrCD.IsDBNull(5))
                                dc.ExtendedProperties.Add("MimeType", rdrCD.GetString(5));

                            if (!rdrCD.IsDBNull(rdrCD.GetOrdinal("constraint_name")))
                            {
                                parCC0.Value = rdrCD.GetString(rdrCD.GetOrdinal("constraint_name"));
                                var rdrCC = cmdCC.ExecuteReader();
                                while (rdrCC.Read())
                                {
                                }
                            }
                        }
                    }

                    if (rdrCI.GetInt32(5) == 1)
                    {
                        fdt.PrimaryKey = new[] {dc};
                        OidColumn = dc.ColumnName;
                    }
                }
            }
            return fdt;
        }

    }
}