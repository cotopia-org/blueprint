using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
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

        public async Task Bind(Blueprint blueprint)
        {
            this.blueprint = blueprint;
            await connection.Send(new { type = "listening", time = time, data = new { blueprint = blueprint.JsonSnapshot() } });

        }
        public void Bind(WSConnection connection)
        {
            this.connection = connection;
            connection.OnDisconnect += Connection_OnDisconnect;
            DateTime = DateTime.UtcNow;
        }

        private void Connection_OnDisconnect(WSConnection connection, DisconnectInfo info)
        {
            if (process != null)
            {
                foreach (var node in process.blueprint.nodes)
                {
                    node.OnStart -= Node_OnStart;
                    node.OnResult -= Node_OnResult;
                    node.OnAddLog -= Node_OnAddLog;
                }
            }
            onDisconnect?.Invoke(this);
        }
        public async Task Bind(Process process)
        {
            this.process = process;
            foreach (var i in this.process.blueprint.nodes)
            {
                i.OnStart += Node_OnStart;
                i.OnAddLog += Node_OnAddLog;
                i.OnResult += Node_OnResult;
            }
            await connection.Send(new { type = "bind-process", time = time, data = new { process = new { id = process.id }, blueprint = blueprint.JsonSnapshot() } });
        }
        private async void Node_OnResult(Node node)
        {
            try
            {
                await connection.Send(new { type = "node", subType = "result", time = time, data = new { nodeId = node.id, result = node.result } });
            }
            catch (Exception ex)
            {
                Debug.Error(ex);
            }
        }
        private async void Node_OnAddLog(runtime.Log log)
        {
            try
            {
                await connection.Send(new { type = "console", subType = "add-log", time = time, log = new { type = log.type, message = log.message, node_id = log.node_id } });
            }
            catch (Exception ex)
            {
                Debug.Error(ex);
            }
        }
        private async void Node_OnStart(core.blocks.Node node)
        {
            try
            {
                await connection.Send(new { type = "node", subType = "start", time = time, data = new { nodeId = node.id, fromNodeId = node.from?.id } });
            }
            catch (Exception ex)
            {
                Debug.Error(ex);
            }
        }
    }
}
