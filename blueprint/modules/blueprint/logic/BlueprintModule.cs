using blueprint.core;
using blueprint.modules.account;
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.request;
using blueprint.modules.blueprint.response;
using blueprint.modules.blueprintProcess.logic;
using blueprint.modules.database.logic;
using blueprint.modules.node.logic;
using blueprint.modules.scheduler.logic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using srtool;

namespace blueprint.modules.blueprint
{
    public class BlueprintModule : Module<BlueprintModule>
    {
        public IMongoCollection<database.blueprint_model> dbContext { get; private set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<database.blueprint_model>("blueprint");
            Indexing();

            SchedulerModule.Instance.OnAction += Instance_OnAction;
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

        private void Instance_OnAction(scheduler.database.SchedulerResponse item)
        {
            if (item.category == "node:pulse")
            {
                var data = item.payload.ToJObject();
                var blueprint_id = (string)data["blueprint_id"];
                var node_id = (string)data["node_id"];
                Exec_pulse(blueprint_id, node_id);
            }
        }

        public async void Exec_pulse(string blueprint_id, string node_id)
        {
            try
            {
                var dbItem = await dbContext.AsQueryable().Where(i => i._id == blueprint_id).FirstOrDefaultAsync();
                if (dbItem != null)
                {
                    var process = await BlueprintProcessModule.Instance.CreateProcess(dbItem.data_snapshot);

                    var pulses = process.blueprint.FindComponents<Pulse>();

                    var pulseNode = pulses.FirstOrDefault(i => i.node.id == node_id);
                    if (pulseNode != null)
                    {
                        pulseNode.node.InvokeFunction(pulseNode.callback);
                        IncExecution(dbItem);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        public async Task<WebhookCallResponse> Exec_webhooktoken(string token)
        {
            var index_token = $"webhook:{token}";
            var dbItem = await dbContext.AsQueryable().Where(i => i.index_tokens.Contains(index_token)).FirstOrDefaultAsync();

            if (dbItem == null)
                return null;

            var process = await BlueprintProcessModule.Instance.CreateProcess(dbItem.data_snapshot);

            var webhooks = process.blueprint.FindComponents<Webhook>();

            var webhookNode = webhooks.FirstOrDefault(i => i.token == token);
            if (webhookNode == null)
                return null;

            webhookNode.node.CallStart();

            IncExecution(dbItem);

            var response = new WebhookCallResponse();
            response.output = webhookNode.node.get_output();
            return response;
        }

        private async void IncExecution(database.blueprint_model dbItem)
        {
            try
            {
                await dbContext.UpdateOneAsync(i => i._id == dbItem._id, Builders<database.blueprint_model>.Update.Inc(j => j.exec_counter, 1));
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
                    throw new AppException(System.Net.HttpStatusCode.NotFound);
            }
            else
            {
                item = new database.blueprint_model();
                item._id = ObjectId.GenerateNewId().ToString();
                item.createDateTime = DateTime.UtcNow;
                item.data_snapshot = new Blueprint().Snapshot();
            }

            item.account_id = fromAccountId;

            var changedBlueprint = await LoadBlueprint(item._id, request.blueprint);
            var mainBlueprint = await LoadBlueprint(item._id, JObject.Parse(item.data_snapshot));

            var changedBlocks = new List<Block>();
            var removedBlocks = new List<Block>();

            await ApplyChanges(mainBlueprint, changedBlueprint, changedBlocks, removedBlocks);

            item.title = request.title;
            item.description = request.description;

            item.data_snapshot = mainBlueprint.Snapshot();

            item.index_tokens = new List<string>();
            item.index_tokens.AddRange(mainBlueprint.FindComponents<Webhook>().Select(i => $"webhook:{i.token}").ToList());

            item.updateDateTime = DateTime.UtcNow;

            await dbContext.ReplaceOneAsync(i => i._id == item._id, item, new ReplaceOptions() { IsUpsert = true });

            HandleExternalProcess(id, changedBlocks, removedBlocks);

            return await Get(item._id, fromAccountId);
        }
        private static void HandleExternalProcess(string id, List<Block> changedBlocks, List<Block> removedBlocks)
        {
            foreach (var block in changedBlocks)
            {
                if (block is Node node)
                {
                    if (node.HasComponent<Pulse>())
                    {
                        var pulseComponents = node.GetComponents<Pulse>();

                        foreach (var pulse in pulseComponents)
                        {
                            var payload = new JObject();
                            payload["name"] = pulse.name;
                            payload["blueprint_id"] = id;
                            payload["node_id"] = pulse.node.id;

                            var _sm_id = $"pulse:{id}_{pulse.node.id}_{pulse.name}";
                            var delay = TimeSpan.FromSeconds((int)pulse.node.GetField(pulse.delayParam));
                            SchedulerModule.Instance.Upsert(_sm_id, delay, payload.ToString(), "node:pulse", true);
                        }
                    }
                }
            }

            foreach (var block in removedBlocks)
            {
                if (block is Node node)
                {
                    if (node.HasComponent<Pulse>())
                    {
                        var pulseComponents = node.GetComponents<Pulse>();

                        foreach (var pulse in pulseComponents)
                        {
                            var _sm_id = $"pulse:{id}_{pulse.node.id}_{pulse.name}";

                            SchedulerModule.Instance.Remove(_sm_id);
                        }
                    }
                }
            }
        }
        public async Task<PaginationResponse<BlueprintResponse>> List(string accountId, Pagination pagination, string search = null, string fromAccountId = null)
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

            var result = new PaginationResponse<BlueprintResponse>();
            result.total = await q1.CountAsync();
            result.page = pagination.Page;
            result.perPage = pagination.PerPage;
            result.items = await List(dbItems, fromAccountId);

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
            var result = await List(new List<string>() { _id.ToString() }, fromAccountId);
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
                var referenceNodes = await NodeModule.Instance.Find_by_ids(referenceIds);

                foreach (var n in blueprint.nodes)
                {
                    var reference = referenceNodes.FirstOrDefault(i => i.id == n.reference_id);
                    if (reference != null && reference.script != null)
                        n.script = new Script(reference.script.code);
                }
            }
        }

        private static async Task ApplyChanges(Blueprint mainBlueprint, Blueprint changedBlueprint, List<Block> changedBlocks, List<Block> removedBlocks)
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

            var referenceNodes = await NodeModule.Instance.Find_by_ids(referenceIds);

            foreach (var changedBlock in changedBlueprint.blocks)
            {
                var mainBlock = mainBlueprint.blocks.FirstOrDefault(i => i.id == changedBlock.id);
                var referenceNode = referenceNodes.FirstOrDefault(i => i.id == changedBlock.reference_id);

                mainBlock.name = changedBlock.name;
                mainBlock.coordinate = changedBlock.coordinate;
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
        private static void UpsertNode(Node main, Node edited, Node reference, bool isAdded)
        {
            if (reference != null)
            {
                if (reference.HasComponent<Webhook>() && !main.HasComponent<Webhook>())
                {
                    var refComponent = reference.GetComponent<Webhook>();

                    var component = main.AddComponent<Webhook>();
                    component.name = refComponent.name;
                    component.token = Utility.CalculateMD5Hash(Guid.NewGuid().ToString()).ToLower();
                }
                else
                if (reference.HasComponent<Pulse>() && !main.HasComponent<Pulse>())
                {
                    var refComponent = reference.GetComponent<Pulse>();
                    var component = main.AddComponent<Pulse>();
                    component.name = refComponent.name;
                    component.delayParam = refComponent.delayParam;
                    component.callback = refComponent.callback;
                }
            }
        }
    }
}
