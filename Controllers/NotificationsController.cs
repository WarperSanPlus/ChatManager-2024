using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatManager.Controllers
{
    public class NotificationsController : Controller
    {
        // GET: Notifications
        public JsonResult Pop()
        {
            User loggedUser = OnlineUsers.GetSessionUser();
            List<string> messages = new List<string>();
            if (loggedUser != null)
            {
                messages = OnlineUsers.PopNotifications(loggedUser.Id);
            }
            return Json(messages, JsonRequestBehavior.AllowGet);
        }
    }
}