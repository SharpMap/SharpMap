using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using GeoAPI.Geometries;
using Common.Logging;

namespace SharpMap.Web.Wms
{
    /// <summary>
    /// Class for requesting and parsing a WMS servers capabilities
    /// </summary>
    [Serializable]
    public class Client : IClient
    {
        static ILog logger = LogManager.GetLogger(typeof(Client));

        private XmlNamespaceManager _nsmgr;


        #region WMS Data structures

        #region Nested type: WmsLayerStyle

        /// <summary>
        /// Structure for storing information about a WMS Layer Style
        /// </summary>
        public struct WmsLayerStyle
        {
            /// <summary>
            /// Abstract
            /// </summary>
            public string Abstract;

            /// <summary>
            /// Legend
            /// </summary>
            public WmsStyleLegend LegendUrl;

            /// <summary>
            /// Name
            /// </summary>
            public string Name;

            /// <summary>
            /// Style Sheet Url
            /// </summary>
            public WmsOnlineResource StyleSheetUrl;

            /// <summary>
            /// Title
            /// </summary>
            public string Title;
        }

        #endregion

        #region Nested type: WmsOnlineResource

        /// <summary>
        /// Structure for storing info on an Online Resource
        /// </summary>
        public struct WmsOnlineResource
        {
            /// <summary>
            /// URI of online resource
            /// </summary>
            public string OnlineResource;

            /// <summary>
            /// Type of online resource (Ex. request method 'Get' or 'Post')
            /// </summary>
            public string Type;
        }

        #endregion

        #region Nested type: WmsServerLayer

        /// <summary>
        /// Structure for holding information about a WMS Layer 
        /// </summary>
        public struct WmsServerLayer
        {
            /// <summary>
            /// Abstract
            /// </summary>
            public string Abstract;

            /// <summary>
            /// Collection of child layers
            /// </summary>
            public WmsServerLayer[] ChildLayers;

            /// <summary>
            /// Coordinate Reference Systems supported by layer
            /// </summary>
            public string[] CRS;

            /// <summary>
            /// Keywords
            /// </summary>
            public string[] Keywords;

            /// <summary>
            /// Latitudal/longitudal extent of this layer
            /// </summary>
            public Envelope LatLonBoundingBox;

            /// <summary>
            /// Extent of this layer in spatial reference system
            /// </summary>
            public List<SpatialReferencedBoundingBox> SRIDBoundingBoxes;

            /// <summary>
            /// Unique name of this layer used for requesting layer
            /// </summary>
            public string Name;

            /// <summary>
            /// Specifies whether this layer is queryable using GetFeatureInfo requests
            /// </summary>
            public bool Queryable;

            /// <summary>
            /// List of styles supported by layer
            /// </summary>
            public WmsLayerStyle[] Style;

            /// <summary>
            /// Layer title
            /// </summary>
            public string Title;
        }

        #endregion

        #region Nested type: WmsStyleLegend

        /// <summary>
        /// Structure for storing WMS Legend information
        /// </summary>
        public struct WmsStyleLegend
        {
            /// <summary>
            /// Online resource for legend style 
            /// </summary>
            public WmsOnlineResource OnlineResource;

            /// <summary>
            /// Size of legend
            /// </summary>
            public Size Size;
        }

        #endregion

        #endregion

        #region Properties

        private string[] _exceptionFormats;
        private Collection<string> _getMapOutputFormats;
        private WmsOnlineResource[] _getMapRequests;

        private Collection<string> _getCapabilitiesOutputFormats;
        private WmsOnlineResource[] _getCapabilitiesRequests;

        private Collection<string> _getFeatureInfoOutputFormats;
        private WmsOnlineResource[] _getFeatureInfoRequests;

        private Collection<string> _describeLayerOutputFormats;
        private WmsOnlineResource[] _describeLayerRequests;

        private Collection<string> _getLegendGraphicOutputFormats;
        private WmsOnlineResource[] _getLegendGraphicRequests;

        private Collection<string> _getStylesOutputFormats;
        private WmsOnlineResource[] _getStylesRequests;

        private Collection<string> _putStylesOutputFormats;
        private WmsOnlineResource[] _putStylesRequests;

        private WmsServerLayer _layer;
        private Capabilities.WmsServiceDescription _serviceDescription;

        private string _version;
        private XmlDocument _xmlDoc;
        private string _baseUrl;
        private string _capabilitiesUrl;
        private IWebProxy _proxy;
        private int _timeOut;
        private ICredentials _credentials = null;
        private XmlNode _vendorSpecificCapabilities;

        /// <summary>
        /// Timeout of webrequest in milliseconds.
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        ///<summary>
        ///</summary>
        public ICredentials Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        /// <summary>
        /// Gets the service description
        /// </summary>
        public Capabilities.WmsServiceDescription ServiceDescription
        {
            get { return _serviceDescription; }
        }

        /// <summary>
        /// Gets the version of the WMS server (ex. "1.3.0")
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        ///<summary>
        ///</summary>
        [Obsolete("Deprecated, use Version property instead.")]  
        public string WmsVersion
        {
            get { return _version; }
        }

        /// <summary>
        /// Gets the available GetMap request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetCapabilitiesRequests
        {
            get { return _getCapabilitiesRequests; }
        }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> GetCapabilitiesOutputFormats
        {
            get { return _getCapabilitiesOutputFormats; }
        }

        /// <summary>
        /// Gets the available GetFeatureInfo request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetFeatureInfoRequests
        {
            get { return _getFeatureInfoRequests; }
        }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> GetFeatureInfoOutputFormats
        {
            get { return _getFeatureInfoOutputFormats; }
        }

        /// <summary>
        /// Gets the available DescribeLayer request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] DescribeLayerRequests
        {
            get { return _describeLayerRequests; }
        }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> DescribeLayerOutputFormats
        {
            get { return _describeLayerOutputFormats; }
        }

        /// <summary>
        /// Gets the available GetLegendGraphic request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetLegendGraphicRequests
        {
            get { return _getLegendGraphicRequests; }
        }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> GetLegendGraphicOutputFormats
        {
            get { return _getLegendGraphicOutputFormats; }
        }

        /// <summary>
        /// Gets the available GetLegendGraphic request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetStylesRequests
        {
            get { return _getStylesRequests; }
        }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> GetStylesOutputFormats
        {
            get { return _getStylesOutputFormats; }
        }

        /// <summary>
        /// Gets the available GetLegendGraphic request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] PutStylesRequests
        {
            get { return _putStylesRequests; }
        }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> PutStylesOutputFormats
        {
            get { return _putStylesOutputFormats; }
        }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> GetMapOutputFormats
        {
            get { return _getMapOutputFormats; }
        }

        /// <summary>
        /// Gets a list of available exception mime type formats
        /// </summary>
        public string[] ExceptionFormats
        {
            get { return _exceptionFormats; }
        }

        /// <summary>
        /// Exposes the capabilitie's VendorSpecificCapabilities as XmlNode object. External modules 
        /// could use this to parse the vendor specific capabilities for their specific purpose.
        /// </summary>
        public XmlNode VendorSpecificCapabilities
        {
            get { return _vendorSpecificCapabilities; }
        }

        /// <summary>
        /// Gets the available GetMap request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetMapRequests
        {
            get { return _getMapRequests; }
        }

        /// <summary>
        /// Gets the hierarchical layer structure
        /// </summary>
        public WmsServerLayer Layer
        {
            get { return _layer; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IWebProxy Proxy
        {
            get { return _proxy;}
            set {_proxy = value;}
        }

        /// <summary>
        /// Gets or sets the base URL for the server without any OGC specific name=value pairs
        /// </summary>
        public string BaseUrl
        {
            get{return _baseUrl;}
            set
            {
                _capabilitiesUrl = CreateCapabilitiesUrl(_baseUrl);
                _baseUrl = value;
            }
        }

        /// <summary>
        /// Gets the entire XML document as text
        /// </summary>
        public string CapabilitiesUrl
        {
            get { return _capabilitiesUrl; }
        }

        /// <summary>
        /// Gets the entire XML document as text
        /// </summary>
        public string GetXmlAsText
        {
            get 
            {
                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);
                XmlDoc.WriteTo(xw);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Gets the entire XML document as byte[]
        /// </summary>
        public byte[] GetXmlAsByteArray
        {
            get
            {
                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);
                XmlDoc.WriteTo(xw);
                byte[] baData = System.Text.Encoding.UTF8.GetBytes(sw.ToString());
                return baData;
            }
        }

        ///<summary>
        ///</summary>
        public XmlDocument XmlDoc
        {
            get { return _xmlDoc; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Just instantiate, no parameters
        /// </summary>
        public Client() {}

        /// <summary>
        /// Initializes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        public Client(string url) 
            : this(url, null, 10000, null, "") { }

        /// <summary>
        /// This Initializes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="proxy">Proxy to use</param>
        public Client(string url, IWebProxy proxy)
            : this(url, proxy, 10000, null, "") { }

        /// <summary>
        /// This Initializes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut"></param>
        public Client(string url, IWebProxy proxy, int timeOut)
            : this(url, proxy, timeOut, null, "") { }

        /// <summary>
        /// Initializes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="credentials">Credentials for authenticating against remote WMS-server</param>
        public Client(string url, IWebProxy proxy, ICredentials credentials)
            : this(url, proxy, 10000, credentials, "") { }

        /// <summary>
        /// Initializes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="credentials">Credentials for authenticating against remote WMS-server</param>
        public Client(string url, IWebProxy proxy, int timeOut, ICredentials credentials)
            : this(url, proxy, timeOut, credentials, "") { }

        /// <summary>
        /// Initializes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="version"></param>
        public Client(string url, IWebProxy proxy, int timeOut, string version)
            : this(url, proxy, timeOut, null, version) { }

        /// <summary>
        /// Initializes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="version"></param>
        /// <param name="credentials"></param>
        public Client(string url, IWebProxy proxy, int timeOut, ICredentials credentials, string version)
        {
            _baseUrl = url;
            _proxy = proxy;
            _timeOut = timeOut;
            _version = version;
            _credentials = credentials;

            _capabilitiesUrl = CreateCapabilitiesUrl(url);
            _xmlDoc = GetRemoteXml();

            ParseVersion();
            ParseCapabilities();
        }


        /// <summary>
        /// Hydrates Client object based on byte array version of XML document
        /// </summary>
        /// <param name="byteXml">byte array version of capabilities document</param>
        public Client(byte[] byteXml)
        {
            Stream stream = new MemoryStream(byteXml);
            var r = new XmlTextReader(stream);
            r.XmlResolver = null;

            _xmlDoc = new XmlDocument();
            XmlDoc.XmlResolver = null;
            XmlDoc.Load(r);
            stream.Close();

            _nsmgr = new XmlNamespaceManager(XmlDoc.NameTable);

            _baseUrl = "";
            _proxy = null;
            _timeOut = 10000;
            _version = "";
            _credentials = null;

            ParseVersion();
            ParseCapabilities();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Downloads servicedescription from WMS service  
        /// </summary>
        /// <returns>XmlDocument from Url. Null if Url is empty or inproper XmlDocument</returns>
        public XmlDocument GetRemoteXml()
        {
            Stream stream = null;

            try
            {
                WebRequest myRequest = WebRequest.Create(_capabilitiesUrl);
                if (_credentials != null)
                    myRequest.Credentials = _credentials;

                myRequest.Timeout = _timeOut;
                if (_proxy != null) myRequest.Proxy = _proxy;
                WebResponse myResponse = myRequest.GetResponse();

                if (myResponse == null)
                    throw new ApplicationException("No web response");

                stream = myResponse.GetResponseStream();

                if (stream == null)
                    throw new ApplicationException("No response stream");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not download capabilities document from the server. The server may not be available right now." + ex.Message);
            }

            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(_capabilitiesUrl, stream);
                xmlTextReader.XmlResolver = null;

                _xmlDoc = new XmlDocument();
                _xmlDoc.XmlResolver = null;
                _xmlDoc.Load(xmlTextReader);
                _nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);
                return _xmlDoc;
            }
            catch (Exception /*ex*/)
            {
                throw new ApplicationException("Could not convert the capabilities file into an XML document. Do you have illegal characters in the document.");
            }
            finally
            {
                stream.Close();
            }

        }

        /// <summary>
        /// Parses a servicedescription and stores the data in the ServiceDescription property
        /// </summary>
        public void ParseCapabilities()
        {
            if (_xmlDoc.DocumentElement.Attributes["version"] != null)
            {
                _version = _xmlDoc.DocumentElement.Attributes["version"].Value;
                if (_version != "1.0.0" && _version != "1.1.0" && _version != "1.1.1" && _version != "1.3.0")
                    throw new ApplicationException("WMS Version " + _version + " is not currently supported");

                _nsmgr.AddNamespace(String.Empty, "http://www.opengis.net/wms");
                if (_version == "1.3.0")
                {
                    _nsmgr.AddNamespace("sm", "http://www.opengis.net/wms");
                }
                else
                    _nsmgr.AddNamespace("sm", "");

                _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            }
            else
                throw (new ApplicationException("No service version number was found in the capabilities XML file!"));

            XmlNode xnService = _xmlDoc.DocumentElement.SelectSingleNode("sm:Service", _nsmgr);
            XmlNode xnCapability = _xmlDoc.DocumentElement.SelectSingleNode("sm:Capability", _nsmgr);
            if (xnService != null)
                ParseServiceDescription(xnService);
            else
                throw (new ApplicationException("No service tag found in the capabilities XML file!"));


            if (xnCapability != null)
                ParseCapability(xnCapability);
            else
                throw (new ApplicationException("No capability tag found in the capabilities XML file!"));
        }

        /// <summary>
        /// Parses service description node
        /// </summary>
        /// <param name="xnlServiceDescription"></param>
        private void ParseServiceDescription(XmlNode xnlServiceDescription)
        {
            XmlNode node = xnlServiceDescription.SelectSingleNode("sm:Title", _nsmgr);
            _serviceDescription.Title = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:OnlineResource/@xlink:href", _nsmgr);
            _serviceDescription.OnlineResource = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:Abstract", _nsmgr);
            _serviceDescription.Abstract = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:Fees", _nsmgr);
            _serviceDescription.Fees = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:AccessConstraints", _nsmgr);
            _serviceDescription.AccessConstraints = (node != null ? node.InnerText : null);

            XmlNodeList xnlKeywords = xnlServiceDescription.SelectNodes("sm:KeywordList/sm:Keyword", _nsmgr);
            if (xnlKeywords != null)
            {
                _serviceDescription.Keywords = new string[xnlKeywords.Count];
                for (int i = 0; i < xnlKeywords.Count; i++)
                    ServiceDescription.Keywords[i] = xnlKeywords[i].InnerText;
            }
            //Contact information
            _serviceDescription.ContactInformation = new Capabilities.WmsContactInformation();
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:Address", _nsmgr);
            _serviceDescription.ContactInformation.Address.Address = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:AddressType",
                                                          _nsmgr);
            _serviceDescription.ContactInformation.Address.AddressType = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:City", _nsmgr);
            _serviceDescription.ContactInformation.Address.City = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:Country", _nsmgr);
            _serviceDescription.ContactInformation.Address.Country = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:PostCode", _nsmgr);
            _serviceDescription.ContactInformation.Address.PostCode = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactElectronicMailAddress", _nsmgr);
            _serviceDescription.ContactInformation.Address.StateOrProvince = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactElectronicMailAddress", _nsmgr);
            _serviceDescription.ContactInformation.ElectronicMailAddress = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactFacsimileTelephone", _nsmgr);
            _serviceDescription.ContactInformation.FacsimileTelephone = (node != null ? node.InnerText : null);
            node =
                xnlServiceDescription.SelectSingleNode(
                    "sm:ContactInformation/sm:ContactPersonPrimary/sm:ContactOrganisation", _nsmgr);
            _serviceDescription.ContactInformation.PersonPrimary.Organisation = (node != null ? node.InnerText : null);
            node =
                xnlServiceDescription.SelectSingleNode(
                    "sm:ContactInformation/sm:ContactPersonPrimary/sm:ContactPerson", _nsmgr);
            _serviceDescription.ContactInformation.PersonPrimary.Person = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactVoiceTelephone", _nsmgr);
            _serviceDescription.ContactInformation.VoiceTelephone = (node != null ? node.InnerText : null);
        }

        /// <summary>
        /// Parses capability node
        /// </summary>
        /// <param name="xnCapability"></param>
        private void ParseCapability(XmlNode xnCapability)
        {
            XmlNode xnRequest = xnCapability.SelectSingleNode("sm:Request", _nsmgr);
            if (xnRequest == null)
                throw (new Exception("Request parameter not specified in Service Description"));
            ParseRequest(xnRequest);
            XmlNode xnLayer = xnCapability.SelectSingleNode("sm:Layer", _nsmgr);
            if (xnLayer == null)
                throw (new Exception("No layer tag found in Service Description"));
            _layer = ParseLayer(xnLayer);

            XmlNode xnException = xnCapability.SelectSingleNode("sm:Exception", _nsmgr);
            if (xnException != null)
                ParseExceptions(xnException);

            _vendorSpecificCapabilities = xnCapability.SelectSingleNode("sm:VendorSpecificCapabilities", _nsmgr);
        }

        /// <summary>
        /// Parses valid exceptions
        /// </summary>
        /// <param name="xnlExceptionNode"></param>
        private void ParseExceptions(XmlNode xnlExceptionNode)
        {
            XmlNodeList xnlFormats = xnlExceptionNode.SelectNodes("sm:Format", _nsmgr);
            if (xnlFormats != null)
            {
                _exceptionFormats = new string[xnlFormats.Count];
                for (int i = 0; i < xnlFormats.Count; i++)
                {
                    _exceptionFormats[i] = xnlFormats[i].InnerText;
                }
            }
        }

        /// <summary>
        /// Parses request node
        /// </summary>
        /// <param name="xmlRequestNode"></param>
        private void ParseRequest(XmlNode xmlRequestNode)
        {
            //GetMap
            XmlNode xnGetMap = xmlRequestNode.SelectSingleNode("sm:GetMap", _nsmgr);
            ParseRequestTypeBlock(xnGetMap, ref _getMapRequests, ref _getMapOutputFormats);

            //GetCapabilities
            XmlNode xnGetCapabilities = xmlRequestNode.SelectSingleNode("sm:GetCapabilities", _nsmgr);
            ParseRequestTypeBlock(xnGetCapabilities, ref _getCapabilitiesRequests, ref _getCapabilitiesOutputFormats);

            //GetFeatureInfo
            XmlNode xnGetFeatureInfo = xmlRequestNode.SelectSingleNode("sm:GetFeatureInfo", _nsmgr);
            ParseRequestTypeBlock(xnGetFeatureInfo, ref _getFeatureInfoRequests, ref _getFeatureInfoOutputFormats);

            //DescribeLayer
            XmlNode xnDescribeLayer = xmlRequestNode.SelectSingleNode("sm:DescribeLayer", _nsmgr);
            ParseRequestTypeBlock(xnDescribeLayer, ref _describeLayerRequests, ref _describeLayerOutputFormats);

            //GetLegendGraphic
            XmlNode xnGetLegendGraphic = xmlRequestNode.SelectSingleNode("sm:GetLegendGraphic", _nsmgr);
            ParseRequestTypeBlock(xnGetLegendGraphic, ref _getLegendGraphicRequests, ref _getLegendGraphicOutputFormats);

            //GetStyles
            XmlNode xnGetStyles = xmlRequestNode.SelectSingleNode("sm:GetStyles", _nsmgr);
            ParseRequestTypeBlock(xnGetStyles, ref _getStylesRequests, ref _getStylesOutputFormats);

            //PutStyles
            XmlNode xnPutStyles = xmlRequestNode.SelectSingleNode("sm:PutStyles", _nsmgr);
            ParseRequestTypeBlock(xnPutStyles, ref _putStylesRequests, ref _putStylesOutputFormats);
        }

        /// <summary>
        /// Parses GetMap request nodes
        /// </summary>
        private void ParseRequestTypeBlock(XmlNode requestNodes, ref WmsOnlineResource[] requestResources, ref Collection<string> outputFormats )
        {
            if (requestNodes != null)
            {
                XmlNode xnlHttp = requestNodes.SelectSingleNode("sm:DCPType/sm:HTTP", _nsmgr);
                if (xnlHttp != null && xnlHttp.HasChildNodes)
                {
                    requestResources = new WmsOnlineResource[xnlHttp.ChildNodes.Count];
                    for (int i = 0; i < xnlHttp.ChildNodes.Count; i++)
                    {
                        WmsOnlineResource wor = new WmsOnlineResource();
                        wor.Type = xnlHttp.ChildNodes[i].Name;
                        XmlNode or = xnlHttp.ChildNodes[i].SelectSingleNode("sm:OnlineResource", _nsmgr);
                        if (or == null || or.Attributes == null)
                            throw new ApplicationException("Online resource not set");
                        wor.OnlineResource = or.Attributes["xlink:href"].InnerText;
                        requestResources[i] = wor;
                    }
                }
                outputFormats = new Collection<string>();

                XmlNodeList xnlFormats = requestNodes.SelectNodes("sm:Format", _nsmgr);
                if (xnlFormats != null)
                    for (int i = 0; i < xnlFormats.Count; i++)
                        outputFormats.Add(xnlFormats[i].InnerText);
            }
            else
            {
                requestResources = null;
                outputFormats = null;
            }
        }

        /// <summary>
        /// Iterates through the layer nodes recursively
        /// </summary>
        /// <param name="xmlLayer"></param>
        /// <returns></returns>
        private WmsServerLayer ParseLayer(XmlNode xmlLayer)
        {
            WmsServerLayer layer = new WmsServerLayer();
            XmlNode node = xmlLayer.SelectSingleNode("sm:Name", _nsmgr);
            layer.Name = (node != null ? node.InnerText : null);
            node = xmlLayer.SelectSingleNode("sm:Title", _nsmgr);
            layer.Title = (node != null ? node.InnerText : null);
            node = xmlLayer.SelectSingleNode("sm:Abstract", _nsmgr);
            layer.Abstract = (node != null ? node.InnerText : null);
            if (xmlLayer.Attributes != null)
            {
                XmlAttribute attr = xmlLayer.Attributes["queryable"];
                layer.Queryable = (attr != null && attr.InnerText == "1");
            }
            else
                layer.Queryable = false;
            layer.SRIDBoundingBoxes = new List<SpatialReferencedBoundingBox>();


            XmlNodeList xnlKeywords = xmlLayer.SelectNodes("sm:KeywordList/sm:Keyword", _nsmgr);
            if (xnlKeywords != null)
            {
                layer.Keywords = new string[xnlKeywords.Count];
                for (int i = 0; i < xnlKeywords.Count; i++)
                    layer.Keywords[i] = xnlKeywords[i].InnerText;
            }

            XmlNodeList xnlCrs = null;
            if(_version == "1.1.0" || _version == "1.1.1")
                xnlCrs = xmlLayer.SelectNodes("sm:SRS", _nsmgr); // <--I think this needs to be version specific
            else
                xnlCrs = xmlLayer.SelectNodes("sm:CRS", _nsmgr);

            if (xnlCrs != null)
            {
                layer.CRS = new string[xnlCrs.Count];
                for (int i = 0; i < xnlCrs.Count; i++)
                    layer.CRS[i] = xnlCrs[i].InnerText;
            }
            XmlNodeList xnlStyle = xmlLayer.SelectNodes("sm:Style", _nsmgr);
            if (xnlStyle != null)
            {
                layer.Style = new WmsLayerStyle[xnlStyle.Count];
                for (int i = 0; i < xnlStyle.Count; i++)
                {
                    node = xnlStyle[i].SelectSingleNode("sm:Name", _nsmgr);
                    layer.Style[i].Name = (node != null ? node.InnerText : null);
                    node = xnlStyle[i].SelectSingleNode("sm:Title", _nsmgr);
                    layer.Style[i].Title = (node != null ? node.InnerText : null);
                    node = xnlStyle[i].SelectSingleNode("sm:Abstract", _nsmgr);
                    layer.Style[i].Abstract = (node != null ? node.InnerText : null);
                    node = xnlStyle[i].SelectSingleNode("sm:LegendUrl", _nsmgr);
                    if (node != null && node.Attributes != null)
                    {
                        layer.Style[i].LegendUrl = new WmsStyleLegend();
                        layer.Style[i].LegendUrl.Size = new Size(
                            int.Parse(node.Attributes["width"].InnerText),
                            int.Parse(node.Attributes["height"].InnerText));
                        layer.Style[i].LegendUrl.OnlineResource.OnlineResource =
                            node.SelectSingleNode("sm:OnlineResource", _nsmgr).Attributes["xlink:href"].InnerText;
                        layer.Style[i].LegendUrl.OnlineResource.Type =
                            node.SelectSingleNode("sm:Format", _nsmgr).InnerText;
                    }
                    node = xnlStyle[i].SelectSingleNode("sm:StyleSheetURL", _nsmgr);
                    if (node != null)
                    {
                        layer.Style[i].StyleSheetUrl = new WmsOnlineResource();
                        layer.Style[i].StyleSheetUrl.OnlineResource =
                            node.SelectSingleNode("sm:OnlineResource", _nsmgr).Attributes["xlink:href"].InnerText;
                        //layer.Style[i].StyleSheetUrl.OnlineResource = node.SelectSingleNode("sm:Format", nsmgr).InnerText;
                    }
                }
            }
            XmlNodeList xnlLayers = xmlLayer.SelectNodes("sm:Layer", _nsmgr);
            if (xnlLayers != null)
            {
                layer.ChildLayers = new WmsServerLayer[xnlLayers.Count];
                for (int i = 0; i < xnlLayers.Count; i++)
                    layer.ChildLayers[i] = ParseLayer(xnlLayers[i]);
            }

            //LatLonBoundingBox is specific for WMS 1.1.1 servers    
            node = xmlLayer.SelectSingleNode("sm:LatLonBoundingBox", _nsmgr);
            if (node != null)
            {
                double minx = WebUtilities.ParseNodeAsDouble(node.Attributes["minx"], -180.0);
                double miny = WebUtilities.ParseNodeAsDouble(node.Attributes["miny"], -90.0);
                double maxx = WebUtilities.ParseNodeAsDouble(node.Attributes["maxx"], 180.0);
                double maxy = WebUtilities.ParseNodeAsDouble(node.Attributes["maxy"], 90.0);
                layer.LatLonBoundingBox = new Envelope(minx, maxx, miny, maxy);
            }
            else
            {
                //EX_GeographicBoundingBox is specific for WMS 1.3.0 servers   
                node = xmlLayer.SelectSingleNode("sm:EX_GeographicBoundingBox", _nsmgr);
                if (node != null)
                {
                    //EX_GeographicBoundingBox is specific for WMS1.3.0 servers so this will be parsed if LatLonBoundingBox is null
                    double minx = WebUtilities.ParseNodeAsDouble(node.SelectSingleNode("sm:westBoundLongitude", _nsmgr), -180.0);
                    double miny = WebUtilities.ParseNodeAsDouble(node.SelectSingleNode("sm:southBoundLatitude", _nsmgr), -90.0);
                    double maxx = WebUtilities.ParseNodeAsDouble(node.SelectSingleNode("sm:eastBoundLongitude", _nsmgr), 180.0);
                    double maxy = WebUtilities.ParseNodeAsDouble(node.SelectSingleNode("sm:northBoundLatitude", _nsmgr), 90.0);
                    layer.LatLonBoundingBox = new Envelope(minx, maxx, miny, maxy);
                }
                else
                {
                    //Not sure what to do in this case. PDD.
                    layer.LatLonBoundingBox = null;
                }
            }

            //if the layer has a specific spatial reference system, the boundingbox in this reference system should be parsed and placed in 
            //the SRIDboundingbox
            XmlNodeList bboxes = xmlLayer.SelectNodes("sm:BoundingBox", _nsmgr);
            foreach (XmlNode bbox in bboxes)
            {
                double minx;
                double miny;
                double maxx;
                double maxy;
                int epsg;

                if (!WebUtilities.TryParseNodeAsDouble(bbox.Attributes["minx"], out minx)) continue;
                if (!WebUtilities.TryParseNodeAsDouble(bbox.Attributes["miny"], out miny)) continue;
                if (!WebUtilities.TryParseNodeAsDouble(bbox.Attributes["maxx"], out maxx)) continue;
                if (!WebUtilities.TryParseNodeAsDouble(bbox.Attributes["maxy"], out maxy)) continue;
                if (!WebUtilities.TryParseNodeAsEpsg(WebUtilities.FindEpsgNode(bbox), out epsg)) continue; 
           
                layer.SRIDBoundingBoxes.Add(new SpatialReferencedBoundingBox(minx, miny, maxx, maxy, epsg));
            }
            return layer;
        }

        ///<summary>
        ///</summary>
        ///<exception cref="ApplicationException"></exception>
        public void ValidateXml()
        {
            try
            {
                  // Now create StringWriter object to get data from xml document.
                var sw = new StringWriter();
                var xw = new XmlTextWriter(sw);
                _xmlDoc.WriteTo(xw);

                var settings = new XmlReaderSettings();

                switch (_version)
                {
                    case "1.1.0":
                        //settings.ProhibitDtd = false;
                        settings.DtdProcessing = DtdProcessing.Parse;
                        settings.ValidationType = ValidationType.DTD;
                        break;
                    case "1.1.1":
                        //settings.ProhibitDtd = false;
                        settings.DtdProcessing = DtdProcessing.Parse;
                        settings.ValidationType = ValidationType.DTD;
                        break;
                    case "1.3.0":
                        settings.Schemas.Add("http://www.opengis.net/wms", "http://schemas.opengis.net/wms/1.3.0/capabilities_1_3_0.xsd");
                        settings.ValidationType = ValidationType.Schema;
                        break;
                    default:
                        if (logger.IsInfoEnabled)
                            logger.Info("Invalid selection: " + _version);
                        break;
                }

                settings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += new ValidationEventHandler(delegate(object sender, ValidationEventArgs args)
                       {
                           throw new ApplicationException("Could not validate the WMS capabilities file completely. Here is the validation error message..." + args.Message);
                       });

				//TextReader, XMLReader, URL, Stream
                var xmlStream = (TextReader)new StringReader(sw.ToString());
                var reader = XmlReader.Create(xmlStream, settings);
                while (reader.Read());
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not validate the WMS capabilities file for this version of the capabilities file. Here is the validation error message..." + ex.Message, ex);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string CreateCapabilitiesUrl(string url)
        {
            var strReq = new StringBuilder(url);
            if (!url.Contains("?"))
                strReq.Append("?");
            if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
                strReq.Append("&");
            if (!url.ToLower().Contains("service=wms"))
                strReq.AppendFormat("SERVICE=WMS&");
            if (!url.ToLower().Contains("request=getcapabilities"))
                strReq.AppendFormat("REQUEST=GetCapabilities&");

			//If version is NOT set at this point then add to query string
            if(_version != "")
                strReq.AppendFormat("VERSION=" + _version + "&");



            return strReq.ToString();
        }

        /// <summary>
        /// Just parse the version number and add to the version property
        /// </summary>
        public void ParseVersion()
        {
            if (_xmlDoc.DocumentElement != null)
            {
            if (_xmlDoc.DocumentElement.Attributes["version"] != null)
            {
                _version = _xmlDoc.DocumentElement.Attributes["version"].Value;
                if (_version != "1.0.0" && _version != "1.1.0" && _version != "1.1.1" && _version != "1.3.0")
                    throw new ApplicationException("WMS Version " + _version + " is not currently supported");
            }
            else
                throw (new ApplicationException("No service version number was found in the capabilities XML file!"));
            }
        }

        #endregion

    }
}