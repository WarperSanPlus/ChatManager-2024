using Newtonsoft.Json;
using Repositories;
using System;

namespace Models
{
    public class Message
    {
        public int Id { get; set; }
        public int IdSender { get; set; }
        [JsonIgnore]
        public User Sender { get; set; }

        public int IdReceiver { get; set; }
        [JsonIgnore]
        public User Receiver { get; set; }

        public string Content { get; set; }
        public DateTime SentAt { get; set; }

        public void FetchUsers()
        {
            this.Sender = UsersRepository.GetUser(this.IdSender);
            this.Receiver = UsersRepository.GetUser(this.IdReceiver);
        }

        public bool IsUserRelated(int userId) => this.IdSender == userId || this.IdReceiver == userId;
        public bool FromSessionUser() => this.IdSender == OnlineUsers.GetSessionUser().Id;
        public User GetOther() => this.FromSessionUser() ? this.Sender : this.Receiver;
    }
}