namespace TokoMart.Models
{
    public class Attachment
    {
        public string Id { get; set; } = string.Empty; 
        public string? TableName { get; set; } = string.Empty;
        public string? FileId { get; set; } = string.Empty;
        public string? FileType { get; set; } = string.Empty;
        public string? FilePath { get; set; } = string.Empty;
        public string? FileName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}