using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class ClassificationDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string? Description { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string TableName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string FieldName { get; set; } = string.Empty;
    }
}
