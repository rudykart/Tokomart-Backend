using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using TokoMart.Data;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;
using System.Text.Json;

namespace TokoMart.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, object>> GetProductsWithCategoryAndMainImage(
    int size, string? sort, string? cursor, string? search)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            //var sql = new StringBuilder(
            //    "SELECT p.id, p.main_image, p.name, p.category, c.name AS category_name, " +
            //    "p.price, p.stock, p.created_at, p.updated_at " +
            //    "FROM products p LEFT JOIN classifications c ON p.category = c.id"
            //);

            var sql = new StringBuilder(
                @"SELECT 
                    p.id, 
                    p.main_image, 
                    p.name, 
                    p.category, 
                    c.name AS category_name, 
                    p.price, 
                    p.stock, 
                    p.created_at, 
                    p.updated_at, 
                    COALESCE(d.discount_value, 0) AS discount_value
                FROM products p
                LEFT JOIN classifications c ON p.category = c.id
                LEFT JOIN discounts d ON p.id = d.product_id 
                    AND CURRENT_TIMESTAMP BETWEEN d.start_at AND d.expired_at"
            );

            var countSql = new StringBuilder(
                "SELECT COUNT(*) FROM products p LEFT JOIN classifications c ON p.category = c.id"
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

            if (!string.IsNullOrEmpty(cursor))
            {
                if (sort == "DESC")
                {
                    conditions.Add("p.updated_at < @cursor"); 
                }
                else
                {
                    conditions.Add("p.updated_at > @cursor"); 
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

            sql.Append(sort == "DESC" ? " ORDER BY p.updated_at DESC" : " ORDER BY p.updated_at ASC");

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
                    { "main_image", reader["main_image"] is DBNull ? "" : reader["main_image"] },
                    { "name", reader["name"] ?? "" },
                    { "category", reader["category"] },
                    { "category_name", reader["category_name"] ?? "" },
                    { "price", reader["price"] ?? "" },
                    { "stock", reader["stock"] ?? "" },
                    { "discount_value", reader["discount_value"] ?? "" },
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
                //var extraItem = dataList.Last(); 
                //nextCursor = Convert.ToDateTime(extraItem["updated_at"]); // Simpan cursor untuk halaman berikutnya
                //dataList.RemoveAt(dataList.Count - 1); // Hapus data ekstra agar hasil sesuai `size`

                dataList.RemoveAt(dataList.Count - 1);
                var extraItem = dataList.Last(); 
                nextCursor = Convert.ToDateTime(extraItem["updated_at"]);

            }

            return new Dictionary<string, object>
    {
        { "products", dataList },
        { "totalData", totalData },
        { "nextCursor", nextCursor?.ToString("o") } // Format ISO 8601
    };
        }


        //        public async Task<Dictionary<string, object>> GetProductsWithCategoryAndMainImage(int size, string? sort, string? cursor, string? search)
        //        {
        //            var searchParams = SearchParamParser.ParseSearchParams(search);

        //            var sql = new StringBuilder(
        //                "SELECT p.id, p.main_image, p.name, p.category, c.name AS category_name, p.price, p.stock, p.created_at, p.updated_at " +
        //                "FROM products p LEFT JOIN classifications c ON p.category = c.id"
        //            );

        //            var countSql = new StringBuilder(
        //                "SELECT COUNT(*) FROM products p LEFT JOIN classifications c ON p.category = c.id"
        //            );

        //            var conditions = new List<string>();
        //            var parameters = new Dictionary<string, object>();

        //            // Filter berdasarkan pencarian
        //            foreach (var entry in searchParams)
        //            {
        //                if (entry.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    conditions.Add("p.name ILIKE @name");
        //                    parameters["@name"] = $"%{entry.Value}%";
        //                }
        //                else
        //                {
        //                    conditions.Add($"p.{entry.Key} = @{entry.Key}");
        //                    parameters[$"@{entry.Key}"] = entry.Value;
        //                }
        //            }

        //            // Jika ada cursor, tambahkan filter updated_at
        //            if (!string.IsNullOrEmpty(cursor))
        //            {
        //                if (sort == "DESC")
        //                {
        //                    conditions.Add("p.updated_at < @cursor"); // Ambil data lebih lama
        //                }
        //                else
        //                {
        //                    conditions.Add("p.updated_at > @cursor"); // Ambil data lebih baru
        //                }
        //                parameters["@cursor"] = DateTime.Parse(cursor);
        //            }

        //            // Tambahkan WHERE jika ada kondisi
        //            if (conditions.Any())
        //            {
        //                string whereClause = " WHERE " + string.Join(" AND ", conditions);
        //                sql.Append(whereClause);

        //                var countConditions = conditions.Where(c => !c.Contains("@cursor")).ToList();
        //                if (countConditions.Any())
        //                {
        //                    countSql.Append(" WHERE " + string.Join(" AND ", countConditions));
        //                }
        //            }

        //            // Order by updated_at sesuai sort
        //            sql.Append(sort == "DESC" ? " ORDER BY p.updated_at DESC" : " ORDER BY p.updated_at ASC");

        //            // Tambahkan LIMIT
        //            sql.Append(" LIMIT @limit");
        //            parameters["@limit"] = size; // Ambil lebih 1 untuk deteksi halaman berikutnya
        //            //parameters["@limit"] = size + 1; // Ambil lebih 1 untuk deteksi halaman berikutnya

        //            var dataList = new List<Dictionary<string, object>>();
        //            DateTime? prevCursor = null;
        //            DateTime? nextCursor = null;
        //            int totalData = 0;

        //            Console.WriteLine("");
        //            Console.WriteLine("");
        //            Console.WriteLine("");
        //            Console.WriteLine("Executing SQL Query:");
        //            Console.WriteLine(sql.ToString());
        //            Console.WriteLine("With Parameters:");
        //            foreach (var param in parameters)
        //            {
        //                Console.WriteLine($"{param.Key} = {param.Value}");
        //            }
        //            Console.WriteLine("");
        //            Console.WriteLine("");
        //            Console.WriteLine("");

        //            await _context.Database.OpenConnectionAsync();

        //            try
        //            {
        //                // Execute product query
        //                using (var command = _context.Database.GetDbConnection().CreateCommand())
        //                {
        //                    command.CommandText = sql.ToString();
        //                    foreach (var param in parameters)
        //                    {
        //                        var dbParam = command.CreateParameter();
        //                        dbParam.ParameterName = param.Key;
        //                        dbParam.Value = param.Value;
        //                        command.Parameters.Add(dbParam);
        //                    }

        //                    using var reader = await command.ExecuteReaderAsync();
        //                    while (await reader.ReadAsync())
        //                    {
        //                        var rowData = new Dictionary<string, object>
        //            {
        //                { "id", reader["id"] ?? "" },
        //                { "main_image", reader["main_image"] is DBNull ? "" : reader["main_image"] },
        //                { "name", reader["name"] ?? "" },
        //                { "category", reader["category"] },
        //                { "category_name", reader["category_name"] ?? "" },
        //                { "price", reader["price"] ?? "" },
        //                { "stock", reader["stock"] ?? "" },
        //                { "created_at", reader["created_at"] ?? "" },
        //                { "updated_at", reader["updated_at"] ?? "" }
        //            };
        //                        dataList.Add(rowData);
        //                    }
        //                }

        //                // Execute count query (TANPA cursor)
        //                using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
        //                {
        //                    countCommand.CommandText = countSql.ToString();
        //                    foreach (var param in parameters.Where(p => p.Key != "@limit" && p.Key != "@cursor"))
        //                    {
        //                        var dbParam = countCommand.CreateParameter();
        //                        dbParam.ParameterName = param.Key;
        //                        dbParam.Value = param.Value;
        //                        countCommand.Parameters.Add(dbParam);
        //                    }

        //                    totalData = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        //                }
        //            }
        //            finally
        //            {
        //                await _context.Database.CloseConnectionAsync();
        //            }

        //            // Pastikan pagination berjalan dengan benar
        //            bool hasNextPage = dataList.Count > size;
        //            if (hasNextPage)
        //            {
        //                dataList.RemoveAt(dataList.Count - 1); // Hapus data ekstra
        //            }

        //            // Set prevCursor dan nextCursor sesuai urutan
        //            if (dataList.Count > 0)
        //            {
        //                var firstItem = dataList.First();
        //                var lastItem = dataList.Last();

        //                if (sort == "DESC")
        //                {
        //                    prevCursor = Convert.ToDateTime(firstItem["updated_at"]);
        //                    nextCursor = hasNextPage ? Convert.ToDateTime(lastItem["updated_at"]) : null;
        //                }
        //                else
        //                {
        //                    prevCursor = Convert.ToDateTime(lastItem["updated_at"]);
        //                    nextCursor = hasNextPage ? Convert.ToDateTime(firstItem["updated_at"]) : null;
        //                }
        //            }

        //            return new Dictionary<string, object>
        //{
        //    { "products", dataList },
        //    { "totalData", totalData },
        //    { "nextCursor", nextCursor?.ToString("o") }, // Format ISO 8601
        //    { "prevCursor", prevCursor?.ToString("o") }  // Format ISO 8601
        //};
        //        }

        //    public async Task<Dictionary<string, object>> GetProductsWithCategoryAndMainImage(int size, string? sort, string? cursor, string? search)
        //    {
        //        var searchParams = SearchParamParser.ParseSearchParams(search);

        //        // Query untuk mengambil produk
        //        var sql = new StringBuilder(
        //            "SELECT p.id, p.main_image, p.name, p.category, c.name AS category_name, p.price, p.stock, p.created_at, p.updated_at " +
        //            "FROM products p LEFT JOIN classifications c ON p.category = c.id"
        //        );

        //        // Query untuk menghitung total data
        //        var countSql = new StringBuilder(
        //            "SELECT COUNT(*) FROM products p LEFT JOIN classifications c ON p.category = c.id"
        //        );

        //        var conditions = new List<string>();
        //        var parameters = new Dictionary<string, object>();

        //        // Filter berdasarkan pencarian
        //        foreach (var entry in searchParams)
        //        {
        //            if (entry.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
        //            {
        //                conditions.Add("p.name ILIKE @name");
        //                parameters["@name"] = $"%{entry.Value}%";
        //            }
        //            else
        //            {
        //                conditions.Add($"p.{entry.Key} = @{entry.Key}");
        //                parameters[$"@{entry.Key}"] = entry.Value;
        //            }
        //        }

        //        // Jika ada cursor, tambahkan filter updated_at untuk query data utama (TIDAK untuk count)
        //        if (!string.IsNullOrEmpty(cursor))
        //        {
        //            if (sort == "DESC")
        //            {
        //                conditions.Add("p.updated_at < @cursor");
        //            }
        //            else if (sort == "ASC")
        //            {
        //                conditions.Add("p.updated_at > @cursor");
        //            }
        //            parameters["@cursor"] = DateTime.Parse(cursor);
        //        }

        //        // Tambahkan WHERE jika ada kondisi
        //        if (conditions.Any())
        //        {
        //            string whereClause = " WHERE " + string.Join(" AND ", conditions);
        //            sql.Append(whereClause);

        //            // **COUNT QUERY TIDAK BOLEH MEMAKAI CURSOR**
        //            var countConditions = conditions.Where(c => !c.Contains("@cursor")).ToList();
        //            if (countConditions.Any())
        //            {
        //                countSql.Append(" WHERE " + string.Join(" AND ", countConditions));
        //            }
        //        }

        //        // Order by updated_at sesuai sort
        //        if (sort == "DESC")
        //        {
        //            sql.Append(" ORDER BY p.updated_at DESC");
        //        }
        //        else if (sort == "ASC")
        //        {
        //            sql.Append(" ORDER BY p.updated_at ASC");
        //        }

        //        // Tambahkan LIMIT untuk pagination
        //        if (size > 0)
        //        {
        //            sql.Append(" LIMIT @limit");
        //            parameters["@limit"] = size + 1; // Ambil 1 data ekstra untuk mengecek apakah ada data berikutnya
        //        }

        //        var dataList = new List<Dictionary<string, object>>();
        //        DateTime? prevCursor = null;
        //        DateTime? nextCursor = null;
        //        int totalData = 0;

        //        await _context.Database.OpenConnectionAsync();

        //        try
        //        {
        //            // Execute product query
        //            using (var command = _context.Database.GetDbConnection().CreateCommand())
        //            {
        //                command.CommandText = sql.ToString();
        //                foreach (var param in parameters)
        //                {
        //                    var dbParam = command.CreateParameter();
        //                    dbParam.ParameterName = param.Key;
        //                    dbParam.Value = param.Value;
        //                    command.Parameters.Add(dbParam);
        //                }

        //                using var reader = await command.ExecuteReaderAsync();
        //                while (await reader.ReadAsync())
        //                {
        //                    var rowData = new Dictionary<string, object>
        //            {
        //                { "id", reader["id"] ?? "" },
        //                { "main_image", reader["main_image"] is DBNull ? "" : reader["main_image"] },
        //                { "name", reader["name"] ?? "" },
        //                { "category", reader["category"] },
        //                { "category_name", reader["category_name"] ?? "" },
        //                { "price", reader["price"] ?? "" },
        //                { "stock", reader["stock"] ?? "" },
        //                { "created_at", reader["created_at"] ?? "" },
        //                { "updated_at", reader["updated_at"] ?? "" }
        //            };
        //                    dataList.Add(rowData);
        //                }
        //            }

        //            // Execute count query (TANPA cursor)
        //            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
        //            {
        //                countCommand.CommandText = countSql.ToString();
        //                foreach (var param in parameters.Where(p => p.Key != "@limit" && p.Key != "@cursor")) // **Hindari memasukkan cursor!**
        //                {
        //                    var dbParam = countCommand.CreateParameter();
        //                    dbParam.ParameterName = param.Key;
        //                    dbParam.Value = param.Value;
        //                    countCommand.Parameters.Add(dbParam);
        //                }

        //                totalData = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        //            }
        //        }
        //        finally
        //        {
        //            await _context.Database.CloseConnectionAsync();
        //        }

        //        // Cek apakah ada data ekstra (untuk menentukan nextCursor)
        //        bool hasNextPage = dataList.Count > size;
        //        if (hasNextPage)
        //        {
        //            dataList.RemoveAt(dataList.Count - 1); // Hapus data ekstra
        //        }

        //        // Set prevCursor dan nextCursor
        //        if (dataList.Count > 0)
        //        {
        //            var firstItem = dataList.First();
        //            var lastItem = dataList.Last();
        //            prevCursor = Convert.ToDateTime(firstItem["updated_at"]);
        //            nextCursor = hasNextPage ? Convert.ToDateTime(lastItem["updated_at"]) : null;
        //        }

        //        // Return hasil dengan prevCursor dan nextCursor
        //        return new Dictionary<string, object>
        //{
        //    { "products", dataList },
        //    { "totalData", totalData },
        //    { "nextCursor", nextCursor?.ToString("o") }, // Format ISO 8601
        //    { "prevCursor", prevCursor?.ToString("o") }  // Format ISO 8601
        //};
        //    }

        //    public async Task<Dictionary<string, object>> GetProductsWithCategoryAndMainImage(int size, string? sort, string? cursor, string? search)
        //    {
        //        var searchParams = SearchParamParser.ParseSearchParams(search);

        //        // Query untuk mengambil produk
        //        var sql = new StringBuilder(
        //            "SELECT p.id, p.main_image, p.name, p.category, c.name AS category_name, p.price, p.stock, p.created_at, p.updated_at " +
        //            "FROM products p LEFT JOIN classifications c ON p.category = c.id"
        //        );

        //        // Query untuk menghitung total data
        //        var countSql = new StringBuilder(
        //            "SELECT COUNT(*) FROM products p LEFT JOIN classifications c ON p.category = c.id"
        //        );

        //        var conditions = new List<string>();
        //        var parameters = new Dictionary<string, object>();

        //        // Filter berdasarkan pencarian
        //        foreach (var entry in searchParams)
        //        {
        //            if (entry.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
        //            {
        //                conditions.Add("p.name ILIKE @name");
        //                parameters["@name"] = $"%{entry.Value}%";
        //            }
        //            else
        //            {
        //                conditions.Add($"p.{entry.Key} = @{entry.Key}");
        //                parameters[$"@{entry.Key}"] = entry.Value;
        //            }
        //        }

        //        // Jika ada cursor, tambahkan filter updated_at untuk query data utama (TIDAK untuk count)
        //        if (!string.IsNullOrEmpty(cursor))
        //        {
        //            if (sort == "DESC")
        //            {
        //                conditions.Add("p.updated_at < @cursor");
        //            }
        //            else if (sort == "ASC")
        //            {
        //                conditions.Add("p.updated_at > @cursor");
        //            }
        //            parameters["@cursor"] = DateTime.Parse(cursor);
        //        }

        //        // Tambahkan WHERE jika ada kondisi
        //        if (conditions.Any())
        //        {
        //            string whereClause = " WHERE " + string.Join(" AND ", conditions);
        //            sql.Append(whereClause);

        //            // **COUNT QUERY TIDAK BOLEH MEMAKAI CURSOR**
        //            var countConditions = conditions.Where(c => !c.Contains("@cursor")).ToList();
        //            if (countConditions.Any())
        //            {
        //                countSql.Append(" WHERE " + string.Join(" AND ", countConditions));
        //            }
        //        }

        //        // Order by updated_at sesuai sort
        //        if (sort == "DESC")
        //        {
        //            sql.Append(" ORDER BY p.updated_at DESC");
        //        }
        //        else if (sort == "ASC")
        //        {
        //            sql.Append(" ORDER BY p.updated_at ASC");
        //        }

        //        // Tambahkan LIMIT untuk pagination
        //        if (size > 0)
        //        {
        //            sql.Append(" LIMIT @limit");
        //            parameters["@limit"] = size + 1; // Ambil 1 data ekstra untuk mengecek apakah ada data berikutnya
        //        }

        //        var dataList = new List<Dictionary<string, object>>();
        //        DateTime? prevCursor = null;
        //        DateTime? nextCursor = null;
        //        int totalData = 0;

        //        await _context.Database.OpenConnectionAsync();

        //        try
        //        {
        //            // Execute product query
        //            using (var command = _context.Database.GetDbConnection().CreateCommand())
        //            {
        //                command.CommandText = sql.ToString();
        //                foreach (var param in parameters)
        //                {
        //                    var dbParam = command.CreateParameter();
        //                    dbParam.ParameterName = param.Key;
        //                    dbParam.Value = param.Value;
        //                    command.Parameters.Add(dbParam);
        //                }

        //                using var reader = await command.ExecuteReaderAsync();
        //                while (await reader.ReadAsync())
        //                {
        //                    var rowData = new Dictionary<string, object>
        //            {
        //                { "id", reader["id"] ?? "" },
        //                { "main_image", reader["main_image"] is DBNull ? "" : reader["main_image"] },
        //                { "name", reader["name"] ?? "" },
        //                { "category", reader["category"] },
        //                { "category_name", reader["category_name"] ?? "" },
        //                { "price", reader["price"] ?? "" },
        //                { "stock", reader["stock"] ?? "" },
        //                { "created_at", reader["created_at"] ?? "" },
        //                { "updated_at", reader["updated_at"] ?? "" }
        //            };
        //                    dataList.Add(rowData);
        //                }
        //            }

        //            // Execute count query (TANPA cursor)
        //            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
        //            {
        //                countCommand.CommandText = countSql.ToString();
        //                foreach (var param in parameters.Where(p => p.Key != "@limit" && p.Key != "@cursor")) // **Hindari memasukkan cursor!**
        //                {
        //                    var dbParam = countCommand.CreateParameter();
        //                    dbParam.ParameterName = param.Key;
        //                    dbParam.Value = param.Value;
        //                    countCommand.Parameters.Add(dbParam);
        //                }

        //                totalData = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        //            }
        //        }
        //        finally
        //        {
        //            await _context.Database.CloseConnectionAsync();
        //        }

        //        // Cek apakah ada data ekstra (untuk menentukan nextCursor)
        //        bool hasNextPage = dataList.Count > size;
        //        if (hasNextPage)
        //        {
        //            dataList.RemoveAt(dataList.Count - 1); // Hapus data ekstra
        //        }

        //        // Set prevCursor dan nextCursor
        //        // Set prevCursor dan nextCursor sesuai dengan sort
        //        if (dataList.Count > 0)
        //        {
        //            var firstItem = dataList.First();
        //            var lastItem = dataList.Last();

        //            if (sort == "DESC")
        //            {
        //                prevCursor = Convert.ToDateTime(lastItem["updated_at"]); // Elemen terakhir jadi prevCursor
        //                nextCursor = hasNextPage ? Convert.ToDateTime(firstItem["updated_at"]) : null; // Elemen pertama jadi nextCursor jika ada halaman berikutnya
        //            }
        //            else // ASC
        //            {
        //                prevCursor = Convert.ToDateTime(firstItem["updated_at"]); // Elemen pertama jadi prevCursor
        //                nextCursor = hasNextPage ? Convert.ToDateTime(lastItem["updated_at"]) : null; // Elemen terakhir jadi nextCursor jika ada halaman berikutnya
        //            }
        //        }


        //        // Return hasil dengan prevCursor dan nextCursor
        //        return new Dictionary<string, object>
        //{
        //    { "products", dataList },
        //    { "totalData", totalData },
        //    { "nextCursor", nextCursor?.ToString("o") }, // Format ISO 8601
        //    { "prevCursor", prevCursor?.ToString("o") }  // Format ISO 8601
        //};
        //    }


        public async Task<Dictionary<string, object>> GetProductsWithCategoryAndMainImage(int size, int page, string sort, string search)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            // Base SQL query
            var sql = new StringBuilder(
                "SELECT p.id, p.main_image, p.name, p.category, c.name AS category_name, p.price, p.stock, p.main_image, p.created_at, p.updated_at " +
                "FROM products p LEFT JOIN classifications c ON p.category = c.id"
            );
            var countSql = new StringBuilder(
                "SELECT COUNT(*) FROM products p LEFT JOIN classifications c ON p.category = c.id"
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
                    { "id", reader["id"] ?? "" },
                    { "main_image", reader["main_image"] is DBNull ? "" : reader["main_image"] },
                    { "name", reader["name"] ?? "" },
                    { "category", reader["category"] },
                    { "category_name", reader["category_name"] ?? "" },
                    { "price", reader["price"] ?? "" },
                    { "stock", reader["stock"] ?? "" },
                    { "created_at", reader["created_at"] ?? "" },
                    { "updated_at", reader["updated_at"] ?? "" }
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
                { "products", dataList },
                { "totalData", totalData }
            };
        }

        //public async Task<Product> GetById(string id)
        public async Task<Product> GetById(string id)
        {
            string query = "SELECT * FROM products WHERE id = {0}";
            //string query = "SELECT p.id, p.name, p.category, c.name AS category_name, p.price, p.stock, p.created_at, p.updated_at FROM products p LEFT JOIN classifications c ON p.category = c.id";
            return await _context.Products.FromSqlRaw(query, id).FirstOrDefaultAsync();
        }

        public async Task<int> Save(Product product)
        {
            if (string.IsNullOrEmpty(product.Id)) { product.Id = Guid.NewGuid().ToString(); }
            string query = "INSERT INTO products (id, name, category, price, stock, main_image, created_at, updated_at ) " +
                             "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6},{7})";
            return await _context.ExecuteNativeCommandAsync(query, product.Id , product.Name, product.Category, product.Price, product.Stock, product.MainImage, DateTime.UtcNow, DateTime.UtcNow);
        }

        public async Task<int> Update(string id, Product product)
        {
            string query = "UPDATE products SET name={0}, category={1}, price={2}, stock={3}, main_image={4}, updated_at={5} " +
                             "WHERE id = {6}";

            return await _context.ExecuteNativeCommandAsync(query, product.Name, product.Category, product.Price, product.Stock, product.MainImage, DateTime.UtcNow, id);
            //string query = "UPDATE products SET name=@name, category=@category, price=@price, stock=@stock, main_image=@main_image, updated_at=@updated_at " +
            //          "WHERE id = @id";

            //Console.WriteLine("\nExecuting SQL Query:");
            //Console.WriteLine(query.ToString());
            //Console.WriteLine("");

            //return await _context.ExecuteNativeCommandAsync(query,
            //    new NpgsqlParameter("@name", product.Name),
            //    new NpgsqlParameter("@category", product.Category),
            //    new NpgsqlParameter("@price", product.Price),
            //    new NpgsqlParameter("@stock", product.Stock),
            //    new NpgsqlParameter("@main_image", (object)product.MainImage ?? DBNull.Value),
            //    new NpgsqlParameter("@updated_at", DateTime.UtcNow),
            //    new NpgsqlParameter("@id", id)
            //);
        }

        public async Task<int> UpdateStock(string id, int stock)
        {
            string query = "UPDATE products SET stock={0} " +
                             "WHERE id = {1}";
            Console.WriteLine("\nExecuting SQL Query:");
            Console.WriteLine(query.ToString());
            Console.WriteLine("");
            return await _context.ExecuteNativeCommandAsync(query, stock, id);
        }

        public async Task<int> DeleteById(string id)
        {
            string query = "DELETE FROM products WHERE id = {0}";
            return await _context.ExecuteNativeCommandAsync(query, id);
        }
    }
}
