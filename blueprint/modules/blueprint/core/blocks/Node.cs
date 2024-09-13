using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.blueprint.runtime;
using blueprint.modules.blueprintlog.logic;
using blueprint.modules.node.types;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;

namespace blueprint.modules.blueprint.core.blocks
{
    public class Node : Block
    {
        public Node from { get; set; }
        public Script script { get; set; }
        // public Dictionary<string, Field> fields { get; set; }
        public Field fields { get; set; }
        public List<component.ComponentBase> components { get; set; }
        public Dictionary<string, object> data { get; set; }
        public string nodeResult { get; set; }
        public Dictionary<string, object> static_data { get; set; }
        public event Action<Node> OnCall;
        public event Action<Log> OnAddLog;

        public Node() : base()
        {
            // fields = new Dictionary<string, Field>();
            fields = new Field();
            components = new List<component.ComponentBase>();
            data = new Dictionary<string, object>();
            static_data = new Dictionary<string, object>();

            coordinate = new Coordinate() { h = 10, w = 10 };
        }
        public object GetField(string address, object alter = null)
        {
            return fields.Value(address, this, alter);
        }
        public void SetField(string address, object value)
        {
            fields.SetValue(address, value);
        }
        public void FieldPush(string address, object value)
        {
            fields.PushValue(address, value);
        }
        public void CallStart()
        {
            CallStart(null);
        }
        public void CallStart(Node fromNode)
        {
            from = fromNode;
            OnCall?.Invoke(this);
            var scriptInput = new ScriptInput();
            scriptInput.AddHostObject("node", new runtime.Node(this));
            script?.Invoke("start", scriptInput);
        }
        public void InvokeFunction(string function)
        {
            var scriptInput = new ScriptInput();
            scriptInput.AddHostObject("node", new runtime.Node(this));
            script?.Invoke(function, scriptInput);
        }
        public void ExecuteNode(string address)
        {
            var arrayField = GetField(address);
            if (arrayField != null && arrayField is List<Field> fields)
            {
                foreach (var field in fields)
                {
                    var nodeId = field.AsString(this);
                    var node = this.bind_blueprint.nodes.FirstOrDefault(i => i.id == nodeId);
                    if (node != null)
                        node.CallStart(this);
                }
            }
        }
        public Node find_byname(string name)
        {
            return this.bind_blueprint.nodes.FirstOrDefault(i => i.name == name);
        }
        public void ExecuteNode(string address, int position)
        {
            var field = GetField(address);
            if (field != null && field is List<Field> fieldArray)
            {
                var nodeId = fieldArray[position].AsString(this);

                var node = this.bind_blueprint.nodes.FirstOrDefault(i => i.id == nodeId);
                if (node != null)
                    node.CallStart(this);
            }
        }
        public int GetFieldArraySize(string address)
        {
            return fields.GetArraySize(address);
        }
        public void set_result(object value)
        {
            nodeResult = value?.ToString();
        }
        public string get_result()
        {
            return nodeResult;
        }
        public object get_data(string name, object alter)
        {
            if (data.TryGetValue(name, out var _val))
                return _val;
            else
                return alter;
        }
        public void set_data(string name, object value)
        {
            if (!data.ContainsKey(name))
                data.Add(name, value);
            else
                data[name] = value;

        }
        public object get_static_data(string name, object alter)
        {
            if (bind_blueprint != null && bind_blueprint.source != null)
            {
                var item = bind_blueprint.source.FindNodeWithId(id);
                if (item != null)
                    return item.get_static_data(name, alter);
                else
                    return alter;
            }

            if (static_data.TryGetValue(name, out var _val))
                return _val;
            else
                return alter;
        }
        public void set_static_data(string name, object value)
        {
            if (bind_blueprint != null && bind_blueprint.source != null)
            {
                var item = bind_blueprint.source.FindNodeWithId(id);
                if (item != null)
                    item.set_static_data(name, value);

                return;
            }

            if (!static_data.ContainsKey(name))
                static_data.Add(name, value);
            else
                static_data[name] = value;

            if (bind_blueprint != null)
                bind_blueprint.InvokeOnChangeStaticData();
        }
        public void BindNode(Node node)
        {
            BindNode("next", node);
        }
        public void BindNode(string address, Node node)
        {
            var field = fields.GetField(address);
            if (field.AsArrayList == null)
                field.AsArrayList = new List<Field>();
            field.AsArrayList.Add(new Field() { value = node });
        }
        public void UnBindNode(Node node)
        {
            UnBindNode("next", node);
        }
        public void UnBindNode(string address, Node node)
        {
            var field = fields.GetField(address);
            field.AsArrayList.RemoveAll(i => i.value == node);
        }
        public T AddComponent<T>() where T : ComponentBase
        {
            return AddComponent<T>(null);
        }
        public T AddComponent<T>(string name) where T : ComponentBase
        {
            var instance = (ComponentBase)Activator.CreateInstance<T>();
            instance.name = name;
            instance.node = this;
            components.Add(instance);
            return GetComponent<T>();
        }
        public T GetComponent<T>() where T : ComponentBase
        {
            foreach (var c in components)
            {
                if (c is T t)
                    return t;
            }
            return default;
        }
        public List<T> GetComponents<T>() where T : ComponentBase
        {
            List<T> result = new List<T>();
            foreach (var c in components)
            {
                if (c is T t)
                    result.Add(t);
            }
            return result;
        }
        public bool HasComponent<T>() where T : ComponentBase
        {
            foreach (var c in components)
            {
                if (c is T t)
                    return true;
            }
            return false;
        }

        public void webresponse(int statusCode, object value)
        {
            bind_blueprint?.SetWebResponse(new WebResponse() { statusCode = statusCode, Content = value?.ToString() });
        }

        public void log(object message)
        {
            OnAddLog?.Invoke(new Log() { message = message, node_id = id, type = "log" });
            if (bind_blueprint != null && bind_blueprint._process != null)
                ProcessLogLogic.Instance.AddLog(bind_blueprint.id, bind_blueprint._process.id, id, "log", message?.ToString());
        }
        public void warning(object message)
        {
            OnAddLog?.Invoke(new Log() { message = message, node_id = id, type = "warning" });
            if (bind_blueprint != null && bind_blueprint._process != null)
                ProcessLogLogic.Instance.AddLog(bind_blueprint.id, bind_blueprint._process.id, id, "warning", message.ToString());
        }
        public void error(object message)
        {
            OnAddLog?.Invoke(new Log() { message = message, node_id = id, type = "error" });
            if (bind_blueprint != null && bind_blueprint._process != null)
                ProcessLogLogic.Instance.AddLog(bind_blueprint.id, bind_blueprint._process.id, id, "error", message.ToString());
        }

        public void print(object message)
        {
            OnAddLog?.Invoke(new Log() { message = message, node_id = id, type = "print" });
            try
            {
                var json = JObject.FromObject(message);
                Console.WriteLine(json.ToString());
            }
            catch
            {
                Console.WriteLine(message.ToString());
            }
        }
    }
}
