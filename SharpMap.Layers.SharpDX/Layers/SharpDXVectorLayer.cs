using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Common.Logging;
using GeoAPI.Geometries;
using SharpDX;
using SharpDX.Direct2D1;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers.Styles;
using SharpMap.Rendering;
using SharpMap.Styles;
using D2D1Factory = SharpDX.Direct2D1.Factory;
using D2D1Bitmap = SharpDX.Direct2D1.Bitmap;

namespace SharpMap.Layers
{
    /// <summary>
    /// A vector layer that uses SharpDX managed wrapper for DirectX
    /// </summary>
    public class SharpDXVectorLayer : VectorLayer
    {
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly D2D1Factory _d2d1Factory;

        private readonly object _syncRoot;
        private IRenderTargetFactory _renderTargetFactory;

        /// <summary>
        /// Gets or sets a value indicating the anti-alias mode.
        /// </summary>
        public AntialiasMode AntialiasMode { get; set; } 

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layername">The layer name</param>
        public SharpDXVectorLayer(string layername)
            :this(layername, null)
        {
        }


        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layername">The layer name</param>
        /// <param name="datasource">The data source</param>
        public SharpDXVectorLayer(string layername, IProvider datasource)
            : base(layername, datasource)
        {
            _syncRoot = new object();

            _d2d1Factory = new D2D1Factory(FactoryType.SingleThreaded);
            AntialiasMode = AntialiasMode.PerPrimitive;
        }

        protected override void ReleaseManagedResources()
        {
            _d2d1Factory.Dispose();
            if (_renderTargetFactory is IDisposable)
                ((IDisposable)_renderTargetFactory).Dispose();
            _renderTargetFactory = null;

            base.ReleaseManagedResources();
        }


        /// <summary>
        /// Gets or sets a value indicating the render target factory
        /// </summary>
        public IRenderTargetFactory RenderTargetFactory
        {
            get
            {
                return _renderTargetFactory ?? (_renderTargetFactory = new WICRenderTargetFactory());
            }
            set
            {
                _renderTargetFactory  = value;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Render(Graphics g, Map map)
        {
            if (map.Center == null)
                throw (new ApplicationException("Cannot render map. View center not specified"));

            g.SmoothingMode = SmoothingMode;
            var envelope = ToSource(map.Envelope); //View to render

            if (DataSource == null)
                throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

            // Get the transform
            var transform = new Matrix3x2(g.Transform.Elements);

            // Save state of the graphics object
            var gs = g.Save();

            // Create and prepare the render target
            var rt = RenderTargetFactory.Create(_d2d1Factory, g, map);

            // Set anti-alias mode and transform
            rt.AntialiasMode = AntialiasMode;
            rt.Transform = transform;

            if (Theme != null)
                RenderInternal(_d2d1Factory, rt, map, envelope, Theme);
            else
                RenderInternal(_d2d1Factory, rt, map, envelope);

            // Clean up the render target
            RenderTargetFactory.CleanUp(rt, g, map);
            
            // Restore the graphics object
            g.Restore(gs);

            // Invoke LayerRendered event
            OnLayerRendered(g);
        }


        private void RenderInternal(D2D1Factory factory, RenderTarget rt, Map map, Envelope envelope)
        {
            //if style is not enabled, we don't need to render anything
            if (!Style.Enabled)
                return;

            var geoms = GetGeometriesInView(envelope);
            if (geoms.Count == 0) return;

            var stylesToRender = GetStylesToRender(Style);

            foreach (var style in stylesToRender)
            {
                if (style is VectorStyle)
                {
                    using (var tmp = SharpDXVectorStyle.FromVectorStyle(rt, factory, (VectorStyle)style))
                    {
                        //Linestring outlines is drawn by drawing the layer once with a thicker line
                        //before drawing the "inline" on top.
                        if (tmp.EnableOutline)
                        {
                            foreach (var geom in geoms)
                            {
                                if (geom != null)
                                {
                                    //Draw background of all line-outlines first
                                    if (geom is ILineString)
                                        SharpDXVectorRenderer.DrawLineString(rt, factory, geom as ILineString, tmp.Outline, tmp.OutlineWidth, tmp.OutlineStrokeStyle, map, tmp.LineOffset);
                                    else if (geom is IMultiLineString)
                                        SharpDXVectorRenderer.DrawMultiLineString(rt, factory, geom as IMultiLineString, tmp.Outline, tmp.OutlineWidth, tmp.OutlineStrokeStyle, map, tmp.LineOffset);
                                }
                            }
                        }

                        foreach (IGeometry geom in geoms)
                        {
                            if (geom != null)
                                RenderGeometry(factory, rt, map, geom, tmp);
                        }


                    }
                }
                else
                {
                    lock(_syncRoot)
                        _logger.Debug(fmh => fmh("Not a VectorStyle!"));
                }
            }
        }

        private void RenderGeometry(D2D1Factory factory, RenderTarget g, Map map, IGeometry feature, SharpDXVectorStyle style)
        {
            if (feature == null)
                return;

            var geometryType = feature.OgcGeometryType;
            switch (geometryType)
            {
                case OgcGeometryType.Polygon:
                    if (style.EnableOutline)
                        SharpDXVectorRenderer.DrawPolygon(g, factory, (IPolygon)feature, style.Fill, style.Outline, style.OutlineWidth, style.OutlineStrokeStyle, ClippingEnabled, map);
                    else
                        SharpDXVectorRenderer.DrawPolygon(g, factory, (IPolygon)feature, style.Fill, null, 0f, null, ClippingEnabled,  map);
                    break;
                case OgcGeometryType.MultiPolygon:
                    if (style.EnableOutline)
                        SharpDXVectorRenderer.DrawMultiPolygon(g, factory, (IMultiPolygon)feature, style.Fill, style.Outline, style.OutlineWidth, style.OutlineStrokeStyle,
                                                        ClippingEnabled, map);
                    else
                        SharpDXVectorRenderer.DrawMultiPolygon(g, factory, (IMultiPolygon)feature, style.Fill, null, 0f, null, ClippingEnabled,
                                                        map);
                    break;
                case OgcGeometryType.LineString:
                    SharpDXVectorRenderer.DrawLineString(g, factory, (ILineString)feature, style.Line, style.LineWidth, style.LineStrokeStyle, map, style.LineOffset);
                    return;
                case OgcGeometryType.MultiLineString:
                    SharpDXVectorRenderer.DrawMultiLineString(g, factory, (IMultiLineString)feature, style.Line, style.LineWidth, style.LineStrokeStyle, map, style.LineOffset);
                    break;
                case OgcGeometryType.Point:
                    if (style.Symbol != null || style.PointColor == null)
                    {
                        SharpDXVectorRenderer.DrawPoint(g, factory, (IPoint)feature, style.Symbol, style.SymbolOffset,
                                                 style.SymbolRotation, map);
                        return;
                    }
                    SharpDXVectorRenderer.DrawPoint(g, factory, (IPoint)feature, style.PointColor, style.PointSize, style.SymbolOffset, map);

                    break;
                case OgcGeometryType.MultiPoint:
                    if (style.Symbol != null || style.PointColor == null)
                    {
                        SharpDXVectorRenderer.DrawMultiPoint(g, factory, (IMultiPoint)feature, style.Symbol,
                                                  style.SymbolOffset, style.SymbolRotation, map);
                    }
                    else
                    {
                        SharpDXVectorRenderer.DrawMultiPoint(g, factory, (IMultiPoint)feature, style.PointColor, style.PointSize, style.SymbolOffset, map);
                    }
                    break;
                case OgcGeometryType.GeometryCollection:
                    var coll = (IGeometryCollection)feature;
                    for (var i = 0; i < coll.NumGeometries; i++)
                    {
                        IGeometry geom = coll[i];
                        RenderGeometry(factory, g, map, geom, style);
                    }
                    break;
                default:
                    lock (_syncRoot)
                        _logger.Debug( fmh => fmh("Unhandled geometry: {0}", feature.OgcGeometryType));
                    break;
            }
        }

        private void RenderInternal(D2D1Factory factory, RenderTarget rt, Map map, 
            Envelope envelope, Rendering.Thematics.ITheme theme)
        {
            var ds = new FeatureDataSet();
            lock (_syncRoot)
            {
                DataSource.Open();
                DataSource.ExecuteIntersectionQuery(envelope, ds);
                DataSource.Close();
            }

            var scale = map.MapScale;
            var zoom = map.Zoom;

            foreach (FeatureDataTable features in ds.Tables)
            {
                // Transform geometries if necessary
                if (CoordinateTransformation != null)
                {
                    for (var i = 0; i < features.Count; i++)
                    {
                        features[i].Geometry = ToTarget(features[i].Geometry);
                    }
                }

                //Linestring outlines is drawn by drawing the layer once with a thicker line
                //before drawing the "inline" on top.
                if (Style.EnableOutline)
                {
                    for (int i = 0; i < features.Count; i++)
                    {
                        var feature = features[i];
                        var outlineStyle = theme.GetStyle(feature) as VectorStyle;
                        if (outlineStyle == null) continue;
                        if (!(outlineStyle.Enabled && outlineStyle.EnableOutline)) continue;

                        double compare = outlineStyle.VisibilityUnits == VisibilityUnits.ZoomLevel ? zoom : scale;

                        if (!(outlineStyle.MinVisible <= compare && compare <= outlineStyle.MaxVisible)) continue;

                        using (var sdxStyle = SharpDXVectorStyle.FromVectorStyle(rt, factory, outlineStyle))
                        {
                            if (sdxStyle != null)
                            {
                                //Draw background of all line-outlines first
                                if (feature.Geometry is ILineString)
                                {
                                    SharpDXVectorRenderer.DrawLineString(rt, factory, (ILineString)feature.Geometry,
                                        sdxStyle.Outline, sdxStyle.OutlineWidth, sdxStyle.OutlineStrokeStyle,
                                        map, sdxStyle.LineOffset);
                                }
                                else if (feature.Geometry is IMultiLineString)
                                {
                                    SharpDXVectorRenderer.DrawMultiLineString(rt, factory, (IMultiLineString)feature.Geometry,
                                        sdxStyle.Outline, sdxStyle.OutlineWidth, sdxStyle.OutlineStrokeStyle,
                                        map, sdxStyle.LineOffset);
                                }
                            }
                        }
                    }
                }


                var sdxVectorStyles = new Dictionary<VectorStyle, SharpDXVectorStyle>();
                for (var i = 0; i < features.Count; i++)
                {
                    var feature = features[i];
                    var style = theme.GetStyle(feature);
                    if (style == null) continue;
                    if (!style.Enabled) continue;

                    var compare = style.VisibilityUnits == VisibilityUnits.ZoomLevel ? zoom : scale;

                    if (!(style.MinVisible <= compare && compare <= style.MaxVisible)) continue;


                    IEnumerable<IStyle> stylesToRender = GetStylesToRender(style);

                    if (stylesToRender == null)
                        return;

                    foreach (var styleToRender in stylesToRender)
                    {
                        if (!styleToRender.Enabled) continue;
                        if (!(styleToRender is VectorStyle)) continue;
                        if (!(style.MinVisible <= compare && compare <= style.MaxVisible)) continue;

                        var vstyle = (VectorStyle) styleToRender;
                        SharpDXVectorStyle sdxStyle;
                        if (!sdxVectorStyles.TryGetValue(vstyle, out sdxStyle))
                        {
                            sdxStyle = SharpDXVectorStyle.FromVectorStyle(rt, factory, vstyle);
                            sdxVectorStyles.Add(vstyle, sdxStyle);
                        }

                        RenderGeometry(factory, rt, map, feature.Geometry, sdxStyle);
                    }
                }

                foreach (var value in sdxVectorStyles.Values)
                    value.Dispose();
            }
        }

        private Collection<IGeometry> GetGeometriesInView(Envelope envelope)
        {
            Collection<IGeometry> geoms;

            // Is datasource already open?
            lock (_syncRoot)
            {
                var alreadyOpen = DataSource.IsOpen;

                // If not open yet, open it
                if (!alreadyOpen) { DataSource.Open(); }

                // Read data
                geoms = DataSource.GetGeometriesInView(envelope);

                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Layer {0}, NumGeometries {1}", LayerName, geoms.Count);
                }

                // If was not open, close it
                if (!alreadyOpen) { DataSource.Close(); }
            }

            // Transform geometries if necessary
            if (CoordinateTransformation != null)
            {
                for (var i = 0; i < geoms.Count; i++)
                {
                    geoms[i] = ToTarget(geoms[i]);
                }
            }

            return geoms;
        }
    }
}
