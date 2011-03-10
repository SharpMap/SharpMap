using System.Net;
using System.Xml;

namespace SharpMap.Web
{
    public interface IClient
    {
        //Properties
        XmlNode VendorSpecificCapabilities { get; }
        int TimeOut { get; set; }
        WebProxy Proxy { get; set; }
        ICredentials Credentials { get; set; }
        string BaseUrl { get; set; }
        string CapabilitiesUrl { get; }
        string GetXmlAsText { get; }
        byte[] GetXmlAsByteArray { get; }
        string Version { get; set; }
        string[] ExceptionFormats { get; }
        XmlDocument XmlDoc { get; }

        //Methods
        string CreateCapabilitiesUrl(string url);
        void ValidateXml();
        void ParseVersion();
        void ParseCapabilities();
        XmlDocument GetRemoteXml();
    }
}
