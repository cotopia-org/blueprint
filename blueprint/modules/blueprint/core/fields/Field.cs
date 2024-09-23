using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core;
using MongoDB.Bson;
using System.Dynamic;

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

        public int GetArraySize(string address)
        {
            var splitItems = address.Split('.');
            var splitCount = splitItems.Length;
            var splitItem = splitItems[0];


            if (int.TryParse(splitItem, out int index))
            {
                if (AsArrayList != null)
                {
                    var x = AsArrayList[index];
                    if (x != null)
                    {
                        if (splitCount == 1)
                            return x.AsArrayList.Count;
                        else
                            return x.GetArraySize(string.Join('.', splitItems.Skip(1)));
                    }
                }
            }
            else
            {
                if (AsSubField != null)
                {
                    var x = AsSubField[splitItem];
                    if (splitCount == 1)
                        return x.AsArrayList.Count;
                    else
                        return x.GetArraySize(string.Join('.', splitItems.Skip(1)));
                }
            }
            return 0;
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
                        AsSubField.Add(splitItem, new Field() { value = value });
                }
                else
                {

                    if (!AsSubField.ContainsKey(splitItem))
                        AsSubField.Add(splitItem, null);

                    Field field;
                    var cValue = AsSubField[splitItem];
                    if (cValue == null || !(cValue is Field))
                    {
                        field = new Field() { value = value };
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

        public void PushValue(string address, object value)
        {
            var field = GetField(address);
            if (field.AsArrayList == null)
                field.AsArrayList = new List<Field>();

            field.AsArrayList.Add(new Field() { value = value });
        }
        public object Value(string address, object alter = null)
        {
            return Value(address, null, alter);
        }
        public Field GetField(string address)
        {
            var splitItems = address.Split('.');
            var splitCount = splitItems.Length;
            var splitItem = splitItems[0];

            if (int.TryParse(splitItem, out int index))
            {
                if (AsArrayList != null)
                {
                    if (splitCount == 1)
                    {
                        if (!(AsArrayList[index].value is Field field))
                        {
                            field = new Field();
                            AsArrayList[index].value = field;
                        }
                        return field;
                    }
                    else
                    {
                        return AsArrayList[index].GetField(string.Join('.', splitItems.Skip(1)));
                    }
                }
            }
            else
            {
                if (AsSubField != null)
                {
                    if (splitCount == 1)
                    {
                        if (!(AsSubField[splitItem].value is Field field))
                        {
                            field = new Field();
                            AsSubField[splitItem].value = field;
                        }
                        return field;
                    }
                    else
                    {

                        return AsSubField[splitItem].GetField(string.Join('.', splitItems.Skip(1)));
                    }
                }
            }
            return null;
        }
        public object Value(string address, Node node, object alter = null)
        {
            var splitItems = address.Split('.');
            var splitCount = splitItems.Length;
            var splitItem = splitItems[0];

            if (int.TryParse(splitItem, out int index))
            {
                if (AsArrayList != null)
                {
                    if (splitCount == 1)
                    {
                        var val = AsArrayList[index].Value(node);
                        return val;
                    }
                    else
                    {
                        return AsArrayList[index].Value(string.Join('.', splitItems.Skip(1)), node);
                    }
                }
            }
            else
            {
                if (AsSubField != null)
                {
                    if (splitCount == 1)
                    {
                        if (AsSubField.TryGetValue(splitItem, out Field _x))
                        {
                            var val = _x.Value(node);
                            return val;
                        }
                        else
                        {
                            return alter;
                        }
                    }
                    else
                    {
                        if (AsSubField.TryGetValue(splitItem, out Field _x))
                        {
                            return _x.Value(string.Join('.', splitItems.Skip(1)), node);
                        }
                        else
                        {
                            return alter;
                        }
                    }
                }
            }
            return alter;
        }
        public object Value(Node fromNode)
        {
            if (value is Expression expression)
            {
                var input = new ScriptInput();
                input.AddHostObject("node", fromNode.Runtime());

                return expression.Value(input);
            }
            else
                return value;
        }

    }
}
