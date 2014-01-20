using System;
using System.Xml;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetCapabilitiesResponse : IHandlerResponse
    {
        private readonly XmlDocument _capabilities;

        public GetCapabilitiesResponse(XmlDocument capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException("capabilities");
            _capabilities = capabilities;
        }

        public string ContentType
        {
            get { return "text/xml"; }
        }

        public XmlDocument Capabilities
        {
            get { return _capabilities; }
        }

        public void WriteToContextAndFlush(IContextResponse response)
        {
            response.Clear();
            response.ContentType = ContentType;
            using (XmlWriter writer = response.CreateXmlWriter())
                Capabilities.WriteTo(writer);
            response.End();
        }
    }
}