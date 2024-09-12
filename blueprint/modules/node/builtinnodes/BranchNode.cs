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
    var type = node.field('type','all');
    switch (type) 
    {
        case 'all':
            node.next();
        break;
        case 'step':
        {
            var count = node.fieldarraycount('next');
            var step = node.get_static_data('step',-1);
            var reverse = node.field('reverse', false);

            if(reverse)
                step --;
            else
                step ++;

            var position = Math.floor(step % count);

            node.set_static_data('step',step);
            node.execnodeposition('next',position);
        }
        break;
        case 'random':
        {
            var count = node.fieldarraycount('next');
            var position = Math.floor(Math.random() * count);
            node.execnodeposition('next',position);
        }
        break;
        case 'unique-random': 
        {
            var count = node.fieldarraycount('next');
            
            var usedIndicesString = node.get_static_data('usedIndices', '[]');
            var usedIndices = JSON.parse(usedIndicesString);

            if (usedIndices.length >= count) {
                usedIndices = [];
            }

            var availableIndices = [];
            for (var i = 0; i < count; i++) {
                if (!usedIndices.includes(i)) {
                    availableIndices.push(i);
                }
            }

            var randomIndex = Math.floor(Math.random() * availableIndices.length);
            var position = availableIndices[randomIndex];

            usedIndices.push(position);

            node.set_static_data('usedIndices', JSON.stringify(usedIndices));

            node.execnodeposition('next', position);
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
