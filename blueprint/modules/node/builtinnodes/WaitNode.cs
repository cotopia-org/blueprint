using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using MongoDB.Bson;
using srtool;

namespace blueprint.modules.node.builtinnodes
{
    public class WaitNode : NodeBuilder
    {
       
        public override string name => "wait-node";
        public override string title => "Wait node";
        public override string script => @"
function start()
{
   node.wait(node.field('delay'),'func1');
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
