using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using System.Text;
using System.Xml.Linq;
using TokoMart.Data;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;

namespace TokoMart.Repositories
{
    public class ClassificationRepository : IClassificationRepository
    {
        private readonly AppDbContext _context;

        public ClassificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Classification> Classifications, int TotalData)> GetAll(int size, int page, string sort, string search)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            var queryBuilder = new StringBuilder("SELECT * FROM classifications");
            var countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM classifications");

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

            var classifications = await _context.Classifications
                .FromSqlRaw(queryBuilder.ToString(), parameters.ToArray())
                .ToListAsync();

            int totalData = await _context.GetTotalDataAsync(countQueryBuilder.ToString(), parameters);

            // Debug log
            Console.WriteLine($"Debug Query Data   : {queryBuilder}");
            Console.WriteLine($"Debug Query Count  : {countQueryBuilder}");
            Console.WriteLine($"Debug Total Count  : {totalData}");

            return (classifications, totalData);
        }

        public async Task<int> Save(Classification classification)
        {
            string query = "INSERT INTO classifications (id, name, description, table_name, field_name, created_at, updated_at ) " +
                             "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})";
            return await _context.ExecuteNativeCommandAsync(query, Guid.NewGuid(), classification.Name, classification.Description, classification.TableName, classification.FieldName, DateTime.UtcNow, DateTime.UtcNow);
        }

        public async Task<int> Update(string id, Classification classification)
        {
            string query = "UPDATE classifications SET name={0}, description={1}, table_name={2}, field_name={3}, updated_at={4} " +
                             "WHERE id = {5}";
            return await _context.ExecuteNativeCommandAsync(query, classification.Name, classification.Description, classification.TableName, classification.FieldName, DateTime.UtcNow, id);
        }

        public async Task<Classification?> GetById(string id)
        {
            string query = "SELECT * FROM classifications WHERE id = {0}";
            return await _context.Classifications.FromSqlRaw(query, id).FirstOrDefaultAsync();
        }

        public async Task<int> DeleteById(string id)
        {
            string query = "DELETE FROM classifications WHERE id = {0}";
            return await _context.ExecuteNativeCommandAsync(query, id);
        }
    }
}
