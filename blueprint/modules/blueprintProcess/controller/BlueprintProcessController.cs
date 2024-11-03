using blueprint.core;
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
        [AuthRequire]
        [ProducesResponseType(typeof(PaginationResponse<LogResponse>), 200)]
        public async Task<IActionResult> List([FromRoute] string id, [FromQuery] int page = 1, [FromQuery] int perPage = 50)
        {
            var accountId = await this.GetAccountId();
            var result = await ProcessLogLogic.Instance
                .List(blueprint_id: id, new Pagination(page, perPage), fromAccountId: accountId);
            return Ok(result);
        }
        [HttpGet("processes/{id}")]
        [AuthRequire]
        [ProducesResponseType(typeof(LogResponse), 200)]
        public async Task<IActionResult> Get([FromRoute] string id, [FromQuery] int page = 1, [FromQuery] int perPage = 50)
        {
            var accountId = await this.GetAccountId();
            var result = await ProcessLogLogic.Instance
                .Get(id, fromAccountId: accountId);
            return Ok(result);
        }
    }
}
