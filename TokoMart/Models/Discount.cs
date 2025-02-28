namespace TokoMart.Models
{
    public class Discount
    {
        public string Id { get; set; } = string.Empty;
        public int DiscountValue { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string ProductId { get; set; } = string.Empty; // Foreign key
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}