using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.node.builtinnodes
{
    public class PulseNode : NodeBuilder
    {
        public override string name => "pulse-node";
        public override string title => "Pulse node";
        public override string script => @"
function on_pulse()
{
    node.next();
}
";
        public override Node Node()
        {
            var node = base.Node();

            var pulse = node.AddComponent<Pulse>();
            pulse.name = "c1";
            pulse.delayParam = "delay";
            pulse.callback = "on_pulse";

            node.SetField("delay", 10);
            node.SetField("next", new List<Field>());

            return node;
        }
    }
}
