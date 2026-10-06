using Microsoft.AspNetCore.Mvc;
using SPCoEdit.Dto;
using SPCoEdit.Dto.CoEdit;
using SPCoEdit.Service;

namespace SPCoEdit.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoEditController(CoEditService service) : ControllerBase
    {
        [HttpPost("Start")]
        public APIResponse<string> Start([FromBody] CoEditRequest body)
        {
            string? callerIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return service.StartCoEdit(body, callerIp);
        }

        [HttpPost("Stop/{fileName}")]
        public APIResponse<string> Stop(string fileName)
        {
            string? callerIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return service.StopCoEdit(fileName, callerIp);
        }
    }
}
