using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using Newtonsoft.Json.Linq;
using srtool;
using System;
using ThirdParty.BouncyCastle.Asn1;

namespace blueprint.modules.blueprint.core
{
    public static class BlueprintSnapshot
    {
        public static string Snapshot(this Blueprint item)
        {
            var r = item.JsonSnapshot();
            r["ver"] = 1;
            return r.ToString(Newtonsoft.Json.Formatting.None);
        }
        public static JObject JsonSnapshot(this Blueprint item)
        {
            var result = new JObject();
            result["id"] = item.id;
            var envFields = new JArray();
            if (item.fields != null)
                foreach (var eField in item.fields)
                    envFields.Add(eField.ObjectToJson());
            if (envFields.Count > 0)
                result["fields"] = envFields;

            var jBlocks = new JArray();
            if (item.blocks != null)
                foreach (var block in item.blocks)
                    jBlocks.Add(block.JsonSnapshot());
            if (jBlocks.Count > 0)
                result["blocks"] = jBlocks;
            return result;
        }
        public static JObject JsonSnapshot(this Block item)
        {
            JObject result;
            if (item is blocks.Node node)
                result = JsonSnapshot(node);
            else
            if (item is blocks.StickyNote note)
                result = JsonSnapshot(note);
            else
            if (item is blocks.Variable variable)
                result = JsonSnapshot(variable);
            else
            {
                result = new JObject();
                result["type"] = "none";
            }
            result["id"] = item.id;
            result["name"] = item.name;
            if (item.reference_id != null)
                result["reference_id"] = item.reference_id;
            result["coordinate"] = item.coordinate.Snapshot();

            return result;
        }
        public static JObject JsonSnapshot(this blocks.Node node)
        {
            var result = new JObject();
            result["type"] = "node";

            if (node.reference_id == null)
            {
                if (node.script != null)
                    result["script"] = node.script.code;
            }

            if (node.data.Count > 0)
            {
                var dataJObject = new JObject();

                foreach (var d in node.data)
                    dataJObject[d.Key] = d.Value.ObjectToJson();

                result["data"] = dataJObject;
            }
            if (node.static_data.Count > 0)
            {
                var dataJObject = new JObject();

                foreach (var d in node.static_data)
                    dataJObject[d.Key] = d.Value.ObjectToJson();

                result["static_data"] = dataJObject;
            }
            var components = new JArray();
            foreach (var c in node.components)
                components.Add(c.JsonSnapshot());
            if (components.Count > 0)
                result["components"] = components;

            result["fields"] = node.fields.JsonSnapshot();
            result["result"] = node.nodeResult;
            return result;
        }

        public static object JsonToObject_data(JObject jObject)
        {
            var type = (string)jObject["type"];
            switch (type)
            {
                case "null":
                    return null;
                case "int":
                    return (int)jObject["value"];
                case "double":
                    return (double)jObject["value"];
                case "bool":
                    return (bool)jObject["value"];
                case "string":
                    return (string)jObject["value"];
                default:
                    return null;
            }

        }
        public static JObject ObjectToJson(this object obj)
        {
            var data = new JObject();
            if (obj == null)
            {
                data["type"] = "null";
                data["value"] = null;
            }
            else
            if (obj is int @int)
            {
                data["type"] = "int";
                data["value"] = @int;
            }
            else
            if (obj is double @double)
            {
                data["type"] = "double";
                data["value"] = @double;
            }
            else
            if (obj is bool boolean)
            {
                data["type"] = "bool";
                data["value"] = boolean;
            }
            else
            if (obj is string @string)
            {
                data["type"] = "string";
                data["value"] = @string;
            }
            else
            {
                data["type"] = "string";
                data["value"] = obj.ToString();
            }

            return data;
        }
        public static JToken JsonSnapshot(this Field field)
        {
            //result["name"] = field.name;
            if (field.value == null)
            {
                return null;
                //result["type"] = "null";
                //result["value"] = null;
            }
            else
            if (field.value is List<Field> @jarray)
            {
                var resultJArray = new JArray();
                foreach (var j in @jarray)
                {
                    resultJArray.Add(JsonSnapshot(j));
                }
                return resultJArray;
            }
            else
            if (field.value is Dictionary<string, Field> @object)
            {
                var resultObject = new JObject();
                foreach (var item in @object)
                    resultObject[item.Key] = JsonSnapshot(item.Value);

                return resultObject;
            }
            else
            if (field.value is Expression expression)
            {
                var result = new JObject();
                result["type"] = "expression";
                result["value"] = expression.expression;
                return result;
            }
            else
            if (field.value is Node node)
            {
                var result = new JObject();
                result["type"] = "node";
                result["value"] = node.id;
                return result;

            }
            else
            if (field.value is double @double)
            {
                return JToken.FromObject(field.value);
            }
            else
            if (field.value is DateTime dateTime)
            {
                return JToken.FromObject(field.value);
            }
            else
            if (field.value is int @int)
            {
                return JToken.FromObject(field.value);
            }
            else
            if (field.value is bool boolean)
            {
                return JToken.FromObject(field.value);
            }
            else
            if (field.value is string @string)
            {
                return JToken.FromObject(field.value);
            }
            else
            {
                var result = new JObject();
                result["type"] = "none";
                return result;
            }


        }
        public static JObject JsonSnapshot(this blocks.StickyNote item)
        {
            var result = new JObject();
            result["id"] = item.id;
            result["type"] = "sticky-note";
            result["coordinate"] = item.coordinate.Snapshot();
            return result;
        }
        public static JObject JsonSnapshot(this blocks.Variable item)
        {
            var result = new JObject();
            result["id"] = item.id;
            result["type"] = "variable";
            result["coordinate"] = item.coordinate.Snapshot();
            return result;
        }
        public static string Snapshot(this Coordinate item)
        {
            return $"{item.x},{item.y},{item.w},{item.h}";
        }

        public static Block LoadBlock(object fromObject, JObject data)
        {
            var type = (string)data["type"];
            Block block = null;
            switch (type)
            {
                case "node":
                    block = LoadNode(fromObject, data);
                    break;
                case "variable":
                    block = LoadVariable(fromObject, data);
                    break;
                case "sticky-note":
                    break;
            }

            if (block != null)
                block.coordinate = LoadCoordinate((string)data["coordinate"]);
            block.id = (string)data["id"];
            block.name = (string)data["name"];
            if (data["reference_id"] != null)
                block.reference_id = (string)data["reference_id"];

            return block;
        }

        public static Coordinate LoadCoordinate(string data)
        {
            if (data == null)
                return new Coordinate();

            var items = data.Split(',').Select(i => float.Parse(i)).ToArray();
            return new Coordinate() { x = items[0], y = items[1], w = items[2], h = items[3] };
        }
        public static Expression LoadExpression(string expression)
        {
            var res = new Expression(expression);

            return res;
        }
        public static blocks.Variable LoadVariable(object fromObject, JObject data)
        {
            var variable = new blocks.Variable();
            variable.parent = fromObject;
            variable.type = Enum.Parse<DataType>((string)data["type"]);
            variable.value = (string)data["value"];

            return variable;
        }
        public static blocks.Node LoadNode(object fromObject, JObject data)
        {
            var node = new blocks.Node();
            node.parent = fromObject;

            if (data["reference_id"] != null)
                node.reference_id = (string)data["reference_id"];

            if (data["reference_id"] == null)
            {
                if (data["script"] != null)
                    node.script = new Script((string)data["script"]);
            }
            if (data["fields"] != null)
            {
                node.fields = LoadField(node, data["fields"]);
            }
            if (data["result"] != null)
                node.nodeResult = data["result"].ToString();

            if (data["data"] != null)
            {
                foreach (var item in ((JObject)data["data"]).Properties())
                {
                    node.data.Add(item.Name, BlueprintSnapshot.JsonToObject_data((JObject)item.Value));
                }
            }
            if (data["static_data"] != null)
            {
                foreach (var staticData in ((JObject)data["static_data"]).Properties())
                {
                    node.static_data.Add(staticData.Name, BlueprintSnapshot.JsonToObject_data((JObject)staticData.Value));
                }
            }

            if (data["components"] != null)
                foreach (JObject componentData in (JArray)data["components"])
                {
                    node.components.Add(LoadComponent(node, componentData));
                }

            return node;
        }
        public static Blueprint LoadBlueprint(string data, Blueprint source = null)
        {
            var result = LoadBlueprint(JObject.Parse(data));
            result.source = source;
            return result;
        }
        public static Blueprint LoadBlueprint(JObject data)
        {
            int ver = data.AsIntDef("ver", 1);

            var blueprint = new Blueprint();
            blueprint.id = data.AsStringDef("id", "");

            if (data["blocks"] != null)
                foreach (JObject i in (JArray)data["blocks"])
                {
                    blueprint.blocks.Add(LoadBlock(blueprint, i));
                }

            return blueprint;
        }
        public static Field LoadField(object fromObject, JToken data)
        {
            var field = new Field();

            switch (data.Type)
            {
                case JTokenType.Object:
                    {
                        var asJObject = (JObject)data;
                        field.type = DataType.@object;

                        if ((string)asJObject["type"] == "expression")
                        {
                            field.type = DataType.expression;
                            field.value = new Expression((string)asJObject["value"]);
                        }
                        else
                        {
                            field.AsSubField = new Dictionary<string, Field>();
                            foreach (var subItem in asJObject)
                            {
                                field.AsSubField.Add(subItem.Key, LoadField(fromObject, subItem.Value));
                            }
                        }
                    }
                    break;
                case JTokenType.Array:
                    {
                        field.AsArrayList = new List<Field>();
                        var asJArray = (JArray)data;
                        field.type = DataType.array;
                        foreach (var subItem in asJArray)
                        {
                            field.AsArrayList.Add(LoadField(fromObject, subItem));
                        }
                    }
                    break;
                case JTokenType.String:
                    {
                        field.type = DataType.@string;
                        field.value = (string)data;
                    }
                    break;
                case JTokenType.Integer:
                    {
                        field.type = DataType.@int;
                        field.value = (int)data;
                    }
                    break;
                case JTokenType.Boolean:
                    {
                        field.type = DataType.@string;
                        field.value = (bool)data;
                    }
                    break;
                case JTokenType.Date:
                    {
                        field.type = DataType.datetime;
                        field.value = (DateTime)data;
                    }
                    break;
                case JTokenType.Float:
                    {
                        field.type = DataType.@double;
                        field.value = (float)data;
                    }
                    break;
                case JTokenType.Null:
                    {
                        field.type = DataType.@null;
                        field.value = null;
                    }
                    break;
            }
            return field;
        }
        public static ComponentBase LoadComponent(blocks.Node node, JObject data)
        {
            var name = (string)data["name"];
            var type = (string)data["type"];

            switch (type)
            {
                case "webhook":
                    return new Webhook() { name = name, node = node, token = (string)data["token"] };
                case "pulse":
                    {
                        Enum.TryParse<origin>((string)data["origin"], out origin origin);
                        return new Pulse() { name = name, node = node, callback = (string)data["callback"], delayParam = (string)data["delayParam"], origin = origin };
                    }
                default:
                    return new ComponentBase() { node = node, name = name };
            }

        }
        public static JObject JsonSnapshot(this ComponentBase component)
        {
            var result = new JObject();
            result["name"] = component.name;
            if (component is Webhook webhook)
            {
                result["type"] = "webhook";
                result["token"] = webhook.token;
            }
            else
            if (component is Pulse pulse)
            {
                result["type"] = "pulse";
                result["callback"] = pulse.callback;
                result["delayParam"] = pulse.delayParam;
                result["origin"] = pulse.origin.ToString();
            }
            else
            {
                result["type"] = "none";
            }
            return result;
        }

    }
}
