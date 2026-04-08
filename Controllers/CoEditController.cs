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
            return service.StartCoEdit(body.NodeID, body.Version, body.FileName);
        }

        [HttpPost("Stop/{fileName}")]
        public APIResponse<string> Stop(string fileName)
        {
            return service.StopCoEdit(fileName);
        }
    }
}
