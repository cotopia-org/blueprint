using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class WebhookNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();
            name = "webhook-node";
            title = "Webhook";
            script = @"
function start()
{
    node.next();
}";
            AddComponent(new database.Component() { name = "Webhook" });
            AddField(new NodeField() { name = "next", type = FieldType.output });
            inputConnection = false;
        }
    }
}
