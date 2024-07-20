using Microsoft.ClearScript;

namespace blueprint.modules.blueprint.runtime
{
    public class Node
    {
        private core.blocks.Node node;
        public string id
        {
            get { return node.id; }
        }
        public string name
        {
            get { return node.name; }
        }
        public Node back
        {
            get { return new Node(node.caller); }
        }
        public Node(core.blocks.Node node)
        {
            this.node = node;
        }
        public void print(object log)
        {
            Console.WriteLine($"Print :{log}");
        }
        public void execnode(string name)
        {
            node.ExecuteNode(name);
        }
        public void execnodeposition(string name, int position)
        {
            node.ExecuteNode(name, position);
        }
        public int getfieldarraycount(string name)
        {
            return node.GetFieldArrayCount(name);
        }
        public object field(string name)
        {
            return node.GetField(name)?.Value();
        }
        public object get_data(string name, object alter = null)
        {
            return node.get_data(name, alter);
        }
        public void set_data(string name, object value)
        {
            node.set_data(name, value);
        }
        public void set_output(object value)
        {
            node.set_output(value);
        }
        public object get_output()
        {
            return node.get_output();
        }
        public void wait(int sec, string function)
        {
            wait((double)sec, function);
        }
        public void wait(double sec, string function)
        {
            BlueprintProcessModule.Instance.Wait(node, sec, function);
        }


    }

}
