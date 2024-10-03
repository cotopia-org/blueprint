
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
namespace blueprint.modules.node.builtinnodes
{
    public class RelayNode : NodeBuilder
    {
        public override string name => "relay-node";
        public override string title => "Relay node";
        public override string script => @"
function start()
{
    node.next();
}
";
        public override Node Node()
        {
            var node = base.Node();
            node.SetField("case", "-");

            return node;
        }
    }
}


