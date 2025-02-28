using TokoMart.Data;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Repositories
{
    public class ProductTransactionRepository : IProductTransactionRepository
    {
        private readonly AppDbContext _context;

        public ProductTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Save(ProductTransaction productTransaction)
        {
            string query = @"
                INSERT INTO product_transactions (product_id, transaction_id, quantity, price, discount)
                VALUES ({0}, {1}, {2}, {3}, {4})";

            return await _context.ExecuteNativeCommandAsync(
                query, productTransaction.ProductId, productTransaction.TransactionId,
                productTransaction.Quantity, productTransaction.Price, productTransaction.Discount
            );
        }

        public async Task<int> SaveRange(List<ProductTransaction> productTransactions)
        {
            if (productTransactions == null || productTransactions.Count == 0)
            {
                return 0;
            }

            var values = new List<string>();
            var parameters = new List<object>();

            for (int i = 0; i < productTransactions.Count; i++)
            {
                values.Add($"(@p{i * 5}, @p{i * 5 + 1}, @p{i * 5 + 2}, @p{i * 5 + 3}, @p{i * 5 + 4})");
                parameters.Add(new Npgsql.NpgsqlParameter($"@p{i * 5}", productTransactions[i].ProductId));
                parameters.Add(new Npgsql.NpgsqlParameter($"@p{i * 5 + 1}", productTransactions[i].TransactionId));
                parameters.Add(new Npgsql.NpgsqlParameter($"@p{i * 5 + 2}", productTransactions[i].Quantity));
                parameters.Add(new Npgsql.NpgsqlParameter($"@p{i * 5 + 3}", productTransactions[i].Price));
                parameters.Add(new Npgsql.NpgsqlParameter($"@p{i * 5 + 4}", (object)productTransactions[i].Discount ?? DBNull.Value));
            }

            string query = $@"
        INSERT INTO product_transactions (product_id, transaction_id, quantity, price, discount)
        VALUES {string.Join(", ", values)}
    ";

            Console.WriteLine("\nExecuting save ranges SQL Query:");
            Console.WriteLine(query);
            Console.WriteLine("");

            return await _context.ExecuteNativeCommandAsync(query, parameters.ToArray());
        }


        //    public async Task<int> SaveRange(List<ProductTransaction> productTransactions)
        //    {
        //        if (productTransactions == null || productTransactions.Count == 0)
        //        {
        //            return 0;
        //        }

        //        var values = new List<string>();
        //        var parameters = new List<object>();
        //        int index = 0;

        //        foreach (var pt in productTransactions)
        //        {
        //            values.Add($"({{ {index} }}, {{ {index + 1} }}, {{ {index + 2} }}, {{ {index + 3} }}, {{ {index + 4} }})");
        //            parameters.Add(pt.ProductId);
        //            parameters.Add(pt.TransactionId);
        //            parameters.Add(pt.Quantity);
        //            parameters.Add(pt.Price);
        //            parameters.Add(pt.Discount);
        //            index += 5;
        //        }

        //        string query = $@"
        //    INSERT INTO product_transactions (product_id, transaction_id, quantity, price, discount)
        //    VALUES {string.Join(", ", values)}
        //";

        //        Console.WriteLine("\nExecuting save ranges SQL Query:");
        //        Console.WriteLine(query.ToString());
        //        Console.WriteLine("");
        //        return await _context.ExecuteNativeCommandAsync(query, parameters.ToArray());
        //    }


        public async Task<int> Update(string transactionId, ProductTransaction productTransaction)
        {
            string query = @"
                UPDATE product_transactions 
                SET quantity = {2}, price = {3}, discount = {4}
                WHERE product_id = {0} AND transaction_id = {1}";

            return await _context.ExecuteNativeCommandAsync(
                query, productTransaction.ProductId, transactionId,
                productTransaction.Quantity, productTransaction.Price, productTransaction.Discount
            );
        }

        public async Task<int> DeleteById(string transactionId)
        {
            string query = "DELETE FROM product_transactions WHERE transaction_id = {0}";

            return await _context.ExecuteNativeCommandAsync(query, transactionId);
        }
    }
}
