﻿using blueprint.modules.blueprintlog.logic;
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
        [HttpGet("{id}/logs")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(PaginationResponse<LogResponse>), 200)]
        public async Task<IActionResult> List([FromRoute] string id, [FromQuery] int page = 1, [FromQuery] int perPage = 50)
        {
            var result = await ProcessLogLogic.Instance
                .List(blueprint_id: id, process_id: null, new Pagination(page, perPage));
            return Ok(result);
        }
        [HttpDelete("{id}/logs")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            await ProcessLogLogic.Instance.DeleteBlueprintLogs(id: id);
            return Ok();
        }

        [HttpGet("logs/{id}")]
        [ProducesResponseType(typeof(LogResponse), 200)]
        public async Task<IActionResult> Get([FromRoute] string id, [FromQuery] int page = 1, [FromQuery] int perPage = 50)
        {
            await Task.Yield();
            return Ok();
        }
        [HttpDelete("logs/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteItem([FromRoute] string id)
        {
            await Task.Yield();
            return Ok();
        }
    }
}