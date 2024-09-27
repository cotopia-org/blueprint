using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.node.builtinnodes
{
    public class HttpRequestNode : NodeBuilder
    {
        public override string name => "http-request-node";
        public override string title => "HttpRequest node";
        public override string script => @"
function start()
{
    var url = node.field('url');
    var method = node.field('method');
    switch(method)
    {
        case 'GET':
          httprequest.get(url,callback_result);
        break;
        case 'DELETE':
          httprequest.delete(url,callback_result);
        break;
    }
}
function callback_result(x)
{
    let result = {};
    result.content = x.content;
    result.statusCode = x.statusCode;

    node.result = result;
    node.next();
}
";

        public override Node Node()
        {
            var node = base.Node();
            node.SetField("method", "GET");
            node.SetField("next", new List<Field>());

            return node;
        }

    }
}
