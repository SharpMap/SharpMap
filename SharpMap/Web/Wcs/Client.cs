using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace SharpMap.Web.Wcs
{
    /// <summary>
    /// Web Coverage Service Client class
    /// </summary>
    public class Client : IClient
    {
        private XmlNamespaceManager _nsmgr;

        #region WCS Data structures


        #endregion

        #region Properties

        private string _version;
        private XmlDocument _xmlDoc;
        private string _baseUrl;
        private string _capabilitiesUrl;
        private IWebProxy _proxy;
        private int _timeOut;
        private ICredentials _credentials;
        private string[] _exceptionFormats;

        //private XmlNode _vendorSpecificCapabilities;

        /// <summary>
        /// Exposes the capabilities' VendorSpecificCapabilities as XmlNode object. External modules 
        /// could use this to parse the vendor specific capabilities for their specific purpose.
        /// </summary>
        public XmlNode VendorSpecificCapabilities
        {
            get 
            {
                return null;// _vendorSpecificCapabilities; 
            }
        }

        /// <summary>
        /// Timeout of <see cref="WebRequest"/> in milliseconds. Defaults to 10 seconds
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        ///<summary>
        /// Gets or sets the credentials used to access the Web coverage service
        ///</summary>
        public ICredentials Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        /// <summary>
        /// Gets or sets the proxy used to establish the connection to the web coverage service
        /// </summary>
        public IWebProxy Proxy
        {
            get
            {
                return _proxy;
            }
            set
            {

                _proxy = value;
            }
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
                using (var sw = new StringWriter())
                {
                    var xw = new XmlTextWriter(sw);
                    XmlDoc.WriteTo(xw);
                    return sw.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the entire XML document as byte[]
        /// </summary>
        public byte[] GetXmlAsByteArray
        {
            get
            {
                using (var sw = new StringWriter())
                {
                    var xw = new XmlTextWriter(sw);
                    XmlDoc.WriteTo(xw);
                    var baData = Encoding.UTF8.GetBytes(sw.ToString());
                    return baData;
                }
            }
        }

        /// <summary>
        /// Gets the version of the WCS server (ex. "1.0.0")
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Gets a list of available exception mime type formats
        /// </summary>
        public string[] ExceptionFormats
        {
            get { return _exceptionFormats; }
        }

        /// <summary>
        /// Gets the full capabilities xml document
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
        /// Initializes client to WCS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wcs server</param>
        public Client(string url)
            : this(url, null, 10000, null, "") { }

        /// <summary>
        /// Initializes client to WCS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wcs server</param>
        /// <param name="proxy">Proxy to use</param>
        public Client(string url, IWebProxy proxy)
            : this(url, proxy, 10000, null, "") { }

        /// <summary>
        /// Initializes client to WCS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wcs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut"></param>
        public Client(string url, IWebProxy proxy, int timeOut)
            : this(url, proxy, timeOut, null, "") { }

        /// <summary>
        /// Initializes client to WCS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wcs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="credentials">Credentials for authenticating against remote WCS-server</param>
        public Client(string url, IWebProxy proxy, ICredentials credentials)
            : this(url, proxy, 10000, credentials, "") { }

        /// <summary>
        /// Initializes client to WCS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wcs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="credentials">Credentials for authenticating against remote WCS-server</param>
        public Client(string url, IWebProxy proxy, int timeOut, ICredentials credentials)
            : this(url, proxy, timeOut, credentials, "") { }

        /// <summary>
        /// Initializes client to WCS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wcs server</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeOut">Web request timeout</param>
        /// <param name="version"></param>
        public Client(string url, IWebProxy proxy, int timeOut, string version)
            : this(url, proxy, timeOut, null, version) { }

        /// <summary>
        /// Initializes client to WCS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wcs server</param>
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
            using (var stream = new MemoryStream(byteXml))
            {
                var r = new XmlTextReader(stream);
                r.XmlResolver = null;

                _xmlDoc = new XmlDocument();
                XmlDoc.XmlResolver = null;
                XmlDoc.Load(r);
            }
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
        /// Function to create a Capabilities Url
        /// </summary>
        /// <param name="url">The base url</param>
        /// <returns>A capabilities url</returns>
        public string CreateCapabilitiesUrl(string url)
        {
            var strReq = new StringBuilder(url);
            if (!url.Contains("?"))
                strReq.Append("?");
            if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
                strReq.Append("&");
            if (!url.ToLower().Contains("service=wcs"))
                strReq.AppendFormat("SERVICE=WCS&");
            if (!url.ToLower().Contains("request=getcapabilities"))
                strReq.AppendFormat("REQUEST=GetCapabilities&");

            if (_version != "")
                strReq.AppendFormat("VERSION=" + _version + "&");

            return strReq.ToString();
        }

        /// <summary>
        /// Parses a GetCapabilities request from an XMLDoc
        /// </summary>
        public void ParseCapabilities()
        {
            if (_xmlDoc == null)
            {
                throw (new ApplicationException("A valid WCS capabilities XML file was not loaded!"));
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Downloads servicedescription from WCS service  
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
                using (var xmlTextReader = new XmlTextReader(_capabilitiesUrl, stream))
                {
                    xmlTextReader.XmlResolver = null;

                    _xmlDoc = new XmlDocument();
                    _xmlDoc.XmlResolver = null;
                    _xmlDoc.Load(xmlTextReader);
                    _nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);
                }
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
        /// Parse the version number from the capabilities document
        /// </summary>
        public void ParseVersion()
        {
            
            if (XmlDoc.DocumentElement.Attributes["version"] != null)
            {
                _version = XmlDoc.DocumentElement.Attributes["version"].Value;
                if (_version != "1.0.0" && _version != "1.1.1")
                    throw new ApplicationException("WCS Version " + _version + " is not currently supported");
            }
            else
                throw (new ApplicationException("No service version number was found in the WCS capabilities XML file!"));

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
        /// Method to validate the web server's response
        /// </summary>
        public void ValidateXml()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
