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
using SharpMap.Rendering.Symbolizer;
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
        /// <param name="stylesOnly">True to skip geometries and read styles only</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static KmlProvider FromKml(Stream stream, bool stylesOnly = false)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return new KmlProvider(KmlFile.Load(stream), stylesOnly, GetFileStreamPath(stream));
        }

        /// <summary>
        /// Creates a KmlProvider from a file
        /// </summary>
        /// <param name="filename">The path to the file</param>
        /// <param name="internalFile">The internal file to read, or null for default kml document</param>
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
        /// <param name="internalFile">The internal file to read, or null for default kml document</param>
        /// <param name="stylesOnly">True to skip geometries and read styles only</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static KmlProvider FromKmz(Stream stream,  string internalFile = null, bool stylesOnly = false)
        {
            var kmz = KmzFile.Open(stream);

            if (string.IsNullOrEmpty(internalFile))
                // typically doc.kml, but sometimes kmzFileNameBase.kml 
                return new KmlProvider(kmz.GetDefaultKmlFile(), stylesOnly, GetFileStreamPath(stream));
            else
                using (var ms = new MemoryStream(kmz.ReadFile(internalFile)))
                    return new KmlProvider(KmlFile.Load(ms), stylesOnly, GetFileStreamPath(stream));
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
        private Dictionary<string, StyleMap> _styleMaps;
        private Dictionary<string, Image> _symbolDict;
        private HashSet<string> _externalFiles;


        private readonly string _folder;

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
        [Obsolete("This Property does not have any effect, as LinearRings are no longer recognised as primary geometries.")]
        public bool RingsArePolygons { get; set; }

        /// <summary>
        /// Creates an instance of this class using the provided KmlFile
        /// </summary>
        /// <param name="kmlFile">The KmlFile</param>
        /// <param name="stylesOnly">True to skip geometries and read styles only</param>
        /// <param name="path">UNC File Path (if known) to resole relative style references</param>
        public KmlProvider(KmlFile kmlFile, bool stylesOnly = false, string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                // trailing dir separator required for resolving possible relative paths
                _folder = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;

            ParseKml(kmlFile, stylesOnly);
        }

        /// <summary>
        /// Method to parse the KmlFile
        /// </summary>
        /// <param name="kmlFile">The file to parse</param>
        /// <param name="stylesOnly">True to skip geometries and read styles only</param>
        private void ParseKml(KmlFile kmlFile, bool stylesOnly)
        {
            var kml = kmlFile.Root as Kml;
            if (kml == null)
            {
                // for further info refer to https://github.com/samcragg/sharpkml/issues/24 
                throw new Exception(
                    "Kml file is null! Please check that the file conforms to http://www.opengis.net/kml/2.2 standards");
            }

            var doc = kml.Feature as Document;
            if (doc == null)
            {
                throw new Exception(
                    "Kml file does not have a document node! please check that the file conforms to http://www.opengis.net/kml/2.2 standards");
            }

            _geometryFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            ConnectionID = doc.Name;
            if (doc.Description != null && !string.IsNullOrEmpty(doc.Description.Text))
                ConnectionID += " (" + doc.Description.Text + ")";

            ExtractStyles(kml);
            ExtractStyleMaps(kml);

            if (stylesOnly) return;
            
            ExtractGeometries(kml);
            ValidatePlacemarkStyles(kml);
        }

        /// <summary>
        /// Method called for rendering each feature (KML Placemark)      
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public VectorStyle GetKmlStyle(FeatureDataRow row)
        {
            // get styleID from row
            var styleId = (string) row["StyleUrl"];

            // get any inline style (overrides) that apply to this Placemark
            var pm = (Placemark) row["Object"];
            var styleOverrides = pm.Styles.OfType<Style>().ToList().FirstOrDefault();

            if (_kmlStyles.ContainsKey(styleId))
            {
                return ApplyStyleOverrides(_kmlStyles[styleId], styleOverrides);
            }
            
            if (_styleMaps.ContainsKey(styleId))
            {
                var sm = _styleMaps[styleId];
                if (_kmlStyles.ContainsKey(sm.NormalStyleUrl))
                    return ApplyStyleOverrides(_kmlStyles[sm.NormalStyleUrl], styleOverrides);
            }

            if (row.Geometry.OgcGeometryType == OgcGeometryType.Point ||
                row.Geometry.OgcGeometryType == OgcGeometryType.MultiPoint)
            {
                return ApplyStyleOverrides(DefaultPointStyle(), styleOverrides);
            }

            return ApplyStyleOverrides(DefaultVectorStyle(), styleOverrides);
        }

        /// <summary>
        /// Parse Styles defined at the head of the KML document 
        /// </summary>
        /// <param name="kml"></param>
        private void ExtractStyles(Element kml)
        {
            _kmlStyles = new Dictionary<string, VectorStyle>();

            _kmlStyles.Add(DefaultStyleId, DefaultVectorStyle());
            _kmlStyles.Add(DefaultPointStyleId, DefaultPointStyle());
            _symbolDict = new Dictionary<string, Image>();
            _externalFiles = new HashSet<string>();

            foreach (var kmlStyle in kml.Flatten().OfType<Style>())
            {
                if (string.IsNullOrEmpty(kmlStyle.Id))
                    continue;
                
                if (_kmlStyles.ContainsKey(kmlStyle.Id))
                    continue;

                SetKmlStyleDefaults(kmlStyle);                    
                
                var vectorStyle = ApplyStyleOverrides(new VectorStyle(){Enabled =  true}, kmlStyle);
                
                _kmlStyles.Add(kmlStyle.Id, vectorStyle);
            }
        }

        /// <summary>
        /// Parse StyleMaps defined at the head of the KML document and attempt to resolve
        /// both the Normal and Highlight Styles which could include external references 
        /// </summary>
        /// <param name="kml"></param>
        private void ExtractStyleMaps(Element kml)
        {
            _styleMaps = new Dictionary<string, StyleMap>();

            foreach (var style in kml.Flatten().OfType<StyleMapCollection>())
            {
                var styleMap = new StyleMap {Id = style.Id};
                if (_styleMaps.ContainsKey(styleMap.Id)) continue;
                    
                style.ToList().ForEach(x =>
                {
                    if (x.State == null) return;
                    if (x.StyleUrl == null) return;
                    if (!x.StyleUrl.OriginalString.Contains("#")) return;

                    var tokens = x.StyleUrl.OriginalString.Split(new char[]{'#'}, StringSplitOptions.RemoveEmptyEntries);
                    var styleName = tokens.Last();

                    if (tokens.Length == 2)
                        LoadExternalStyle(x.StyleUrl.OriginalString);

                    switch (x.State.Value)
                    {
                        case StyleState.Normal:
                            styleMap.NormalStyleUrl = styleName;
                            break;
                        case StyleState.Highlight:
                            styleMap.HighlightStyleUrl = styleName;
                            break;
                    }
                });
                _styleMaps.Add(styleMap.Id, styleMap);
            }
        }

        /// <summary>
        /// Parse external Style reference   
        /// </summary>
        /// <param name="styleUrl"></param>
        private void LoadExternalStyle(string styleUrl)
        {
            // style defined in external document at specified relative or absolute location
            if (string.IsNullOrWhiteSpace(styleUrl)) return;
            if (styleUrl.StartsWith("#")) return; // internal style
            
            var tokens = styleUrl.Split(new char[]{'#'}, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2) return;

            if (_kmlStyles.ContainsKey(tokens[1])) return;

            // attempt each external file once only
            if (_externalFiles.Contains(tokens[0])) return;

            _externalFiles.Add(tokens[0]);

            if (Uri.IsWellFormedUriString(tokens[0], UriKind.Absolute))
                // eg HTTP: / FTP: / FILE:
                LoadExternalStyles(new Uri(tokens[0]));
            else if (!string.IsNullOrWhiteSpace(_folder))
                // relative FILE reference
                // This does not cater for:
                // 1) highly unlikely situation where "external" style is
                // actually a separate KML entry in the same KMZ archive
                // 2) relative HTTP: reference
                LoadExternalStyles(new Uri(Path.GetFullPath(_folder + tokens[0])));
        }

        /// <summary>
        /// Attempt to append all styles from an external kml/kmz resource  <param name="url"></param> to this KmlProvider's styles
        /// </summary>
        /// <param name="url"></param>
        private void LoadExternalStyles(Uri url)
        {
            try
            {
                var request = WebRequest.Create(url);
                using (var response = request.GetResponse())
                {
                    // not as efficient as reading the styles directly, but simplifies dealing with KML or KMZ.
                    // Also, external style files typically contain styles only (ie no  geometries). 
                    KmlProvider prov = null;
                    if (url.ToString().ToLower().EndsWith(".kml"))
                        prov = FromKml(response.GetResponseStream(), true);
                    else if (url.ToString().ToLower().EndsWith(".kmz"))
                        prov = FromKmz(response.GetResponseStream(), null, true);

                    foreach (var key in prov._kmlStyles.Keys)
                        if (!_kmlStyles.ContainsKey(key))
                            _kmlStyles.Add(key, prov._kmlStyles[key]);
                    
                    foreach (var key in prov._styleMaps.Keys)
                        if (!_styleMaps.ContainsKey(key))
                            _styleMaps.Add(key, prov._styleMaps[key]);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Parse inline styles (ie Styles defined within KML element).  
        /// </summary>
        /// <remarks>
        /// If StyleUrl references an external resource then attempt to load that resource.
        /// If StyleUrl is not defined then ensure default values are set on inline style.
        /// has already been loaded 
        /// </remarks>
        /// <param name="kml"></param>
        private void ValidatePlacemarkStyles(Element kml)
        {
            foreach (var f in kml.Flatten().OfType<Placemark>())
            {
                if (f.StyleUrl != null && !string.IsNullOrWhiteSpace(f.StyleUrl.OriginalString))
                {
                    // StyleUrl
                    if (!f.StyleUrl.OriginalString.StartsWith("#"));
                        LoadExternalStyle(f.StyleUrl.OriginalString);
                }
                else
                {
                    // Possible inline style: ensure KML default styling values are set  
                    var kmlStyle = f.Styles.OfType<Style>().ToList().FirstOrDefault();                    
                    if (kmlStyle != null)
                        SetKmlStyleDefaults(kmlStyle);
                }
            }
        }

        /// <summary>
        /// StyleMap definition
        /// </summary>
        private class StyleMap
        {
            public string Id { get; set; }
            public string NormalStyleUrl { get; set; }
            public string HighlightStyleUrl { get; set; }
        }

        
        /// <summary>
        /// Apply PolyStyle, LineStyle, and/or IconStyle elements of a KML Style to the given VectorStyle.
        /// </summary>
        /// <remarks>
        /// As per KML documentation, in cases where a style element is defined both in a
        /// shared style and in an inline style for a Feature (eg Placemark) the value of
        /// the Feature's inline style takes precedence over the value for the shared style
        /// </remarks>
        /// <param name="vectorStyle">cloned if kmlStyle is not null</param>
        /// <param name="kmlStyle">Style to apply to existing VectorStyle</param>
        private VectorStyle ApplyStyleOverrides(VectorStyle vectorStyle, Style kmlStyle)
        {
            if (kmlStyle == null) return vectorStyle;

            if (kmlStyle.Polygon == null && kmlStyle.Line == null && kmlStyle.Icon == null) return vectorStyle;

            vectorStyle = vectorStyle.Clone();
            
            if (kmlStyle.Polygon != null)
            {
                // fill
                var fill = kmlStyle.Polygon.Fill.GetValueOrDefault(((SolidBrush) vectorStyle.Fill).Color.A > 0);
                var argb = kmlStyle.Polygon.Color?.Argb ?? ((SolidBrush)vectorStyle.Fill).Color.ToArgb();
                
                vectorStyle.Fill =
                    fill
                        ? new SolidBrush(Color.FromArgb(argb))
                        : new SolidBrush(Color.Transparent);

                // outline                
                vectorStyle.EnableOutline = kmlStyle.Polygon.Outline.GetValueOrDefault(vectorStyle.EnableOutline);

                var width = kmlStyle.Line?.Width ?? vectorStyle.Outline.Width;
                argb = kmlStyle.Line?.Color?.Argb ?? vectorStyle.Outline.Color.ToArgb();
                
                vectorStyle.Outline = vectorStyle.EnableOutline
                    ? new Pen(Color.FromArgb(argb), (float) width)
                    : new Pen(Color.Transparent);
            }

            if (kmlStyle.Line != null)
            {
                var width = kmlStyle.Line.Width.GetValueOrDefault(vectorStyle.Line.Width);
                var argb = kmlStyle.Line.Color?.Argb ?? vectorStyle.Line.Color.ToArgb();
                vectorStyle.Line = new Pen(Color.FromArgb(argb), (float) width);
            }

            try
            {
                if (kmlStyle.Icon != null)
                {
                    RasterPointSymbolizer rps = null;
                    
                    if (kmlStyle.Icon.Icon != null && kmlStyle.Icon.Icon.Href != null)
                    {
                        rps = new RasterPointSymbolizer();
                        if (_symbolDict.ContainsKey(kmlStyle.Icon.Icon.Href.ToString()))
                        {
                            rps.Symbol = _symbolDict[kmlStyle.Icon.Icon.Href.ToString()];
                        }
                        else
                        {
                            var newSymbol = GetImageFromUrl(kmlStyle.Icon.Icon.Href);
                            _symbolDict.Add(kmlStyle.Icon.Icon.Href.ToString(), newSymbol);
                            rps.Symbol = newSymbol;
                        }

                        vectorStyle.PointSymbolizer = rps;
                    }

                    if (vectorStyle.PointSymbolizer is RasterPointSymbolizer)
                    {
                        rps =(RasterPointSymbolizer) vectorStyle.PointSymbolizer;
                        
                        var argb = kmlStyle.Icon.Color?.Argb ?? ((SolidBrush)vectorStyle.PointColor).Color.ToArgb();
                        rps.SymbolColor = Color.FromArgb(argb);
                        rps.RemapColor = Color.White;
                        rps.Scale = (float)kmlStyle.Icon.Scale.GetValueOrDefault(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return vectorStyle;
        }

        /// <summary>
        /// Convert KML geometry to NTS geometry     
        /// </summary>
        /// <param name="kml"></param>
        public void ExtractGeometries(Element kml)
        {
            _geometrys = new Dictionary<Placemark, List<IGeometry>>();

            //todo handle other geom types such as gxTrack and gxMutliTrack
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

//            foreach (var f in kml.Flatten().OfType<LinearRing>())
//            {
//                ProcessLinearRingGeometry(f);
//            }

            foreach (var f in kml.Flatten().OfType<MultipleGeometry>())
            {
                ProcessMultipleGeometry(f);
            }
        }

        private void ProcessMultipleGeometry(MultipleGeometry f)
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
//                if (g is LinearRing)
//                {
//                    ProcessLinearRingGeometry((LinearRing) g);
//                }
                if (g is MultipleGeometry)
                {
                    ProcessMultipleGeometry((MultipleGeometry)g);
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
            IGeometry pGeom;
            if (f.Coordinates.Count == 1)
            {
                var coord = f.Coordinates.First();
                var coords = new Coordinate(coord.Longitude, coord.Latitude);

                pGeom = _geometryFactory.CreatePoint(coords);
            }
            else
            {
                pGeom = _geometryFactory.CreateLineString(
                        f.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray());
            }
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        private void ProcessPointGeometry(Point f)
        {
            var coords = new Coordinate(f.Coordinate.Longitude, f.Coordinate.Latitude);

            var pGeom = _geometryFactory.CreatePoint(coords);
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        [Obsolete("ProcessPolygonGeometry correctly handles exterior and interior LinearRings")]
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

        void IDisposable.Dispose()
        {
            // throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the features within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
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

        /// <summary>
        /// Returns all objects whose <see cref="GeoAPI.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="GeoAPI.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public IGeometry GetGeometryByID(uint oid)
        {
            var sid = oid.ToString(NumberFormatInfo.InvariantInfo);
            var tmp = _geometrys.FirstOrDefault(x => x.Key.Id == sid);
            
            return tmp.Value != null ?
                _geometryFactory.BuildGeometry(tmp.Value) : null;
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
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

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(_geometryFactory.ToGeometry(box), ds);
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return _geometrys.Count;
        }

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="oid">The id of the row.</param>
        /// <returns>datarow</returns>
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

        /// <summary>
        /// <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>The 2d extent of the layer</returns>
        public Envelope GetExtents()
        {
            var retEnv = new Envelope();

            _geometrys.Values.ToList().ForEach(x => x.ForEach(v => retEnv.ExpandToInclude(v.EnvelopeInternal)));
            return retEnv;
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            IsOpen = false;
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        public string ConnectionID { get; private set; }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID { get { return 4326; } set { }}

        #region private helper methods

        private static string GetFileStreamPath(Stream stream) => stream is FileStream fs ? fs.Name : null;

        private static void AddColumnsToFeatureDataTable(FeatureDataTable fdt)
        {
            if (!fdt.Columns.Contains("Id"))
                fdt.Columns.Add("Id", typeof(string));

            if (!fdt.Columns.Contains("StyleUrl"))
                fdt.Columns.Add("StyleUrl", typeof(string));

            if (!fdt.Columns.Contains("Object"))
                fdt.Columns.Add("Object", typeof(Placemark));
        }

        private static object[] GetAssetProperties(Feature f)
        {
            var styleUrl = ""; 
            if (f.StyleUrl != null)
            {
                var tokens = f.StyleUrl.ToString().Split(new char[]{'#'}, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 0) styleUrl = tokens.Last();
            }
           
            return new object[]
            {
                f.Id,
                styleUrl,
                f
            };
        }

        /// <summary>
        /// Ensure default styling values are set
        /// </summary>
        /// <remarks>
        /// SharpKml uses nullable types and does not set recognised KML defaults. 
        /// This helper methoed ensures  Styles are fully defined prior to storing
        /// in internal dictionary, thus simplifying subsequent VectorStyle generation. 
        /// </remarks>
        /// <param name="style">The Style to check defaults</param>
        private static void SetKmlStyleDefaults(Style style)
        {
            if (style.Polygon != null)
            {
                style.Polygon.Fill = style.Polygon.Fill.GetValueOrDefault(PolygonStyle.DefaultFill);
                style.Polygon.Outline = style.Polygon.Outline.GetValueOrDefault(PolygonStyle.DefaultOutline);
                style.Polygon.Color = style.Polygon.Color.GetValueOrDefault(ColorStyle.DefaultColor);
            }
            
            if (style.Line != null)
            {
                style.Line.Width = style.Line.Width.GetValueOrDefault(LineStyle.DefaultWidth);
                style.Line.Color = style.Line.Color.GetValueOrDefault(ColorStyle.DefaultColor);
            }

            if (style.Icon != null)
            {
                if (style.Icon.Icon == null) style.Icon.Icon = new IconStyle.IconLink(Pushpins.YellowPushpin); 
                style.Icon.Scale = style.Icon.Scale.GetValueOrDefault(IconStyle.DefaultScale);
                style.Icon.Color = style.Icon.Color.GetValueOrDefault(ColorStyle.DefaultColor);
                style.Icon.Heading = style.Icon.Heading.GetValueOrDefault(0);
            }
        }

        private static VectorStyle DefaultPointStyle()
        {
            var vectorStyle = new VectorStyle();
            vectorStyle.Enabled = true;
            try
            {
                var defaultIcon = GetImageFromUrl(Pushpins.RedPushpin); 
                vectorStyle.PointSymbolizer = new RasterPointSymbolizer {Symbol = defaultIcon, Scale = 1f};
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
