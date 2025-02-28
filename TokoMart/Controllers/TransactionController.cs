using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoMart.DTO;
using TokoMart.Services;

namespace TokoMart.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    [Authorize(Roles = "admin,user")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int size = 0,
            [FromQuery] int page = 0,
            [FromQuery] string? sort = null,
            [FromQuery] string? filter = null
            )
        {
            var response = await _transactionService.GetAll(size, page, sort, filter);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await _transactionService.GetById(id);
            return StatusCode(response.Status, response);
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto transactionDto)
        {
            // mengambil id dari token 
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Received Transaction DTO:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(transactionDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            if (transactionDto == null || transactionDto.Products == null || transactionDto.Products.Count == 0)
            {
                return BadRequest(new { message = "Invalid transaction data" });
            }

            int result = await _transactionService.Save(transactionDto, userId);

            if (result == 1)
            {
                return Ok(new { message = "Transaction saved successfully" });
            }

            return BadRequest(new { message = "Failed to save transaction" });
        }
    }
}
