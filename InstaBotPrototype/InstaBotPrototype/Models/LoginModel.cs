using System.ComponentModel.DataAnnotations;

namespace InstaBotPrototype.Models
{
    public class LoginModel
    {
        [Required]
        public string Login { get; set; }

        public string Email { get; set; }
    
        [Required]
        public string Password { get; set; }
    }
}