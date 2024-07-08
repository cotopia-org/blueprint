using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.fields;

namespace blueprint.modules.blueprint.core.blocks
{
    public class Node : Block
    {
        public Node caller { get; set; }
        public Script script { get; set; }
        public List<Field> fields { get; set; }
        public List<component.ComponentBase> components { get; set; }
        public Dictionary<string, object> data { get; set; }
        public Node() : base()
        {
            fields = new List<Field>();
            components = new List<component.ComponentBase>();
            data = new Dictionary<string, object>();
            coordinate = new Coordinate() { h = 10, w = 10 };
        }
        public void AddField(Field field)
        {
            if (!fields.Contains(field))
            {
                field.parent = this;
                fields.Add(field);
            }
        }

        public void SetField(string name, object value)
        {
            var field = GetField(name);
            if (field != null)
            {
                field.value = value;
            }
        }
        public Field GetField(string name)
        {
            return fields.FirstOrDefault(i => i.name == name);
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
        public void FunctionInvoke(string function)
        {
            script?.Invoke("node", new runtime.Node(this), function);
        }
        public void ExecuteNode(string name)
        {
            var field = fields.Where(i => i.type == DataType.node && i.name == name).FirstOrDefault();
            if (field != null)
            {
                foreach (var n in field.nodes_value)
                    n.Execute(this);
            }
        }
        public void ExecuteNode(string name, int position)
        {
            var field = fields.Where(i => i.type == DataType.node && i.name == name).FirstOrDefault();
            if (field != null)
            {
                var node = field.nodes_value[position];
                node.Execute(this);
            }
        }
        public int GetFieldArrayCount(string name)
        {
            var item = fields.FirstOrDefault(i => i.type == DataType.node && i.name == name);
            if (item != null)
                return item.nodes_ids_value.Count;
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
