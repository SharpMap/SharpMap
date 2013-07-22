using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Text;
using GeoAPI.Geometries;
using SharpMap.Converters.WellKnownBinary;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Abstract provider for spatially enabled databases
    /// </summary>
    [Serializable]
    public abstract class SpatialDbProvider : BaseProvider
    {
        private static readonly Common.Logging.ILog Logger = Common.Logging.LogManager.GetCurrentClassLogger();
        
        private bool Initialized { get; set; }

        //private bool _isOpen;

        private string _schema;
        private string _table;

        // ReSharper disable InconsistentNaming
        internal readonly SharpMapFeatureColumn _oidColumn;
        internal readonly SharpMapFeatureColumn _geometryColumn;
        // ReSharper restore InconsistentNaming

        private readonly SharpMapFeatureColumns _featureColumns;

        private string _definitionQuery;
        private string _orderQuery;
        private int _targetSrid;

        private SpatialDbUtility _dbUtility;

        /// <summary>
        /// Gets or sets the <see cref="SpatialDbUtility"/> class. 
        /// </summary>
        /// <remarks>This property can only be set once to a non-<value>null</value> value.</remarks>
        protected SpatialDbUtility DbUtility
        {
            get { return _dbUtility; }
            set
            {
                // make sure this is only set once!
                if (_dbUtility != null)
                    return;
                _dbUtility = value;
            }
        }

        /// <summary>
        /// Gets the SQL-FROM statement
        /// </summary>
        protected virtual string From { get { return _dbUtility.DecorateTable(_schema, _table); } }

        [NonSerialized]
        private Envelope _cachedExtent;

        [NonSerialized]
        private int _cachedFeatureCount = -1;

        private Envelope _areaOfInterest;

        /// <summary>
        /// Event raised when the database schema for this provider has changed;
        /// </summary>
        public event EventHandler SchemaChanged;

        /// <summary>
        /// Method called when the <see cref="Schema"/> has been changed. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.SchemaChanged"/> event.
        /// </summary>
        /// <param name="e">The arguments associated with the event</param>
        protected virtual void OnSchemaChanged(EventArgs e)
        {
            Logger.Debug(t => t("Schema changed to {0}", _schema));
            Initialized = false;

            var handler = SchemaChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the table for this provider has changed;
        /// </summary>
        public event EventHandler TableChanged;

        /// <summary>
        /// Method called when the <see cref="Table"/> has been changed. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.TableChanged"/> event.
        /// </summary>
        /// <param name="e">The arguments associated with the event</param>
        protected virtual void OnTableChanged(EventArgs e)
        {
            Logger.Debug(t => t("Table changed to {0}", _table));
            Initialized = false;

            var handler = TableChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the object id (oid) column for this provider has changed;
        /// </summary>
        public event EventHandler OidColumnChanged;

        /// <summary>
        /// Method called when the <see cref="ObjectIdColumn"/> has been changed. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.OidColumnChanged"/> event.
        /// </summary>
        /// <param name="e">The arguments associated with the event</param>
        protected virtual void OnOidColumnChanged(EventArgs e)
        {
            Logger.Debug(t => t("OidColumnChanged changed to {0}", _oidColumn.Column));
            Initialized = false;

            var handler = OidColumnChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the geometry column for this provider has changed;
        /// </summary>
        public event EventHandler GeometryColumnChanged;

        /// <summary>
        /// Method called when the <see cref="GeometryColumn"/> has been changed. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.GeometryColumnChanged"/> event.
        /// </summary>
        /// <param name="e">The arguments associated with the event</param>
        protected virtual void OnGeometryColumnChanged(EventArgs e)
        {
            Logger.Debug(t => t("Geometry column changed to {0}", _geometryColumn.Column));
            Initialized = false;

            var handler = GeometryColumnChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the feature columns string for this provider has changed;
        /// </summary>
        public event EventHandler DefinitionQueryChanged;

        /// <summary>
        /// Method called when the <see cref="DefinitionQuery"/> has been changed. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.DefinitionQueryChanged"/> event.
        /// </summary>
        /// <param name="e">The arguments associated with the event</param>
        [Obsolete]
        protected virtual void OnDefinitionQueryChanged(EventArgs e)
        {
            Logger.Debug(t => t("DefinitionQuery changed to {0}", _definitionQuery));
            Initialized = false;

            var handler = DefinitionQueryChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the <see cref="FeatureColumns"/> string for this provider has changed;
        /// </summary>
        public event EventHandler FeatureColumnsChanged;

        /// <summary>
        /// Method that handles when the <see cref="SharpMapFeatureColumns.FeatureColumnsChanged"/> event. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.FeatureColumnsChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments associated with the event</param>
        private void OnFeatureColumnsChanged(object sender, EventArgs e)
        {
            Logger.Debug(t => t("FeatureColumns changed"));
            Initialized = false;

            var handler = FeatureColumnsChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the <see cref="TargetSRID"/> for this provider has changed;
        /// </summary>
        public event EventHandler TargetSridChanged;

        /// <summary>
        /// Method called when the <see cref="TargetSRID"/> has been changed. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.TargetSridChanged"/> event.
        /// </summary>
        /// <param name="e">The arguments associated with the event</param>
        protected virtual void OnTargetSridChanged(EventArgs e)
        {
            Logger.Debug(t => t("TragetSrid changed to {0}", _targetSrid));
            Initialized = false;

            var handler = TargetSridChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when <see cref="AreaOfInterest"/> for this provider has changed
        /// </summary>
        public event EventHandler AreaOfInterestChanged;

        /// <summary>
        /// Method called when the <see cref="AreaOfInterest"/> has been changed. This invokes the
        /// <see cref="E:SharpMap.Data.Providers.SpatialDbProvider.AreaOfInterestChanged"/> event.
        /// </summary>
        /// <param name="e">The arguments associated with the event</param>
        protected void OnAreaOfInterestChanged(EventArgs e)
        {
            Logger.Debug(t => t("Area of interst changed to {0}", _areaOfInterest));
            var handler = AreaOfInterestChanged;
            if (handler != null) handler(this, e);
        }

        #region construction and disposal

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="spatialDbUtility">The spatial db utility class</param>
        /// <param name="connectionString">The connection string</param>
        /// <param name="table">The table name</param>
        protected SpatialDbProvider(SpatialDbUtility spatialDbUtility, string connectionString, string table)
            : this(spatialDbUtility, connectionString, string.Empty, table, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="spatialDbUtility">The spatial db utility class</param>
        /// <param name="connectionString">The connection string</param>
        /// <param name="schema">The name of the schema</param>
        /// <param name="table">The table name</param>
        protected SpatialDbProvider(SpatialDbUtility spatialDbUtility, string connectionString, string schema, string table)
            : this(spatialDbUtility, connectionString, schema, table, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="spatialDbUtility">The spatial db utility class</param>
        /// <param name="connectionString">The connection string</param>
        /// <param name="schema">The name of the schema</param>
        /// <param name="table">The table name</param>
        /// <param name="oidColumn">The object ID column</param>
        /// <param name="geometryColumn">The geometry column</param>
        protected SpatialDbProvider(SpatialDbUtility spatialDbUtility,
            string connectionString, string schema, string table, string oidColumn,
                                    string geometryColumn)
            : base(0)
        {
            ConnectionID = connectionString;

            _dbUtility = spatialDbUtility;
            _schema = schema;
            _table = table;

            _oidColumn = new SharpMapFeatureColumn { Column = oidColumn };
            _geometryColumn = new SharpMapFeatureColumn { Column = geometryColumn };

            // Additional columns
            _featureColumns = new SharpMapFeatureColumns(this, spatialDbUtility);
            _featureColumns.FeatureColumnsChanged += OnFeatureColumnsChanged;
        }

        /// <summary>
        /// Releases all managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            _featureColumns.FeatureColumnsChanged -= OnFeatureColumnsChanged;
            base.ReleaseManagedResources();
        }

        #endregion construction and disposal

        /// <summary>
        /// Convenience function to create and open a connection to the database backend.
        /// </summary>
        /// <returns>An open connection to the database backend.</returns>
        protected abstract DbConnection CreateOpenDbConnection();

        /// <summary>
        /// Convenience function to create a data adapter.
        /// </summary>
        /// <returns>An open connection to the database backend.</returns>
        protected abstract DbDataAdapter CreateDataAdapter();

        /// <summary>
        /// Function to initialize the provider
        /// </summary>
        protected void Initialize()
        {
            CheckDisposed();

            if (Initialized)
                return;

            // make sure the cached extent gets cleared
            _cachedExtent = null;
            _cachedFeatureCount = -1;

            InitializeInternal();
        }

        /// <summary>
        /// Method to initialize the spatial provider
        /// </summary>
        protected virtual void InitializeInternal()
        {
            Initialized = true;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString
        {
            get { return ConnectionID; }
        }

        /// <summary>
        /// Gets or sets the name of the database schema
        /// </summary>
        public virtual string Schema
        {
            get { return _schema; }
            set
            {
                if (!string.Equals(value, _schema, StringComparison.InvariantCultureIgnoreCase))
                {
                    _schema = value;
                    OnSchemaChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name oft the database table
        /// </summary>
        public string Table
        {
            get { return _table; }
            set
            {
                if (!string.Equals(value, _table, StringComparison.InvariantCultureIgnoreCase))
                {
                    _table = value;
                    OnTableChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name oft the object id (oid) column
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _oidColumn.Column; }
            set
            {
                if (!string.Equals(value, ObjectIdColumn, StringComparison.InvariantCultureIgnoreCase))
                {
                    _oidColumn.Column = value;
                    OnOidColumnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name oft the geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _geometryColumn.Column; }
            set
            {
                if (!string.Equals(value, GeometryColumn, StringComparison.InvariantCultureIgnoreCase))
                {
                    _geometryColumn.Column = value;
                    OnGeometryColumnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name oft the geometry column
        /// </summary>
        public SharpMapFeatureColumns FeatureColumns
        {
            get { return _featureColumns; }
        }

        /// <summary>
        /// Gets or sets the definition query
        /// </summary>
        [Obsolete("Define constraints via FeatureColumns")]
        public string DefinitionQuery
        {
            get { return _definitionQuery; }
            set
            {
                if (!string.Equals(value, _definitionQuery, StringComparison.InvariantCultureIgnoreCase))
                {
                    _definitionQuery = value;
                    OnDefinitionQueryChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Columns or T-SQL expressions for sorting (ORDER BY clause)
        /// </summary>
        [Obsolete("Define order by via FeatureColumns")]
        public string OrderQuery
        {
            get { return _orderQuery; }
            set { _orderQuery = value; }
        }

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
            get { return _geometryExpression ?? "{0}"; }
            set
            {
                _geometryExpression = value;
            }
        }

        /// <summary>
        /// Gets or sets the area of interest. Setting this property
        /// </summary>
        public Envelope AreaOfInterest
        {
            get { return _areaOfInterest; }
            set
            {
                if (ReferenceEquals(_areaOfInterest, value))
                    return;
                
                if (_areaOfInterest != null && _areaOfInterest.Equals(value))
                    return;

                if (value != null && value.Equals(_areaOfInterest))
                    return;
                _areaOfInterest = value;
                OnAreaOfInterestChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the target SRID. Setting this helps to avoid using on-the-fly reprojection
        /// </summary>
        public virtual int TargetSRID
        {
            get
            {
                return _targetSrid < 0 ? SRID : _targetSrid;
            }
            set
            {
                if (value != TargetSRID)
                {
                    _targetSrid = value;
                    OnTargetSridChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets whether the provider needs to use the transform function
        /// </summary>
        public bool NeedsTransform
        {
            get { return TargetSRID > -1 && TargetSRID != SRID; }
        }

        /// <summary>
        /// <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public override sealed Envelope GetExtents()
        {
            if (_areaOfInterest != null)
                return _areaOfInterest;

            if (_cachedExtent != null)
                return _cachedExtent;

            Initialize();
            return _cachedExtent = GetExtentsInternal();
        }

        /// <summary>
        /// Function to determine the extents of the datasource
        /// </summary>
        /// <returns>The extents</returns>
        protected abstract Envelope GetExtentsInternal();

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public override sealed int GetFeatureCount()
        {
            if (_cachedFeatureCount >= 0)
                return _cachedFeatureCount;

            Initialize();
            return _cachedFeatureCount = GetFeatureCountInternal();
        }

        /// <summary>
        /// Method to get the number of features in the datasource
        /// </summary>
        /// <returns>The number of features</returns>
        protected virtual int GetFeatureCountInternal()
        {
            using (var conn = CreateOpenDbConnection())
            {
                using (var command = conn.CreateCommand())
                {
                    var sql = new StringBuilder();
                    sql.AppendFormat("SELECT COUNT(*) FROM {0}", _dbUtility.DecorateTable(Schema, Table));
#pragma warning disable 612,618
                    if (!String.IsNullOrEmpty(DefinitionQuery))
                        sql.AppendFormat(" WHERE {0}", DefinitionQuery);
#pragma warning restore 612,618
                    else
                        sql.Append(FeatureColumns.GetWhereClause());

                    sql.Append(";");

                    command.CommandText = sql.ToString();
                    return (int)command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="rowId">The id of the row</param>
        /// <returns>The feature data row</returns>
        public override sealed FeatureDataRow GetFeature(uint rowId)
        {
            Initialize();
            return GetFeatureInternal(rowId);
        }

        /// <summary>
        /// Gets a name describing the provider. This name is made up of <see cref="Schema"/>, <see cref="Table"/> and <see cref="GeometryColumn"/>.
        /// </summary>
        protected string Name
        {
            get
            {
                var sb = new StringBuilder(_dbUtility.DecorateTable(_schema, _table));
                sb.AppendFormat(".{0}", GeometryColumn);
                return sb.ToString();
            }
        }

        private FeatureDataTable _baseTable;
        
        private string _geometryExpression;

        /// <summary>
        /// Function to create a new, empty <see cref="FeatureDataTable"/>
        /// </summary>
        /// <returns>A feature data table</returns>
        protected virtual FeatureDataTable CreateNewTable()
        {
            return CreateNewTable(false);
        }

        /// <summary>
        /// Function to create a new, empty <see cref="FeatureDataTable"/>
        /// </summary>
        /// <param name="force">Value indicating that a new feature data table should be created, no matter what.</param>
        /// <returns>A feature data table</returns>
        protected virtual FeatureDataTable CreateNewTable(bool force)
        {
            if (force || _baseTable == null)
            {
                var fdt = new FeatureDataTable { TableName = Name };

                using (var cn = CreateOpenDbConnection())
                {
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = FeatureColumns.GetSelectClause(From);
                        using (var da = CreateDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            fdt = (FeatureDataTable)da.FillSchema(fdt, SchemaType.Source);
                        }
                    }
                }

                //Remove the geometry column, which is always the last!
                fdt.Columns.RemoveAt(fdt.Columns.Count - 1);
                if (_baseTable == null)
                {
                    _baseTable = fdt;
                    return _baseTable;
                }
                return fdt;
            }
            return _baseTable;
            /*
            var res = new FeatureDataTable(dt);
            var resColumns = res.Columns;
            foreach (var column in dt.Columns)
            {
                resColumns.Add(new DataColumn(column.))
            }
            res.PrimaryKey = new [] {res.Columns[0]};
            return res;
             */
        }

        /// <summary>
        /// Function to get a specific feature from the database.
        /// </summary>
        /// <param name="oid">The object id</param>
        /// <returns>A feature data row</returns>
        protected virtual FeatureDataRow GetFeatureInternal(uint oid)
        {
            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectClause(From)
                                      + string.Format(" WHERE {0}={1};", _dbUtility.DecorateEntity(ObjectIdColumn), oid);

                    Logger.Debug(t => t("Executing query:\n{0}", PrintCommand(cmd)));
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            dr.Read();
                            var fdt = CreateNewTable();
                            FeatureDataRow row = null;
                            fdt.BeginLoadData();
                            var numColumns = fdt.Columns.Count;
                            var data = new object[numColumns+1];
                            if (dr.GetValues(data) > 0)
                            {
                                var loadData = new object[numColumns];
                                Array.Copy(data, 0, loadData, 0, numColumns);
                                row = (FeatureDataRow)fdt.LoadDataRow(loadData, true);
                                row.Geometry = GeometryFromWKB.Parse((byte[])data[numColumns], Factory);
                            }
                            fdt.EndLoadData();
                            return row;
                        }
                    }
                }
            }
            return null;
        }

        private static string PrintCommand(DbCommand cmd)
        {
            var sb = new StringBuilder();
            foreach (DbParameter parameter in cmd.Parameters)
            {
                if (sb.Length == 0) sb.Append("Parameter:");
                sb.AppendFormat("\n{0}({1}) = {2}", parameter.ParameterName, parameter.DbType, parameter.Value);
            }
            sb.AppendFormat("\n{0}", cmd.CommandText);
            return sb.ToString();
        }


        /// <summary>
        /// Function to get a specific feature's geometry from the database.
        /// </summary>
        /// <param name="oid">The object id</param>
        /// <returns>A geometry</returns>
        public override sealed IGeometry GetGeometryByID(uint oid)
        {
            Initialize();
            return GetGeometryByIDInternal(oid);
        }

        /// <summary>
        /// Function to get a specific feature's geometry from the database.
        /// </summary>
        /// <param name="oid">The object id</param>
        /// <returns>A geometry</returns>
        protected virtual IGeometry GetGeometryByIDInternal(uint oid)
        {
            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectColumnClause(cmd, _geometryColumn, oid);
                    Logger.Debug(t => t("Executing query:\n{0}", PrintCommand(cmd)));
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                var geometry = GeometryFromWKB.Parse((byte[])dr.GetValue(0), Factory);
                                return geometry;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the geometries of features that lie within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox">The bounding box</param>
        /// <returns>Geometries within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
        public override sealed Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            Initialize();
            return GetGeometriesInViewInternal(bbox);
        }

        /// <summary>
        /// Gets the geometries of features that lie within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox">The bounding box</param>
        /// <returns>Geometries within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
        protected virtual Collection<IGeometry> GetGeometriesInViewInternal(Envelope bbox)
        {
            var res = new Collection<IGeometry>();

            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectColumnClause(cmd, _geometryColumn, GetSpatialWhere(bbox, cmd));
                    Logger.Debug(t => t("Executing query:\n{0}", PrintCommand(cmd)));
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                                res.Add(GeometryFromWKB.Parse((byte[])dr.GetValue(0), Factory));
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Function to generate a spatial where clause for the intersection queries.
        /// </summary>
        /// <param name="bbox">The bounding box</param>
        /// <param name="command">The command object, that is supposed to execute the query.</param>
        /// <returns>The spatial component of a SQL where clause</returns>
        protected abstract string GetSpatialWhere(Envelope bbox, DbCommand command);

        /// <summary>
        /// Function to generate a spatial where clause for the intersection queries.
        /// </summary>
        /// <param name="bbox">The geometry</param>
        /// <param name="command">The command object, that is supposed to execute the query.</param>
        /// <returns>The spatial component of a SQL where clause</returns>
        protected abstract string GetSpatialWhere(IGeometry bbox, DbCommand command);

        /// <summary>
        /// Gets the object of features that lie within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox">The bounding box</param>
        /// <returns>A collection of object ids</returns>
        public override sealed Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            Initialize();
            return GetObjectIDsInViewInternal(bbox);
        }

        /// <summary>
        /// Gets the object ids of features that lie within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox">The bounding box</param>
        /// <returns>A collection of object ids</returns>
        protected virtual Collection<uint> GetObjectIDsInViewInternal(Envelope bbox)
        {
            var res = new Collection<uint>();

            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectColumnClause(cmd, _oidColumn, GetSpatialWhere(bbox, cmd));
                    Logger.Debug(t => t("Executing query:\n{0}", PrintCommand(cmd)));
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                                res.Add(Convert.ToUInt32(dr.GetValue(0)));
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public override sealed void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            Initialize();
            ExecuteIntersectionQueryInternal(box, ds);
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="spatialWhere">Geometry to intersect with</param>
        /// <param name="fds">FeatureDataSet to fill data into</param>
        protected virtual void ExecuteIntersectionQueryInternal(object spatialWhere, FeatureDataSet fds)
        {
            var fdt = CreateNewTable(true);

            fdt.BeginLoadData();

            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    string from = null;

                    var spatialWhereString = string.Empty;
                    var env = spatialWhere as Envelope;

                    if (env != null)
                    {
                        from = GetFrom(env, cmd);
                        spatialWhereString = GetSpatialWhere(env, cmd);
                    }
                    else
                    {
                        var geom = spatialWhere as IGeometry;
                        if (geom != null)
                        {
                            from = GetFrom(geom, cmd);
                            spatialWhereString = GetSpatialWhere(geom, cmd);
                        }
                    }

                    cmd.CommandText = FeatureColumns.GetSelectClause(from)
#pragma warning disable 612,618
                        + (string.IsNullOrEmpty(DefinitionQuery)
#pragma warning restore 612,618
                               ? FeatureColumns.GetWhereClause(spatialWhereString)
                               : (" WHERE " + _definitionQuery +
                                    (string.IsNullOrEmpty(spatialWhereString)
                                            ? ""
                                            : " AND " + spatialWhereString)))

                        + FeatureColumns.GetGroupByClause()
                        + FeatureColumns.GetOrderByClause();

                    var numColumns = fdt.Columns.Count;
                    var geomIndex = numColumns;

                    Logger.Debug(t => t("Executing query:\n{0}", PrintCommand(cmd)));
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var data = new object[numColumns+1];
                            if (dr.GetValues(data) > 0)
                            {
                                var loadData = new object[geomIndex];
                                Array.Copy(data, 0, loadData, 0, geomIndex);
                                var row = (FeatureDataRow)fdt.LoadDataRow(loadData, true);
                                row.Geometry = GeometryFromWKB.Parse((byte[])data[geomIndex], Factory);
                            }
                        }
                    }
                }
            }

            fdt.EndLoadData();

            fds.Tables.Add(fdt);
        }

        /// <summary>
        /// Method to generate a SQL-From statement for a bounding box query
        /// </summary>
        /// <param name="envelope">The envelope to query</param>
        /// <param name="command">The command object that is supposed to perform the query</param>
        /// <returns>A SQL From statement string</returns>
        protected virtual string GetFrom(Envelope envelope, DbCommand command)
        {
            return _dbUtility.DecorateTable(_schema, _table);
        }

        /// <summary>
        /// Method to generate a SQL-From statement for a geometry query
        /// </summary>
        /// <param name="geometry">The envelope to query</param>
        /// <param name="command">The command object that is supposed to perform the query</param>
        /// <returns>A SQL From statement string</returns>
        protected virtual string GetFrom(IGeometry geometry, DbCommand command)
        {
            return _dbUtility.DecorateTable(_schema, _table);
        }

        /// <summary>
        /// Method to perform preparatory things for executing an intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter.</param>
        protected override sealed void OnBeginExecuteIntersectionQuery(IGeometry geom)
        {
            Initialize();
            OnBeginExecuteIntersectionQueryInternal(geom);

            base.OnBeginExecuteIntersectionQuery(geom);
        }

        /// <summary>
        /// Method to perform the actual intersection query against a bounding box
        /// </summary>
        /// <param name="box">The bounding box</param>
        /// <param name="ds">The feature data set to store the results in.</param>
        protected virtual void ExecuteIntersectionQueryInternal(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQueryInternal((object)box, ds);
        }

        /// <summary>
        /// Method to perform preparatory things for executing an intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter.</param>
        protected virtual void OnBeginExecuteIntersectionQueryInternal(IGeometry geom)
        {
        }

        /// <summary>
        /// Method to perform the intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter</param>
        /// <param name="ds">The feature data set to store the results in</param>
        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            ExecuteIntersectionQueryInternal(geom, ds);
        }

        /// <summary>
        /// Handler method to handle changes of <see cref="BaseProvider.SRID"/>.
        /// </summary>
        /// <param name="eventArgs">Event arguments.</param>
        protected override void OnSridChanged(EventArgs eventArgs)
        {
            Initialized = false;
            base.OnSridChanged(eventArgs);
        }
    }
}