using Models;
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
    }
}