using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class DiscountService
    {
        private readonly IDiscountRepository _discountRepository;

        public DiscountService(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository;
        }

        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetDiscountWithProduct(int size, int page, string sort, string search)
        {
            if (page <= 0) page = 1;
            if (size <= 0) size = 10;

            var result = await _discountRepository.GetDiscountWithProduct(size, page, sort, search);

            var dataList = result["discounts"] as List<Dictionary<string, object>>;
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
            var result = await _discountRepository.GetById(id);

            var response = new ApiResponse<Dictionary<string, object>>
            {
                Title = "OK",
                Status = 200,
                Payload = result
            };

            return response;
        }

        public async Task<int> Save(DiscountDto discountDto)
        {
            var data = new Discount
            {
                Id = Guid.NewGuid().ToString(),
                DiscountValue = discountDto.DiscountValue,
                ProductId = discountDto.ProductId,
                StartAt = discountDto.StartAt,
                ExpiredAt = discountDto.ExpiredAt

            };
            return await _discountRepository.Save(data);
        }

        public async Task<int> Update(string id, DiscountDto discountDto)
        {
            var data = new Discount
            {
                DiscountValue = discountDto.DiscountValue,
                ProductId = discountDto.ProductId,
                StartAt = discountDto.StartAt,
                ExpiredAt = discountDto.ExpiredAt

            };
            return await _discountRepository.Update(id, data);
        }

        public async Task<int> DeleteById(string id)
        {
            return await _discountRepository.DeleteById(id);
        }
    }
}

