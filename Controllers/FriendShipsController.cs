using Models;
using System.Web.Mvc;

namespace Controllers
{
    [OnlineUsers.UserAccess]
    public class FriendShipsController : Controller
    {
        public ActionResult Index() => this.View();
    }
}