using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.blueprint.core
{
    public static class builtin_nodes
    {
        public static Node _log_node()
        {
            var node = new Node();
            node.id = util.GenerateId();
            node.name = "log-node";

            node.script =
                new Script(
                @"
function start()
{
    var ff = node.field(""text"");
    node.print(node.field(""text""));
    //node.log(node.field(""text""));
    node.next();
}
"
);

            node.SetField("text", "test log");
            node.SetField("next", new List<Field>());


            return node;
        }
        public static Node _webhook_node(string token)
        {
            var node = new Node();

            node.id = util.GenerateId();
            node.name = "webhook-node";
            node.script =
                new Script(
@"
function start()
{
    node.set_output(node.field(""output"")); 
    node.next();
}
"
);
            node.SetField("output", "OK");
            node.SetField("next", new List<Field>());

            var webhook = node.AddComponent<Webhook>();
            webhook.name = "c1";
            webhook.token = token;

            return node;
        }
        public static Node _start_node()
        {
            var node = new Node();

            node.id = util.GenerateId();
            node.name = "start-node";
            node.script =
                new Script(
                @"
function start()
{
    node.next();
}
"
            );

            node.SetField("next", new List<Field>());

            return node;
        }
        public static Node _delay_node()
        {
            var node = new Node();
            node.id = util.GenerateId();
            node.name = "delay-node";

            node.script =
                new Script(
                @"
function start()
{
   node.wait(node.field(""delay""),""func1"");
}
function func1()
{
   node.next();
}
");

            node.SetField("delay", 1);
            node.SetField("next", new List<Field>());


            return node;
        }

        public static Node _test_node()
        {
            var node = new Node();
            node.id = util.GenerateId();
            node.name = "test_node";

            node.script =
            new Script(
            @"
function start()
{
    rest.get('https://filesamples.com/samples/code/json/sample1.json',
    response=>
    {
        node.print(response.data); 
        node.next(); 
    }
    );
}"
);
            node.SetField("next", new List<Field>());

            return node;
        }
        public static Node _pulse_node()
        {
            var node = new Node();

            node.id = util.GenerateId();
            node.name = "pulse-node";
            node.script =
                new Script(
@"
function on_pulse()
{
    node.next();
}
"
);
            node.SetField("delay", 10);
            node.SetField("next", new List<Field>());

            var pulse = node.AddComponent<Pulse>();
            pulse.name = "c1";
            pulse.delayParam = "delay";
            pulse.callback = "on_pulse";

            return node;
        }

        public static Node _condition_node()
        {
            var node = new Node();

            node.id = util.GenerateId();
            node.name = "condition-node";
            node.script =
                new Script(
@"
function start()
{
    var operator = node.field(""operator"");
    var valueA = node.field(""valueA"");
    var valueB = node.field(""valueB"");

    var comparisonResult = false;
    var output_name = ""next_false"";
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
            node.print(""Invalid operator"");
            output_name = ""next_false"";
            return;
    }

    if (comparisonResult) 
        output_name = ""next_true"";
    else
        output_name = ""next_false"";

    node.execnode(output_name);
}
"
);
            node.SetField("operator", "=");
            node.SetField("valueA", "test");
            node.SetField("valueB", "test");

            node.SetField("next_true", new List<Field>());
            node.SetField("next_false", new List<Field>());

            return node;
        }

        public static Node _branch_node()
        {
            var node = new Node();

            node.id = util.GenerateId();
            node.name = "branch-node";
            node.script =
                new Script(
@"
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
"
);
            node.SetField("type", "all");
            node.SetField("next", new List<Field>());

            return node;
        }

    }

}
