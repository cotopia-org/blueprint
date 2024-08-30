using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.node.builtinnodes
{
    public class BranchNode : NodeBuilder
    {
        public override string id => "65c4115a0111a2ca6cd021a9";
        public override string name => "branch-node";
        public override string title => "Branch node";
        public override string script => @"
function start()
{
    var type = node.field(""type"");
    switch (type) {
        case 'all':
            node.next();
            break;
        case 'random':
        {
            var count = node.fieldarraycount(""next"");
            var position = Math.floor(Math.random() * count);
            node.execnodeposition(""next"",position);
        }
        break;
    }
}
";
        public override Node Node()
        {
            var node = base.Node();
            node.SetField("type", "all");
            node.SetField("next", new List<Field>());

            return node;
        }
    }
}
