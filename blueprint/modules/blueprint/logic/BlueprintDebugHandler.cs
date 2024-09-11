using blueprint.modules.blueprint.core;
using blueprint.srtool;
using srtool;
namespace blueprint.modules.blueprint.logic
{
    public class BlueprintDebugHandler
    {
        public string id { get; set; }
        public Blueprint blueprint { get; set; }
        public WSConnection connection { get; set; }
        public Process process { get; set; }
        private DateTime DateTime;
        private float time { get { return (float)Math.Round((DateTime.UtcNow - DateTime).TotalSeconds, 5); } }
        public event Action<BlueprintDebugHandler> onDisconnect;

        public void Bind(Blueprint blueprint)
        {
            this.blueprint = blueprint;
        }
        public void Bind(WSConnection connection)
        {
            this.connection = connection;
            connection.OnDisconnect += Connection_OnDisconnect;
            DateTime = DateTime.UtcNow;
            connection.Send(new { topic = "listening", time = time });
        }

        private void Connection_OnDisconnect(WSConnection connection, DisconnectInfo info)
        {
            foreach (var i in this.process.blueprint.nodes)
            {
                i.OnCall -= Node_OnCall;
                i.OnAddLog -= Node_OnAddLog;

            }
            onDisconnect?.Invoke(this);
        }

        public void Bind(Process process)
        {
            this.process = process;
            foreach (var i in this.process.blueprint.nodes)
            {
                i.OnCall += Node_OnCall;
                i.OnAddLog += Node_OnAddLog;

            }
            connection.Send(new { topic = "bind-process", time = time, data = new { blueprint = blueprint.JsonSnapshot() } });
        }

        private void Node_OnAddLog(runtime.Log log)
        {
            connection.Send(new { topic = "console", time = time, type = "add-log", log = new { type = log.type, message = log.message, node_id = log.node_id } });
        }

        private void Node_OnCall(core.blocks.Node node)
        {
            connection.Send(new { topic = "call-node", time = time, data = new { nodeId = node.id, fromNodeId = node.from?.id } });
            if (node.from != null)
                connection.Send(new { topic = "update-node", time = time, data = new { node = new { id = node.from.id, result = node.from.get_result()?.ConvertToJson() } } });
        }
    }
}
