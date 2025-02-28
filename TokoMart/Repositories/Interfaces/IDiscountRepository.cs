using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface IDiscountRepository
    {
        Task<Dictionary<string, object>> GetDiscountWithProduct(int size, int page, string sort, string search);
        Task<Dictionary<string, object>> GetById(string id);
        Task<int> Save(Discount discount);
        Task<int> Update(string id, Discount discount);
        Task<int> DeleteById(string id);
    }
}
