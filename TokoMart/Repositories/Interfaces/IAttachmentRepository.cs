using TokoMart.Models;

namespace TokoMart.Repositories.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<(List<Attachment> Attachments, int TotalData)> GetAllByFileId(string fileId, string tableName);
        Task<List<Attachment>> GetAllByFileId(string fileId);
        Task<int> SaveRange(List<Attachment> attachment);
        Task<int> Save(Attachment attachment);
        //Task<int> Update(string id, Attachment attachment);
        Task<Attachment> GetById(string id);
        Task<int> DeleteById(string id);
        Task<int> DeleteByPath(string path);
    }
}
