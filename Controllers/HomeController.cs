using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult About()
        {
            return View();
        }
    }
}