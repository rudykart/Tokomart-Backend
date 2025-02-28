using Microsoft.EntityFrameworkCore;
using TokoMart.Data;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IProductTransactionRepository _productTransactionRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly AppDbContext _context;

        public TransactionService(ITransactionRepository transactionRepository, IProductTransactionRepository productTransactionRepository, IProductRepository productRepository, IUserRepository userRepository, INotificationRepository notificationRepository, AppDbContext context)
        {
            _transactionRepository = transactionRepository;
            _productTransactionRepository = productTransactionRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _context = context;
        }

        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetAll(int size, int page, string sort, string search)
        {
            if (page <= 0) page = 1;
            if (size <= 0) size = 10;

            var result = await _transactionRepository.GetAll(size, page, sort, search);

            var dataList = result["transactions"] as List<Dictionary<string, object>>;
            var totalData = Convert.ToInt32(result["totalData"]);

            int totalPages = (int)Math.Ceiling((double)totalData / size);

            var response = new ApiResponse<List<Dictionary<string, object>>>
            {
                Title = "OK",
                Status = 200,
                Payload = dataList ?? new List<Dictionary<string, object>>(),
                Meta = new MetaData
                {
                    TotalData = totalData,
                    CurrentPage = page,
                    PageSize = size,
                    TotalPages = totalPages,
                    NextPage = page < totalPages,
                    PreviousPage = page > 1
                }
            };

            return response;
        }

        public async Task<ApiResponse<Dictionary<string, object>>> GetById(string id)
        {
            var result = await _transactionRepository.GetById(id);

            if (result == null)
            {
                return new ApiResponse<Dictionary<string, object>>
                {
                    Title = "Not Found",
                    Status = 404,
                    Payload = null
                };
            }

            var transaction = result["transaction"] as Dictionary<string, object>;
            var products = result["products"] as List<Dictionary<string, object>> ?? new List<Dictionary<string, object>>();

            transaction["products"] = products;

            var response = new ApiResponse<Dictionary<string, object>>
            {
                Title = "OK",
                Status = 200,
                Payload = transaction
            };

            return response;
        }

        public async Task<int> Save(TransactionDto transactionDto,string userId)
        {
            if (UserContextService.GetUserId() != transactionDto.UserId)
            {
                return 0; 
            }

            // Buat ID transaksi baru
            string transactionId = Guid.NewGuid().ToString();

            double totalAmount = 0;
            var productTransactions = new List<ProductTransaction>();

            // Mulai transaksi database
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in transactionDto.Products)
                {
                    Product product = await _productRepository.GetById(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return 0; 
                    }

                    double itemTotalPrice = (product.Price ?? 0) * item.Quantity;
                    totalAmount += itemTotalPrice;

                    int stockUpdate = product.Stock - item.Quantity;

                    productTransactions.Add(new ProductTransaction
                    {
                        ProductId = item.ProductId,
                        TransactionId = transactionId,
                        Quantity = item.Quantity,
                        Discount = item.Discount,
                        Price = product.Price ?? 0
                        //Discount = 0 // Jika ada logika diskon, bisa dihitung di sini
                    });

                    Console.WriteLine(" Transaction service ke: 1");
                    await _productRepository.UpdateStock(product.Id, stockUpdate);
                    Console.WriteLine(" Transaction service ke: 2");
                }
                Console.WriteLine(" Transaction service ke: 3");
                // Simpan transaksi ke database
                var transactionData = new Transaction
                {
                    Id = transactionId,
                    CustomerId = transactionDto.CustomerId,
                    UserId = transactionDto.UserId,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _transactionRepository.Save(transactionData);
                Console.WriteLine(" Transaction service ke: 4");

                //if (UserContextService.GetUserRole().Equals("user"))
                if (UserContextService.GetUserRole().Equals("user", StringComparison.OrdinalIgnoreCase))
                    {
                    var userList = await _userRepository.GetByRole("admin");
                    foreach (var user in userList)
                    {
                        var notification = new Notification
                        {
                            Title = "Transactions",
                            TableName = TableConstants.Transactions.TableName,
                            //Description = $"{customerDto.Name} has been updated",
                            Description = "You have new transaction",
                            PathId = transactionId,
                            CreatedBy = UserContextService.GetUserId(),
                            UserId = user.Id
                        };
                        await _notificationRepository.Save(notification);
                    }
                }

                await _productTransactionRepository.SaveRange(productTransactions); // Simpan semua product_transactions sekaligus
                Console.WriteLine(" Transaction service ke: 5");

                await transaction.CommitAsync();

                return 1;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
