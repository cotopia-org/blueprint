using blueprint.core;
using blueprint.modules.blueprintlog.logic;
using blueprint.modules.blueprintlog.response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using srtool;

namespace blueprint.modules.blueprintlog.controller
{
    [Tags("Blueprint log")]
    [ApiController]
    [Route("v1/blueprints")]
    public class BlueprintLogController : ControllerBase
    {
        [AuthRequire()]
        [HttpGet("{id}/logs")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(PaginationResponse<LogResponse>), 200)]
        public async Task<IActionResult> List([FromRoute] string id, [FromQuery] int page = 1, [FromQuery] int perPage = 50)
        {
            var accountId = await this.GetAccountId();
            var result = await ProcessLogLogic.Instance
                .List(id, new Pagination(page, perPage), accountId);
            return Ok(result);
        }
        [AuthRequire()]
        [HttpDelete("{id}/logs")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteBlueprintLogs([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            await ProcessLogLogic.Instance.DeleteBlueprintLogs(id: id, accountId);
            return Ok();
        }
        [AuthRequire()]
        [HttpGet("logs/{id}")]
        [ProducesResponseType(typeof(LogResponse), 200)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            var result = await ProcessLogLogic.Instance.Get(id, accountId);
            return Ok(result);
        }
        [AuthRequire()]
        [HttpDelete("logs/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteLog([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            await ProcessLogLogic.Instance.DeleteLog(id: id, accountId);
            return Ok();
        }
    }
}
