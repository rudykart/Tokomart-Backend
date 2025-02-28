using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class ClassificationService
    {
        private readonly IClassificationRepository _classificationRepository;
        public ClassificationService(IClassificationRepository classificationRepository)
        {
            _classificationRepository = classificationRepository;
        }

        public async Task<ApiResponse<List<Classification>>> GetAll()
        {
            return await GetAll(0, 0, "", "");
        }

        public async Task<ApiResponse<List<Classification>>> GetAll(int size, int page)
        {
            return await GetAll(size, page, "", "");
        }

        public async Task<ApiResponse<List<Classification>>> GetAll(int size, int page, string sort, string search)
        {
            if (page <= 0) page = 1;
            if (size <= 0) size = 10;
            var (classifications, totalData) = await _classificationRepository.GetAll(size, page, sort, search);
            int totalPages = (int)Math.Ceiling((double)totalData / size);

            var response = new ApiResponse<List<Classification>>
            {
                Title = "OK",
                Status = 200,
                Payload = classifications,
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

        public async Task<int> Save(ClassificationDto classificationDto)
        {
            var data = new Classification
            {
                Name = classificationDto.Name,
                Description = classificationDto.Description,
                TableName = classificationDto.TableName,
                FieldName = classificationDto.FieldName

            };
            return await _classificationRepository.Save(data);
        }
        
        public async Task<int> Update(string id, ClassificationDto classificationDto)
        {
            var data = new Classification
            {
                Id = id,
                Name = classificationDto.Name,
                Description = classificationDto.Description,
                TableName = classificationDto.TableName,
                FieldName = classificationDto.FieldName

            };
            return await _classificationRepository.Update(id,data);
        }

        public async Task<ApiResponse<Classification>> GetById(string id)
        {
            var classification = await _classificationRepository.GetById(id);

            if (classification == null)
            {
                return new ApiResponse<Classification>
                {
                    Title = "Not Found",
                    Status = 404,
                    Payload = null
                };
            }

            return new ApiResponse<Classification>
            {
                Title = "OK",
                Status = 200,
                Payload = classification
            };
        }

        public async Task<int> DeleteById(string id)
        {
            return await _classificationRepository.DeleteById(id);
        }

    }
}
