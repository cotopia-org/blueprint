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
            title = "Switch";
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
            AddField(new NodeField() { name = "case", type = FieldType.@string });
            AddField(new NodeField()
            {
                name = "items",
                type = FieldType.array,
                fields = new List<NodeField>() {
                    new NodeField() { name = "name", type = FieldType.@string },
                    new NodeField() { name = "next", type = FieldType.output }
                     }
            });
            AddField(new NodeField() { name = "next_default", type = FieldType.output });
        }
    }
}


