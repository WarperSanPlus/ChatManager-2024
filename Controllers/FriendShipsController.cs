using Models;
using Repositories;
using System.Linq;
using System.Web.Mvc;

namespace Controllers
{
    [OnlineUsers.UserAccess]
    public class FriendShipsController : Controller
    {
        public ActionResult Index() => this.View();

        public ActionResult GetRelations(
            bool forceRefresh = false,
            string targetName = null,
            bool showFriends = false,
            bool showOthers = true,
            bool showReceived = false,
            bool showSent = false,
            bool showDeclined = false,
            bool showBlocked = false)
        {
            var relationRepo = RelationShipRepository.Instance;
            var userRepo = UsersRepository.Instance;

            if (!forceRefresh && !relationRepo.HasChanged && !userRepo.HasChanged)
                return null;

            var userId = OnlineUsers.GetSessionUser().Id;
            var relations = relationRepo.GetRelationShips().Where(r => r.IsUserInRelation(userId));

            // Fill missing users
            foreach (var item in userRepo.ToList())
            {
                var targetId = item.Id;

                if (targetId == userId)
                    continue;

                if (relations.Any(r => r.IsUserInRelation(targetId)))
                    continue;

                relations = relations.Append(new RelationShip()
                {
                    IdOrigin = userId,
                    IdDestination = targetId,
                    State = RelationShipState.None
                });
            }

            foreach (var rel in relations)
                rel.FetchUsers();

            relations = relations.Where(r => r.GetOther().Verified);

            // Search
            relations = relations.Where(r => (r.State == RelationShipState.None && showOthers)
                || (r.State == RelationShipState.Accepted && showFriends)
                || (r.State == RelationShipState.Pending && showSent)
                || (r.State == RelationShipState.Denied && showDeclined)
            );
            relations = relations.OrderBy(r => r.GetOther().FirstName).ThenBy(r => r.GetOther().LastName);

            return this.PartialView(relations);
        }

        #region Actions

        public JsonResult AcceptRequest(int targetUserId, bool isAccepting)
        {
            var localUserId = OnlineUsers.GetSessionUser().Id;
            var repo = RelationShipRepository.Instance;

            var relation = repo.GetRelationShip(localUserId, targetUserId);

            // If the relation doesn't exist, skip
            if (relation == null)
                return null;

            // If the relation isn't pending, skip
            if (relation.State != RelationShipState.Pending)
                return null;

            // Update relation
            if (isAccepting)
            {
                relation.State = RelationShipState.Accepted;
            }
            else
            {
                relation.State = RelationShipState.Denied;
                (relation.IdOrigin, relation.IdDestination) = (relation.IdDestination, relation.IdOrigin);
            }

            _ = repo.Update(relation);

            return null;
        }

        public JsonResult CreateRequest(int targetUserId)
        {
            var localUserId = OnlineUsers.GetSessionUser().Id;
            var repo = RelationShipRepository.Instance;

            var relation = repo.GetRelationShip(localUserId, targetUserId);

            if (relation != null)
            {
                // If not denied by local, skip
                if (!relation.FromSessionUser() || relation.State != RelationShipState.Denied)
                    return null;

                relation.State = RelationShipState.Pending;
                _ = repo.Update(relation);
            }
            else
            {
                // Add new relation
                _ = repo.Add(new RelationShip()
                {
                    IdOrigin = localUserId,
                    IdDestination = targetUserId,
                    State = RelationShipState.Pending,
                });
            }

            return null;
        }

        public JsonResult CancelRequest(int targetUserId)
        {
            var localUserId = OnlineUsers.GetSessionUser().Id;
            var repo = RelationShipRepository.Instance;

            var relation = repo.GetRelationShip(localUserId, targetUserId);

            // If relation doesn't exist, skip
            if (relation == null)
                return null;

            // Remove relation
            _ = repo.Delete(relation.Id);

            return null;
        }

        #endregion
    }
}