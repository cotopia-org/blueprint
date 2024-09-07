using blueprint.core;
using blueprint.modules.drive.logic;
using blueprint.modules.drive.response;
using Microsoft.AspNetCore.Mvc;

namespace blueprint.modules.drive.controller
{
    [Tags("Frive-file")]
    [ApiController]
    [Route("v1/drive/files")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public FileController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            DriveModule.Instance.WebRootPath = _hostEnvironment.WebRootPath;
        }

        [HttpPost]
        [AuthRequire()]
        [ProducesResponseType(typeof(FileResponse), 200)]
        public async Task<IActionResult> Add(IFormFile file, string title = null)
        {
            string accountId = await this.GetAccountId();
            var result = await DriveModule.Instance.Add(accountId, file, title);
            return Ok(result);
        }
        [Route("{id}")]
        [HttpPut]
        [AuthRequire]
        [ProducesResponseType(typeof(FileResponse), 200)]
        public async Task<IActionResult> Update([FromRoute] string id, string title = null, string description = null)
        {
            await Task.Yield();
            //string accountId = await this.GetAccountId();

            //var result = await MediaLogic.Instance.Upload(accountId, file, title);
            return Ok("");
        }
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(FileResponse), 200)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var accountId = await this.GetAccountId();
            var result = await DriveModule.Instance.Get(id);
            return Ok(result);
        }
        [HttpGet]
        [ProducesResponseType(typeof(FileResponse), 200)]
        public async Task<IActionResult> List(string directoryId = null, string search = null, int page = 1, int perPage = 100)
        {
            await Task.Yield();
            //string accountId = await this.GetAccountId();

            //var result = await DriveLogic.Instance.List(accountId, file, title);
            return Ok();
        }
        //[Route("{id}")]
        //[HttpDelete]
        //[AuthRequire]
        //[ProducesResponseType(typeof(FileResponse), 200)]
        //public async Task<IActionResult> Delete([FromRoute] string id)
        //{
        //    //string accountId = await this.GetAccountId();

        //    //var result = await MediaLogic.Instance.Upload(accountId, file, title);
        //    return Ok("");
        //}
        //[Route("{id}/move")]
        //[HttpPost]
        //[AuthRequire]
        //[ProducesResponseType(typeof(FileResponse), 200)]
        //public async Task<IActionResult> Move([FromRoute] string id, string toDirectoryId)
        //{
        //    //string accountId = await this.GetAccountId();

        //    //var result = await MediaLogic.Instance.Upload(accountId, file, title);
        //    return Ok("");
        //}
    }
}
