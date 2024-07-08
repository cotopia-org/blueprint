using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using Newtonsoft.Json.Linq;
using srtool;

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
        public static JObject JsonSnapshot(this Field field)
        {
            var result = new JObject();
            if (field.id != null)
                result["id"] = field.id;

            result["name"] = field.name;
            result["type"] = field.type.ToString();
            if (field.value == null)
            {
                result["value_type"] = "null";
                result["value"] = null;
            }
            else
            if (field.value is Expression expression)
            {
                result["value_type"] = "expression";
                result["value"] = expression.JsonSnapshot();
            }
            else
            if (field.type == DataType.node)
            {
                result["value_type"] = "nodes";
                var ids = new JArray();
                foreach (var i in field.nodes_value)
                    ids.Add(i.id);
                result["value"] = ids;
            }
            else
            if (field.value is double @double)
            {
                result["value_type"] = "double";
                result["value"] = @double;
            }
            else
            if (field.value is int @int)
            {
                result["value_type"] = "int";
                result["value"] = @int;
            }
            else
            if (field.value is bool boolean)
            {
                result["value_type"] = "bool";
                result["value"] = boolean;
            }
            else
            if (field.value is string @string)
            {
                result["value_type"] = "string";
                result["value"] = @string;
            }

            return result;
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
        public static Expression LoadExpression(JObject data)
        {
            var expression = (string)data["expression"];
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
                foreach (JObject fieldData in (JArray)data["fields"])
                {
                    node.fields.Add(LoadField(node, fieldData));
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
            field.parent = fromObject;

            if (data["id"] != null)
                field.id = (string)data["id"];

            field.name = (string)data["name"];
            field.type = Enum.Parse<DataType>((string)data["type"]);
            var valueType = (string)data["value_type"];
            switch (valueType)
            {
                case "null":
                    field.value = null;
                    break;
                case "expression":
                    field.value = LoadExpression((JObject)data["value"]);
                    break;
                case "nodes":
                    foreach (string id in (JArray)data["value"])
                        field.nodes_ids_value.Add(id);
                    break;
                case "string":
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
