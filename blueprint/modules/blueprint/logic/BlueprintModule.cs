using srtool;
using blueprint.core;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using blueprint.modules.account;
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.request;
using blueprint.modules.blueprint.response;
using blueprint.modules.blueprintProcess.logic;
using blueprint.modules.database.logic;
using blueprint.modules.node.logic;
using blueprint.modules.schedule.logic;
using blueprint.modules.blueprint.logic;
using blueprint.srtool;
using System.Net.WebSockets;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using blueprint.modules.node.response;
namespace blueprint.modules.blueprint
{
    public partial class BlueprintModule : Module<BlueprintModule>
    {
        public IMongoCollection<database.blueprint_model> dbContext { get; private set; }
        private List<BlueprintDebugHandler> debugItems { get; set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<database.blueprint_model>("blueprint");
            Indexing();
            debugItems = new List<BlueprintDebugHandler>();
            BlueprintProcessModule.Instance.OnCreateProcess += Instance_OnCreateProcess;
            ScheduleModule.Instance.OnAction += Instance_OnAction;
        }

        private void Instance_OnCreateProcess(Process process)
        {
            BlueprintDebugHandler debugItem = null;
            lock (debugItems)
            {
                if (debugItems.Count > 0)
                {
                    debugItem = debugItems.FirstOrDefault(i => i.id == process.blueprint.id);

                }
            }
            if (debugItem != null)
            {
                debugItems.Remove(debugItem);
                debugItem.Bind(process);
            }
        }
        private async void Indexing()
        {
            try
            {
                var builder = Builders<database.blueprint_model>.IndexKeys.Ascending(i => i.index_tokens);
                await dbContext.Indexes.CreateOneAsync(new CreateIndexModel<database.blueprint_model>(builder, new CreateIndexOptions() { Background = true }));
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        private void Instance_OnAction(schedule.database.SchedulerResponse item)
        {
            if (item.category == "blueprint")
            {
                var data = item.payload.ToJObject();

                var type = (string)data["type"];
                switch (type)
                {
                    case "cron":
                        var blueprint_id = (string)data["blueprint_id"];
                        var node_id = (string)data["node_id"];
                        Exec_cron(blueprint_id, node_id);
                        break;
                }

            }
        }
        public async void Exec_cron(string blueprint_id, string node_id)
        {
            try
            {
                var dbItem = await dbContext.AsQueryable().Where(i => i._id == blueprint_id).FirstOrDefaultAsync();
                if (dbItem != null)
                {
                    var source = await BlueprintModule.Instance.GetBlueprint(blueprint_id);
                    var process = await BlueprintProcessModule.Instance.CreateProcess(source, dbItem.data_snapshot);

                    var crons = process.blueprint.FindComponents<Cron>();

                    var node = crons.FirstOrDefault(i => i.node.id == node_id);
                    if (node != null)
                    {
                        node.node.InvokeFunction(node.callback);
                        IncExecution(dbItem._id);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }

        public async Task<WebResponse> Exec_webhooktoken(string token, HttpContext context)
        {
            var index_token = $"webhook:{token}";
            var dbItem = await SuperCache.Get(async () =>
            {
                var item = await dbContext.AsQueryable().Where(i => i.index_tokens.Contains(index_token)).FirstOrDefaultAsync();
                return item;
            }, new CacheSetting() { key = $"webhook_blueprint_{index_token}", timeLife = TimeSpan.FromSeconds(60) });

            if (dbItem == null)
            {
                var appE = new AppException(System.Net.HttpStatusCode.NotFound);
                throw appE;
            }

            if (!dbItem.active)
            {
                var appE = new AppException(System.Net.HttpStatusCode.NotFound);
                appE.AddHint("hint", "Blueprint is deactivate.");
                throw appE;
            }

            var sourceBlueprint = await GetBlueprint(dbItem._id);
            var process = await BlueprintProcessModule.Instance.CreateProcess(sourceBlueprint, dbItem.data_snapshot);

            var webhookNode = process.blueprint.FindComponents<Webhook>().Where(i => i.token == token).Select(i => i.node).FirstOrDefault();
            if (webhookNode == null)
            {
                var appE = new AppException(System.Net.HttpStatusCode.NotFound);
                appE.AddHint("hint", "Webhook node is not found.");
                throw appE;
            }

            var queryData = new JObject();
            foreach (var i in context.Request.Query)
                queryData[i.Key] = i.Value.ToString();

            var headersData = new JObject();
            foreach (var i in context.Request.Headers)
                headersData[i.Key] = i.Value.ToString();

            var resItem = new
            {
                webhook_token = token,
                headers = headersData,
                query = queryData,
                remoteIp_v4 = context.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                remoteIp_v6 = context.Connection.RemoteIpAddress.MapToIPv6().ToString(),
            };

            webhookNode.result = JObject.FromObject(resItem).ToString(Newtonsoft.Json.Formatting.Indented);
            webhookNode.CallStart();

            //IncExecution(dbItem._id);

            //var timeout = TimeSpan.FromSeconds(Convert.ToInt32(webhookNode.GetField("timeout")));
            var result = await process.blueprint.WaitForWebResponse(TimeSpan.FromSeconds(10));

            return result;
        }

        private async void IncExecution(string id)
        {
            try
            {
                await dbContext.UpdateOneAsync(i => i._id == id, Builders<database.blueprint_model>.Update.Inc(j => j.exec_counter, 1));
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        public async Task<BlueprintResponse> Upsert(string id, BlueprintRequest request, string fromAccountId)
        {
            database.blueprint_model item;

            if (id != null)
            {
                item = await dbContext.AsQueryable().FirstOrDefaultAsync(i => i._id == id);
                if (item == null)
                    throw AppException.NotFoundObject();
            }
            else
            {
                item = new database.blueprint_model();
                item.account_id = fromAccountId;
                item.index_tokens = new List<string>();
                item._id = ObjectId.GenerateNewId().ToString();
                item.createDateTime = DateTime.UtcNow;
                item.data_snapshot = new Blueprint().Snapshot();
            }
            item.active = request.active;

            var changedBlueprint = await LoadBlueprint(item._id, request.blueprint);
            var mainBlueprint = await GetBlueprint(item._id);

            if (mainBlueprint == null)
            {
                mainBlueprint = new Blueprint();
                mainBlueprint.id = item._id;
            }

            var changedBlocks = new List<Block>();
            var removedBlocks = new List<Block>();

            await ApplyChanges(mainBlueprint, changedBlueprint, changedBlocks, removedBlocks);

            item.title = request.title;
            item.description = request.description;

            item.data_snapshot = mainBlueprint.Snapshot();

            foreach (var token in item.index_tokens)
                SuperCache.Remove($"webhook_blueprint_{token}");

            item.index_tokens = new List<string>();
            item.index_tokens.AddRange(mainBlueprint.FindComponents<Webhook>().Select(i => $"webhook:{i.token}").ToList());

            item.updateDateTime = DateTime.UtcNow;

            await dbContext.ReplaceOneAsync(i => i._id == item._id, item, new ReplaceOptions() { IsUpsert = true });

            Handle_Cron(id, changedBlocks, removedBlocks, item.active);
            return await Get(item._id, fromAccountId);
        }
        public async Task Delete(string id, string fromAccountId)
        {

            var dbItem = await dbContext.AsQueryable().FirstOrDefaultAsync(i => i._id == id);

            if (dbItem != null)
            {
                if (dbItem.account_id != fromAccountId)
                    throw AppException.ForbiddenAccessObject();
            }
            else
            {
                throw AppException.NotFoundObject();
            }
            await dbContext.DeleteOneAsync(i => i._id == id);
        }
        private static void Handle_Cron(string id, List<Block> changedBlocks, List<Block> removedBlocks, bool active)
        {
            foreach (var block in changedBlocks)
            {
                if (block is Node node)
                {
                    if (node.HasComponent<Cron>())
                    {
                        var cronComponents = node.GetComponents<Cron>();

                        foreach (var cron in cronComponents)
                        {
                            var payload = new JObject();
                            payload["type"] = "cron";

                            payload["name"] = cron.name;
                            payload["blueprint_id"] = id;
                            payload["node_id"] = cron.node.id;

                            var _sm_id = $"blueprint:{id}:cron:{cron.node.id}>{cron.name}";
                            var cronExpression = (string)cron.node.GetField(cron.expressionParam);

                            if (active)
                                ScheduleModule.Instance.Upsert(_sm_id, cronExpression, payload.ToString(), "blueprint");
                            else
                                ScheduleModule.Instance.Remove(_sm_id);
                        }
                    }
                }
            }

            foreach (var block in removedBlocks)
            {
                if (block is Node node)
                {
                    if (node.HasComponent<Cron>())
                    {
                        var pulseComponents = node.GetComponents<Cron>();

                        foreach (var pulse in pulseComponents)
                        {
                            var _sm_id = $"pulse:{id}:{pulse.node.id}:{pulse.name}";

                            ScheduleModule.Instance.Remove(_sm_id);
                        }
                    }
                }
            }
        }
        public async Task<PaginationResponse<BlueprintRowResponse>> List(string accountId, Pagination pagination, string search = null, string fromAccountId = null)
        {
            var q1 = dbContext.AsQueryable();

            if (!string.IsNullOrEmpty(accountId))
            {
                q1 = q1.Where(i => i.account_id == accountId);
            }

            if (!string.IsNullOrEmpty(search))
                q1 = q1.Where(i => i.title.ToLower().Contains(search.ToLower()) || i.description.ToLower().Contains(search.ToLower()));

            var dbItems = await q1
             .OrderByDescending(i => i.updateDateTime)
             .Skip(pagination.Skip)
             .Take(pagination.Take).ToListAsync();

            var result = new PaginationResponse<BlueprintRowResponse>();
            result.total = await q1.CountAsync();
            result.page = pagination.Page;
            result.perPage = pagination.PerPage;
            var list = await List(dbItems, fromAccountId);

            result.items = list.Select(i =>
            new BlueprintRowResponse()
            {
                createDateTime = i.createDateTime,
                updateDateTime = i.updateDateTime,
                title = i.title,
                creator = i.creator,
                description = i.description,
                id = i.id,
                run = i.run,
            }).ToList();
            return result;
        }
        public async Task<List<BlueprintResponse>> List(List<string> ids, string fromAccountId = null)
        {
            if (ids == null)
                return new List<BlueprintResponse>();

            var _ids = ids.Distinct().ToList();

            var dbItems = await dbContext.AsQueryable().Where(i => _ids.Contains(i._id)).ToListAsync();
            return await List(dbItems, fromAccountId);
        }
        public async Task<List<BlueprintResponse>> List(List<database.blueprint_model> dbItems, string fromAccountId = null)
        {
            var data = dbItems.Select(i => new
            {
                res = new BlueprintResponse()
                {
                    id = i._id.ToString(),
                    title = i.title,
                    description = i.description,
                    createDateTime = i.createDateTime,
                    updateDateTime = i.updateDateTime,
                    run = i.active,
                },
                accId = i.account_id,
                snapshot = i.data_snapshot,
            }).ToList();

            var accounts = await AccountModule.Instance.List(data.Select(i => i.accId).Distinct().ToList());
            //Set Accounts
            data.ForEach(item => { item.res.creator = accounts.FirstOrDefault(i => i.id == item.accId); });

            if (dbItems.Count == 1)
            {
                data.ForEach(item => { item.res.blueprint = item.snapshot.ToJObject(); });

                var ids = new List<string>();

                foreach (var item in data.Select(i => i.res.blueprint))
                {
                    if (item != null && item["blocks"] != null)
                        foreach (JObject block in (JArray)item["blocks"])
                        {
                            if ((string)block["type"] == "node")
                            {
                                var reference_id = (string)block["reference_id"];
                                if (!ids.Contains(reference_id))
                                    ids.Add(reference_id);
                            }
                        }
                }

                var referenceNodes = await NodeModule.Instance.List(ids, fromAccountId);
                data.ForEach(acc => { acc.res.referenceNodes = referenceNodes; });
            }

            return data.Select(i => i.res).ToList();
        }
        public async Task<BlueprintResponse> Get(string id, string fromAccountId = null)
        {
            var _id = id.ToObjectId();
            var dbItem = await dbContext.AsQueryable().FirstOrDefaultAsync(i => i._id == id);
            if (fromAccountId != null)
                if (dbItem.account_id != fromAccountId)
                {
                    throw AppException.ForbiddenAccessObject();
                }
            var result = await List(new List<database.blueprint_model>() { dbItem }, fromAccountId);
            return result.FirstOrDefault();
        }

        public async Task<Blueprint> LoadBlueprint(string id, JObject blueprintData)
        {
            var blueprint = BlueprintSnapshot.LoadBlueprint(blueprintData);
            blueprint.id = id;

            await FillReference(blueprint);

            return blueprint;
        }
        public async Task FillReference(Blueprint blueprint)
        {
            var referenceIds = blueprint.nodes.Select(i => i.reference_id).OrderedDistinct().ToList();
            if (referenceIds.Count > 0)
            {
                var referenceNodes = await NodeModule.Instance.List(referenceIds);

                foreach (var n in blueprint.nodes)
                {
                    var reference = referenceNodes.FirstOrDefault(i => i.id == n.reference_id);
                    if (reference != null && reference.script != null)
                        n.script = new Script(reference.script);
                }
            }
        }

        private async Task ApplyChanges(Blueprint mainBlueprint, Blueprint changedBlueprint, List<Block> changedBlocks, List<Block> removedBlocks)
        {
            var addBlockIds = new List<string>();
            var removedBlockIds = new List<string>();

            foreach (var block in changedBlueprint.nodes)
            {
                if (!mainBlueprint.blocks.Exists(i => i.id == block.id))
                {
                    addBlockIds.Add(block.id);
                }
            }
            foreach (var block in mainBlueprint.nodes)
            {
                if (!changedBlueprint.blocks.Exists(i => i.id == block.id))
                {
                    removedBlockIds.Add(block.id);
                }
            }
            foreach (var id in addBlockIds)
            {
                var changeBlock = changedBlueprint.blocks.FirstOrDefault(i => i.id == id);

                Block newBlock = null;

                if (changeBlock is Node _node)
                {
                    newBlock = new Node();
                    _node.fields = ((Node)changeBlock).fields;
                }
                else
                if (changeBlock is StickyNote _stickyNode)
                {
                    newBlock = new StickyNote();
                    _stickyNode.text = ((StickyNote)changeBlock).text;
                }

                if (newBlock != null)
                {
                    newBlock.id = changeBlock.id;
                    newBlock.name = changeBlock.name;
                    newBlock.reference_id = changeBlock.reference_id;
                    newBlock.coordinate = changeBlock.coordinate;
                    mainBlueprint.AddBlock(newBlock);
                }
            }
            foreach (var id in removedBlockIds)
            {
                var block = mainBlueprint.blocks.FirstOrDefault(i => i.id == id);

                if (block != null)
                {
                    removedBlocks.Add(block);
                    mainBlueprint.RemoveBlock(block.id);
                }
            }

            var referenceIds = changedBlueprint.nodes.Select(i => i.reference_id).Distinct().ToList();

            var referenceNodes = await NodeModule.Instance.List(referenceIds);

            foreach (var changedBlock in changedBlueprint.blocks)
            {
                var mainBlock = mainBlueprint.blocks.FirstOrDefault(i => i.id == changedBlock.id);
                var referenceNode = referenceNodes.FirstOrDefault(i => i.id == changedBlock.reference_id);

                mainBlock.name = changedBlock.name;
                mainBlock.coordinate = changedBlock.coordinate;
                mainBlock.reference_id = changedBlock.reference_id;
                bool isAdded = addBlockIds.Contains(changedBlock.id);

                if (changedBlock is Node editNode)
                {
                    var mainNode = mainBlock as Node;
                    mainNode.fields = editNode.fields;
                    UpsertNode(mainNode, editNode, referenceNode, isAdded);
                    changedBlocks.Add(mainNode);
                }
                else
                if (changedBlock is StickyNote editStickyNote)
                {
                    var mainStickyNote = mainBlock as StickyNote;
                    mainStickyNote.text = editStickyNote.text;

                    changedBlocks.Add(editStickyNote);
                }

            }
        }
        private void UpsertNode(Node main, Node edited, NodeResponse reference, bool isAdded)
        {
            if (reference != null)
            {
                if (reference.components != null)
                {
                    var webhook = reference.components.FirstOrDefault(i => i.name == "Webhook");
                    if (webhook != null && !main.HasComponent<Webhook>())
                    {
                        var component = main.AddComponent<Webhook>();

                        if (isAdded)
                            component.token = Utility.CalculateMD5Hash(Guid.NewGuid().ToString()).ToLower();
                    }

                    var cron = reference.components.FirstOrDefault(i => i.name == "Cron");
                    if (cron != null && !main.HasComponent<Cron>())
                    {
                        var component = main.AddComponent<Cron>();
                        component.expressionParam = cron.param1;
                        component.callback = cron.param2;
                    }
                }
            }
        }
        public async Task<Blueprint> GetBlueprint(string id)
        {
            return await SuperCache.Get(async () =>
            {
                var item = await dbContext.AsQueryable().FirstOrDefaultAsync(i => i._id == id);
                if (item != null)
                {
                    var blueprint = await LoadBlueprint(item._id, JObject.Parse(item.data_snapshot));
                    var newBlueprintSaveHandler = new BlueprintSaveHandler(blueprint);
                    blueprint.onChangeStaticData += newBlueprintSaveHandler.doSave;
                    return blueprint;
                }
                return null;

            }, new CacheSetting() { key = $"blueprint:{id}", timeLife = TimeSpan.FromMinutes(5) });
        }

        public async Task LiveTrace(WSConnection connection, string id, string fromAccountId = null)
        {
            var blueprint = await GetBlueprint(id);

            if (fromAccountId != null)
            {
                var dbBlueprint = await dbContext.AsQueryable().Where(i => i._id == id).FirstOrDefaultAsync();
                if (dbBlueprint != null)
                {
                    if (dbBlueprint.account_id != fromAccountId)
                    {
                        throw AppException.ForbiddenAccessObject();
                    }
                }
            }

            var debugHandler = new BlueprintDebugHandler();
            debugHandler.id = id;

            debugHandler.onDisconnect += DebugHandler_onDisconnect;
            debugHandler.Bind(connection);
            debugHandler.Bind(blueprint);
            lock (debugItems)
                debugItems.Add(debugHandler);
        }

        private void DebugHandler_onDisconnect(BlueprintDebugHandler item)
        {
            lock (debugItems)
                debugItems.Remove(item);
        }
    }
}
