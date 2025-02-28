using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using TokoMart.Data;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;

namespace TokoMart.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {

        private readonly AppDbContext _context;

        public AttachmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Attachment> Attachments, int TotalData)> GetAllByFileId(string fileId, string tableName)
        {
            // Query utama untuk mengambil data
            var queryBuilder = new StringBuilder("SELECT * FROM attachments WHERE file_id = @fileId AND table_name = @tableName");

            // Query untuk menghitung total data
            var countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM attachments WHERE file_id = @fileId AND table_name = @tableName");

            // Membuat parameter query
            var parameters = new List<NpgsqlParameter>
    {
        new NpgsqlParameter("@fileId", fileId), // Menggunakan parameter fileId
        new NpgsqlParameter("@tableName", tableName) // Menggunakan parameter tableName
    };

            // Eksekusi query untuk mendapatkan attachments
            var attachments = await _context.Attachments
                .FromSqlRaw(queryBuilder.ToString(), parameters.ToArray())
                .ToListAsync();

            // Eksekusi query untuk mendapatkan total data
            var totalData = await _context.Database
                .ExecuteSqlRawAsync(countQueryBuilder.ToString(), parameters.ToArray());

            return (attachments, totalData);
        }
        
        public async Task<List<Attachment> > GetAllByFileId(string fileId)
        {
            // Query utama untuk mengambil data
            var queryBuilder = new StringBuilder("SELECT * FROM attachments WHERE file_id = @fileId");

            // Membuat parameter query
            var parameters = new List<NpgsqlParameter>
    {
        new NpgsqlParameter("@fileId", fileId), // Menggunakan parameter fileId
    };

            // Eksekusi query untuk mendapatkan attachments
            var attachments = await _context.Attachments
                .FromSqlRaw(queryBuilder.ToString(), parameters.ToArray())
                .ToListAsync();


            return attachments;
        }


        public async Task<Attachment> GetById(string id)
        {
            string query = "SELECT * FROM attachments WHERE id = {0}";
            return await _context.Attachments.FromSqlRaw(query, id).FirstOrDefaultAsync();
        }

        public async Task<int> Save(Attachment attachment)
        {
            string query = "INSERT INTO attachments (id, table_name, file_id, file_type, file_path, file_name, created_at, updated_at ) " +
                             "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})";
            return await _context.ExecuteNativeCommandAsync(query, Guid.NewGuid(), attachment.TableName, attachment.FileId, attachment.FileType, attachment.FilePath, attachment.FileName,  DateTime.UtcNow, DateTime.UtcNow);
        }

        public async Task<int> SaveRange(List<Attachment> attachments)
        {
            if (attachments == null || attachments.Count == 0)
            {
                return 0;
            }

            var values = new List<string>();
            var parameters = new List<object>();
            DateTime now = DateTime.UtcNow;
            int index = 0;

            foreach (var pt in attachments)
            {
                pt.Id = Guid.NewGuid().ToString();
                pt.CreatedAt = now;
                pt.UpdatedAt = now;

                values.Add($"(@p{index}, @p{index + 1}, @p{index + 2}, @p{index + 3}, @p{index + 4}, @p{index + 5}, @p{index + 6}, @p{index + 7})");

                parameters.Add(pt.Id);
                parameters.Add(pt.TableName);
                parameters.Add(pt.FileId);
                parameters.Add(pt.FileType);
                parameters.Add(pt.FilePath);
                parameters.Add(pt.FileName);
                parameters.Add(pt.CreatedAt);
                parameters.Add(pt.UpdatedAt);

                index += 8;
            }

            string query = $@"
                INSERT INTO attachments (id, table_name, file_id, file_type, file_path, file_name, created_at, updated_at)
                VALUES {string.Join(", ", values)}";

            return await _context.ExecuteNativeCommandAsync(query, parameters.ToArray());
        }


        public async Task<int> DeleteById(string id)
        {
            string query = "DELETE FROM attachments WHERE id = {0}";
            return await _context.ExecuteNativeCommandAsync(query, id);
        }
        public async Task<int> DeleteByPath(string path)
        {
            string query = "DELETE FROM attachments WHERE file_path = {0}";
            return await _context.ExecuteNativeCommandAsync(query, path);
        }

    }
}
