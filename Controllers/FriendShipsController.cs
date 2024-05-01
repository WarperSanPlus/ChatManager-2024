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
            string targetName = "",
            bool showFriends = false,
            bool showOthers = true,
            bool showReceived = false,
            bool showSent = false,
            bool showDeclined = false,
            bool showBlocked = false)
        {
            var relationRepo = RelationShipRepository.Instance;
            var userRepo = UsersRepository.Instance;

            if (!forceRefresh && !relationRepo.HasChanged && !userRepo.HasChanged && !OnlineUsers.HasChanged())
                return null;

            var userId = OnlineUsers.GetSessionUser().Id;
            var relations = relationRepo.ToList().Where(r => r.IsUserInRelation(userId));

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
            relations = relations.Where(r => (showOthers && !r.GetOther().Blocked && r.State == RelationShipState.None)
                || (showFriends && r.State == RelationShipState.Accepted)
                || (showSent && r.State == RelationShipState.Pending && r.FromSessionUser())
                || (showReceived && r.State == RelationShipState.Pending && !r.FromSessionUser())
                || (showDeclined && r.State == RelationShipState.Denied)
                || (showBlocked && r.GetOther().Blocked));

            if (targetName != null)
            {
                targetName = targetName.ToLower();
             
                if (targetName.Length > 0)
                    relations = relations.Where(r => r.GetOther().GetFullName().ToLower().Contains(targetName));
            }

            // Sort by first name, than last name
            relations = relations.OrderBy(r => r.GetOther().FirstName).ThenBy(r => r.GetOther().LastName);

            return this.PartialView(relations);
        }

        #region Actions

        public JsonResult AcceptRequest(int targetUserId, bool isAccepting)
        {
            User usager = OnlineUsers.GetSessionUser();
            var localUserId = usager.Id;
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
                OnlineUsers.AddNotification(targetUserId, usager.GetFullName() + " a accepté(e) votre requête");
                
            }
            else
            {
                OnlineUsers.AddNotification(targetUserId, usager.GetFullName() + " a refusé(e) votre requête");
                relation.State = RelationShipState.Denied;
                (relation.IdOrigin, relation.IdDestination) = (relation.IdDestination, relation.IdOrigin);
            }
           
            _ = repo.Update(relation);

            return null;
        }

        public JsonResult CreateRequest(int targetUserId)
        {
            User usager = OnlineUsers.GetSessionUser();
            var localUserId = usager.Id;
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
                // Skip if target doesn't exist
                if (UsersRepository.GetUser(targetUserId) == null)
                    return null;
                // Add new relation
                _ = repo.Add(new RelationShip()
                {
                    IdOrigin = localUserId,
                    IdDestination = targetUserId,
                    State = RelationShipState.Pending,
                });
            }

            OnlineUsers.AddNotification(targetUserId, $"Vous avez reçu une demande d'amitié de {usager.GetFullName()}");

            return null;
        }

        public JsonResult CancelRequest(int targetUserId)
        {
            User usager = OnlineUsers.GetSessionUser();
            var localUserId = usager.Id;
            var repo = RelationShipRepository.Instance;

            var relation = repo.GetRelationShip(localUserId, targetUserId);
            OnlineUsers.AddNotification(targetUserId, $"{usager.GetFullName()} a refusé(e) votre requête");
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