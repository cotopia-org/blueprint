using blueprint.core;
using blueprint.modules.drive.response;
using Microsoft.AspNetCore.Mvc;

namespace blueprint.modules.drive.controller
{
    [Tags("drive")]
    [ApiController]
    [Route("v1/drive")]
    public class DriveController : ControllerBase
    {
        [Route("info")]
        [HttpGet]
        [AuthRequire]
        [ProducesResponseType(typeof(DriveInfoResponse), 200)]
        public async Task<IActionResult> GetDriveInfo()
        {
            //string accountId = await this.GetAccountId();

            //var result = await MediaLogic.Instance.Upload(accountId, file, title);
            return Ok("");
        }
    }
}
