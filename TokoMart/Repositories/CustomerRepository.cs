using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using TokoMart.Data;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;

namespace TokoMart.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Customer> Customers, int TotalData)> GetAll(int size, int page, string sort, string search)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            var queryBuilder = new StringBuilder("SELECT * FROM customers");
            var countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM customers");

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

            var customers = await _context.Customers
                .FromSqlRaw(queryBuilder.ToString(), parameters.ToArray())
                .ToListAsync();

            int totalData = await _context.GetTotalDataAsync(countQueryBuilder.ToString(), parameters);

            Console.WriteLine($"Debug Query Data   : {queryBuilder}");
            Console.WriteLine($"Debug Query Count  : {countQueryBuilder}");
            Console.WriteLine($"Debug Total Count  : {totalData}");

            return (customers, totalData);
        }
        public async Task<int> Save(Customer customer)
        {
            Console.WriteLine("idd customer == > "+ customer.Id);
            string query = "INSERT INTO customers (id, name, phone_number, email, member, created_at, updated_at ) " +
                             "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})";
            return await _context.ExecuteNativeCommandAsync(query, customer.Id, customer.Name, customer.PhoneNumber, customer.Email, customer.Member, DateTime.UtcNow, DateTime.UtcNow);
        }

        public async Task<int> Update(string id, Customer customer)
        {
            string query = "UPDATE customers SET name={0}, phone_number={1}, email={2}, member={3}, updated_at={4} " +
                             "WHERE id = {5}";
            return await _context.ExecuteNativeCommandAsync(query, customer.Name, customer.PhoneNumber, customer.Email, customer.Member, DateTime.UtcNow, id);
        }

        public async Task<Customer?> GetById(string id)
        {
            string query = "SELECT * FROM customers WHERE id = {0}";
            return await _context.Customers.FromSqlRaw(query, id).FirstOrDefaultAsync();
        }

        public async Task<int> DeleteById(string id)
        {
            string query = "DELETE FROM customers WHERE id = {0}";
            return await _context.ExecuteNativeCommandAsync(query, id);
        }
    }
}
