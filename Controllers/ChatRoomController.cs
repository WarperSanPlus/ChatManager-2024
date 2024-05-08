using Models;
using Repositories;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Controllers
{
    [OnlineUsers.UserAccess]
    public class ChatRoomController : Controller
    {
        // GET: ChatRoom
        public ActionResult Index()
        {
            var currentUser = OnlineUsers.GetSessionUser();
            this.ViewBag.CurrentChat = this.HttpContext.Cache["CurrentChat" + currentUser.Id];

            return this.View();
        }

        public ActionResult GetOnlineFriends(bool forceRefresh = false)
        {
            var userRepo = UsersRepository.Instance;
            var relationRepo = RelationShipRepository.Instance;

            if (!forceRefresh && !userRepo.HasChanged && !relationRepo.HasChanged && !MessageRepository.Instance.HasChanged)
                return null;

            // Get all users
            var currentUser = OnlineUsers.GetSessionUser();
            var users = userRepo.ToList().AsEnumerable().Where(u => u.Id != currentUser.Id);

            // Get friends
            var relations = relationRepo.ToList().Where(
                r => r.IsUserInRelation(currentUser.Id) && r.State == RelationShipState.Accepted
            );
            users = users
                .Where(u => relations.Any(r => r.IsUserInRelation(u.Id)))
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName);

            return this.PartialView(users);
        }

        #region Messages

        public ActionResult GetMessages(bool forceRefresh = false, int? userId = null)
        {
            var messageRepo = MessageRepository.Instance;
            var userRepo = UsersRepository.Instance;
            var currentUser = OnlineUsers.GetSessionUser();

            this.HttpContext.Cache["CurrentChat" + currentUser.Id] = userId ?? -1;

            if (!forceRefresh && !messageRepo.HasUserNoticed(currentUser.Id) && !userRepo.HasChanged && !RelationShipRepository.Instance.HasChanged)
                return null;

            // Check if user exists
            if (!userId.HasValue || userRepo.Get(userId.Value) == null)
                return null;

            // Get messages
            var messages = messageRepo
                .ToList()
                .Where(m => m.IsUserRelated(currentUser.Id) && m.IsUserRelated(userId.Value))
                .OrderBy(m => m.SentAt);

            return this.PartialView(messages);
        }

        public JsonResult AddMessage(int? userId = null, string content = null)
        {
            content = content ?? string.Empty;
            content = content.Trim();
            if (content.Length == 0 || !userId.HasValue)
                return null;

            var userRepo = UsersRepository.Instance;

            // Check if user exists
            if (userRepo.Get(userId.Value) == null)
                return null;

            var currentUser = OnlineUsers.GetSessionUser();

            var relation = RelationShipRepository.Instance.GetRelationShip(currentUser.Id, userId.Value);

            // Check if friend
            if (relation == null || relation.State != RelationShipState.Accepted)
                return null;

            _ = MessageRepository.Instance.Add(new Message()
            {
                IdSender = currentUser.Id,
                IdReceiver = userId.Value,
                Content = content,
                SentAt = System.DateTime.Now,
            });
            Debug.WriteLine("ADDED");

            // If the receiver isn't looking at the chat
            OnlineUsers.AddNotification(userId.Value, "Vous avez reçu un message de " + currentUser.GetFullName());

            return null;
        }

        public JsonResult ModifyMessage(int? id = null, string content = null)
        {
            if (!id.HasValue)
                return null;

            // Check if message exists
            var messageRepo = MessageRepository.Instance;
            var message = messageRepo.Get(id.Value);

            if (message == null)
                return null;

            // Check if message from local
            var currentUser = OnlineUsers.GetSessionUser();
            var adminDelete = currentUser.IsAdmin && message.IdSender != currentUser.Id;

            if (!adminDelete && message.IdSender != currentUser.Id)
                return null;

            content = content ?? "";

            // Delete message
            if (adminDelete || content.Trim().Length == 0)
            {
                _ = messageRepo.Delete(id.Value);
                return null;
            }

            message.Content = content.Trim();
            _ = messageRepo.Update(message);

            return null;
        }

        #endregion
        //ModerationCommentaire
        #region Moderation


        [OnlineUsers.AdminAccess]
        public ActionResult ModerationCommentaire()
        {


            return this.View();
        }


        [OnlineUsers.AdminAccess]
        public ActionResult EntrerMessageModeration()
        {


            return this.View(Repositories.MessageRepository.Instance.ToList());
        }
        [OnlineUsers.AdminAccess]
        public ActionResult EffacerMessage(int IDMessage)
        {

            Repositories.MessageRepository.Instance.ToList();
            return null;
        }
        #endregion
    }
}