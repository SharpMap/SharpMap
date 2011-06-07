namespace SharpMap.Demo.Wms.Controllers
{
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Poly()
        {
            return this.View();
        }
    }
}
