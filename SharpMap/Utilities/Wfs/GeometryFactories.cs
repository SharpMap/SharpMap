// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml;
using SharpMap.Data;
using GeoAPI.Geometries;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class is the base class for geometry production.
    /// It provides some parsing routines for XML compliant to GML2/GML3.
    /// </summary>
    internal abstract class GeometryFactory : IDisposable
    {
        #region Fields

        protected const string _GMLNS = "http://www.opengis.net/gml";
        private readonly NumberFormatInfo _formatInfo = new NumberFormatInfo();
        private readonly HttpClientUtil _httpClientUtil;
        private readonly List<IPathNode> _pathNodes = new List<IPathNode>();
        private int[] _axisOrder;
        protected AlternativePathNodesCollection _CoordinatesNode;
        private string _Cs;
        protected IPathNode _FeatureNode;
        protected XmlReader _FeatureReader;
        protected WfsFeatureTypeInfo _FeatureTypeInfo;
        protected XmlReader _GeomReader;

        protected Collection<IGeometry> _Geoms = new Collection<IGeometry>();

        protected FeatureDataTable _LabelInfo;
        protected IPathNode _LabelNode;
        protected AlternativePathNodesCollection _ServiceExceptionNode;
        private string _Ts;
        protected XmlReader _XmlReader;
        #endregion

        #region Constructors

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        protected GeometryFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                  FeatureDataTable labelInfo)
        {
            _FeatureTypeInfo = featureTypeInfo;
            Factory = featureTypeInfo.Factory;
            _httpClientUtil = httpClientUtil;
            createReader(httpClientUtil);

            try
            {
                if (labelInfo != null)
                {
                    _LabelInfo = labelInfo;
                    var pathNodes = new IPathNode[labelInfo.Columns.Count];
                    for (var i = 0; i < pathNodes.Length; i++)
                    {
                        pathNodes[i] = new PathNode(_FeatureTypeInfo.FeatureTypeNamespace, _LabelInfo.Columns[i].ColumnName, (NameTable)_XmlReader.NameTable);
                    }
                    _LabelNode = new AlternativePathNodesCollection(pathNodes);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while initializing the label path node!");
                throw ex;
            }

            initializePathNodes();
            initializeSeparators();
        }

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        protected GeometryFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        {
            _FeatureTypeInfo = featureTypeInfo;
            Factory = featureTypeInfo.Factory;
            _XmlReader = xmlReader;
            initializePathNodes();
            initializeSeparators();
        }

        #endregion

        internal IGeometryFactory Factory { get; set; }

        /// <summary>
        /// Gets or sets the axis order
        /// </summary>
        internal int[] AxisOrder
        {
            get { return _axisOrder; }
            set { _axisOrder = value; }
        }

        #region Internal Member



        /// <summary>
        /// Abstract method - overwritten by derived classes for producing instances
        /// derived from <see cref="GeoAPI.Geometries.IGeometry"/>.
        /// </summary>
        internal abstract Collection<IGeometry> createGeometries();

        /// <summary>
        /// This method parses quickly without paying attention to
        /// context validation, polygon boundaries and multi-geometries.
        /// This accelerates the geometry parsing process, 
        /// but in scarce cases can lead to errors. 
        /// </summary>
        /// <param name="geometryType">The geometry type (Point, LineString, Polygon, MultiPoint, MultiCurve, 
        /// MultiLineString (deprecated), MultiSurface, MultiPolygon (deprecated)</param>
        /// <returns>The created geometries</returns>
        internal virtual Collection<IGeometry> createQuickGeometries(string geometryType)
        {
            // Ignore multi-geometries
            if (geometryType.Equals("MultiPointPropertyType")) geometryType = "PointPropertyType";
            else if (geometryType.Equals("MultiLineStringPropertyType")) geometryType = "LineStringPropertyType";
            else if (geometryType.Equals("MultiPolygonPropertyType")) geometryType = "PolygonPropertyType";
            else if (geometryType.Equals("MultiCurvePropertyType")) geometryType = "CurvePropertyType";
            else if (geometryType.Equals("MultiSurfacePropertyType")) geometryType = "SurfacePropertyType";

            string serviceException = null;

            while (_XmlReader.Read())
            {
                if (_CoordinatesNode.Matches(_XmlReader))
                {
                    try
                    {
                        switch (geometryType)
                        {
                            case "PointPropertyType":
                                _Geoms.Add(Factory.CreatePoint(ParseCoordinates(_XmlReader.ReadSubtree())[0]));
                                break;
                            case "LineStringPropertyType":
                            case "CurvePropertyType":
                                _Geoms.Add(Factory.CreateLineString(ParseCoordinates(_XmlReader.ReadSubtree())));
                                break;
                            case "PolygonPropertyType":
                            case "SurfacePropertyType":
                                _Geoms.Add(Factory.CreatePolygon(
                                    Factory.CreateLinearRing(ParseCoordinates(_XmlReader.ReadSubtree())), null));
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("An exception occured while parsing a " + geometryType + " geometry: " +
                                         ex.Message);
                        throw ex;
                    }
                    continue;
                }

                if (_ServiceExceptionNode.Matches(_XmlReader))
                {
                    serviceException = _XmlReader.ReadInnerXml();
                    Trace.TraceError("A service exception occured: " + serviceException);
                    throw new Exception("A service exception occured: " + serviceException);
                }
            }

            return _Geoms;
        }

        #endregion

        #region Protected Member

        /// <summary>
        /// This method parses a coordinates or posList(from 'GetFeature' response). 
        /// </summary>
        /// <param name="reader">An XmlReader instance at the position of the coordinates to read</param>
        /// <returns>A point collection (the collected coordinates)</returns>
        protected Coordinate[] ParseCoordinates(XmlReader reader)
        {
            if (!reader.Read()) return null;

            string name = reader.LocalName;
            string coordinateString = reader.ReadElementString();
            var vertices = new List<Coordinate>();
            string[][] coordinateValues;
            int i = 0, length = 0;

            if (name.Equals("coordinates"))
            {
                var coords = coordinateString.Split(_Ts[0]);
                coordinateValues = coords.Select(s => s.Split(_Cs[0])).ToArray();
            }
            else
            {
                // we assume there are only x,y pairs
                var coords = coordinateString.Split(' ');
                var odds = coords.Where((s, idx) => idx%2 == 0);
                var evens = coords.Where((s, idx) => idx%2 != 0);
                coordinateValues = (from o in odds
                    from e in evens
                    select new [] {o, e}).ToArray();
            }
            length = coordinateValues.Length;

            while (i < length)
            {
                var c = new double[2];
                var values = coordinateValues[i++];
                c[_axisOrder[0]] = Convert.ToDouble(values[0], _formatInfo);
                c[_axisOrder[1]] = Convert.ToDouble(values[1], _formatInfo);

                var coordinate = values.Length > 2
                    ? new Coordinate(c[0], c[1], Convert.ToDouble(values[2]))
                    : new Coordinate(c[0], c[1]);
                    
                vertices.Add(coordinate);
            }

            return vertices.ToArray();
        }

        /// <summary>
        /// This method retrieves an XmlReader within a specified context.
        /// </summary>
        /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
        /// <param name="labels">A dictionary for recording label values. Pass 'null' to ignore searching for label values</param>
        /// <param name="pathNodes">A list of <see cref="IPathNode"/> instances defining the context of the retrieved reader</param>
        /// <returns>A sub-reader of the XmlReader given as argument</returns>
        protected XmlReader GetSubReaderOf(XmlReader reader, Dictionary<string, string> labels, params IPathNode[] pathNodes)
        {
            _pathNodes.Clear();
            _pathNodes.AddRange(pathNodes);
            return GetSubReaderOf(reader, labels, _pathNodes);
        }

        /// <summary>
        /// This method retrieves an XmlReader within a specified context.
        /// Moreover it collects label values before or after a geometry could be found.
        /// </summary>
        /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
        /// <param name="labels">A dictionary for recording label values. Pass 'null' to ignore searching for label values</param>
        /// <param name="pathNodes">A list of <see cref="IPathNode"/> instances defining the context of the retrieved reader</param>
        /// <returns>A sub-reader of the XmlReader given as argument</returns>
        protected XmlReader GetSubReaderOf(XmlReader reader, Dictionary<string, string> labels, List<IPathNode> pathNodes)
        {
            string errorMessage = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (pathNodes[0].Matches(reader))
                    {
                        pathNodes.RemoveAt(0);

                        if (pathNodes.Count > 0)
                            return GetSubReaderOf(reader.ReadSubtree(), null, pathNodes);

                        return reader.ReadSubtree();
                    }

                    if (labels != null)
                        if (_LabelNode != null)
                            if (_LabelNode.Matches(reader))
                            {
                                var labelName = reader.Name;
                                var labelValue = reader.ReadString();

                                // remove the namespace
                                if (labelName.Contains(":"))
                                    labelName = labelName.Split(':')[1];

                                labels.Add(labelName, labelValue);
                            }


                    if (_ServiceExceptionNode.Matches(reader))
                    {
                        errorMessage = reader.ReadInnerXml();
                        Trace.TraceError("A service exception occured: " + errorMessage);
                        throw new Exception("A service exception occured: " + errorMessage);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// This method adds labels to the collection.
        /// </summary>
        protected void AddLabel(Dictionary<string, string> labelValues, IGeometry geom)
        {
            if (_LabelInfo == null || geom == null || labelValues == null) return;

            try
            {
                FeatureDataRow row = _LabelInfo.NewRow();
                foreach (var keyPair in labelValues)
                {
                    var labelName = keyPair.Key;
                    var labelValue = keyPair.Value;

                    row[labelName] = labelValue;
                }
                
                row.Geometry = geom;
                _LabelInfo.AddRow(row);

                labelValues.Clear();
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while adding a label to the collection!");
                throw ex;
            }
        }

        #endregion

        #region Private Member

        /// <summary>
        /// This method initializes the XmlReader member.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        private void createReader(HttpClientUtil httpClientUtil)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            //xmlReaderSettings.ProhibitDtd = true;
            xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
            _XmlReader = XmlReader.Create(httpClientUtil.GetDataStream(), xmlReaderSettings);
        }

        /// <summary>
        /// This method initializes path nodes needed by the derived classes.
        /// </summary>
        private void initializePathNodes()
        {
            IPathNode coordinatesNode = new PathNode("http://www.opengis.net/gml", "coordinates",
                                                     (NameTable) _XmlReader.NameTable);
            IPathNode posListNode = new PathNode("http://www.opengis.net/gml", "posList",
                                                 (NameTable) _XmlReader.NameTable);
            IPathNode posNode = new PathNode("http://www.opengis.net/gml", "pos",
                                                 (NameTable)_XmlReader.NameTable);
            IPathNode ogcServiceExceptionNode = new PathNode("http://www.opengis.net/ogc", "ServiceException",
                                                             (NameTable) _XmlReader.NameTable);
            IPathNode serviceExceptionNode = new PathNode("", "ServiceException", (NameTable) _XmlReader.NameTable);
                //ServiceExceptions without ogc prefix are returned by deegree. PDD.
            IPathNode exceptionTextNode = new PathNode("http://www.opengis.net/ows", "ExceptionText",
                                                       (NameTable) _XmlReader.NameTable);
            _CoordinatesNode = new AlternativePathNodesCollection(coordinatesNode, posListNode, posNode);
            _ServiceExceptionNode = new AlternativePathNodesCollection(ogcServiceExceptionNode, exceptionTextNode,
                                                                       serviceExceptionNode);
            _FeatureNode = new PathNode(_FeatureTypeInfo.FeatureTypeNamespace, _FeatureTypeInfo.Name,
                                        (NameTable) _XmlReader.NameTable);
        }

        /// <summary>
        /// This method initializes separator variables for parsing coordinates.
        /// From GML specification: Coordinates can be included in a single string, but there is no 
        /// facility for validating string content. The value of the 'cs' attribute 
        /// is the separator for coordinate values, and the value of the 'ts' 
        /// attribute gives the tuple separator (a single space by default); the 
        /// default values may be changed to reflect local usage.
        /// </summary>
        private void initializeSeparators()
        {
            string decimalDel = string.IsNullOrEmpty(_FeatureTypeInfo.DecimalDel) ? ":" : _FeatureTypeInfo.DecimalDel;
            _Cs = string.IsNullOrEmpty(_FeatureTypeInfo.Cs) ? "," : _FeatureTypeInfo.Cs;
            _Ts = string.IsNullOrEmpty(_FeatureTypeInfo.Ts) ? " " : _FeatureTypeInfo.Ts;
            _formatInfo.NumberDecimalSeparator = decimalDel;
        }

        #endregion

        #region IDisposable Member

        /// <summary>
        /// This method closes the XmlReader member and the used <see cref="HttpClientUtil"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (_XmlReader != null)
                _XmlReader.Close();
            if (_httpClientUtil != null)
                _httpClientUtil.Close();
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="GeoAPI.Geometries.IPoint"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class PointFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PointFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal PointFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo, FeatureDataTable labelInfo) 
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointFactory"/> class.
        /// This constructor shall just be called from the MultiPoint factory. The feature node therefore is deactivated.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal PointFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
            : base(xmlReader, featureTypeInfo)
        {
            _FeatureNode.IsActive = false;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="GeoAPI.Geometries.IPoint"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<IGeometry> createGeometries()
        {
            IPathNode pointNode = new PathNode(_GMLNS, "Point", (NameTable) _XmlReader.NameTable);
            var labelValues = new Dictionary<string, string>();
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while ((_GeomReader = GetSubReaderOf(_FeatureReader, labelValues, pointNode, _CoordinatesNode)) !=
                           null)
                    {
                        _Geoms.Add(Factory.CreatePoint(ParseCoordinates(_GeomReader)[0]));
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValues, _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a point geometry string: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="GeoAPI.Geometries.ILineString"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class LineStringFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LineStringFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal LineStringFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo, FeatureDataTable labelInfo)
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineStringFactory"/> class.
        /// This constructor shall just be called from the MultiLineString factory. The feature node therefore is deactivated.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal LineStringFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
            : base(xmlReader, featureTypeInfo)
        {
            _FeatureNode.IsActive = false;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="GeoAPI.Geometries.ILineString"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<IGeometry> createGeometries()
        {
            IPathNode lineStringNode = new PathNode(_GMLNS, "LineString", (NameTable) _XmlReader.NameTable);
            var labelValues = new Dictionary<string, string>();
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader = GetSubReaderOf(_FeatureReader, labelValues, lineStringNode, _CoordinatesNode)) !=
                        null)
                    {
                        _Geoms.Add(Factory.CreateLineString(ParseCoordinates(_GeomReader)));
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValues, _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a line geometry string: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="GeoAPI.Geometries.IPolygon"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class PolygonFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal PolygonFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo, FeatureDataTable labelInfo)
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonFactory"/> class.
        /// This constructor shall just be called from the MultiPolygon factory. The feature node therefore is deactivated.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal PolygonFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
            : base(xmlReader, featureTypeInfo)
        {
            _FeatureNode.IsActive = false;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="GeoAPI.Geometries.IPolygon"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<IGeometry> createGeometries()
        {
            XmlReader outerBoundaryReader = null;
            XmlReader innerBoundariesReader = null;

            IPathNode polygonNode = new PathNode(_GMLNS, "Polygon", (NameTable) _XmlReader.NameTable);
            IPathNode outerBoundaryNode = new PathNode(_GMLNS, "outerBoundaryIs", (NameTable) _XmlReader.NameTable);
            IPathNode exteriorNode = new PathNode(_GMLNS, "exterior", (NameTable) _XmlReader.NameTable);
            IPathNode outerBoundaryNodeAlt = new AlternativePathNodesCollection(outerBoundaryNode, exteriorNode);
            IPathNode innerBoundaryNode = new PathNode(_GMLNS, "innerBoundaryIs", (NameTable) _XmlReader.NameTable);
            IPathNode interiorNode = new PathNode(_GMLNS, "interior", (NameTable) _XmlReader.NameTable);
            IPathNode innerBoundaryNodeAlt = new AlternativePathNodesCollection(innerBoundaryNode, interiorNode);
            IPathNode linearRingNode = new PathNode(_GMLNS, "LinearRing", (NameTable) _XmlReader.NameTable);
            var labelValues = new Dictionary<string, string>();
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    ILinearRing shell = null;
                    var holes = new List<ILinearRing>();
                    while ((_GeomReader = GetSubReaderOf(_FeatureReader, labelValues, polygonNode)) != null)
                    {
                        //polygon = new Polygon();

                        if (
                            (outerBoundaryReader =
                             GetSubReaderOf(_GeomReader, null, outerBoundaryNodeAlt, linearRingNode, _CoordinatesNode)) !=
                            null)
                            shell = Factory.CreateLinearRing(ParseCoordinates(outerBoundaryReader));

                        while (
                            (innerBoundariesReader =
                             GetSubReaderOf(_GeomReader, null, innerBoundaryNodeAlt, linearRingNode, _CoordinatesNode)) !=
                            null)
                            holes.Add(Factory.CreateLinearRing(ParseCoordinates(innerBoundariesReader)));

                        _Geoms.Add(Factory.CreatePolygon(shell, holes.ToArray()));
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValues, _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a polygon geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="GeoAPI.Geometries.IMultiPoint"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class MultiPointFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPointFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal MultiPointFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo, FeatureDataTable labelInfo) 
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPointFactory"/> class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal MultiPointFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
            : base(xmlReader, featureTypeInfo)
        {
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="GeoAPI.Geometries.IMultiPoint"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<IGeometry> createGeometries()
        {
            IPathNode multiPointNode = new PathNode(_GMLNS, "MultiPoint", (NameTable) _XmlReader.NameTable);
            IPathNode pointMemberNode = new PathNode(_GMLNS, "pointMember", (NameTable) _XmlReader.NameTable);
            var labelValues = new Dictionary<string, string>();
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader = GetSubReaderOf(_FeatureReader, labelValues, multiPointNode, pointMemberNode)) !=
                        null)
                    {
                        GeometryFactory geomFactory = new PointFactory(_GeomReader, _FeatureTypeInfo) { AxisOrder = AxisOrder};
                        var points = geomFactory.createGeometries();

                        var pointArray = new IPoint[points.Count];
                        var i = 0;
                        foreach (IPoint point in points)
                            pointArray[i] = point;

                        _Geoms.Add(Factory.CreateMultiPoint(pointArray));
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValues, _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-point geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces objects of type <see cref="GeoAPI.Geometries.IMultiLineString"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class MultiLineStringFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineStringFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal MultiLineStringFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                        FeatureDataTable labelInfo) 
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineStringFactory"/> class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal MultiLineStringFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
            : base(xmlReader, featureTypeInfo)
        {
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="GeoAPI.Geometries.IMultiLineString"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<IGeometry> createGeometries()
        {
            IPathNode multiLineStringNode = new PathNode(_GMLNS, "MultiLineString", (NameTable) _XmlReader.NameTable);
            IPathNode multiCurveNode = new PathNode(_GMLNS, "MultiCurve", (NameTable) _XmlReader.NameTable);
            IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
            IPathNode lineStringMemberNode = new PathNode(_GMLNS, "lineStringMember", (NameTable) _XmlReader.NameTable);
            IPathNode curveMemberNode = new PathNode(_GMLNS, "curveMember", (NameTable) _XmlReader.NameTable);
            IPathNode lineStringMemberNodeAlt = new AlternativePathNodesCollection(lineStringMemberNode, curveMemberNode);
            var labelValues = new Dictionary<string, string>();
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader =
                         GetSubReaderOf(_FeatureReader, labelValues, multiLineStringNodeAlt, lineStringMemberNodeAlt)) !=
                        null)
                    {
                        GeometryFactory geomFactory = new LineStringFactory(_GeomReader, _FeatureTypeInfo) { AxisOrder = AxisOrder };
                        Collection<IGeometry> lineStrings = geomFactory.createGeometries();

                        var lineStringArray = new ILineString[lineStrings.Count];
                        var i = 0;
                        foreach (ILineString lineString in lineStrings)
                            lineStringArray[i++] = lineString;

                        _Geoms.Add(Factory.CreateMultiLineString(lineStringArray));
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValues, _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-lineString geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="GeoAPI.Geometries.IMultiPolygon"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class MultiPolygonFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPolygonFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal MultiPolygonFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo, FeatureDataTable labelInfo) 
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPolygonFactory"/> class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal MultiPolygonFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
            : base(xmlReader, featureTypeInfo)
        {
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="GeoAPI.Geometries.IMultiPolygon"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<IGeometry> createGeometries()
        {
            //IMultiPolygon multiPolygon = null;

            IPathNode multiPolygonNode = new PathNode(_GMLNS, "MultiPolygon", (NameTable) _XmlReader.NameTable);
            IPathNode multiSurfaceNode = new PathNode(_GMLNS, "MultiSurface", (NameTable) _XmlReader.NameTable);
            IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);
            IPathNode polygonMemberNode = new PathNode(_GMLNS, "polygonMember", (NameTable) _XmlReader.NameTable);
            IPathNode surfaceMemberNode = new PathNode(_GMLNS, "surfaceMember", (NameTable) _XmlReader.NameTable);
            IPathNode polygonMemberNodeAlt = new AlternativePathNodesCollection(polygonMemberNode, surfaceMemberNode);
            IPathNode linearRingNode = new PathNode(_GMLNS, "LinearRing", (NameTable) _XmlReader.NameTable);
            var labelValues = new Dictionary<string, string>();
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader =
                         GetSubReaderOf(_FeatureReader, labelValues, multiPolygonNodeAlt)) != null)
                    {
                        XmlReader memberReader;
                        var polygons = new List<IGeometry>();
                        while ((memberReader = GetSubReaderOf(_GeomReader, labelValues, polygonMemberNodeAlt)) != null)
                        {
                            GeometryFactory geomFactory = new PolygonFactory(memberReader, _FeatureTypeInfo) { AxisOrder = AxisOrder };
                            var polygon = geomFactory.createGeometries()[0]; // polygon element has a maxOccurs=1
                            
                            polygons.Add(polygon);

                            geomFound = true;
                        }

                        if (geomFound)
                        {
                            var polygonArray = new IPolygon[polygons.Count];
                            var i = 0;
                            foreach (IPolygon polygon in polygons)
                                polygonArray[i++] = polygon;

                            _Geoms.Add(Factory.CreateMultiPolygon(polygonArray));
                        }
                        
                    }
                    if (geomFound) AddLabel(labelValues, _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-polygon geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class must detect the geometry type of the queried layer.
    /// Therefore it works a bit slower than the other factories. Specify the geometry type manually,
    /// if it isn't specified in 'DescribeFeatureType'.
    /// </summary>
    internal class UnspecifiedGeometryFactory_WFS1_0_0_GML2 : GeometryFactory
    {
        #region Fields

        private readonly HttpClientUtil _HttpClientUtil;
        private readonly bool _QuickGeometries;
        private bool _MultiGeometries;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnspecifiedGeometryFactory_WFS1_0_0_GML2"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="multiGeometries">A boolean value specifying whether multi-geometries should be created</param>
        /// <param name="quickGeometries">A boolean value specifying whether the factory should create geometries quickly, but without validation</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal UnspecifiedGeometryFactory_WFS1_0_0_GML2(HttpClientUtil httpClientUtil,
                                                          WfsFeatureTypeInfo featureTypeInfo, bool multiGeometries,
                                                          bool quickGeometries, FeatureDataTable labelInfo)
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
            _HttpClientUtil = httpClientUtil;
            _MultiGeometries = multiGeometries;
            _QuickGeometries = quickGeometries;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method detects the geometry type from 'GetFeature' response and uses a geometry factory to create the 
        /// appropriate geometries.
        /// </summary>
        /// <returns></returns>
        internal override Collection<IGeometry> createGeometries()
        {
            GeometryFactory geomFactory = null;

            string geometryTypeString = string.Empty;
            string serviceException = null;

            if (_QuickGeometries) _MultiGeometries = false;

            IPathNode pointNode = new PathNode(_GMLNS, "Point", (NameTable) _XmlReader.NameTable);
            IPathNode lineStringNode = new PathNode(_GMLNS, "LineString", (NameTable) _XmlReader.NameTable);
            IPathNode polygonNode = new PathNode(_GMLNS, "Polygon", (NameTable) _XmlReader.NameTable);
            IPathNode multiPointNode = new PathNode(_GMLNS, "MultiPoint", (NameTable) _XmlReader.NameTable);
            IPathNode multiLineStringNode = new PathNode(_GMLNS, "MultiLineString", (NameTable) _XmlReader.NameTable);
            IPathNode multiCurveNode = new PathNode(_GMLNS, "MultiCurve", (NameTable) _XmlReader.NameTable);
            IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
            IPathNode multiPolygonNode = new PathNode(_GMLNS, "MultiPolygon", (NameTable) _XmlReader.NameTable);
            IPathNode multiSurfaceNode = new PathNode(_GMLNS, "MultiSurface", (NameTable) _XmlReader.NameTable);
            IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);

            while (_XmlReader.Read())
            {
                if (_XmlReader.NodeType == XmlNodeType.Element)
                {
                    if (_MultiGeometries)
                    {
                        if (multiPointNode.Matches(_XmlReader))
                        {
                            geomFactory = new MultiPointFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                            geometryTypeString = "MultiPointPropertyType";
                            break;
                        }
                        if (multiLineStringNodeAlt.Matches(_XmlReader))
                        {
                            geomFactory = new MultiLineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                            geometryTypeString = "MultiLineStringPropertyType";
                            break;
                        }
                        if (multiPolygonNodeAlt.Matches(_XmlReader))
                        {
                            geomFactory = new MultiPolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                            geometryTypeString = "MultiPolygonPropertyType";
                            break;
                        }
                    }

                    if (pointNode.Matches(_XmlReader))
                    {
                        geomFactory = new PointFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        geometryTypeString = "PointPropertyType";
                        break;
                    }
                    if (lineStringNode.Matches(_XmlReader))
                    {
                        geomFactory = new LineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        geometryTypeString = "LineStringPropertyType";
                        break;
                    }
                    if (polygonNode.Matches(_XmlReader))
                    {
                        geomFactory = new PolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        geometryTypeString = "PolygonPropertyType";
                        break;
                    }
                    if (_ServiceExceptionNode.Matches(_XmlReader))
                    {
                        serviceException = _XmlReader.ReadInnerXml();
                        Trace.TraceError("A service exception occured: " + serviceException);
                        throw new Exception("A service exception occured: " + serviceException);
                    }
                }
            }

            _FeatureTypeInfo.Geometry._GeometryType = geometryTypeString;
            if (geomFactory != null)
            {
                geomFactory.AxisOrder = AxisOrder;

                var res = _QuickGeometries
                    ? geomFactory.createQuickGeometries(geometryTypeString)
                    : geomFactory.createGeometries();
                
                geomFactory.Dispose();
                return res;
            }


            return _Geoms;
        }

        #endregion
    }
}