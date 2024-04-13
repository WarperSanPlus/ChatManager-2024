using Models;
using Repositories;
using System;
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
            User user = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                Entrer NouvelEntrer = EntrerRepository.Instance.GetEntrer(user.Id);
                NouvelEntrer.sortie = NouvelEntrer.entrer;
                EntrerRepository.Instance.Update(NouvelEntrer);
            }
     
            return View();
        }
       
        public ActionResult SupprimerJour(DateTime date)
        {

                EntrerRepository.Instance.SupprimerJour(date);

            return View();

        }



        public ActionResult SupprimerJoueur(User user)
        {

            EntrerRepository.Instance.UtilisateurEntrerSupprimer(user);

            return View();

        }
    }
}