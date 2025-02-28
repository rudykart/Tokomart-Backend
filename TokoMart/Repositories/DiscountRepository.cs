using Microsoft.EntityFrameworkCore;
using System.Text;
using TokoMart.Data;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;
using static TokoMart.TableConstants;

namespace TokoMart.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly AppDbContext _context;

        public DiscountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, object>> GetDiscountWithProduct(int size, int page, string sort, string search)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            // Base SQL query
            var sql = new StringBuilder(
                "SELECT d.id, d.discount_value, d.start_at, d.expired_at, d.product_id, p.name AS product_name, p.created_at, p.updated_at " +
                "FROM discounts d LEFT JOIN products p ON d.product_id = p.id"
            );
            var countSql = new StringBuilder(
                "SELECT COUNT(*) FROM discounts d LEFT JOIN products p ON d.product_id = p.id"
            );

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            foreach (var entry in searchParams)
            {
                if (entry.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("p.name ILIKE @name");
                    parameters["@name"] = $"%{entry.Value}%";
                }
                else
                {
                    conditions.Add($"p.{entry.Key} = @{entry.Key}");
                    parameters[$"@{entry.Key}"] = entry.Value;
                }
            }

            if (conditions.Any())
            {
                var whereClause = " WHERE " + string.Join(" AND ", conditions);
                sql.Append(whereClause);
                countSql.Append(whereClause);
            }

            sql.Append($" ORDER BY p.updated_at {(string.IsNullOrEmpty(sort) ? "DESC" : sort)}");
            sql.Append(" LIMIT @limit OFFSET @offset");

            parameters["@limit"] = size;
            parameters["@offset"] = (page - 1) * size;

            var dataList = new List<Dictionary<string, object>>();
            int totalData;

            await _context.Database.OpenConnectionAsync();

            try
            {
                // Execute product query
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql.ToString();
                    foreach (var param in parameters)
                    {
                        var dbParam = command.CreateParameter();
                        dbParam.ParameterName = param.Key;
                        dbParam.Value = param.Value;
                        command.Parameters.Add(dbParam);
                    }

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var rowData = new Dictionary<string, object>
                        {
                            { "id", reader["id"] },
                            { "discount_value", reader["discount_value"] },
                            { "product_id", reader["product_id"] },
                            { "product_name", reader["product_name"] },
                            { "start_at", reader["start_at"] },
                            { "expired_at", reader["expired_at"] },
                            { "created_at", reader["created_at"] },
                            { "updated_at", reader["updated_at"] }
                        };
                        dataList.Add(rowData);
                    }
                }

                // Execute count query
                using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
                {
                    countCommand.CommandText = countSql.ToString();
                    foreach (var param in parameters.Where(p => p.Key != "@limit" && p.Key != "@offset"))
                    {
                        var dbParam = countCommand.CreateParameter();
                        dbParam.ParameterName = param.Key;
                        dbParam.Value = param.Value;
                        countCommand.Parameters.Add(dbParam);
                    }

                    totalData = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            // Return results as a single dictionary
            return new Dictionary<string, object>
            {
                { "discounts", dataList },
                { "totalData", totalData }
            };
        }

        public async Task<Dictionary<string, object>> GetById(string id)
        {
            var sql = new StringBuilder(
                "SELECT d.id, d.discount_value, d.start_at, d.expired_at, d.product_id, " +
                "p.name AS product_name, p.created_at, p.updated_at " +
                "FROM discounts d LEFT JOIN products p ON d.product_id = p.id " +
                "WHERE d.id = @id"
            );

            var parameters = new Dictionary<string, object> { { "@id", id } };

            var result = new Dictionary<string, object>();

            await _context.Database.OpenConnectionAsync();

            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql.ToString();

                    foreach (var param in parameters)
                    {
                        var dbParam = command.CreateParameter();
                        dbParam.ParameterName = param.Key;
                        dbParam.Value = param.Value;
                        command.Parameters.Add(dbParam);
                    }

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        result["id"] = reader["id"];
                        result["discount_value"] = reader["discount_value"];
                        result["product_id"] = reader["product_id"];
                        result["product_name"] = reader["product_name"];
                        result["start_at"] = reader["start_at"];
                        result["expired_at"] = reader["expired_at"];
                        result["created_at"] = reader["created_at"];
                        result["updated_at"] = reader["updated_at"];
                    }
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return result;
        }

        public async Task<int> Save(Discount discount)
        {
            if (string.IsNullOrEmpty(discount.Id)) { discount.Id = Guid.NewGuid().ToString(); }
            string query = "INSERT INTO discounts (id, discount_value, product_id, start_at, expired_at, created_at, updated_at ) " +
                             "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})";
            return await _context.ExecuteNativeCommandAsync(query, discount.Id, discount.DiscountValue, discount.ProductId, discount.StartAt, discount.ExpiredAt, DateTime.UtcNow, DateTime.UtcNow);
        }

        public async Task<int> Update(string id, Discount discount)
        {
            string query = "UPDATE discounts SET discount_value = {0}, product_id = {1}, start_at = {2}, expired_at = {3}, updated_at = {4} WHERE id = {5}";

            return await _context.ExecuteNativeCommandAsync(
                query,
                discount.DiscountValue,
                discount.ProductId,
                discount.StartAt,
                discount.ExpiredAt,
                DateTime.UtcNow,
                id
            );
        }


        public async Task<int> DeleteById(string id)
        {
            string query = "DELETE FROM discounts WHERE id = {0}";
            return await _context.ExecuteNativeCommandAsync(query, id);
        }
    }
}
