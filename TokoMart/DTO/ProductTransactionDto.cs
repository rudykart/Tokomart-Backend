using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class ProductTransactionDto
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public string ProductId { get; set; }
        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; }
        public double? Discount { get; set; }
    }
}
