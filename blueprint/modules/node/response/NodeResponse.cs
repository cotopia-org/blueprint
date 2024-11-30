using blueprint.modules.account.response;
using blueprint.modules.drive.response;
using blueprint.modules.node.database;
using blueprint.modules.node.types;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.node.response
{
    public class NodeResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string script { get; set; }
        public List<Component> components { get; set; }
        public List<NodeField> fields { get; set; }
        public AccountResponse creator { get; set; }
        public FileResponse icon_media { get; set; }
        public bool inputConnection{get; set; }
        public DateTime updateDateTime { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
