using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using TokoMart.Constants;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Services;
using TokoMart.Utils;

namespace TokoMart.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Authorize(Roles = "admin,user")]
        // GET http://your-api-url/products?size=10&page=1&sort=name_asc&filter=category:electronics
        public async Task<IActionResult> GetAll(
            [FromQuery] int size = 0,
            [FromQuery] int page = 0,
            [FromQuery] string? sort = null,
            [FromQuery] string? filter = null
            )
        {
            var products = await _productService.GetProductsWithCategoryAndMainImage(size, page, sort, filter);
            return Ok(products);
        }

        [HttpGet("transaction")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetAll(
    [FromQuery] int size = 0, // Default size 10
    [FromQuery] string? sort = null,
    [FromQuery] string? cursor = null,
    [FromQuery] string? filter = null
)
        {

            var response = await _productService.GetProductsWithCategoryAndMainImage(size, sort, cursor, filter);

            if (response.Status != 200)
            {
                return StatusCode(response.Status, response);
            }

            return Ok(response);
        }


        [HttpGet("category/list")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetCategory()
        {
            var products = await _productService.GetDataListAsync();

            return Ok(products);
        }



        [HttpGet("dropdown")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetAllProductForDropDown(
            [FromQuery] int size = 0,
            [FromQuery] string? cursor = null,
            [FromQuery] string? search = null)
        {
            var products = await _productService.GetAllProductForDropDown(size, cursor, search);
            return Ok(products);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetById(string id)
        {
            var products = await _productService.GetById(id);
            if (products == null) return NotFound();
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SaveProductWithAttachments([FromForm] ProductDto productDto, [FromForm] List<IFormFile>? files = null, [FromForm(Name = "index_main_image")] string? indexMainImage = null)
        {
            if (indexMainImage != null)
            {
                productDto.MainImage = indexMainImage;
                Console.WriteLine("\n productDTO.MainImage controller = " + productDto.MainImage);
            }
            if (productDto.MainImage == null)
            {
                Console.WriteLine("\n productDTO.MainImage controller = null");
            }
            if (productDto.MainImage == "")
            {
                Console.WriteLine("\n productDTO.MainImage controller = ..");
            }

            if (files != null && files.Count > 0)
            {
                if (files.Count > 5)
                {
                    return BadRequest(new { message = "Maximum of 5 images are allowed per product." });
                }

                foreach (var file in files)
                {
                    var validationMessage = FileUploadHelper.ValidateFile(file);
                    if (!string.IsNullOrEmpty(validationMessage))
                    {
                        return BadRequest(new { message = validationMessage });
                    }
                }
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);


            var saveProductResult = await _productService.SaveProduct(productDto, files);

            if (saveProductResult > 0)
            {
                return CreatedAtAction(nameof(SaveProductWithAttachments), new { id = saveProductResult }, new { message = "Product saved successfully." });
            }

            return StatusCode(500, new { message = "An error occurred while saving the product." });
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateProductWithAttachments(
            [FromRoute] string id,
            [FromForm] ProductDto productDto,
            [FromForm] List<IFormFile>? files = null,
            //[FromForm(Name = "delete_file_paths")] Object deleteFilePaths = null)
            //[FromForm(Name = "delete_file_paths")] string[] deleteFilePaths = null)
            [FromForm(Name = "delete_file_paths")] string? deleteFilePaths = null
            , [FromForm(Name = "index_main_image")] string? indexMainImage = null)

        {
            if (indexMainImage != null)
            {
                productDto.MainImage = indexMainImage;
                Console.WriteLine("\n productDTO.MainImage controller = " + productDto.MainImage);
            }

            List<string> deleteFilePathsList = new List<string>();
            if (deleteFilePaths != null)
            {
                Console.WriteLine("Count data foto delete = " + deleteFilePaths);
                //List<string> deleteFilePathsList = new List<string>(deleteFilePaths.Split(','));
                deleteFilePathsList = new List<string>(deleteFilePaths.Split(','));

                if (deleteFilePathsList != null)
                {
                    foreach (string item in deleteFilePathsList)
                    {
                        Console.WriteLine("delete path file = " + item);
                    }
                }
            }

            if (files != null && files.Count > 0)
            {
                // Validasi maksimal 5 file
                if (files.Count > 5)
                {
                    return BadRequest(new { message = "Maximum of 5 images are allowed per product." });
                }

                foreach (var file in files)
                {
                    // Validasi file menggunakan helper
                    var validationMessage = FileUploadHelper.ValidateFile(file);
                    if (!string.IsNullOrEmpty(validationMessage))
                    {
                        return BadRequest(new { message = validationMessage });
                    }
                }
            }

            // Simpan produk ke database
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //var updateProductResult = await _productService.UpdateProduct(id, productDto, files, null);
            var updateProductResult = await _productService.UpdateProduct(id, productDto, files, deleteFilePathsList);

            if (updateProductResult > 0)
            {
                return Ok(new { message = "Product updated successfully." });
            }

            return StatusCode(500, new { message = "An error occurred while updated the product." });
        }

        [HttpPost("{id}/add-stock")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> AddStock(string id, [FromBody] ProductStockDto request)
        {
            try
            {
                var result = await _productService.AddStock(id, request.AddStock);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _productService.DeleteById(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
