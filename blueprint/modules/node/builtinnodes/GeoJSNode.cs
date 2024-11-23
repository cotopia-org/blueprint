using blueprint.modules.blueprint.core;
using blueprint.modules.node.types;

namespace blueprint.modules.node.builtinnodes
{
    public class GeoJSNode : NodeBuilder
    {
        public override void Build()
        {
            base.Build();

            name = "geojs.io-node";
            title = "Geo ip info";
            script = @"
function start()
{
      var ipAddress = node.field('ipAddress');
      node.print('ip address ' + ipAddress);
      httprequest.get('https://get.geojs.io/v1/ip/geo/' + ipAddress + '.json',callback_result);
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
            AddField(new NodeField() { name = "ipAddress", type = FieldType.@string });
            AddField(new NodeField() { name = "next", type = FieldType.output });
        }
    }
}
