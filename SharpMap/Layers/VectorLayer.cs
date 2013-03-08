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

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
#if !DotSpatialProjections
using GeoAPI;
using GeoAPI.CoordinateSystems.Transformations;
#else
using DotSpatial.Projections;
#endif
using SharpMap.Data;
using SharpMap.Data.Providers;
using GeoAPI.Geometries;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using System.Collections.Generic;
using Common.Logging;

namespace SharpMap.Layers
{
    /// <summary>
    /// Class for vector layer properties
    /// </summary>
    [Serializable]
    public class VectorLayer : Layer, ICanQueryLayer
    {
        static ILog logger = LogManager.GetLogger(typeof(VectorLayer));

        private bool _clippingEnabled;
        private bool _isQueryEnabled = true;
        private IProvider _dataSource;
        private SmoothingMode _smoothingMode;
        private ITheme _theme;

        /// <summary>
        /// Initializes a new layer
        /// </summary>
        /// <param name="layername">Name of layer</param>
        public VectorLayer(string layername)
            :base(new VectorStyle())
        {
            LayerName = layername;
            SmoothingMode = SmoothingMode.AntiAlias;
        }

        /// <summary>
        /// Initializes a new layer with a specified datasource
        /// </summary>
        /// <param name="layername">Name of layer</param>
        /// <param name="dataSource">Data source</param>
        public VectorLayer(string layername, IProvider dataSource) : this(layername)
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
        /// <para>Clipping will clip <see cref="GeoAPI.Geometries.IPolygon"/> and
        /// <see cref="GeoAPI.Geometries.IMultiPolygon"/> to the current view prior
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
        public IProvider DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
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
                Envelope box;
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
                {
                    var boxTrans = GeometryTransform.TransformBox(box, CoordinateTransformation.MathTransform);
                    return boxTrans;
                }            
#else
                    return GeometryTransform.TransformBox(box, CoordinateTransformation.Source, CoordinateTransformation.Target);
#endif
                return box;
            }
        }

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
            set { DataSource.SRID = value; }
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
        /// Renders the layer to a graphics object
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        /// 
        public override void Render(Graphics g, Map map)
        {
            if (map.Center == null)
                throw (new ApplicationException("Cannot render map. View center not specified"));

            g.SmoothingMode = SmoothingMode;
            var envelope = map.Envelope; //View to render
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

            if (DataSource == null)
                throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));





            //If thematics is enabled, we use a slighty different rendering approach
            if (Theme != null)
                RenderInternal(g, map, envelope, Theme);
            else
                RenderInternal(g, map, envelope);


            base.Render(g, map);
        }

        /// <summary>
        /// Method to render this layer to the map, applying <paramref name="theme"/>.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map object</param>
        /// <param name="envelope">The envelope to render</param>
        /// <param name="theme">The theme to apply</param>
        protected void RenderInternal(Graphics g, Map map, Envelope envelope, ITheme theme)
        {
            var ds = new FeatureDataSet();
            lock (_dataSource)
            {
                DataSource.Open();
                DataSource.ExecuteIntersectionQuery(envelope, ds);
                DataSource.Close();
            }



            foreach (FeatureDataTable features in ds.Tables)
            {


                if (CoordinateTransformation != null)
                    for (int i = 0; i < features.Count; i++)
#if !DotSpatialProjections
                        features[i].Geometry = GeometryTransform.TransformGeometry(features[i].Geometry,
                                                                                    CoordinateTransformation.
                                                                                        MathTransform,
                                GeometryServiceProvider.Instance.CreateGeometryFactory((int)CoordinateTransformation.TargetCS.AuthorityCode));
#else
                    features[i].Geometry = GeometryTransform.TransformGeometry(features[i].Geometry,
                                                                                CoordinateTransformation.Source,
                                                                                CoordinateTransformation.Target,
                                                                                CoordinateTransformation.TargetFactory);

#endif

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
                        if (!(outlineStyle.MinVisible <= map.Zoom && map.Zoom <= outlineStyle.MaxVisible)) continue;

                        using (outlineStyle = outlineStyle.Clone())
                        {
                            if (outlineStyle != null)
                            {
                                //Draw background of all line-outlines first
                                if (feature.Geometry is ILineString)
                                {
                                    VectorRenderer.DrawLineString(g, feature.Geometry as ILineString, outlineStyle.Outline,
                                                                        map, outlineStyle.LineOffset);
                                }
                                else if (feature.Geometry is IMultiLineString)
                                {
                                    VectorRenderer.DrawMultiLineString(g, feature.Geometry as IMultiLineString,
                                                                        outlineStyle.Outline, map, outlineStyle.LineOffset);
                                }
                            }
                        }
                    }
                }


                for (int i = 0; i < features.Count; i++)
                {
                    var feature = features[i];
                    var style = theme.GetStyle(feature);
                    if (style == null) continue;
                    if (!style.Enabled) continue;
                    if (!(style.MinVisible <= map.Zoom && map.Zoom <= style.MaxVisible)) continue;


                    IStyle[] stylesToRender = GetStylesToRender(style);

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
                                RenderGeometry(g, map, feature.Geometry, clone);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method to render this layer to the map, applying <see cref="Style"/>.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map object</param>
        /// <param name="envelope">The envelope to render</param>
        protected void RenderInternal(Graphics g, Map map, Envelope envelope)
        {
            //if style is not enabled, we don't need to render anything
            if (!Style.Enabled) return;

            IStyle[] stylesToRender = GetStylesToRender(Style);
            
            if (stylesToRender== null)
                return;

            foreach (var style in stylesToRender)
            {
                if (!(style is VectorStyle) || !style.Enabled)
                    continue;
                using (var vStyle = (style as VectorStyle).Clone())
                {
                    if (vStyle != null)
                    {
                        Collection<IGeometry> geoms;
                        // Is datasource already open?
                        lock (_dataSource)
                        {
                            bool alreadyOpen = DataSource.IsOpen;

                            // If not open yet, open it
                            if (!alreadyOpen) { DataSource.Open(); }

                            // Read data
                            geoms = DataSource.GetGeometriesInView(envelope);

                            if (logger.IsDebugEnabled)
                            {
                                logger.DebugFormat("Layer {0}, NumGeometries {1}", LayerName, geoms.Count);
                            }

                            // If was not open, close it
                            if (!alreadyOpen) { DataSource.Close(); }
                        }
                        if (CoordinateTransformation != null)
                            for (int i = 0; i < geoms.Count; i++)
#if !DotSpatialProjections
                                geoms[i] = GeometryTransform.TransformGeometry(geoms[i], CoordinateTransformation.MathTransform,
                                    GeometryServiceProvider.Instance.CreateGeometryFactory((int)CoordinateTransformation.TargetCS.AuthorityCode));
#else
                    geoms[i] = GeometryTransform.TransformGeometry(geoms[i], 
                        CoordinateTransformation.Source, 
                        CoordinateTransformation.Target, 
                        CoordinateTransformation.TargetFactory);
#endif
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
                                        if (geom is ILineString)
                                            VectorRenderer.DrawLineString(g, geom as ILineString, vStyle.Outline, map, vStyle.LineOffset);
                                        else if (geom is IMultiLineString)
                                            VectorRenderer.DrawMultiLineString(g, geom as IMultiLineString, vStyle.Outline, map, vStyle.LineOffset);
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < geoms.Count; i++)
                        {
                            if (geoms[i] != null)
                                RenderGeometry(g, map, geoms[i], vStyle);
                        }

                        if (vStyle.LineSymbolizer != null)
                        {
                            vStyle.LineSymbolizer.Symbolize(g, map);
                            vStyle.LineSymbolizer.End(g, map);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unpacks styles to render (can be nested group-styles)
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        private IStyle[] GetStylesToRender(IStyle style)
        {
            IStyle[] stylesToRender = null;
            if (style is GroupStyle)
            {
                var gs = style as GroupStyle;
                List<IStyle> styles = new List<IStyle>();
                for (int i = 0; i < gs.Count; i++)
                {
                    styles.AddRange(GetStylesToRender(gs[i]));
                }
                stylesToRender = styles.ToArray();
            }
            else if (style is VectorStyle)
            {
                stylesToRender = new IStyle[] { style };
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
        protected void RenderGeometry(Graphics g, Map map, IGeometry feature, VectorStyle style)
        {
            if (feature == null)
                return;

            var geometryType = feature.OgcGeometryType;
            switch (geometryType)
            {
                case OgcGeometryType.Polygon:
                    if (style.EnableOutline)
                        VectorRenderer.DrawPolygon(g, (IPolygon) feature, style.Fill, style.Outline, _clippingEnabled,
                                                   map);
                    else
                        VectorRenderer.DrawPolygon(g, (IPolygon) feature, style.Fill, null, _clippingEnabled, map);
                    break;
                case OgcGeometryType.MultiPolygon:
                    if (style.EnableOutline)
                        VectorRenderer.DrawMultiPolygon(g, (IMultiPolygon) feature, style.Fill, style.Outline,
                                                        _clippingEnabled, map);
                    else
                        VectorRenderer.DrawMultiPolygon(g, (IMultiPolygon) feature, style.Fill, null, _clippingEnabled,
                                                        map);
                    break;
                case OgcGeometryType.LineString:
                    if (style.LineSymbolizer != null)
                    {
                        style.LineSymbolizer.Render(map, (ILineString)feature, g);    
                        return;
                    }
                    VectorRenderer.DrawLineString(g, (ILineString) feature, style.Line, map, style.LineOffset);
                    return;
                case OgcGeometryType.MultiLineString:
                    if (style.LineSymbolizer != null)
                    {
                        style.LineSymbolizer.Render(map, (IMultiLineString)feature, g);    
                        return;
                    }
                    VectorRenderer.DrawMultiLineString(g, (IMultiLineString) feature, style.Line, map, style.LineOffset);
                    break;
                case OgcGeometryType.Point:
                    if (style.PointSymbolizer != null)
                    {
                        VectorRenderer.DrawPoint(style.PointSymbolizer, g, (IPoint)feature, map);
                        return;
                    }

                    if (style.Symbol != null || style.PointColor == null)
                    {
                        VectorRenderer.DrawPoint(g, (IPoint)feature, style.Symbol, style.SymbolScale, style.SymbolOffset,
                                                 style.SymbolRotation, map);
                        return;
                    }
                    VectorRenderer.DrawPoint(g, (IPoint)feature, style.PointColor, style.PointSize, style.SymbolOffset, map);

                    break;
                case OgcGeometryType.MultiPoint:
                    if (style.PointSymbolizer != null)
                    {
                        VectorRenderer.DrawMultiPoint(style.PointSymbolizer, g, (IMultiPoint)feature, map);
                    }
                    if (style.Symbol != null || style.PointColor == null)
                    {
                        VectorRenderer.DrawMultiPoint(g, (IMultiPoint) feature, style.Symbol, style.SymbolScale,
                                                  style.SymbolOffset, style.SymbolRotation, map);
                    }
                    else
                    {
                        VectorRenderer.DrawMultiPoint(g, (IMultiPoint)feature, style.PointColor, style.PointSize, style.SymbolOffset, map);
                    }
                    break;
                case OgcGeometryType.GeometryCollection:                    
                    IGeometryCollection coll = (IGeometryCollection)feature;
                    for (var i = 0; i < coll.NumGeometries; i++)
                    {
                        IGeometry geom = coll[i];
                        RenderGeometry(g, map, geom, style);
                    }
                    break;
                default:
                    break;
            }
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
                int tableCount = ds.Tables.Count;
                _dataSource.ExecuteIntersectionQuery(box, ds);
                if (ds.Tables.Count > tableCount)
                {
                    //We added a table, name it according to layer
                    ds.Tables[ds.Tables.Count - 1].TableName = LayerName;
                }
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
                            GeometryServiceProvider.Instance.CreateGeometryFactory((int)CoordinateTransformation.TargetCS.AuthorityCode));
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
                int tableCount = ds.Tables.Count;
                _dataSource.ExecuteIntersectionQuery(geometry, ds);
                if (ds.Tables.Count > tableCount)
                {
                    //We added a table, name it according to layer
                    ds.Tables[ds.Tables.Count - 1].TableName = LayerName;
                }
                _dataSource.Close();
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
    }
}