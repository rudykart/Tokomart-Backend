using Microsoft.EntityFrameworkCore;
using System.Text;
using TokoMart.Data;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static TokoMart.TableConstants;

namespace TokoMart.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> DeleteById(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, object>> GetAll(int size, int page, string sort, string search)
        {
            var searchParams = SearchParamParser.ParseSearchParams(search);

            // Query utama untuk mendapatkan daftar transaksi
            var sql = new StringBuilder(@"
        SELECT 
            t.id AS transaction_id,
            t.total_amount,
            t.customer_id,
            c.name AS customer_name,  -- Nama customer
            t.user_id,
            u.name AS user_name,  -- Nama user
            t.created_at,
            t.updated_at,
            COUNT(pt.product_id) AS total_item_purchased,  -- Jumlah jenis produk dalam transaksi
            COALESCE(SUM(pt.quantity), 0) AS total_quantity_purchased  -- Jumlah total produk yang dibeli
        FROM transactions t
        LEFT JOIN product_transactions pt ON t.id = pt.transaction_id
        LEFT JOIN customers c ON t.customer_id = c.id  
        LEFT JOIN users u ON t.user_id = u.id  
    ");

            var countSql = new StringBuilder("SELECT COUNT(*) FROM transactions t");

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            foreach (var entry in searchParams)
            {
                if (entry.Key.Equals("customer_id", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("t.customer_id = @customer_id");
                    parameters["@customer_id"] = entry.Value;
                }
                else if (entry.Key.Equals("user_id", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("t.user_id = @user_id");
                    parameters["@user_id"] = entry.Value;
                }
            }

            if (conditions.Any())
            {
                var whereClause = " WHERE " + string.Join(" AND ", conditions);
                sql.Append(whereClause);
                countSql.Append(whereClause);
            }

            sql.Append(@"
        GROUP BY 
            t.id, t.total_amount, t.customer_id, c.name, 
            t.user_id, u.name, t.created_at, t.updated_at
    ");

            // Sorting & pagination
            sql.Append($" ORDER BY t.updated_at {(string.IsNullOrEmpty(sort) ? "DESC" : sort)}");
            sql.Append(" LIMIT @limit OFFSET @offset");

            parameters["@limit"] = size;
            parameters["@offset"] = (page - 1) * size;

            var dataList = new List<Dictionary<string, object>>();
            int totalData;

            Console.WriteLine("\nExecuting SQL Query:");
            Console.WriteLine(sql.ToString());
            Console.WriteLine("With Parameters:");
            foreach (var param in parameters)
            {
                Console.WriteLine($"{param.Key} = {param.Value}");
            }
            Console.WriteLine("");

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

                    await _context.Database.OpenConnectionAsync(); 

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var rowData = new Dictionary<string, object>
                {
                    { "transaction_id", reader["transaction_id"] },
                    { "total_amount", reader["total_amount"] },
                    { "customer_id", reader["customer_id"] },
                    { "customer_name", reader["customer_name"] },
                    { "user_id", reader["user_id"] },
                    { "user_name", reader["user_name"] },
                    { "created_at", reader["created_at"] },
                    { "updated_at", reader["updated_at"] },
                    { "total_item_purchased", reader["total_item_purchased"] },
                    { "total_quantity_purchased", reader["total_quantity_purchased"] }
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

            // Mengembalikan hasil query
            return new Dictionary<string, object>
    {
        { "transactions", dataList },
        { "totalData", totalData }
    };
        }


        public async Task<Dictionary<string, object>> GetById(string id)
        {
            var sql = @"
        SELECT 
            t.id AS transaction_id,
            t.total_amount,
            t.customer_id,
            c.name AS customer_name,
            t.user_id,
            u.name AS user_name,
            t.created_at,
            t.updated_at
        FROM transactions t
        LEFT JOIN customers c ON t.customer_id = c.id
        LEFT JOIN users u ON t.user_id = u.id
        WHERE t.id = @transactionId";

            var productSql = @"
        SELECT 
            p.id AS product_id,
            p.name AS product_name,
            pt.quantity,
            pt.price,
            pt.discount
        FROM product_transactions pt
        JOIN products p ON pt.product_id = p.id
        WHERE pt.transaction_id = @transactionId";

            Dictionary<string, object> transactionData = null;
            List<Dictionary<string, object>> products = new List<Dictionary<string, object>>();

            await _context.Database.OpenConnectionAsync();
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    var param = command.CreateParameter();
                    param.ParameterName = "@transactionId";
                    param.Value = id;
                    command.Parameters.Add(param);

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        transactionData = new Dictionary<string, object>
                {
                    { "transaction_id", reader["transaction_id"] },
                    { "total_amount", reader["total_amount"] },
                    { "customer_id", reader["customer_id"] },
                    { "customer_name", reader["customer_name"] },
                    { "user_id", reader["user_id"] },
                    { "user_name", reader["user_name"] },
                    { "created_at", reader["created_at"] },
                    { "updated_at", reader["updated_at"] }
                };
                    }
                }

                if (transactionData == null)
                {
                    return null;
                }

                using (var productCommand = _context.Database.GetDbConnection().CreateCommand())
                {
                    productCommand.CommandText = productSql;
                    var param = productCommand.CreateParameter();
                    param.ParameterName = "@transactionId";
                    param.Value = id;
                    productCommand.Parameters.Add(param);

                    using var productReader = await productCommand.ExecuteReaderAsync();
                    while (await productReader.ReadAsync())
                    {
                        var productData = new Dictionary<string, object>
                {
                    { "product_id", productReader["product_id"] },
                    { "product_name", productReader["product_name"] },
                    { "quantity", productReader["quantity"] },
                    { "price", productReader["price"] },
                    { "discount", productReader["discount"] }
                };
                        products.Add(productData);
                    }
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            // Mengembalikan hasil sebagai objek JSON-like
            return new Dictionary<string, object>
    {
        { "transaction", transactionData },
        { "products", products }
    };
        }


        public async Task<int> Save(Transaction transaction)
        {
            if (string.IsNullOrEmpty(transaction.Id)) { transaction.Id = Guid.NewGuid().ToString(); }
            string query = "INSERT INTO transactions (id, total_amount, customer_id, user_id, created_at, updated_at ) " +
                             "VALUES ({0}, {1}, {2}, {3}, {4}, {5})";
            return await _context.ExecuteNativeCommandAsync(query,
                transaction.Id, transaction.TotalAmount, transaction.CustomerId, transaction.UserId, DateTime.UtcNow, DateTime.UtcNow);
        }

        public async Task<int> Update(string id, Transaction transaction)
        {
            string query = @"
        UPDATE transactions 
        SET total_amount = {1}, 
            customer_id = {2}, 
            user_id = {3}, 
            updated_at = {4}
        WHERE id = {0}";

            return await _context.ExecuteNativeCommandAsync(
                query, id, transaction.TotalAmount, transaction.CustomerId, transaction.UserId, DateTime.UtcNow
            );
        }

    }
}
