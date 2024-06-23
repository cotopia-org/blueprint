using blueprint.modules.drive.response;
using Newtonsoft.Json;

namespace blueprint.modules.account.response
{
    public class AccountResponse
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string email { get; set; }
        public string lastName { get; set; }
        public FileResponse avatar { get; set; }
        public DateTime? signupDateTime { get; set; }
    }
}
