using blueprint.core;
using blueprint.modules.account;
using blueprint.modules.auth.request;
using blueprint.modules.auth.response;
using Microsoft.AspNetCore.Mvc;

namespace blueprint.modules.auth.controller
{
    [Tags("auth")]
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }
        [Route("sign-up")]
        [HttpPost]
        [ProducesResponseType(typeof(SigninResponse), 200)]
        public async Task<IActionResult> Signup([FromBody] SignupRequest data)
        {
            var result = await AuthModule.Instance.Signup(data);
            return Ok(result);
        }
        [Route("sign-in")]
        [HttpPost]
        [ProducesResponseType(typeof(SigninResponse), 200)]
        public async Task<IActionResult> Signin([FromBody] SigninRequest data)
        {
            return Ok(await AuthModule.Instance.Signin(data, TimeSpan.FromHours(24)));
        }
        [Route("sign-out")]
        [HttpPost]
        [AuthRequire]
        [ProducesResponseType(typeof(SignoutResponse), 200)]
        public async Task<IActionResult> Signout()
        {
            var input = new SignoutRequest();
            input.sessionId = this.GetLoginSessionId();
            var result = await AuthModule.Instance.Signout(input);

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [Route("forget-password")]
        [HttpPost]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest data)
        {
            await Task.Yield();
            return Ok();
        }
        [Route("reset-password")]
        [HttpPost]
        [ProducesResponseType(typeof(ResetPasswordResponse), 200)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest data)
        {
            var accountId = await this.GetAccountId();
            var result = await AccountModule.Instance.ResetPassword(accountId, data);
            return Ok();
        }

        [Route("access-token")]
        [HttpPost()]
        [ProducesResponseType(typeof(AccessTokenResponse), 200)]
        public async Task<IActionResult> GetAccessToken([FromBody] AccessTokenRequest data)
        {
            var result = await AuthModule.Instance.GenerateAccessToken(data.refreshToken, TimeSpan.FromHours(24));
            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [Route("change-password")]
        [HttpPost]
        [AuthRequire]
        [ProducesResponseType(typeof(ResetPasswordResponse), 200)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest data)
        {
            var accountId = await this.GetAccountId();
            var result = await AccountModule.Instance.ChangePassword(accountId, data);
            return Ok();
        }
        //[Route("delete-account")]
        //[HttpDelete]
        //[ProducesResponseType(typeof(DeleteAccountRequest), 200)]
        //public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest data)
        //{
        //    var accountId = await this.GetAccountId();
        //    await AccountModule.Instance.DeleteAccount(accountId, data);
        //    return Ok();
        //}
    }
}