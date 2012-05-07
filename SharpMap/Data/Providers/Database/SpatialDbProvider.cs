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
        private bool Initialized { get; set; }

        private bool _isOpen;

        private string _schema;
        private string _table;

        private readonly SharpMapFeatureColumn _oidColumn;
        private readonly SharpMapFeatureColumn _geometryColumn;

        private readonly SharpMapFeatureColumns _featureColumns;
        
        private string _definitionQuery;
        private string _orderQuery;
        private int _targetSrid;

        private SpatialDbUtility _dbUtility;
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

        protected virtual void OnSchemaChanged(EventArgs e)
        {
            Initialized = false;

            var handler = SchemaChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the table for this provider has changed;
        /// </summary>
        public event EventHandler TableChanged;

        protected virtual void OnTableChanged(EventArgs e)
        {
            Initialized = false;

            var handler = TableChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the object id (oid) column for this provider has changed;
        /// </summary>
        public event EventHandler OidColumnChanged;

        protected virtual void OnOidColumnChanged(EventArgs e)
        {
            Initialized = false;

            var handler = OidColumnChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the geometry column for this provider has changed;
        /// </summary>
        public event EventHandler GeometryColumnChanged;

        protected virtual void OnGeometryColumnChanged(EventArgs e)
        {
            Initialized = false;

            var handler = GeometryColumnChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the feature columns string for this provider has changed;
        /// </summary>
        public event EventHandler DefinitionQueryChanged;

        protected virtual void OnDefinitionQueryChanged(EventArgs e)
        {
            Initialized = false;

            var handler = DefinitionQueryChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the <see cref="FeatureColumns"/> string for this provider has changed;
        /// </summary>
        public event EventHandler FeatureColumnsChanged;

        private void OnFeatureColumnsChanged(object sender, EventArgs e)
        {
            Initialized = false;

            var handler = FeatureColumnsChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when the <see cref="TargetSRID"/> for this provider has changed;
        /// </summary>
        public event EventHandler TargetSridChanged;

        protected virtual void OnTargetSridChanged(EventArgs e)
        {
            Initialized = false;

            var handler = TargetSridChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Event raised when <see cref="AreaOfInterest"/> for this provider has changed
        /// </summary>
        public event EventHandler AreaOfInterestChanged;

        protected void OnAreaOfInterestChanged(EventArgs e)
        {
            var handler = AreaOfInterestChanged;
            if (handler != null) handler(this, e);
        }

        #region construction and disposal

        protected SpatialDbProvider(SpatialDbUtility spatialDbUtility, string connectionString, string table)
            : this(spatialDbUtility, connectionString, string.Empty, table, string.Empty, string.Empty)
        {
        }

        protected SpatialDbProvider(SpatialDbUtility spatialDbUtility, string connectionString, string schema, string table)
            : this(spatialDbUtility, connectionString, schema, table, string.Empty, string.Empty)
        {
        }

        protected SpatialDbProvider(SpatialDbUtility spatialDbUtility,
            string connectionString, string schema, string table, string oidColumn,
                                    string geometryColumn)
            : base(0)
        {
            ConnectionID = connectionString;
            
            _dbUtility = spatialDbUtility;
            _schema = schema;
            _table = table;
            
            _oidColumn = new SharpMapFeatureColumn {Column = oidColumn};
            _geometryColumn = new SharpMapFeatureColumn {Column = geometryColumn};

            // Additional columns
            _featureColumns = new SharpMapFeatureColumns(this, spatialDbUtility);
            _featureColumns.FeatureColumnsChanged += OnFeatureColumnsChanged;
        }

        protected override void ReleaseManagedResources()
        {
            _featureColumns.FeatureColumnsChanged -= OnFeatureColumnsChanged;
            base.ReleaseManagedResources();
        }

        #endregion
        
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

        protected abstract void InitializeInternal();

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
                if (value != _areaOfInterest)
                {
                    if (value != null && value.Equals(_areaOfInterest))
                            return;
                    _areaOfInterest = value;
                    OnAreaOfInterestChanged(EventArgs.Empty);
                }
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
        public bool NeedsTransfrom
        {
            get { return TargetSRID > - 1 && TargetSRID != SRID; }
        }

        public override sealed Envelope GetExtents()
        {
            if (_areaOfInterest != null)
                return _areaOfInterest;
            
            if (_cachedExtent != null)
                return _cachedExtent;

            Initialize();
            return _cachedExtent = GetExtentsInternal();
        }

        protected abstract Envelope GetExtentsInternal();

        public override sealed int GetFeatureCount()
        {
            if (_cachedFeatureCount >= 0)
                return _cachedFeatureCount;

            Initialize();
            return _cachedFeatureCount = GetFeatureCountInternal();
        }

        protected virtual int GetFeatureCountInternal()
        {
            using (var conn = CreateOpenDbConnection())
            {
                using (var command = conn.CreateCommand())
                {
                    var sql = new StringBuilder();
                    sql.AppendFormat("SELECT COUNT(*) FROM {0}", _dbUtility.DecorateEntity(Table));
                    if (!String.IsNullOrEmpty(DefinitionQuery))
                        sql.AppendFormat(" WHERE {0}", DefinitionQuery);
                    else
                        sql.Append(FeatureColumns.GetWhereClause());
                
                    sql.Append(";");

                    command.CommandText = sql.ToString();
                    return (int)command.ExecuteScalar();
                }
            }
        }

        public override sealed FeatureDataRow GetFeature(uint oid)
        {
            Initialize();
            return GetFeatureInternal(oid);
        }

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

        protected virtual FeatureDataTable CreateNewTable(bool force = false)
        {
            if (force || _baseTable == null)
            {
                var fdt = new FeatureDataTable {TableName = Name};

                using (var cn = CreateOpenDbConnection())
                {
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = FeatureColumns.GetSelectClause(From);
                        using (var da = CreateDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            fdt = (FeatureDataTable) da.FillSchema(fdt, SchemaType.Source);
                        }
                    }
                }

                //Remove the geometry column, which is always the last!
                fdt.Columns.RemoveAt(fdt.Columns.Count-1);
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

        protected virtual FeatureDataRow GetFeatureInternal(uint oid)
        {
            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectClause(From)
                                      + string.Format(" WHERE {0}={1};", _dbUtility.DecorateEntity("_smOid_"), oid);
                    
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            var fdr = CreateNewTable().NewRow();
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr.GetValue(1), Factory);
                            fdr[0] = dr.GetValue(0);
                            for (var i = 2; i < dr.FieldCount; i++)
                                fdr[i - 1] = dr.GetValue(i);
                            return fdr;
                        }
                    }
                }
            }
            return null;
        }

        public override sealed IGeometry GetGeometryByID(uint oid)
        {
            Initialize();
            return GetGeometryByIDInternal(oid);
        }

        protected virtual IGeometry GetGeometryByIDInternal(uint oid)
        {
            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectColumnClause(cmd, ObjectIdColumn, oid);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            var geometry = GeometryFromWKB.Parse((byte[])dr.GetValue(0), Factory);
                            return geometry;
                        }
                    }
                }
            }
            return null;
        }

        public override sealed Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            Initialize();
            return GetGeometriesInViewInternal(bbox);
        }

        protected virtual Collection<IGeometry> GetGeometriesInViewInternal(Envelope bbox)
        {
            var res = new Collection<IGeometry>();

            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectColumnClause(cmd, FeatureColumns.GetGeometryColumn(true), spatialWhere: GetSpatialWhere(bbox, cmd));
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                                res.Add(GeometryFromWKB.Parse((byte[]) dr.GetValue(0), Factory));
                        }
                    }
                }
            }
            return res;
        }

        protected abstract string GetSpatialWhere(Envelope bbox, DbCommand command);

        protected abstract string GetSpatialWhere(IGeometry bbox, DbCommand command);

        public override sealed Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            Initialize();
            return GetObjectIDsInViewInternal(bbox);
        }

        protected virtual Collection<uint> GetObjectIDsInViewInternal(Envelope bbox)
        {
            var res = new Collection<uint>();

            using (var cn = CreateOpenDbConnection())
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = FeatureColumns.GetSelectColumnClause(cmd, _dbUtility.DecorateEntity(ObjectIdColumn), spatialWhere: GetSpatialWhere(bbox, cmd));
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

        public override sealed void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            Initialize();
            ExecuteIntersectionQueryInternal(box, ds);
        }

        protected virtual void ExecuteIntersectionQueryInternal(object spatialWhere, FeatureDataSet fds)
        {
            var fdt = CreateNewTable(true);
            
            fdt.BeginLoadData();
            
            using (var cn = CreateOpenDbConnection())
            {
                using(var cmd = cn.CreateCommand())
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

                    cmd.CommandText =
                          FeatureColumns.GetSelectClause(from)
                        + (string.IsNullOrEmpty(DefinitionQuery)
                               ? FeatureColumns.GetWhereClause(spatialWhereString)
                               : (" WHERE " + _definitionQuery + 
                                    ( string.IsNullOrEmpty(spatialWhereString) 
                                            ? "" 
                                            : " AND " + spatialWhereString)))
                        
                        + FeatureColumns.GetGroupByClause()
                        + FeatureColumns.GetOrderByClause();

                    var numColumns = fdt.Columns.Count;
                    var geomIndex = numColumns - 1;

                    using (var dr = cmd.ExecuteReader())
                    {
                        var data = new object[numColumns];
                        if (dr.GetValues(data) > 0)
                        {
                            var loadData = new object[geomIndex];
                            Array.Copy(data, 0, loadData, 0, geomIndex);
                            var row = (FeatureDataRow) fdt.LoadDataRow(data, true);
                            row.Geometry = GeometryFromWKB.Parse((byte[]) data[geomIndex], Factory);
                        }
                    }
                }
            }
            
            fdt.EndLoadData();

            fds.Tables.Add(fdt);
        }

        protected virtual string GetFrom(Envelope envelope, DbCommand command)
        {
            return _dbUtility.DecorateTable(_schema, _table);
        }

        protected virtual string GetFrom(IGeometry geometry, DbCommand command)
        {
            return _dbUtility.DecorateTable(_schema, _table);
        }

        protected override sealed void OnBeginExecuteIntersectionQuery(IGeometry geom)
        {
            Initialize();
            OnBeginExecuteIntersectionQueryInternal(geom);

            base.OnBeginExecuteIntersectionQuery(geom);
        }

        protected virtual void ExecuteIntersectionQueryInternal(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQueryInternal((object)box, ds);
        }

        protected virtual void OnBeginExecuteIntersectionQueryInternal(IGeometry geom)
        {
        }

        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            ExecuteIntersectionQueryInternal((object)geom, ds);
        }

        protected override void OnSridChanged(EventArgs eventArgs)
        {
            Initialized = false;
            base.OnSridChanged(eventArgs);
        }

    }
}