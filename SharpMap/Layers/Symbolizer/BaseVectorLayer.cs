using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
#if !DotSpatialProjections
using ProjNet.CoordinateSystems.Transformations;
#else
using DotSpatial.Projections
#endif
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Geometries;
using SharpMap.Rendering.Symbolizer;

namespace SharpMap.Layers.Symbolizer
{
    /// <summary>
    /// Base class for all vector layers using <see cref="ISymbolizer{TGeometry}"/> approach.
    /// </summary>
    /// <typeparam name="TGeometry">The geometry type</typeparam>
    public abstract class BaseVectorLayer<TGeometry> : Layer, ICanQueryLayer, IDisposable
        where TGeometry : class, IGeometryClassifier
    {
        #region Private fields

        private IProvider _dataSource;

        #endregion

        protected BaseVectorLayer(string layerName, IProvider dataSource, ISymbolizer<TGeometry> symbolizer)
        {
            LayerName = layerName;
            _dataSource = dataSource;
            Symbolizer = symbolizer;
        }

        /// <summary>
        /// Gets or sets whether smoothing (antialiasing) is applied to lines and curves and the edges of filled areas
        /// </summary>
        public SmoothingMode SmoothingMode { get; set; }

        /// <summary>
        /// Gets or sets the datasource
        /// </summary>
        public IProvider DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        /// <summary>
        /// Gets or sets the symbolizer associated with this layer.
        /// </summary>
        public ISymbolizer<TGeometry> Symbolizer { get; set; }

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

            /*
            //If thematics is enabled, we use a slighty different rendering approach
            if (Theme != null)
                RenderInternal(g, map, envelope, Theme);
            else
                RenderInternal(g, map, envelope);


            base.Render(g, map);
            */
            // Terminate Rendering
            OnRendered(g, map);
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override BoundingBox Envelope
        {
            get
            {
                if (DataSource == null)
                    throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));
                
                BoundingBox box;
                lock (_dataSource)
                {
                    bool wasOpen = DataSource.IsOpen;
                    if (!wasOpen)
                        DataSource.Open();
                    box = DataSource.GetExtents();
                    if (!wasOpen) //Restore state
                        DataSource.Close();
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

        private SmoothingMode _oldSmoothingMode;
        private Collection<Geometry> _geometries;

        /// <summary>
        /// Method called to initialize the rendering process
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="map"></param>
        protected virtual void OnRender(Graphics graphics, Map map)
        {
            // Get query envelope
            BoundingBox envelope = map.Envelope;

            // Convert bounding box to datasource's coordinate system
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                CoordinateTransformation.MathTransform.Invert();
                envelope = GeometryTransform.TransformBox(envelope, CoordinateTransformation.MathTransform);
                CoordinateTransformation.MathTransform.Invert();
#else
                envelope = GeometryTransform.TransformBox(envelope, CoordinateTransformation.Target, CoordinateTransformation.Source);
#endif
            }

            lock (_dataSource)
            {
                bool wasOpen = _dataSource.IsOpen;
                if (!_dataSource.IsOpen) _dataSource.Open();

                _geometries = DataSource.GetGeometriesInView(envelope);
                Console.Out.WriteLine(string.Format("Layer {0}, NumGeometries {1}", LayerName, _geometries.Count));
                if (!wasOpen)
                    _dataSource.Close();
            }

            _oldSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode;
            
            //Setting up the Symbolizer
            Symbolizer.Begin(graphics, map, 0);
        }

        protected virtual void OnRendering(Graphics graphics, Map map)
        {
            
            //lock (Symbolizer)
            //{
            //    Action<Map, TGeometry, Graphics> a = Symbolizer.Render;
            //    Parallel.ForEach(_geometrys, a)
            //}
            //Parallel.ForEach()
            //while (true)
            //{
            //    AttributedGeometry<TGeometry> ag = _geometrys.Dequeue();
            //    Symbolizer.Render(map, ag.Geometry, graphics);
            //}
            foreach (Geometry geometry in _geometries)
            {
                Symbolizer.Render(map, geometry as TGeometry, graphics);
            }
            Symbolizer.Symbolize(graphics, map);
        }

        protected virtual void OnRendered(Graphics graphics, Map map)
        {
            Symbolizer.End(graphics, map);
            graphics.SmoothingMode = _oldSmoothingMode;
        }

        /// <summary>
        /// Gets or sets the <see cref="LabelLayer"/> associated with the vector layer
        /// </summary>
        public LabelLayer LabelLayer { get; set; }

        #region IDisposable Members

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            /*
            if (_dataSource != null)
                DataSource.Dispose();
             */
        }

        #endregion


        #region Implementation of ICanQueryLayer

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                CoordinateTransformation.MathTransform.Invert();
                box = GeometryTransform.TransformBox(box, CoordinateTransformation.MathTransform);
                CoordinateTransformation.MathTransform.Invert();
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
        public void ExecuteIntersectionQuery(Geometry geometry, FeatureDataSet ds)
        {
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                CoordinateTransformation.MathTransform.Invert();
                geometry = GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.MathTransform);
                CoordinateTransformation.MathTransform.Invert();
#else
                geometry = GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.Target, CoordinateTransformation.Source);
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