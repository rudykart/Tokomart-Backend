using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Services;

namespace TokoMart.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _authService.Register(registerDto);
            if (success == 0)
            {
                return BadRequest("User registration failed.");
            }

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _authService.Login(loginDto);
            if (token == null)
            {
                //return Unauthorized("Invalid credentials.");
                var problemDetails = new ValidationProblemDetails
                {
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Invalid username or password.",
                };

                //problemDetails.Errors.Add("username", new[] { "The Username field is required or incorrect." });
                //problemDetails.Errors.Add("password", new[] { "The Password field is required or incorrect." });

                return Unauthorized(problemDetails);
            }

            return Ok(token);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthOnly()
        {
            return Ok("You are autenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AuthAdminOnly()
        {
            return Ok("You are Admin autenticated");
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var username = User.Identity?.Name;
            var name = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (username == null || name == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                Username = username,
                Name = name,
                Role = role
            });
        }
    }
}
