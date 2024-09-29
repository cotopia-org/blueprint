using blueprint.modules.blueprintProcess.logic;
using blueprint.modules.blueprintlog.logic;
using System.Net;
using Newtonsoft.Json.Linq;
using srtool;
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
        public Node from
        {
            get { return new Node(node.from); }
        }
        public Node(core.blocks.Node node)
        {
            this.node = node;
        }
        public void next()
        {
            node.ExecuteNode("next");
        }
        public Node findnode(string name)
        {
            return new Node(node.find_byname(name));
        }
        public void execnode(string address)
        {
            node.ExecuteNode(address);
        }
        public void execnode(string address, int position)
        {
            node.ExecuteNode(address, position);
        }
        public int field_count(string address)
        {
            return node.GetFieldArraySize(address);
        }
        public object field(string address, object alter = null)
        {
            return node.GetField(address, alter);
        }
        public bool is_exist_field(string address)
        {
            return node.IsExistField(address);
        }
        public object get_data(string name, object alter = null)
        {
            return node.get_data(name, alter);
        }
        public void set_data(string name, object value)
        {
            node.set_data(name, value);
        }
        public object get_static_data(string name, object alter = null)
        {
            return node.get_static_data(name, alter);
        }
        public void set_static_data(string name, object value)
        {
            node.set_static_data(name, value);
        }
        public object result
        {
            get
            {
                try
                {
                    var json = node.result;
                    var result = JObject.Parse(json).ToObject<object>();
                    return result;
                }
                catch
                {
                    return new object();
                }
            }
            set
            {
                try
                {
                    node.result = JObject.FromObject(value).ToString();
                }
                catch (Exception e)
                {
                    Debug.Error(e);
                }
            }
        }
        public void webresponse(int statusCode, string content)
        {
            node.webresponse(statusCode, content);
        }
        public void wait(int sec, string function)
        {
            wait((double)sec, function);
        }
        public void wait(double sec, string function)
        {
            BlueprintProcessModule.Instance.Wait(node, sec, function);
        }
        public void print(object message)
        {
            node.print(message);
        }
        public void log(object message)
        {
            node.log(message);
        }
        public void warning(object message)
        {
            node.warning(message);
        }
        public void error(object message)
        {
            node.error(message);
        }
    }
}