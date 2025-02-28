using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoMart.DTO;
using TokoMart.Services;

namespace TokoMart.Controllers
{
    [Route("api/discounts")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class DiscountController : ControllerBase
    {
        private readonly DiscountService _service;

        public DiscountController(DiscountService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetDiscountWithProduct(
            [FromQuery] int size = 0,
            [FromQuery] int page = 0,
            [FromQuery] string? sort = null,
            [FromQuery] string? filter = null
            )
        {
            var discounts = await _service.GetDiscountWithProduct(size, page, sort, filter);
            return Ok(discounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var discounts = await _service.GetById(id);
            if (discounts == null) return NotFound();
            return Ok(discounts);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] DiscountDto discountDto)
        {
            // Validasi manual dengan TryValidateObject
            var context = new ValidationContext(discountDto, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(discountDto, context, validationResults, true);

            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        ModelState.AddModelError(memberName, validationResult.ErrorMessage);
                    }
                }
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.Save(discountDto);
            return CreatedAtAction(nameof(GetById), new { id = result }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DiscountDto discountDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.Update(id, discountDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _service.DeleteById(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
