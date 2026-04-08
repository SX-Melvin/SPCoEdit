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
        public APIResponse<string> Get([FromBody] CoEditRequest body)
        {
            return service.StartCoEdit(body.NodeID, body.Version, body.FileName);
        }
    }
}
