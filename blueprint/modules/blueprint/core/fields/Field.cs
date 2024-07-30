using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core;
using MongoDB.Bson;

namespace blueprint.modules.blueprint.core.fields
{
    public class Field
    {
        public DataType type { get; set; }
        public object value { get; set; }

        public List<Field> AsArrayList
        {
            get
            {
                if (value is List<Field> _x)
                    return _x;
                else
                    return null;
            }
            set
            {
                this.value = value;
            }
        }
        public Dictionary<string, Field> AsSubField
        {
            get
            {
                if (value is Dictionary<string, Field> _x)
                    return _x;
                else
                    return null;
            }
            set
            {
                this.value = value;
            }
        }
        public string AsString(Node node)
        {
            return (string)Value(node);
        }
        public int AsInt(Node node)
        {
            return (int)Value(node);
        }
        public object GetValue(string address)
        {
            return null;
        }
        public void SetValue(string address, object value)
        {

            var splitItems = address.Split('.');
            var splitCount = splitItems.Length;
            var splitItem = splitItems[0];

            if (int.TryParse(splitItem, out int index))
            {
                if (AsArrayList == null)
                    AsArrayList = new List<Field>();

                while (AsArrayList.Count <= index)
                {
                    AsArrayList.Add(new Field());
                }

                if (splitCount == 1)
                {
                    AsArrayList[index].value = value;
                }
                else
                {
                    Field field;
                    var cValue = AsArrayList[index];
                    if (cValue == null || !(cValue is Field))
                    {
                        field = new Field();
                        AsArrayList[index] = field;
                    }
                    else
                    {
                        field = (Field)cValue;
                    }

                    field.SetValue(string.Join('.', splitItems.Skip(1)), value);
                }
            }
            else
            {
                if (AsSubField == null)
                    AsSubField = new Dictionary<string, Field>();

                if (splitCount == 1)
                {
                    if (AsSubField.ContainsKey(splitItem))
                        AsSubField[splitItem].value = value;
                    else
                        AsSubField.Add(splitItem, new Field());
                }
                else
                {

                    if (!AsSubField.ContainsKey(splitItem))
                        AsSubField.Add(splitItem, null);

                    Field field;
                    var cValue = AsSubField[splitItem];
                    if (cValue == null || !(cValue is Field))
                    {
                        field = new Field();
                        AsSubField[splitItem] = field;
                    }
                    else
                    {
                        field = (Field)cValue;
                    }

                    field.SetValue(string.Join('.', splitItems.Skip(1)), value);
                }
            }
        }
        public object Value(Node fromNode)
        {
            if (value is Expression expression)
                return expression.Value(this, fromNode);
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
                type = DataType.node;
                if (value == null || value is not List<string>)
                    value = new List<string>();
                return (List<string>)value;
            }
        }
        public List<Node> nodes_value(Node node)
        {
            var ids = AsArrayList.Select(i => i.AsString(node));
            return node.bind_blueprint.blocks.Where(i => ids.Contains(i.id)).Where(i => i is Node).Select(i => (Node)i).ToList();
        }
        #endregion
    }
}
