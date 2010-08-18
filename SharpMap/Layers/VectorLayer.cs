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
using ProjNet.CoordinateSystems.Transformations;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Geometries;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using Point=SharpMap.Geometries.Point;

namespace SharpMap.Layers
{
    /// <summary>
    /// Class for vector layer properties
    /// </summary>
    /// <example>
    /// Adding a VectorLayer to a map:
    /// <code lang="C#">
    /// //Initialize a new map
    /// SharpMap.Map myMap = new SharpMap.Map(new System.Drawing.Size(300,600));
    /// //Create a layer
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    /// //Add datasource
    /// myLayer.DataSource = new SharpMap.Data.Providers.ShapeFile(@"C:\data\MyShapeData.shp");
    /// //Set up styles
    /// myLayer.Style.Outline = new Pen(Color.Magenta, 3f);
    /// myLayer.Style.EnableOutline = true;
    /// myMap.Layers.Add(myLayer);
    /// //Zoom to fit the data in the view
    /// myMap.ZoomToExtents();
    /// //Render the map:
    /// System.Drawing.Image mapImage = myMap.GetMap();
    /// </code>
    /// </example>
    public class VectorLayer : Layer, ICanQueryLayer, IDisposable
    {
        private bool _ClippingEnabled = false;
        private bool _isQueryEnabled = true;
        private IProvider _DataSource;
        private SmoothingMode _SmoothingMode;
        private VectorStyle _Style;
        private ITheme _theme;

        /// <summary>
        /// Initializes a new layer
        /// </summary>
        /// <param name="layername">Name of layer</param>
        public VectorLayer(string layername)
        {
            Style = new VectorStyle();
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
            _DataSource = dataSource;
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
        /// <para>Clipping will clip <see cref="SharpMap.Geometries.Polygon"/> and
        /// <see cref="SharpMap.Geometries.MultiPolygon"/> to the current view prior
        /// to rendering the object.</para>
        /// <para>Enabling clipping might improve rendering speed if you are rendering 
        /// only small portions of very large objects.</para>
        /// </remarks>
        public bool ClippingEnabled
        {
            get { return _ClippingEnabled; }
            set { _ClippingEnabled = value; }
        }

        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE
        /// </summary>
        public bool IsQueryEnabled 
        {
            get { return _isQueryEnabled; }
            set {_isQueryEnabled = value; } 
        }

        /// <summary>
        /// Render whether smoothing (antialiasing) is applied to lines and curves and the edges of filled areas
        /// </summary>
        public SmoothingMode SmoothingMode
        {
            get { return _SmoothingMode; }
            set { _SmoothingMode = value; }
        }

        /// <summary>
        /// Gets or sets the datasource
        /// </summary>
        public IProvider DataSource
        {
            get { return _DataSource; }
            set { _DataSource = value; }
        }

        /// <summary>
        /// Gets or sets the rendering style of the vector layer.
        /// </summary>
        public VectorStyle Style
        {
            get { return _Style; }
            set { _Style = value; }
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

                bool wasOpen = DataSource.IsOpen;
                if (!wasOpen)
                    DataSource.Open();
                BoundingBox box = DataSource.GetExtents();
                if (!wasOpen) //Restore state
                    DataSource.Close();
                if (CoordinateTransformation != null)
                    return GeometryTransform.TransformBox(box, CoordinateTransformation.MathTransform);
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
        public void Dispose()
        {
            if (DataSource is IDisposable)
                ((IDisposable) DataSource).Dispose();
        }

        #endregion

        /// <summary>
        /// Renders the layer to a graphics object
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            if (map.Center == null)
                throw (new ApplicationException("Cannot render map. View center not specified"));

            g.SmoothingMode = SmoothingMode;
            BoundingBox envelope = map.Envelope; //View to render
            if (CoordinateTransformation != null)
            {
                CoordinateTransformation.MathTransform.Invert();
                envelope = GeometryTransform.TransformBox(envelope, CoordinateTransformation.MathTransform);
                CoordinateTransformation.MathTransform.Invert();
            }

            //List<SharpMap.Geometries.Geometry> features = this.DataSource.GetGeometriesInView(map.Envelope);

            if (DataSource == null)
                throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

            //If thematics is enabled, we use a slighty different rendering approach
            if (Theme != null)
            {
                FeatureDataSet ds = new FeatureDataSet();
                DataSource.Open();
                DataSource.ExecuteIntersectionQuery(envelope, ds);
                DataSource.Close();

                FeatureDataTable features = (FeatureDataTable) ds.Tables[0];

                if (CoordinateTransformation != null)
                    for (int i = 0; i < features.Count; i++)
                        features[i].Geometry = GeometryTransform.TransformGeometry(features[i].Geometry,
                                                                                   CoordinateTransformation.
                                                                                       MathTransform);

                //Linestring outlines is drawn by drawing the layer once with a thicker line
                //before drawing the "inline" on top.
                if (Style.EnableOutline)
                {
                    //foreach (SharpMap.Geometries.Geometry feature in features)
                    for (int i = 0; i < features.Count; i++)
                    {
                        FeatureDataRow feature = features[i];
                        //Draw background of all line-outlines first
                        if (feature.Geometry is LineString)
                        {
                            VectorStyle outlinestyle1 = Theme.GetStyle(feature) as VectorStyle;
                            if (outlinestyle1.Enabled && outlinestyle1.EnableOutline)
                                VectorRenderer.DrawLineString(g, feature.Geometry as LineString, outlinestyle1.Outline,
                                                              map);
                        }
                        else if (feature.Geometry is MultiLineString)
                        {
                            VectorStyle outlinestyle2 = Theme.GetStyle(feature) as VectorStyle;
                            if (outlinestyle2.Enabled && outlinestyle2.EnableOutline)
                                VectorRenderer.DrawMultiLineString(g, feature.Geometry as MultiLineString,
                                                                   outlinestyle2.Outline, map);
                        }
                    }
                }

                for (int i = 0; i < features.Count; i++)
                {
                    FeatureDataRow feature = features[i];
                    VectorStyle style = Theme.GetStyle(feature) as VectorStyle;
                    if (style == null) continue;
                    if (!style.Enabled) continue;
                    if (!(style.MinVisible <= map.Zoom && map.Zoom <= style.MaxVisible)) continue;
                    RenderGeometry(g, map, feature.Geometry, style);
                }
            }
            else
            {
                DataSource.Open();

                Collection<Geometry> geoms = DataSource.GetGeometriesInView(envelope);
                DataSource.Close();

                if (CoordinateTransformation != null)
                    for (int i = 0; i < geoms.Count; i++)
                        geoms[i] = GeometryTransform.TransformGeometry(geoms[i], CoordinateTransformation.MathTransform);

                //Linestring outlines is drawn by drawing the layer once with a thicker line
                //before drawing the "inline" on top.
                if (Style.EnableOutline)
                {
                    foreach (Geometry geom in geoms)
                    {
                        if (geom != null)
                        {
                            //Draw background of all line-outlines first
                            switch (geom.GetType().FullName)
                            {
                                case "SharpMap.Geometries.LineString":
                                    VectorRenderer.DrawLineString(g, geom as LineString, Style.Outline, map);
                                    break;
                                case "SharpMap.Geometries.MultiLineString":
                                    VectorRenderer.DrawMultiLineString(g, geom as MultiLineString, Style.Outline, map);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                for (int i = 0; i < geoms.Count; i++)
                {
                    if (geoms[i] != null)
                        RenderGeometry(g, map, geoms[i], Style);
                }
            }


            base.Render(g, map);
        }

        private void RenderGeometry(Graphics g, Map map, Geometry feature, VectorStyle style)
        {
            //ToDo: Add Property 'public GeometryType2 GeometryType { get; }' to remove this
            GeometryType2 geometryType = feature.GeometryType;
            //(GeometryType2)Enum.Parse(typeof(GeometryType2), feature.GetType().Name);
            
            switch (geometryType)
            //switch (feature.GetType().FullName)
            {
                case GeometryType2.Polygon:
                //case "SharpMap.Geometries.Polygon":
                    if (style.EnableOutline)
                        VectorRenderer.DrawPolygon(g, (Polygon) feature, style.Fill, style.Outline, _ClippingEnabled,
                                                   map);
                    else
                        VectorRenderer.DrawPolygon(g, (Polygon) feature, style.Fill, null, _ClippingEnabled, map);
                    break;
                case GeometryType2.MultiPolygon:
                //case "SharpMap.Geometries.MultiPolygon":
                    if (style.EnableOutline)
                        VectorRenderer.DrawMultiPolygon(g, (MultiPolygon) feature, style.Fill, style.Outline,
                                                        _ClippingEnabled, map);
                    else
                        VectorRenderer.DrawMultiPolygon(g, (MultiPolygon) feature, style.Fill, null, _ClippingEnabled,
                                                        map);
                    break;
                case GeometryType2.LineString:
                //case "SharpMap.Geometries.LineString":
                    VectorRenderer.DrawLineString(g, (LineString) feature, style.Line, map);
                    break;
                case GeometryType2.MultiLineString:
                //case "SharpMap.Geometries.MultiLineString":
                    VectorRenderer.DrawMultiLineString(g, (MultiLineString) feature, style.Line, map);
                    break;
                case GeometryType2.Point:
                //case "SharpMap.Geometries.Point":
                    VectorRenderer.DrawPoint(g, (Point) feature, style.Symbol, style.SymbolScale, style.SymbolOffset,
                                             style.SymbolRotation, map);
                    break;
                case GeometryType2.MultiPoint:
                //case "SharpMap.Geometries.MultiPoint":
                    VectorRenderer.DrawMultiPoint(g, (MultiPoint) feature, style.Symbol, style.SymbolScale,
                                                  style.SymbolOffset, style.SymbolRotation, map);
                    break;
                case GeometryType2.GeometryCollection:
                //case "SharpMap.Geometries.GeometryCollection":
                    foreach (Geometry geom in (GeometryCollection) feature)
                        RenderGeometry(g, map, geom, style);
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
        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            _DataSource.Open();
            _DataSource.ExecuteIntersectionQuery(box, ds);
            _DataSource.Close();
        }

        #endregion
    }
}