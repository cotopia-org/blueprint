using Newtonsoft.Json.Linq;

namespace blueprint.modules.blueprint.request
{
    public class BlueprintRequest
    {
        public string title { get; set; }
        public string description { get; set; }
        public JObject blueprint { get; set; }
        public bool run { get; set; }
    }
}
