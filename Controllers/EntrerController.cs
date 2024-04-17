using Repositories;
using System;
using System.Web.Mvc;

namespace Controllers
{
    public class EntrerController : Controller
    {
        public JsonResult SupprimerJour(DateTime date)
        {
            EntryRepository.Instance.SupprimerJour(date);

            return null;
        }
    }
}