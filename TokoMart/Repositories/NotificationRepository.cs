using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using System.Text.Json;
using TokoMart.Data;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;
using static TokoMart.TableConstants;

namespace TokoMart.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Dictionary<string, object>> GetAllByUserId(
    int size, string? sort, string? cursor, string? search,string? userId, bool? hasRead)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            var sql = new StringBuilder(
                @"SELECT 
                    n.id, 
                    n.title, 
                    n.description, 
                    n.has_read, 
                    n.table_name, 
                    n.path_id, 
                    n.user_id, 
                    n.created_at, 
                    n.updated_at,
                    n.created_by,
                    u.name AS created_by_name
                FROM notifications n
                LEFT JOIN users u ON n.created_by = u.id "
            );

            var countSql = new StringBuilder(
                "SELECT COUNT(*) FROM notifications n LEFT JOIN users u ON n.created_by = u.id"
            );

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            // Filter berdasarkan pencarian
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

            if (hasRead != null)
            {
                conditions.Add(" n.has_read = @has_read");
                parameters["@has_read"] = hasRead;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                conditions.Add(" n.user_id = @user_id");
                parameters["@user_id"] = userId;
            }

            if (!string.IsNullOrEmpty(cursor))
            {
                if (sort == "DESC")
                {
                    conditions.Add("n.created_at < @cursor"); // data lebih lama
                }
                else
                {
                    conditions.Add("n.created_at > @cursor"); // data lebih baru
                }
                parameters["@cursor"] = DateTime.Parse(cursor);
            }

            if (conditions.Any())
            {
                string whereClause = " WHERE " + string.Join(" AND ", conditions);
                sql.Append(whereClause);

                var countConditions = conditions.Where(c => !c.Contains("@cursor")).ToList();
                if (countConditions.Any())
                {
                    countSql.Append(" WHERE " + string.Join(" AND ", countConditions));
                }
            }

            sql.Append(sort == "DESC" ? " ORDER BY n.created_at DESC" : " ORDER BY n.created_at ASC");

            sql.Append(" LIMIT @limit");
            parameters["@limit"] = size + 1;

            var dataList = new List<Dictionary<string, object>>();
            DateTime? nextCursor = null;
            int totalData = 0;

            Console.WriteLine("\nExecuting SQL Query:");
            Console.WriteLine(sql.ToString());
            Console.WriteLine("With Parameters:");
            foreach (var param in parameters)
            {
                Console.WriteLine($"{param.Key} = {param.Value}");
            }
            Console.WriteLine("");

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
                    while (await reader.ReadAsync())
                    {
                        var rowData = new Dictionary<string, object>
                {
                    { "id", reader["id"] ?? "" },
                    { "title", reader["title"] is DBNull ? "" : reader["title"] },
                    { "description", reader["description"] ?? "" },
                    { "has_read", reader["has_read"] },
                    { "table_name", reader["table_name"] ?? "" },
                    { "path_id", reader["path_id"] ?? "" },
                    { "user_id", reader["user_id"] ?? "" },
                    { "created_by_name", reader["created_by_name"] ?? "" },
                    { "created_at", reader["created_at"] ?? "" },
                    { "updated_at", reader["updated_at"] ?? "" }
                };
                        dataList.Add(rowData);
                    }
                }


                Console.WriteLine("\n--- DATA LIST ---");
                var jsonData = JsonSerializer.Serialize(dataList, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(jsonData);
                Console.WriteLine("----------------\n");

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
        { "notifications", dataList },
        { "total_data", totalData },
        { "next_cursor", nextCursor?.ToString("o") } // Format ISO 8601
    };
        }

        public async Task<Dictionary<string, object>> GetAllByUserId(string userId, int size, string? cursor, bool? hasRead)
        {
            var sql = new StringBuilder(
                "SELECT n.id, n.title, n.description, n.has_read, n.table_name, n.path_id, n.user_id, n.created_at, n.updated_at n.created_by " +
                "FROM notifications n WHERE n.user_id = @user_id"
            );

            var countSql = new StringBuilder(
                "SELECT COUNT(*) FROM notifications WHERE user_id = @user_id"
            );

            var parameters = new Dictionary<string, object> { { "@user_id", userId } };

            if (!string.IsNullOrEmpty(cursor))
            {
                sql.Append(" AND n.updated_at < @cursor");
                parameters["@cursor"] = DateTime.Parse(cursor);
            }

            if (hasRead != null)
            {
                sql.Append(" AND n.has_read = @has_read");
                countSql.Append(" AND has_read = @has_read");
                parameters["@has_read"] = hasRead;
            }

            sql.Append(" ORDER BY n.updated_at DESC LIMIT @limit");
            parameters["@limit"] = size;

            var dataList = new List<Dictionary<string, object>>();
            string? nextCursor = null;
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
                            { "title", reader["title"] },
                            { "description", reader["description"] },
                            { "has_read", reader["has_read"] },
                            { "table_name", reader["table_name"] },
                            { "path_id", reader["path_id"] },
                            { "user_id", reader["user_id"] },
                            { "created_at", reader["created_at"] },
                            { "updated_at", reader["updated_at"] },
                            { "created_by", reader["created_by"] }
                        };
                        dataList.Add(rowData);
                    }
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            if (dataList.Count > 0)
            {
                nextCursor = dataList.Last()["updated_at"].ToString();
            }

            return new Dictionary<string, object>
            {
                { "notifications", dataList },
                { "total_data", totalData },  // Total data tersedia
                { "next_cursor", nextCursor } // Untuk request halaman berikutnya
            };
        }

        public async Task<Dictionary<string, object>> GetById(string id)
        {
            var sql = new StringBuilder(
                "SELECT n.id, n.title, n.description, n.has_read, n.table_name, n.path_id, n.user_id, n.created_at, n.updated_at, n.created_by  " +
                "FROM notifications n WHERE n.id = @id"
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
                        result["title"] = reader["title"];
                        result["description"] = reader["description"];
                        result["has_read"] = reader["has_read"];
                        result["table_name"] = reader["table_name"];
                        result["path_id"] = reader["path_id"];
                        result["user_id"] = reader["user_id"];
                        result["created_at"] = reader["created_at"];
                        result["updated_at"] = reader["updated_at"];
                        result["created_by"] = reader["created_by"];
                    }
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return result;
        }

        public async Task<int> Read(string id)
        {
            string query = @"
                UPDATE notifications 
                SET has_read = {1}, 
                    updated_at = {2}
                WHERE id = {0}";

            return await _context.ExecuteNativeCommandAsync(query, id, true, DateTime.UtcNow);
        }
        
        public async Task<int> ReadAllByUserId(string userId)
        {
            string query = @"
                UPDATE notifications 
                SET has_read = {1}, 
                    updated_at = {2}
                WHERE user_id = {0} AND has_read = {3}";

            return await _context.ExecuteNativeCommandAsync(query, userId, true, DateTime.UtcNow,false);
        }

        public async Task<int> CountDataByUserId(string userId, bool? hasRead)
        {
            var countSql = new StringBuilder("SELECT COUNT(*) FROM notifications n ");

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (hasRead != null)
            {
                conditions.Add("n.has_read = @has_read");
                parameters["@has_read"] = hasRead;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                conditions.Add("n.user_id = @user_id");
                parameters["@user_id"] = userId;
            }

            if (conditions.Any())
            {
                countSql.Append(" WHERE " + string.Join(" AND ", conditions));
            }

            int totalData = 0;

            Console.WriteLine("\nExecuting SQLcountt Query:");
            Console.WriteLine(countSql.ToString());
            Console.WriteLine("With Parameters:");
            foreach (var param in parameters)
            {
                Console.WriteLine($"{param.Key} = {param.Value}");
            }
            Console.WriteLine("");
            await _context.Database.OpenConnectionAsync();
            try
            {
                using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
                {
                    countCommand.CommandText = countSql.ToString();
                    foreach (var param in parameters)
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

            return totalData;
        }


        public async Task<int> Save(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.Id))
            {
                notification.Id = Guid.NewGuid().ToString();
            }

            string query = @"
                INSERT INTO notifications 
                (id, title, description, has_read, table_name, path_id, user_id, created_at, updated_at, created_by) 
                VALUES (@id, @title, @description, @has_read, @table_name, @path_id, @user_id, @created_at, @updated_at, @created_by)";

            var parameters = new List<NpgsqlParameter>
    {
        new("@id", notification.Id),
        new("@title", notification.Title),
        new("@description", notification.Description ?? ""),
        new("@has_read", notification.HasRead), 
        new("@table_name", notification.TableName ?? ""),
        new("@path_id", notification.PathId ?? ""),
        new("@user_id", notification.UserId ?? ""),
        new("@created_by", notification.CreatedBy ?? ""),
        new("@created_at", DateTime.UtcNow),
        new("@updated_at", DateTime.UtcNow)
    };

            return await _context.ExecuteNativeCommandAsync(query, parameters.ToArray());
        }

    }
}
