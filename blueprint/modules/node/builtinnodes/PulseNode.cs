using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class PulseNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();

            name = "pulse-node";
            title = "Pulse";
            script = @"
function on_pulse()
{
    node.next();
}
";
            AddComponent(new database.Component() { name = "Cron", param1 = "cronExpression", param2 = "on_pulse" });
            AddField(new NodeField() { name = "cronExpression", defaultValue = "*/10 * * * *", type = FieldType.@string, required = true });
            AddField(new NodeField() { name = "next", type = FieldType.output });
        }
    }
}
