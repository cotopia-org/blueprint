using blueprint.modules.blueprint;
using blueprint.modules.processlog.logic;
using Microsoft.AspNetCore.Mvc;
using srtool;
namespace blueprint.modules.blueprintlog.controller
{
    [Tags("log")]
    public class logController : ControllerBase
    {
        [Route("v1/logs")]
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> List([FromQuery] string blueprint_id = null, [FromQuery]int page = 1, [FromQuery]int perPage = 50)
        {
            var result = await ProcessLogLogic.Instance
                .List(blueprint_id, new Pagination(page, perPage));
            return Ok(result);
        }
    }
}
