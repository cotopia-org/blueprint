using blueprint.core;
using blueprint.modules.auth;
using blueprint.modules.blueprint.request;
using blueprint.modules.blueprint.response;
using blueprint.modules.blueprintlog.logic;
using blueprint.srtool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using srtool;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Net.WebSockets;
using ZstdSharp.Unsafe;

namespace blueprint.modules.blueprint.controller
{
    [Tags("Blueprint")]
    [ApiController]
    [Route("v1/blueprints")]
    public class BlueprintController : ControllerBase
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
        [Route("{id}/live-trace")]
        public async Task<IActionResult> LiveTrace([FromRoute] string id)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var accountId = await this.GetAccountId();

                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var wsConnection = new WSConnection();
                wsConnection.Init(webSocket);
                await BlueprintModule.Instance.LiveTrace(wsConnection, id, accountId);
                await wsConnection.RecivedLoop();
            }
            else
            {
                return BadRequest("WebSocket request expected.");
            }

            return new EmptyResult(); // WebSocket is now handled.
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
            await Task.Yield();
            return Ok();
        }
    }
}
