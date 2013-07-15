using System.Net;
using System.Xml;

namespace SharpMap.Web
{
    /// <summary>
    /// Interface for client classes accessing OGC Web services
    /// </summary>
    public interface IClient
    {
        //Properties
        /// <summary>
        /// An <see cref="XmlNode"/> specifying specific capabilities, limitations of the server
        /// </summary>
        XmlNode VendorSpecificCapabilities { get; }

        /// <summary>
        /// Gets or sets a value indicating the timeout (in milliseconds) for the connection
        /// </summary>
        int TimeOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the proxy to use.
        /// </summary>
        IWebProxy Proxy { get; set; }

        /// <summary>
        /// Gets or set a value indicating the <see cref="ICredentials"/> to use for accessing the <see cref="Proxy"/>.
        /// </summary>
        ICredentials Credentials { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating the base uniform resource locator of the web service.
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Gets a value indicating the uniform resource locator for the GetCapabilities request.
        /// </summary>
        string CapabilitiesUrl { get; }

        /// <summary>
        /// Gets  the web server's response as a text string.
        /// </summary>
        string GetXmlAsText { get; }

        /// <summary>
        /// Gets the web servers's response as an array of bytes
        /// </summary>
        byte[] GetXmlAsByteArray { get; }

        /// <summary>
        /// Gets or sets the version of the web service to use.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string[] ExceptionFormats { get; }
        
        /// <summary>
        /// Gets a value indicating the web-servers result as an <see cref="XmlDocument"/>
        /// </summary>
        XmlDocument XmlDoc { get; }

        //Methods

        /// <summary>
        /// Method to create a complete capabilities uniform resource locator
        /// </summary>
        /// <param name="url">The base url</param>
        /// <returns>
        /// The uniform resource locator for the capabilities request.
        /// </returns>
        string CreateCapabilitiesUrl(string url);

        /// <summary>
        /// Method to validate the web server's response
        /// </summary>
        void ValidateXml();

        /// <summary>
        /// Method to parse the web-server's version
        /// </summary>
        void ParseVersion();

        /// <summary>
        /// Method to parse the web-server's capabilities
        /// </summary>
        void ParseCapabilities();


        /// <summary>
        /// Method to get the web server's response as a <see cref="XmlDocument"/>
        /// </summary>
        /// <returns>The web server's response as a<see cref="XmlDocument"/></returns>
        XmlDocument GetRemoteXml();
    }
}
