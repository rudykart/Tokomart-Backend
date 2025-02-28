using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoMart.Services;

namespace TokoMart.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications(
            [FromQuery] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? cursor = null)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("user id = " + userId.ToString());
            var response = await _notificationService.GetAllByUserId(size, cursor, search, userId.ToString(), null);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Get unread notifications (has_read = false) for a user.
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications(
            [FromQuery] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? cursor = null)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _notificationService.GetAllByUserId(size, cursor, search, userId, false);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Get read notifications (has_read = true) for a user.
        /// </summary>
        [HttpGet("read")]
        public async Task<IActionResult> GetReadNotifications(
            [FromQuery] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? cursor = null)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _notificationService.GetAllByUserId(size, cursor, search, userId, true);
            return StatusCode(response.Status, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ReadById(string id)
        {
            var notification = await _notificationService.Read(id);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

        [HttpGet("read-all")]
        public async Task<IActionResult> ReadAllByUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var notification = await _notificationService.ReadAllByUserId(userId);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

        [HttpGet("count-all")]
        public async Task<IActionResult> CountDataByUserId()
        {
            var notification = await _notificationService.CountDataByUserId(null);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

        [HttpGet("count-all-unread")]
        public async Task<IActionResult> CountDataByUserIdUnread()
        {
            var notification = await _notificationService.CountDataByUserId(false);
            if (notification == null) return NotFound();
            return Ok(notification);
        }
        [HttpGet("count-all-read")]
        public async Task<IActionResult> CountDataByUserIdread()
        {
            var notification = await _notificationService.CountDataByUserId(true);
            if (notification == null) return NotFound();
            return Ok(notification);
        }
    }
}
