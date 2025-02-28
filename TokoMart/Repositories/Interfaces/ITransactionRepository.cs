using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Dictionary<string, object>> GetAll(int size, int page, string sort, string search);
        Task<Dictionary<string, object>> GetById(string id);
        Task<int> Save(Transaction transaction);
        Task<int> Update(string id, Transaction transaction);
        Task<int> DeleteById(string id);
    }
}
