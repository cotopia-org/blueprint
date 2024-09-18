using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.node.builtinnodes
{
    public class LogNode : NodeBuilder
    {
        public override string name => "log-node";
        public override string title => "Log node";
        public override string script => @"
function start()
{
    node.print(node.field('text'));
    node.log(node.field('text'));
    node.next();
}
";
        public override Node Node()
        {
            var node = base.Node();
            node.SetField("text", "test log");
            node.SetField("next", new List<Field>());
            return node;
        }
    }
}
