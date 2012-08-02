namespace SharpMap.Demo.Wms.Handlers
{
    using System;
    using System.Web;

    using GeoAPI;

    using NetTopologySuite;

    using SharpMap.Demo.Wms.Helpers;
    using SharpMap.Web.Wms;

    public abstract class AbstractStdMapHandler : IHttpHandler
    {
        private static readonly object SyncLock = new object();

        static AbstractStdMapHandler()
        {
            lock (SyncLock)
            {
                GeometryServiceProvider.Instance = new NtsGeometryServices();            
            }
        }

        public abstract void ProcessRequest(HttpContext context);

        protected string GetFixedUrl(HttpRequest request)
        {
            Uri uri = request.Url;
            string absoluteUri = uri.AbsoluteUri;
            return uri.Query.Length <= 0 ? absoluteUri : absoluteUri.Replace(uri.Query, String.Empty);
        }

        protected Capabilities.WmsServiceDescription GetDescription(string url) 
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

        protected Map GetMap(HttpRequest request)
        {
            string type = request.Params["MAP_TYPE"];
            if (String.Equals(type, "OL", StringComparison.InvariantCultureIgnoreCase)) 
                return ShapefileHelper.OpenLayers();
            if (String.Equals(type, "PM", StringComparison.InvariantCultureIgnoreCase))
                return ShapefileHelper.PolyMaps();
            if (String.Equals(type, "SQL", StringComparison.InvariantCultureIgnoreCase))
                return DatabaseHelper.SqlServer();
            string format = String.Format("unsupported map type: '{0}'", type);
            throw new NotSupportedException(format);
        }        

        public bool IsReusable
        {
            get { return false; }
        }
    }
}