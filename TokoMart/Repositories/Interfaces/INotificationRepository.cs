using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Dictionary<string, object>> GetAllByUserId(int size, string? sort, string? cursor, string? search, string? userId, Boolean? hasRead);
        Task<Dictionary<string, object>> GetAllByUserId(string userId, int size, string? cursor, bool? hasRead);
        Task<Dictionary<string, object>> GetById(string id);
        Task<int> Save(Notification notification);
        Task<int> Read(string id);
        Task<int> ReadAllByUserId(string userId);
        Task<int> CountDataByUserId(string userId, bool? hasRead);
    }
}
