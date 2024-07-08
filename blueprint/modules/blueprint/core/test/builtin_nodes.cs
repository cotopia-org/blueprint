using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.blueprint.core.test
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
    node.print(node.field(""text""));
    node.execnode(""next"");
}
"
                );

            node.AddField(new Field()
            {
                name = "text",
                type = DataType.@string,
                value = "test log"
                //value = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //value = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var output = new Field();
            output.name = "next";
            output.type = DataType.node;
            node.AddField(output);

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
    node.execnode(""next"");
    node.set_output(node.field(""output"")); 
}
"
);
            node.AddField(new Field()
            {
                name = "output",
                type = DataType.@string,
                value = "OK"
                //value = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //value = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var webhook = node.AddComponent<Webhook>();
            webhook.name = "c1";
            webhook.token = token;
            var output = new Field();
            output.name = "next";
            output.type = DataType.node;
            node.AddField(output);

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
    node.execnode(""next"");
}
"
            );

            var output = new Field();
            output.name = "next";
            output.type = DataType.node;

            node.AddField(output);

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
   node.execnode(""next"");
}
"
                );

            node.AddField(new Field()
            {
                name = "delay",
                type = DataType.@double,
                value = 1
                //expression = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //expression = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var output = new Field();
            output.name = "next";
            output.type = DataType.node;
            node.AddField(output);

            return node;
        }

        public static Node _online_smartphones()
        {
            var node = new Node();
            node.id = util.GenerateId();
            node.name = "smartphones-node";

            node.script =
            new Script(
            @"
function start()
{
    node.rest_get(""https://filesamples.com/samples/code/json/sample1.json"",
    function(result)
    {
        node.print(result.body); 
        node.execnode(""next""); 
    }
    );
}"
);

            var output = new Field();
            output.name = "next";
            output.type = DataType.node;
            node.AddField(output);

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
    node.execnode(""next"");
}
"
);
            node.AddField(new Field()
            {
                name = "delay",
                type = DataType.@double,
                value = 10
                //expression = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //expression = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var pulse = node.AddComponent<Pulse>();
            pulse.name = "c1";
            pulse.delayParam = "delay";
            pulse.callback = "on_pulse";

            var output = new Field();
            output.name = "next";
            output.type = DataType.node;
            node.AddField(output);

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

            node.AddField(new Field()
            {
                name = "operator",
                type = DataType.@string,
                value = "="
            });

            node.AddField(new Field()
            {
                name = "valueA",
                type = DataType.@object,
                value = "test"
            });
            node.AddField(new Field()
            {
                name = "valueB",
                type = DataType.@object,
                value = "test"
            });


            var outputTrue = new Field();
            outputTrue.name = "next_true";
            outputTrue.type = DataType.node;
            node.AddField(outputTrue);

            var outputFalse = new Field();
            outputTrue.name = "next_false";
            outputTrue.type = DataType.node;
            node.AddField(outputFalse);

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
            node.execnode(""next"");
            break;
        case 'random':
        {
            var count = node.getfieldarraycount(""next"");
            var position = Math.floor(Math.random() * count);
            node.execnodeposition(""next"",position);
        }
        break;
    }
}
"
);
            node.AddField(new Field()
            {
                name = "type",
                type = DataType.@string,
                value = "all"//all,randrobin,random,sudorandom
                //expression = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //expression = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var output = new Field();
            output.name = "next";
            output.type = DataType.node;
            node.AddField(output);

            return node;
        }
    }

}
