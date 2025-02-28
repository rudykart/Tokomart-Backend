using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TokoMart.DTO
{
    public class MetaData
    {
        public int TotalData { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool NextPage { get; set; }
        public bool PreviousPage { get; set; }
        public string NextCursor { get; set; } = string.Empty;
        public string PrevCursor { get; set; } = string.Empty;
    }
}
