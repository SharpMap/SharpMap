namespace SharpMap.Demo.Wms
{
    using SharpMap.CoordinateSystems;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class MvcApplication : HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            var defaults = new { controller = "Home", action = "Index", id = UrlParameter.Optional };
            routes.MapRoute("Default", "{controller}/{action}/{id}", defaults);
        }

        protected void Application_Start()
        {
            var gss = NetTopologySuite.NtsGeometryServices.Instance;
            var css = new CoordinateSystemServices(
                new ProjNet.CoordinateSystems.CoordinateSystemFactory(),
                new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory(),
                Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);

            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
