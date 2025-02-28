using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TokoMart.Data;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Repositories
{
    public class CustomRepository : ICustomRepository
    {
        private readonly AppDbContext _context;

        public CustomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetIdByTableNameAsync(string tableName, Dictionary<string, object>? dictWhere = null)
        {
            var sql = $"SELECT id FROM {tableName}";

            if (dictWhere != null && dictWhere.Count > 0)
            {
                var conditions = string.Join(" AND ", dictWhere.Keys.Select(k => $"{k} = @{k}"));
                sql += $" WHERE {conditions}";
            }

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            if (dictWhere != null)
            {
                foreach (var entry in dictWhere)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{entry.Key}";
                    parameter.Value = entry.Value;
                    command.Parameters.Add(parameter);
                }
            }

            await _context.Database.OpenConnectionAsync();
            try
            {
                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<string?> GetNameByIdAsync(string tableName, string id, Dictionary<string, object>? dictWhere = null)
        {
            var sql = $"SELECT name FROM {tableName} WHERE id = @id";

            if (dictWhere != null && dictWhere.Count > 0)
            {
                var conditions = string.Join(" AND ", dictWhere.Keys.Select(k => $"{k} = @{k}"));
                sql += $" AND {conditions}";
            }

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            if (dictWhere != null)
            {
                foreach (var entry in dictWhere)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{entry.Key}";
                    parameter.Value = entry.Value;
                    command.Parameters.Add(parameter);
                }
            }

            await _context.Database.OpenConnectionAsync();
            try
            {
                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<List<Dictionary<string, object>>> GetDataListAsync(string tableName, Dictionary<string, object>? dictWhere = null)
        {
            var sql = $"SELECT id, {(tableName == "discounts" ? "discount_value" : "name")} FROM {tableName}";

            if (dictWhere != null && dictWhere.Count > 0)
            {
                var conditions = string.Join(" AND ", dictWhere.Keys.Select(k => $"{k} = @{k}"));
                sql += $" WHERE {conditions}";
            }

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            if (dictWhere != null)
            {
                foreach (var entry in dictWhere)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{entry.Key}";
                    parameter.Value = entry.Value;
                    command.Parameters.Add(parameter);
                }
            }

            await _context.Database.OpenConnectionAsync();
            var dataList = new List<Dictionary<string, object>>();

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var rowData = new Dictionary<string, object>
                {
                    { "id", reader["id"] },
                    { tableName == "discounts" ? "discount_value" : "name", reader[1] }
                };
                    dataList.Add(rowData);
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return  dataList ;
            //return new Dictionary<string, object> { { "payload", dataList } };
        }

        public async Task<Dictionary<string, object>> GetDataListAsync(
            string tableName,
            int size,
            string? cursor,
            string? search,
            Dictionary<string, object>? dictWhere = null)
        {
            var sql = new StringBuilder($"SELECT id, {(tableName == "discounts" ? "discount_value" : "name")}, updated_at FROM {tableName}");
            var countSql = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");
            var parameters = new Dictionary<string, object>();
            var conditions = new List<string>();

            // Menambahkan filter berdasarkan kondisi (WHERE)
            if (dictWhere != null && dictWhere.Count > 0)
            {
                foreach (var entry in dictWhere)
                {
                    conditions.Add($"{entry.Key} = @{entry.Key}");
                    parameters[$"@{entry.Key}"] = entry.Value;
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                //conditions.Add($"{(tableName == "discounts" ? "discount_value" : "name")} ILIKE @name");
                //parameters["@name"] = $"%{search}%";


                conditions.Add("name ILIKE @name");
                parameters["@name"] = $"%{search}%";
            }

            if (conditions.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", conditions));
                countSql.Append(" WHERE " + string.Join(" AND ", conditions));
            }

            if (!string.IsNullOrEmpty(cursor))
            {
                var cursorCondition = conditions.Any() ? " AND" : " WHERE";
                sql.Append($"{cursorCondition} updated_at < @cursor");
                parameters["@cursor"] = DateTime.Parse(cursor);
            }

            // Order by updated_at DESC dan batasi jumlah data
            sql.Append(" ORDER BY updated_at DESC ");
            if (size!=0)
            {
            sql.Append(" LIMIT @limit");
            parameters["@limit"] = size + 1;
            }

            var dataList = new List<Dictionary<string, object>>();
            DateTime? nextCursor = null;
            int totalData = 0;

            await _context.Database.OpenConnectionAsync();

            try
            {
                using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
                {
                    countCommand.CommandText = countSql.ToString();
                    foreach (var param in parameters.Where(p => p.Key != "@limit" && p.Key != "@cursor"))
                    {
                        var dbParam = countCommand.CreateParameter();
                        dbParam.ParameterName = param.Key;
                        dbParam.Value = param.Value;
                        countCommand.Parameters.Add(dbParam);
                    }

                    totalData = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                // Mengambil data berdasarkan pagination dan pencarian
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
                    { tableName == "discounts" ? "discount_value" : "name", reader[1] },
                    { "updated_at", reader["updated_at"] }
                };
                        dataList.Add(rowData);
                    }
                }


                Console.WriteLine("\nExecuting SQL Query:");
                Console.WriteLine(sql.ToString());
                Console.WriteLine("With Parameters:");


                foreach (var param in parameters)
                {
                    Console.WriteLine($"{param.Key} = {param.Value}");
                }
                Console.WriteLine("");

                Console.WriteLine("\n--- DATA LIST ---");
                var jsonData = JsonSerializer.Serialize(dataList, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(jsonData);
                Console.WriteLine("----------------\n");
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            bool hasNextPage = dataList.Count > size;
            if (hasNextPage)
            {
                dataList.RemoveAt(dataList.Count - 1);
                var extraItem = dataList.Last();
                nextCursor = Convert.ToDateTime(extraItem["updated_at"]);

            }

            return new Dictionary<string, object>
    {
        { "data", dataList },
        { "total_data", totalData },
        { "next_cursor", nextCursor?.ToString("o") }
    };
        }

    }
}
