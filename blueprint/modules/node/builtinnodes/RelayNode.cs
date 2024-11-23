
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.node.types;
namespace blueprint.modules.node.builtinnodes
{
    public class RelayNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();
            name = "relay-node";
            title = "Relay";
            script = @"
function start()
{
    node.next();
}
";
            AddField(new NodeField() { name = "next", type = FieldType.output });

        }
    }
}


