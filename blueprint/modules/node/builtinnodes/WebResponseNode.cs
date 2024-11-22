using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class WebResponseNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();
            name = "web-response-node";
            title = "web response node";
            script = @"
function start()
{
    var content = node.field('content','OK');
    var status = node.field('status',200);
    node.webresponse(status,content)
    node.next();
}";
            AddField(new NodeField() { name = "content", defaultValue = "OK", fieldType = FieldType.@string });
            AddField(new NodeField() { name = "status", defaultValue = "200", fieldType = FieldType.number, required = true });
            AddField(new NodeField() { name = "next", fieldType = FieldType.output });
        }


    }
}
