﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoMart.DTO;
using TokoMart.Services;

namespace TokoMart.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;
        public UserController(UserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int size = 0,
            [FromQuery] int page = 0,
            [FromQuery] string? sort = null,
            [FromQuery] string? filter = null
            )
        {
            var users = await _service.GetAll(size, page, sort, filter);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var classifications = await _service.GetById(id);
            if (classifications == null) return NotFound();
            return Ok(classifications);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.Save(userDto);
            return CreatedAtAction(nameof(GetById), new { id = result }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.Update(id, userDto);
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
