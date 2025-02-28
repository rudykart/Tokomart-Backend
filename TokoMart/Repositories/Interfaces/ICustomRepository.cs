using Microsoft.EntityFrameworkCore;

namespace TokoMart.Repositories.Interfaces
{
    public interface ICustomRepository
    {
        Task<string?> GetIdByTableNameAsync(string tableName, Dictionary<string, object>? mapWhere = null);
        Task<string?> GetNameByIdAsync(string tableName, string id, Dictionary<string, object>? mapWhere = null);
        Task<List<Dictionary<string, object>>> GetDataListAsync(string tableName, Dictionary<string, object>? mapWhere = null);
        Task<Dictionary<string, object>> GetDataListAsync(
            string tableName,
            int size,
            string? cursor,
            string? search,
            Dictionary<string, object>? dictWhere = null);
    }
}
