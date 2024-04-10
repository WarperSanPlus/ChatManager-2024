using System.Web.Mvc;

namespace Controllers
{
    public class ChatRoomController : Controller
    {
        // GET: ChatRoom
        public ActionResult Index() => this.View();
    }
}