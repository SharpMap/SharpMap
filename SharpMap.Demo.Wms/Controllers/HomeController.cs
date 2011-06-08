namespace SharpMap.Demo.Wms.Controllers
{
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return this.RedirectToAction("Openlayers");
        }

        public ActionResult Openlayers()
        {
            return this.View();
        }

        public ActionResult Polymaps()
        {
            return this.View();
        }
    }
}
