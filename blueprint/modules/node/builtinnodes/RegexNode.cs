using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class RegexNode : NodeBuilder
    {

        public override void Build()
        {
            base.Build();
            name = "regex-node";
            title = "Regex node";
            script = @"                  

function start()
{
    var regexPattern = node.field('pattern', ' ');  
    var regexFlags = node.field('flags', 'g');
    var text = node.field('text', ' ');
    
    //var regex = new RegExp(regexPattern);  
    let regex = /\b\w+\b/g;d
    var matchItems = text.match(regex);

    var result = { matches: matchItems || [] };

    node.result = result;
    node.next();
}";
            AddField(new NodeField() { name = "pattern", fieldType = FieldType.@string });
            AddField(new NodeField() { name = "flags", fieldType = FieldType.@string });
            AddField(new NodeField() { name = "text", fieldType = FieldType.@string });
            AddField(new NodeField() { name = "next", fieldType = FieldType.output });
        }
    }
}
