using Newtonsoft.Json;
using Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    /// <summary>
    /// Defines the connection between two users
    /// </summary>
    public class RelationShip
    {
        public int Id { get; set; }

        public int IdOrigin { get; set; }
        [JsonIgnore]
        public User Origin { get; set; }

        public int IdDestination { get; set; }
        [JsonIgnore]
        public User Destination { get; set; }

        public RelationShipState State { get; set; }

        public void FetchUsers()
        {
            this.Origin = UsersRepository.GetUser(this.IdOrigin);
            this.Destination = UsersRepository.GetUser(this.IdDestination);
        }

        public bool IsUserInRelation(int userId) => this.IdOrigin == userId || this.IdDestination == userId;
        public bool FromSessionUser() => this.IdOrigin == OnlineUsers.GetSessionUser().Id;
        public User GetOther() => this.FromSessionUser() ? this.Destination : this.Origin;
    }

    public enum RelationShipState
    {
        None = 0x0,
        Pending = 0x1,
        Accepted = 0x2,
        Denied = 0x4,
    }
}