using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.node.builtinnodes
{
    public class WebhookNode : NodeBuilder
    {
        public override string id => "65c4115a0111a5ca6bd122c2";
        public override string name => "webhook-node";
        public override string title => "Webhook node";
        public override string script => @"
function start()
{
    node.next();
}";
        public override Node Node()
        {
            var node = base.Node();
            node.SetField("next", new List<Field>());

            var webhook = node.AddComponent<Webhook>();
            webhook.name = "c1";

            return node;
        }
    }
}
