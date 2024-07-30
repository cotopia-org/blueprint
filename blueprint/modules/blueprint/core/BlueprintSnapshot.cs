using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using Newtonsoft.Json.Linq;
using srtool;
using System;

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
                    envFields.Add(eField.JsonSnapshot());
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

            var data = new JArray();
            foreach (var d in node.data)
                data.Add(d.Value.JsonSnapshot());
            if (data.Count > 0)
                result["data"] = data;

            var components = new JArray();
            foreach (var c in node.components)
                components.Add(c.JsonSnapshot());
            if (components.Count > 0)
                result["components"] = components;

            var fields = new JArray();
            foreach (var f in node.fields)
                fields.Add(f.JsonSnapshot());
            if (fields.Count > 0)
                result["fields"] = fields;

            return result;
        }

        public static JObject JsonSnapshot(this object obj)
        {
            var data = new JObject();
            if (obj == null)
            {
                data["value"] = "null";
                data["value"] = null;
            }
            else
            if (obj is int @int)
            {
                data["value"] = "int";
                data["value"] = @int;
            }
            else
            if (obj is double @double)
            {
                data["value"] = "double";
                data["value"] = @double;
            }
            else
            if (obj is bool boolean)
            {
                data["value"] = "bool";
                data["value"] = boolean;
            }
            else
            if (obj is string @string)
            {
                data["value"] = "string";
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
                return JToken.FromObject(null);
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
        public static JObject JsonSnapshot(this Expression expression)
        {
            var res = new JObject();
            res["expression"] = expression.expression;
            return res;
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
                var fieldsObject = (JObject)data["fields"];

                foreach (var fieldname in fieldsObject.Properties().Select(i => i.Name).ToList())
                {
                    var jObject = (JObject)fieldsObject[fieldname];
                    node.fields.Add(fieldname, LoadField(node, jObject));
                }
            }

            if (data["components"] != null)
                foreach (JObject componentData in (JArray)data["components"])
                {
                    node.components.Add(LoadComponent(node, componentData));
                }

            return node;
        }
        public static Blueprint LoadBlueprint(string data)
        {
            return LoadBlueprint(JObject.Parse(data));
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
        public static Field LoadField(object fromObject, JObject data)
        {
            var field = new Field();
            var type = (string)data["type"];
            switch (type)
            {
                case "null":
                    field.value = null;
                    break;
                case "expression":
                    field.type = DataType.expression;
                    field.value = LoadExpression((string)data["value"]);
                    break;
                case "node":
                    field.type = DataType.node;
                    field.value = (string)data["value"];
                    break;
                case "string":
                    field.type = DataType.@string;
                    field.value = (string)data["value"];
                    break;
                case "double":
                    field.value = (double)data["value"];
                    break;
                case "int":
                    field.value = (int)data["value"];
                    break;
                case "bool":
                    field.value = (bool)data["value"];
                    break;
                case "datetime":
                    field.value = (DateTime)data["value"];
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
