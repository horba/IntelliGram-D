using System;

namespace InstaBotPrototype.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}