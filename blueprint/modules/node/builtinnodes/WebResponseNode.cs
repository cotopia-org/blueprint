using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;

namespace blueprint.modules.node.builtinnodes
{
    public class WebResponseNode : NodeBuilder
    {
        public override string id => "65c4115a0111a5ca6bb012e2";
        public override string name => "web-response-node";
        public override string title => "web response node";
        public override string script => @"
function start()
{
    node.webresponse(200,node.field(""text""))
    node.next();
}";
        public override Node Node()
        {
            var node = base.Node();
            node.SetField("value", "ok");
            return node;
        }


    }
}
