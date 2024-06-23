using srtool;
using blueprint.core;
using blueprint.modules.account.response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.account.controller
{
    [Tags("account")]
    [ApiController]
    [Route("v1/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            var result = await AccountModule.Instance.Get(id, accountId);
            return Ok(result);
        }
        [HttpGet]
        [ProducesResponseType(typeof(PaginationResponse<AccountResponse>), 200)]
        public async Task<IActionResult> List([FromQuery] string search = null, [FromQuery] int page = 1, [Range(1, 100)] int perPage = 100)
        {
            var accountId = await this.GetAccountId();
            var result = await AccountModule.Instance.List(new Pagination(page, perPage), search: search, fromAccountId: accountId);
            return Ok(result);
        }
        [HttpGet]
        [Route("me")]
        [AuthRequire]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        public async Task<IActionResult> Me()
        {
            var accountId = await this.GetAccountId();
            var result = await AccountModule.Instance.Get(accountId, accountId);
            return Ok(result);
        }
    }
}
