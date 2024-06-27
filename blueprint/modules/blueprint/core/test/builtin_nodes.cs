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
                type = FieldType.@string,
                value = "test log"
                //value = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //value = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var output = new Field();
            output.name = "next";
            output.type = FieldType.node;
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
                type = FieldType.@string,
                value = "OK"
                //value = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //value = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var webhook = node.AddComponent<Webhook>();
            webhook.name = "c1";
            webhook.token = token;
            var output = new Field();
            output.name = "next";
            output.type = FieldType.node;
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
            output.type = FieldType.node;

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
                type = FieldType.@double,
                value = 1
                //expression = new Expression() { active = true, script = new Script(ScriptType.javascript, "{{5*5}}") }
                //expression = new Expression() { active = true, script = new Script(ScriptType.lua, "{{5*5*2}}") }
            });

            var output = new Field();
            output.name = "next";
            output.type = FieldType.node;
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
            output.type = FieldType.node;
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
                type = FieldType.@double,
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
            output.type = FieldType.node;
            node.AddField(output);

            return node;
        }
    }

}
