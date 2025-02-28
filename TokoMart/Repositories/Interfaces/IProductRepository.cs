using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface IProductRepository
    {
        //Task<(IEnumerable<Product> Products, int TotalData)> GetAll(int size, int page, string sort, string search);
        Task<Dictionary<string, object>> GetProductsWithCategoryAndMainImage(
            int size,
            string? sort,
            string? cursor,
            string? filter);
        Task<Dictionary<string, object>> GetProductsWithCategoryAndMainImage(int size, int page, string sort, string search);
        Task<int> Save( Product product);
        Task<int> Update(string id, Product product);
        Task<int> UpdateStock(string id, int stock);
        Task<Product> GetById(string id);
        Task<int> DeleteById(string id);
    }
}
