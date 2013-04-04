using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace SharpMap.Web.Wfs
{
    /// <summary>
    /// Class for requesting and parsing a WFS servers capabilities
    /// </summary>
    [Serializable]
    public class Client : IClient
    {
        private XmlNamespaceManager _nsmgr;

        #region WFS Data structures

        #endregion

        #region Properties

        private string _version;
        private XmlDocument _xmlDoc;
        private string _baseUrl;
        private string _capabilitiesUrl;
        private IWebProxy _proxy;
        private int _timeOut;
        private ICredentials _credentials;
        private XmlNode _vendorSpecificCapabilities;

        /// <summary>
        /// Exposes the capabilities VendorSpecificCapabilities as XmlNode object. External modules 
        /// could use this to parse the vendor specific capabilities for their specific purpose.
        /// </summary>
        public XmlNode VendorSpecificCapabilities
        {
            get { return _vendorSpecificCapabilities; }
        }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Defaults to 10 seconds
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
        /// 
        /// </summary>
        public IWebProxy Proxy
        {
            get{return _proxy;}
            set{_proxy = value;}
        }

        /// <summary>
        /// Gets or sets the base URL for the server without any OGC specific name=value pairs
        /// </summary>
        public string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
            set
            {
                _baseUrl = value;
                _capabilitiesUrl = CreateCapabilitiesUrl(_baseUrl);
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

        private Capabilities.WfsServiceIdentification _serviceIdentification;
        /// <summary>
        /// Gets the service description
        /// </summary>
        public Capabilities.WfsServiceIdentification ServiceIdentification
        {
            get { return _serviceIdentification; }
        }

        private Capabilities.WfsServiceProvider _serviceProvider;
        /// <summary>
        /// Gets the service provider
        /// </summary>
        public Capabilities.WfsServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        /// <summary>
        /// Gets the version of the WFS server (ex. "1.1.0")
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        private string[] _exceptionFormats;
        /// <summary>
        /// Gets a list of available exception mime type formats
        /// </summary>
        public string[] ExceptionFormats
        {
            get
            {
                return _exceptionFormats;
            }
        }

        /// <summary>
        /// Gets the capabilities information as <see cref="XmlDocument"/>
        /// </summary>
        public XmlDocument XmlDoc
        {
            get { return _xmlDoc; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Just instantiate, no parameters
        /// </summary>
        public Client() { }

        /// <summary>
        /// Initializes WFS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wfs server</param>
        public Client(string url)
            : this(url, null, 10000, null, "") { }

        /// <summary>
        /// This Initializes WFS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wfs server</param>
        /// <param name="proxy">Proxy to use</param>
        public Client(string url, IWebProxy proxy)
            : this(url, proxy, 10000, null, "") { }

        /// <summary>
        /// This Initializes WFS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wfs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        public Client(string url, IWebProxy proxy, int timeOut)
            : this(url, proxy, timeOut, null, "") { }

        /// <summary>
        /// Initializes WFS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wfs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="credentials">Credentials for authenticating against remote WFS-server</param>
        public Client(string url, IWebProxy proxy, ICredentials credentials)
            : this(url, proxy, 10000, credentials, "") { }

        /// <summary>
        /// Initializes WFS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wfs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="credentials">Credentials for authenticating against remote WFS-server</param>
        public Client(string url, IWebProxy proxy, int timeOut, ICredentials credentials)
            : this(url, proxy, timeOut, credentials, "") { }

        /// <summary>
        /// Initializes WFS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wfs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="version"></param>
        public Client(string url, IWebProxy proxy, int timeOut, string version)
            : this(url, proxy, timeOut, null, version) { }

        /// <summary>
        /// Initializes WFS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wfs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="version"></param>
        /// <param name="credentials">Credentials for authenticating against remote WFS-server</param>
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
            if (!url.ToLower().Contains("service=wfs"))
                strReq.AppendFormat("SERVICE=WFS&");
            if (!url.ToLower().Contains("request=getcapabilities"))
                strReq.AppendFormat("REQUEST=GetCapabilities&");

            if (_version != "")
                strReq.AppendFormat("VERSION=" + _version + "&");

            return strReq.ToString();
        }

        /// <summary>
        /// Downloads servicedescription from WFS service  
        /// </summary>
        /// <returns>XmlDocument from Url. Null if Url is empty or inproper XmlDocument</returns>
        public XmlDocument GetRemoteXml()
        {
            Stream stream;

            try
            {
                var myRequest = WebRequest.Create(_capabilitiesUrl);
                myRequest.Timeout = _timeOut;
                if (_proxy != null) myRequest.Proxy = _proxy;
                using (var myResponse = myRequest.GetResponse())
                {
                    stream = myResponse.GetResponseStream();
                }

                if (stream == null)
                        throw new ApplicationException("No response stream");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not download capabilities document from the server. The server may not be available right now." + ex.Message);
            }

            try
            {
                var xmlTextReader = new XmlTextReader(_capabilitiesUrl, stream);
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
        /// Method to parse the web-server's version
        /// </summary>
        public void ParseVersion()
        {
            var doc = XmlDoc.DocumentElement;
            if (doc == null)
                throw new InvalidOperationException("Could not get DocumentElement");
            
            if (doc.Attributes["version"] != null)
            {
                _version = doc.Attributes["version"].Value;
                if (_version != "1.0.0" && _version != "1.1.0" && _version != "2.0.0")
                    throw new ApplicationException("WFS Version " + _version + " is not currently supported");
            }
            else
                throw (new ApplicationException("No service version number was found in the WFS capabilities XML file!"));

        }

        /// <summary>
        /// Parses a servicedescription and stores the data in the ServiceIdentification property
        /// </summary>
        public void ParseCapabilities()
        {
            if(_xmlDoc == null)
            {
                throw (new ApplicationException("A valid WFS capabilities XML file was not loaded!"));   
            }

            var documentElement = _xmlDoc.DocumentElement;
            if (documentElement == null)
                throw (new ApplicationException("A valid WFS capabilities XML file was not loaded!"));

            switch (_version)
            {
                case "1.0.0":
                    if (documentElement.Attributes["version"] != null)
                    {
                        _nsmgr.AddNamespace(String.Empty, "http://www.opengis.net/wfs");
                        _nsmgr.AddNamespace("sm", "");
                        _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                        _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    }
                    else
                        throw (new ApplicationException("No service version number found!"));

                    var xnService = documentElement.SelectSingleNode("sm:Service", _nsmgr);
                    if (xnService != null)
                        ParseService(xnService);
                    else
                        throw (new ApplicationException("No service tag found!"));

                    //XmlNode xnCapability = doc.DocumentElement.SelectSingleNode("sm:Capability", _nsmgr);
                    break;

                case "1.1.0":
                case "2.0.0":
                    if (documentElement.Attributes["version"] != null)
                    {
                        _version = documentElement.Attributes["version"].Value;
                        if (_version != "1.1.0" && _version != "2.0.0")
                            throw new ApplicationException("WFS Version " + _version + " is not currently supported");

                        _nsmgr.AddNamespace(String.Empty, "http://www.opengis.net/wfs");
                        _nsmgr.AddNamespace("ows", "http://www.opengis.net/ows");
                        _nsmgr.AddNamespace("ogc", "http://www.opengis.net/ogc");
                        _nsmgr.AddNamespace("wfs", "http://www.opengis.net/wfs");
                        _nsmgr.AddNamespace("gml", "http://www.opengis.net/gml");
                        _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                        _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    }
                    else
                        throw (new ApplicationException(
                            "No service version number was found in the WFS capabilities XML file!"));

                    var xnServiceIdentification = documentElement.SelectSingleNode("ows:ServiceIdentification", _nsmgr);

                    if (xnServiceIdentification != null)
                        ParseServiceIdentification(xnServiceIdentification);
                    else
                        throw (new ApplicationException(
                            "No ServiceIdentification tag found in the capabilities XML file!"));

                    var xnServiceProvider = documentElement.SelectSingleNode("ows:ServiceProvider", _nsmgr);

                    if (xnServiceProvider != null)
                        ParseServiceProvider(xnServiceProvider);
                    else
                        throw (new ApplicationException("No service tag found in the capabilities XML file!"));
                    break;
                default:
                    throw new ApplicationException("Invalid Version");

            }

            /*
            if (_version == "1.0.0")
            {
                #region Version 1.0.0

                if (_xmlDoc.DocumentElement.Attributes["version"] != null)
                {
                    _nsmgr.AddNamespace(String.Empty, "http://www.opengis.net/wfs");
                    _nsmgr.AddNamespace("sm", "");
                    _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                    _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                }
                else
                    throw (new ApplicationException("No service version number found!"));

                XmlNode xnService = _xmlDoc.DocumentElement.SelectSingleNode("sm:Service", _nsmgr);
                //XmlNode xnCapability = doc.DocumentElement.SelectSingleNode("sm:Capability", _nsmgr);
                if (xnService != null)
                    ParseService(xnService);
                else
                    throw (new ApplicationException("No service tag found!"));


                //if (xnCapability != null)
                //    ParseCapability(xnCapability);
                //else
                //    throw (new ApplicationException("No capability tag found!"));

                #endregion
            }
            else
            {
                #region Versions 1.1.0 and 2.0.0

                if (_xmlDoc.DocumentElement.Attributes["version"] != null)
                {
                    _version = _xmlDoc.DocumentElement.Attributes["version"].Value;
                    if (_version != "1.1.0" && _version != "2.0.0")
                        throw new ApplicationException("WFS Version " + _version + " is not currently supported");

                    _nsmgr.AddNamespace(String.Empty, "http://www.opengis.net/wfs");
                    _nsmgr.AddNamespace("ows", "http://www.opengis.net/ows");
                    _nsmgr.AddNamespace("ogc", "http://www.opengis.net/ogc");
                    _nsmgr.AddNamespace("wfs", "http://www.opengis.net/wfs");
                    _nsmgr.AddNamespace("gml", "http://www.opengis.net/gml");
                    _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                    _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                }
                else
                    throw (new ApplicationException("No service version number was found in the WFS capabilities XML file!"));

                XmlNode xnServiceIdentification = _xmlDoc.DocumentElement.SelectSingleNode("ows:ServiceIdentification", _nsmgr);

                if (xnServiceIdentification != null)
                    ParseServiceIdentification(xnServiceIdentification);
                else
                    throw (new ApplicationException("No ServiceIdentification tag found in the capabilities XML file!"));

                XmlNode xnServiceProvider = XmlDoc.DocumentElement.SelectSingleNode("ows:ServiceProvider", _nsmgr);

                if (xnServiceProvider != null)
                    ParseServiceProvider(xnServiceProvider);
                else
                    throw (new ApplicationException("No service tag found in the capabilities XML file!"));

                //XmlNode xnCapability = XmlDoc.DocumentElement.SelectSingleNode("sm:Capability", _nsmgr);
                //if (xnCapability != null)
                //    ParseCapability(xnCapability);
                //else
                //    throw (new ApplicationException("No capability tag found in the capabilities XML file!"));   

                #endregion
            }
             */
            _vendorSpecificCapabilities = documentElement.SelectSingleNode("sm:VendorSpecificCapabilities");
        }

        /// <summary>
        /// Parses service description node
        /// </summary>
        /// <param name="xnlService"></param>
        private void ParseService(XmlNode xnlService)
        {
            XmlNode node = xnlService.SelectSingleNode("sm:Title", _nsmgr);
            _serviceIdentification.Title = (node != null ? node.InnerText : null);
            node = xnlService.SelectSingleNode("sm:Abstract", _nsmgr);
            _serviceIdentification.Abstract = (node != null ? node.InnerText : null);
            node = xnlService.SelectSingleNode("sm:Fees", _nsmgr);
            _serviceIdentification.Fees = (node != null ? node.InnerText : null);
            node = xnlService.SelectSingleNode("sm:AccessConstraints", _nsmgr);
            _serviceIdentification.AccessConstraints = (node != null ? node.InnerText : null);

            XmlNodeList xnlKeywords = xnlService.SelectNodes("sm:KeywordList/sm:Keyword", _nsmgr);
            if (xnlKeywords != null)
            {
                _serviceIdentification.Keywords = new string[xnlKeywords.Count];
                for (int i = 0; i < xnlKeywords.Count; i++)
                    _serviceIdentification.Keywords[i] = xnlKeywords[i].InnerText;
            }
        }

        /// <summary>
        /// Parses service description node
        /// </summary>
        /// <param name="xnlServiceId"></param>
        private void ParseServiceIdentification(XmlNode xnlServiceId)
        {
            XmlNode node = xnlServiceId.SelectSingleNode("ows:Title", _nsmgr);
            _serviceIdentification.Title = (node != null ? node.InnerText : null);
            node = xnlServiceId.SelectSingleNode("ows:Abstract", _nsmgr);
            _serviceIdentification.Abstract = (node != null ? node.InnerText : null);
            XmlNodeList xnlKeywords = xnlServiceId.SelectNodes("ows:Keywords/ows:Keyword", _nsmgr);
            if (xnlKeywords != null)
            {
                _serviceIdentification.Keywords = new string[xnlKeywords.Count];
                for (int i = 0; i < xnlKeywords.Count; i++)
                    _serviceIdentification.Keywords[i] = xnlKeywords[i].InnerText;
            }
            node = xnlServiceId.SelectSingleNode("ows:ServiceType", _nsmgr);
            _serviceIdentification.ServiceType = (node != null ? node.InnerText : null);
            node = xnlServiceId.SelectSingleNode("ows:ServiceTypeVersion", _nsmgr);
            _serviceIdentification.ServiceTypeVersion = (node != null ? node.InnerText : null);
            node = xnlServiceId.SelectSingleNode("ows:Fees", _nsmgr);
            _serviceIdentification.Fees = (node != null ? node.InnerText : null);
            node = xnlServiceId.SelectSingleNode("ows:AccessConstraints", _nsmgr);
            _serviceIdentification.AccessConstraints = (node != null ? node.InnerText : null);

        }

        private void ParseServiceProvider(XmlNode xnlServiceProvider)
        {
            XmlNode node = xnlServiceProvider.SelectSingleNode("ows:ProviderName", _nsmgr);
            _serviceProvider.ProviderName = (node != null ? node.InnerText : null);
            node = xnlServiceProvider.SelectSingleNode("ows:ProviderSite", _nsmgr);
            _serviceProvider.ProviderSite = (node != null ? node.InnerText : null);
            XmlNode nodeServiceContact = xnlServiceProvider.SelectSingleNode("ows:ServiceContact", _nsmgr);
            
            if(nodeServiceContact != null)
            {
                XmlNode node2 = nodeServiceContact.SelectSingleNode("ows:IndividualName", _nsmgr);
                _serviceProvider.ServiceContactDetail.IndividualName = (node2 != null ? node2.InnerText : null);
                node2 = node2.SelectSingleNode("ows:PositionName", _nsmgr);
                _serviceProvider.ServiceContactDetail.PositionName = (node2 != null ? node2.InnerText : null);

                XmlNode nodeContactInfo = xnlServiceProvider.SelectSingleNode("ows:ContactInfo", _nsmgr);

                if(nodeContactInfo != null)
                {
                    XmlNode nodePhone = nodeContactInfo.SelectSingleNode("ows:Phone", _nsmgr);
                    XmlNode nodeAddress = nodeContactInfo.SelectSingleNode("ows:Address", _nsmgr);

                    if(nodePhone != null)
                    {
                        XmlNode node4 = nodePhone.SelectSingleNode("ows:Voice", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.Telephone.Voice = (node4 != null ? node4.InnerText : null);
                        node4 = node.SelectSingleNode("ows:Facsimile", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.Telephone.Facsimile = (node4 != null ? node4.InnerText : null);
                    }

                    if(nodeAddress != null)
                    {
                        XmlNode node5 = nodeAddress.SelectSingleNode("ows:DeliveryPoint", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.AddressDetails.DeliveryPoint = (node5 != null ? node5.InnerText : null);
                        node5 = nodeAddress.SelectSingleNode("ows:City", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.AddressDetails.City = (node5 != null ? node5.InnerText : null);
                        node5 = nodeAddress.SelectSingleNode("ows:AdministrativeArea", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.AddressDetails.AdministrativeArea = (node5 != null ? node5.InnerText : null);
                        node5 = nodeAddress.SelectSingleNode("ows:PostalCode", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.AddressDetails.PostalCode = (node5 != null ? node5.InnerText : null);
                        node5 = nodeAddress.SelectSingleNode("ows:Country", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.AddressDetails.Country= (node5 != null ? node5.InnerText : null);
                        node5 = nodeAddress.SelectSingleNode("ows:ElectronicMailAddress", _nsmgr);
                        _serviceProvider.ServiceContactDetail.ContactInformation.AddressDetails.ElectronicMailAddress = (node5 != null ? node5.InnerText : null);
                    }

                    XmlNode node6 = nodeContactInfo.SelectSingleNode("sm:OnlineResource/@xlink:href", _nsmgr);
                    _serviceProvider.ServiceContactDetail.ContactInformation.OnlineResource = (node6 != null ? node6.InnerText : null);
                    node6 = nodeContactInfo.SelectSingleNode("ows:HoursOfService", _nsmgr);
                    _serviceProvider.ServiceContactDetail.ContactInformation.HoursOfService = (node6 != null ? node6.InnerText : null);
                    node6 = nodeContactInfo.SelectSingleNode("ows:ContactInstructions", _nsmgr);
                    _serviceProvider.ServiceContactDetail.ContactInformation.ContactInstructions = (node6 != null ? node6.InnerText : null);
                }

                node2 = nodeServiceContact.SelectSingleNode("ows:Role", _nsmgr);
                _serviceProvider.ServiceContactDetail.Role = (node2 != null ? node2.InnerText : null);
            }
        }

        /// <summary>
        /// Method to validate the web server's response
        /// </summary>
        public void ValidateXml()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
