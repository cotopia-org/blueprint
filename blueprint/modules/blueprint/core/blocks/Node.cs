using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;
using blueprint.modules.node.types;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace blueprint.modules.blueprint.core.blocks
{
    public class Node : Block
    {
        public Node caller { get; set; }
        public Script script { get; set; }
        public Dictionary<string, Field> fields { get; set; }
        public List<component.ComponentBase> components { get; set; }
        public Dictionary<string, object> data { get; set; }
        public Node() : base()
        {
            fields = new Dictionary<string, Field>();
            components = new List<component.ComponentBase>();
            data = new Dictionary<string, object>();
            coordinate = new Coordinate() { h = 10, w = 10 };
        }
        public object field(string address)
        {
            //  var item = fields.Explore(address);

            return null;
        }
        public object setfield(string address, object value)
        {
            return null;
        }
        public object field_push(string address, object value)
        {
            return null;
        }

        public void AddField_old(string name, Field field)
        {
            if (!fields.ContainsKey(name))
            {
                fields.Add(name, field);
            }
        }

        public void SetField(string address, object value)
        {
            var field = GetField(address);
            if (field != null)
            {
                field.value = value;
            }
            else
            {

            }
        }
        public Field GetField(string address)
        {
            var split = address.Split('.');
            fields.TryGetValue(split[0], out Field value);
            return value;
        }
        public void Execute()
        {
            Execute(null);
        }

        public void Execute(Node fromNode)
        {
            caller = fromNode;
            script?.Invoke("node", new runtime.Node(this), "start");

        }
        public void InvokeFunction(string function)
        {
            script?.Invoke("node", new runtime.Node(this), function);
        }
        public void ExecuteNode(string address)
        {
            var field = GetField(address);

            if (field != null)
            {
                foreach (var subField in field.AsArrayList)
                {
                    // subField.as
                }
            }

            //var field = fields.Where(i => i.type == DataType.node && i.name == address).FirstOrDefault();
            //if (field != null)
            //{
            //    foreach (var n in field.nodes_value)
            //        n.Execute(this);
            //}
        }
        public void ExecuteNode(string name, int position)
        {
            var field = GetField(name);
            if (field != null)
            {
                var node = field.nodes_value(this)[position];
                node.Execute(this);
            }
        }
        public int GetFieldArrayCount(string name)
        {
            var field = GetField(name);
            if (field != null)
                return field.nodes_ids_value.Count;
            else
                return 0;
        }
        public void set_output(object value)
        {
            set_data("_$$_OUTPUT_$$_", value);
        }
        public object get_output()
        {
            return get_data("_$$_OUTPUT_$$_", null);
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
        public void BindNode(Node node)
        {
            BindNode("next", node);
        }

        public void BindNode(string fieldName, Node node)
        {
            var field = GetField(fieldName);
            if (!field.nodes_ids_value.Contains(node.id))
                field.nodes_ids_value.Add(node.id);
        }
        public void UnBindNode(Node node)
        {
            UnBindNode("next", node);
        }
        public void UnBindNode(string fieldName, Node node)
        {
            var field = GetField(fieldName);
            if (field.nodes_ids_value.Contains(node.id))
                field.nodes_ids_value.Remove(node.id);
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
    }
}
