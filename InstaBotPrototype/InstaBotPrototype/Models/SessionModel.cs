using System;

namespace InstaBotPrototype.Models
{
    public class SessionModel
    {
        public int UserId { get; set; }
        public Guid SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActive { get; set; }
    }
}