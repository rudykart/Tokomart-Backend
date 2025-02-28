using TokoMart.DTO;
using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface IClassificationRepository
    {
        Task<(List<Classification> Classifications, int TotalData)> GetAll(int size, int page, string sort, string search);
        Task<int> Save(Classification classification);
        Task<int> Update(string id, Classification classification);
        Task<Classification> GetById(string id);
        Task<int> DeleteById(string id);
    }
}
