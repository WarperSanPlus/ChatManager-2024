using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class RelationShipRepository : Repository<RelationShip>
    {
        public static RelationShipRepository Instance => (RelationShipRepository)DB.GetRepo<RelationShip>();

        public IEnumerable<RelationShip> GetRelationShips() => Instance.ToList();
        public RelationShip GetRelationShip(int user1, int user2) 
            => this.ToList().FirstOrDefault(r => r.IsUserInRelation(user1) && r.IsUserInRelation(user2));
        public void SuprimerUserFriend(int userId)
        {
            try
            {
                this.BeginTransaction();

                var entrersToDelete = Instance.ToList().Where(e => e.IdDestination == userId || e.IdOrigin == userId).ToList();

                foreach (var entrer in entrersToDelete)
                {
                    _ = this.Delete(entrer.Id);
                }

                this.EndTransaction();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete Relation pas bien fait - {ex.Message}");
                this.EndTransaction();
            }
        }   
    }
}