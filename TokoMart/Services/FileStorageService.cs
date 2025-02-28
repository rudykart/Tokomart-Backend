using System;
using Microsoft.AspNetCore.Hosting.Server;
using TokoMart.DTO;
//using static TokoMart.Constants.TableConstants;

namespace TokoMart.Services
{
    public class FileStorageService
    {
        private readonly string _storageRoot;

        public FileStorageService(string storageRoot)
        {
            _storageRoot = storageRoot;
        }

        public async Task<Dictionary<string, string>> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            Console.WriteLine($"Nama File: {file.FileName}");
            Console.WriteLine($"Tipe File: {file.ContentType}");
            Console.WriteLine($"Ukuran File: {file.Length} bytes");

            //Console.WriteLine($"Debug Total Count  : {totalData}");
            Console.WriteLine($"masukk service file storage 1");
            Console.WriteLine($"root file : {Directory.GetCurrentDirectory()}");


            Console.WriteLine($"isi _storageRoot : {_storageRoot} ");
            // Buat folder penyimpanan jika belum ada
            if (!Directory.Exists(_storageRoot))
            {
                Console.WriteLine($"masukk service file storage 2");
                Directory.CreateDirectory(_storageRoot);
            }
            Console.WriteLine($"masukk service file storage 3");

            // Generate nama file unik
            //var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            // **Perbaikan * *: Bersihkan nama file(hapus spasi, ubah ke lowercase)
            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName); // Ambil nama tanpa ekstensi

            Console.WriteLine($"masukk service file storage 4");
            var extension = Path.GetExtension(file.FileName); // Ambil ekstensi file

            Console.WriteLine($"masukk service file storage 5");
            var cleanedFileName = originalFileName
                .Trim()               // Hapus spasi di awal & akhir
                .Replace(" ", "-")    // Ganti spasi dengan tanda strip
                .Replace("(", "")     // Hapus tanda kurung buka
                .Replace(")", "")     // Hapus tanda kurung tutup
                .ToLower();           // Ubah jadi huruf kecil

            // **Tambahkan GUID untuk menghindari duplikasi nama**
            var fileName = $"{Guid.NewGuid()}_{cleanedFileName}{extension}";

            Console.WriteLine($"masukk service file storage 6");
            var absoluteFilePath = Path.Combine(_storageRoot, fileName); // Path penuh di server
            Console.WriteLine($"masukk service file storage 7");
            var relativeFilePath = Path.Combine("uploads", fileName);   // Path relatif dari wwwroot

            //absoluteFilePath:
            //Menggunakan _storageRoot untuk menyimpan file secara fisik di server.

            //relativeFilePath:
            //Dimulai dari uploads / untuk menghilangkan path absolut(C:/ Users / ...).
            //Dibangun dengan asumsi uploads/ berada di dalam wwwroot /.

            // Simpan file
            Console.WriteLine($"masukk service file storage 8");
            using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Kembalikan dictionary dengan filePath dan fileName
            return new Dictionary<string, string>
            {
                { "fileName", fileName },
                { "filePath", relativeFilePath }
            };
        }

        public async Task DeleteByPath(string relativeFilePath)
        {
            // Bangun path absolut dari path relatif
            var absoluteFilePath = Path.Combine(_storageRoot, relativeFilePath.Replace("uploads\\", ""));

            // Hapus file jika ada
            if (File.Exists(absoluteFilePath))
            {
                Console.WriteLine($"Deleting file: {absoluteFilePath}");
                await Task.Run(() => File.Delete(absoluteFilePath));
            }
            else
            {
                Console.WriteLine($"File not found: {absoluteFilePath}");
                throw new FileNotFoundException("File not found", absoluteFilePath);
            }
        }

        public string GetFileUrl(string relativeFilePath)
        {
            // Ubah backslash (\) menjadi forward slash (/)
            var sanitizedPath = relativeFilePath.Replace("\\", "/");

            // Kembalikan URL yang dapat diakses
            return $"/{sanitizedPath}";
        }

        //public async Task DeleteAsync(string filePath)
        //{
        //    var fullPath = Path.Combine(_storageRoot, filePath);

        //    if (File.Exists(fullPath))
        //    {
        //        await Task.Run(() => File.Delete(fullPath));
        //    }
        //}

        //public string GetFileUrl(string filePath)
        //{
        //    // Contoh: kembalikan URL relatif
        //    return $"/uploads/{filePath}";
        //}
    }
}
