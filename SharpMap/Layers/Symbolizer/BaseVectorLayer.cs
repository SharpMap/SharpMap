using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI;
using GeoAPI.Geometries;
#if !DotSpatialProjections
using GeoAPI.CoordinateSystems.Transformations;
#else
using DotSpatial.Projections;
#endif
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Rendering.Symbolizer;
using IGeometry = GeoAPI.Geometries.IGeometry;
using Common.Logging;

namespace SharpMap.Layers.Symbolizer
{
    /// <summary>
    /// Base class for all vector layers using <see cref="ISymbolizer{TGeometry}"/> approach.
    /// </summary>
    /// <typeparam name="TGeometry">The geometry type</typeparam>
    [Serializable]
    public abstract class BaseVectorLayer<TGeometry> : Layer, ICanQueryLayer
        where TGeometry : class//, IGeometry
    {
        #region Private fields

        private readonly object _dataSourceLock = new object();
        private IProvider _dataSource;
        private Collection<IGeometry> _geometries;

        #endregion

        static readonly ILog logger = LogManager.GetLogger(typeof(BaseVectorLayer<TGeometry>));
        
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The name of the layer</param>
        /// <param name="dataSource">The data source</param>
        /// <param name="symbolizer">The symbolizer</param>
        protected BaseVectorLayer(string layerName, IProvider dataSource, ISymbolizer<TGeometry> symbolizer)
        {
            LayerName = layerName;
            _dataSource = dataSource;
            Symbolizer = symbolizer;
        }

        /// <summary>
        /// Gets or sets whether smoothing (antialiasing) is applied to lines and curves and the edges of filled areas
        /// </summary>
        [Obsolete("Use Symbolizer.SmoothingMode")]
        public SmoothingMode SmoothingMode
        {
            get { return Symbolizer.SmoothingMode; }
            set { Symbolizer.SmoothingMode = value; }
        }

        /// <summary>
        /// Gets or sets the datasource
        /// </summary>
        public IProvider DataSource
        {
            get { return _dataSource; }
            set
            {
                lock (_dataSourceLock)
                {
                    _dataSource = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the symbolizer associated with this layer.
        /// </summary>
        public ISymbolizer<TGeometry> Symbolizer { get; set; }

        /// <summary>
        /// Method to render the layer upon the graphics object <paramref name="g"/> using the map <paramref name="map"/>
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        public override void Render(Graphics g, Map map)
        {
            // Map setup correctly
            if (map.Center == null)
                throw (new ApplicationException("Cannot render map. View center not specified"));

            //Data source set?
            if (DataSource == null)
                throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

            // Symbolizer set
            if (Symbolizer == null)
                throw new ApplicationException("Symbolizer property not set on layer '" + LayerName + "'");

            // Initialize Rendering
            OnRender(g, map);
            
            // Render
            OnRendering(g, map);

            // Terminate Rendering
            OnRendered(g, map);
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override Envelope Envelope
        {
            get
            {
                if (DataSource == null)
                    throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

                Envelope box;
                lock (_dataSourceLock)
                {
                    var wasOpen = _dataSource.IsOpen;
                    if (!wasOpen)
                        DataSource.Open();
                    box = _dataSource.GetExtents();
                    if (!wasOpen) //Restore state
                        _dataSource.Close();
                }
                if (CoordinateTransformation != null)
#if !DotSpatialProjections
                    return GeometryTransform.TransformBox(box, CoordinateTransformation.MathTransform);
#else
                    return GeometryTransform.TransformBox(box, CoordinateTransformation.Source, CoordinateTransformation.Target);
#endif
                return box;
            }
        }

        /// <summary>
        /// Method called to initialize the rendering process
        /// </summary>
        /// <param name="graphics">The graphics object to render upon</param>
        /// <param name="map">The map</param>
        protected virtual void OnRender(Graphics graphics, Map map)
        {
            // Get query envelope
            var envelope = map.Envelope;

            // Convert bounding box to datasource's coordinate system
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                if (ReverseCoordinateTransformation != null)
                {
                    envelope = GeometryTransform.TransformBox(envelope, ReverseCoordinateTransformation.MathTransform);
                }
                else
                {
                    CoordinateTransformation.MathTransform.Invert();
                    envelope = GeometryTransform.TransformBox(envelope, CoordinateTransformation.MathTransform);
                    CoordinateTransformation.MathTransform.Invert();
                }
#else
                envelope = GeometryTransform.TransformBox(envelope, CoordinateTransformation.Target, CoordinateTransformation.Source);
#endif
            }

            lock (_dataSource)
            {
                var wasOpen = _dataSource.IsOpen;
                if (!_dataSource.IsOpen) _dataSource.Open();

                _geometries = DataSource.GetGeometriesInView(envelope);

                if (logger.IsDebugEnabled)
                    logger.DebugFormat("Layer {0}, NumGeometries {1}", LayerName, _geometries.Count);

                if (!wasOpen)
                    _dataSource.Close();
            }

            //Setting up the Symbolizer
            Symbolizer.Begin(graphics, map, 0);
        }

        /// <summary>
        /// Method called to render the layer
        /// </summary>
        /// <param name="graphics">The graphics object to render upon</param>
        /// <param name="map">The map</param>
        protected virtual void OnRendering(Graphics graphics, Map map)
        {
            foreach (var geometry in _geometries)
            {
                if (geometry != null)
                    Symbolizer.Render(map, geometry as TGeometry, graphics);
            }
            Symbolizer.Symbolize(graphics, map);
        }

        /// <summary>
        /// Method called to signal that the layer has been rendered
        /// </summary>
        /// <param name="graphics">The graphics object to render upon</param>
        /// <param name="map">The map</param>
        protected virtual void OnRendered(Graphics graphics, Map map)
        {
            Symbolizer.End(graphics, map);
        }

        /// <summary>
        /// Gets or sets the <see cref="LabelLayer"/> associated with the vector layer
        /// </summary>
        public LabelLayer LabelLayer { get; set; }

        /// <summary>
        /// Release all managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            Symbolizer = null;
            _dataSource = null;
            _geometries = null;
            base.ReleaseManagedResources();
        }


        #region Implementation of ICanQueryLayer

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                if (ReverseCoordinateTransformation != null)
                {
                    box = GeometryTransform.TransformBox(box, ReverseCoordinateTransformation.MathTransform);
                }
                else
                {
                    CoordinateTransformation.MathTransform.Invert();
                    box = GeometryTransform.TransformBox(box, CoordinateTransformation.MathTransform);
                    CoordinateTransformation.MathTransform.Invert();
                }
#else
                box = GeometryTransform.TransformBox(box, CoordinateTransformation.Target, CoordinateTransformation.Source);
#endif
            }

            lock (_dataSource)
            {
                _dataSource.Open();
                _dataSource.ExecuteIntersectionQuery(box, ds);
                _dataSource.Close();
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                if (ReverseCoordinateTransformation != null)
                {
                    geometry = GeometryTransform.TransformGeometry(geometry, ReverseCoordinateTransformation.MathTransform,
                            GeometryServiceProvider.Instance.CreateGeometryFactory((int)ReverseCoordinateTransformation.TargetCS.AuthorityCode));
                }
                else
                {
                    CoordinateTransformation.MathTransform.Invert();
                    geometry = GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.MathTransform,
                            GeometryServiceProvider.Instance.CreateGeometryFactory((int)CoordinateTransformation.SourceCS.AuthorityCode));
                    CoordinateTransformation.MathTransform.Invert();
                }
#else
                geometry = GeometryTransform.TransformGeometry(geometry, 
                    CoordinateTransformation.Target, 
                    CoordinateTransformation.Source, 
                    CoordinateTransformation.SourceFactory);
#endif
            }

            lock (_dataSource)
            {
                _dataSource.Open();
                _dataSource.ExecuteIntersectionQuery(geometry, ds);
                _dataSource.Close();
            }
        }

        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE
        /// </summary>
        public bool IsQueryEnabled { get; set; }

        #endregion

    }
}