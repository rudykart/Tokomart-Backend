namespace TokoMart.Models
{
    public class Notification
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool HasRead { get; set; }
        public string? TableName { get; set; } = string.Empty;
        public string? PathId { get; set; } = string.Empty;
        public string? UserId { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}