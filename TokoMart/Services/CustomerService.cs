using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TokoMart.Data;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly NotificationService _notificationService;
        private readonly AppDbContext _context;
        private readonly CustomService _customService;

        public CustomerService(ICustomerRepository customerRepository, NotificationService notificationService, AppDbContext context, CustomService customService)
        {
            _customerRepository = customerRepository;
            _notificationService = notificationService;
            _context = context;
            _customService = customService;
        }

        public async Task<ApiResponse<List<Customer>>> GetAll()
        {
            return await GetAll(0, 0, "", "");
        }

        public async Task<ApiResponse<List<Customer>>> GetAll(int size, int page)
        {
            return await GetAll(size, page, "", "");
        }

        public async Task<ApiResponse<List<Customer>>> GetAll(int size, int page, string sort, string search)
        {
            if (page <= 0) page = 1;
            if (size <= 0) size = 10;
            var (customers, totalData) = await _customerRepository.GetAll(size, page, sort, search);
            int totalPages = (int)Math.Ceiling((double)totalData / size);

            var response = new ApiResponse<List<Customer>>
            {
                Title = "OK",
                Status = 200,
                Payload = customers,
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

        public async Task<int> Save(CustomerDto customerDto)
        {
            var userRole = UserContextService.GetUserRole(); 
            var customerId = Guid.NewGuid().ToString();
            var customer = new Customer
            {
                Id = customerId,
                Name = customerDto.Name,
                PhoneNumber = customerDto.PhoneNumber,
                Email = customerDto.Email,
                Member = customerDto.Member
                //UserId = id user yang menabmah kan customer
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _customerRepository.Save(customer);

                if (userRole.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    Dictionary<string, object> dictWhere = new Dictionary<string, object>
                    {
                        { "role", "admin" }
                    };

                    var dataList = await _customService.GetDataListAsync(TableConstants.Users.TableName, dictWhere);

                    if (dataList != null && dataList.Any())
                    {
                        foreach (var rowData in dataList)
                        {
                            var id = rowData["id"].ToString();
                            var notification = new Notification
                            {
                                Title = "New Customer Added",
                                TableName = TableConstants.Customers.TableName,
                                Description = $"{customerDto.Name} has been added as a customer",
                                PathId = customerId,
                                CreatedBy = UserContextService.GetUserId(),
                                UserId = id
                            };
                            await _notificationService.Save(notification);
                        }
                    }
                }

                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<int> Update(string id, CustomerDto customerDto)
        {
            var userRole = UserContextService.GetUserRole(); // 👈 Panggil langsung tanpa instance
            if (string.IsNullOrEmpty(userRole))
            {
                throw new InvalidOperationException("User role cannot be null or empty.");
            }

            var customer = new Customer
            {
                Id = id,
                Name = customerDto.Name,
                PhoneNumber = customerDto.PhoneNumber,
                Email = customerDto.Email,
                Member = customerDto.Member,
                //UserId = _userContext.GetUserId() // Ambil ID pengguna yang sedang login
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _customerRepository.Update(id, customer);

                if (userRole.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    Dictionary<string, object> dictWhere = new Dictionary<string, object>
            {
                { "role", "admin" }
            };

                    var dataList = await _customService.GetDataListAsync(TableConstants.Users.TableName, dictWhere);

                    if (dataList != null && dataList.Any())
                    {
                        foreach (var rowData in dataList)
                        {
                            var adminId = rowData["id"].ToString();
                            var notification = new Notification
                            {
                                Title = "Customer Updated",
                                TableName = TableConstants.Customers.TableName,
                                Description = $"{customerDto.Name} has been updated",
                                PathId = id,
                                CreatedBy = UserContextService.GetUserId(),
                                UserId = adminId
                            };
                            await _notificationService.Save(notification);
                        }
                    }
                }

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

        public async Task<ApiResponse<Customer>> GetById(string id)
        {
            var customer = await _customerRepository.GetById(id);

            if (customer == null)
            {
                return new ApiResponse<Customer>
                {
                    Title = "Not Found",
                    Status = 404,
                    Payload = null
                };
            }

            return new ApiResponse<Customer>
            {
                Title = "OK",
                Status = 200,
                Payload = customer
            };
        }

        public async Task<int> DeleteById(string id)
        {
            return await _customerRepository.DeleteById(id);
        }
    }
}
