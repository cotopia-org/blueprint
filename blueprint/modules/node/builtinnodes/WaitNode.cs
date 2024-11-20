using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;
using MongoDB.Bson;
using srtool;

namespace blueprint.modules.node.builtinnodes
{
    public class WaitNode : NodeBuilder
    {

        public override void Build()
        {
            base.Build();
            name = "wait-node";
            title = "Wait node";
            script = @"
function start()
{
   node.wait(node.field('delay'),'func1');
}
function func1()
{
   node.next();
}
";
            AddField(new NodeField() { name = "delay", fieldType = FieldType.@int });
            AddField(new NodeField() { name = "next", fieldType = FieldType.output });
        }
    }
}
