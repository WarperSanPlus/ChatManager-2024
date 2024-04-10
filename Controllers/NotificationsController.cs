using Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Controllers
{
    public class NotificationsController : Controller
    {
        // GET: Notifications
        public JsonResult Pop()
        {
            User loggedUser = OnlineUsers.GetSessionUser();
            var messages = new List<string>();
            if (loggedUser != null)
            {
                messages = OnlineUsers.PopNotifications(loggedUser.Id);
            }

            return this.Json(messages, JsonRequestBehavior.AllowGet);
        }
    }
}