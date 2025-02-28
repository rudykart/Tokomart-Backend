using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class TransactionDto
    {
        [Required] 
        public string CustomerId { get; set; } = null!;
        [Required]
        public string UserId { get; set; } = null!;
        public List<ProductTransactionDto> Products { get; set; } = new List<ProductTransactionDto>();
    }
}

