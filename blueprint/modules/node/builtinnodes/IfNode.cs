using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class IfNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();

            name = "if-node";
            title = "If node";
            script = @"
function start()
{
    var operator = node.field('operator');
    var valueA = node.field('valueA'); 
    var valueB = node.field('valueB');
    var comparisonResult = false;
    var output_name = 'next_false';
    switch (operator) {
        case '>':
            comparisonResult = valueA > valueB;
            break;
        case '<':
            comparisonResult = valueA < valueB;
            break;
        case '>=':
            comparisonResult = valueA >= valueB;
            break;
        case '<=':
            comparisonResult = valueA <= valueB;
            break;
        case '=':
            comparisonResult = valueA == valueB;
            break;
        case '!=':
            comparisonResult = valueA != valueB;
            break;
        default:
            node.print('Invalid operator');
            output_name = 'next_false';
            return;
    }

    if (comparisonResult) 
        output_name = 'next_true';
    else
        output_name = 'next_false';

    node.execnode(output_name);
}
";
            AddField(new NodeField() { name = "operator", defaultValue = "=", fieldType = FieldType.@string });
            AddField(new NodeField() { name = "valueA", defaultValue = "test", fieldType = FieldType.@string });
            AddField(new NodeField() { name = "valueB", defaultValue = "test", fieldType = FieldType.@string });
            AddField(new NodeField() { name = "next_true", fieldType = FieldType.output });
            AddField(new NodeField() { name = "next_false", fieldType = FieldType.output });
        }
    }
}
