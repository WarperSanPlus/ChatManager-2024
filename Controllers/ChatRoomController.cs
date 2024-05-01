using Models;
using Repositories;
using System.Linq;
using System.Web.Mvc;

namespace Controllers
{
    [OnlineUsers.UserAccess]
    public class ChatRoomController : Controller
    {
        // GET: ChatRoom
        public ActionResult Index() => this.View();

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

            if (!forceRefresh && !userRepo.HasChanged && !messageRepo.HasChanged && !RelationShipRepository.Instance.HasChanged)
                return null;

            // Check if user exists
            if (!userId.HasValue || userRepo.Get(userId.Value) == null)
                return null;

            var currentUser = OnlineUsers.GetSessionUser();

            // Get messages
            var messages = messageRepo
                .ToList()
                .Where(m => m.IsUserRelated(currentUser.Id) && m.IsUserRelated(userId.Value))
                .OrderBy(m => m.SentAt)
                .GroupBy(m =>
                {
                    var date = m.SentAt;
                    return ((date.Hour * 60) + date.Minute) / 30;
                });

            return this.PartialView(messages);
        }

        public JsonResult AddMessage(int userId, string content)
        {
            content = content ?? string.Empty;
            content = content.Trim();
            if (content.Length == 0)
                return null;

            var userRepo = UsersRepository.Instance;

            // Check if user exists
            if (userRepo.Get(userId) == null)
                return null;

            var currentUser = OnlineUsers.GetSessionUser();

            var relation = RelationShipRepository.Instance.GetRelationShip(currentUser.Id, userId);

            // Check if friend
            if (relation == null || relation.State != RelationShipState.Accepted)
                return null;

            _ = MessageRepository.Instance.Add(new Message()
            {
                IdSender = currentUser.Id,
                IdReceiver = userId,
                Content = content,
                SentAt = System.DateTime.Now,
            });

            return null;
        }

        #endregion
    }
}