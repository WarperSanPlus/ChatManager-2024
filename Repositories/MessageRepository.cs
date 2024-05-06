using Models;
using System;
using System.Linq;

namespace Repositories
{
    public class MessageRepository : Repository<Message>
    {
        public static MessageRepository Instance => (MessageRepository)DB.GetRepo<Message>();

        public void DeleteMessages(int userId)
        {
            try
            {
                this.BeginTransaction();

                var messagesToDelete = Instance.ToList().Where(u => u.IsUserRelated(userId));

                foreach (var message in messagesToDelete)
                    _ = this.Delete(message.Id);

                this.EndTransaction();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error while deleting messages - {ex.Message}");
                this.EndTransaction();
            }
        }
    }
}