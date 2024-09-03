using blueprint.core;
using blueprint.modules.blueprint;
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

namespace blueprint.modules.blueprintProcess.logic
{
    public class BlueprintProcessModule : Module<BlueprintProcessModule>
    {
        public IMongoCollection<database.Process> dbContext { get; private set; }


        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<database.Process>("process");
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
        public async Task<Process> CreateProcess(Blueprint source, string snapshot)
        {
            var process = new Process();
            process.id = ObjectId.GenerateNewId().ToString();
            process.blueprint = BlueprintSnapshot.LoadBlueprint(snapshot);
            process.blueprint.source = source;
            await BlueprintModule.Instance.FillReference(process.blueprint);
            process.blueprint._process = process;
            return process;
        }
        public async Task<Process> GetProcessById(string id)
        {
            return await SuperCache.Get(async () =>
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

            }, new CacheSetting() { key = $"process_{id}", timeLife = TimeSpan.FromMinutes(10) });


        }
        public async void SaveProcess(Process process)
        {
            try
            {
                SuperCache.Set(process, new CacheSetting() { key = $"process_{process.id}", timeLife = TimeSpan.FromMinutes(10) });

                var cKey = $"process_{process.id}_saving";
                if (!SuperCache.Exist(cKey))
                {
                    SuperCache.Set(true, new CacheSetting() { key = cKey, timeLife = TimeSpan.FromSeconds(15) });

                    await Task.Delay(TimeSpan.FromSeconds(10));

                    process = SuperCache.Get<Process>(cKey);
                    if (process != null)
                    {
                        var dbProcess = new database.Process();
                        dbProcess._id = process.id;
                        dbProcess.blueprint_id = process.blueprint.id;
                        dbProcess.createDateTime = DateTime.UtcNow;
                        dbProcess.snapshot = process.blueprint.Snapshot();

                        SuperCache.Set(dbProcess, new CacheSetting() { timeLife = TimeSpan.FromMinutes(1), key = $"process_{process.id}_save_item" });
                        await dbContext.ReplaceOneAsync(i => i._id == dbProcess._id, dbProcess, new ReplaceOptions() { IsUpsert = true });
                        SuperCache.Remove(cKey);
                    }
                }
                else
                {
                    return;
                }

            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }

        public void Wait(Node node, double waitTime, string callBackFunction)
        {
            if (node.bind_blueprint._process != null)
            {
                SaveProcess(node.bind_blueprint._process);
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
