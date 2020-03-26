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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using SharpMap.Data;
using SharpMap.Data.Providers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
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
    [Serializable]
    public class LabelLayer : Layer
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
        public delegate Coordinate GetLocationMethod(FeatureDataRow fdr);
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

        private IBaseProvider _dataSource;
        private GetLabelMethod _getLabelMethod;
        private GetPriorityMethod _getPriorityMethod;
        private GetLocationMethod _getLocationMethod;
        private Envelope _envelope;

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

        /// <summary>
        /// Name of the column that contains the value indicating the rotation value of the label
        /// </summary>
        private string _rotationColumn;


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
        /// This property is overridden by the <see cref="LabelStringDelegate"/>.
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
        /// <para>The label delegate must take a <see cref="SharpMap.Data.FeatureDataRow"/> and return a GeoAPI.Geometries.Coordinate.</para>
        /// <para>If the delegate returns a null, the centroid of the feature will be used</para>
        /// <example>
        /// Creating a custom position by using X and Y values from the FeatureDataRow attributes "LabelX" and "LabelY", using
        /// an anonymous delegate:
        /// <code lang="C#">
        /// myLabelLayer.LabelPositionDelegate = delegate(SharpMap.Data.FeatureDataRow fdr)
        ///				{ return new GeoAPI.Geometries.Coordinate(Convert.ToDouble(fdr["LabelX"]), Convert.ToDouble(fdr["LabelY"]));};
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
        public override Envelope Envelope
        {
            get
            {
                if (DataSource == null)
                    throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

                if (_envelope != null && CacheExtent)
                    return ToTarget(_envelope.Copy());

                var wasOpen = DataSource.IsOpen;
                if (!wasOpen)
                    DataSource.Open();
                var box = DataSource.GetExtents();
                if (!wasOpen) //Restore state
                    DataSource.Close();

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
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (DataSource != null)
                DataSource.Dispose();
            base.ReleaseManagedResources();
        }

        #endregion

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, MapViewport map)
        {
            if (DataSource == null)
                throw (new ApplicationException("DataSource property not set on layer '" + LayerName + "'"));

            var layerEnvelope = ToSource(map.Envelope); //View to render
            List<BaseLabel> labels = null;

            using (var ds = new FeatureDataSet())
            {
                DataSource.Open();
                DataSource.ExecuteIntersectionQuery(layerEnvelope, ds);
                DataSource.Close();
                if (ds.Tables.Count > 0)
                {
                    g.TextRenderingHint = TextRenderingHint;
                    g.SmoothingMode = SmoothingMode;

                    labels = CreateLabelDefinitions(g, map, ds.Tables[0]);
                }
            }
            
            if (labels == null || labels.Count == 0)
            {
                // Obsolete (and will cause infinite loop)
                //base.Render(g, map);
                return;
            }

            if (Style.CollisionDetection)
                _labelFilter?.Invoke(labels);

            var combinedArea = RectangleF.Empty;
            
            for (int i = 0; i < labels.Count; i++)
            {
                var baseLabel = labels[i]; 
                if (!baseLabel.Show)
                    continue;

                var font = baseLabel.Style.GetFontForGraphics(g);
                
                if (labels[i] is Label)
                {
                    var label = baseLabel as Label;
                    var canvasArea = VectorRenderer.DrawLabelEx(
                        g, label.Location, label.Style.Offset,
                        font, label.Style.ForeColor, label.Style.BackColor, 
                        label.Style.Halo, label.Rotation, label.Text, map, 
                        label.Style.HorizontalAlignment, label.LabelPoint);

                    combinedArea = canvasArea.ExpandToInclude(combinedArea);
                }
                else if (labels[i] is PathLabel)
                {
                    var pathLabel  = labels[i] as PathLabel;
                    var lblStyle = pathLabel.Style;

                    var background = pathLabel.AffectedArea.ExteriorRing.TransformToImage(map);
                    if (lblStyle.BackColor != null && lblStyle.BackColor != Brushes.Transparent)
                        using (var gp = new GraphicsPath())
                        {
                            gp.AddPolygon(background);
                            g.FillPath(lblStyle.BackColor, gp);
                        }

                    g.DrawString(lblStyle.Halo, new SolidBrush(lblStyle.ForeColor), 
                        pathLabel.Text, font.FontFamily, (int) font.Style, font.Size,
                        lblStyle.GetStringFormat(), lblStyle.IgnoreLength, pathLabel.Location, pathLabel.Box.Height);
                    
                    combinedArea = background.ToRectangleF().ExpandToInclude(combinedArea);
                }
            }

            CanvasArea = combinedArea; 
            
            // Obsolete (and will cause infinite loop)
            //base.Render(g, map);
        }

       private List<BaseLabel> CreateLabelDefinitions(Graphics g, MapViewport map, FeatureDataTable features)
        {
            var labels = new List<BaseLabel>();
            var factory = new GeometryFactory();

            for (int i = 0; i < features.Count; i++)
            {
                var feature = features[i];

                LabelStyle style;
                if (Theme != null) //If thematics is enabled, lets override the style
                    style = Theme.GetStyle(feature) as LabelStyle;
                else
                    style = Style;

                // Do we need to render at all?
                if (!style.Enabled) continue;

                // Rotation
                float rotationStyle = style != null ? style.Rotation : 0f;
                float rotationColumn = 0f;
                if (!String.IsNullOrEmpty(RotationColumn))
                    Single.TryParse(feature[RotationColumn].ToString(), NumberStyles.Any, Map.NumberFormatEnUs,
                        out rotationColumn);
                float rotation = rotationStyle + rotationColumn;

                // Priority
                int priority = Priority;
                if (_getPriorityMethod != null)
                    priority = _getPriorityMethod(feature);
                else if (!String.IsNullOrEmpty(PriorityColumn))
                    Int32.TryParse(feature[PriorityColumn].ToString(), NumberStyles.Any, Map.NumberFormatEnUs,
                        out priority);

                // Text
                string text;
                if (_getLabelMethod != null)
                    text = _getLabelMethod(feature);
                else
                    text = feature[LabelColumn].ToString();

                if (String.IsNullOrEmpty(text)) continue;

                // Geometry
                feature.Geometry = ToTarget(feature.Geometry);

                // for lineal geometries, clip to ensure proper labeling
                if (feature.Geometry is ILineal)
                    feature.Geometry = ClipLinealGeomToViewExtents(map, feature.Geometry);

                if (feature.Geometry is IPolygonal)
                {
                    // TO CONSIDER clip to ViewExtents?
                    // This will ensure that polygons only partly in view will be labelled
                    // but perhaps need complexity threshold (eg Pts < 500) so as not to impact rendering
                    // or new prop bool PartialPolygonalLabel
                }
                
                if (feature.Geometry is IGeometryCollection geoms)
                {
                    if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.All)
                    {
                        for (int j = 0; j < geoms.Count; j++)
                        {
                            var lbl = CreateLabelDefinition(feature, geoms.GetGeometryN(j), text, rotation,
                                priority, style, map, g, _getLocationMethod);
                            if (lbl != null)
                                labels.Add(lbl);
                        }
                    }
                    else if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.CommonCenter)
                    {
                        if (geoms.NumGeometries > 0)
                        {
                            var pt = geoms.Centroid;
                            double closest = double.MaxValue;
                            int idxOfClosest = 0;
                            for (int j = 0; j < geoms.NumGeometries; j++)
                            {
                                var geom = geoms.GetGeometryN(j);
                                double dist = geom.Distance(pt);
                                if (dist < closest)
                                {
                                    closest = dist;
                                    idxOfClosest = j;
                                }
                            }

                            var lbl = CreateLabelDefinition(feature, geoms.GetGeometryN(idxOfClosest), text,
                                rotation, priority, style, map, g, _getLocationMethod);
                            if (lbl != null)
                                labels.Add(lbl);
                        }
                    }
                    else if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.First)
                    {
                        if (geoms.NumGeometries > 0)
                        {
                            var lbl = CreateLabelDefinition(feature, geoms.GetGeometryN(0), text,
                                rotation, priority, style, map, g, _getLocationMethod);
                            if (lbl != null)
                                labels.Add(lbl);
                        }
                    }
                    else if (MultipartGeometryBehaviour == MultipartGeometryBehaviourEnum.Largest)
                    {
                        if (geoms.NumGeometries > 0)
                        {
                            double largestVal = 0d;
                            int idxOfLargest = 0;
                            for (var j = 0; j < geoms.NumGeometries; j++)
                            {
                                var geom = geoms.GetGeometryN(j);
                                if (geom is ILineString lineString && lineString.Length > largestVal)
                                {
                                    largestVal = lineString.Length;
                                    idxOfLargest = j;
                                }
                                if (geom is IMultiLineString multiLineString && multiLineString.Length > largestVal)
                                {
                                    largestVal = multiLineString.Length;
                                    idxOfLargest = j;
                                }
                                if (geom is IPolygon polygon && polygon.Area > largestVal)
                                {
                                    largestVal = polygon.Area;
                                    idxOfLargest = j;
                                }
                                if (geom is IMultiPolygon multiPolygon && multiPolygon.Area > largestVal)
                                {
                                    largestVal = multiPolygon.Area;
                                    idxOfLargest = j;
                                }
                            }

                            var lbl = CreateLabelDefinition(feature, geoms.GetGeometryN(idxOfLargest), text, rotation, priority, style,
                                map, g, _getLocationMethod);
                            if (lbl != null)
                                labels.Add(lbl);
                        }
                    }
                }
                else
                {
                    BaseLabel lbl = CreateLabelDefinition(feature, feature.Geometry, text, rotation, priority, style, map, g, _getLocationMethod);
                    if (lbl != null)
                        labels.Add(lbl);
                }
            }

            return labels;
        }
        private static BaseLabel CreateLabelDefinition(FeatureDataRow fdr, IGeometry geom, string text, float rotation, 
            int priority, LabelStyle style, MapViewport map, Graphics g, GetLocationMethod getLocationMethod)
        {
            //ONLY atomic geometries
            Debug.Assert(!(geom is IGeometryCollection));

            if (geom == null) 
                return null;

            BaseLabel lbl;
            var font = style.GetFontForGraphics(g);

            var size = VectorRenderer.SizeOfString(g, text, font);

            if (geom is ILineString ls)
                return CreatePathLabel(ls, text, size, priority, style, map);

            var worldPosition = getLocationMethod == null
                ? geom.EnvelopeInternal.Centre
                : getLocationMethod(fdr);

            if (worldPosition == null) return null;

            var position = map.WorldToImage(worldPosition);

            var location = new PointF(
                position.X - size.Width*(short) style.HorizontalAlignment*0.5f,
                position.Y - size.Height*(short) (2 - (int) style.VerticalAlignment)*0.5f);

            if (location.X - size.Width > map.Size.Width || location.X + size.Width < 0 ||
                location.Y - size.Height > map.Size.Height || location.Y + size.Height < 0)
                return null;

            if (!style.CollisionDetection)
                lbl = new Label(text, location, rotation, priority, null, style)
                    {LabelPoint = position};
            else
            {
                //Collision detection is enabled so we need to measure the size of the string
                lbl = new Label(text, location, rotation, priority,
                                new LabelBox(location.X - style.CollisionBuffer.Width,
                                             location.Y - style.CollisionBuffer.Height,
                                             size.Width + 2f*style.CollisionBuffer.Width,
                                             size.Height + 2f*style.CollisionBuffer.Height), style) 
                                { LabelPoint = position }; 
            }

            return lbl;
        }

        private static BaseLabel CreatePathLabel(ILineString line, string text, SizeF textMeasure,
            int priority, LabelStyle style, MapViewport map)
        {
            if (line == null)
                return null;

            var factory = line.Factory;

            // Simplify the line for smoother labeling
            double avgCharacterSpace = 2d * textMeasure.Width / text.Length * map.PixelWidth;
            var simplifier = new NetTopologySuite.Simplify.VWLineSimplifier(line.Coordinates, avgCharacterSpace);
            line = factory.CreateLineString(simplifier.Simplify());

            var labelLength = textMeasure.Width * map.PixelWidth;
            var labelHeight = textMeasure.Height * map.PixelHeight;

            var offsetX = style.Offset.X * map.PixelWidth; // positive = increasing measure
            var offsetY = style.Offset.Y * map.PixelHeight; // positive = right side of line

            var start = 0d;
            if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center)
                start = line.Length * 0.5 - labelLength * 0.5;
            else if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right)
                start = line.Length - labelLength;

            start += offsetX;

            // Constrain label length
            if (labelLength > 0.95 * line.Length && !style.IgnoreLength ||
                start + labelLength < 0 || start > line.Length)
                return null;

            // LengthIndexedLine idea courtesy FObermaier
            NetTopologySuite.LinearReferencing.LengthIndexedLine lil;

            // optimize for detailed lines (eg labelling rivers at continental level)
            // ratio and NumPoints based on instinct.... feel free to revise
            var mid = start + labelLength / 2.0;
            if (labelLength / line.Length < 0.5 && line.NumPoints > 200 && mid >= 0 && mid < line.Length)
            {
                lil = new NetTopologySuite.LinearReferencing.LengthIndexedLine(line);
                var midPt = lil.ExtractPoint(mid);
                // extract slightly more than label length to ensure offsetCurve follows line geometry
                var halfLen = labelLength * 0.6;
                // ensure non-negative indexes constrained to line length (due to special LengthIndexLine functionality) 
                line = (LineString) lil.ExtractLine(Math.Max(0, mid - halfLen), Math.Min(mid + halfLen, line.Length));
                // reset start
                lil = new NetTopologySuite.LinearReferencing.LengthIndexedLine(line);
                mid = lil.IndexOf(midPt);
                start = mid - labelLength / 2.0;
            }

            // basic extend
            var end = start + labelLength;
            if (start < 0 || end > line.Length)
            {
                line = ExtendLine(line,
                    start < 0 ? -1 * start : 0,
                    end > line.Length ? end - line.Length : 0);
                start = 0;
                end = start + labelLength;
            }

            lil = new NetTopologySuite.LinearReferencing.LengthIndexedLine(line);
            // reverse
            var startPt = lil.ExtractPoint(start);
            var endPt = lil.ExtractPoint(end);
            if (LineNeedsReversing(startPt, endPt, false, map))
            {
                start = end;
                end = start - labelLength;
            }
            line = (ILineString) lil.ExtractLine(start, end);

            // Build offset curve
            ILineString offsetCurve;
            var bufferParameters =
                new NetTopologySuite.Operation.Buffer.BufferParameters(4,
                    GeoAPI.Operation.Buffer.EndCapStyle.Flat);

            // determine offset curve that will run through the vertical centre of the text
            if (style.VerticalAlignment != LabelStyle.VerticalAlignmentEnum.Middle)
            {
                var ocb = new NetTopologySuite.Operation.Buffer.OffsetCurveBuilder(factory.PrecisionModel,
                    bufferParameters);

                // Left side positive
                var offsetCurvePoints = ocb.GetOffsetCurve(line.Coordinates,
                    ((int) style.VerticalAlignment - 1) * 0.5 * labelHeight - offsetY);
                offsetCurve = factory.CreateLineString(offsetCurvePoints);
            }
            else
            {
                offsetCurve = line;
            }

            // basic extend
            var ratio = labelLength / offsetCurve.Length;
            if (ratio > 1.01)
            {
                var diff = labelLength - offsetCurve.Length;
                offsetCurve = ExtendLine(offsetCurve, diff / 2d, diff / 2d);
            }

            // enclosing polygon in world coords
            var affectedArea = (IPolygon) offsetCurve.Buffer(0.5d * labelHeight, bufferParameters);

            // fast, basic check (technically should use polygons for rotated views)
            if (!map.Envelope.Contains(affectedArea.EnvelopeInternal))
                return null;

            // using labelBox to pass text height to WarpedPath
            return new PathLabel(text, LineStringToPath(offsetCurve, map), 0, priority, 
                new LabelBox(0,0,textMeasure.Width,textMeasure.Height), style)
            {
                AffectedArea = affectedArea
            };
        }

        private static ILineString ExtendLine(ILineString line, double startDist, double endDist)
        {
            var numPts = (startDist > 0 ? 1 : 0) + (endDist > 0 ? 1 : 0);
            if (numPts == 0) return line;

            var cs = line.Factory.CoordinateSequenceFactory.Create(line.CoordinateSequence.Count + numPts, line.CoordinateSequence.Dimension);
            var offset = 0;
            
            if (startDist > 0)
            {
                var rad = Azimuth(line.Coordinates[1], line.Coordinates[0]);
                var coords = new[]
                {
                    Traverse(line.Coordinates[0], rad, startDist)
                };
                var startSeq = line.Factory.CoordinateSequenceFactory.Create(coords);
                CoordinateSequences.Copy(startSeq, 0, cs, offset++, 1);
            }

            CoordinateSequences.Copy(line.CoordinateSequence, 0, cs, offset, line.CoordinateSequence.Count);
            offset += line.CoordinateSequence.Count;

            if (endDist > 0)
            {
                var endPoints = new  []
                {
                    line.Coordinates[line.Coordinates.Length - 2],
                    line.Coordinates[line.Coordinates.Length - 1]
                };

                var rad = Azimuth(endPoints[0], endPoints[1]);
                var coords = new[]
                {
                    Traverse(endPoints[1], rad, endDist)
                };
                var es = line.Factory.CoordinateSequenceFactory.Create(coords);
                CoordinateSequences.Copy(es, 0, cs, offset, 1);
            }

            return line.Factory.CreateLineString(cs);
        }

        private static double Azimuth( Coordinate c1, Coordinate c2)
        {
            var dX = c2.X - c1.X;
            var dY = c2.Y - c1.Y;
            return  Math.PI / 2 - Math.Atan2(dY, dX);
        }
        
        private static Coordinate Traverse(Coordinate coord, double azimuth, double dist)
        {
            return new Coordinate(
                coord.X + dist * Math.Sin(azimuth),
                coord.Y + dist * Math.Cos(azimuth)
            );
        }
        private IGeometry ClipLinealGeomToViewExtents(MapViewport map, IGeometry geom)
        {
            if (map.MapTransform.IsIdentity)
            {
                var lineClipping = new CohenSutherlandLineClipping(map.Envelope.MinX, map.Envelope.MinY,
                    map.Envelope.MaxX, map.Envelope.MaxY);

                if (geom is ILineString)
                    return lineClipping.ClipLineString(geom as ILineString);
                
                if (geom is IMultiLineString)
                    return lineClipping.ClipLineString(geom as IMultiLineString);
            }
            else
            {
                var clipPolygon = new Polygon(new LinearRing(new[]
                    {
                        new Coordinate(map.Center.X - map.Zoom * .5, map.Center.Y - map.MapHeight * .5),
                        new Coordinate(map.Center.X - map.Zoom * .5, map.Center.Y + map.MapHeight * .5),
                        new Coordinate(map.Center.X + map.Zoom * .5, map.Center.Y + map.MapHeight * .5),
                        new Coordinate(map.Center.X + map.Zoom * .5, map.Center.Y - map.MapHeight * .5),
                        new Coordinate(map.Center.X - map.Zoom * .5, map.Center.Y - map.MapHeight * .5)
                    }
                ));

                var at = AffineTransformation.RotationInstance(
                    Degrees.ToRadians(map.MapTransformRotation), map.Center.X, map.Center.Y);

                clipPolygon = (Polygon) at.Transform(clipPolygon);

                if (geom is ILineString)
                    return clipPolygon.Intersection(geom as ILineString);
                
                if (geom is IMultiLineString)
                    return clipPolygon.Intersection(geom as IMultiLineString);
            }

            return null;
        }
       
        /// <summary>
        /// Very basic test to check for positive direction of Linestring, taking into account map rotation
        /// </summary>
        /// <param name="start">start of text</param>
        /// <param name="end">end of text</param>
        /// <param name="isRightToLeft"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private static bool LineNeedsReversing(Coordinate start, Coordinate end, bool isRightToLeft, MapViewport map)
        {
            double startX, endX;
            if (map.MapTransform.IsIdentity)
            {
                startX = start.X;
                endX = end.X;
            }
            else
            {
                var pts = map.WorldToImage(new[] {start, end}, true);
                startX = pts[0].X;
                endX = pts[1].X;
            }
            
            var dx = endX - startX;
            if (isRightToLeft && dx < 0)
                return false;
            
            return isRightToLeft || !(dx >= 0);
        }
        
        /// <summary>
        /// Function to transform a linestring to a graphics path for further processing
        /// </summary>
        /// <param name="lineString">The Linestring</param>
        /// <param name="map">The map</param>
        /// <!--<param name="useClipping">A value indicating whether clipping should be applied or not</param>-->
        /// <returns>A GraphicsPath</returns>
        public static GraphicsPath LineStringToPath(ILineString lineString, MapViewport map/*, bool useClipping*/)
        {
            var gp = new GraphicsPath(FillMode.Alternate);
                gp.AddLines(lineString.TransformToImage(map));
            return gp;
        }
    }
}
