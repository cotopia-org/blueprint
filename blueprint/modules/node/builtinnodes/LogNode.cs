using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.node.builtinnodes
{
    public class LogNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();

            name = "log-node";
            title = "Log node";
            script = @"
function start()
{
    var text = node.field('text');
    node.print(text);
    node.log(text);
    let result = {};
    result.text = text;
    node.result = result;
    node.next();
}
";
            AddField(new NodeField() { name = "text", defaultValue = "Test", fieldType = FieldType.@string, required = true });
            AddField(new NodeField() { name = "next", fieldType = FieldType.output });
        }
    }
}
