namespace SharpMap.Demo.Wms.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Web;
    using Helpers;
    using Web.Wms;

    public class StdWmsMapHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                HttpRequest request = context.Request;
                Uri uri = request.Url;
                string url;
                string absoluteUri = uri.AbsoluteUri;
                if (uri.Query.Length > 0)
                {
                    string s = absoluteUri.Replace(uri.Query, String.Empty);
                    url = s;
                }
                else url = absoluteUri;
                Capabilities.WmsServiceDescription description = GetDescription(url);

                Map map;
                string type = request.Params["MAP_TYPE"];                
                if (String.Equals(type, "OL", StringComparison.InvariantCultureIgnoreCase))
                    map = MapHelper.OpenLayers();
                else if (String.Equals(type, "PM", StringComparison.InvariantCultureIgnoreCase))
                    map = MapHelper.PolyMaps();
                else
                {
                    string format = String.Format("unsupported map type: '{0}'", type);
                    throw new NotSupportedException(format);
                }
                WmsServer.ParseQueryString(map, description);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }

        private static Capabilities.WmsServiceDescription GetDescription(string url) 
        {
            Capabilities.WmsServiceDescription description = new Capabilities.WmsServiceDescription("Acme Corp. Map Server", url);
            description.MaxWidth = 500;
            description.MaxHeight = 500;
            description.Abstract = "Map Server maintained by Acme Corporation. Contact: webmaster@wmt.acme.com. High-quality maps showing roadrunner nests and possible ambush locations.";
            description.Keywords = new[] { "bird", "roadrunner", "ambush" };
            description.ContactInformation.PersonPrimary.Person = "John Doe";
            description.ContactInformation.PersonPrimary.Organisation = "Acme Inc";
            description.ContactInformation.Address.AddressType = "postal";
            description.ContactInformation.Address.Country = "Neverland";
            description.ContactInformation.VoiceTelephone = "1-800-WE DO MAPS";
            return description;
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}