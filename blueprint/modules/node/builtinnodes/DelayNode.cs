using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.node.builtinnodes
{
    public class DelayNode : NodeBuilder
    {
        public override string id => "65c4115a0111a5ca6bd472c4";
        public override string name => "delay-node";
        public override string title => "Delay node";
        public override string script => @"
function start()
{
   node.wait(node.field(""delay""),""func1"");
}
function func1()
{
   node.next();
}
";
        public override Node Node()
        {
            var node = base.Node();
            node.SetField("delay", 1);
            node.SetField("next", new List<Field>());
            return node;
        }
    }
}
