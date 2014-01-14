using System;
using System.Xml;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetCapabilities : AbstractHandler
    {        
        public GetCapabilities(Capabilities.WmsServiceDescription description) : 
            base(description) { }

        protected override WmsParams ValidateParams(IContext context, int targetSrid)
        {
            // Version is mandatory only if REQUEST != GetCapabilities
            string version = context.Params["VERSION"];
            if (version != null)
            {
                if (!String.Equals(version, "1.3.0", Case))
                    return WmsParams.Failure("Only version 1.3.0 supported");
            }
            // Service parameter is mandatory for GetCapabilities request
            string service = context.Params["SERVICE"];
            if (service == null)
                return WmsParams.Failure("Required parameter SERVICE not specified");

            if (!String.Equals(service, "WMS", StringComparison.InvariantCulture))
                return WmsParams.Failure(
                    "Invalid service for GetCapabilities Request. Service parameter must be 'WMS'");

            return new WmsParams { Service = service, Version = version };
        }

        public override void Handle(Map map, IContext context)
        {
            WmsParams @params = ValidateParams(context, 0);
            if (!@params.IsValid)
            {
                WmsException.ThrowWmsException(@params.ErrorCode, @params.Error, context);
                return;
            }

            XmlDocument capabilities = ServerCapabilities.GetCapabilities(map, Description);
            context.Clear();
            context.ContentType = "text/xml";
            using (XmlWriter writer = context.CreateXmlWriter())
                capabilities.WriteTo(writer);            
            context.End();
        }
    }
}
