﻿using blueprint.modules.blueprintProcess.logic;
using blueprint.modules.blueprintlog.logic;
using Microsoft.ClearScript;
using System.Net;

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
        public Node find_byname(string name)
        {
            return new Node(node.find_byname(name));
        }
        public void execnode(string address)
        {
            node.ExecuteNode(address);
        }
        public void execnodeposition(string address, int position)
        {
            node.ExecuteNode(address, position);
        }
        public int fieldarraycount(string address)
        {
            return node.GetFieldArraySize(address);
        }
        public object field(string address)
        {
            return node.GetField(address);
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
        public void set_result(object value)
        {
            node.set_result(value);
        }
        public object get_result()
        {
            return node.get_result();
        }
        public void blueprint_response(object value)
        {
            node.bind_blueprint.Response(node, value);
        }
        public void wait(int sec, string function)
        {
            wait((double)sec, function);
        }
        public void wait(double sec, string function)
        {
            BlueprintProcessModule.Instance.Wait(node, sec, function);
        }
        public void log(object message)
        {
            if (node.bind_blueprint != null && node.bind_blueprint._process != null)
                ProcessLogLogic.Instance.AddLog(node.bind_blueprint.id, node.bind_blueprint._process.id, "log", message.ToString());
        }
        public void warning(object message)
        {
            if (node.bind_blueprint != null && node.bind_blueprint._process != null)
                ProcessLogLogic.Instance.AddLog(node.bind_blueprint.id, node.bind_blueprint._process.id, "warning", message.ToString());
        }
        public void error(object message)
        {
            if (node.bind_blueprint != null && node.bind_blueprint._process != null)
                ProcessLogLogic.Instance.AddLog(node.bind_blueprint.id, node.bind_blueprint._process.id, "error", message.ToString());
        }
    }
}
