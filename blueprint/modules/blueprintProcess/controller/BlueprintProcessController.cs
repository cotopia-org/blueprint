using blueprint.modules.blueprintlog.logic;
using blueprint.modules.blueprintlog.response;
using Microsoft.AspNetCore.Mvc;
using srtool;

namespace blueprint.modules.blueprintProcess.controller
{
    [Tags("Blueprint process")]
    [ApiController]
    [Route("v1/blueprints")]
    public class BlueprintProcessController : ControllerBase
    {
        [HttpGet("{id}/processes")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(PaginationResponse<LogResponse>), 200)]
        public async Task<IActionResult> List([FromRoute] string id, [FromQuery] int page = 1, [FromQuery] int perPage = 50)
        {
            var result = await ProcessLogLogic.Instance
                .List(blueprint_id: id, process_id: null, new Pagination(page, perPage));
            return Ok(result);
        }
        [HttpGet("processes/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(PaginationResponse<LogResponse>), 200)]
        public async Task<IActionResult> Get([FromRoute] string id, [FromQuery] int page = 1, [FromQuery] int perPage = 50)
        {
            var result = await ProcessLogLogic.Instance
                .List(blueprint_id: null, process_id: id, new Pagination(page, perPage));
            return Ok(result);
        }
    }
}
