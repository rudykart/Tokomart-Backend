using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface IProductTransactionRepository
    {
        Task<int> Save(ProductTransaction productTransaction);
        Task<int> SaveRange(List<ProductTransaction> productTransactions);
        Task<int> Update(string transactionId, ProductTransaction productTransaction);
        Task<int> DeleteById(string id);
    }
}
