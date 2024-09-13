using Newtonsoft.Json.Linq;

namespace blueprint.modules.blueprint.runtime
{
    public class JsonHandler
    {
        JObject json;
        public JsonHandler(JObject json)
        {
            this.json = json;
        }
        public string item(string key)
        {
            return json[key].ToString();
        }
    }
}
