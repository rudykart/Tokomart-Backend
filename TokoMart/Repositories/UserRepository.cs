using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TokoMart.Data;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;

namespace TokoMart.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<User> Users, int TotalData)> GetAll(int size, int page, string sort, string search)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            var queryBuilder = new StringBuilder("SELECT * FROM users");
            var countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM users");

            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            foreach (var entry in searchParams)
            {
                if (entry.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("name ILIKE @name");
                    parameters.Add(new NpgsqlParameter("@name", $"%{entry.Value}%"));
                }
                else
                {
                    conditions.Add($"{entry.Key} = @{entry.Key}");
                    parameters.Add(new NpgsqlParameter($"@{entry.Key}", entry.Value));
                }
            }

            if (conditions.Any())
            {
                queryBuilder.Append(" WHERE " + string.Join(" AND ", conditions));
                countQueryBuilder.Append(" WHERE " + string.Join(" AND ", conditions));
            }

            queryBuilder.Append($" ORDER BY updated_at {(string.IsNullOrEmpty(sort) ? "DESC" : sort)}");
            queryBuilder.Append(" LIMIT @limit OFFSET @offset");

            parameters.Add(new NpgsqlParameter("@limit", size));
            parameters.Add(new NpgsqlParameter("@offset", (page - 1) * size));

            var users = await _context.Users
                .FromSqlRaw(queryBuilder.ToString(), parameters.ToArray())
                .ToListAsync();

            int totalData = await _context.GetTotalDataAsync(countQueryBuilder.ToString(), parameters);

            Console.WriteLine($"Debug Query Data   : {queryBuilder}");
            Console.WriteLine($"Debug Query Count  : {countQueryBuilder}");
            Console.WriteLine($"Debug Total Count  : {totalData}");

            return (users, totalData);
        }


        public async Task<User?> GetById(string id)
        {
            string query = "SELECT * FROM users WHERE id = {0}";
            return await _context.Users.FromSqlRaw(query, id).FirstOrDefaultAsync();
        }

        //public async Task<User?> GetByRole(string role)
        public async Task<List<User>> GetByRole(string role)
        {
            string query = "SELECT * FROM users WHERE role = {0}";
            return await _context.Users.FromSqlRaw(query, role).ToListAsync();

            //var users = await _context.Users
            //    .FromSqlRaw(queryBuilder.ToString(), parameters.ToArray())
            //    .ToListAsync();
        }

        public async Task<User?> FindByUsername(string username)
        {
            var query = "SELECT * FROM users WHERE username = {0}";
            //var query = "SELECT * FROM users WHERE username = {0} AND password = {1}";
            return await _context.Users.FromSqlRaw(query, username).FirstOrDefaultAsync();
        }



        public async Task<int> Save(User user)
        {
            string query = "INSERT INTO users (id, name, username, password, role, created_at, updated_at) " +
                      "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})";
            Console.WriteLine("\n\n query yang di eksekusi => " + query + "\n\n");
            return await _context.ExecuteNativeCommandAsync(query, Guid.NewGuid(), user.Name, user.Username, user.Password, user.Role, DateTime.UtcNow, DateTime.UtcNow);
        }

        public async Task<int> Update(string id, User user)
        {
            string query = "UPDATE users SET name={0}, username={1}, password={2}, role={3}, updated_at={4} " +
                             "WHERE id = {5}";
            Console.WriteLine("\n\n query yang di eksekusi => "+query+ "\n\n");
            return await _context.ExecuteNativeCommandAsync(query, user.Name, user.Username, user.Password, user.Role, DateTime.UtcNow, id);
        }

        public async Task<int> DeleteById(string id)
        {
            string query = "DELETE FROM users WHERE id = {0}";
            return await _context.ExecuteNativeCommandAsync(query, id);
        }
    }
}
