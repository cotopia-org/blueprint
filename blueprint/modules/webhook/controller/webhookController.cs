using blueprint.modules.blueprint;
using Microsoft.AspNetCore.Mvc;

namespace blueprint.modules.webhook.controller
{
    [Tags("webhook")]
    public class webhookController : ControllerBase
    {
        [Route("v1/webhooks/{token}")]
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get([FromRoute] string token)
        {
            var result = await BlueprintModule.Instance.Exec_webhooktoken(token);
            if (result == null)
                return NotFound(new { message = "Not found webhook-token." });
            return Ok(result.output);
        }
    }
}
