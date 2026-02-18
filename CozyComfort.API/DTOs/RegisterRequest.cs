using System.ComponentModel.DataAnnotations;

namespace CozyComfort.API.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        // Optional Role property
        public string? Role { get; set; }
    }
}
