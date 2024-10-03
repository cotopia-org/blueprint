using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.blueprint.core.blocks;

namespace blueprint.modules.node.builtinnodes
{
    public class SwitchNode : NodeBuilder
    {
        public override string name => "switch-node";
        public override string title => "Switch node";
        public override string script => @"
function start()
{
    var caseToken = node.field('case','');
    var fieldAddress = 'next.' + caseToken;
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
        public override Node Node()
        {
            var node = base.Node();
            node.SetField("case", "-");

            return node;
        }
    }
}


