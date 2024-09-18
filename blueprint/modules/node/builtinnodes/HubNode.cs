using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.node.builtinnodes
{
    public class HubNode : NodeBuilder
    {
        public override string name => "hub-node";
        public override string title => "Hub node";
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
            var count = node.fieldcount('next');
            var step = node.get_static_data('step',-1);
            var reverse = node.field('reverse', false);

            if(reverse)
                step --;
            else
                step ++;

            var position = Math.floor(step % count);

            node.set_static_data('step',step);
            node.execnode('next',position);
        }
        break;
        case 'random':
        {
            var count = node.fieldcount('next');
            var position = Math.floor(Math.random() * count);
            node.execnode('next',position);
        }
        break;
        case 'unique-random': 
        {
            var count = node.fieldcount('next');
            
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

            node.execnode('next', position);
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
