using TokoMart.DTO;
using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface IUserRepository
    {
        //Task<User?> Login(string username, string password);
        Task<(List<User> Users, int TotalData)> GetAll(int size, int page, string sort, string search);
        Task<User?> GetById(string id);
        Task<List<User>> GetByRole(string role);
        Task<int> Save(User user);
        Task<int> Update(string id, User user);
        Task<User?> FindByUsername(string username);
        Task<int> DeleteById(string id);
    }
}
