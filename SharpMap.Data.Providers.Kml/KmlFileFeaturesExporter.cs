// Copyright 2015 - Spartaco Giubbolini (spartaco@sgsoftware.it)
//
// This file is part of SharpMap.Data.Providers.Kml.
// SharpMap.Data.Providers.Kml is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Data.Providers.Kml is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using ColorMode = SharpKml.Dom.ColorMode;
using Point = SharpKml.Dom.Point;

namespace SharpMap.Data.Providers
{
    public partial class KmlFileFeaturesExporter
    {
        #region NotCloseableMemoryStream
        private class NotCloseableMemoryStream : MemoryStream
        {
            private bool _disposing;

            public override void Close()
            {
                if (_disposing)
                    base.Close();
            }

            public void ForceDispose()
            {
                _disposing = true;
                Dispose(true);
            }
        }
        #endregion

        #region ExportContext
        public class ExportContext
        {
            public ExportContext(KmlFileFeaturesExporter exporter, IList<string> additionalFiles, FeatureDataRow feature)
            {
                if (exporter == null)
                    throw new ArgumentNullException("exporter");

                if (additionalFiles == null)
                    throw new ArgumentNullException("additionalFiles");

                if (feature == null)
                    throw new ArgumentNullException("feature");

                Exporter = exporter;
                AdditionalFiles = additionalFiles;
                Feature = feature;
            }

            public KmlFileFeaturesExporter Exporter { get; private set; }

            public IList<string> AdditionalFiles { get; private set; }

            public FeatureDataRow Feature { get; private set; }

            public bool IsKmz { get { return Exporter.IsKmz; } }
        }
        #endregion

        #region fields
        private readonly FeatureDataTable _featureDataTable;
        private int _sequence;
        private ICoordinateTransformationFactory _coordinateTransformationFactory;
        private ICoordinateSystemFactory _coordinateSystemFactory;
        private IGeometryFactory _earthGeometryFactory;
        private int _earthSrid;
        private ICoordinateSystem _earthCs;
        private readonly List<string> _additionalFiles;
        #endregion

        #region events
        public event EventHandler Started;
        public event EventHandler Ended;
        #endregion

        #region ctor
        public KmlFileFeaturesExporter(FeatureDataTable featureDataTable)
        {
            if (featureDataTable == null)
                throw new ArgumentNullException("featureDataTable");

            _featureDataTable = featureDataTable;

            EarthSrid = 4326;
            GetFeatureSnippet = feature => string.Empty; // this forces empty snippets

            _additionalFiles = new List<string>();
        }
        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the name of the column that provides the identifiers
        /// </summary>
        public string IdColumn { get; set; }

        /// <summary>
        /// Gets or sets the name of the column that provides the name of the feature
        /// </summary>
        public string NameColumn { get; set; }

        public string Snippet { get; set; }

        public Func<ExportContext, StyleSelector> GetStyle { get; set; }

        public Func<ExportContext, string> GetFeatureSnippet { get; set; }

        public ICoordinateTransformation CoordinateTransformation { get; set; }

        public int EarthSrid
        {
            get { return _earthSrid; }
            set
            {
                if (_earthSrid != value)
                {
                    _earthSrid = value;
                    _earthGeometryFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(value);

                    _earthCs = SharpMap.Session.Instance.CoordinateSystemServices.GetCoordinateSystem(value);
                }

            }
        }

        private bool IsKmz { get; set; }

        #endregion

        #region public methods

        public KmzFile ExportToKmz()
        {
            IsKmz = true;
            var kml = InternalExport();

            var kmz = KmzFile.Create(kml);
            foreach (var additionalFile in _additionalFiles)
            {
                using (var file = File.OpenRead(additionalFile))
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);

                    ms.Seek(0, SeekOrigin.Begin);

                    var bytes = ms.ToArray();
                    kmz.AddFile(Path.GetFileName(additionalFile), bytes);
                }

                File.Delete(additionalFile);
            }

            return kmz;
        }

        public KmlFile ExportToKml()
        {
            IsKmz = false;

            return InternalExport();
        }

        public static Func<ExportContext, SharpKml.Dom.Style> CreateFromSharpmapStyle(IStyle sharpmapStyle)
        {
            if (sharpmapStyle == null)
                throw new ArgumentNullException("sharpmapStyle");

            var vectorStyle = sharpmapStyle as VectorStyle;
            if (vectorStyle == null) return null;

            SharpKml.Dom.Style style = null;

            var install = new Action<ExportContext>(context =>
            {
                style = new SharpKml.Dom.Style
                {
                    Id = sharpmapStyle.GetHashCode().ToString(CultureInfo.InvariantCulture)
                };

                if (vectorStyle.Line != null)
                {
                    var lineStyle = new LineStyle();
                    style.Line = lineStyle;

                    var color = vectorStyle.Line.Color;
                    lineStyle.Color = new Color32(color.A, color.B, color.G, color.R);
                    lineStyle.ColorMode = ColorMode.Normal;
                    lineStyle.Width = vectorStyle.Line.Width;
                }

                var solidColor = ConvertToColor32(vectorStyle.Fill);
                if (solidColor != null)
                {
                    var polygonStyle = new PolygonStyle();
                    style.Polygon = polygonStyle;

                    polygonStyle.Fill = true;
                    polygonStyle.Color = solidColor;
                }

                if (vectorStyle.Symbol == null)
                {
                    if (vectorStyle.PointSize > 0)
                    {
                        var iconStyle = new IconStyle();

                        var pointColor = vectorStyle.PointColor != null
                            ? ConvertToColor32(vectorStyle.PointColor) ?? new Color32(255, 0, 0, 0)
                            : new Color32(255, 0, 0, 0);

                        iconStyle.Color = pointColor;
                        iconStyle.ColorMode = ColorMode.Normal;
                        iconStyle.Icon = new IconStyle.IconLink(Pushpins.ShadedDot);
                        iconStyle.Scale = vectorStyle.PointSize / 6;

                        style.Icon = iconStyle;
                    }
                }
                else
                {
                    var additionalFile = SaveImagetoDisk(vectorStyle.Symbol);
                    Debug.Assert(additionalFile != null, "additionalFile != null");

                    context.AdditionalFiles.Add(additionalFile);

                    var iconStyle = new IconStyle
                    {
                        Icon = new IconStyle.IconLink(new Uri(context.IsKmz ? Path.GetFileName(additionalFile) : additionalFile, UriKind.Relative)),
                        Scale = vectorStyle.SymbolScale
                    };

                    style.Icon = iconStyle;
                }
            });

            EventHandler endedHandler = delegate { style = null; };

            return context =>
            {
                if (style == null && sharpmapStyle.Enabled)
                {
                    install(context);

                    context.Exporter.Ended -= endedHandler;
                    context.Exporter.Ended += endedHandler;
                }

                return style;
            };
        }

        public static Func<ExportContext, SharpKml.Dom.Style> CreateFromSharpmapTheme(ITheme theme)
        {
            if (theme == null)
                throw new ArgumentNullException("theme");

            return context =>
            {
                var sharpmapStyle = theme.GetStyle(context.Feature);
                return sharpmapStyle != null ? CreateFromSharpmapStyle(sharpmapStyle)(context) : null;
            };
        }

        #endregion

        #region protected members

        protected virtual void OnStarted()
        {
            var handler = Started;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void OnEnded()
        {
            var handler = Ended;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected int GetNextSequence()
        {
            return ++_sequence;
        }

        protected virtual IGeometry ToTarget(IGeometry geometry)
        {
            if (geometry.SRID == EarthSrid || (geometry.SRID <= 0 && CoordinateTransformation == null))
                return geometry;

            if (CoordinateTransformation == null || geometry.SRID != CoordinateTransformation.SourceCS.AuthorityCode)
            {
                var sourceCs = SharpMap.Session.Instance.CoordinateSystemServices.GetCoordinateSystem(geometry.SRID);

                CoordinateTransformation = SharpMap.Session.Instance.CoordinateSystemServices.CreateTransformation(
                    sourceCs,
                    _earthCs);
            }

            return GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.MathTransform, _earthGeometryFactory);
        }

        protected void AddAdditionalFile(string file)
        {
            _additionalFiles.Add(file);
        }

        protected virtual Feature CreateKmlFeature(FeatureDataRow feature, StyleSelector style)
        {
            var geometry = feature.Geometry;

            geometry = ToTarget(geometry);

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    {
                        var location = geometry.Coordinate;
                        var p = new Point { Coordinate = new Vector(location.Y, location.X) };

                        return WrapPlacemark(p, style, feature);
                    }
                case OgcGeometryType.MultiPoint:
                    {
                        var multiGeometry = new MultipleGeometry();

                        foreach (var coordinate in geometry.Coordinates)
                        {
                            var p = new Point { Coordinate = new Vector(coordinate.Y, coordinate.X) };

                            multiGeometry.AddGeometry(p);
                        }

                        return WrapPlacemark(multiGeometry, style, feature);
                    }
                case OgcGeometryType.LineString:
                    {
                        var lineString = CreateLineString(geometry);

                        return WrapPlacemark(lineString, style, feature);
                    }
                case OgcGeometryType.Polygon:
                    {
                        var polygon = (IPolygon)geometry;

                        var kmlPolygon = CreateKmlPolygon(polygon);

                        return WrapPlacemark(kmlPolygon, style, feature);
                    }
                case OgcGeometryType.MultiLineString:
                    {
                        var multiGeometry = new MultipleGeometry();

                        var multiLineString = (IMultiLineString)geometry;
                        foreach (var innerGeometry in multiLineString.Geometries)
                        {
                            var lineString = CreateLineString(innerGeometry);

                            multiGeometry.AddGeometry(lineString);
                        }

                        return WrapPlacemark(multiGeometry, style, feature);
                    }
                case OgcGeometryType.MultiPolygon:
                    {
                        var multiGeometry = new MultipleGeometry();
                        var multiPoly = (IMultiPolygon)geometry;

                        foreach (var innerGeometry in multiPoly.Geometries.Cast<IPolygon>())
                        {
                            var polygon = CreateKmlPolygon(innerGeometry);

                            multiGeometry.AddGeometry(polygon);
                        }

                        return WrapPlacemark(multiGeometry, style, feature);
                    }
                default:
                    throw new InvalidOperationException("Geometry not supported");
            }
        }

        protected virtual string CreateDescription(FeatureDataRow feature)
        {
            var values = feature.Table.Columns
                .Cast<DataColumn>()
                .Where(dc => dc.ColumnName != NameColumn)
                .ToDictionary(dc => dc.ColumnName, dc => feature[dc]);

            var s = values.Select(kvp => kvp.Key + " = " + kvp.Value);

            return string.Join("\r\n", s);
        }

        protected virtual Document CreateRootDocument()
        {
            var document = new Document { Name = _featureDataTable.TableName, Open = true, Visibility = true };

            // document's name can't be empty
            if (string.IsNullOrEmpty(document.Name))
                document.Name = "Document" + Guid.NewGuid();

            if (!string.IsNullOrEmpty(Snippet))
                document.Snippet = new Snippet { MaximumLines = 2, Text = Snippet };

            document.Time = new Timestamp { When = DateTime.Now };

            return document;
        }

        #endregion

        #region private members

        private KmlFile InternalExport()
        {
            OnStarted();

            _additionalFiles.Clear();

            var kml = new Kml();

            var document = CreateRootDocument();

            kml.Feature = document;

            var embeddedStyles = new Dictionary<string, StyleSelector>();

            foreach (FeatureDataRow featureRow in _featureDataTable.Rows)
            {
                var kmlStyle = GetStyle != null ? GetStyle(new ExportContext(this, _additionalFiles, featureRow)) : null;
                if (kmlStyle != null)
                {
                    var xml = GetStyleXml(kmlStyle);

                    if (!embeddedStyles.ContainsKey(xml))
                    {
                        document.AddStyle(kmlStyle);

                        embeddedStyles.Add(xml, kmlStyle);
                    }
                }
                var feature = CreateKmlFeature(featureRow, kmlStyle);
                document.AddFeature(feature);
            }

            var kmlFile = KmlFile.Create(kml, true);

            OnEnded();

            return kmlFile;
        }

        private static string GetStyleXml(Element e)
        {
            var kml = KmlFile.Create(e, true);
            // we must to use NotCloseableMemoryStream because kml.Save pretend to close the stream we provide, but we do not want that.
            var ms = new NotCloseableMemoryStream();
            try
            {
                kml.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(ms))
                    return reader.ReadToEnd();
            }
            finally
            {
                // we can't just use the Dispose method because it does not call its virtual overload
                ms.ForceDispose();
            }
        }

        private LineString CreateLineString(IGeometry geometry)
        {
            var lineString = new LineString { Coordinates = new CoordinateCollection() };

            foreach (var coordinate in geometry.Coordinates)
            {
                var v = new Vector(coordinate.Y, coordinate.X);
                lineString.Coordinates.Add(v);
            }

            return lineString;
        }
        private Polygon CreateKmlPolygon(IPolygon polygon)
        {
            var kmlPolygon = new Polygon();
            var ring = new LinearRing { Coordinates = new CoordinateCollection() };

            kmlPolygon.OuterBoundary = new OuterBoundary { LinearRing = ring };

            foreach (var coordinate in polygon.ExteriorRing.Coordinates)
            {
                ring.Coordinates.Add(new Vector(coordinate.Y, coordinate.X));
            }

            foreach (var interiorRing in polygon.InteriorRings)
            {
                var innerBoundary = new InnerBoundary();
                kmlPolygon.AddInnerBoundary(innerBoundary);

                ring = new LinearRing { Coordinates = new CoordinateCollection() };

                innerBoundary.LinearRing = ring;

                foreach (var coordinate in interiorRing.Coordinates)
                {
                    ring.Coordinates.Add(new Vector(coordinate.Y, coordinate.X));
                }
            }

            return kmlPolygon;
        }
        private static string SaveImagetoDisk(Image img)
        {
            var tmpPath = Path.GetTempFileName();
            tmpPath = Path.ChangeExtension(tmpPath, ".png");
            using (var file = File.Create(tmpPath))
            {
                img.Save(file, ImageFormat.Png);
            }

            return tmpPath;
        }

        private static Color32? ConvertToColor32(Brush brush)
        {
            var solidFill = brush as SolidBrush;
            if (solidFill != null)
            {
                return new Color32(solidFill.Color.A, solidFill.Color.B, solidFill.Color.G,
                    solidFill.Color.R);
            }

            var hatchFill = brush as HatchBrush;
            if (hatchFill != null)
            {
                // hatchBrush fill is not supported by KML, so we just use a solid color
                return new Color32(hatchFill.ForegroundColor.A, hatchFill.ForegroundColor.B,
                    hatchFill.ForegroundColor.G, hatchFill.ForegroundColor.R);
            }

            return null;
        }

        private Placemark WrapPlacemark(Geometry kmlGeometry, StyleSelector style, FeatureDataRow feature)
        {
            var placemark = new Placemark();
            if (!string.IsNullOrEmpty(NameColumn))
                placemark.Name = Get(feature, NameColumn);

            placemark.Description = new Description { Text = CreateDescription(feature) };
            placemark.Geometry = kmlGeometry;
            if (GetFeatureSnippet != null)
            {
                placemark.Snippet = new Snippet { Text = GetFeatureSnippet(new ExportContext(this, _additionalFiles, feature)) };
            }

            if (style != null)
            {
                placemark.StyleUrl = new Uri("#" + style.Id, UriKind.Relative);
            }

            return placemark;
        }

        private static string Get(FeatureDataRow feature, string columnName)
        {
            var value = feature[columnName];
            return value == null ? null : value.ToString();
        }

        #endregion
    }
}
