// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using GeoAPI;
using GeoAPI.Geometries;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// Web Feature Service geometry types
    /// </summary>
    public enum GeometryTypeEnum
    {
        /// <summary>
        /// Point
        /// </summary>
        PointPropertyType,
        /// <summary>
        /// Linestring
        /// </summary>
        LineStringPropertyType,
        /// <summary>
        /// Curve
        /// </summary>
        CurvePropertyType,
        /// <summary>
        /// Polygon
        /// </summary>
        PolygonPropertyType,
        /// <summary>
        /// Surface
        /// </summary>
        SurfacePropertyType,
        /// <summary>
        /// Multipoint
        /// </summary>
        MultiPointPropertyType,
        /// <summary>
        /// MultiLinestring
        /// </summary>
        MultiLineStringPropertyType,
        /// <summary>
        /// Multicurve
        /// </summary>
        MultiCurvePropertyType,
        /// <summary>
        /// MultiPolygon
        /// </summary>
        MultiPolygonPropertyType,
        /// <summary>
        /// MultiSurface
        /// </summary>
        MultiSurfacePropertyType,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    };


    /// <summary>
    /// Web Feature Type info class
    /// </summary>
    public class WfsFeatureTypeInfo
    {
        #region Fields with Properties

        internal IGeometryFactory Factory { get; private set; }

        private BoundingBox _BoundingBox = new BoundingBox();
        private string _Cs = ",";
        private string _DecimalDel = ".";
        private string _FeatureTypeNamespace = string.Empty;
        private GeometryInfo _Geometry = new GeometryInfo();
        private string _Name = string.Empty;

        private string _Prefix = string.Empty;
        private string _ServiceURI = string.Empty;
        private string _SRID;
        private string _Ts = " ";

        /// <summary>
        /// Gets or sets the name of the featuretype.
        /// This argument is obligatory for data retrieving.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Gets or sets the prefix of the featuretype and it's nested elements.
        /// This argument is obligatory for data retrieving, if the featuretype is declared with a 
        /// prefix in 'GetCapabilities'.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix
        {
            get { return _Prefix; }
            set { _Prefix = value; }
        }

        /// <summary>
        /// Gets or sets the featuretype namespace.
        /// This argument is obligatory for data retrieving, except when using the quick geometries option.
        /// </summary>
        public string FeatureTypeNamespace
        {
            get { return _FeatureTypeNamespace; }
            set { _FeatureTypeNamespace = value; }
        }

        /// <summary>
        /// Gets the qualified name of the featuretype (with namespace URI).
        /// </summary>
        internal string QualifiedName
        {
            get { return _FeatureTypeNamespace + _Name; }
        }

        /// <summary>
        /// Gets or sets the service URI for WFS 'GetFeature' request.
        /// This argument is obligatory for data retrieving.
        /// </summary>
        public string ServiceURI
        {
            get { return _ServiceURI; }
            set { _ServiceURI = value; }
        }

        /// <summary>
        /// Gets or sets information about the geometry of the featuretype.
        /// Setting at least the geometry name is obligatory for data retrieving.
        /// </summary>
        public GeometryInfo Geometry
        {
            get { return _Geometry; }
            set { _Geometry = value; }
        }

        /// <summary>
        /// Gets or sets the spatial extent of the featuretype - defined as minimum bounding rectangle. 
        /// </summary>
        public BoundingBox BBox
        {
            get { return _BoundingBox; }
            set { _BoundingBox = value; }
        }

        /// <summary>
        /// Gets or sets the spatial reference ID
        /// </summary>
        public string SRID
        {
            get { return _SRID; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "4326";
                if (string.Compare(_SRID, value, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    Factory = GeometryServiceProvider.Instance.CreateGeometryFactory(int.Parse(value));
                    _SRID = value;
                }
            }
        }

        //Coordinates can be included in a single string, but there is no 
        //facility for validating string content. The value of the 'cs' attribute 
        //is the separator for coordinate values, and the value of the 'ts' 
        //attribute gives the tuple separator (a single space by default); the 
        //default values may be changed to reflect local usage.

        /// <summary>
        /// Decimal separator (for gml:coordinates)
        /// </summary>
        public string DecimalDel
        {
            get { return _DecimalDel; }
            set { _DecimalDel = value; }
        }

        /// <summary>
        /// Separator for coordinate values (for gml:coordinates)
        /// </summary>
        public string Cs
        {
            get { return _Cs; }
            set { _Cs = value; }
        }

        /// <summary>
        /// Tuple separator (for gml:coordinates)
        /// </summary>
        public string Ts
        {
            get { return _Ts; }
            set { _Ts = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WfsFeatureTypeInfo"/> class.
        /// </summary>
        /// <param name="serviceUri">
        /// The Web Feature Service URI
        /// </param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureTypeNamespace">
        /// Use an empty string or 'null', if there is no namespace for the featuretype.
        /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
        /// </param>
        /// <param name="featureType">
        /// The feature type name
        /// </param>
        /// <param name="geometryName">
        /// The geometry name is the property of the featuretype from which geometry information can be obtained from.
        /// Usually this property is called something like 'Shape' or 'geom'. It is absolutely necessary to give this parameter. 
        /// </param>
        /// <param name="geometryType">
        /// Specifying the geometry type helps to accelerate the rendering process.   
        /// </param>
        public WfsFeatureTypeInfo(string serviceUri, string nsPrefix, string featureTypeNamespace, string featureType,
                                  string geometryName, GeometryTypeEnum geometryType)
            :this()
        {
            _ServiceURI = serviceUri;
            _Prefix = nsPrefix;
            _FeatureTypeNamespace = string.IsNullOrEmpty(featureTypeNamespace) ? string.Empty : featureTypeNamespace;
            _Name = featureType;
            _Geometry._GeometryName = geometryName;
            _Geometry._GeometryType = geometryType.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WfsFeatureTypeInfo"/> class.
        /// </summary>
        /// <param name="serviceUri">
        /// The Web Feature Service URI
        /// </param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureTypeNamespace">
        /// Use an empty string or 'null', if there is no namespace for the featuretype.
        /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
        /// </param>
        /// <param name="featureType">
        /// The feature type name
        /// </param>
        /// <param name="geometryName">
        /// The geometry name is the property of the featuretype from which geometry information can be obtained from.
        /// Usually this property is called something like 'Shape' or 'geom'. It is absolutely necessary to give this parameter. 
        /// </param>
        public WfsFeatureTypeInfo(string serviceUri, string nsPrefix, string featureTypeNamespace, string featureType,
                                  string geometryName)
            : this(serviceUri, nsPrefix, featureTypeNamespace, featureType, geometryName, GeometryTypeEnum.Unknown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WfsFeatureTypeInfo"/> class.
        /// </summary>
        public WfsFeatureTypeInfo()
        {
            SRID = "4326";
        }

        #endregion

        #region Nested Types

        #region BoundingBox

        /// <summary>
        /// The bounding box defines the spatial extent of a featuretype.
        /// </summary>
        public class BoundingBox
        {
            /// <summary>
            /// Maximum latitude
            /// </summary>
            public double _MaxLat;
            /// <summary>
            /// Maximum longitude
            /// </summary>
            public double _MaxLong;
            /// <summary>
            /// Minimum latitude
            /// </summary>
            public double _MinLat;
            /// <summary>
            /// Minimum longitude
            /// </summary>
            public double _MinLong;
        }

        #endregion

        #region GeometryInfo

        /// <summary>
        /// The geometry info comprises the name of the geometry attribute (e.g. 'Shape" or 'geom')
        /// and the type of the featuretype's geometry.
        /// </summary>
        public class GeometryInfo
        {
            /// <summary>
            /// The name of the geometry, may be 'shape' or 'geom'
            /// </summary>
            public string _GeometryName = string.Empty;
            /// <summary>
            /// The type of the features's geometry
            /// </summary>
            public string _GeometryType = string.Empty;
        }

        #endregion

        #endregion
    }
}