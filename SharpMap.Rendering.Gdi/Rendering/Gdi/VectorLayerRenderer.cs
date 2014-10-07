using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using GeoAPI;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Features;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Gdi.Decoration;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using SharpMap.Utilities;

namespace SharpMap.Rendering.Gdi
{
    public class VectorLayerRenderer : BaseLayerDeviceRenderer<VectorLayer>
    {
        protected override void OnRenderInternal(VectorLayer vl, Graphics g, GdiRenderingArguments ra,
            IProgressHandler handler)
        {

            g.SmoothingMode = vl.SmoothingMode;
            var envelope = ra.Map.Envelope; //View to render
            if (vl.CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                if (vl.ReverseCoordinateTransformation != null)
                {
                    envelope = GeometryTransform.TransformBox(envelope, vl.ReverseCoordinateTransformation.MathTransform);
                }
                else
                {
                    vl.CoordinateTransformation.MathTransform.Invert();
                    envelope = GeometryTransform.TransformBox(envelope, vl.CoordinateTransformation.MathTransform);
                    vl.CoordinateTransformation.MathTransform.Invert();
                }
#else
                envelope = GeometryTransform.TransformBox(envelope, vl.CoordinateTransformation.Target, vl.CoordinateTransformation.Source);
#endif
            }

            if (vl.DataSource == null)
                throw (new ApplicationException("DataSource property not set on layer '" + vl.LayerName + "'"));





            //If thematics is enabled, we use a slighty different rendering approach
            if (vl.Theme != null)
                OnRenderInternalTheme(vl, g, ra.Map, envelope, vl.Theme);
            else
                OnRenderInternalGeometries(vl, g, ra.Map, envelope);
        }

        /// <summary>
        /// Method to render this layer to the map, applying <paramref name="theme"/>.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map object</param>
        /// <param name="envelope">The envelope to render</param>
        /// <param name="theme">The theme to apply</param>
        protected static void OnRenderInternalTheme(VectorLayer vl, Graphics g, Map map, Envelope envelope, ITheme theme)
        {
            var ds = new FeatureDataSet();
            lock (vl.DataSource)
            {
                vl.DataSource.Open();
                vl.DataSource.ExecuteIntersectionQuery(envelope, ds);
                vl.DataSource.Close();
            }



            foreach (FeatureDataTable features in ds.Tables)
            {

                IFeatureCollection collection = features;
                if (vl.CoordinateTransformation != null)
                    for (int i = 0; i < features.Count; i++)
#if !DotSpatialProjections
                    {
                        
                        collection[i].Geometry = GeometryTransform.TransformGeometry(collection[i].Geometry,
                            vl.CoordinateTransformation.
                                MathTransform,
                            GeometryServiceProvider.Instance.CreateGeometryFactory((int)vl.CoordinateTransformation.TargetCS.AuthorityCode));
                    }
#else
                    features[i].Geometry = GeometryTransform.TransformGeometry(features[i].Geometry,
                                                                                CoordinateTransformation.Source,
                                                                                CoordinateTransformation.Target,
                                                                                CoordinateTransformation.TargetFactory);

#endif

                //Linestring outlines is drawn by drawing the layer once with a thicker line
                //before drawing the "inline" on top.
                if (vl.Style.EnableOutline)
                {
                    for (int i = 0; i < features.Count; i++)
                    {
                        var feature = collection[i];
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
                    var feature = collection[i];
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
                                RenderGeometry(g, map, feature.Geometry, clone, vl.ClippingEnabled);
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
        protected void OnRenderInternalGeometries(VectorLayer vl, Graphics g, Map map, Envelope envelope)
        {
            //if style is not enabled, we don't need to render anything
            if (!vl.Style.Enabled) return;

            IStyle[] stylesToRender = GetStylesToRender(vl.Style);
            
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
                        IList<IGeometry> geoms;
                        // Is datasource already open?                        
                        IProvider ds = vl.DataSource;
                        lock (ds)
                        {
                            bool alreadyOpen = ds.IsOpen;

                            // If not open yet, open it
                            if (!alreadyOpen) { ds.Open(); }

                            // Read data
                            var temp = ds.GetGeometriesInView(envelope);
                            geoms = (temp ?? new IGeometry[0]).ToList();


                            // If was not open, close it
                            if (!alreadyOpen) { ds.Close(); }
                        }
                        if (vl.CoordinateTransformation != null)
                            for (int i = 0; i < geoms.Count; i++)
#if !DotSpatialProjections
                                geoms[i] = GeometryTransform.TransformGeometry(geoms[i], vl.CoordinateTransformation.MathTransform,
                                    GeometryServiceProvider.Instance.CreateGeometryFactory((int)vl.CoordinateTransformation.TargetCS.AuthorityCode));
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
                                RenderGeometry(g, map, geoms[i], vStyle, vl.ClippingEnabled);
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
        private static IStyle[] GetStylesToRender(IStyle style)
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
        protected static void RenderGeometry(Graphics g, Map map, IGeometry feature, VectorStyle style, bool clippingEnabled)
        {
            if (feature == null)
                return;

            var geometryType = feature.OgcGeometryType;
            switch (geometryType)
            {
                case OgcGeometryType.Polygon:
                    if (style.EnableOutline)
                        VectorRenderer.DrawPolygon(g, (IPolygon) feature, style.Fill, style.Outline, clippingEnabled,
                            map);
                    else
                        VectorRenderer.DrawPolygon(g, (IPolygon) feature, style.Fill, null, clippingEnabled, map);
                    break;
                case OgcGeometryType.MultiPolygon:
                    if (style.EnableOutline)
                        VectorRenderer.DrawMultiPolygon(g, (IMultiPolygon) feature, style.Fill, style.Outline,
                            clippingEnabled, map);
                    else
                        VectorRenderer.DrawMultiPolygon(g, (IMultiPolygon) feature, style.Fill, null, clippingEnabled,
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
                        RenderGeometry(g, map, geom, style, clippingEnabled);
                    }
                    break;
                default:
                    break;
            }
        }

    }
}