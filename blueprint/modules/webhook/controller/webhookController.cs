using blueprint.modules.blueprint;
using Microsoft.AspNetCore.Mvc;

namespace blueprint.modules.webhook.controller
{
    [Tags("Webhook")]
    public class webhookController : ControllerBase
    {
        [Route("wh/{token}")]
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get([FromRoute] string token)
        {
            var result = await BlueprintModule.Instance.Exec_webhooktoken(token, HttpContext);
            if (result == null)
                return NotFound(new { message = "Not found webhook-token." });
            //else
            //    if (result.statusCode == 200)
            //    return Ok(result.text);
            //else
            //    return BadRequest();

            return new ContentResult() { StatusCode = result.statusCode, Content = result.Content, ContentType = "text/plain;  charset=utf-8" };
        }
    }
}
