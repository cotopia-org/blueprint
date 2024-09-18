using blueprint.modules.blueprint.core;

namespace blueprint.modules.node.builtinnodes
{
    public class GeoJSNode : NodeBuilder
    {
        public override string name => "geojs.io-node";
        public override string title => "Geo ip info";
        public override string script => @"
function start()
{
      var ipAddress = node.field('ipAddress');
      httprequest.get('https://get.geojs.io/v1/ip/geo/' + ipAddress + '.json',callback_result);
}
function callback_result(x)
{
    let result = {};
    result.content = x.content;
    result.statusCode = x.statusCode;

    var jsonResult = JSON.stringify(result);
    node.set_json_result(jsonResult);
    node.next();
}
";
    }
}
