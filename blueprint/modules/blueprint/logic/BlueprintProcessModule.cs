using blueprint.core;
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.database.logic;
using blueprint.modules.scheduler.logic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using srtool;

namespace blueprint.modules.blueprint
{
    public class BlueprintProcessModule : Module<BlueprintProcessModule>
    {
        public IMongoCollection<database.process> dbContext { get; private set; }


        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<database.process>("process");
            SchedulerModule.Instance.OnAction += Instance_OnAction;
        }

        private void Instance_OnAction(scheduler.database.SchedulerResponse scheduler)
        {
            if (scheduler.category == "process")
            {
                var payload = JObject.Parse(scheduler.payload);
                var type = (string)payload["type"];
                switch (type)
                {
                    case "wait":
                        On_wait(payload);
                        break;
                }
            }
        }
        private async void On_wait(JObject payload)
        {
            try
            {
                var function = (string)payload["function"];
                var processId = (string)payload["processId"];
                var nodeId = (string)payload["nodeId"];

                var process = await GetProcessById(processId);
                if (process != null)
                {
                    var node = process.blueprint.FindNodeWithId(nodeId);
                    if (node != null)
                    {
                        node.InvokeFunction(function);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        public async Task<Process> CreateProcess(string snapshot)
        {
            var process = new Process();
            process.id = ObjectId.GenerateNewId().ToString();
            process.blueprint = BlueprintSnapshot.LoadBlueprint(snapshot);
            await BlueprintModule.Instance.FillReference(process.blueprint);
            process.blueprint._process = process;
            await Task.Yield();
            return process;
        }
        public async Task<Process> GetProcessById(string id)
        {
            var dbItem = await dbContext.AsQueryable().Where(i => i._id == id).FirstOrDefaultAsync();
            if (dbItem == null)
                return null;

            var process = new Process();
            process.id = dbItem._id;
            process.blueprint = BlueprintSnapshot.LoadBlueprint(dbItem.snapshot);
            await BlueprintModule.Instance.FillReference(process.blueprint);

            process.blueprint._process = process;

            return process;
        }
        public async Task SaveProcess(Process process)
        {
            var dbProcess = new database.process();
            dbProcess._id = process.id;
            dbProcess.blueprint_id = process.blueprint.id;
            dbProcess.createDateTime = DateTime.UtcNow;
            dbProcess.snapshot = process.blueprint.Snapshot();

            await dbContext.ReplaceOneAsync(i => i._id == dbProcess._id, dbProcess, new ReplaceOptions() { IsUpsert = true });
        }

        public async void Wait(Node node, double waitTime, string callBackFunction)
        {
            if (node.bind_blueprint._process != null)
            {
                await SaveProcess(node.bind_blueprint._process);
                var data = new JObject();
                data["type"] = "wait";
                data["nodeId"] = node.id;
                data["function"] = callBackFunction;
                data["processId"] = node.bind_blueprint._process.id;
                data["duration"] = waitTime;
                SchedulerModule.Instance.Upsert($"process_{node.bind_blueprint._process.id}", TimeSpan.FromSeconds(waitTime), data.ToString(Newtonsoft.Json.Formatting.None), "process", false);
            }
        }
    }
}
