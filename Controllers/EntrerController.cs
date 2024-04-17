using Repositories;
using System;
using System.Web.Mvc;

namespace Controllers
{
    public class EntrerController : Controller
    {
        public JsonResult SupprimerJour(DateTime date)
        {
            EntrerRepository.Instance.SupprimerJour(date);

            return null;
        }
    }
}