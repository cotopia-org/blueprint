using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class SwitchNode : NodeBuilder
    {

        public override void Build()
        {
            base.Build();
            name = "switch-node";
            title = "Switch node";
            script = @"
function start()
{
    var caseToken = node.field('case','');
    var fieldAddress = 'items.' + caseToken;
    if(node.is_exist_field(fieldAddress))
    {
        node.execnode(fieldAddress);
    }
    else
    {
        node.execnode('next_default');
    }
}
";
            AddField(new NodeField() { name = "case", fieldType = FieldType.@string });
            AddField(new NodeField()
            {
                name = "items",
                fieldType = FieldType.array,
                fields = new List<NodeField>() {
                    new NodeField() { name = "name", fieldType = FieldType.@string },
                    new NodeField() { name = "next", fieldType = FieldType.output }
                     }
            });
            AddField(new NodeField() { name = "next_default", fieldType = FieldType.output });
        }
    }
}


