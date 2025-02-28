using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<(List<Customer> Customers, int TotalData)> GetAll(int size, int page, string sort, string search);
        Task<int> Save(Customer customer);
        Task<int> Update(string id, Customer customer);
        Task<Customer> GetById(string id);
        Task<int> DeleteById(string id);
    }
}
