using Models;
using Repositories;
using System.Web.Mvc;
namespace Controllers
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
        public ActionResult DeconnextionImprevue()
        {
            EntrerRepository.Instance.Create(new Entrer());
            return View();
        }

    }
}