﻿using blueprint.core;
using blueprint.modules.account;
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.blocks;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.logic;
using blueprint.modules.blueprint.request;
using blueprint.modules.blueprint.response;
using blueprint.modules.database;
using blueprint.modules.node.logic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using srtool;

namespace blueprint.modules.blueprint
{
    public class BlueprintModule : Module<BlueprintModule>
    {
        public IMongoCollection<database.blueprint> dbContext { get; private set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<database.blueprint>("blueprint");
            Indexing();
        }
        private async void Indexing()
        {
            try
            {
                var builder = Builders<database.blueprint>.IndexKeys.Ascending(i => i.index_tokens);
                await dbContext.Indexes.CreateOneAsync(new CreateIndexModel<database.blueprint>(builder, new CreateIndexOptions() { Background = true }));
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        public async Task<WebhookCallResponse> Exec_token(string token)
        {
            var dbItem = await dbContext.AsQueryable().Where(i => i.index_tokens.Contains($"webhook:{token}")).FirstOrDefaultAsync();

            if (dbItem == null)
                return null;

            var process = await BlueprintProcessModule.Instance.CreateProcess(dbItem.data_snapshot);

            var webhooks = process.Blueprint.FindComponents<Webhook>();

            var webhook = webhooks.FirstOrDefault(i => i.token == token);

            webhook.node.Execute();
            IncExecution(dbItem);

            var response = new WebhookCallResponse();
            response.output = webhook.node.get_output();
            return response;
        }

        private async void IncExecution(database.blueprint dbItem)
        {
            await dbContext.UpdateOneAsync(i => i._id == dbItem._id, Builders<database.blueprint>.Update.Inc(j => j.exec_counter, 1));
        }

        public async Task<BlueprintResponse> Upsert(string id, BlueprintRequest request, string fromAccountId)
        {
            database.blueprint item;

            if (id != null)
            {
                item = await dbContext.AsQueryable().FirstOrDefaultAsync(i => i._id == id);
            }
            else
            {
                item = new database.blueprint();
                item._id = ObjectId.GenerateNewId().ToString();
                item.createDateTime = DateTime.UtcNow;
                item.data_snapshot = new Blueprint().Snapshot();
            }

            item.account_id = fromAccountId;

            var changedBlueprint = await LoadBlueprint(item._id, request.blueprint);
            var mainBlueprint = await LoadBlueprint(item._id, JObject.Parse(item.data_snapshot));

            await ApplyChanges(mainBlueprint, changedBlueprint);

            item.title = request.title;
            item.description = request.description;

            item.data_snapshot = mainBlueprint.Snapshot();

            item.index_tokens = new List<string>();
            item.index_tokens.AddRange(mainBlueprint.FindComponents<Webhook>().Select(i => $"webhook:{i.token}").ToList());

            item.updateDateTime = DateTime.UtcNow;

            await dbContext.ReplaceOneAsync(i => i._id == item._id, item, new ReplaceOptions() { IsUpsert = true });

            return await Get(item._id, fromAccountId);
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
            //   var dbAccounts = await dbContext.Find_Cahce("_id", ids.Select(i => i.ToObjectId()).ToList());
            return await List(dbItems, fromAccountId);
        }
        public async Task<List<BlueprintResponse>> List(List<database.blueprint> dbItems, string fromAccountId = null)
        {
            var datas = dbItems.Select(i => new
            {
                res = new BlueprintResponse()
                {
                    id = i._id.ToString(),
                    title = i.title,
                    blueprint = i.data_snapshot.ToJObject(),
                    description = i.description,
                    createDateTime = i.createDateTime,
                    updateDateTime = i.updateDateTime,
                },
                accId = i.account_id,
            }).ToList();
            List<KeyValuePair<string, JObject>> replaceItems = new List<KeyValuePair<string, JObject>>();

            foreach (var item in datas.Select(i => i.res.blueprint))
            {
                if (item != null && item["blocks"] != null)
                    foreach (JObject block in (JArray)item["blocks"])
                    {
                        if ((string)block["type"] == "node")
                        {
                            string reference_id = (string)block["reference_id"];
                            replaceItems.Add(new KeyValuePair<string, JObject>(reference_id, block));
                        }
                    }
            }

            var accounts = await AccountModule.Instance.List(datas.Select(i => i.accId).Distinct().ToList());
            //Set Accounts
            datas.ForEach(acc => { acc.res.creator = accounts.FirstOrDefault(i => i.id == acc.accId); });

            if (dbItems.Count == 1)
            {
                //if (replaceItems.Count > 0)
                //{
                //    foreach (var item in replaceItems)
                //    {
                //        string reference_id = (string)item.Value["reference_id"];
                //        item.Value.Remove("reference_id");
                //        item.Value["reference"] = referenceNodes.FirstOrDefault(i => i.id == reference_id)?.ToJObject();
                //    }
                //}

                var ids = replaceItems.Select(i => i.Key).OrderedDistinct().ToList();
                var referenceNodes = await NodeModule.Instance.List(ids, fromAccountId);
                datas.ForEach(acc => { acc.res.referenceNodes = referenceNodes; });
            }

            return datas.Select(i => i.res).ToList();
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

            await FillRefrence(blueprint);

            return blueprint;
        }
        public async Task FillRefrence(Blueprint blueprint)
        {
            var refrenceIds = blueprint.nodes.Select(i => i.reference_id).OrderedDistinct().ToList();
            if (refrenceIds.Count > 0)
            {
                var refrenceNodes = await NodeModule.Instance.Find_byids(refrenceIds);

                foreach (var n in blueprint.nodes)
                {
                    var refrence = refrenceNodes.FirstOrDefault(i => i.id == n.reference_id);
                    if (refrence != null && refrence.script != null)
                        n.script = new Script(refrence.script.code);
                }
            }
        }

        private static async Task ApplyChanges(Blueprint mainBlueprint, Blueprint changedBlueprint)
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
                    mainBlueprint.RemoveBlock(block.id);
                }
            }

            var refrenceIds = changedBlueprint.nodes.Select(i => i.reference_id).Distinct().ToList();


            var refrenceNodes = await NodeModule.Instance.Find_byids(refrenceIds);

            foreach (var changedBlock in changedBlueprint.blocks)
            {
                var mainBlock = mainBlueprint.blocks.FirstOrDefault(i => i.id == changedBlock.id);
                var refrenceNode = refrenceNodes.FirstOrDefault(i => i.id == changedBlock.reference_id);

                mainBlock.name = changedBlock.name;
                mainBlock.coordinate = changedBlock.coordinate;
                bool isAdded = addBlockIds.Contains(changedBlock.id);

                if (changedBlock is Node editNode)
                {
                    var mainNode = mainBlock as Node;
                    mainNode.fields = editNode.fields;
                    UpsertNode(mainNode, editNode, refrenceNode, isAdded);
                }
                else
                if (changedBlock is StickyNote editStickyNote)
                {
                    var mainStickyNote = mainBlock as StickyNote;
                    mainStickyNote.text = editStickyNote.text;
                }
            }
        }
        private static void UpsertNode(Node main, Node edited, Node refrence, bool isAdded)
        {
            if (refrence != null && refrence.HasComponent<Webhook>() && !main.HasComponent<Webhook>())
            {
                var webhook = main.AddComponent<Webhook>();
                webhook.token = Utility.CalculateMD5Hash(Guid.NewGuid().ToString()).ToLower();
            }
        }

    }
}
