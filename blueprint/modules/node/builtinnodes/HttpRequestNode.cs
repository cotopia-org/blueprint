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
    var jsonResult = node.field('jsonResult');
    switch(method)
    {
        case 'GET':
          httprequest.get(url,(x)=>{callback_result(x,jsonResult);});
        break;
        case 'DELETE':
          httprequest.delete(url,(x)=>{callback_result(x,jsonResult);});
        break;
    }
}
function callback_result(x, jsonResult)
{
    let result = {};
    if( jsonResult)
        result.content =  JSON.parse(x.content);
    else
        result.content = x.content;

    result.statusCode = x.statusCode;

    node.result = result;
    node.next();
}
";
            AddField(new NodeField() { name = "url", type = FieldType.@string, defaultValue = "https://domain-name", required = true });
            AddField(new NodeField() { name = "jsonResult", type = FieldType.@bool, defaultValue = "false", required = true });
            AddField(new NodeField()
            {
                name = "method",
                type = FieldType.@string,
                defaultValue = "GET",
                required = true,
                listValue = new List<EnumValue>() {
                    new EnumValue() { value = "GET" },
                    new EnumValue() { value = "POST" },
                    new EnumValue() { value = "PUT" },
                    new EnumValue() { value = "DELETE" },
                     }
            });
            AddField(new NodeField()
            {
                name = "parameters",
                type = FieldType.array,
                fields = new List<NodeField>() {
                    new NodeField() { name = "name", type = FieldType.@string, required = true },
                    new NodeField() { name = "value", type = FieldType.@string, required = true },
                     }
            });

            AddField(new NodeField() { name = "next", type = FieldType.output });
        }

    }
}
