using System;
using System.Collections.ObjectModel;
using System.Data;
using GeoAPI;
using GeoAPI.Geometries;
using SharpMap.Base;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Abstract base provider that handles geometry factory based on SRID
    /// </summary>
    [Serializable]
    public abstract class BaseProvider : DisposableObject, IProvider
    {
        private int _srid;
        private bool _isOpen;

        /// <summary>
        /// Event raised when <see cref="SRID"/> has changed
        /// </summary>
        public event EventHandler SridChanged;

        /// <summary>
        /// Gets or sets the factory to create geometries.
        /// </summary>
        public IGeometryFactory Factory { get; protected set; }

        /// <summary>
        /// Creates an instance of this class. The <see cref="ConnectionID"/> is set to <see cref="String.Empty"/>,
        /// the spatial reference id to <c>0</c> and an appropriate factory is chosen.
        /// </summary>
        protected BaseProvider()
            :this(0)
        {
        }

        /// <summary>
        /// Creates an instance of this class. The <see cref="ConnectionID"/> is set to <see cref="String.Empty"/>,
        /// the spatial reference id to <paramref name="srid"/> and an appropriate factory is chosen.
        /// </summary>
        /// <param name="srid">The spatial reference id</param>
        protected BaseProvider(int srid)
        {
            ConnectionID = string.Empty;
            SRID = srid;
            Factory = GeometryServiceProvider.Instance.CreateGeometryFactory(SRID);
        }

        /// <summary>
        /// Releases all managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            Factory = null;
            base.ReleaseManagedResources();
        }

        #region Implementation of IProvider

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        public string ConnectionID { get; protected set; }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public virtual bool IsOpen { get { return _isOpen; } }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set
            {
                if (value != _srid)
                {
                    _srid = value;
                    OnSridChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Handler method to handle changes of <see cref="SRID"/>.
        /// </summary>
        /// <param name="eventArgs">Event arguments.</param>
        protected virtual void OnSridChanged(EventArgs eventArgs)
        {
            Factory = GeometryServiceProvider.Instance.CreateGeometryFactory(SRID);
            
            if (SridChanged != null)
                SridChanged(this, eventArgs);
        }

        /// <summary>
        /// Gets the features within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
        public abstract Collection<IGeometry> GetGeometriesInView(Envelope bbox);

        /// <summary>
        /// Returns all objects whose <see cref="GeoAPI.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="GeoAPI.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        public abstract Collection<uint> GetObjectIDsInView(Envelope bbox);

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public abstract IGeometry GetGeometryByID(uint oid);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            OnBeginExecuteIntersectionQuery(geom);
            OnExecuteIntersectionQuery(geom, ds);
            OnEndExecuteIntersectionQuery();
        }

        /// <summary>
        /// Method to perform preparatory things for executing an intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter.</param>
        protected virtual void OnBeginExecuteIntersectionQuery(IGeometry geom)
        {
        }

        /// <summary>
        /// Method to perform the intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter</param>
        /// <param name="ds">The feature data set to store the results in</param>
        protected abstract void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds);

        /// <summary>
        /// Method to do cleanup work after having performed the intersection query against the data source
        /// </summary>
        protected virtual void OnEndExecuteIntersectionQuery()
        {
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public abstract void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds);

        /// <summary>
        /// Function to return the total number of features in the dataset
        /// </summary>
        /// <returns>The number of features</returns>
        public abstract int GetFeatureCount();

        /// <summary>
        /// Function to return a <see cref="SharpMap.Data.FeatureDataRow"/> based on <paramref name="rowId">RowID</paramref>
        /// </summary>
        /// <param name="rowId">The unique identifier of the row</param>
        /// <returns>datarow</returns>
        public abstract FeatureDataRow GetFeature(uint rowId);

        /// <summary>
        /// Function to return the <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>The extent of the dataset</returns>
        public abstract Envelope GetExtents();

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public virtual void Open()
        {
            _isOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public virtual void Close()
        {
            _isOpen = false;
        }

        #endregion

        /// <summary>
        /// Method to clone the feature data tables schema.
        /// </summary>
        /// <param name="baseTable">The feature data table</param>
        /// <returns>An empty feature data table, having the same schema as <paramref name="baseTable"/></returns>
        protected static FeatureDataTable CloneTableStructure(FeatureDataTable baseTable)
        {
            var res = new FeatureDataTable(baseTable);
            var cols = res.Columns;
            foreach (DataColumn column in baseTable.Columns)
            {
                cols.Add(new DataColumn(column.ColumnName, column.DataType, column.Expression, column.ColumnMapping)
                    
                /*{AllowDBNull = column.AllowDBNull, AutoIncrement = column.AutoIncrement, AutoIncrementSeed = column.AutoIncrementSeed,
                    AutoIncrementStep = column.AutoIncrementStep, Caption = column.Caption}*/);
            }
            /*
            var constraints = res.Constraints;
            foreach (var constraint in baseTable.Constraints)
            {
                var uc = constraint as UniqueConstraint;
                if (uc != null)
                {
                }
            }
            */
            return res;
        }
    }
}