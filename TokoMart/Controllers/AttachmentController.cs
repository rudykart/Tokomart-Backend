using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoMart.Services;

namespace TokoMart.Controllers
{
    [Route("api/attachments")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly AttachmentService _service;

        public AttachmentController(AttachmentService service)
        {
            _service = service;
        }


        [HttpGet("{id}/{tableName}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetAll(string id, string tableName)
        {
            var data = await _service.GetAllByFileId(id, tableName);
            return Ok(data);
        }
    }
}
