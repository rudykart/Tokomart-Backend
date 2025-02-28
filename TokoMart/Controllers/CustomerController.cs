using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoMart.DTO;
using TokoMart.Services;

namespace TokoMart.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _service;
        public CustomerController(CustomerService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int size = 0,
            [FromQuery] int page = 0,
            [FromQuery] string? sort = null,
            [FromQuery] string? filter = null
            )
        {
            var customers = await _service.GetAll(size, page, sort, filter);
            return Ok(customers);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetById(string id)
        {
            var customers = await _service.GetById(id);
            if (customers == null) return NotFound();
            return Ok(customers);
        }

        [HttpPost]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> Save([FromBody] CustomerDto customerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.Save(customerDto);
            return CreatedAtAction(nameof(GetById), new { id = result }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(string id, [FromBody] CustomerDto customerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.Update(id, customerDto);
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
