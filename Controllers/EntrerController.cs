using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MoviesDBManager.Models;
using Mail;
using ChatManager.Models;
using System.Web.UI.WebControls;
namespace ChatManager.Controllers
{
    public class EntrerController : Controller
    {



        //public ActionResult Entrer(bool forceRefresh = false)
        //{
        //    if (forceRefresh || DB.Entrers.HasChanged)
        //    {
        //        return PartialView(DB.Entrers.ToList().OrderBy(c => c.entrer));
        //    }
        //    return null;
        //}


        //public ActionResult Creation(bool forceRefresh = false)
        //{
        //    return RedirectToAction("Indexs");
        //}

        //public ActionResult Indexs(bool forceRefresh = false)
        //{
        //    return View();
        //}


    }
}