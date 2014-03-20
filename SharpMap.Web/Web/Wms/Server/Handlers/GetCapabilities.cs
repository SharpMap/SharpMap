using System;
using System.Xml;
using SharpMap.Web.Wms.Exceptions;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetCapabilities : AbstractHandler
    {
        public GetCapabilities(Capabilities.WmsServiceDescription description) :
            base(description) { }

        protected override WmsParams ValidateParams(IContextRequest request, int targetSrid)
        {
            // version is mandatory only if REQUEST != GetCapabilities
            string version = request.GetParam("VERSION");
            if (version != null)
            {
                if (!String.Equals(version, "1.3.0", Case))
                    throw new WmsOperationNotSupportedException("Only version 1.3.0 supported");
            }
            // service parameter is mandatory for GetCapabilities request
            string service = request.GetParam("SERVICE");
            if (service == null)
                throw new WmsParameterNotSpecifiedException("SERVICE");

            if (!String.Equals(service, "WMS", StringComparison.InvariantCulture))
                throw new WmsInvalidParameterException("Invalid service for GetCapabilities Request. Service parameter must be 'WMS'");

            return new WmsParams { Service = service, Version = version };
        }

        public override IHandlerResponse Handle(Map map, IContextRequest request)
        {
            ValidateParams(request, 0);

            XmlDocument doc = ServerCapabilities.GetCapabilities(map, Description, request);
            return new GetCapabilitiesResponse(doc);
        }
    }
}
