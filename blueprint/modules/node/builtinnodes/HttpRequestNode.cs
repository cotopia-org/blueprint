using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class HttpRequestNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();
            name = "http-request-node";
            title = "HttpRequest";
            script = @"
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
            AddField(new NodeField() { name = "url", type = FieldType.@string, defaultValue = "https://domain-name", required = true });
            AddField(new NodeField() { name = "method", type = FieldType.@string, defaultValue = "GET", required = true });
            AddField(new NodeField() { name = "next", type = FieldType.output });
        }

    }
}
