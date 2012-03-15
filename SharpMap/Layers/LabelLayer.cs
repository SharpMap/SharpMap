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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
#if !DotSpatialProjections
using ProjNet.CoordinateSystems.Transformations;
#else
using DotSpatial.Projections;
#endif
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Geometries;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using Point=SharpMap.Geometries.Point;
using Transform = SharpMap.Utilities.Transform;
using SharpMap.Rendering.Symbolizer;

namespace SharpMap.Layers
{
    /// <summary>
    /// Label layer class
    /// </summary>
    /// <example>
    /// Creates a new label layer and sets the label text to the "Name" column in the FeatureDataTable of the datasource
    /// <code lang="C#">
    /// //Set up a label layer
    /// SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels");
    /// layLabel.DataSource = layCountries.DataSource;
    /// layLabel.Enabled = true;
    /// layLabel.LabelColumn = "Name";
    /// layLabel.Style = new SharpMap.Styles.LabelStyle();
    /// layLabel.Style.CollisionDetection = true;
    /// layLabel.Style.CollisionBuffer = new SizeF(20, 20);
    /// layLabel.Style.ForeColor = Color.White;
    /// layLabel.Style.Font = new Font(FontFamily.GenericSerif, 8);
    /// layLabel.MaxVisible = 90;
    /// layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
    /// </code>
    /// </example>
    public class LabelLayer : Layer, IDisposable
    {
        #region Delegates

        /// <summary>
        /// Delegate method for creating advanced label texts
        /// </summary>
        /// <param name="fdr">the <see cref="FeatureDataRow"/> to build the label for</param>
        /// <returns>the label</returns>
        public delegate string GetLabelMethod(FeatureDataRow fdr);

        /// <summary>
        /// Delegate method for calculating the priority of label rendering
        /// </summary>
        /// <param name="fdr">the <see cref="FeatureDataRow"/> to compute the priority value from</param>
        /// <returns>the priority value</returns>
        public delegate int GetPriorityMethod(FeatureDataRow fdr);

        /// <summary>
        /// Delegate method for advanced placement of the label position
        /// </summary>
        /// <param name="fdr">the <see cref="FeatureDataRow"/> to compute the label position from</param>
        /// <returns>the priority value</returns>
        public delegate Point GetLocationMethod(FeatureDataRow fdr);
        #endregion

        #region MultipartGeometryBehaviourEnum enum

        /// <summary>
        /// Labelling behaviour for Multipart geometry collections
        /// </summary>
        public enum MultipartGeometryBehaviourEnum
        {
            /// <summary>
            /// Place label on all parts (default)
            /// </summary>
            All,
            /// <summary>
            /// Place label on object which the greatest length or area.
            /// </summary>
            /// <remarks>
            /// Multipoint geometries will default to <see cref="First"/>
            /// </remarks>
            Largest,
            /// <summary>
            /// The center of the combined geometries
            /// </summary>
            CommonCenter,
            /// <summary>
            /// Center of the first geometry in the collection (fastest method)
            /// </summary>
            First
        }

        #endregion

        private IProvider _dataSource;
        private GetLabelMethod _getLabelMethod;
        private GetPriorityMethod _getPriorityMethod;
        private GetLocationMethod _getLocationMethod;

        /// <summary>
        /// Name of the column that holds the value for the label.
        /// </summary>
        private string _labelColumn;

        /// <summary>
        /// Delegate for custom Label Collision Detection
        /// </summary>
        private LabelCollisionDetection.LabelFilterMethod _labelFilter;

        /// <summary>
        /// A value indication the priority of the label in cases of label-collision detection
        /// </summary>
        private int _priority;
        /// <summary>
        /// Name of the column that contains the value indicating the priority of the label in case of label-collision detection
        /// </summary>
        private string _priorityColumn = "";

        private string _rotationColumn;
        //private LabelStyle _Style;
        private TextRenderingHint _textRenderingHint;

        private ITheme _theme;

        /// <summary>
        /// Creates a new instance of a LabelLayer
        /// </summary>
        public LabelLayer(string layername)
            :base(new LabelStyle())
        {
            //_Style = new LabelStyle();
            LayerName = layername;
            SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderingHint = TextRenderingHint.AntiAlias;
            MultipartGeometryBehaviour = MultipartGeometryBehaviourEnum.All;
            _labelFilter = LabelCollisionDetection.SimpleCollisionDetection;
        }

        /// <summary>
        /// Gets or sets labelling behavior on multipart geometries
        /// </summary>
        /// <remarks>Default value is <see cref="MultipartGeometryBehaviourEnum.All"/></remarks>
        public MultipartGeometryBehaviourEnum MultipartGeometryBehaviour { get; set; }

        /// <summary>
        /// Filtermethod delegate for performing filtering
        /// </summary>
        /// <remarks>
        /// Default method is <see cref="SharpMap.Rendering.LabelCollisionDetection.SimpleCollisionDetection"/>
        /// </remarks>
        public LabelCollisionDetection.LabelFilterMethod LabelFilter
        {
            get { return _labelFilter; }
            set { _labelFilter = value; }
        }


        /// <summary>
        /// Render whether smoothing (antialiasing) is applied to lines and curves and the edges of filled areas
        /// </summary>
        public SmoothingMode SmoothingMode { get; set; }

        /// <summary>
        /// Specifies the quality of text rendering
        /// </summary>
        public TextRenderingHint TextRenderingHint
        {
            get { return _textRenderingHint; }
            set { _textRenderingHint = value; }
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
        /// Gets or sets the rendering style of the label layer.
        /// </summary>
        public new LabelStyle Style
        {
            get { return base.Style as LabelStyle; }
            set { base.Style = value; }
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
        /// Data column or expression where label text is extracted from.
        /// </summary>
        /// <remarks>
        /// This property is overriden by the <see cref="LabelStringDelegate"/>.
        /// </remarks>
        public string LabelColumn
        {
            get { return _labelColumn; }
            set { _labelColumn = value; }
        }

        /// <summary>
        /// Gets or sets the method for creating a custom label string based on a feature.
        /// </summary>
        /// <remarks>
        /// <para>If this method is not null, it will override the <see cref="LabelColumn"/> value.</para>
        /// <para>The label delegate must take a <see cref="SharpMap.Data.FeatureDataRow"/> and return a string.</para>
        /// <example>
        /// Creating a label-text by combining attributes "ROADNAME" and "STATE" into one string, using
        /// an anonymous delegate:
        /// <code lang="C#">
        /// myLabelLayer.LabelStringDelegate = delegate(SharpMap.Data.FeatureDataRow fdr)
        ///				{ return fdr["ROADNAME"].ToString() + ", " + fdr["STATE"].ToString(); };
        /// </code>
        /// </example>
        /// </remarks>
        public GetLabelMethod LabelStringDelegate
        {
            get { return _getLabelMethod; }
            set { _getLabelMethod = value; }
        }
        /// <summary>
        /// Gets or sets the method for creating a custom position based on a feature.
        /// </summary>
        /// <remarks>
        /// <para>If this method is not null, it will override the position based on the centroid of the boundingbox of the feature </para>
        /// <para>The label delegate must take a <see cref="SharpMap.Data.FeatureDataRow"/> and return a SharpMap.Geometries.Point.</para>
        /// <para>If the delegate returns a null, the centroid of the feature will be used</para>
        /// <example>
        /// Creating a custom position by using X and Y values from the FeatureDataRow attributes "LabelX" and "LabelY", using
        /// an anonymous delegate:
        /// <code lang="C#">
        /// myLabelLayer.LabelPositionDelegate = delegate(SharpMap.Data.FeatureDataRow fdr)
        ///				{ return new SharpMap.Geometries.Point(Convert.ToDouble(fdr["LabelX"]), Convert.ToDouble(fdr["LabelY"]));};
        /// </code>
        /// </example>
        /// </remarks>
        public GetLocationMethod LabelPositionDelegate
        {
            get { return _getLocationMethod; }
            set { _getLocationMethod = value; }
        }


        /// <summary>
        /// Gets or sets the method for calculating the render priority of a label based on a feature.
        /// </summary>
        /// <remarks>
        /// <para>If this method is not null, it will override the <see cref="PriorityColumn"/> value.</para>
        /// <para>The label delegate must take a <see cref="SharpMap.Data.FeatureDataRow"/> and return an Int32.</para>
        /// <example>
        /// Creating a priority by combining attributes "capital" and "population" into one value, using
        /// an anonymous delegate:
        /// <code lang="C#">
        /// myLabelLayer.PriorityDelegate = delegate(SharpMap.Data.FeatureDataRow fdr) 
        ///     { 
        ///         Int32 retVal = 100000000 * (Int32)( (String)fdr["capital"] == "Y" ? 1 : 0 );
        ///         return  retVal + Convert.ToInt32(fdr["population"]);
        ///     };
        /// </code>
        /// </example>
        /// </remarks>
        public GetPriorityMethod PriorityDelegate
        {
            get { return _getPriorityMethod; }
            set { _getPriorityMethod = value; }
        }

        /// <summary>
        /// Data column from where the label rotation is derived.
        /// If this is empty, rotation will be zero, or aligned to a linestring.
        /// Rotation are in degrees (positive = clockwise).
        /// </summary>
        public string RotationColumn
        {
            get { return _rotationColumn; }
            set { _rotationColumn = value; }
        }

        /// <summary>
        /// A value indication the priority of the label in cases of label-collision detection
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// Name of the column that holds the value indicating the priority of the label in cases of label-collision detection
        /// </summary>
        public string PriorityColumn
        {
            get { return _priorityColumn; }
            set { _priorityColumn = value; }
        }

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        public override BoundingBox Envelope
        {
            get
            {
                bool wasOpen = DataSource.IsOpen;
                if (!wasOpen)
                    DataSource.Open();
                BoundingBox box = DataSource.GetExtents();
                if (!wasOpen) //Restore state
                    DataSource.Close();
                if (CoordinateTransformation != null)
#if !DotSpatialProjections
                {
                    var boxTrans = GeometryTransform.TransformBox(box, CoordinateTransformation.MathTransform);
                    return boxTrans.Intersection(CoordinateTransformation.TargetCS.DefaultEnvelope);

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
        public void Dispose()
        {
            if (DataSource != null) DataSource.Dispose();
        }

        #endregion

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            if (Style.Enabled && Style.MaxVisible >= map.Zoom && Style.MinVisible < map.Zoom)
            {
                
                if (DataSource == null)
                    throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));
                g.TextRenderingHint = TextRenderingHint;
                g.SmoothingMode = SmoothingMode;

                BoundingBox envelope = map.Envelope; //View to render
                var lineClipping = new CohenSutherlandLineClipping(envelope.Min.X, envelope.Min.Y,
                                                                   envelope.Max.X, envelope.Max.Y);

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
                FeatureDataSet ds = new FeatureDataSet();
                DataSource.Open();
                DataSource.ExecuteIntersectionQuery(envelope, ds);
                DataSource.Close();
                if (ds.Tables.Count == 0)
                {
                    base.Render(g, map);
                    return;
                }

                FeatureDataTable features = ds.Tables[0];


                //Initialize label collection
                List<BaseLabel> labels = new List<BaseLabel>();

                //List<System.Drawing.Rectangle> LabelBoxes; //Used for collision detection
                //Render labels
                for (int i = 0; i < features.Count; i++)
                {
                    FeatureDataRow feature = features[i];
                    if (CoordinateTransformation != null)
#if !DotSpatialProjections
                        features[i].Geometry = GeometryTransform.TransformGeometry(features[i].Geometry,
                                                                                   CoordinateTransformation.
                                                                                       MathTransform);
#else
                        features[i].Geometry = GeometryTransform.TransformGeometry(features[i].Geometry,
                                                                               CoordinateTransformation.Source,
                                                                               CoordinateTransformation.Target);
#endif
                    LabelStyle style;
                    if (Theme != null) //If thematics is enabled, lets override the style
                        style = Theme.GetStyle(feature) as LabelStyle;
                    else
                        style = Style;

                    float rotationStyle = style != null ? style.Rotation : 0f;
                    float rotationColumn = 0f;
                    if (!String.IsNullOrEmpty(RotationColumn))
                        Single.TryParse(feature[RotationColumn].ToString(), NumberStyles.Any, Map.NumberFormatEnUs,
                                       out rotationColumn);
                    float rotation = rotationStyle + rotationColumn;

                    int priority = Priority;
                    if (_getPriorityMethod != null)
                        priority = _getPriorityMethod(feature);
                    else if (!String.IsNullOrEmpty(PriorityColumn))
                        Int32.TryParse(feature[PriorityColumn].ToString(), NumberStyles.Any, Map.NumberFormatEnUs,
                                     out priority);

                    string text;
                    if (_getLabelMethod != null)
                        text = _getLabelMethod(feature);
                    else
                        text = feature[LabelColumn].ToString();

                    if (!String.IsNullOrEmpty(text))
                    {
                        // for lineal geometries, try clipping to ensure proper labeling
                        if (feature.Geometry is ILineal)
                        {
                            if (feature.Geometry is LineString)
                                feature.Geometry = lineClipping.ClipLineString(feature.Geometry as LineString);
                            else if (feature.Geometry is MultiLineString)
                                feature.Geometry = lineClipping.ClipLineString(feature.Geometry as MultiLineString);
                        }

                        if (feature.Geometry is GeometryCollection)
                        {
                            if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.All)
                            {
                                foreach (Geometry geom in (feature.Geometry as GeometryCollection))
                                {
                                    BaseLabel lbl = CreateLabel(feature,geom, text, rotation, priority, style, map, g, _getLocationMethod);
                                    if (lbl != null)
                                        labels.Add(lbl);
                                }
                            }
                            else if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.CommonCenter)
                            {
                                BaseLabel lbl = CreateLabel(feature, feature.Geometry, text, rotation, priority, style, map, g, _getLocationMethod);
                                if (lbl != null)
                                    labels.Add(lbl);
                            }
                            else if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.First)
                            {
                                if ((feature.Geometry as GeometryCollection).Collection.Count > 0)
                                {
                                    BaseLabel lbl = CreateLabel(feature, (feature.Geometry as GeometryCollection).Collection[0], text,
                                                            rotation, style, map, g);
                                    if (lbl != null)
                                        labels.Add(lbl);
                                }
                            }
                            else if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.Largest)
                            {
                                GeometryCollection coll = (feature.Geometry as GeometryCollection);
                                if (coll.NumGeometries > 0)
                                {
                                    double largestVal = 0;
                                    int idxOfLargest = 0;
                                    for (int j = 0; j < coll.NumGeometries; j++)
                                    {
                                        Geometry geom = coll.Geometry(j);
                                        if (geom is LineString && ((LineString) geom).Length > largestVal)
                                        {
                                            largestVal = ((LineString) geom).Length;
                                            idxOfLargest = j;
                                        }
                                        if (geom is MultiLineString && ((MultiLineString) geom).Length > largestVal)
                                        {
                                            largestVal = ((MultiLineString)geom).Length;
                                            idxOfLargest = j;
                                        }
                                        if (geom is Polygon && ((Polygon) geom).Area > largestVal)
                                        {
                                            largestVal = ((Polygon) geom).Area;
                                            idxOfLargest = j;
                                        }
                                        if (geom is MultiPolygon && ((MultiPolygon) geom).Area > largestVal)
                                        {
                                            largestVal = ((MultiPolygon) geom).Area;
                                            idxOfLargest = j;
                                        }
                                    }

                                    BaseLabel lbl = CreateLabel(feature, coll.Geometry(idxOfLargest), text, rotation, priority, style,
                                                            map, g, _getLocationMethod);
                                    if (lbl != null)
                                        labels.Add(lbl);
                                }
                            }
                        }
                        else
                        {
                            BaseLabel lbl = CreateLabel(feature, feature.Geometry, text, rotation, priority, style, map, g, _getLocationMethod);
                            if (lbl != null)
                                labels.Add(lbl);
                        }
                    }
                }
                if (labels.Count > 0) //We have labels to render...
                {
                    if (Style.CollisionDetection && _labelFilter != null)
                        _labelFilter(labels);
                    
                    for (int i = 0; i < labels.Count; i++)
                    {   
                        // Don't show the label if not necessary
                        if (!labels[i].Show)
                        {
                            continue;
                        }

                        if (labels[i] is Label)
                        {
                            var label = labels[i] as Label;
                            if (label.Style.IsTextOnPath == false || label.TextOnPathLabel==null)
                            {
                                VectorRenderer.DrawLabel(g, label.Location, label.Style.Offset,
                                                            label.Style.Font, label.Style.ForeColor,
                                                            label.Style.BackColor, Style.Halo, label.Rotation,
                                                            label.Text, map);
                            }
                            else
                            {
                                if (label.Style.BackColor != null && label.Style.BackColor != System.Drawing.Brushes.Transparent)
                                {
                                    //draw background
                                    if (label.TextOnPathLabel.RegionList.Count > 0)
                                    {
                                        g.FillRectangles(labels[i].Style.BackColor, labels[i].TextOnPathLabel.RegionList.ToArray());
                                        //g.FillPolygon(labels[i].Style.BackColor, labels[i].TextOnPathLabel.PointsText.ToArray());
                                    }
                                }
                                label.TextOnPathLabel.DrawTextOnPath();
                            }
                        }
                        else if (labels[i] is PathLabel)
                        {
                            var plbl = labels[i] as PathLabel;
                            var lblStyle = plbl.Style;
                            g.DrawString(lblStyle.Halo, new SolidBrush(lblStyle.ForeColor), plbl.Text,
                                         lblStyle.Font.FontFamily, (int) lblStyle.Font.Style, lblStyle.Font.Size,
                                         lblStyle.GetStringFormat(), lblStyle.IgnoreLength, plbl.Location);
                        }
                    }
                }
            }
            base.Render(g, map);
        }


        private BaseLabel CreateLabel(FeatureDataRow fdr, Geometry feature, string text, float rotation, LabelStyle style, Map map, Graphics g)
        {
            return CreateLabel(fdr, feature, text, rotation, Priority, style, map, g, _getLocationMethod);
        }

        private static BaseLabel CreateLabel(FeatureDataRow fdr, Geometry feature, string text, float rotation, int priority, LabelStyle style, Map map, Graphics g, GetLocationMethod _getLocationMethod)
        {
            BaseLabel lbl = null;

            SizeF size = VectorRenderer.SizeOfString(g, text, style.Font);

            if (feature is ILineal)
            {
                var line = feature as LineString;
                if (line != null)
                {
                    if (style.IsTextOnPath == false)
                    {
                        if (size.Width < 0.95 * line.Length / map.PixelWidth || style.IgnoreLength)
                        {
                            var positiveLineString = PositiveLineString(line, false);
                            var lineStringPath = LineStringToPath(positiveLineString, map /*, false*/);
                            var rect = lineStringPath.GetBounds();

                            if (style.CollisionDetection && !style.CollisionBuffer.IsEmpty)
                            {
                                var cbx = style.CollisionBuffer.Width;
                                var cby = style.CollisionBuffer.Height;
                                rect.Inflate(2 * cbx, 2 * cby);
                                rect.Offset(-cbx, -cby);
                            }
                            var labelBox = new LabelBox(rect);

                            lbl = new PathLabel(text, lineStringPath, 0, priority, labelBox, style);
                        }
                    }
                    else
                    {
                        //get centriod
                        System.Drawing.PointF position2 = map.WorldToImage(feature.GetBoundingBox().GetCentroid());
                        lbl = new Label(text, position2, rotation, priority, style);
                        if (size.Width < 0.95 * line.Length / map.PixelWidth || !style.IgnoreLength)
                        {
                            CalculateLabelAroundOnLineString(line, ref lbl, map, g, size);
                        }
                    }
                }
                return lbl;
            }
            
            PointF position = Transform.WorldtoMap(feature.GetBoundingBox().GetCentroid(), map);
            if (_getLocationMethod != null)
            {
                Point p = _getLocationMethod(fdr);
                if (p !=null)
                    position = Transform.WorldtoMap(p, map);
            }
            position.X = position.X - size.Width*(short) style.HorizontalAlignment*0.5f;
            position.Y = position.Y - size.Height*(short) (2-(int)style.VerticalAlignment)*0.5f;
            if (position.X - size.Width > map.Size.Width || position.X + size.Width < 0 ||
                position.Y - size.Height > map.Size.Height || position.Y + size.Height < 0)
                return null;

            if (!style.CollisionDetection)
                lbl = new Label(text, position, rotation, priority, null, style);
            else
            {
                //Collision detection is enabled so we need to measure the size of the string
                lbl = new Label(text, position, rotation, priority,
                                new LabelBox(position.X - size.Width*0.5f - style.CollisionBuffer.Width,
                                             position.Y + size.Height*0.5f + style.CollisionBuffer.Height,
                                             size.Width + 2f*style.CollisionBuffer.Width,
                                             size.Height + style.CollisionBuffer.Height*2f), style);
            }

            /*
            if (feature is LineString)
            {
                var line = feature as LineString;

                //Only label feature if it is long enough, or it is definately wanted                
                if (line.Length / map.PixelSize > size.Width || style.IgnoreLength)
                {
                    CalculateLabelOnLinestring(line, ref lbl, map);
                }
                else
                    return null;
            }
            */
            return lbl;
        }

        /// <summary>
        /// Very basic test to check for positve direction of Linestring
        /// </summary>
        /// <param name="line">The linestring to test</param>
        /// <param name="isRightToLeft">Value indicating whether labels are to be printed right to left</param>
        /// <returns>The positively directed linestring</returns>
        private static LineString PositiveLineString(LineString line, bool isRightToLeft)
        {
            var s = line.StartPoint;
            var e = line.EndPoint;

            var dx = e.X - s.X;
            if (isRightToLeft && dx < 0)
                return line;
            
            if (!isRightToLeft && dx >= 0)
                return line;

            var revCoord = new Stack<Point>(line.Vertices);
            return new LineString(revCoord.ToArray());
        }

        //private static void WarpedLabel(MultiLineString line, ref BaseLabel baseLabel, Map map)
        //{
        //    var path = MultiLineStringToPath(line, map, true);

        //    var pathLabel = new PathLabel(baseLabel.Text, path, 0f, baseLabel.Priority, new LabelBox(path.GetBounds()), baseLabel.Style);
        //    baseLabel = pathLabel;
        //}

        //private static void WarpedLabel(LineString line, ref BaseLabel baseLabel, Map map)
        //{
            
        //    var path = LineStringToPath(line, map, false);

        //    var pathLabel = new PathLabel(baseLabel.Text, path, 0f, baseLabel.Priority, new LabelBox(path.GetBounds()), baseLabel.Style);
        //    baseLabel = pathLabel;
        //}


        /// <summary>
        /// Function to transform a linestring to a graphics path for further processing
        /// </summary>
        /// <param name="lineString">The Linestring</param>
        /// <param name="map">The map</param>
        ///// <param name="useClipping">A value indicating whether clipping should be applied or not</param>
        /// <returns>A GraphicsPath</returns>
        public static GraphicsPath LineStringToPath(LineString lineString, Map map/*, bool useClipping*/)
        {
            var gp = new GraphicsPath(FillMode.Alternate);
            //if (!useClipping)
                gp.AddLines(lineString.TransformToImage(map));
            //else
            //{
            //    var bb = map.Envelope;
            //    var cohenSutherlandLineClipping = new CohenSutherlandLineClipping(bb.Left, bb.Bottom, bb.Right, bb.Top);
            //    var clippedLineStrings = cohenSutherlandLineClipping.ClipLineString(lineString);
            //    foreach (var clippedLineString in clippedLineStrings.LineStrings)
            //    {
            //        var s = clippedLineString.StartPoint;
            //        var e = clippedLineString.EndPoint;
                    
            //        var dx = e.X - s.X;
            //        //var dy = e.Y - s.Y;

            //        LineString revcls = null;
            //        if (dx < 0)
            //            revcls = ReverseLineString(clippedLineString);
                    
            //        gp.StartFigure();
            //        gp.AddLines(revcls == null ? clippedLineString.TransformToImage(map) : revcls.TransformToImage(map));
            //    }
            //}
            return gp;
        }

        //private static LineString ReverseLineString(LineString clippedLineString)
        //{
        //    var coords = new Stack<Point>(clippedLineString.Vertices);
        //    return new LineString(coords.ToArray());
        //}

        ///// <summary>
        ///// Function to transform a linestring to a graphics path for further processing
        ///// </summary>
        ///// <param name="multiLineString">The Linestring</param>
        ///// <param name="map">The map</param>
        ///// <param name="useClipping">A value indicating whether clipping should be applied or not</param>
        ///// <returns>A GraphicsPath</returns>
        //public static GraphicsPath MultiLineStringToPath(MultiLineString multiLineString, Map map, bool useClipping)
        //{
        //    var gp = new GraphicsPath(FillMode.Alternate);
        //    foreach (var lineString in multiLineString.LineStrings)
        //        gp.AddPath(LineStringToPath(lineString, map, useClipping), false);

        //    return gp;
        //}

        //private static GraphicsPath LineToGraphicsPath(LineString line, Map map)
        //{
        //    GraphicsPath path = new GraphicsPath();
        //    path.AddLines(line.TransformToImage(map));
        //    return path;
        //}

        private static void CalculateLabelOnLinestring(LineString line, ref BaseLabel baseLabel, Map map)
        {
            double dx, dy;
            var label = baseLabel as Label;

            // first find the middle segment of the line
            int midPoint = (line.Vertices.Count - 1)/2;
            if (line.Vertices.Count > 2)
            {
                dx = line.Vertices[midPoint + 1].X - line.Vertices[midPoint].X;
                dy = line.Vertices[midPoint + 1].Y - line.Vertices[midPoint].Y;
            }
            else
            {
                midPoint = 0;
                dx = line.Vertices[1].X - line.Vertices[0].X;
                dy = line.Vertices[1].Y - line.Vertices[0].Y;
            }
            if (dy == 0)
                label.Rotation = 0;
            else if (dx == 0)
                label.Rotation = 90;
            else
            {
                // calculate angle of line					
                double angle = -Math.Atan(dy/dx) + Math.PI*0.5;
                angle *= (180d/Math.PI); // convert radians to degrees
                label.Rotation = (float) angle - 90; // -90 text orientation
            }
            double tmpx = line.Vertices[midPoint].X + (dx*0.5);
            double tmpy = line.Vertices[midPoint].Y + (dy*0.5);
            label.LabelPoint = map.WorldToImage(new Point(tmpx, tmpy));
        }
        private static void CalculateLabelAroundOnLineString(SharpMap.Geometries.LineString line, ref BaseLabel label, Map map, System.Drawing.Graphics g, System.Drawing.SizeF textSize)
        {
            IList<SharpMap.Geometries.Point> sPoints = line.Vertices;

            // only get point in enverlop of map
            Collection<System.Drawing.PointF> colPoint = new Collection<System.Drawing.PointF>();
            bool bCheckStarted = false;
            for (int j = 0; j < sPoints.Count; j++)
            {
                if (map.Envelope.Grow(map.PixelSize * 10).Contains(sPoints[j]))
                {
                    //points[j] = map.WorldToImage(sPoints[j]);
                    colPoint.Add(map.WorldToImage(sPoints[j]));
                    bCheckStarted = true;
                }
                else if (bCheckStarted == true)
                {
                    // fix bug curved line out of map in center segment of line
                    break;
                }
            }

            if (colPoint.Count > 1)
            {
                label.TextOnPathLabel = new SharpMap.Rendering.TextOnPath();
                switch (label.Style.HorizontalAlignment)
                {
                    case SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left:
                        label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Left;
                        break;
                    case SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Right:
                        label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Right;
                        break;
                    case SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center:
                        label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Center;
                        break;
                    default:
                        label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Center;
                        break;
                }
                switch (label.Style.VerticalAlignment)
                {
                    case SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom:
                        label.TextOnPathLabel.TextPathPathPosition = SharpMap.Rendering.TextPathPosition.UnderPath;
                        break;
                    case SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top:
                        label.TextOnPathLabel.TextPathPathPosition = SharpMap.Rendering.TextPathPosition.OverPath;
                        break;
                    case SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Middle:
                        label.TextOnPathLabel.TextPathPathPosition = SharpMap.Rendering.TextPathPosition.CenterPath;
                        break;
                    default:
                        label.TextOnPathLabel.TextPathPathPosition = SharpMap.Rendering.TextPathPosition.CenterPath;
                        break;
                }
                int idxStartPath = 0;
                int numberPoint = colPoint.Count;
                // start Optimzes Path points                
                int step = 100;
                if (colPoint.Count >= step * 2)
                {
                    numberPoint = step * 2; ;
                    switch (label.Style.HorizontalAlignment)
                    {
                        case SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left:
                            //label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Left;
                            idxStartPath = 0;
                            break;
                        case SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Right:
                            //label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Right;
                            idxStartPath = colPoint.Count - step;
                            break;
                        case SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center:
                            //label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Center;
                            idxStartPath = (int)colPoint.Count / 2 - step;
                            break;
                        default:
                            //label.TextOnPathLabel.TextPathAlignTop = SharpMap.Rendering.TextPathAlign.Center;
                            idxStartPath = (int)colPoint.Count / 2 - step;
                            break;
                    }
                }
                // end optimize path point
                System.Drawing.PointF[] points = new System.Drawing.PointF[numberPoint];
                int count = 0;
                if (colPoint[0].X <= colPoint[colPoint.Count - 1].X)
                {
                    for (int l = idxStartPath; l < numberPoint + idxStartPath; l++)
                    {
                        points[count] = colPoint[l];
                        count++;
                    }
                }
                else
                {
                    //reverse the path                    
                    for (int k = numberPoint - 1 + idxStartPath; k >= idxStartPath; k--)
                    {
                        points[count] = colPoint[k];
                        count++;
                    }
                }
                //get text size in page units ie pixels
                float textheight = label.Style.Font.Size;
                switch (label.Style.Font.Unit)
                {
                    case System.Drawing.GraphicsUnit.Display:
                        textheight = textheight * g.DpiY / 75;
                        break;
                    case System.Drawing.GraphicsUnit.Document:
                        textheight = textheight * g.DpiY / 300;
                        break;
                    case System.Drawing.GraphicsUnit.Inch:
                        textheight = textheight * g.DpiY;
                        break;
                    case System.Drawing.GraphicsUnit.Millimeter:
                        textheight = (float)(textheight / 25.4 * g.DpiY);
                        break;
                    case System.Drawing.GraphicsUnit.Pixel:
                        //do nothing
                        break;
                    case System.Drawing.GraphicsUnit.Point:
                        textheight = textheight * g.DpiY / 72;
                        break;
                }
                System.Drawing.Font topFont = new System.Drawing.Font(label.Style.Font.FontFamily, textheight, label.Style.Font.Style);
                //
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddLines(points);

                label.TextOnPathLabel.PathColorTop = System.Drawing.Color.Transparent;
                label.TextOnPathLabel.Text = label.Text;
                label.TextOnPathLabel.LetterSpacePercentage = 90;
                label.TextOnPathLabel.FillColorTop = new System.Drawing.SolidBrush(label.Style.ForeColor);
                label.TextOnPathLabel.Font = topFont;
                label.TextOnPathLabel.PathDataTop = path.PathData;
                label.TextOnPathLabel.Graphics = g;
                //label.TextOnPathLabel.ShowPath=true;
                //label.TextOnPathLabel.PathColorTop = System.Drawing.Color.YellowGreen;
                if (label.Style.Halo != null)
                {
                    label.TextOnPathLabel.ColorHalo = label.Style.Halo;
                }
                else
                {
                    label.TextOnPathLabel.ColorHalo = null;// new System.Drawing.Pen(label.Style.ForeColor, (float)0.5); 
                }
                path.Dispose();

                // MeasureString to get region
                label.TextOnPathLabel.MeasureString = true;
                label.TextOnPathLabel.DrawTextOnPath();
                label.TextOnPathLabel.MeasureString = false;
                // Get Region label for CollissionDetection here.
                System.Drawing.Drawing2D.GraphicsPath pathRegion = new System.Drawing.Drawing2D.GraphicsPath();

                if (label.TextOnPathLabel.RegionList.Count > 0)
                {
                    //int idxCenter = (int)label.TextOnPathLabel.PointsText.Count / 2;
                    //System.Drawing.Drawing2D.Matrix rotationMatrix = g.Transform.Clone();// new Matrix();
                    //rotationMatrix.RotateAt(label.TextOnPathLabel.Angles[idxCenter], label.TextOnPathLabel.PointsText[idxCenter]);
                    //if (label.TextOnPathLabel.PointsTextUp.Count > 0)
                    //{
                    //    for (int up = label.TextOnPathLabel.PointsTextUp.Count - 1; up >= 0; up--)
                    //    {
                    //        label.TextOnPathLabel.PointsText.Add(label.TextOnPathLabel.PointsTextUp[up]);
                    //    }

                    //}                 
                    pathRegion.AddRectangles(label.TextOnPathLabel.RegionList.ToArray());

                    // get box for detect colission here              
                    label.Box = new LabelBox(pathRegion.GetBounds());
                    //g.FillRectangle(System.Drawing.Brushes.YellowGreen, label.Box);
                }
                pathRegion.Dispose();
            }

        }
    }
}