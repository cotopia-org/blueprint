using blueprint.core;
using blueprint.modules.blueprint.request;
using blueprint.modules.blueprint.response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using srtool;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.blueprint.controller
{
    [Tags("blueprint")]
    [ApiController]
    [Route("v1/blueprints")]
    public class blueprintController : ControllerBase
    {
        [HttpPost]
        [AuthRequire()]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Insert([FromBody] BlueprintRequest request)
        {
            var accountId = await this.GetAccountId();
            var response = await BlueprintModule.Instance.Upsert(null, request, accountId);
            return Ok(response);
        }
        [HttpPut]
        [Route("{id}")]
        [AuthRequire()]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] BlueprintRequest request)
        {
            var accountId = await this.GetAccountId();
            var response = await BlueprintModule.Instance.Upsert(id, request, accountId);
            return Ok(response);
        }
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(BlueprintResponse), 200)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            var result = await BlueprintModule.Instance.Get(id, accountId);
            return Ok(result);
        }
        [HttpGet]
        [ProducesResponseType(typeof(PaginationResponse<BlueprintResponse>), 200)]
        public async Task<IActionResult> List([FromQuery] string search = null, [FromQuery] int page = 1, [Range(1, 100)] int perPage = 100)
        {
            var accountId = await this.GetAccountId();
            var result = await BlueprintModule.Instance.List(accountId, new Pagination(page, perPage), search: search, fromAccountId: accountId);
            return Ok(result);
        }
        [HttpDelete]
        [Route("{id}")]
        [AuthRequire()]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            return Ok();
        }

        [Route("run-webhook")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RunWithWebhook([FromQuery] string token)
        {
            var result = await BlueprintModule.Instance.Exec_token(token);
            if (result == null)
                return NotFound(new { message = "Not found webhook-token." });
            return Ok(result.output);
        }
    }
}
