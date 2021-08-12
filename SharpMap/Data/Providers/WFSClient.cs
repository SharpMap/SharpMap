// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml.XPath;
using SharpMap.CoordinateSystems;
using SharpMap.Utilities.Indexing;
using SharpMap.Utilities.SpatialIndexing;
using SharpMap.Utilities.Wfs;
using NetTopologySuite.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// WFS dataprovider
    /// This provider can be used to obtain data from an OGC Web Feature Service.
    /// It performs the following requests: 'GetCapabilities', 'DescribeFeatureType' and 'GetFeature'.
    /// This class is optimized for performing requests to GeoServer (http://geoserver.org).
    /// Supported geometries are:
    /// - PointPropertyType
    /// - LineStringPropertyType
    /// - PolygonPropertyType
    /// - CurvePropertyType
    /// - SurfacePropertyType
    /// - MultiPointPropertyType
    /// - MultiLineStringPropertyType
    /// - MultiPolygonPropertyType
    /// - MultiCurvePropertyType
    /// - MultiSurfacePropertyType
    /// </summary>
    /// <example>
    /// <code lang="C#">
    ///SharpMap.Map demoMap;
    ///
    ///const string getCapabilitiesURI = "http://localhost:8080/geoserver/wfs";
    ///const string serviceURI = "http://localhost:8080/geoserver/wfs";
    ///
    ///demoMap = new SharpMap.Map(new Size(600, 600));
    ///demoMap.MinimumZoom = 0.005;
    ///demoMap.BackColor = Color.White;
    ///
    ///SharpMap.Layers.VectorLayer layer1 = new SharpMap.Layers.VectorLayer("States");
    ///SharpMap.Layers.VectorLayer layer2 = new SharpMap.Layers.VectorLayer("SelectedStatesAndHousholds");
    ///SharpMap.Layers.VectorLayer layer3 = new SharpMap.Layers.VectorLayer("New Jersey");
    ///SharpMap.Layers.VectorLayer layer4 = new SharpMap.Layers.VectorLayer("Roads");
    ///SharpMap.Layers.VectorLayer layer5 = new SharpMap.Layers.VectorLayer("Landmarks");
    ///SharpMap.Layers.VectorLayer layer6 = new SharpMap.Layers.VectorLayer("Poi");
    ///    
    /// // Demo data from Geoserver 1.5.3 and Geoserver 1.6.0 
    ///    
    ///WFS prov1 = new WFS(getCapabilitiesURI, "topp", "states", WFS.WFSVersionEnum.WFS1_0_0);
    ///    
    /// // Bypass 'GetCapabilities' and 'DescribeFeatureType', if you know all necessary metadata.
    ///WfsFeatureTypeInfo featureTypeInfo = new WfsFeatureTypeInfo(serviceURI, "topp", null, "states", "the_geom");
    /// // 'WFS.WFSVersionEnum.WFS1_1_0' supported by Geoserver 1.6.x
    ///WFS prov2 = new SharpMap.Data.Providers.WFS(featureTypeInfo, WFS.WFSVersionEnum.WFS1_1_0);
    /// // Bypass 'GetCapabilities' and 'DescribeFeatureType' again...
    /// // It's possible to specify the geometry type, if 'DescribeFeatureType' does not...(.e.g 'GeometryAssociationType')
    /// // This helps to accelerate the initialization process in case of unprecise geometry information.
    ///WFS prov3 = new WFS(serviceURI, "topp", "http://www.openplans.org/topp", "states", "the_geom", GeometryTypeEnum.MultiSurfacePropertyType, WFS.WFSVersionEnum.WFS1_1_0);
    ///
    /// // Get data-filled FeatureTypeInfo after initialization of dataprovider (useful in Web Applications for caching metadata.
    ///WfsFeatureTypeInfo info = prov1.FeatureTypeInfo;
    ///
    /// // Use cached 'GetCapabilities' response of prov1 (featuretype hosted by same service).
    /// // Compiled XPath expressions are re-used automatically!
    /// // If you use a cached 'GetCapabilities' response make sure the data provider uses the same version of WFS as the one providing the cache!!!
    ///WFS prov4 = new WFS(prov1.GetCapabilitiesCache, "tiger", "tiger_roads", WFS.WFSVersionEnum.WFS1_0_0);
    ///WFS prov5 = new WFS(prov1.GetCapabilitiesCache, "tiger", "poly_landmarks", WFS.WFSVersionEnum.WFS1_0_0);
    ///WFS prov6 = new WFS(prov1.GetCapabilitiesCache, "tiger", "poi", WFS.WFSVersionEnum.WFS1_0_0);
    /// // Clear cache of prov1 - data providers do not have any cache, if they use the one of another data provider  
    ///prov1.GetCapabilitiesCache = null;
    ///
    /// //Filters
    ///IFilter filter1 = new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "California");
    ///IFilter filter2 = new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "Vermont");
    ///IFilter filter3 = new PropertyIsBetweenFilter_FE1_1_0("HOUSHOLD", "600000", "4000000");
    ///IFilter filter4 = new PropertyIsLikeFilter_FE1_1_0("STATE_NAME", "New*");
    ///
    /// // SelectedStatesAndHousholds: Green
    ///OGCFilterCollection filterCollection1 = new OGCFilterCollection();
    ///filterCollection1.AddFilter(filter1);
    ///filterCollection1.AddFilter(filter2);
    ///OGCFilterCollection filterCollection2 = new OGCFilterCollection();
    ///filterCollection2.AddFilter(filter3);
    ///filterCollection1.AddFilterCollection(filterCollection2);
    ///filterCollection1.Junctor = OGCFilterCollection.JunctorEnum.Or;
    ///prov2.OGCFilter = filterCollection1;
    ///
    /// // Like-Filter('New*'): Bisque
    ///prov3.OGCFilter = filter4;
    ///
    /// // Layer Style
    ///layer1.Style.Fill = new SolidBrush(Color.IndianRed);    // States
    ///layer2.Style.Fill = new SolidBrush(Color.Green); // SelectedStatesAndHousholds
    ///layer3.Style.Fill = new SolidBrush(Color.Bisque); // e.g. New York, New Jersey,...
    ///layer5.Style.Fill = new SolidBrush(Color.LightBlue);
    ///
    /// // Labels
    /// // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
    /// // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
    /// // If a label property is set, the quick geometry option is automatically set to 'false'.
    ///prov3.Label = "STATE_NAME";
    ///SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("labels");
    ///layLabel.DataSource = prov3;
    ///layLabel.Enabled = true;
    ///layLabel.LabelColumn = prov3.Label;
    ///layLabel.Style = new SharpMap.Styles.LabelStyle();
    ///layLabel.Style.CollisionDetection = false;
    ///layLabel.Style.CollisionBuffer = new SizeF(5, 5);
    ///layLabel.Style.ForeColor = Color.Black;
    ///layLabel.Style.Font = new Font(FontFamily.GenericSerif, 10);
    ///layLabel.MaxVisible = 90;
    ///layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
    /// // Options 
    /// // Defaults: MultiGeometries: true, QuickGeometries: false, GetFeatureGETRequest: false
    /// // Render with validation...
    ///prov1.QuickGeometries = false;
    /// // Important when connecting to an UMN MapServer
    ///prov1.GetFeatureGETRequest = true;
    /// // Ignore multi-geometries...
    ///prov1.MultiGeometries = false;
    ///
    /// // Quick geometries
    /// // We need this option for prov2 since we have not passed a featuretype namespace
    ///prov2.QuickGeometries = true;
    ///prov4.QuickGeometries = true;
    ///prov5.QuickGeometries = true;
    ///prov6.QuickGeometries = true;
    ///
    ///layer1.DataSource = prov1;
    ///layer2.DataSource = prov2;
    ///layer3.DataSource = prov3;
    ///layer4.DataSource = prov4;
    ///layer5.DataSource = prov5;
    ///layer6.DataSource = prov6;
    ///
    ///demoMap.Layers.Add(layer1);
    ///demoMap.Layers.Add(layer2);
    ///demoMap.Layers.Add(layer3);
    ///demoMap.Layers.Add(layer4);
    ///demoMap.Layers.Add(layer5);
    ///demoMap.Layers.Add(layer6);
    ///demoMap.Layers.Add(layLabel);
    ///
    ///demoMap.Center = new SharpMap.Geometries.Coordinate(-74.0, 40.7);
    ///demoMap.Zoom = 10;
    /// // Alternatively zoom closer
    /// // demoMap.Zoom = 0.2;
    /// // Render map
    ///this.mapImage1.Image = demoMap.GetMap();
    /// </code> 
    ///</example>
    public partial class WFS : IProvider
    {
        #region Enumerations

        /// <summary>
        /// This enumeration consists of expressions denoting WFS versions.
        /// </summary>
        public enum WFSVersionEnum
        {
            /// <summary>
            /// Version 1.0.0
            /// </summary>
            WFS1_0_0,
            /// <summary>
            /// Version 1.1.0
            /// </summary>
            WFS1_1_0
        } ;

        #endregion

        #region Fields

        // Info about the featuretype to query obtained from 'GetCapabilites' and 'DescribeFeatureType'

        private readonly GeometryTypeEnum _geometryType = GeometryTypeEnum.Unknown;
        private readonly string _getCapabilitiesUri;
        private readonly HttpClientUtil _httpClientUtil = new HttpClientUtil();
        private readonly IWFS_TextResources _textResources;

        private readonly WFSVersionEnum _wfsVersion;

        private bool _disposed;
        private string _featureType;
        private WfsFeatureTypeInfo _featureTypeInfo;
        private IXPathQueryManager _featureTypeInfoQueryManager;
        private bool _isOpen;
        private FeatureDataTable _labelInfo;
        private int[] _axisOrder;

        /// <summary>
        /// Tree used for fast query of data
        /// </summary>
        private ISpatialIndex<uint> _tree;

        private string _nsPrefix;

        // The type of geometry can be specified in case of unprecise information (e.g. 'GeometryAssociationType').
        // It helps to accelerate the rendering process significantly.

        #endregion

        #region Properties

        private bool _getFeatureGETRequest;
        private string _label;
        private bool _multiGeometries = true;
        private IFilter _ogcFilter;
        private bool _quickGeometries;

        /// <summary>
        /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
        /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
        /// </summary>
        public IXPathQueryManager GetCapabilitiesCache
        {
            get { return _featureTypeInfoQueryManager; }
            set { _featureTypeInfoQueryManager = value; }
        }

        /// <summary>
        /// Gets feature metadata 
        /// </summary>
        public WfsFeatureTypeInfo FeatureTypeInfo
        {
            get { return _featureTypeInfo; }
        }

        /// <summary>
        /// Gets or sets a value indicating the axis order
        /// </summary>
        /// <remarks>
        /// The axis order is an array of array offsets. It can be einter {0, 1} or {1, 0}.
        /// <para/>If not set explictly, <see cref="AxisOrderRegistry"/> is asked for a value based on <see cref="SRID"/>.</remarks>
        public int[] AxisOrder
        {
            get { return _axisOrder ?? new AxisOrderRegistry()[SRID.ToString(NumberFormatInfo.InvariantInfo)]; }
            set
            {
                if (value != null)
                {
                    if (value.Length != 2)
                        throw new ArgumentException("Axis order array must have 2 elements");
                    if (!((value[0] == 0 && value[1] == 1)||
                          (value[0] == 1 && value[1] == 0)))
                        throw new ArgumentException("Axis order array values must be 0 or 1");
                    if (value[0] + value[1] != 1)
                        throw new ArgumentException("Sum of values in axis order array must 1");
                }
                _axisOrder = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the spatial index factory
        /// </summary>
        public static ISpatialIndexFactory<uint> SpatialIndexFactory = new QuadTreeFactory();

        /// <summary>
        /// Gets or sets a value indicating whether extracting geometry information 
        /// from 'GetFeature' response shall be done quickly without paying attention to
        /// context validation, polygon boundaries and multi-geometries.
        /// This option accelerates the geometry parsing process, 
        /// but in scarce cases can lead to errors. 
        /// </summary>
        public bool QuickGeometries
        {
            get { return _quickGeometries; }
            set { _quickGeometries = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the 'GetFeature' parser
        /// should ignore multi-geometries (MultiPoint, MultiLineString, MultiCurve, MultiPolygon, MultiSurface). 
        /// By default it does not. Ignoring multi-geometries can lead to a better performance.
        /// </summary>
        public bool MultiGeometries
        {
            get { return _multiGeometries; }
            set { _multiGeometries = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the 'GetFeature' request
        /// should be done with HTTP GET. This option can be important when obtaining
        /// data from a WFS provided by an UMN MapServer.
        /// </summary>
        public bool GetFeatureGETRequest
        {
            get { return _getFeatureGETRequest; }
            set { _getFeatureGETRequest = value; }
        }

        /// <summary>
        /// Gets or sets an OGC Filter.
        /// </summary>
        public IFilter OGCFilter
        {
            get { return _ogcFilter; }
            set { _ogcFilter = value; }
        }

        /// <summary>
        /// Gets or sets the property of the featuretype responsible for labels
        /// </summary>
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>
        /// Gets or sets the network credentials used for authenticating the request with the Internet resource
        /// </summary>
        public ICredentials Credentials
        {
            get { return _httpClientUtil.Credentials; }
            set { _httpClientUtil.Credentials = value; }
        }

        /// <summary>
        /// Gets and sets the proxy Url of the request. 
        /// </summary>
        public string ProxyUrl
        {
            get { return _httpClientUtil.ProxyUrl; }
            set { _httpClientUtil.ProxyUrl = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="getCapabilitiesURI">The URL for the 'GetCapabilities' request.</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureType">The name of the feature type</param>
        /// <param name="geometryType">
        /// Specifying the geometry type helps to accelerate the rendering process, 
        /// if the geometry type in 'DescribeFeatureType is unprecise.   
        /// </param>
        /// <param name="proxyUrl">Optional Proxy url</param>
        /// <param name="wfsVersion">The desired WFS Server version.</param>
        public WFS(string getCapabilitiesURI, string nsPrefix, string featureType, GeometryTypeEnum geometryType,
                   WFSVersionEnum wfsVersion, string proxyUrl = null)
        {
            _getCapabilitiesUri = getCapabilitiesURI;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else 
                _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;

            if (string.IsNullOrEmpty(nsPrefix))
                ResolveFeatureType(featureType);
            else
            {
                _nsPrefix = nsPrefix;
                _featureType = featureType;
            }

            _geometryType = geometryType;
            ProxyUrl = proxyUrl;
            GetFeatureTypeInfo();
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="getCapabilitiesURI">The URL for the 'GetCapabilities' request.</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureType">The name of the feature type</param>
        /// <param name="wfsVersion">The desired WFS Server version.</param>
        public WFS(string getCapabilitiesURI, string nsPrefix, string featureType, WFSVersionEnum wfsVersion)
            : this(getCapabilitiesURI, nsPrefix, featureType, GeometryTypeEnum.Unknown, wfsVersion)
        {
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with a 
        /// <see cref="WfsFeatureTypeInfo"/> object, 
        /// so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
        /// </summary>
        public WFS(WfsFeatureTypeInfo featureTypeInfo, WFSVersionEnum wfsVersion)
        {
            _featureTypeInfo = featureTypeInfo;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all mandatory
        /// metadata for retrieving a featuretype, so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
        /// </summary>
        /// <param name="serviceURI">The service URL</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureTypeNamespace">
        /// Use an empty string or 'null', if there is no namespace for the featuretype.
        /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
        /// </param>
        /// <param name="geometryName">
        /// The name of the geometry.   
        /// </param>
        /// <param name="geometryType">
        /// Specifying the geometry type helps to accelerate the rendering process.   
        /// </param>
        /// <param name="featureType">The name of the feature type</param>
        /// <param name="wfsVersion">The desired WFS Server version.</param>
        public WFS(string serviceURI, string nsPrefix, string featureTypeNamespace, string featureType,
                   string geometryName, GeometryTypeEnum geometryType, WFSVersionEnum wfsVersion)
        {
            _featureTypeInfo = new WfsFeatureTypeInfo(serviceURI, nsPrefix, featureTypeNamespace, featureType,
                                                      geometryName, geometryType);

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all mandatory
        /// metadata for retrieving a featuretype, so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
        /// </summary>
        /// <param name="serviceURI">The service URL</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureTypeNamespace">
        /// Use an empty string or 'null', if there is no namespace for the featuretype.
        /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
        /// </param>
        /// <param name="geometryName">The name of the geometry</param>
        /// <param name="featureType">The name of the feature type</param>
        /// <param name="wfsVersion">The desired WFS Server version.</param>
        public WFS(string serviceURI, string nsPrefix, string featureTypeNamespace, string featureType,
                   string geometryName, WFSVersionEnum wfsVersion)
            : this(
                serviceURI, nsPrefix, featureTypeNamespace, featureType, geometryName, GeometryTypeEnum.Unknown,
                wfsVersion)
        {
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="getCapabilitiesCache">
        /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
        /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
        ///</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="geometryType">
        /// Specifying the geometry type helps to accelerate the rendering process, 
        /// if the geometry type in 'DescribeFeatureType is unprecise.   
        /// </param>
        /// <param name="featureType">The name of the feature type</param>
        /// <param name="wfsVersion">The desired WFS Server version.</param>
        /// <param name="proxyUrl">Optional proxy url</param>
        public WFS(IXPathQueryManager getCapabilitiesCache, string nsPrefix, string featureType,
                   GeometryTypeEnum geometryType, WFSVersionEnum wfsVersion, string proxyUrl = null)
        {
            _featureTypeInfoQueryManager = getCapabilitiesCache;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else 
                _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;

            if (string.IsNullOrEmpty(nsPrefix))
                ResolveFeatureType(featureType);
            else
            {
                _nsPrefix = nsPrefix;
                _featureType = featureType;
            }

            _geometryType = geometryType;
            ProxyUrl = proxyUrl;
            GetFeatureTypeInfo();
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="getCapabilitiesCache">
        /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
        /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
        ///</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureType">The name of the feature type</param>
        /// <param name="wfsVersion">The desired WFS Server version.</param>
        public WFS(IXPathQueryManager getCapabilitiesCache, string nsPrefix, string featureType,
                   WFSVersionEnum wfsVersion)
            : this(getCapabilitiesCache, nsPrefix, featureType, GeometryTypeEnum.Unknown, wfsVersion)
        {
        }

        #endregion

        #region IProvider Member

        /// <summary>
        /// Gets the features within the specified <see cref="SharpMap.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="SharpMap.Geometries.Envelope"/></returns>
        public virtual Collection<Geometry> GetGeometriesInView(Envelope bbox)
        {
            if (_featureTypeInfo == null) 
                return null;

            // if cache is not enabled make a call to server with the provided bounding box
            if (!UseCache || Label == null)
            {
                _tree = null;
                return LoadGeometries(bbox);
            }

            // if cache is enabled but data is not downloaded then make a server call with an infinite envelope to download all the geometries
            if (_labelInfo == null)
            {
                LoadGeometries(new Envelope(double.MinValue, double.MaxValue, double.MinValue, double.MaxValue));

                // creates the spatial index
                var extent = GetExtents();

                _tree = SpatialIndexFactory.Create(extent, _labelInfo.Count,
                    _labelInfo.Rows
                        .Cast<FeatureDataRow>()
                        .Select((row, idx) => SpatialIndexFactory.Create((uint) idx, row.Geometry.EnvelopeInternal)));
            }

            // we then must filter the geometries locally
            var ids = _tree.Search(bbox);

            var coll = new Collection<Geometry>();
            for (var i = 0; i < ids.Count; i++)
            {
                var featureRow = (FeatureDataRow) _labelInfo.Rows[(int)ids[i]];
                coll.Add(featureRow.Geometry);
            }

            return coll;
        }

        /// <summary>
        /// Returns all objects whose <see cref="SharpMap.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="SharpMap.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown in any case</exception>
        public virtual Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        /// <exception cref="Exception">Thrown in any case</exception>
        public virtual Geometry GetGeometryByID(uint oid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public virtual void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            if (_labelInfo == null) return;

            var table = _labelInfo.Clone();

            if (_tree != null)
            {
                // use the index for fast query
                var ids = _tree.Search(geom.EnvelopeInternal);
                for (var i = 0; i < ids.Count; i++)
                {
                    var featureRow = (FeatureDataRow)_labelInfo.Rows[(int)ids[i]];
                    var featureGeometry = featureRow.Geometry;
                    if (featureGeometry.Intersects(geom))
                    {
                        var newRow = (FeatureDataRow) table.Rows.Add(featureRow.ItemArray);
                        newRow.Geometry = featureGeometry;
                    }
                }
            }
            else
            {
                for (var i = 0; i < _labelInfo.Rows.Count; i++)
                {
                    var featureRow = (FeatureDataRow) _labelInfo.Rows[i];
                    var featureGeometry = featureRow.Geometry;
                    if (featureGeometry.Intersects(geom))
                    {
                        var newRow = (FeatureDataRow) table.Rows.Add(featureRow.ItemArray);
                        newRow.Geometry = featureGeometry;
                    }
                }
            }
            ds.Tables.Add(table);
            // Destroy internal reference if cache is disabled
            if (!UseCache)
                _labelInfo = null;
        }


        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public virtual void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            if (_labelInfo == null) return;

            var table = _labelInfo.Clone();

            if (_tree != null)
            {
                // use the index for fast query
                
                var ids = _tree.Search(box);
                for (var i = 0; i < ids.Count; i++)
                {
                    var featureRow = (FeatureDataRow)_labelInfo.Rows[(int)ids[i]];
                    var featureGeometry = featureRow.Geometry;
                    var newRow = (FeatureDataRow)table.Rows.Add(featureRow.ItemArray);
                    newRow.Geometry = featureGeometry;
                }
            }
            else
            {
                // we must filter the geometries locally
                for (var i = 0; i < _labelInfo.Rows.Count; i++)
                {
                    var featureRow = (FeatureDataRow) _labelInfo.Rows[i];
                    var featureGeometry = featureRow.Geometry;
                    if (box.Intersects(featureGeometry.EnvelopeInternal))
                    {
                        var newRow = (FeatureDataRow) table.Rows.Add(featureRow.ItemArray);
                        newRow.Geometry = featureGeometry;
                    }
                }
            }
            ds.Tables.Add(table);
            // Destroy internal reference
            if (!UseCache)
                _labelInfo = null;
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        /// <exception cref="Exception">Thrown in any case</exception>
        public virtual int GetFeatureCount()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="rowId">The id of the row.</param>
        /// <returns>datarow</returns>
        /// <exception cref="Exception">Thrown in any case</exception>
        public virtual FeatureDataRow GetFeature(uint rowId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// The <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>The 2d extent of the layer</returns>
        public virtual Envelope GetExtents()
        {
            if (!UseCache || _labelInfo == null || _labelInfo.Rows.Count == 0)
            {
                return new Envelope(new Coordinate(_featureTypeInfo.BBox._MinLong, _featureTypeInfo.BBox._MinLat),
                    new Coordinate(_featureTypeInfo.BBox._MaxLong, _featureTypeInfo.BBox._MaxLat));
            }

            // here we try to fix a problem that happens when the server provides an incorrect bounding box for the data
            // we simply calculate the extent from all the geometries we got.

            Envelope env = null;

            for (var i = 0; i < _labelInfo.Rows.Count; i++)
            {
                var featureRow = (FeatureDataRow)_labelInfo.Rows[i];
                var geom = featureRow.Geometry;

                env = env == null ? geom.EnvelopeInternal : env.ExpandedBy(geom.EnvelopeInternal);
            }
            return env;
        }

        /// <summary>
        /// Gets the service-qualified name of the featuretype.
        /// The service-qualified name enables the differentiation between featuretypes 
        /// from different services with an equal qualified name and therefore can be
        /// regarded as an ID for the featuretype.
        /// </summary>
        public string ConnectionID
        {
            get { return _featureTypeInfo.ServiceURI + "/" + _featureTypeInfo.QualifiedName; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public virtual void Open()
        {
            _isOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public virtual void Close()
        {
            _isOpen = false;
            _httpClientUtil.Close();
        }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public virtual int SRID
        {
            get { return Convert.ToInt32(_featureTypeInfo.SRID); }
            set { _featureTypeInfo.SRID = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled.
        /// </summary>
        /// <remarks>
        /// When cache is enabled all geometries are downloaded from server depending on the OGC filter set, 
        /// and then cached on client to fullfill next requests.
        /// </remarks>
        public bool UseCache { get; set; }

        #endregion

        #region IDisposable Member

        /// <summary>
        /// Method to perform cleanup work
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Implementation of the Dispose patter
        /// </summary>
        /// <param name="disposing">Flag indicating if called from <see cref="Dispose()"/> or a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _featureTypeInfoQueryManager = null;
                    _labelInfo = null;
                    _httpClientUtil.Close();
                }
                _disposed = true;
            }
        }

        #endregion

        #region Private Member

        private Collection<Geometry> LoadGeometries(Envelope bbox)
        {
            var geometryTypeString = _featureTypeInfo.Geometry._GeometryType;

            Utilities.Wfs.GeometryFactory geomFactory = null;

            if (UseCache)
            {
                // we want to download all the elements of the feature
                _labelInfo = new FeatureDataTable();
                foreach (var element in FeatureTypeInfo.Elements)
                    _labelInfo.Columns.Add(element.Name);

                _quickGeometries = false;
            }
            else if (!string.IsNullOrEmpty(_label))
            {
                _labelInfo = new FeatureDataTable();
                _labelInfo.Columns.Add(_label);
                // Turn off quick geometries, if a label is applied...
                _quickGeometries = false;
            }

            // Configuration for GetFeature request */
            WFSClientHTTPConfigurator config = new WFSClientHTTPConfigurator(_textResources);
            config.configureForWfsGetFeatureRequest(_httpClientUtil, _featureTypeInfo, _label, bbox, _ogcFilter,
                                                    _getFeatureGETRequest, UseCache);

            try
            {
                Collection<Geometry> geoms;
                switch (geometryTypeString)
                {
                    /* Primitive geometry elements */

                    // GML2
                    case "PointPropertyType":
                        geomFactory = new PointFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML2
                    case "LineStringPropertyType":
                        geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML2
                    case "PolygonPropertyType":
                        geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML3
                    case "CurvePropertyType":
                        geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML3
                    case "SurfacePropertyType":
                        geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    /* Aggregate geometry elements */

                    // GML2
                    case "MultiPointPropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiPointFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new PointFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML2
                    case "MultiLineStringPropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiLineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML2
                    case "MultiPolygonPropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiPolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML3
                    case "MultiCurvePropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiLineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // GML3
                    case "MultiSurfacePropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiPolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                    // .e.g. 'gml:GeometryAssociationType' or 'GeometryPropertyType'
                    //It's better to set the geometry type manually, if it is known...
                    default:
                        geomFactory = new UnspecifiedGeometryFactory_WFS1_0_0_GML2(_httpClientUtil, _featureTypeInfo,
                                                                                   _multiGeometries, _quickGeometries,
                                                                                   _labelInfo);

                        geomFactory.AxisOrder = AxisOrder;
                        geoms = geomFactory.createGeometries();

                        return geoms;
                }

                geomFactory.AxisOrder = AxisOrder;
                geoms = _quickGeometries
                            ? geomFactory.createQuickGeometries(geometryTypeString)
                            : geomFactory.createGeometries();

                return geoms;
            }
            // Free resources (net connection of geometry factory)
            finally
            {
                if (geomFactory != null)
                {
                    geomFactory.Dispose();
                }
            }
        }
        /// <summary>
        /// This method gets metadata about the featuretype to query from 'GetCapabilities' and 'DescribeFeatureType'.
        /// </summary>
        private void GetFeatureTypeInfo()
        {
            try
            {
                _featureTypeInfo = new WfsFeatureTypeInfo();
                WFSClientHTTPConfigurator config = new WFSClientHTTPConfigurator(_textResources);

                _featureTypeInfo.Prefix = _nsPrefix;
                _featureTypeInfo.Name = _featureType;

                string featureQueryName = string.IsNullOrEmpty(_nsPrefix)
                                              ? _featureType
                                              : _nsPrefix + ":" + _featureType;

                /***************************/
                /* GetCapabilities request  /
                /***************************/

                if (_featureTypeInfoQueryManager == null)
                {
                    /* Initialize IXPathQueryManager with configured HttpClientUtil */
                    _featureTypeInfoQueryManager =
                        new XPathQueryManager_CompiledExpressionsDecorator(new XPathQueryManager());
                    _featureTypeInfoQueryManager.SetDocumentToParse(
                        config.configureForWfsGetCapabilitiesRequest(_httpClientUtil, _getCapabilitiesUri));
                    /* Namespaces for XPath queries */
                    _featureTypeInfoQueryManager.AddNamespace(_textResources.NSWFSPREFIX, _textResources.NSWFS);
                    _featureTypeInfoQueryManager.AddNamespace(_textResources.NSOWSPREFIX, _textResources.NSOWS);
                    _featureTypeInfoQueryManager.AddNamespace(_textResources.NSXLINKPREFIX, _textResources.NSXLINK);
                }

                /* Service URI (for WFS GetFeature request) */
                _featureTypeInfo.ServiceURI = _featureTypeInfoQueryManager.GetValueFromNode
                    (_featureTypeInfoQueryManager.Compile(_textResources.XPATH_GETFEATURERESOURCE));
                /* If no GetFeature URI could be found, try GetCapabilities URI */
                if (_featureTypeInfo.ServiceURI == null) _featureTypeInfo.ServiceURI = _getCapabilitiesUri;
                else if (_featureTypeInfo.ServiceURI.EndsWith("?", StringComparison.Ordinal))
                    _featureTypeInfo.ServiceURI =
                        _featureTypeInfo.ServiceURI.Remove(_featureTypeInfo.ServiceURI.Length - 1);

                /* URI for DescribeFeatureType request */
                string describeFeatureTypeUri = _featureTypeInfoQueryManager.GetValueFromNode
                    (_featureTypeInfoQueryManager.Compile(_textResources.XPATH_DESCRIBEFEATURETYPERESOURCE));
                /* If no DescribeFeatureType URI could be found, try GetCapabilities URI */
                if (describeFeatureTypeUri == null) describeFeatureTypeUri = _getCapabilitiesUri;
                else if (describeFeatureTypeUri.EndsWith("?", StringComparison.Ordinal))
                    describeFeatureTypeUri =
                        describeFeatureTypeUri.Remove(describeFeatureTypeUri.Length - 1);

                /* Spatial reference ID */
                var crs = _featureTypeInfoQueryManager.GetValueFromNode(
                    _featureTypeInfoQueryManager.Compile(_textResources.XPATH_SRS),
                    new[] {new DictionaryEntry("_param1", featureQueryName)});
                /* If no SRID could be found, try '4326' by default */
                if (crs == null) _featureTypeInfo.SRID = "4326";
                else
                    /* Extract number */
                    _featureTypeInfo.SRID = crs.Substring(crs.LastIndexOf(":") + 1);

                /* Bounding Box */
                IXPathQueryManager bboxQuery = _featureTypeInfoQueryManager.GetXPathQueryManagerInContext(
                    _featureTypeInfoQueryManager.Compile(_textResources.XPATH_BBOX),
                    new[] {new DictionaryEntry("_param1", featureQueryName)});

                if (bboxQuery != null)
                {
                    WfsFeatureTypeInfo.BoundingBox bbox = new WfsFeatureTypeInfo.BoundingBox();
                    NumberFormatInfo formatInfo = new NumberFormatInfo();
                    formatInfo.NumberDecimalSeparator = ".";
                    string bboxVal = null;

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MinLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINY))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MinLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINY))) !=
                                null
                                    ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MaxLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXY))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MaxLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXY))) !=
                                null
                                    ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MinLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINX))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MinLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINX))) !=
                                null
                                    ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MaxLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXX))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MaxLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXX))) !=
                                null
                                    ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (SRID != 4326)
                    {
                        // we must to transform the bbox coordinates into the SRS projection
                        var transformation = Session.Instance.CoordinateSystemServices.CreateTransformation(4326, SRID);
                        if (transformation == null)
                            throw new InvalidOperationException("Can't transform geometries to layer SRID");

                        var maxPoint = transformation.MathTransform.Transform(new[] { bbox._MaxLong, bbox._MaxLat });
                        var minPoint = transformation.MathTransform.Transform(new[] { bbox._MinLong, bbox._MinLat });

                        bbox._MaxLong = maxPoint[0];
                        bbox._MaxLat = maxPoint[1];
                        bbox._MinLong = minPoint[0];
                        bbox._MinLat = minPoint[1];
                    }
                    _featureTypeInfo.BBox = bbox;
                }

                //Continue with a clone in order to preserve the 'GetCapabilities' response
                IXPathQueryManager describeFeatureTypeQueryManager = _featureTypeInfoQueryManager.Clone();

                /******************************/
                /* DescribeFeatureType request /
                /******************************/

                /* Initialize IXPathQueryManager with configured HttpClientUtil */
                describeFeatureTypeQueryManager.ResetNamespaces();
                describeFeatureTypeQueryManager.SetDocumentToParse(config.configureForWfsDescribeFeatureTypeRequest
                                                                       (_httpClientUtil, describeFeatureTypeUri,
                                                                        featureQueryName));

                /* Namespaces for XPath queries */
                describeFeatureTypeQueryManager.AddNamespace(_textResources.NSSCHEMAPREFIX, _textResources.NSSCHEMA);
                describeFeatureTypeQueryManager.AddNamespace(_textResources.NSGMLPREFIX, _textResources.NSGML);

                /* Get target namespace */
                string targetNs = describeFeatureTypeQueryManager.GetValueFromNode(
                    describeFeatureTypeQueryManager.Compile(_textResources.XPATH_TARGETNS));
                if (targetNs != null)
                    _featureTypeInfo.FeatureTypeNamespace = targetNs;

                /* Get geometry */
                string geomType = _geometryType == GeometryTypeEnum.Unknown ? null : _geometryType.ToString();
                string geomName = null;
                string geomComplexTypeName = null;

                /* The easiest way to get geometry info, just ask for the 'gml'-prefixed type-attribute... 
                   Simple, but effective in 90% of all cases...this is the standard GeoServer creates.*/
                /* example: <xs:element nillable = "false" name = "the_geom" maxOccurs = "1" type = "gml:MultiPolygonPropertyType" minOccurs = "0" /> */
                /* Try to get context of the geometry element by asking for a 'gml:*' type-attribute */
                IXPathQueryManager geomQuery = describeFeatureTypeQueryManager.GetXPathQueryManagerInContext(
                    describeFeatureTypeQueryManager.Compile(_textResources.XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY));
                if (geomQuery != null)
                {
                    geomName = geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_NAMEATTRIBUTEQUERY));

                    /* Just, if not set manually... */
                    if (geomType == null)
                        geomType = geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_TYPEATTRIBUTEQUERY));

                    /* read all the elements */
                    var iterator = geomQuery.GetIterator(geomQuery.Compile("//ancestor::xs:sequence/xs:element"));
                    foreach (XPathNavigator node in iterator)
                    {
                        node.MoveToAttribute("type", string.Empty);
                        var type = node.Value;

                        if (type.StartsWith("gml:")) // we skip geometry element cause we already found it
                            continue;

                        node.MoveToParent();

                        node.MoveToAttribute("name", string.Empty);
                        var name = node.Value;

                        _featureTypeInfo.Elements.Add(new WfsFeatureTypeInfo.ElementInfo(name, type));
                    }
                }
                else
                {
                    /* Try to get context of a complexType with element ref ='gml:*' - use the global context */
                    /* example:
                    <xs:complexType name="geomType">
                        <xs:sequence>
                            <xs:element ref="gml:polygonProperty" minOccurs="0"/>
                        </xs:sequence>
                    </xs:complexType> */
                    geomQuery = describeFeatureTypeQueryManager.GetXPathQueryManagerInContext(
                        describeFeatureTypeQueryManager.Compile(
                            _textResources.XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY));
                    if (geomQuery != null)
                    {
                        /* Ask for the name of the complextype - use the local context*/
                        geomComplexTypeName =
                            geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_NAMEATTRIBUTEQUERY));

                        if (geomComplexTypeName != null)
                        {
                            /* Ask for the name of an element with a complextype of 'geomComplexType' - use the global context */
                            geomName =
                                describeFeatureTypeQueryManager.GetValueFromNode(
                                    describeFeatureTypeQueryManager.Compile(
                                        _textResources.XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY), new[]
                                                                                                  {
                                                                                                      new DictionaryEntry
                                                                                                          ("_param1",
                                                                                                           _featureTypeInfo
                                                                                                               .
                                                                                                               FeatureTypeNamespace)
                                                                                                      ,
                                                                                                      new DictionaryEntry
                                                                                                          ("_param2",
                                                                                                           geomComplexTypeName)
                                                                                                  });
                        }
                        else
                        {
                            /* The geometry element must be an ancestor, if we found an anonymous complextype */
                            /* Ask for the element hosting the anonymous complextype - use the global context */
                            /* example: 
                            <xs:element name ="SHAPE">
                                <xs:complexType>
                            	    <xs:sequence>
                              		    <xs:element ref="gml:lineStringProperty" minOccurs="0"/>
                                  </xs:sequence>
                                </xs:complexType>
                            </xs:element> */
                            geomName =
                                describeFeatureTypeQueryManager.GetValueFromNode(
                                    describeFeatureTypeQueryManager.Compile(
                                        _textResources.XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE));
                        }
                        /* Just, if not set manually... */
                        if (geomType == null)
                        {
                            /* Ask for the 'ref'-attribute - use the local context */
                            if (
                                (geomType =
                                 geomQuery.GetValueFromNode(
                                     geomQuery.Compile(_textResources.XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY))) != null)
                            {
                                switch (geomType)
                                {
                                    case "gml:pointProperty":
                                        geomType = "PointPropertyType";
                                        break;
                                    case "gml:lineStringProperty":
                                        geomType = "LineStringPropertyType";
                                        break;
                                    case "gml:curveProperty":
                                        geomType = "CurvePropertyType";
                                        break;
                                    case "gml:polygonProperty":
                                        geomType = "PolygonPropertyType";
                                        break;
                                    case "gml:surfaceProperty":
                                        geomType = "SurfacePropertyType";
                                        break;
                                    case "gml:multiPointProperty":
                                        geomType = "MultiPointPropertyType";
                                        break;
                                    case "gml:multiLineStringProperty":
                                        geomType = "MultiLineStringPropertyType";
                                        break;
                                    case "gml:multiCurveProperty":
                                        geomType = "MultiCurvePropertyType";
                                        break;
                                    case "gml:multiPolygonProperty":
                                        geomType = "MultiPolygonPropertyType";
                                        break;
                                    case "gml:multiSurfaceProperty":
                                        geomType = "MultiSurfacePropertyType";
                                        break;
                                        // e.g. 'gml:_geometryProperty' 
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                if (geomName == null)
                    /* Default value for geometry column = geom */
                    geomName = "geom";

                if (geomType == null)
                    /* Set geomType to an empty string in order to avoid exceptions.
                    The geometry type is not necessary by all means - it can be detected in 'GetFeature' response too.. */
                    geomType = string.Empty;

                /* Remove prefix */
                if (geomType.Contains(":"))
                    geomType = geomType.Substring(geomType.IndexOf(":") + 1);

                WfsFeatureTypeInfo.GeometryInfo geomInfo = new WfsFeatureTypeInfo.GeometryInfo();
                geomInfo._GeometryName = geomName;
                geomInfo._GeometryType = geomType;
                _featureTypeInfo.Geometry = geomInfo;
            }
            finally
            {
                _httpClientUtil.Close();
            }
        }

        private void ResolveFeatureType(string featureType)
        {
            if (featureType.Contains(":"))
            {
                var split = featureType.Split(':');
                _nsPrefix = split[0];
                _featureType = split[1];
            }
            else
                _featureType = featureType;
        }

        #endregion

        #region Nested Types

        #region WFSClientHTTPConfigurator

        /// <summary>
        /// This class configures a <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> class 
        /// for requests to a Web Feature Service.
        /// </summary>
        private class WFSClientHTTPConfigurator
        {
            #region Fields

            private readonly IWFS_TextResources _WfsTextResources;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="WFS.WFSClientHTTPConfigurator"/> class.
            /// An instance of this class can be used to configure a <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> object.
            /// </summary>
            /// <param name="wfsTextResources">
            /// An instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources" /> 
            /// for getting version-specific text resources for WFS request configuration.
            ///</param>
            internal WFSClientHTTPConfigurator(IWFS_TextResources wfsTextResources)
            {
                _WfsTextResources = wfsTextResources;
            }

            #endregion

            #region Internal Member

            /// <summary>
            /// Configures for WFS 'GetCapabilities' request using an instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
            /// </summary>
            internal HttpClientUtil configureForWfsGetCapabilitiesRequest(HttpClientUtil httpClientUtil,
                                                                          string targetUrl)
            {
                httpClientUtil.Reset();
                httpClientUtil.Url = targetUrl + _WfsTextResources.GetCapabilitiesRequest();
                return httpClientUtil;
            }

            /// <summary>
            /// Configures for WFS 'DescribeFeatureType' request using an instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
            /// </summary>
            internal HttpClientUtil configureForWfsDescribeFeatureTypeRequest(HttpClientUtil httpClientUtil,
                                                                              string targetUrl,
                                                                              string featureTypeName)
            {
                httpClientUtil.Reset();
                httpClientUtil.Url = targetUrl + _WfsTextResources.DescribeFeatureTypeRequest(featureTypeName);
                return httpClientUtil;
            }

            /// <summary>
            /// Configures for WFS 'GetFeature' request using an instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
            /// </summary>
            internal HttpClientUtil configureForWfsGetFeatureRequest(HttpClientUtil httpClientUtil,
                                                                     WfsFeatureTypeInfo featureTypeInfo,
                                                                     string labelProperty, Envelope boundingBox,
                                                                     IFilter filter, bool GET, bool loadAllElements)
            {
                httpClientUtil.Reset();
                httpClientUtil.Url = featureTypeInfo.ServiceURI;

                if (GET)
                {
                    /* HTTP-GET */
                    httpClientUtil.Url += _WfsTextResources.GetFeatureGETRequest(featureTypeInfo, boundingBox, filter, loadAllElements);
                    return httpClientUtil;
                }

                /* HTTP-POST */
                httpClientUtil.PostData = _WfsTextResources.GetFeaturePOSTRequest(featureTypeInfo, labelProperty,
                                                                                  boundingBox, filter, loadAllElements);
                httpClientUtil.AddHeader(HttpRequestHeader.ContentType.ToString(), "text/xml");
                return httpClientUtil;
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
