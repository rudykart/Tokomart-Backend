using Microsoft.AspNetCore.Identity;
using TokoMart.DTO;
using TokoMart.Models;
using TokoMart.Repositories;
using TokoMart.Repositories.Interfaces;

namespace TokoMart.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<ApiResponse<List<User>>> GetAll(int size, int page, string sort, string search)
        {
            if (page <= 0) page = 1;
            if (size <= 0) size = 10;
            var (users, totalData) = await _userRepository.GetAll(size, page, sort, search);
            int totalPages = (int)Math.Ceiling((double)totalData / size);

            var response = new ApiResponse<List<User>>
            {
                Title = "OK",
                Status = 200,
                Payload = users,
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

        public async Task<int> Save(UserDto userDto)
        {
            User user = await _userRepository.FindByUsername(userDto.Username);
            if (user != null)
            {
                return 0;
            }

            var passwordHasher = new PasswordHasher<UserDto>();
            var hashPassword = passwordHasher.HashPassword(userDto, userDto.Password);
            var data = new User
            {
                Name = userDto.Name,
                Username = userDto.Username,
                Password = hashPassword,
                Role = userDto.Role

            };
            return await _userRepository.Save(data);
        }

        public async Task<int> Update(string id, UserDto userDto)
        {
            User data = new User
            {
                Id = id,
                Name = userDto.Name,
                Username = userDto.Username,
                Role = userDto.Role

            };

            if (userDto.Password != "" || userDto.Password != null)
            {
                var passwordHasher = new PasswordHasher<UserDto>();
                var hashPassword = passwordHasher.HashPassword(userDto, userDto.Password);
                data.Password = hashPassword;
            }
            return await _userRepository.Update(id, data);
        }

        public async Task<ApiResponse<User>> GetById(string id)
        {
            var classification = await _userRepository.GetById(id);

            if (classification == null)
            {
                return new ApiResponse<User>
                {
                    Title = "Not Found",
                    Status = 404,
                    Payload = null
                };
            }

            return new ApiResponse<User>
            {
                Title = "OK",
                Status = 200,
                Payload = classification
            };
        }

        public async Task<int> DeleteById(string id)
        {
            return await _userRepository.DeleteById(id);
        }
    }
}
