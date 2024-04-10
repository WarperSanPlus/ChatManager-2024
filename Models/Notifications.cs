using System;

namespace Models
{
    public class Notification
    {
        public int TargetUserId { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
    }
}