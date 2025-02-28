namespace TokoMart.DTO
{
    public class ApiResponse<T>
    {
        public string Title { get; set; }
        public int Status { get; set; }
        public T Payload { get; set; }
        public object Meta { get; set; }
    }
}
