using blueprint.core;
using blueprint.modules.node.logic;
using blueprint.modules.node.request;
using blueprint.modules.node.response;
using Microsoft.AspNetCore.Mvc;
using srtool;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.node.controller
{
    [Tags("node")]
    [ApiController]
    [Route("v1/nodes")]
    public class NodeController : ControllerBase
    {
        [HttpPost]
        [AuthRequire()]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Insert([FromBody] NodeRequest request)
        {
            var accountId = await this.GetAccountId();

            var response = await NodeModule.Instance.Upsert(null, request, accountId);

            return Ok(response);
        }
        [HttpPut]
        [Route("{id}")]
        [AuthRequire()]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] NodeRequest request)
        {
            var accountId = await this.GetAccountId();
            var response = await NodeModule.Instance.Upsert(id, request, accountId);

            return Ok(response);
        }
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(NodeResponse), 200)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            var result = await NodeModule.Instance.Get(id, accountId);
            return Ok(result);
        }
        [HttpGet]
        [ProducesResponseType(typeof(PaginationResponse<NodeResponse>), 200)]
        public async Task<IActionResult> List([FromQuery] string search = null, [FromQuery] int page = 1, [Range(1, 100)] int perPage = 100)
        {
            var accountId = await this.GetAccountId();
            var result = await NodeModule.Instance.List(new Pagination(page, perPage), search: search, fromAccountId: accountId);
            return Ok(result);
        }

        [HttpDelete]
        [Route("{id}")]
        [AuthRequire()]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            await NodeModule.Instance.Delete(id, accountId);
            return Ok();
        }
    }
}
