// Copyright 2014 -      Robert Smart (www.cnl-software.com)
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using GeoAPI.Geometries;
using SharpKml.Dom;
using SharpKml.Engine;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using LinearRing = SharpKml.Dom.LinearRing;
using LineString = SharpKml.Dom.LineString;
using Point = SharpKml.Dom.Point;
using Polygon = SharpKml.Dom.Polygon;
using Style = SharpKml.Dom.Style;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Kml/Kmz provider
    /// </summary>
    public class KmlProvider : IProvider
    {
        #region Static factory methods
        
        /// <summary>
        /// Creates a KmlProvider from a file
        /// </summary>
        /// <param name="filename">The path to the file</param>
        /// <returns>A Kml provider</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static KmlProvider FromKml(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (!File.Exists(filename))
                throw new FileNotFoundException("File not found", "filename");

            using (var s = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return FromKml(s);
            }
        }

        /// <summary>
        /// Creates a KmlProvider from a Kml stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static KmlProvider FromKml(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return new KmlProvider(KmlFile.Load(stream));
        }

        /// <summary>
        /// Creates a KmlProvider from a file
        /// </summary>
        /// <param name="filename">The path to the file</param>
        /// <param name="internalFile">The internal file to read</param>
        /// <returns>A Kml provider</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static KmlProvider FromKmz(string filename, string internalFile = null)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (!File.Exists(filename))
                throw new FileNotFoundException("File not found", "filename"); 
            
            using (var s = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return FromKmz(s, internalFile);
            }
        }

        /// <summary>
        /// Creates a KmlProvider from a Kmz stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="internalFile">The internal file to read</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static KmlProvider FromKmz(Stream stream, string internalFile = null)
        {
            var kmz = KmzFile.Open(stream);
            if (string.IsNullOrEmpty(internalFile))
                return new KmlProvider(kmz.GetDefaultKmlFile());

            //NOTE:DON'T KNOW IF THIS IS CORRECT!
            using (var ms = new MemoryStream(kmz.ReadFile(internalFile)))
            {
                return new KmlProvider(KmlFile.Load(ms));
            }
        }
        #endregion

        #region static constructor and fields

        private static readonly FeatureDataTable _schemaTable;

        static KmlProvider()
        {
            _schemaTable = new FeatureDataTable();
            AddColumnsToFeatureDataTable(_schemaTable);
        }

        #endregion

        #region private fields and constants
        private IGeometryFactory _geometryFactory;
        private Dictionary<Placemark, List<IGeometry>> _geometrys;
        private Dictionary<string, VectorStyle> _kmlStyles;
        private List<StyleMap> _styleMaps;

        private const string DefaultStyleId = "{6787C5B3-6482-4B96-9C2D-2C6236D2AC50}";
        private const string DefaultPointStyleId = "{E2892545-7CF4-48A1-B8F0-5A0BF06EF0E1}";

        #endregion

        /// <summary>
        /// Method to create a theme for the Layer
        /// </summary>
        /// <returns>A theme</returns>
        /// <example language="C#">
        /// <code>
        /// </code>
        /// </example>
        public ITheme GetKmlTheme()
        {
            //todo layer will need to do this
            //Layer.Theme = assetTheme;
            return new CustomTheme(GetKmlStyle);
        }

        /// <summary>
        /// Gets or sets a value indicating that <see cref="SharpKml.Dom.LinearRing"/> are to be treated as polygons
        /// </summary>
        public bool RingsArePolygons { get; set; }

        /// <summary>
        /// Creates an instance of this class using the provided KmlFile
        /// </summary>
        /// <param name="kmlFile">The KmlFile</param>
        public KmlProvider(KmlFile kmlFile)
        {
            ParseKml(kmlFile);
        }

        /// <summary>
        /// Method to parse the KmlFile
        /// </summary>
        /// <param name="kmlFile">The file to parse</param>
        private void ParseKml(KmlFile kmlFile)
        {
            var kml = kmlFile.Root as Kml;
            if (kml == null)
            {
                throw new Exception("Kml file is null! Please check that the file conforms to http://www.opengis.net/kml/2.2 standards");
            }

            var doc = kml.Feature as Document;
            if (doc == null)
            {
                throw new Exception("Kml file does not have a document node! please check that the file conforms to http://www.opengis.net/kml/2.2 standards");
            }

            _geometryFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            ConnectionID = doc.Name;
            if (!string.IsNullOrEmpty(doc.Description.Text))
                ConnectionID += " (" + doc.Description.Text + ")";

            ExtractStyles(kml);
            ExtractStyleMaps(kml);
            ExtractGeometries(kml);

        }

        private void ExtractStyleMaps(Element kml)
        {
            _styleMaps = new List<StyleMap>();
            foreach (var style in kml.Flatten().OfType<StyleMapCollection>())
            {
                var styleMap = new StyleMap { Id = style.Id };
                _styleMaps.Add(styleMap);
                style.ToList().ForEach(x =>
                {
                    if (x.State != null)

                        switch (x.State.Value)
                        {
                            case StyleState.Normal:
                                styleMap.NormalStyleUrl = x.StyleUrl.ToString().Replace("#", "");
                                break;
                            case StyleState.Highlight:
                                styleMap.HighlightStyleUrl = x.StyleUrl.ToString().Replace("#", "");
                                break;
                        }

                });
            }
        }

        /// <summary>
        /// Style map class
        /// </summary>
        private class StyleMap
        {
            public string Id { get; set; }
            public string NormalStyleUrl { get; set; }
            public string HighlightStyleUrl { get; set; }
        }

        //todo needs buffing up
        private void ExtractStyles(Element kml)
        {
            _kmlStyles = new Dictionary<string, VectorStyle>();

            _kmlStyles.Add(DefaultStyleId, DefaultVectorStyle());
            _kmlStyles.Add(DefaultPointStyleId, DefaultPointStyle());

            foreach (var style in kml.Flatten().OfType<Style>())
            {
                if (string.IsNullOrEmpty(style.Id))
                    continue;

                var vectorStyle = new VectorStyle();
                vectorStyle.Enabled = true;

                if (style.Polygon != null)
                {

                    if (style.Polygon.Fill != null)
                    {
                        if (style.Polygon.Fill.Value)
                        {
                            var color = new SolidBrush(Color.FromArgb(style.Polygon.Color.Value.Argb));
                            //fill the polygon
                            vectorStyle.Fill = color;
                            vectorStyle.PointColor = color; //Color.FromArgb(100, color.R, color.G, color.B)
                        }
                        else
                        {
                            //don't fill it 
                            var color = new SolidBrush(Color.Transparent);
                            vectorStyle.Fill = color; //Color.FromArgb(100, color.R, color.G, color.B)
                            vectorStyle.PointColor = color;
                        }
                    }
                    else
                    {
                        var color = new SolidBrush(Color.FromArgb(style.Polygon.Color.Value.Argb));
                        //fill the polygon
                        vectorStyle.Fill = color;
                        vectorStyle.PointColor = color; //Color.FromArgb(100, color.R, color.G, color.B)
                    }

                    vectorStyle.EnableOutline = true;
                }

                if (style.Line != null)
                {
                    if (style.Line.Width != null)
                    {
                        var linePen = new Pen(
                            Color.FromArgb(style.Line.Color != null
                                ? style.Line.Color.Value.Argb
                                : Color.Black.ToArgb()), (float)style.Line.Width);

                        vectorStyle.Line = linePen;
                        vectorStyle.Outline = linePen;
                    }
                }

                try
                {
                    var symbolDict = new Dictionary<string, Image>();

                    if (style.Icon != null && style.Icon.Icon != null && style.Icon.Icon.Href != null)
                    {
                        if (symbolDict.ContainsKey(style.Icon.Icon.Href.ToString()))
                        {
                            vectorStyle.Symbol = symbolDict[style.Icon.Icon.Href.ToString()];
                        }
                        else
                        {
                            var newSymbol = GetImageFromUrl(style.Icon.Icon.Href);
                            symbolDict.Add(style.Icon.Icon.Href.ToString(), newSymbol);
                            vectorStyle.Symbol = newSymbol;
                        }

                        vectorStyle.SymbolScale = 1f;
                    }
                }
                catch (Exception ex)
                {

                    Trace.WriteLine(ex.Message);
                }


                _kmlStyles.Add(style.Id, vectorStyle);

            }
        }

        public VectorStyle GetKmlStyle(FeatureDataRow row)
        {
            //get styleID from row
            var styleId = (string)row["StyleUrl"];

            if (_kmlStyles.ContainsKey(styleId))
            {
                return _kmlStyles[styleId];
            }

            //look in style maps
            foreach (var stl in _styleMaps)
            {

                if (stl.Id == styleId)
                {
                    return _kmlStyles[stl.NormalStyleUrl];
                }
            }

            if (row.Geometry.OgcGeometryType == OgcGeometryType.Point ||
                row.Geometry.OgcGeometryType == OgcGeometryType.MultiPoint)
            {
                return DefaultPointStyle();
            }

            return DefaultVectorStyle();


        }

        private static VectorStyle DefaultPointStyle()
        {
            var vectorStyle = new VectorStyle();
            vectorStyle.Enabled = true;
            try
            {
                var defaultIcon = GetImageFromUrl(new Uri(@"http://www.google.com/intl/en_us/mapfiles/ms/icons/red-pushpin.png"));

                //use this as default symbol
                vectorStyle.Symbol = defaultIcon;
                vectorStyle.SymbolScale = 1f;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            vectorStyle.PointSize = 2f;
            vectorStyle.PointColor = Brushes.DarkGray;

            return vectorStyle;
        }

        private static VectorStyle DefaultVectorStyle()
        {
            var vectorStyle = new VectorStyle();
            vectorStyle.Enabled = true;

            vectorStyle.Line = new Pen(Brushes.DarkGray, 2f);
            vectorStyle.Outline = new Pen(Brushes.DarkGray, 2f);
            vectorStyle.Fill = new SolidBrush(Color.LightGray);
            vectorStyle.EnableOutline = true;

            return vectorStyle;
        }

        public void ExtractGeometries(Element kml)
        {
            _geometrys = new Dictionary<Placemark, List<IGeometry>>();

            //todo handle other geom types
            foreach (var f in kml.Flatten().OfType<Polygon>())
            {
                ProcessPolygonGeometry(f);
            }

            foreach (var f in kml.Flatten().OfType<LineString>())
            {
                ProcessLineStringGeometry(f);
            }

            foreach (var f in kml.Flatten().OfType<Point>())
            {
                ProcessPointGeometry(f);
            }

            foreach (var f in kml.Flatten().OfType<LinearRing>())
            {
                ProcessLinearRingGeometry(f);
            }

            foreach (var f in kml.Flatten().OfType<MultipleGeometry>())
            {
                ProcessMuiltipleGeometry(f);
            }
        }

        private void ProcessMuiltipleGeometry(MultipleGeometry f)
        {
            f.Geometry.ToList().ForEach(g =>
            {
                if (g is Polygon)
                {
                    ProcessPolygonGeometry((Polygon)g);
                }
                if (g is LineString)
                {
                    ProcessLineStringGeometry((LineString)g);
                }
                if (g is Point)
                {
                    ProcessPointGeometry((Point)g);
                }
                if (g is LinearRing)
                {
                    ProcessLinearRingGeometry((LinearRing) g);
                }
                if (g is MultipleGeometry)
                {
                    ProcessMuiltipleGeometry((MultipleGeometry)g);
                }
            });
        }

        private void ProcessPolygonGeometry(Polygon f)
        {
            var outerRing = _geometryFactory.CreateLinearRing(
                f.OuterBoundary.LinearRing.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray());

            var innerHoles = new List<ILinearRing>();

            foreach (var hole in f.InnerBoundary)
            {
                innerHoles.Add(_geometryFactory.CreateLinearRing(
                        hole.LinearRing.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray()));
            }

            var pGeom = _geometryFactory.CreatePolygon(outerRing, innerHoles.ToArray());
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        private void ProcessLineStringGeometry(LineString f)
        {
            var pGeom =_geometryFactory.CreateLineString(
                    f.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray());
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        private void ProcessPointGeometry(Point f)
        {
            var coords = new Coordinate(f.Coordinate.Longitude, f.Coordinate.Latitude);

            var pGeom = _geometryFactory.CreatePoint(coords);
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        private void ProcessLinearRingGeometry(LinearRing f)
        {
            var ring = _geometryFactory.CreateLinearRing(
                    f.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray());

            var geom = RingsArePolygons ? (IGeometry)_geometryFactory.CreatePolygon(ring) : ring;
            AddGeometryToCollection(f.GetParent<Placemark>(), geom);
        }

        private void AddGeometryToCollection(Placemark parent, IGeometry geom)
        {
            List<IGeometry> placeMarkGeoms;
            if (_geometrys.TryGetValue(parent, out placeMarkGeoms) == false)
            {
                placeMarkGeoms = new List<IGeometry>();
                _geometrys.Add(parent, placeMarkGeoms);
            }

            placeMarkGeoms.Add(geom);
        }

        private static object[] GetAssetProperties(Feature f)
        {
            return new object[]
            {
                f.Id,
                f.StyleUrl != null ? f.StyleUrl.ToString().Replace("#", "") : "",
                f
            };
        }

        /*
        private static FeatureDataRow GetAssetFeatureDataRow(FeatureDataTable fdt, string id, string styleId, Placemark geom)
        {
            FeatureDataRow newRow = fdt.NewRow();
            newRow["Id"] = id;
            newRow["StyleUrl"] = styleId;
            newRow["Object"] = geom;
            //newRow["Label"] = obj.Style.Label;

            return newRow;
        }
         */
        private static void AddColumnsToFeatureDataTable(FeatureDataTable fdt)
        {
            if (!fdt.Columns.Contains("Id"))
                fdt.Columns.Add("Id", typeof(string));

            if (!fdt.Columns.Contains("StyleUrl"))
                fdt.Columns.Add("StyleUrl", typeof(string));

            if (!fdt.Columns.Contains("Object"))
                fdt.Columns.Add("Object", typeof(Placemark));

            //if (!fdt.Columns.Contains("Label"))
            //    fdt.Columns.Add("Label", typeof(string));
        }

        public void Dispose()
        {
            // throw new System.NotImplementedException();
        }

        public Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var box = _geometryFactory.ToGeometry(bbox);
            var retCollection = new Collection<IGeometry>();

            foreach (var geometryList in _geometrys.Values)
            {
                geometryList.Where(box.Intersects).ToList().ForEach(retCollection.Add);
            }

            return retCollection;

        }

        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var box = _geometryFactory.ToGeometry(bbox);
            var res = new Collection<uint>();

            uint id = 0;
            
            _geometrys.Where(x => box.Intersects(_geometryFactory.BuildGeometry(x.Value))).ToList().ForEach(x =>
            {
                res.Add(id);
                id++;
            });
            return res;
        }

        public IGeometry GetGeometryByID(uint oid)
        {
            var sid = oid.ToString(NumberFormatInfo.InvariantInfo);
            var tmp = _geometrys.FirstOrDefault(x => x.Key.Id == sid);
            
            return tmp.Value != null ?
                _geometryFactory.BuildGeometry(tmp.Value) : null;
        }

        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var fdt = (FeatureDataTable)_schemaTable.Copy();

            /* // NOTE WHY IS THIS, No other provider behaves like that?
            if (ds.Tables.Count > 0)
            {
                fdt = ds.Tables[0];
            }
            else
            {
                fdt = new FeatureDataTable();
            }*/

            var pGeom = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            fdt.BeginLoadData();
            foreach (var feature in _geometrys)
            {
                feature.Value.Where(pGeom.Intersects).ToList()
                    .ForEach(v =>
                    {
                        var newRow = (FeatureDataRow) fdt.LoadDataRow(GetAssetProperties(feature.Key), true);
                        newRow.Geometry = v;
                    }
                    );
            }
            fdt.EndLoadData();

            ds.Tables.Add(fdt);
        }

        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(_geometryFactory.ToGeometry(box), ds);
        }

        public int GetFeatureCount()
        {
            return _geometrys.Count;
        }

        public FeatureDataRow GetFeature(uint oid)
        {
            var sid = oid.ToString(NumberFormatInfo.InvariantInfo);
            var tmp = _geometrys.FirstOrDefault(x => x.Key.Id == sid);

            if (tmp.Value != null)
            {
                var res = (FeatureDataRow) _schemaTable.NewRow();
                res.ItemArray = GetAssetProperties(tmp.Key);
                res.Geometry = _geometryFactory.BuildGeometry(tmp.Value);
                res.AcceptChanges();
                return res;
            }
            return null;
        }

        public Envelope GetExtents()
        {
            var retEnv = new Envelope();

            _geometrys.Values.ToList().ForEach(x => x.ForEach(v => retEnv.ExpandToInclude(v.EnvelopeInternal)));
            return retEnv;
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public string ConnectionID { get; private set; }

        public bool IsOpen { get; private set; }

        public int SRID { get { return 4326; } set { }}

        #region private helper methods
        private static Image GetImageFromUrl(Uri url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            using (var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var stream = httpWebReponse.GetResponseStream())
                {
                    if (stream != null)
                        return Image.FromStream(stream);
                }
            }
            return VectorStyle.DefaultSymbol;
        }
        #endregion

    }
}
