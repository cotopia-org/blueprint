using blueprint.modules.account.response;
using blueprint.modules.node.types;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.node.request
{
    public class NodeRequest
    {
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string script { get; set; }
        public List<NodeField> fields { get; set; }
    }
}
