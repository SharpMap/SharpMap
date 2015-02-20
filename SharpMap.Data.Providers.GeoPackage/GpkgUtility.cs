using System;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// http://www.geopackage.org/spec/
    /// </summary>
    internal class GpkgUtility
    {
        public static void CheckRequirements(string filename, string password = null)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("The geopackage file was not found", filename);

            //Reqirement 1
            //A GeoPackage SHALL be a SQLite database file using version 3 of the SQLite file format.
            //The first 16 bytes of a GeoPackage SHALL contain 'SQLite format 3' in ASCII.
            using (var fs = File.OpenRead(filename))
            {
                var bytes = new byte[16];
                if (fs.Read(bytes, 0, 16) != 16)
                    throw new GeoPackageException("File not long enought to hold valid header");

                var chars = new char[16];
                int bytesUsed, charsUsed;
                bool completed;
                Encoding.ASCII.GetDecoder()
                    .Convert(bytes, 0, 16, chars, 0, 16, true, out bytesUsed, out charsUsed, out completed);
                if (!new string(chars).Contains("SQLite format 3"))
                    throw new GeoPackageException(
                        "Violation of requirement 1:\nThe first 16 bytes of a GeoPackage SHALL contain 'SQLite format 3'");

                //Requirement 2
                //A GeoPackage SHALL contain 0x47503130 ("GP10" in ASCII) in the application id field of the 
                //SQLite database header to indicate a GeoPackage version 1.0 file.
                bytes = new byte[4];
                fs.Seek(68, SeekOrigin.Begin);
                fs.Read(bytes, 0, 4);
                if (BitConverter.ToInt32(bytes, 0) != 0x47503130)
                    throw new GeoPackageException(
                        "Violation of requirement 2:\nA GeoPackage SHALL contain 0x47503130 ('GP10' in ASCII) in the application id field of the SQLite database header to indicate a GeoPackage version 1.0 file.");
            }

            //Requirement 2
            //A GeoPackage SHALL contain 0x47503130 ("GP10" in ASCII) in the application id field of the 
            //SQLite database header to indicate a GeoPackage version 1.0 file.
            var connectionString = CreateConnectionString(filename, password);
            using (var cn = new SQLiteConnection(connectionString))
            {
                cn.Open();
                var applicationId = (int)new SQLiteCommand("PRAGMA application_id;", cn).ExecuteScalar();
                if (applicationId != 1196437808)
                    throw new GeoPackageException(
                        "Violation of requirement 2:\nA GeoPackage SHALL contain 0x47503130 ('GP10' in ASCII) in the application id field of the SQLite database header to indicate a GeoPackage version 1.0 file.");

            }

            //Requirement 3
            //A GeoPackage SHALL have the file extension name '.gpkg'
            //It is RECOMMENDED that Extended GeoPackages use the file extension '.gpkx', but this is NOT a GeoPackage requirement
            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension) || (extension = extension.ToLowerInvariant()) != ".gpkg" || extension != ".gpkx")
            {
                throw new GeoPackageException(
                    "Violation of requirement 3:\nA GeoPackage SHALL have the file extension name '.gpkg'.\nIt is RECOMMENDED that Extended GeoPackages use the file extension '.gpkx', but this is NOT a GeoPackage requirement");
            }

            //TODO check more requirements
        }

        /// <summary>
        /// The size of the connection pool. A value less than 2 disables connection pooling
        /// </summary>
        public static volatile int MaxPoolSize = 0;

        /// <summary>
        /// Function to create a connection string to the GeoPackage database
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <param name="password">The password</param>
        /// <param name="mustExist">A value indicating that the database has to exist</param>
        /// <returns>A SQLite connection string</returns>
        public static string CreateConnectionString(string filename, string password, bool mustExist = true)
        {
            var sb = new StringBuilder(512);
            sb.AppendFormat("DataSource={0};version=3;", filename);
            
            // the pool size
            var poolSize = MaxPoolSize;
            if (poolSize > 1)
                sb.AppendFormat("Pooling=true;Max Pool Size={0};", poolSize);

            // a password
            if (!String.IsNullOrWhiteSpace(password))
                sb.AppendFormat("Password={0};", password);

            // the database must exist
            if (mustExist)
                sb.AppendFormat("FailIfMissing=True;");

            return sb.ToString();
        }

        public static void CreateGeoPackage(string filename, string password = null)
        {
            using (var cn = new SQLiteConnection(CreateConnectionString(filename, password)))
            {
                cn.Open();

                new SQLiteCommand(CreateSpatialRefSys, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateContents, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateGeometryColumns, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateDataColumns, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateDataColumnConstraints, cn).ExecuteNonQuery();

                new SQLiteCommand(CreateTileMatrixSet, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrix, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixZoomLevelInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixZoomLevelUpdate, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixMatrixWidthInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixMatrixWidthUpdate, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixMatrixHeightInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixMatrixHeightUpdate, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixPixelXSizeInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixPixelXSizeUpdate, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixPixelYSizeInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateTileMatrixPixelYSizeUpdate, cn).ExecuteNonQuery();

                new SQLiteCommand(CreateMetadata, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataMdScopeInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataMdScopeUpdate, cn).ExecuteNonQuery();

                new SQLiteCommand(CreateMetadataReference, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceReferenceScopeInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceReferenceScopeUpdate, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceColumnNameInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceColumnNameUpdate, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceRowIDValueInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceRowIDValueUpdate, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceTimestampInsert, cn).ExecuteNonQuery();
                new SQLiteCommand(CreateMetadataReferenceTimestampUpdate, cn).ExecuteNonQuery();
            }
        }

        private const string CreateSpatialRefSys = 
            @"CREATE TABLE gpkg_spatial_ref_sys (srs_name TEXT NOT NULL, srs_id INTEGER NOT NULL PRIMARY KEY, organization TEXT NOT NULL, organization_coordsys_id INTEGER NOT NULL, definition  TEXT NOT NULL, description TEXT);";
        private const string CreateContents = 
            @"CREATE TABLE gpkg_contents (table_name TEXT NOT NULL PRIMARY KEY, data_type TEXT NOT NULL, identifier TEXT UNIQUE, description TEXT DEFAULT '', last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')), min_x DOUBLE, min_y DOUBLE, max_x DOUBLE, max_y DOUBLE, srs_id INTEGER, CONSTRAINT fk_gc_r_srs_id FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id));";
        private const string CreateGeometryColumns =
            @"CREATE TABLE gpkg_geometry_columns (table_name TEXT NOT NULL, column_name TEXT NOT NULL, geometry_type_name TEXT NOT NULL, srs_id INTEGER NOT NULL, z TINYINT NOT NULL, m TINYINT NOT NULL, CONSTRAINT pk_geom_cols PRIMARY KEY (table_name, column_name), CONSTRAINT uk_gc_table_name UNIQUE (table_name), CONSTRAINT fk_gc_tn FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name), CONSTRAINT fk_gc_srs FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys (srs_id));";
        private const string CreateTileMatrixSet =
            @"CREATE TABLE gpkg_tile_matrix_set (table_name TEXT NOT NULL PRIMARY KEY, srs_id INTEGER NOT NULL, min_x DOUBLE NOT NULL, min_y DOUBLE NOT NULL, max_x DOUBLE NOT NULL, max_y DOUBLE NOT NULL, CONSTRAINT fk_gtms_table_name FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name), CONSTRAINT fk_gtms_srs FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys (srs_id));";
        private const string CreateTileMatrix =
            @"CREATE TABLE gpkg_tile_matrix (table_name TEXT NOT NULL, zoom_level INTEGER NOT NULL, matrix_width INTEGER NOT NULL, matrix_height INTEGER NOT NULL, tile_width INTEGER NOT NULL, tile_height INTEGER NOT NULL, pixel_x_size DOUBLE NOT NULL, pixel_y_size DOUBLE NOT NULL, CONSTRAINT pk_ttm PRIMARY KEY (table_name, zoom_level), CONSTRAINT fk_tmm_table_name FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name));";
        private const string CreateDataColumns =
            @"CREATE TABLE gpkg_data_columns (table_name TEXT NOT NULL, column_name TEXT NOT NULL, name TEXT, title TEXT, description TEXT, mime_type TEXT, constraint_name TEXT, CONSTRAINT pk_gdc PRIMARY KEY (table_name, column_name), CONSTRAINT fk_gdc_tn FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name));";
        private const string CreateDataColumnConstraints =
            @"CREATE TABLE gpkg_data_column_constraints (constraint_name TEXT NOT NULL, constraint_type TEXT NOT NULL, value TEXT, min NUMERIC, minIsInclusive BOOLEAN, max NUMERIC, maxIsInclusive BOOLEAN, CONSTRAINT gdcc_ntv UNIQUE (constraint_name, constraint_type, value))";
        private const string CreateMetadata =
            @"CREATE TABLE gpkg_metadata (id INTEGER CONSTRAINT m_pk PRIMARY KEY ASC NOT NULL UNIQUE, md_scope TEXT NOT NULL DEFAULT 'dataset', md_standard_uri TEXT NOT NULL, mime_type TEXT NOT NULL DEFAULT 'text/xml', metadata TEXT NOT NULL);";
        private const string CreateMetadataReference =
            @"CREATE TABLE gpkg_metadata_reference (reference_scope TEXT NOT NULL, table_name TEXT, column_name TEXT, row_id_value INTEGER, timestamp DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')), md_file_id INTEGER NOT NULL, md_parent_id INTEGER, CONSTRAINT crmr_mfi_fk FOREIGN KEY (md_file_id) REFERENCES gpkg_metadata(id), CONSTRAINT crmr_mpi_fk FOREIGN KEY (md_parent_id) REFERENCES gpkg_metadata(id));";

        #region trigger tiles
        private const string CreateTileMatrixZoomLevelInsert =
            @"CREATE TRIGGER 'gpkg_tile_matrix_zoom_level_insert'
        BEFORE INSERT ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: zoom_level cannot be less than 0')
        WHERE (NEW.zoom_level < 0);
        END;";

        private const string CreateTileMatrixZoomLevelUpdate =
            @"CREATE TRIGGER 'gpkg_tile_matrix_zoom_level_update'
        BEFORE UPDATE of zoom_level ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: zoom_level cannot be less than 0')
        WHERE (NEW.zoom_level < 0);
        END;";

        private const string CreateTileMatrixMatrixWidthInsert =
            @"CREATE TRIGGER 'gpkg_tile_matrix_matrix_width_insert'
        BEFORE INSERT ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: matrix_width cannot be less than 1')
        WHERE (NEW.matrix_width < 1);
        END;";

        private const string CreateTileMatrixMatrixWidthUpdate =
            @"CREATE TRIGGER 'gpkg_tile_matrix_matrix_width_update'
        BEFORE UPDATE OF matrix_width ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: matrix_width cannot be less than 1')
        WHERE (NEW.matrix_width < 1);
        END";

        private const string CreateTileMatrixMatrixHeightInsert =
            @"CREATE TRIGGER 'gpkg_tile_matrix_matrix_height_insert'
        BEFORE INSERT ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: matrix_height cannot be less than 1')
        WHERE (NEW.matrix_height < 1);
        END;";

        private const string CreateTileMatrixMatrixHeightUpdate =
            @"CREATE TRIGGER 'gpkg_tile_matrix_matrix_height_update'
        BEFORE UPDATE OF matrix_height ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: matrix_height cannot be less than 1')
        WHERE (NEW.matrix_height < 1);
        END;";

        private const string CreateTileMatrixPixelXSizeInsert =
            @"CREATE TRIGGER 'gpkg_tile_matrix_pixel_x_size_insert'
        BEFORE INSERT ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: pixel_x_size must be greater than 0')
        WHERE NOT (NEW.pixel_x_size > 0);
        END;";

        private const string CreateTileMatrixPixelXSizeUpdate =
            @"CREATE TRIGGER 'gpkg_tile_matrix_pixel_x_size_update'
        BEFORE UPDATE OF pixel_x_size ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: pixel_x_size must be greater than 0')
        WHERE NOT (NEW.pixel_x_size > 0);
        END;";

        private const string CreateTileMatrixPixelYSizeInsert =
            @"CREATE TRIGGER 'gpkg_tile_matrix_pixel_y_size_insert'
        BEFORE INSERT ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: pixel_y_size must be greater than 0')
        WHERE NOT (NEW.pixel_y_size > 0);
        END;";

        private const string CreateTileMatrixPixelYSizeUpdate =
            @"CREATE TRIGGER 'gpkg_tile_matrix_pixel_y_size_update'
        BEFORE UPDATE OF pixel_y_size ON 'gpkg_tile_matrix'
        FOR EACH ROW BEGIN
        SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: pixel_y_size must be greater than 0')
        WHERE NOT (NEW.pixel_y_size > 0);
        END;";

        #endregion

        #region trigger metadata

        private const string CreateMetadataMdScopeInsert =
            @"CREATE TRIGGER 'gpkg_metadata_md_scope_insert'
BEFORE INSERT ON 'gpkg_metadata'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'insert on table gpkg_metadata violates
constraint: md_scope must be one of undefined | fieldSession |
collectionSession | series | dataset | featureType | feature |
attributeType | attribute | tile | model | catalog | schema |
taxonomy software | service | collectionHardware |
nonGeographicDataset | dimensionGroup')
WHERE NOT(NEW.md_scope IN
('undefined','fieldSession','collectionSession','series','dataset',
'featureType','feature','attributeType','attribute','tile','model',
'catalog','schema','taxonomy','software','service',
'collectionHardware','nonGeographicDataset','dimensionGroup'));
END;";

        private const string CreateMetadataMdScopeUpdate =
            @"CREATE TRIGGER 'gpkg_metadata_md_scope_update'
BEFORE UPDATE OF 'md_scope' ON 'gpkg_metadata'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'update on table gpkg_metadata violates
constraint: md_scope must be one of undefined | fieldSession |
collectionSession | series | dataset | featureType | feature |
attributeType | attribute | tile | model | catalog | schema |
taxonomy software | service | collectionHardware |
nonGeographicDataset | dimensionGroup')
WHERE NOT(NEW.md_scope IN
('undefined','fieldSession','collectionSession','series','dataset',
'featureType','feature','attributeType','attribute','tile','model',
'catalog','schema','taxonomy','software','service',
'collectionHardware','nonGeographicDataset','dimensionGroup'));
END;";

        #endregion

        private const string CreateMetadataReferenceReferenceScopeInsert =
            @"CREATE TRIGGER 'gpkg_metadata_reference_reference_scope_insert'
BEFORE INSERT ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'insert on table gpkg_metadata_reference
violates constraint: reference_scope must be one of ""geopackage"",
""table"", ""column"", ""row"", ""row/col""')
WHERE NOT NEW.reference_scope IN
('geopackage','table','column','row','row/col');
END;";


        private const string CreateMetadataReferenceReferenceScopeUpdate =
            @"CREATE TRIGGER 'gpkg_metadata_reference_reference_scope_update'
BEFORE UPDATE OF 'reference_scope' ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'update on table gpkg_metadata_reference
violates constraint: referrence_scope must be one of ""geopackage"",
""table"", ""column"", ""row"", ""row/col""')
WHERE NOT NEW.reference_scope IN
('geopackage','table','column','row','row/col');
END;";

        private const string CreateMetadataReferenceColumnNameInsert =
            @"CREATE TRIGGER 'gpkg_metadata_reference_column_name_insert'
BEFORE INSERT ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'insert on table gpkg_metadata_reference
violates constraint: column name must be NULL when reference_scope
is  ""geopackage"", ""table"" or ""row""')
WHERE (NEW.reference_scope IN ('geopackage','table','row')
AND NEW.column_name IS NOT NULL);
SELECT RAISE(ABORT, 'insert on table gpkg_metadata_reference
violates constraint: column name must be defined for the specified
table when reference_scope is ""column"" or ""row/col""')
WHERE (NEW.reference_scope IN ('column','row/col')
AND NOT NEW.table_name IN (
SELECT name FROM SQLITE_MASTER WHERE type = 'table'
AND name = NEW.table_name
AND sql LIKE ('%' || NEW.column_name || '%')));
END;";

        private const string CreateMetadataReferenceColumnNameUpdate =
            @"CREATE TRIGGER 'gpkg_metadata_reference_column_name_update'
BEFORE UPDATE OF column_name ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'update on table gpkg_metadata_reference
violates constraint: column name must be NULL when reference_scope
is  ""geopackage"", ""table"" or ""row""')
WHERE (NEW.reference_scope IN ('geopackage','table','row')
AND NEW.column_nameIS NOT NULL);
SELECT RAISE(ABORT, 'update on table gpkg_metadata_reference
violates constraint: column name must be defined for the specified
table when reference_scope is ""column"" or ""row/col""')
WHERE (NEW.reference_scope IN ('column','row/col')
AND NOT NEW.table_name IN (
SELECT name FROM SQLITE_MASTER WHERE type = 'table'
AND name = NEW.table_name
AND sql LIKE ('%' || NEW.column_name || '%')));
END;";

        private const string CreateMetadataReferenceRowIDValueInsert =
            @"CREATE TRIGGER 'gpkg_metadata_reference_row_id_value_insert'
BEFORE INSERT ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'insert on table gpkg_metadata_reference
violates constraint: row_id_value must be NULL when reference_scope
is ""geopackage"", ""table"" or ""column""')
WHERE NEW.reference_scope IN ('geopackage','table','column')
AND NEW.row_id_value IS NOT NULL;
SELECT RAISE(ABORT, 'insert on table gpkg_metadata_reference
violates constraint: row_id_value must exist in specified table when
reference_scope is ""row"" or ""row/col""')
WHERE NEW.reference_scope IN ('row','row/col')
AND NOT EXISTS (SELECT rowid
FROM (SELECT NEW.table_name AS table_name) WHERE rowid =
NEW.row_id_value);
END;";

        private const string CreateMetadataReferenceRowIDValueUpdate =
            @"CREATE TRIGGER 'gpkg_metadata_reference_row_id_value_update'
BEFORE UPDATE OF 'row_id_value' ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'update on table gpkg_metadata_reference
violates constraint: row_id_value must be NULL when reference_scope
is ""geopackage"", ""table"" or ""column""')
WHERE NEW.reference_scope IN ('geopackage','table','column')
AND NEW.row_id_value IS NOT NULL;
SELECT RAISE(ABORT, 'update on table gpkg_metadata_reference
violates constraint: row_id_value must exist in specified table when
reference_scope is ""row"" or ""row/col""')
WHERE NEW.reference_scope IN ('row','row/col')
AND NOT EXISTS (SELECT rowid
FROM (SELECT NEW.table_name AS table_name) WHERE rowid =
NEW.row_id_value);
END;";

        private const string CreateMetadataReferenceTimestampInsert =
            @"CREATE TRIGGER 'gpkg_metadata_reference_timestamp_insert'
BEFORE INSERT ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'insert on table gpkg_metadata_reference
violates constraint: timestamp must be a valid time in ISO 8601
""yyyy-mm-ddThh-mm-ss.cccZ"" form')
WHERE NOT (NEW.timestamp GLOB
'[1-2][0-9][0-9][0-9]-[0-1][0-9]-[1-3][0-9]T[0-2][0-9]:[0-5][0-
9]:[0-5][0-9].[0-9][0-9][0-9]Z'
AND strftime('%s',NEW.timestamp) NOT NULL);
END;";

        private const string CreateMetadataReferenceTimestampUpdate =
            @"CREATE TRIGGER 'gpkg_metadata_reference_timestamp_update'
BEFORE UPDATE OF 'timestamp' ON 'gpkg_metadata_reference'
FOR EACH ROW BEGIN
SELECT RAISE(ABORT, 'update on table gpkg_metadata_reference
violates constraint: timestamp must be a valid time in ISO 8601
""yyyy-mm-ddThh-mm-ss.cccZ"" form')
WHERE NOT (NEW.timestamp GLOB
'[1-2][0-9][0-9][0-9]-[0-1][0-9]-[1-3][0-9]T[0-2][0-9]:[0-5][0-
9]:[0-5][0-9].[0-9][0-9][0-9]Z'
AND strftime('%s',NEW.timestamp) NOT NULL);
END;";

        #region metadata reference

        #endregion

        public static Type GetTypeForDataTypeString(string dataTypeString)
        {
            dataTypeString = dataTypeString.ToLowerInvariant();
            switch (dataTypeString)
            {
                case "tinyint":
                    return typeof (byte);

                case "int4":
                case "smallint":
                    return typeof (short);

                case "integer":
                case "int":
                case "mediumint":
                    return typeof(int);

                case "int8":
                case "bigint":
                    return typeof (long);

                case "unsigned big int":
                    return typeof (ulong);

                case "text":
                case "clob":
                    return typeof (string);

                case "blob":
                    return typeof (byte[]);

                case "double":
                case "double precision":
                    return typeof(double);

                case "real":
                case "float":
                    return typeof(float);

                case "date":
                case "datetime":
                    return typeof(DateTime);

                case "numeric":
                    return typeof (decimal);

                case "boolean":
                    return typeof (bool);

                default:
                    if (dataTypeString.StartsWith("character(") ||
                        dataTypeString.StartsWith("varchar(") ||
                        dataTypeString.StartsWith("varying character(") ||
                        dataTypeString.StartsWith("nchar(") ||
                        dataTypeString.StartsWith("native character(") ||
                        dataTypeString.StartsWith("nvarchar(")
                        )
                        return typeof (string);
                    if (dataTypeString.StartsWith("decimal("))
                        return typeof (decimal);
                    break;
            }

            throw new GeoPackageException(string.Format("Unknown data type '{0}'", dataTypeString));
        }
    }
}
