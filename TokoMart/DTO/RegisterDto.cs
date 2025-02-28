using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and ConfirmPassword must match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
