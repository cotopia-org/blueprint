using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.nodes;

namespace blueprint.modules.blueprint.core.fields
{
    public class Field
    {
        public object parent { get; set; }
        public Blueprint bind_blueprint
        {
            get
            {
                if (parent == null)
                    return null;
                return parent as Blueprint;
            }
        }
        public Node bind_node
        {
            get
            {
                if (parent == null)
                    return null;
                return parent as Node;
            }
        }

        public string id { get; set; }
        public string name { get; set; }
        public FieldType type { get; set; }
        //public Expression expression { get; set; }
        public object value { get; set; }

        public object Value()
        {
            if (value is Expression expression)
                return expression.Value(this);
            else
            if (value is Field field)
                return field.Value();
            else
                return value;
        }
        #region Util
        public void AddNode(Node node)
        {
            nodes_ids_value.Add(node.id);
        }
        public void RemoveNode(Node node)
        {
            nodes_ids_value.Remove(node.id);
        }
        public List<string> nodes_ids_value
        {
            get
            {
                type = FieldType.node;
                if (value == null || value is not List<string>)
                    value = new List<string>();
                return (List<string>)value;
            }
        }
        public List<Node> nodes_value
        {
            get
            {
                var ids = nodes_ids_value;
                Blueprint blueprint = null;

                if (bind_blueprint != null)
                    blueprint = bind_blueprint;
                else
                if (bind_node != null && bind_node.bind_blueprint != null)
                    blueprint = bind_node.bind_blueprint;
                else
                    return new List<Node>();

                return blueprint.blocks.Where(i => ids.Contains(i.id)).Where(i => i is Node).Select(i => (Node)i).ToList();


            }
        }
        public string as_string
        {
            get
            {
                return (string)Value();
            }
        }
        public int as_int
        {
            get
            {
                return (int)Value();
            }
        }
        #endregion
    }
}
