namespace TokoMart.Models
{
    public class Product
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public double? Price { get; set; }
        public int Stock { get; set; }
        public string? MainImage { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}