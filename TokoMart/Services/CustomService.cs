using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class CustomService
    {
        private readonly ICustomRepository _customRepository;

        public CustomService(ICustomRepository customRepository)
        {
            _customRepository = customRepository;
        }

        public async Task<string?> GetIdByTableNameAsync(string tableName, Dictionary<string, object>? mapWhere = null)
        {
            return await _customRepository.GetIdByTableNameAsync(tableName, mapWhere);
        }

        public async Task<string?> GetNameByIdAsync(string tableName, string id, Dictionary<string, object>? mapWhere = null)
        {
            return await _customRepository.GetNameByIdAsync(tableName, id, mapWhere);
        }

        public async Task<List<Dictionary<string, object>>> GetDataListAsync(string tableName, Dictionary<string, object>? mapWhere = null)
        {
            var dataList = await _customRepository.GetDataListAsync(tableName, mapWhere);
            return dataList;
        }
        
        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetDataListResponseAsync(string tableName, Dictionary<string, object>? dicWhere = null)
        {
            var dataList = await _customRepository.GetDataListAsync(tableName, dicWhere);

            var response = new ApiResponse<List<Dictionary<string, object>>>
            {
                Title = "OK",
                Status = 200,
                Payload = dataList
            };

            return response;
        }

    }

}
