using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class ProductStockDto
    {
        [Required]
        public int AddStock { get; set; }
    }
}
