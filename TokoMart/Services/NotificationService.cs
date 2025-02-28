using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetAllByUserId(int size, string? cursor, string? search, string userId, bool? hasRead)
        {
            var result = await _notificationRepository.GetAllByUserId( size, "DESC",  cursor, search, userId, hasRead);

            var dataList = result["notifications"] as List<Dictionary<string, object>> ?? new();
            var totalData = Convert.ToInt32(result["total_data"]);
            var nextCursor = result["next_cursor"] as string;

            var response = new ApiResponse<List<Dictionary<string, object>>>
            {
                Title = "OK",
                Status = 200,
                Payload = dataList,
                Meta = new MetaData
                {
                    TotalData = totalData,
                    PageSize = size,
                    NextCursor = nextCursor
                }
            };

            return response;
        }

        public async Task<int> Read(string id)
        {
            return await _notificationRepository.Read(id);
        }

        public async Task<int> ReadAllByUserId(string userId)
        {
            return await _notificationRepository.ReadAllByUserId(userId);
        }

        public async Task<int> Save(Notification notification)
        {
            return await _notificationRepository.Save(notification);
        }


        public async Task<ApiResponse<Dictionary<string, object>>> CountDataByUserId(bool? hasRead)
        {
            var result = await _notificationRepository.CountDataByUserId(UserContextService.GetUserId(), hasRead);


            var response = new ApiResponse<Dictionary<string, object>>
            {
                Title = "OK",
                Status = 200,
                Payload = new Dictionary<string, object>
        {
            { "count_data", result }
        }
            };

            return response;
        }
    }
}
