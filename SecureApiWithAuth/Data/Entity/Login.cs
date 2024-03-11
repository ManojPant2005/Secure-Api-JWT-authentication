using System.ComponentModel.DataAnnotations;

namespace SecureApiWithAuth.Data.Entity
{
    public class Login
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
