using System.Web.Mvc;

namespace Controllers
{
    public class HomeController : Controller
    {
        public ActionResult About() => this.View();
    }
}