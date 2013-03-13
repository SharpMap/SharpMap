namespace SharpMap.Demo.Wms.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Web;

    using SharpMap.Web.Wms;

    public class StdWmsMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                var request = context.Request;
                var url = this.GetFixedUrl(request);
                var description = this.GetDescription(url);
                var map = this.GetMap(request);
                WmsServer.ParseQueryString(map, description, 10, null, context);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }
    }
}