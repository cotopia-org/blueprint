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

            node.SetField("cronExpression", "*/10 * * * *");
            node.SetField("next", new List<Field>());

            var cron = node.AddComponent<Cron>();
            cron.name = "c1";
            cron.expressionParam = "cronExpression";
            cron.callback = "on_pulse";

            return node;
        }
    }
}
