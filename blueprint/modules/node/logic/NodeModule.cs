using blueprint.core;
using blueprint.modules.account;
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint.core.component;
using blueprint.modules.blueprint.core.test;
using blueprint.modules.blueprint.request;
using blueprint.modules.database.logic;
using blueprint.modules.drive.logic;
using blueprint.modules.node.database;
using blueprint.modules.node.request;
using blueprint.modules.node.response;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using srtool;
using System.Collections.Generic;

namespace blueprint.modules.node.logic
{
    public class NodeModule : Module<NodeModule>
    {
        public IMongoCollection<database.node> dbContext { get; private set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<database.node>("node");
            {
                var node = builtin_nodes._log_node();
                var dbItem = new database.node();
                dbItem._id = "65c4115a0111a5ca6bd473c6";
                dbItem.title = "log";
                dbItem.name = node.name;
                dbItem.data = BlueprintSnapshot.JsonSnapshot(node).ToString(Newtonsoft.Json.Formatting.None);
                dbItem.script = node.script.code;
                dbItem.updateDateTime = DateTime.UtcNow;
                dbItem.createDateTime = DateTime.UtcNow;

                await dbContext.ReplaceOneAsync(i => i._id == dbItem._id, dbItem, new ReplaceOptions() { IsUpsert = true });
            }
            {
                var node = builtin_nodes._delay_node();
                var dbItem = new database.node();
                dbItem._id = "65c4115a0111a5ca6bd472c4";
                dbItem.title = "delay";

                dbItem.name = node.name;
                dbItem.data = BlueprintSnapshot.JsonSnapshot(node).ToString(Newtonsoft.Json.Formatting.None);
                dbItem.script = node.script.code;
                dbItem.updateDateTime = DateTime.UtcNow;
                dbItem.createDateTime = DateTime.UtcNow;

                await dbContext.ReplaceOneAsync(i => i._id == dbItem._id, dbItem, new ReplaceOptions() { IsUpsert = true });
            }
            {
                var node = builtin_nodes._webhook_node(null);
                var dbItem = new database.node();
                dbItem._id = "65c4115a0111a5ca6bd122c2";
                dbItem.title = "web hook";

                dbItem.name = node.name;
                dbItem.data = BlueprintSnapshot.JsonSnapshot(node).ToString(Newtonsoft.Json.Formatting.None);
                dbItem.script = node.script.code;

                dbItem.updateDateTime = DateTime.UtcNow;
                dbItem.createDateTime = DateTime.UtcNow;

                await dbContext.ReplaceOneAsync(i => i._id == dbItem._id, dbItem, new ReplaceOptions() { IsUpsert = true });
            }

            {
                var node = builtin_nodes._pulse_node();
                var dbItem = new database.node();
                dbItem._id = "65c4115a0111a5ca6bd022c2";
                dbItem.title = "pulse";

                dbItem.name = node.name;
                dbItem.data = BlueprintSnapshot.JsonSnapshot(node).ToString(Newtonsoft.Json.Formatting.None);
                dbItem.script = node.script.code;

                dbItem.updateDateTime = DateTime.UtcNow;
                dbItem.createDateTime = DateTime.UtcNow;

                await dbContext.ReplaceOneAsync(i => i._id == dbItem._id, dbItem, new ReplaceOptions() { IsUpsert = true });
            }
        }
        public async Task<NodeResponse> Upsert(string id, NodeRequest request, string fromAccountId)
        {
            node.database.node dbItem;

            if (id == null)
            {
                dbItem = new database.node();
                dbItem._id = ObjectId.GenerateNewId().ToString();
                dbItem.createDateTime = DateTime.UtcNow;
            }
            else
            {
                dbItem = await dbContext.AsQueryable().FirstOrDefaultAsync(i => i._id == id);
            }

            if (dbItem == null)
                throw AppException.NotFoundObject();

            if (fromAccountId != null)
            {
                if (dbItem.account_id != fromAccountId)
                    throw AppException.ForbiddenAccessObject();
            }

            dbItem.updateDateTime = DateTime.UtcNow;
            dbItem.name = request.name;

            dbItem.title = request.title;
            dbItem.description = request.description;
            dbItem.account_id = fromAccountId;
            dbItem.script = request.script;

            var node = new blueprint.core.blocks.Node();
            node.name = request.name;
            node.script = new Script(request.script);
            node.coordinate = new blueprint.core.nodes.Coordinate();

            var nodeSnapshot = BlueprintSnapshot.JsonSnapshot(node);
            dbItem.data = nodeSnapshot.ToString(Newtonsoft.Json.Formatting.None);

            await dbContext.ReplaceOneAsync(i => i._id == dbItem._id, dbItem, new ReplaceOptions() { IsUpsert = true });

            return await Get(dbItem._id, fromAccountId);
        }
        public async Task<PaginationResponse<NodeResponse>> List(Pagination pagination, string search = null, string fromAccountId = null)
        {
            var q1 = dbContext.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q1 = q1.Where(i => i.title.ToLower().Contains(search.ToLower()) || i.description.ToLower().Contains(search.ToLower()));

            var dbAccounts = await q1
             .OrderByDescending(i => i.createDateTime)
             .Skip(pagination.Skip)
             .Take(pagination.Take).ToListAsync();
            var qCount = await q1.CountAsync();
            var result = new PaginationResponse<NodeResponse>();

            result.total = qCount;
            result.page = pagination.Page;
            result.perPage = pagination.PerPage;
            result.items = await List(dbAccounts, fromAccountId);

            return result;
        }
        public async Task<List<NodeResponse>> List(List<string> ids, string fromAccountId = null)
        {
            if (ids == null || ids.Count == 0)
                return new List<NodeResponse>();

            var dbItems = await dbContext.AsQueryable().Where(i => ids.Contains(i._id)).ToListAsync();
            return await List(dbItems, fromAccountId);
        }
        public async Task<List<NodeResponse>> List(List<database.node> dbItems, string fromAccountId = null)
        {
            var results = dbItems.Select(i => new
            {
                item = new NodeResponse()
                {
                    id = i._id.ToString(),
                    title = i.title,
                    name = i.name,
                    description = i.description,
                    // fields = i.fields,
                    data = i.data?.ToJObject(),
                    createDateTime = i.createDateTime,
                    updateDateTime = i.updateDateTime,
                },
                mediaId = i.icon_media_id,
                accountId = i.account_id,
            }).ToList();

            var accounts = await AccountModule.Instance.List(results.Select(i => i.accountId).Distinct().ToList(), fromAccountId);
            var medias = await DriveModule.Instance.List(results.Select(i => i.mediaId).Distinct().ToList());
            results.ForEach(i => { i.item.icon_media = medias.FirstOrDefault(j => j.id == i.mediaId); });
            results.ForEach(i => { i.item.creator = accounts.FirstOrDefault(j => j.id == i.accountId); });

            return results.Select(i => i.item).ToList();
        }
        public async Task<NodeResponse> Get(string id, string fromAccountId = null)
        {
            var _id = id.ToObjectId();
            var result = await List(new List<string>() { _id.ToString() }, fromAccountId);
            return result.FirstOrDefault();
        }
        public async Task Delete(string id, string fromAccountId = null)
        {
            var dbItem = await dbContext.AsQueryable().FirstOrDefaultAsync(i => i._id == id);

            if (dbItem == null)
                throw AppException.NotFoundObject();

            if (fromAccountId != null)
            {
                if (dbItem.account_id != fromAccountId)
                    throw AppException.ForbiddenAccessObject();
            }

            await dbContext.DeleteOneAsync(i => i._id == id);
        }
        public async Task<blueprint.core.blocks.Node> Find_by_name(string name)
        {
            return (await Find_by_name(new string[] { name })).FirstOrDefault();
        }
        public async Task<List<blueprint.core.blocks.Node>> Find_by_name(string[] names)
        {
            var dbItems = await dbContext.AsQueryable().Where(i => names.Contains(i.name)).ToListAsync();

            var result = new List<blueprint.core.blocks.Node>();
            foreach (var dbItem in dbItems)
            {
                var nodeJson = JObject.Parse(dbItem.data);
                var node = blueprint.core.BlueprintSnapshot.LoadNode(null, nodeJson);
                node.reference_id = dbItem._id;

                result.Add(node);
            }
            return result;
        }
        public async Task<blueprint.core.blocks.Node> Find_by_id(string id)
        {
            return (await Find_by_ids(new List<string>() { id })).FirstOrDefault();
        }
        public async Task<List<blueprint.core.blocks.Node>> Find_by_ids(List<string> ids)
        {
            if (ids.Count == 0)
                return new List<blueprint.core.blocks.Node>();

            var dbItems = await dbContext.AsQueryable().Where(i => ids.Contains(i._id)).ToListAsync();

            var result = new List<blueprint.core.blocks.Node>();
            foreach (var i in dbItems)
            {
                var nodeJson = JObject.Parse(i.data);
                var node = blueprint.core.BlueprintSnapshot.LoadNode(null, nodeJson);
                node.reference_id = i._id;
                node.id = i._id;
                result.Add(node);
            }
            return result;
        }
    }
}
