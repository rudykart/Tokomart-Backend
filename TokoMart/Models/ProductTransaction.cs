namespace TokoMart.Models
{
    public class ProductTransaction
    {
        public string ProductId { get; set; } = string.Empty; // Foreign key
        public string TransactionId { get; set; } = string.Empty; // Foreign key
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double? Discount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}