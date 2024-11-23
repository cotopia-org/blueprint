using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class HubNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();

            name = "hub-node";
            title = "Hub";
            script = @"
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
            var count = node.field_count('next');
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
            var count = node.field_count('next');
            var position = Math.floor(Math.random() * count);
            node.execnode('next',position);
        }
        break;
        case 'unique-random': 
        {
            var count = node.field_count('next');
            
            var usedIndicesString  = node.get_static_data('usedIndices', '[]');
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
            AddField(new NodeField() { name = "type", type = FieldType.@string, defaultValue = "all", required = true });
            AddField(new NodeField() { name = "next", type = FieldType.output });
        }
    }

}
