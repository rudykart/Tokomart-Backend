using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class ProductDto
    {

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string? Category { get; set; } = string.Empty;
        [Required]
        public double? Price { get; set; }
        [Required]
        public int Stock { get; set; }
        public string? MainImage { get; set; } = string.Empty;
        //public int? IndexMainImage { get; set; } 
    }
}
