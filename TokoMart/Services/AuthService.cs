using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        //public async Task<TokenDto> Login(LoginDto loginDto)
        public async Task<ApiResponse<TokenDto>> Login(LoginDto loginDto)
        {
            User user = await _userRepository.FindByUsername(loginDto.Username);

            if (user == null)
            {
                return null;
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, loginDto.Password) == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var token = GenerateJwtToken(user);

            return new ApiResponse<TokenDto>
            {
                Title = "OK",
                Status = 200,
                Payload = new TokenDto
                {
                    AccessToken = token,
                    RefreshToken = GenerateRefreshToken(),
                    Role = user.Role,
                    Name = user.Name,
                    UserId = user.Id
                }
            };
        }

        public async Task<int> Register(RegisterDto registerDto)
        {
            // Hash password yang diterima dari UserDto
            var passwordHasher = new PasswordHasher<RegisterDto>();
            var hashPassword = passwordHasher.HashPassword(registerDto, registerDto.Password);


            User user = await _userRepository.FindByUsername(registerDto.Username);
            if (user != null)
            {
                return 0;
            }

            var data = new User
            {
                Name = registerDto.Name,
                Username = registerDto.Username,
                Password = hashPassword, 
                Role = "admin"
            };

            return await _userRepository.Save(data);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                //new Claim(ClaimTypes.UserData, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetValue<string>("AppSettings:Token")!)
                );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<string>(("AppSettings:Issuer")),
                audience: _config.GetValue<string>(("AppSettings:Audience")),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public string GenerateRefreshToken()
        {
            // Create a 32-byte array to hold cryptographically secure random bytes
            var randomNumber = new byte[32];

            // Use a cryptographically secure random number generator 
            // to fill the byte array with random values
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);

            // Convert the random bytes to a base64 encoded string 
            return Convert.ToBase64String(randomNumber);
        }
    }
}
