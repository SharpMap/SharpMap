// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using NetTopologySuite.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SharpMap.Layers
{
    /// <summary>
    /// Class for vector layer properties
    /// </summary>
    [Serializable]
    public class VectorLayer : Layer, ICanQueryLayer, ICloneable
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(VectorLayer));

        private bool _clippingEnabled;
        private bool _isQueryEnabled = true;
        private IBaseProvider _dataSource;
        private SmoothingMode _smoothingMode;
        private ITheme _theme;
        private Envelope _envelope;

        /// <summary>
        /// Initializes a new layer
        /// </summary>
        /// <param name="layername">Name of layer</param>
        public VectorLayer(string layername)
            : base(new VectorStyle())
        {
            LayerName = layername;
            SmoothingMode = SmoothingMode.AntiAlias;
        }

        /// <summary>
        /// Initializes a new layer with a specified datasource
        /// </summary>
        /// <param name="layername">Name of layer</param>
        /// <param name="dataSource">Data source</param>
        public VectorLayer(string layername, IBaseProvider dataSource) : this(layername)
        {
            _dataSource = dataSource;
        }
        /// <summary>
        /// Gets or sets a Dictionary with themes suitable for this layer. A theme in the dictionary can be used for rendering be setting the Theme Property using a delegate function
        /// </summary>
        public Dictionary<string, ITheme> Themes
        {
            get;
            set;
        }



        /// <summary>
        /// Gets or sets thematic settings for the layer. Set to null to ignore thematics
        /// </summary>
        public ITheme Theme
        {
            get { return _theme; }
            set { _theme = value; }
        }

        /// <summary>
        /// Specifies whether polygons should be clipped prior to rendering
        /// </summary>
        /// <remarks>
        /// <para>Clipping will clip <see cref="GeoAPI.Geometries.Polygon"/> and
        /// <see cref="GeoAPI.Geometries.MultiPolygon"/> to the current view prior
        /// to rendering the object.</para>
        /// <para>Enabling clipping might improve rendering speed if you are rendering 
        /// only small portions of very large objects.</para>
        /// </remarks>
        public bool ClippingEnabled
        {
            get { return _clippingEnabled; }
            set { _clippingEnabled = value; }
        }

        /// <summary>
        /// Render whether smoothing (antialiasing) is applied to lines and curves and the edges of filled areas
        /// </summary>
        public SmoothingMode SmoothingMode
        {
            get { return _smoothingMode; }
            set { _smoothingMode = value; }
        }

        /// <summary>
        /// Gets or sets the datasource
        /// </summary>
        public IBaseProvider DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                _envelope = null;
            }
        }

        /// <summary>
        /// Gets or sets the rendering style of the vector layer.
        /// </summary>
        public new VectorStyle Style
        {
            get { return base.Style as VectorStyle; }
            set { base.Style = value; }
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

                if (_envelope != null && CacheExtent)
                    return ToTarget(_envelope.Copy());

                Envelope box;
                lock (_dataSource)
                {
                    // Is datasource already open?
                    bool wasOpen = DataSource.IsOpen;
                    if (!wasOpen) { DataSource.Open(); }

                    box = DataSource.GetExtents();

                    if (!wasOpen) { DataSource.Close(); }
                }

                if (CacheExtent)
                    _envelope = box;

                return ToTarget(box);
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the layer envelope should be treated as static or not.
        /// </summary>
        /// <remarks>
        /// When CacheExtent is enabled the layer Envelope will be calculated only once from DataSource, this 
        /// helps to speed up the Envelope calculation with some DataProviders. Default is false for backward
        /// compatibility.
        /// </remarks>
        public virtual bool CacheExtent { get; set; }

        /// <summary>
        /// Gets or sets the SRID of this VectorLayer's data source
        /// </summary>
        public override int SRID
        {
            get
            {
                if (DataSource == null)
                    throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

                return DataSource.SRID;
            }
            set
            {
                DataSource.SRID = value;
                base.SRID = value;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the object
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (DataSource != null)
                DataSource.Dispose();
            base.ReleaseManagedResources();
        }

        #endregion

        /// <summary>
        /// Renders the layer to a graphics object, using the given map viewport
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="mvp">Map view port</param>
        public override void Render(Graphics g, MapViewport mvp)
        {
            if (mvp.Center == null)
                throw (new ApplicationException("Cannot render map. View center not specified"));

            g.SmoothingMode = SmoothingMode;
            var envelope = ToSource(mvp.Envelope); //View to render

            if (DataSource == null)
                throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

            //If thematics is enabled, we use a slighty different rendering approach
            if (Theme != null)
                RenderInternal(g, mvp, envelope, Theme);
            else
                RenderInternal(g, mvp, envelope);


            //OnLayerRendered(g);
            // Obsolete (and will cause infinite loop)
            base.Render(g, mvp);
        }

        /// <summary>
        /// Method to render this layer to the map, applying <paramref name="theme"/>.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map object</param>
        /// <param name="envelope">The envelope to render</param>
        /// <param name="theme">The theme to apply</param>
        protected void RenderInternal(Graphics g, MapViewport map, Envelope envelope, ITheme theme)
        {
            var canvasArea = RectangleF.Empty;
            var combinedArea = RectangleF.Empty;

            var ds = new FeatureDataSet();
            lock (_dataSource)
            {
                // Is datasource already open?
                bool wasOpen = DataSource.IsOpen;
                if (!wasOpen) { DataSource.Open(); }

                DataSource.ExecuteIntersectionQuery(envelope, ds);

                if (!wasOpen) { DataSource.Close(); }
            }

            double scale = map.GetMapScale((int)g.DpiX);
            double zoom = map.Zoom;

            Func<MapViewport, FeatureDataRow, IStyle> evalStyle;


            if (theme is IThemeEx)
                evalStyle = new ThemeExEvaluator((IThemeEx)theme).GetStyle;
            else
                evalStyle = new ThemeEvaluator(theme).GetStyle;

            foreach (FeatureDataTable features in ds.Tables)
            {
                // Transform geometries if necessary
                if (CoordinateTransformation != null)
                {
                    for (int i = 0; i < features.Count; i++)
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
                        var outlineStyle = evalStyle(map, feature) as VectorStyle;
                        if (outlineStyle == null) continue;
                        if (!(outlineStyle.Enabled && outlineStyle.EnableOutline)) continue;

                        var compare = outlineStyle.VisibilityUnits == VisibilityUnits.ZoomLevel ? zoom : scale;

                        if (!(outlineStyle.MinVisible <= compare && compare <= outlineStyle.MaxVisible)) continue;

                        using (outlineStyle = outlineStyle.Clone())
                        {
                            if (outlineStyle != null)
                            {
                                //Draw background of all line-outlines first
                                if (feature.Geometry is LineString)
                                {
                                    canvasArea = VectorRenderer.DrawLineStringEx(g, feature.Geometry as LineString, outlineStyle.Outline,
                                                                        map, outlineStyle.LineOffset);
                                }
                                else if (feature.Geometry is MultiLineString)
                                {
                                    canvasArea = VectorRenderer.DrawMultiLineStringEx(g, feature.Geometry as MultiLineString,
                                                                        outlineStyle.Outline, map, outlineStyle.LineOffset);
                                }
                                combinedArea = canvasArea.ExpandToInclude(combinedArea);
                            }
                        }
                    }
                }


                for (int i = 0; i < features.Count; i++)
                {
                    var feature = features[i];
                    var style = evalStyle(map, feature);
                    if (style == null) continue;
                    if (!style.Enabled) continue;

                    double compare = style.VisibilityUnits == VisibilityUnits.ZoomLevel ? zoom : scale;

                    if (!(style.MinVisible <= compare && compare <= style.MaxVisible)) continue;


                    IEnumerable<IStyle> stylesToRender = GetStylesToRender(style);

                    if (stylesToRender == null)
                        return;

                    foreach (var vstyle in stylesToRender)
                    {
                        if (!(vstyle is VectorStyle) || !vstyle.Enabled)
                            continue;

                        using (var clone = (vstyle as VectorStyle).Clone())
                        {
                            if (clone != null)
                            {
                                canvasArea = RenderGeometryEx(g, map, feature.Geometry, clone);
                                combinedArea = canvasArea.ExpandToInclude(combinedArea);
                            }
                        }
                    }
                }
            }

            CanvasArea = combinedArea;
        }

        /// <summary>
        /// Method to render this layer to the map, applying <see cref="Style"/>.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map object</param>
        /// <param name="envelope">The envelope to render</param>
        protected void RenderInternal(Graphics g, MapViewport map, Envelope envelope)
        {
            //if style is not enabled, we don't need to render anything
            if (!Style.Enabled) return;

            IEnumerable<IStyle> stylesToRender = GetStylesToRender(Style);

            if (stylesToRender == null)
                return;

            var canvasArea = RectangleF.Empty;
            var combinedArea = RectangleF.Empty;

            Collection<Geometry> geoms = null;

            foreach (var style in stylesToRender)
            {
                if (!(style is VectorStyle) || !style.Enabled)
                    continue;
                using (var vStyle = (style as VectorStyle).Clone())
                {
                    if (vStyle != null)
                    {
                        if (geoms == null)
                        {
                            lock (_dataSource)
                            {
                                // Is datasource already open?
                                bool wasOpen = DataSource.IsOpen;
                                if (!wasOpen) { DataSource.Open(); }

                                // Read data
                                geoms = DataSource.GetGeometriesInView(envelope);

                                if (!wasOpen) { DataSource.Close(); }
                            }

                            if (_logger.IsDebugEnabled)
                            {
                                _logger.DebugFormat("Layer {0}, NumGeometries {1}", LayerName, geoms.Count);
                            }

                            // Transform geometries if necessary
                            if (CoordinateTransformation != null)
                            {
                                for (int i = 0; i < geoms.Count; i++)
                                {
                                    geoms[i] = ToTarget(geoms[i]);
                                }
                            }
                        }

                        if (vStyle.LineSymbolizer != null)
                        {
                            vStyle.LineSymbolizer.Begin(g, map, geoms.Count);
                        }
                        else
                        {
                            //Linestring outlines is drawn by drawing the layer once with a thicker line
                            //before drawing the "inline" on top.
                            if (vStyle.EnableOutline)
                            {
                                foreach (var geom in geoms)
                                {
                                    if (geom != null)
                                    {
                                        //Draw background of all line-outlines first
                                        if (geom is LineString)
                                            canvasArea = VectorRenderer.DrawLineStringEx(g, geom as LineString, vStyle.Outline, map, vStyle.LineOffset);
                                        else if (geom is MultiLineString)
                                            canvasArea = VectorRenderer.DrawMultiLineStringEx(g, geom as MultiLineString, vStyle.Outline, map, vStyle.LineOffset);
                                        combinedArea = canvasArea.ExpandToInclude(combinedArea);
                                    }
                                }
                            }
                        }

                        foreach (Geometry geom in geoms)
                        {
                            if (geom != null)
                            {
                                canvasArea = RenderGeometryEx(g, map, geom, vStyle);
                                combinedArea = canvasArea.ExpandToInclude(combinedArea);
                            }
                        }

                        if (vStyle.LineSymbolizer != null)
                        {
                            vStyle.LineSymbolizer.Symbolize(g, map);
                            vStyle.LineSymbolizer.End(g, map);
                        }
                    }
                }
            }

            CanvasArea = combinedArea;
        }

        /// <summary>
        /// Unpacks styles to render (can be nested group-styles)
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public static IEnumerable<IStyle> GetStylesToRender(IStyle style)
        {
            IStyle[] stylesToRender = null;
            if (style is GroupStyle)
            {
                var gs = style as GroupStyle;
                var styles = new List<IStyle>();
                for (var i = 0; i < gs.Count; i++)
                {
                    styles.AddRange(GetStylesToRender(gs[i]));
                }
                stylesToRender = styles.ToArray();
            }
            else if (style is VectorStyle)
            {
                stylesToRender = new[] { style };
            }

            return stylesToRender;
        }

        /// <summary>
        /// Method to render <paramref name="feature"/> using <paramref name="style"/>
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        /// <param name="feature">The feature's geometry</param>
        /// <param name="style">The style to apply</param>
        protected void RenderGeometry(Graphics g, MapViewport map, Geometry feature, VectorStyle style)
        {
            RenderGeometryEx(g, map, feature, style);
        }

        /// <summary>
        /// Function to render <paramref name="feature"/> using <paramref name="style"/> and returning the area covered.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        /// <param name="feature">The feature's geometry</param>
        /// <param name="style">The style to apply</param>
        protected RectangleF RenderGeometryEx(Graphics g, MapViewport map, Geometry feature, VectorStyle style)
        {
            if (feature == null) return RectangleF.Empty;

            var geometryType = feature.OgcGeometryType;
            switch (geometryType)
            {
                case OgcGeometryType.Polygon:
                    if (style.EnableOutline)
                        return VectorRenderer.DrawPolygonEx(g, (Polygon)feature, style.Fill, style.Outline, _clippingEnabled,
                                                   map);
                    else
                        return VectorRenderer.DrawPolygonEx(g, (Polygon)feature, style.Fill, null, _clippingEnabled, map);

                case OgcGeometryType.MultiPolygon:
                    if (style.EnableOutline)
                        return VectorRenderer.DrawMultiPolygonEx(g, (MultiPolygon)feature, style.Fill, style.Outline, _clippingEnabled, map);
                    return VectorRenderer.DrawMultiPolygonEx(g, (MultiPolygon)feature, style.Fill, null, _clippingEnabled, map);
                case OgcGeometryType.LineString:
                    if (style.LineSymbolizer != null)
                    {
                        style.LineSymbolizer.Render(map, (LineString)feature, g);
                        return RectangleF.Empty;
                    }
                    return VectorRenderer.DrawLineStringEx(g, (LineString)feature, style.Line, map, style.LineOffset);

                case OgcGeometryType.MultiLineString:
                    if (style.LineSymbolizer != null)
                    {
                        style.LineSymbolizer.Render(map, (MultiLineString)feature, g);
                        return RectangleF.Empty;
                    }
                    return VectorRenderer.DrawMultiLineStringEx(g, (MultiLineString)feature, style.Line, map, style.LineOffset);


                case OgcGeometryType.Point:
                    if (style.PointSymbolizer != null)
                        return VectorRenderer.DrawPointEx(style.PointSymbolizer, g, (NetTopologySuite.Geometries.Point)feature, map);

                    if (style.Symbol != null || style.PointColor == null)
                        return VectorRenderer.DrawPointEx(g, (NetTopologySuite.Geometries.Point)feature, style.Symbol, style.SymbolScale, style.SymbolOffset,
                                                 style.SymbolRotation, map);

                    return VectorRenderer.DrawPointEx(g, (NetTopologySuite.Geometries.Point)feature, style.PointColor, style.PointSize, style.SymbolOffset, map);

                case OgcGeometryType.MultiPoint:
                    if (style.PointSymbolizer != null)
                        return VectorRenderer.DrawMultiPointEx(style.PointSymbolizer, g, (MultiPoint)feature, map);

                    if (style.Symbol != null || style.PointColor == null)
                        return VectorRenderer.DrawMultiPointEx(g, (MultiPoint)feature, style.Symbol, style.SymbolScale, style.SymbolOffset, style.SymbolRotation, map);

                    return VectorRenderer.DrawMultiPointEx(g, (MultiPoint)feature, style.PointColor, style.PointSize, style.SymbolOffset, map);

                case OgcGeometryType.GeometryCollection:
                    var coll = (GeometryCollection)feature;
                    var combinedArea = RectangleF.Empty;
                    for (var i = 0; i < coll.NumGeometries; i++)
                    {
                        Geometry geom = coll[i];
                        var canvasArea = RenderGeometryEx(g, map, geom, style);
                        combinedArea = canvasArea.ExpandToInclude(combinedArea);
                    }
                    return combinedArea;

            }
            throw new NotSupportedException();
        }

        #region Implementation of ICanQueryLayer

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            box = ToSource(box);

            int tableCount = ds.Tables.Count;

            lock (_dataSource)
            {
                // Is datasource already open?
                bool wasOpen = _dataSource.IsOpen;
                if (!wasOpen) { _dataSource.Open(); }

                _dataSource.ExecuteIntersectionQuery(box, ds);

                if (!wasOpen) { DataSource.Close(); }
            }

            if (ds.Tables.Count > tableCount)
            {
                //We added a table, name it according to layer
                ds.Tables[ds.Tables.Count - 1].TableName = LayerName;
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geometry, FeatureDataSet ds)
        {
            geometry = ToSource(geometry);

            int tableCount = ds.Tables.Count;

            lock (_dataSource)
            {
                // Is datasource already open?
                bool wasOpen = DataSource.IsOpen;
                if (!wasOpen) { DataSource.Open(); }

                _dataSource.ExecuteIntersectionQuery(geometry, ds);

                if (!wasOpen) { DataSource.Close(); }
            }

            if (ds.Tables.Count > tableCount)
            {
                //We added a table, name it according to layer
                ds.Tables[ds.Tables.Count - 1].TableName = LayerName;
            }
        }

        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE
        /// </summary>
        public bool IsQueryEnabled
        {
            get { return _isQueryEnabled; }
            set { _isQueryEnabled = value; }
        }

        #endregion

        /// <inheritdoc cref="ICloneable.Clone()"/>>
        public object Clone()
        {
            var res = (VectorLayer)MemberwiseClone();
            res.Style = Style.Clone();
            if (Theme is ICloneable)
                res.Theme = (ITheme)((ICloneable)Theme).Clone();
            return res;
        }

        #region Theme evaluators
        private abstract class ThemeEvaluatorBase
        {
            public abstract IStyle GetStyle(MapViewport mvp, FeatureDataRow feature);
        }

        private class ThemeEvaluator : ThemeEvaluatorBase
        {
            private readonly ITheme _theme;

            public ThemeEvaluator(ITheme theme)
            {
                _theme = theme;
            }
            public sealed override IStyle GetStyle(MapViewport mvp, FeatureDataRow feature)
            {
                return _theme.GetStyle(feature);
            }
        }

        private class ThemeExEvaluator : ThemeEvaluatorBase
        {
            private readonly IThemeEx _theme;

            public ThemeExEvaluator(IThemeEx theme)
            {
                _theme = theme;
            }
            public sealed override IStyle GetStyle(MapViewport mvp, FeatureDataRow feature)
            {
                return _theme.GetStyle(mvp, feature);
            }
        }
        #endregion
    }
}
