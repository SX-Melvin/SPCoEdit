using Microsoft.AspNetCore.Mvc;

namespace SPCoEdit.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoEditController : ControllerBase
    {
        [HttpGet("Start/NodeID/{nodeID}")]
        public string Get(long nodeID)
        {
            return "";
        }
    }
}
