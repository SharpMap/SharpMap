using SharpMap.Web.Wms;
using SharpMap.Web.Wms.Server;

namespace SharpMap.Demo.Wms.Handlers
{
    public class StdWmsMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(IContext context)
        {
            string url = GetFixedUrl(context.Request);
            Capabilities.WmsServiceDescription description = GetDescription(url);
            Map map = GetMap(context.Request);
            WmsServer.ParseQueryString(map, description, 10, null, context);
        }
    }
}