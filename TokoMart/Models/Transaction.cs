namespace TokoMart.Models
{
    public class Transaction
    {
        public string Id { get; set; } = string.Empty;
        public double TotalAmount { get; set; }
        public string? CustomerId { get; set; } // Foreign key
        public string? UserId { get; set; } // Foreign key
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}