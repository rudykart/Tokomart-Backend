//using Microsoft.AspNetCore.Mvc.RazorPages;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class AttachmentService
    {
        private readonly IAttachmentRepository _attachmentRepository;

        public AttachmentService(IAttachmentRepository attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        public async Task<ApiResponse<List<Attachment>>> GetAllByFileId(string fileId, string tableName)
        {
            var (attachments, totalData) = await _attachmentRepository.GetAllByFileId( fileId, tableName);


            var response = new ApiResponse<List<Attachment>>
            {
                Title = "OK",
                Status = 200,
                Payload = attachments.ToList(),
                Meta = new MetaData
                {
                    TotalData = totalData
                }
            };

            return response;
        }
        public async Task<ApiResponse<Attachment>> GetById(string id)
        {
            var attachment = await _attachmentRepository.GetById(id);

            if (attachment == null)
            {

                //Directory.GetCurrentDirectory()

                return new ApiResponse<Attachment>
                {
                    Title = "Not Found",
                    Status = 404,
                    Payload = null
                };
            }

            return new ApiResponse<Attachment>
            {
                Title = "OK",
                Status = 200,
                Payload = attachment
            };
        }

        public async Task<int> Save(Attachment attachment)
        {
            var data = new Attachment
            {
                FileId = attachment.FileId,
                FilePath = attachment.FilePath,
                TableName = attachment.TableName

            };
            return await _attachmentRepository.Save(data);
        }

        public async Task<int> DeleteById(string id)
        {
            return await _attachmentRepository.DeleteById(id);
        }
        public async Task<int> DeleteByPath(string path)
        {
            return await _attachmentRepository.DeleteByPath(path);
        }
    }
}
