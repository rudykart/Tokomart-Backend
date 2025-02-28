using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TokoMart.Data;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories;
using TokoMart.Repositories.Interfaces;
using TokoMart.Utils;

namespace TokoMart.Services
{
    public class ProductService
    {

        private readonly IProductRepository _productRepository;
        private readonly ICustomRepository _customRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly FileStorageService _fileStorageService;
        private readonly AppDbContext _context;

        public ProductService(IProductRepository productRepository, ICustomRepository customRepository, IAttachmentRepository attachmentRepository, FileStorageService fileStorageService, AppDbContext context)
        {
            _productRepository = productRepository;
            _customRepository = customRepository;
            _attachmentRepository = attachmentRepository;
            _fileStorageService = fileStorageService;
            _context = context;
        }

        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetProductsWithCategoryAndMainImage(int size, string? sort, string? cursor, string? search)
        {
            try
            {
                if (size == 0)
                {
                    size = 10;
                }
                //size = Math.Max(size, 10); // Pastikan size minimal 10 untuk default pagination

                var result = await _productRepository.GetProductsWithCategoryAndMainImage(size, sort, cursor, search);

                var dataList = result.TryGetValue("products", out var productsObj) ? productsObj as List<Dictionary<string, object>> ?? new List<Dictionary<string, object>>() : new List<Dictionary<string, object>>();
                var totalData = result.TryGetValue("totalData", out var totalDataObj) && totalDataObj != null ? Convert.ToInt32(totalDataObj) : 0;
                var nextCursor = result.TryGetValue("nextCursor", out var nextCursorObj) ? nextCursorObj as string : null;
                var prevCursor = result.TryGetValue("prevCursor", out var prevCursorObj) ? nextCursorObj as string : null;

                return new ApiResponse<List<Dictionary<string, object>>>
                {
                    Title = "OK",
                    Status = 200,
                    Payload = dataList,
                    Meta = new MetaData
                    {
                        TotalData = totalData,
                        PageSize = size,
                        PrevCursor = prevCursor,
                        NextCursor = nextCursor
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                return new ApiResponse<List<Dictionary<string, object>>>
                {
                    Title = "Error",
                    Status = 500,
                    Payload = new List<Dictionary<string, object>>(),
                    Meta = new MetaData
                    {
                        TotalData = 0,
                        PageSize = size,
                        NextCursor = null
                    }
                    //ErrorMessage = "Terjadi kesalahan dalam mengambil data produk."
                };
            }
        }

        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetProductsWithCategoryAndMainImage(int size, int page, string sort, string search)
        {
            if (page <= 0) page = 1;
            if (size <= 0) size = 10;

            var result = await _productRepository.GetProductsWithCategoryAndMainImage(size, page, sort, search);

            var dataList = result["products"] as List<Dictionary<string, object>>;
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

        public async Task<int> Save(ProductDto productDTO)
        {
            return await Save("", productDTO);
        }

        public async Task<int> Save(string id,ProductDto productDTO)
        {
            var data = new Product
            {
                Id=id,
                //MainImage = productDTO.MainImage,
                Name = productDTO.Name,
                Category = productDTO.Category,
                Price = productDTO.Price,
                Stock = productDTO.Stock

            };
            return await _productRepository.Save(data);
        } 

        public async Task<int> SaveProduct(ProductDto productDTO, List<IFormFile>? files = null)
        {
            string id = Guid.NewGuid().ToString();
            string fileName = "";
            int i = 0;

            Console.WriteLine($"productDTO.MainImage = {productDTO.MainImage}");
            Console.WriteLine("productDTO.MainImage = "+productDTO.MainImage);
            Console.WriteLine("productDTO.productDTO.Name, = " + productDTO.Name);

            // Mulai transaksi
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (files != null && files.Count > 0) { 
                        foreach (var file in files)
                        {
                            // Proses upload file ke storage
                            var fileUpload = await _fileStorageService.UploadAsync(file);
                            //if (i == 0 )
                            //{
                            //    fileName = fileUpload["fileName"];
                            //}


                            if (i == 0 && productDTO.MainImage == "")
                            {
                                fileName = fileUpload["fileName"];
                            }
                            else if (i == int.Parse(productDTO.MainImage) && productDTO.MainImage != "")
                            {
                                fileName = fileUpload["fileName"];
                            }

                            // Tambahkan lampiran ke daftar
                            var attachment = new Attachment
                            {
                                FilePath = fileUpload["filePath"],
                                FileName = fileUpload["fileName"],
                                FileId = id,
                                FileType = "image",
                                TableName = TableConstants.Products.TableName
                            };

                            await _attachmentRepository.Save(attachment);
                            i++;
                        }
                    }

                    var data = new Product
                    {
                        Id = id,

                        MainImage = (files != null && files.Count > 0) ? fileName : "",
                        Name = productDTO.Name,
                        Category = productDTO.Category,
                        Price = productDTO.Price,
                        Stock = productDTO.Stock

                    };

                    int result = await _productRepository.Save(data);

                    await transaction.CommitAsync();

                    return result;

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error: {ex.Message}");
                    return 0;
                }
            }
        }

        public async Task<int> UpdateProduct(string id, ProductDto productDTO, List<IFormFile>? files = null, List<string>? deleteFilePaths = null)
        //public async Task<int> UpdateProduct(string id, ProductDto productDTO, List<IFormFile>? files = null, string[] deleteFilePaths = null)
        {
            // Mulai transaksi database
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingProduct = await _productRepository.GetById(id);
                if (existingProduct == null)
                {
                    return 0; 
                }


                if (deleteFilePaths != null && deleteFilePaths.Count > 0)
                //if (deleteFilePaths != null && deleteFilePaths.Length > 0)
                {

                        //for (int i = 0; i < deleteFilePaths.Length; i++)
                    foreach (string filePath in deleteFilePaths)
                    {
                        // Hapus data attachment di database terlebih dahulu
                        await _attachmentRepository.DeleteByPath(filePath);
                        //await _attachmentRepository.DeleteByPath(deleteFilePaths[i]);

                        // Baru hapus file dari storage
                        await _fileStorageService.DeleteByPath(filePath);
                        //await _fileStorageService.DeleteByPath(deleteFilePaths[i]);
                    }
                }

                if (files != null && files.Any())
                {
                    var newAttachments = new List<Attachment>();

                    foreach (var file in files)
                    {
                        var fileUpload = await _fileStorageService.UploadAsync(file);
                        if (fileUpload == null || !fileUpload.ContainsKey("filePath"))
                        {
                            await transaction.RollbackAsync();
                            return 0; 
                        }

                        newAttachments.Add(new Attachment
                        {
                            Id = Guid.NewGuid().ToString(),
                            FilePath = fileUpload["filePath"],
                            FileName = fileUpload["fileName"],
                            FileId = id,
                            FileType = "image",
                            TableName = TableConstants.Products.TableName
                        });
                    }

                    await _attachmentRepository.SaveRange(newAttachments);
                }


                //Task<(List<Attachment> Attachments, int TotalData)> GetAllByFileId(string fileId, string tableName);

                //existingProduct.MainImage = productDTO.MainImage;
                if (productDTO.MainImage!="")
                {
                List<Attachment> listAttachments =  await _attachmentRepository.GetAllByFileId(id);
                existingProduct.MainImage = listAttachments[int.Parse(productDTO.MainImage)].FileName;
                }
                existingProduct.Name = productDTO.Name;
                existingProduct.Category = productDTO.Category;
                existingProduct.Price = productDTO.Price;
                existingProduct.Stock = productDTO.Stock;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await _productRepository.Update(id, existingProduct);

                await transaction.CommitAsync();
                return 1;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }


        public async Task<int> AddStock(string id, int addStok)
        {
            var existingProduct = await _productRepository.GetById(id);
            if (existingProduct == null)
            {
                return 0; 
            }
            var data = new Product
            {
                Stock = existingProduct.Stock + addStok

            };
            return await _productRepository.UpdateStock(id, addStok);
        }

        public async Task<ApiResponse<Product>> GetById(string id)
        {
            var data = await _productRepository.GetById(id);

            if (data == null)
            {
                return new ApiResponse<Product>
                {
                    Title = "Not Found",
                    Status = 404,
                    Payload = null
                };
            }

            return new ApiResponse<Product>
            {
                Title = "OK",
                Status = 200,
                Payload = data
            };
        }

        public async Task<int> DeleteById(string id)
        {
            var att = await _attachmentRepository.GetAllByFileId(id,TableConstants.Products.TableName);

            List<Attachment> listAttachments = att.Attachments;

            foreach (Attachment attachment in listAttachments)
            {
                await _fileStorageService.DeleteByPath(attachment.FilePath);
                await _attachmentRepository.DeleteByPath(attachment.FilePath);
            }

            return await _productRepository.DeleteById(id);
        }

        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetDataListAsync()
        {
            Dictionary<string, object> dicWhere = new Dictionary<string, object>();
            dicWhere.Add(TableConstants.Classifications.FieldName, "category");
            var dataList = await _customRepository.GetDataListAsync(TableConstants.Classifications.TableName, dicWhere);

            var response = new ApiResponse<List<Dictionary<string, object>>>
            {
                Title = "OK",
                Status = 200,
                Payload = dataList
            };

            return response;
        }

        public async Task<ApiResponse<List<Dictionary<string, object>>>> GetAllProductForDropDown(int size, string? cursor, string? search)
        {
            //if (size <= 0) size = 10;

            var result = await _customRepository.GetDataListAsync(TableConstants.Products.TableName, size , cursor, search);

            var dataList = result["data"] as List<Dictionary<string, object>> ?? new();
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
    }
}
