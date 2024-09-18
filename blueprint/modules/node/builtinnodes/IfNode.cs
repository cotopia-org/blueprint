using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.node.builtinnodes
{
    public class IfNode : NodeBuilder
    {
        public override string name => "if-node";
        public override string title => "If node";
        public override string script => @"
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
        public override Node Node()
        {
            var node = base.Node();

            node.SetField("operator", "=");
            node.SetField("valueA", "test");
            node.SetField("valueB", "test");

            node.SetField("next_true", new List<Field>());
            node.SetField("next_false", new List<Field>());

            return node;
        }
    }
}
