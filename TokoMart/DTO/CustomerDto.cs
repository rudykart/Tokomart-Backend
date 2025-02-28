using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class CustomerDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        public string? PhoneNumber { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        public string? Email { get; set; } = string.Empty;
        public string? Member { get; set; } = string.Empty;
    }
}
