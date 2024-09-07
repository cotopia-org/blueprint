using blueprint.modules.blueprint.core;
using blueprint.srtool;

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
            connection.Send(new { type = "listening", time = time });
        }

        private void Connection_OnDisconnect(WSConnection arg1, DisconnectInfo arg2)
        {
            foreach (var i in this.process.blueprint.nodes)
            {
                i.OnStart -= Node_OnStart;
            }
            onDisconnect?.Invoke(this);
        }

        public void Bind(Process process)
        {
            this.process = process;
            foreach (var i in this.process.blueprint.nodes)
            {
                i.OnStart += Node_OnStart;
            }
            connection.Send(new { type = "bind-process", time = time, data = new { blueprint = blueprint.JsonSnapshot() } });
        }

        private void Node_OnStart(core.blocks.Node node)
        {
            connection.Send(new { type = "start-node", time = time, data = new { nodeId = node.id, fromNodeId = node.from?.id } });
        }
    }
}
