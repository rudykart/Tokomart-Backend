using System.Security.Claims;

namespace TokoMart.Services
{
    public static class UserContextService
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static string GetUserId()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? throw new UnauthorizedAccessException("User ID not found in token.");
        }

        public static string GetUserRole()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Role)?.Value
                   ?? throw new UnauthorizedAccessException("User Role not found in token.");
        }
        //private readonly IHttpContextAccessor _httpContextAccessor;

        //public UserContextService(IHttpContextAccessor httpContextAccessor)
        //{
        //    _httpContextAccessor = httpContextAccessor;
        //}

        // untuk mendapatkan user id
        //public string GetUserId()
        //{
        //    var user = _httpContextAccessor.HttpContext?.User;
        //    return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //           ?? throw new UnauthorizedAccessException("User ID not found in token.");
        //}

        //public string GetUseRole()
        //{
        //    var user = _httpContextAccessor.HttpContext?.User;
        //    return user?.FindFirst(ClaimTypes.Role)?.Value
        //           ?? throw new UnauthorizedAccessException("User Role not found in token.");
        //}
    }
}
