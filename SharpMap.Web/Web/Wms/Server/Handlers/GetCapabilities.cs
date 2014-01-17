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
            // Version is mandatory only if REQUEST != GetCapabilities
            string version = request.Params["VERSION"];
            if (version != null)
            {
                if (!String.Equals(version, "1.3.0", Case))
                    throw new WmsOperationNotSupportedException("Only version 1.3.0 supported");
                //return WmsParams.Failure("Only version 1.3.0 supported");
            }
            // Service parameter is mandatory for GetCapabilities request
            string service = request.Params["SERVICE"];
            if (service == null)
                throw new WmsParameterNotSpecifiedException("SERVICE");
            //return WmsParams.Failure("Required parameter SERVICE not specified");

            if (!String.Equals(service, "WMS", StringComparison.InvariantCulture))
                throw new WmsInvalidParameterException("Invalid service for GetCapabilities Request. Service parameter must be 'WMS'");
            //return WmsParams.Failure(
            //    "Invalid service for GetCapabilities Request. Service parameter must be 'WMS'");

            return new WmsParams { Service = service, Version = version };
        }

        public override IHandlerResponse Handle(Map map, IContextRequest request)
        {
            WmsParams @params = ValidateParams(request, 0);
            if (!@params.IsValid)
            {
                throw new WmsInvalidParameterException(@params.Error, @params.ErrorCode);
                //WmsException.ThrowWmsException(@params.ErrorCode, @params.Error, context);
                //return;
            }

            XmlDocument doc = ServerCapabilities.GetCapabilities(map, Description);
            return new GetCapabilitiesResponse(doc);
        }
    }

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

        public void WriteToContextAndFlush(IContextResponse response)
        {
            response.Clear();
            response.ContentType = "text/xml";
            using (XmlWriter writer = response.CreateXmlWriter())
                _capabilities.WriteTo(writer);
            response.End();
        }
    }
}
