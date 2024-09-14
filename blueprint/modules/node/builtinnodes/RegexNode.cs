using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;

namespace blueprint.modules.node.builtinnodes
{
    public class RegexNode : NodeBuilder
    {
        public override string id => "65c4115b0111a5ca3cc012b3";
        public override string name => "regex-node";
        public override string title => "Regex node";
        public override string script => @"
function start()
{
    var regexPattern = node.field('pattern', ' ');  
    var regexFlags = node.field('flags', 'g');
    var text = node.field('text', ' ');

    var regex = new RegExp(regexPattern, regexFlags);  

    var matcheItems = text.match(regex);

    var result = { matches: matcheItems || [] };

    var jsonResult = JSON.stringify(result);

    node.set_json_result(jsonResult);
    node.next();
}";
        public override Node Node()
        {
            var node = base.Node();
            return node;
        }


    }
}
