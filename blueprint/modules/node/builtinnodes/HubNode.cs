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
            if( count > 0)
            {
                var step = node.get_static_data('step',-1);
                var reverse = node.field('reverse', false);
                step ++;

                var position = Math.floor(step % count);

                if(reverse)
                position = count - 1 - position;

                node.set_static_data('step',step);

                let result = {};
                result.step = step;
                node.result = result;

                node.execnode('next',position);
            }
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
            
            var calledIndicesString  = node.get_static_data('calledIndices', '[]');
            var calledIndices = JSON.parse(calledIndicesString);

            if (calledIndices.length >= count) {
                calledIndices = [];
            }

            var availableIndices = [];
            for (var i = 0; i < count; i++) {
                if (!calledIndices.includes(i)) {
                    availableIndices.push(i);
                }
            }

            var randomIndex = Math.floor(Math.random() * availableIndices.length);
            var position = availableIndices[randomIndex];

            calledIndices.push(position);

            node.set_static_data('calledIndices', JSON.stringify(calledIndices));
            let result = {};
            result.calledIndices = JSON.stringify(calledIndices);
            node.result = result;

            node.execnode('next', position);
        }
        break;
    }
}
";
            AddField(new NodeField()
            {
                name = "type",
                type = FieldType.@string,
                defaultValue = "all",
                required = true
            ,
                listValue = new List<EnumValue>()
                {
                    new EnumValue(){ value = "all" , display = "ALL"},
                    new EnumValue(){ value = "step"},
                    new EnumValue(){ value = "random"},
                    new EnumValue(){ value = "unique-random"}
                }
            });
            AddField(new NodeField() { name = "reverse", type = FieldType.@bool, defaultValue = "false" });
            AddField(new NodeField() { name = "next", type = FieldType.output });
        }
    }

}
