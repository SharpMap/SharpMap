using System;
using System.Diagnostics;
using System.Web;
using GeoAPI;
using NetTopologySuite;
using SharpMap.Demo.Wms.Helpers;
using SharpMap.Web.Wms;
using SharpMap.Web.Wms.Exceptions;
using SharpMap.Web.Wms.Server;

namespace SharpMap.Demo.Wms.Handlers
{
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

        public abstract void ProcessRequest(IContext context);

        protected string GetFixedUrl(IContextRequest request)
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

        protected Map GetMap(IContextRequest request)
        {
            string type = request.GetParam("MAP_TYPE");
            if (String.IsNullOrEmpty(type))
                throw new WmsParameterNotSpecifiedException("MAP_TYPE");
            if (String.Equals(type, "DEF", StringComparison.InvariantCultureIgnoreCase))
                return ShapefileHelper.Default();
            if (String.Equals(type, "SPH", StringComparison.InvariantCultureIgnoreCase))
                return ShapefileHelper.Spherical();
            if (String.Equals(type, "SQL", StringComparison.InvariantCultureIgnoreCase))
                return DatabaseHelper.SqlServer();
            if (String.Equals(type, "BRU", StringComparison.InvariantCultureIgnoreCase))
                return BruTileHelper.Osm();
            string format = String.Format("unsupported map type: '{0}'", type);
            throw new NotSupportedException(format);
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                ProcessRequest(new Context(context));
            }
            catch (Exception ex)
            {
                // unhandled exceptions
                Trace.WriteLine(ex);
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}