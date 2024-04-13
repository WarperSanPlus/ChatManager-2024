using Models;
using Repositories;
using System;
using System.Web.Mvc;
namespace Controllers
{
    public class EntrerController : Controller
    {
        public ActionResult DeconnexionImprevue()
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
       
        public JsonResult SupprimerJour(DateTime date)
        {
            EntrerRepository.Instance.SupprimerJour(date);

            return null;
        }
    }
}