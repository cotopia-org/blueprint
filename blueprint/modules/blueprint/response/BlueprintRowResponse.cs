using blueprint.modules.account.response;

namespace blueprint.modules.blueprint.response
{
    public class BlueprintRowResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public AccountResponse creator { get; set; }
        public bool run { get; set; }
        public string description { get; set; }
        public int nodeCount { get; set; }
        public long execCount { get; set; }

        public DateTime updateDateTime { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
