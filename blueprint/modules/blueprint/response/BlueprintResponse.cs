using blueprint.modules.account.response;
using blueprint.modules.node.response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.blueprint.response
{
    public class BlueprintResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public AccountResponse creator { get; set; }

        public string description { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject blueprint { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<NodeResponse> referenceNodes { get; set; }
        public DateTime updateDateTime { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
