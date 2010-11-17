using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using SharpMap.Geometries;

namespace SharpMap.Web.Wms
{
    /// <summary>
    /// Class for requesting and parsing a WMS servers capabilities
    /// </summary>
    [Serializable]
    public class Client
    {
        private XmlNode _vendorSpecificCapabilities;
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
            public BoundingBox LatLonBoundingBox;

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
        private WmsServerLayer _layer;
        private Capabilities.WmsServiceDescription _serviceDescription;

        private string _wmsVersion;

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
        public string WmsVersion
        {
            get { return _wmsVersion; }
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
        /// Gets the available GetMap request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetMapRequests
        {
            get { return _getMapRequests; }
        }

        /// <summary>
        /// Gets the hiarchial layer structure
        /// </summary>
        public WmsServerLayer Layer
        {
            get { return _layer; }
        }

        #endregion

        /// <summary>
        /// Initalizes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        public Client(string url)
            : this(url, null)
        {
        }

        /// <summary>
        /// Initalizes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="proxy">Proxy to use</param>
        public Client(string url, WebProxy proxy)
        {
            StringBuilder strReq = new StringBuilder(url);
            if (!url.Contains("?"))
                strReq.Append("?");
            if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
                strReq.Append("&");
            if (!url.ToLower().Contains("service=wms"))
                strReq.AppendFormat("SERVICE=WMS&");
            if (!url.ToLower().Contains("request=getcapabilities"))
                strReq.AppendFormat("REQUEST=GetCapabilities&");

            XmlDocument xml = GetRemoteXml(strReq.ToString(), proxy);
            ParseCapabilities(xml);
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
        /// Downloads servicedescription from WMS service  
        /// </summary>
        /// <returns>XmlDocument from Url. Null if Url is empty or inproper XmlDocument</returns>
        private XmlDocument GetRemoteXml(string url, WebProxy proxy)
        {
            try
            {
                WebRequest myRequest = WebRequest.Create(url);
                if (proxy != null) myRequest.Proxy = proxy;

                WebResponse myResponse = myRequest.GetResponse();
                if (myResponse == null)
                    throw new ApplicationException("No web response");

                Stream stream = myResponse.GetResponseStream();
                if (stream == null)
                    throw new ApplicationException("No response stream");

                XmlTextReader r = new XmlTextReader(url, stream);
                r.XmlResolver = null;
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.Load(r);
                stream.Close();
                _nsmgr = new XmlNamespaceManager(doc.NameTable);
                return doc;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not download capabilities", ex);
            }
        }


        /// <summary>
        /// Parses a servicedescription and stores the data in the ServiceDescription property
        /// </summary>
        /// <param name="doc">XmlDocument containing a valid Service Description</param>
        private void ParseCapabilities(XmlDocument doc)
        {
            if (doc.DocumentElement == null)
                throw new ApplicationException("No document element found");

            if (doc.DocumentElement.Attributes["version"] != null)
            {
                _wmsVersion = doc.DocumentElement.Attributes["version"].Value;
                if (_wmsVersion != "1.0.0" && _wmsVersion != "1.1.0" && _wmsVersion != "1.1.1" && _wmsVersion != "1.3.0")
                    throw new ApplicationException("WMS Version " + _wmsVersion + " not supported");

                _nsmgr.AddNamespace(String.Empty, "http://www.opengis.net/wms");
                _nsmgr.AddNamespace("sm", _wmsVersion == "1.3.0" ? "http://www.opengis.net/wms" : "");
                _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            }
            else
                throw (new ApplicationException("No service version number found!"));

            XmlNode xnService = doc.DocumentElement.SelectSingleNode("sm:Service", _nsmgr);
            XmlNode xnCapability = doc.DocumentElement.SelectSingleNode("sm:Capability", _nsmgr);
            if (xnService != null)
                ParseServiceDescription(xnService);
            else
                throw (new ApplicationException("No service tag found!"));


            if (xnCapability != null)
                ParseCapability(xnCapability);
            else
                throw (new ApplicationException("No capability tag found!"));
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
            XmlNode xnGetMap = xmlRequestNode.SelectSingleNode("sm:GetMap", _nsmgr);
            ParseGetMapRequest(xnGetMap);
            //TODO:
            //XmlNode xnGetFeatureInfo = xmlRequestNodes.SelectSingleNode("sm:GetFeatureInfo", nsmgr);
            //XmlNode xnCapa = xmlRequestNodes.SelectSingleNode("sm:GetCapabilities", nsmgr); <-- We don't really need this do we?			
        }

        /// <summary>
        /// Parses GetMap request nodes
        /// </summary>
        /// <param name="getMapRequestNodes"></param>
        private void ParseGetMapRequest(XmlNode getMapRequestNodes)
        {
            XmlNode xnlHttp = getMapRequestNodes.SelectSingleNode("sm:DCPType/sm:HTTP", _nsmgr);
            if (xnlHttp != null && xnlHttp.HasChildNodes)
            {
                _getMapRequests = new WmsOnlineResource[xnlHttp.ChildNodes.Count];
                for (int i = 0; i < xnlHttp.ChildNodes.Count; i++)
                {
                    WmsOnlineResource wor = new WmsOnlineResource();
                    wor.Type = xnlHttp.ChildNodes[i].Name;
                    XmlNode or = xnlHttp.ChildNodes[i].SelectSingleNode("sm:OnlineResource", _nsmgr);
                    if (or == null || or.Attributes == null)
                        throw new ApplicationException("Online resource not set");
                    wor.OnlineResource = or.Attributes["xlink:href"].InnerText;
                    _getMapRequests[i] = wor;
                }
            }
            _getMapOutputFormats = new Collection<string>();

            XmlNodeList xnlFormats = getMapRequestNodes.SelectNodes("sm:Format", _nsmgr);
            if (xnlFormats != null)
                for (int i = 0; i < xnlFormats.Count; i++)
                    _getMapOutputFormats.Add(xnlFormats[i].InnerText);
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
            XmlNodeList xnlCrs = xmlLayer.SelectNodes("sm:CRS", _nsmgr);
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
                double minx = ParseNodeAsDouble(node.Attributes["minx"], -180.0);
                double miny = ParseNodeAsDouble(node.Attributes["miny"], -90.0);
                double maxx = ParseNodeAsDouble(node.Attributes["maxx"], 180.0);
                double maxy = ParseNodeAsDouble(node.Attributes["maxy"], 90.0);
                layer.LatLonBoundingBox = new BoundingBox(minx, miny, maxx, maxy);
            }
            else
            {
                //EX_GeographicBoundingBox is specific for WMS 1.3.0 servers   
                node = xmlLayer.SelectSingleNode("sm:EX_GeographicBoundingBox", _nsmgr);
                if (node != null)
                {
                    //EX_GeographicBoundingBox is specific for WMS1.3.0 servers so this will be parsed if LatLonBoundingBox is null
                    double minx = ParseNodeAsDouble(node.SelectSingleNode("sm:westBoundLongitude", _nsmgr), -180.0);
                    double miny = ParseNodeAsDouble(node.SelectSingleNode("sm:southBoundLatitude", _nsmgr), -90.0);
                    double maxx = ParseNodeAsDouble(node.SelectSingleNode("sm:eastBoundLongitude", _nsmgr), 180.0);
                    double maxy = ParseNodeAsDouble(node.SelectSingleNode("sm:northBoundLatitude", _nsmgr), 90.0);
                    layer.LatLonBoundingBox = new BoundingBox(minx, miny, maxx, maxy);
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

                if (!TryParseNodeAsDouble(bbox.Attributes["minx"], out minx)) continue;
                if (!TryParseNodeAsDouble(bbox.Attributes["miny"], out miny)) continue;
                if (!TryParseNodeAsDouble(bbox.Attributes["maxx"], out maxx)) continue;
                if (!TryParseNodeAsDouble(bbox.Attributes["maxy"], out maxy)) continue;
                if (!TryParseNodeAsEpsg(FindEpsgNode(bbox), out epsg)) continue; 
           
                layer.SRIDBoundingBoxes.Add(new SpatialReferencedBoundingBox(minx, miny, maxx, maxy, epsg));
            }
            return layer;
        }

        private static XmlNode FindEpsgNode(XmlNode bbox)
        {
            if (bbox == null || bbox.Attributes == null)
                throw new ArgumentNullException("bbox");

            XmlNode epsgNode = ((bbox.Attributes["srs"] ?? bbox.Attributes["crs"]) ?? bbox.Attributes["SRS"]) ??
                               bbox.Attributes["CRS"];
            return epsgNode;
        }

        private static bool TryParseNodeAsEpsg(XmlNode node, out int epsg)
        {
            epsg = default(int);
            if (node == null) return false;
            string epsgString = node.Value;
            if (String.IsNullOrEmpty(epsgString)) return false;
            const string prefix = "EPSG:";
            int index = epsgString.IndexOf(prefix);
            if (index < 0) return false;
            return (int.TryParse(epsgString.Substring(index + prefix.Length), NumberStyles.Any, Map.NumberFormatEnUs, out epsg));
        }

        private static double ParseNodeAsDouble(XmlNode node, double defaultValue)
        {
            if (node == null) return defaultValue;
            if (String.IsNullOrEmpty(node.InnerText)) return defaultValue;
            double value;
            if (Double.TryParse(node.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out value))
                return value;
            return defaultValue;
        }

        private static bool TryParseNodeAsDouble(XmlNode node, out double value)
        {
            value = default(double);
            if (node == null) return false;
            if (String.IsNullOrEmpty(node.InnerText)) return false;
            return Double.TryParse(node.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out value);
        }
    }
}